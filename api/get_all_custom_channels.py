"""This example gets all custom channels in an ad client.
Tags: customchannels.list
    "ad_client_id", help="The ad client ID for which to get custom channels"
        # Retrieve custom channel list in pages and display data as we receive it.
        request = service.customchannels().list(
            custom_channels = result["items"]
            for custom_channel in custom_channels:
                        'Custom channel with id "%s" and name "%s" was found. '
                        % (custom_channel["id"], custom_channel["name"])
                if "targetingInfo" in custom_channel:
                    print("  Targeting info:")
                    targeting_info = custom_channel["targetingInfo"]
                    if "adsAppearOn" in targeting_info:
                        print("    Ads appear on: %s" % targeting_info["adsAppearOn"])
                    if "location" in targeting_info:
                        print("    Location: %s" % targeting_info["location"])
                    if "description" in targeting_info:
                        print("    Description: %s" % targeting_info["description"])
                    if "siteLanguage" in targeting_info:
                        print("    Site language: %s" % targeting_info["siteLanguage"])
            request = service.customchannels().list_next(request, result)
