from .shared_params.metadata import Metadata
__all__ = ["BatchCreateParams", "OutputExpiresAfter"]
class BatchCreateParams(TypedDict, total=False):
    completion_window: Required[Literal["24h"]]
    """The time frame within which the batch should be processed.
    Currently only `24h` is supported.
    endpoint: Required[
    """The endpoint to be used for all requests in the batch.
    Currently `/v1/responses`, `/v1/chat/completions`, `/v1/embeddings`,
    `/v1/completions`, `/v1/moderations`, `/v1/images/generations`,
    `/v1/images/edits`, and `/v1/videos` are supported. Note that `/v1/embeddings`
    batches are also restricted to a maximum of 50,000 embedding inputs across all
    requests in the batch.
    input_file_id: Required[str]
    """The ID of an uploaded file that contains requests for the new batch.
    metadata: Optional[Metadata]
    output_expires_after: OutputExpiresAfter
    The expiration policy for the output and/or error file that are generated for a
class OutputExpiresAfter(TypedDict, total=False):
    The expiration policy for the output and/or error file that are generated for a batch.
    anchor: Required[Literal["created_at"]]
    """Anchor timestamp after which the expiration policy applies.
    Supported anchors: `created_at`. Note that the anchor is the file creation time,
    not the time the batch is created.
    seconds: Required[int]
    """The number of seconds after the anchor time that the file will expire.
    Must be between 3600 (1 hour) and 2592000 (30 days).
    `/v1/completions`, `/v1/moderations`, `/v1/images/generations`, and
    `/v1/images/edits` are supported. Note that `/v1/embeddings` batches are also
