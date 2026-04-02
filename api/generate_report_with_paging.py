"""This example retrieves a report for the specified ad client.
Please only use pagination if your application requires it due to memory or
storage constraints.
If you need to retrieve more than 5000 rows, please check generate_report.py, as
due to current limitations you will not be able to use paging for large reports.
MAX_PAGE_SIZE = 50
# This is the maximum number of obtainable rows for paged reports.
ROW_LIMIT = 5000
    "ad_client_id", help="The ID of the ad client for which to generate a report"
        # Retrieve report in pages and display data as we receive it.
        rows_to_obtain = MAX_PAGE_SIZE
                    startIndex=start_index,
                    maxResults=rows_to_obtain,
            # If this is the first page, display the headers.
            if start_index == 0:
            # Display results for this page.
            start_index += len(result["rows"])
            # Check to see if we're going to go above the limit and get as many
            # results as we can.
            if start_index + MAX_PAGE_SIZE > ROW_LIMIT:
                rows_to_obtain = ROW_LIMIT - start_index
                if rows_to_obtain <= 0:
            if start_index >= int(result["totalMatchedRows"]):
