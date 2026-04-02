from .._compat import model_dump
def openapi_dumps(obj: Any) -> bytes:
    Serialize an object to UTF-8 encoded JSON bytes.
    Extends the standard json.dumps with support for additional types
    commonly used in the SDK, such as `datetime`, `pydantic.BaseModel`, etc.
    return json.dumps(
        obj,
        cls=_CustomEncoder,
        # Uses the same defaults as httpx's JSON serialization
        ensure_ascii=False,
        separators=(",", ":"),
        allow_nan=False,
    ).encode()
class _CustomEncoder(json.JSONEncoder):
    def default(self, o: Any) -> Any:
        if isinstance(o, datetime):
            return o.isoformat()
        if isinstance(o, pydantic.BaseModel):
            return model_dump(o, exclude_unset=True, mode="json", by_alias=True)
        return super().default(o)
# Extracted from https://github.com/pfmoore/pkg_metadata
from email.header import Header, decode_header, make_header
METADATA_FIELDS = [
    # Name, Multiple-Use
    ("Metadata-Version", False),
    ("Name", False),
    ("Version", False),
    ("Dynamic", True),
    ("Platform", True),
    ("Supported-Platform", True),
    ("Summary", False),
    ("Description", False),
    ("Description-Content-Type", False),
    ("Keywords", False),
    ("Home-page", False),
    ("Download-URL", False),
    ("Author", False),
    ("Author-email", False),
    ("Maintainer", False),
    ("Maintainer-email", False),
    ("License", False),
    ("License-Expression", False),
    ("License-File", True),
    ("Classifier", True),
    ("Requires-Dist", True),
    ("Requires-Python", False),
    ("Requires-External", True),
    ("Project-URL", True),
    ("Provides-Extra", True),
    ("Provides-Dist", True),
    ("Obsoletes-Dist", True),
def json_name(field: str) -> str:
    return field.lower().replace("-", "_")
def msg_to_json(msg: Message) -> dict[str, Any]:
    """Convert a Message object into a JSON-compatible dictionary."""
    def sanitise_header(h: Header | str) -> str:
        if isinstance(h, Header):
            for bytes, encoding in decode_header(h):
                if encoding == "unknown-8bit":
                        # See if UTF-8 works
                        bytes.decode("utf-8")
                        encoding = "utf-8"
                        # If not, latin1 at least won't fail
                        encoding = "latin1"
                chunks.append((bytes, encoding))
            return str(make_header(chunks))
        return str(h)
    for field, multi in METADATA_FIELDS:
        if field not in msg:
        key = json_name(field)
        if multi:
            value: str | list[str] = [
                sanitise_header(v) for v in msg.get_all(field)  # type: ignore
            value = sanitise_header(msg.get(field))  # type: ignore
            if key == "keywords":
                # Accept both comma-separated and space-separated
                # forms, for better compatibility with old data.
                if "," in value:
                    value = [v.strip() for v in value.split(",")]
                    value = value.split()
    payload = cast(str, msg.get_payload())
    if payload:
        result["description"] = payload
