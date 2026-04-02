"""JSON Model tests
Unit tests for the JSON model.
import googleapiclient.model
CSV_TEXT_MOCK = "column1,column2,column3\nstring1,1.2,string2"
class Model(unittest.TestCase):
    def test_json_no_body(self):
        model = JsonModel(data_wrapper=False)
        path_params = {}
        query_params = {}
        headers, unused_params, query, body = model.request(
            headers, path_params, query_params, body
        self.assertEqual(headers["accept"], "application/json")
        self.assertTrue("content-type" not in headers)
        self.assertNotEqual(query, "")
        self.assertEqual(body, None)
        self.assertEqual(headers["content-type"], "application/json")
        self.assertEqual(body, "{}")
    def test_json_body_data_wrapper(self):
        model = JsonModel(data_wrapper=True)
        self.assertEqual(body, '{"data": {}}')
    def test_json_body_default_data(self):
        """Test that a 'data' wrapper doesn't get added if one is already present."""
        body = {"data": "foo"}
        self.assertEqual(body, '{"data": "foo"}')
    def test_json_build_query(self):
        query_params = {
            "foo": 1,
            "bar": "\N{COMET}",
            "baz": ["fe", "fi", "fo", "fum"],  # Repeated parameters
            "qux": [],
        query_dict = urllib.parse.parse_qs(query[1:])
        self.assertEqual(query_dict["foo"], ["1"])
        self.assertEqual(query_dict["bar"], ["\N{COMET}"])
        self.assertEqual(query_dict["baz"], ["fe", "fi", "fo", "fum"])
        self.assertTrue("qux" not in query_dict)
    def test_user_agent(self):
        headers = {"user-agent": "my-test-app/1.23.4"}
        headers, unused_params, unused_query, body = model.request(
        self.assertEqual(headers["user-agent"], "my-test-app/1.23.4 (gzip)")
    def test_x_goog_api_client(self):
        # test header composition for cloud clients that wrap discovery
        headers = {"x-goog-api-client": "gccl/1.23.4"}
            headers["x-goog-api-client"],
            "gccl/1.23.4"
            + " gdcl/"
            + _LIBRARY_VERSION
            + " gl-python/"
            + platform.python_version(),
    @unittest.skipIf(
        not HAS_API_VERSION,
        "Skip this test when an older version of google-api-core is used",
    def test_x_goog_api_version(self):
        # test header composition for clients that wrap discovery
        api_version = "20240401"
        headers, _, _, body = model.request(
            headers, path_params, query_params, body, api_version
            headers[API_VERSION_METADATA_KEY],
            api_version,
    def test_bad_response(self):
        resp = httplib2.Response({"status": "401"})
        resp.reason = "Unauthorized"
        content = b'{"error": {"message": "not authorized"}}'
            content = model.response(resp, content)
            self.fail("Should have thrown an exception")
            self.assertTrue("not authorized" in str(e))
        resp["content-type"] = "application/json"
    def test_good_response(self):
        resp = httplib2.Response({"status": "200"})
        resp.reason = "OK"
        content = '{"data": "is good"}'
        self.assertEqual(content, "is good")
    def test_good_response_wo_data(self):
        content = '{"foo": "is good"}'
        self.assertEqual(content, {"foo": "is good"})
    def test_good_response_wo_data_str(self):
        content = '"data goes here"'
        self.assertEqual(content, "data goes here")
    def test_no_content_response(self):
        resp = httplib2.Response({"status": "204"})
        resp.reason = "No Content"
        content = ""
        self.assertEqual(content, {})
    def test_logging(self):
        class MockLogging(object):
                self.info_record = []
                self.debug_record = []
            def info(self, message, *args):
                self.info_record.append(message % args)
            def debug(self, message, *args):
                self.debug_record.append(message % args)
        class MockResponse(dict):
            def __init__(self, items):
                super(MockResponse, self).__init__()
                self.status = items["status"]
                for key, value in items.items():
                    self[key] = value
        old_logging = googleapiclient.model.LOGGER
        googleapiclient.model.LOGGER = MockLogging()
        googleapiclient.model.dump_request_response = True
        request_body = {"field1": "value1", "field2": "value2"}
        body_string = model.request({}, {}, {}, request_body)[-1]
        json_body = json.loads(body_string)
        self.assertEqual(request_body, json_body)
        response = {
            "status": 200,
            "response_field_1": "response_value_1",
            "response_field_2": "response_value_2",
        response_body = model.response(MockResponse(response), body_string)
        self.assertEqual(request_body, response_body)
            googleapiclient.model.LOGGER.info_record[:2],
            ["--request-start--", "-headers-start-"],
            "response_field_1: response_value_1"
            in googleapiclient.model.LOGGER.info_record
            "response_field_2: response_value_2"
            json.loads(googleapiclient.model.LOGGER.info_record[-2]), request_body
            googleapiclient.model.LOGGER.info_record[-1], "--response-end--"
        googleapiclient.model.LOGGER = old_logging
    def test_no_data_wrapper_deserialize(self):
        self.assertEqual(content, {"data": "is good"})
    def test_no_data_wrapper_deserialize_text_format(self):
        content = CSV_TEXT_MOCK
        self.assertEqual(content, CSV_TEXT_MOCK)
    def test_no_data_wrapper_deserialize_raise_type_error(self):
        buffer = io.StringIO()
        buffer.write("String buffer")
        resp = httplib2.Response({"status": "500"})
        resp.reason = "The JSON object must be str, bytes or bytearray, not StringIO"
        content = buffer
            model.response(resp, content)
    def test_data_wrapper_deserialize(self):
    def test_data_wrapper_deserialize_nodata(self):
        content = '{"atad": "is good"}'
        self.assertEqual(content, {"atad": "is good"})
