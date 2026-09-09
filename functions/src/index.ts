import { onRequest } from "firebase-functions/v2/https";

export const health = onRequest(
  { region: "asia-south1" },
  (_req, res) => {
    res.status(200).json({
      ok: true,
      service: "lucky-dangle",
    });
  },
);

export * from "./premium";
export * from "./restoreOtp";
export * from "./support";
export * from "./runtimeConfig";
export * from "./purchaseOtp";
export * from "./entitlementValidation";
export * from "./mailTemplateInitializer";
export * from "./dodoWebhook";
