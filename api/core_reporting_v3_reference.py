"""Reference command-line example for Google Analytics Core Reporting API v3.
This application demonstrates how to use the python client library to access
all the pieces of data returned by the Google Analytics Core Reporting API v3.
The application manages autorization by saving an OAuth2.0 token in a local
file and reusing the token for subsequent requests.
Before You Begin:
Update the client_secrets.json file
  You must update the clients_secrets.json file with a client id, client
  secret, and the redirect uri. You get these values by creating a new project
  in the Google APIs console and registering for OAuth2.0 for installed
  applications: https://code.google.com/apis/console
  Learn more about registering your analytics application here:
  http://developers.google.com/analytics/devguides/reporting/core/v3/gdataAuthorization
Supply your TABLE_ID
  You will also need to identify from which profile to access data by
  specifying the TABLE_ID constant below. This value is of the form: ga:xxxx
  where xxxx is the profile ID. You can get the profile ID by either querying
  the Management API or by looking it up in the account settings of the
  Google Anlaytics web interface.
Sample Usage:
  $ python core_reporting_v3_reference.py ga:xxxx
Where the table ID is used to identify from which Google Anlaytics profile
to retrieve data. This ID is in the format ga:xxxx where xxxx is the
profile ID.
Also you can also get help on all the command-line flags the program
understands by running:
  $ python core_reporting_v3_reference.py --help
__author__ = "api.nickm@gmail.com (Nick Mihailovski)"
from oauth2client.client import AccessTokenRefreshError
    "table_id",
        "The table ID of the profile you wish to access. "
        "Format is ga:xxx where xxx is your profile ID."
        "analytics",
        "v3",
        scope="https://www.googleapis.com/auth/analytics.readonly",
    # Try to make a request to the API. Print the results or handle errors.
        results = get_api_query(service, flags.table_id).execute()
        print_results(results)
    except TypeError as error:
        # Handle errors in constructing a query.
        print(("There was an error in constructing your query : %s" % error))
    except HttpError as error:
        # Handle API errors.
                "Arg, there was an API error : %s : %s"
                % (error.resp.status, error._get_reason())
    except AccessTokenRefreshError:
        # Handle Auth errors.
            "The credentials have been revoked or expired, please re-run "
            "the application to re-authorize"
def get_api_query(service, table_id):
    """Returns a query object to retrieve data from the Core Reporting API.
      service: The service object built by the Google API Python client library.
      table_id: str The table ID form which to retrieve data.
        service.data()
        .ga()
        .get(
            ids=table_id,
            start_date="2012-01-01",
            end_date="2012-01-15",
            metrics="ga:visits",
            dimensions="ga:source,ga:keyword",
            sort="-ga:visits",
            filters="ga:medium==organic",
            start_index="1",
            max_results="25",
def print_results(results):
    """Prints all the results in the Core Reporting API Response.
      results: The response returned from the Core Reporting API.
    print_report_info(results)
    print_pagination_info(results)
    print_profile_info(results)
    print_query(results)
    print_column_headers(results)
    print_totals_for_all_results(results)
    print_rows(results)
def print_report_info(results):
    """Prints general information about this report.
    print("Report Infos:")
    print("Contains Sampled Data = %s" % results.get("containsSampledData"))
    print("Kind                  = %s" % results.get("kind"))
    print("ID                    = %s" % results.get("id"))
    print("Self Link             = %s" % results.get("selfLink"))
def print_pagination_info(results):
    """Prints common pagination details.
    print("Pagination Infos:")
    print("Items per page = %s" % results.get("itemsPerPage"))
    print("Total Results  = %s" % results.get("totalResults"))
    # These only have values if other result pages exist.
    if results.get("previousLink"):
        print("Previous Link  = %s" % results.get("previousLink"))
    if results.get("nextLink"):
        print("Next Link      = %s" % results.get("nextLink"))
def print_profile_info(results):
    """Prints information about the profile.
    print("Profile Infos:")
    info = results.get("profileInfo")
    print("Account Id      = %s" % info.get("accountId"))
    print("Web Property Id = %s" % info.get("webPropertyId"))
    print("Profile Id      = %s" % info.get("profileId"))
    print("Table Id        = %s" % info.get("tableId"))
    print("Profile Name    = %s" % info.get("profileName"))
def print_query(results):
    """The query returns the original report query as a dict.
    print("Query Parameters:")
    query = results.get("query")
    for key, value in query.iteritems():
        print("%s = %s" % (key, value))
def print_column_headers(results):
    """Prints the information for each column.
    The main data from the API is returned as rows of data. The column
    headers describe the names and types of each column in rows.
    print("Column Headers:")
    headers = results.get("columnHeaders")
        # Print Dimension or Metric name.
            "\t%s name:    = %s"
            % (header.get("columnType").title(), header.get("name"))
        print("\tColumn Type = %s" % header.get("columnType"))
        print("\tData Type   = %s" % header.get("dataType"))
def print_totals_for_all_results(results):
    """Prints the total metric value for all pages the query matched.
    print("Total Metrics For All Results:")
    print("This query returned %s rows." % len(results.get("rows")))
    print(("But the query matched %s total results." % results.get("totalResults")))
    print("Here are the metric totals for the matched total results.")
    totals = results.get("totalsForAllResults")
    for metric_name, metric_total in totals.iteritems():
        print("Metric Name  = %s" % metric_name)
        print("Metric Total = %s" % metric_total)
def print_rows(results):
    """Prints all the rows of data returned by the API.
    print("Rows:")
    if results.get("rows", []):
        for row in results.get("rows"):
            print("\t".join(row))
        print("No Rows Found")
