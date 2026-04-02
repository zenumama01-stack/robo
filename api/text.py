from .annotation import Annotation
__all__ = ["Text"]
class Text(BaseModel):
    """The data that makes up the text."""
from django.db.models.expressions import Func, Value
from django.db.models.fields import CharField, IntegerField, TextField
from django.db.models.functions import Cast, Coalesce
from django.db.models.lookups import Transform
class MySQLSHA2Mixin:
        return super().as_sql(
            template="SHA2(%%(expressions)s, %s)" % self.function[3:],
            **extra_context,
class OracleHashMixin:
            template=(
                "LOWER(RAWTOHEX(STANDARD_HASH(UTL_I18N.STRING_TO_RAW("
                "%(expressions)s, 'AL32UTF8'), '%(function)s')))"
class PostgreSQLSHAMixin:
            template="ENCODE(DIGEST(%(expressions)s, '%(function)s'), 'hex')",
            function=self.function.lower(),
class Chr(Transform):
    function = "CHR"
    lookup_name = "chr"
    output_field = CharField()
            function="CHAR",
            template="%(function)s(%(expressions)s USING utf16)",
            template="%(function)s(%(expressions)s USING NCHAR_CS)",
    def as_sqlite(self, compiler, connection, **extra_context):
        return super().as_sql(compiler, connection, function="CHAR", **extra_context)
class ConcatPair(Func):
    Concatenate two arguments together. This is used by `Concat` because not
    all backend databases support more than two arguments.
    function = "CONCAT"
    def pipes_concat_sql(self, compiler, connection, **extra_context):
        coalesced = self.coalesce()
        return super(ConcatPair, coalesced).as_sql(
            template="(%(expressions)s)",
            arg_joiner=" || ",
    as_sqlite = pipes_concat_sql
        c = self.copy()
        c.set_source_expressions(
                    if isinstance(expression.output_field, (CharField, TextField))
                    else Cast(expression, TextField())
                for expression in c.get_source_expressions()
        return c.pipes_concat_sql(compiler, connection, **extra_context)
        # Use CONCAT_WS with an empty separator so that NULLs are ignored.
            function="CONCAT_WS",
            template="%(function)s('', %(expressions)s)",
    def coalesce(self):
        # null on either side results in null for expression, wrap with
        # coalesce
                Coalesce(expression, Value(""))
class Concat(Func):
    Concatenate text fields together. Backends that result in an entire
    null expression when any arguments are null will wrap each argument in
    coalesce functions to ensure a non-null result.
    function = None
    template = "%(expressions)s"
    def __init__(self, *expressions, **extra):
        if len(expressions) < 2:
            raise ValueError("Concat must take at least two expressions")
        paired = self._paired(expressions, output_field=extra.get("output_field"))
        super().__init__(paired, **extra)
    def _paired(self, expressions, output_field):
        # wrap pairs of expressions in successive concat functions
        # exp = [a, b, c, d]
        # -> ConcatPair(a, ConcatPair(b, ConcatPair(c, d))))
        if len(expressions) == 2:
            return ConcatPair(*expressions, output_field=output_field)
        return ConcatPair(
            expressions[0],
            self._paired(expressions[1:], output_field=output_field),
            output_field=output_field,
class Left(Func):
    function = "LEFT"
    arity = 2
    def __init__(self, expression, length, **extra):
        expression: the name of a field, or an expression returning a string
        length: the number of characters to return from the start of the string
        if not hasattr(length, "resolve_expression"):
            if length < 1:
                raise ValueError("'length' must be greater than 0.")
        super().__init__(expression, length, **extra)
    def get_substr(self):
        return Substr(self.source_expressions[0], Value(1), self.source_expressions[1])
        return self.get_substr().as_oracle(compiler, connection, **extra_context)
        return self.get_substr().as_sqlite(compiler, connection, **extra_context)
class Length(Transform):
    """Return the number of characters in the expression."""
    function = "LENGTH"
    lookup_name = "length"
    output_field = IntegerField()
            compiler, connection, function="CHAR_LENGTH", **extra_context
class Lower(Transform):
    function = "LOWER"
    lookup_name = "lower"
class LPad(Func):
    function = "LPAD"
    def __init__(self, expression, length, fill_text=Value(" "), **extra):
            not hasattr(length, "resolve_expression")
            and length is not None
            and length < 0
            raise ValueError("'length' must be greater or equal to 0.")
        super().__init__(expression, length, fill_text, **extra)
class LTrim(Transform):
    function = "LTRIM"
    lookup_name = "ltrim"
class MD5(OracleHashMixin, Transform):
    function = "MD5"
    lookup_name = "md5"
class Ord(Transform):
    function = "ASCII"
    lookup_name = "ord"
        return super().as_sql(compiler, connection, function="ORD", **extra_context)
        return super().as_sql(compiler, connection, function="UNICODE", **extra_context)
class Repeat(Func):
    function = "REPEAT"
    def __init__(self, expression, number, **extra):
            not hasattr(number, "resolve_expression")
            and number is not None
            and number < 0
            raise ValueError("'number' must be greater or equal to 0.")
        super().__init__(expression, number, **extra)
        expression, number = self.source_expressions
        length = None if number is None else Length(expression) * number
        rpad = RPad(expression, length, expression)
        return rpad.as_sql(compiler, connection, **extra_context)
class Replace(Func):
    function = "REPLACE"
    def __init__(self, expression, text, replacement=Value(""), **extra):
        super().__init__(expression, text, replacement, **extra)
class Reverse(Transform):
    function = "REVERSE"
    lookup_name = "reverse"
        # REVERSE in Oracle is undocumented and doesn't support multi-byte
        # strings. Use a special subquery instead.
        suffix = connection.features.bare_select_suffix
        sql, params = super().as_sql(
                "(SELECT LISTAGG(s) WITHIN GROUP (ORDER BY n DESC) FROM "
                f"(SELECT LEVEL n, SUBSTR(%(expressions)s, LEVEL, 1) s{suffix} "
                "CONNECT BY LEVEL <= LENGTH(%(expressions)s)) "
                "GROUP BY %(expressions)s)"
        return sql, params * 3
class Right(Left):
    function = "RIGHT"
        return Substr(
            self.source_expressions[0],
            self.source_expressions[1] * Value(-1),
            self.source_expressions[1],
class RPad(LPad):
    function = "RPAD"
class RTrim(Transform):
    function = "RTRIM"
    lookup_name = "rtrim"
class SHA1(OracleHashMixin, PostgreSQLSHAMixin, Transform):
    function = "SHA1"
    lookup_name = "sha1"
class SHA224(MySQLSHA2Mixin, PostgreSQLSHAMixin, Transform):
    function = "SHA224"
    lookup_name = "sha224"
        raise NotSupportedError("SHA224 is not supported on Oracle.")
class SHA256(MySQLSHA2Mixin, OracleHashMixin, PostgreSQLSHAMixin, Transform):
    function = "SHA256"
    lookup_name = "sha256"
class SHA384(MySQLSHA2Mixin, OracleHashMixin, PostgreSQLSHAMixin, Transform):
    function = "SHA384"
    lookup_name = "sha384"
class SHA512(MySQLSHA2Mixin, OracleHashMixin, PostgreSQLSHAMixin, Transform):
    function = "SHA512"
    lookup_name = "sha512"
class StrIndex(Func):
    Return a positive integer corresponding to the 1-indexed position of the
    first occurrence of a substring inside another string, or 0 if the
    substring is not found.
    function = "INSTR"
        return super().as_sql(compiler, connection, function="STRPOS", **extra_context)
class Substr(Func):
    function = "SUBSTRING"
    def __init__(self, expression, pos, length=None, **extra):
        pos: an integer > 0, or an expression returning an integer
        length: an optional number of characters to return
        if not hasattr(pos, "resolve_expression"):
            if pos < 1:
                raise ValueError("'pos' must be greater than 0")
        expressions = [expression, pos]
        if length is not None:
            expressions.append(length)
        super().__init__(*expressions, **extra)
        return super().as_sql(compiler, connection, function="SUBSTR", **extra_context)
class Trim(Transform):
    function = "TRIM"
    lookup_name = "trim"
class Upper(Transform):
    function = "UPPER"
    lookup_name = "upper"
import gzip
import secrets
from gzip import GzipFile
from gzip import compress as gzip_compress
from html import escape
from html.parser import HTMLParser
from django.utils.functional import (
    SimpleLazyObject,
    cached_property,
    keep_lazy_text,
    lazy,
from django.utils.translation import gettext_lazy, pgettext
@keep_lazy_text
def capfirst(x):
    """Capitalize the first letter of a string."""
    if not x:
    if not isinstance(x, str):
        x = str(x)
    return x[0].upper() + x[1:]
# Set up regular expressions
re_newlines = _lazy_re_compile(r"\r\n|\r")  # Used in normalize_newlines
re_camel_case = _lazy_re_compile(r"(((?<=[a-z])[A-Z])|([A-Z](?![A-Z]|$)))")
def wrap(text, width):
    A word-wrap function that preserves existing line breaks. Expects that
    existing line breaks are posix newlines.
    Preserve all white space except added line breaks consume the space on
    which they break the line.
    Don't wrap long words, thus the output text may have lines longer than
    ``width``.
    wrapper = textwrap.TextWrapper(
        width=width,
        break_long_words=False,
        break_on_hyphens=False,
        replace_whitespace=False,
    for line in text.splitlines():
        wrapped = wrapper.wrap(line)
        if not wrapped:
            # If `line` contains only whitespaces that are dropped, restore it.
            result.append(line)
            result.extend(wrapped)
    if text.endswith("\n"):
        # If `text` ends with a newline, preserve it.
        result.append("")
    return "\n".join(result)
def add_truncation_text(text, truncate=None):
    if truncate is None:
        truncate = pgettext(
            "String to return when truncating text", "%(truncated_text)s…"
    if "%(truncated_text)s" in truncate:
        return truncate % {"truncated_text": text}
    # The truncation text didn't contain the %(truncated_text)s string
    # replacement argument so just append it to the text.
    if text.endswith(truncate):
        # But don't append the truncation text if the current text already ends
        # in this.
    return f"{text}{truncate}"
def calculate_truncate_chars_length(length, replacement):
    truncate_len = length
    for char in add_truncation_text("", replacement):
        if not unicodedata.combining(char):
            truncate_len -= 1
            if truncate_len == 0:
    return truncate_len
class TruncateHTMLParser(HTMLParser):
    class TruncationCompleted(Exception):
    def __init__(self, *, length, replacement, convert_charrefs=True):
        super().__init__(convert_charrefs=convert_charrefs)
        self.tags = deque()
        self.output = []
        self.remaining = length
        self.replacement = replacement
    def void_elements(self):
        from django.utils.html import VOID_ELEMENTS
        return VOID_ELEMENTS
    def handle_startendtag(self, tag, attrs):
        self.handle_starttag(tag, attrs)
        if tag not in self.void_elements:
            self.handle_endtag(tag)
        self.output.append(self.get_starttag_text())
            self.tags.appendleft(tag)
            self.output.append(f"</{tag}>")
            # Remove from the stack only if the tag matches the most recently
            # opened tag (LIFO). This avoids O(n) linear scans for unmatched
            # end tags if `deque.remove()` would be called.
            if self.tags and self.tags[0] == tag:
                self.tags.popleft()
    def handle_data(self, data):
        data, output = self.process(data)
        data_len = len(data)
        if self.remaining < data_len:
            self.remaining = 0
            self.output.append(add_truncation_text(output, self.replacement))
            raise self.TruncationCompleted
        self.remaining -= data_len
        self.output.append(output)
    def feed(self, data):
            super().feed(data)
        except self.TruncationCompleted:
            self.output.extend([f"</{tag}>" for tag in self.tags])
            self.tags.clear()
            self.reset()
            # No data was handled.
class TruncateCharsHTMLParser(TruncateHTMLParser):
        self.length = length
        self.processed_chars = 0
            length=calculate_truncate_chars_length(length, replacement),
            replacement=replacement,
            convert_charrefs=convert_charrefs,
    def process(self, data):
        self.processed_chars += len(data)
        if (self.processed_chars == self.length) and (
            sum(len(p) for p in self.output) + len(data) == len(self.rawdata)
            self.output.append(data)
        output = escape("".join(data[: self.remaining]))
        return data, output
class TruncateWordsHTMLParser(TruncateHTMLParser):
        data = re.split(r"(?<=\S)\s+(?=\S)", data)
        output = escape(" ".join(data[: self.remaining]))
class Truncator(SimpleLazyObject):
    An object used to truncate text, either by characters or words.
    def __init__(self, text):
        super().__init__(lambda: str(text))
    def chars(self, num, truncate=None, html=False):
        Return the text truncated to be no longer than the specified number
        of characters.
        `truncate` specifies what should be used to notify that the string has
        been truncated, defaulting to a translatable string of an ellipsis.
        self._setup()
        length = int(num)
        if length <= 0:
        text = unicodedata.normalize("NFC", self._wrapped)
        if html:
            parser = TruncateCharsHTMLParser(length=length, replacement=truncate)
            parser.feed(text)
            return "".join(parser.output)
        return self._text_chars(length, truncate, text)
    def _text_chars(self, length, truncate, text):
        """Truncate a string after a certain number of chars."""
        truncate_len = calculate_truncate_chars_length(length, truncate)
        s_len = 0
        end_index = None
        for i, char in enumerate(text):
            if unicodedata.combining(char):
                # Don't consider combining characters
                # as adding to the string length
            s_len += 1
            if end_index is None and s_len > truncate_len:
                end_index = i
            if s_len > length:
                # Return the truncated string
                return add_truncation_text(text[: end_index or 0], truncate)
        # Return the original string since no truncation was necessary
    def words(self, num, truncate=None, html=False):
        Truncate a string after a certain number of words. `truncate` specifies
        what should be used to notify that the string has been truncated,
        defaulting to ellipsis.
            parser = TruncateWordsHTMLParser(length=length, replacement=truncate)
            parser.feed(self._wrapped)
        return self._text_words(length, truncate)
    def _text_words(self, length, truncate):
        Truncate a string after a certain number of words.
        Strip newlines in the string.
        words = self._wrapped.split()
        if len(words) > length:
            words = words[:length]
            return add_truncation_text(" ".join(words), truncate)
        return " ".join(words)
def get_valid_filename(name):
    Return the given string converted to a string that can be used for a clean
    filename. Remove leading and trailing spaces; convert other spaces to
    underscores; and remove anything that is not an alphanumeric, dash,
    underscore, or dot.
    >>> get_valid_filename("john's portrait in 2004.jpg")
    'johns_portrait_in_2004.jpg'
    s = str(name).strip().replace(" ", "_")
    s = re.sub(r"(?u)[^-\w.]", "", s)
    if s in {"", ".", ".."}:
def get_text_list(list_, last_word=gettext_lazy("or")):
    >>> get_text_list(['a', 'b', 'c', 'd'])
    'a, b, c or d'
    >>> get_text_list(['a', 'b', 'c'], 'and')
    'a, b and c'
    >>> get_text_list(['a', 'b'], 'and')
    'a and b'
    >>> get_text_list(['a'])
    'a'
    >>> get_text_list([])
    if not list_:
    if len(list_) == 1:
        return str(list_[0])
    return "%s %s %s" % (
        # Translators: This string is used as a separator between list elements
        _(", ").join(str(i) for i in list_[:-1]),
        str(last_word),
        str(list_[-1]),
def normalize_newlines(text):
    """Normalize CRLF and CR newlines to just LF."""
    return re_newlines.sub("\n", str(text))
def phone2numeric(phone):
    """Convert a phone number with letters into its numeric equivalent."""
    char2number = {
        "a": "2",
        "b": "2",
        "c": "2",
        "d": "3",
        "e": "3",
        "f": "3",
        "g": "4",
        "h": "4",
        "i": "4",
        "j": "5",
        "k": "5",
        "l": "5",
        "m": "6",
        "n": "6",
        "o": "6",
        "p": "7",
        "q": "7",
        "r": "7",
        "s": "7",
        "t": "8",
        "u": "8",
        "v": "8",
        "w": "9",
        "x": "9",
        "y": "9",
        "z": "9",
    return "".join(char2number.get(c, c) for c in phone.lower())
def _get_random_filename(max_random_bytes):
    return b"a" * secrets.randbelow(max_random_bytes)
def compress_string(s, *, max_random_bytes=None):
    compressed_data = gzip_compress(s, compresslevel=6, mtime=0)
    if not max_random_bytes:
        return compressed_data
    compressed_view = memoryview(compressed_data)
    header = bytearray(compressed_view[:10])
    header[3] = gzip.FNAME
    filename = _get_random_filename(max_random_bytes) + b"\x00"
    return bytes(header) + filename + compressed_view[10:]
class StreamingBuffer(BytesIO):
    def read(self):
        ret = self.getvalue()
        self.truncate()
# Like compress_string, but for iterators of strings.
def compress_sequence(sequence, *, max_random_bytes=None):
    buf = StreamingBuffer()
    filename = _get_random_filename(max_random_bytes) if max_random_bytes else None
    with GzipFile(
        filename=filename, mode="wb", compresslevel=6, fileobj=buf, mtime=0
    ) as zfile:
        # Output headers...
        yield buf.read()
        for item in sequence:
            zfile.write(item)
            zfile.flush()
            data = buf.read()
async def acompress_sequence(sequence, *, max_random_bytes=None):
        async for item in sequence:
# Expression to match some_token and some_token="with spaces" (and similarly
# for single-quoted strings).
smart_split_re = _lazy_re_compile(
    ((?:
        [^\s'"]*
            (?:"(?:[^"\\]|\\.)*" | '(?:[^'\\]|\\.)*')
    ) | \S+)
def smart_split(text):
    Generator that splits a string by spaces, leaving quoted phrases together.
    Supports both single and double quotes, and supports escaping quotes with
    backslashes. In the output, strings will keep their initial and trailing
    quote marks and escaped quotes will remain escaped (the results can then
    be further processed with unescape_string_literal()).
    >>> list(smart_split(r'This is "a person\'s" test.'))
    ['This', 'is', '"a person\\\'s"', 'test.']
    >>> list(smart_split(r"Another 'person\'s' test."))
    ['Another', "'person\\'s'", 'test.']
    >>> list(smart_split(r'A "\"funky\" style" test.'))
    ['A', '"\\"funky\\" style"', 'test.']
    for bit in smart_split_re.finditer(str(text)):
        yield bit[0]
def unescape_string_literal(s):
    Convert quoted string literals to unquoted strings with escaped quotes and
    backslashes unquoted::
        >>> unescape_string_literal('"abc"')
        'abc'
        >>> unescape_string_literal("'abc'")
        >>> unescape_string_literal('"a \"bc\""')
        'a "bc"'
        >>> unescape_string_literal("'\'ab\' c'")
        "'ab' c"
    if not s or s[0] not in "\"'" or s[-1] != s[0]:
        raise ValueError("Not a string literal: %r" % s)
    quote = s[0]
    return s[1:-1].replace(r"\%s" % quote, quote).replace(r"\\", "\\")
def slugify(value, allow_unicode=False):
    Convert to ASCII if 'allow_unicode' is False. Convert spaces or repeated
    dashes to single dashes. Remove characters that aren't alphanumerics,
    underscores, or hyphens. Convert to lowercase. Also strip leading and
    trailing whitespace, dashes, and underscores.
    if allow_unicode:
        value = unicodedata.normalize("NFKC", value)
            unicodedata.normalize("NFKD", value)
    value = re.sub(r"[^\w\s-]", "", value.lower())
    return re.sub(r"[-\s]+", "-", value).strip("-_")
def camel_case_to_spaces(value):
    Split CamelCase and convert to lowercase. Strip surrounding whitespace.
    return re_camel_case.sub(r" \1", value).strip().lower()
def _format_lazy(format_string, *args, **kwargs):
    Apply str.format() on 'format_string' where format_string, args,
    and/or kwargs might be lazy.
    return format_string.format(*args, **kwargs)
format_lazy = lazy(_format_lazy, str)
    from langchain_community.document_loaders import TextLoader
DEPRECATED_LOOKUP = {"TextLoader": "langchain_community.document_loaders"}
from functools import partial, reduce
from math import gcd
    Pattern,
from ._loop import loop_last
from ._pick import pick_bool
from ._wrap import divide_line
from .align import AlignMethod
from .cells import cell_len, set_cell_size
from .containers import Lines
from .control import strip_control_codes
from .emoji import EmojiVariant
from .segment import Segment
from .style import Style, StyleType
if TYPE_CHECKING:  # pragma: no cover
    from .console import Console, ConsoleOptions, JustifyMethod, OverflowMethod
DEFAULT_JUSTIFY: "JustifyMethod" = "default"
DEFAULT_OVERFLOW: "OverflowMethod" = "fold"
_re_whitespace = re.compile(r"\s+$")
TextType = Union[str, "Text"]
"""A plain string or a :class:`Text` instance."""
GetStyleCallable = Callable[[str], Optional[StyleType]]
class Span(NamedTuple):
    """A marked up region in some text."""
    start: int
    """Span start index."""
    end: int
    """Span end index."""
    style: Union[str, Style]
    """Style associated with the span."""
        return f"Span({self.start}, {self.end}, {self.style!r})"
        return self.end > self.start
    def split(self, offset: int) -> Tuple["Span", Optional["Span"]]:
        """Split a span in to 2 from a given offset."""
        if offset < self.start:
            return self, None
        if offset >= self.end:
        start, end, style = self
        span1 = Span(start, min(end, offset), style)
        span2 = Span(span1.end, end, style)
        return span1, span2
    def move(self, offset: int) -> "Span":
        """Move start and end by a given offset.
            offset (int): Number of characters to add to start and end.
            TextSpan: A new TextSpan with adjusted position.
        return Span(start + offset, end + offset, style)
    def right_crop(self, offset: int) -> "Span":
        """Crop the span at the given offset.
            offset (int): A value between start and end.
            Span: A new (possibly smaller) span.
        if offset >= end:
        return Span(start, min(offset, end), style)
    def extend(self, cells: int) -> "Span":
        """Extend the span by the given number of cells.
            cells (int): Additional space to add to end of span.
            Span: A span.
        if cells:
            return Span(start, end + cells, style)
class Text(JupyterMixin):
    """Text with color / style.
        text (str, optional): Default unstyled text. Defaults to "".
        style (Union[str, Style], optional): Base style for text. Defaults to "".
        justify (str, optional): Justify method: "left", "center", "full", "right". Defaults to None.
        overflow (str, optional): Overflow method: "crop", "fold", "ellipsis". Defaults to None.
        no_wrap (bool, optional): Disable text wrapping, or None for default. Defaults to None.
        end (str, optional): Character to end text with. Defaults to "\\\\n".
        tab_size (int): Number of spaces per tab, or ``None`` to use ``console.tab_size``. Defaults to None.
        spans (List[Span], optional). A list of predefined style spans. Defaults to None.
        "_text",
        "style",
        "justify",
        "overflow",
        "no_wrap",
        "tab_size",
        "_spans",
        "_length",
        text: str = "",
        style: Union[str, Style] = "",
        justify: Optional["JustifyMethod"] = None,
        overflow: Optional["OverflowMethod"] = None,
        no_wrap: Optional[bool] = None,
        tab_size: Optional[int] = None,
        spans: Optional[List[Span]] = None,
        sanitized_text = strip_control_codes(text)
        self._text = [sanitized_text]
        self.style = style
        self.justify: Optional["JustifyMethod"] = justify
        self.overflow: Optional["OverflowMethod"] = overflow
        self.no_wrap = no_wrap
        self.end = end
        self.tab_size = tab_size
        self._spans: List[Span] = spans or []
        self._length: int = len(sanitized_text)
        return self._length
        return bool(self._length)
        return self.plain
        return f"<text {self.plain!r} {self._spans!r} {self.style!r}>"
    def __add__(self, other: Any) -> "Text":
        if isinstance(other, (str, Text)):
            result = self.copy()
            result.append(other)
        if not isinstance(other, Text):
        return self.plain == other.plain and self._spans == other._spans
    def __contains__(self, other: object) -> bool:
            return other in self.plain
        elif isinstance(other, Text):
            return other.plain in self.plain
    def __getitem__(self, slice: Union[int, slice]) -> "Text":
        def get_text_at(offset: int) -> "Text":
            _Span = Span
            text = Text(
                self.plain[offset],
                spans=[
                    _Span(0, 1, style)
                    for start, end, style in self._spans
                    if end > offset >= start
                end="",
        if isinstance(slice, int):
            return get_text_at(slice)
            start, stop, step = slice.indices(len(self.plain))
            if step == 1:
                lines = self.divide([start, stop])
                return lines[1]
                # This would be a bit of work to implement efficiently
                # For now, its not required
                raise TypeError("slices with step!=1 are not supported")
    def cell_len(self) -> int:
        """Get the number of cells required to render this text."""
        return cell_len(self.plain)
    def markup(self) -> str:
        """Get console markup to render this Text.
            str: A string potentially creating markup tags.
        from .markup import escape
        output: List[str] = []
        plain = self.plain
        markup_spans = [
            (0, False, self.style),
            *((span.start, False, span.style) for span in self._spans),
            *((span.end, True, span.style) for span in self._spans),
            (len(plain), True, self.style),
        markup_spans.sort(key=itemgetter(0, 1))
        position = 0
        append = output.append
        for offset, closing, style in markup_spans:
            if offset > position:
                append(escape(plain[position:offset]))
                position = offset
                append(f"[/{style}]" if closing else f"[{style}]")
        markup = "".join(output)
        return markup
    def from_markup(
        emoji: bool = True,
        emoji_variant: Optional[EmojiVariant] = None,
    ) -> "Text":
        """Create Text instance from markup.
            text (str): A string containing console markup.
            emoji (bool, optional): Also render emoji code. Defaults to True.
            emoji_variant (str, optional): Optional emoji variant, either "text" or "emoji". Defaults to None.
            Text: A Text instance with markup rendered.
        from .markup import render
        rendered_text = render(text, style, emoji=emoji, emoji_variant=emoji_variant)
        rendered_text.justify = justify
        rendered_text.overflow = overflow
        rendered_text.end = end
        return rendered_text
    def from_ansi(
        tab_size: Optional[int] = 8,
        """Create a Text object from a string containing ANSI escape codes.
            text (str): A string containing escape codes.
        from .ansi import AnsiDecoder
        joiner = Text(
            justify=justify,
            overflow=overflow,
            no_wrap=no_wrap,
            end=end,
            tab_size=tab_size,
            style=style,
        decoder = AnsiDecoder()
        result = joiner.join(line for line in decoder.decode(text))
    def styled(
        style: StyleType = "",
        """Construct a Text instance with a pre-applied styled. A style applied in this way won't be used
        to pad the text when it is justified.
            style (Union[str, Style]): Style to apply to the text. Defaults to "".
            Text: A text instance with a style applied to the entire string.
        styled_text = cls(text, justify=justify, overflow=overflow)
        styled_text.stylize(style)
        return styled_text
    def assemble(
        *parts: Union[str, "Text", Tuple[str, StyleType]],
        tab_size: int = 8,
        meta: Optional[Dict[str, Any]] = None,
        """Construct a text instance by combining a sequence of strings with optional styles.
        The positional arguments should be either strings, or a tuple of string + style.
            meta (Dict[str, Any], optional). Meta data to apply to text, or None for no meta data. Default to None
            Text: A new text instance.
        text = cls(
        append = text.append
        _Text = Text
            if isinstance(part, (_Text, str)):
                append(part)
                append(*part)
        if meta:
            text.apply_meta(meta)
    def plain(self) -> str:
        """Get the text as a single string."""
        if len(self._text) != 1:
            self._text[:] = ["".join(self._text)]
        return self._text[0]
    @plain.setter
    def plain(self, new_text: str) -> None:
        """Set the text to a new value."""
        if new_text != self.plain:
            sanitized_text = strip_control_codes(new_text)
            self._text[:] = [sanitized_text]
            old_length = self._length
            self._length = len(sanitized_text)
            if old_length > self._length:
                self._trim_spans()
    def spans(self) -> List[Span]:
        """Get a reference to the internal list of spans."""
        return self._spans
    @spans.setter
    def spans(self, spans: List[Span]) -> None:
        """Set spans."""
        self._spans = spans[:]
    def blank_copy(self, plain: str = "") -> "Text":
        """Return a new Text instance with copied metadata (but not the string or spans)."""
        copy_self = Text(
            plain,
            style=self.style,
            justify=self.justify,
            overflow=self.overflow,
            no_wrap=self.no_wrap,
            end=self.end,
            tab_size=self.tab_size,
        return copy_self
    def copy(self) -> "Text":
        """Return a copy of this instance."""
            self.plain,
        copy_self._spans[:] = self._spans
    def stylize(
        style: Union[str, Style],
        start: int = 0,
        end: Optional[int] = None,
        """Apply a style to the text, or a portion of the text.
            style (Union[str, Style]): Style instance or style definition to apply.
            start (int): Start offset (negative indexing is supported). Defaults to 0.
            end (Optional[int], optional): End offset (negative indexing is supported), or None for end of text. Defaults to None.
            length = len(self)
                start = length + start
            if end is None:
                end = length
                end = length + end
            if start >= length or end <= start:
                # Span not in text or not valid
            self._spans.append(Span(start, min(length, end), style))
    def stylize_before(
        """Apply a style to the text, or a portion of the text. Styles will be applied before other styles already present.
            self._spans.insert(0, Span(start, min(length, end), style))
    def apply_meta(
        self, meta: Dict[str, Any], start: int = 0, end: Optional[int] = None
        """Apply metadata to the text, or a portion of the text.
            meta (Dict[str, Any]): A dict of meta information.
        style = Style.from_meta(meta)
        self.stylize(style, start=start, end=end)
    def on(self, meta: Optional[Dict[str, Any]] = None, **handlers: Any) -> "Text":
        """Apply event handlers (used by Textual project).
            >>> from rich.text import Text
            >>> text = Text("hello world")
            >>> text.on(click="view.toggle('world')")
            meta (Dict[str, Any]): Mapping of meta information.
            **handlers: Keyword args are prefixed with "@" to defined handlers.
            Text: Self is returned to method may be chained.
        meta = {} if meta is None else meta
        meta.update({f"@{key}": value for key, value in handlers.items()})
        self.stylize(Style.from_meta(meta))
    def remove_suffix(self, suffix: str) -> None:
        """Remove a suffix if it exists.
            suffix (str): Suffix to remove.
        if self.plain.endswith(suffix):
            self.right_crop(len(suffix))
    def get_style_at_offset(self, console: "Console", offset: int) -> Style:
        """Get the style of a character at give offset.
            console (~Console): Console where text will be rendered.
            offset (int): Offset in to text (negative indexing supported)
            Style: A Style instance.
        # TODO: This is a little inefficient, it is only used by full justify
        if offset < 0:
            offset = len(self) + offset
        get_style = console.get_style
        style = get_style(self.style).copy()
        for start, end, span_style in self._spans:
            if end > offset >= start:
                style += get_style(span_style, default="")
    def extend_style(self, spaces: int) -> None:
        """Extend the Text given number of spaces where the spaces have the same style as the last character.
            spaces (int): Number of spaces to add to the Text.
        if spaces <= 0:
        spans = self.spans
        new_spaces = " " * spaces
        if spans:
            end_offset = len(self)
            self._spans[:] = [
                span.extend(spaces) if span.end >= end_offset else span
                for span in spans
            self._text.append(new_spaces)
            self._length += spaces
            self.plain += new_spaces
    def highlight_regex(
        re_highlight: Union[Pattern[str], str],
        style: Optional[Union[GetStyleCallable, StyleType]] = None,
        style_prefix: str = "",
        """Highlight text with a regular expression, where group names are
        translated to styles.
            re_highlight (Union[re.Pattern, str]): A regular expression object or string.
            style (Union[GetStyleCallable, StyleType]): Optional style to apply to whole match, or a callable
                which accepts the matched text and returns a style. Defaults to None.
            style_prefix (str, optional): Optional prefix to add to style group names.
            int: Number of regex matches
        append_span = self._spans.append
        if isinstance(re_highlight, str):
            re_highlight = re.compile(re_highlight)
        for match in re_highlight.finditer(plain):
            get_span = match.span
                start, end = get_span()
                match_style = style(plain[start:end]) if callable(style) else style
                if match_style is not None and end > start:
                    append_span(_Span(start, end, match_style))
            for name in match.groupdict().keys():
                start, end = get_span(name)
                if start != -1 and end > start:
                    append_span(_Span(start, end, f"{style_prefix}{name}"))
    def highlight_words(
        words: Iterable[str],
        case_sensitive: bool = True,
        """Highlight words with a style.
            words (Iterable[str]): Words to highlight.
            style (Union[str, Style]): Style to apply.
            case_sensitive (bool, optional): Enable case sensitive matching. Defaults to True.
            int: Number of words highlighted.
        re_words = "|".join(re.escape(word) for word in words)
        add_span = self._spans.append
        for match in re.finditer(
            re_words, self.plain, flags=0 if case_sensitive else re.IGNORECASE
            start, end = match.span(0)
            add_span(_Span(start, end, style))
    def rstrip(self) -> None:
        """Strip whitespace from end of text."""
        self.plain = self.plain.rstrip()
    def rstrip_end(self, size: int) -> None:
        """Remove whitespace beyond a certain width at the end of the text.
            size (int): The desired size of the text.
        text_length = len(self)
        if text_length > size:
            excess = text_length - size
            whitespace_match = _re_whitespace.search(self.plain)
            if whitespace_match is not None:
                whitespace_count = len(whitespace_match.group(0))
                self.right_crop(min(whitespace_count, excess))
    def set_length(self, new_length: int) -> None:
        """Set new length of the text, clipping or padding is required."""
        if length != new_length:
            if length < new_length:
                self.pad_right(new_length - length)
                self.right_crop(length - new_length)
    ) -> Iterable[Segment]:
        tab_size: int = console.tab_size if self.tab_size is None else self.tab_size
        justify = self.justify or options.justify or DEFAULT_JUSTIFY
        overflow = self.overflow or options.overflow or DEFAULT_OVERFLOW
        lines = self.wrap(
            options.max_width,
            tab_size=tab_size or 8,
            no_wrap=pick_bool(self.no_wrap, options.no_wrap, False),
        all_lines = Text("\n").join(lines)
        yield from all_lines.render(console, end=self.end)
        text = self.plain
        max_text_width = max(cell_len(line) for line in lines) if lines else 0
        words = text.split()
        min_text_width = (
            max(cell_len(word) for word in words) if words else max_text_width
        return Measurement(min_text_width, max_text_width)
    def render(self, console: "Console", end: str = "") -> Iterable["Segment"]:
        """Render the text as Segments.
            end (Optional[str], optional): Optional end character.
            Iterable[Segment]: Result of render that may be written to the console.
        _Segment = Segment
        if not self._spans:
            yield Segment(text)
                yield _Segment(end)
        get_style = partial(console.get_style, default=Style.null())
        enumerated_spans = list(enumerate(self._spans, 1))
        style_map = {index: get_style(span.style) for index, span in enumerated_spans}
        style_map[0] = get_style(self.style)
        spans = [
            (0, False, 0),
            *((span.start, False, index) for index, span in enumerated_spans),
            *((span.end, True, index) for index, span in enumerated_spans),
            (len(text), True, 0),
        spans.sort(key=itemgetter(0, 1))
        stack: List[int] = []
        stack_append = stack.append
        stack_pop = stack.remove
        style_cache: Dict[Tuple[Style, ...], Style] = {}
        style_cache_get = style_cache.get
        combine = Style.combine
        def get_current_style() -> Style:
            """Construct current style from stack."""
            styles = tuple(style_map[_style_id] for _style_id in sorted(stack))
            cached_style = style_cache_get(styles)
            if cached_style is not None:
                return cached_style
            current_style = combine(styles)
            style_cache[styles] = current_style
            return current_style
        for (offset, leaving, style_id), (next_offset, _, _) in zip(spans, spans[1:]):
            if leaving:
                stack_pop(style_id)
                stack_append(style_id)
            if next_offset > offset:
                yield _Segment(text[offset:next_offset], get_current_style())
    def join(self, lines: Iterable["Text"]) -> "Text":
        """Join text together with this instance as the separator.
            lines (Iterable[Text]): An iterable of Text instances to join.
            Text: A new text instance containing join text.
        new_text = self.blank_copy()
        def iter_text() -> Iterable["Text"]:
            if self.plain:
                for last, line in loop_last(lines):
        extend_text = new_text._text.extend
        append_span = new_text._spans.append
        extend_spans = new_text._spans.extend
        for text in iter_text():
            extend_text(text._text)
            if text.style:
                append_span(_Span(offset, offset + len(text), text.style))
            extend_spans(
                _Span(offset + start, offset + end, style)
                for start, end, style in text._spans
            offset += len(text)
        new_text._length = offset
        return new_text
    def expand_tabs(self, tab_size: Optional[int] = None) -> None:
        """Converts tabs to spaces.
            tab_size (int, optional): Size of tabs. Defaults to 8.
        if "\t" not in self.plain:
        if tab_size is None:
            tab_size = self.tab_size
            tab_size = 8
        new_text: List[Text] = []
        append = new_text.append
        for line in self.split("\n", include_separator=True):
            if "\t" not in line.plain:
                append(line)
                cell_position = 0
                parts = line.split("\t", include_separator=True)
                    if part.plain.endswith("\t"):
                        part._text[-1] = part._text[-1][:-1] + " "
                        cell_position += part.cell_len
                        tab_remainder = cell_position % tab_size
                        if tab_remainder:
                            spaces = tab_size - tab_remainder
                            part.extend_style(spaces)
                            cell_position += spaces
        result = Text("").join(new_text)
        self._text = [result.plain]
        self._length = len(self.plain)
        self._spans[:] = result._spans
        max_width: int,
        pad: bool = False,
        """Truncate text if it is longer that a given width.
            max_width (int): Maximum number of characters in text.
            overflow (str, optional): Overflow method: "crop", "fold", or "ellipsis". Defaults to None, to use self.overflow.
            pad (bool, optional): Pad with spaces if the length is less than max_width. Defaults to False.
        _overflow = overflow or self.overflow or DEFAULT_OVERFLOW
        if _overflow != "ignore":
            length = cell_len(self.plain)
            if length > max_width:
                if _overflow == "ellipsis":
                    self.plain = set_cell_size(self.plain, max_width - 1) + "…"
                    self.plain = set_cell_size(self.plain, max_width)
            if pad and length < max_width:
                spaces = max_width - length
                self._text = [f"{self.plain}{' ' * spaces}"]
    def _trim_spans(self) -> None:
        """Remove or modify any spans that are over the end of the text."""
        max_offset = len(self.plain)
                span
                if span.end < max_offset
                else _Span(span.start, min(max_offset, span.end), span.style)
            for span in self._spans
            if span.start < max_offset
    def pad(self, count: int, character: str = " ") -> None:
        """Pad left and right with a given number of characters.
            count (int): Width of padding.
            character (str): The character to pad with. Must be a string of length 1.
        assert len(character) == 1, "Character must be a string of length 1"
        if count:
            pad_characters = character * count
            self.plain = f"{pad_characters}{self.plain}{pad_characters}"
                _Span(start + count, end + count, style)
    def pad_left(self, count: int, character: str = " ") -> None:
        """Pad the left with a given character.
            count (int): Number of characters to pad.
            character (str, optional): Character to pad with. Defaults to " ".
            self.plain = f"{character * count}{self.plain}"
    def pad_right(self, count: int, character: str = " ") -> None:
        """Pad the right with a given character.
            self.plain = f"{self.plain}{character * count}"
    def align(self, align: AlignMethod, width: int, character: str = " ") -> None:
        """Align text to a given width.
            align (AlignMethod): One of "left", "center", or "right".
            width (int): Desired width.
        self.truncate(width)
        excess_space = width - cell_len(self.plain)
        if excess_space:
            if align == "left":
                self.pad_right(excess_space, character)
            elif align == "center":
                left = excess_space // 2
                self.pad_left(left, character)
                self.pad_right(excess_space - left, character)
                self.pad_left(excess_space, character)
    def append(
        self, text: Union["Text", str], style: Optional[Union[str, "Style"]] = None
        """Add text with an optional style.
            text (Union[Text, str]): A str or Text to append.
            style (str, optional): A style name. Defaults to None.
            Text: Returns self for chaining.
        if not isinstance(text, (str, Text)):
            raise TypeError("Only str or Text can be appended to Text")
        if len(text):
            if isinstance(text, str):
                self._text.append(sanitized_text)
                offset = len(self)
                text_length = len(sanitized_text)
                    self._spans.append(Span(offset, offset + text_length, style))
                self._length += text_length
            elif isinstance(text, Text):
                if style is not None:
                        "style must not be set when appending Text instance"
                text_length = self._length
                    self._spans.append(
                        _Span(text_length, text_length + len(text), text.style)
                self._text.append(text.plain)
                self._spans.extend(
                    _Span(start + text_length, end + text_length, style)
                    for start, end, style in text._spans.copy()
                self._length += len(text)
    def append_text(self, text: "Text") -> "Text":
        """Append another Text instance. This method is more performant that Text.append, but
        only works for Text.
            text (Text): The Text instance to append to this instance.
            self._spans.append(_Span(text_length, text_length + len(text), text.style))
    def append_tokens(
        self, tokens: Iterable[Tuple[str, Optional[StyleType]]]
        """Append iterable of str and style. Style may be a Style instance or a str style definition.
            tokens (Iterable[Tuple[str, Optional[StyleType]]]): An iterable of tuples containing str content and style.
        append_text = self._text.append
        for content, style in tokens:
            content = strip_control_codes(content)
            append_text(content)
                append_span(_Span(offset, offset + len(content), style))
            offset += len(content)
        self._length = offset
    def copy_styles(self, text: "Text") -> None:
        """Copy styles from another Text instance.
            text (Text): A Text instance to copy styles from, must be the same length.
        self._spans.extend(text._spans)
    def split(
        separator: str = "\n",
        include_separator: bool = False,
        allow_blank: bool = False,
    ) -> Lines:
        """Split rich text in to lines, preserving styles.
            separator (str, optional): String to split on. Defaults to "\\\\n".
            include_separator (bool, optional): Include the separator in the lines. Defaults to False.
            allow_blank (bool, optional): Return a blank line if the text ends with a separator. Defaults to False.
            List[RichText]: A list of rich text, one per line of the original.
        assert separator, "separator must not be empty"
        if separator not in text:
            return Lines([self.copy()])
        if include_separator:
            lines = self.divide(
                match.end() for match in re.finditer(re.escape(separator), text)
            def flatten_spans() -> Iterable[int]:
                for match in re.finditer(re.escape(separator), text):
                    yield end
            lines = Lines(
                line for line in self.divide(flatten_spans()) if line.plain != separator
        if not allow_blank and text.endswith(separator):
            lines.pop()
    def divide(self, offsets: Iterable[int]) -> Lines:
        """Divide text in to a number of lines at given offsets.
            offsets (Iterable[int]): Offsets used to divide text.
            Lines: New RichText instances between offsets.
        _offsets = list(offsets)
        if not _offsets:
        text_length = len(text)
        divide_offsets = [0, *_offsets, text_length]
        line_ranges = list(zip(divide_offsets, divide_offsets[1:]))
        style = self.style
        justify = self.justify
        overflow = self.overflow
        new_lines = Lines(
            _Text(
                text[start:end],
            for start, end in line_ranges
            return new_lines
        _line_appends = [line._spans.append for line in new_lines._lines]
        line_count = len(line_ranges)
        for span_start, span_end, style in self._spans:
            lower_bound = 0
            upper_bound = line_count
            start_line_no = (lower_bound + upper_bound) // 2
                line_start, line_end = line_ranges[start_line_no]
                if span_start < line_start:
                    upper_bound = start_line_no - 1
                elif span_start > line_end:
                    lower_bound = start_line_no + 1
            if span_end < line_end:
                end_line_no = start_line_no
                end_line_no = lower_bound = start_line_no
                    line_start, line_end = line_ranges[end_line_no]
                    if span_end < line_start:
                        upper_bound = end_line_no - 1
                    elif span_end > line_end:
                        lower_bound = end_line_no + 1
                    end_line_no = (lower_bound + upper_bound) // 2
            for line_no in range(start_line_no, end_line_no + 1):
                line_start, line_end = line_ranges[line_no]
                new_start = max(0, span_start - line_start)
                new_end = min(span_end - line_start, line_end - line_start)
                if new_end > new_start:
                    _line_appends[line_no](_Span(new_start, new_end, style))
    def right_crop(self, amount: int = 1) -> None:
        """Remove a number of characters from the end of the text."""
        max_offset = len(self.plain) - amount
        self._text = [self.plain[:-amount]]
        self._length -= amount
    def wrap(
        """Word wrap the text.
            justify (str, optional): Justify method: "default", "left", "center", "full", "right". Defaults to "default".
            overflow (str, optional): Overflow method: "crop", "fold", or "ellipsis". Defaults to None.
            tab_size (int, optional): Default tab size. Defaults to 8.
            no_wrap (bool, optional): Disable wrapping, Defaults to False.
            Lines: Number of lines.
        wrap_justify = justify or self.justify or DEFAULT_JUSTIFY
        wrap_overflow = overflow or self.overflow or DEFAULT_OVERFLOW
        no_wrap = pick_bool(no_wrap, self.no_wrap, False) or overflow == "ignore"
        lines = Lines()
        for line in self.split(allow_blank=True):
            if "\t" in line:
                line.expand_tabs(tab_size)
            if no_wrap:
                new_lines = Lines([line])
                offsets = divide_line(str(line), width, fold=wrap_overflow == "fold")
                new_lines = line.divide(offsets)
            for line in new_lines:
                line.rstrip_end(width)
            if wrap_justify:
                new_lines.justify(
                    console, width, justify=wrap_justify, overflow=wrap_overflow
                line.truncate(width, overflow=wrap_overflow)
            lines.extend(new_lines)
    def fit(self, width: int) -> Lines:
        """Fit the text in to given width by chopping in to lines.
            width (int): Maximum characters in a line.
            Lines: Lines container.
        lines: Lines = Lines()
        append = lines.append
        for line in self.split():
            line.set_length(width)
    def detect_indentation(self) -> int:
        """Auto-detect indentation of code.
            int: Number of spaces used to indent code.
        _indentations = {
            len(match.group(1))
            for match in re.finditer(r"^( *)(.*)$", self.plain, flags=re.MULTILINE)
            indentation = (
                reduce(gcd, [indent for indent in _indentations if not indent % 2]) or 1
            indentation = 1
        return indentation
    def with_indent_guides(
        indent_size: Optional[int] = None,
        character: str = "│",
        style: StyleType = "dim green",
        """Adds indent guide lines to text.
            indent_size (Optional[int]): Size of indentation, or None to auto detect. Defaults to None.
            character (str, optional): Character to use for indentation. Defaults to "│".
            style (Union[Style, str], optional): Style of indent guides.
            Text: New text with indentation guides.
        _indent_size = self.detect_indentation() if indent_size is None else indent_size
        text = self.copy()
        text.expand_tabs()
        indent_line = f"{character}{' ' * (_indent_size - 1)}"
        re_indent = re.compile(r"^( *)(.*)$")
        new_lines: List[Text] = []
        add_line = new_lines.append
        blank_lines = 0
        for line in text.split(allow_blank=True):
            match = re_indent.match(line.plain)
            if not match or not match.group(2):
                blank_lines += 1
            indent = match.group(1)
            full_indents, remaining_space = divmod(len(indent), _indent_size)
            new_indent = f"{indent_line * full_indents}{' ' * remaining_space}"
            line.plain = new_indent + line.plain[len(new_indent) :]
            line.stylize(style, 0, len(new_indent))
            if blank_lines:
                new_lines.extend([Text(new_indent, style=style)] * blank_lines)
            add_line(line)
            new_lines.extend([Text("", style=style)] * blank_lines)
        new_text = text.blank_copy("\n").join(new_lines)
        """\nLorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.\n"""
    text.highlight_words(["Lorem"], "bold")
    text.highlight_words(["ipsum"], "italic")
    console.rule("justify='left'")
    console.print(text, style="red")
    console.rule("justify='center'")
    console.print(text, style="green", justify="center")
    console.rule("justify='right'")
    console.print(text, style="blue", justify="right")
    console.rule("justify='full'")
    console.print(text, style="magenta", justify="full")
    pygments.lexers.text
    ~~~~~~~~~~~~~~~~~~~~
    Lexers for non-source code file types.
from pygments.lexers.configs import ApacheConfLexer, NginxConfLexer, \
    SquidConfLexer, LighttpdConfLexer, IniLexer, RegeditLexer, PropertiesLexer, \
    UnixConfigLexer
from pygments.lexers.console import PyPyLogLexer
from pygments.lexers.textedit import VimLexer
from pygments.lexers.markup import BBCodeLexer, MoinWikiLexer, RstLexer, \
    TexLexer, GroffLexer
from pygments.lexers.installers import DebianControlLexer, DebianSourcesLexer, SourcesListLexer
from pygments.lexers.make import MakefileLexer, BaseMakefileLexer, CMakeLexer
from pygments.lexers.haxe import HxmlLexer
from pygments.lexers.sgf import SmartGameFormatLexer
from pygments.lexers.diff import DiffLexer, DarcsPatchLexer
from pygments.lexers.data import YamlLexer
from pygments.lexers.textfmts import IrcLogsLexer, GettextLexer, HttpLexer
Text-based visual representations of graphs
from networkx.utils import open_file
__all__ = ["generate_network_text", "write_network_text"]
class BaseGlyphs:
    def as_dict(cls):
            a: getattr(cls, a)
            for a in dir(cls)
            if not a.startswith("_") and a != "as_dict"
class AsciiBaseGlyphs(BaseGlyphs):
    empty: str = "+"
    newtree_last: str = "+-- "
    newtree_mid: str = "+-- "
    endof_forest: str = "    "
    within_forest: str = ":   "
    within_tree: str = "|   "
class AsciiDirectedGlyphs(AsciiBaseGlyphs):
    last: str = "L-> "
    mid: str = "|-> "
    backedge: str = "<-"
    vertical_edge: str = "!"
class AsciiUndirectedGlyphs(AsciiBaseGlyphs):
    last: str = "L-- "
    mid: str = "|-- "
    backedge: str = "-"
    vertical_edge: str = "|"
class UtfBaseGlyphs(BaseGlyphs):
    # Notes on available box and arrow characters
    # https://en.wikipedia.org/wiki/Box-drawing_character
    # https://stackoverflow.com/questions/2701192/triangle-arrow
    empty: str = "╙"
    newtree_last: str = "╙── "
    newtree_mid: str = "╟── "
    within_forest: str = "╎   "
    within_tree: str = "│   "
class UtfDirectedGlyphs(UtfBaseGlyphs):
    last: str = "└─╼ "
    mid: str = "├─╼ "
    backedge: str = "╾"
    vertical_edge: str = "╽"
class UtfUndirectedGlyphs(UtfBaseGlyphs):
    last: str = "└── "
    mid: str = "├── "
    backedge: str = "─"
    vertical_edge: str = "│"
def generate_network_text(
    graph,
    with_labels=True,
    sources=None,
    max_depth=None,
    ascii_only=False,
    vertical_chains=False,
    """Generate lines in the "network text" format
    This works via a depth-first traversal of the graph and writing a line for
    each unique node encountered. Non-tree edges are written to the right of
    each node, and connection to a non-tree edge is indicated with an ellipsis.
    This representation works best when the input graph is a forest, but any
    graph can be represented.
    This notation is original to networkx, although it is simple enough that it
    may be known in existing literature. See #5602 for details. The procedure
    is summarized as follows:
    1. Given a set of source nodes (which can be specified, or automatically
    discovered via finding the (strongly) connected components and choosing one
    node with minimum degree from each), we traverse the graph in depth first
    order.
    2. Each reachable node will be printed exactly once on it's own line.
    3. Edges are indicated in one of four ways:
        a. a parent "L-style" connection on the upper left. This corresponds to
        a traversal in the directed DFS tree.
        b. a backref "<-style" connection shown directly on the right. For
        directed graphs, these are drawn for any incoming edges to a node that
        is not a parent edge. For undirected graphs, these are drawn for only
        the non-parent edges that have already been represented (The edges that
        have not been represented will be handled in the recursive case).
        c. a child "L-style" connection on the lower right. Drawing of the
        children are handled recursively.
        d. if ``vertical_chains`` is true, and a parent node only has one child
        a "vertical-style" edge is drawn between them.
    4. The children of each node (wrt the directed DFS tree) are drawn
    underneath and to the right of it. In the case that a child node has already
    been drawn the connection is replaced with an ellipsis ("...") to indicate
    that there is one or more connections represented elsewhere.
    5. If a maximum depth is specified, an edge to nodes past this maximum
    depth will be represented by an ellipsis.
    6. If a node has a truthy "collapse" value, then we do not traverse past
    that node.
    graph : nx.DiGraph | nx.Graph
        Graph to represent
    with_labels : bool | str
        If True will use the "label" attribute of a node to display if it
        exists otherwise it will use the node value itself. If given as a
        string, then that attribute name will be used instead of "label".
    sources : List
        Specifies which nodes to start traversal from. Note: nodes that are not
        reachable from one of these sources may not be shown. If unspecified,
        the minimal set of nodes needed to reach all others will be used.
    max_depth : int | None
        The maximum depth to traverse before stopping. Defaults to None.
    ascii_only : Boolean
        If True only ASCII characters are used to construct the visualization
    vertical_chains : Boolean
        If True, chains of nodes will be drawn vertically when possible.
    Yields
    str : a line of generated text
    >>> graph = nx.path_graph(10)
    >>> graph.add_node("A")
    >>> graph.add_node("B")
    >>> graph.add_node("C")
    >>> graph.add_node("D")
    >>> graph.add_edge(9, "A")
    >>> graph.add_edge(9, "B")
    >>> graph.add_edge(9, "C")
    >>> graph.add_edge("C", "D")
    >>> graph.add_edge("C", "E")
    >>> graph.add_edge("C", "F")
    >>> nx.write_network_text(graph)
    ╙── 0
        └── 1
            └── 2
                └── 3
                    └── 4
                        └── 5
                            └── 6
                                └── 7
                                    └── 8
                                        └── 9
                                            ├── A
                                            ├── B
                                            └── C
                                                ├── D
                                                ├── E
                                                └── F
    >>> nx.write_network_text(graph, vertical_chains=True)
        8
        9
    class StackFrame(NamedTuple):
        parent: Any
        node: Any
        indents: list
        this_islast: bool
        this_vertical: bool
    collapse_attr = "collapse"
    is_directed = graph.is_directed()
    if is_directed:
        glyphs = AsciiDirectedGlyphs if ascii_only else UtfDirectedGlyphs
        succ = graph.succ
        pred = graph.pred
        glyphs = AsciiUndirectedGlyphs if ascii_only else UtfUndirectedGlyphs
        succ = graph.adj
        pred = graph.adj
    if isinstance(with_labels, str):
        label_attr = with_labels
    elif with_labels:
        label_attr = "label"
        label_attr = None
    if max_depth == 0:
        yield glyphs.empty + " ..."
    elif len(graph.nodes) == 0:
        yield glyphs.empty
        # If the nodes to traverse are unspecified, find the minimal set of
        # nodes that will reach the entire graph
        if sources is None:
            sources = _find_sources(graph)
        # Populate the stack with each:
        # 1. parent node in the DFS tree (or None for root nodes),
        # 2. the current node in the DFS tree
        # 2. a list of indentations indicating depth
        # 3. a flag indicating if the node is the final one to be written.
        # Reverse the stack so sources are popped in the correct order.
        last_idx = len(sources) - 1
        stack = [
            StackFrame(None, node, [], (idx == last_idx), False)
            for idx, node in enumerate(sources)
        ][::-1]
        num_skipped_children = defaultdict(lambda: 0)
        seen_nodes = set()
            parent, node, indents, this_islast, this_vertical = stack.pop()
            if node is not Ellipsis:
                skip = node in seen_nodes
                    # Mark that we skipped a parent's child
                    num_skipped_children[parent] += 1
                if this_islast:
                    # If we reached the last child of a parent, and we skipped
                    # any of that parents children, then we should emit an
                    # ellipsis at the end after this.
                    if num_skipped_children[parent] and parent is not None:
                        # Append the ellipsis to be emitted last
                        next_islast = True
                        try_frame = StackFrame(
                            node, Ellipsis, indents, next_islast, False
                        stack.append(try_frame)
                        # Redo this frame, but not as a last object
                        next_islast = False
                            parent, node, indents, next_islast, this_vertical
                seen_nodes.add(node)
            if not indents:
                # Top level items (i.e. trees in the forest) get different
                # glyphs to indicate they are not actually connected
                    this_vertical = False
                    this_prefix = indents + [glyphs.newtree_last]
                    next_prefix = indents + [glyphs.endof_forest]
                    this_prefix = indents + [glyphs.newtree_mid]
                    next_prefix = indents + [glyphs.within_forest]
                # Non-top-level items
                if this_vertical:
                    this_prefix = indents
                    next_prefix = indents
                        this_prefix = indents + [glyphs.last]
                        this_prefix = indents + [glyphs.mid]
                        next_prefix = indents + [glyphs.within_tree]
            if node is Ellipsis:
                label = " ..."
                if label_attr is not None:
                    label = str(graph.nodes[node].get(label_attr, node))
                    label = str(node)
                # Determine if we want to show the children of this node.
                if collapse_attr is not None:
                    collapse = graph.nodes[node].get(collapse_attr, False)
                    collapse = False
                # Determine:
                # (1) children to traverse into after showing this node.
                # (2) parents to immediately show to the right of this node.
                    # In the directed case we must show every successor node
                    # note: it may be skipped later, but we don't have that
                    # information here.
                    children = list(succ[node])
                    # In the directed case we must show every predecessor
                    # except for parent we directly traversed from.
                    handled_parents = {parent}
                    # Showing only the unseen children results in a more
                    # concise representation for the undirected case.
                    children = [
                        child for child in succ[node] if child not in seen_nodes
                    # In the undirected case, parents are also children, so we
                    # only need to immediately show the ones we can no longer
                    # traverse
                    handled_parents = {*children, parent}
                if max_depth is not None and len(indents) == max_depth - 1:
                    # Use ellipsis to indicate we have reached maximum depth
                        children = [Ellipsis]
                if collapse:
                    # Collapsing a node is the same as reaching maximum depth
                # The other parents are other predecessors of this node that
                # are not handled elsewhere.
                other_parents = [p for p in pred[node] if p not in handled_parents]
                if other_parents:
                        other_parents_labels = ", ".join(
                                str(graph.nodes[p].get(label_attr, p))
                                for p in other_parents
                            [str(p) for p in other_parents]
                    suffix = " ".join(["", glyphs.backedge, other_parents_labels])
            # Emit the line for this node, this will be called for each node
            # exactly once.
                yield "".join(this_prefix + [glyphs.vertical_edge])
            yield "".join(this_prefix + [label, suffix])
            if vertical_chains:
                    num_children = len(set(children))
                    num_children = len(set(children) - {parent})
                # The next node can be drawn vertically if it is the only
                # remaining child of this node.
                next_is_vertical = num_children == 1
                next_is_vertical = False
            # Push children on the stack in reverse order so they are popped in
            # the original order.
            for idx, child in enumerate(children[::-1]):
                next_islast = idx == 0
                    node, child, next_prefix, next_islast, next_is_vertical
@open_file(1, "w")
def write_network_text(
    """Creates a nice text representation of a graph
    path : string or file or callable or None
       Filename or file handle for data output.
       if a function, then it will be called for each generated line.
       if None, this will default to "sys.stdout.write"
    end : string
        The line ending character
    >>> graph = nx.balanced_tree(r=2, h=2, create_using=nx.DiGraph)
        ├─╼ 1
        │   ├─╼ 3
        │   └─╼ 4
        └─╼ 2
            ├─╼ 5
            └─╼ 6
    >>> # A near tree with one non-tree edge
    >>> graph.add_edge(5, 1)
        ├─╼ 1 ╾ 5
            │   └─╼  ...
    >>> graph = nx.cycle_graph(5)
        ├── 1
        │   └── 2
        │       └── 3
        │           └── 4 ─ 0
        └──  ...
    >>> graph = nx.cycle_graph(5, nx.DiGraph)
    ╙── 0 ╾ 4
        ╽
        └─╼  ...
    >>> nx.write_network_text(graph, vertical_chains=True, ascii_only=True)
    +-- 0 <- 4
        L->  ...
    >>> graph = nx.generators.barbell_graph(4, 2)
    >>> nx.write_network_text(graph, vertical_chains=False)
    ╙── 4
        ├── 5
        │   └── 6
        │       ├── 7
        │       │   ├── 8 ─ 6
        │       │   │   └── 9 ─ 6, 7
        │       │   └──  ...
        │       └──  ...
            ├── 0
            │   ├── 1 ─ 3
            │   │   └── 2 ─ 0, 3
            │   └──  ...
        │   │
        │   6
        │   ├── 7
        │   │   ├── 8 ─ 6
        │   │   │   │
        │   │   │   9 ─ 6, 7
        │   │   └──  ...
            │   │   │
            │   │   2 ─ 0, 3
    >>> graph = nx.complete_graph(5, create_using=nx.Graph)
        │   ├── 2 ─ 0
        │   │   ├── 3 ─ 0, 1
        │   │   │   └── 4 ─ 0, 1, 2
    >>> graph = nx.complete_graph(3, create_using=nx.DiGraph)
    ╙── 0 ╾ 1, 2
        ├─╼ 1 ╾ 2
        │   ├─╼ 2 ╾ 0
        │   │   └─╼  ...
        # The path is unspecified, write to stdout
        _write = sys.stdout.write
    elif hasattr(path, "write"):
        # The path is already an open file
        _write = path.write
    elif callable(path):
        # The path is a custom callable
        _write = path
        raise TypeError(type(path))
    for line in generate_network_text(
        with_labels=with_labels,
        sources=sources,
        max_depth=max_depth,
        ascii_only=ascii_only,
        vertical_chains=vertical_chains,
        _write(line + end)
def _find_sources(graph):
    Determine a minimal set of nodes such that the entire graph is reachable
    # For each connected part of the graph, choose at least
    # one node as a starting point, preferably without a parent
    if graph.is_directed():
        # Choose one node from each SCC with minimum in_degree
        sccs = list(nx.strongly_connected_components(graph))
        # condensing the SCCs forms a dag, the nodes in this graph with
        # 0 in-degree correspond to the SCCs from which the minimum set
        # of nodes from which all other nodes can be reached.
        scc_graph = nx.condensation(graph, sccs)
        supernode_to_nodes = {sn: [] for sn in scc_graph.nodes()}
        # Note: the order of mapping differs between pypy and cpython
        # so we have to loop over graph nodes for consistency
        mapping = scc_graph.graph["mapping"]
        for n in graph.nodes:
            sn = mapping[n]
            supernode_to_nodes[sn].append(n)
        sources = []
        for sn in scc_graph.nodes():
            if scc_graph.in_degree[sn] == 0:
                scc = supernode_to_nodes[sn]
                node = min(scc, key=lambda n: graph.in_degree[n])
                sources.append(node)
        # For undirected graph, the entire graph will be reachable as
        # long as we consider one node from every connected component
            min(cc, key=lambda n: graph.degree[n])
            for cc in nx.connected_components(graph)
        sources = sorted(sources, key=lambda n: graph.degree[n])
    return sources
def _parse_network_text(lines):
    """Reconstructs a graph from a network text representation.
    This is mainly used for testing.  Network text is for display, not
    serialization, as such this cannot parse all network text representations
    because node labels can be ambiguous with the glyphs and indentation used
    to represent edge structure. Additionally, there is no way to determine if
    disconnected graphs were originally directed or undirected.
    lines : list or iterator of strings
        Input data in network text format
    G: NetworkX graph
        The graph corresponding to the lines in network text format.
    from typing import Any, NamedTuple, Union
    class ParseStackFrame(NamedTuple):
        indent: int
        has_vertical_child: int | None
    initial_line_iter = iter(lines)
    is_ascii = None
    is_directed = None
    ##############
    # Initial Pass
    # Do an initial pass over the lines to determine what type of graph it is.
    # Remember what these lines were, so we can reiterate over them in the
    # parsing pass.
    initial_lines = []
        first_line = next(initial_line_iter)
        initial_lines.append(first_line)
        # The first character indicates if it is an ASCII or UTF graph
        first_char = first_line[0]
        if first_char in {
            UtfBaseGlyphs.empty,
            UtfBaseGlyphs.newtree_mid[0],
            UtfBaseGlyphs.newtree_last[0],
        elif first_char in {
            AsciiBaseGlyphs.empty,
            AsciiBaseGlyphs.newtree_mid[0],
            AsciiBaseGlyphs.newtree_last[0],
            raise AssertionError(f"Unexpected first character: {first_char}")
    if is_ascii:
        directed_glyphs = AsciiDirectedGlyphs.as_dict()
        undirected_glyphs = AsciiUndirectedGlyphs.as_dict()
        directed_glyphs = UtfDirectedGlyphs.as_dict()
        undirected_glyphs = UtfUndirectedGlyphs.as_dict()
    # For both directed / undirected glyphs, determine which glyphs never
    # appear as substrings in the other undirected / directed glyphs.  Glyphs
    # with this property unambiguously indicates if a graph is directed /
    # undirected.
    directed_items = set(directed_glyphs.values())
    undirected_items = set(undirected_glyphs.values())
    unambiguous_directed_items = []
    for item in directed_items:
        other_items = undirected_items
        other_supersets = [other for other in other_items if item in other]
        if not other_supersets:
            unambiguous_directed_items.append(item)
    unambiguous_undirected_items = []
    for item in undirected_items:
        other_items = directed_items
            unambiguous_undirected_items.append(item)
    for line in initial_line_iter:
        initial_lines.append(line)
        if any(item in line for item in unambiguous_undirected_items):
            is_directed = False
        elif any(item in line for item in unambiguous_directed_items):
            is_directed = True
    if is_directed is None:
        # Not enough information to determine, choose undirected by default
    glyphs = directed_glyphs if is_directed else undirected_glyphs
    # the backedge symbol by itself can be ambiguous, but with spaces around it
    # becomes unambiguous.
    backedge_symbol = " " + glyphs["backedge"] + " "
    # Reconstruct an iterator over all of the lines.
    parsing_line_iter = chain(initial_lines, initial_line_iter)
    # Parsing Pass
    is_empty = None
    noparent = object()  # sentinel value
    # keep a stack of previous nodes that could be parents of subsequent nodes
    stack = [ParseStackFrame(noparent, -1, None)]
    for line in parsing_line_iter:
        if line == glyphs["empty"]:
            # If the line is the empty glyph, we are done.
            # There shouldn't be anything else after this.
            is_empty = True
        if backedge_symbol in line:
            # This line has one or more backedges, separate those out
            node_part, backedge_part = line.split(backedge_symbol)
            backedge_nodes = [u.strip() for u in backedge_part.split(", ")]
            # Now the node can be parsed
            node_part = node_part.rstrip()
            prefix, node = node_part.rsplit(" ", 1)
            node = node.strip()
            # Add the backedges to the edge list
            edges.extend([(u, node) for u in backedge_nodes])
            # No backedge, the tail of this line is the node
            prefix, node = line.rsplit(" ", 1)
        prev = stack.pop()
        if node in glyphs["vertical_edge"]:
            # Previous node is still the previous node, but we know it will
            # have exactly one child, which will need to have its nesting level
            # adjusted.
            modified_prev = ParseStackFrame(
                prev.node,
                prev.indent,
            stack.append(modified_prev)
        # The length of the string before the node characters give us a hint
        # about our nesting level. The only case where this doesn't work is
        # when there are vertical chains, which is handled explicitly.
        indent = len(prefix)
        curr = ParseStackFrame(node, indent, None)
        if prev.has_vertical_child:
            # In this case we know prev must be the parent of our current line,
            # so we don't have to search the stack. (which is good because the
            # indentation check wouldn't work in this case).
            # If the previous node nesting-level is greater than the current
            # nodes nesting-level than the previous node was the end of a path,
            # and is not our parent. We can safely pop nodes off the stack
            # until we find one with a comparable nesting-level, which is our
            # parent.
            while curr.indent <= prev.indent:
        if node == "...":
            # The current previous node is no longer a valid parent,
            # keep it popped from the stack.
            stack.append(prev)
            # The previous and current nodes may still be parents, so add them
            # back onto the stack.
            stack.append(curr)
            # Add the node and the edge to its parent to the node / edge lists.
            nodes.append(curr.node)
            if prev.node is not noparent:
                edges.append((prev.node, curr.node))
        # Sanity check
        assert len(nodes) == 0
    # Reconstruct the graph
    cls = nx.DiGraph if is_directed else nx.Graph
    new = cls()
    new.add_nodes_from(nodes)
    new.add_edges_from(edges)
# Skip text characters for text token, place those to pending buffer
# and increment current pos
# Rule to skip pure text
# '{}$%@~+=:' reserved for extensions
# !!!! Don't confuse with "Markdown ASCII Punctuation" chars
# http://spec.commonmark.org/0.15/#ascii-punctuation-character
_TerminatorChars = {
def _terminator_char_regex() -> re.Pattern[str]:
    return re.compile("[" + re.escape("".join(_TerminatorChars)) + "]")
def text(state: StateInline, silent: bool) -> bool:
    pos = state.pos
    posMax = state.posMax
    terminator_char = _terminator_char_regex().search(state.src, pos)
    pos = terminator_char.start() if terminator_char else posMax
    if pos == state.pos:
        state.pending += state.src[state.pos : pos]
    "TextConnectable",
    "TextReceiveStream",
    "TextSendStream",
    "TextStream",
from dataclasses import InitVar, dataclass, field
from ..abc import (
    AnyByteReceiveStream,
    AnyByteSendStream,
    AnyByteStream,
    AnyByteStreamConnectable,
    ObjectReceiveStream,
    ObjectSendStream,
    ObjectStream,
    ObjectStreamConnectable,
    from typing import override
@dataclass(eq=False)
class TextReceiveStream(ObjectReceiveStream[str]):
    Stream wrapper that decodes bytes to strings using the given encoding.
    Decoding is done using :class:`~codecs.IncrementalDecoder` which returns any
    completely received unicode characters as soon as they come in.
    :param transport_stream: any bytes-based receive stream
    :param encoding: character encoding to use for decoding bytes to strings (defaults
        to ``utf-8``)
    :param errors: handling scheme for decoding errors (defaults to ``strict``; see the
        `codecs module documentation`_ for a comprehensive list of options)
    .. _codecs module documentation:
        https://docs.python.org/3/library/codecs.html#codec-objects
    transport_stream: AnyByteReceiveStream
    encoding: InitVar[str] = "utf-8"
    errors: InitVar[str] = "strict"
    _decoder: codecs.IncrementalDecoder = field(init=False)
    def __post_init__(self, encoding: str, errors: str) -> None:
        decoder_class = codecs.getincrementaldecoder(encoding)
        self._decoder = decoder_class(errors=errors)
    async def receive(self) -> str:
            chunk = await self.transport_stream.receive()
            decoded = self._decoder.decode(chunk)
        await self.transport_stream.aclose()
        self._decoder.reset()
        return self.transport_stream.extra_attributes
class TextSendStream(ObjectSendStream[str]):
    Sends strings to the wrapped stream as bytes using the given encoding.
    :param AnyByteSendStream transport_stream: any bytes-based send stream
    :param str encoding: character encoding to use for encoding strings to bytes
        (defaults to ``utf-8``)
    :param str errors: handling scheme for encoding errors (defaults to ``strict``; see
        the `codecs module documentation`_ for a comprehensive list of options)
    transport_stream: AnyByteSendStream
    errors: str = "strict"
    _encoder: Callable[..., tuple[bytes, int]] = field(init=False)
    def __post_init__(self, encoding: str) -> None:
        self._encoder = codecs.getencoder(encoding)
    async def send(self, item: str) -> None:
        encoded = self._encoder(item, self.errors)[0]
        await self.transport_stream.send(encoded)
class TextStream(ObjectStream[str]):
    A bidirectional stream that decodes bytes to strings on receive and encodes strings
    to bytes on send.
    Extra attributes will be provided from both streams, with the receive stream
    providing the values in case of a conflict.
    :param AnyByteStream transport_stream: any bytes-based stream
    :param str encoding: character encoding to use for encoding/decoding strings to/from
        bytes (defaults to ``utf-8``)
    transport_stream: AnyByteStream
    _receive_stream: TextReceiveStream = field(init=False)
    _send_stream: TextSendStream = field(init=False)
        self._receive_stream = TextReceiveStream(
            self.transport_stream, encoding=encoding, errors=errors
        self._send_stream = TextSendStream(
        return await self._receive_stream.receive()
        await self._send_stream.send(item)
        await self.transport_stream.send_eof()
        await self._send_stream.aclose()
        await self._receive_stream.aclose()
            **self._send_stream.extra_attributes,
            **self._receive_stream.extra_attributes,
class TextConnectable(ObjectStreamConnectable[str]):
    def __init__(self, connectable: AnyByteStreamConnectable):
        :param connectable: the bytestream endpoint to wrap
        self.connectable = connectable
    async def connect(self) -> TextStream:
        stream = await self.connectable.connect()
        return TextStream(stream)
        """Append another Text instance. This method is more performant than Text.append, but
        """Divide text into a number of lines at given offsets.
                if overflow == "ignore":
                    lines.append(line)
