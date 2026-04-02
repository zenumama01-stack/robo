class RequestBuilderBase(object):
    def __init__(self, request_url, client):
        """Initialize a request builder which returns a request
        when request() is called
            request_url (str): The URL to construct the request
                for
                The client with which the request will be made
    def append_to_request_url(self, url_segment):
        """Appends a URL portion to the current request URL
            url_segment (str): The segment you would like to append
                to the existing request URL.
        return self._request_url + "/" + url_segment
