#!/bin/bash

# Settings for SUTP service (TechnicalSupportService)
SERVICE_NAME="sutp-service"
APP_NAME="TechnicalSupportService.SUTP"
ZIP_FILE="$APP_NAME.zip"
INSTALL_DIR="/opt/technical-support/sutp"
FILES_DIR="$INSTALL_DIR/files"
USER_NAME="technicalsupport"

# Root check
if [ "$EUID" -ne 0 ]; then
    echo "Please run as root: sudo $0"
    exit 1
fi

# Check if zip exists
if [ ! -f "$ZIP_FILE" ]; then
    echo "Error: $ZIP_FILE not found in current directory."
    exit 1
fi

echo "=== Updating $APP_NAME (preserving config and files) ==="

# ─── Stop service ─────────────────────────────────────────────────────
if systemctl is-active --quiet $SERVICE_NAME; then
    echo "Stopping $SERVICE_NAME service..."
    systemctl stop $SERVICE_NAME
fi

# ─── Backup current config ────────────────────────────────────────────
echo "Backing up current appsettings.json..."
if [ -f "$INSTALL_DIR/appsettings.json" ]; then
    cp "$INSTALL_DIR/appsettings.json" /tmp/appsettings.json.bak
fi

# ─── Extract to temp ─────────────────────────────────────────────────
TMP_EXTRACT="/tmp/sutp_update"
rm -rf "$TMP_EXTRACT"
mkdir -p "$TMP_EXTRACT"

echo "Extracting $ZIP_FILE to temporary folder..."
unzip -o "$ZIP_FILE" -d "$TMP_EXTRACT"

# ─── Update files (preserve config and uploads) ───────────────────────
echo "Updating files (preserving appsettings.json and files/)..."
# --delete: remove old files not in the new version
# --exclude: keep config and uploaded files
rsync -av --delete \
    --exclude='appsettings.json' \
    --exclude='appsettings.*.json' \
    --exclude='files/' \
    --exclude='logs/' \
    "$TMP_EXTRACT/" "$INSTALL_DIR/"

# ─── Restore config if it was lost ────────────────────────────────────
if [ ! -f "$INSTALL_DIR/appsettings.json" ] && [ -f /tmp/appsettings.json.bak ]; then
    echo "Restoring appsettings.json from backup..."
    cp /tmp/appsettings.json.bak "$INSTALL_DIR/appsettings.json"
fi
rm -f /tmp/appsettings.json.bak

# ─── Permissions ──────────────────────────────────────────────────────
echo "Setting permissions..."
chmod +x "$INSTALL_DIR/$APP_NAME"
chown -R "$USER_NAME:$USER_NAME" "$INSTALL_DIR"

# Cleanup temp
rm -rf "$TMP_EXTRACT"

# ─── Restart service ─────────────────────────────────────────────────
echo ""
echo "=== Restarting service ==="
systemctl restart $SERVICE_NAME
sleep 5

echo "=== Service Status ==="
systemctl status $SERVICE_NAME --no-pager --lines=5

echo ""
echo "✅ $APP_NAME updated successfully."
echo "   Installation directory: $INSTALL_DIR"
echo "   Files directory:        $FILES_DIR"