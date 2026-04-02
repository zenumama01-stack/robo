"""This example gets all the saved reports for the logged
in user's default account.
Tags: savedreports.list
        request = service.reports().saved().list(maxResults=MAX_PAGE_SIZE)
            saved_reports = result["items"]
            for saved_report in saved_reports:
                        'Saved report with ID "%s" and name "%s" was found.'
                        % (saved_report["id"], saved_report["name"])
            request = service.reports().saved().list_next(request, result)
