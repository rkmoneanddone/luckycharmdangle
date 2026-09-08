import nodemailer from "nodemailer";
import { defineSecret } from "firebase-functions/params";

export const SMTP_PASSWORD = defineSecret("SMTP_PASSWORD");

const SMTP_HOST = "smtp.hostinger.com";
const SMTP_PORT = 465;
const SMTP_USER = "connect@quickstories.in";
const FROM = '"Lucky Dangle" <connect@quickstories.in>';

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

function money(amountMinor: number, currency: string): string {
  const value = amountMinor / 100;
  if (currency === "INR") return `INR ${value.toFixed(2)}`;
  if (currency === "USD") return `USD ${value.toFixed(2)}`;
  return `${currency} ${value.toFixed(2)}`;
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
    input.plan === "premium_12m" ? "1 Year" : "6 Months";

  await transporter().sendMail({
    from: FROM,
    to: input.email,
    replyTo: SMTP_USER,
    subject: "Lucky Dangle Premium Activated",
    text:
`Thank you for supporting Lucky Dangle.

Premium has been activated successfully.

Plan: ${planText}
Amount: ${money(input.amountMinor, input.currency)}
Payment reference: ${input.paymentId}
Premium active until: ${input.expiresAt.toUTCString()}

Please keep this email for your records.

Support: connect@quickstories.in

Rohit Kumar Mallick
Lucky Dangle
Founder - https://nirnexai.com/

Lucky Dangle`,
  });
}

export async function sendRestoreOtpEmail(
  email: string,
  code: string,
) {
  await transporter().sendMail({
    from: FROM,
    to: email,
    replyTo: SMTP_USER,
    subject: "Your Lucky Dangle verification code",
    text:
`Your Lucky Dangle Premium verification code is:

${code}

This code expires in 10 minutes.

If you did not request this code, you can ignore this email.

Support: connect@quickstories.in

Rohit Kumar Mallick
Lucky Dangle
Founder - https://nirnexai.com/

Lucky Dangle`,
  });
}

export async function sendCoffeeThankYouEmail(input: {
  email: string;
  amountMinor: number;
  currency: string;
  paymentId: string;
}) {
  await transporter().sendMail({
    from: FROM,
    to: input.email,
    replyTo: SMTP_USER,
    subject: "Thank you for supporting Lucky Dangle",
    text:
`Thank you for buying Rohit a coffee and supporting Lucky Dangle.

Contribution: ${money(input.amountMinor, input.currency)}
Payment reference: ${input.paymentId}

Your support helps keep Lucky Dangle growing.

Support: connect@quickstories.in

Rohit Kumar Mallick
Lucky Dangle
Founder - https://nirnexai.com/

Lucky Dangle`,
  });
}