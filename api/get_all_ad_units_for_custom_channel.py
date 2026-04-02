"""This example gets all ad units corresponding to a specified custom channel.
To get custom channels, run get_all_custom_channels.py.
Tags: customchannels.adunits.list
    "ad_client_id", help="The ID of the ad client with the specified custom channel"
    "custom_channel_id", help="The ID of the custom channel for which to get ad units"
    custom_channel_id = flags.custom_channel_id
        request = (
            service.customchannels()
            .adunits()
            .list(
                adClientId=ad_client_id,
                customChannelId=custom_channel_id,
                maxResults=MAX_PAGE_SIZE,
