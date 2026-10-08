#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
GAME_CATEGORY="${1:-deep-north-update}"
VERSION="$(tr -d '[:space:]' < "$ROOT/version.txt")"
OUT="$ROOT/dist/thunderstore.toml"

python_bin() {
  if command -v python3 >/dev/null 2>&1; then
    echo python3
  else
    echo python
  fi
}

if ! [[ "$GAME_CATEGORY" =~ ^[a-z0-9]+(-[a-z0-9]+)*$ ]]; then
  echo "thunderstore: category slug must look like deep-north-update, got: $GAME_CATEGORY" >&2
  exit 1
fi

mkdir -p "$ROOT/dist"
"$(python_bin)" - "$ROOT/manifest.json" "$OUT" "$VERSION" "$GAME_CATEGORY" <<'PY'
import json
import sys
from pathlib import Path

manifest_path, dest, version, game_category = sys.argv[1:]
manifest = json.loads(Path(manifest_path).read_text(encoding="utf-8"))
categories = ["mods", "ai-generated", "tweaks", "client-side", "server-side", game_category]
seen = []
for category in categories:
    if category not in seen:
        seen.append(category)


def toml_string(value):
    # A JSON string literal is a valid TOML basic string, so quotes and backslashes are escaped for free.
    return json.dumps(str(value), ensure_ascii=False)


quoted = ", ".join(toml_string(category) for category in seen)
text = f"""[config]
schemaVersion = "0.0.1"

[package]
namespace = "EvuMods"
name = {toml_string(manifest["name"])}
versionNumber = {toml_string(version)}
description = {toml_string(manifest["description"])}
websiteUrl = {toml_string(manifest["website_url"])}
containsNsfwContent = false

[publish]
repository = "https://thunderstore.io"
communities = ["valheim"]

[publish.categories]
valheim = [{quoted}]
"""
Path(dest).write_text(text, encoding="utf-8")
PY
echo "thunderstore: $OUT"
