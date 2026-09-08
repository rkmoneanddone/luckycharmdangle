# Lucky Dangle Firebase Backend

This folder contains the server-side payment and entitlement backend for Lucky Dangle.

## Current Phase

Phase 1 creates only the safe backend foundation.

No Razorpay or Dodo secret is stored here.
No payment gateway is live yet.
No desktop application code is changed by this phase.

## Planned payment purposes

### Premium

India:
- 6 months: INR 199
- 12 months: INR 299
- Provider: Razorpay

International:
- 6 months: USD 6
- 12 months: USD 9
- Provider: Dodo Payments

Premium is a fixed-duration entitlement with manual renewal and no automatic renewal.

### Support / Coffee

India presets:
- INR 100
- INR 150
- INR 200
- Custom amount

Support payments never create a premium entitlement.

International support will use Dodo Payments once the merchant account is approved.

## Planned Firestore collections

- customers
- payments
- entitlements
- supportPayments
- webhookEvents

All payment and entitlement writes are server-side only.
The WPF desktop client does not receive provider secret keys and does not directly write payment state.