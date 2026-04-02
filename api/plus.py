"""Simple command-line sample for the Google+ API.
Command-line application that retrieves the list of the user's posts."""
        "plus",
        person = service.people().get(userId="me").execute()
        print("Got your ID: %s" % person["displayName"])
        print("%-040s -> %s" % ("[Activitity ID]", "[Content]"))
        # Don't execute the request until we reach the paging loop below.
        request = service.activities().list(userId=person["id"], collection="public")
        # Loop over every activity and print the ID and a short snippet of content.
            activities_doc = request.execute()
            for item in activities_doc.get("items", []):
                print("%-040s -> %s" % (item["id"], item["object"]["content"][:30]))
            request = service.activities().list_next(request, activities_doc)
