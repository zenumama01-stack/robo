"""Channel notifications support.
  channel = new_webhook_channel("https://example.com/my_web_hook")
    bucket="some_bucket_id", body=channel.body()).execute()
from __future__ import absolute_import
from googleapiclient import _helpers as util
from googleapiclient import errors
# The unix time epoch starts at midnight 1970.
EPOCH = datetime.datetime(1970, 1, 1)
# Map the names of the parameters in the JSON channel description to
# the parameter names we use in the Channel class.
CHANNEL_PARAMS = {
    "address": "address",
    "expiration": "expiration",
    "params": "params",
    "resourceId": "resource_id",
    "resourceUri": "resource_uri",
    "token": "token",
X_GOOG_CHANNEL_ID = "X-GOOG-CHANNEL-ID"
X_GOOG_MESSAGE_NUMBER = "X-GOOG-MESSAGE-NUMBER"
X_GOOG_RESOURCE_STATE = "X-GOOG-RESOURCE-STATE"
X_GOOG_RESOURCE_URI = "X-GOOG-RESOURCE-URI"
X_GOOG_RESOURCE_ID = "X-GOOG-RESOURCE-ID"
def _upper_header_keys(headers):
    new_headers = {}
    for k, v in headers.items():
        new_headers[k.upper()] = v
    return new_headers
class Notification(object):
    """A Notification from a Channel.
    @util.positional(5)
    def __init__(self, message_number, state, resource_uri, resource_id):
        """Notification constructor.
            of "exists", "not_exists", or "sync".
        self.message_number = message_number
        self.state = state
        self.resource_uri = resource_uri
        self.resource_id = resource_id
class Channel(object):
    """A Channel for notifications.
        address,
        expiration=None,
        params=None,
        resource_id="",
        resource_uri="",
        """Create a new Channel.
        self.type = type
        self.token = token
        self.address = address
        self.expiration = expiration
    def body(self):
        """Build a body from the Channel.
            "id": self.id,
            "token": self.token,
            "type": self.type,
            "address": self.address,
        if self.params:
            result["params"] = self.params
        if self.resource_id:
            result["resourceId"] = self.resource_id
        if self.resource_uri:
            result["resourceUri"] = self.resource_uri
        if self.expiration:
            result["expiration"] = self.expiration
    def update(self, resp):
        """Update a channel with information from the response of watch().
        for json_name, param_name in CHANNEL_PARAMS.items():
            value = resp.get(json_name)
            if value is not None:
                setattr(self, param_name, value)
def notification_from_headers(channel, headers):
    """Parse a notification from the webhook request headers, validate
    headers = _upper_header_keys(headers)
    channel_id = headers[X_GOOG_CHANNEL_ID]
    if channel.id != channel_id:
        raise errors.InvalidNotificationError(
            "Channel id mismatch: %s != %s" % (channel.id, channel_id)
        message_number = int(headers[X_GOOG_MESSAGE_NUMBER])
        state = headers[X_GOOG_RESOURCE_STATE]
        resource_uri = headers[X_GOOG_RESOURCE_URI]
        resource_id = headers[X_GOOG_RESOURCE_ID]
        return Notification(message_number, state, resource_uri, resource_id)
@util.positional(2)
def new_webhook_channel(url, token=None, expiration=None, params=None):
    """Create a new webhook Channel.
    expiration_ms = 0
    if expiration:
        delta = expiration - EPOCH
        expiration_ms = (
            delta.microseconds / 1000 + (delta.seconds + delta.days * 24 * 3600) * 1000
        if expiration_ms < 0:
    return Channel(
        "web_hook",
        str(uuid.uuid4()),
        expiration=expiration_ms,
        params=params,
