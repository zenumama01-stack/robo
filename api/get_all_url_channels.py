"""This example gets all URL channels in an ad client.
Tags: urlchannels.list
    "ad_client_id", help="The ad client ID for which to get URL channels"
        # Retrieve URL channel list in pages and display data as we receive it.
        request = service.urlchannels().list(
            url_channels = result["items"]
            for url_channel in url_channels:
                        'URL channel with URL pattern "%s" was found.'
                        % url_channel["urlPattern"]
