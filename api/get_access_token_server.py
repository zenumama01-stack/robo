def get_auth_token(auth_url, redirect_uri):
    """Easy way to get the auth token. Wraps up all the threading
        auth_url (str): URL of auth server, including query params
            needed to get access token.
    token_acquired = threading.Event()
    s = GetAccessTokenServer((host_address, port), token_acquired, GetAccessTokenRequestHandler)
    token_acquired.wait()  # First wait for the response from the auth server
    auth_token = s.authentication_token
    return auth_token
class GetAccessTokenServer(HTTPServer, object):
        super(HTTPServer, self).init(server_address, RequestHandlerClass)
        self._access_token = None
        self._authentication_token = None
        return self._access_token
        self._access_token = value
    def authentication_token(self):
        return self._authentication_token
    @authentication_token.setter
    def authentication_token(self, value):
        self._authentication_token = value
class GetAccessTokenRequestHandler(BaseHTTPRequestHandler):
        if "access_token" in params:
            # Extract the access token query param
            self.server.access_token = params["access_token"][0]
        if "authentication_token" in params:
            # Extract the auth token query param
            self.server.authentication_token = params["authentication_token"][0]
