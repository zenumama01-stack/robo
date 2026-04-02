from __future__ import unicode_literals, with_statement
from .http_provider_base import HttpProviderBase
from .http_response import HttpResponse
class HttpProvider(HttpProviderBase):
    def send(self, method, headers, url, data=None, content=None, path=None):
        """Send the built request using all the specified
            method (str): The HTTP method to use (ex. GET)
            headers (dict of (str, str)): A dictionary of name-value
                pairs for headers in the request
            url (str): The URL for the request to be sent to
            data (str): Defaults to None, data to include in the body
                of the request which is not in JSON format
            content (dict): Defaults to None, a dictionary to include
                in JSON format in the body of the request
            path (str): Defaults to None, the path to the local file
                to send in the body of the request
            :class:`HttpResponse<onedrivesdk.http_response.HttpResponse>`:
                The response to the request
        if path:
            with open(path, mode='rb') as f:
                request = requests.Request(method,
                                           data=f)
                prepped = request.prepare()
                response = session.send(prepped)
                                       json=content)
        custom_response = HttpResponse(response.status_code, response.headers, response.text)
        return custom_response
    def download(self, headers, url, path):
        """Downloads a file to the stated path
                pairs to be used as headers in the request
            url (str): The URL from which to download the file
            path (str): The local path to save the downloaded file
        response = requests.get(
            headers=headers)
        if response.status_code == 200:
            with open(path, 'wb') as f:
                for chunk in response.iter_content(chunk_size=1024):
                        f.write(chunk)
                        f.flush()
            custom_response = HttpResponse(response.status_code, response.headers, None)
