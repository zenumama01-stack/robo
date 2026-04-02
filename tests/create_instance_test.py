# Copyright 2015, Google, Inc.
from create_instance import main
PROJECT = os.environ["GOOGLE_CLOUD_PROJECT"]
BUCKET = os.environ["CLOUD_STORAGE_BUCKET"]
@pytest.mark.flaky(max_runs=3, min_passes=1)
def test_main(capsys):
    instance_name = "test-instance-{}".format(uuid.uuid4())
    main(PROJECT, BUCKET, "us-central1-f", instance_name, wait=False)
    out, _ = capsys.readouterr()
    assert "Instances in project" in out
    assert "zone us-central1-f" in out
    assert instance_name in out
    assert "Deleting instance" in out
