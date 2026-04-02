"""Simple command-line sample for Audit API.
Command-line application that retrieves events through the Audit API.
This works only for Google Apps for Business, Education, and ISP accounts.
It can not be used for the basic Google Apps product.
  $ python audit.py
You can also get help on all the command-line flags the program understands
by running:
  $ python audit.py --help
To get detailed log output run:
  $ python audit.py --logging_level=DEBUG
__author__ = "rahulpaul@google.com (Rahul Paul)"
        "audit",
        "v1",
        scope="https://www.googleapis.com/auth/apps/reporting/audit.readonly",
        activities = service.activities()
        # Retrieve the first two activities
        print("Retrieving the first 2 activities...")
        activity_list = activities.list(
            applicationId="207535951991",
            customerId="C01rv1wm7",
            maxResults="2",
            actorEmail="admin@enterprise-audit-clientlib.com",
        ).execute()
        pprint.pprint(activity_list)
        # Now retrieve the next 2 events
        match = re.search("(?<=continuationToken=).+$", activity_list["next"])
        if match is not None:
            next_token = match.group(0)
            print("\nRetrieving the next 2 activities...")
                continuationToken=next_token,
