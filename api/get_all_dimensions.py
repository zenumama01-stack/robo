"""Gets all dimensions available for the logged in user's account.
Tags: metadata.dimensions.list
        # Retrieve metrics list in pages and display data as we receive it.
        request = service.metadata().dimensions().list()
                dimensions = result["items"]
                for dimension in dimensions:
                            'Dimension id "%s" for product(s): [%s] was found. '
                                dimension["id"],
                                ", ".join(dimension["supportedProducts"]),
                print("No dimensions found!")
