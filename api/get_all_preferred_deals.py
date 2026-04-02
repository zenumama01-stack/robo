"""Gets all preferred deals for the logged in user's account.
Tags: preferreddeals.list
        # Retrieve preferred deals list in pages and display data as we receive it.
        request = service.preferreddeals().list()
                deals = result["items"]
                for deal in deals:
                    output = 'Deal id "%s" ' % deal["id"]
                    if "advertiserName" in deal:
                        output += 'for advertiser "%s" ' % deal["advertiserName"]
                    if "buyerNetworkName" in deal:
                        output += 'on network "%s" ' % deal["buyerNetworkName"]
                    output += "was found."
                print("No preferred deals found!")
