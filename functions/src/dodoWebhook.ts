import { onRequest } from "firebase-functions/v2/https";
import { defineSecret } from "firebase-functions/params";
import { initializeApp, getApps } from "firebase-admin/app";
import { getFirestore, Timestamp } from "firebase-admin/firestore";
import { Webhook } from "standardwebhooks";
import {
  SMTP_PASSWORD,
  sendCoffeeThankYouEmail,
  sendPremiumActivatedEmail,
} from "./email";
import {
  grantPremiumEntitlementFromVerifiedPayment,
} from "./premium";

if (getApps().length === 0) {
  initializeApp();
}

const db = getFirestore();
const REGION = "asia-south1";

const DODO_WEBHOOK_SECRET =
  defineSecret("DODO_WEBHOOK_SECRET");

const PAYMENT_ENVIRONMENT =
  String(process.env.PAYMENT_ENVIRONMENT ?? "test")
    .trim()
    .toLowerCase() === "live"
    ? "live"
    : "test";

function normalizeEmail(email: string): string {
  return email.trim().toLowerCase();
}

export const dodoWebhook = onRequest(
  {
    region: REGION,
    secrets: [
      DODO_WEBHOOK_SECRET,
      SMTP_PASSWORD,
    ],
  },
  async (req, res) => {
    if (req.method !== "POST") {
      res.status(405).json({ error: "POST required." });
      return;
    }

    const webhookId =
      String(req.get("webhook-id") ?? "");

    const webhookSignature =
      String(req.get("webhook-signature") ?? "");

    const webhookTimestamp =
      String(req.get("webhook-timestamp") ?? "");

    if (
      !webhookId ||
      !webhookSignature ||
      !webhookTimestamp
    ) {
      res.status(400).json({
        error: "Missing webhook signature headers.",
      });
      return;
    }

    try {
      const rawPayload =
        req.rawBody?.toString("utf8") ??
        JSON.stringify(req.body ?? {});

      const verifier =
        new Webhook(DODO_WEBHOOK_SECRET.value());

      await verifier.verify(rawPayload, {
        "webhook-id": webhookId,
        "webhook-signature": webhookSignature,
        "webhook-timestamp": webhookTimestamp,
      });

      const eventType =
        String(req.body?.type ?? "unknown");

      const payload =
        req.body?.data ?? {};

      const metadata =
        payload?.metadata ?? {};

      const checkoutId =
        String(metadata?.checkout_id ?? "").trim();

      const webhookEventRef =
        db.collection("dodoWebhookEvents").doc(webhookId);

      const existingEvent =
        await webhookEventRef.get();

      const existingStatus =
        String(existingEvent.data()?.status ?? "");

      if (
        existingEvent.exists &&
        (
          existingStatus === "processed" ||
          existingStatus === "ignored" ||
          existingStatus === "rejected"
        )
      ) {
        res.status(200).json({
          received: true,
          duplicate: true,
        });
        return;
      }

      await webhookEventRef.set(
        {
          webhookId,
          eventType,
          environment: PAYMENT_ENVIRONMENT,
          status: "received",
          receivedAt: Timestamp.now(),
        },
        { merge: true },
      );

      if (eventType !== "payment.succeeded") {
        const lowerEvent = eventType.toLowerCase();
        const terminalState =
          lowerEvent.includes("fail")
            ? "failed"
            : lowerEvent.includes("cancel")
              ? "cancelled"
              : "";

        if (terminalState && checkoutId) {
          const metadataType =
            String(metadata?.type ?? "").trim().toLowerCase();

          const collection =
            metadataType === "premium"
              ? "premiumCheckouts"
              : metadataType === "coffee"
                ? "supportCheckouts"
                : "";

          if (collection) {
            const checkoutRef =
              db.collection(collection).doc(checkoutId);
            const checkoutSnap = await checkoutRef.get();

            if (
              checkoutSnap.exists &&
              String(checkoutSnap.data()?.status ?? "") !== "paid"
            ) {
              await checkoutRef.set(
                {
                  status: terminalState,
                  updatedAt: Timestamp.now(),
                },
                { merge: true },
              );
            }
          }

          await webhookEventRef.set(
            {
              status: "processed",
              reason: `payment_${terminalState}`,
              checkoutId,
              updatedAt: Timestamp.now(),
            },
            { merge: true },
          );

          res.status(200).json({
            received: true,
            processed: true,
            state: terminalState,
          });
          return;
        }

        await webhookEventRef.set(
          {
            status: "ignored",
            reason: "unsupported_event",
            updatedAt: Timestamp.now(),
          },
          { merge: true },
        );

        res.status(200).json({
          received: true,
          ignored: true,
        });
        return;
      }

      if (!checkoutId) {
        await webhookEventRef.set(
          {
            status: "ignored",
            reason: "missing_checkout_id",
            updatedAt: Timestamp.now(),
          },
          { merge: true },
        );

        res.status(200).json({
          received: true,
          ignored: true,
        });
        return;
      }

      const metadataType =
        String(metadata?.type ?? "").trim().toLowerCase();

      if (metadataType === "premium") {
        const checkoutRef =
          db.collection("premiumCheckouts").doc(checkoutId);

        const checkoutSnap =
          await checkoutRef.get();

        if (!checkoutSnap.exists) {
          await webhookEventRef.set(
            {
              status: "rejected",
              reason: "premium_checkout_not_found",
              checkoutId,
              updatedAt: Timestamp.now(),
            },
            { merge: true },
          );

          res.status(400).json({
            error: "Premium checkout not found.",
          });
          return;
        }

        const checkout = checkoutSnap.data()!;

        const paymentId =
          String(payload?.payment_id ?? "").trim();

        const providerOrderId =
          String(payload?.checkout_session_id ?? "").trim();

        const currency =
          String(payload?.currency ?? "")
            .trim()
            .toUpperCase();

        const totalAmount =
          Number(payload?.total_amount ?? NaN);

        const customerEmail =
          normalizeEmail(
            String(payload?.customer?.email ?? ""),
          );

        const productId =
          String(
            payload?.product_cart?.[0]?.product_id ?? "",
          ).trim();

        const metadataPlan =
          String(metadata?.plan ?? "").trim();

        const valid =
          checkout.provider === "dodo" &&
          checkout.environment === PAYMENT_ENVIRONMENT &&
          String(checkout.providerOrderId ?? "") ===
            providerOrderId &&
          String(checkout.providerProductId ?? "") ===
            productId &&
          Number(checkout.amount ?? NaN) ===
            totalAmount &&
          String(checkout.currency ?? "")
            .toUpperCase() === currency &&
          normalizeEmail(String(checkout.email ?? "")) ===
            customerEmail &&
          String(checkout.plan ?? "") === metadataPlan &&
          (
            metadataPlan === "premium_6m" ||
            metadataPlan === "premium_12m"
          ) &&
          String(payload?.status ?? "")
            .toLowerCase() === "succeeded" &&
          Boolean(paymentId);

        if (!valid) {
          await webhookEventRef.set(
            {
              status: "rejected",
              reason:
                "premium_payment_validation_failed",
              checkoutId,
              paymentId,
              updatedAt: Timestamp.now(),
            },
            { merge: true },
          );

          console.error(
            "Dodo Premium validation failed",
            {
              checkoutId,
              paymentId,
              providerOrderId,
              productId,
              totalAmount,
              currency,
              metadataPlan,
            },
          );

          res.status(400).json({
            error: "Premium payment validation failed.",
          });
          return;
        }

        const entitlement =
          await grantPremiumEntitlementFromVerifiedPayment(
            checkoutId,
            paymentId,
            "dodo",
          );

        if (entitlement.newlyGranted) {
          try {
            await sendPremiumActivatedEmail({
              email: entitlement.email,
              plan: String(checkout.plan ?? ""),
              amountMinor:
                Number(checkout.amount ?? 0),
              currency:
                String(checkout.currency ?? "USD"),
              paymentId,
              expiresAt: entitlement.expiresAt,
            });
          } catch (mailError) {
            console.error(
              "Dodo Premium confirmation email failed",
              mailError,
            );
          }
        }

        await webhookEventRef.set(
          {
            status: "processed",
            checkoutId,
            paymentId,
            type: "premium",
            processedAt: Timestamp.now(),
          },
          { merge: true },
        );

        res.status(200).json({
          received: true,
          processed: true,
          type: "premium",
          duplicatePayment:
            !entitlement.newlyGranted,
        });
        return;
      }

      if (metadataType !== "coffee") {
        await webhookEventRef.set(
          {
            status: "ignored",
            reason: "unsupported_payment_type",
            checkoutId,
            updatedAt: Timestamp.now(),
          },
          { merge: true },
        );

        res.status(200).json({
          received: true,
          ignored: true,
        });
        return;
      }

      const checkoutRef =
        db.collection("supportCheckouts").doc(checkoutId);

      const checkoutSnap =
        await checkoutRef.get();

      if (!checkoutSnap.exists) {
        await webhookEventRef.set(
          {
            status: "rejected",
            reason: "checkout_not_found",
            checkoutId,
            updatedAt: Timestamp.now(),
          },
          { merge: true },
        );

        res.status(400).json({
          error: "Checkout not found.",
        });
        return;
      }

      const checkout = checkoutSnap.data()!;

      const paymentId =
        String(payload?.payment_id ?? "").trim();

      const providerOrderId =
        String(payload?.checkout_session_id ?? "").trim();

      const currency =
        String(payload?.currency ?? "").trim().toUpperCase();

      const totalAmount =
        Number(payload?.total_amount ?? NaN);

      const customerEmail =
        normalizeEmail(
          String(payload?.customer?.email ?? ""),
        );

      const productId =
        String(
          payload?.product_cart?.[0]?.product_id ?? "",
        ).trim();

      const valid =
        checkout.type === "coffee" &&
        checkout.provider === "dodo" &&
        checkout.environment === PAYMENT_ENVIRONMENT &&
        String(checkout.providerOrderId ?? "") ===
          providerOrderId &&
        String(checkout.providerProductId ?? "") ===
          productId &&
        Number(checkout.amount ?? NaN) ===
          totalAmount &&
        String(checkout.currency ?? "").toUpperCase() ===
          currency &&
        normalizeEmail(String(checkout.email ?? "")) ===
          customerEmail &&
        String(payload?.status ?? "").toLowerCase() ===
          "succeeded" &&
        Boolean(paymentId);

      if (!valid) {
        await webhookEventRef.set(
          {
            status: "rejected",
            reason: "payment_validation_failed",
            checkoutId,
            paymentId,
            updatedAt: Timestamp.now(),
          },
          { merge: true },
        );

        console.error("Dodo Coffee validation failed", {
          checkoutId,
          paymentId,
          providerOrderId,
          productId,
          totalAmount,
          currency,
        });

        res.status(400).json({
          error: "Payment validation failed.",
        });
        return;
      }

      const paymentRef =
        db.collection("payments").doc(
          `${PAYMENT_ENVIRONMENT}_coffee_dodo_${paymentId}`,
        );

      let newlyProcessed = false;

      await db.runTransaction(
        async (tx) => {
          const paymentSnap =
            await tx.get(paymentRef);

          if (paymentSnap.exists) {
            return;
          }

          tx.create(paymentRef, {
            type: "coffee",
            provider: "dodo",
            environment: PAYMENT_ENVIRONMENT,
            paymentId,
            checkoutId,
            email: checkout.email,
            emailHash: checkout.emailHash,
            amount: checkout.amount,
            currency: checkout.currency,
            status: "paid",
            createdAt: Timestamp.now(),
          });

          tx.set(
            checkoutRef,
            {
              status: "paid",
              paymentId,
              paidAt: Timestamp.now(),
            },
            { merge: true },
          );

          tx.set(
            webhookEventRef,
            {
              status: "processed",
              checkoutId,
              paymentId,
              processedAt: Timestamp.now(),
            },
            { merge: true },
          );

          newlyProcessed = true;
        },
      );

      if (newlyProcessed) {
        try {
          await sendCoffeeThankYouEmail({
            email: String(checkout.email ?? ""),
            amountMinor: Number(checkout.amount ?? 0),
            currency: String(checkout.currency ?? "USD"),
            paymentId,
          });
        } catch (mailError) {
          console.error(
            "Dodo Coffee thank-you email failed",
            mailError,
          );
        }
      }

      res.status(200).json({
        received: true,
        processed: true,
        duplicatePayment: !newlyProcessed,
      });
    } catch (error) {
      console.error(
        "Dodo webhook verification/processing failed",
        error,
      );

      res.status(401).json({
        error: "Invalid webhook signature or payload.",
      });
    }
  },
);