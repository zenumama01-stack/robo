from __future__ import generators, unicode_literals
from .error import OneDriveError
class HttpResponse(object):
    def __init__(self, status, headers, content):
        """Initialize the HttpResponse class returned after
        an HTTP request is made
            status (int): HTTP status (ex. 200, 201, etc.)
            headers (dict of (str, str)): The headers in the
            content (str): The body of the response
        self._status = status
        if self.content and (self.status < 200 or self.status >= 300):
                message = json.loads(self.content)
            except ValueError:  # Invalid or empty response message
                from .error import ErrorCode
                message = {
                        "code": ErrorCode.Malformed,
                        "message": "The following invalid JSON was returned:\n%s" % self.content
            if "error" in message:
                if type(message["error"]) == dict:
                    raise OneDriveError(message["error"], self.status)
                    raise Exception(str(message["error"]))
        properties = {
            'Status': self.status,
            'Headers': self.headers,
            'Content': self.content
        ret = ""
        for k, v in properties.items():
            ret += "{}: {}\n".format(k, v)
        """The HTTP status of the response
            int: HTTP status
    def headers(self):
        """The headers of the response
            dict of (str, str):
                The headers used by the response
        return self._headers
    def content(self):
        """The content of the response
            str: The body of the response
        return self._content
