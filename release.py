#!/usr/bin/env python3
"""Build & zip release with correct folder structure.

Usage:
  python3 release.py              # build & zip DifficultyScaling
  python3 release.py 1.2.7        # build & zip with explicit version

Zip structure:
  DifficultyScaling.dll
  DifficultyScaling_cfg/config.toml
  DifficultyScaling_cfg/Difficulty/01_story/scaling.toml
  ...
Output: Proj/Release/DifficultyScaling_v<version>.zip
"""

import os
import re
import shutil
import subprocess
import sys
import tempfile
import zipfile

HERE = os.path.dirname(os.path.abspath(__file__))
PROJ = os.path.normpath(os.path.join(HERE, ".."))
RELEASE_DIR = os.path.join(PROJ, "Release")


def die(msg: str):
    print(f"ERROR: {msg}", file=sys.stderr)
    sys.exit(1)


def get_version() -> str:
    src_dir = os.path.join(HERE, "DifficultyScaling", "src")
    for fname in os.listdir(src_dir):
        if not fname.endswith(".cs"):
            continue
        path = os.path.join(src_dir, fname)
        with open(path, encoding="utf-8") as f:
            for line in f:
                m = re.search(r'MelonInfo\([^,]+,\s*"[^"]+",\s*"([^"]+)"', line)
                if m:
                    return m.group(1)
    raise SystemExit("ERROR: Could not extract version from src/*.cs")


def build():
    proj = os.path.join(HERE, "DifficultyScaling")
    print(f">>> Building {proj} ...")
    res = subprocess.run(
        ["dotnet", "build", "-c", "Release", "-nologo", "-v", "q"],
        cwd=proj,
        capture_output=True,
        text=True,
    )
    if res.returncode != 0:
        print(res.stderr, file=sys.stderr)
        die("Build failed")


def package(version: str):
    dll_path = os.path.join(HERE, "DifficultyScaling", "bin", "Release", "net6.0", "DifficultyScaling.dll")
    cfg_dir = os.path.join(HERE, "DifficultyScaling_cfg")

    if not os.path.isfile(dll_path):
        die(f"Build artifact not found: {dll_path}")
    if not os.path.isdir(cfg_dir):
        die(f"Config directory not found: {cfg_dir}")

    os.makedirs(RELEASE_DIR, exist_ok=True)
    zip_name = f"DifficultyScaling_v{version}.zip"
    zip_path = os.path.join(RELEASE_DIR, zip_name)

    tmp = tempfile.mkdtemp()
    try:
        shutil.copy2(dll_path, os.path.join(tmp, "DifficultyScaling.dll"))
        shutil.copytree(cfg_dir, os.path.join(tmp, "DifficultyScaling_cfg"))

        with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zf:
            for root, _dirs, files in os.walk(tmp):
                for fn in files:
                    full = os.path.join(root, fn)
                    zf.write(full, os.path.relpath(full, tmp))
    finally:
        shutil.rmtree(tmp, ignore_errors=True)

    print(f">>> Created: {zip_path}")


def main():
    version = sys.argv[1] if len(sys.argv) > 1 else get_version()
    build()
    package(version)


if __name__ == "__main__":
    main()
