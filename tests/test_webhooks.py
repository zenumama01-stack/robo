from openai._exceptions import InvalidWebhookSignatureError
# Standardized test constants (matches TypeScript implementation)
TEST_SECRET = "whsec_RdvaYFYUXuIFuEbvZHwMfYFhUf7aMYjYcmM24+Aj40c="
TEST_PAYLOAD = '{"id": "evt_685c059ae3a481909bdc86819b066fb6", "object": "event", "created_at": 1750861210, "type": "response.completed", "data": {"id": "resp_123"}}'
TEST_TIMESTAMP = 1750861210  # Fixed timestamp that matches our test signature
TEST_WEBHOOK_ID = "wh_685c059ae39c8190af8c71ed1022a24d"
TEST_SIGNATURE = "v1,gUAg4R2hWouRZqRQG4uJypNS8YK885G838+EHb4nKBY="
def create_test_headers(
    timestamp: int | None = None, signature: str | None = None, webhook_id: str | None = None
) -> dict[str, str]:
    """Helper function to create test headers"""
        "webhook-signature": signature or TEST_SIGNATURE,
        "webhook-timestamp": str(timestamp or TEST_TIMESTAMP),
        "webhook-id": webhook_id or TEST_WEBHOOK_ID,
class TestWebhooks:
    @mock.patch("time.time", mock.MagicMock(return_value=TEST_TIMESTAMP))
    def test_unwrap_with_secret(self, client: openai.OpenAI) -> None:
        headers = create_test_headers()
        unwrapped = client.webhooks.unwrap(TEST_PAYLOAD, headers, secret=TEST_SECRET)
        assert unwrapped.id == "evt_685c059ae3a481909bdc86819b066fb6"
        assert unwrapped.created_at == 1750861210
    def test_unwrap_without_secret(self, client: openai.OpenAI) -> None:
        with pytest.raises(ValueError, match="The webhook secret must either be set"):
            client.webhooks.unwrap(TEST_PAYLOAD, headers)
    def test_verify_signature_valid(self, client: openai.OpenAI) -> None:
        # Should not raise - this is a truly valid signature for this timestamp
        client.webhooks.verify_signature(TEST_PAYLOAD, headers, secret=TEST_SECRET)
    def test_verify_signature_invalid_secret_format(self, client: openai.OpenAI) -> None:
            client.webhooks.verify_signature(TEST_PAYLOAD, headers, secret=None)
    def test_verify_signature_invalid(self, client: openai.OpenAI) -> None:
        with pytest.raises(InvalidWebhookSignatureError, match="The given webhook signature does not match"):
            client.webhooks.verify_signature(TEST_PAYLOAD, headers, secret="invalid_secret")
    def test_verify_signature_missing_webhook_signature_header(self, client: openai.OpenAI) -> None:
        headers = create_test_headers(signature=None)
        del headers["webhook-signature"]
        with pytest.raises(ValueError, match="Could not find webhook-signature header"):
    def test_verify_signature_missing_webhook_timestamp_header(self, client: openai.OpenAI) -> None:
        del headers["webhook-timestamp"]
        with pytest.raises(ValueError, match="Could not find webhook-timestamp header"):
    def test_verify_signature_missing_webhook_id_header(self, client: openai.OpenAI) -> None:
        del headers["webhook-id"]
        with pytest.raises(ValueError, match="Could not find webhook-id header"):
    def test_verify_signature_payload_bytes(self, client: openai.OpenAI) -> None:
        client.webhooks.verify_signature(TEST_PAYLOAD.encode("utf-8"), headers, secret=TEST_SECRET)
    def test_unwrap_with_client_secret(self) -> None:
        test_client = openai.OpenAI(base_url=base_url, api_key="test-api-key", webhook_secret=TEST_SECRET)
        unwrapped = test_client.webhooks.unwrap(TEST_PAYLOAD, headers)
    def test_verify_signature_timestamp_too_old(self, client: openai.OpenAI) -> None:
        # Use a timestamp that's older than 5 minutes from our test timestamp
        old_timestamp = TEST_TIMESTAMP - 400  # 6 minutes 40 seconds ago
        headers = create_test_headers(timestamp=old_timestamp, signature="v1,dummy_signature")
        with pytest.raises(InvalidWebhookSignatureError, match="Webhook timestamp is too old"):
    def test_verify_signature_timestamp_too_new(self, client: openai.OpenAI) -> None:
        # Use a timestamp that's in the future beyond tolerance from our test timestamp
        future_timestamp = TEST_TIMESTAMP + 400  # 6 minutes 40 seconds in the future
        headers = create_test_headers(timestamp=future_timestamp, signature="v1,dummy_signature")
        with pytest.raises(InvalidWebhookSignatureError, match="Webhook timestamp is too new"):
    def test_verify_signature_custom_tolerance(self, client: openai.OpenAI) -> None:
        # Use a timestamp that's older than default tolerance but within custom tolerance
        old_timestamp = TEST_TIMESTAMP - 400  # 6 minutes 40 seconds ago from test timestamp
        # Should fail with default tolerance
        # Should also fail with custom tolerance of 10 minutes (signature won't match)
            client.webhooks.verify_signature(TEST_PAYLOAD, headers, secret=TEST_SECRET, tolerance=600)
    def test_verify_signature_recent_timestamp_succeeds(self, client: openai.OpenAI) -> None:
        # Use a recent timestamp with dummy signature
        headers = create_test_headers(signature="v1,dummy_signature")
        # Should fail on signature verification (not timestamp validation)
    def test_verify_signature_multiple_signatures_one_valid(self, client: openai.OpenAI) -> None:
        # Test multiple signatures: one invalid, one valid
        multiple_signatures = f"v1,invalid_signature {TEST_SIGNATURE}"
        headers = create_test_headers(signature=multiple_signatures)
        # Should not raise when at least one signature is valid
    def test_verify_signature_multiple_signatures_all_invalid(self, client: openai.OpenAI) -> None:
        # Test multiple invalid signatures
        multiple_invalid_signatures = "v1,invalid_signature1 v1,invalid_signature2"
        headers = create_test_headers(signature=multiple_invalid_signatures)
class TestAsyncWebhooks:
    async def test_unwrap_with_secret(self, async_client: openai.AsyncOpenAI) -> None:
        unwrapped = async_client.webhooks.unwrap(TEST_PAYLOAD, headers, secret=TEST_SECRET)
    async def test_unwrap_without_secret(self, async_client: openai.AsyncOpenAI) -> None:
            async_client.webhooks.unwrap(TEST_PAYLOAD, headers)
    async def test_verify_signature_valid(self, async_client: openai.AsyncOpenAI) -> None:
        async_client.webhooks.verify_signature(TEST_PAYLOAD, headers, secret=TEST_SECRET)
    async def test_verify_signature_invalid_secret_format(self, async_client: openai.AsyncOpenAI) -> None:
            async_client.webhooks.verify_signature(TEST_PAYLOAD, headers, secret=None)
    async def test_verify_signature_invalid(self, async_client: openai.AsyncOpenAI) -> None:
            async_client.webhooks.verify_signature(TEST_PAYLOAD, headers, secret="invalid_secret")
    async def test_verify_signature_missing_webhook_signature_header(self, async_client: openai.AsyncOpenAI) -> None:
    async def test_verify_signature_missing_webhook_timestamp_header(self, async_client: openai.AsyncOpenAI) -> None:
    async def test_verify_signature_missing_webhook_id_header(self, async_client: openai.AsyncOpenAI) -> None:
    async def test_verify_signature_payload_bytes(self, async_client: openai.AsyncOpenAI) -> None:
        async_client.webhooks.verify_signature(TEST_PAYLOAD.encode("utf-8"), headers, secret=TEST_SECRET)
    async def test_unwrap_with_client_secret(self) -> None:
        test_async_client = openai.AsyncOpenAI(base_url=base_url, api_key="test-api-key", webhook_secret=TEST_SECRET)
        unwrapped = test_async_client.webhooks.unwrap(TEST_PAYLOAD, headers)
    async def test_verify_signature_timestamp_too_old(self, async_client: openai.AsyncOpenAI) -> None:
    async def test_verify_signature_timestamp_too_new(self, async_client: openai.AsyncOpenAI) -> None:
    async def test_verify_signature_multiple_signatures_one_valid(self, async_client: openai.AsyncOpenAI) -> None:
    async def test_verify_signature_multiple_signatures_all_invalid(self, async_client: openai.AsyncOpenAI) -> None:
