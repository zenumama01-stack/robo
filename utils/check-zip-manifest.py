def main(zip_path, manifest_path):
    Compare a zip file's contents against a manifest file.
    Returns 0 if they match, 1 if there are differences.
    if not os.path.exists(zip_path):
        print(f"ERROR: Zip file not found: {zip_path}", file=sys.stderr)
    if not os.path.exists(manifest_path):
        print(f"ERROR: Manifest file not found: {manifest_path}", file=sys.stderr)
        with zipfile.ZipFile(zip_path, 'r', allowZip64=True) as z:
            files_in_zip = set(z.namelist())
    except zipfile.BadZipFile:
        print(f"ERROR: Invalid zip file: {zip_path}", file=sys.stderr)
        print(f"ERROR: Failed to read zip file {zip_path}: {e}", file=sys.stderr)
        with open(manifest_path, 'r', encoding='utf-8') as manifest:
            files_in_manifest = {line.strip() for line in manifest.readlines() if line.strip()}
        print(f"ERROR: Failed to read manifest file {manifest_path}: {e}", file=sys.stderr)
    added_files = files_in_zip - files_in_manifest
    removed_files = files_in_manifest - files_in_zip
    if not added_files and not removed_files:
        print("OK: Zip contents match manifest - no differences found")
    print("ERROR: Zip contents do not match manifest!")
    print(f"Zip file: {zip_path}")
    print(f"Manifest: {manifest_path}")
    if added_files:
        print(f"Files in zip but NOT in manifest ({len(added_files)} files):")
        for f in sorted(added_files):
            print(f"  + {f}")
    if removed_files:
        print(f"Files in manifest but NOT in zip ({len(removed_files)} files):")
        for f in sorted(removed_files):
            print(f"  - {f}")
    print("ACTION REQUIRED:")
        print("- Add the new files to the manifest, or")
        print("- Remove them from the zip if they shouldn't be included")
        print("- Remove the missing files from the manifest, or")
        print("- Add them to the zip if they should be included")
    if len(sys.argv) != 3:
        print("Usage: check-zip-manifest.py <zip_file> <manifest_file>", file=sys.stderr)
        print("", file=sys.stderr)
        print("Compares the contents of a zip file against a manifest file.", file=sys.stderr)
        print("Returns 0 if they match, 1 if there are differences or errors.", file=sys.stderr)
