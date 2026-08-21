#!/bin/bash

# Settings for SUTP service (TechnicalSupportService)
SERVICE_NAME="sutp-service"
APP_NAME="TechnicalSupportService.SUTP"
ZIP_FILE="$APP_NAME.zip"
INSTALL_DIR="/opt/technical-support/sutp"
FILES_DIR="$INSTALL_DIR/files"
USER_NAME="technicalsupport"
DESCRIPTION="SUTP - Система управления технической поддержкой"
APP_PORT="5000"

# Parse command line arguments for port
while [[ $# -gt 0 ]]; do
    case $1 in
        --port|-p)
            APP_PORT="$2"
            shift 2
            ;;
        *)
            echo "Unknown option: $1"
            echo "Usage: $0 [--port <port_number>]"
            exit 1
            ;;
    esac
done

# Root check
if [ "$EUID" -ne 0 ]; then
    echo "Please run as root: sudo $0 [--port <port>]"
    exit 1
fi

# Validate port
if ! [[ "$APP_PORT" =~ ^[0-9]+$ ]] || [ "$APP_PORT" -lt 1 ] || [ "$APP_PORT" -gt 65535 ]; then
    echo "Error: Invalid port number: $APP_PORT"
    exit 1
fi

# Check if zip exists
if [ ! -f "$ZIP_FILE" ]; then
    echo "Error: $ZIP_FILE not found in current directory."
    echo "Expected file: $ZIP_FILE (built by publish-linux-x64.ps1)"
    exit 1
fi

# Путь к исполняемому файлу
APP_EXEC="$INSTALL_DIR/$APP_NAME"

echo "=== Installing $SERVICE_NAME on port $APP_PORT ==="

# ─── User and group ──────────────────────────────────────────────────
if ! getent group "$USER_NAME" &>/dev/null; then
    groupadd --system "$USER_NAME"
    echo "Created group: $USER_NAME"
fi

if ! id "$USER_NAME" &>/dev/null; then
    useradd --system --no-create-home --shell /usr/sbin/nologin \
            --gid "$USER_NAME" "$USER_NAME"
    echo "Created user: $USER_NAME"
fi

# ─── Unpack ───────────────────────────────────────────────────────────
echo "Unpacking $ZIP_FILE to $INSTALL_DIR..."

mkdir -p "$INSTALL_DIR"
rm -rf "$INSTALL_DIR"/*
unzip -o "$ZIP_FILE" -d "$INSTALL_DIR"

# ─── Files directory ──────────────────────────────────────────────────
# Create files directory for file uploads (matches appsettings.json FileStorage:LocalPath)
mkdir -p "$FILES_DIR"

# ─── Permissions
chmod +x "$APP_EXEC"
chown -R "$USER_NAME:$USER_NAME" "$INSTALL_DIR"
echo "Set permissions for $INSTALL_DIR to $USER_NAME"

# ─── Systemd service ─────────────────────────────────────────────────
cat > /etc/systemd/system/$SERVICE_NAME.service << EOF
[Unit]
Description=$DESCRIPTION
After=network.target postgresql.service
Wants=network.target

[Service]
Type=simple
User=$USER_NAME
Group=$USER_NAME
WorkingDirectory=$INSTALL_DIR
ExecStart=$APP_EXEC --urls http://*:$APP_PORT
Restart=always
RestartSec=10
TimeoutStartSec=60
TimeoutStopSec=30
KillSignal=SIGINT
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
SyslogIdentifier=$SERVICE_NAME
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
EOF

echo "Created service file: /etc/systemd/system/$SERVICE_NAME.service"

systemctl daemon-reload
systemctl enable $SERVICE_NAME

echo ""
echo "✅ Service '$SERVICE_NAME' configured to run on http://*:$APP_PORT"
echo "   Installation directory: $INSTALL_DIR"
echo "   Files directory:        $FILES_DIR"
echo ""

# ─── CKepler for PostgreSQL ───────────────────────────────────────────
echo "=== PREREQUISITES: Ensure PostgreSQL is running ==="
echo "  sudo systemctl status postgresql"
echo "  Database: SUTP (will be created automatically by the application)"
echo ""

# ─── Start service ────────────────────────────────────────────────────
echo "=== Starting service ==="
systemctl restart $SERVICE_NAME
sleep 5

echo "=== Service Status ==="
systemctl status $SERVICE_NAME --no-pager --lines=5

echo ""
echo "=== Management Commands ==="
echo "  Start service:    sudo systemctl start $SERVICE_NAME"
echo "  Stop service:     sudo systemctl stop $SERVICE_NAME"
echo "  Restart service:  sudo systemctl restart $SERVICE_NAME"
echo "  Check status:     sudo systemctl status $SERVICE_NAME"
echo "  View logs:        sudo journalctl -u $SERVICE_NAME -f"
echo ""
echo "Access the application at: http://$(hostname -I | awk '{print $1}'):$APP_PORT"