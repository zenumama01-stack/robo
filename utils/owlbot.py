import synthtool as s
from synthtool import gcp
from synthtool.languages import python
common = gcp.CommonTemplates()
# ----------------------------------------------------------------------------
# Add templated files
templated_files = common.py_library(
    unit_test_python_versions=[
        "3.7",
        "3.8",
        "3.9",
        "3.10",
        "3.11",
        "3.12",
        "3.13",
        "3.14",
# Copy kokoro configs.
s.move(templated_files / ".kokoro")
s.move(templated_files / ".trampolinerc")  # config file for trampoline_v2
# Also move issue templates
s.move(
    templated_files / ".github",
    excludes=["CODEOWNERS", "workflows", "auto-approve.yml"],
# Move scripts folder needed for samples CI
s.move(templated_files / "scripts")
# Copy CONTRIBUTING.rst
s.move(templated_files / "CONTRIBUTING.rst")
# Copy configuration file for renovate
s.move(templated_files / "renovate.json")
# Samples templates
python.py_samples(skip_readmes=True)
for noxfile in Path(".").glob("**/noxfile.py"):
    s.shell.run(["nox", "-s", "format"], cwd=noxfile.parent, hide_output=False)
