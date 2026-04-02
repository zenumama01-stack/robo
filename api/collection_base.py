class CollectionRequestBase(RequestBase):
        super(CollectionRequestBase, self).__init__(request_url, client, options)
    def _page_from_response(self, response):
        """Get the collection page from within the response
            response (:class:`CollectionResponseBase`): 
                The response to get the collection page from
            The collection page from within the response
        if response:
            if "@odata.nextLink" in response._prop_dict:
                next_page_link = response._prop_dict["@odata.nextLink"]
                response.collection_page._next_page_link = next_page_link
            return response.collection_page
class CollectionResponseBase(object):
    def __init__(self, prop_dict={}):
        self._prop_dict = prop_dict
        self._collection_page = None
    def collection_page(self):
class CollectionPageBase(object):
    def __init__(self, prop_list = []):
        self._prop_list = prop_list
        return len(self._prop_list)
