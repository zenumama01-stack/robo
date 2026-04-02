"""Http tests
Unit tests for the googleapiclient.http.
from io import FileIO
# Do not remove the httplib2 import
from googleapiclient.errors import BatchError, HttpError, InvalidChunkSizeError
    MAX_URI_LENGTH,
    MediaInMemoryUpload,
    MediaIoBaseDownload,
    _StreamSlice,
    set_user_agent,
class MockCredentials:
    """Mock class for all Credentials objects."""
    def __init__(self, bearer_token, expired=False):
        super(MockCredentials, self).__init__()
        self._authorized = 0
        self._refreshed = 0
        self._applied = 0
        self._bearer_token = bearer_token
        self._access_token_expired = expired
    def access_token(self):
        return self._bearer_token
    def access_token_expired(self):
        return self._access_token_expired
    def authorize(self, http):
        self._authorized += 1
            # Modify the request headers to add the appropriate
            # Authorization header.
            self.apply(headers)
                uri, method, body, headers, redirections, connection_type
        # Replace the request method with our own closure.
        # Set credentials as a property of the request method.
        setattr(http.request, "credentials", self)
    def refresh(self, http):
        self._refreshed += 1
    def apply(self, headers):
        self._applied += 1
        headers["authorization"] = self._bearer_token + " " + str(self._refreshed)
class HttpMockWithErrors(object):
    def __init__(self, num_errors, success_json, success_data):
        self.num_errors = num_errors
        self.success_json = success_json
        self.success_data = success_data
    def request(self, *args, **kwargs):
        if not self.num_errors:
            return httplib2.Response(self.success_json), self.success_data
        elif self.num_errors == 5:
            ex = ConnectionResetError  # noqa: F821
        elif self.num_errors == 4:
            ex = httplib2.ServerNotFoundError()
        elif self.num_errors == 3:
            ex = OSError()
            ex.errno = socket.errno.EPIPE
        elif self.num_errors == 2:
            ex = ssl.SSLError()
            # Initialize the timeout error code to the platform's error code.
                # For Windows:
                ex.errno = socket.errno.WSAETIMEDOUT
                # For Linux/Mac:
                ex = socket.timeout()
        self.num_errors -= 1
        raise ex
class HttpMockWithNonRetriableErrors(object):
            # set errno to a non-retriable value
                ex.errno = socket.errno.WSAEHOSTUNREACH
                ex.errno = socket.errno.EHOSTUNREACH
            # Now raise the correct timeout error.
def _postproc_none(*kwargs):
class TestUserAgent(unittest.TestCase):
    def test_set_user_agent(self):
        http = HttpMockSequence([({"status": "200"}, "echo_request_headers")])
        http = set_user_agent(http, "my_app/5.5")
        resp, content = http.request("http://example.com")
        self.assertEqual("my_app/5.5", content["user-agent"])
    def test_set_user_agent_nested(self):
        http = set_user_agent(http, "my_library/0.1")
        self.assertEqual("my_app/5.5 my_library/0.1", content["user-agent"])
class TestMediaUpload(unittest.TestCase):
    def test_media_file_upload_closes_fd_in___del__(self):
        file_desc = mock.Mock(spec=io.TextIOWrapper)
        opener = mock.mock_open(file_desc)
        with mock.patch("builtins.open", return_value=opener):
            upload = MediaFileUpload(datafile("test_close"), mimetype="text/plain")
        self.assertIs(upload.stream(), file_desc)
        del upload
        file_desc.close.assert_called_once_with()
    def test_media_file_upload_mimetype_detection(self):
        upload = MediaFileUpload(datafile("small.png"))
        self.assertEqual("image/png", upload.mimetype())
        upload = MediaFileUpload(datafile("empty"))
        self.assertEqual("application/octet-stream", upload.mimetype())
    def test_media_file_upload_to_from_json(self):
        upload = MediaFileUpload(datafile("small.png"), chunksize=500, resumable=True)
        self.assertEqual(190, upload.size())
        self.assertEqual(True, upload.resumable())
        self.assertEqual(500, upload.chunksize())
        self.assertEqual(b"PNG", upload.getbytes(1, 3))
        json = upload.to_json()
        new_upload = MediaUpload.new_from_json(json)
        self.assertEqual("image/png", new_upload.mimetype())
        self.assertEqual(190, new_upload.size())
        self.assertEqual(True, new_upload.resumable())
        self.assertEqual(500, new_upload.chunksize())
        self.assertEqual(b"PNG", new_upload.getbytes(1, 3))
    def test_media_file_upload_raises_on_file_not_found(self):
        with self.assertRaises(FileNotFoundError):
            MediaFileUpload(datafile("missing.png"))
    def test_media_file_upload_raises_on_invalid_chunksize(self):
        self.assertRaises(
            datafile("small.png"),
            mimetype="image/png",
            chunksize=-2,
            resumable=True,
    def test_media_inmemory_upload(self):
        media = MediaInMemoryUpload(
            b"abcdef", mimetype="text/plain", chunksize=10, resumable=True
        self.assertEqual("text/plain", media.mimetype())
        self.assertEqual(10, media.chunksize())
        self.assertTrue(media.resumable())
        self.assertEqual(b"bc", media.getbytes(1, 2))
        self.assertEqual(6, media.size())
    def test_http_request_to_from_json(self):
        media_upload = MediaFileUpload(
            datafile("small.png"), chunksize=500, resumable=True
        req = HttpRequest(
            _postproc_none,
            "http://example.com",
            method="POST",
            body="{}",
            headers={"content-type": 'multipart/related; boundary="---flubber"'},
            methodId="foo",
            resumable=media_upload,
        json = req.to_json()
        new_req = HttpRequest.from_json(json, http, _postproc_none)
            {"content-type": 'multipart/related; boundary="---flubber"'},
            new_req.headers,
        self.assertEqual("http://example.com", new_req.uri)
        self.assertEqual("{}", new_req.body)
        self.assertEqual(http, new_req.http)
        self.assertEqual(media_upload.to_json(), new_req.resumable.to_json())
        self.assertEqual(random.random, new_req._rand)
        self.assertEqual(time.sleep, new_req._sleep)
class TestMediaIoBaseUpload(unittest.TestCase):
    def test_media_io_base_upload_from_file_io(self):
        fd = FileIO(datafile("small.png"), "r")
    def test_media_io_base_upload_from_file_object(self):
        f = open(datafile("small.png"), "rb")
            fd=f, mimetype="image/png", chunksize=500, resumable=True
    def test_media_io_base_upload_serializable(self):
        upload = MediaIoBaseUpload(fd=f, mimetype="image/png")
            self.fail("MediaIoBaseUpload should not be serializable.")
        except NotImplementedError:
    def test_media_io_base_upload_from_bytes(self):
        fd = io.BytesIO(f.read())
    def test_media_io_base_upload_raises_on_invalid_chunksize(self):
            fd,
            "image/png",
    def test_media_io_base_upload_streamable(self):
        fd = io.BytesIO(b"stuff")
        self.assertEqual(True, upload.has_stream())
        self.assertEqual(fd, upload.stream())
    def test_media_io_base_next_chunk_retries(self):
        # Simulate errors for both the request that creates the resumable upload
        # and the upload itself.
                ({"status": "500"}, ""),
                ({"status": "503"}, ""),
                ({"status": "200", "location": "location"}, ""),
                ({"status": "403"}, USER_RATE_LIMIT_EXCEEDED_RESPONSE_NO_STATUS),
                ({"status": "403"}, RATE_LIMIT_EXCEEDED_RESPONSE),
                ({"status": "429"}, ""),
                ({"status": "200"}, "{}"),
        model = JsonModel()
        uri = "https://www.googleapis.com/someapi/v1/upload/?foo=bar"
        request = HttpRequest(
            http, model.response, uri, method=method, headers={}, resumable=upload
        sleeptimes = []
        request._sleep = lambda x: sleeptimes.append(x)
        request._rand = lambda: 10
        request.execute(num_retries=3)
        self.assertEqual([20, 40, 80, 20, 40, 80], sleeptimes)
    def test_media_io_base_next_chunk_no_retry_403_not_configured(self):
        fd = io.BytesIO(b"i am png")
            [({"status": "403"}, NOT_CONFIGURED_RESPONSE), ({"status": "200"}, "{}")]
        request._rand = lambda: 1.0
        request._sleep = mock.MagicMock()
        request._sleep.assert_not_called()
    def test_media_io_base_empty_file(self):
        fd = io.BytesIO()
                        "status": "200",
                        "location": "https://www.googleapis.com/someapi/v1/upload?foo=bar",
                    "{}",
        request.execute()
        # Check that "Content-Range" header is not set in the PUT request
        self.assertTrue("Content-Range" not in http.request_sequence[-1][-1])
        self.assertEqual("0", http.request_sequence[-1][-1]["Content-Length"])
class TestMediaIoBaseDownload(unittest.TestCase):
        self.request = zoo.animals().get_media(name="Lion")
        self.fd = io.BytesIO()
    def test_media_io_base_download(self):
        self.request.http = HttpMockSequence(
                ({"status": "200", "content-range": "0-2/5"}, b"123"),
                ({"status": "200", "content-range": "3-4/5"}, b"45"),
        self.assertEqual(True, self.request.http.follow_redirects)
        download = MediaIoBaseDownload(fd=self.fd, request=self.request, chunksize=3)
        self.assertEqual(self.fd, download._fd)
        self.assertEqual(3, download._chunksize)
        self.assertEqual(0, download._progress)
        self.assertEqual(None, download._total_size)
        self.assertEqual(False, download._done)
        self.assertEqual(self.request.uri, download._uri)
        status, done = download.next_chunk()
        self.assertEqual(self.fd.getvalue(), b"123")
        self.assertEqual(False, done)
        self.assertEqual(3, download._progress)
        self.assertEqual(5, download._total_size)
        self.assertEqual(3, status.resumable_progress)
        self.assertEqual(self.fd.getvalue(), b"12345")
        self.assertEqual(True, done)
        self.assertEqual(5, download._progress)
    def test_media_io_base_download_range_request_header(self):
                    {"status": "200", "content-range": "0-2/5"},
                    "echo_request_headers_as_json",
        result = json.loads(self.fd.getvalue().decode("utf-8"))
        self.assertEqual(result.get("range"), "bytes=0-2")
    def test_media_io_base_download_custom_request_headers(self):
                    {"status": "200", "content-range": "3-4/5"},
        self.request.headers["Cache-Control"] = "no-store"
        self.assertEqual(download._headers.get("Cache-Control"), "no-store")
        # assert that that the header we added to the original request is
        # sent up to the server on each call to next_chunk
        self.assertEqual(result.get("Cache-Control"), "no-store")
        download._fd = self.fd = io.BytesIO()
    def test_media_io_base_download_handle_redirects(self):
                        "content-location": "https://secure.example.net/lion",
                ({"status": "200", "content-range": "0-2/5"}, b"abc"),
        self.assertEqual("https://secure.example.net/lion", download._uri)
    def test_media_io_base_download_handle_4xx(self):
        self.request.http = HttpMockSequence([({"status": "400"}, "")])
            self.fail("Should raise an exception")
        except HttpError:
        # Even after raising an exception we can pick up where we left off.
            [({"status": "200", "content-range": "0-2/5"}, b"123")]
    def test_media_io_base_download_retries_connection_errors(self):
        self.request.http = HttpMockWithErrors(
            5, {"status": "200", "content-range": "0-2/3"}, b"123"
        download._sleep = lambda _x: 0  # do nothing
        download._rand = lambda: 10
        status, done = download.next_chunk(num_retries=5)
    def test_media_io_base_download_retries_5xx(self):
        # Set time.sleep and random.random stubs.
        download._sleep = lambda x: sleeptimes.append(x)
        status, done = download.next_chunk(num_retries=3)
        # Check for exponential backoff using the rand function above.
        self.assertEqual([20, 40, 80], sleeptimes)
        # Reset time.sleep stub.
        del sleeptimes[0 : len(sleeptimes)]
    def test_media_io_base_download_empty_file(self):
            [({"status": "200", "content-range": "0-0/0"}, b"")]
        self.assertEqual(0, download._total_size)
    def test_media_io_base_download_empty_file_416_response(self):
            [({"status": "416", "content-range": "0-0/0"}, b"")]
    def test_media_io_base_download_unknown_media_size(self):
        self.request.http = HttpMockSequence([({"status": "200"}, b"123")])
EXPECTED = """POST /someapi/v1/collection/?foo=bar HTTP/1.1
Content-Type: application/json
MIME-Version: 1.0
Host: www.googleapis.com
content-length: 2\r\n\r\n{}"""
NO_BODY_EXPECTED = """POST /someapi/v1/collection/?foo=bar HTTP/1.1
content-length: 0\r\n\r\n"""
NO_BODY_EXPECTED_GET = """GET /someapi/v1/collection/?foo=bar HTTP/1.1
Host: www.googleapis.com\r\n\r\n"""
RESPONSE = """HTTP/1.1 200 OK
Content-Length: 14
ETag: "etag/pony"\r\n\r\n{"answer": 42}"""
BATCH_RESPONSE = b"""--batch_foobarbaz
Content-Type: application/http
Content-Transfer-Encoding: binary
Content-ID: <randomness + 1>
HTTP/1.1 200 OK
ETag: "etag/pony"\r\n\r\n{"foo": 42}
--batch_foobarbaz
Content-ID: <randomness + 2>
ETag: "etag/sheep"\r\n\r\n{"baz": "qux"}
--batch_foobarbaz--"""
BATCH_ERROR_RESPONSE = b"""--batch_foobarbaz
HTTP/1.1 403 Access Not Configured
Content-Length: 245
ETag: "etag/sheep"\r\n\r\n{
    "domain": "usageLimits",
    "reason": "accessNotConfigured",
    "message": "Access Not Configured",
    "debugInfo": "QuotaState: BLOCKED"
  "code": 403,
  "message": "Access Not Configured"
BATCH_RESPONSE_WITH_401 = b"""--batch_foobarbaz
HTTP/1.1 401 Authorization Required
ETag: "etag/pony"\r\n\r\n{"error": {"message":
  "Authorizaton failed."}}
BATCH_SINGLE_RESPONSE = b"""--batch_foobarbaz
USER_RATE_LIMIT_EXCEEDED_RESPONSE_NO_STATUS = """{
    "reason": "userRateLimitExceeded",
    "message": "User Rate Limit Exceeded"
}"""
USER_RATE_LIMIT_EXCEEDED_RESPONSE_WITH_STATUS = """{
  "message": "User Rate Limit Exceeded",
  "status": "PERMISSION_DENIED"
RATE_LIMIT_EXCEEDED_RESPONSE = """{
    "reason": "rateLimitExceeded",
    "message": "Rate Limit Exceeded"
NOT_CONFIGURED_RESPONSE = """{
LIST_NOT_CONFIGURED_RESPONSE = """[
]"""
class Callbacks(object):
        self.responses = {}
        self.exceptions = {}
    def f(self, request_id, response, exception):
        self.responses[request_id] = response
        self.exceptions[request_id] = exception
class TestHttpRequest(unittest.TestCase):
    def test_unicode(self):
        http = HttpMock(datafile("zoo.json"), headers={"status": "200"})
        uri = "https://www.googleapis.com/someapi/v1/collection/?foo=bar"
        self.assertEqual(uri, http.uri)
        self.assertEqual(str, type(http.uri))
        self.assertEqual(method, http.method)
        self.assertEqual(str, type(http.method))
    def test_empty_content_type(self):
        """Test for #284"""
        http = HttpMock(None, headers={"status": 200})
            http, _postproc_none, uri, method=method, headers={"content-type": ""}
        self.assertEqual("", http.headers.get("content-type"))
    def test_no_retry_connection_errors(self):
            HttpMockWithNonRetriableErrors(1, {"status": "200"}, '{"foo": "bar"}'),
            "https://www.example.com/json_api_endpoint",
        request._sleep = lambda _x: 0  # do nothing
        with self.assertRaises(OSError):
            response = request.execute(num_retries=3)
    def test_retry_connection_errors_non_resumable(self):
            HttpMockWithErrors(5, {"status": "200"}, '{"foo": "bar"}'),
        response = request.execute(num_retries=5)
        self.assertEqual({"foo": "bar"}, response)
    def test_retry_connection_errors_resumable(self):
        with open(datafile("small.png"), "rb") as small_png_file:
            small_png_fd = io.BytesIO(small_png_file.read())
            fd=small_png_fd, mimetype="image/png", chunksize=500, resumable=True
            HttpMockWithErrors(
                5, {"status": "200", "location": "location"}, '{"foo": "bar"}'
            "https://www.example.com/file_upload",
            resumable=upload,
    def test_retry(self):
        num_retries = 6
        resp_seq = [({"status": "500"}, "")] * (num_retries - 4)
        resp_seq.append(({"status": "403"}, RATE_LIMIT_EXCEEDED_RESPONSE))
        resp_seq.append(
            ({"status": "403"}, USER_RATE_LIMIT_EXCEEDED_RESPONSE_NO_STATUS)
            ({"status": "403"}, USER_RATE_LIMIT_EXCEEDED_RESPONSE_WITH_STATUS)
        resp_seq.append(({"status": "429"}, ""))
        resp_seq.append(({"status": "200"}, "{}"))
        http = HttpMockSequence(resp_seq)
        request.execute(num_retries=num_retries)
        self.assertEqual(num_retries, len(sleeptimes))
        for retry_num in range(num_retries):
            self.assertEqual(10 * 2 ** (retry_num + 1), sleeptimes[retry_num])
    def test_no_retry_succeeds(self):
        num_retries = 5
        resp_seq = [({"status": "200"}, "{}")] * (num_retries)
        self.assertEqual(0, len(sleeptimes))
    def test_no_retry_fails_fast(self):
        http = HttpMockSequence([({"status": "500"}, ""), ({"status": "200"}, "{}")])
    def test_no_retry_403_not_configured_fails_fast(self):
    def test_no_retry_403_fails_fast(self):
        http = HttpMockSequence([({"status": "403"}, ""), ({"status": "200"}, "{}")])
    def test_no_retry_401_fails_fast(self):
        http = HttpMockSequence([({"status": "401"}, ""), ({"status": "200"}, "{}")])
    def test_no_retry_403_list_fails(self):
                ({"status": "403"}, LIST_NOT_CONFIGURED_RESPONSE),
    def test_null_postproc(self):
        resp, content = HttpRequest.null_postproc("foo", "bar")
        self.assertEqual(resp, "foo")
        self.assertEqual(content, "bar")
class TestBatch(unittest.TestCase):
        self.request1 = HttpRequest(
            "https://www.googleapis.com/someapi/v1/collection/?foo=bar",
        self.request2 = HttpRequest(
    def test_id_to_from_content_id_header(self):
        self.assertEqual("12", batch._header_to_id(batch._id_to_header("12")))
    def test_invalid_content_id_header(self):
        self.assertRaises(BatchError, batch._header_to_id, "[foo+x]")
        self.assertRaises(BatchError, batch._header_to_id, "foo+1")
        self.assertRaises(BatchError, batch._header_to_id, "<foo>")
    def test_serialize_request(self):
        s = batch._serialize_request(request).splitlines()
        self.assertEqual(EXPECTED.splitlines(), s)
    def test_serialize_request_media_body(self):
        body = f.read()
        # Just testing it shouldn't raise an exception.
    def test_serialize_request_no_body(self):
            body=b"",
        self.assertEqual(NO_BODY_EXPECTED.splitlines(), s)
    def test_serialize_get_request_no_body(self):
        self.assertEqual(NO_BODY_EXPECTED_GET.splitlines(), s)
    def test_deserialize_response(self):
        resp, content = batch._deserialize_response(RESPONSE)
        self.assertEqual(200, resp.status)
        self.assertEqual("OK", resp.reason)
        self.assertEqual(11, resp.version)
        self.assertEqual('{"answer": 42}', content)
    def test_new_id(self):
        id_ = batch._new_id()
        self.assertEqual("1", id_)
        self.assertEqual("2", id_)
        batch.add(self.request1, request_id="3")
        self.assertEqual("4", id_)
    def test_add(self):
        batch.add(self.request1, request_id="1")
        self.assertRaises(KeyError, batch.add, self.request1, request_id="1")
    def test_add_fail_for_over_limit(self):
        from googleapiclient.http import MAX_BATCH_LIMIT
        for i in range(0, MAX_BATCH_LIMIT):
            batch.add(
                HttpRequest(
        self.assertRaises(BatchError, batch.add, self.request1)
    def test_add_fail_for_resumable(self):
        self.request1.resumable = upload
        with self.assertRaises(BatchError) as batch_error:
        str(batch_error.exception)
    def test_execute_empty_batch_no_http(self):
        ret = batch.execute()
        self.assertEqual(None, ret)
    def test_execute(self):
        callbacks = Callbacks()
        batch.add(self.request1, callback=callbacks.f)
        batch.add(self.request2, callback=callbacks.f)
                        "content-type": 'multipart/mixed; boundary="batch_foobarbaz"',
                    BATCH_RESPONSE,
        self.assertEqual({"foo": 42}, callbacks.responses["1"])
        self.assertEqual(None, callbacks.exceptions["1"])
        self.assertEqual({"baz": "qux"}, callbacks.responses["2"])
        self.assertEqual(None, callbacks.exceptions["2"])
    def test_execute_request_body(self):
        batch.add(self.request1)
        batch.add(self.request2)
                    "echo_request_body",
            self.fail("Should raise exception")
        except BatchError as e:
            boundary, _ = e.content.split(None, 1)
            self.assertEqual("--", boundary[:2])
            parts = e.content.split(boundary)
            self.assertEqual(4, len(parts))
            self.assertEqual("", parts[0])
            self.assertEqual("--", parts[3].rstrip())
            header = parts[1].splitlines()[1]
            self.assertEqual("Content-Type: application/http", header)
    def test_execute_request_body_with_custom_long_request_ids(self):
        batch.add(self.request1, request_id="abc" * 20)
        batch.add(self.request2, request_id="def" * 20)
            for partindex, request_id in ((1, "abc" * 20), (2, "def" * 20)):
                lines = parts[partindex].splitlines()
                for n, line in enumerate(lines):
                    if line.startswith("Content-ID:"):
                        # assert correct header folding
                        self.assertTrue(line.endswith("+"), line)
                        header_continuation = lines[n + 1]
                            header_continuation,
                            " %s>" % request_id,
    def test_execute_initial_refresh_oauth2(self):
        cred = MockCredentials("Foo", expired=True)
                    BATCH_SINGLE_RESPONSE,
        cred.authorize(http)
        self.assertIsNone(callbacks.exceptions["1"])
        self.assertEqual(1, cred._refreshed)
        self.assertEqual(1, cred._authorized)
        self.assertEqual(1, cred._applied)
    def test_execute_refresh_and_retry_on_401(self):
        cred_1 = MockCredentials("Foo")
        cred_2 = MockCredentials("Bar")
                    BATCH_RESPONSE_WITH_401,
        creds_http_1 = HttpMockSequence([])
        cred_1.authorize(creds_http_1)
        creds_http_2 = HttpMockSequence([])
        cred_2.authorize(creds_http_2)
        self.request1.http = creds_http_1
        self.request2.http = creds_http_2
        self.assertEqual(1, cred_1._refreshed)
        self.assertEqual(0, cred_2._refreshed)
        self.assertEqual(1, cred_1._authorized)
        self.assertEqual(1, cred_2._authorized)
        self.assertEqual(1, cred_2._applied)
        self.assertEqual(2, cred_1._applied)
    def test_http_errors_passed_to_callback(self):
        self.assertEqual(None, callbacks.responses["1"])
        self.assertEqual(401, callbacks.exceptions["1"].resp.status)
            "Authorization Required", callbacks.exceptions["1"].resp.reason
    def test_execute_global_callback(self):
        batch = BatchHttpRequest(callback=callbacks.f)
    def test_execute_batch_http_error(self):
                    BATCH_ERROR_RESPONSE,
        expected = (
            "<HttpError 403 when requesting "
            "https://www.googleapis.com/someapi/v1/collection/?foo=bar returned "
            '"Access Not Configured". '
            "Details: \"[{'domain': 'usageLimits', 'reason': 'accessNotConfigured', 'message': 'Access Not Configured', 'debugInfo': 'QuotaState: BLOCKED'}]\">"
        self.assertEqual(expected, str(callbacks.exceptions["2"]))
class TestRequestUriTooLong(unittest.TestCase):
    def test_turn_get_into_post(self):
        def _postproc(resp, content):
                ({"status": "200"}, "echo_request_headers"),
        # Send a long query parameter.
        query = {"q": "a" * MAX_URI_LENGTH + "?&"}
            _postproc,
            "http://example.com?" + urllib.parse.urlencode(query),
            headers={},
        # Query parameters should be sent in the body.
        response = req.execute()
        self.assertEqual(b"q=" + b"a" * MAX_URI_LENGTH + b"%3F%26", response)
        # Extra headers should be set.
        self.assertEqual("GET", response["x-http-method-override"])
        self.assertEqual(str(MAX_URI_LENGTH + 8), response["content-length"])
        self.assertEqual("application/x-www-form-urlencoded", response["content-type"])
class TestStreamSlice(unittest.TestCase):
    """Test _StreamSlice."""
        self.stream = io.BytesIO(b"0123456789")
    def test_read(self):
        s = _StreamSlice(self.stream, 0, 4)
        self.assertEqual(b"", s.read(0))
        self.assertEqual(b"0", s.read(1))
        self.assertEqual(b"123", s.read())
    def test_read_too_much(self):
        s = _StreamSlice(self.stream, 1, 4)
        self.assertEqual(b"1234", s.read(6))
    def test_read_all(self):
        s = _StreamSlice(self.stream, 2, 1)
        self.assertEqual(b"2", s.read(-1))
class TestResponseCallback(unittest.TestCase):
    """Test adding callbacks to responses."""
    def test_ensure_response_callback(self):
        m = JsonModel()
            m.response,
        h = HttpMockSequence([({"status": 200}, "{}")])
        responses = []
        def _on_response(resp, responses=responses):
            responses.append(resp)
        request.add_response_callback(_on_response)
        request.execute(http=h)
        self.assertEqual(1, len(responses))
class TestHttpMock(unittest.TestCase):
    def test_default_response_headers(self):
        http = HttpMock(datafile("zoo.json"))
        self.assertEqual(resp.status, 200)
    def test_error_response(self):
        http = HttpMock(datafile("bad_request.json"), {"status": "400"})
        self.assertRaises(HttpError, request.execute)
class TestHttpBuild(unittest.TestCase):
    original_socket_default_timeout = None
    def setUpClass(cls):
        cls.original_socket_default_timeout = socket.getdefaulttimeout()
    def tearDownClass(cls):
        socket.setdefaulttimeout(cls.original_socket_default_timeout)
    def test_build_http_sets_default_timeout_if_none_specified(self):
        socket.setdefaulttimeout(None)
    def test_build_http_default_timeout_can_be_overridden(self):
        socket.setdefaulttimeout(1.5)
        self.assertAlmostEqual(http.timeout, 1.5, delta=0.001)
    def test_build_http_default_timeout_can_be_set_to_zero(self):
        socket.setdefaulttimeout(0)
        self.assertEqual(http.timeout, 0)
    def test_build_http_default_308_is_excluded_as_redirect(self):
        self.assertTrue(308 not in http.redirect_codes)
    logging.getLogger().setLevel(logging.ERROR)
from django.http import HttpRequest, HttpResponse, HttpResponseNotAllowed
from django.views.decorators.http import (
    condition,
    conditional_page,
    require_http_methods,
    require_safe,
class RequireHttpMethodsTest(SimpleTestCase):
    def test_wrapped_sync_function_is_not_coroutine_function(self):
        def sync_view(request):
            return HttpResponse()
        wrapped_view = require_http_methods(["GET"])(sync_view)
        self.assertIs(iscoroutinefunction(wrapped_view), False)
    def test_wrapped_async_function_is_coroutine_function(self):
        async def async_view(request):
        wrapped_view = require_http_methods(["GET"])(async_view)
        self.assertIs(iscoroutinefunction(wrapped_view), True)
    def test_require_http_methods_methods(self):
        @require_http_methods(["GET", "PUT"])
            return HttpResponse("OK")
        request.method = "GET"
        self.assertIsInstance(my_view(request), HttpResponse)
        request.method = "PUT"
        request.method = "HEAD"
        self.assertIsInstance(my_view(request), HttpResponseNotAllowed)
        request.method = "POST"
        request.method = "DELETE"
    async def test_require_http_methods_methods_async_view(self):
        async def my_view(request):
        self.assertIsInstance(await my_view(request), HttpResponse)
        self.assertIsInstance(await my_view(request), HttpResponseNotAllowed)
class RequireSafeDecoratorTest(SimpleTestCase):
    def test_require_safe_accepts_only_safe_methods(self):
        my_safe_view = require_safe(my_view)
        self.assertIsInstance(my_safe_view(request), HttpResponse)
        self.assertIsInstance(my_safe_view(request), HttpResponseNotAllowed)
    async def test_require_safe_accepts_only_safe_methods_async_view(self):
        @require_safe
        self.assertIsInstance(await async_view(request), HttpResponse)
        self.assertIsInstance(await async_view(request), HttpResponseNotAllowed)
class ConditionDecoratorTest(SimpleTestCase):
    def etag_func(request, *args, **kwargs):
        return '"b4246ffc4f62314ca13147c9d4f76974"'
    def latest_entry(request, *args, **kwargs):
        return datetime.datetime(2023, 1, 2, 23, 21, 47)
        wrapped_view = condition(
            etag_func=self.etag_func, last_modified_func=self.latest_entry
        )(sync_view)
        )(async_view)
    def test_condition_decorator(self):
        @condition(
            etag_func=self.etag_func,
            last_modified_func=self.latest_entry,
        response = my_view(request)
        self.assertEqual(response.status_code, 200)
        self.assertEqual(response.headers["ETag"], '"b4246ffc4f62314ca13147c9d4f76974"')
            response.headers["Last-Modified"],
            "Mon, 02 Jan 2023 23:21:47 GMT",
    async def test_condition_decorator_async_view(self):
        response = await async_view(request)
class ConditionalPageTests(SimpleTestCase):
        wrapped_view = conditional_page(sync_view)
        wrapped_view = conditional_page(async_view)
    def test_conditional_page_decorator_successful(self):
        @conditional_page
            response.content = b"test"
            response["Cache-Control"] = "public"
        response = sync_view(request)
        self.assertIsNotNone(response.get("Etag"))
    async def test_conditional_page_decorator_successful_async_view(self):
from datetime import date
from django.contrib.sitemaps import Sitemap
from .base import SitemapTestsBase
class HTTPSitemapTests(SitemapTestsBase):
    use_sitemap_err_msg = (
        "To use sitemaps, either enable the sites framework or pass a "
        "Site/RequestSite object in your view."
    def test_simple_sitemap_index(self):
        "A simple sitemap index can be rendered"
        response = self.client.get("/simple/index.xml")
        expected_content = """<?xml version="1.0" encoding="UTF-8"?>
<sitemapindex xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
<sitemap><loc>%s/simple/sitemap-simple.xml</loc><lastmod>%s</lastmod></sitemap>
</sitemapindex>
""" % (
            self.base_url,
            date.today(),
        self.assertXMLEqual(response.text, expected_content)
    def test_sitemap_not_callable(self):
        """A sitemap may not be callable."""
        response = self.client.get("/simple-not-callable/index.xml")
    def test_paged_sitemap(self):
        """A sitemap may have multiple pages."""
        response = self.client.get("/simple-paged/index.xml")
<sitemap><loc>{0}/simple/sitemap-simple.xml</loc><lastmod>{1}</lastmod></sitemap><sitemap><loc>{0}/simple/sitemap-simple.xml?p=2</loc><lastmod>{1}</lastmod></sitemap>
""".format(self.base_url, date.today())
                "DIRS": [os.path.join(os.path.dirname(__file__), "templates")],
    def test_simple_sitemap_custom_lastmod_index(self):
        "A simple sitemap index can be rendered with a custom template"
        response = self.client.get("/simple/custom-lastmod-index.xml")
<!-- This is a customized template -->
    def test_simple_sitemap_section(self):
        "A simple sitemap section can be rendered"
        response = self.client.get("/simple/sitemap-simple.xml")
        expected_content = (
            '<?xml version="1.0" encoding="UTF-8"?>\n'
            '<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9" '
            'xmlns:xhtml="http://www.w3.org/1999/xhtml">\n'
            "<url><loc>%s/location/</loc><lastmod>%s</lastmod>"
            "<changefreq>never</changefreq><priority>0.5</priority></url>\n"
            "</urlset>"
        ) % (
    def test_no_section(self):
        response = self.client.get("/simple/sitemap-simple2.xml")
            str(response.context["exception"]),
            "No sitemap available for section: 'simple2'",
        self.assertEqual(response.status_code, 404)
    def test_empty_page(self):
        response = self.client.get("/simple/sitemap-simple.xml?p=0")
        self.assertEqual(str(response.context["exception"]), "Page 0 empty")
    def test_page_not_int(self):
        response = self.client.get("/simple/sitemap-simple.xml?p=test")
        self.assertEqual(str(response.context["exception"]), "No page 'test'")
    def test_simple_sitemap(self):
        "A simple sitemap can be rendered"
        response = self.client.get("/simple/sitemap.xml")
    def test_simple_custom_sitemap(self):
        "A simple sitemap can be rendered with a custom template"
        response = self.client.get("/simple/custom-sitemap.xml")
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
<url><loc>%s/location/</loc><lastmod>%s</lastmod><changefreq>never</changefreq><priority>0.5</priority></url>
</urlset>
    def test_sitemap_last_modified(self):
        "Last-Modified header is set correctly"
        response = self.client.get("/lastmod/sitemap.xml")
            response.headers["Last-Modified"], "Wed, 13 Mar 2013 10:00:00 GMT"
    def test_sitemap_last_modified_date(self):
        The Last-Modified header should be support dates (without time).
        response = self.client.get("/lastmod/date-sitemap.xml")
            response.headers["Last-Modified"], "Wed, 13 Mar 2013 00:00:00 GMT"
    def test_sitemap_last_modified_tz(self):
        The Last-Modified header should be converted from timezone aware dates
        to GMT.
        response = self.client.get("/lastmod/tz-sitemap.xml")
            response.headers["Last-Modified"], "Wed, 13 Mar 2013 15:00:00 GMT"
    def test_sitemap_last_modified_missing(self):
        "Last-Modified header is missing when sitemap has no lastmod"
        response = self.client.get("/generic/sitemap.xml")
        self.assertFalse(response.has_header("Last-Modified"))
    def test_sitemap_last_modified_mixed(self):
        "Last-Modified header is omitted when lastmod not on all items"
        response = self.client.get("/lastmod-mixed/sitemap.xml")
    def test_sitemaps_lastmod_mixed_ascending_last_modified_missing(self):
        The Last-Modified header is omitted when lastmod isn't found in all
        sitemaps. Test sitemaps are sorted by lastmod in ascending order.
        response = self.client.get("/lastmod-sitemaps/mixed-ascending.xml")
    def test_sitemaps_lastmod_mixed_descending_last_modified_missing(self):
        sitemaps. Test sitemaps are sorted by lastmod in descending order.
        response = self.client.get("/lastmod-sitemaps/mixed-descending.xml")
    def test_sitemaps_lastmod_ascending(self):
        The Last-Modified header is set to the most recent sitemap lastmod.
        Test sitemaps are sorted by lastmod in ascending order.
        response = self.client.get("/lastmod-sitemaps/ascending.xml")
            response.headers["Last-Modified"], "Sat, 20 Apr 2013 05:00:00 GMT"
    def test_sitemaps_lastmod_descending(self):
        Test sitemaps are sorted by lastmod in descending order.
        response = self.client.get("/lastmod-sitemaps/descending.xml")
    def test_sitemap_get_latest_lastmod_none(self):
        sitemapindex.lastmod is omitted when Sitemap.lastmod is
        callable and Sitemap.get_latest_lastmod is not implemented
        response = self.client.get("/lastmod/get-latest-lastmod-none-sitemap.xml")
        self.assertNotContains(response, "<lastmod>")
    def test_sitemap_get_latest_lastmod(self):
        sitemapindex.lastmod is included when Sitemap.lastmod is
        attribute and Sitemap.get_latest_lastmod is implemented
        response = self.client.get("/lastmod/get-latest-lastmod-sitemap.xml")
        self.assertContains(response, "<lastmod>2013-03-13T10:00:00</lastmod>")
    def test_sitemap_latest_lastmod_timezone(self):
        lastmod datestamp shows timezones if Sitemap.get_latest_lastmod
        returns an aware datetime.
        response = self.client.get("/lastmod/latest-lastmod-timezone-sitemap.xml")
        self.assertContains(response, "<lastmod>2013-03-13T10:00:00-05:00</lastmod>")
    def test_localized_priority(self):
        """The priority value should not be localized."""
        with translation.override("fr"):
            self.assertEqual("0,3", localize(0.3))
            # Priorities aren't rendered in localized format.
            self.assertContains(response, "<priority>0.5</priority>")
            self.assertContains(response, "<lastmod>%s</lastmod>" % date.today())
    @modify_settings(INSTALLED_APPS={"remove": "django.contrib.sites"})
    def test_requestsite_sitemap(self):
        # Hitting the flatpages sitemap without the sites framework installed
        # doesn't raise an exception.
            "<url><loc>http://testserver/location/</loc><lastmod>%s</lastmod>"
        ) % date.today()
    def test_sitemap_get_urls_no_site_1(self):
        Check we get ImproperlyConfigured if we don't pass a site object to
        Sitemap.get_urls and no Site objects exist
        Site.objects.all().delete()
        with self.assertRaisesMessage(ImproperlyConfigured, self.use_sitemap_err_msg):
            Sitemap().get_urls()
    def test_sitemap_get_urls_no_site_2(self):
        Check we get ImproperlyConfigured when we don't pass a site object to
        Sitemap.get_urls if Site objects exists, but the sites framework is not
        actually installed.
    def test_sitemap_item(self):
        Check to make sure that the raw item is included with each
        Sitemap.get_url() url result.
        test_sitemap = Sitemap()
        test_sitemap.items = TestModel.objects.order_by("pk").all
        def is_testmodel(url):
            return isinstance(url["item"], TestModel)
        item_in_url_info = all(map(is_testmodel, test_sitemap.get_urls()))
        self.assertTrue(item_in_url_info)
    def test_cached_sitemap_index(self):
        A cached sitemap index can be rendered (#2713).
        response = self.client.get("/cached/index.xml")
<sitemap><loc>%s/cached/sitemap-simple.xml</loc><lastmod>%s</lastmod></sitemap>
    def test_x_robots_sitemap(self):
        self.assertEqual(response.headers["X-Robots-Tag"], "noindex, noodp, noarchive")
    def test_empty_sitemap(self):
        response = self.client.get("/empty/sitemap.xml")
    @override_settings(LANGUAGES=(("en", "English"), ("pt", "Portuguese")))
    def test_simple_i18n_sitemap_index(self):
        A simple i18n sitemap index can be rendered, without logging variable
        lookup errors.
        with self.assertNoLogs("django.template", "DEBUG"):
            response = self.client.get("/simple/i18n.xml")
            "<url><loc>{0}/en/i18n/testmodel/{1}/</loc><changefreq>never</changefreq>"
            "<priority>0.5</priority></url><url><loc>{0}/pt/i18n/testmodel/{1}/</loc>"
        ).format(self.base_url, self.i18n_model.pk)
    def test_alternate_i18n_sitemap_index(self):
        A i18n sitemap with alternate/hreflang links can be rendered.
        response = self.client.get("/alternates/i18n.xml")
        url, pk = self.base_url, self.i18n_model.pk
        expected_urls = f"""
<url><loc>{url}/en/i18n/testmodel/{pk}/</loc><changefreq>never</changefreq><priority>0.5</priority>
<xhtml:link rel="alternate" hreflang="en" href="{url}/en/i18n/testmodel/{pk}/"/>
<xhtml:link rel="alternate" hreflang="pt" href="{url}/pt/i18n/testmodel/{pk}/"/>
</url>
<url><loc>{url}/pt/i18n/testmodel/{pk}/</loc><changefreq>never</changefreq><priority>0.5</priority>
""".replace("\n", "")
            f'<?xml version="1.0" encoding="UTF-8"?>\n'
            f'<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9" '
            f'xmlns:xhtml="http://www.w3.org/1999/xhtml">\n'
            f"{expected_urls}\n"
            f"</urlset>"
        LANGUAGES=(("en", "English"), ("pt", "Portuguese"), ("es", "Spanish"))
    def test_alternate_i18n_sitemap_limited(self):
        A i18n sitemap index with limited languages can be rendered.
        response = self.client.get("/limited/i18n.xml")
<xhtml:link rel="alternate" hreflang="es" href="{url}/es/i18n/testmodel/{pk}/"/>
<url><loc>{url}/es/i18n/testmodel/{pk}/</loc><changefreq>never</changefreq><priority>0.5</priority>
    def test_alternate_i18n_sitemap_xdefault(self):
        A i18n sitemap index with x-default can be rendered.
        response = self.client.get("/x-default/i18n.xml")
<xhtml:link rel="alternate" hreflang="x-default" href="{url}/i18n/testmodel/{pk}/"/>
    def test_language_for_item_i18n_sitemap(self):
        A i18n sitemap index in which item can be chosen to be displayed for a
        lang or not.
        only_pt = I18nTestModel.objects.create(name="Only for PT")
        response = self.client.get("/item-by-lang/i18n.xml")
        url, pk, only_pt_pk = self.base_url, self.i18n_model.pk, only_pt.pk
        expected_urls = (
            f"<url><loc>{url}/en/i18n/testmodel/{pk}/</loc>"
            f"<changefreq>never</changefreq><priority>0.5</priority></url>"
            f"<url><loc>{url}/pt/i18n/testmodel/{pk}/</loc>"
            f"<url><loc>{url}/pt/i18n/testmodel/{only_pt_pk}/</loc>"
    def test_alternate_language_for_item_i18n_sitemap(self):
        response = self.client.get("/item-by-lang-alternates/i18n.xml")
            f"<changefreq>never</changefreq><priority>0.5</priority>"
            f'<xhtml:link rel="alternate" '
            f'hreflang="en" href="{url}/en/i18n/testmodel/{pk}/"/>'
            f'hreflang="pt" href="{url}/pt/i18n/testmodel/{pk}/"/>'
            f'hreflang="x-default" href="{url}/i18n/testmodel/{pk}/"/></url>'
            f'hreflang="pt" href="{url}/pt/i18n/testmodel/{only_pt_pk}/"/></url>'
    def test_sitemap_without_entries(self):
        response = self.client.get("/sitemap-without-entries/sitemap.xml")
            'xmlns:xhtml="http://www.w3.org/1999/xhtml">\n\n'
    def test_callable_sitemod_partial(self):
        Not all items have `lastmod`. Therefore the `Last-Modified` header
        is not set by the detail or index sitemap view.
        index_response = self.client.get("/callable-lastmod-partial/index.xml")
        sitemap_response = self.client.get("/callable-lastmod-partial/sitemap.xml")
        self.assertNotIn("Last-Modified", index_response)
        self.assertNotIn("Last-Modified", sitemap_response)
        expected_content_index = """<?xml version="1.0" encoding="UTF-8"?>
        <sitemap><loc>http://example.com/simple/sitemap-callable-lastmod.xml</loc></sitemap>
        expected_content_sitemap = (
            "<url><loc>http://example.com/location/</loc>"
            "<lastmod>2013-03-13</lastmod></url><url>"
            "<loc>http://example.com/location/</loc></url>\n"
        self.assertXMLEqual(index_response.text, expected_content_index)
        self.assertXMLEqual(sitemap_response.text, expected_content_sitemap)
    def test_callable_sitemod_full(self):
        All items in the sitemap have `lastmod`. The `Last-Modified` header
        is set for the detail and index sitemap view.
        index_response = self.client.get("/callable-lastmod-full/index.xml")
        sitemap_response = self.client.get("/callable-lastmod-full/sitemap.xml")
            index_response.headers["Last-Modified"], "Thu, 13 Mar 2014 10:00:00 GMT"
            sitemap_response.headers["Last-Modified"], "Thu, 13 Mar 2014 10:00:00 GMT"
        <sitemap><loc>http://example.com/simple/sitemap-callable-lastmod.xml</loc><lastmod>2014-03-13T10:00:00</lastmod></sitemap>
            "<lastmod>2013-03-13</lastmod></url>"
            "<lastmod>2014-03-13</lastmod></url>\n"
    def test_callable_sitemod_no_items(self):
        index_response = self.client.get("/callable-lastmod-no-items/index.xml")
    MAX_HEADER_LENGTH,
    MAX_URL_LENGTH,
    base36_to_int,
    escape_leading_slashes,
    int_to_base36,
    is_same_domain,
    parse_etags,
    parse_header_parameters,
    parse_http_date,
    quote_etag,
    url_has_allowed_host_and_scheme,
    urlencode,
    urlsafe_base64_decode,
    urlsafe_base64_encode,
class URLEncodeTests(SimpleTestCase):
    cannot_encode_none_msg = (
        "Cannot encode None for key 'a' in a query string. Did you mean to "
        "pass an empty string or omit the value?"
    def test_tuples(self):
        self.assertEqual(urlencode((("a", 1), ("b", 2), ("c", 3))), "a=1&b=2&c=3")
    def test_dict(self):
        result = urlencode({"a": 1, "b": 2, "c": 3})
        self.assertEqual(result, "a=1&b=2&c=3")
    def test_dict_containing_sequence_not_doseq(self):
        self.assertEqual(urlencode({"a": [1, 2]}, doseq=False), "a=%5B1%2C+2%5D")
    def test_dict_containing_tuple_not_doseq(self):
        self.assertEqual(urlencode({"a": (1, 2)}, doseq=False), "a=%281%2C+2%29")
    def test_custom_iterable_not_doseq(self):
        class IterableWithStr:
                return "custom"
                yield from range(0, 3)
        self.assertEqual(urlencode({"a": IterableWithStr()}, doseq=False), "a=custom")
    def test_dict_containing_sequence_doseq(self):
        self.assertEqual(urlencode({"a": [1, 2]}, doseq=True), "a=1&a=2")
    def test_dict_containing_empty_sequence_doseq(self):
        self.assertEqual(urlencode({"a": []}, doseq=True), "")
    def test_multivaluedict(self):
        result = urlencode(
            MultiValueDict(
                    "name": ["Adrian", "Simon"],
                    "position": ["Developer"],
            doseq=True,
        self.assertEqual(result, "name=Adrian&name=Simon&position=Developer")
    def test_dict_with_bytes_values(self):
        self.assertEqual(urlencode({"a": b"abc"}, doseq=True), "a=abc")
    def test_dict_with_sequence_of_bytes(self):
            urlencode({"a": [b"spam", b"eggs", b"bacon"]}, doseq=True),
            "a=spam&a=eggs&a=bacon",
    def test_dict_with_bytearray(self):
        self.assertEqual(urlencode({"a": bytearray(range(2))}, doseq=True), "a=0&a=1")
    def test_generator(self):
        self.assertEqual(urlencode({"a": range(2)}, doseq=True), "a=0&a=1")
        self.assertEqual(urlencode({"a": range(2)}, doseq=False), "a=range%280%2C+2%29")
    def test_none(self):
        with self.assertRaisesMessage(TypeError, self.cannot_encode_none_msg):
            urlencode({"a": None})
    def test_none_in_sequence(self):
            urlencode({"a": [None]}, doseq=True)
    def test_none_in_generator(self):
        def gen():
            urlencode({"a": gen()}, doseq=True)
class Base36IntTests(SimpleTestCase):
    def test_roundtrip(self):
        for n in [0, 1, 1000, 1000000]:
            self.assertEqual(n, base36_to_int(int_to_base36(n)))
    def test_negative_input(self):
        with self.assertRaisesMessage(ValueError, "Negative base36 conversion input."):
            int_to_base36(-1)
    def test_to_base36_errors(self):
        for n in ["1", "foo", {1: 2}, (1, 2, 3), 3.141]:
                int_to_base36(n)
    def test_invalid_literal(self):
        for n in ["#", " "]:
                ValueError, "invalid literal for int() with base 36: '%s'" % n
                base36_to_int(n)
    def test_input_too_large(self):
        with self.assertRaisesMessage(ValueError, "Base36 input too large"):
            base36_to_int("1" * 14)
    def test_to_int_errors(self):
        for n in [123, {1: 2}, (1, 2, 3), 3.141]:
    def test_values(self):
        for n, b36 in [(0, "0"), (1, "1"), (42, "16"), (818469960, "django")]:
            self.assertEqual(int_to_base36(n), b36)
            self.assertEqual(base36_to_int(b36), n)
class URLHasAllowedHostAndSchemeTests(unittest.TestCase):
    def test_bad_urls(self):
        bad_urls = (
            "http:///example.com",
            "https://example.com",
            "ftp://example.com",
            r"\\example.com",
            r"\\\example.com",
            r"/\\/example.com",
            r"\\//example.com",
            r"/\/example.com",
            r"\/example.com",
            r"/\example.com",
            r"http:/\//example.com",
            r"http:\/example.com",
            r"http:/\example.com",
            'javascript:alert("XSS")',
            "\njavascript:alert(x)",
            "java\nscript:alert(x)",
            "\x08//example.com",
            r"http://otherserver\@example.com",
            r"http:\\testserver\@example.com",
            r"http://testserver\me:pass@example.com",
            r"http://testserver\@example.com",
            r"http:\\testserver\confirm\me@example.com",
            "http:999999999",
            "ftp:9999999999",
            "http://[2001:cdba:0000:0000:0000:0000:3257:9652/",
            "http://2001:cdba:0000:0000:0000:0000:3257:9652]/",
        for bad_url in bad_urls:
            with self.subTest(url=bad_url):
                self.assertIs(
                    url_has_allowed_host_and_scheme(
                        bad_url, allowed_hosts={"testserver", "testserver2"}
    def test_good_urls(self):
        good_urls = (
            "/view/?param=http://example.com",
            "/view/?param=https://example.com",
            "/view?param=ftp://example.com",
            "view/?param=//example.com",
            "https://testserver/",
            "HTTPS://testserver/",
            "//testserver/",
            "http://testserver/confirm?email=me@example.com",
            "/url%20with%20spaces/",
            "path/http:2222222222",
        for good_url in good_urls:
            with self.subTest(url=good_url):
                        good_url, allowed_hosts={"otherserver", "testserver"}
    def test_basic_auth(self):
        # Valid basic auth credentials are allowed.
                r"http://user:pass@testserver/", allowed_hosts={"user:pass@testserver"}
    def test_no_allowed_hosts(self):
        # A path without host is allowed.
                "/confirm/me@example.com", allowed_hosts=None
        # Basic auth without host is not allowed.
                r"http://testserver\@example.com", allowed_hosts=None
    def test_allowed_hosts_str(self):
                "http://good.com/good", allowed_hosts="good.com"
                "http://good.co/evil", allowed_hosts="good.com"
    def test_secure_param_https_urls(self):
        secure_urls = (
            "https://example.com/p",
            "HTTPS://example.com/p",
        for url in secure_urls:
            with self.subTest(url=url):
                        url, allowed_hosts={"example.com"}, require_https=True
    def test_secure_param_non_https_urls(self):
        insecure_urls = (
            "http://example.com/p",
            "ftp://example.com/p",
            "//example.com/p",
        for url in insecure_urls:
    def test_max_url_length(self):
        allowed_host = "example.com"
        max_extra_characters = "é" * (MAX_URL_LENGTH - len(allowed_host) - 1)
        max_length_boundary_url = f"{allowed_host}/{max_extra_characters}"
        cases = [
            (max_length_boundary_url, True),
            (max_length_boundary_url + "ú", False),
        for url, expected in cases:
                    url_has_allowed_host_and_scheme(url, allowed_hosts={allowed_host}),
class URLSafeBase64Tests(unittest.TestCase):
        bytestring = b"foo"
        encoded = urlsafe_base64_encode(bytestring)
        decoded = urlsafe_base64_decode(encoded)
        self.assertEqual(bytestring, decoded)
class IsSameDomainTests(unittest.TestCase):
    def test_good(self):
        for pair in (
            ("example.com", "example.com"),
            ("example.com", ".example.com"),
            ("foo.example.com", ".example.com"),
            ("example.com:8888", "example.com:8888"),
            ("example.com:8888", ".example.com:8888"),
            ("foo.example.com:8888", ".example.com:8888"),
            self.assertIs(is_same_domain(*pair), True)
    def test_bad(self):
            ("example2.com", "example.com"),
            ("foo.example.com", "example.com"),
            ("example.com:9999", "example.com:8888"),
            ("foo.example.com:8888", ""),
            self.assertIs(is_same_domain(*pair), False)
class ETagProcessingTests(unittest.TestCase):
    def test_parsing(self):
            parse_etags(r'"" ,  "etag", "e\\tag", W/"weak"'),
            ['""', '"etag"', r'"e\\tag"', 'W/"weak"'],
        self.assertEqual(parse_etags("*"), ["*"])
        # Ignore RFC 2616 ETags that are invalid according to RFC 9110.
        self.assertEqual(parse_etags(r'"etag", "e\"t\"ag"'), ['"etag"'])
    def test_quoting(self):
        self.assertEqual(quote_etag("etag"), '"etag"')  # unquoted
        self.assertEqual(quote_etag('"etag"'), '"etag"')  # quoted
        self.assertEqual(quote_etag('W/"etag"'), 'W/"etag"')  # quoted, weak
class HttpDateProcessingTests(unittest.TestCase):
    def test_http_date(self):
        t = 1167616461.0
        self.assertEqual(http_date(t), "Mon, 01 Jan 2007 01:54:21 GMT")
    def test_parsing_rfc1123(self):
        parsed = parse_http_date("Sun, 06 Nov 1994 08:49:37 GMT")
            datetime.fromtimestamp(parsed, UTC),
            datetime(1994, 11, 6, 8, 49, 37, tzinfo=UTC),
    @unittest.skipIf(platform.architecture()[0] == "32bit", "The Year 2038 problem.")
    @mock.patch("django.utils.http.datetime")
    def test_parsing_rfc850(self, mocked_datetime):
        mocked_datetime.side_effect = datetime
        now_1 = datetime(2019, 11, 6, 8, 49, 37, tzinfo=UTC)
        now_2 = datetime(2020, 11, 6, 8, 49, 37, tzinfo=UTC)
        now_3 = datetime(2048, 11, 6, 8, 49, 37, tzinfo=UTC)
        tests = (
                now_1,
                "Tuesday, 31-Dec-69 08:49:37 GMT",
                datetime(2069, 12, 31, 8, 49, 37, tzinfo=UTC),
                "Tuesday, 10-Nov-70 08:49:37 GMT",
                datetime(1970, 11, 10, 8, 49, 37, tzinfo=UTC),
                "Sunday, 06-Nov-94 08:49:37 GMT",
                now_2,
                "Wednesday, 31-Dec-70 08:49:37 GMT",
                datetime(2070, 12, 31, 8, 49, 37, tzinfo=UTC),
                "Friday, 31-Dec-71 08:49:37 GMT",
                datetime(1971, 12, 31, 8, 49, 37, tzinfo=UTC),
                now_3,
                "Sunday, 31-Dec-00 08:49:37 GMT",
                datetime(2000, 12, 31, 8, 49, 37, tzinfo=UTC),
                "Friday, 31-Dec-99 08:49:37 GMT",
                datetime(1999, 12, 31, 8, 49, 37, tzinfo=UTC),
        for now, rfc850str, expected_date in tests:
            with self.subTest(rfc850str=rfc850str):
                mocked_datetime.now.return_value = now
                parsed = parse_http_date(rfc850str)
                mocked_datetime.now.assert_called_once_with(tz=UTC)
                    expected_date,
            mocked_datetime.reset_mock()
    def test_parsing_asctime(self):
        parsed = parse_http_date("Sun Nov  6 08:49:37 1994")
    def test_parsing_asctime_nonascii_digits(self):
        """Non-ASCII unicode decimals raise an error."""
            parse_http_date("Sun Nov  6 08:49:37 １９９４")
            parse_http_date("Sun Nov １２ 08:49:37 1994")
    def test_parsing_year_less_than_70(self):
        parsed = parse_http_date("Sun Nov  6 08:49:37 0037")
            datetime(2037, 11, 6, 8, 49, 37, tzinfo=UTC),
class EscapeLeadingSlashesTests(unittest.TestCase):
    def test(self):
            ("//example.com", "/%2Fexample.com"),
            ("//", "/%2F"),
        for url, expected in tests:
                self.assertEqual(escape_leading_slashes(url), expected)
class ParseHeaderParameterTests(unittest.TestCase):
            ("", ("", {})),
            (None, ("", {})),
            ("text/plain", ("text/plain", {})),
            ("text/vnd.just.made.this.up ; ", ("text/vnd.just.made.this.up", {})),
            ("text/plain;charset=us-ascii", ("text/plain", {"charset": "us-ascii"})),
                'text/plain ; charset="us-ascii"',
                ("text/plain", {"charset": "us-ascii"}),
                'text/plain ; charset="us-ascii"; another=opt',
                ("text/plain", {"charset": "us-ascii", "another": "opt"}),
                'attachment; filename="silly.txt"',
                ("attachment", {"filename": "silly.txt"}),
                'attachment; filename="strange;name"',
                ("attachment", {"filename": "strange;name"}),
                'attachment; filename="strange;name";size=123;',
                ("attachment", {"filename": "strange;name", "size": "123"}),
                'attachment; filename="strange;name";;;;size=123;;;',
                'form-data; name="files"; filename="fo\\"o;bar"',
                ("form-data", {"name": "files", "filename": 'fo"o;bar'}),
                'form-data; name="files"; filename="\\"fo\\"o;b\\\\ar\\""',
                ("form-data", {"name": "files", "filename": '"fo"o;b\\ar"'}),
        for header, expected in tests:
            with self.subTest(header=header):
                self.assertEqual(parse_header_parameters(header), expected)
    def test_rfc2231_parsing(self):
        test_data = (
                "Content-Type: application/x-stuff; "
                "title*=us-ascii'en-us'This%20is%20%2A%2A%2Afun%2A%2A%2A",
                "This is ***fun***",
                "Content-Type: application/x-stuff; title*=UTF-8''foo-%c3%a4.html",
                "foo-ä.html",
                "Content-Type: application/x-stuff; title*=iso-8859-1''foo-%E4.html",
        for raw_line, expected_title in test_data:
            parsed = parse_header_parameters(raw_line)
            self.assertEqual(parsed[1]["title"], expected_title)
    def test_rfc2231_wrong_title(self):
        Test wrongly formatted RFC 2231 headers (missing double single quotes).
        Parsing should not crash (#24209).
                "title*='This%20is%20%2A%2A%2Afun%2A%2A%2A",
                "'This%20is%20%2A%2A%2Afun%2A%2A%2A",
            ("Content-Type: application/x-stuff; title*='foo.html", "'foo.html"),
            ("Content-Type: application/x-stuff; title*=bar.html", "bar.html"),
    def test_header_max_length(self):
        base_header = "Content-Type: application/x-stuff; title*="
        base_header_len = len(base_header)
        test_data = [
            (MAX_HEADER_LENGTH, {}),
            (MAX_HEADER_LENGTH, {"max_length": None}),
            (MAX_HEADER_LENGTH + 1, {"max_length": None}),
            (100, {"max_length": 100}),
        for line_length, kwargs in test_data:
            with self.subTest(line_length=line_length, kwargs=kwargs):
                title = "x" * (line_length - base_header_len)
                line = base_header + title
                assert len(line) == line_length
                parsed = parse_header_parameters(line, **kwargs)
                expected = ("content-type: application/x-stuff", {"title": title})
                self.assertEqual(parsed, expected)
    def test_header_too_long(self):
            ("x" * (MAX_HEADER_LENGTH + 1), {}),
            ("x" * 101, {"max_length": 100}),
        for line, kwargs in test_data:
            with self.subTest(line_length=len(line), kwargs=kwargs):
                    parse_header_parameters(line, **kwargs)
class ContentDispositionHeaderTests(unittest.TestCase):
            ((False, None), None),
            ((False, "example"), 'inline; filename="example"'),
            ((True, None), "attachment"),
            ((True, "example"), 'attachment; filename="example"'),
                (True, '"example" file\\name'),
                'attachment; filename="\\"example\\" file\\\\name"',
            ((True, "espécimen"), "attachment; filename*=utf-8''esp%C3%A9cimen"),
                (True, '"espécimen" filename'),
                "attachment; filename*=utf-8''%22esp%C3%A9cimen%22%20filename",
            ((True, "some\nfile"), "attachment; filename*=utf-8''some%0Afile"),
        for (is_attachment, filename), expected in tests:
            with self.subTest(is_attachment=is_attachment, filename=filename):
                    content_disposition_header(is_attachment, filename), expected
