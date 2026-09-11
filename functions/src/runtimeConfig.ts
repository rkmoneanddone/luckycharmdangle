import { onRequest } from "firebase-functions/v2/https";
import { initializeApp, getApps } from "firebase-admin/app";
import { getFirestore, Timestamp } from "firebase-admin/firestore";

if (getApps().length === 0) initializeApp();

const db = getFirestore();
const REGION = "asia-south1";
const CONFIG_CACHE_MS = 15 * 60 * 1000;

export type PublicRuntimeConfig = {
  supportEmail: string;
  privacyUrl: string;
  refundPolicyText?: string;
  emailSignature: string;
  coffee: {
    indiaDefault: number;
    indiaPresets: number[];
    indiaMin: number;
    indiaMax: number;
    internationalDefault: number;
    internationalPresets: number[];
    internationalMin: number;
    internationalMax: number;
  };
  premium: {
    india6mMin: number;
    india12mMin: number;
    indiaMax: number;
    international6mMin: number;
    international12mMin: number;
    internationalMax: number;
  };
  providers: {
    razorpayEnabled: boolean;
    dodoEnabled: boolean;
    dodoPremium6mProductId: string;
    dodoPremium12mProductId: string;
    dodoCoffeeProductId: string;
    
    dodoIndiaPremium6mProductId: string;
    dodoIndiaPremium12mProductId: string;
    dodoIndiaCoffeeProductId: string;
dodoLivePremium6mProductId: string;
    dodoLivePremium12mProductId: string;
    dodoLiveCoffeeProductId: string;
  
    dodoLiveIndiaPremium6mProductId: string;
    dodoLiveIndiaPremium12mProductId: string;
    dodoLiveIndiaCoffeeProductId: string;
};
  payments: {
    production: boolean;
  };
  updates: {
    latestVersion: string;
    storeUrl: string;
  };
};

export const DEFAULT_PUBLIC_CONFIG: PublicRuntimeConfig = {
  supportEmail: "connect@quickstories.in",
  privacyUrl: "https://www.quickstories.in/lucky-dangle-privacy.html",
  refundPolicyText:
    "Lucky Dangle Premium purchases are generally non-refundable. " +
    "Please verify the selected plan, email address, and payment amount " +
    "before completing payment. Refunds will only be provided where " +
    "required by applicable law. For payment or access issues, contact " +
    "connect@quickstories.in.",  emailSignature:
    "Rohit Kumar Mallick\n" +
    "Creator - Lucky Dangle\n" +
    "Founder - https://nirnexai.com/ , https://parentsboard.in/",
  coffee: {
    indiaDefault: 200,
    indiaPresets: [100, 150, 200],
    indiaMin: 100,
    indiaMax: 5000,
    internationalDefault: 3,
    internationalPresets: [3, 5, 10],
    internationalMin: 3,
    internationalMax: 100,
  },
  premium: {
    india6mMin: 299,
    india12mMin: 449,
    indiaMax: 19999,
    international6mMin: 6,
    international12mMin: 9,
    internationalMax: 200,
  },
  providers: {
    razorpayEnabled: false,
    dodoEnabled: true,
    dodoPremium6mProductId: "pdt_0NnCRAFAqYH04hR2VvDgu",
    dodoPremium12mProductId: "pdt_0NnCRSWuroWWdiVAHYjvm",
    dodoCoffeeProductId: "pdt_0NnCTFnzAO8ozDcPu1qTb",
    dodoIndiaPremium6mProductId: "pdt_0NnM8AQqHD1XEhLyzyT6C",
    dodoIndiaPremium12mProductId: "pdt_0NnM9jzfJLjhSgUA42MPD",
    dodoIndiaCoffeeProductId: "pdt_0NnMAhCAa15nvgyczzZ38",
    dodoLivePremium6mProductId: "pdt_0NnFS5cucmfjcOU1KUYfZ",
    dodoLivePremium12mProductId: "pdt_0NnFS5ndTNTTzhUXgDGXn",
    dodoLiveCoffeeProductId: "pdt_0NnFS5yrE6CWFoeXqX1fZ",
    dodoLiveIndiaPremium6mProductId: "pdt_0NnMA4RynJFsheY60hrkA",
    dodoLiveIndiaPremium12mProductId: "pdt_0NnMA4Eu9MLO1Nwm4qEaX",
    dodoLiveIndiaCoffeeProductId: "pdt_0NnMAth6EVGySudrhcQjR",
  },
  payments: {
    production: false,
  },
  updates: {
    // Current published Store version. Equal version = completely silent.
    latestVersion: "0.1.7.0",
    storeUrl: "https://apps.microsoft.com/detail/9N11M525D0M9",
  },
};

let cachedConfig: PublicRuntimeConfig | null = null;
let cachedConfigAt = 0;

function numberOr(value: unknown, fallback: number): number {
  const n = Number(value);
  return Number.isFinite(n) ? n : fallback;
}

function numberArrayOr(
  value: unknown,
  fallback: number[],
): number[] {
  if (!Array.isArray(value)) return fallback;

  const values = value
    .map((item) => Number(item))
    .filter((item) => Number.isFinite(item));

  return values.length > 0 ? values : fallback;
}

function mergeConfig(data: any): PublicRuntimeConfig {
  const d = data ?? {};
  const coffee = d.coffee ?? {};
  const premium = d.premium ?? {};
  const providers = d.providers ?? {};
  const payments = d.payments ?? {};
  const updates = d.updates ?? {};

  return {
    supportEmail:
      String(
        d.supportEmail ??
        DEFAULT_PUBLIC_CONFIG.supportEmail,
      ),
    privacyUrl:
      String(
        d.privacyUrl ??
        DEFAULT_PUBLIC_CONFIG.privacyUrl,
      ),
    refundPolicyText:
      String(
        d.refundPolicyText ??
        DEFAULT_PUBLIC_CONFIG.refundPolicyText ??
        "",
      ),    emailSignature:
      String(
        d.emailSignature ??
        DEFAULT_PUBLIC_CONFIG.emailSignature,
      ),
    coffee: {
      indiaDefault: numberOr(
        coffee.indiaDefault,
        DEFAULT_PUBLIC_CONFIG.coffee.indiaDefault,
      ),
      indiaPresets: numberArrayOr(
        coffee.indiaPresets,
        DEFAULT_PUBLIC_CONFIG.coffee.indiaPresets,
      ),
      indiaMin: numberOr(
        coffee.indiaMin,
        DEFAULT_PUBLIC_CONFIG.coffee.indiaMin,
      ),
      indiaMax: numberOr(
        coffee.indiaMax,
        DEFAULT_PUBLIC_CONFIG.coffee.indiaMax,
      ),
      internationalDefault: numberOr(
        coffee.internationalDefault,
        DEFAULT_PUBLIC_CONFIG.coffee.internationalDefault,
      ),
      internationalPresets: numberArrayOr(
        coffee.internationalPresets,
        DEFAULT_PUBLIC_CONFIG.coffee.internationalPresets,
      ),
      internationalMin: numberOr(
        coffee.internationalMin,
        DEFAULT_PUBLIC_CONFIG.coffee.internationalMin,
      ),
      internationalMax: numberOr(
        coffee.internationalMax,
        DEFAULT_PUBLIC_CONFIG.coffee.internationalMax,
      ),
    },
    premium: {
      india6mMin: numberOr(
        premium.india6mMin,
        DEFAULT_PUBLIC_CONFIG.premium.india6mMin,
      ),
      india12mMin: numberOr(
        premium.india12mMin,
        DEFAULT_PUBLIC_CONFIG.premium.india12mMin,
      ),
      indiaMax: numberOr(
        premium.indiaMax,
        DEFAULT_PUBLIC_CONFIG.premium.indiaMax,
      ),
      international6mMin: numberOr(
        premium.international6mMin,
        DEFAULT_PUBLIC_CONFIG.premium.international6mMin,
      ),
      international12mMin: numberOr(
        premium.international12mMin,
        DEFAULT_PUBLIC_CONFIG.premium.international12mMin,
      ),
      internationalMax: numberOr(
        premium.internationalMax,
        DEFAULT_PUBLIC_CONFIG.premium.internationalMax,
      ),
    },
    providers: {
      razorpayEnabled:
        providers.razorpayEnabled ??
        DEFAULT_PUBLIC_CONFIG.providers.razorpayEnabled,
      dodoEnabled:
        providers.dodoEnabled ??
        DEFAULT_PUBLIC_CONFIG.providers.dodoEnabled,
      dodoPremium6mProductId:
        String(
          providers.dodoPremium6mProductId ??
          DEFAULT_PUBLIC_CONFIG.providers.dodoPremium6mProductId,
        ),
      dodoPremium12mProductId:
        String(
          providers.dodoPremium12mProductId ??
          DEFAULT_PUBLIC_CONFIG.providers.dodoPremium12mProductId,
        ),
      dodoCoffeeProductId:
        String(
          providers.dodoCoffeeProductId ??
          DEFAULT_PUBLIC_CONFIG.providers.dodoCoffeeProductId,
        ),
      dodoIndiaPremium6mProductId: String(
        providers.dodoIndiaPremium6mProductId ??
        DEFAULT_PUBLIC_CONFIG.providers.dodoIndiaPremium6mProductId,
      ),
      dodoIndiaPremium12mProductId: String(
        providers.dodoIndiaPremium12mProductId ??
        DEFAULT_PUBLIC_CONFIG.providers.dodoIndiaPremium12mProductId,
      ),
      dodoIndiaCoffeeProductId: String(
        providers.dodoIndiaCoffeeProductId ??
        DEFAULT_PUBLIC_CONFIG.providers.dodoIndiaCoffeeProductId,
      ),
      dodoLivePremium6mProductId:
        String(
          providers.dodoLivePremium6mProductId ??
          DEFAULT_PUBLIC_CONFIG.providers.dodoLivePremium6mProductId,
        ),
      dodoLivePremium12mProductId:
        String(
          providers.dodoLivePremium12mProductId ??
          DEFAULT_PUBLIC_CONFIG.providers.dodoLivePremium12mProductId,
        ),
      dodoLiveCoffeeProductId:
        String(
          providers.dodoLiveCoffeeProductId ??
          DEFAULT_PUBLIC_CONFIG.providers.dodoLiveCoffeeProductId,
        ),
      dodoLiveIndiaPremium6mProductId: String(
        providers.dodoLiveIndiaPremium6mProductId ??
        DEFAULT_PUBLIC_CONFIG.providers.dodoLiveIndiaPremium6mProductId,
      ),
      dodoLiveIndiaPremium12mProductId: String(
        providers.dodoLiveIndiaPremium12mProductId ??
        DEFAULT_PUBLIC_CONFIG.providers.dodoLiveIndiaPremium12mProductId,
      ),
      dodoLiveIndiaCoffeeProductId: String(
        providers.dodoLiveIndiaCoffeeProductId ??
        DEFAULT_PUBLIC_CONFIG.providers.dodoLiveIndiaCoffeeProductId,
      ),
    },
    payments: {
      production:
        payments.production ??
        DEFAULT_PUBLIC_CONFIG.payments.production,
    },
    updates: {
      latestVersion:
        String(
          updates.latestVersion ??
          DEFAULT_PUBLIC_CONFIG.updates.latestVersion,
        ),
      storeUrl:
        String(
          updates.storeUrl ??
          DEFAULT_PUBLIC_CONFIG.updates.storeUrl,
        ),
    },
  };
}

export async function getRuntimeConfig(
  forceRefresh = false,
): Promise<PublicRuntimeConfig> {
  const now = Date.now();

  if (
    !forceRefresh &&
    cachedConfig &&
    now - cachedConfigAt < CONFIG_CACHE_MS
  ) {
    return cachedConfig;
  }

  const ref = db.collection("appConfig").doc("public");
  const snap = await ref.get();

  if (!snap.exists) {
    await ref.set({
      ...DEFAULT_PUBLIC_CONFIG,
      createdAt: Timestamp.now(),
      updatedAt: Timestamp.now(),
    });

    cachedConfig = DEFAULT_PUBLIC_CONFIG;
    cachedConfigAt = now;
    return cachedConfig;
  }

  cachedConfig = mergeConfig(snap.data());
  cachedConfigAt = now;
  return cachedConfig;
}

function setCors(res: any) {
  res.set("Access-Control-Allow-Origin", "*");
  res.set("Access-Control-Allow-Headers", "Content-Type");
  res.set("Access-Control-Allow-Methods", "GET,OPTIONS");
}

export const getPublicConfig = onRequest(
  { region: REGION },
  async (req, res) => {
    setCors(res);

    if (req.method === "OPTIONS") {
      res.status(204).send("");
      return;
    }

    if (req.method !== "GET") {
      res.status(405).json({ error: "GET required." });
      return;
    }

    try {
      const config = await getRuntimeConfig();
      res.set("Cache-Control", "public, max-age=300");
      res.json(config);
    } catch (error) {
      console.error("getPublicConfig failed", error);
      res.json(DEFAULT_PUBLIC_CONFIG);
    }
  },
);
