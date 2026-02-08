# Backend (Docker)

## Local development with Docker Compose

1. Set required environment variables in your shell:
   - `POSTGRES_DB`
   - `POSTGRES_USER`
   - `POSTGRES_PASSWORD`
   - `STRIPE_SECRET_KEY`
   - `STRIPE_WEBHOOK_SECRET`
   - `STRIPE_PUBLISHABLE_KEY`
   - `CORS_ORIGINS`
2. Run:
   ```bash
   docker compose up --build
   ```
3. The API is available at `http://localhost:5000`.

## Run the container alone (VPS/PaaS)

1. Build:
   ```bash
   docker build -t ecommerce-webapi .
   ```
2. Run:
   ```bash
   docker run --rm -p 5000:5000 \
     -e ConnectionStrings__DbConnection="Host=<db-host>;Port=5432;Database=<db-name>;Username=<db-user>;Password=<db-password>" \
     -e Stripe__SecretKey="<stripe-secret-key>" \
     -e Stripe__WebhookSecret="<stripe-webhook-secret>" \
     -e Stripe__PublishableKey="<stripe-publishable-key>" \
     -e CORS_ORIGINS="<comma-separated-origins>" \
     ecommerce-webapi
   ```
