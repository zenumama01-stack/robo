#  Copyright 2014 Google Inc. All Rights Reserved.
"""Simple command-line sample for Google Coordinate.
Pulls a list of jobs, creates a job and marks a job complete for a given
Coordinate team. Client IDs for installed applications are created in the
Google API Console. See the documentation for more information:
   https://developers.google.com/console/help/#WhatIsKey
  $ python coordinate.py -t teamId
  $ python coordinate.py --help
  $ python coordinate.py -t teamId --logging_level=DEBUG
__author__ = "zachn@google.com (Zach Newell)"
from googleapiclient.discovery import build
from googleapiclient.discovery import http
argparser.add_argument("teamId", help="Coordinate Team ID")
        "coordinate",
        scope="https://www.googleapis.com/auth/coordinate",
    service = build("coordinate", "v1", http=http)
        # List all the jobs for a team
        jobs_result = service.jobs().list(teamId=flags.teamId).execute(http=http)
        print("List of Jobs:")
        pprint.pprint(jobs_result)
        # Multiline note
        note = """
    These are notes...
    on different lines
        # Insert a job and store the results
        insert_result = (
            service.jobs()
            .insert(
                body="",
                title="Google Campus",
                teamId=flags.teamId,
                address="1600 Amphitheatre Parkway Mountain View, CA 94043",
                lat="37.422120",
                lng="122.084429",
                assignee=None,
                note=note,
        pprint.pprint(insert_result)
        # Close the job
        update_result = (
            .update(
                jobId=insert_result["id"],
                progress="COMPLETED",
        pprint.pprint(update_result)
    except client.AccessTokenRefreshError as e:
