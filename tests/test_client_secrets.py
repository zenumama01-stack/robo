from openai.types.realtime import ClientSecretCreateResponse
class TestClientSecrets:
        client_secret = client.realtime.client_secrets.create()
        assert_matches_type(ClientSecretCreateResponse, client_secret, path=["response"])
        client_secret = client.realtime.client_secrets.create(
                "seconds": 10,
        response = client.realtime.client_secrets.with_raw_response.create()
        client_secret = response.parse()
        with client.realtime.client_secrets.with_streaming_response.create() as response:
class TestAsyncClientSecrets:
        client_secret = await async_client.realtime.client_secrets.create()
        client_secret = await async_client.realtime.client_secrets.create(
        response = await async_client.realtime.client_secrets.with_raw_response.create()
        async with async_client.realtime.client_secrets.with_streaming_response.create() as response:
            client_secret = await response.parse()
