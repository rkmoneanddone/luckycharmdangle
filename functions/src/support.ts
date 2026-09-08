import { onRequest } from "firebase-functions/v2/https";
import { defineSecret } from "firebase-functions/params";
import { initializeApp, getApps } from "firebase-admin/app";
import { getFirestore, Timestamp } from "firebase-admin/firestore";
import crypto from "node:crypto";
import { SMTP_PASSWORD, sendCoffeeThankYouEmail } from "./email";

if (getApps().length === 0) initializeApp();

const db = getFirestore();
const REGION = "asia-south1";
const BASE_URL =
  "https://asia-south1-lucky-dangle.cloudfunctions.net";

const RAZORPAY_KEY_ID = defineSecret("RAZORPAY_KEY_ID");
const RAZORPAY_KEY_SECRET = defineSecret("RAZORPAY_KEY_SECRET");

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
) {
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

export const createCoffeeCheckout = onRequest(
  {
    region: REGION,
    secrets: [
      RAZORPAY_KEY_ID,
      RAZORPAY_KEY_SECRET,
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

      const market =
        String(req.body?.market ?? "").toUpperCase();

      const requestedAmount =
        Number(req.body?.amount ?? 0);

      if (!isValidEmail(email)) {
        res.status(400).json({
          error: "Valid email required.",
        });
        return;
      }

      if (market === "IN") {
        const amountPaise =
          Math.round(requestedAmount * 100);

        if (
          !Number.isFinite(amountPaise) ||
          amountPaise < INDIA_MIN_PAISE ||
          amountPaise > INDIA_MAX_PAISE
        ) {
          res.status(400).json({
            error:
              "Coffee contribution must be between INR 100 and INR 5,000.",
          });
          return;
        }

        const checkoutRef =
          db.collection("supportCheckouts").doc();

        await checkoutRef.set({
          type: "coffee",
          email,
          emailHash: emailHash(email),
          market: "IN",
          provider: "razorpay",
          environment: PAYMENT_ENVIRONMENT,
          status: "created",
          amount: amountPaise,
          currency: "INR",
          createdAt: Timestamp.now(),
        });

        const order =
          await createRazorpayOrder(
            checkoutRef.id,
            email,
            amountPaise,
          );

        await checkoutRef.set(
          { providerOrderId: order.id },
          { merge: true },
        );

        res.json({
          checkoutId: checkoutRef.id,
          checkoutUrl:
            `${BASE_URL}/coffeeRazorpayCheckout?checkoutId=` +
            encodeURIComponent(checkoutRef.id),
          provider: "razorpay",
        });

        return;
      }

      const amountCents =
        Math.round(requestedAmount * 100);

      if (
        !Number.isFinite(amountCents) ||
        amountCents < INTL_MIN_CENTS ||
        amountCents > INTL_MAX_CENTS
      ) {
        res.status(400).json({
          error:
            "Coffee contribution must be between USD 3 and USD 100.",
        });
        return;
      }

      // Dodo adapter plugs in here later. The WPF client remains unchanged.
      res.status(503).json({
        error:
          "International coffee payments are temporarily unavailable while Dodo approval is pending.",
      });
    } catch (error) {
      console.error(error);

      res.status(500).json({
        error:
          error instanceof Error
            ? error.message
            : "Unable to create coffee checkout.",
      });
    }
  },
);

export const coffeeRazorpayCheckout = onRequest(
  {
    region: REGION,
    secrets: [RAZORPAY_KEY_ID],
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
  key: ${JSON.stringify(RAZORPAY_KEY_ID.value())},
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
      RAZORPAY_KEY_SECRET,
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
        .createHmac("sha256", RAZORPAY_KEY_SECRET.value())
        .update(`${returnedOrderId}|${paymentId}`)
        .digest("hex");

      if (
        !crypto.timingSafeEqual(
          Buffer.from(expected, "utf8"),
          Buffer.from(signature, "utf8"),
        )
      ) {
        res.status(400).json({
          error: "Payment signature verification failed.",
        });
        return;
      }

      const paymentRef =
        db.collection("payments")
          .doc(`${PAYMENT_ENVIRONMENT}_coffee_razorpay_${paymentId}`);

      const priorPayment = await paymentRef.get();

      if (!priorPayment.exists) {
        const now = new Date();

        await paymentRef.set({
          type: "coffee",
          provider: "razorpay",
          environment: PAYMENT_ENVIRONMENT,
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
