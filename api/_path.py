from urllib.parse import quote
# Matches '.' or '..' where each dot is either literal or percent-encoded (%2e / %2E).
_DOT_SEGMENT_RE = re.compile(r"^(?:\.|%2[eE]){1,2}$")
_PLACEHOLDER_RE = re.compile(r"\{(\w+)\}")
def _quote_path_segment_part(value: str) -> str:
    """Percent-encode `value` for use in a URI path segment.
    Considers characters not in `pchar` set from RFC 3986 §3.3 to be unsafe.
    https://datatracker.ietf.org/doc/html/rfc3986#section-3.3
    # quote() already treats unreserved characters (letters, digits, and -._~)
    # as safe, so we only need to add sub-delims, ':', and '@'.
    # Notably, unlike the default `safe` for quote(), / is unsafe and must be quoted.
    return quote(value, safe="!$&'()*+,;=:@")
def _quote_query_part(value: str) -> str:
    """Percent-encode `value` for use in a URI query string.
    Considers &, = and characters not in `query` set from RFC 3986 §3.4 to be unsafe.
    https://datatracker.ietf.org/doc/html/rfc3986#section-3.4
    return quote(value, safe="!$'()*+,;:@/?")
def _quote_fragment_part(value: str) -> str:
    """Percent-encode `value` for use in a URI fragment.
    Considers characters not in `fragment` set from RFC 3986 §3.5 to be unsafe.
    https://datatracker.ietf.org/doc/html/rfc3986#section-3.5
    return quote(value, safe="!$&'()*+,;=:@/?")
def _interpolate(
    template: str,
    values: Mapping[str, Any],
    quoter: Callable[[str], str],
    """Replace {name} placeholders in `template`, quoting each value with `quoter`.
    Placeholder names are looked up in `values`.
        KeyError: If a placeholder is not found in `values`.
    # re.split with a capturing group returns alternating
    # [text, name, text, name, ..., text] elements.
    parts = _PLACEHOLDER_RE.split(template)
    for i in range(1, len(parts), 2):
        name = parts[i]
        if name not in values:
            raise KeyError(f"a value for placeholder {{{name}}} was not provided")
        val = values[name]
        if val is None:
            parts[i] = "null"
        elif isinstance(val, bool):
            parts[i] = "true" if val else "false"
            parts[i] = quoter(str(values[name]))
    return "".join(parts)
def path_template(template: str, /, **kwargs: Any) -> str:
    """Interpolate {name} placeholders in `template` from keyword arguments.
        template: The template string containing {name} placeholders.
        **kwargs: Keyword arguments to interpolate into the template.
        The template with placeholders interpolated and percent-encoded.
        Safe characters for percent-encoding are dependent on the URI component.
        Placeholders in path and fragment portions are percent-encoded where the `segment`
        and `fragment` sets from RFC 3986 respectively are considered safe.
        Placeholders in the query portion are percent-encoded where the `query` set from
        RFC 3986 §3.3 is considered safe except for = and & characters.
        KeyError: If a placeholder is not found in `kwargs`.
        ValueError: If resulting path contains /./ or /../ segments (including percent-encoded dot-segments).
    # Split the template into path, query, and fragment portions.
    fragment_template: str | None = None
    query_template: str | None = None
    rest = template
    if "#" in rest:
        rest, fragment_template = rest.split("#", 1)
    if "?" in rest:
        rest, query_template = rest.split("?", 1)
    path_template = rest
    # Interpolate each portion with the appropriate quoting rules.
    path_result = _interpolate(path_template, kwargs, _quote_path_segment_part)
    # Reject dot-segments (. and ..) in the final assembled path.  The check
    # runs after interpolation so that adjacent placeholders or a mix of static
    # text and placeholders that together form a dot-segment are caught.
    # Also reject percent-encoded dot-segments to protect against incorrectly
    # implemented normalization in servers/proxies.
    for segment in path_result.split("/"):
        if _DOT_SEGMENT_RE.match(segment):
            raise ValueError(f"Constructed path {path_result!r} contains dot-segment {segment!r} which is not allowed")
    result = path_result
    if query_template is not None:
        result += "?" + _interpolate(query_template, kwargs, _quote_query_part)
    if fragment_template is not None:
        result += "#" + _interpolate(fragment_template, kwargs, _quote_fragment_part)
from typing import Dict, Protocol, Union, runtime_checkable
####
# from jaraco.path 3.7.1
class Symlink(str):
    A string indicating the target of a symlink.
FilesSpec = Dict[str, Union[str, bytes, Symlink, 'FilesSpec']]
class TreeMaker(Protocol):
    def __truediv__(self, *args, **kwargs): ...  # pragma: no cover
    def mkdir(self, **kwargs): ...  # pragma: no cover
    def write_text(self, content, **kwargs): ...  # pragma: no cover
    def write_bytes(self, content): ...  # pragma: no cover
    def symlink_to(self, target): ...  # pragma: no cover
def _ensure_tree_maker(obj: Union[str, TreeMaker]) -> TreeMaker:
    return obj if isinstance(obj, TreeMaker) else pathlib.Path(obj)  # type: ignore[return-value]
    spec: FilesSpec,
    prefix: Union[str, TreeMaker] = pathlib.Path(),  # type: ignore[assignment]
    Build a set of files/directories, as described by the spec.
    Each key represents a pathname, and the value represents
    the content. Content may be a nested directory.
    >>> spec = {
    ...     'README.txt': "A README file",
    ...     "foo": {
    ...         "__init__.py": "",
    ...         "bar": {
    ...             "__init__.py": "",
    ...         },
    ...         "baz.py": "# Some code",
    ...         "bar.py": Symlink("baz.py"),
    ...     },
    ...     "bing": Symlink("foo"),
    ... }
    >>> target = getfixture('tmp_path')
    >>> build(spec, target)
    >>> target.joinpath('foo/baz.py').read_text(encoding='utf-8')
    '# Some code'
    >>> target.joinpath('bing/bar.py').read_text(encoding='utf-8')
    for name, contents in spec.items():
        create(contents, _ensure_tree_maker(prefix) / name)
def create(content: Union[str, bytes, FilesSpec], path):
    path.mkdir(exist_ok=True)
    build(content, prefix=path)  # type: ignore[arg-type]
@create.register
def _(content: bytes, path):
    path.write_bytes(content)
def _(content: str, path):
    path.write_text(content, encoding='utf-8')
def _(content: Symlink, path):
    path.symlink_to(content)
# end from jaraco.path
"""Utilities for working with paths."""
def normalize_path_segments(segments: Sequence[str]) -> list[str]:
    """Drop '.' and '..' from a sequence of str segments"""
    resolved_path: list[str] = []
    for seg in segments:
        if seg == "..":
            # ignore any .. segments that would otherwise cause an
            # IndexError when popped from resolved_path if
            # resolving for rfc3986
            with suppress(IndexError):
                resolved_path.pop()
        elif seg != ".":
            resolved_path.append(seg)
    if segments and segments[-1] in (".", ".."):
        # do some post-processing here.
        # if the last segment was a relative dir,
        # then we need to append the trailing '/'
        resolved_path.append("")
    return resolved_path
def normalize_path(path: str) -> str:
    # Drop '.' and '..' from str path
    if path and path[0] == "/":
        # preserve the "/" root element of absolute paths, copying it to the
        # normalised output as per sections 5.2.4 and 6.2.2.3 of rfc3986.
        prefix = "/"
    segments = path.split("/")
    return prefix + "/".join(normalize_path_segments(segments))
