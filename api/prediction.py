"""Simple command-line sample for the Google Prediction API
Command-line application that trains on your input data. This sample does
the same thing as the Hello Prediction! example. You might want to run
the setup.sh script to load the sample data to Google Storage.
  $ python prediction.py "bucket/object" "model_id" "project_id"
  $ python prediction.py --help
  $ python prediction.py --logging_level=DEBUG
__author__ = (
    "jcgregorio@google.com (Joe Gregorio), " "marccohen@google.com (Marc Cohen)"
from apiclient import sample_tools
# Time to wait (in seconds) between successive checks of training status.
SLEEP_TIME = 10
    "object_name", help="Full Google Storage path of csv data (ex bucket/object)"
    "model_id", help="Model Id of your choosing to name trained model"
argparser.add_argument("project_id", help="Project Id of your Google Cloud Project")
def print_header(line):
    """Format and print header block sized to length of line"""
    header_str = "="
    header_line = header_str * len(line)
    print("\n" + header_line)
    print(line)
    print(header_line)
    # If you previously ran this app with an earlier version of the API
    # or if you change the list of scopes below, revoke your app's permission
    # here: https://accounts.google.com/IssuedAuthSubTokens
    # Then re-run the app to re-authorize it.
        "prediction",
        "v1.6",
        scope=(
            "https://www.googleapis.com/auth/prediction",
            "https://www.googleapis.com/auth/devstorage.read_only",
        # Get access to the Prediction API.
        papi = service.trainedmodels()
        # List models.
        print_header("Fetching list of first ten models")
        result = papi.list(maxResults=10, project=flags.project_id).execute()
        print("List results:")
        pprint.pprint(result)
        # Start training request on a data set.
        print_header("Submitting model training request")
        body = {"id": flags.model_id, "storageDataLocation": flags.object_name}
        start = papi.insert(body=body, project=flags.project_id).execute()
        print("Training results:")
        pprint.pprint(start)
        # Wait for the training to complete.
        print_header("Waiting for training to complete")
            status = papi.get(id=flags.model_id, project=flags.project_id).execute()
            state = status["trainingStatus"]
            print("Training state: " + state)
            if state == "DONE":
            elif state == "RUNNING":
                time.sleep(SLEEP_TIME)
                raise Exception("Training Error: " + state)
            # Job has completed.
            print("Training completed:")
            pprint.pprint(status)
        # Describe model.
        print_header("Fetching model description")
        result = papi.analyze(id=flags.model_id, project=flags.project_id).execute()
        print("Analyze results:")
        # Make some predictions using the newly trained model.
        print_header("Making some predictions")
        for sample_text in ["mucho bueno", "bonjour, mon cher ami"]:
            body = {"input": {"csvInstance": [sample_text]}}
            result = papi.predict(
                body=body, id=flags.model_id, project=flags.project_id
            print('Prediction results for "%s"...' % sample_text)
        # Delete model.
        print_header("Deleting model")
        result = papi.delete(id=flags.model_id, project=flags.project_id).execute()
        print("Model deleted.")
