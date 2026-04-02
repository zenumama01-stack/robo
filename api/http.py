"""Classes to encapsulate a single HTTP request.
The classes implement a command pattern, with every
object supporting an execute() method that does the
actual HTTP request.
import socket
# TODO(issue 221): Remove this conditional import jibbajabba.
    _ssl_SSLError = object()
    _ssl_SSLError = ssl.SSLError
from email.generator import Generator
from email.parser import FeedParser
from googleapiclient import _auth
    BatchError,
    InvalidChunkSizeError,
    ResumableUploadError,
    UnexpectedBodyError,
    UnexpectedMethodError,
from googleapiclient.model import JsonModel
LOGGER = logging.getLogger(__name__)
DEFAULT_CHUNK_SIZE = 100 * 1024 * 1024
MAX_URI_LENGTH = 2048
MAX_BATCH_LIMIT = 1000
_TOO_MANY_REQUESTS = 429
DEFAULT_HTTP_TIMEOUT_SEC = 60
_LEGACY_BATCH_URI = "https://www.googleapis.com/batch"
def _should_retry_response(resp_status, content):
    """Determines whether a response should be retried.
    reason = None
    # Retry on 5xx errors.
    if resp_status >= 500:
    # Retry on 429 errors.
    if resp_status == _TOO_MANY_REQUESTS:
    # For 403 errors, we have to check for the `reason` in the response to
    # determine if we should retry.
    if resp_status == http_client.FORBIDDEN:
        # If there's no details about the 403 type, don't retry.
        if not content:
        # Content is in JSON format.
            data = json.loads(content.decode("utf-8"))
                # There are many variations of the error json so we need
                # to determine the keyword which has the error detail. Make sure
                # that the order of the keywords below isn't changed as it can
                # break user code. If the "errors" key exists, we must use that
                # first.
                # See Issue #1243
                # https://github.com/googleapis/google-api-python-client/issues/1243
                        for kw in ["errors", "status", "message"]
                    reason = data["error"][error_detail_keyword]
                    if isinstance(reason, list) and len(reason) > 0:
                        reason = reason[0]
                        if "reason" in reason:
                            reason = reason["reason"]
                reason = data[0]["error"]["errors"]["reason"]
        except (UnicodeDecodeError, ValueError, KeyError):
            LOGGER.warning("Invalid JSON content from response: %s", content)
        LOGGER.warning('Encountered 403 Forbidden with reason "%s"', reason)
        # Only retry on rate limit related failures.
        if reason in ("userRateLimitExceeded", "rateLimitExceeded"):
    # Everything else is a success or non-retriable so break.
def _retry_request(
    http, num_retries, req_type, sleep, rand, uri, method, *args, **kwargs
    """Retries an HTTP request multiple times while handling errors.
    resp = None
    content = None
    exception = None
    for retry_num in range(num_retries + 1):
        if retry_num > 0:
            # Sleep before retrying.
            sleep_time = rand() * 2**retry_num
            LOGGER.warning(
                "Sleeping %.2f seconds before retry %d of %d for %s: %s %s, after %s",
                sleep_time,
                retry_num,
                num_retries,
                req_type,
                resp.status if resp else exception,
            sleep(sleep_time)
            resp, content = http.request(uri, method, *args, **kwargs)
        # Retry on SSL errors and socket timeout errors.
        except _ssl_SSLError as ssl_error:
            exception = ssl_error
        except socket.timeout as socket_timeout:
            # Needs to be before socket.error as it's a subclass of OSError
            # socket.timeout has no errorcode
            exception = socket_timeout
        except ConnectionError as connection_error:
            exception = connection_error
        except OSError as socket_error:
            # errno's contents differ by platform, so we have to match by name.
            # Some of these same errors may have been caught above, e.g. ECONNRESET *should* be
            # raised as a ConnectionError, but some libraries will raise it as a socket.error
            # with an errno corresponding to ECONNRESET
            if socket.errno.errorcode.get(socket_error.errno) not in {
                "WSAETIMEDOUT",
                "ETIMEDOUT",
                "EPIPE",
                "ECONNABORTED",
                "ECONNREFUSED",
                "ECONNRESET",
            }:
            exception = socket_error
        except httplib2.ServerNotFoundError as server_not_found_error:
            exception = server_not_found_error
        if exception:
            if retry_num == num_retries:
                raise exception
        if not _should_retry_response(resp.status, content):
    return resp, content
class MediaUploadProgress(object):
    """Status of a resumable upload."""
    def __init__(self, resumable_progress, total_size):
        """Constructor.
        self.resumable_progress = resumable_progress
        self.total_size = total_size
    def progress(self):
        """Percent of upload completed, as a float.
        if self.total_size is not None and self.total_size != 0:
            return float(self.resumable_progress) / float(self.total_size)
class MediaDownloadProgress(object):
    """Status of a resumable download."""
        """Percent of download completed, as a float.
class MediaUpload(object):
    """Describes a media object to upload.
    Base class that defines the interface of MediaUpload subclasses.
    Note that subclasses of MediaUpload may allow you to control the chunksize
    when uploading a media object. It is important to keep the size of the chunk
    as large as possible to keep the upload efficient. Other factors may influence
    the size of the chunk you use, particularly if you are working in an
    environment where individual HTTP requests may have a hardcoded time limit,
    such as under certain classes of requests under Google App Engine.
    Streams are io.Base compatible objects that support seek(). Some MediaUpload
    subclasses support using streams directly to upload data. Support for
    streaming may be indicated by a MediaUpload sub-class and if appropriate for a
    platform that stream will be used for uploading the media object. The support
    for streaming is indicated by has_stream() returning True. The stream() method
    should return an io.Base object that supports seek(). On platforms where the
    underlying httplib module supports streaming, for example Python 2.6 and
    later, the stream will be passed into the http library which will result in
    less memory being used and possibly faster uploads.
    If you need to upload media that can't be uploaded using any of the existing
    MediaUpload sub-class then you can sub-class MediaUpload for your particular
    needs.
    def chunksize(self):
        """Chunk size for resumable uploads.
    def mimetype(self):
        """Mime type of the body.
        return "application/octet-stream"
    def size(self):
        """Size of upload.
    def resumable(self):
        """Whether this upload is resumable.
    def getbytes(self, begin, end):
        """Get bytes from the media.
    def has_stream(self):
        """Does the underlying upload support a streaming interface.
    def stream(self):
        """A stream interface to the data being uploaded.
    def _to_json(self, strip=None):
        """Utility function for creating a JSON representation of a MediaUpload.
        t = type(self)
        d = copy.copy(self.__dict__)
        if strip is not None:
            for member in strip:
                del d[member]
        d["_class"] = t.__name__
        d["_module"] = t.__module__
        return json.dumps(d)
    def to_json(self):
        """Create a JSON representation of an instance of MediaUpload.
        return self._to_json()
    def new_from_json(cls, s):
        """Utility class method to instantiate a MediaUpload subclass from a JSON
        data = json.loads(s)
        # Find and call the right classmethod from_json() to restore the object.
        module = data["_module"]
        m = __import__(module, fromlist=module.split(".")[:-1])
        kls = getattr(m, data["_class"])
        from_json = getattr(kls, "from_json")
        return from_json(s)
class MediaIoBaseUpload(MediaUpload):
    """A MediaUpload for a io.Base objects.
    def __init__(self, fd, mimetype, chunksize=DEFAULT_CHUNK_SIZE, resumable=False):
        super(MediaIoBaseUpload, self).__init__()
        self._fd = fd
        self._mimetype = mimetype
        if not (chunksize == -1 or chunksize > 0):
            raise InvalidChunkSizeError()
        self._chunksize = chunksize
        self._resumable = resumable
        self._fd.seek(0, os.SEEK_END)
        self._size = self._fd.tell()
        return self._chunksize
        return self._mimetype
        return self._size
        return self._resumable
    def getbytes(self, begin, length):
        self._fd.seek(begin)
        return self._fd.read(length)
        return self._fd
        """This upload type is not serializable."""
        raise NotImplementedError("MediaIoBaseUpload is not serializable.")
class MediaFileUpload(MediaIoBaseUpload):
    """A MediaUpload for a file.
        self, filename, mimetype=None, chunksize=DEFAULT_CHUNK_SIZE, resumable=False
        self._fd = None
        self._fd = open(self._filename, "rb")
        if mimetype is None:
            # No mimetype provided, make a guess.
            mimetype, _ = mimetypes.guess_type(filename)
                # Guess failed, use octet-stream.
                mimetype = "application/octet-stream"
        super(MediaFileUpload, self).__init__(
            self._fd, mimetype, chunksize=chunksize, resumable=resumable
    def __del__(self):
        if self._fd:
            self._fd.close()
        """Creating a JSON representation of an instance of MediaFileUpload.
        return self._to_json(strip=["_fd"])
    def from_json(s):
        d = json.loads(s)
        return MediaFileUpload(
            d["_filename"],
            mimetype=d["_mimetype"],
            chunksize=d["_chunksize"],
            resumable=d["_resumable"],
class MediaInMemoryUpload(MediaIoBaseUpload):
    """MediaUpload for a chunk of bytes.
    DEPRECATED: Use MediaIoBaseUpload with either io.TextIOBase or io.StringIO for
        mimetype="application/octet-stream",
        chunksize=DEFAULT_CHUNK_SIZE,
        resumable=False,
        """Create a new MediaInMemoryUpload.
        fd = io.BytesIO(body)
        super(MediaInMemoryUpload, self).__init__(
            fd, mimetype, chunksize=chunksize, resumable=resumable
class MediaIoBaseDownload(object):
    """ "Download media resources.
          print "Download %d%%." % int(status.progress() * 100)
      print "Download Complete!"
    def __init__(self, fd, request, chunksize=DEFAULT_CHUNK_SIZE):
        self._request = request
        self._uri = request.uri
        self._total_size = None
        self._done = False
        # Stubs for testing.
        self._sleep = time.sleep
        self._rand = random.random
        self._headers = {}
        for k, v in request.headers.items():
            # allow users to supply custom headers by setting them on the request
            # but strip out the ones that are set by default on requests generated by
            # API methods like Drive's files().get(fileId=...)
            if not k.lower() in ("accept", "accept-encoding", "user-agent"):
                self._headers[k] = v
    def next_chunk(self, num_retries=0):
        """Get the next chunk of the download.
        headers = self._headers.copy()
        headers["range"] = "bytes=%d-%d" % (
            self._progress,
            self._progress + self._chunksize - 1,
        http = self._request.http
        resp, content = _retry_request(
            "media download",
            self._sleep,
            self._rand,
            self._uri,
            "GET",
        if resp.status in [200, 206]:
            if "content-location" in resp and resp["content-location"] != self._uri:
                self._uri = resp["content-location"]
            self._progress += len(content)
            self._fd.write(content)
            if "content-range" in resp:
                content_range = resp["content-range"]
                length = content_range.rsplit("/", 1)[1]
                self._total_size = int(length)
            elif "content-length" in resp:
                self._total_size = int(resp["content-length"])
            if self._total_size is None or self._progress == self._total_size:
                self._done = True
            return MediaDownloadProgress(self._progress, self._total_size), self._done
        elif resp.status == 416:
            # 416 is Range Not Satisfiable
            # This typically occurs with a zero byte file
            if self._total_size == 0:
                    MediaDownloadProgress(self._progress, self._total_size),
                    self._done,
        raise HttpError(resp, content, uri=self._uri)
class _StreamSlice(object):
    """Truncated stream.
    Takes a stream and presents a stream that is a slice of the original stream.
    This is used when uploading media in chunks. In later versions of Python a
    stream can be passed to httplib in place of the string of data to send. The
    problem is that httplib just blindly reads to the end of the stream. This
    wrapper presents a virtual stream that only reads to the end of the chunk.
    def __init__(self, stream, begin, chunksize):
        self._begin = begin
        self._stream.seek(begin)
    def read(self, n=-1):
        """Read n bytes.
        # The data left available to read sits in [cur, end)
        cur = self._stream.tell()
        end = self._begin + self._chunksize
        if n == -1 or cur + n > end:
            n = end - cur
        return self._stream.read(n)
class HttpRequest(object):
    """Encapsulates a single HTTP request."""
    @util.positional(4)
        postproc,
        method="GET",
        body=None,
        headers=None,
        methodId=None,
        resumable=None,
        """Constructor for an HttpRequest.
        self.headers = headers or {}
        self.methodId = methodId
        self.http = http
        self.postproc = postproc
        self.resumable = resumable
        self.response_callbacks = []
        self._in_error_state = False
        # The size of the non-media part of the request.
        self.body_size = len(self.body or "")
        # The resumable URI to send chunks to.
        self.resumable_uri = None
        # The bytes that have been uploaded.
        self.resumable_progress = 0
    def execute(self, http=None, num_retries=0):
        """Execute the request.
            http = self.http
        if self.resumable:
            while body is None:
                _, body = self.next_chunk(http=http, num_retries=num_retries)
        # Non-resumable case.
        if "content-length" not in self.headers:
            self.headers["content-length"] = str(self.body_size)
        # If the request URI is too long then turn it into a POST request.
        # Assume that a GET request never contains a request body.
        if len(self.uri) > MAX_URI_LENGTH and self.method == "GET":
            self.method = "POST"
            self.headers["x-http-method-override"] = "GET"
            self.headers["content-type"] = "application/x-www-form-urlencoded"
            parsed = urllib.parse.urlparse(self.uri)
            self.uri = urllib.parse.urlunparse(
                (parsed.scheme, parsed.netloc, parsed.path, parsed.params, None, None)
            self.body = parsed.query
            self.headers["content-length"] = str(len(self.body))
        # Handle retries for server-side errors.
            "request",
            str(self.uri),
            method=str(self.method),
            body=self.body,
            headers=self.headers,
        for callback in self.response_callbacks:
            callback(resp)
        if resp.status >= 300:
            raise HttpError(resp, content, uri=self.uri)
        return self.postproc(resp, content)
    def add_response_callback(self, cb):
        """add_response_headers_callback
        self.response_callbacks.append(cb)
    def next_chunk(self, http=None, num_retries=0):
        """Execute the next step of a resumable upload.
              print "Upload %d%% complete." % int(status.progress() * 100)
        if self.resumable.size() is None:
            size = "*"
            size = str(self.resumable.size())
        if self.resumable_uri is None:
            start_headers = copy.copy(self.headers)
            start_headers["X-Upload-Content-Type"] = self.resumable.mimetype()
            if size != "*":
                start_headers["X-Upload-Content-Length"] = size
            start_headers["content-length"] = str(self.body_size)
                "resumable URI request",
                method=self.method,
                headers=start_headers,
            if resp.status == 200 and "location" in resp:
                self.resumable_uri = resp["location"]
                raise ResumableUploadError(resp, content)
        elif self._in_error_state:
            # If we are in an error state then query the server for current state of
            # the upload by sending an empty PUT and reading the 'range' header in
            headers = {"Content-Range": "bytes */%s" % size, "content-length": "0"}
            resp, content = http.request(self.resumable_uri, "PUT", headers=headers)
            status, body = self._process_response(resp, content)
            if body:
                # The upload was complete.
                return (status, body)
        if self.resumable.has_stream():
            data = self.resumable.stream()
            if self.resumable.chunksize() == -1:
                data.seek(self.resumable_progress)
                chunk_end = self.resumable.size() - self.resumable_progress - 1
                # Doing chunking with a stream, so wrap a slice of the stream.
                data = _StreamSlice(
                    data, self.resumable_progress, self.resumable.chunksize()
                chunk_end = min(
                    self.resumable_progress + self.resumable.chunksize() - 1,
                    self.resumable.size() - 1,
            data = self.resumable.getbytes(
                self.resumable_progress, self.resumable.chunksize()
            # A short read implies that we are at EOF, so finish the upload.
            if len(data) < self.resumable.chunksize():
                size = str(self.resumable_progress + len(data))
            chunk_end = self.resumable_progress + len(data) - 1
            # Must set the content-length header here because httplib can't
            # calculate the size when working with _StreamSlice.
            "Content-Length": str(chunk_end - self.resumable_progress + 1),
        # An empty file results in chunk_end = -1 and size = 0
        # sending "bytes 0--1/0" results in an invalid request
        # Only add header "Content-Range" if chunk_end != -1
        if chunk_end != -1:
            headers["Content-Range"] = "bytes %d-%d/%s" % (
                self.resumable_progress,
                chunk_end,
                self._sleep(self._rand() * 2**retry_num)
                    "Retry #%d for media upload: %s %s, following status: %d"
                    % (retry_num, self.method, self.uri, resp.status)
                resp, content = http.request(
                    self.resumable_uri, method="PUT", body=data, headers=headers
                self._in_error_state = True
        return self._process_response(resp, content)
    def _process_response(self, resp, content):
        """Process the response from a single chunk upload.
        if resp.status in [200, 201]:
            return None, self.postproc(resp, content)
        elif resp.status == 308:
            # A "308 Resume Incomplete" indicates we are not done.
                self.resumable_progress = int(resp["range"].split("-")[1]) + 1
                # If resp doesn't contain range header, resumable progress is 0
            if "location" in resp:
            MediaUploadProgress(self.resumable_progress, self.resumable.size()),
        """Returns a JSON representation of the HttpRequest."""
        if d["resumable"] is not None:
            d["resumable"] = self.resumable.to_json()
        del d["http"]
        del d["postproc"]
        del d["_sleep"]
        del d["_rand"]
    def from_json(s, http, postproc):
        """Returns an HttpRequest populated with info from a JSON object."""
            d["resumable"] = MediaUpload.new_from_json(d["resumable"])
        return HttpRequest(
            uri=d["uri"],
            method=d["method"],
            body=d["body"],
            headers=d["headers"],
            methodId=d["methodId"],
            resumable=d["resumable"],
    def null_postproc(resp, contents):
        return resp, contents
class BatchHttpRequest(object):
    """Batches multiple HttpRequest objects into a single HTTP request.
        \"\"\"Do something with the animals list response.\"\"\"
        \"\"\"Do something with the farmers list response.\"\"\"
    def __init__(self, callback=None, batch_uri=None):
        """Constructor for a BatchHttpRequest.
        if batch_uri is None:
            batch_uri = _LEGACY_BATCH_URI
        if batch_uri == _LEGACY_BATCH_URI:
                "You have constructed a BatchHttpRequest using the legacy batch "
                "endpoint %s. This endpoint will be turned down on August 12, 2020. "
                "Please provide the API-specific endpoint or use "
                "service.new_batch_http_request(). For more details see "
                "https://developers.googleblog.com/2018/03/discontinuing-support-for-json-rpc-and.html"
                "and https://developers.google.com/api-client-library/python/guide/batch.",
                _LEGACY_BATCH_URI,
        self._batch_uri = batch_uri
        # Global callback to be called for each individual response in the batch.
        self._callback = callback
        # A map from id to request.
        self._requests = {}
        # A map from id to callback.
        self._callbacks = {}
        # List of request ids, in the order in which they were added.
        self._order = []
        # The last auto generated id.
        self._last_auto_id = 0
        # Unique ID on which to base the Content-ID headers.
        self._base_id = None
        # A map from request id to (httplib2.Response, content) response pairs
        self._responses = {}
        # A map of id(Credentials) that have been refreshed.
        self._refreshed_credentials = {}
    def _refresh_and_apply_credentials(self, request, http):
        """Refresh the credentials and apply to the request.
        # For the credentials to refresh, but only once per refresh_token
        # If there is no http per the request then refresh the http passed in
        # via execute()
        creds = None
        request_credentials = False
        if request.http is not None:
            creds = _auth.get_credentials_from_http(request.http)
            request_credentials = True
        if creds is None and http is not None:
            creds = _auth.get_credentials_from_http(http)
        if creds is not None:
            if id(creds) not in self._refreshed_credentials:
                _auth.refresh_credentials(creds)
                self._refreshed_credentials[id(creds)] = 1
        # Only apply the credentials if we are using the http object passed in,
        # otherwise apply() will get called during _serialize_request().
        if request.http is None or not request_credentials:
            _auth.apply_credentials(creds, request.headers)
    def _id_to_header(self, id_):
        """Convert an id to a Content-ID header value.
        if self._base_id is None:
            self._base_id = uuid.uuid4()
        # NB: we intentionally leave whitespace between base/id and '+', so RFC2822
        # line folding works properly on Python 3; see
        # https://github.com/googleapis/google-api-python-client/issues/164
        return "<%s + %s>" % (self._base_id, urllib.parse.quote(id_))
    def _header_to_id(self, header):
        """Convert a Content-ID header value to an id.
        if header[0] != "<" or header[-1] != ">":
            raise BatchError("Invalid value for Content-ID: %s" % header)
        if "+" not in header:
        base, id_ = header[1:-1].split(" + ", 1)
        return urllib.parse.unquote(id_)
    def _serialize_request(self, request):
        """Convert an HttpRequest object into a string.
        # Construct status line
        parsed = urllib.parse.urlparse(request.uri)
        request_line = urllib.parse.urlunparse(
            ("", "", parsed.path, parsed.params, parsed.query, "")
        status_line = request.method + " " + request_line + " HTTP/1.1\n"
        major, minor = request.headers.get("content-type", "application/json").split(
            "/"
        msg = MIMENonMultipart(major, minor)
        headers = request.headers.copy()
            credentials = _auth.get_credentials_from_http(request.http)
            if credentials is not None:
                _auth.apply_credentials(credentials, headers)
        # MIMENonMultipart adds its own Content-Type header.
        if "content-type" in headers:
            del headers["content-type"]
        for key, value in headers.items():
            msg[key] = value
        msg["Host"] = parsed.netloc
        msg.set_unixfrom(None)
        if request.body is not None:
            msg.set_payload(request.body)
            msg["content-length"] = str(len(request.body))
        # Serialize the mime message.
        fp = io.StringIO()
        # maxheaderlen=0 means don't line wrap headers.
        g = Generator(fp, maxheaderlen=0)
        g.flatten(msg, unixfrom=False)
        return status_line + body
    def _deserialize_response(self, payload):
        """Convert string into httplib2 response and content.
        # Strip off the status line
        status_line, payload = payload.split("\n", 1)
        protocol, status, reason = status_line.split(" ", 2)
        # Parse the rest of the response
        parser = FeedParser()
        parser.feed(payload)
        msg = parser.close()
        msg["status"] = status
        # Create httplib2.Response from the parsed headers.
        resp = httplib2.Response(msg)
        resp.reason = reason
        resp.version = int(protocol.split("/", 1)[1].replace(".", ""))
        content = payload.split("\r\n\r\n", 1)[1]
    def _new_id(self):
        """Create a new id.
        self._last_auto_id += 1
        while str(self._last_auto_id) in self._requests:
        return str(self._last_auto_id)
    def add(self, request, callback=None, request_id=None):
        """Add a new request.
        if len(self._order) >= MAX_BATCH_LIMIT:
            raise BatchError(
                "Exceeded the maximum calls(%d) in a single batch request."
                % MAX_BATCH_LIMIT
        if request_id is None:
            request_id = self._new_id()
        if request.resumable is not None:
            raise BatchError("Media requests cannot be used in a batch request.")
        if request_id in self._requests:
            raise KeyError("A request with this ID already exists: %s" % request_id)
        self._requests[request_id] = request
        self._callbacks[request_id] = callback
        self._order.append(request_id)
    def _execute(self, http, order, requests):
        """Serialize batch request, send to server, process response.
        message = MIMEMultipart("mixed")
        # Message should not write out it's own headers.
        setattr(message, "_write_headers", lambda self: None)
        # Add all the individual requests.
        for request_id in order:
            request = requests[request_id]
            msg = MIMENonMultipart("application", "http")
            msg["Content-ID"] = self._id_to_header(request_id)
            body = self._serialize_request(request)
            message.attach(msg)
        g = Generator(fp, mangle_from_=False)
        g.flatten(message, unixfrom=False)
            "multipart/mixed; " 'boundary="%s"'
        ) % message.get_boundary()
            self._batch_uri, method="POST", body=body, headers=headers
            raise HttpError(resp, content, uri=self._batch_uri)
        # Prepend with a content-type header so FeedParser can handle it.
        header = "content-type: %s\r\n\r\n" % resp["content-type"]
        # PY3's FeedParser only accepts unicode. So we should decode content
        # here, and encode each payload again.
        for_parser = header + content
        parser.feed(for_parser)
        mime_response = parser.close()
        if not mime_response.is_multipart():
                "Response not in multipart/mixed format.", resp=resp, content=content
        for part in mime_response.get_payload():
            request_id = self._header_to_id(part["Content-ID"])
            response, content = self._deserialize_response(part.get_payload())
            # We encode content here to emulate normal http response.
            if isinstance(content, str):
                content = content.encode("utf-8")
            self._responses[request_id] = (response, content)
    def execute(self, http=None):
        """Execute all the requests as a single batched HTTP request.
        # If we have no requests return
        if len(self._order) == 0:
        # If http is not supplied use the first valid one given in the requests.
            for request_id in self._order:
                request = self._requests[request_id]
                if request is not None:
                    http = request.http
            raise ValueError("Missing a valid http object.")
        # Special case for OAuth2Credentials-style objects which have not yet been
        # refreshed with an initial access_token.
            if not _auth.is_valid(creds):
                LOGGER.info("Attempting refresh to obtain initial access_token")
        self._execute(http, self._order, self._requests)
        # Loop over all the requests and check for 401s. For each 401 request the
        # credentials should be refreshed and then sent again in a separate batch.
        redo_requests = {}
        redo_order = []
            resp, content = self._responses[request_id]
            if resp["status"] == "401":
                redo_order.append(request_id)
                self._refresh_and_apply_credentials(request, http)
                redo_requests[request_id] = request
        if redo_requests:
            self._execute(http, redo_order, redo_requests)
        # Now process all callbacks that are erroring, and raise an exception for
        # ones that return a non-2xx response? Or add extra parameter to callback
        # that contains an HttpError?
            callback = self._callbacks[request_id]
                    raise HttpError(resp, content, uri=request.uri)
                response = request.postproc(resp, content)
                exception = e
            if callback is not None:
                callback(request_id, response, exception)
            if self._callback is not None:
                self._callback(request_id, response, exception)
class HttpRequestMock(object):
    """Mock of HttpRequest.
    Do not construct directly, instead use RequestMockBuilder.
    def __init__(self, resp, content, postproc):
        """Constructor for HttpRequestMock
        if resp is None:
            self.resp = httplib2.Response({"status": 200, "reason": "OK"})
        if "reason" in self.resp:
            self.resp.reason = self.resp["reason"]
        Same behavior as HttpRequest.execute(), but the response is
        mocked and not really from an HTTP request/response.
        return self.postproc(self.resp, self.content)
class RequestMockBuilder(object):
    """A simple mock of HttpRequest
      response = '{"data": {"id": "tag:google.c...'
      googleapiclient.discovery.build("plus", "v1", requestBuilder=requestBuilder)
    def __init__(self, responses, check_unexpected=False):
        """Constructor for RequestMockBuilder
        self.responses = responses
        self.check_unexpected = check_unexpected
    def __call__(
        """Implements the callable interface that discovery.build() expects
        of requestBuilder, which is to build an object compatible with
        parameters and the expected response.
        if methodId in self.responses:
            response = self.responses[methodId]
            resp, content = response[:2]
            if len(response) > 2:
                # Test the body against the supplied expected_body.
                expected_body = response[2]
                if bool(expected_body) != bool(body):
                    # Not expecting a body and provided one
                    # or expecting a body and not provided one.
                    raise UnexpectedBodyError(expected_body, body)
                if isinstance(expected_body, str):
                    expected_body = json.loads(expected_body)
                body = json.loads(body)
                if body != expected_body:
            return HttpRequestMock(resp, content, postproc)
        elif self.check_unexpected:
            raise UnexpectedMethodError(methodId=methodId)
            model = JsonModel(False)
            return HttpRequestMock(None, "{}", model.response)
class HttpMock(object):
    """Mock of httplib2.Http"""
    def __init__(self, filename=None, headers=None):
            headers = {"status": "200"}
        if filename:
            with open(filename, "rb") as f:
                self.data = f.read()
            self.data = None
        self.response_headers = headers
        self.headers = None
        self.uri = None
        self.method = None
        self.body = None
        redirections=1,
        connection_type=None,
        self.headers = headers
        return httplib2.Response(self.response_headers), self.data
class HttpMockSequence(object):
    """Mock of httplib2.Http
        ({'status': '200'}, '{"access_token":"1/3w","expires_in":3600}'),
      resp, content = http.request("http://examples.com")
    def __init__(self, iterable):
        self._iterable = iterable
        self.follow_redirects = True
        self.request_sequence = list()
        # Remember the request so after the fact this mock can be examined
        self.request_sequence.append((uri, method, body, headers))
        resp, content = self._iterable.pop(0)
        if content == b"echo_request_headers":
            content = headers
        elif content == b"echo_request_headers_as_json":
            content = json.dumps(headers)
        elif content == b"echo_request_body":
            if hasattr(body, "read"):
                content = body.read()
                content = body
        elif content == b"echo_request_uri":
            content = uri
        return httplib2.Response(resp), content
def set_user_agent(http, user_agent):
    """Set the user-agent on every request.
      h = set_user_agent(h, "my-app-name/6.0")
    request_orig = http.request
    # The closure that will replace 'httplib2.Http.request'.
    def new_request(
        redirections=httplib2.DEFAULT_MAX_REDIRECTS,
        """Modify the request headers to add the user-agent."""
        if "user-agent" in headers:
            headers["user-agent"] = user_agent + " " + headers["user-agent"]
            headers["user-agent"] = user_agent
        resp, content = request_orig(
            method=method,
            redirections=redirections,
            connection_type=connection_type,
    http.request = new_request
    return http
def tunnel_patch(http):
    """Tunnel PATCH requests over POST.
      h = tunnel_patch(h, "my-app-name/6.0")
        if method == "PATCH":
            if "oauth_token" in headers.get("authorization", ""):
                    "OAuth 1.0 request made with Credentials after tunnel_patch."
            headers["x-http-method-override"] = "PATCH"
            method = "POST"
def build_http():
    """Builds httplib2.Http object
    if socket.getdefaulttimeout() is not None:
        http_timeout = socket.getdefaulttimeout()
        http_timeout = DEFAULT_HTTP_TIMEOUT_SEC
    http = httplib2.Http(timeout=http_timeout)
    # 308's are used by several Google APIs (Drive, YouTube)
    # for Resumable Uploads rather than Permanent Redirects.
    # This asks httplib2 to exclude 308s from the status codes
    # it treats as redirects
        http.redirect_codes = http.redirect_codes - {308}
        # Apache Beam tests depend on this library and cannot
        # currently upgrade their httplib2 version
        # http.redirect_codes does not exist in previous versions
        # of httplib2, so pass
from ..networking.exceptions import (
    CertificateVerifyError,
    HTTPError,
    TransportError,
    ContentTooShortError,
    ThrottledDownload,
    parse_http_range,
class HttpFD(FileDownloader):
        url = info_dict['url']
        request_data = info_dict.get('request_data', None)
        request_extensions = {}
        impersonate_target = self._get_impersonate_target(info_dict)
        if impersonate_target is not None:
            request_extensions['impersonate'] = impersonate_target
        class DownloadContext(dict):
            __getattr__ = dict.get
            __setattr__ = dict.__setitem__
            __delattr__ = dict.__delitem__
        ctx = DownloadContext()
        ctx.filename = filename
        ctx.tmpfilename = self.temp_name(filename)
        ctx.stream = None
        # Disable compression
        headers = HTTPHeaderDict({'Accept-Encoding': 'identity'}, info_dict.get('http_headers'))
        is_test = self.params.get('test', False)
        chunk_size = self._TEST_FILE_SIZE if is_test else (
            self.params.get('http_chunk_size')
            or info_dict.get('downloader_options', {}).get('http_chunk_size')
            or 0)
        ctx.open_mode = 'wb'
        ctx.resume_len = 0
        ctx.block_size = self.params.get('buffersize', 1024)
        ctx.start_time = time.time()
        # parse given Range
        req_start, req_end, _ = parse_http_range(headers.get('Range'))
        if self.params.get('continuedl', True):
            # Establish possible resume length
            if os.path.isfile(ctx.tmpfilename):
                ctx.resume_len = os.path.getsize(ctx.tmpfilename)
        ctx.is_resume = ctx.resume_len > 0
        class SucceedDownload(Exception):
        class RetryDownload(Exception):
            def __init__(self, source_error):
                self.source_error = source_error
        class NextFragment(Exception):
        def establish_connection():
            ctx.chunk_size = (random.randint(int(chunk_size * 0.95), chunk_size)
                              if not is_test and chunk_size else chunk_size)
            if ctx.resume_len > 0:
                range_start = ctx.resume_len
                if req_start is not None:
                    # offset the beginning of Range to be within request
                    range_start += req_start
                if ctx.is_resume:
                    self.report_resuming_byte(ctx.resume_len)
                ctx.open_mode = 'ab'
            elif req_start is not None:
                range_start = req_start
            elif ctx.chunk_size > 0:
                range_start = 0
                range_start = None
            ctx.is_resume = False
            if ctx.chunk_size:
                chunk_aware_end = range_start + ctx.chunk_size - 1
                # we're not allowed to download outside Range
                range_end = chunk_aware_end if req_end is None else min(chunk_aware_end, req_end)
            elif req_end is not None:
                # there's no need for chunked downloads, so download until the end of Range
                range_end = req_end
                range_end = None
            if try_call(lambda: range_start > range_end):
                raise RetryDownload(Exception(f'Conflicting range. (start={range_start} > end={range_end})'))
            if try_call(lambda: range_end >= ctx.content_len):
                range_end = ctx.content_len - 1
            request = Request(url, request_data, headers, extensions=request_extensions)
            has_range = range_start is not None
            if has_range:
                request.headers['Range'] = f'bytes={int(range_start)}-{int_or_none(range_end) or ""}'
            # Establish connection
                ctx.data = self.ydl.urlopen(request)
                # When trying to resume, Content-Range HTTP header of response has to be checked
                # to match the value of requested Range HTTP header. This is due to a webservers
                # that don't support resuming and serve a whole file with no Content-Range
                # set in response despite of requested Range (see
                # https://github.com/ytdl-org/youtube-dl/issues/6057#issuecomment-126129799)
                    content_range = ctx.data.headers.get('Content-Range')
                    content_range_start, content_range_end, content_len = parse_http_range(content_range)
                    # Content-Range is present and matches requested Range, resume is possible
                    if range_start == content_range_start and (
                            # Non-chunked download
                            not ctx.chunk_size
                            # Chunked download and requested piece or
                            # its part is promised to be served
                            or content_range_end == range_end
                            or content_len < range_end):
                        ctx.content_len = content_len
                        if content_len or req_end:
                            ctx.data_len = min(content_len or req_end, req_end or content_len) - (req_start or 0)
                    # Content-Range is either not present or invalid. Assuming remote webserver is
                    # trying to send the whole file, resume is not possible, so wiping the local file
                    # and performing entire redownload
                    elif range_start > 0:
                        self.report_unable_to_resume()
                ctx.data_len = ctx.content_len = int_or_none(ctx.data.headers.get('Content-length', None))
            except HTTPError as err:
                if err.status == 416:
                    # Unable to resume (requested range not satisfiable)
                        # Open the connection again without the range header
                        ctx.data = self.ydl.urlopen(
                            Request(url, request_data, headers))
                        content_length = ctx.data.headers['Content-Length']
                        if err.status < 500 or err.status >= 600:
                        # Examine the reported length
                        if (content_length is not None
                                and (ctx.resume_len - 100 < int(content_length) < ctx.resume_len + 100)):
                            # The file had already been fully downloaded.
                            # Explanation to the above condition: in issue #175 it was revealed that
                            # YouTube sometimes adds or removes a few bytes from the end of the file,
                            # changing the file size slightly and causing problems for some users. So
                            # I decided to implement a suggested change and consider the file
                            # completely downloaded if the file size differs less than 100 bytes from
                            # the one in the hard drive.
                            self.report_file_already_downloaded(ctx.filename)
                            self.try_rename(ctx.tmpfilename, ctx.filename)
                                'filename': ctx.filename,
                                'downloaded_bytes': ctx.resume_len,
                                'total_bytes': ctx.resume_len,
                            raise SucceedDownload
                            # The length does not match, we start the download over
                elif err.status < 500 or err.status >= 600:
                    # Unexpected HTTP error
                raise RetryDownload(err)
            except CertificateVerifyError:
            except TransportError as err:
        def close_stream():
            if ctx.stream is not None:
                if ctx.tmpfilename != '-':
                    ctx.stream.close()
        def download():
            data_len = ctx.data.headers.get('Content-length')
            if ctx.data.headers.get('Content-encoding'):
                # Content-encoding is present, Content-length is not reliable anymore as we are
                # doing auto decompression. (See: https://github.com/yt-dlp/yt-dlp/pull/6176)
                data_len = None
            # Range HTTP header may be ignored/unsupported by a webserver
            # (e.g. extractor/scivee.py, extractor/bambuser.py).
            # However, for a test we still would like to download just a piece of a file.
            # To achieve this we limit data_len to _TEST_FILE_SIZE and manually control
            # block size when downloading a file.
            if is_test and (data_len is None or int(data_len) > self._TEST_FILE_SIZE):
                data_len = self._TEST_FILE_SIZE
            if data_len is not None:
                data_len = int(data_len) + ctx.resume_len
                min_data_len = self.params.get('min_filesize')
                max_data_len = self.params.get('max_filesize')
                if min_data_len is not None and data_len < min_data_len:
                    self.to_screen(
                        f'\r[download] File is smaller than min-filesize ({data_len} bytes < {min_data_len} bytes). Aborting.')
                if max_data_len is not None and data_len > max_data_len:
                        f'\r[download] File is larger than max-filesize ({data_len} bytes > {max_data_len} bytes). Aborting.')
            byte_counter = 0 + ctx.resume_len
            block_size = ctx.block_size
            # measure time over whole while-loop, so slow_down() and best_block_size() work together properly
            now = None  # needed for slow_down() in the first loop run
            before = start  # start measuring
            def retry(e):
                close_stream()
                if ctx.tmpfilename == '-':
                    ctx.resume_len = byte_counter
                raise RetryDownload(e)
                    # Download and write
                    data_block = ctx.data.read(block_size if not is_test else min(block_size, data_len - byte_counter))
                    retry(err)
                byte_counter += len(data_block)
                # exit loop when download is finished
                if len(data_block) == 0:
                # Open destination file just in time
                if ctx.stream is None:
                        ctx.stream, ctx.tmpfilename = self.sanitize_open(
                            ctx.tmpfilename, ctx.open_mode)
                        assert ctx.stream is not None
                        ctx.filename = self.undo_temp_name(ctx.tmpfilename)
                        self.report_destination(ctx.filename)
                        self.report_error(f'unable to open for writing: {err}')
                    ctx.stream.write(data_block)
                    self.to_stderr('\n')
                    self.report_error(f'unable to write data: {err}')
                # Apply rate limit
                self.slow_down(start, now, byte_counter - ctx.resume_len)
                # end measuring of one loop run
                after = now
                # Adjust block size
                if not self.params.get('noresizebuffer', False):
                    block_size = self.best_block_size(after - before, len(data_block))
                before = after
                # Progress message
                speed = self.calc_speed(start, now, byte_counter - ctx.resume_len)
                if ctx.data_len is None:
                    eta = None
                    eta = self.calc_eta(start, time.time(), ctx.data_len - ctx.resume_len, byte_counter - ctx.resume_len)
                    'status': 'downloading',
                    'downloaded_bytes': byte_counter,
                    'total_bytes': ctx.data_len,
                    'tmpfilename': ctx.tmpfilename,
                    'eta': eta,
                    'elapsed': now - ctx.start_time,
                    'ctx_id': info_dict.get('ctx_id'),
                if data_len is not None and byte_counter == data_len:
                if speed and speed < (self.params.get('throttledratelimit') or 0):
                    # The speed must stay below the limit for 3 seconds
                    # This prevents raising error when the speed temporarily goes down
                    if ctx.throttle_start is None:
                        ctx.throttle_start = now
                    elif now - ctx.throttle_start > 3:
                        if ctx.stream is not None and ctx.tmpfilename != '-':
                        raise ThrottledDownload
                elif speed:
                    ctx.throttle_start = None
                self.report_error('Did not get any data blocks')
            if not is_test and ctx.chunk_size and ctx.content_len is not None and byte_counter < ctx.content_len:
                raise NextFragment
            if data_len is not None and byte_counter != data_len:
                err = ContentTooShortError(byte_counter, int(data_len))
            # Update file modification time
            if self.params.get('updatetime'):
                info_dict['filetime'] = self.try_utime(ctx.filename, ctx.data.headers.get('last-modified', None))
                'total_bytes': byte_counter,
                'elapsed': time.time() - ctx.start_time,
        for retry in RetryManager(self.params.get('retries'), self.report_retry):
                establish_connection()
                return download()
            except RetryDownload as err:
                retry.error = err.source_error
            except NextFragment:
                retry.error = None
                retry.attempt -= 1
            except SucceedDownload:
            except:  # noqa: E722
from base64 import b64decode
from fastapi.exceptions import HTTPException
from fastapi.openapi.models import HTTPBase as HTTPBaseModel
from fastapi.openapi.models import HTTPBearer as HTTPBearerModel
from fastapi.security.utils import get_authorization_scheme_param
from starlette.requests import Request
from starlette.status import HTTP_401_UNAUTHORIZED
class HTTPBasicCredentials(BaseModel):
    The HTTP Basic credentials given as the result of using `HTTPBasic` in a
    dependency.
    [FastAPI docs for HTTP Basic Auth](https://fastapi.tiangolo.com/advanced/security/http-basic-auth/).
    username: Annotated[str, Doc("The HTTP Basic username.")]
    password: Annotated[str, Doc("The HTTP Basic password.")]
class HTTPAuthorizationCredentials(BaseModel):
    The HTTP authorization credentials in the result of using `HTTPBearer` or
    `HTTPDigest` in a dependency.
    The HTTP authorization header value is split by the first space.
    The first part is the `scheme`, the second part is the `credentials`.
    For example, in an HTTP Bearer token scheme, the client will send a header
    like:
    Authorization: Bearer deadbeef12346
    In this case:
    * `scheme` will have the value `"Bearer"`
    * `credentials` will have the value `"deadbeef12346"`
    scheme: Annotated[
            The HTTP authorization scheme extracted from the header value.
    credentials: Annotated[
            The HTTP authorization credentials extracted from the header value.
    model: HTTPBaseModel
        scheme: str,
        scheme_name: str | None = None,
        auto_error: bool = True,
        self.model = HTTPBaseModel(scheme=scheme, description=description)
        self.scheme_name = scheme_name or self.__class__.__name__
        self.auto_error = auto_error
    def make_authenticate_headers(self) -> dict[str, str]:
        return {"WWW-Authenticate": f"{self.model.scheme.title()}"}
    def make_not_authenticated_error(self) -> HTTPException:
        return HTTPException(
            status_code=HTTP_401_UNAUTHORIZED,
            detail="Not authenticated",
            headers=self.make_authenticate_headers(),
    async def __call__(self, request: Request) -> HTTPAuthorizationCredentials | None:
        authorization = request.headers.get("Authorization")
        scheme, credentials = get_authorization_scheme_param(authorization)
        if not (authorization and scheme and credentials):
            if self.auto_error:
                raise self.make_not_authenticated_error()
        return HTTPAuthorizationCredentials(scheme=scheme, credentials=credentials)
class HTTPBasic(HTTPBase):
    HTTP Basic authentication.
    Ref: https://datatracker.ietf.org/doc/html/rfc7617
    ## Usage
    Create an instance object and use that object as the dependency in `Depends()`.
    The dependency result will be an `HTTPBasicCredentials` object containing the
    `username` and the `password`.
    from fastapi.security import HTTPBasic, HTTPBasicCredentials
    security = HTTPBasic()
    @app.get("/users/me")
    def read_current_user(credentials: Annotated[HTTPBasicCredentials, Depends(security)]):
        return {"username": credentials.username, "password": credentials.password}
        scheme_name: Annotated[
                Security scheme name.
                It will be included in the generated OpenAPI (e.g. visible at `/docs`).
        realm: Annotated[
                HTTP Basic authentication realm.
        description: Annotated[
                Security scheme description.
        auto_error: Annotated[
            bool,
                By default, if the HTTP Basic authentication is not provided (a
                header), `HTTPBasic` will automatically cancel the request and send the
                client an error.
                If `auto_error` is set to `False`, when the HTTP Basic authentication
                is not available, instead of erroring out, the dependency result will
                be `None`.
                This is useful when you want to have optional authentication.
                It is also useful when you want to have authentication that can be
                provided in one of multiple optional ways (for example, in HTTP Basic
                authentication or in an HTTP Bearer token).
        ] = True,
        self.model = HTTPBaseModel(scheme="basic", description=description)
        self.realm = realm
        if self.realm:
            return {"WWW-Authenticate": f'Basic realm="{self.realm}"'}
        return {"WWW-Authenticate": "Basic"}
    async def __call__(  # type: ignore
        self, request: Request
    ) -> HTTPBasicCredentials | None:
        scheme, param = get_authorization_scheme_param(authorization)
        if not authorization or scheme.lower() != "basic":
            data = b64decode(param).decode("ascii")
        except (ValueError, UnicodeDecodeError, binascii.Error) as e:
            raise self.make_not_authenticated_error() from e
        username, separator, password = data.partition(":")
        if not separator:
        return HTTPBasicCredentials(username=username, password=password)
    HTTP Bearer token authentication.
    The dependency result will be an `HTTPAuthorizationCredentials` object containing
    the `scheme` and the `credentials`.
    from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer
    security = HTTPBearer()
    def read_current_user(
        credentials: Annotated[HTTPAuthorizationCredentials, Depends(security)]
        return {"scheme": credentials.scheme, "credentials": credentials.credentials}
        bearerFormat: Annotated[str | None, Doc("Bearer token format.")] = None,
                By default, if the HTTP Bearer token is not provided (in an
                `Authorization` header), `HTTPBearer` will automatically cancel the
                request and send the client an error.
                If `auto_error` is set to `False`, when the HTTP Bearer token
                provided in one of multiple optional ways (for example, in an HTTP
                Bearer token or in a cookie).
        self.model = HTTPBearerModel(bearerFormat=bearerFormat, description=description)
        if scheme.lower() != "bearer":
class HTTPDigest(HTTPBase):
    HTTP Digest authentication.
    **Warning**: this is only a stub to connect the components with OpenAPI in FastAPI,
    but it doesn't implement the full Digest scheme, you would need to subclass it
    and implement it in your code.
    Ref: https://datatracker.ietf.org/doc/html/rfc7616
    from fastapi.security import HTTPAuthorizationCredentials, HTTPDigest
    security = HTTPDigest()
                By default, if the HTTP Digest is not provided, `HTTPDigest` will
                automatically cancel the request and send the client an error.
                If `auto_error` is set to `False`, when the HTTP Digest is not
                available, instead of erroring out, the dependency result will
                provided in one of multiple optional ways (for example, in HTTP
                Digest or in a cookie).
        self.model = HTTPBaseModel(scheme="digest", description=description)
        if scheme.lower() != "digest":
from django.utils.cache import cc_delim_re, get_conditional_response, set_response_etag
class ConditionalGetMiddleware(MiddlewareMixin):
    Handle conditional GET operations. If the response has an ETag or
    Last-Modified header and the request has If-None-Match or
    If-Modified-Since, replace the response with HttpNotModified. Add an ETag
    header if needed.
        # It's too late to prevent an unsafe request with a 412 response, and
        # for a HEAD request, the response body is always empty so computing
        # an accurate ETag isn't possible.
        if request.method != "GET":
        if self.needs_etag(response) and not response.has_header("ETag"):
            set_response_etag(response)
        etag = response.get("ETag")
        last_modified = response.get("Last-Modified")
        last_modified = last_modified and parse_http_date_safe(last_modified)
        if etag or last_modified:
            return get_conditional_response(
                etag=etag,
                last_modified=last_modified,
    def needs_etag(self, response):
        """Return True if an ETag header should be added to response."""
        cache_control_headers = cc_delim_re.split(response.get("Cache-Control", ""))
        return all(header.lower() != "no-store" for header in cache_control_headers)
from binascii import Error as BinasciiError
from datetime import UTC, datetime
from email.utils import formatdate
from urllib.parse import quote, unquote
from urllib.parse import urlencode as original_urlencode
from django.utils.datastructures import MultiValueDict
# Based on RFC 9110 Appendix A.
ETAG_MATCH = _lazy_re_compile(
    \A(      # start of string and capture group
    (?:W/)?  # optional weak indicator
    "        # opening quote
    [^"]*    # any sequence of non-quote characters
    "        # end quote
    )\Z      # end of string and capture group
    re.X,
MAX_HEADER_LENGTH = 10_000
MONTHS = "jan feb mar apr may jun jul aug sep oct nov dec".split()
__D = r"(?P<day>[0-9]{2})"
__D2 = r"(?P<day>[ 0-9][0-9])"
__M = r"(?P<mon>\w{3})"
__Y = r"(?P<year>[0-9]{4})"
__Y2 = r"(?P<year>[0-9]{2})"
__T = r"(?P<hour>[0-9]{2}):(?P<min>[0-9]{2}):(?P<sec>[0-9]{2})"
RFC1123_DATE = _lazy_re_compile(r"^\w{3}, %s %s %s %s GMT$" % (__D, __M, __Y, __T))
RFC850_DATE = _lazy_re_compile(r"^\w{6,9}, %s-%s-%s %s GMT$" % (__D, __M, __Y2, __T))
ASCTIME_DATE = _lazy_re_compile(r"^\w{3} %s %s %s %s$" % (__M, __D2, __T, __Y))
RFC3986_GENDELIMS = ":/?#[]@"
RFC3986_SUBDELIMS = "!$&'()*+,;="
MAX_URL_LENGTH = 2048
MAX_URL_REDIRECT_LENGTH = 16384
def urlencode(query, doseq=False):
    A version of Python's urllib.parse.urlencode() function that can operate on
    MultiValueDict and non-string values.
    if isinstance(query, MultiValueDict):
        query = query.lists()
    elif hasattr(query, "items"):
        query = query.items()
    query_params = []
    for key, value in query:
                "Cannot encode None for key '%s' in a query string. Did you "
                "mean to pass an empty string or omit the value?" % key
        elif not doseq or isinstance(value, (str, bytes)):
            query_val = value
                itr = iter(value)
                # Consume generators and iterators, when doseq=True, to
                # work around https://bugs.python.org/issue31706.
                query_val = []
                for item in itr:
                    if item is None:
                            "Cannot encode None for key '%s' in a query "
                            "string. Did you mean to pass an empty string or "
                            "omit the value?" % key
                    elif not isinstance(item, bytes):
                        item = str(item)
                    query_val.append(item)
        query_params.append((key, query_val))
    return original_urlencode(query_params, doseq)
def http_date(epoch_seconds=None):
    Format the time to match the RFC 5322 date format as specified by RFC 9110
    Section 5.6.7.
    `epoch_seconds` is a floating point number expressed in seconds since the
    epoch, in UTC - such as that outputted by time.time(). If set to None, it
    defaults to the current time.
    Output a string in the format 'Wdy, DD Mon YYYY HH:MM:SS GMT'.
    return formatdate(epoch_seconds, usegmt=True)
def parse_http_date(date):
    Parse a date format as specified by HTTP RFC 9110 Section 5.6.7.
    The three formats allowed by the RFC are accepted, even if only the first
    one is still in widespread use.
    Return an integer expressed in seconds since the epoch, in UTC.
    # email.utils.parsedate() does the job for RFC 1123 dates; unfortunately
    # RFC 9110 makes it mandatory to support RFC 850 dates too. So we roll
    # our own RFC-compliant parsing.
    for regex in RFC1123_DATE, RFC850_DATE, ASCTIME_DATE:
        m = regex.match(date)
        if m is not None:
        raise ValueError("%r is not in a valid HTTP date format" % date)
        year = int(m["year"])
        if year < 100:
            current_year = datetime.now(tz=UTC).year
            current_century = current_year - (current_year % 100)
            if year - (current_year % 100) > 50:
                # year that appears to be more than 50 years in the future are
                # interpreted as representing the past.
                year += current_century - 100
                year += current_century
        month = MONTHS.index(m["mon"].lower()) + 1
        day = int(m["day"])
        hour = int(m["hour"])
        min = int(m["min"])
        sec = int(m["sec"])
        result = datetime(year, month, day, hour, min, sec, tzinfo=UTC)
        return int(result.timestamp())
        raise ValueError("%r is not a valid date" % date) from exc
def parse_http_date_safe(date):
    Same as parse_http_date, but return None if the input is invalid.
        return parse_http_date(date)
# Base 36 functions: useful for generating compact URLs
def base36_to_int(s):
    Convert a base 36 string to an int. Raise ValueError if the input won't fit
    into an int.
    # To prevent overconsumption of server resources, reject any
    # base36 string that is longer than 13 base36 digits (13 digits
    # is sufficient to base36-encode any 64-bit integer)
    if len(s) > 13:
        raise ValueError("Base36 input too large")
    return int(s, 36)
def int_to_base36(i):
    """Convert an integer to a base36 string."""
    char_set = "0123456789abcdefghijklmnopqrstuvwxyz"
        raise ValueError("Negative base36 conversion input.")
    if i < 36:
        return char_set[i]
    b36_parts = []
    while i != 0:
        i, n = divmod(i, 36)
        b36_parts.append(char_set[n])
    return "".join(reversed(b36_parts))
def urlsafe_base64_encode(s):
    Encode a bytestring to a base64 string for use in URLs. Strip any trailing
    equal signs.
    return base64.urlsafe_b64encode(s).rstrip(b"\n=").decode("ascii")
def urlsafe_base64_decode(s):
    Decode a base64 encoded string. Add back any trailing equal signs that
    might have been stripped.
    s = s.encode()
        return base64.urlsafe_b64decode(s.ljust(len(s) + len(s) % 4, b"="))
    except (LookupError, BinasciiError) as e:
        raise ValueError(e)
def parse_etags(etag_str):
    Parse a string of ETags given in an If-None-Match or If-Match header as
    defined by RFC 9110. Return a list of quoted ETags, or ['*'] if all ETags
    should be matched.
    if etag_str.strip() == "*":
        return ["*"]
        # Parse each ETag individually, and return any that are valid.
        etag_matches = (ETAG_MATCH.match(etag.strip()) for etag in etag_str.split(","))
        return [match[1] for match in etag_matches if match]
def quote_etag(etag_str):
    If the provided string is already a quoted ETag, return it. Otherwise, wrap
    the string in quotes, making it a strong ETag.
    if ETAG_MATCH.match(etag_str):
        return etag_str
        return '"%s"' % etag_str
def is_same_domain(host, pattern):
    Return ``True`` if the host is either an exact match or a match
    to the wildcard pattern.
    Any pattern beginning with a period matches a domain and all of its
    subdomains. (e.g. ``.example.com`` matches ``example.com`` and
    ``foo.example.com``). Anything else is an exact string match.
    pattern = pattern.lower()
        pattern[0] == "."
        and (host.endswith(pattern) or host == pattern[1:])
        or pattern == host
def url_has_allowed_host_and_scheme(url, allowed_hosts, require_https=False):
    Return ``True`` if the url uses an allowed host and a safe scheme.
    Always return ``False`` on an empty url.
    If ``require_https`` is ``True``, only 'https' will be considered a valid
    scheme, as opposed to 'http' and 'https' with the default, ``False``.
    Note: "True" doesn't entail that a URL is "safe". It may still be e.g.
    quoted incorrectly. Ensure to also use django.utils.encoding.iri_to_uri()
    on the path component of untrusted URLs.
    if not url:
    if allowed_hosts is None:
        allowed_hosts = set()
    elif isinstance(allowed_hosts, str):
        allowed_hosts = {allowed_hosts}
    # Chrome treats \ completely as / in paths but it could be part of some
    # basic auth credentials so we need to check both URLs.
    return _url_has_allowed_host_and_scheme(
        url, allowed_hosts, require_https=require_https
    ) and _url_has_allowed_host_and_scheme(
        url.replace("\\", "/"), allowed_hosts, require_https=require_https
def _url_has_allowed_host_and_scheme(url, allowed_hosts, require_https=False):
    # Chrome considers any URL with more than two slashes to be absolute, but
    # urlsplit is not so flexible. Treat any url with three slashes as unsafe.
    if url.startswith("///") or len(url) > MAX_URL_LENGTH:
        # urlsplit does not perform validation of inputs. Unicode normalization
        # is very slow on Windows and can be a DoS attack vector.
        # https://docs.python.org/3/library/urllib.parse.html#url-parsing-security
        url_info = urlsplit(url)
    except ValueError:  # e.g. invalid IPv6 addresses
    # Forbid URLs like http:///example.com - with a scheme, but without a
    # hostname. In that URL, example.com is not the hostname but, a path
    # component. However, Chrome will still consider example.com to be the
    # hostname, so we must not allow this syntax.
    if not url_info.netloc and url_info.scheme:
    # Forbid URLs that start with control characters. Some browsers (like
    # Chrome) ignore quite a few control characters at the start of a
    # URL and might consider the URL as scheme relative.
    if unicodedata.category(url[0])[0] == "C":
    scheme = url_info.scheme
    # Consider URLs without a scheme (e.g. //example.com/p) to be http.
    if not url_info.scheme and url_info.netloc:
        scheme = "http"
    valid_schemes = ["https"] if require_https else ["http", "https"]
    return (not url_info.netloc or url_info.netloc in allowed_hosts) and (
        not scheme or scheme in valid_schemes
def escape_leading_slashes(url):
    If redirecting to an absolute path (two leading slashes), a slash must be
    escaped to prevent browsers from handling the path as schemaless and
    redirecting to another host.
    if url.startswith("//"):
        url = "/%2F{}".format(url.removeprefix("//"))
def _parseparam(s):
    while s[:1] == ";":
        s = s[1:]
        end = s.find(";")
        while end > 0 and (s.count('"', 0, end) - s.count('\\"', 0, end)) % 2:
            end = s.find(";", end + 1)
            end = len(s)
        f = s[:end]
        yield f.strip()
        s = s[end:]
def parse_header_parameters(line, max_length=MAX_HEADER_LENGTH):
    Parse a Content-type like header.
    Return the main content-type and a dictionary of options.
    If `line` is longer than `max_length`, `ValueError` is raised.
        return "", {}
    if max_length is not None and len(line) > max_length:
        raise ValueError("Unable to parse header parameters (value too long).")
    parts = _parseparam(";" + line)
    key = parts.__next__().lower()
    pdict = {}
    for p in parts:
        i = p.find("=")
        if i >= 0:
            has_encoding = False
            name = p[:i].strip().lower()
            if name.endswith("*"):
                # Embedded lang/encoding, like "filename*=UTF-8''file.ext".
                # https://tools.ietf.org/html/rfc2231#section-4
                name = name[:-1]
                if p.count("'") == 2:
                    has_encoding = True
            value = p[i + 1 :].strip()
            if len(value) >= 2 and value[0] == value[-1] == '"':
                value = value[1:-1]
                value = value.replace("\\\\", "\\").replace('\\"', '"')
            if has_encoding:
                encoding, lang, value = value.split("'")
                value = unquote(value, encoding=encoding)
            pdict[name] = value
    return key, pdict
def content_disposition_header(as_attachment, filename):
    Construct a Content-Disposition HTTP header value from the given filename
    as specified by RFC 6266.
        disposition = "attachment" if as_attachment else "inline"
            filename.encode("ascii")
            is_ascii = True
            is_ascii = False
        # Quoted strings can contain horizontal tabs, space characters, and
        # characters from 0x21 to 0x7e, except 0x22 (`"`) and 0x5C (`\`) which
        # can still be expressed but must be escaped with their own `\`.
        # https://datatracker.ietf.org/doc/html/rfc9110#name-quoted-strings
        quotable_characters = r"^[\t \x21-\x7e]*$"
        if is_ascii and re.match(quotable_characters, filename):
            file_expr = 'filename="{}"'.format(
                filename.replace("\\", "\\\\").replace('"', r"\"")
            file_expr = "filename*=utf-8''{}".format(quote(filename))
        return f"{disposition}; {file_expr}"
    elif as_attachment:
        return "attachment"
Decorators for views based on HTTP headers.
from django.http import HttpResponseNotAllowed
from django.middleware.http import ConditionalGetMiddleware
from django.utils.cache import get_conditional_response
from django.utils.decorators import decorator_from_middleware
from django.utils.http import http_date, quote_etag
conditional_page = decorator_from_middleware(ConditionalGetMiddleware)
def require_http_methods(request_method_list):
    Decorator to make a view only accept particular request methods. Usage::
        @require_http_methods(["GET", "POST"])
        def my_view(request):
            # I can assume now that only GET or POST requests make it this far
    Note that request methods should be in uppercase.
            async def inner(request, *args, **kwargs):
                if request.method not in request_method_list:
                    response = HttpResponseNotAllowed(request_method_list)
                        "Method Not Allowed (%s): %s",
                return await func(request, *args, **kwargs)
            def inner(request, *args, **kwargs):
                return func(request, *args, **kwargs)
require_GET = require_http_methods(["GET"])
require_GET.__doc__ = "Decorator to require that a view only accepts the GET method."
require_POST = require_http_methods(["POST"])
require_POST.__doc__ = "Decorator to require that a view only accepts the POST method."
require_safe = require_http_methods(["GET", "HEAD"])
require_safe.__doc__ = (
    "Decorator to require that a view only accepts safe methods: GET and HEAD."
def condition(etag_func=None, last_modified_func=None):
    Decorator to support conditional retrieval (or change) for a view
    The parameters are callables to compute the ETag and last modified time for
    the requested resource, respectively. The callables are passed the same
    parameters as the view itself. The ETag function should return a string (or
    None if the resource doesn't exist), while the last_modified function
    should return a datetime object (or None if the resource doesn't exist).
    The ETag function should return a complete ETag, including quotes (e.g.
    '"etag"'), since that's the only way to distinguish between weak and strong
    ETags. If an unquoted ETag is returned (e.g. 'etag'), it will be converted
    to a strong ETag by adding quotes.
    This decorator will either pass control to the wrapped view function or
    return an HTTP 304 response (unmodified) or 412 response (precondition
    failed), depending upon the request method. In either case, the decorator
    will add the generated ETag and Last-Modified headers to the response if
    the headers aren't already set and if the request's method is safe.
        def _pre_process_request(request, *args, **kwargs):
            # Compute values (if any) for the requested resource.
            res_last_modified = None
            if last_modified_func:
                if dt := last_modified_func(request, *args, **kwargs):
                    if not timezone.is_aware(dt):
                        dt = timezone.make_aware(dt, datetime.UTC)
                    res_last_modified = int(dt.timestamp())
            # The value from etag_func() could be quoted or unquoted.
            res_etag = etag_func(request, *args, **kwargs) if etag_func else None
            res_etag = quote_etag(res_etag) if res_etag is not None else None
            response = get_conditional_response(
                etag=res_etag,
                last_modified=res_last_modified,
            return response, res_etag, res_last_modified
        def _post_process_request(request, response, res_etag, res_last_modified):
            # Set relevant headers on the response if they don't already exist
            # and if the request method is safe.
                if res_last_modified and not response.has_header("Last-Modified"):
                    response.headers["Last-Modified"] = http_date(res_last_modified)
                if res_etag:
                    response.headers.setdefault("ETag", res_etag)
                response, res_etag, res_last_modified = _pre_process_request(
                    request, *args, **kwargs
                    response = await func(request, *args, **kwargs)
                _post_process_request(request, response, res_etag, res_last_modified)
                    response = func(request, *args, **kwargs)
# Shortcut decorators for common cases based on ETag or Last-Modified only
def etag(etag_func):
    return condition(etag_func=etag_func)
def last_modified(last_modified_func):
    return condition(last_modified_func=last_modified_func)
from django.conf.urls.i18n import i18n_patterns
from django.contrib.sitemaps import GenericSitemap, Sitemap, views
from django.views.decorators.cache import cache_page
from ..models import I18nTestModel, TestModel
class SimpleSitemap(Sitemap):
    changefreq = "never"
    priority = 0.5
    location = "/location/"
    lastmod = date.today()
        return [object()]
class SimplePagedSitemap(Sitemap):
        return [object() for x in range(Sitemap.limit + 1)]
class SimpleI18nSitemap(Sitemap):
    i18n = True
        return I18nTestModel.objects.order_by("pk").all()
class AlternatesI18nSitemap(SimpleI18nSitemap):
    alternates = True
class LimitedI18nSitemap(AlternatesI18nSitemap):
    languages = ["en", "es"]
class XDefaultI18nSitemap(AlternatesI18nSitemap):
    x_default = True
class ItemByLangSitemap(SimpleI18nSitemap):
        if item.name == "Only for PT":
            return ["pt"]
        return super().get_languages_for_item(item)
class ItemByLangAlternatesSitemap(AlternatesI18nSitemap):
class EmptySitemap(Sitemap):
class FixedLastmodSitemap(SimpleSitemap):
    lastmod = datetime(2013, 3, 13, 10, 0, 0)
class FixedLastmodMixedSitemap(Sitemap):
    loop = 0
        o1 = TestModel()
        o1.lastmod = datetime(2013, 3, 13, 10, 0, 0)
        o2 = TestModel()
        return [o1, o2]
class FixedNewerLastmodSitemap(SimpleSitemap):
    lastmod = datetime(2013, 4, 20, 5, 0, 0)
class DateSiteMap(SimpleSitemap):
    lastmod = date(2013, 3, 13)
class TimezoneSiteMap(SimpleSitemap):
    lastmod = datetime(2013, 3, 13, 10, 0, 0, tzinfo=timezone.get_fixed_timezone(-300))
class CallableLastmodPartialSitemap(Sitemap):
    """Not all items have `lastmod`."""
    def lastmod(self, obj):
        return obj.lastmod
class CallableLastmodFullSitemap(Sitemap):
    """All items have `lastmod`."""
        o2.lastmod = datetime(2014, 3, 13, 10, 0, 0)
class CallableLastmodNoItemsSitemap(Sitemap):
class GetLatestLastmodNoneSiteMap(Sitemap):
        return datetime(2013, 3, 13, 10, 0, 0)
class GetLatestLastmodSiteMap(SimpleSitemap):
def testmodelview(request, id):
simple_sitemaps = {
    "simple": SimpleSitemap,
simple_i18n_sitemaps = {
    "i18n": SimpleI18nSitemap,
alternates_i18n_sitemaps = {
    "i18n-alternates": AlternatesI18nSitemap,
limited_i18n_sitemaps = {
    "i18n-limited": LimitedI18nSitemap,
xdefault_i18n_sitemaps = {
    "i18n-xdefault": XDefaultI18nSitemap,
item_by_lang_i18n_sitemaps = {
    "i18n-item-by-lang": ItemByLangSitemap,
item_by_lang_alternates_i18n_sitemaps = {
    "i18n-item-by-lang-alternates": ItemByLangAlternatesSitemap,
simple_sitemaps_not_callable = {
    "simple": SimpleSitemap(),
simple_sitemaps_paged = {
    "simple": SimplePagedSitemap,
empty_sitemaps = {
    "empty": EmptySitemap,
fixed_lastmod_sitemaps = {
    "fixed-lastmod": FixedLastmodSitemap,
fixed_lastmod_mixed_sitemaps = {
    "fixed-lastmod-mixed": FixedLastmodMixedSitemap,
sitemaps_lastmod_mixed_ascending = {
    "no-lastmod": EmptySitemap,
    "lastmod": FixedLastmodSitemap,
sitemaps_lastmod_mixed_descending = {
sitemaps_lastmod_ascending = {
    "date": DateSiteMap,
    "datetime": FixedLastmodSitemap,
    "datetime-newer": FixedNewerLastmodSitemap,
sitemaps_lastmod_descending = {
generic_sitemaps = {
    "generic": GenericSitemap({"queryset": TestModel.objects.order_by("pk").all()}),
get_latest_lastmod_none_sitemaps = {
    "get-latest-lastmod-none": GetLatestLastmodNoneSiteMap,
get_latest_lastmod_sitemaps = {
    "get-latest-lastmod": GetLatestLastmodSiteMap,
latest_lastmod_timezone_sitemaps = {
    "latest-lastmod-timezone": TimezoneSiteMap,
generic_sitemaps_lastmod = {
    "generic": GenericSitemap(
            "queryset": TestModel.objects.order_by("pk").all(),
            "date_field": "lastmod",
callable_lastmod_partial_sitemap = {
    "callable-lastmod": CallableLastmodPartialSitemap,
callable_lastmod_full_sitemap = {
    "callable-lastmod": CallableLastmodFullSitemap,
callable_lastmod_no_items_sitemap = {
    "callable-lastmod": CallableLastmodNoItemsSitemap,
urlpatterns = [
    path("simple/index.xml", views.index, {"sitemaps": simple_sitemaps}),
    path("simple-paged/index.xml", views.index, {"sitemaps": simple_sitemaps_paged}),
        "simple-not-callable/index.xml",
        views.index,
        {"sitemaps": simple_sitemaps_not_callable},
        "simple/custom-lastmod-index.xml",
            "sitemaps": simple_sitemaps,
            "template_name": "custom_sitemap_lastmod_index.xml",
        "simple/sitemap-<section>.xml",
        views.sitemap,
        {"sitemaps": simple_sitemaps},
        name="django.contrib.sitemaps.views.sitemap",
        "simple/sitemap.xml",
        "simple/i18n.xml",
        {"sitemaps": simple_i18n_sitemaps},
        "alternates/i18n.xml",
        {"sitemaps": alternates_i18n_sitemaps},
        "limited/i18n.xml",
        {"sitemaps": limited_i18n_sitemaps},
        "x-default/i18n.xml",
        {"sitemaps": xdefault_i18n_sitemaps},
        "simple/custom-sitemap.xml",
        {"sitemaps": simple_sitemaps, "template_name": "custom_sitemap.xml"},
        "empty/sitemap.xml",
        {"sitemaps": empty_sitemaps},
        "lastmod/sitemap.xml",
        {"sitemaps": fixed_lastmod_sitemaps},
        "lastmod-mixed/sitemap.xml",
        {"sitemaps": fixed_lastmod_mixed_sitemaps},
        "lastmod/date-sitemap.xml",
        {"sitemaps": {"date-sitemap": DateSiteMap}},
        "lastmod/tz-sitemap.xml",
        {"sitemaps": {"tz-sitemap": TimezoneSiteMap}},
        "lastmod-sitemaps/mixed-ascending.xml",
        {"sitemaps": sitemaps_lastmod_mixed_ascending},
        "lastmod-sitemaps/mixed-descending.xml",
        {"sitemaps": sitemaps_lastmod_mixed_descending},
        "lastmod-sitemaps/ascending.xml",
        {"sitemaps": sitemaps_lastmod_ascending},
        "item-by-lang/i18n.xml",
        {"sitemaps": item_by_lang_i18n_sitemaps},
        "item-by-lang-alternates/i18n.xml",
        {"sitemaps": item_by_lang_alternates_i18n_sitemaps},
        "lastmod-sitemaps/descending.xml",
        {"sitemaps": sitemaps_lastmod_descending},
        "lastmod/get-latest-lastmod-none-sitemap.xml",
        {"sitemaps": get_latest_lastmod_none_sitemaps},
        name="django.contrib.sitemaps.views.index",
        "lastmod/get-latest-lastmod-sitemap.xml",
        {"sitemaps": get_latest_lastmod_sitemaps},
        "lastmod/latest-lastmod-timezone-sitemap.xml",
        {"sitemaps": latest_lastmod_timezone_sitemaps},
        "generic/sitemap.xml",
        {"sitemaps": generic_sitemaps},
        "generic-lastmod/sitemap.xml",
        {"sitemaps": generic_sitemaps_lastmod},
        "cached/index.xml",
        cache_page(1)(views.index),
        {"sitemaps": simple_sitemaps, "sitemap_url_name": "cached_sitemap"},
        "cached/sitemap-<section>.xml",
        cache_page(1)(views.sitemap),
        name="cached_sitemap",
        "sitemap-without-entries/sitemap.xml",
        {"sitemaps": {}},
        "callable-lastmod-partial/index.xml",
        {"sitemaps": callable_lastmod_partial_sitemap},
        "callable-lastmod-partial/sitemap.xml",
        "callable-lastmod-full/index.xml",
        {"sitemaps": callable_lastmod_full_sitemap},
        "callable-lastmod-full/sitemap.xml",
        "callable-lastmod-no-items/index.xml",
        {"sitemaps": callable_lastmod_no_items_sitemap},
        "generic-lastmod/index.xml",
urlpatterns += i18n_patterns(
    path("i18n/testmodel/<int:id>/", testmodelview, name="i18n_testmodel"),
import yarl
from fsspec.asyn import AbstractAsyncStreamedFile, AsyncFileSystem, sync, sync_wrapper
from fsspec.callbacks import DEFAULT_CALLBACK
from fsspec.exceptions import FSTimeoutError
from fsspec.spec import AbstractBufferedFile
from fsspec.utils import (
    DEFAULT_BLOCK_SIZE,
    glob_translate,
    isfilelike,
    nullcontext,
    tokenize,
from ..caching import AllBytes
# https://stackoverflow.com/a/15926317/3821154
ex = re.compile(r"""<(a|A)\s+(?:[^>]*?\s+)?(href|HREF)=["'](?P<url>[^"']+)""")
ex2 = re.compile(r"""(?P<url>http[s]?://[-a-zA-Z0-9@:%_+.~#?&/=]+)""")
logger = logging.getLogger("fsspec.http")
async def get_client(**kwargs):
    return aiohttp.ClientSession(**kwargs)
class HTTPFileSystem(AsyncFileSystem):
    Simple File-System for fetching data via HTTP(S)
    ``ls()`` is implemented by loading the parent page and doing a regex
    match on the result. If simple_link=True, anything of the form
    "http(s)://server.com/stuff?thing=other"; otherwise only links within
    HTML href tags will be used.
    protocol = ("http", "https")
    sep = "/"
        simple_links=True,
        same_scheme=True,
        size_policy=None,
        cache_type="bytes",
        asynchronous=False,
        loop=None,
        client_kwargs=None,
        get_client=get_client,
        encoded=False,
        **storage_options,
        NB: if this is called async, you must await set_client
        block_size: int
            Blocks to read bytes; if 0, will default to raw requests file-like
            objects instead of HTTPFile instances
        simple_links: bool
            If True, will consider both HTML <a> tags and anything that looks
            like a URL; if False, will consider only the former.
        same_scheme: True
            When doing ls/glob, if this is True, only consider paths that have
            http/https matching the input URLs.
        size_policy: this argument is deprecated
        client_kwargs: dict
            Passed to aiohttp.ClientSession, see
            https://docs.aiohttp.org/en/stable/client_reference.html
            For example, ``{'auth': aiohttp.BasicAuth('user', 'pass')}``
        get_client: Callable[..., aiohttp.ClientSession]
            A callable, which takes keyword arguments and constructs
            an aiohttp.ClientSession. Its state will be managed by
            the HTTPFileSystem class.
        storage_options: key-value
            Any other parameters passed on to requests
        cache_type, cache_options: defaults used in open()
        super().__init__(self, asynchronous=asynchronous, loop=loop, **storage_options)
        self.block_size = block_size if block_size is not None else DEFAULT_BLOCK_SIZE
        self.simple_links = simple_links
        self.same_schema = same_scheme
        self.cache_type = cache_type
        self.cache_options = cache_options
        self.client_kwargs = client_kwargs or {}
        self.get_client = get_client
        self.encoded = encoded
        self.kwargs = storage_options
        # Clean caching-related parameters from `storage_options`
        # before propagating them as `request_options` through `self.kwargs`.
        # TODO: Maybe rename `self.kwargs` to `self.request_options` to make
        #       it clearer.
        request_options = copy(storage_options)
        self.use_listings_cache = request_options.pop("use_listings_cache", False)
        request_options.pop("listings_expiry_time", None)
        request_options.pop("max_paths", None)
        request_options.pop("skip_instance_cache", None)
        self.kwargs = request_options
    def fsid(self):
        return "http"
    def encode_url(self, url):
        return yarl.URL(url, encoded=self.encoded)
    def close_session(loop, session):
        if loop is not None and loop.is_running():
                sync(loop, session.close, timeout=0.1)
            except (TimeoutError, FSTimeoutError, NotImplementedError):
        connector = getattr(session, "_connector", None)
        if connector is not None:
            # close after loop is dead
            connector._close()
    async def set_session(self):
            self._session = await self.get_client(loop=self.loop, **self.client_kwargs)
            if not self.asynchronous:
                weakref.finalize(self, self.close_session, self.loop, self._session)
        return self._session
        """For HTTP, we always want to keep the full URL"""
    def _parent(cls, path):
        # override, since _strip_protocol is different for URLs
        par = super()._parent(path)
        if len(par) > 7:  # "http://..."
            return par
    async def _ls_real(self, url, detail=True, **kwargs):
        # ignoring URL-encoded arguments
        kw = self.kwargs.copy()
        kw.update(kwargs)
        logger.debug(url)
        session = await self.set_session()
        async with session.get(self.encode_url(url), **self.kwargs) as r:
            self._raise_not_found_for_status(r, url)
            if "Content-Type" in r.headers:
                mimetype = r.headers["Content-Type"].partition(";")[0]
                mimetype = None
            if mimetype in ("text/html", None):
                    text = await r.text(errors="ignore")
                    if self.simple_links:
                        links = ex2.findall(text) + [u[2] for u in ex.findall(text)]
                        links = [u[2] for u in ex.findall(text)]
                    links = []  # binary, not HTML
        out = set()
        parts = urlparse(url)
        for l in links:
            if isinstance(l, tuple):
                l = l[1]
            if l.startswith("/") and len(l) > 1:
                # absolute URL on this server
                l = f"{parts.scheme}://{parts.netloc}{l}"
            if l.startswith("http"):
                if self.same_schema and l.startswith(url.rstrip("/") + "/"):
                    out.add(l)
                elif l.replace("https", "http").startswith(
                    url.replace("https", "http").rstrip("/") + "/"
                    # allowed to cross http <-> https
                if l not in ["..", "../"]:
                    # Ignore FTP-like "parent"
                    out.add("/".join([url.rstrip("/"), l.lstrip("/")]))
        if not out and url.endswith("/"):
            out = await self._ls_real(url.rstrip("/"), detail=False)
                    "name": u,
                    "size": None,
                    "type": "directory" if u.endswith("/") else "file",
                for u in out
            return sorted(out)
    async def _ls(self, url, detail=True, **kwargs):
        if self.use_listings_cache and url in self.dircache:
            out = self.dircache[url]
            out = await self._ls_real(url, detail=detail, **kwargs)
            self.dircache[url] = out
    ls = sync_wrapper(_ls)
    def _raise_not_found_for_status(self, response, url):
        Raises FileNotFoundError for 404s, otherwise uses raise_for_status.
            raise FileNotFoundError(url)
    async def _cat_file(self, url, start=None, end=None, **kwargs):
        if start is not None or end is not None:
            if start == end:
            headers = kw.pop("headers", {}).copy()
            headers["Range"] = await self._process_limits(url, start, end)
            kw["headers"] = headers
        async with session.get(self.encode_url(url), **kw) as r:
            out = await r.read()
    async def _get_file(
        self, rpath, lpath, chunk_size=5 * 2**20, callback=DEFAULT_CALLBACK, **kwargs
        logger.debug(rpath)
        async with session.get(self.encode_url(rpath), **kw) as r:
                size = int(r.headers["content-length"])
                size = None
            callback.set_size(size)
            self._raise_not_found_for_status(r, rpath)
            if isfilelike(lpath):
                outfile = lpath
                outfile = open(lpath, "wb")  # noqa: ASYNC230
                chunk = True
                while chunk:
                    chunk = await r.content.read(chunk_size)
                    outfile.write(chunk)
                    callback.relative_update(len(chunk))
                if not isfilelike(lpath):
                    outfile.close()
    async def _put_file(
        lpath,
        rpath,
        chunk_size=5 * 2**20,
        callback=DEFAULT_CALLBACK,
        mode="overwrite",
        if mode != "overwrite":
            raise NotImplementedError("Exclusive write")
        async def gen_chunks():
            # Support passing arbitrary file-like objects
            # and use them instead of streams.
            if isinstance(lpath, io.IOBase):
                context = nullcontext(lpath)
                use_seek = False  # might not support seeking
                context = open(lpath, "rb")  # noqa: ASYNC230
                use_seek = True
            with context as f:
                if use_seek:
                    callback.set_size(f.seek(0, 2))
                    callback.set_size(getattr(f, "size", None))
                chunk = f.read(chunk_size)
        method = method.lower()
        if method not in ("post", "put"):
                f"method has to be either 'post' or 'put', not: {method!r}"
        meth = getattr(session, method)
        async with meth(self.encode_url(rpath), data=gen_chunks(), **kw) as resp:
            self._raise_not_found_for_status(resp, rpath)
    async def _exists(self, path, strict=False, **kwargs):
            logger.debug(path)
            r = await session.get(self.encode_url(path), **kw)
            async with r:
                    self._raise_not_found_for_status(r, path)
                return r.status < 400
        except aiohttp.ClientError:
    async def _isfile(self, path, **kwargs):
        return await self._exists(path, **kwargs)
        autocommit=None,  # XXX: This differs from the base class.
        cache_type=None,
        """Make a file-like object
            Full URL with protocol
        mode: string
            must be "rb"
        block_size: int or None
            Bytes to download in one request; use instance value if None. If
            zero, will return a streaming Requests file-like instance.
        kwargs: key-value
            Any other parameters, passed to requests calls
        if mode != "rb":
        block_size = block_size if block_size is not None else self.block_size
        kw["asynchronous"] = self.asynchronous
        size = size or info.update(self.info(path, **kwargs)) or info["size"]
        session = sync(self.loop, self.set_session)
        if block_size and size and info.get("partial", True):
            return HTTPFile(
                session=session,
                block_size=block_size,
                cache_type=cache_type or self.cache_type,
                cache_options=cache_options or self.cache_options,
                loop=self.loop,
            return HTTPStreamFile(
    async def open_async(self, path, mode="rb", size=None, **kwargs):
        if size is None:
                size = (await self._info(path, **kwargs))["size"]
        return AsyncStreamFile(
    def ukey(self, url):
        """Unique identifier; assume HTTP files are static, unchanging"""
        return tokenize(url, self.kwargs, self.protocol)
    async def _info(self, url, **kwargs):
        """Get info of URL
        Tries to access location via HEAD, and then GET methods, but does
        not fetch the data.
        It is possible that the server does not supply any size information, in
        which case size will be given as None (and certain operations on the
        corresponding file will not work).
        for policy in ["head", "get"]:
                info.update(
                    await _file_info(
                        self.encode_url(url),
                        size_policy=policy,
                        **self.kwargs,
                if info.get("size") is not None:
                if policy == "get":
                    # If get failed, then raise a FileNotFoundError
                    raise FileNotFoundError(url) from exc
                logger.debug("", exc_info=exc)
        return {"name": url, "size": None, **info, "type": "file"}
    async def _glob(self, path, maxdepth=None, **kwargs):
        Find files by glob-matching.
        This implementation is idntical to the one in AbstractFileSystem,
        but "?" is not considered as a character for globbing, because it is
        so common in URLs, often identifying the "query" part.
        if maxdepth is not None and maxdepth < 1:
            raise ValueError("maxdepth must be at least 1")
        ends_with_slash = path.endswith("/")  # _strip_protocol strips trailing slash
        path = self._strip_protocol(path)
        append_slash_to_dirname = ends_with_slash or path.endswith(("/**", "/*"))
        idx_star = path.find("*") if path.find("*") >= 0 else len(path)
        idx_brace = path.find("[") if path.find("[") >= 0 else len(path)
        min_idx = min(idx_star, idx_brace)
        detail = kwargs.pop("detail", False)
        if not has_magic(path):
            if await self._exists(path, **kwargs):
                if not detail:
                    return [path]
                    return {path: await self._info(path, **kwargs)}
                    return []  # glob of non-existent returns empty
        elif "/" in path[:min_idx]:
            min_idx = path[:min_idx].rindex("/")
            root = path[: min_idx + 1]
            depth = path[min_idx + 1 :].count("/") + 1
            root = ""
        if "**" in path:
            if maxdepth is not None:
                idx_double_stars = path.find("**")
                depth_double_stars = path[idx_double_stars:].count("/") + 1
                depth = depth - depth_double_stars + maxdepth
                depth = None
        allpaths = await self._find(
            root, maxdepth=depth, withdirs=True, detail=True, **kwargs
        pattern = glob_translate(path + ("/" if ends_with_slash else ""))
        out = {
                p.rstrip("/")
                if not append_slash_to_dirname
                and info["type"] == "directory"
                and p.endswith("/")
                else p
            ): info
            for p, info in sorted(allpaths.items())
            if pattern.match(p.rstrip("/"))
            return list(out)
    async def _isdir(self, path):
        # override, since all URLs are (also) files
            return bool(await self._ls(path))
    async def _pipe_file(self, path, value, mode="overwrite", **kwargs):
        Write bytes to a remote file over HTTP.
        path : str
            Target URL where the data should be written
        value : bytes
            Data to be written
        mode : str
            How to write to the file - 'overwrite' or 'append'
        **kwargs : dict
            Additional parameters to pass to the HTTP request
        url = self._strip_protocol(path)
        headers = kwargs.pop("headers", {})
        headers["Content-Length"] = str(len(value))
        async with session.put(url, data=value, headers=headers, **kwargs) as r:
            r.raise_for_status()
class HTTPFile(AbstractBufferedFile):
    A file-like object pointing to a remote HTTP(S) resource
    Supports only reading, with read-ahead of a predetermined block-size.
    In the case that the server does not supply the filesize, only reading of
    the complete file in one go is supported.
        Full URL of the remote resource, including the protocol
    session: aiohttp.ClientSession or None
        All calls will be made within this session, to avoid restarting
        connections where the server allows this
        The amount of read-ahead to do, in bytes. Default is 5MB, or the value
        configured for the FileSystem creating this file
    size: None or int
        If given, this is the size of the file in bytes, and we don't attempt
        to call the server to find the value.
    kwargs: all other key-values are passed to requests calls.
        fs,
            raise NotImplementedError("File mode not supported")
        self.asynchronous = asynchronous
        self.loop = loop
        self.details = {"name": url, "size": size, "type": "file"}
            fs=fs,
            path=url,
            cache_type=cache_type,
            cache_options=cache_options,
    def read(self, length=-1):
        """Read bytes from file
            Read up to this many bytes. If negative, read all content to end of
            file. If the server has not supplied the filesize, attempting to
            read only part of the data will raise a ValueError.
            (length < 0 and self.loc == 0)  # explicit read all
            # but not when the size is known and fits into a block anyways
            and not (self.size is not None and self.size <= self.blocksize)
        if self.size is None:
            length = min(self.size - self.loc, length)
        return super().read(length)
    async def async_fetch_all(self):
        """Read whole file in one shot, without caching
        This is only called when position is still at zero,
        and read() is called without a byte-count.
        logger.debug(f"Fetch all for {self}")
        if not isinstance(self.cache, AllBytes):
            r = await self.session.get(self.fs.encode_url(self.url), **self.kwargs)
                self.cache = AllBytes(
                    size=len(out), fetcher=None, blocksize=None, data=out
                self.size = len(out)
    _fetch_all = sync_wrapper(async_fetch_all)
    def _parse_content_range(self, headers):
        """Parse the Content-Range header"""
        s = headers.get("Content-Range", "")
        m = re.match(r"bytes (\d+-\d+|\*)/(\d+|\*)", s)
        if m[1] == "*":
            start = end = None
            start, end = [int(x) for x in m[1].split("-")]
        total = None if m[2] == "*" else int(m[2])
        return start, end, total
    async def async_fetch_range(self, start, end):
        """Download a block of data
        The expectation is that the server returns only the requested bytes,
        with HTTP code 206. If this is not the case, we first check the headers,
        and then stream the output - if the data size is bigger than we
        requested, an exception is raised.
        logger.debug(f"Fetch range for {self}: {start}-{end}")
        headers = kwargs.pop("headers", {}).copy()
        headers["Range"] = f"bytes={start}-{end - 1}"
        logger.debug(f"{self.url} : {headers['Range']}")
        r = await self.session.get(
            self.fs.encode_url(self.url), headers=headers, **kwargs
            if r.status == 416:
                # range request outside file
            # If the server has handled the range request, it should reply
            # with status 206 (partial content). But we'll guess that a suitable
            # Content-Range header or a Content-Length no more than the
            # requested range also mean we have got the desired range.
            response_is_range = (
                r.status == 206
                or self._parse_content_range(r.headers)[0] == start
                or int(r.headers.get("Content-Length", end + 1)) <= end - start
            if response_is_range:
                # partial content, as expected
            elif start > 0:
                    "The HTTP server doesn't appear to support range requests. "
                    "Only reading this file from the beginning is supported. "
                    "Open with block_size=0 for a streaming file interface."
                # Response is not a range, but we want the start of the file,
                # so we can read the required amount anyway.
                cl = 0
                    chunk = await r.content.read(2**20)
                    # data size unknown, let's read until we have enough
                        out.append(chunk)
                        cl += len(chunk)
                        if cl > end - start:
                out = b"".join(out)[: end - start]
    _fetch_range = sync_wrapper(async_fetch_range)
magic_check = re.compile("([*[])")
def has_magic(s):
    match = magic_check.search(s)
    return match is not None
class HTTPStreamFile(AbstractBufferedFile):
    def __init__(self, fs, url, mode="rb", loop=None, session=None, **kwargs):
        self.asynchronous = kwargs.pop("asynchronous", False)
        self.details = {"name": url, "size": None}
        super().__init__(fs=fs, path=url, mode=mode, cache_type="none", **kwargs)
        async def cor():
            r = await self.session.get(self.fs.encode_url(url), **kwargs).__aenter__()
            self.fs._raise_not_found_for_status(r, url)
        self.r = sync(self.loop, cor)
        self.loop = fs.loop
    def seek(self, loc, whence=0):
        if loc == 0 and whence == 1:
        if loc == self.loc and whence == 0:
        raise ValueError("Cannot seek streaming HTTP file")
    async def _read(self, num=-1):
        out = await self.r.content.read(num)
        self.loc += len(out)
    read = sync_wrapper(_read)
    async def _close(self):
        self.r.close()
        asyncio.run_coroutine_threadsafe(self._close(), self.loop)
        super().close()
class AsyncStreamFile(AbstractAsyncStreamedFile):
        self, fs, url, mode="rb", loop=None, session=None, size=None, **kwargs
        self.r = None
        super().__init__(fs=fs, path=url, mode=mode, cache_type="none")
    async def read(self, num=-1):
        if self.r is None:
                self.fs.encode_url(self.url), **self.kwargs
            self.fs._raise_not_found_for_status(r, self.url)
            self.r = r
    async def close(self):
        if self.r is not None:
        await super().close()
async def get_range(session, url, start, end, file=None, **kwargs):
    # explicit get a range when we know it must be safe
    kwargs = kwargs.copy()
    r = await session.get(url, headers=headers, **kwargs)
        with open(file, "r+b") as f:  # noqa: ASYNC230
            f.seek(start)
            f.write(out)
async def _file_info(url, session, size_policy="head", **kwargs):
    """Call HEAD on the server to get details about the file (size/checksum etc.)
    Default operation is to explicitly allow redirects and use encoding
    'identity' (no compression) to get the true size of the target.
    logger.debug("Retrieve file size for %s", url)
    ar = kwargs.pop("allow_redirects", True)
    head = kwargs.get("headers", {}).copy()
    head["Accept-Encoding"] = "identity"
    kwargs["headers"] = head
    if size_policy == "head":
        r = await session.head(url, allow_redirects=ar, **kwargs)
    elif size_policy == "get":
        r = await session.get(url, allow_redirects=ar, **kwargs)
        raise TypeError(f'size_policy must be "head" or "get", got {size_policy}')
        if "Content-Length" in r.headers:
            # Some servers may choose to ignore Accept-Encoding and return
            # compressed content, in which case the returned size is unreliable.
            if "Content-Encoding" not in r.headers or r.headers["Content-Encoding"] in [
                "identity",
                info["size"] = int(r.headers["Content-Length"])
        elif "Content-Range" in r.headers:
            info["size"] = int(r.headers["Content-Range"].split("/")[1])
            info["mimetype"] = r.headers["Content-Type"].partition(";")[0]
        if r.headers.get("Accept-Ranges") == "none":
            # Some servers may explicitly discourage partial content requests, but
            # the lack of "Accept-Ranges" does not always indicate they would fail
            info["partial"] = False
        info["url"] = str(r.url)
        for checksum_field in ["ETag", "Content-MD5", "Digest", "Last-Modified"]:
            if r.headers.get(checksum_field):
                info[checksum_field] = r.headers[checksum_field]
async def _file_size(url, session=None, *args, **kwargs):
        session = await get_client()
    info = await _file_info(url, session=session, *args, **kwargs)
    return info.get("size")
file_size = sync_wrapper(_file_size)
from typing import Mapping, Tuple
from .http_exceptions import HttpProcessingError as HttpProcessingError
from .http_parser import (
    HeadersParser as HeadersParser,
    HttpParser as HttpParser,
    HttpRequestParser as HttpRequestParser,
    HttpResponseParser as HttpResponseParser,
    RawRequestMessage as RawRequestMessage,
    RawResponseMessage as RawResponseMessage,
from .http_websocket import (
    WS_CLOSED_MESSAGE as WS_CLOSED_MESSAGE,
    WS_CLOSING_MESSAGE as WS_CLOSING_MESSAGE,
    WS_KEY as WS_KEY,
    WebSocketReader as WebSocketReader,
    WebSocketWriter as WebSocketWriter,
    ws_ext_gen as ws_ext_gen,
    ws_ext_parse as ws_ext_parse,
from .http_writer import (
    StreamWriter as StreamWriter,
    "HttpProcessingError",
    "RESPONSES",
    "SERVER_SOFTWARE",
    # .http_writer
    "StreamWriter",
    # .http_parser
    "HeadersParser",
    "HttpParser",
    "HttpRequestParser",
    "HttpResponseParser",
    "RawRequestMessage",
    "RawResponseMessage",
    # .http_websocket
    "WS_CLOSED_MESSAGE",
    "WS_CLOSING_MESSAGE",
    "WS_KEY",
    "WebSocketReader",
    "WebSocketWriter",
    "ws_ext_gen",
    "ws_ext_parse",
SERVER_SOFTWARE: str = "Python/{0[0]}.{0[1]} aiohttp/{1}".format(
    sys.version_info, __version__
RESPONSES: Mapping[int, Tuple[str, str]] = {
    v: (v.phrase, v.description) for v in HTTPStatus.__members__.values()
from typing import Annotated, Optional
        scheme_name: Optional[str] = None,
        self.model: HTTPBaseModel = HTTPBaseModel(
            scheme=scheme, description=description
    async def __call__(
    ) -> Optional[HTTPAuthorizationCredentials]:
    ) -> Optional[HTTPBasicCredentials]:
        bearerFormat: Annotated[Optional[str], Doc("Bearer token format.")] = None,
    but it doesn't implement the full Digest scheme, you would need to to subclass it
from ...http_client import HTTPClient
@click.group()
def http():
    """Make HTTP requests to the LiteLLM proxy server"""
@http.command()
@click.argument("method")
@click.argument("uri")
    help="Data to send in the request body (as JSON string)",
    help="JSON data to send in the request body (as JSON string)",
    "--header",
    help="HTTP headers in 'key:value' format. Can be specified multiple times.",
    uri: str,
    data: Optional[str] = None,
    header: tuple[str, ...] = (),
    """Make an HTTP request to the LiteLLM proxy server
    METHOD: HTTP method (GET, POST, PUT, DELETE, etc.)
    URI: URI path (will be appended to base_url)
        litellm http request GET /models
        litellm http request POST /chat/completions -j '{"model": "gpt-4", "messages": [{"role": "user", "content": "Hello"}]}'
        litellm http request GET /health/test_connection -H "X-Custom-Header:value"
    # Parse headers from key:value format
    for h in header:
            key, value = h.split(":", 1)
            headers[key.strip()] = value.strip()
            raise click.BadParameter(f"Invalid header format: {h}. Expected format: 'key:value'")
    # Parse JSON data if provided
    json_data = None
    if json:
            json_data = json_lib.loads(json)
            raise click.BadParameter(f"Invalid JSON format: {e}")
    # Parse data if provided
    request_data = None
            request_data = json_lib.loads(data)
            # If not JSON, use as raw data
            request_data = data
    client = HTTPClient(ctx.obj["base_url"], ctx.obj["api_key"])
        response = client.request(
            uri=uri,
            data=request_data,
            json=json_data,
        rich.print_json(data=response)
        click.echo(f"Error: HTTP {e.response.status_code}", err=True)
            rich.print_json(data=error_body)
        except json_lib.JSONDecodeError:
            click.echo(e.response.text, err=True)
        raise click.Abort()
