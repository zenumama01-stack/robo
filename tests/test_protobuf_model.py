"""Protocol Buffer Model tests
Unit tests for the Protocol Buffer model.
__author__ = "mmcdonald@google.com (Matt McDonald)"
from googleapiclient.model import ProtocolBufferModel
class MockProtocolBuffer(object):
    def __init__(self, data=None):
        return self.data == other.data
    def FromString(cls, string):
        return cls(string)
    def SerializeToString(self):
        return self.data
        self.model = ProtocolBufferModel(MockProtocolBuffer)
    def test_no_body(self):
        headers, params, query, body = self.model.request(
        self.assertEqual(headers["accept"], "application/x-protobuf")
    def test_body(self):
        body = MockProtocolBuffer("data")
        self.assertEqual(headers["content-type"], "application/x-protobuf")
        self.assertEqual(body, "data")
        content = "data"
        content = self.model.response(resp, content)
        self.assertEqual(content, MockProtocolBuffer("data"))
        self.assertEqual(content, MockProtocolBuffer())
