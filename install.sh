#!/usr/bin/env bash
set -euo pipefail

readonly REPOSITORY="alexis-jose-calderon-diaz/dbox"
readonly INSTALL_DIR="$HOME/.local/bin"

case "$(uname -s)" in
  Linux) platform="linux" ;;
  Darwin) platform="osx" ;;
  *)
    printf 'Unsupported operating system: %s.\n' "$(uname -s)" >&2
    exit 1
    ;;
esac

case "$(uname -m)" in
  x86_64 | amd64) architecture="x64" ;;
  aarch64 | arm64) architecture="arm64" ;;
  *)
    printf 'Unsupported architecture: %s.\n' "$(uname -m)" >&2
    exit 1
    ;;
esac

asset="dbox-${platform}-${architecture}"
url="https://github.com/${REPOSITORY}/releases/latest/download/${asset}"
temporary_file="$(mktemp)"
trap 'rm -f "$temporary_file"' EXIT

if ! curl --fail --location --silent --show-error --retry 3 --output "$temporary_file" "$url"; then
  printf 'Failed to download %s from the latest GitHub Release.\n' "$asset" >&2
  exit 1
fi

mkdir -p "$INSTALL_DIR"
path_was_set=false
case ":$PATH:" in
  *":$INSTALL_DIR:"*) path_was_set=true ;;
esac

mv "$temporary_file" "$INSTALL_DIR/dbox"
chmod +x "$INSTALL_DIR/dbox"
export PATH="$INSTALL_DIR:$PATH"

printf 'Installed dbox to %s\n' "$INSTALL_DIR/dbox"
dbox --version

if [ "$path_was_set" = false ]; then
  printf '\nAdd %s to your PATH to use dbox in new shells:\n' "$INSTALL_DIR"
  printf '  export PATH="%s:$PATH"\n' "$INSTALL_DIR"
fi
