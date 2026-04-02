from ...types.realtime import client_secret_create_params
from ...types.realtime.client_secret_create_response import ClientSecretCreateResponse
__all__ = ["ClientSecrets", "AsyncClientSecrets"]
class ClientSecrets(SyncAPIResource):
    def with_raw_response(self) -> ClientSecretsWithRawResponse:
        return ClientSecretsWithRawResponse(self)
    def with_streaming_response(self) -> ClientSecretsWithStreamingResponse:
        return ClientSecretsWithStreamingResponse(self)
        expires_after: client_secret_create_params.ExpiresAfter | Omit = omit,
        session: client_secret_create_params.Session | Omit = omit,
    ) -> ClientSecretCreateResponse:
        Create a Realtime client secret with an associated session configuration.
        Client secrets are short-lived tokens that can be passed to a client app, such
        as a web frontend or mobile client, which grants access to the Realtime API
        without leaking your main API key. You can configure a custom TTL for each
        client secret.
        You can also attach session configuration options to the client secret, which
        will be applied to any sessions created using that client secret, but these can
        also be overridden by the client connection.
        [Learn more about authentication with client secrets over WebRTC](https://platform.openai.com/docs/guides/realtime-webrtc).
        Returns the created client secret and the effective session object. The client
        secret is a string that looks like `ek_1234`.
          expires_after: Configuration for the client secret expiration. Expiration refers to the time
              after which a client secret will no longer be valid for creating sessions. The
              session itself may continue after that time once started. A secret can be used
              to create multiple sessions until it expires.
          session: Session configuration to use for the client secret. Choose either a realtime
              session or a transcription session.
            "/realtime/client_secrets",
                client_secret_create_params.ClientSecretCreateParams,
            cast_to=ClientSecretCreateResponse,
class AsyncClientSecrets(AsyncAPIResource):
    def with_raw_response(self) -> AsyncClientSecretsWithRawResponse:
        return AsyncClientSecretsWithRawResponse(self)
    def with_streaming_response(self) -> AsyncClientSecretsWithStreamingResponse:
        return AsyncClientSecretsWithStreamingResponse(self)
class ClientSecretsWithRawResponse:
    def __init__(self, client_secrets: ClientSecrets) -> None:
        self._client_secrets = client_secrets
            client_secrets.create,
class AsyncClientSecretsWithRawResponse:
    def __init__(self, client_secrets: AsyncClientSecrets) -> None:
class ClientSecretsWithStreamingResponse:
class AsyncClientSecretsWithStreamingResponse:
