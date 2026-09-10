import { onRequest } from "firebase-functions/v2/https";
import { issueEntitlementValidationToken } from "./entitlementValidation";
import { initializeApp, getApps } from "firebase-admin/app";
import { getFirestore, Timestamp } from "firebase-admin/firestore";
import crypto from "node:crypto";
import { SMTP_PASSWORD, sendRestoreOtpEmail } from "./email";
import { getRuntimeConfig } from "./runtimeConfig";
import {
  PaymentEnvironment,
  resolvePaymentEnvironment,
} from "./paymentEnvironment";

if (getApps().length === 0) initializeApp();

const db = getFirestore();
const REGION = "asia-south1";


function setCors(res: any) {
  res.set("Access-Control-Allow-Origin", "*");
  res.set("Access-Control-Allow-Headers", "Content-Type");
  res.set("Access-Control-Allow-Methods", "POST,OPTIONS");
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

function entitlementDocId(
  email: string,
  environment: PaymentEnvironment,
): string {
  return `${environment}_${emailHash(email)}`;
}
function otpHash(email: string, code: string): string {
  return crypto
    .createHash("sha256")
    .update(`${normalizeEmail(email)}:${code}`)
    .digest("hex");
}

function newOtp(): string {
  return crypto.randomInt(100000, 1000000).toString();
}

export const sendRestoreOtp = onRequest(
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

    const email = normalizeEmail(String(req.body?.email ?? ""));

    if (!email || !email.includes("@")) {
      res.status(400).json({ error: "Valid email required." });
      return;
    }

    const runtimeConfig = await getRuntimeConfig(true);
    const environment =
      resolvePaymentEnvironment(runtimeConfig);

    const eHash = emailHash(email);
    const entitlementRef =
      db.collection("premiumEntitlements").doc(entitlementDocId(email, environment));
    const entitlementSnap = await entitlementRef.get();

    // Do not reveal whether the account exists.
    if (!entitlementSnap.exists) {
      res.json({ ok: true });
      return;
    }

    const entitlement = entitlementSnap.data()!;
    const expiresAt =
      entitlement.expiresAt?.toDate?.() as Date | undefined;

    if (!expiresAt || expiresAt <= new Date()) {
      res.json({ ok: true });
      return;
    }

    const otpRef =
      db.collection("premiumRestoreOtps")
        .doc(`${environment}_${eHash}`);

    const prior = await otpRef.get();

    if (prior.exists) {
      const sentAt =
        prior.data()?.sentAt?.toDate?.() as Date | undefined;

      if (sentAt && Date.now() - sentAt.getTime() < 60000) {
        res.status(429).json({
          error: "Please wait 60 seconds before requesting another code.",
        });
        return;
      }
    }

    const code = newOtp();
    const now = new Date();
    const expiry = new Date(now.getTime() + 10 * 60 * 1000);

    await otpRef.set({
      emailHash: eHash,
      environment,
      codeHash: otpHash(email, code),
      attempts: 0,
      consumed: false,
      sentAt: Timestamp.fromDate(now),
      expiresAt: Timestamp.fromDate(expiry),
    });

    await sendRestoreOtpEmail(email, code);

    res.json({ ok: true });
  },
);

export const verifyRestoreOtp = onRequest(
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

    const email = normalizeEmail(String(req.body?.email ?? ""));
    const code = String(req.body?.code ?? "").trim();

    if (!email || !/^\d{6}$/.test(code)) {
      res.status(400).json({
        error: "Enter the 6-digit verification code.",
      });
      return;
    }

    const runtimeConfig = await getRuntimeConfig(true);
    const environment =
      resolvePaymentEnvironment(runtimeConfig);

    const eHash = emailHash(email);

    const otpRef =
      db.collection("premiumRestoreOtps")
        .doc(`${environment}_${eHash}`);

    const entitlementRef =
      db.collection("premiumEntitlements").doc(entitlementDocId(email, environment));

    const otpSnap = await otpRef.get();
    const entitlementSnap = await entitlementRef.get();

    if (!otpSnap.exists || !entitlementSnap.exists) {
      res.status(400).json({
        error: "Invalid or expired verification code.",
      });
      return;
    }

    const otp = otpSnap.data()!;
    const entitlement = entitlementSnap.data()!;
    const otpExpiry =
      otp.expiresAt?.toDate?.() as Date | undefined;
    const attempts = Number(otp.attempts ?? 0);

    if (otp.consumed || !otpExpiry || otpExpiry <= new Date()) {
      res.status(400).json({
        error: "Invalid or expired verification code.",
      });
      return;
    }

    if (attempts >= 5) {
      res.status(429).json({
        error: "Too many verification attempts. Request a new code.",
      });
      return;
    }

    if (otp.codeHash !== otpHash(email, code)) {
      await otpRef.update({ attempts: attempts + 1 });

      res.status(400).json({
        error: "Invalid verification code.",
      });
      return;
    }

    const premiumExpiry =
      entitlement.expiresAt?.toDate?.() as Date | undefined;

    if (!premiumExpiry || premiumExpiry <= new Date()) {
      res.status(400).json({
        error: "No active Premium purchase was found.",
      });
      return;
    }

    await otpRef.update({
      consumed: true,
      verifiedAt: Timestamp.now(),
    });

    const validationToken =
      await issueEntitlementValidationToken(
        email,
        environment,
      );

    res.json({
      status: "active",
      email,
      expiresAtUtc: premiumExpiry.toISOString(),
      restoreCode: "",
      validationToken,
    });
  },
);
