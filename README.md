# Unmanned Lock System - LINE LIFF x QR x SmartLock

LINE LIFF内で購入→QR発行→iPad店舗スキャン→サーバ判定→解錠を実現する無人入室システム。

## Architecture

```
[LINE LIFF App] → [ASP.NET Core API] → [PostgreSQL]
                       ↓
[iPad Scanner]  → [API /device/scan] → [SmartLock Adapter (Mock)]
                       ↓
[Admin Dashboard] → [API /admin/*]
```

## Quick Start (Local Development)

### 1. Docker Compose (Backend + DB)

```bash
docker compose up -d
```

Backend: http://localhost:5000
Swagger UI: http://localhost:5000/swagger

### 2. Frontend Dev Servers

```bash
# Customer LIFF App
cd frontend/liff-app && npm run dev    # http://localhost:5173

# Device Scanner
cd frontend/device-app && npm run dev  # http://localhost:5174

# Admin Dashboard
cd frontend/admin-app && npm run dev   # http://localhost:5175
```

### Default Credentials (Dev)

- **Admin Login**: admin@example.com / admin123
- **Device API Key**: dev-device-key-12345
- **Mock LINE User**: test-user-001 (auto-set in dev mode)

## API Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | /api/auth/line-login | - | LINE login |
| POST | /api/auth/admin/login | - | Admin login |
| GET | /api/plans | - | List plans |
| POST | /api/checkout/create | Customer | Create checkout session |
| POST | /api/checkout/mock-complete/{id} | Customer | Mock payment (dev) |
| POST | /api/webhooks/stripe | - | Stripe webhook |
| GET | /api/passes/me | Customer | My passes |
| POST | /api/passes/{id}/qr | Customer | Generate QR token |
| POST | /api/device/scan | Device | Scan QR token |
| GET | /api/admin/passes | Admin | List all passes |
| POST | /api/admin/passes/{id}/revoke | Admin | Revoke pass |
| GET | /api/admin/logs | Admin | Access logs |

## Pricing

| Plan | Price | Duration |
|------|-------|----------|
| 1 Hour | 600 JPY | 60 min |
| 1 Day | 2,400 JPY | 24 hours |
| Monthly | 9,800 JPY | 30 days |

## Railway Deployment

### 1. Create Railway Project

```bash
railway login
railway init
```

### 2. Add PostgreSQL

Railway Dashboard → New → Database → PostgreSQL

### 3. Set Environment Variables

```
UseMockPayment=true
UseMockAuth=true
DeviceApiKey=<generate-secure-key>
Jwt__QrSecret=<generate-32-char-min-secret>
Jwt__AdminSecret=<generate-32-char-min-secret>
Cors__Origins=https://<your-railway-domain>
```

### 4. Deploy

```bash
railway up
```

## LINE LIFF Setup (When Ready)

1. Go to [LINE Developers Console](https://developers.line.biz/)
2. Create a Provider → Channel (LINE Login)
3. Create LIFF App:
   - Endpoint URL: `https://<your-domain>/liff/`
   - Scope: `profile`, `openid`
4. Set environment variables:
   ```
   Liff__LiffId=<your-liff-id>
   Liff__ChannelId=<your-channel-id>
   Liff__ChannelSecret=<your-channel-secret>
   UseMockAuth=false
   ```

## Stripe Setup (When Ready)

1. Create [Stripe Account](https://stripe.com/)
2. Get API keys from Dashboard → Developers → API keys
3. Set webhook endpoint: `https://<your-domain>/api/webhooks/stripe`
4. Subscribe to events: `checkout.session.completed`
5. Set environment variables:
   ```
   Stripe__SecretKey=sk_live_xxx
   Stripe__WebhookSecret=whsec_xxx
   UseMockPayment=false
   ```

## Security

- QR tokens: JWT HS256, 90s TTL, single-use, door-bound
- Scan: Serializable transaction, SELECT FOR UPDATE
- Webhook: Stripe signature verification + idempotency
- Fail-safe: Default deny, unlock failure does not rollback pass
- Rate limiting: 10 req/10s on scan endpoint

## Future Extensions (Section 12 - Hooks Ready)

- Multi-store / Multi-door (store_id on all entities)
- Device management (nullable DeviceId on AccessLog)
- Zones / Seats (nullable ZoneId, SeatId)
- Booking system (nullable BookingId)
- Overage billing
- Membership / Subscription management
- Coupons
- Refunds / Remote unlock
- Admin 2FA, Device signing, Key rotation
