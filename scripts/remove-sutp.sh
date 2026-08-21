#!/bin/bash

# Settings for SUTP service (TechnicalSupportService)
SERVICE_NAME="sutp-service"
APP_NAME="TechnicalSupportService.SUTP"
INSTALL_DIR="/opt/technical-support/sutp"
FILES_DIR="$INSTALL_DIR/files"
USER_NAME="technicalsupport"

# Root check
if [ "$EUID" -ne 0 ]; then
    echo "Please run as root: sudo $0"
    exit 1
fi

echo "=== Removing $SERVICE_NAME ==="
echo "  Install dir: $INSTALL_DIR"
echo "  Files dir:   $FILES_DIR"
echo "  User:        $USER_NAME"
echo ""

# ─── Confirm ──────────────────────────────────────────────────────────
read -p "Are you sure you want to remove the service and all files? (yes/no): " CONFIRM
if [ "$CONFIRM" != "yes" ]; then
    echo "Cancelled."
    exit 0
fi

# ─── Stop and disable service ────────────────────────────────────────
echo "Stopping and disabling service..."
if systemctl is-active --quiet $SERVICE_NAME; then
    systemctl stop $SERVICE_NAME
    echo "  Stopped $SERVICE_NAME"
fi

if systemctl is-enabled --quiet $SERVICE_NAME 2>/dev/null; then
    systemctl disable $SERVICE_NAME
    echo "  Disabled $SERVICE_NAME"
fi

# ─── Remove systemd service file ──────────────────────────────────────
if [ -f "/etc/systemd/system/$SERVICE_NAME.service" ]; then
    rm -f "/etc/systemd/system/$SERVICE_NAME.service"
    systemctl daemon-reload
    echo "  Removed service file"
fi

# ─── Remove application directory ────────────────────────────────────
if [ -d "$INSTALL_DIR" ]; then
    rm -rf "$INSTALL_DIR"
    echo "  Removed $INSTALL_DIR"
fi

# ─── Optionally remove user/group ────────────────────────────────────
# Check if user has other directories or services using it
echo ""
echo "Service user '$USER_NAME' information:"
if id "$USER_NAME" &>/dev/null; then
    # Check for owned directories under /opt or elsewhere
    OWNED=$(find /opt -user "$USER_NAME" 2>/dev/null | head -5)
    HOME_DIR=$(eval echo "~$USER_NAME" 2>/dev/null)
    IS_SYSTEM_USER=$(id -u "$USER_NAME" 2>/dev/null)
    
    if [ -n "$OWNED" ]; then
        echo "  ⚠ User '$USER_NAME' still owns other files:"
        echo "$OWNED"
        echo "  Skipping user removal."
    else
        read -p "  Remove system user '$USER_NAME'? (yes/no): " REMOVE_USER
        if [ "$REMOVE_USER" = "yes" ]; then
            userdel "$USER_NAME" 2>/dev/null
            echo "  Removed user '$USER_NAME'"
            
            # Remove group if it exists
            if getent group "$USER_NAME" &>/dev/null; then
                groupdel "$USER_NAME" 2>/dev/null
                echo "  Removed group '$USER_NAME'"
            fi
        fi
    fi
else
    echo "  User '$USER_NAME' not found (already removed or never created)."
fi

echo ""
echo "✅ Service '$SERVICE_NAME' has been removed."
echo "   The database was NOT affected — PostgreSQL data is preserved."
echo "   To drop the database: sudo -u postgres dropdb SUTP"