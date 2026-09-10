import { defineSecret } from "firebase-functions/params";

export type PaymentEnvironment = "test" | "live";

export const RAZORPAY_TEST_KEY_ID =
  defineSecret("RAZORPAY_KEY_ID");

export const RAZORPAY_TEST_KEY_SECRET =
  defineSecret("RAZORPAY_KEY_SECRET");

export const DODO_TEST_API_KEY =
  defineSecret("DODO_PAYMENTS_API_KEY");

export const DODO_TEST_WEBHOOK_SECRET =
  defineSecret("DODO_WEBHOOK_SECRET");

export const RAZORPAY_LIVE_KEY_ID =
  defineSecret("RAZORPAY_LIVE_KEY_ID");

export const RAZORPAY_LIVE_KEY_SECRET =
  defineSecret("RAZORPAY_LIVE_KEY_SECRET");

export const DODO_LIVE_API_KEY =
  defineSecret("DODO_LIVE_API_KEY");

export const DODO_LIVE_WEBHOOK_SECRET =
  defineSecret("DODO_LIVE_WEBHOOK_SECRET");

export function parsePaymentEnvironment(
  value: unknown,
): PaymentEnvironment {
  const normalized =
    String(value ?? "").trim().toLowerCase();

  if (normalized === "test" || normalized === "live") {
    return normalized;
  }

  throw new Error("Invalid payment environment.");
}

export function getRazorpayKeyId(
  environment: PaymentEnvironment,
): string {
  return environment === "live"
    ? RAZORPAY_LIVE_KEY_ID.value()
    : RAZORPAY_TEST_KEY_ID.value();
}

export function getRazorpayKeySecret(
  environment: PaymentEnvironment,
): string {
  return environment === "live"
    ? RAZORPAY_LIVE_KEY_SECRET.value()
    : RAZORPAY_TEST_KEY_SECRET.value();
}

export function getDodoApiKey(
  environment: PaymentEnvironment,
): string {
  return environment === "live"
    ? DODO_LIVE_API_KEY.value()
    : DODO_TEST_API_KEY.value();
}

export function getDodoWebhookSecret(
  environment: PaymentEnvironment,
): string {
  return environment === "live"
    ? DODO_LIVE_WEBHOOK_SECRET.value()
    : DODO_TEST_WEBHOOK_SECRET.value();
}

export function getDodoBaseUrl(
  environment: PaymentEnvironment,
): string {
  return environment === "live"
    ? "https://live.dodopayments.com"
    : "https://test.dodopayments.com";
}
