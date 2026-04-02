import json as json_lib
from tweepy.models import ModelFactory
class Parser:
    def parse(self, payload, *args, **kwargs):
        Parse the response payload and return the result.
        Returns a tuple that contains the result data and the cursors
        (or None if not present).
class RawParser(Parser):
        return payload
class JSONParser(Parser):
    payload_format = 'json'
    def parse(self, payload, *, return_cursors=False, **kwargs):
        if not payload:
            json = json_lib.loads(payload)
            raise TweepyException(f'Failed to parse JSON payload: {e}')
        if return_cursors and isinstance(json, dict):
            if 'next' in json:
                return json, json['next']
            elif 'next_cursor' in json:
                if 'previous_cursor' in json:
                    cursors = json['previous_cursor'], json['next_cursor']
                    return json, cursors
                    return json, json['next_cursor']
class ModelParser(JSONParser):
    def __init__(self, model_factory=None):
        JSONParser.__init__(self)
        self.model_factory = model_factory or ModelFactory
    def parse(self, payload, *, api=None, payload_list=False,
              payload_type=None, return_cursors=False):
            if payload_type is None:
            model = getattr(self.model_factory, payload_type)
            raise TweepyException(
                f'No model for this payload type: {payload_type}'
        json = JSONParser.parse(self, payload, return_cursors=return_cursors)
        if isinstance(json, tuple):
            json, cursors = json
            cursors = None
            if payload_list:
                result = model.parse_list(api, json)
                result = model.parse(api, json)
                f"Unable to parse response payload: {json}"
            ) from None
        if cursors:
            return result, cursors
    pygments.lexers.parsers
    ~~~~~~~~~~~~~~~~~~~~~~~
    Lexers for parser generators.
from pygments.lexer import RegexLexer, DelegatingLexer, \
    include, bygroups, using
from pygments.token import Punctuation, Other, Text, Comment, Operator, \
    Keyword, Name, String, Number, Whitespace
from pygments.lexers.jvm import JavaLexer
from pygments.lexers.c_cpp import CLexer, CppLexer
from pygments.lexers.objective import ObjectiveCLexer
from pygments.lexers.d import DLexer
from pygments.lexers.dotnet import CSharpLexer
from pygments.lexers.ruby import RubyLexer
from pygments.lexers.python import PythonLexer
from pygments.lexers.perl import PerlLexer
__all__ = ['RagelLexer', 'RagelEmbeddedLexer', 'RagelCLexer', 'RagelDLexer',
           'RagelCppLexer', 'RagelObjectiveCLexer', 'RagelRubyLexer',
           'RagelJavaLexer', 'AntlrLexer', 'AntlrPythonLexer',
           'AntlrPerlLexer', 'AntlrRubyLexer', 'AntlrCppLexer',
           'AntlrCSharpLexer', 'AntlrObjectiveCLexer',
           'AntlrJavaLexer', 'AntlrActionScriptLexer',
           'TreetopLexer', 'EbnfLexer']
class RagelLexer(RegexLexer):
    """A pure `Ragel <www.colm.net/open-source/ragel>`_ lexer.  Use this
    for fragments of Ragel.  For ``.rl`` files, use
    :class:`RagelEmbeddedLexer` instead (or one of the
    language-specific subclasses).
    name = 'Ragel'
    url = 'http://www.colm.net/open-source/ragel/'
    aliases = ['ragel']
    filenames = []
    version_added = '1.1'
        'whitespace': [
            (r'\s+', Whitespace)
        'comments': [
            (r'\#.*$', Comment),
            (r'(access|action|alphtype)\b', Keyword),
            (r'(getkey|write|machine|include)\b', Keyword),
            (r'(any|ascii|extend|alpha|digit|alnum|lower|upper)\b', Keyword),
            (r'(xdigit|cntrl|graph|print|punct|space|zlen|empty)\b', Keyword)
        'numbers': [
            (r'0x[0-9A-Fa-f]+', Number.Hex),
            (r'[+-]?[0-9]+', Number.Integer),
        'literals': [
            (r'"(\\\\|\\[^\\]|[^"\\])*"', String.Double),
            (r"'(\\\\|\\[^\\]|[^'\\])*'", String.Single),
            (r'\[(\\\\|\\[^\\]|[^\\\]])*\]', String),          # square bracket literals
            (r'/(?!\*)(\\\\|\\[^\\]|[^/\\])*/', String.Regex),  # regular expressions
        'identifiers': [
            (r'[a-zA-Z_]\w*', Name.Variable),
            (r',', Operator),                           # Join
            (r'\||&|--?', Operator),                    # Union, Intersection and Subtraction
            (r'\.|<:|:>>?', Operator),                  # Concatention
            (r':', Operator),                           # Label
            (r'->', Operator),                          # Epsilon Transition
            (r'(>|\$|%|<|@|<>)(/|eof\b)', Operator),    # EOF Actions
            (r'(>|\$|%|<|@|<>)(!|err\b)', Operator),    # Global Error Actions
            (r'(>|\$|%|<|@|<>)(\^|lerr\b)', Operator),  # Local Error Actions
            (r'(>|\$|%|<|@|<>)(~|to\b)', Operator),     # To-State Actions
            (r'(>|\$|%|<|@|<>)(\*|from\b)', Operator),  # From-State Actions
            (r'>|@|\$|%', Operator),                    # Transition Actions and Priorities
            (r'\*|\?|\+|\{[0-9]*,[0-9]*\}', Operator),  # Repetition
            (r'!|\^', Operator),                        # Negation
            (r'\(|\)', Operator),                       # Grouping
            include('literals'),
            include('whitespace'),
            include('comments'),
            include('numbers'),
            include('identifiers'),
            (r'\{', Punctuation, 'host'),
            (r'=', Operator),
            (r';', Punctuation),
        'host': [
            (r'(' + r'|'.join((  # keep host code in largest possible chunks
                r'[^{}\'"/#]+',  # exclude unsafe characters
                r'[^\\]\\[{}]',  # allow escaped { or }
                # strings and comments may safely contain unsafe characters
                r'"(\\\\|\\[^\\]|[^"\\])*"',
                r"'(\\\\|\\[^\\]|[^'\\])*'",
                r'//.*$\n?',            # single line comment
                r'/\*(.|\n)*?\*/',      # multi-line javadoc-style comment
                r'\#.*$\n?',            # ruby comment
                # regular expression: There's no reason for it to start
                # with a * and this stops confusion with comments.
                r'/(?!\*)(\\\\|\\[^\\]|[^/\\])*/',
                # / is safe now that we've handled regex and javadoc comments
                r'/',
            )) + r')+', Other),
            (r'\{', Punctuation, '#push'),
            (r'\}', Punctuation, '#pop'),
class RagelEmbeddedLexer(RegexLexer):
    A lexer for Ragel embedded in a host language file.
    This will only highlight Ragel statements. If you want host language
    highlighting then call the language-specific Ragel lexer.
    name = 'Embedded Ragel'
    aliases = ['ragel-em']
    filenames = ['*.rl']
            (r'(' + r'|'.join((   # keep host code in largest possible chunks
                r'[^%\'"/#]+',    # exclude unsafe characters
                r'%(?=[^%]|$)',   # a single % sign is okay, just not 2 of them
                r'//.*$\n?',  # single line comment
                r'\#.*$\n?',  # ruby/ragel comment
                r'/(?!\*)(\\\\|\\[^\\]|[^/\\])*/',  # regular expression
            # Single Line FSM.
            # Please don't put a quoted newline in a single line FSM.
            # That's just mean. It will break this.
            (r'(%%)(?![{%])(.*)($|;)(\n?)', bygroups(Punctuation,
                                                     using(RagelLexer),
                                                     Punctuation, Text)),
            # Multi Line FSM.
            (r'(%%%%|%%)\{', Punctuation, 'multi-line-fsm'),
        'multi-line-fsm': [
            (r'(' + r'|'.join((  # keep ragel code in largest possible chunks.
                r'(' + r'|'.join((
                    r'[^}\'"\[/#]',   # exclude unsafe characters
                    r'\}(?=[^%]|$)',   # } is okay as long as it's not followed by %
                    r'\}%(?=[^%]|$)',  # ...well, one %'s okay, just not two...
                    r'[^\\]\\[{}]',   # ...and } is okay if it's escaped
                    # allow / if it's preceded with one of these symbols
                    # (ragel EOF actions)
                    r'(>|\$|%|<|@|<>)/',
                    # specifically allow regex followed immediately by *
                    # so it doesn't get mistaken for a comment
                    r'/(?!\*)(\\\\|\\[^\\]|[^/\\])*/\*',
                    # allow / as long as it's not followed by another / or by a *
                    r'/(?=[^/*]|$)',
                    # We want to match as many of these as we can in one block.
                    # Not sure if we need the + sign here,
                    # does it help performance?
                )) + r')+',
                r"\[(\\\\|\\[^\\]|[^\]\\])*\]",  # square bracket literal
                r'/\*(.|\n)*?\*/',          # multi-line javadoc-style comment
                r'//.*$\n?',                # single line comment
                r'\#.*$\n?',                # ruby/ragel comment
            )) + r')+', using(RagelLexer)),
            (r'\}%%', Punctuation, '#pop'),
        return '@LANG: indep' in text
class RagelRubyLexer(DelegatingLexer):
    A lexer for Ragel in a Ruby host file.
    name = 'Ragel in Ruby Host'
    aliases = ['ragel-ruby', 'ragel-rb']
        super().__init__(RubyLexer, RagelEmbeddedLexer, **options)
        return '@LANG: ruby' in text
class RagelCLexer(DelegatingLexer):
    A lexer for Ragel in a C host file.
    name = 'Ragel in C Host'
    aliases = ['ragel-c']
        super().__init__(CLexer, RagelEmbeddedLexer, **options)
        return '@LANG: c' in text
class RagelDLexer(DelegatingLexer):
    A lexer for Ragel in a D host file.
    name = 'Ragel in D Host'
    aliases = ['ragel-d']
        super().__init__(DLexer, RagelEmbeddedLexer, **options)
        return '@LANG: d' in text
class RagelCppLexer(DelegatingLexer):
    A lexer for Ragel in a C++ host file.
    name = 'Ragel in CPP Host'
    aliases = ['ragel-cpp']
        super().__init__(CppLexer, RagelEmbeddedLexer, **options)
        return '@LANG: c++' in text
class RagelObjectiveCLexer(DelegatingLexer):
    A lexer for Ragel in an Objective C host file.
    name = 'Ragel in Objective C Host'
    aliases = ['ragel-objc']
        super().__init__(ObjectiveCLexer, RagelEmbeddedLexer, **options)
        return '@LANG: objc' in text
class RagelJavaLexer(DelegatingLexer):
    A lexer for Ragel in a Java host file.
    name = 'Ragel in Java Host'
    aliases = ['ragel-java']
        super().__init__(JavaLexer, RagelEmbeddedLexer, **options)
        return '@LANG: java' in text
class AntlrLexer(RegexLexer):
    Generic ANTLR Lexer.
    Should not be called directly, instead
    use DelegatingLexer for your target language.
    name = 'ANTLR'
    aliases = ['antlr']
    url = 'https://www.antlr.org'
    _id = r'[A-Za-z]\w*'
    _TOKEN_REF = r'[A-Z]\w*'
    _RULE_REF = r'[a-z]\w*'
    _STRING_LITERAL = r'\'(?:\\\\|\\\'|[^\']*)\''
    _INT = r'[0-9]+'
            (r'//.*$', Comment),
            (r'/\*(.|\n)*?\*/', Comment),
            (r'(lexer|parser|tree)?(\s*)(grammar\b)(\s*)(' + _id + ')(;)',
             bygroups(Keyword, Whitespace, Keyword, Whitespace, Name.Class,
                      Punctuation)),
            # optionsSpec
            (r'options\b', Keyword, 'options'),
            # tokensSpec
            (r'tokens\b', Keyword, 'tokens'),
            # attrScope
            (r'(scope)(\s*)(' + _id + r')(\s*)(\{)',
             bygroups(Keyword, Whitespace, Name.Variable, Whitespace,
                      Punctuation), 'action'),
            # exception
            (r'(catch|finally)\b', Keyword, 'exception'),
            (r'(@' + _id + r')(\s*)(::)?(\s*)(' + _id + r')(\s*)(\{)',
             bygroups(Name.Label, Whitespace, Punctuation, Whitespace,
                      Name.Label, Whitespace, Punctuation), 'action'),
            # rule
            (r'((?:protected|private|public|fragment)\b)?(\s*)(' + _id + ')(!)?',
             bygroups(Keyword, Whitespace, Name.Label, Punctuation),
             ('rule-alts', 'rule-prelims')),
        'exception': [
            (r'\s', Whitespace),
            (r'\[', Punctuation, 'nested-arg-action'),
            (r'\{', Punctuation, 'action'),
        'rule-prelims': [
            (r'returns\b', Keyword),
            # throwsSpec
            (r'(throws)(\s+)(' + _id + ')',
            (r'(,)(\s*)(' + _id + ')',
             bygroups(Punctuation, Whitespace, Name.Label)),  # Additional throws
            # ruleScopeSpec - scope followed by target language code or name of action
            # TODO finish implementing other possibilities for scope
            # L173 ANTLRv3.g from ANTLR book
            (r'(scope)(\s+)(\{)', bygroups(Keyword, Whitespace, Punctuation),
             'action'),
            (r'(scope)(\s+)(' + _id + r')(\s*)(;)',
             bygroups(Keyword, Whitespace, Name.Label, Whitespace, Punctuation)),
            # ruleAction
            (r'(@' + _id + r')(\s*)(\{)',
             bygroups(Name.Label, Whitespace, Punctuation), 'action'),
            # finished prelims, go to rule alts!
            (r':', Punctuation, '#pop')
        'rule-alts': [
            # These might need to go in a separate 'block' state triggered by (
            (r':', Punctuation),
            # literals
            (r'<<([^>]|>[^>])>>', String),
            # identifiers
            # Tokens start with capital letter.
            (r'\$?[A-Z_]\w*', Name.Constant),
            # Rules start with small letter.
            (r'\$?[a-z_]\w*', Name.Variable),
            # operators
            (r'(\+|\||->|=>|=|\(|\)|\.\.|\.|\?|\*|\^|!|\#|~)', Operator),
            (r',', Punctuation),
            (r';', Punctuation, '#pop')
        'tokens': [
            (r'\{', Punctuation),
            (r'(' + _TOKEN_REF + r')(\s*)(=)?(\s*)(' + _STRING_LITERAL
             + r')?(\s*)(;)',
                      String, Whitespace, Punctuation)),
        'options': [
            (r'(' + _id + r')(\s*)(=)(\s*)(' +
             '|'.join((_id, _STRING_LITERAL, _INT, r'\*')) + r')(\s*)(;)',
             bygroups(Name.Variable, Whitespace, Punctuation, Whitespace,
                      Text, Whitespace, Punctuation)),
        'action': [
            (r'(' + r'|'.join((    # keep host code in largest possible chunks
                r'[^${}\'"/\\]+',  # exclude unsafe characters
                # backslashes are okay, as long as we are not backslashing a %
                r'\\(?!%)',
                # Now that we've handled regex and javadoc comments
                # it's safe to let / through.
            (r'(\\)(%)', bygroups(Punctuation, Other)),
            (r'(\$[a-zA-Z]+)(\.?)(text|value)?',
             bygroups(Name.Variable, Punctuation, Name.Property)),
        'nested-arg-action': [
            (r'(' + r'|'.join((    # keep host code in largest possible chunks.
                r'[^$\[\]\'"/]+',  # exclude unsafe characters
            (r'\[', Punctuation, '#push'),
            (r'\]', Punctuation, '#pop'),
            (r'(\\\\|\\\]|\\\[|[^\[\]])+', Other),
        return re.search(r'^\s*grammar\s+[a-zA-Z0-9]+\s*;', text, re.M)
# http://www.antlr.org/wiki/display/ANTLR3/Code+Generation+Targets
class AntlrCppLexer(DelegatingLexer):
    ANTLR with C++ Target
    name = 'ANTLR With CPP Target'
    aliases = ['antlr-cpp']
    filenames = ['*.G', '*.g']
        super().__init__(CppLexer, AntlrLexer, **options)
        return AntlrLexer.analyse_text(text) and \
            re.search(r'^\s*language\s*=\s*C\s*;', text, re.M)
class AntlrObjectiveCLexer(DelegatingLexer):
    ANTLR with Objective-C Target
    name = 'ANTLR With ObjectiveC Target'
    aliases = ['antlr-objc']
        super().__init__(ObjectiveCLexer, AntlrLexer, **options)
            re.search(r'^\s*language\s*=\s*ObjC\s*;', text)
class AntlrCSharpLexer(DelegatingLexer):
    ANTLR with C# Target
    name = 'ANTLR With C# Target'
    aliases = ['antlr-csharp', 'antlr-c#']
        super().__init__(CSharpLexer, AntlrLexer, **options)
            re.search(r'^\s*language\s*=\s*CSharp2\s*;', text, re.M)
class AntlrPythonLexer(DelegatingLexer):
    ANTLR with Python Target
    name = 'ANTLR With Python Target'
    aliases = ['antlr-python']
        super().__init__(PythonLexer, AntlrLexer, **options)
            re.search(r'^\s*language\s*=\s*Python\s*;', text, re.M)
class AntlrJavaLexer(DelegatingLexer):
    ANTLR with Java Target
    name = 'ANTLR With Java Target'
    aliases = ['antlr-java']
        super().__init__(JavaLexer, AntlrLexer, **options)
        # Antlr language is Java by default
        return AntlrLexer.analyse_text(text) and 0.9
class AntlrRubyLexer(DelegatingLexer):
    ANTLR with Ruby Target
    name = 'ANTLR With Ruby Target'
    aliases = ['antlr-ruby', 'antlr-rb']
        super().__init__(RubyLexer, AntlrLexer, **options)
            re.search(r'^\s*language\s*=\s*Ruby\s*;', text, re.M)
class AntlrPerlLexer(DelegatingLexer):
    ANTLR with Perl Target
    name = 'ANTLR With Perl Target'
    aliases = ['antlr-perl']
        super().__init__(PerlLexer, AntlrLexer, **options)
            re.search(r'^\s*language\s*=\s*Perl5\s*;', text, re.M)
class AntlrActionScriptLexer(DelegatingLexer):
    ANTLR with ActionScript Target
    name = 'ANTLR With ActionScript Target'
    aliases = ['antlr-actionscript', 'antlr-as']
        from pygments.lexers.actionscript import ActionScriptLexer
        super().__init__(ActionScriptLexer, AntlrLexer, **options)
            re.search(r'^\s*language\s*=\s*ActionScript\s*;', text, re.M)
class TreetopBaseLexer(RegexLexer):
    A base lexer for `Treetop <http://treetop.rubyforge.org/>`_ grammars.
    Not for direct use; use :class:`TreetopLexer` instead.
    .. versionadded:: 1.6
            include('space'),
            (r'require[ \t]+[^\n\r]+[\n\r]', Other),
            (r'module\b', Keyword.Namespace, 'module'),
            (r'grammar\b', Keyword, 'grammar'),
        'module': [
            include('end'),
            (r'module\b', Keyword, '#push'),
            (r'[A-Z]\w*(?:::[A-Z]\w*)*', Name.Namespace),
        'grammar': [
            (r'rule\b', Keyword, 'rule'),
            (r'include\b', Keyword, 'include'),
            (r'[A-Z]\w*', Name),
        'include': [
            (r'[A-Z]\w*(?:::[A-Z]\w*)*', Name.Class, '#pop'),
        'rule': [
            (r'([A-Za-z_]\w*)(:)', bygroups(Name.Label, Punctuation)),
            (r'[A-Za-z_]\w*', Name),
            (r'[()]', Punctuation),
            (r'[?+*/&!~]', Operator),
            (r'\[(?:\\.|\[:\^?[a-z]+:\]|[^\\\]])+\]', String.Regex),
            (r'([0-9]*)(\.\.)([0-9]*)',
             bygroups(Number.Integer, Operator, Number.Integer)),
            (r'(<)([^>]+)(>)', bygroups(Punctuation, Name.Class, Punctuation)),
            (r'\{', Punctuation, 'inline_module'),
            (r'\.', String.Regex),
        'inline_module': [
            (r'\{', Other, 'ruby'),
            (r'[^{}]+', Other),
        'ruby': [
            (r'\{', Other, '#push'),
            (r'\}', Other, '#pop'),
        'space': [
            (r'[ \t\n\r]+', Whitespace),
            (r'#[^\n]*', Comment.Single),
        'end': [
            (r'end\b', Keyword, '#pop'),
class TreetopLexer(DelegatingLexer):
    A lexer for Treetop grammars.
    name = 'Treetop'
    aliases = ['treetop']
    filenames = ['*.treetop', '*.tt']
    url = 'https://cjheath.github.io/treetop'
        super().__init__(RubyLexer, TreetopBaseLexer, **options)
class EbnfLexer(RegexLexer):
    Lexer for `ISO/IEC 14977 EBNF
    <https://en.wikipedia.org/wiki/Extended_Backus%E2%80%93Naur_Form>`_
    grammars.
    name = 'EBNF'
    aliases = ['ebnf']
    filenames = ['*.ebnf']
    mimetypes = ['text/x-ebnf']
    url = 'https://en.wikipedia.org/wiki/Extended_Backus%E2%80%93Naur_Form'
            include('comment_start'),
            include('identifier'),
            (r'=', Operator, 'production'),
        'production': [
            (r'"[^"]*"', String.Double),
            (r"'[^']*'", String.Single),
            (r'(\?[^?]*\?)', Name.Entity),
            (r'[\[\]{}(),|]', Punctuation),
            (r'-', Operator),
            (r';', Punctuation, '#pop'),
            (r'\.', Punctuation, '#pop'),
            (r'\s+', Text),
        'comment_start': [
            (r'\(\*', Comment.Multiline, 'comment'),
            (r'[^*)]', Comment.Multiline),
            (r'\*\)', Comment.Multiline, '#pop'),
            (r'[*)]', Comment.Multiline),
        'identifier': [
            (r'([a-zA-Z][\w \-]*)', Keyword),
from .tsl import USING_TSL_PACK
# Updated mapping of file extensions to parsers
PARSERS = {
    ".py": "python",
    ".js": "javascript",
    ".mjs": "javascript",  # mjs file extension stands for "module JavaScript."
    ".go": "go",
    ".bash": "bash",
    ".c": "c",
    ".cc": "cpp",
    ".cs": "c_sharp",
    ".cl": "commonlisp",
    ".cpp": "cpp",
    ".css": "css",
    ".dockerfile": "dockerfile",
    ".dot": "dot",
    ".el": "elisp",
    ".ex": "elixir",
    ".elm": "elm",
    ".et": "embedded_template",
    ".erl": "erlang",
    ".gomod": "gomod",
    ".hack": "hack",
    ".hs": "haskell",
    ".hcl": "hcl",
    ".html": "html",
    ".java": "java",
    ".jsdoc": "jsdoc",
    ".json": "json",
    ".jl": "julia",
    ".kt": "kotlin",
    ".lua": "lua",
    ".mk": "make",
    ".md": "markdown",  # https://github.com/ikatyang/tree-sitter-markdown/issues/59
    ".m": "objc",
    ".ml": "ocaml",
    ".mli": "ocaml_interface",
    ".pl": "perl",
    ".php": "php",
    ".ql": "ql",
    ".r": "r",
    ".R": "r",
    ".regex": "regex",
    ".rst": "rst",
    ".rb": "ruby",
    ".rs": "rust",
    ".scala": "scala",
    ".sql": "sql",
    ".sqlite": "sqlite",
    ".tf": "hcl",
    ".toml": "toml",
    ".tsq": "tsq",
    ".tsx": "typescript",
    ".ts": "typescript",
    ".yaml": "yaml",
if USING_TSL_PACK:
    # Replace the PARSERS dictionary with a comprehensive mapping based on the language pack
        # A
        ".as": "actionscript",
        ".adb": "ada",
        ".ads": "ada",
        ".agda": "agda",
        ".ino": "arduino",
        ".asm": "asm",
        ".s": "asm",
        ".astro": "astro",
        # B
        ".sh": "bash",
        ".zsh": "bash",
        ".bean": "beancount",
        ".bib": "bibtex",
        ".bicep": "bicep",
        ".bb": "bitbake",
        ".bbappend": "bitbake",
        ".bbclass": "bitbake",
        # C
        ".h": "c",
        ".cairo": "cairo",
        ".capnp": "capnp",
        ".chatito": "chatito",
        ".clar": "clarity",
        ".clj": "clojure",
        ".cljs": "clojure",
        ".cljc": "clojure",
        ".edn": "clojure",
        ".cmake": "cmake",
        "CMakeLists.txt": "cmake",
        ".lisp": "commonlisp",
        ".cpon": "cpon",
        ".cxx": "cpp",
        ".hpp": "cpp",
        ".hxx": "cpp",
        ".h++": "cpp",
        ".cs": "csharp",
        ".csv": "csv",
        ".cu": "cuda",
        ".cuh": "cuda",
        ".d": "d",
        # D
        ".dart": "dart",
        "Dockerfile": "dockerfile",
        ".dtd": "dtd",
        # E
        ".exs": "elixir",
        ".hrl": "erlang",
        # F
        ".fnl": "fennel",
        ".fir": "firrtl",
        ".fish": "fish",
        ".f": "fortran",
        ".f90": "fortran",
        ".f95": "fortran",
        ".f03": "fortran",
        ".f08": "fortran",
        ".fc": "func",
        # G
        ".gd": "gdscript",
        ".gitattributes": "gitattributes",
        ".gitcommit": "gitcommit",
        ".gitignore": "gitignore",
        ".gleam": "gleam",
        ".glsl": "glsl",
        ".vert": "glsl",
        ".frag": "glsl",
        ".gn": "gn",
        ".gni": "gn",
        "go.mod": "gomod",
        "go.sum": "gosum",
        ".groovy": "groovy",
        ".launch": "gstlaunch",
        # H
        ".ha": "hare",
        ".hx": "haxe",
        ".tfvars": "hcl",
        ".heex": "heex",
        ".hlsl": "hlsl",
        ".htm": "html",
        ".hypr": "hyprlang",
        # I
        ".ispc": "ispc",
        # J
        ".janet": "janet",
        ".jsx": "javascript",
        ".mjs": "javascript",
        ".jsonnet": "jsonnet",
        ".libsonnet": "jsonnet",
        # K
        "Kconfig": "kconfig",
        ".kdl": "kdl",
        ".kts": "kotlin",
        # L
        ".tex": "latex",
        ".sty": "latex",
        ".cls": "latex",
        ".ld": "linkerscript",
        ".ll": "llvm",
        ".td": "tablegen",
        ".luadoc": "luadoc",
        ".luap": "luap",
        ".luau": "luau",
        # M
        ".magik": "magik",
        "Makefile": "make",
        ".md": "markdown",
        ".markdown": "markdown",
        ".m": "matlab",  # Note: .m is used by both MATLAB and Objective-C, prioritizing MATLAB here
        ".mat": "matlab",
        ".mermaid": "mermaid",
        "meson.build": "meson",
        # N
        ".ninja": "ninja",
        ".nix": "nix",
        ".nqc": "nqc",
        # O
        # .m extension is handled under MATLAB section (dual use extension)
        ".mm": "objc",
        ".odin": "odin",
        ".org": "org",
        # P
        ".pas": "pascal",
        ".pp": "pascal",
        ".pem": "pem",
        ".pm": "perl",
        ".pgn": "pgn",
        ".po": "po",
        ".pot": "po",
        ".pony": "pony",
        ".ps1": "powershell",
        ".psm1": "powershell",
        ".printf": "printf",
        ".prisma": "prisma",
        ".properties": "properties",
        ".proto": "proto",
        ".psv": "psv",
        ".purs": "purescript",
        "MANIFEST.in": "pymanifest",
        # Q
        "qmldir": "qmldir",
        ".qml": "qmljs",
        # R
        ".rkt": "racket",
        ".re2c": "re2c",
        ".inputrc": "readline",
        "requirements.txt": "requirements",
        ".ron": "ron",
        # S
        ".sc": "scala",
        ".scm": "scheme",  # .scm is primarily used for Scheme files
        ".ss": "scheme",
        ".scss": "scss",
        ".smali": "smali",
        ".smithy": "smithy",
        ".sol": "solidity",
        ".rq": "sparql",
        ".nut": "squirrel",
        ".bzl": "starlark",
        "BUILD": "starlark",
        "WORKSPACE": "starlark",
        ".svelte": "svelte",
        ".swift": "swift",
        # T
        ".tcl": "tcl",
        ".thrift": "thrift",
        ".tsv": "tsv",
        ".twig": "twig",
        ".typ": "typst",
        # U
        ".rules": "udev",
        ".ungram": "ungrammar",
        ".tal": "uxntal",
        # V
        # Note: .v extension is used by both V language and Verilog
        # Prioritizing Verilog as it's more commonly used
        ".sv": "verilog",
        ".v": "verilog",
        # For V language, users may need to specify parser manually
        ".vhd": "vhdl",
        ".vhdl": "vhdl",
        ".vim": "vim",
        ".vimrc": "vim",
        ".vue": "vue",
        # W
        ".wgsl": "wgsl",
        # X
        ".XCompose": "xcompose",
        ".xml": "xml",
        ".svg": "xml",
        ".xsl": "xml",
        # Y
        ".yuck": "yuck",
        # Z
        ".zig": "zig",
def filename_to_lang(filename):
    # First check if the full filename (like "Dockerfile" or "go.mod") is in PARSERS
    basename = os.path.basename(filename)
    if basename in PARSERS:
        return PARSERS[basename]
    # If not found by full filename, check by extension
    file_extension = os.path.splitext(filename)[1]
    return PARSERS.get(file_extension)
