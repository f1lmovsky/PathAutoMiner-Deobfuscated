#!/usr/bin/env python3
"""
Build script for PAM (Path Auto Miner).

Concatenates numbered .cs files from src/ into a single PAM_Combined.cs,
then optionally runs minify_pam.py to produce PAM_Minified.cs.

Usage:
    python tools/build_pam.py                 # concatenate only
    python tools/build_pam.py --minify        # concatenate + minify
    python tools/build_pam.py --minify --stats # concatenate + minify with stats
"""

import argparse
import os
import sys
import subprocess
import glob

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
ROOT_DIR = os.path.dirname(SCRIPT_DIR)
SRC_DIR = os.path.join(ROOT_DIR, "src")
COMBINED_OUTPUT = os.path.join(ROOT_DIR, "PAM_Combined.cs")
MINIFIED_OUTPUT = os.path.join(ROOT_DIR, "PAM_Minified.cs")
MINIFY_SCRIPT = os.path.join(SCRIPT_DIR, "minify_pam.py")


def collect_source_files():
    """Collect all .cs files from src/ sorted by numeric prefix."""
    pattern = os.path.join(SRC_DIR, "*.cs")
    files = sorted(glob.glob(pattern))
    if not files:
        print(f"ERROR: No .cs files found in {SRC_DIR}")
        sys.exit(1)
    return files


def concatenate(files, output_path):
    """Concatenate source files with section markers."""
    parts = []
    for fpath in files:
        fname = os.path.basename(fpath)
        with open(fpath, "r", encoding="utf-8") as f:
            content = f.read()
        parts.append(f"// ========================== {fname} ==========================")
        parts.append(content.rstrip())
        parts.append("")

    combined = "\n".join(parts)

    with open(output_path, "w", encoding="utf-8") as f:
        f.write(combined)

    char_count = len(combined)
    print(f"Combined:  {len(files)} files -> {output_path}")
    print(f"           {char_count} chars ({char_count // 1024} KB)")
    return output_path


def minify(input_path, output_path, extra_args):
    """Run minify_pam.py on the combined file."""
    cmd = [sys.executable, MINIFY_SCRIPT, input_path, "--output", output_path] + extra_args
    print(f"\nMinifying: {os.path.basename(input_path)} -> {os.path.basename(output_path)}")
    result = subprocess.run(cmd, cwd=ROOT_DIR)
    return result.returncode


def main():
    parser = argparse.ArgumentParser(description="Build PAM script from src/ files")
    parser.add_argument("--minify", action="store_true", help="Also run the minifier")
    parser.add_argument("--stats", action="store_true", help="Show minifier stats")
    parser.add_argument("--output", default=COMBINED_OUTPUT, help="Combined output path")
    args = parser.parse_args()

    files = collect_source_files()
    print(f"Source:    {len(files)} files in {SRC_DIR}")
    for f in files:
        print(f"           - {os.path.basename(f)}")

    combined = concatenate(files, args.output)

    if args.minify:
        extra = []
        if args.stats:
            extra.append("--stats")
        rc = minify(combined, MINIFIED_OUTPUT, extra)
        if rc != 0:
            print("ERROR: Minification failed")
            sys.exit(rc)

    print("\nDone.")


if __name__ == "__main__":
    main()
