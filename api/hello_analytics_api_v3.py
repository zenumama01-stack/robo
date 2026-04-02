"""Simple intro to using the Google Analytics API v3.
Google Analytics data. The sample traverses the Management API to obtain the
authorized user's first profile ID. Then the sample uses this ID to
contstruct a Core Reporting API query to return the top 25 organic search
terms.
Before you begin, you must sigup for a new project in the Google APIs console:
Then register the project to use OAuth2.0 for installed applications.
Finally you will need to add the client id, client secret, and redirect URL
into the client_secrets.json file that is in the same directory as this sample.
  $ python hello_analytics_api_v3.py
  $ python hello_analytics_api_v3.py --help
        first_profile_id = get_first_profile_id(service)
        if not first_profile_id:
            print("Could not find a valid profile for this user.")
            results = get_top_keywords(service, first_profile_id)
def get_first_profile_id(service):
    """Traverses Management API to return the first profile id.
    This first queries the Accounts collection to get the first account ID.
    This ID is used to query the Webproperties collection to retrieve the first
    webproperty ID. And both account and webproperty IDs are used to query the
    Profile collection to get the first profile id.
      A string with the first profile ID. None if a user does not have any
      accounts, webproperties, or profiles.
    accounts = service.management().accounts().list().execute()
    if accounts.get("items"):
        firstAccountId = accounts.get("items")[0].get("id")
        webproperties = (
            service.management()
            .webproperties()
            .list(accountId=firstAccountId)
        if webproperties.get("items"):
            firstWebpropertyId = webproperties.get("items")[0].get("id")
            profiles = (
                .profiles()
                .list(accountId=firstAccountId, webPropertyId=firstWebpropertyId)
            if profiles.get("items"):
                return profiles.get("items")[0].get("id")
def get_top_keywords(service, profile_id):
    """Executes and returns data from the Core Reporting API.
    This queries the API for the top 25 organic search terms by visits.
      profile_id: String The profile ID from which to retrieve analytics data.
      The response returned from the Core Reporting API.
            ids="ga:" + profile_id,
    """Prints out the results.
    This prints out the profile name, the column headers, and all the rows of
    data.
    print("Profile Name: %s" % results.get("profileInfo").get("profileName"))
    # Print header.
    output = []
    for header in results.get("columnHeaders"):
        output.append("%30s" % header.get("name"))
    print("".join(output))
    # Print data table.
            for cell in row:
                output.append("%30s" % cell)
