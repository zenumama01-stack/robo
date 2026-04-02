from onedrivesdk import http_provider_base, http_response
class HttpProviderWithProxy(http_provider_base.HttpProviderBase):
    """Use this HttpProvider when you want to proxy your requests.
    For example, if you have an HTTP request capture suite, you
    can use this provider to proxy the requests through that
    capture suite.    
    DEFAULT_PROXIES = {
        'http': 'http://127.0.0.1:8888',
        'https': 'https://127.0.0.1:8888'
    def __init__(self, proxies=None, verify_ssl=True):
        """Initializes the provider. Proxy and SSL settings are stored
        in the object and applied to every request.
            proxies (dict of str:str):
                Mapping of protocols to proxy URLs. See `requests`
                documentation:
                http://docs.python-requests.org/en/latest/api/#requests.request
                If None, HttpProviderWithProxy.DEFAULT_PROXIES is used.
            verify_ssl (bool):
                Whether SSL certs should be verified during
                request proxy.
        self.proxies = proxies if proxies is not None \
            else HttpProviderWithProxy.DEFAULT_PROXIES
        self.verify_ssl = verify_ssl
                response = session.send(prepped,
                                        verify=self.verify_ssl,
                                        proxies=self.proxies)
        custom_response = http_response.HttpResponse(response.status_code, response.headers, response.text)
            custom_response = http_response.HttpResponse(response.status_code, response.headers, None)
