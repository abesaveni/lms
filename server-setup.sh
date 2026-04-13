#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# LiveExpert.AI — One-time Hetzner server setup script
# Run as root (or with sudo) on a fresh Ubuntu 22.04/24.04 Hetzner server.
# Usage: bash server-setup.sh <your-domain.com>
# ─────────────────────────────────────────────────────────────────────────────
set -euo pipefail

DOMAIN="${1:-liveexpert.ai}"
APP_DIR="/opt/liveexpert"
REPO_URL="https://github.com/Amar434/Lms.git"

echo "═══════════════════════════════════════════════════════"
echo "  LiveExpert.AI — Server Setup  (domain: $DOMAIN)"
echo "═══════════════════════════════════════════════════════"

# ── 1. System updates ────────────────────────────────────────────────────────
apt-get update -y && apt-get upgrade -y
apt-get install -y curl git unzip ufw

# ── 2. Install Docker ────────────────────────────────────────────────────────
if ! command -v docker &>/dev/null; then
  curl -fsSL https://get.docker.com | sh
  systemctl enable --now docker
fi

# ── 3. Install Docker Compose v2 (plugin) ────────────────────────────────────
if ! docker compose version &>/dev/null; then
  apt-get install -y docker-compose-plugin
fi

# ── 4. Firewall ──────────────────────────────────────────────────────────────
ufw allow OpenSSH
ufw allow 80/tcp
ufw allow 443/tcp
ufw --force enable

# ── 5. Clone / update repo ───────────────────────────────────────────────────
if [ -d "$APP_DIR/.git" ]; then
  echo "Repo already cloned — pulling latest..."
  git -C "$APP_DIR" pull origin main
else
  echo "Cloning repository..."
  git clone "$REPO_URL" "$APP_DIR"
fi

# ── 6. Create .env from template ────────────────────────────────────────────
ENV_FILE="$APP_DIR/LMS-Backend/.env"
if [ ! -f "$ENV_FILE" ]; then
  cp "$APP_DIR/LMS-Backend/.env.example" "$ENV_FILE"
  echo ""
  echo "⚠️  IMPORTANT: Edit $ENV_FILE and fill in your real credentials before continuing!"
  echo "   nano $ENV_FILE"
  echo ""
  exit 0
fi

# ── 7. Patch nginx domain placeholder ───────────────────────────────────────
sed -i "s/DOMAIN_PLACEHOLDER/$DOMAIN/g" "$APP_DIR/nginx/nginx.conf"

# ── 8. Issue SSL certificate ─────────────────────────────────────────────────
echo ""
echo "Starting nginx for ACME challenge..."
cd "$APP_DIR"
docker compose up -d nginx

echo "Requesting SSL certificate for $DOMAIN..."
docker compose run --rm certbot certonly \
  --webroot -w /var/www/certbot \
  --email "admin@$DOMAIN" \
  --agree-tos --no-eff-email \
  -d "$DOMAIN" -d "www.$DOMAIN" || true

# ── 9. Start all services ────────────────────────────────────────────────────
docker compose up -d --build

echo ""
echo "═══════════════════════════════════════════════════════"
echo "  ✅ Deployment complete!"
echo "  🌐 https://$DOMAIN"
echo "═══════════════════════════════════════════════════════"
docker compose ps
