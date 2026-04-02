import changesummary
import describe
    SCRIPTS_DIR / ".." / "googleapiclient" / "discovery_cache" / "documents"
REFERENCE_DOC_DIR = SCRIPTS_DIR / ".." / "docs" / "dyn"
TEMP_DIR = SCRIPTS_DIR / "temp"
# Clear discovery documents and reference documents directory
shutil.rmtree(DISCOVERY_DOC_DIR, ignore_errors=True)
shutil.rmtree(REFERENCE_DOC_DIR, ignore_errors=True)
# Check out a fresh copy
subprocess.call(["git", "checkout", DISCOVERY_DOC_DIR])
subprocess.call(["git", "checkout", REFERENCE_DOC_DIR])
# Snapshot current discovery artifacts to a temporary directory
with tempfile.TemporaryDirectory() as current_discovery_doc_dir:
    shutil.copytree(DISCOVERY_DOC_DIR, current_discovery_doc_dir, dirs_exist_ok=True)
    # Download discovery artifacts and generate documentation
    describe.generate_all_api_documents(
        doc_destination_dir=REFERENCE_DOC_DIR,
    # Get a list of files changed using `git diff`
    git_diff_output = subprocess.check_output(
            "git",
            "diff",
            "origin/main",
            "--name-only",
            "--",
            DISCOVERY_DOC_DIR / "*.json",
            REFERENCE_DOC_DIR / "*.html",
            REFERENCE_DOC_DIR / "*.md",
    # Create lists of the changed files
    all_changed_files = [
        pathlib.Path(file_name).name for file_name in git_diff_output.split("\n")
    json_changed_files = [file for file in all_changed_files if file.endswith(".json")]
    # Analyze the changes in discovery artifacts using the changesummary module
    changesummary.ChangeSummary(
        DISCOVERY_DOC_DIR, current_discovery_doc_dir, TEMP_DIR, json_changed_files
    # Write a list of the files changed to a file called `changed files` which will be used in the `createcommits.sh` script.
    with open(TEMP_DIR / "changed_files", "w") as f:
        f.writelines("\n".join(all_changed_files))
