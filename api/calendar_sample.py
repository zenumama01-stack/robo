"""Simple command-line sample for the Calendar API.
Command-line application that retrieves the list of the user's calendars."""
        "calendar",
        scope="https://www.googleapis.com/auth/calendar.readonly",
        page_token = None
            calendar_list = service.calendarList().list(pageToken=page_token).execute()
            for calendar_list_entry in calendar_list["items"]:
                print(calendar_list_entry["summary"])
            page_token = calendar_list.get("nextPageToken")
            if not page_token:
            "the application to re-authorize."
