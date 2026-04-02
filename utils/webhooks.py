import hmac
from typing import cast
from ..._types import HeadersLike
from ..._utils import get_required_header
from ..._exceptions import InvalidWebhookSignatureError
from ...types.webhooks.unwrap_webhook_event import UnwrapWebhookEvent
class Webhooks(SyncAPIResource):
    def unwrap(
        payload: str | bytes,
        headers: HeadersLike,
        secret: str | None = None,
    ) -> UnwrapWebhookEvent:
        """Validates that the given payload was sent by OpenAI and parses the payload."""
        if secret is None:
            secret = self._client.webhook_secret
        self.verify_signature(payload=payload, headers=headers, secret=secret)
            UnwrapWebhookEvent,
                type_=UnwrapWebhookEvent,
                value=json.loads(payload),
    def verify_signature(
        tolerance: int = 300,
        """Validates whether or not the webhook payload was sent by OpenAI.
            payload: The webhook payload
            headers: The webhook headers
            secret: The webhook secret (optional, will use client secret if not provided)
            tolerance: Maximum age of the webhook in seconds (default: 300 = 5 minutes)
                "The webhook secret must either be set using the env var, OPENAI_WEBHOOK_SECRET, "
                "on the client class, OpenAI(webhook_secret='123'), or passed to this function"
        signature_header = get_required_header(headers, "webhook-signature")
        timestamp = get_required_header(headers, "webhook-timestamp")
        webhook_id = get_required_header(headers, "webhook-id")
        # Validate timestamp to prevent replay attacks
            timestamp_seconds = int(timestamp)
            raise InvalidWebhookSignatureError("Invalid webhook timestamp format") from None
        now = int(time.time())
        if now - timestamp_seconds > tolerance:
            raise InvalidWebhookSignatureError("Webhook timestamp is too old") from None
        if timestamp_seconds > now + tolerance:
            raise InvalidWebhookSignatureError("Webhook timestamp is too new") from None
        # Extract signatures from v1,<base64> format
        # The signature header can have multiple values, separated by spaces.
        # Each value is in the format v1,<base64>. We should accept if any match.
        signatures: list[str] = []
        for part in signature_header.split():
            if part.startswith("v1,"):
                signatures.append(part[3:])
                signatures.append(part)
        # Decode the secret if it starts with whsec_
        if secret.startswith("whsec_"):
            decoded_secret = base64.b64decode(secret[6:])
            decoded_secret = secret.encode()
        body = payload.decode("utf-8") if isinstance(payload, bytes) else payload
        # Prepare the signed payload (OpenAI uses webhookId.timestamp.payload format)
        signed_payload = f"{webhook_id}.{timestamp}.{body}"
        expected_signature = base64.b64encode(
            hmac.new(decoded_secret, signed_payload.encode(), hashlib.sha256).digest()
        ).decode()
        # Accept if any signature matches
        if not any(hmac.compare_digest(expected_signature, sig) for sig in signatures):
            raise InvalidWebhookSignatureError(
                "The given webhook signature does not match the expected signature"
class AsyncWebhooks(AsyncAPIResource):
                value=json.loads(body),
            raise InvalidWebhookSignatureError("The given webhook signature does not match the expected signature")
from .._types import HeadersLike
from .._utils import get_required_header
from .._models import construct_type
from .._exceptions import InvalidWebhookSignatureError
from ..types.webhooks.unwrap_webhook_event import UnwrapWebhookEvent
