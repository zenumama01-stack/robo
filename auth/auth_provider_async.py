def authenticate_async(self, code, redirect_uri, client_secret=None):
    """Takes in a code string, gets the access token,
    and creates session property bag in async
            The code provided by the oauth provider
        client_secret (str): Defaults to None, the client
            secret of your app
    future = self._loop.run_in_executor(None,
                                        self.authenticate,
    yield from future
def authenticate_request_async(self, request):
    """Authenticate and append the required
    headers to the request in async
                                        self.authenticate_request,
                                        request)
def refresh_token_async(self):
                                        self.refresh_token)
AuthProvider.authenticate_async = authenticate_async
AuthProvider.authenticate_request_async = authenticate_async
AuthProvider.refresh_token_async = authenticate_async
