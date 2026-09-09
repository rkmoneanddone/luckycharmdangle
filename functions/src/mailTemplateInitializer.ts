import { onRequest } from "firebase-functions/v2/https";
import { initializeApp, getApps } from "firebase-admin/app";
import { getFirestore, Timestamp } from "firebase-admin/firestore";
import {
  DEFAULT_PUBLIC_CONFIG,
  getRuntimeConfig,
} from "./runtimeConfig";

if (getApps().length === 0) initializeApp();

const db = getFirestore();
const REGION = "asia-south1";

const DEFAULT_TEMPLATES = {
  premiumActivated: {
    enabled: true,
    subject: "Lucky Dangle Premium Activated",
    text:
`Thank you for supporting Lucky Dangle.

Premium has been activated successfully.

Plan: {{plan}}
Amount: {{amount}}
Payment reference: {{paymentId}}
Premium active until: {{expiresAt}}

Please keep this email for your records.

Support: {{supportEmail}}

{{signature}}`,
  },
  purchaseOtp: {
    enabled: true,
    subject: "Verify your email for Lucky Dangle Premium",
    text:
`Your Lucky Dangle Premium verification code is:

{{otp}}

This code expires in 10 minutes.

Enter this code in Lucky Dangle before starting payment.

If you did not request this code, you can ignore this email.

Support: {{supportEmail}}

{{signature}}`,
  },
  restoreOtp: {
    enabled: true,
    subject: "Your Lucky Dangle verification code",
    text:
`Your Lucky Dangle Premium verification code is:

{{otp}}

This code expires in 10 minutes.

If you did not request this code, you can ignore this email.

Support: {{supportEmail}}

{{signature}}`,
  },
  coffeeThankYou: {
    enabled: true,
    subject: "Thank you for supporting Lucky Dangle",
    text:
`Thank you for buying Rohit a coffee and supporting Lucky Dangle.

Contribution: {{amount}}
Payment reference: {{paymentId}}

Your support helps keep Lucky Dangle growing.

Support: {{supportEmail}}

{{signature}}`,
  },
} as const;

function setCors(res: any) {
  res.set("Access-Control-Allow-Origin", "*");
  res.set("Access-Control-Allow-Headers", "Content-Type");
  res.set("Access-Control-Allow-Methods", "POST,OPTIONS");
}

export const initializeMailTemplates = onRequest(
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
      const now = Timestamp.now();

      const configRef =
        db.collection("appConfig").doc("public");

      const configSnap = await configRef.get();

      if (!configSnap.exists) {
        await configRef.set({
          ...DEFAULT_PUBLIC_CONFIG,
          createdAt: now,
          updatedAt: now,
        });
      } else {
        const data = configSnap.data() ?? {};

        const patch: Record<string, unknown> = {};

        if (!data.emailSignature) {
          patch.emailSignature =
            DEFAULT_PUBLIC_CONFIG.emailSignature;
        }

        if (!data.refundPolicyText) {
          patch.refundPolicyText =
            DEFAULT_PUBLIC_CONFIG.refundPolicyText;
        }

        if (!data.supportEmail) {
          patch.supportEmail =
            DEFAULT_PUBLIC_CONFIG.supportEmail;
        }

        if (!data.privacyUrl) {
          patch.privacyUrl =
            DEFAULT_PUBLIC_CONFIG.privacyUrl;
        }

        if (Object.keys(patch).length > 0) {
          patch.updatedAt = now;
          await configRef.set(
            patch,
            { merge: true },
          );
        }
      }

      const created: string[] = [];
      const existing: string[] = [];

      for (const [name, template] of
        Object.entries(DEFAULT_TEMPLATES)) {
        const ref =
          db.collection("mailTemplates").doc(name);

        const snap = await ref.get();

        if (!snap.exists) {
          await ref.set({
            ...template,
            createdAt: now,
            updatedAt: now,
          });
          created.push(name);
        } else {
          existing.push(name);
        }
      }

      await getRuntimeConfig(true);

      res.json({
        ok: true,
        created,
        existing,
        expectedTemplates:
          Object.keys(DEFAULT_TEMPLATES),
      });
    } catch (error) {
      console.error(
        "initializeMailTemplates failed",
        error,
      );

      res.status(500).json({
        error:
          "Unable to initialize Lucky Dangle mail templates.",
      });
    }
  },
);