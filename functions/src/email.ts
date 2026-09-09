import nodemailer from "nodemailer";
import { defineSecret } from "firebase-functions/params";
import { initializeApp, getApps } from "firebase-admin/app";
import {
  FieldValue,
  getFirestore,
  Timestamp,
} from "firebase-admin/firestore";
import crypto from "node:crypto";
import { getRuntimeConfig } from "./runtimeConfig";

if (getApps().length === 0) initializeApp();

const db = getFirestore();

export const SMTP_PASSWORD = defineSecret("SMTP_PASSWORD");

const SMTP_HOST = "smtp.hostinger.com";
const SMTP_PORT = 465;
const SMTP_USER = "connect@quickstories.in";
const FROM = '"Lucky Dangle" <connect@quickstories.in>';

const TEMPLATE_CACHE_MS = 15 * 60 * 1000;

type MailTemplate = {
  subject: string;
  text: string;
  enabled: boolean;
};

type CachedTemplate = {
  value: MailTemplate;
  loadedAt: number;
};

const templateCache = new Map<string, CachedTemplate>();

const DEFAULT_TEMPLATES: Record<string, MailTemplate> = {
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
};

function transporter() {
  return nodemailer.createTransport({
    host: SMTP_HOST,
    port: SMTP_PORT,
    secure: true,
    auth: {
      user: SMTP_USER,
      pass: SMTP_PASSWORD.value(),
    },
  });
}

function money(
  amountMinor: number,
  currency: string,
): string {
  const value = amountMinor / 100;

  if (currency === "INR")
    return `INR ${value.toFixed(2)}`;

  if (currency === "USD")
    return `USD ${value.toFixed(2)}`;

  return `${currency} ${value.toFixed(2)}`;
}

async function loadTemplate(
  name: string,
): Promise<MailTemplate> {
  const fallback = DEFAULT_TEMPLATES[name];

  if (!fallback)
    throw new Error(`Unknown mail template: ${name}`);

  const cached = templateCache.get(name);

  if (
    cached &&
    Date.now() - cached.loadedAt < TEMPLATE_CACHE_MS
  ) {
    return cached.value;
  }

  try {
    const ref = db.collection("mailTemplates").doc(name);
    const snap = await ref.get();

    if (!snap.exists) {
      await ref.set({
        subject: fallback.subject,
        text: fallback.text,
        enabled: true,
        createdAt: Timestamp.now(),
        updatedAt: Timestamp.now(),
      });

      templateCache.set(name, {
        value: fallback,
        loadedAt: Date.now(),
      });

      return fallback;
    }

    const data = snap.data() ?? {};

    const value: MailTemplate = {
      enabled: data.enabled !== false,
      subject:
        String(data.subject ?? fallback.subject),
      text:
        String(data.text ?? fallback.text),
    };

    templateCache.set(name, {
      value,
      loadedAt: Date.now(),
    });

    return value;
  } catch (error) {
    console.error(
      `Mail template load failed: ${name}`,
      error,
    );

    return fallback;
  }
}

function render(
  input: string,
  values: Record<string, string>,
): string {
  return input.replace(
    /\{\{([A-Za-z0-9_]+)\}\}/g,
    (_match, key: string) => values[key] ?? "",
  );
}

function recipientHash(to: string): string {
  return crypto
    .createHash("sha256")
    .update(to.trim().toLowerCase())
    .digest("hex");
}

async function logDelivery(input: {
  type: string;
  to: string;
  status: "accepted" | "rejected" | "failed" | "disabled";
  messageId?: string;
  accepted?: unknown;
  rejected?: unknown;
  error?: unknown;
}) {
  try {
    await db.collection("mailDeliveries").add({
      type: input.type,
      recipientHash: recipientHash(input.to),
      status: input.status,
      messageId: input.messageId ?? "",
      accepted: input.accepted ?? [],
      rejected: input.rejected ?? [],
      error:
        input.error instanceof Error
          ? input.error.message
          : input.error
            ? String(input.error)
            : "",
      createdAt: FieldValue.serverTimestamp(),
    });
  } catch (logError) {
    console.error(
      "Mail delivery audit write failed",
      logError,
    );
  }
}

async function sendTemplate(
  name: string,
  to: string,
  values: Record<string, string>,
) {
  const template = await loadTemplate(name);

  if (!template.enabled) {
    console.log("Lucky Dangle email disabled", {
      type: name,
      recipientHash: recipientHash(to),
    });

    await logDelivery({
      type: name,
      to,
      status: "disabled",
    });

    return;
  }

  const runtimeConfig =
    await getRuntimeConfig();

  const supportEmail =
    runtimeConfig.supportEmail || SMTP_USER;

  const common = {
    supportEmail,
    signature: runtimeConfig.emailSignature,
    ...values,
  };

  try {
    const info = await transporter().sendMail({
      from: FROM,
      to,
      replyTo: supportEmail,
      subject: render(template.subject, common),
      text: render(template.text, common),
    });

    const rejected =
      Array.isArray(info.rejected)
        ? info.rejected
        : [];

    const status =
      rejected.length > 0
        ? "rejected"
        : "accepted";

    console.log("Lucky Dangle email result", {
      type: name,
      recipientHash: recipientHash(to),
      messageId: info.messageId,
      accepted: info.accepted,
      rejected: info.rejected,
    });

    await logDelivery({
      type: name,
      to,
      status,
      messageId: info.messageId,
      accepted: info.accepted,
      rejected: info.rejected,
    });
  } catch (error) {
    console.error(
      "Lucky Dangle email send failed",
      {
        type: name,
        recipientHash: recipientHash(to),
        error,
      },
    );

    await logDelivery({
      type: name,
      to,
      status: "failed",
      error,
    });

    throw error;
  }
}

export async function sendPremiumActivatedEmail(input: {
  email: string;
  plan: string;
  amountMinor: number;
  currency: string;
  paymentId: string;
  expiresAt: Date;
}) {
  const planText =
    input.plan === "premium_12m"
      ? "1 Year"
      : "6 Months";

  await sendTemplate(
    "premiumActivated",
    input.email,
    {
      plan: planText,
      amount:
        money(
          input.amountMinor,
          input.currency,
        ),
      paymentId: input.paymentId,
      expiresAt: input.expiresAt.toUTCString(),
    },
  );
}

export async function sendPurchaseOtpEmail(
  email: string,
  code: string,
) {
  await sendTemplate(
    "purchaseOtp",
    email,
    { otp: code },
  );
}

export async function sendRestoreOtpEmail(
  email: string,
  code: string,
) {
  await sendTemplate(
    "restoreOtp",
    email,
    { otp: code },
  );
}

export async function sendCoffeeThankYouEmail(input: {
  email: string;
  amountMinor: number;
  currency: string;
  paymentId: string;
}) {
  await sendTemplate(
    "coffeeThankYou",
    input.email,
    {
      amount:
        money(
          input.amountMinor,
          input.currency,
        ),
      paymentId: input.paymentId,
    },
  );
}