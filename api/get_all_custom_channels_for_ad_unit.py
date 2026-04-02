"""This example gets all custom channels an ad unit has been added to.
To get ad clients, run get_all_ad_clients.py. To get ad units, run
get_all_ad_units.py.
    "ad_client_id", help="The ID of the ad client with the specified ad unit"
    "ad_unit_id", help="The ID of the ad unit for which to get custom channels"
    ad_unit_id = flags.ad_unit_id
            service.adunits()
            .customchannels()
                adClientId=ad_client_id, adUnitId=ad_unit_id, maxResults=MAX_PAGE_SIZE
                        'Custom channel with code "%s" and name "%s" was found. '
                        % (custom_channel["code"], custom_channel["name"])
