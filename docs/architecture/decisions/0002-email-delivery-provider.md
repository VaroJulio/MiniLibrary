# 0002 - Email Delivery Provider for Notifications

## Estado

Propuesto

## Contexto

MiniLibrary has a fully functional notification system (Req 19.1-19.10) that creates
in-app notifications and calls `INotificationService.SendEmailAsync()` for email delivery.
However, the current implementation is a placeholder that only logs the email content
without actually delivering it.

We need to choose an email delivery provider that:
- Works reliably in production (Azure-hosted)
- Has a free/low-cost tier for our scale (~100-500 emails/day max)
- Provides a .NET SDK or simple REST API
- Supports HTML emails
- Offers delivery tracking and bounce handling

## Options Considered

### Option A: SendGrid (Twilio)

- **Free tier**: 100 emails/day
- **Pros**: Excellent .NET SDK, proven reliability, good deliverability, easy setup
- **Cons**: Requires external account, rate limits on free tier
- **NuGet**: `SendGrid` (official SDK)

### Option B: Azure Communication Services (Email)

- **Free tier**: 100 emails/day (with Azure subscription)
- **Pros**: Native Azure integration, no external vendor, unified billing
- **Cons**: Newer service, less community support, requires Azure Communication resource
- **NuGet**: `Azure.Communication.Email`

### Option C: SMTP (Generic)

- **Pros**: Provider-agnostic, works with any SMTP server
- **Cons**: More config, less observability, no built-in bounce handling
- **NuGet**: Built-in `System.Net.Mail` or `MailKit`

## Decision

**TBD** - To be decided before implementation. Recommendation leans toward **SendGrid**
for its mature SDK, reliability, and simplicity. Final decision depends on whether the
project will remain Azure-only (favoring Option B) or multi-cloud.

## Consequences

- Need to add email provider credentials to environment variables and secrets
- Need to add MailHog/Papercut to docker-compose for local development testing
- Need HTML email templates (simple, responsive)
- Must handle delivery failures gracefully (no crash on send failure)
- Must respect user notification preferences (already implemented in domain)
