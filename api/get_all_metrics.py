"""Gets all metrics available for the logged in user's account.
Tags: metadata.metrics.list
        request = service.metadata().metrics().list()
                metrics = result["items"]
                for metric in metrics:
                            'Metric id "%s" for product(s): [%s] was found. '
                            % (metric["id"], ", ".join(metric["supportedProducts"]))
                print("No metrics found!")
