"""App Engine memcache based cache for the discovery document."""
# This is only an optional dependency because we only import this
# module when google.appengine.api.memcache is available.
from google.appengine.api import memcache
from . import base
from ..discovery_cache import DISCOVERY_DOC_MAX_AGE
NAMESPACE = "google-api-client"
class Cache(base.Cache):
    """A cache with app engine memcache API."""
    def __init__(self, max_age):
        self._max_age = max_age
    def get(self, url):
            return memcache.get(url, namespace=NAMESPACE)
            LOGGER.warning(e, exc_info=True)
    def set(self, url, content):
            memcache.set(url, content, time=int(self._max_age), namespace=NAMESPACE)
cache = Cache(max_age=DISCOVERY_DOC_MAX_AGE)
