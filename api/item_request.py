class ItemRequest(RequestBase):
    """The type ItemRequest."""
        """Constructs a new ItemRequest.
            request_url (str): The url to perform the ItemRequest
        super(ItemRequest, self).__init__(request_url, client, options)
        """Deletes the specified Item."""
        """Gets the specified Item.
                The Item.
        entity = Item(json.loads(self.send().content))
    def update(self, item):
        """Updates the specified Item.
            item (:class:`Item<onedrivesdk.model.item.Item>`):
                The Item to update.
                The updated Item.
        entity = Item(json.loads(self.send(item).content))
            if value.permissions:
                if "permissions@odata.nextLink" in value._prop_dict:
                    next_page_link = value._prop_dict["permissions@odata.nextLink"]
                    value.permissions._next_page_link = next_page_link
            if value.subscriptions:
                if "subscriptions@odata.nextLink" in value._prop_dict:
                    next_page_link = value._prop_dict["subscriptions@odata.nextLink"]
                    value.subscriptions._next_page_link = next_page_link
            if value.versions:
                if "versions@odata.nextLink" in value._prop_dict:
                    next_page_link = value._prop_dict["versions@odata.nextLink"]
                    value.versions._next_page_link = next_page_link
            if value.children:
                if "children@odata.nextLink" in value._prop_dict:
                    next_page_link = value._prop_dict["children@odata.nextLink"]
                    value.children._next_page_link = next_page_link
            if value.tags:
                if "tags@odata.nextLink" in value._prop_dict:
                    next_page_link = value._prop_dict["tags@odata.nextLink"]
                    value.tags._next_page_link = next_page_link
            if value.thumbnails:
                if "thumbnails@odata.nextLink" in value._prop_dict:
                    next_page_link = value._prop_dict["thumbnails@odata.nextLink"]
                    value.thumbnails._next_page_link = next_page_link
        """Gets the specified Item in async.
    def update_async(self, item):
        """Updates the specified Item in async
                                                    item)
