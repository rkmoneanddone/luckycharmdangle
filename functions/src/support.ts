import { onRequest } from "firebase-functions/v2/https";
import { defineSecret } from "firebase-functions/params";
import { initializeApp, getApps } from "firebase-admin/app";
import { getFirestore, Timestamp } from "firebase-admin/firestore";
import crypto from "node:crypto";
import { SMTP_PASSWORD, sendCoffeeThankYouEmail } from "./email";
import { getRuntimeConfig } from "./runtimeConfig";
import {
  DODO_LIVE_API_KEY,
  DODO_TEST_API_KEY,
  RAZORPAY_LIVE_KEY_ID,
  RAZORPAY_LIVE_KEY_SECRET,
  RAZORPAY_TEST_KEY_ID,
  RAZORPAY_TEST_KEY_SECRET,
  PaymentEnvironment,
  getDodoApiKey,
  getDodoBaseUrl,
  getRazorpayKeyId,
  getRazorpayKeySecret,
  parsePaymentEnvironment,
  resolvePaymentEnvironment,
} from "./paymentEnvironment";

if (getApps().length === 0) initializeApp();

const db = getFirestore();
const REGION = "asia-south1";
const BASE_URL =
  "https://asia-south1-lucky-dangle.cloudfunctions.net";



const PAYMENT_ENVIRONMENT =
  String(process.env.PAYMENT_ENVIRONMENT ?? "test")
    .trim()
    .toLowerCase() === "live"
    ? "live"
    : "test";

const INDIA_MIN_PAISE = 10000;
const INDIA_MAX_PAISE = 500000;

const INTL_MIN_CENTS = 300;
const INTL_MAX_CENTS = 10000;

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

function isValidEmail(email: string): boolean {
  return /^[^@\s]+@[^@\s]+\.[A-Za-z]{2,}$/.test(email);
}

async function createRazorpayOrder(
  checkoutId: string,
  email: string,
  amountPaise: number,
  environment: PaymentEnvironment,
) {
  const auth = Buffer.from(
    `${getRazorpayKeyId(environment)}:${getRazorpayKeySecret(environment)}`,
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
          type: "coffee",
        },
      }),
    },
  );

  if (!response.ok) {
    const detail = await response.text();
    console.error("Razorpay coffee order error", detail);
    throw new Error("Razorpay order creation failed.");
  }

  return await response.json() as { id: string };
}

async function createDodoCoffeeCheckout(
  checkoutId: string,
  email: string,
  amountMinor: number,
  currency: "INR" | "USD",
  productId: string,
  environment: PaymentEnvironment,
) {
  const response = await fetch(
    `${getDodoBaseUrl(environment)}/checkouts`,
    {
      method: "POST",
      headers: {
        Authorization: `Bearer ${getDodoApiKey(environment)}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        product_cart: [
          {
            product_id: productId,
            quantity: 1,
            amount: amountMinor,
          },
        ],
        customer: {
          email,
          name: "Lucky Dangle Supporter",
        },
        billing_currency: currency,
        feature_flags: {
          allow_customer_editing_email: false,
          allow_currency_selection: false,
        },
        metadata: {
          product: "lucky_dangle",
          type: "coffee",
          checkout_id: checkoutId,
          email_hash: emailHash(email),
          environment,
        },
        return_url:
          `${BASE_URL}/coffeeReturn?checkoutId=` +
          encodeURIComponent(checkoutId),
      }),
    },
  );

  if (!response.ok) {
    const detail = await response.text();
    console.error("Dodo Coffee checkout error", detail);
    throw new Error("Dodo coffee checkout creation failed.");
  }

  const result = await response.json() as {
    session_id: string;
    checkout_url: string | null;
    payment_id?: string | null;
  };

  if (!result.session_id || !result.checkout_url) {
    throw new Error("Dodo returned an invalid Coffee checkout session.");
  }

  return result;
}
export const createCoffeeCheckout = onRequest(
  {
    region: REGION,
    secrets: [
      DODO_TEST_API_KEY,
      DODO_LIVE_API_KEY,
    ],
  },
  async (req, res) => {
    setCors(res);

    if (req.method === "OPTIONS") {
      res.status(204).send("");
      return;
    }

    if (req.method !== "POST") {
      res.status(405).json({
        error: "Method not allowed.",
      });
      return;
    }

    try {
      const email =
        normalizeEmail(String(req.body?.email ?? ""));

      const market =
        String(req.body?.market ?? "").toUpperCase();

      const requestedAmount =
        Number(req.body?.amount ?? 0);

      const runtimeConfig =
        await getRuntimeConfig(true);

      const environment =
        resolvePaymentEnvironment(runtimeConfig);

      const coffeeConfig =
        runtimeConfig.coffee;

      const providerConfig =
        runtimeConfig.providers;

      if (!isValidEmail(email)) {
        res.status(400).json({
          error: "Valid email required.",
        });
        return;
      }

      if (market !== "IN" && market !== "INTL") {
        res.status(400).json({
          error: "Invalid market.",
        });
        return;
      }

      if (!providerConfig.dodoEnabled) {
        res.status(503).json({
          error: "Coffee payments are temporarily unavailable.",
        });
        return;
      }

      const isIndia =
        market === "IN";

      const amountMinor =
        Math.round(requestedAmount * 100);

      const minimumMinor =
        (isIndia
          ? coffeeConfig.indiaMin
          : coffeeConfig.internationalMin) * 100;

      const maximumMinor =
        (isIndia
          ? coffeeConfig.indiaMax
          : coffeeConfig.internationalMax) * 100;

      if (
        !Number.isFinite(amountMinor) ||
        amountMinor < minimumMinor ||
        amountMinor > maximumMinor
      ) {
        res.status(400).json({
          error: "Coffee amount is outside the allowed range.",
        });
        return;
      }

      const currency: "INR" | "USD" =
        isIndia ? "INR" : "USD";

      const productId =
        environment === "live"
          ? (
              isIndia
                ? providerConfig.dodoLiveIndiaCoffeeProductId
                : providerConfig.dodoLiveCoffeeProductId
            )
          : (
              isIndia
                ? providerConfig.dodoIndiaCoffeeProductId
                : providerConfig.dodoCoffeeProductId
            );

      if (!productId) {
        res.status(503).json({
          error: "Dodo Coffee product is not configured.",
        });
        return;
      }

      const checkoutRef =
        db.collection("supportCheckouts").doc();

      await checkoutRef.set({
        type: "coffee",
        email,
        emailHash: emailHash(email),
        market,
        provider: "dodo",
        environment,
        status: "created",
        currency,
        providerProductId: productId,
        createdAt: Timestamp.now(),
      });

      try {
        const session =
          await createDodoCoffeeCheckout(
            checkoutRef.id,
            email,
            amountMinor,
            currency,
            productId,
            environment,
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

        console.error(
          "Coffee checkout creation failed",
          error,
        );

        res.status(502).json({
          error: "Could not start Coffee payment.",
        });
        return;
      }
    } catch (error) {
      console.error(
        "createCoffeeCheckout failed",
        error,
      );

      res.status(500).json({
        error: "Could not create Coffee checkout.",
      });
    }
  },
);
export const coffeeRazorpayCheckout = onRequest(
  {
    region: REGION,
    secrets: [RAZORPAY_TEST_KEY_ID, RAZORPAY_LIVE_KEY_ID],
  },
  async (req, res) => {
    try {
      const checkoutId =
        String(req.query.checkoutId ?? "");

      const snap =
        await db.collection("supportCheckouts")
          .doc(checkoutId)
          .get();

      if (!snap.exists) {
        res.status(404).send("Checkout not found.");
        return;
      }

      const data = snap.data()!;
      const environment =
        parsePaymentEnvironment(data.environment);

      if (
        data.type !== "coffee" ||
        data.provider !== "razorpay"
      ) {
        res.status(400).send("Invalid checkout.");
        return;
      }

      const amount =
        Number(data.amount ?? INDIA_MIN_PAISE);

      const html = `<!doctype html>
<html>
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<title>Buy Rohit a Coffee</title>
<script src="https://checkout.razorpay.com/v1/checkout.js"></script>
<style>
body{font-family:Segoe UI,Arial,sans-serif;background:#151822;color:#fff;
display:flex;align-items:center;justify-content:center;min-height:100vh;margin:0}
.card{width:min(460px,88vw);background:#1d202b;border:1px solid #5a4926;
border-radius:24px;padding:34px;text-align:center}
h1{margin:0 0 8px}.muted{color:#aeb4c3}
.price{font-size:34px;font-weight:700;margin:24px 0;color:#ffd36a}
button{background:#ffc857;border:0;border-radius:13px;padding:14px 24px;
font-weight:700;font-size:16px;cursor:pointer}
</style>
</head>
<body>
<div class="card">
<h1>Buy Rohit a Coffee</h1>
<div class="muted">Support Lucky Dangle</div>
<div class="price">&#8377;${amount / 100}</div>
<button id="pay">Pay securely</button>
<p class="muted" id="status"></p>
</div>
<script>
const options = {
  key: ${JSON.stringify(getRazorpayKeyId(environment))},
  amount: ${amount},
  currency: "INR",
  name: "Lucky Dangle",
  description: "Buy Rohit a Coffee",
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
          ${JSON.stringify(`${BASE_URL}/markCoffeeCheckoutState`)},
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
      ${JSON.stringify(`${BASE_URL}/coffeeRazorpayVerify`)},
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
        "<h1>Thank you &#10003;</h1>" +
        "<p class='muted'>Your support means a lot. " +
        "You can return to Lucky Dangle.</p>";
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

export const coffeeRazorpayVerify = onRequest(
  {
    region: REGION,
    secrets: [
      RAZORPAY_TEST_KEY_SECRET,
      RAZORPAY_LIVE_KEY_SECRET,
      SMTP_PASSWORD,
    ],
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

      const checkoutRef =
        db.collection("supportCheckouts").doc(checkoutId);

      const checkoutSnap = await checkoutRef.get();

      if (!checkoutSnap.exists) {
        res.status(404).json({
          error: "Checkout not found.",
        });
        return;
      }

      const checkout = checkoutSnap.data()!;
      const environment =
        parsePaymentEnvironment(checkout.environment);

      if (
        checkout.type !== "coffee" ||
        checkout.provider !== "razorpay"
      ) {
        res.status(400).json({
          error: "Invalid checkout.",
        });
        return;
      }

      if (
        !paymentId ||
        !returnedOrderId ||
        !signature
      ) {
        res.status(400).json({
          error: "Missing Razorpay verification data.",
        });
        return;
      }

      if (returnedOrderId !== checkout.providerOrderId) {
        res.status(400).json({
          error: "Order verification failed.",
        });
        return;
      }

      const expected = crypto
        .createHmac("sha256", getRazorpayKeySecret(environment))
        .update(`${returnedOrderId}|${paymentId}`)
        .digest("hex");

      const expectedBuffer =
        Buffer.from(expected, "utf8");
      const signatureBuffer =
        Buffer.from(signature, "utf8");

      const validSignature =
        expectedBuffer.length === signatureBuffer.length &&
        crypto.timingSafeEqual(
          expectedBuffer,
          signatureBuffer,
        );

      if (!validSignature) {
        res.status(400).json({
          error: "Payment signature verification failed.",
        });
        return;
      }

      const paymentRef =
        db.collection("payments")
          .doc(`${environment}_coffee_razorpay_${paymentId}`);

      const priorPayment = await paymentRef.get();

      if (!priorPayment.exists) {
        const now = new Date();

        await paymentRef.set({
          type: "coffee",
          provider: "razorpay",
          environment,
          paymentId,
          checkoutId,
          email: checkout.email,
          emailHash: checkout.emailHash,
          amount: checkout.amount,
          currency: checkout.currency,
          status: "paid",
          createdAt: Timestamp.fromDate(now),
        });

        await checkoutRef.set(
          {
            status: "paid",
            paymentId,
            paidAt: Timestamp.fromDate(now),
          },
          { merge: true },
        );

        try {
          await sendCoffeeThankYouEmail({
            email: String(checkout.email ?? ""),
            amountMinor: Number(checkout.amount ?? 0),
            currency: String(checkout.currency ?? "INR"),
            paymentId,
          });
        } catch (mailError) {
          console.error(
            "Coffee thank-you email failed",
            mailError,
          );
        }
      }

      res.json({
        ok: true,
        status: "paid",
      });
    } catch (error) {
      console.error(error);

      res.status(500).json({
        error:
          error instanceof Error
            ? error.message
            : "Payment verification failed.",
      });
    }
  },
);
export const coffeeReturn = onRequest(
  { region: REGION },
  async (req, res) => {
    const checkoutId =
      String(req.query.checkoutId ?? "").trim();

    let status = "processing";

    if (checkoutId) {
      const snap =
        await db.collection("supportCheckouts")
          .doc(checkoutId)
          .get();

      if (
        snap.exists &&
        snap.data()?.type === "coffee"
      ) {
        status =
          String(
            snap.data()?.status ?? "processing",
          );
      }
    }

    const terminalFailure =
      status === "failed" ||
      status === "checkout_failed";

    const heading =
      status === "paid"
        ? "Thank you!"
        : status === "cancelled"
          ? "Payment cancelled"
          : terminalFailure
            ? "Payment failed"
            : "Payment processing";

    const message =
      status === "paid"
        ? "Payment confirmed. Thank you for supporting Lucky Dangle."
        : status === "cancelled"
          ? "The payment was cancelled. No successful payment was recorded."
          : terminalFailure
            ? "The payment could not be completed. You can return to Lucky Dangle and try again."
            : "Lucky Dangle is waiting for final payment confirmation.";

    const html = `<!doctype html>
<html>
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1">
<title>Lucky Dangle - Payment Status</title>
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
  width:min(620px,88vw);
  background:#1d202b;
  border:1px solid #5a4926;
  border-radius:24px;
  padding:42px;
  text-align:center;
}
h1{margin:0 0 16px}
p{color:#d7dbea;font-size:18px;line-height:1.5}
</style>
</head>
<body>
<div class="card">
<h1>${heading}</h1>
<p>${message}</p>
<p>You can return to Lucky Dangle and close this browser tab.</p>
</div>
</body>
</html>`;

    res.set(
      "Content-Type",
      "text/html; charset=utf-8",
    );
    res.status(200).send(html);
  },
);
export const markCoffeeCheckoutState = onRequest(
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
      db.collection("supportCheckouts").doc(checkoutId);
    const snap = await ref.get();

    if (!snap.exists || snap.data()?.type !== "coffee") {
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
export const coffeeStatus = onRequest(
  { region: REGION },
  async (req, res) => {
    setCors(res);

    if (req.method === "OPTIONS") {
      res.status(204).send("");
      return;
    }

    const checkoutId =
      String(
        req.method === "GET"
          ? req.query.checkoutId ?? ""
          : req.body?.checkoutId ?? "",
      ).trim();

    if (!checkoutId) {
      res.status(400).json({
        error: "Checkout id required.",
      });
      return;
    }

    const snap =
      await db.collection("supportCheckouts")
        .doc(checkoutId)
        .get();

    if (!snap.exists) {
      res.status(404).json({
        error: "Checkout not found.",
      });
      return;
    }

    const data = snap.data()!;

    if (data.type !== "coffee") {
      res.status(400).json({
        error: "Invalid checkout.",
      });
      return;
    }

    res.json({
      status: String(data.status ?? "created"),
      provider: String(data.provider ?? ""),
      currency: String(data.currency ?? ""),
      amount: Number(data.amount ?? 0),
      paymentId: String(data.paymentId ?? ""),
    });
  },
);
