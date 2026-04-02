from ..request.drives_collection import DrivesCollectionRequestBuilder
from ..request.shares_collection import SharesCollectionRequestBuilder
class OneDriveClient(object):
    def __init__(self, base_url, auth_provider, http_provider, loop=None):
        """Initialize the :class:`OneDriveClient` to be
            used for all OneDrive API interactions
            base_url (str): The OneDrive base url to use for API interactions
            auth_provider(:class:`AuthProviderBase<onedrivesdk.auth_provider_base.AuthProviderBase>`):
                The authentication provider used by the client to auth
                with OneDrive services
            http_provider(:class:`HttpProviderBase<onedrivesdk.http_provider_base.HttpProviderBase>`):
                The HTTP provider used by the client to send all 
                requests to OneDrive
            loop (BaseEventLoop): Default to None, the AsyncIO loop 
                to use for all async requests
        self.base_url = base_url
        self.auth_provider = auth_provider
        self.http_provider = http_provider
    def auth_provider(self):
        """Gets and sets the client auth provider
            :class:`AuthProviderBase<onedrivesdk.auth_provider_base.AuthProviderBase>`: 
            The authentication provider
        return self._auth_provider
    @auth_provider.setter
    def auth_provider(self, value):
        self._auth_provider = value
    def http_provider(self):
        """Gets and sets the client HTTP provider
            :class:`HttpProviderBase<onedrivesdk.http_provider_base.HttpProviderBase>`: 
                The HTTP provider
        return self._http_provider
    @http_provider.setter
    def http_provider(self, value):
        self._http_provider = value
    def base_url(self):
        """Gets and sets the base URL used by the client to make requests
            str: The base URL
    def base_url(self, value):
        self._base_url = value
        """Get the DrivesCollectionRequestBuilder for constructing requests
            :class:`DrivesCollectionRequestBuilder<onedrivesdk.request.drives_collection.DrivesCollectionRequestBuilder>`:
                The DrivesCollectionRequestBuilder to return
        return DrivesCollectionRequestBuilder(self.base_url + "drives", self)
        """Get the SharesCollectionRequestBuilder for constructing requests
            :class:`SharesCollectionRequestBuilder<onedrivesdk.request.shares_collection.SharesCollectionRequestBuilder>`:
                The SharesCollectionRequestBuilder to return
        return SharesCollectionRequestBuilder(self.base_url + "shares", self)
