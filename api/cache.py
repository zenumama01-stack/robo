import pickle
    import fcntl
    # Probably on a windows system
    # TODO: use win32file
class Cache:
    """Cache interface"""
    def __init__(self, timeout=60):
        """Initialize the cache
            timeout: number of seconds to keep a cached entry
    def store(self, key, value):
        """Add new record to cache
            key: entry key
            value: data of entry
        raise NotImplementedError
    def get(self, key, timeout=None):
        """Get cached entry if exists and not expired
            key: which entry to get
            timeout: override timeout with this value [optional]
    def count(self):
        """Get count of entries currently stored in cache"""
    def cleanup(self):
        """Delete any expired entries in cache."""
    def flush(self):
        """Delete all cached entries"""
class MemoryCache(Cache):
    """In-memory cache"""
        Cache.__init__(self, timeout)
        self._entries = {}
        self.lock = threading.Lock()
    def __getstate__(self):
        # pickle
        return {'entries': self._entries, 'timeout': self.timeout}
    def __setstate__(self, state):
        # unpickle
        self._entries = state['entries']
        self.timeout = state['timeout']
    def _is_expired(self, entry, timeout):
        return timeout > 0 and (time.time() - entry[0]) >= timeout
        self.lock.acquire()
        self._entries[key] = (time.time(), value)
        self.lock.release()
            # check to see if we have this key
            entry = self._entries.get(key)
            if not entry:
                # no hit, return nothing
            # use provided timeout in arguments if provided
            # otherwise use the one provided during init.
            if timeout is None:
                timeout = self.timeout
            # make sure entry is not expired
            if self._is_expired(entry, timeout):
                # entry expired, delete and return nothing
                del self._entries[key]
            # entry found and not expired, return it
            return entry[1]
        return len(self._entries)
            for k, v in dict(self._entries).items():
                if self._is_expired(v, self.timeout):
                    del self._entries[k]
        self._entries.clear()
class FileCache(Cache):
    """File-based cache"""
    # locks used to make cache thread-safe
    cache_locks = {}
    def __init__(self, cache_dir, timeout=60):
        if os.path.exists(cache_dir) is False:
            os.mkdir(cache_dir)
        self.cache_dir = cache_dir
        if cache_dir in FileCache.cache_locks:
            self.lock = FileCache.cache_locks[cache_dir]
            FileCache.cache_locks[cache_dir] = self.lock
        if os.name == 'posix':
            self._lock_file = self._lock_file_posix
            self._unlock_file = self._unlock_file_posix
        elif os.name == 'nt':
            self._lock_file = self._lock_file_win32
            self._unlock_file = self._unlock_file_win32
            log.warning('FileCache locking not supported on this system!')
            self._lock_file = self._lock_file_dummy
            self._unlock_file = self._unlock_file_dummy
    def _get_path(self, key):
        md5 = hashlib.md5()
        md5.update(key.encode('utf-8'))
        return os.path.join(self.cache_dir, md5.hexdigest())
    def _lock_file_dummy(self, path, exclusive=True):
    def _unlock_file_dummy(self, lock):
    def _lock_file_posix(self, path, exclusive=True):
        lock_path = path + '.lock'
        if exclusive is True:
            f_lock = open(lock_path, 'w')
            fcntl.lockf(f_lock, fcntl.LOCK_EX)
            f_lock = open(lock_path, 'r')
            fcntl.lockf(f_lock, fcntl.LOCK_SH)
        if os.path.exists(lock_path) is False:
            f_lock.close()
        return f_lock
    def _unlock_file_posix(self, lock):
        lock.close()
    def _lock_file_win32(self, path, exclusive=True):
        # TODO: implement
    def _unlock_file_win32(self, lock):
    def _delete_file(self, path):
        os.remove(path)
        if os.path.exists(path + '.lock'):
            os.remove(path + '.lock')
        path = self._get_path(key)
            # acquire lock and open file
            f_lock = self._lock_file(path)
            datafile = open(path, 'wb')
            # write data
            pickle.dump((time.time(), value), datafile)
            # close and unlock file
            datafile.close()
            self._unlock_file(f_lock)
        return self._get(self._get_path(key), timeout)
    def _get(self, path, timeout):
        if os.path.exists(path) is False:
            # no record
            # acquire lock and open
            f_lock = self._lock_file(path, False)
            datafile = open(path, 'rb')
            # read pickled object
            created_time, value = pickle.load(datafile)
            # check if value is expired
            if timeout > 0:
                if (time.time() - created_time) >= timeout:
                    # expired! delete from cache
                    value = None
                    self._delete_file(path)
            # unlock and return result
        c = 0
        for entry in os.listdir(self.cache_dir):
            if entry.endswith('.lock'):
            c += 1
        return c
            self._get(os.path.join(self.cache_dir, entry), None)
            self._delete_file(os.path.join(self.cache_dir, entry))
class MemCacheCache(Cache):
    def __init__(self, client, timeout=60):
            client: The memcache client
        self.client = client
        self.client.set(key, value, time=self.timeout)
            timeout: override timeout with this value [optional].
            DOES NOT WORK HERE
        return self.client.get(key)
        """Get count of entries currently stored in cache. RETURN 0"""
        """Delete any expired entries in cache. NO-OP"""
        """Delete all cached entries. NO-OP"""
class RedisCache(Cache):
    """Cache running in a redis server"""
    def __init__(self, client,
                 timeout=60,
                 keys_container='tweepy:keys',
                 pre_identifier='tweepy:'):
        self.keys_container = keys_container
        self.pre_identifier = pre_identifier
        # Returns true if the entry has expired
        """Store the key, value pair in our redis server"""
        # Prepend tweepy to our key,
        # this makes it easier to identify tweepy keys in our redis server
        key = self.pre_identifier + key
        # Get a pipe (to execute several redis commands in one step)
        pipe = self.client.pipeline()
        # Set our values in a redis hash (similar to python dict)
        pipe.set(key, pickle.dumps((time.time(), value)))
        # Set the expiration
        pipe.expire(key, self.timeout)
        # Add the key to a set containing all the keys
        pipe.sadd(self.keys_container, key)
        # Execute the instructions in the redis server
        pipe.execute()
        """Given a key, returns an element from the redis table"""
        # Check to see if we have this key
        unpickled_entry = self.client.get(key)
        if not unpickled_entry:
            # No hit, return nothing
        entry = pickle.loads(unpickled_entry)
        # Use provided timeout in arguments if provided
        # Make sure entry is not expired
            self.delete_entry(key)
        """Note: This is not very efficient,
        since it retrieves all the keys from the redis
        server to know how many keys we have"""
        return len(self.client.smembers(self.keys_container))
    def delete_entry(self, key):
        """Delete an object from the redis table"""
        pipe.srem(self.keys_container, key)
        pipe.delete(key)
        """Cleanup all the expired keys"""
        keys = self.client.smembers(self.keys_container)
        for key in keys:
            entry = self.client.get(key)
            if entry:
                entry = pickle.loads(entry)
                if self._is_expired(entry, self.timeout):
        """Delete all entries from the cache"""
class MongodbCache(Cache):
    """A simple pickle-based MongoDB cache system."""
    def __init__(self, db, timeout=3600, collection='tweepy_cache'):
        """Should receive a "database" cursor from pymongo."""
        self.col = db[collection]
        self.col.create_index('created', expireAfterSeconds=timeout)
        from bson.binary import Binary
        now = datetime.datetime.utcnow()
        blob = Binary(pickle.dumps(value))
        self.col.insert({'created': now, '_id': key, 'value': blob})
        if timeout:
        obj = self.col.find_one({'_id': key})
        if obj:
            return pickle.loads(obj['value'])
        return self.col.find({}).count()
        return self.col.remove({'_id': key})
        """MongoDB will automatically clear expired keys."""
        self.col.drop()
        self.col.create_index('created', expireAfterSeconds=self.timeout)
from .utils import expand_path, traverse_obj, version_tuple, write_json_file
    def __init__(self, ydl):
        self._ydl = ydl
    def _get_root_dir(self):
        res = self._ydl.params.get('cachedir')
        if res is None:
            cache_root = os.getenv('XDG_CACHE_HOME', '~/.cache')
            res = os.path.join(cache_root, 'yt-dlp')
        return expand_path(res)
    def _get_cache_fn(self, section, key, dtype):
        assert re.match(r'^[\w.-]+$', section), f'invalid section {section!r}'
        key = urllib.parse.quote(key, safe='').replace('%', ',')  # encode non-ascii characters
        return os.path.join(self._get_root_dir(), section, f'{key}.{dtype}')
    def enabled(self):
        return self._ydl.params.get('cachedir') is not False
    def store(self, section, key, data, dtype='json'):
        assert dtype in ('json',)
        if not self.enabled:
        fn = self._get_cache_fn(section, key, dtype)
            os.makedirs(os.path.dirname(fn), exist_ok=True)
            self._ydl.write_debug(f'Saving {section}.{key} to cache')
            write_json_file({'yt-dlp_version': __version__, 'data': data}, fn)
            tb = traceback.format_exc()
            self._ydl.report_warning(f'Writing cache to {fn!r} failed: {tb}')
    def _validate(self, data, min_ver):
        version = traverse_obj(data, 'yt-dlp_version')
        if not version:  # Backward compatibility
            data, version = {'data': data}, '2022.08.19'
        if not min_ver or version_tuple(version) >= version_tuple(min_ver):
            return data['data']
        self._ydl.write_debug(f'Discarding old cache from version {version} (needs {min_ver})')
    def load(self, section, key, dtype='json', default=None, *, min_ver=None):
        cache_fn = self._get_cache_fn(section, key, dtype)
                with open(cache_fn, encoding='utf-8') as cachef:
                    self._ydl.write_debug(f'Loading {section}.{key} from cache')
                    return self._validate(json.load(cachef), min_ver)
            except (ValueError, KeyError):
                    file_size = os.path.getsize(cache_fn)
                except OSError as oe:
                    file_size = str(oe)
                self._ydl.report_warning(f'Cache retrieval from {cache_fn} failed ({file_size})')
    def remove(self):
            self._ydl.to_screen('Cache is disabled (Did you combine --no-cache-dir and --rm-cache-dir?)')
        cachedir = self._get_root_dir()
        if not any((term in cachedir) for term in ('cache', 'tmp')):
            raise Exception(f'Not removing directory {cachedir} - this does not look like a cache dir')
        self._ydl.to_screen(
            f'Removing cache dir {cachedir} .', skip_eol=True)
        if os.path.exists(cachedir):
            self._ydl.to_screen('.', skip_eol=True)
            shutil.rmtree(cachedir)
        self._ydl.to_screen('.')
"""PUBLIC API"""
import enum
from yt_dlp.extractor.youtube.pot._provider import (
    IEContentProvider,
    IEContentProviderError,
    register_preference_generic,
    register_provider_generic,
from yt_dlp.extractor.youtube.pot._registry import (
    _pot_cache_provider_preferences,
    _pot_cache_providers,
    _pot_pcs_providers,
from yt_dlp.extractor.youtube.pot.provider import PoTokenRequest
class PoTokenCacheProviderError(IEContentProviderError):
    """An error occurred while fetching a PO Token"""
class PoTokenCacheProvider(IEContentProvider, abc.ABC, suffix='PCP'):
    def get(self, key: str) -> str | None:
    def store(self, key: str, value: str, expires_at: int):
    def delete(self, key: str):
class CacheProviderWritePolicy(enum.Enum):
    WRITE_ALL = enum.auto()    # Write to all cache providers
    WRITE_FIRST = enum.auto()  # Write to only the first cache provider
class PoTokenCacheSpec:
    key_bindings: dict[str, str | None]
    default_ttl: int
    write_policy: CacheProviderWritePolicy = CacheProviderWritePolicy.WRITE_ALL
    # Internal
    _provider: PoTokenCacheSpecProvider | None = None
class PoTokenCacheSpecProvider(IEContentProvider, abc.ABC, suffix='PCSP'):
    def is_available(self) -> bool:
    def generate_cache_spec(self, request: PoTokenRequest) -> PoTokenCacheSpec | None:
        """Generate a cache spec for the given request"""
def register_provider(provider: type[PoTokenCacheProvider]):
    """Register a PoTokenCacheProvider class"""
    return register_provider_generic(
        provider=provider,
        base_class=PoTokenCacheProvider,
        registry=_pot_cache_providers.value,
def register_spec(provider: type[PoTokenCacheSpecProvider]):
    """Register a PoTokenCacheSpecProvider class"""
        base_class=PoTokenCacheSpecProvider,
        registry=_pot_pcs_providers.value,
def register_preference(
        *providers: type[PoTokenCacheProvider]) -> typing.Callable[[CacheProviderPreference], CacheProviderPreference]:
    """Register a preference for a PoTokenCacheProvider"""
    return register_preference_generic(
        PoTokenCacheProvider,
        _pot_cache_provider_preferences.value,
        *providers,
if typing.TYPE_CHECKING:
    CacheProviderPreference = typing.Callable[[PoTokenCacheProvider, PoTokenRequest], int]
from django.contrib.sessions.backends.base import CreateError, SessionBase, UpdateError
from django.core.cache import caches
KEY_PREFIX = "django.contrib.sessions.cache"
class SessionStore(SessionBase):
    A cache-based session store.
    cache_key_prefix = KEY_PREFIX
        self._cache = caches[settings.SESSION_CACHE_ALIAS]
        super().__init__(session_key)
    def cache_key(self):
        return self.cache_key_prefix + self._get_or_create_session_key()
    async def acache_key(self):
        return self.cache_key_prefix + await self._aget_or_create_session_key()
            session_data = self._cache.get(self.cache_key)
            # Some backends (e.g. memcache) raise an exception on invalid
            # cache keys. If this happens, reset the session. See #17810.
            session_data = None
        if session_data is not None:
            return session_data
            session_data = await self._cache.aget(await self.acache_key())
        # Because a cache can fail silently (e.g. memcache), we don't know if
        # we are failing to create a new session because of a key collision or
        # because the cache is missing. So we try for a (large) number of times
        # and then raise an exception. That's the risk you shoulder if using
        # cache backing.
        for i in range(10000):
                self.save(must_create=True)
            except CreateError:
            "Unable to create a new session key. "
            "It is likely that the cache is unavailable."
                await self.asave(must_create=True)
        if self.session_key is None:
            return self.create()
        if must_create:
            func = self._cache.add
        elif self._cache.get(self.cache_key) is not None:
            func = self._cache.set
            raise UpdateError
        result = func(
            self.cache_key,
            self._get_session(no_load=must_create),
            self.get_expiry_age(),
        if must_create and not result:
            raise CreateError
            return await self.acreate()
            func = self._cache.aadd
        elif await self._cache.aget(await self.acache_key()) is not None:
            func = self._cache.aset
        result = await func(
            await self.acache_key(),
            await self._aget_session(no_load=must_create),
            await self.aget_expiry_age(),
            bool(session_key) and (self.cache_key_prefix + session_key) in self._cache
        return bool(session_key) and await self._cache.ahas_key(
            self.cache_key_prefix + session_key
            session_key = self.session_key
        self._cache.delete(self.cache_key_prefix + session_key)
        await self._cache.adelete(self.cache_key_prefix + session_key)
Cache middleware. If enabled, each Django-powered page will be cached based on
URL. The canonical way to enable cache middleware is to set
``UpdateCacheMiddleware`` as your first piece of middleware, and
``FetchFromCacheMiddleware`` as the last::
    MIDDLEWARE = [
        'django.middleware.cache.UpdateCacheMiddleware',
        'django.middleware.cache.FetchFromCacheMiddleware'
This is counterintuitive, but correct: ``UpdateCacheMiddleware`` needs to run
last during the response phase, which processes middleware bottom-up;
``FetchFromCacheMiddleware`` needs to run last during the request phase, which
processes middleware top-down.
The single-class ``CacheMiddleware`` can be used for some simple sites.
However, if any other piece of middleware needs to affect the cache key, you'll
need to use the two-part ``UpdateCacheMiddleware`` and
``FetchFromCacheMiddleware``. This'll most often happen when you're using
Django's ``LocaleMiddleware``.
More details about how the caching works:
* Only GET or HEAD-requests with status code 200 are cached.
* The number of seconds each page is stored for is set by the "max-age" section
  of the response's "Cache-Control" header, falling back to the
  CACHE_MIDDLEWARE_SECONDS setting if the section was not found.
* This middleware expects that a HEAD request is answered with the same
  response headers exactly like the corresponding GET request.
* When a hit occurs, a shallow copy of the original response object is returned
  from process_request.
* Pages will be cached based on the contents of the request headers listed in
  the response's "Vary" header.
* This middleware also sets ETag, Last-Modified, Expires and Cache-Control
  headers on the response object.
from django.core.cache import DEFAULT_CACHE_ALIAS, caches
from django.utils.cache import (
    get_cache_key,
    get_max_age,
    has_vary_header,
    learn_cache_key,
    patch_response_headers,
from django.utils.deprecation import MiddlewareMixin
from django.utils.http import parse_http_date_safe
class UpdateCacheMiddleware(MiddlewareMixin):
    Response-phase cache middleware that updates the cache if the response is
    cacheable.
    Must be used as part of the two-part update/fetch cache middleware.
    UpdateCacheMiddleware must be the first piece of middleware in MIDDLEWARE
    so that it'll get called last during the response phase.
    def __init__(self, get_response):
        super().__init__(get_response)
        self.cache_timeout = settings.CACHE_MIDDLEWARE_SECONDS
        self.page_timeout = None
        self.key_prefix = settings.CACHE_MIDDLEWARE_KEY_PREFIX
        self.cache_alias = settings.CACHE_MIDDLEWARE_ALIAS
        return caches[self.cache_alias]
    def _should_update_cache(self, request, response):
        return hasattr(request, "_cache_update_cache") and request._cache_update_cache
    def process_response(self, request, response):
        """Set the cache, if needed."""
        if not self._should_update_cache(request, response):
            # We don't need to update the cache, just return.
        if response.streaming or response.status_code not in (200, 304):
        # Don't cache responses that set a user-specific (and maybe security
        # sensitive) cookie in response to a cookie-less request.
            not request.COOKIES
            and response.cookies
            and has_vary_header(response, "Cookie")
        # Don't cache responses when the Cache-Control header is set to
        # private, no-cache, or no-store.
        cache_control = response.get("Cache-Control", ())
            directive in cache_control
            for directive in (
                "no-cache",
                "no-store",
        # Page timeout takes precedence over the "max-age" and the default
        # cache timeout.
        timeout = self.page_timeout
            # The timeout from the "max-age" section of the "Cache-Control"
            # header takes precedence over the default cache timeout.
            timeout = get_max_age(response)
                timeout = self.cache_timeout
                # max-age was set to 0, don't cache.
        patch_response_headers(response, timeout)
        if timeout and response.status_code == 200:
            cache_key = learn_cache_key(
                request, response, timeout, self.key_prefix, cache=self.cache
                response.add_post_render_callback(
                    lambda r: self.cache.set(cache_key, r, timeout)
                self.cache.set(cache_key, response, timeout)
class FetchFromCacheMiddleware(MiddlewareMixin):
    Request-phase cache middleware that fetches a page from the cache.
    FetchFromCacheMiddleware must be the last piece of middleware in MIDDLEWARE
    so that it'll get called last during the request phase.
    def process_request(self, request):
        Check whether the page is already cached and return the cached
        version if available.
        if request.method not in ("GET", "HEAD"):
            request._cache_update_cache = False
            return None  # Don't bother checking the cache.
        # try and get the cached GET response
        cache_key = get_cache_key(request, self.key_prefix, "GET", cache=self.cache)
        if cache_key is None:
            request._cache_update_cache = True
            return None  # No cache information available, need to rebuild.
        response = self.cache.get(cache_key)
        # if it wasn't found and we are looking for a HEAD, try looking just
        # for that
        if response is None and request.method == "HEAD":
            cache_key = get_cache_key(
                request, self.key_prefix, "HEAD", cache=self.cache
        # Derive the age estimation of the cached response.
        if (max_age_seconds := get_max_age(response)) is not None and (
            expires_timestamp := parse_http_date_safe(response["Expires"])
            now_timestamp = int(time.time())
            remaining_seconds = expires_timestamp - now_timestamp
            # Use Age: 0 if local clock got turned back.
            response["Age"] = max(0, max_age_seconds - remaining_seconds)
        # hit, return cached response
class CacheMiddleware(UpdateCacheMiddleware, FetchFromCacheMiddleware):
    Cache middleware that provides basic behavior for many simple sites.
    Also used as the hook point for the cache decorator, which is generated
    using the decorator-from-middleware utility.
    def __init__(self, get_response, cache_timeout=None, page_timeout=None, **kwargs):
        # We need to differentiate between "provided, but using default value",
        # and "not provided". If the value is provided using a default, then
        # we fall back to system defaults. If it is not provided at all,
        # we need to use middleware defaults.
            key_prefix = kwargs["key_prefix"]
                key_prefix = ""
            self.key_prefix = key_prefix
            cache_alias = kwargs["cache_alias"]
            if cache_alias is None:
                cache_alias = DEFAULT_CACHE_ALIAS
            self.cache_alias = cache_alias
        if cache_timeout is not None:
            self.cache_timeout = cache_timeout
        self.page_timeout = page_timeout
from django.core.cache import InvalidCacheBackendError, caches
from django.core.cache.utils import make_template_fragment_key
from django.template import Library, Node, TemplateSyntaxError, VariableDoesNotExist
class CacheNode(Node):
    def __init__(self, nodelist, expire_time_var, fragment_name, vary_on, cache_name):
        self.expire_time_var = expire_time_var
        self.fragment_name = fragment_name
        self.vary_on = vary_on
        self.cache_name = cache_name
            expire_time = self.expire_time_var.resolve(context)
                '"cache" tag got an unknown variable: %r' % self.expire_time_var.var
        if expire_time is not None:
                expire_time = int(expire_time)
                    '"cache" tag got a non-integer timeout value: %r' % expire_time
        if self.cache_name:
                cache_name = self.cache_name.resolve(context)
                    '"cache" tag got an unknown variable: %r' % self.cache_name.var
                fragment_cache = caches[cache_name]
            except InvalidCacheBackendError:
                    "Invalid cache name specified for cache tag: %r" % cache_name
                fragment_cache = caches["template_fragments"]
                fragment_cache = caches["default"]
        vary_on = [var.resolve(context) for var in self.vary_on]
        cache_key = make_template_fragment_key(self.fragment_name, vary_on)
        value = fragment_cache.get(cache_key)
            value = self.nodelist.render(context)
            fragment_cache.set(cache_key, value, expire_time)
@register.tag("cache")
def do_cache(parser, token):
    This will cache the contents of a template fragment for a given amount
    of time.
        {% load cache %}
        {% cache [expire_time] [fragment_name] %}
            .. some expensive processing ..
        {% endcache %}
    This tag also supports varying by a list of arguments::
        {% cache [expire_time] [fragment_name] [var1] [var2] .. %}
    Optionally the cache to use may be specified thus::
        {% cache .... using="cachename" %}
    Each unique set of arguments will result in a unique cache entry.
    nodelist = parser.parse(("endcache",))
    parser.delete_first_token()
    tokens = token.split_contents()
    if len(tokens) < 3:
        raise TemplateSyntaxError("'%r' tag requires at least 2 arguments." % tokens[0])
    if len(tokens) > 3 and tokens[-1].startswith("using="):
        cache_name = parser.compile_filter(tokens[-1].removeprefix("using="))
        tokens = tokens[:-1]
        cache_name = None
    return CacheNode(
        nodelist,
        parser.compile_filter(tokens[1]),
        tokens[2],  # fragment_name can't be a variable.
        [parser.compile_filter(t) for t in tokens[3:]],
This module contains helper functions for controlling caching. It does so by
managing the "Vary" header of responses. It includes functions to patch the
header of response objects directly and decorators that change functions to do
that header-patching themselves.
For information on the Vary header, see RFC 9110 Section 12.5.5.
Essentially, the "Vary" HTTP header defines which headers a cache should take
into account when building its cache key. Requests with the same path but
different header content for headers named in "Vary" need to get different
cache keys to prevent delivery of wrong content.
An example: i18n middleware would need to distinguish caches by the
"Accept-language" header.
from django.http import HttpResponse, HttpResponseNotModified
from django.utils.http import http_date, parse_etags, parse_http_date_safe, quote_etag
from django.utils.timezone import get_current_timezone_name
from django.utils.translation import get_language
cc_delim_re = _lazy_re_compile(r"\s*,\s*")
def patch_cache_control(response, **kwargs):
    Patch the Cache-Control header by adding all keyword arguments to it.
    The transformation is as follows:
    * All keyword parameter names are turned to lowercase, and underscores
      are converted to hyphens.
    * If the value of a parameter is True (exactly True, not just a
      true value), only the parameter name is added to the header.
    * All other parameters are added with their value, after applying
      str() to it.
    def dictitem(s):
        t = s.split("=", 1)
        if len(t) > 1:
            return (t[0].lower(), t[1])
            return (t[0].lower(), True)
    def dictvalue(*t):
        if t[1] is True:
            return t[0]
            return "%s=%s" % (t[0], t[1])
    cc = defaultdict(set)
    if response.get("Cache-Control"):
        for field in cc_delim_re.split(response.headers["Cache-Control"]):
            directive, value = dictitem(field)
            if directive == "no-cache":
                # no-cache supports multiple field names.
                cc[directive].add(value)
                cc[directive] = value
    # If there's already a max-age header but we're being asked to set a new
    # max-age, use the minimum of the two ages. In practice this happens when
    # a decorator and a piece of middleware both operate on a given view.
    if "max-age" in cc and "max_age" in kwargs:
        kwargs["max_age"] = min(int(cc["max-age"]), kwargs["max_age"])
    # Allow overriding private caching and vice versa
    if "private" in cc and "public" in kwargs:
        del cc["private"]
    elif "public" in cc and "private" in kwargs:
        del cc["public"]
    for k, v in kwargs.items():
        directive = k.replace("_", "-")
            cc[directive].add(v)
            cc[directive] = v
    directives = []
    for directive, values in cc.items():
        if isinstance(values, set):
            if True in values:
                # True takes precedence.
                values = {True}
            directives.extend([dictvalue(directive, value) for value in values])
            directives.append(dictvalue(directive, values))
    cc = ", ".join(directives)
    response.headers["Cache-Control"] = cc
def get_max_age(response):
    Return the max-age from the response Cache-Control header as an integer,
    or None if it wasn't found or wasn't an integer.
    if not response.has_header("Cache-Control"):
    cc = dict(
        _to_tuple(el) for el in cc_delim_re.split(response.headers["Cache-Control"])
        return int(cc["max-age"])
    except (ValueError, TypeError, KeyError):
def set_response_etag(response):
    if not response.streaming and response.content:
        response.headers["ETag"] = quote_etag(
            md5(response.content, usedforsecurity=False).hexdigest(),
def _precondition_failed(request):
    response = HttpResponse(status=412)
        "Precondition Failed: %s",
def _not_modified(request, response=None):
    new_response = HttpResponseNotModified()
        # Preserve the headers required by RFC 9110 Section 15.4.5, as well as
        # Last-Modified.
        for header in (
            "Cache-Control",
            "Content-Location",
            "Date",
            "ETag",
            "Expires",
            "Last-Modified",
            "Vary",
            if header in response:
                new_response.headers[header] = response.headers[header]
        # Preserve cookies as per the cookie specification: "If a proxy server
        # receives a response which contains a Set-cookie header, it should
        # propagate the Set-cookie header to the client, regardless of whether
        # the response was 304 (Not Modified) or 200 (OK).
        # https://curl.haxx.se/rfc/cookie_spec.html
        new_response.cookies = response.cookies
    return new_response
def get_conditional_response(request, etag=None, last_modified=None, response=None):
    # Only return conditional responses on successful requests.
    if response and not (200 <= response.status_code < 300):
    # Get HTTP request headers.
    if_match_etags = parse_etags(request.META.get("HTTP_IF_MATCH", ""))
    if_unmodified_since = request.META.get("HTTP_IF_UNMODIFIED_SINCE")
    if_unmodified_since = if_unmodified_since and parse_http_date_safe(
        if_unmodified_since
    if_none_match_etags = parse_etags(request.META.get("HTTP_IF_NONE_MATCH", ""))
    if_modified_since = request.META.get("HTTP_IF_MODIFIED_SINCE")
    if_modified_since = if_modified_since and parse_http_date_safe(if_modified_since)
    # Evaluation of request preconditions below follows RFC 9110 Section
    # 13.2.2.
    # Step 1: Test the If-Match precondition.
    if if_match_etags and not _if_match_passes(etag, if_match_etags):
        return _precondition_failed(request)
    # Step 2: Test the If-Unmodified-Since precondition.
        not if_match_etags
        and if_unmodified_since
        and not _if_unmodified_since_passes(last_modified, if_unmodified_since)
    # Step 3: Test the If-None-Match precondition.
    if if_none_match_etags and not _if_none_match_passes(etag, if_none_match_etags):
        if request.method in ("GET", "HEAD"):
            return _not_modified(request, response)
    # Step 4: Test the If-Modified-Since precondition.
        not if_none_match_etags
        and if_modified_since
        and not _if_modified_since_passes(last_modified, if_modified_since)
        and request.method in ("GET", "HEAD")
    # Step 5: Test the If-Range precondition (not supported).
    # Step 6: Return original response since there isn't a conditional
def _if_match_passes(target_etag, etags):
    Test the If-Match comparison as defined in RFC 9110 Section 13.1.1.
    if not target_etag:
        # If there isn't an ETag, then there can't be a match.
    elif etags == ["*"]:
        # The existence of an ETag means that there is "a current
        # representation for the target resource", even if the ETag is weak,
        # so there is a match to '*'.
    elif target_etag.startswith("W/"):
        # A weak ETag can never strongly match another ETag.
        # Since the ETag is strong, this will only return True if there's a
        # strong match.
        return target_etag in etags
def _if_unmodified_since_passes(last_modified, if_unmodified_since):
    Test the If-Unmodified-Since comparison as defined in RFC 9110 Section
    13.1.4.
    return last_modified and last_modified <= if_unmodified_since
def _if_none_match_passes(target_etag, etags):
    Test the If-None-Match comparison as defined in RFC 9110 Section 13.1.2.
        # If there isn't an ETag, then there isn't a match.
        # representation for the target resource", so there is a match to '*'.
        # The comparison should be weak, so look for a match after stripping
        # off any weak indicators.
        target_etag = target_etag.strip("W/")
        etags = (etag.strip("W/") for etag in etags)
        return target_etag not in etags
def _if_modified_since_passes(last_modified, if_modified_since):
    Test the If-Modified-Since comparison as defined in RFC 9110 Section
    13.1.3.
    return not last_modified or last_modified > if_modified_since
def patch_response_headers(response, cache_timeout=None):
    Add HTTP caching headers to the given HttpResponse: Expires and
    Cache-Control.
    Each header is only added if it isn't already set.
    cache_timeout is in seconds. The CACHE_MIDDLEWARE_SECONDS setting is used
    by default.
    if cache_timeout is None:
        cache_timeout = settings.CACHE_MIDDLEWARE_SECONDS
    if cache_timeout < 0:
        cache_timeout = 0  # Can't have max-age negative
    if not response.has_header("Expires"):
        response.headers["Expires"] = http_date(time.time() + cache_timeout)
    patch_cache_control(response, max_age=cache_timeout)
def add_never_cache_headers(response):
    Add headers to a response to indicate that a page should never be cached.
    patch_response_headers(response, cache_timeout=-1)
    patch_cache_control(
        response, no_cache=True, no_store=True, must_revalidate=True, private=True
def patch_vary_headers(response, newheaders):
    Add (or update) the "Vary" header in the given HttpResponse object.
    newheaders is a list of header names that should be in "Vary". If headers
    contains an asterisk, then "Vary" header will consist of a single asterisk
    '*'. Otherwise, existing headers in "Vary" aren't removed.
    # Note that we need to keep the original order intact, because cache
    # implementations may rely on the order of the Vary contents in, say,
    # computing an MD5 hash.
    if response.has_header("Vary"):
        vary_headers = cc_delim_re.split(response.headers["Vary"])
        vary_headers = []
    # Use .lower() here so we treat headers as case-insensitive.
    existing_headers = {header.lower() for header in vary_headers}
    additional_headers = [
        newheader
        for newheader in newheaders
        if newheader.lower() not in existing_headers
    vary_headers += additional_headers
    if "*" in vary_headers:
        response.headers["Vary"] = "*"
        response.headers["Vary"] = ", ".join(vary_headers)
def has_vary_header(response, header_query):
    Check to see if the response has a given header name in its Vary header.
    if not response.has_header("Vary"):
    return header_query.lower() in existing_headers
def _i18n_cache_key_suffix(request, cache_key):
    """If necessary, add the current locale or time zone to the cache key."""
    if settings.USE_I18N:
        # first check if LocaleMiddleware or another middleware added
        # LANGUAGE_CODE to request, then fall back to the active language
        # which in turn can also fall back to settings.LANGUAGE_CODE
        cache_key += ".%s" % getattr(request, "LANGUAGE_CODE", get_language())
        cache_key += ".%s" % get_current_timezone_name()
    return cache_key
def _generate_cache_key(request, method, headerlist, key_prefix):
    """Return a cache key from the headers given in the header list."""
    ctx = md5(usedforsecurity=False)
    for header in headerlist:
        value = request.META.get(header)
            ctx.update(value.encode())
    url = md5(request.build_absolute_uri().encode("ascii"), usedforsecurity=False)
    cache_key = "views.decorators.cache.cache_page.%s.%s.%s.%s" % (
        key_prefix,
        url.hexdigest(),
        ctx.hexdigest(),
    return _i18n_cache_key_suffix(request, cache_key)
def _generate_cache_header_key(key_prefix, request):
    """Return a cache key for the header cache."""
    cache_key = "views.decorators.cache.cache_header.%s.%s" % (
def get_cache_key(request, key_prefix=None, method="GET", cache=None):
    Return a cache key based on the request URL and query. It can be used
    in the request phase because it pulls the list of headers to take into
    account from the global URL registry and uses those to build a cache key
    to check against.
    If there isn't a headerlist stored, return None, indicating that the page
    needs to be rebuilt.
        key_prefix = settings.CACHE_MIDDLEWARE_KEY_PREFIX
    cache_key = _generate_cache_header_key(key_prefix, request)
        cache = caches[settings.CACHE_MIDDLEWARE_ALIAS]
    headerlist = cache.get(cache_key)
    if headerlist is not None:
        return _generate_cache_key(request, method, headerlist, key_prefix)
def learn_cache_key(request, response, cache_timeout=None, key_prefix=None, cache=None):
    Learn what headers to take into account for some request URL from the
    response object. Store those headers in a global URL registry so that
    later access to that URL will know what headers to take into account
    without building the response object itself. The headers are named in the
    Vary header of the response, but we want to prevent response generation.
    The list of headers to use for cache key generation is stored in the same
    cache as the pages themselves. If the cache ages some data out of the
    cache, this just means that we have to build the response once to get at
    the Vary header and so at the list of headers to use for the cache key.
        is_accept_language_redundant = settings.USE_I18N
        # If i18n is used, the generated cache key will be suffixed with the
        # current locale. Adding the raw value of Accept-Language is redundant
        # in that case and would result in storing the same content under
        # multiple keys in the cache. See #18191 for details.
        headerlist = []
        for header in cc_delim_re.split(response.headers["Vary"]):
            header = header.upper().replace("-", "_")
            if header != "ACCEPT_LANGUAGE" or not is_accept_language_redundant:
                headerlist.append("HTTP_" + header)
        headerlist.sort()
        cache.set(cache_key, headerlist, cache_timeout)
        return _generate_cache_key(request, request.method, headerlist, key_prefix)
        # if there is no Vary header, we still need a cache key
        # for the request.build_absolute_uri()
        cache.set(cache_key, [], cache_timeout)
        return _generate_cache_key(request, request.method, [], key_prefix)
def _to_tuple(s):
    if len(t) == 2:
        return t[0].lower(), t[1]
    return t[0].lower(), True
from django.middleware.cache import CacheMiddleware
from django.utils.cache import add_never_cache_headers, patch_cache_control
from django.utils.decorators import decorator_from_middleware_with_args
def cache_page(timeout, *, cache=None, key_prefix=None):
    Decorator for views that tries getting the page from the cache and
    populates the cache if the page isn't in the cache yet.
    The cache is keyed by the URL and some data from the headers.
    Additionally there is the key prefix that is used to distinguish different
    cache areas in a multi-site setup. You could use the
    get_current_site().domain, for example, as that is unique across a Django
    Additionally, all headers from the response's Vary header will be taken
    into account on caching -- just like the middleware does.
    return decorator_from_middleware_with_args(CacheMiddleware)(
        page_timeout=timeout,
        cache_alias=cache,
        key_prefix=key_prefix,
def _check_request(request, decorator_name):
    # Ensure argument looks like a request.
            f"{decorator_name} didn't receive an HttpRequest. If you are "
            "decorating a classmethod, be sure to use @method_decorator."
def cache_control(**kwargs):
    def _cache_controller(viewfunc):
        if iscoroutinefunction(viewfunc):
            async def _view_wrapper(request, *args, **kw):
                _check_request(request, "cache_control")
                response = await viewfunc(request, *args, **kw)
                patch_cache_control(response, **kwargs)
            def _view_wrapper(request, *args, **kw):
                response = viewfunc(request, *args, **kw)
        return wraps(viewfunc)(_view_wrapper)
    return _cache_controller
def never_cache(view_func):
    Decorator that adds headers to a response so that it will never be cached.
    if iscoroutinefunction(view_func):
        async def _view_wrapper(request, *args, **kwargs):
            _check_request(request, "never_cache")
            response = await view_func(request, *args, **kwargs)
            add_never_cache_headers(response)
        def _view_wrapper(request, *args, **kwargs):
            response = view_func(request, *args, **kwargs)
    return wraps(view_func)(_view_wrapper)
from langchain_classic._api import create_importer
    from langchain_community.cache import (
        AstraDBCache,
        AstraDBSemanticCache,
        AzureCosmosDBSemanticCache,
        CassandraCache,
        CassandraSemanticCache,
        FullLLMCache,
        FullMd5LLMCache,
        GPTCache,
        InMemoryCache,
        MomentoCache,
        RedisCache,
        RedisSemanticCache,
        SQLAlchemyCache,
        SQLAlchemyMd5Cache,
        SQLiteCache,
        UpstashRedisCache,
# Create a way to dynamically look up deprecated imports.
# Used to consolidate logic for raising deprecation warnings and
# handling optional imports.
DEPRECATED_LOOKUP = {
    "FullLLMCache": "langchain_community.cache",
    "SQLAlchemyCache": "langchain_community.cache",
    "SQLiteCache": "langchain_community.cache",
    "UpstashRedisCache": "langchain_community.cache",
    "RedisCache": "langchain_community.cache",
    "RedisSemanticCache": "langchain_community.cache",
    "GPTCache": "langchain_community.cache",
    "MomentoCache": "langchain_community.cache",
    "InMemoryCache": "langchain_community.cache",
    "CassandraCache": "langchain_community.cache",
    "CassandraSemanticCache": "langchain_community.cache",
    "FullMd5LLMCache": "langchain_community.cache",
    "SQLAlchemyMd5Cache": "langchain_community.cache",
    "AstraDBCache": "langchain_community.cache",
    "AstraDBSemanticCache": "langchain_community.cache",
    "AzureCosmosDBSemanticCache": "langchain_community.cache",
_import_attribute = create_importer(__package__, deprecated_lookups=DEPRECATED_LOOKUP)
    """Look up attributes dynamically."""
    return _import_attribute(name)
    "AstraDBCache",
    "AstraDBSemanticCache",
    "AzureCosmosDBSemanticCache",
    "CassandraCache",
    "CassandraSemanticCache",
    "FullLLMCache",
    "FullMd5LLMCache",
    "GPTCache",
    "InMemoryCache",
    "MomentoCache",
    "RedisCache",
    "RedisSemanticCache",
    "SQLAlchemyCache",
    "SQLAlchemyMd5Cache",
    "SQLiteCache",
    "UpstashRedisCache",
"""Module contains code for a cache backed embedder.
The cache backed embedder is a wrapper around an embedder that caches
embeddings in a key-value store. The cache is used to avoid recomputing
embeddings for the same text.
The text is hashed and the hash is used as the key in the cache.
from typing import Literal, cast
from langchain_core.stores import BaseStore, ByteStore
from langchain_classic.storage.encoder_backed import EncoderBackedStore
NAMESPACE_UUID = uuid.UUID(int=1985)
def _sha1_hash_to_uuid(text: str) -> uuid.UUID:
    """Return a UUID derived from *text* using SHA-1 (deterministic).
    Deterministic and fast, **but not collision-resistant**.
    A malicious attacker could try to create two different texts that hash to the same
    UUID. This may not necessarily be an issue in the context of caching embeddings,
    but new applications should swap this out for a stronger hash function like
    xxHash, BLAKE2 or SHA-256, which are collision-resistant.
    sha1_hex = hashlib.sha1(text.encode("utf-8"), usedforsecurity=False).hexdigest()
    # Embed the hex string in `uuid5` to obtain a valid UUID.
    return uuid.uuid5(NAMESPACE_UUID, sha1_hex)
def _make_default_key_encoder(namespace: str, algorithm: str) -> Callable[[str], str]:
    """Create a default key encoder function.
        namespace: Prefix that segregates keys from different embedding models.
        algorithm:
           * `'sha1'` - fast but not collision-resistant
           * `'blake2b'` - cryptographically strong, faster than SHA-1
           * `'sha256'` - cryptographically strong, slower than SHA-1
           * `'sha512'` - cryptographically strong, slower than SHA-1
        A function that encodes a key using the specified algorithm.
        _warn_about_sha1_encoder()
    def _key_encoder(key: str) -> str:
        """Encode a key using the specified algorithm."""
            return f"{namespace}{_sha1_hash_to_uuid(key)}"
            return f"{namespace}{hashlib.blake2b(key.encode('utf-8')).hexdigest()}"
            return f"{namespace}{hashlib.sha256(key.encode('utf-8')).hexdigest()}"
            return f"{namespace}{hashlib.sha512(key.encode('utf-8')).hexdigest()}"
        msg = f"Unsupported algorithm: {algorithm}"
    return _key_encoder
def _value_serializer(value: Sequence[float]) -> bytes:
    """Serialize a value."""
    return json.dumps(value).encode()
def _value_deserializer(serialized_value: bytes) -> list[float]:
    """Deserialize a value."""
    return cast("list[float]", json.loads(serialized_value.decode()))
# The warning is global; track emission, so it appears only once.
_warned_about_sha1: bool = False
def _warn_about_sha1_encoder() -> None:
    global _warned_about_sha1  # noqa: PLW0603
    if not _warned_about_sha1:
            "Using default key encoder: SHA-1 is *not* collision-resistant. "
            "While acceptable for most cache scenarios, a motivated attacker "
            "can craft two different payloads that map to the same cache key. "
            "If that risk matters in your environment, supply a stronger "
            "encoder (e.g. SHA-256 or BLAKE2) via the `key_encoder` argument. "
            "If you change the key encoder, consider also creating a new cache, "
            "to avoid (the potential for) collisions with existing keys.",
        _warned_about_sha1 = True
class CacheBackedEmbeddings(Embeddings):
    """Interface for caching results from embedding models.
    The interface allows works with any store that implements
    the abstract store interface accepting keys of type str and values of list of
    floats.
    If need be, the interface can be extended to accept other implementations
    of the value serializer and deserializer, as well as the key encoder.
    Note that by default only document embeddings are cached. To cache query
    embeddings too, pass in a query_embedding_store to constructor.
        from langchain_classic.embeddings import CacheBackedEmbeddings
        from langchain_classic.storage import LocalFileStore
        store = LocalFileStore("./my_cache")
        underlying_embedder = OpenAIEmbeddings()
        embedder = CacheBackedEmbeddings.from_bytes_store(
            underlying_embedder, store, namespace=underlying_embedder.model
        # Embedding is computed and cached
        embeddings = embedder.embed_documents(["hello", "goodbye"])
        # Embeddings are retrieved from the cache, no computation is done
        underlying_embeddings: Embeddings,
        document_embedding_store: BaseStore[str, list[float]],
        batch_size: int | None = None,
        query_embedding_store: BaseStore[str, list[float]] | None = None,
        """Initialize the embedder.
            underlying_embeddings: the embedder to use for computing embeddings.
            document_embedding_store: The store to use for caching document embeddings.
            batch_size: The number of documents to embed between store updates.
            query_embedding_store: The store to use for caching query embeddings.
                If `None`, query embeddings are not cached.
        self.document_embedding_store = document_embedding_store
        self.query_embedding_store = query_embedding_store
        self.underlying_embeddings = underlying_embeddings
        self.batch_size = batch_size
        """Embed a list of texts.
        The method first checks the cache for the embeddings.
        If the embeddings are not found, the method uses the underlying embedder
        to embed the documents and stores the results in the cache.
            texts: A list of texts to embed.
            A list of embeddings for the given texts.
        vectors: list[list[float] | None] = self.document_embedding_store.mget(
            texts,
        all_missing_indices: list[int] = [
            i for i, vector in enumerate(vectors) if vector is None
        for missing_indices in batch_iterate(self.batch_size, all_missing_indices):
            missing_texts = [texts[i] for i in missing_indices]
            missing_vectors = self.underlying_embeddings.embed_documents(missing_texts)
            self.document_embedding_store.mset(
                list(zip(missing_texts, missing_vectors, strict=False)),
            for index, updated_vector in zip(
                missing_indices, missing_vectors, strict=False
                vectors[index] = updated_vector
            "list[list[float]]",
            vectors,
        )  # Nones should have been resolved by now
        vectors: list[list[float] | None] = await self.document_embedding_store.amget(
            texts
        # batch_iterate supports None batch_size which returns all elements at once
        # as a single batch.
            missing_vectors = await self.underlying_embeddings.aembed_documents(
                missing_texts,
            await self.document_embedding_store.amset(
        By default, this method does not cache queries. To enable caching, set the
        `cache_query` parameter to `True` when initializing the embedder.
            text: The text to embed.
            The embedding for the given text.
        if not self.query_embedding_store:
            return self.underlying_embeddings.embed_query(text)
        (cached,) = self.query_embedding_store.mget([text])
        vector = self.underlying_embeddings.embed_query(text)
        self.query_embedding_store.mset([(text, vector)])
        return vector
            return await self.underlying_embeddings.aembed_query(text)
        (cached,) = await self.query_embedding_store.amget([text])
        vector = await self.underlying_embeddings.aembed_query(text)
        await self.query_embedding_store.amset([(text, vector)])
    def from_bytes_store(
        document_embedding_cache: ByteStore,
        namespace: str = "",
        query_embedding_cache: bool | ByteStore = False,
        key_encoder: Callable[[str], str]
        | Literal["sha1", "blake2b", "sha256", "sha512"] = "sha1",
    ) -> CacheBackedEmbeddings:
        """On-ramp that adds the necessary serialization and encoding to the store.
            underlying_embeddings: The embedder to use for embedding.
            document_embedding_cache: The cache to use for storing document embeddings.
            namespace: The namespace to use for document cache.
                This namespace is used to avoid collisions with other caches.
                For example, set it to the name of the embedding model used.
            query_embedding_cache: The cache to use for storing query embeddings.
                True to use the same cache as document embeddings.
                False to not cache query embeddings.
            key_encoder: Optional callable to encode keys. If not provided,
                a default encoder using SHA-1 will be used. SHA-1 is not
                collision-resistant, and a motivated attacker could craft two
                different texts that hash to the same cache key.
                If you change a key encoder in an existing cache, consider
                just creating a new cache, to avoid (the potential for)
                collisions with existing keys or having duplicate keys
                for the same text in the cache.
            An instance of CacheBackedEmbeddings that uses the provided cache.
        if isinstance(key_encoder, str):
            key_encoder = _make_default_key_encoder(namespace, key_encoder)
        elif callable(key_encoder):
            # If a custom key encoder is provided, it should not be used with a
            # A user can handle namespacing in directly their custom key encoder.
                    "Do not supply `namespace` when using a custom key_encoder; "
                    "add any prefixing inside the encoder itself."
                "key_encoder must be either 'blake2b', 'sha1', 'sha256', 'sha512' "
                "or a callable that encodes keys."
        document_embedding_store = EncoderBackedStore[str, list[float]](
            document_embedding_cache,
            key_encoder,
            _value_serializer,
            _value_deserializer,
        if query_embedding_cache is True:
            query_embedding_store = document_embedding_store
        elif query_embedding_cache is False:
            query_embedding_store = None
            query_embedding_store = EncoderBackedStore[str, list[float]](
                query_embedding_cache,
            underlying_embeddings,
            document_embedding_store,
            query_embedding_store=query_embedding_store,
from langchain_core.caches import RETURN_VAL_TYPE, BaseCache
__all__ = ["RETURN_VAL_TYPE", "BaseCache"]
"""Standard tests for the `BaseCache` abstraction.
We don't recommend implementing externally managed `BaseCache` abstractions at this
from langchain_core.outputs import Generation
from langchain_tests.base import BaseStandardTests
class SyncCacheTestSuite(BaseStandardTests):
    """Test suite for checking the `BaseCache` API of a caching layer for LLMs.
    This test suite verifies the basic caching API of a caching layer for LLMs.
    The test suite is designed for synchronous caching layers.
    Implementers should subclass this test suite and provide a fixture
    that returns an empty cache for each test.
    def cache(self) -> BaseCache:
        """Get the cache class to test.
        The returned cache should be EMPTY.
    def get_sample_prompt(self) -> str:
        """Return a sample prompt for testing."""
        return "Sample prompt for testing."
    def get_sample_llm_string(self) -> str:
        """Return a sample LLM string for testing."""
        return "Sample LLM string configuration."
    def get_sample_generation(self) -> Generation:
        """Return a sample `Generation` object for testing."""
        return Generation(
            text="Sample generated text.",
            generation_info={"reason": "test"},
    def test_cache_is_empty(self, cache: BaseCache) -> None:
        """Test that the cache is empty."""
            cache.lookup(self.get_sample_prompt(), self.get_sample_llm_string()) is None
    def test_update_cache(self, cache: BaseCache) -> None:
        """Test updating the cache."""
        prompt = self.get_sample_prompt()
        llm_string = self.get_sample_llm_string()
        generation = self.get_sample_generation()
        cache.update(prompt, llm_string, [generation])
        assert cache.lookup(prompt, llm_string) == [generation]
    def test_cache_still_empty(self, cache: BaseCache) -> None:
        """Test that the cache is still empty.
        This test should follow a test that updates the cache.
        This just verifies that the fixture is set up properly to be empty after each
        test.
    def test_clear_cache(self, cache: BaseCache) -> None:
        """Test clearing the cache."""
        assert cache.lookup(prompt, llm_string) is None
    def test_cache_miss(self, cache: BaseCache) -> None:
        """Test cache miss."""
        assert cache.lookup("Nonexistent prompt", self.get_sample_llm_string()) is None
    def test_cache_hit(self, cache: BaseCache) -> None:
        """Test cache hit."""
    def test_update_cache_with_multiple_generations(self, cache: BaseCache) -> None:
        """Test updating the cache with multiple `Generation` objects."""
        generations = [
            self.get_sample_generation(),
            Generation(text="Another generated text."),
        cache.update(prompt, llm_string, generations)
        assert cache.lookup(prompt, llm_string) == generations
class AsyncCacheTestSuite(BaseStandardTests):
    Verifies the basic caching API of a caching layer for LLMs.
    Implementers should subclass this test suite and provide a fixture that returns an
    empty cache for each test.
    async def cache(self) -> BaseCache:
    async def test_cache_is_empty(self, cache: BaseCache) -> None:
            await cache.alookup(self.get_sample_prompt(), self.get_sample_llm_string())
            is None
    async def test_update_cache(self, cache: BaseCache) -> None:
        await cache.aupdate(prompt, llm_string, [generation])
        assert await cache.alookup(prompt, llm_string) == [generation]
    async def test_cache_still_empty(self, cache: BaseCache) -> None:
    async def test_clear_cache(self, cache: BaseCache) -> None:
        await cache.aclear()
        assert await cache.alookup(prompt, llm_string) is None
    async def test_cache_miss(self, cache: BaseCache) -> None:
            await cache.alookup("Nonexistent prompt", self.get_sample_llm_string())
    async def test_cache_hit(self, cache: BaseCache) -> None:
    async def test_update_cache_with_multiple_generations(
        cache: BaseCache,
        await cache.aupdate(prompt, llm_string, generations)
        assert await cache.alookup(prompt, llm_string) == generations
"""Cache Management"""
from pip._vendor.packaging.tags import Tag, interpreter_name, interpreter_version
from pip._vendor.packaging.utils import canonicalize_name
from pip._internal.exceptions import InvalidWheelFilename
from pip._internal.models.direct_url import DirectUrl
from pip._internal.models.link import Link
from pip._internal.models.wheel import Wheel
from pip._internal.utils.temp_dir import TempDirectory, tempdir_kinds
from pip._internal.utils.urls import path_to_url
ORIGIN_JSON_NAME = "origin.json"
def _hash_dict(d: dict[str, str]) -> str:
    """Return a stable sha224 of a dictionary."""
    s = json.dumps(d, sort_keys=True, separators=(",", ":"), ensure_ascii=True)
    return hashlib.sha224(s.encode("ascii")).hexdigest()
    """An abstract class - provides cache directories for data from links
    :param cache_dir: The root of the cache.
    def __init__(self, cache_dir: str) -> None:
        assert not cache_dir or os.path.isabs(cache_dir)
        self.cache_dir = cache_dir or None
    def _get_cache_path_parts(self, link: Link) -> list[str]:
        """Get parts of part that must be os.path.joined with cache_dir"""
        # We want to generate an url to use as our cache key, we don't want to
        # just reuse the URL because it might have other items in the fragment
        # and we don't care about those.
        key_parts = {"url": link.url_without_fragment}
        if link.hash_name is not None and link.hash is not None:
            key_parts[link.hash_name] = link.hash
        if link.subdirectory_fragment:
            key_parts["subdirectory"] = link.subdirectory_fragment
        # Include interpreter name, major and minor version in cache key
        # to cope with ill-behaved sdists that build a different wheel
        # depending on the python version their setup.py is being run on,
        # and don't encode the difference in compatibility tags.
        # https://github.com/pypa/pip/issues/7296
        key_parts["interpreter_name"] = interpreter_name()
        key_parts["interpreter_version"] = interpreter_version()
        # Encode our key url with sha224, we'll use this because it has similar
        # security properties to sha256, but with a shorter total output (and
        # thus less secure). However the differences don't make a lot of
        # difference for our use case here.
        hashed = _hash_dict(key_parts)
        # We want to nest the directories some to prevent having a ton of top
        # level directories where we might run out of sub directories on some
        # FS.
        parts = [hashed[:2], hashed[2:4], hashed[4:6], hashed[6:]]
        return parts
    def _get_candidates(self, link: Link, canonical_package_name: str) -> list[Any]:
        can_not_cache = not self.cache_dir or not canonical_package_name or not link
        if can_not_cache:
        path = self.get_path_for_link(link)
            return [(candidate, path) for candidate in os.listdir(path)]
    def get_path_for_link(self, link: Link) -> str:
        """Return a directory to store cached items in for link."""
        link: Link,
        package_name: str | None,
        supported_tags: list[Tag],
    ) -> Link:
        """Returns a link to a cached item if it exists, otherwise returns the
        passed link.
class SimpleWheelCache(Cache):
    """A cache of wheels for future installs."""
        super().__init__(cache_dir)
        """Return a directory to store cached wheels for link
        Because there are M wheels for any one sdist, we provide a directory
        to cache them in, and then consult that directory when looking up
        cache hits.
        We only insert things into the cache if they have plausible version
        numbers, so that we don't contaminate the cache with things that were
        not unique. E.g. ./package might have dozens of installs done for it
        and build a version of 0.0...and if we built and cached a wheel, we'd
        end up using the same wheel even if the source has been edited.
        :param link: The link of the sdist for which this will cache wheels.
        parts = self._get_cache_path_parts(link)
        assert self.cache_dir
        # Store wheels within the root cache_dir
        return os.path.join(self.cache_dir, "wheels", *parts)
        candidates = []
        if not package_name:
            return link
        canonical_package_name = canonicalize_name(package_name)
        for wheel_name, wheel_dir in self._get_candidates(link, canonical_package_name):
                wheel = Wheel(wheel_name)
            except InvalidWheelFilename:
            if wheel.name != canonical_package_name:
                    "Ignoring cached wheel %s for %s as it "
                    "does not match the expected distribution name %s.",
                    wheel_name,
                    package_name,
            if not wheel.supported(supported_tags):
                # Built for a different python/arch/etc
            candidates.append(
                    wheel.support_index_min(supported_tags),
                    wheel_dir,
        if not candidates:
        _, wheel_name, wheel_dir = min(candidates)
        return Link(path_to_url(os.path.join(wheel_dir, wheel_name)))
class EphemWheelCache(SimpleWheelCache):
    """A SimpleWheelCache that creates it's own temporary cache directory"""
        self._temp_dir = TempDirectory(
            kind=tempdir_kinds.EPHEM_WHEEL_CACHE,
            globally_managed=True,
        super().__init__(self._temp_dir.path)
class CacheEntry:
        persistent: bool,
        self.persistent = persistent
        self.origin: DirectUrl | None = None
        origin_direct_url_path = Path(self.link.file_path).parent / ORIGIN_JSON_NAME
        if origin_direct_url_path.exists():
                self.origin = DirectUrl.from_json(
                    origin_direct_url_path.read_text(encoding="utf-8")
                    "Ignoring invalid cache entry origin file %s for %s (%s)",
                    origin_direct_url_path,
                    link.filename,
class WheelCache(Cache):
    """Wraps EphemWheelCache and SimpleWheelCache into a single Cache
    This Cache allows for gracefully degradation, using the ephem wheel cache
    when a certain link is not found in the simple wheel cache first.
        self._wheel_cache = SimpleWheelCache(cache_dir)
        self._ephem_cache = EphemWheelCache()
        return self._wheel_cache.get_path_for_link(link)
    def get_ephem_path_for_link(self, link: Link) -> str:
        return self._ephem_cache.get_path_for_link(link)
        cache_entry = self.get_cache_entry(link, package_name, supported_tags)
        if cache_entry is None:
        return cache_entry.link
    def get_cache_entry(
    ) -> CacheEntry | None:
        """Returns a CacheEntry with a link to a cached item if it exists or
        None. The cache entry indicates if the item was found in the persistent
        or ephemeral cache.
        retval = self._wheel_cache.get(
            link=link,
            package_name=package_name,
            supported_tags=supported_tags,
        if retval is not link:
            return CacheEntry(retval, persistent=True)
        retval = self._ephem_cache.get(
            return CacheEntry(retval, persistent=False)
    def record_download_origin(cache_dir: str, download_info: DirectUrl) -> None:
        origin_path = Path(cache_dir) / ORIGIN_JSON_NAME
        if origin_path.exists():
                origin = DirectUrl.from_json(origin_path.read_text(encoding="utf-8"))
                    "Could not read origin file %s in cache entry (%s). "
                    "Will attempt to overwrite it.",
                    origin_path,
                # TODO: use DirectUrl.equivalent when
                # https://github.com/pypa/pip/pull/10564 is merged.
                if origin.url != download_info.url:
                        "Origin URL %s in cache entry %s does not match download URL "
                        "%s. This is likely a pip bug or a cache corruption issue. "
                        "Will overwrite it with the new value.",
                        origin.url,
                        cache_dir,
                        download_info.url,
        origin_path.write_text(download_info.to_json(), encoding="utf-8")
from optparse import Values
from pip._internal.cli.status_codes import ERROR, SUCCESS
from pip._internal.exceptions import CommandError, PipError
from pip._internal.utils import filesystem
from pip._internal.utils.logging import getLogger
logger = getLogger(__name__)
class CacheCommand(Command):
    Inspect and manage pip's wheel cache.
    Subcommands:
    - dir: Show the cache directory.
    - info: Show information about the cache.
    - list: List filenames of packages stored in the cache.
    - remove: Remove one or more package from the cache.
    - purge: Remove all items from the cache.
    ``<pattern>`` can be a glob expression or a package name.
    ignore_require_venv = True
    usage = """
        %prog dir
        %prog info
        %prog list [<pattern>] [--format=[human, abspath]]
        %prog remove <pattern>
        %prog purge
    def add_options(self) -> None:
        self.cmd_opts.add_option(
            "--format",
            action="store",
            dest="list_format",
            default="human",
            choices=("human", "abspath"),
            help="Select the output format among: human (default) or abspath",
        self.parser.insert_option_group(0, self.cmd_opts)
    def handler_map(self) -> dict[str, Callable[[Values, list[str]], None]]:
            "dir": self.get_cache_dir,
            "info": self.get_cache_info,
            "list": self.list_cache_items,
            "remove": self.remove_cache_items,
            "purge": self.purge_cache,
    def run(self, options: Values, args: list[str]) -> int:
        handler_map = self.handler_map()
        if not options.cache_dir:
            logger.error("pip cache commands can not function since cache is disabled.")
            return ERROR
        # Determine action
        if not args or args[0] not in handler_map:
                "Need an action (%s) to perform.",
                ", ".join(sorted(handler_map)),
        action = args[0]
        # Error handling happens here, not in the action-handlers.
            handler_map[action](options, args[1:])
        except PipError as e:
            logger.error(e.args[0])
        return SUCCESS
    def get_cache_dir(self, options: Values, args: list[str]) -> None:
            raise CommandError("Too many arguments")
        logger.info(options.cache_dir)
    def get_cache_info(self, options: Values, args: list[str]) -> None:
        num_http_files = len(self._find_http_files(options))
        num_packages = len(self._find_wheels(options, "*"))
        http_cache_location = self._cache_dir(options, "http-v2")
        old_http_cache_location = self._cache_dir(options, "http")
        wheels_cache_location = self._cache_dir(options, "wheels")
        http_cache_size = filesystem.format_size(
            filesystem.directory_size(http_cache_location)
            + filesystem.directory_size(old_http_cache_location)
        wheels_cache_size = filesystem.format_directory_size(wheels_cache_location)
            textwrap.dedent(
                    Package index page cache location (pip v23.3+): {http_cache_location}
                    Package index page cache location (older pips): {old_http_cache_location}
                    Package index page cache size: {http_cache_size}
                    Number of HTTP files: {num_http_files}
                    Locally built wheels location: {wheels_cache_location}
                    Locally built wheels size: {wheels_cache_size}
                    Number of locally built wheels: {package_count}
            .format(
                http_cache_location=http_cache_location,
                old_http_cache_location=old_http_cache_location,
                http_cache_size=http_cache_size,
                num_http_files=num_http_files,
                wheels_cache_location=wheels_cache_location,
                package_count=num_packages,
                wheels_cache_size=wheels_cache_size,
            .strip()
        logger.info(message)
    def list_cache_items(self, options: Values, args: list[str]) -> None:
        if len(args) > 1:
            pattern = args[0]
            pattern = "*"
        files = self._find_wheels(options, pattern)
        if options.list_format == "human":
            self.format_for_human(files)
            self.format_for_abspath(files)
    def format_for_human(self, files: list[str]) -> None:
            logger.info("No locally built wheels cached.")
            wheel = os.path.basename(filename)
            size = filesystem.format_file_size(filename)
            results.append(f" - {wheel} ({size})")
        logger.info("Cache contents:\n")
        logger.info("\n".join(sorted(results)))
    def format_for_abspath(self, files: list[str]) -> None:
            logger.info("\n".join(sorted(files)))
    def remove_cache_items(self, options: Values, args: list[str]) -> None:
            raise CommandError("Please provide a pattern")
        files = self._find_wheels(options, args[0])
        no_matching_msg = "No matching packages"
        if args[0] == "*":
            # Only fetch http files if no specific pattern given
            files += self._find_http_files(options)
            # Add the pattern to the log message
            no_matching_msg += f' for pattern "{args[0]}"'
            logger.warning(no_matching_msg)
        bytes_removed = 0
            bytes_removed += os.stat(filename).st_size
            os.unlink(filename)
            logger.verbose("Removed %s", filename)
        http_dirs = filesystem.subdirs_without_files(self._cache_dir(options, "http"))
        wheel_dirs = filesystem.subdirs_without_wheels(
            self._cache_dir(options, "wheels")
        dirs = [*http_dirs, *wheel_dirs]
        for subdir in dirs:
                for file in subdir.iterdir():
                    file.unlink(missing_ok=True)
                subdir.rmdir()
                # If the directory is already gone, that's fine.
            logger.verbose("Removed %s", subdir)
        # selfcheck.json is no longer used by pip.
        selfcheck_json = self._cache_dir(options, "selfcheck.json")
        if os.path.isfile(selfcheck_json):
            os.remove(selfcheck_json)
            logger.verbose("Removed legacy selfcheck.json file")
        logger.info("Files removed: %s (%s)", len(files), format_size(bytes_removed))
        logger.info("Directories removed: %s", len(dirs))
    def purge_cache(self, options: Values, args: list[str]) -> None:
        return self.remove_cache_items(options, ["*"])
    def _cache_dir(self, options: Values, subdir: str) -> str:
        return os.path.join(options.cache_dir, subdir)
    def _find_http_files(self, options: Values) -> list[str]:
        old_http_dir = self._cache_dir(options, "http")
        new_http_dir = self._cache_dir(options, "http-v2")
        return filesystem.find_files(old_http_dir, "*") + filesystem.find_files(
            new_http_dir, "*"
    def _find_wheels(self, options: Values, pattern: str) -> list[str]:
        wheel_dir = self._cache_dir(options, "wheels")
        # The wheel filename format, as specified in PEP 427, is:
        #     {distribution}-{version}(-{build})?-{python}-{abi}-{platform}.whl
        # Additionally, non-alphanumeric values in the distribution are
        # normalized to underscores (_), meaning hyphens can never occur
        # before `-{version}`.
        # Given that information:
        # - If the pattern we're given contains a hyphen (-), the user is
        #   providing at least the version. Thus, we can just append `*.whl`
        #   to match the rest of it.
        # - If the pattern we're given doesn't contain a hyphen (-), the
        #   user is only providing the name. Thus, we append `-*.whl` to
        #   match the hyphen before the version, followed by anything else.
        # PEP 427: https://www.python.org/dev/peps/pep-0427/
        pattern = pattern + ("*.whl" if "-" in pattern else "-*.whl")
        return filesystem.find_files(wheel_dir, pattern)
"""HTTP cache implementation."""
from typing import Any, BinaryIO, Callable
from pip._vendor.cachecontrol.cache import SeparateBodyBaseCache
from pip._vendor.cachecontrol.caches import SeparateBodyFileCache
from pip._internal.utils.filesystem import (
    adjacent_tmp_file,
    copy_directory_permissions,
    replace,
from pip._internal.utils.misc import ensure_dir
def is_from_cache(response: Response) -> bool:
    return getattr(response, "from_cache", False)
def suppressed_cache_errors() -> Generator[None, None, None]:
    """If we can't access the cache then we can just skip caching and process
    requests as if caching wasn't enabled.
class SafeFileCache(SeparateBodyBaseCache):
    A file based cache which is safe to use even when the target directory may
    not be accessible or writable.
    There is a race condition when two processes try to write and/or read the
    same entry at the same time, since each entry consists of two separate
    files (https://github.com/psf/cachecontrol/issues/324).  We therefore have
    additional logic that makes sure that both files to be present before
    returning an entry; this fixes the read side of the race condition.
    For the write side, we assume that the server will only ever return the
    same data for the same URL, which ought to be the case for files pip is
    downloading.  PyPI does not have a mechanism to swap out a wheel for
    another wheel, for example.  If this assumption is not true, the
    CacheControl issue will need to be fixed.
    def __init__(self, directory: str) -> None:
        assert directory is not None, "Cache directory must not be None."
        self.directory = directory
    def _get_cache_path(self, name: str) -> str:
        # From cachecontrol.caches.file_cache.FileCache._fn, brought into our
        # class for backwards-compatibility and to avoid using a non-public
        hashed = SeparateBodyFileCache.encode(name)
        parts = list(hashed[:5]) + [hashed]
        return os.path.join(self.directory, *parts)
    def get(self, key: str) -> bytes | None:
        # The cache entry is only valid if both metadata and body exist.
        metadata_path = self._get_cache_path(key)
        body_path = metadata_path + ".body"
        if not (os.path.exists(metadata_path) and os.path.exists(body_path)):
        with suppressed_cache_errors():
            with open(metadata_path, "rb") as f:
    def _write_to_file(self, path: str, writer_func: Callable[[BinaryIO], Any]) -> None:
        """Common file writing logic with proper permissions and atomic replacement."""
            ensure_dir(os.path.dirname(path))
            with adjacent_tmp_file(path) as f:
                writer_func(f)
                # Inherit the read/write permissions of the cache directory
                # to enable multi-user cache use-cases.
                copy_directory_permissions(self.directory, f)
            replace(f.name, path)
    def _write(self, path: str, data: bytes) -> None:
        self._write_to_file(path, lambda f: f.write(data))
    def _write_from_io(self, path: str, source_file: BinaryIO) -> None:
        self._write_to_file(path, lambda f: shutil.copyfileobj(source_file, f))
    def set(
        self, key: str, value: bytes, expires: int | datetime | None = None
        path = self._get_cache_path(key)
        self._write(path, value)
    def delete(self, key: str) -> None:
            os.remove(path + ".body")
    def get_body(self, key: str) -> BinaryIO | None:
            return open(body_path, "rb")
    def set_body(self, key: str, body: bytes) -> None:
        path = self._get_cache_path(key) + ".body"
        self._write(path, body)
    def set_body_from_io(self, key: str, body_file: BinaryIO) -> None:
        """Set the body of the cache entry from a file object."""
        self._write_from_io(path, body_file)
The cache object API for implementing caches. The default is a thread
safe in-memory dictionary.
from threading import Lock
from typing import IO, TYPE_CHECKING, MutableMapping
class DictCache(BaseCache):
    def __init__(self, init_dict: MutableMapping[str, bytes] | None = None) -> None:
        self.lock = Lock()
        self.data = init_dict or {}
        return self.data.get(key, None)
            self.data.update({key: value})
            if key in self.data:
                self.data.pop(key)
class SeparateBodyBaseCache(BaseCache):
    In this variant, the body is not stored mixed in with the metadata, but is
    passed in (as a bytes-like object) in a separate call to ``set_body()``.
    That is, the expected interaction pattern is::
        cache.set(key, serialized_metadata)
        cache.set_body(key)
    Similarly, the body should be loaded separately via ``get_body()``.
    def get_body(self, key: str) -> IO[bytes] | None:
        Return the body as file-like object.
from typing import Any, Callable, Dict, Generic, Hashable, Tuple, TypeVar, cast
    "SimpleCache",
    "FastDictCache",
    "memoized",
_T = TypeVar("_T", bound=Hashable)
_U = TypeVar("_U")
class SimpleCache(Generic[_T, _U]):
    Very simple cache that discards the oldest item when the cache size is
    exceeded.
    :param maxsize: Maximum size of the cache. (Don't make it too big.)
    def __init__(self, maxsize: int = 8) -> None:
        assert maxsize > 0
        self._data: dict[_T, _U] = {}
        self._keys: deque[_T] = deque()
        self.maxsize: int = maxsize
    def get(self, key: _T, getter_func: Callable[[], _U]) -> _U:
        Get object from the cache.
        If not found, call `getter_func` to resolve it, and put that on the top
        of the cache instead.
        # Look in cache first.
            return self._data[key]
            # Not found? Get it.
            value = getter_func()
            self._data[key] = value
            self._keys.append(key)
            # Remove the oldest key when the size is exceeded.
            if len(self._data) > self.maxsize:
                key_to_remove = self._keys.popleft()
                if key_to_remove in self._data:
                    del self._data[key_to_remove]
        "Clear cache."
        self._data = {}
        self._keys = deque()
_K = TypeVar("_K", bound=Tuple[Hashable, ...])
class FastDictCache(Dict[_K, _V]):
    Fast, lightweight cache which keeps at most `size` items.
    It will discard the oldest items in the cache first.
    The cache is a dictionary, which doesn't keep track of access counts.
    It is perfect to cache little immutable objects which are not expensive to
    create, but where a dictionary lookup is still much faster than an object
    :param get_value: Callable that's called in case of a missing key.
    # NOTE: This cache is used to cache `prompt_toolkit.layout.screen.Char` and
    #       `prompt_toolkit.Document`. Make sure to keep this really lightweight.
    #       Accessing the cache should stay faster than instantiating new
    #       objects.
    #       (Dictionary lookups are really fast.)
    #       SimpleCache is still required for cases where the cache key is not
    #       the same as the arguments given to the function that creates the
    #       value.)
    def __init__(self, get_value: Callable[..., _V], size: int = 1000000) -> None:
        assert size > 0
        self._keys: deque[_K] = deque()
        self.get_value = get_value
    def __missing__(self, key: _K) -> _V:
        if len(self) > self.size:
            if key_to_remove in self:
                del self[key_to_remove]
        result = self.get_value(*key)
        self[key] = result
_F = TypeVar("_F", bound=Callable[..., object])
def memoized(maxsize: int = 1024) -> Callable[[_F], _F]:
    Memoization decorator for immutable classes and pure functions.
    def decorator(obj: _F) -> _F:
        cache: SimpleCache[Hashable, Any] = SimpleCache(maxsize=maxsize)
        @wraps(obj)
        def new_callable(*a: Any, **kw: Any) -> Any:
            def create_new() -> Any:
                return obj(*a, **kw)
            key = (a, tuple(sorted(kw.items())))
            return cache.get(key, create_new)
        return cast(_F, new_callable)
# coding=utf-8
# Copyright 2025-present, the HuggingFace Inc. team.
"""Contains the 'hf cache' command group with cache management subcommands."""
from typing import Annotated, Any, Callable, Dict, List, Mapping, Optional, Tuple
from huggingface_hub.errors import CLIError
    ANSI,
    CachedRepoInfo,
    CachedRevisionInfo,
    CacheNotFound,
    HFCacheInfo,
    _format_size,
    scan_cache_dir,
    tabulate,
from ..utils._parsing import parse_duration, parse_size
from ._cli_utils import (
    RepoIdArg,
    RepoTypeOpt,
    RevisionOpt,
    TokenOpt,
    get_hf_api,
    typer_factory,
cache_cli = typer_factory(help="Manage local cache directory.")
#### Cache helper utilities
class _DeletionResolution:
    revisions: frozenset[str]
    selected: dict[CachedRepoInfo, frozenset[CachedRevisionInfo]]
    missing: tuple[str, ...]
_FILTER_PATTERN = re.compile(r"^(?P<key>[a-zA-Z_]+)\s*(?P<op>==|!=|>=|<=|>|<|=)\s*(?P<value>.+)$")
_ALLOWED_OPERATORS = {"=", "!=", ">", "<", ">=", "<="}
_FILTER_KEYS = {"accessed", "modified", "refs", "size", "type"}
_SORT_KEYS = {"accessed", "modified", "name", "size"}
_SORT_PATTERN = re.compile(r"^(?P<key>[a-zA-Z_]+)(?::(?P<order>asc|desc))?$")
_SORT_DEFAULT_ORDER = {
    # Default ordering: accessed/modified/size are descending (newest/biggest first), name is ascending
    "accessed": "desc",
    "modified": "desc",
    "size": "desc",
    "name": "asc",
# Dynamically generate SortOptions enum from _SORT_KEYS
_sort_options_dict = {}
for key in sorted(_SORT_KEYS):
    _sort_options_dict[key] = key
    _sort_options_dict[f"{key}_asc"] = f"{key}:asc"
    _sort_options_dict[f"{key}_desc"] = f"{key}:desc"
SortOptions = Enum("SortOptions", _sort_options_dict, type=str, module=__name__)  # type: ignore
class CacheDeletionCounts:
    """Simple counters summarizing cache deletions for CLI messaging."""
    repo_count: int
    partial_revision_count: int
    total_revision_count: int
CacheEntry = Tuple[CachedRepoInfo, Optional[CachedRevisionInfo]]
RepoRefsMap = Dict[CachedRepoInfo, frozenset[str]]
def summarize_deletions(
    selected_by_repo: Mapping[CachedRepoInfo, frozenset[CachedRevisionInfo]],
) -> CacheDeletionCounts:
    """Summarize deletions across repositories."""
    repo_count = 0
    total_revisions = 0
    revisions_in_full_repos = 0
    for repo, revisions in selected_by_repo.items():
        total_revisions += len(revisions)
        if len(revisions) == len(repo.revisions):
            repo_count += 1
            revisions_in_full_repos += len(revisions)
    partial_revision_count = total_revisions - revisions_in_full_repos
    return CacheDeletionCounts(repo_count, partial_revision_count, total_revisions)
def print_cache_selected_revisions(selected_by_repo: Mapping[CachedRepoInfo, frozenset[CachedRevisionInfo]]) -> None:
    """Pretty-print selected cache revisions during confirmation prompts."""
    for repo in sorted(selected_by_repo.keys(), key=lambda repo: (repo.repo_type, repo.repo_id.lower())):
        repo_key = f"{repo.repo_type}/{repo.repo_id}"
        revisions = sorted(selected_by_repo[repo], key=lambda rev: rev.commit_hash)
            print(f"  - {repo_key} (entire repo)")
        print(f"  - {repo_key}:")
            refs = " ".join(sorted(revision.refs)) or "(detached)"
            print(f"      {revision.commit_hash} [{refs}] {revision.size_on_disk_str}")
def build_cache_index(
    hf_cache_info: HFCacheInfo,
) -> Tuple[
    Dict[str, CachedRepoInfo],
    Dict[str, Tuple[CachedRepoInfo, CachedRevisionInfo]],
    """Create lookup tables so CLI commands can resolve repo ids and revisions quickly."""
    repo_lookup: dict[str, CachedRepoInfo] = {}
    revision_lookup: dict[str, tuple[CachedRepoInfo, CachedRevisionInfo]] = {}
    for repo in hf_cache_info.repos:
        repo_key = repo.cache_id.lower()
        repo_lookup[repo_key] = repo
        for revision in repo.revisions:
            revision_lookup[revision.commit_hash.lower()] = (repo, revision)
    return repo_lookup, revision_lookup
def collect_cache_entries(
    hf_cache_info: HFCacheInfo, *, include_revisions: bool
) -> Tuple[List[CacheEntry], RepoRefsMap]:
    """Flatten cache metadata into rows consumed by `hf cache ls`."""
    entries: List[CacheEntry] = []
    repo_refs_map: RepoRefsMap = {}
    sorted_repos = sorted(hf_cache_info.repos, key=lambda repo: (repo.repo_type, repo.repo_id.lower()))
    for repo in sorted_repos:
        repo_refs_map[repo] = frozenset({ref for revision in repo.revisions for ref in revision.refs})
        if include_revisions:
            for revision in sorted(repo.revisions, key=lambda rev: rev.commit_hash):
                entries.append((repo, revision))
            entries.append((repo, None))
        entries.sort(
            key=lambda entry: (
                entry[0].cache_id,
                entry[1].commit_hash if entry[1] is not None else "",
        entries.sort(key=lambda entry: entry[0].cache_id)
    return entries, repo_refs_map
def compile_cache_filter(
    expr: str, repo_refs_map: RepoRefsMap
) -> Callable[[CachedRepoInfo, Optional[CachedRevisionInfo], float], bool]:
    """Convert a `hf cache ls` filter expression into the yes/no test we apply to each cache entry before displaying it."""
    match = _FILTER_PATTERN.match(expr.strip())
        raise ValueError(f"Invalid filter expression: '{expr}'.")
    key = match.group("key").lower()
    op = match.group("op")
    value_raw = match.group("value").strip()
    if op not in _ALLOWED_OPERATORS:
        raise ValueError(f"Unsupported operator '{op}' in filter '{expr}'. Must be one of {list(_ALLOWED_OPERATORS)}.")
    if key not in _FILTER_KEYS:
        raise ValueError(f"Unsupported filter key '{key}' in '{expr}'. Must be one of {list(_FILTER_KEYS)}.")
    # at this point we know that key is in `_FILTER_KEYS`
    if key == "size":
        size_threshold = parse_size(value_raw)
        return lambda repo, revision, _: _compare_numeric(
            revision.size_on_disk if revision is not None else repo.size_on_disk,
            size_threshold,
    if key in {"modified", "accessed"}:
        seconds = parse_duration(value_raw.strip())
        def _time_filter(repo: CachedRepoInfo, revision: Optional[CachedRevisionInfo], now: float) -> bool:
            timestamp = (
                repo.last_accessed
                if key == "accessed"
                else revision.last_modified
                if revision is not None
                else repo.last_modified
            return _compare_numeric(now - timestamp, op, seconds)
        return _time_filter
    if key == "type":
        expected = value_raw.lower()
        if op != "=":
            raise ValueError(f"Only '=' is supported for 'type' filters. Got '{op}'.")
        def _type_filter(repo: CachedRepoInfo, revision: Optional[CachedRevisionInfo], _: float) -> bool:
            return repo.repo_type.lower() == expected
        return _type_filter
    else:  # key == "refs"
            raise ValueError(f"Only '=' is supported for 'refs' filters. Got {op}.")
        def _refs_filter(repo: CachedRepoInfo, revision: Optional[CachedRevisionInfo], _: float) -> bool:
            refs = revision.refs if revision is not None else repo_refs_map.get(repo, frozenset())
            return value_raw.lower() in [ref.lower() for ref in refs]
        return _refs_filter
def _build_cache_export_payload(
    entries: List[CacheEntry], *, include_revisions: bool, repo_refs_map: RepoRefsMap
) -> List[Dict[str, Any]]:
    """Normalize cache entries into serializable records for JSON/CSV exports."""
    payload: List[Dict[str, Any]] = []
    for repo, revision in entries:
            if revision is None:
            record: Dict[str, Any] = {
                "repo_id": repo.repo_id,
                "repo_type": repo.repo_type,
                "revision": revision.commit_hash,
                "snapshot_path": str(revision.snapshot_path),
                "size_on_disk": revision.size_on_disk,
                "last_accessed": repo.last_accessed,
                "last_modified": revision.last_modified,
                "refs": sorted(revision.refs),
                "size_on_disk": repo.size_on_disk,
                "last_modified": repo.last_modified,
                "refs": sorted(repo_refs_map.get(repo, frozenset())),
        payload.append(record)
def print_cache_entries_table(
    """Render cache entries as a table and show a human-readable summary."""
        message = "No cached revisions found." if include_revisions else "No cached repositories found."
    table_rows: List[List[str]]
        headers = ["ID", "REVISION", "SIZE", "LAST_MODIFIED", "REFS"]
        table_rows = [
                repo.cache_id,
                revision.commit_hash,
                revision.size_on_disk_str.rjust(8),
                revision.last_modified_str,
                " ".join(sorted(revision.refs)),
            for repo, revision in entries
        headers = ["ID", "SIZE", "LAST_ACCESSED", "LAST_MODIFIED", "REFS"]
                repo.size_on_disk_str.rjust(8),
                repo.last_accessed_str or "",
                repo.last_modified_str,
                " ".join(sorted(repo_refs_map.get(repo, frozenset()))),
            for repo, _ in entries
    print(tabulate(table_rows, headers=headers))  # type: ignore[arg-type]
    unique_repos = {repo for repo, _ in entries}
    repo_count = len(unique_repos)
        revision_count = sum(1 for _, revision in entries if revision is not None)
        total_size = sum(revision.size_on_disk for _, revision in entries if revision is not None)
        revision_count = sum(len(repo.revisions) for repo in unique_repos)
        total_size = sum(repo.size_on_disk for repo in unique_repos)
    summary = f"\nFound {repo_count} repo(s) for a total of {revision_count} revision(s) and {_format_size(total_size)} on disk."
    print(ANSI.bold(summary))
def print_cache_entries_json(
    """Dump cache entries as JSON for scripting or automation."""
    payload = _build_cache_export_payload(entries, include_revisions=include_revisions, repo_refs_map=repo_refs_map)
    json.dump(payload, sys.stdout, indent=2)
def _compare_numeric(left: Optional[float], op: str, right: float) -> bool:
    """Evaluate numeric comparisons for filters."""
    if left is None:
    comparisons = {
        "=": left == right,
        "!=": left != right,
        ">": left > right,
        "<": left < right,
        ">=": left >= right,
        "<=": left <= right,
    if op not in comparisons:
        raise ValueError(f"Unsupported numeric comparison operator: {op}")
    return comparisons[op]
def compile_cache_sort(sort_expr: str) -> tuple[Callable[[CacheEntry], tuple[Any, ...]], bool]:
    """Convert a `hf cache ls` sort expression into a key function for sorting entries.
        A tuple of (key_function, reverse_flag) where reverse_flag indicates whether
        to sort in descending order (True) or ascending order (False).
    match = _SORT_PATTERN.match(sort_expr.strip().lower())
        raise ValueError(f"Invalid sort expression: '{sort_expr}'. Expected format: 'key' or 'key:asc' or 'key:desc'.")
    explicit_order = match.group("order")
    if key not in _SORT_KEYS:
        raise ValueError(f"Unsupported sort key '{key}' in '{sort_expr}'. Must be one of {list(_SORT_KEYS)}.")
    # Use explicit order if provided, otherwise use default for the key
    order = explicit_order if explicit_order else _SORT_DEFAULT_ORDER[key]
    reverse = order == "desc"
    def _sort_key(entry: CacheEntry) -> tuple[Any, ...]:
        repo, revision = entry
            # Sort by cache_id (repo type/id)
            value: Any = repo.cache_id.lower()
            return (value,)
            # Use revision size if available, otherwise repo size
            value = revision.size_on_disk if revision is not None else repo.size_on_disk
        if key == "accessed":
            # For revisions, accessed is not available per-revision, use repo's last_accessed
            # For repos, use repo's last_accessed
            value = repo.last_accessed if repo.last_accessed is not None else 0.0
        if key == "modified":
            # Use revision's last_modified if available, otherwise repo's last_modified
                value = revision.last_modified if revision.last_modified is not None else 0.0
                value = repo.last_modified if repo.last_modified is not None else 0.0
        # Should never reach here due to validation above
        raise ValueError(f"Unsupported sort key: {key}")
    return _sort_key, reverse
def _resolve_deletion_targets(hf_cache_info: HFCacheInfo, targets: list[str]) -> _DeletionResolution:
    """Resolve the deletion targets into a deletion resolution."""
    repo_lookup, revision_lookup = build_cache_index(hf_cache_info)
    selected: dict[CachedRepoInfo, set[CachedRevisionInfo]] = defaultdict(set)
    revisions: set[str] = set()
    missing: list[str] = []
    for raw_target in targets:
        target = raw_target.strip()
        if not target:
        lowered = target.lower()
        if re.fullmatch(r"[0-9a-fA-F]{40}", lowered):
            match = revision_lookup.get(lowered)
                missing.append(raw_target)
            repo, revision = match
            selected[repo].add(revision)
            revisions.add(revision.commit_hash)
        matched_repo = repo_lookup.get(lowered)
        if matched_repo is None:
        for revision in matched_repo.revisions:
            selected[matched_repo].add(revision)
    frozen_selected = {repo: frozenset(revs) for repo, revs in selected.items()}
    return _DeletionResolution(
        revisions=frozenset(revisions),
        selected=frozen_selected,
        missing=tuple(missing),
#### Cache CLI commands
@cache_cli.command(
        "hf cache ls",
        "hf cache ls --revisions",
        'hf cache ls --filter "size>1GB" --limit 20',
        "hf cache ls --format json",
def ls(
    cache_dir: Annotated[
            help="Cache directory to scan (defaults to Hugging Face cache).",
    revisions: Annotated[
            help="Include revisions in the output instead of aggregated repositories.",
    filter: Annotated[
        Optional[list[str]],
            "--filter",
            help="Filter entries (e.g. 'size>1GB', 'type=model', 'accessed>7d'). Can be used multiple times.",
    format: Annotated[
            help="Output format.",
    ] = OutputFormat.table,
    quiet: Annotated[
            help="Print only IDs (repo IDs or revision hashes).",
    sort: Annotated[
        Optional[SortOptions],
            help="Sort entries by key. Supported keys: 'accessed', 'modified', 'name', 'size'. "
            "Append ':asc' or ':desc' to explicitly set the order (e.g., 'modified:asc'). "
            "Defaults: 'accessed', 'modified', 'size' default to 'desc' (newest/biggest first); "
            "'name' defaults to 'asc' (alphabetical).",
    limit: Annotated[
        Optional[int],
            help="Limit the number of results returned. Returns only the top N entries after sorting.",
    """List cached repositories or revisions."""
        hf_cache_info = scan_cache_dir(cache_dir)
    except CacheNotFound as exc:
        raise CLIError(f"Cache directory not found: {exc.cache_dir}") from exc
    filters = filter or []
    entries, repo_refs_map = collect_cache_entries(hf_cache_info, include_revisions=revisions)
        filter_fns = [compile_cache_filter(expr, repo_refs_map) for expr in filters]
        raise typer.BadParameter(str(exc)) from exc
    for fn in filter_fns:
        entries = [entry for entry in entries if fn(entry[0], entry[1], now)]
    # Apply sorting if requested
            sort_key_fn, reverse = compile_cache_sort(sort.value)
            entries.sort(key=sort_key_fn, reverse=reverse)
    # Apply limit if requested
        if limit < 0:
            raise typer.BadParameter(f"Limit must be a positive integer, got {limit}.")
        entries = entries[:limit]
    if quiet:
            print(revision.commit_hash if revision is not None else repo.cache_id)
    formatters = {
        OutputFormat.table: print_cache_entries_table,
        OutputFormat.json: print_cache_entries_json,
    return formatters[format](entries, include_revisions=revisions, repo_refs_map=repo_refs_map)
        "hf cache rm model/gpt2",
        "hf cache rm <revision_hash>",
        "hf cache rm model/gpt2 --dry-run",
        "hf cache rm model/gpt2 --yes",
def rm(
    targets: Annotated[
        list[str],
        typer.Argument(
            help="One or more repo IDs (e.g. model/bert-base-uncased) or revision hashes to delete.",
    yes: Annotated[
            "-y",
            "--yes",
            help="Skip confirmation prompt.",
    dry_run: Annotated[
            help="Preview deletions without removing anything.",
    """Remove cached repositories or revisions."""
    resolution = _resolve_deletion_targets(hf_cache_info, targets)
    if resolution.missing:
        print("Could not find the following targets in the cache:")
        for entry in resolution.missing:
            print(f"  - {entry}")
    if len(resolution.revisions) == 0:
        print("Nothing to delete.")
        raise typer.Exit(code=0)
    strategy = hf_cache_info.delete_revisions(*sorted(resolution.revisions))
    counts = summarize_deletions(resolution.selected)
    summary_parts: list[str] = []
    if counts.repo_count:
        summary_parts.append(f"{counts.repo_count} repo(s)")
    if counts.partial_revision_count:
        summary_parts.append(f"{counts.partial_revision_count} revision(s)")
    if not summary_parts:
        summary_parts.append(f"{counts.total_revision_count} revision(s)")
    summary_text = " and ".join(summary_parts)
    print(f"About to delete {summary_text} totalling {strategy.expected_freed_size_str}.")
    print_cache_selected_revisions(resolution.selected)
        print("Dry run: no files were deleted.")
    if not yes and not typer.confirm("Proceed with deletion?", default=False):
        print("Deletion cancelled.")
    strategy.execute()
        f"Deleted {counts.repo_count} repo(s) and {counts.total_revision_count} revision(s); freed {strategy.expected_freed_size_str}."
@cache_cli.command(examples=["hf cache prune", "hf cache prune --dry-run"])
def prune(
    """Remove detached revisions from the cache."""
    selected: dict[CachedRepoInfo, frozenset[CachedRevisionInfo]] = {}
        detached = frozenset(revision for revision in repo.revisions if len(revision.refs) == 0)
        if not detached:
        selected[repo] = detached
        revisions.update(revision.commit_hash for revision in detached)
    if len(revisions) == 0:
        print("No unreferenced revisions found. Nothing to prune.")
    resolution = _DeletionResolution(
        selected=selected,
        missing=(),
    counts = summarize_deletions(selected)
        f"About to delete {counts.total_revision_count} unreferenced revision(s) ({strategy.expected_freed_size_str} total)."
    print_cache_selected_revisions(selected)
    if not yes and not typer.confirm("Proceed?"):
        print("Pruning cancelled.")
    print(f"Deleted {counts.total_revision_count} unreferenced revision(s); freed {strategy.expected_freed_size_str}.")
        "hf cache verify gpt2",
        "hf cache verify gpt2 --revision refs/pr/1",
        "hf cache verify my-dataset --repo-type dataset",
def verify(
    repo_id: RepoIdArg,
    repo_type: RepoTypeOpt = RepoTypeOpt.model,
    revision: RevisionOpt = None,
            help="Cache directory to use when verifying files from cache (defaults to Hugging Face cache).",
    local_dir: Annotated[
            help="If set, verify files under this directory instead of the cache.",
    fail_on_missing_files: Annotated[
            "--fail-on-missing-files",
            help="Fail if some files exist on the remote but are missing locally.",
    fail_on_extra_files: Annotated[
            "--fail-on-extra-files",
            help="Fail if some files exist locally but are not present on the remote revision.",
    """Verify checksums for a single repo revision from cache or a local directory.
      - Verify main revision in cache: `hf cache verify gpt2`
      - Verify specific revision: `hf cache verify gpt2 --revision refs/pr/1`
      - Verify dataset: `hf cache verify karpathy/fineweb-edu-100b-shuffle --repo-type dataset`
      - Verify local dir: `hf cache verify deepseek-ai/DeepSeek-OCR --local-dir /path/to/repo`
    if local_dir is not None and cache_dir is not None:
        print("Cannot pass both --local-dir and --cache-dir. Use one or the other.")
        raise typer.Exit(code=2)
    api = get_hf_api(token=token)
    result = api.verify_repo_checksums(
        repo_id=repo_id,
        repo_type=repo_type.value if hasattr(repo_type, "value") else str(repo_type),
        revision=revision,
        local_dir=local_dir,
        cache_dir=cache_dir,
    exit_code = 0
    has_mismatches = bool(result.mismatches)
    if has_mismatches:
        print("❌ Checksum verification failed for the following file(s):")
        for m in result.mismatches:
            print(f"  - {m['path']}: expected {m['expected']} ({m['algorithm']}), got {m['actual']}")
    if result.missing_paths:
        if fail_on_missing_files:
            print("Missing files (present remotely, absent locally):")
            for p in result.missing_paths:
                print(f"  - {p}")
            warning = (
                f"{len(result.missing_paths)} remote file(s) are missing locally. "
                "Use --fail-on-missing-files for details."
            print(f"⚠️  {warning}")
    if result.extra_paths:
        if fail_on_extra_files:
            print("Extra files (present locally, absent remotely):")
            for p in result.extra_paths:
                f"{len(result.extra_paths)} local file(s) do not exist on the remote repo. "
                "Use --fail-on-extra-files for details."
    verified_location = result.verified_path
    if exit_code != 0:
        print(f"❌ Verification failed for '{repo_id}' ({repo_type.value}) in {verified_location}.")
        print(f"  Revision: {result.revision}")
        raise typer.Exit(code=exit_code)
    print(f"✅ Verified {result.checked_count} file(s) for '{repo_id}' ({repo_type.value}) in {verified_location}")
    print("  All checksums match.")
