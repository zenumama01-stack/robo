# -*- coding:utf-8 -*-
"""Model tests
Unit tests for model utility methods.
from googleapiclient.model import BaseModel, makepatch
TEST_CASES = [
    # (message, original, modified, expected)
    ("Remove an item from an object", {"a": 1, "b": 2}, {"a": 1}, {"b": None}),
    ("Add an item to an object", {"a": 1}, {"a": 1, "b": 2}, {"b": 2}),
    ("No changes", {"a": 1, "b": 2}, {"a": 1, "b": 2}, {}),
    ("Empty objects", {}, {}, {}),
    ("Modify an item in an object", {"a": 1, "b": 2}, {"a": 1, "b": 3}, {"b": 3}),
    ("Change an array", {"a": 1, "b": [2, 3]}, {"a": 1, "b": [2]}, {"b": [2]}),
        "Modify a nested item",
        {"a": 1, "b": {"foo": "bar", "baz": "qux"}},
        {"a": 1, "b": {"foo": "bar", "baz": "qaax"}},
        {"b": {"baz": "qaax"}},
        "Modify a nested array",
        {"a": 1, "b": [{"foo": "bar", "baz": "qux"}]},
        {"a": 1, "b": [{"foo": "bar", "baz": "qaax"}]},
        {"b": [{"foo": "bar", "baz": "qaax"}]},
        "Remove item from a nested array",
        {"a": 1, "b": [{"foo": "bar"}]},
        {"b": [{"foo": "bar"}]},
        "Remove a nested item",
        {"a": 1, "b": {"foo": "bar"}},
        {"b": {"baz": None}},
class TestPatch(unittest.TestCase):
        for (msg, orig, mod, expected_patch) in TEST_CASES:
            self.assertEqual(expected_patch, makepatch(orig, mod), msg=msg)
class TestBaseModel(unittest.TestCase):
    def test_build_query(self):
        model = BaseModel()
        test_cases = [
            ("hello", "world", "?hello=world"),
            ("hello", "세계", "?hello=%EC%84%B8%EA%B3%84"),
            ("hello", "こんにちは", "?hello=%E3%81%93%E3%82%93%E3%81%AB%E3%81%A1%E3%81%AF"),
            ("hello", "你好", "?hello=%E4%BD%A0%E5%A5%BD"),
            ("hello", "مرحبا", "?hello=%D9%85%D8%B1%D8%AD%D8%A8%D8%A7"),
        for case in test_cases:
            key, value, expect = case
            self.assertEqual(expect, model._build_query({key: value}))
