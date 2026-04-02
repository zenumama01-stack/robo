class ItemContentRequest(RequestBase):
        """Initialize the ItemContentRequest
            request_url (str): The url to perform the ItemContentRequest
        super(ItemContentRequest, self).__init__(request_url, client, options)
    def upload(self, content_local_path):
        """Uploads the file using PUT
            content_local_path (str):
                The path to the local file to upload.
                The created Item.
        entity_response = self.send(path=content_local_path)
        entity = Item(json.loads(entity_response.content))
    def download(self, content_local_path):
        """Downloads the specified Item.
                The path where the Item should be downloaded to
        self.download_item(content_local_path)
class ItemContentRequestBuilder(RequestBuilderBase):
        """Initialize the ItemContentRequestBuilder
            request_url (str): The request URL to initialize
                the ItemContentRequestBuilder at
                The client to use for requests made by the
                ItemContentRequestBuilder
        super(ItemContentRequestBuilder, self).__init__(request_url, client)
    def request(self):
        """Builds the ItemContentRequest
            :class:`ItemContentRequest<onedrivesdk.request.item_content.ItemContentRequest>`:
                The ItemContentRequest
        return ItemContentRequest(self._request_url, self._client, None)    def upload_async(self, content_local_path):
        """Uploads the file using PUT in async
                                                    self.put,
                                                    content_local_path)
    def download_async(self, content_local_path):
        Downloads the specified Item in async.
                                                    self.download,
        return ItemContentRequest(self._request_url, self._client, None)    def upload_async(self, content_local_path):
        return ItemContentRequest(self._request_url, self._client, None)    def upload_async(self, content_local_path):
