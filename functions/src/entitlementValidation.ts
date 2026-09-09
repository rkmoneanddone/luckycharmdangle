import { onRequest } from "firebase-functions/v2/https";
import { initializeApp, getApps } from "firebase-admin/app";
import { getFirestore, Timestamp } from "firebase-admin/firestore";
import crypto from "node:crypto";

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

function emailHash(email: string): string {
  return crypto
    .createHash("sha256")
    .update(normalizeEmail(email))
    .digest("hex");
}

function tokenHash(token: string): string {
  return crypto
    .createHash("sha256")
    .update(token)
    .digest("hex");
}

function entitlementDocId(email: string): string {
  return `${PAYMENT_ENVIRONMENT}_${emailHash(email)}`;
}

export async function issueEntitlementValidationToken(
  email: string,
): Promise<string> {
  const token = crypto.randomBytes(32).toString("base64url");

  await db.collection("premiumEntitlements")
    .doc(entitlementDocId(email))
    .set(
      {
        validationTokenHash: tokenHash(token),
        validationTokenIssuedAt: Timestamp.now(),
        updatedAt: Timestamp.now(),
      },
      { merge: true },
    );

  return token;
}

export const revalidatePremiumEntitlement = onRequest(
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
      const email =
        normalizeEmail(String(req.body?.email ?? ""));
      const validationToken =
        String(req.body?.validationToken ?? "").trim();

      if (!email || !email.includes("@") || !validationToken) {
        res.status(400).json({
          error: "Email and validation token are required.",
        });
        return;
      }

      const snap =
        await db.collection("premiumEntitlements")
          .doc(entitlementDocId(email))
          .get();

      if (!snap.exists) {
        res.status(401).json({ status: "inactive" });
        return;
      }

      const data = snap.data()!;
      const expectedHash =
        String(data.validationTokenHash ?? "");
      const suppliedHash = tokenHash(validationToken);

      const expectedBuffer = Buffer.from(expectedHash, "utf8");
      const suppliedBuffer = Buffer.from(suppliedHash, "utf8");

      const validToken =
        expectedBuffer.length === suppliedBuffer.length &&
        crypto.timingSafeEqual(
          expectedBuffer,
          suppliedBuffer,
        );

      if (!validToken) {
        res.status(401).json({
          status: "inactive",
          error: "Entitlement validation failed.",
        });
        return;
      }

      const expiresAt =
        data.expiresAt?.toDate?.() as Date | undefined;

      if (
        data.status !== "active" ||
        !expiresAt ||
        expiresAt <= new Date()
      ) {
        res.json({
          status: "inactive",
          email,
          expiresAtUtc:
            expiresAt?.toISOString?.() ?? null,
        });
        return;
      }

      res.json({
        status: "active",
        email,
        expiresAtUtc: expiresAt.toISOString(),
      });
    } catch (error) {
      console.error(
        "revalidatePremiumEntitlement failed",
        error,
      );

      res.status(500).json({
        error: "Unable to validate Premium entitlement.",
      });
    }
  },
);