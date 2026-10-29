#!/bin/bash
set -euo pipefail
VR_ROOT="$(cd "$(dirname "$0")" && pwd)"
VR_ADB='/Applications/Meta Quest Developer Hub.app/Contents/Resources/bin/adb'
if [ ! -x "$VR_ADB" ]; then VR_ADB="$(command -v adb)"; fi
VR_APK="$VR_ROOT/Builds/TabernacleVR.apk"
test -f "$VR_APK" || { echo 'The APK has not been built yet.'; exit 1; }
VR_COUNT=$("$VR_ADB" devices | awk 'NR>1 && $2=="device" {n++} END {print n+0}')
test "$VR_COUNT" -eq 1 || { echo 'Connect exactly one authorized Quest with a data-capable USB cable. Approve USB debugging inside the headset.'; "$VR_ADB" devices -l; exit 1; }
VR_MODEL=$("$VR_ADB" shell getprop ro.product.model | tr -d '\r')
case "$VR_MODEL" in *Quest*) ;; *) echo "Expected a Quest; found $VR_MODEL"; exit 1;; esac
"$VR_ADB" install -r "$VR_APK"
VR_ACTIVITY=$("$VR_ADB" shell cmd package resolve-activity --brief org.tabernacle.walkthrough | tr -d '\r' | tail -1)
case "$VR_ACTIVITY" in org.tabernacle.walkthrough/*) ;; *) echo 'Installed, but the launch activity could not be resolved. Open Tabernacle Walkthrough from Unknown Sources.'; exit 1;; esac
"$VR_ADB" shell am start -n "$VR_ACTIVITY"
echo 'Tabernacle Walkthrough installed and launched. Right trigger: teleport. Right stick: turn. A: next stop. B: performance.'
