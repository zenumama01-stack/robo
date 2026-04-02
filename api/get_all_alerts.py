"""Gets all alerts available for the logged in user's account.
Tags: alerts.list
        # Retrieve alerts list in pages and display data as we receive it.
        request = service.alerts().list()
            if "items" in result:
                alerts = result["items"]
                for alert in alerts:
                            'Alert id "%s" with severity "%s" and type "%s" was found. '
                            % (alert["id"], alert["severity"], alert["type"])
                print("No alerts found!")
