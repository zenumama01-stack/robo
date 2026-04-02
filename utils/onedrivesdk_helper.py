from ..http_provider import HttpProvider
from ..auth_provider import AuthProvider
def get_default_client(client_id, scopes):
    """Deprecated. Proxy of :method:`get_consumer_client()`.
    Get a client using the default HttpProvider and
    AuthProvider classes.
        client_id (str): The client id for your application
        scopes (list of str): The scopes required for your
        :class:`OneDriveClient<onedrivesdk.requests.one_drive_client.OneDriveClient>`:
            A OneDriveClient for making OneDrive requests.
    return get_consumer_client(client_id, scopes)
def get_consumer_client(client_id, scopes):
    """Get a client using the default HttpProvider and
    AuthProvider classes
    http_provider = HttpProvider()
    auth_provider = AuthProvider(http_provider=http_provider,
                                 client_id=client_id,
                                 scopes=scopes)
    return OneDriveClient("https://api.onedrive.com/v1.0/",
                          auth_provider,
                          http_provider)
def get_business_client(client_id, scopes, base_url):
        base_url (str): Base URL of OneDrive for Business tenant.
            For example, "https://my-sharepoint.contoso.com/v1.0/"
    return OneDriveClient(base_url,
