"""Discovery document cache tests."""
    from googleapiclient.discovery_cache.file_cache import Cache as FileCache
    FileCache = None
@unittest.skipIf(FileCache is None, "FileCache unavailable.")
class FileCacheTest(unittest.TestCase):
        "googleapiclient.discovery_cache.file_cache.FILENAME",
        new="google-api-python-client-discovery-doc-tests.cache",
    def test_expiration(self):
        def future_now():
            return datetime.datetime.now() + datetime.timedelta(
                seconds=DISCOVERY_DOC_MAX_AGE
        mocked_datetime = mock.MagicMock()
        mocked_datetime.datetime.now.side_effect = future_now
        cache = FileCache(max_age=DISCOVERY_DOC_MAX_AGE)
        first_url = "url-1"
        first_url_content = "url-1-content"
        cache.set(first_url, first_url_content)
        # Make sure the content is cached.
        self.assertEqual(first_url_content, cache.get(first_url))
        # Simulate another `set` call in the future date.
            "googleapiclient.discovery_cache.file_cache.datetime", new=mocked_datetime
            cache.set("url-2", "url-2-content")
        # Make sure the content is expired
        self.assertEqual(None, cache.get(first_url))
