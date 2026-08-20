# Branded Authentication Email Concept

## Status and purpose

This document records an exploratory Vennusign design option. It is not an approved work package, architecture decision, implementation authorization, roadmap commitment, or a claim that any of the described capability exists today.

It captures a gap observed on 2026-08-19 between an approved authentication decision and the behaviour that shipped, together with the options for closing it, so the choice is available later rather than rediscovered.

## Observed behaviour

Customer account verification codes on `dev` are sent by Microsoft, not by Vennusign. The message a customer receives has:

- sender `account-security-noreply@accountprotection.microsoft.com`;
- subject "Your Vennusign Customers account verification code";
- body headed "Vennusign Customers / Account verification code", carrying an 8-digit code valid for 30 minutes;
- a footer reading "Microsoft Corporation, One Microsoft Way, Redmond, WA" and a link to Microsoft's privacy statement.

The tenant display name is the only Vennusign-controlled element. Everything else is Microsoft's default one-time-passcode template.

## Conflict with an approved decision

`docs/design/approved/authentication/decisions.md`, decision 3, states:

> **Entra is never surfaced as a brand.** No Microsoft or Entra logos, no "powered by" text, no redirect through a visibly Microsoft-branded domain the customer would notice. The experience reads as Vennusign's own login end to end.

A verification email arriving from a `microsoft.com` domain with a Microsoft Corporation postal footer does not satisfy that decision. The decision was written about the sign-in surface; the transactional email attached to that surface was not considered at the time, and the default behaviour was inherited rather than chosen.

This is a gap between approved design and shipped behaviour, not a defect in the authentication flow itself. Sign-in works correctly.

## Options

### Option A — Company branding only

Tenant-level branding (logo, background, colours, sign-in text) applies to the hosted sign-in experience and the tenant name shown in the email.

- Cost: portal configuration, no code, no new infrastructure.
- Limit: **does not** change the sender address, the Microsoft footer, the template, or the subject line. Decision 3 remains unsatisfied.
- Reasonable as an interim improvement, not as the answer.

### Option B — Custom email provider

Microsoft Entra External ID can delegate one-time-passcode delivery to a system Vennusign owns. Entra raises an `OnOtpSend` event to a REST endpoint registered as a custom authentication extension; that endpoint receives the passcode and sends the message through a provider of Vennusign's choosing, controlling template, subject, sender address, and localization.

- Requires: a REST endpoint (an Azure Function is the documented shape), an email provider (Azure Communication Services and SendGrid are both documented), a custom authentication extension registered in the CIAM tenant, and the app permissions that extension needs.
- Effect: removes Microsoft from the customer-visible path entirely and satisfies decision 3.
- Relevant existing capability: `vennusign.com` mail already moved to Zoho (2026-08-19), so a sending identity on the correct domain partly exists. Whether Zoho is the right transactional-send provider, versus Azure Communication Services alongside the rest of the Azure footprint, is open.

## Decisions required

- Whether decision 3 is intended to cover transactional email, or only the interactive sign-in surface. If only the latter, this document closes with no action and decision 3 should be amended to say so explicitly.
- Which provider sends Vennusign transactional mail, and whether authentication email shares that path with other product email (invitations, alerts, receipts) or stays separate.
- Whether the same treatment is expected for every authentication message (verification code, password reset, and any future notification), or only account verification.
- Where the custom extension endpoint is hosted and how it is deployed, given it would sit outside the current `deploy-dev.yml` app set.
- Whether this is worth doing before real customers see the current email, which determines whether it is scheduled or left as a known gap.

## Notes

The gap is invisible from the code side: nothing in `src/Vennu.Api` sends this message, and no configuration in the repository controls it. It is entirely tenant-side behaviour, which is why it survived review of the authentication implementation.

References: [custom email provider for OTP send events](https://learn.microsoft.com/en-us/entra/identity-platform/custom-extension-email-otp-get-started), [email one-time passcode authentication](https://learn.microsoft.com/en-us/entra/external-id/one-time-passcode), [custom authentication extensions overview](https://learn.microsoft.com/en-us/entra/identity-platform/custom-extension-overview).
