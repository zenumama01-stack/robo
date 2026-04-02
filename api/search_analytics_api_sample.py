"""Example for using the Google Search Analytics API (part of Search Console API).
A basic python command-line example that uses the searchAnalytics.query method
of the Google Search Console API. This example demonstrates how to query Google
search results data for your property. Learn more at
https://developers.google.com/webmaster-tools/
To use:
1) Install the Google Python client library, as shown at https://developers.google.com/webmaster-tools/v3/libraries.
2) Sign up for a new project in the Google APIs console at https://code.google.com/apis/console.
3) Register the project to use OAuth2.0 for installed applications.
4) Copy your client ID, client secret, and redirect URL into the client_secrets.json file included in this package.
5) Run the app in the command-line as shown below.
Sample usage:
  $ python search_analytics_api_sample.py 'https://www.example.com/' '2015-05-01' '2015-05-30'
    "property_uri",
    help=("Site or app URI to query data for (including " "trailing slash)."),
    "start_date",
    help=("Start date of the requested date range in " "YYYY-MM-DD format."),
    "end_date",
    help=("End date of the requested date range in " "YYYY-MM-DD format."),
        "searchconsole",
        scope="https://www.googleapis.com/auth/webmasters.readonly",
    # First run a query to learn which dates we have data for. You should always
    # check which days in a date range have data before running your main query.
    # This query shows data for the entire range, grouped and sorted by day,
    # descending; any days without data will be missing from the results.
    request = {
        "startDate": flags.start_date,
        "endDate": flags.end_date,
        "dimensions": ["date"],
    response = execute_request(service, flags.property_uri, request)
    print_table(response, "Available dates")
    # Get totals for the date range.
    request = {"startDate": flags.start_date, "endDate": flags.end_date}
    print_table(response, "Totals")
    # Get top 10 queries for the date range, sorted by click count, descending.
        "dimensions": ["query"],
        "rowLimit": 10,
    print_table(response, "Top Queries")
    # Get top 11-20 mobile queries for the date range, sorted by click count, descending.
        "dimensionFilterGroups": [
            {"filters": [{"dimension": "device", "expression": "mobile"}]}
        "startRow": 10,
    print_table(response, "Top 11-20 Mobile Queries")
    # Get top 10 pages for the date range, sorted by click count, descending.
        "dimensions": ["page"],
    print_table(response, "Top Pages")
    # Get the top 10 queries in India, sorted by click count, descending.
            {"filters": [{"dimension": "country", "expression": "ind"}]}
    print_table(response, "Top queries in India")
    # Group by both country and device.
        "dimensions": ["country", "device"],
    print_table(response, "Group by country and device")
    # Group by total number of Search Appearance count.
    # Note: It is not possible to use searchAppearance with other
    # dimensions.
        "dimensions": ["searchAppearance"],
    print_table(response, "Search Appearance Features")
def execute_request(service, property_uri, request):
    """Executes a searchAnalytics.query request.
      service: The searchconsole service to use when executing the query.
      property_uri: The site or app URI to request data for.
      request: The request to be executed.
      An array of response rows.
    return service.searchanalytics().query(siteUrl=property_uri, body=request).execute()
def print_table(response, title):
    """Prints out a response table.
    Each row contains key(s), clicks, impressions, CTR, and average position.
      response: The server response to be printed as a table.
      title: The title of the table.
    print("\n --" + title + ":")
    if "rows" not in response:
        print("Empty response")
    rows = response["rows"]
    row_format = "{:<20}" + "{:>20}" * 4
    print(row_format.format("Keys", "Clicks", "Impressions", "CTR", "Position"))
    for row in rows:
        keys = ""
        # Keys are returned only if one or more dimensions are requested.
        if "keys" in row:
            keys = ",".join(row["keys"]).encode("utf-8").decode()
            row_format.format(
                keys, row["clicks"], row["impressions"], row["ctr"], row["position"]
