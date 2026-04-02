"""Reference command-line example for Google Analytics Management API v3.
all the pieces of data returned by the Google Analytics Management API v3.
file and reusing the token for subsequent requests. It then traverses the
Google Analytics Management hierarchy. It first retrieves and prints all the
authorized user's accounts, next it prints all the web properties for the
first account, then all the profiles for the first web property and finally
all the goals for the first profile. The sample then prints all the
user's advanced segments.
  https://developers.google.com/analytics/devguides/config/mgmt/v3/mgmtAuthorization
  $ python management_v3_reference.py
  $ python management_v3_reference.py --help
    # Traverse the Management hierarchy and print results or handle errors.
        traverse_hierarchy(service)
            "The credentials have been revoked or expired, please re-run"
def traverse_hierarchy(service):
    """Traverses the management API hierarchy and prints results.
    This retrieves and prints the authorized user's accounts. It then
    retrieves and prints all the web properties for the first account,
    retrieves and prints all the profiles for the first web property,
    and retrieves and prints all the goals for the first profile.
      HttpError: If an error occurred when accessing the API.
      AccessTokenRefreshError: If the current token was invalid.
    print_accounts(accounts)
        print_webproperties(webproperties)
            print_profiles(profiles)
                firstProfileId = profiles.get("items")[0].get("id")
                goals = (
                    .goals()
                        accountId=firstAccountId,
                        webPropertyId=firstWebpropertyId,
                        profileId=firstProfileId,
                print_goals(goals)
    print_segments(service.management().segments().list().execute())
def print_accounts(accounts_response):
    """Prints all the account info in the Accounts Collection.
      accounts_response: The response object returned from querying the Accounts
          collection.
    print("------ Account Collection -------")
    print_pagination_info(accounts_response)
    for account in accounts_response.get("items", []):
        print("Account ID      = %s" % account.get("id"))
        print("Kind            = %s" % account.get("kind"))
        print("Self Link       = %s" % account.get("selfLink"))
        print("Account Name    = %s" % account.get("name"))
        print("Created         = %s" % account.get("created"))
        print("Updated         = %s" % account.get("updated"))
        child_link = account.get("childLink")
        print("Child link href = %s" % child_link.get("href"))
        print("Child link type = %s" % child_link.get("type"))
    if not accounts_response.get("items"):
        print("No accounts found.\n")
def print_webproperties(webproperties_response):
    """Prints all the web property info in the WebProperties collection.
      webproperties_response: The response object returned from querying the
          Webproperties collection.
    print("------ Web Properties Collection -------")
    print_pagination_info(webproperties_response)
    for webproperty in webproperties_response.get("items", []):
        print("Kind               = %s" % webproperty.get("kind"))
        print("Account ID         = %s" % webproperty.get("accountId"))
        print("Web Property ID    = %s" % webproperty.get("id"))
            ("Internal Web Property ID = %s" % webproperty.get("internalWebPropertyId"))
        print("Website URL        = %s" % webproperty.get("websiteUrl"))
        print("Created            = %s" % webproperty.get("created"))
        print("Updated            = %s" % webproperty.get("updated"))
        print("Self Link          = %s" % webproperty.get("selfLink"))
        parent_link = webproperty.get("parentLink")
        print("Parent link href   = %s" % parent_link.get("href"))
        print("Parent link type   = %s" % parent_link.get("type"))
        child_link = webproperty.get("childLink")
        print("Child link href    = %s" % child_link.get("href"))
        print("Child link type    = %s" % child_link.get("type"))
    if not webproperties_response.get("items"):
        print("No webproperties found.\n")
def print_profiles(profiles_response):
    """Prints all the profile info in the Profiles Collection.
      profiles_response: The response object returned from querying the
          Profiles collection.
    print("------ Profiles Collection -------")
    print_pagination_info(profiles_response)
    for profile in profiles_response.get("items", []):
        print("Kind                      = %s" % profile.get("kind"))
        print("Account ID                = %s" % profile.get("accountId"))
        print("Web Property ID           = %s" % profile.get("webPropertyId"))
        print(("Internal Web Property ID = %s" % profile.get("internalWebPropertyId")))
        print("Profile ID                = %s" % profile.get("id"))
        print("Profile Name              = %s" % profile.get("name"))
        print("Currency         = %s" % profile.get("currency"))
        print("Timezone         = %s" % profile.get("timezone"))
        print("Default Page     = %s" % profile.get("defaultPage"))
                "Exclude Query Parameters        = %s"
                % profile.get("excludeQueryParameters")
                "Site Search Category Parameters = %s"
                % profile.get("siteSearchCategoryParameters")
                "Site Search Query Parameters    = %s"
                % profile.get("siteSearchQueryParameters")
        print("Created          = %s" % profile.get("created"))
        print("Updated          = %s" % profile.get("updated"))
        print("Self Link        = %s" % profile.get("selfLink"))
        parent_link = profile.get("parentLink")
        print("Parent link href = %s" % parent_link.get("href"))
        print("Parent link type = %s" % parent_link.get("type"))
        child_link = profile.get("childLink")
        print("Child link href  = %s" % child_link.get("href"))
        print("Child link type  = %s" % child_link.get("type"))
    if not profiles_response.get("items"):
        print("No profiles found.\n")
def print_goals(goals_response):
    """Prints all the goal info in the Goals collection.
      goals_response: The response object returned from querying the Goals
          collection
    print("------ Goals Collection -------")
    print_pagination_info(goals_response)
    for goal in goals_response.get("items", []):
        print("Goal ID     = %s" % goal.get("id"))
        print("Kind        = %s" % goal.get("kind"))
        print("Self Link        = %s" % goal.get("selfLink"))
        print("Account ID               = %s" % goal.get("accountId"))
        print("Web Property ID          = %s" % goal.get("webPropertyId"))
        print(("Internal Web Property ID = %s" % goal.get("internalWebPropertyId")))
        print("Profile ID               = %s" % goal.get("profileId"))
        print("Goal Name   = %s" % goal.get("name"))
        print("Goal Value  = %s" % goal.get("value"))
        print("Goal Active = %s" % goal.get("active"))
        print("Goal Type   = %s" % goal.get("type"))
        print("Created     = %s" % goal.get("created"))
        print("Updated     = %s" % goal.get("updated"))
        parent_link = goal.get("parentLink")
        # Print the goal details depending on the type of goal.
        if goal.get("urlDestinationDetails"):
            print_url_destination_goal_details(goal.get("urlDestinationDetails"))
        elif goal.get("visitTimeOnSiteDetails"):
            print_visit_time_on_site_goal_details(goal.get("visitTimeOnSiteDetails"))
        elif goal.get("visitNumPagesDetails"):
            print_visit_num_pages_goal_details(goal.get("visitNumPagesDetails"))
        elif goal.get("eventDetails"):
            print_event_goal_details(goal.get("eventDetails"))
    if not goals_response.get("items"):
        print("No goals found.\n")
def print_url_destination_goal_details(goal_details):
    """Prints all the URL Destination goal type info.
      goal_details: The details portion of the goal response.
    print("------ Url Destination Goal -------")
    print("Goal URL            = %s" % goal_details.get("url"))
    print("Case Sensitive      = %s" % goal_details.get("caseSensitive"))
    print("Match Type          = %s" % goal_details.get("matchType"))
    print("First Step Required = %s" % goal_details.get("firstStepRequired"))
    print("------ Url Destination Goal Steps -------")
    for goal_step in goal_details.get("steps", []):
        print("Step Number  = %s" % goal_step.get("number"))
        print("Step Name    = %s" % goal_step.get("name"))
        print("Step URL     = %s" % goal_step.get("url"))
    if not goal_details.get("steps"):
        print("No Steps Configured")
def print_visit_time_on_site_goal_details(goal_details):
    """Prints all the Visit Time On Site goal type info.
    print("------ Visit Time On Site Goal -------")
    print("Comparison Type  = %s" % goal_details.get("comparisonType"))
    print("comparison Value = %s" % goal_details.get("comparisonValue"))
def print_visit_num_pages_goal_details(goal_details):
    """Prints all the Visit Num Pages goal type info.
    print("------ Visit Num Pages Goal -------")
def print_event_goal_details(goal_details):
    """Prints all the Event goal type info.
    print("------ Event Goal -------")
    print("Use Event Value  = %s" % goal_details.get("useEventValue"))
    for event_condition in goal_details.get("eventConditions", []):
        event_type = event_condition.get("type")
        print("Type             = %s" % event_type)
        if event_type in ("CATEGORY", "ACTION", "LABEL"):
            print("Match Type       = %s" % event_condition.get("matchType"))
            print("Expression       = %s" % event_condition.get("expression"))
        else:  # VALUE type.
            print("Comparison Type  = %s" % event_condition.get("comparisonType"))
            print("Comparison Value = %s" % event_condition.get("comparisonValue"))
def print_segments(segments_response):
    """Prints all the segment info in the Segments collection.
      segments_response: The response object returned from querying the
          Segments collection.
    print("------ Segments Collection -------")
    print_pagination_info(segments_response)
    for segment in segments_response.get("items", []):
        print("Segment ID = %s" % segment.get("id"))
        print("Kind       = %s" % segment.get("kind"))
        print("Self Link  = %s" % segment.get("selfLink"))
        print("Name       = %s" % segment.get("name"))
        print("Definition = %s" % segment.get("definition"))
        print("Created    = %s" % segment.get("created"))
        print("Updated    = %s" % segment.get("updated"))
def print_pagination_info(management_response):
      management_response: The common reponse object for each collection in the
          Management API.
    print("Items per page = %s" % management_response.get("itemsPerPage"))
    print("Total Results  = %s" % management_response.get("totalResults"))
    print("Start Index    = %s" % management_response.get("startIndex"))
    if management_response.get("previousLink"):
        print("Previous Link  = %s" % management_response.get("previousLink"))
    if management_response.get("nextLink"):
        print("Next Link      = %s" % management_response.get("nextLink"))
