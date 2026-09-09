import { onRequest } from "firebase-functions/v2/https";
import { defineSecret } from "firebase-functions/params";
import { initializeApp, getApps } from "firebase-admin/app";
import { getFirestore, Timestamp } from "firebase-admin/firestore";
import crypto from "node:crypto";
import { SMTP_PASSWORD, sendPremiumActivatedEmail } from "./email";
import { getRuntimeConfig } from "./runtimeConfig";
import { consumePremiumPurchaseVerification } from "./purchaseOtp";
import { issueEntitlementValidationToken } from "./entitlementValidation";

if (getApps().length === 0) {
  initializeApp();
}

const db = getFirestore();
const REGION = "asia-south1";
const BASE_URL =
  "https://asia-south1-lucky-dangle.cloudfunctions.net";

const RAZORPAY_KEY_ID = defineSecret("RAZORPAY_KEY_ID");
const RAZORPAY_KEY_SECRET = defineSecret("RAZORPAY_KEY_SECRET");
const DODO_PAYMENTS_API_KEY = defineSecret("DODO_PAYMENTS_API_KEY");

type PremiumPlan = "premium_6m" | "premium_12m";

const PRICES = {
  premium_6m: { minimumInr: 19900, minimumUsd: 600, months: 6 },
  premium_12m: { minimumInr: 29900, minimumUsd: 900, months: 12 },
} as const;

const MAX_INR_PAISE = 1999900;
const MAX_USD_CENTS = 20000;

const PAYMENT_ENVIRONMENT =
  String(process.env.PAYMENT_ENVIRONMENT ?? "test")
    .trim()
    .toLowerCase() === "live"
    ? "live"
    : "test";

function providerForMarket(market: string): "razorpay" | "dodo" {
  return market === "IN" ? "razorpay" : "dodo";
}

function setCors(res: any) {
  res.set("Access-Control-Allow-Origin", "*");
  res.set("Access-Control-Allow-Headers", "Content-Type");
  res.set("Access-Control-Allow-Methods", "GET,POST,OPTIONS");
}

function normalizeEmail(email: string): string {
  return email.trim().toLowerCase();
}

function emailHash(email: string): string {
  return crypto
    .createHash("sha256")
    .update(normalizeEmail(email))
    .digest("hex");
}

function entitlementDocId(email: string): string {
  return `${PAYMENT_ENVIRONMENT}_${emailHash(email)}`;
}
function codeHash(code: string): string {
  return crypto
    .createHash("sha256")
    .update(code.trim().toUpperCase())
    .digest("hex");
}

function newRestoreCode(): string {
  const raw = crypto.randomBytes(9)
    .toString("base64url")
    .toUpperCase()
    .replace(/[^A-Z0-9]/g, "")
    .slice(0, 12);

  return `LC-${raw.slice(0, 4)}-${raw.slice(4, 8)}-${raw.slice(8, 12)}`;
}

function isPlan(value: unknown): value is PremiumPlan {
  return value === "premium_6m" || value === "premium_12m";
}

function addMonthsUtc(start: Date, months: number): Date {
  const d = new Date(start.getTime());
  d.setUTCMonth(d.getUTCMonth() + months);
  return d;
}

export async function grantPremiumEntitlementFromVerifiedPayment(
  checkoutId: string,
  paymentId: string,
  provider: "razorpay" | "dodo",
) {
  const checkoutRef =
    db.collection("premiumCheckouts").doc(checkoutId);

  const paymentRef =
    db.collection("payments").doc(
      `${PAYMENT_ENVIRONMENT}_${provider}_${paymentId}`,
    );

  return db.runTransaction(async (tx) => {
    const checkoutSnap = await tx.get(checkoutRef);

    if (!checkoutSnap.exists) {
      throw new Error("Checkout not found.");
    }

    const checkout = checkoutSnap.data()!;
    const plan = checkout.plan as PremiumPlan;
    const email = checkout.email as string;
    const eHash = emailHash(email);

    if (!isPlan(plan)) {
      throw new Error("Invalid Premium plan.");
    }

    if (String(checkout.provider ?? "") !== provider) {
      throw new Error("Checkout provider mismatch.");
    }

    const entitlementRef =
      db.collection("premiumEntitlements")
        .doc(entitlementDocId(email));

    const priorPayment = await tx.get(paymentRef);
    const currentEntitlement = await tx.get(entitlementRef);

    if (priorPayment.exists) {
      const current = currentEntitlement.data();

      return {
        email,
        expiresAt:
          current?.expiresAt?.toDate?.() ?? new Date(),
        restoreCode: String(checkout.restoreCode ?? ""),
        newlyGranted: false,
      };
    }

    const now = new Date();

    const currentExpiry =
      currentEntitlement.data()?.expiresAt?.toDate?.() as
        Date | undefined;

    const start =
      currentExpiry && currentExpiry > now
        ? currentExpiry
        : now;

    const expiresAt =
      addMonthsUtc(start, PRICES[plan].months);

    const restoreCode = newRestoreCode();

    tx.set(
      entitlementRef,
      {
        email,
        emailHash: eHash,
        product: "lucky_dangle_premium",
        status: "active",
        plan,
        provider,
        environment: PAYMENT_ENVIRONMENT,
        paymentId,
        purchasedAt: Timestamp.fromDate(now),
        expiresAt: Timestamp.fromDate(expiresAt),
        restoreCodeHash: codeHash(restoreCode),
        updatedAt: Timestamp.fromDate(now),
      },
      { merge: true },
    );

    tx.set(paymentRef, {
      type: "premium",
      provider,
      environment: PAYMENT_ENVIRONMENT,
      paymentId,
      checkoutId,
      email,
      emailHash: eHash,
      plan,
      amount: Number(checkout.amount ?? 0),
      currency: String(
        checkout.currency ??
        (provider === "razorpay" ? "INR" : "USD"),
      ),
      status: "paid",
      createdAt: Timestamp.fromDate(now),
    });

    tx.set(
      checkoutRef,
      {
        status: "paid",
        paymentId,
        provider,
        restoreCode,
        expiresAt: Timestamp.fromDate(expiresAt),
        paidAt: Timestamp.fromDate(now),
      },
      { merge: true },
    );

    return {
      email,
      expiresAt,
      restoreCode,
      newlyGranted: true,
    };
  });
}
async function createRazorpayOrder(checkoutId: string, email: string, plan: PremiumPlan, amountPaise: number) {
  const auth = Buffer.from(
    `${RAZORPAY_KEY_ID.value()}:${RAZORPAY_KEY_SECRET.value()}`,
  ).toString("base64");

  const response = await fetch(
    "https://api.razorpay.com/v1/orders",
    {
      method: "POST",
      headers: {
        Authorization: `Basic ${auth}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        amount: amountPaise,
        currency: "INR",
        receipt: checkoutId.slice(0, 40),
        notes: {
          checkout_id: checkoutId,
          email_hash: emailHash(email),
          plan,
        },
      }),
    },
  );

  if (!response.ok) {
    const detail = await response.text();
    console.error("Razorpay order error", detail);
    throw new Error("Razorpay order creation failed.");
  }

  return await response.json() as { id: string };
}

async function createDodoPremiumCheckout(
  checkoutId: string,
  email: string,
  plan: PremiumPlan,
  amountUsdCents: number,
  productId: string,
) {
  const response = await fetch(
    "https://test.dodopayments.com/checkouts",
    {
      method: "POST",
      headers: {
        Authorization: `Bearer ${DODO_PAYMENTS_API_KEY.value()}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        product_cart: [
          {
            product_id: productId,
            quantity: 1,
            amount: amountUsdCents,
          },
        ],
        customer: {
          email,
          name: "Lucky Dangle Customer",
        },
        billing_currency: "USD",
        feature_flags: {
          allow_customer_editing_email: false,
          allow_currency_selection: false,
        },
        metadata: {
          product: "lucky_dangle",
          type: "premium",
          plan,
          checkout_id: checkoutId,
          email_hash: emailHash(email),
          environment: PAYMENT_ENVIRONMENT,
        },
        return_url:
          `${BASE_URL}/premiumReturn?checkoutId=` +
          encodeURIComponent(checkoutId),
      }),
    },
  );

  if (!response.ok) {
    const detail = await response.text();
    console.error("Dodo Premium checkout error", detail);
    throw new Error("Dodo checkout creation failed.");
  }

  const result = await response.json() as {
    session_id: string;
    checkout_url: string | null;
    payment_id?: string | null;
  };

  if (!result.session_id || !result.checkout_url) {
    throw new Error("Dodo returned an invalid checkout session.");
  }

  return result;
}
export const createPremiumCheckout = onRequest(
  {
    region: REGION,
    secrets: [
      RAZORPAY_KEY_ID,
      RAZORPAY_KEY_SECRET,
      DODO_PAYMENTS_API_KEY,
    ],
  },
  async (req, res) => {
    setCors(res);

    if (req.method === "OPTIONS") {
      res.status(204).send("");
      return;
    }

    if (req.method !== "POST") {
      res.status(405).json({ error: "POST required." });
      return;
    }

    try {
      const email =
        normalizeEmail(String(req.body?.email ?? ""));
      const verificationToken =
        String(req.body?.verificationToken ?? "").trim();

      const plan = req.body?.plan;
      const market =
        String(req.body?.market ?? "").toUpperCase();

      const requestedAmount =
        Number(req.body?.amount ?? 0);

      const runtimeConfig = await getRuntimeConfig();
      const premiumConfig = runtimeConfig.premium;
      const providerConfig = runtimeConfig.providers;

      if (!email || !email.includes("@")) {
        res.status(400).json({
          error: "Valid email required.",
        });
        return;
      }

      if (!isPlan(plan)) {
        res.status(400).json({
          error: "Invalid Premium plan.",
        });
        return;
      }

      const isIndia = market === "IN";

      const requestedMinor =
        Math.round(requestedAmount * 100);

      const minimumMinor = isIndia
        ? (
            plan === "premium_12m"
              ? premiumConfig.india12mMin
              : premiumConfig.india6mMin
          ) * 100
        : (
            plan === "premium_12m"
              ? premiumConfig.international12mMin
              : premiumConfig.international6mMin
          ) * 100;

      const maximumMinor = isIndia
        ? premiumConfig.indiaMax * 100
        : premiumConfig.internationalMax * 100;

      const currency = isIndia ? "INR" : "USD";

      if (
        !Number.isFinite(requestedMinor) ||
        requestedMinor < minimumMinor
      ) {
        res.status(400).json({
          error:
            `Amount must be at least ${currency} ${minimumMinor / 100}.`,
        });
        return;
      }

      if (requestedMinor > maximumMinor) {
        res.status(400).json({
          error:
            `Contribution amount cannot exceed ${currency} ${maximumMinor / 100}.`,
        });
        return;
      }

      if (isIndia && !providerConfig.razorpayEnabled) {
        res.status(503).json({
          error: "Premium payments are temporarily unavailable.",
        });
        return;
      }

      if (!isIndia && !providerConfig.dodoEnabled) {
        res.status(503).json({
          error:
            "International Premium payments are temporarily unavailable.",
        });
        return;
      }

      const verifiedForCheckout =
        await consumePremiumPurchaseVerification(
          email,
          verificationToken,
        );

      if (!verifiedForCheckout) {
        res.status(403).json({
          error: "Email verification required.",
        });
        return;
      }

      const checkoutRef =
        db.collection("premiumCheckouts").doc();

      if (!isIndia) {
        const productId =
          plan === "premium_12m"
            ? providerConfig.dodoPremium12mProductId
            : providerConfig.dodoPremium6mProductId;

        if (!productId) {
          res.status(503).json({
            error: "Dodo Premium product is not configured.",
          });
          return;
        }

        await checkoutRef.set({
          email,
          emailHash: emailHash(email),
          plan,
          market: "INTL",
          provider: "dodo",
          environment: PAYMENT_ENVIRONMENT,
          status: "created",
          amount: requestedMinor,
          currency: "USD",
          providerProductId: productId,
          createdAt: Timestamp.now(),
        });

        try {
          const session =
            await createDodoPremiumCheckout(
              checkoutRef.id,
              email,
              plan,
              requestedMinor,
              productId,
            );

          await checkoutRef.set(
            {
              providerOrderId: session.session_id,
              providerPaymentId:
                session.payment_id ?? null,
            },
            { merge: true },
          );

          res.json({
            checkoutId: checkoutRef.id,
            checkoutUrl: session.checkout_url,
            provider: "dodo",
          });
          return;
        } catch (error) {
          await checkoutRef.set(
            {
              status: "checkout_failed",
              updatedAt: Timestamp.now(),
            },
            { merge: true },
          );

          throw error;
        }
      }

      await checkoutRef.set({
        email,
        emailHash: emailHash(email),
        plan,
        market: "IN",
        provider: "razorpay",
        environment: PAYMENT_ENVIRONMENT,
        status: "created",
        amount: requestedMinor,
        currency: "INR",
        createdAt: Timestamp.now(),
      });

      const order =
        await createRazorpayOrder(
          checkoutRef.id,
          email,
          plan,
          requestedMinor,
        );

      await checkoutRef.set(
        {
          providerOrderId: order.id,
        },
        { merge: true },
      );

      res.json({
        checkoutId: checkoutRef.id,
        checkoutUrl:
          `${BASE_URL}/razorpayCheckout?checkoutId=` +
          encodeURIComponent(checkoutRef.id),
        provider: "razorpay",
      });
    } catch (error) {
      console.error(error);

      res.status(500).json({
        error:
          error instanceof Error
            ? error.message
            : "Unable to create checkout.",
      });
    }
  },
);
export const razorpayCheckout = onRequest(
  {
    region: REGION,
    secrets: [RAZORPAY_KEY_ID],
  },
  async (req, res) => {
    try {
      const checkoutId =
        String(req.query.checkoutId ?? "");

      const snap =
        await db.collection("premiumCheckouts")
          .doc(checkoutId)
          .get();

      if (!snap.exists) {
        res.status(404).send("Checkout not found.");
        return;
      }

      const data = snap.data()!;

      if (data.provider !== "razorpay") {
        res.status(400).send("Invalid checkout provider.");
        return;
      }

      const plan = data.plan as PremiumPlan;
      const amount =
        Number(data.amount ?? PRICES[plan].minimumInr);

      const html = `<!doctype html>
<html>
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<title>Lucky Dangle Premium</title>
<script src="https://checkout.razorpay.com/v1/checkout.js"></script>
<style>
body{font-family:Segoe UI,Arial,sans-serif;background:#151822;color:#fff;
display:flex;align-items:center;justify-content:center;min-height:100vh;margin:0}
.card{width:min(460px,88vw);background:#1d202b;border:1px solid #3a4050;
border-radius:24px;padding:34px;text-align:center}
h1{margin:0 0 8px}.muted{color:#aeb4c3}
.price{font-size:34px;font-weight:700;margin:24px 0;color:#ffd36a}
button{background:#ffc857;border:0;border-radius:13px;padding:14px 24px;
font-weight:700;font-size:16px;cursor:pointer}
</style>
</head>
<body>
<div class="card">
<h1>Lucky Dangle Premium</h1>
<div class="muted">Unlock all Premium Collections</div>
<div class="price">&#8377;${amount / 100}</div>
<button id="pay">Pay securely</button>
<p class="muted" id="status"></p>
</div>
<script>
const options = {
  key: ${JSON.stringify(RAZORPAY_KEY_ID.value())},
  amount: ${amount},
  currency: "INR",
  name: "Lucky Dangle",
  description: ${JSON.stringify(
    plan === "premium_12m"
      ? "Premium - 1 Year"
      : "Premium - 6 Months"
  )},
  order_id: ${JSON.stringify(data.providerOrderId)},
  prefill: {
    email: ${JSON.stringify(data.email)}
  },
  theme: {
    color: "#FFC857"
  },
  modal: {
    ondismiss: async function () {
      try {
        await fetch(
          ${JSON.stringify(`${BASE_URL}/markPremiumCheckoutState`)},
          {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
              checkoutId: ${JSON.stringify(checkoutId)},
              state: "cancelled"
            })
          }
        );
      } catch (_) {}

      document.getElementById("status").textContent =
        "Payment cancelled. You can return to Lucky Dangle.";
    }
  },
  handler: async function (r) {
    document.getElementById("status").textContent =
      "Verifying payment...";

    const response = await fetch(
      ${JSON.stringify(`${BASE_URL}/razorpayVerify`)},
      {
        method: "POST",
        headers: {
          "Content-Type":"application/json"
        },
        body: JSON.stringify({
          checkoutId: ${JSON.stringify(checkoutId)},
          razorpay_payment_id: r.razorpay_payment_id,
          razorpay_order_id: r.razorpay_order_id,
          razorpay_signature: r.razorpay_signature
        })
      }
    );

    const result = await response.json();

    if (response.ok) {
      document.querySelector(".card").innerHTML =
        "<h1>Payment successful &#10003;</h1>" +
        "<p class='muted'>Premium is being unlocked in Lucky Dangle. " +
        "You can return to the app.</p>";
    } else {
      document.getElementById("status").textContent =
        result.error || "Payment verification failed.";
    }
  }
};

const rzp = new Razorpay(options);


rzp.on("payment.failed", function () {
  document.getElementById("status").textContent =
    "Payment attempt failed. You can retry in Razorpay, or close the payment window to return to Lucky Dangle.";
});

document.getElementById("pay").onclick = () => rzp.open();

setTimeout(() => rzp.open(), 350);
</script>
</body>
</html>`;

      res.set("Content-Type", "text/html; charset=utf-8");
      res.status(200).send(html);
    } catch (error) {
      console.error(error);
      res.status(500).send("Unable to open checkout.");
    }
  },
);

export const razorpayVerify = onRequest(
  {
    region: REGION,
    secrets: [RAZORPAY_KEY_SECRET, SMTP_PASSWORD],
  },
  async (req, res) => {
    setCors(res);

    if (req.method === "OPTIONS") {
      res.status(204).send("");
      return;
    }

    try {
      const checkoutId =
        String(req.body?.checkoutId ?? "");

      const paymentId =
        String(req.body?.razorpay_payment_id ?? "");

      const returnedOrderId =
        String(req.body?.razorpay_order_id ?? "");

      const signature =
        String(req.body?.razorpay_signature ?? "");

      const checkoutSnap =
        await db.collection("premiumCheckouts")
          .doc(checkoutId)
          .get();

      if (!checkoutSnap.exists) {
        res.status(404).json({
          error: "Checkout not found.",
        });
        return;
      }

      const checkout = checkoutSnap.data()!;

      const expectedOrderId =
        String(checkout.providerOrderId ?? "");

      if (returnedOrderId !== expectedOrderId) {
        res.status(400).json({
          error: "Order mismatch.",
        });
        return;
      }

      const generated =
        crypto
          .createHmac(
            "sha256",
            RAZORPAY_KEY_SECRET.value(),
          )
          .update(
            `${expectedOrderId}|${paymentId}`,
          )
          .digest("hex");

      const valid =
        generated.length === signature.length &&
        crypto.timingSafeEqual(
          Buffer.from(generated),
          Buffer.from(signature),
        );

      if (!valid) {
        res.status(401).json({
          error: "Invalid payment signature.",
        });
        return;
      }

      const entitlement =
        await grantPremiumEntitlementFromVerifiedPayment(
          checkoutId,
          paymentId,
          "razorpay",
        );

      if (entitlement.newlyGranted) {
        try {
          await sendPremiumActivatedEmail({
            email: entitlement.email,
            plan: String(checkout.plan ?? ""),
            amountMinor: Number(checkout.amount ?? 0),
            currency: String(checkout.currency ?? "INR"),
            paymentId,
            expiresAt: entitlement.expiresAt,
          });
        } catch (mailError) {
          console.error(
            "Premium confirmation email failed",
            mailError,
          );
        }
      }

      res.json({
        ok: true,
        expiresAtUtc:
          entitlement.expiresAt.toISOString(),
      });
    } catch (error) {
      console.error(error);

      res.status(500).json({
        error: "Payment verification failed.",
      });
    }
  },
);

export const markPremiumCheckoutState = onRequest(
  { region: REGION },
  async (req, res) => {
    setCors(res);

    if (req.method === "OPTIONS") {
      res.status(204).send("");
      return;
    }

    if (req.method !== "POST") {
      res.status(405).json({ error: "POST required." });
      return;
    }

    const checkoutId =
      String(req.body?.checkoutId ?? "").trim();
    const state =
      String(req.body?.state ?? "").trim().toLowerCase();

    if (
      !checkoutId ||
      state !== "cancelled"
    ) {
      res.status(400).json({ error: "Only cancelled state is accepted." });
      return;
    }

    const ref =
      db.collection("premiumCheckouts").doc(checkoutId);
    const snap = await ref.get();

    if (!snap.exists) {
      res.status(404).json({ error: "Checkout not found." });
      return;
    }

    if (String(snap.data()?.status ?? "") !== "paid") {
      await ref.set(
        {
          status: state,
          updatedAt: Timestamp.now(),
        },
        { merge: true },
      );
    }

    res.json({ ok: true });
  },
);
export const premiumStatus = onRequest(
  { region: REGION },
  async (req, res) => {
    setCors(res);

    try {
      const checkoutId =
        String(req.query.checkoutId ?? "");

      if (!checkoutId) {
        res.status(400).json({
          error: "checkoutId required.",
        });
        return;
      }

      const snap =
        await db.collection("premiumCheckouts")
          .doc(checkoutId)
          .get();

      if (!snap.exists) {
        res.status(404).json({
          error: "Checkout not found.",
        });
        return;
      }

      const data = snap.data()!;

      if (
        data.status !== "paid" ||
        !data.expiresAt
      ) {
        res.json({
          status: data.status ?? "pending",
        });
        return;
      }

      const validationToken =
        await issueEntitlementValidationToken(
          String(data.email ?? ""),
        );

      res.json({
        status: "active",
        email: data.email,
        expiresAtUtc:
          data.expiresAt.toDate().toISOString(),
        restoreCode:
          data.restoreCode ?? "",
        validationToken,
      });
    } catch (error) {
      console.error(error);

      res.status(500).json({
        error: "Status lookup failed.",
      });
    }
  },
);

export const restorePremium = onRequest(
  { region: REGION },
  async (req, res) => {
    setCors(res);

    if (req.method === "OPTIONS") {
      res.status(204).send("");
      return;
    }

    try {
      const email =
        normalizeEmail(
          String(req.body?.email ?? ""),
        );

      const restoreCode =
        String(
          req.body?.restoreCode ?? "",
        ).trim();

      if (!email || !restoreCode) {
        res.status(400).json({
          error:
            "Email and restore code are required.",
        });
        return;
      }

      const snap =
        await db.collection("premiumEntitlements")
          .doc(entitlementDocId(email))
          .get();

      if (!snap.exists) {
        res.status(404).json({
          error: "Premium purchase not found.",
        });
        return;
      }

      const data = snap.data()!;

      const matches =
        data.restoreCodeHash ===
        codeHash(restoreCode);

      const expiresAt =
        data.expiresAt?.toDate?.() as
          Date | undefined;

      if (
        !matches ||
        !expiresAt ||
        expiresAt <= new Date()
      ) {
        res.status(401).json({
          error:
            "Restore code is invalid or Premium has expired.",
        });
        return;
      }

      res.json({
        status: "active",
        email,
        expiresAtUtc:
          expiresAt.toISOString(),
        restoreCode: "",
      });
    } catch (error) {
      console.error(error);

      res.status(500).json({
        error: "Restore failed.",
      });
    }
  },
);

export const premiumReturn = onRequest(
  { region: REGION },
  async (_req, res) => {
    res.set(
      "Content-Type",
      "text/html; charset=utf-8",
    );

    res.status(200).send(`<!doctype html>
<html>
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<title>Lucky Dangle Premium</title>
<style>
body{
  font-family:Segoe UI,Arial,sans-serif;
  background:#151822;
  color:#fff;
  display:flex;
  align-items:center;
  justify-content:center;
  min-height:100vh;
  margin:0;
}
.card{
  width:min(480px,88vw);
  background:#1d202b;
  border:1px solid #3a4050;
  border-radius:24px;
  padding:34px;
  text-align:center;
}
h1{
  margin:0 0 12px;
}
p{
  color:#aeb4c3;
  line-height:1.55;
  margin:8px 0;
}
</style>
</head>
<body>
<div class="card">
  <h1>Thank you!</h1>
  <p>Payment received. Lucky Dangle is verifying your Premium access.</p>
  <p>You can return to Lucky Dangle and close this browser tab.</p>
</div>
</body>
</html>`);
  },
);