#     https://www.apache.org/licenses/LICENSE-2.0
from changesummary import ChangeType
import pandas as pd
SCRIPTS_DIR = pathlib.Path(__file__).parent.resolve()
CHANGE_SUMMARY_DIR = SCRIPTS_DIR / "temp"
class BuildPrBody:
    """Represents the PR body which contains the change summary between 2
    directories containing artifacts.
    def __init__(self, change_summary_directory):
        """Initializes an instance of a BuildPrBody.
            change_summary_directory (str): The relative path to the directory
            which contains the change summary output.
        self._change_summary_directory = change_summary_directory
    def get_commit_uri(self, name):
        """Return a uri to the last commit for the given API Name.
            name (str): The name of the api.
        url = "https://github.com/googleapis/google-api-python-client/commit/"
        sha = None
        api_link = ""
        file_path = pathlib.Path(self._change_summary_directory) / "{0}.sha".format(
        if file_path.is_file():
            with open(file_path, "r") as f:
                sha = f.readline().rstrip()
                if sha:
                    api_link = "{0}{1}".format(url, sha)
        return api_link
    def generate_pr_body(self):
        Generates a PR body given an input file `'allapis.dataframe'` and
        writes it to disk with file name `'allapis.summary'`.
        directory = pathlib.Path(self._change_summary_directory)
        dataframe = pd.read_csv(directory / "allapis.dataframe")
        dataframe["Version"] = dataframe["Version"].astype(str)
        dataframe["Commit"] = np.vectorize(self.get_commit_uri)(dataframe["Name"])
        stable_and_breaking = (
            dataframe[
                dataframe["IsStable"] & (dataframe["ChangeType"] == ChangeType.DELETED)
            ][["Name", "Version", "Commit"]]
            .drop_duplicates()
            .agg(" ".join, axis=1)
            .values
        prestable_and_breaking = (
                (dataframe["IsStable"] == False)
                & (dataframe["ChangeType"] == ChangeType.DELETED)
        all_apis = (
            dataframe[["Summary", "Commit"]]
        with open(directory / "allapis.summary", "w") as f:
            if len(stable_and_breaking) > 0:
                f.writelines(
                        "## Deleted keys were detected in the following stable discovery artifacts:\n",
                        "\n".join(stable_and_breaking),
                        "\n\n",
            if len(prestable_and_breaking) > 0:
                        "## Deleted keys were detected in the following pre-stable discovery artifacts:\n",
                        "\n".join(prestable_and_breaking),
            if len(all_apis) > 0:
                        "## Discovery Artifact Change Summary:\n",
                        "\n".join(all_apis),
    BuildPrBody(CHANGE_SUMMARY_DIR).generate_pr_body()
