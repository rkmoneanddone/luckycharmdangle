export type PaymentProvider = "razorpay" | "dodo";
export type PaymentPurpose = "premium" | "support";
export type PremiumPlan = "premium_6m" | "premium_12m";
export type PaymentStatus =
  | "created"
  | "pending"
  | "paid"
  | "failed"
  | "refunded";

export interface PremiumPricing {
  plan: PremiumPlan;
  durationMonths: 6 | 12;
  indiaAmountPaise: number;
  internationalAmountCents: number;
}

export const PREMIUM_PRICING: readonly PremiumPricing[] = [
  {
    plan: "premium_6m",
    durationMonths: 6,
    indiaAmountPaise: 19900,
    internationalAmountCents: 600
  },
  {
    plan: "premium_12m",
    durationMonths: 12,
    indiaAmountPaise: 29900,
    internationalAmountCents: 900
  }
] as const;

export const INDIA_SUPPORT_PRESETS_PAISE = [
  10000,
  15000,
  20000
] as const;
export interface PremiumEntitlement {
  customerId: string;
  product: "lucky_dangle_premium";
  plan: PremiumPlan;
  provider: PaymentProvider;
  paymentId: string;
  status: "active" | "expired" | "revoked";
  purchasedAt: string;
  expiresAt: string;
}