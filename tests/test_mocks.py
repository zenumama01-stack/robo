"""Mock tests
Unit tests for the Mocks.
from googleapiclient.errors import HttpError, UnexpectedBodyError, UnexpectedMethodError
from googleapiclient.http import HttpMock, RequestMockBuilder
class Mocks(unittest.TestCase):
        self.zoo_http = HttpMock(datafile("zoo.json"), {"status": "200"})
    def test_default_response(self):
        requestBuilder = RequestMockBuilder({})
        activity = plus.activities().get(activityId="tag:blah").execute()
        self.assertEqual({}, activity)
    def test_simple_response(self):
            {"plus.activities.get": (None, '{"foo": "bar"}')}
        self.assertEqual({"foo": "bar"}, activity)
    def test_unexpected_call(self):
        requestBuilder = RequestMockBuilder({}, check_unexpected=True)
            plus.activities().get(activityId="tag:blah").execute()
            self.fail("UnexpectedMethodError should have been raised")
        except UnexpectedMethodError:
    def test_simple_unexpected_body(self):
            {"zoo.animals.insert": (None, '{"data": {"foo": "bar"}}', None)}
            http=self.zoo_http,
            zoo.animals().insert(body="{}").execute()
            self.fail("UnexpectedBodyError should have been raised")
        except UnexpectedBodyError:
    def test_simple_expected_body(self):
            {"zoo.animals.insert": (None, '{"data": {"foo": "bar"}}', "{}")}
            zoo.animals().insert(body="").execute()
    def test_simple_wrong_body(self):
                "zoo.animals.insert": (
                    '{"data": {"foo": "bar"}}',
            zoo.animals().insert(body='{"data": {"foo": "blah"}}').execute()
    def test_simple_matching_str_body(self):
        activity = zoo.animals().insert(body={"data": {"foo": "bar"}}).execute()
    def test_simple_matching_dict_body(self):
                    {"data": {"foo": "bar"}},
    def test_errors(self):
        errorResponse = httplib2.Response({"status": 500, "reason": "Server Error"})
            {"plus.activities.list": (errorResponse, b"{}")}
            activity = (
                plus.activities().list(collection="public", userId="me").execute()
            self.fail("An exception should have been thrown")
            self.assertEqual(b"{}", e.content)
            self.assertEqual(500, e.resp.status)
            self.assertEqual("Server Error", e.resp.reason)
