import { onRequest } from "firebase-functions/v2/https";
import { initializeApp, getApps } from "firebase-admin/app";
import { getFirestore, Timestamp } from "firebase-admin/firestore";
import crypto from "node:crypto";
import { SMTP_PASSWORD, sendPurchaseOtpEmail } from "./email";

if (getApps().length === 0) initializeApp();

const db = getFirestore();
const REGION = "asia-south1";

const PAYMENT_ENVIRONMENT =
  String(process.env.PAYMENT_ENVIRONMENT ?? "test")
    .trim()
    .toLowerCase() === "live"
    ? "live"
    : "test";

function setCors(res: any) {
  res.set("Access-Control-Allow-Origin", "*");
  res.set("Access-Control-Allow-Headers", "Content-Type");
  res.set("Access-Control-Allow-Methods", "POST,OPTIONS");
}

function normalizeEmail(email: string): string {
  return email.trim().toLowerCase();
}

function isValidEmail(email: string): boolean {
  return /^[^@\s]+@[^@\s]+\.[A-Za-z]{2,}$/.test(email);
}

function emailHash(email: string): string {
  return crypto
    .createHash("sha256")
    .update(normalizeEmail(email))
    .digest("hex");
}

function otpHash(email: string, code: string): string {
  return crypto
    .createHash("sha256")
    .update(`${normalizeEmail(email)}:${code}`)
    .digest("hex");
}

function verificationTokenHash(token: string): string {
  return crypto
    .createHash("sha256")
    .update(token)
    .digest("hex");
}

function newOtp(): string {
  return crypto.randomInt(100000, 1000000).toString();
}

export const sendPremiumPurchaseOtp = onRequest(
  { region: REGION, secrets: [SMTP_PASSWORD] },
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
      const email = normalizeEmail(
        String(req.body?.email ?? ""),
      );

      if (!isValidEmail(email)) {
        res.status(400).json({
          error: "Valid email required.",
        });
        return;
      }

      const eHash = emailHash(email);

      const otpRef =
        db.collection("premiumPurchaseOtps")
          .doc(`${PAYMENT_ENVIRONMENT}_${eHash}`);

      const prior = await otpRef.get();

      if (prior.exists) {
        const sentAt =
          prior.data()?.sentAt?.toDate?.() as Date | undefined;

        if (sentAt && Date.now() - sentAt.getTime() < 60000) {
          res.status(429).json({
            error:
              "Please wait 60 seconds before requesting another code.",
          });
          return;
        }
      }

      const code = newOtp();
      const now = new Date();
      const expiry =
        new Date(now.getTime() + 10 * 60 * 1000);

      await otpRef.set({
        emailHash: eHash,
        environment: PAYMENT_ENVIRONMENT,
        codeHash: otpHash(email, code),
        attempts: 0,
        consumed: false,
        sentAt: Timestamp.fromDate(now),
        expiresAt: Timestamp.fromDate(expiry),
      });

      await sendPurchaseOtpEmail(email, code);

      res.json({ ok: true });
    } catch (error) {
      console.error("sendPremiumPurchaseOtp failed", error);
      res.status(500).json({
        error: "Unable to send verification code.",
      });
    }
  },
);

export const verifyPremiumPurchaseOtp = onRequest(
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

    try {
      const email = normalizeEmail(
        String(req.body?.email ?? ""),
      );
      const code = String(req.body?.code ?? "").trim();

      if (!isValidEmail(email) || !/^\d{6}$/.test(code)) {
        res.status(400).json({
          error: "Enter the 6-digit verification code.",
        });
        return;
      }

      const eHash = emailHash(email);

      const otpRef =
        db.collection("premiumPurchaseOtps")
          .doc(`${PAYMENT_ENVIRONMENT}_${eHash}`);

      const otpSnap = await otpRef.get();

      if (!otpSnap.exists) {
        res.status(400).json({
          error: "Invalid or expired verification code.",
        });
        return;
      }

      const otp = otpSnap.data()!;
      const expiry =
        otp.expiresAt?.toDate?.() as Date | undefined;
      const attempts = Number(otp.attempts ?? 0);

      if (otp.consumed || !expiry || expiry <= new Date()) {
        res.status(400).json({
          error: "Invalid or expired verification code.",
        });
        return;
      }

      if (attempts >= 5) {
        res.status(429).json({
          error:
            "Too many verification attempts. Request a new code.",
        });
        return;
      }

      if (otp.codeHash !== otpHash(email, code)) {
        await otpRef.update({
          attempts: attempts + 1,
        });

        res.status(400).json({
          error: "Invalid verification code.",
        });
        return;
      }

      const token = crypto.randomBytes(32).toString("base64url");
      const verifiedAt = new Date();
      const tokenExpiry =
        new Date(verifiedAt.getTime() + 15 * 60 * 1000);

      await otpRef.update({
        consumed: true,
        verifiedAt: Timestamp.fromDate(verifiedAt),
        verificationTokenHash:
          verificationTokenHash(token),
        verificationTokenExpiresAt:
          Timestamp.fromDate(tokenExpiry),
      });

      res.json({
        ok: true,
        email,
        verificationToken: token,
        expiresAtUtc: tokenExpiry.toISOString(),
      });
    } catch (error) {
      console.error("verifyPremiumPurchaseOtp failed", error);
      res.status(500).json({
        error: "Unable to verify code.",
      });
    }
  },
);

export async function consumePremiumPurchaseVerification(
  email: string,
  token: string,
): Promise<boolean> {
  const normalized = normalizeEmail(email);

  if (!isValidEmail(normalized) || !token) {
    return false;
  }

  const eHash = emailHash(normalized);

  const otpRef =
    db.collection("premiumPurchaseOtps")
      .doc(`${PAYMENT_ENVIRONMENT}_${eHash}`);

  return db.runTransaction(async (tx) => {
    const snap = await tx.get(otpRef);

    if (!snap.exists) return false;

    const data = snap.data()!;
    const tokenExpiry =
      data.verificationTokenExpiresAt?.toDate?.() as
        Date | undefined;

    if (
      !data.consumed ||
      data.checkoutConsumed === true ||
      !tokenExpiry ||
      tokenExpiry <= new Date() ||
      data.verificationTokenHash !==
        verificationTokenHash(token)
    ) {
      return false;
    }

    tx.update(otpRef, {
      checkoutConsumed: true,
      checkoutConsumedAt: Timestamp.now(),
    });

    return true;
  });
}