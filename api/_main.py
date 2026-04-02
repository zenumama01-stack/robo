from . import chat, audio, files, image, models, completions, fine_tuning
def register_commands(parser: ArgumentParser) -> None:
    subparsers = parser.add_subparsers(help="All API subcommands")
    chat.register(subparsers)
    image.register(subparsers)
    audio.register(subparsers)
    files.register(subparsers)
    models.register(subparsers)
    completions.register(subparsers)
    fine_tuning.register(subparsers)
from . import migrate, fine_tunes
def register_commands(parser: ArgumentParser, subparser: _SubParsersAction[ArgumentParser]) -> None:
    migrate.register(subparser)
    namespaced = parser.add_subparsers(title="Tools", help="Convenience client side tools")
    fine_tunes.register(namespaced)
# Secret Labs' Regular Expression Engine
# Copyright (c) 1998-2001 by Secret Labs AB.  All rights reserved.
# This version of the SRE library can be redistributed under CNRI's
# Python 1.6 license.  For any other use, please contact Secret Labs
# AB (info@pythonware.com).
# Portions of this engine have been developed in cooperation with
# CNRI.  Hewlett-Packard provided funding for 1.6 integration and
# other compatibility work.
# 2010-01-16 mrab Python front-end re-written and extended
r"""Support for regular expressions (RE).
This module provides regular expression matching operations similar to those
found in Perl. It supports both 8-bit and Unicode strings; both the pattern and
the strings being processed can contain null bytes and characters outside the
US ASCII range.
Regular expressions can contain both special and ordinary characters. Most
ordinary characters, like "A", "a", or "0", are the simplest regular
expressions; they simply match themselves. You can concatenate ordinary
characters, so last matches the string 'last'.
There are a few differences between the old (legacy) behaviour and the new
(enhanced) behaviour, which are indicated by VERSION0 or VERSION1.
The special characters are:
    "."                 Matches any character except a newline.
    "^"                 Matches the start of the string.
    "$"                 Matches the end of the string or just before the
                        newline at the end of the string.
    "*"                 Matches 0 or more (greedy) repetitions of the preceding
                        RE. Greedy means that it will match as many repetitions
                        as possible.
    "+"                 Matches 1 or more (greedy) repetitions of the preceding
                        RE.
    "?"                 Matches 0 or 1 (greedy) of the preceding RE.
    *?,+?,??            Non-greedy versions of the previous three special
    *+,++,?+            Possessive versions of the previous three special
    {m,n}               Matches from m to n repetitions of the preceding RE.
    {m,n}?              Non-greedy version of the above.
    {m,n}+              Possessive version of the above.
    {...}               Fuzzy matching constraints.
    "\\"                Either escapes special characters or signals a special
    [...]               Indicates a set of characters. A "^" as the first
                        character indicates a complementing set.
    "|"                 A|B, creates an RE that will match either A or B.
    (...)               Matches the RE inside the parentheses. The contents are
                        captured and can be retrieved or matched later in the
    (?flags-flags)      VERSION1: Sets/clears the flags for the remainder of
                        the group or pattern; VERSION0: Sets the flags for the
                        entire pattern.
    (?:...)             Non-capturing version of regular parentheses.
    (?>...)             Atomic non-capturing version of regular parentheses.
    (?flags-flags:...)  Non-capturing version of regular parentheses with local
                        flags.
    (?P<name>...)       The substring matched by the group is accessible by
    (?<name>...)        The substring matched by the group is accessible by
    (?P=name)           Matches the text matched earlier by the group named
    (?#...)             A comment; ignored.
    (?=...)             Matches if ... matches next, but doesn't consume the
    (?!...)             Matches if ... doesn't match next.
    (?<=...)            Matches if preceded by ....
    (?<!...)            Matches if not preceded by ....
    (?(id)yes|no)       Matches yes pattern if group id matched, the (optional)
                        no pattern otherwise.
    (?(DEFINE)...)      If there's no group called "DEFINE", then ... will be
                        ignored, but any group definitions will be available.
    (?|...|...)         (?|A|B), creates an RE that will match either A or B,
                        but reuses capture group numbers across the
                        alternatives.
    (*FAIL)             Forces matching to fail, which means immediate
                        backtracking.
    (*F)                Abbreviation for (*FAIL).
    (*PRUNE)            Discards the current backtracking information. Its
                        effect doesn't extend outside an atomic group or a
                        lookaround.
    (*SKIP)             Similar to (*PRUNE), except that it also sets where in
                        the text the next attempt at matching the entire
                        pattern will start. Its effect doesn't extend outside
                        an atomic group or a lookaround.
The fuzzy matching constraints are: "i" to permit insertions, "d" to permit
deletions, "s" to permit substitutions, "e" to permit any of these. Limits are
optional with "<=" and "<". If any type of error is provided then any type not
provided is not permitted.
A cost equation may be provided.
    (?:fuzzy){i<=2}
    (?:fuzzy){i<=1,s<=2,d<=1,1i+1s+1d<3}
VERSION1: Set operators are supported, and a set can include nested sets. The
set operators, in order of increasing precedence, are:
    ||  Set union ("x||y" means "x or y").
    ~~  (double tilde) Symmetric set difference ("x~~y" means "x or y, but not
        both").
    &&  Set intersection ("x&&y" means "x and y").
    --  (double dash) Set difference ("x--y" means "x but not y").
Implicit union, ie, simple juxtaposition like in [ab], has the highest
precedence.
VERSION0 and VERSION1:
The special sequences consist of "\\" and a character from the list below. If
the ordinary character is not on the list, then the resulting RE will match the
second character.
    \number         Matches the contents of the group of the same number if
                    number is no more than 2 digits, otherwise the character
                    with the 3-digit octal code.
    \a              Matches the bell character.
    \A              Matches only at the start of the string.
    \b              Matches the empty string, but only at the start or end of a
                    word.
    \B              Matches the empty string, but not at the start or end of a
    \d              Matches any decimal digit; equivalent to the set [0-9] when
                    matching a bytestring or a Unicode string with the ASCII
                    flag, or the whole range of Unicode digits when matching a
                    Unicode string.
    \D              Matches any non-digit character; equivalent to [^\d].
    \f              Matches the formfeed character.
    \g<name>        Matches the text matched by the group named name.
    \G              Matches the empty string, but only at the position where
                    the search started.
    \h              Matches horizontal whitespace.
    \K              Keeps only what follows for the entire match.
    \L<name>        Named list. The list is provided as a keyword argument.
    \m              Matches the empty string, but only at the start of a word.
    \M              Matches the empty string, but only at the end of a word.
    \n              Matches the newline character.
    \N{name}        Matches the named character.
    \p{name=value}  Matches the character if its property has the specified
    \P{name=value}  Matches the character if its property hasn't the specified
    \r              Matches the carriage-return character.
    \s              Matches any whitespace character; equivalent to
                    [ \t\n\r\f\v].
    \S              Matches any non-whitespace character; equivalent to [^\s].
    \t              Matches the tab character.
    \uXXXX          Matches the Unicode codepoint with 4-digit hex code XXXX.
    \UXXXXXXXX      Matches the Unicode codepoint with 8-digit hex code
                    XXXXXXXX.
    \v              Matches the vertical tab character.
    \w              Matches any alphanumeric character; equivalent to
                    [a-zA-Z0-9_] when matching a bytestring or a Unicode string
                    with the ASCII flag, or the whole range of Unicode
                    alphanumeric characters (letters plus digits plus
                    underscore) when matching a Unicode string. With LOCALE, it
                    will match the set [0-9_] plus characters defined as
                    letters for the current locale.
    \W              Matches the complement of \w; equivalent to [^\w].
    \xXX            Matches the character with 2-digit hex code XX.
    \X              Matches a grapheme.
    \Z              Matches only at the end of the string.
    \\              Matches a literal backslash.
This module exports the following functions:
    match      Match a regular expression pattern at the beginning of a string.
    fullmatch  Match a regular expression pattern against all of a string.
    search     Search a string for the presence of a pattern.
    sub        Substitute occurrences of a pattern found in a string using a
               template string.
    subf       Substitute occurrences of a pattern found in a string using a
               format string.
    subn       Same as sub, but also return the number of substitutions made.
    subfn      Same as subf, but also return the number of substitutions made.
    split      Split a string by the occurrences of a pattern. VERSION1: will
               split at zero-width match; VERSION0: won't split at zero-width
               match.
    splititer  Return an iterator yielding the parts of a split string.
    findall    Find all occurrences of a pattern in a string.
    finditer   Return an iterator yielding a match object for each match.
    compile    Compile a pattern into a Pattern object.
    purge      Clear the regular expression cache.
    escape     Backslash all non-alphanumerics or special characters in a
Most of the functions support a concurrent parameter: if True, the GIL will be
released during matching, allowing other Python threads to run concurrently. If
the string changes during matching, the behaviour is undefined. This parameter
is not needed when working on the builtin (immutable) string classes.
Some of the functions in this module take flags as optional parameters. Most of
these flags can also be set within an RE:
    A   a   ASCII         Make \w, \W, \b, \B, \d, and \D match the
                          corresponding ASCII character categories. Default
                          when matching a bytestring.
    B   b   BESTMATCH     Find the best fuzzy match (default is first).
    D       DEBUG         Print the parsed pattern.
    E   e   ENHANCEMATCH  Attempt to improve the fit after finding the first
                          fuzzy match.
    F   f   FULLCASE      Use full case-folding when performing
                          case-insensitive matching in Unicode.
    I   i   IGNORECASE    Perform case-insensitive matching.
    L   L   LOCALE        Make \w, \W, \b, \B, \d, and \D dependent on the
                          current locale. (One byte per character only.)
    M   m   MULTILINE     "^" matches the beginning of lines (after a newline)
                          as well as the string. "$" matches the end of lines
                          (before a newline) as well as the end of the string.
    P   p   POSIX         Perform POSIX-standard matching (leftmost longest).
    R   r   REVERSE       Searches backwards.
    S   s   DOTALL        "." matches any character at all, including the
                          newline.
    U   u   UNICODE       Make \w, \W, \b, \B, \d, and \D dependent on the
                          Unicode locale. Default when matching a Unicode
    V0  V0  VERSION0      Turn on the old legacy behaviour.
    V1  V1  VERSION1      Turn on the new enhanced behaviour. This flag
                          includes the FULLCASE flag.
    W   w   WORD          Make \b and \B work with default Unicode word breaks
                          and make ".", "^" and "$" work with Unicode line
                          breaks.
    X   x   VERBOSE       Ignore whitespace and comments for nicer looking REs.
This module also defines an exception 'error'.
# Public symbols.
__all__ = ["cache_all", "compile", "DEFAULT_VERSION", "escape", "findall",
  "finditer", "fullmatch", "match", "purge", "search", "split", "splititer",
  "sub", "subf", "subfn", "subn", "template", "Scanner", "A", "ASCII", "B",
  "BESTMATCH", "D", "DEBUG", "E", "ENHANCEMATCH", "S", "DOTALL", "F",
  "FULLCASE", "I", "IGNORECASE", "L", "LOCALE", "M", "MULTILINE", "P", "POSIX",
  "R", "REVERSE", "T", "TEMPLATE", "U", "UNICODE", "V0", "VERSION0", "V1",
  "VERSION1", "X", "VERBOSE", "W", "WORD", "error", "Regex", "__version__",
  "__doc__", "RegexFlag"]
__version__ = "2026.1.15"
# --------------------------------------------------------------------
# Public interface.
def match(pattern, string, flags=0, pos=None, endpos=None, partial=False,
  concurrent=None, timeout=None, ignore_unused=False, **kwargs):
    """Try to apply the pattern at the start of the string, returning a match
    object, or None if no match was found."""
    pat = _compile(pattern, flags, ignore_unused, kwargs, True)
    return pat.match(string, pos, endpos, concurrent, partial, timeout)
def fullmatch(pattern, string, flags=0, pos=None, endpos=None, partial=False,
    """Try to apply the pattern against all of the string, returning a match
    return pat.fullmatch(string, pos, endpos, concurrent, partial, timeout)
def search(pattern, string, flags=0, pos=None, endpos=None, partial=False,
    """Search through string looking for a match to the pattern, returning a
    match object, or None if no match was found."""
    return pat.search(string, pos, endpos, concurrent, partial, timeout)
def sub(pattern, repl, string, count=0, flags=0, pos=None, endpos=None,
    """Return the string obtained by replacing the leftmost (or rightmost with a
    reverse pattern) non-overlapping occurrences of the pattern in string by the
    replacement repl. repl can be either a string or a callable; if a string,
    backslash escapes in it are processed; if a callable, it's passed the match
    object and must return a replacement string to be used."""
    return pat.sub(repl, string, count, pos, endpos, concurrent, timeout)
def subf(pattern, format, string, count=0, flags=0, pos=None, endpos=None,
    replacement format. format can be either a string or a callable; if a string,
    it's treated as a format string; if a callable, it's passed the match object
    and must return a replacement string to be used."""
    return pat.subf(format, string, count, pos, endpos, concurrent, timeout)
def subn(pattern, repl, string, count=0, flags=0, pos=None, endpos=None,
    """Return a 2-tuple containing (new_string, number). new_string is the string
    obtained by replacing the leftmost (or rightmost with a reverse pattern)
    non-overlapping occurrences of the pattern in the source string by the
    replacement repl. number is the number of substitutions that were made. repl
    can be either a string or a callable; if a string, backslash escapes in it
    are processed; if a callable, it's passed the match object and must return a
    replacement string to be used."""
    return pat.subn(repl, string, count, pos, endpos, concurrent, timeout)
def subfn(pattern, format, string, count=0, flags=0, pos=None, endpos=None,
    replacement format. number is the number of substitutions that were made. format
    can be either a string or a callable; if a string, it's treated as a format
    string; if a callable, it's passed the match object and must return a
    return pat.subfn(format, string, count, pos, endpos, concurrent, timeout)
def split(pattern, string, maxsplit=0, flags=0, concurrent=None, timeout=None,
  ignore_unused=False, **kwargs):
    """Split the source string by the occurrences of the pattern, returning a
    list containing the resulting substrings.  If capturing parentheses are used
    in pattern, then the text of all groups in the pattern are also returned as
    part of the resulting list.  If maxsplit is nonzero, at most maxsplit splits
    occur, and the remainder of the string is returned as the final element of
    the list."""
    return pat.split(string, maxsplit, concurrent, timeout)
def splititer(pattern, string, maxsplit=0, flags=0, concurrent=None,
  timeout=None, ignore_unused=False, **kwargs):
    "Return an iterator yielding the parts of a split string."
    return pat.splititer(string, maxsplit, concurrent, timeout)
def findall(pattern, string, flags=0, pos=None, endpos=None, overlapped=False,
    """Return a list of all matches in the string. The matches may be overlapped
    if overlapped is True. If one or more groups are present in the pattern,
    return a list of groups; this will be a list of tuples if the pattern has
    more than one group. Empty matches are included in the result."""
    return pat.findall(string, pos, endpos, overlapped, concurrent, timeout)
def finditer(pattern, string, flags=0, pos=None, endpos=None, overlapped=False,
  partial=False, concurrent=None, timeout=None, ignore_unused=False, **kwargs):
    """Return an iterator over all matches in the string. The matches may be
    overlapped if overlapped is True. For each match, the iterator returns a
    match object. Empty matches are included in the result."""
    return pat.finditer(string, pos, endpos, overlapped, concurrent, partial,
def compile(pattern, flags=0, ignore_unused=False, cache_pattern=None, **kwargs):
    "Compile a regular expression pattern, returning a pattern object."
    if cache_pattern is None:
        cache_pattern = _cache_all
    return _compile(pattern, flags, ignore_unused, kwargs, cache_pattern)
def purge():
    "Clear the regular expression cache"
    _cache.clear()
    _locale_sensitive.clear()
# Whether to cache all patterns.
_cache_all = True
def cache_all(value=True):
    """Sets whether to cache all patterns, even those are compiled explicitly.
    Passing None has no effect, but returns the current setting."""
    global _cache_all
        return _cache_all
    _cache_all = value
def template(pattern, flags=0):
    "Compile a template pattern, returning a pattern object."
    return _compile(pattern, flags | TEMPLATE, False, {}, False)
def escape(pattern, special_only=True, literal_spaces=False):
    """Escape a string for use as a literal in a pattern. If special_only is
    True, escape only special characters, else escape all non-alphanumeric
    characters. If literal_spaces is True, don't escape spaces."""
    # Convert it to Unicode.
    if isinstance(pattern, bytes):
        p = pattern.decode("latin-1")
        p = pattern
    s = []
    if special_only:
        for c in p:
            if c == " " and literal_spaces:
                s.append(c)
            elif c in _METACHARS or c.isspace():
                s.append("\\")
            elif c in _ALNUM:
    r = "".join(s)
    # Convert it back to bytes if necessary.
        r = r.encode("latin-1")
# Internals.
from regex import _regex_core
from regex import _regex
from threading import RLock as _RLock
from locale import getpreferredencoding as _getpreferredencoding
from regex._regex_core import *
from regex._regex_core import (_ALL_VERSIONS, _ALL_ENCODINGS, _FirstSetError,
  _UnscopedFlagSet, _check_group_features, _compile_firstset,
  _compile_replacement, _flatten_code, _fold_case, _get_required_string,
  _parse_pattern, _shrink_cache)
from regex._regex_core import (ALNUM as _ALNUM, Info as _Info, OP as _OP, Source
  as _Source, Fuzzy as _Fuzzy)
# Version 0 is the old behaviour, compatible with the original 're' module.
# Version 1 is the new behaviour, which differs slightly.
DEFAULT_VERSION = RegexFlag.VERSION0
_METACHARS = frozenset("()[]{}?*+|^$\\.-#&~")
_regex_core.DEFAULT_VERSION = DEFAULT_VERSION
# Caches for the patterns and replacements.
_cache = {}
_cache_lock = _RLock()
_named_args = {}
_replacement_cache = {}
_locale_sensitive = {}
# Maximum size of the cache.
_MAXCACHE = 500
_MAXREPCACHE = 500
def _compile(pattern, flags, ignore_unused, kwargs, cache_it):
    "Compiles a regular expression to a PatternObject."
    global DEFAULT_VERSION
        from regex import DEFAULT_VERSION
    # We won't bother to cache the pattern if we're debugging.
    if (flags & DEBUG) != 0:
        cache_it = False
    # What locale is this pattern using?
    locale_key = (type(pattern), pattern)
    if _locale_sensitive.get(locale_key, True) or (flags & LOCALE) != 0:
        # This pattern is, or might be, locale-sensitive.
        pattern_locale = _getpreferredencoding()
        # This pattern is definitely not locale-sensitive.
        pattern_locale = None
    def complain_unused_args():
        if ignore_unused:
        # Complain about any unused keyword arguments, possibly resulting from a typo.
        unused_kwargs = set(kwargs) - {k for k, v in args_needed}
        if unused_kwargs:
            any_one = next(iter(unused_kwargs))
            raise ValueError('unused keyword argument {!a}'.format(any_one))
    if cache_it:
            # Do we know what keyword arguments are needed?
            args_key = pattern, type(pattern), flags
            args_needed = _named_args[args_key]
            # Are we being provided with its required keyword arguments?
            args_supplied = set()
            if args_needed:
                for k, v in args_needed:
                        args_supplied.add((k, frozenset(kwargs[k])))
                        raise error("missing named list: {!r}".format(k))
            complain_unused_args()
            args_supplied = frozenset(args_supplied)
            # Have we already seen this regular expression and named list?
            pattern_key = (pattern, type(pattern), flags, args_supplied,
              DEFAULT_VERSION, pattern_locale)
            return _cache[pattern_key]
            # It's a new pattern, or new named list for a known pattern.
    # Guess the encoding from the class of the pattern string.
        guess_encoding = UNICODE
    elif isinstance(pattern, bytes):
        guess_encoding = ASCII
    elif isinstance(pattern, Pattern):
            raise ValueError("cannot process flags argument with a compiled pattern")
        raise TypeError("first argument must be a string or compiled pattern")
    # Set the default version in the core code in case it has been changed.
    global_flags = flags
        caught_exception = None
            source = _Source(pattern)
            info = _Info(global_flags, source.char_type, kwargs)
            info.guess_encoding = guess_encoding
            source.ignore_space = bool(info.flags & VERBOSE)
            parsed = _parse_pattern(source, info)
        except _UnscopedFlagSet:
            # Remember the global flags for the next attempt.
            global_flags = info.global_flags
        except error as e:
            caught_exception = e
        if caught_exception:
            raise error(caught_exception.msg, caught_exception.pattern,
              caught_exception.pos)
    if not source.at_end():
        raise error("unbalanced parenthesis", pattern, source.pos)
    # Check the global flags for conflicts.
    version = (info.flags & _ALL_VERSIONS) or DEFAULT_VERSION
    if version not in (0, VERSION0, VERSION1):
        raise ValueError("VERSION0 and VERSION1 flags are mutually incompatible")
    if (info.flags & _ALL_ENCODINGS) not in (0, ASCII, LOCALE, UNICODE):
        raise ValueError("ASCII, LOCALE and UNICODE flags are mutually incompatible")
    if isinstance(pattern, bytes) and (info.flags & UNICODE):
        raise ValueError("cannot use UNICODE flag with a bytes pattern")
    if not (info.flags & _ALL_ENCODINGS):
            info.flags |= UNICODE
            info.flags |= ASCII
    reverse = bool(info.flags & REVERSE)
    fuzzy = isinstance(parsed, _Fuzzy)
    # Remember whether this pattern as an inline locale flag.
    _locale_sensitive[locale_key] = info.inline_locale
    # Fix the group references.
        parsed.fix_groups(pattern, reverse, False)
    # Should we print the parsed pattern?
    if flags & DEBUG:
        parsed.dump(indent=0, reverse=reverse)
    # Optimise the parsed pattern.
    parsed = parsed.optimise(info, reverse)
    parsed = parsed.pack_characters(info)
    # Get the required string.
    req_offset, req_chars, req_flags = _get_required_string(parsed, info.flags)
    # Build the named lists.
    named_lists = {}
    named_list_indexes = [None] * len(info.named_lists_used)
    args_needed = set()
    for key, index in info.named_lists_used.items():
        name, case_flags = key
        values = frozenset(kwargs[name])
        if case_flags:
            items = frozenset(_fold_case(info, v) for v in values)
            items = values
        named_lists[name] = values
        named_list_indexes[index] = items
        args_needed.add((name, values))
    # Check the features of the groups.
    _check_group_features(info, parsed)
    # Compile the parsed pattern. The result is a list of tuples.
    code = parsed.compile(reverse)
    # Is there a group call to the pattern as a whole?
    key = (0, reverse, fuzzy)
    ref = info.call_refs.get(key)
    if ref is not None:
        code = [(_OP.CALL_REF, ref)] + code + [(_OP.END, )]
    # Add the final 'success' opcode.
    code += [(_OP.SUCCESS, )]
    # Compile the additional copies of the groups that we need.
    for group, rev, fuz in info.additional_groups:
        code += group.compile(rev, fuz)
    # Flatten the code into a list of ints.
    code = _flatten_code(code)
    if not parsed.has_simple_start():
        # Get the first set, if possible.
            fs_code = _compile_firstset(info, parsed.get_firstset(reverse))
            fs_code = _flatten_code(fs_code)
            code = fs_code + code
        except _FirstSetError:
    # The named capture groups.
    index_group = dict((v, n) for n, v in info.group_index.items())
    # Create the PatternObject.
    # Local flags like IGNORECASE affect the code generation, but aren't needed
    # by the PatternObject itself. Conversely, global flags like LOCALE _don't_
    # affect the code generation but _are_ needed by the PatternObject.
    compiled_pattern = _regex.compile(pattern, info.flags | version, code,
      info.group_index, index_group, named_lists, named_list_indexes,
      req_offset, req_chars, req_flags, info.group_count)
    # Do we need to reduce the size of the cache?
    if len(_cache) >= _MAXCACHE:
        with _cache_lock:
            _shrink_cache(_cache, _named_args, _locale_sensitive, _MAXCACHE)
        if (info.flags & LOCALE) == 0:
        args_needed = frozenset(args_needed)
        # Store this regular expression and named list.
        pattern_key = (pattern, type(pattern), flags, args_needed,
        _cache[pattern_key] = compiled_pattern
        # Store what keyword arguments are needed.
        _named_args[args_key] = args_needed
    return compiled_pattern
def _compile_replacement_helper(pattern, template):
    "Compiles a replacement template."
    # This function is called by the _regex module.
    # Have we seen this before?
    key = pattern.pattern, pattern.flags, template
    compiled = _replacement_cache.get(key)
    if compiled is not None:
        return compiled
    if len(_replacement_cache) >= _MAXREPCACHE:
        _replacement_cache.clear()
    is_unicode = isinstance(template, str)
    source = _Source(template)
    if is_unicode:
        def make_string(char_codes):
            return "".join(chr(c) for c in char_codes)
            return bytes(char_codes)
    compiled = []
    literal = []
        ch = source.get()
        if not ch:
        if ch == "\\":
            # '_compile_replacement' will return either an int group reference
            # or a string literal. It returns items (plural) in order to handle
            # a 2-character literal (an invalid escape sequence).
            is_group, items = _compile_replacement(source, pattern, is_unicode)
            if is_group:
                # It's a group, so first flush the literal.
                if literal:
                    compiled.append(make_string(literal))
                compiled.extend(items)
                literal.extend(items)
            literal.append(ord(ch))
    # Flush the literal.
    _replacement_cache[key] = compiled
# We define Pattern here after all the support objects have been defined.
_pat = _compile('', 0, False, {}, False)
Pattern = type(_pat)
Match = type(_pat.match(''))
del _pat
# Make Pattern public for typing annotations.
__all__.append("Pattern")
__all__.append("Match")
# We'll define an alias for the 'compile' function so that the repr of a
# pattern object is eval-able.
Regex = compile
# Register myself for pickling.
import copyreg as _copy_reg
def _pickle(pattern):
    return _regex.compile, pattern._pickled_data
_copy_reg.pickle(Pattern, _pickle)
from .cli import *  # NOQA
from .cli import __all__  # NOQA
from .std import TqdmDeprecationWarning
     "Please use `tqdm.cli.*` instead of `tqdm._main.*`",
import click
import pygments.lexers
import pygments.util
import rich.console
import rich.markup
import rich.progress
import rich.syntax
import rich.table
from ._client import Client
from ._exceptions import RequestError
from ._models import Response
    import httpcore  # pragma: no cover
def print_help() -> None:
    console = rich.console.Console()
    console.print("[bold]HTTPX :butterfly:", justify="center")
    console.print("A next generation HTTP client.", justify="center")
        "Usage: [bold]httpx[/bold] [cyan]<URL> [OPTIONS][/cyan] ", justify="left"
    table = rich.table.Table.grid(padding=1, pad_edge=True)
    table.add_column("Parameter", no_wrap=True, justify="left", style="bold")
    table.add_column("Description")
        "-m, --method [cyan]METHOD",
        "Request method, such as GET, POST, PUT, PATCH, DELETE, OPTIONS, HEAD.\n"
        "[Default: GET, or POST if a request body is included]",
        "-p, --params [cyan]<NAME VALUE> ...",
        "Query parameters to include in the request URL.",
        "-c, --content [cyan]TEXT", "Byte content to include in the request body."
        "-d, --data [cyan]<NAME VALUE> ...", "Form data to include in the request body."
        "-f, --files [cyan]<NAME FILENAME> ...",
        "Form files to include in the request body.",
    table.add_row("-j, --json [cyan]TEXT", "JSON data to include in the request body.")
        "-h, --headers [cyan]<NAME VALUE> ...",
        "Include additional HTTP headers in the request.",
        "--cookies [cyan]<NAME VALUE> ...", "Cookies to include in the request."
        "--auth [cyan]<USER PASS>",
        "Username and password to include in the request. Specify '-' for the password"
        " to use a password prompt. Note that using --verbose/-v will expose"
        " the Authorization header, including the password encoding"
        " in a trivially reversible format.",
        "--proxy [cyan]URL",
        "Send the request via a proxy. Should be the URL giving the proxy address.",
        "--timeout [cyan]FLOAT",
        "Timeout value to use for network operations, such as establishing the"
        " connection, reading some data, etc... [Default: 5.0]",
    table.add_row("--follow-redirects", "Automatically follow redirects.")
    table.add_row("--no-verify", "Disable SSL verification.")
        "--http2", "Send the request using HTTP/2, if the remote server supports it."
        "--download [cyan]FILE",
        "Save the response content as a file, rather than displaying it.",
    table.add_row("-v, --verbose", "Verbose output. Show request as well as response.")
    table.add_row("--help", "Show this message and exit.")
    console.print(table)
def get_lexer_for_response(response: Response) -> str:
    content_type = response.headers.get("Content-Type")
    if content_type is not None:
        mime_type, _, _ = content_type.partition(";")
            return typing.cast(
                str, pygments.lexers.get_lexer_for_mimetype(mime_type.strip()).name
        except pygments.util.ClassNotFound:  # pragma: no cover
    return ""  # pragma: no cover
def format_request_headers(request: httpcore.Request, http2: bool = False) -> str:
    version = "HTTP/2" if http2 else "HTTP/1.1"
    headers = [
        (name.lower() if http2 else name, value) for name, value in request.headers
    method = request.method.decode("ascii")
    target = request.url.target.decode("ascii")
    lines = [f"{method} {target} {version}"] + [
        f"{name.decode('ascii')}: {value.decode('ascii')}" for name, value in headers
def format_response_headers(
    http_version: bytes,
    reason_phrase: bytes | None,
    version = http_version.decode("ascii")
    reason = (
        codes.get_reason_phrase(status)
        if reason_phrase is None
        else reason_phrase.decode("ascii")
    lines = [f"{version} {status} {reason}"] + [
def print_request_headers(request: httpcore.Request, http2: bool = False) -> None:
    http_text = format_request_headers(request, http2=http2)
    syntax = rich.syntax.Syntax(http_text, "http", theme="ansi_dark", word_wrap=True)
    console.print(syntax)
    syntax = rich.syntax.Syntax("", "http", theme="ansi_dark", word_wrap=True)
def print_response_headers(
    http_text = format_response_headers(http_version, status, reason_phrase, headers)
def print_response(response: Response) -> None:
    lexer_name = get_lexer_for_response(response)
    if lexer_name:
        if lexer_name.lower() == "json":
                text = json.dumps(data, indent=4)
            except ValueError:  # pragma: no cover
                text = response.text
        syntax = rich.syntax.Syntax(text, lexer_name, theme="ansi_dark", word_wrap=True)
        console.print(f"<{len(response.content)} bytes of binary data>")
_PCTRTT = typing.Tuple[typing.Tuple[str, str], ...]
_PCTRTTT = typing.Tuple[_PCTRTT, ...]
_PeerCertRetDictType = typing.Dict[str, typing.Union[str, _PCTRTTT, _PCTRTT]]
def format_certificate(cert: _PeerCertRetDictType) -> str:  # pragma: no cover
    for key, value in cert.items():
            lines.append(f"*   {key}:")
                if key in ("subject", "issuer"):
                    for sub_item in item:
                        lines.append(f"*     {sub_item[0]}: {sub_item[1]!r}")
                elif isinstance(item, tuple) and len(item) == 2:
                    lines.append(f"*     {item[0]}: {item[1]!r}")
                    lines.append(f"*     {item!r}")
            lines.append(f"*   {key}: {value!r}")
    name: str, info: typing.Mapping[str, typing.Any], verbose: bool = False
    if name == "connection.connect_tcp.started" and verbose:
        host = info["host"]
        console.print(f"* Connecting to {host!r}")
    elif name == "connection.connect_tcp.complete" and verbose:
        stream = info["return_value"]
        server_addr = stream.get_extra_info("server_addr")
        console.print(f"* Connected to {server_addr[0]!r} on port {server_addr[1]}")
    elif name == "connection.start_tls.complete" and verbose:  # pragma: no cover
        ssl_object = stream.get_extra_info("ssl_object")
        version = ssl_object.version()
        cipher = ssl_object.cipher()
        server_cert = ssl_object.getpeercert()
        alpn = ssl_object.selected_alpn_protocol()
        console.print(f"* SSL established using {version!r} / {cipher[0]!r}")
        console.print(f"* Selected ALPN protocol: {alpn!r}")
        if server_cert:
            console.print("* Server certificate:")
            console.print(format_certificate(server_cert))
    elif name == "http11.send_request_headers.started" and verbose:
        request = info["request"]
        print_request_headers(request, http2=False)
    elif name == "http2.send_request_headers.started" and verbose:  # pragma: no cover
        print_request_headers(request, http2=True)
    elif name == "http11.receive_response_headers.complete":
        http_version, status, reason_phrase, headers = info["return_value"]
        print_response_headers(http_version, status, reason_phrase, headers)
    elif name == "http2.receive_response_headers.complete":  # pragma: no cover
        status, headers = info["return_value"]
        http_version = b"HTTP/2"
        reason_phrase = None
def download_response(response: Response, download: typing.BinaryIO) -> None:
    content_length = response.headers.get("Content-Length")
    with rich.progress.Progress(
        "[progress.description]{task.description}",
        "[progress.percentage]{task.percentage:>3.0f}%",
        rich.progress.BarColumn(bar_width=None),
        rich.progress.DownloadColumn(),
        rich.progress.TransferSpeedColumn(),
    ) as progress:
        description = f"Downloading [bold]{rich.markup.escape(download.name)}"
        download_task = progress.add_task(
            total=int(content_length or 0),
            start=content_length is not None,
        for chunk in response.iter_bytes():
            download.write(chunk)
            progress.update(download_task, completed=response.num_bytes_downloaded)
def validate_json(
    ctx: click.Context,
    param: click.Option | click.Parameter,
    value: typing.Any,
) -> typing.Any:
        return json.loads(value)
    except json.JSONDecodeError:  # pragma: no cover
        raise click.BadParameter("Not valid JSON")
def validate_auth(
    if value == (None, None):
    username, password = value
    if password == "-":  # pragma: no cover
        password = click.prompt("Password", hide_input=True)
    return (username, password)
def handle_help(
    if not value or ctx.resilient_parsing:
    print_help()
    ctx.exit()
@click.command(add_help_option=False)
@click.argument("url", type=str)
@click.option(
    "--method",
    "method",
        "Request method, such as GET, POST, PUT, PATCH, DELETE, OPTIONS, HEAD. "
        "[Default: GET, or POST if a request body is included]"
    "--params",
    type=(str, str),
    multiple=True,
    help="Query parameters to include in the request URL.",
    "--content",
    "-c",
    help="Byte content to include in the request body.",
    "--data",
    "-d",
    "data",
    help="Form data to include in the request body.",
    "--files",
    type=(str, click.File(mode="rb")),
    help="Form files to include in the request body.",
    "--json",
    callback=validate_json,
    help="JSON data to include in the request body.",
    "--headers",
    "-h",
    help="Include additional HTTP headers in the request.",
    "--cookies",
    help="Cookies to include in the request.",
    "--auth",
    default=(None, None),
    callback=validate_auth,
        "Username and password to include in the request. "
        "Specify '-' for the password to use a password prompt. "
        "Note that using --verbose/-v will expose the Authorization header, "
        "including the password encoding in a trivially reversible format."
    "--proxy",
    help="Send the request via a proxy. Should be the URL giving the proxy address.",
    "--timeout",
    "timeout",
    default=5.0,
        "Timeout value to use for network operations, such as establishing the "
        "connection, reading some data, etc... [Default: 5.0]"
    "--follow-redirects",
    "follow_redirects",
    is_flag=True,
    help="Automatically follow redirects.",
    "--no-verify",
    help="Disable SSL verification.",
    "--http2",
    "http2",
    type=bool,
    help="Send the request using HTTP/2, if the remote server supports it.",
    "--download",
    type=click.File("wb"),
    help="Save the response content as a file, rather than displaying it.",
    help="Verbose. Show request as well as response.",
    "--help",
    is_eager=True,
    expose_value=False,
    callback=handle_help,
    help="Show this message and exit.",
def main(
    params: list[tuple[str, str]],
    data: list[tuple[str, str]],
    files: list[tuple[str, click.File]],
    json: str,
    headers: list[tuple[str, str]],
    cookies: list[tuple[str, str]],
    auth: tuple[str, str] | None,
    proxy: str,
    timeout: float,
    verify: bool,
    http2: bool,
    download: typing.BinaryIO | None,
    verbose: bool,
    An HTTP command line client.
    Sends a request and displays the response.
    if not method:
        method = "POST" if content or data or files or json else "GET"
        with Client(proxy=proxy, timeout=timeout, http2=http2, verify=verify) as client:
            with client.stream(
                params=list(params),
                data=dict(data),
                files=files,  # type: ignore
                cookies=dict(cookies),
                extensions={"trace": functools.partial(trace, verbose=verbose)},
                if download is not None:
                    download_response(response, download)
                        print_response(response)
        console.print(f"[red]{type(exc).__name__}[/red]: {exc}")
    sys.exit(0 if response.is_success else 1)
