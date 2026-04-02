"""This example gets all ad clients for the logged in user's account.
Tags: adclients.list
        parents=[],
        # Retrieve ad client list in pages and display data as we receive it.
        request = service.adclients().list(maxResults=MAX_PAGE_SIZE)
        while request is not None:
            result = request.execute()
            ad_clients = result["items"]
            for ad_client in ad_clients:
                        'Ad client for product "%s" with ID "%s" was found. '
                        % (ad_client["productCode"], ad_client["id"])
                        "\tSupports reporting: %s"
                        % (ad_client["supportsReporting"] and "Yes" or "No")
            request = service.adclients().list_next(request, result)
