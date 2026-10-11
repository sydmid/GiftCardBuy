# 🗺️ GiftCardBuy Platform Roadmap & Feature Backlog

Production-grade Persian (fa-IR, RTL) digital gift-card commerce platform built with .NET 10.

---

## 🏗️ Architecture & Development Guidelines

1. **CQRS / Vertical Slices**: Place commands, queries, and handlers inside `src/GiftStore.Application/UseCases/<Domain>`.
2. **Domain Isolation**: Pure domain aggregates, value objects, and domain events inside `src/GiftStore.Domain/Aggregates`.
3. **Data Protection at Rest**: Encrypt all voucher codes, secrets, and PINs using `IEncryptionService`.
4. **Audit Logging**: Audit sensitive user/staff operations using `AuditEvent`.
5. **Persian Localization (RTL / fa-IR)**: User-facing UI and error messages MUST be in Persian.
6. **Testing**: Write unit tests for domain rules in `tests/GiftStore.Domain.Tests` and use cases in `tests/GiftStore.Application.Tests`.

---

## 🎯 Feature Backlog & Priority Implementation Plan

### Milestone 1: Core Wallet & Rapid Checkout
- [ ] **Feature #1: Pre-Funded Toman Wallet Aggregate & Handlers**
  - **Goal**: Enable instant 1-click checkout without passing through bank gateways on every order.
  - **Domain**: `src/GiftStore.Domain/Aggregates/Wallet/Wallet.cs`, `WalletTransaction.cs`
  - **Application**: `src/GiftStore.Application/UseCases/Wallet/WalletUseCases.cs` (`DepositFundsCommand`, `PayWithWalletCommand`)
  - **Tests**: `tests/GiftStore.Domain.Tests/WalletTests.cs`, `tests/GiftStore.Application.Tests/WalletUseCasesTests.cs`

### Milestone 2: Social Gifting & Personalization
- [ ] **Feature #2: Social Gifting Engine ("ارسال به عنوان هدیه")**
  - **Goal**: Allow purchasing gift cards for friends with customized e-card themes, dynamic Persian text, and scheduled delivery.
  - **Domain**: `GiftingDetails` value object attached to `Order` aggregate.
  - **Application**: `src/GiftStore.Application/UseCases/Checkout/` update `CreateOrderCommand` with `SendAsGift`, `RecipientMobile`, `ScheduledAtUtc`.
  - **Background Worker**: Scheduled dispatch job via Quartz.NET.

### Milestone 3: Anti-Fraud & Compliance
- [ ] **Feature #3: Automated Iranian KYC & Bank Card Verification**
  - **Goal**: Cross-check buyer's National ID (کد ملی), mobile number, and bank card (شماره کارت/شبا) via Shahkar/Finnotech API to prevent phishing.
  - **Application**: `src/GiftStore.Application/UseCases/KYC/VerifyCustomerCardCommand.cs`
  - **Security**: Risk scoring engine flagging high-value first-time purchases for manual staff approval.

### Milestone 4: Multi-Channel Fulfillment
- [ ] **Feature #4: SMS & Telegram Bot Notification Dispatch**
  - **Goal**: Dispatch fulfilled voucher codes automatically via SMS (Kavenegar API) and Telegram Bot.
  - **Infrastructure**: `src/GiftStore.Infrastructure/Notifications/KavenegarSmsService.cs`, `TelegramBotService.cs`
  - **Outbox**: Outbox event processor sending encrypted/masked reveal links.

### Milestone 5: Brand Experience & Knowledge Base
- [ ] **Feature #5: Interactive Brand Redemption Guides Engine**
  - **Goal**: Embedded step-by-step copyable instructions per product brand (PlayStation, Apple, Steam, Xbox).
  - **Domain**: `StoreProduct` aggregate extension (`RedemptionGuideMarkdown`, `BrandRegion`).
  - **UI**: Razor Pages UI rendering interactive copy buttons and region compatibility tags.

### Milestone 6: Enterprise & B2B Expansion
- [ ] **Feature #6: B2B Corporate Bulk Purchasing & Official Tax Invoicing**
  - **Goal**: Dedicated corporate dashboard for batch gift card orders with official tax invoicing (فاکتور رسمی).
  - **Domain**: Corporate customer account type, bulk purchase request aggregate.
  - **Application**: Batch CSV/Excel order processing handler.
