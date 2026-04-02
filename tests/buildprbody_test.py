# http://www.apache.org/licenses/LICENSE-2.0
"""BuildPrBody tests."""
__author__ = "partheniou@google.com (Anthonios Partheniou)"
from buildprbody import BuildPrBody
CHANGE_SUMMARY_DIR = SCRIPTS_DIR / "test_resources" / "buildprbody_resources"
EXPECTED_PR_BODY_OUTPUT = """\
## Deleted keys were detected in the following stable discovery artifacts:
bigquery v2 https://github.com/googleapis/google-api-python-client/commit/123
cloudtasks v2 https://github.com/googleapis/google-api-python-client/commit/456
## Discovery Artifact Change Summary:
feat(bigquery): update the api https://github.com/googleapis/google-api-python-client/commit/123
feat(cloudtasks): update the api https://github.com/googleapis/google-api-python-client/commit/456
feat(drive): update the api https://github.com/googleapis/google-api-python-client/commit/789
class TestBuildPrBody(unittest.TestCase):
        self.buildprbody = BuildPrBody(change_summary_directory=CHANGE_SUMMARY_DIR)
    def test_get_commit_uri_returns_correct_string(self):
        base_uri = "https://github.com/googleapis/google-api-python-client/commit/"
        expected_uri = "".join([base_uri, "123"])
        result = self.buildprbody.get_commit_uri(name="bigquery")
        self.assertEqual(result, expected_uri)
        expected_uri = "".join([base_uri, "456"])
        result = self.buildprbody.get_commit_uri(name="cloudtasks")
        expected_uri = "".join([base_uri, "789"])
        result = self.buildprbody.get_commit_uri(name="drive")
    def test_generate_pr_body(self):
        self.buildprbody.generate_pr_body()
        with open(CHANGE_SUMMARY_DIR / "allapis.summary") as f:
            pr_body = "".join(f.readlines())
        self.assertEqual(pr_body, EXPECTED_PR_BODY_OUTPUT)
