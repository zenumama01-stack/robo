"""This example gets all ad units in an ad client.
Tags: adunits.list
        # Retrieve ad unit list in pages and display data as we receive it.
        request = service.adunits().list(
            adClientId=ad_client_id, maxResults=MAX_PAGE_SIZE
            ad_units = result["items"]
            for ad_unit in ad_units:
                        'Ad unit with code "%s", name "%s" and status "%s" was found. '
                        % (ad_unit["code"], ad_unit["name"], ad_unit["status"])
            request = service.adunits().list_next(request, result)
