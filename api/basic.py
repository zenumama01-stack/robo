"""Basic query against the public shopping search API"""
SHOPPING_API_VERSION = "v1"
DEVELOPER_KEY = "<YOUR DEVELOPER KEY>"
    """Get and print a feed of all public products available in the
    United States.
    Note: The source and country arguments are required to pass to the list
    client = build("shopping", SHOPPING_API_VERSION, developerKey=DEVELOPER_KEY)
    resource = client.products()
    request = resource.list(source="public", country="US")
    response = request.execute()
    pprint.pprint(response)
    pygments.lexers.basic
    Lexers for BASIC like languages (other than VB.net).
from pygments.lexer import RegexLexer, bygroups, default, words, include
from pygments.token import Comment, Error, Keyword, Name, Number, \
    Punctuation, Operator, String, Text, Whitespace
from pygments.lexers import _vbscript_builtins
__all__ = ['BlitzBasicLexer', 'BlitzMaxLexer', 'MonkeyLexer', 'CbmBasicV2Lexer',
           'QBasicLexer', 'VBScriptLexer', 'BBCBasicLexer']
class BlitzMaxLexer(RegexLexer):
    For BlitzMax source code.
    name = 'BlitzMax'
    url = 'http://blitzbasic.com'
    aliases = ['blitzmax', 'bmax']
    filenames = ['*.bmx']
    mimetypes = ['text/x-bmx']
    version_added = '1.4'
    bmax_vopwords = r'\b(Shl|Shr|Sar|Mod)\b'
    bmax_sktypes = r'@{1,2}|[!#$%]'
    bmax_lktypes = r'\b(Int|Byte|Short|Float|Double|Long)\b'
    bmax_name = r'[a-z_]\w*'
    bmax_var = (rf'({bmax_name})(?:(?:([ \t]*)({bmax_sktypes})|([ \t]*:[ \t]*\b(?:Shl|Shr|Sar|Mod)\b)'
                rf'|([ \t]*)(:)([ \t]*)(?:{bmax_lktypes}|({bmax_name})))(?:([ \t]*)(Ptr))?)')
    bmax_func = bmax_var + r'?((?:[ \t]|\.\.\n)*)([(])'
    flags = re.MULTILINE | re.IGNORECASE
    tokens = {
        'root': [
            # Text
            (r'\s+', Whitespace),
            (r'(\.\.)(\n)', bygroups(Text, Whitespace)),  # Line continuation
            # Comments
            (r"'.*?\n", Comment.Single),
            (r'([ \t]*)\bRem\n(\n|.)*?\s*\bEnd([ \t]*)Rem', Comment.Multiline),
            # Data types
            ('"', String.Double, 'string'),
            (r'[0-9]+\.[0-9]*(?!\.)', Number.Float),
            (r'\.[0-9]*(?!\.)', Number.Float),
            (r'[0-9]+', Number.Integer),
            (r'\$[0-9a-f]+', Number.Hex),
            (r'\%[10]+', Number.Bin),
            # Other
            (rf'(?:(?:(:)?([ \t]*)(:?{bmax_vopwords}|([+\-*/&|~]))|Or|And|Not|[=<>^]))', Operator),
            (r'[(),.:\[\]]', Punctuation),
            (r'(?:#[\w \t]*)', Name.Label),
            (r'(?:\?[\w \t]*)', Comment.Preproc),
            # Identifiers
            (rf'\b(New)\b([ \t]?)([(]?)({bmax_name})',
             bygroups(Keyword.Reserved, Whitespace, Punctuation, Name.Class)),
            (rf'\b(Import|Framework|Module)([ \t]+)({bmax_name}\.{bmax_name})',
             bygroups(Keyword.Reserved, Whitespace, Keyword.Namespace)),
            (bmax_func, bygroups(Name.Function, Whitespace, Keyword.Type,
                                 Operator, Whitespace, Punctuation, Whitespace,
                                 Keyword.Type, Name.Class, Whitespace,
                                 Keyword.Type, Whitespace, Punctuation)),
            (bmax_var, bygroups(Name.Variable, Whitespace, Keyword.Type, Operator,
                                Whitespace, Punctuation, Whitespace, Keyword.Type,
                                Name.Class, Whitespace, Keyword.Type)),
            (rf'\b(Type|Extends)([ \t]+)({bmax_name})',
             bygroups(Keyword.Reserved, Whitespace, Name.Class)),
            # Keywords
            (r'\b(Ptr)\b', Keyword.Type),
            (r'\b(Pi|True|False|Null|Self|Super)\b', Keyword.Constant),
            (r'\b(Local|Global|Const|Field)\b', Keyword.Declaration),
            (words((
                'TNullMethodException', 'TNullFunctionException',
                'TNullObjectException', 'TArrayBoundsException',
                'TRuntimeException'), prefix=r'\b', suffix=r'\b'), Name.Exception),
                'Strict', 'SuperStrict', 'Module', 'ModuleInfo',
                'End', 'Return', 'Continue', 'Exit', 'Public', 'Private',
                'Var', 'VarPtr', 'Chr', 'Len', 'Asc', 'SizeOf', 'Sgn', 'Abs', 'Min', 'Max',
                'New', 'Release', 'Delete', 'Incbin', 'IncbinPtr', 'IncbinLen',
                'Framework', 'Include', 'Import', 'Extern', 'EndExtern',
                'Function', 'EndFunction', 'Type', 'EndType', 'Extends', 'Method', 'EndMethod',
                'Abstract', 'Final', 'If', 'Then', 'Else', 'ElseIf', 'EndIf',
                'For', 'To', 'Next', 'Step', 'EachIn', 'While', 'Wend', 'EndWhile',
                'Repeat', 'Until', 'Forever', 'Select', 'Case', 'Default', 'EndSelect',
                'Try', 'Catch', 'EndTry', 'Throw', 'Assert', 'Goto', 'DefData', 'ReadData',
                'RestoreData'), prefix=r'\b', suffix=r'\b'),
             Keyword.Reserved),
            # Final resolve (for variable names and such)
            (rf'({bmax_name})', Name.Variable),
        'string': [
            (r'""', String.Double),
            (r'"C?', String.Double, '#pop'),
            (r'[^"]+', String.Double),
class BlitzBasicLexer(RegexLexer):
    For BlitzBasic source code.
    name = 'BlitzBasic'
    aliases = ['blitzbasic', 'b3d', 'bplus']
    filenames = ['*.bb', '*.decls']
    mimetypes = ['text/x-bb']
    version_added = '2.0'
    bb_sktypes = r'@{1,2}|[#$%]'
    bb_name = r'[a-z]\w*'
    bb_var = (rf'({bb_name})(?:([ \t]*)({bb_sktypes})|([ \t]*)([.])([ \t]*)(?:({bb_name})))?')
            (r";.*?\n", Comment.Single),
            (r'\.[0-9]+(?!\.)', Number.Float),
            (words(('Shl', 'Shr', 'Sar', 'Mod', 'Or', 'And', 'Not',
                    'Abs', 'Sgn', 'Handle', 'Int', 'Float', 'Str',
                    'First', 'Last', 'Before', 'After'),
                   prefix=r'\b', suffix=r'\b'),
             Operator),
            (r'([+\-*/~=<>^])', Operator),
            (r'[(),:\[\]\\]', Punctuation),
            (rf'\.([ \t]*)({bb_name})', Name.Label),
            (rf'\b(New)\b([ \t]+)({bb_name})',
            (rf'\b(Gosub|Goto)\b([ \t]+)({bb_name})',
             bygroups(Keyword.Reserved, Whitespace, Name.Label)),
            (rf'\b(Object)\b([ \t]*)([.])([ \t]*)({bb_name})\b',
             bygroups(Operator, Whitespace, Punctuation, Whitespace, Name.Class)),
            (rf'\b{bb_var}\b([ \t]*)(\()',
             bygroups(Name.Function, Whitespace, Keyword.Type, Whitespace, Punctuation,
                      Whitespace, Name.Class, Whitespace, Punctuation)),
            (rf'\b(Function)\b([ \t]+){bb_var}',
             bygroups(Keyword.Reserved, Whitespace, Name.Function, Whitespace, Keyword.Type,
                      Whitespace, Punctuation, Whitespace, Name.Class)),
            (rf'\b(Type)([ \t]+)({bb_name})',
            (r'\b(Pi|True|False|Null)\b', Keyword.Constant),
            (r'\b(Local|Global|Const|Field|Dim)\b', Keyword.Declaration),
                'End', 'Return', 'Exit', 'Chr', 'Len', 'Asc', 'New', 'Delete', 'Insert',
                'Include', 'Function', 'Type', 'If', 'Then', 'Else', 'ElseIf', 'EndIf',
                'For', 'To', 'Next', 'Step', 'Each', 'While', 'Wend',
                'Repeat', 'Until', 'Forever', 'Select', 'Case', 'Default',
                'Goto', 'Gosub', 'Data', 'Read', 'Restore'), prefix=r'\b', suffix=r'\b'),
            # (r'(%s)' % (bb_name), Name.Variable),
            (bb_var, bygroups(Name.Variable, Whitespace, Keyword.Type,
            (r'[^"\n]+', String.Double),
class MonkeyLexer(RegexLexer):
    For Monkey source code.
    name = 'Monkey'
    aliases = ['monkey']
    filenames = ['*.monkey']
    mimetypes = ['text/x-monkey']
    url = 'https://blitzresearch.itch.io/monkeyx'
    version_added = '1.6'
    name_variable = r'[a-z_]\w*'
    name_function = r'[A-Z]\w*'
    name_constant = r'[A-Z_][A-Z0-9_]*'
    name_class = r'[A-Z]\w*'
    name_module = r'[a-z0-9_]*'
    keyword_type = r'(?:Int|Float|String|Bool|Object|Array|Void)'
    # ? == Bool // % == Int // # == Float // $ == String
    keyword_type_special = r'[?%#$]'
    flags = re.MULTILINE
            (r"'.*", Comment),
            (r'(?i)^#rem\b', Comment.Multiline, 'comment'),
            # preprocessor directives
            (r'(?i)^(?:#If|#ElseIf|#Else|#EndIf|#End|#Print|#Error)\b', Comment.Preproc),
            # preprocessor variable (any line starting with '#' that is not a directive)
            (r'^#', Comment.Preproc, 'variables'),
            (r'\$[0-9a-fA-Z]+', Number.Hex),
            # Native data types
            (rf'\b{keyword_type}\b', Keyword.Type),
            # Exception handling
            (r'(?i)\b(?:Try|Catch|Throw)\b', Keyword.Reserved),
            (r'Throwable', Name.Exception),
            # Builtins
            (r'(?i)\b(?:Null|True|False)\b', Name.Builtin),
            (r'(?i)\b(?:Self|Super)\b', Name.Builtin.Pseudo),
            (r'\b(?:HOST|LANG|TARGET|CONFIG)\b', Name.Constant),
            (r'(?i)^(Import)(\s+)(.*)(\n)',
             bygroups(Keyword.Namespace, Whitespace, Name.Namespace, Whitespace)),
            (r'(?i)^Strict\b.*\n', Keyword.Reserved),
            (r'(?i)(Const|Local|Global|Field)(\s+)',
             bygroups(Keyword.Declaration, Whitespace), 'variables'),
            (r'(?i)(New|Class|Interface|Extends|Implements)(\s+)',
             bygroups(Keyword.Reserved, Whitespace), 'classname'),
            (r'(?i)(Function|Method)(\s+)',
             bygroups(Keyword.Reserved, Whitespace), 'funcname'),
            (r'(?i)(?:End|Return|Public|Private|Extern|Property|'
             r'Final|Abstract)\b', Keyword.Reserved),
            # Flow Control stuff
            (r'(?i)(?:If|Then|Else|ElseIf|EndIf|'
             r'Select|Case|Default|'
             r'While|Wend|'
             r'Repeat|Until|Forever|'
             r'For|To|Until|Step|EachIn|Next|'
             r'Exit|Continue)(?=\s)', Keyword.Reserved),
            # not used yet
            (r'(?i)\b(?:Module|Inline)\b', Keyword.Reserved),
            # Array
            (r'[\[\]]', Punctuation),
            (r'<=|>=|<>|\*=|/=|\+=|-=|&=|~=|\|=|[-&*/^+=<>|~]', Operator),
            (r'(?i)(?:Not|Mod|Shl|Shr|And|Or)', Operator.Word),
            (r'[(){}!#,.:]', Punctuation),
            # catch the rest
            (rf'{name_constant}\b', Name.Constant),
            (rf'{name_function}\b', Name.Function),
            (rf'{name_variable}\b', Name.Variable),
        'funcname': [
            (rf'(?i){name_function}\b', Name.Function),
            (r':', Punctuation, 'classname'),
            (r'\(', Punctuation, 'variables'),
            (r'\)', Punctuation, '#pop')
        'classname': [
            (rf'{name_module}\.', Name.Namespace),
            (rf'{keyword_type}\b', Keyword.Type),
            (rf'{name_class}\b', Name.Class),
            # array (of given size)
            (r'(\[)(\s*)(\d*)(\s*)(\])',
             bygroups(Punctuation, Whitespace, Number.Integer, Whitespace, Punctuation)),
            # generics
            (r'\s+(?!<)', Whitespace, '#pop'),
            (r'<', Punctuation, '#push'),
            (r'>', Punctuation, '#pop'),
            (r'\n', Whitespace, '#pop'),
            default('#pop')
        'variables': [
            (rf'{keyword_type_special}', Keyword.Type),
            (r',', Punctuation, '#push'),
            (r'[^"~]+', String.Double),
            (r'~q|~n|~r|~t|~z|~~', String.Escape),
            (r'"', String.Double, '#pop'),
        'comment': [
            (r'(?i)^#rem.*?', Comment.Multiline, "#push"),
            (r'(?i)^#end.*?', Comment.Multiline, "#pop"),
            (r'\n', Comment.Multiline),
            (r'.+', Comment.Multiline),
class CbmBasicV2Lexer(RegexLexer):
    For CBM BASIC V2 sources.
    name = 'CBM BASIC V2'
    aliases = ['cbmbas']
    filenames = ['*.bas']
    url = 'https://en.wikipedia.org/wiki/Commodore_BASIC'
    flags = re.IGNORECASE
            (r'rem.*\n', Comment.Single),
            (r'new|run|end|for|to|next|step|go(to|sub)?|on|return|stop|cont'
             r'|if|then|input#?|read|wait|load|save|verify|poke|sys|print#?'
             r'|list|clr|cmd|open|close|get#?', Keyword.Reserved),
            (r'data|restore|dim|let|def|fn', Keyword.Declaration),
            (r'tab|spc|sgn|int|abs|usr|fre|pos|sqr|rnd|log|exp|cos|sin|tan|atn'
             r'|peek|len|val|asc|(str|chr|left|right|mid)\$', Name.Builtin),
            (r'[-+*/^<>=]', Operator),
            (r'not|and|or', Operator.Word),
            (r'"[^"\n]*.', String),
            (r'\d+|[-+]?\d*\.\d*(e[-+]?\d+)?', Number.Float),
            (r'[(),:;]', Punctuation),
            (r'\w+[$%]?', Name),
    def analyse_text(text):
        # if it starts with a line number, it shouldn't be a "modern" Basic
        # like VB.net
        if re.match(r'^\d+', text):
            return 0.2
class QBasicLexer(RegexLexer):
    For QBasic source code.
    name = 'QBasic'
    aliases = ['qbasic', 'basic']
    filenames = ['*.BAS', '*.bas']
    mimetypes = ['text/basic']
    url = 'https://en.wikipedia.org/wiki/QBasic'
    declarations = ('DATA', 'LET')
    functions = (
        'ABS', 'ASC', 'ATN', 'CDBL', 'CHR$', 'CINT', 'CLNG',
        'COMMAND$', 'COS', 'CSNG', 'CSRLIN', 'CVD', 'CVDMBF', 'CVI',
        'CVL', 'CVS', 'CVSMBF', 'DATE$', 'ENVIRON$', 'EOF', 'ERDEV',
        'ERDEV$', 'ERL', 'ERR', 'EXP', 'FILEATTR', 'FIX', 'FRE',
        'FREEFILE', 'HEX$', 'INKEY$', 'INP', 'INPUT$', 'INSTR', 'INT',
        'IOCTL$', 'LBOUND', 'LCASE$', 'LEFT$', 'LEN', 'LOC', 'LOF',
        'LOG', 'LPOS', 'LTRIM$', 'MID$', 'MKD$', 'MKDMBF$', 'MKI$',
        'MKL$', 'MKS$', 'MKSMBF$', 'OCT$', 'PEEK', 'PEN', 'PLAY',
        'PMAP', 'POINT', 'POS', 'RIGHT$', 'RND', 'RTRIM$', 'SADD',
        'SCREEN', 'SEEK', 'SETMEM', 'SGN', 'SIN', 'SPACE$', 'SPC',
        'SQR', 'STICK', 'STR$', 'STRIG', 'STRING$', 'TAB', 'TAN',
        'TIME$', 'TIMER', 'UBOUND', 'UCASE$', 'VAL', 'VARPTR',
        'VARPTR$', 'VARSEG'
    metacommands = ('$DYNAMIC', '$INCLUDE', '$STATIC')
    operators = ('AND', 'EQV', 'IMP', 'NOT', 'OR', 'XOR')
    statements = (
        'BEEP', 'BLOAD', 'BSAVE', 'CALL', 'CALL ABSOLUTE',
        'CALL INTERRUPT', 'CALLS', 'CHAIN', 'CHDIR', 'CIRCLE', 'CLEAR',
        'CLOSE', 'CLS', 'COLOR', 'COM', 'COMMON', 'CONST', 'DATA',
        'DATE$', 'DECLARE', 'DEF FN', 'DEF SEG', 'DEFDBL', 'DEFINT',
        'DEFLNG', 'DEFSNG', 'DEFSTR', 'DEF', 'DIM', 'DO', 'LOOP',
        'DRAW', 'END', 'ENVIRON', 'ERASE', 'ERROR', 'EXIT', 'FIELD',
        'FILES', 'FOR', 'NEXT', 'FUNCTION', 'GET', 'GOSUB', 'GOTO',
        'IF', 'THEN', 'INPUT', 'INPUT #', 'IOCTL', 'KEY', 'KEY',
        'KILL', 'LET', 'LINE', 'LINE INPUT', 'LINE INPUT #', 'LOCATE',
        'LOCK', 'UNLOCK', 'LPRINT', 'LSET', 'MID$', 'MKDIR', 'NAME',
        'ON COM', 'ON ERROR', 'ON KEY', 'ON PEN', 'ON PLAY',
        'ON STRIG', 'ON TIMER', 'ON UEVENT', 'ON', 'OPEN', 'OPEN COM',
        'OPTION BASE', 'OUT', 'PAINT', 'PALETTE', 'PCOPY', 'PEN',
        'PLAY', 'POKE', 'PRESET', 'PRINT', 'PRINT #', 'PRINT USING',
        'PSET', 'PUT', 'PUT', 'RANDOMIZE', 'READ', 'REDIM', 'REM',
        'RESET', 'RESTORE', 'RESUME', 'RETURN', 'RMDIR', 'RSET', 'RUN',
        'SCREEN', 'SEEK', 'SELECT CASE', 'SHARED', 'SHELL', 'SLEEP',
        'SOUND', 'STATIC', 'STOP', 'STRIG', 'SUB', 'SWAP', 'SYSTEM',
        'TIME$', 'TIMER', 'TROFF', 'TRON', 'TYPE', 'UEVENT', 'UNLOCK',
        'VIEW', 'WAIT', 'WHILE', 'WEND', 'WIDTH', 'WINDOW', 'WRITE'
    keywords = (
        'ACCESS', 'ALIAS', 'ANY', 'APPEND', 'AS', 'BASE', 'BINARY',
        'BYVAL', 'CASE', 'CDECL', 'DOUBLE', 'ELSE', 'ELSEIF', 'ENDIF',
        'INTEGER', 'IS', 'LIST', 'LOCAL', 'LONG', 'LOOP', 'MOD',
        'NEXT', 'OFF', 'ON', 'OUTPUT', 'RANDOM', 'SIGNAL', 'SINGLE',
        'STEP', 'STRING', 'THEN', 'TO', 'UNTIL', 'USING', 'WEND'
            (r'\n+', Text),
            (r'\s+', Text.Whitespace),
            (r'^(\s*)(\d*)(\s*)(REM .*)$',
             bygroups(Text.Whitespace, Name.Label, Text.Whitespace,
                      Comment.Single)),
            (r'^(\s*)(\d+)(\s*)',
             bygroups(Text.Whitespace, Name.Label, Text.Whitespace)),
            (r'(?=[\s]*)(\w+)(?=[\s]*=)', Name.Variable.Global),
            (r'(?=[^"]*)\'.*$', Comment.Single),
            (r'"[^\n"]*"', String.Double),
            (r'(END)(\s+)(FUNCTION|IF|SELECT|SUB)',
             bygroups(Keyword.Reserved, Text.Whitespace, Keyword.Reserved)),
            (r'(DECLARE)(\s+)([A-Z]+)(\s+)(\S+)',
             bygroups(Keyword.Declaration, Text.Whitespace, Name.Variable,
                      Text.Whitespace, Name)),
            (r'(DIM)(\s+)(SHARED)(\s+)([^\s(]+)',
                      Text.Whitespace, Name.Variable.Global)),
            (r'(DIM)(\s+)([^\s(]+)',
             bygroups(Keyword.Declaration, Text.Whitespace, Name.Variable.Global)),
            (r'^(\s*)([a-zA-Z_]+)(\s*)(\=)',
             bygroups(Text.Whitespace, Name.Variable.Global, Text.Whitespace,
                      Operator)),
            (r'(GOTO|GOSUB)(\s+)(\w+\:?)',
             bygroups(Keyword.Reserved, Text.Whitespace, Name.Label)),
            (r'(SUB)(\s+)(\w+\:?)',
            include('declarations'),
            include('functions'),
            include('metacommands'),
            include('operators'),
            include('statements'),
            include('keywords'),
            (r'[a-zA-Z_]\w*[$@#&!]', Name.Variable.Global),
            (r'[a-zA-Z_]\w*\:', Name.Label),
            (r'\-?\d*\.\d+[@|#]?', Number.Float),
            (r'\-?\d+[@|#]', Number.Float),
            (r'\-?\d+#?', Number.Integer.Long),
            (r'\-?\d+#?', Number.Integer),
            (r'!=|==|:=|\.=|<<|>>|[-~+/\\*%=<>&^|?:!.]', Operator),
            (r'[\[\]{}(),;]', Punctuation),
            (r'[\w]+', Name.Variable.Global),
        # can't use regular \b because of X$()
        # XXX: use words() here
        'declarations': [
            (r'\b({})(?=\(|\b)'.format('|'.join(map(re.escape, declarations))),
             Keyword.Declaration),
        'functions': [
            (r'\b({})(?=\(|\b)'.format('|'.join(map(re.escape, functions))),
        'metacommands': [
            (r'\b({})(?=\(|\b)'.format('|'.join(map(re.escape, metacommands))),
             Keyword.Constant),
        'operators': [
            (r'\b({})(?=\(|\b)'.format('|'.join(map(re.escape, operators))), Operator.Word),
        'statements': [
            (r'\b({})\b'.format('|'.join(map(re.escape, statements))),
        'keywords': [
            (r'\b({})\b'.format('|'.join(keywords)), Keyword),
        if '$DYNAMIC' in text or '$STATIC' in text:
            return 0.9
class VBScriptLexer(RegexLexer):
    VBScript is scripting language that is modeled on Visual Basic.
    name = 'VBScript'
    aliases = ['vbscript']
    filenames = ['*.vbs', '*.VBS']
    url = 'https://learn.microsoft.com/en-us/previous-versions/t0aew7h6(v=vs.85)'
    version_added = '2.4'
            (r"'[^\n]*", Comment.Single),
            ('&h[0-9a-f]+', Number.Hex),
            # Float variant 1, for example: 1., 1.e2, 1.2e3
            (r'[0-9]+\.[0-9]*(e[+-]?[0-9]+)?', Number.Float),
            (r'\.[0-9]+(e[+-]?[0-9]+)?', Number.Float),  # Float variant 2, for example: .1, .1e2
            (r'[0-9]+e[+-]?[0-9]+', Number.Float),  # Float variant 3, for example: 123e45
            ('#.+#', String),  # date or time value
            (r'(dim)(\s+)([a-z_][a-z0-9_]*)',
             bygroups(Keyword.Declaration, Whitespace, Name.Variable), 'dim_more'),
            (r'(function|sub)(\s+)([a-z_][a-z0-9_]*)',
             bygroups(Keyword.Declaration, Whitespace, Name.Function)),
            (r'(class)(\s+)([a-z_][a-z0-9_]*)',
             bygroups(Keyword.Declaration, Whitespace, Name.Class)),
            (r'(const)(\s+)([a-z_][a-z0-9_]*)',
             bygroups(Keyword.Declaration, Whitespace, Name.Constant)),
            (r'(end)(\s+)(class|function|if|property|sub|with)',
             bygroups(Keyword, Whitespace, Keyword)),
            (r'(on)(\s+)(error)(\s+)(goto)(\s+)(0)',
             bygroups(Keyword, Whitespace, Keyword, Whitespace, Keyword, Whitespace, Number.Integer)),
            (r'(on)(\s+)(error)(\s+)(resume)(\s+)(next)',
             bygroups(Keyword, Whitespace, Keyword, Whitespace, Keyword, Whitespace, Keyword)),
            (r'(option)(\s+)(explicit)', bygroups(Keyword, Whitespace, Keyword)),
            (r'(property)(\s+)(get|let|set)(\s+)([a-z_][a-z0-9_]*)',
             bygroups(Keyword.Declaration, Whitespace, Keyword.Declaration, Whitespace, Name.Property)),
            (r'rem\s.*[^\n]*', Comment.Single),
            (words(_vbscript_builtins.KEYWORDS, suffix=r'\b'), Keyword),
            (words(_vbscript_builtins.OPERATORS), Operator),
            (words(_vbscript_builtins.OPERATOR_WORDS, suffix=r'\b'), Operator.Word),
            (words(_vbscript_builtins.BUILTIN_CONSTANTS, suffix=r'\b'), Name.Constant),
            (words(_vbscript_builtins.BUILTIN_FUNCTIONS, suffix=r'\b'), Name.Builtin),
            (words(_vbscript_builtins.BUILTIN_VARIABLES, suffix=r'\b'), Name.Builtin),
            (r'[a-z_][a-z0-9_]*', Name),
            (r'\b_\n', Operator),
            (words(r'(),.:'), Punctuation),
            (r'.+(\n)?', Error)
        'dim_more': [
            (r'(\s*)(,)(\s*)([a-z_][a-z0-9]*)',
             bygroups(Whitespace, Punctuation, Whitespace, Name.Variable)),
            default('#pop'),
            (r'\"\"', String.Double),
            (r'\n', Error, '#pop'),  # Unterminated string
class BBCBasicLexer(RegexLexer):
    BBC Basic was supplied on the BBC Micro, and later Acorn RISC OS.
    It is also used by BBC Basic For Windows.
    base_keywords = ['OTHERWISE', 'AND', 'DIV', 'EOR', 'MOD', 'OR', 'ERROR',
                     'LINE', 'OFF', 'STEP', 'SPC', 'TAB', 'ELSE', 'THEN',
                     'OPENIN', 'PTR', 'PAGE', 'TIME', 'LOMEM', 'HIMEM', 'ABS',
                     'ACS', 'ADVAL', 'ASC', 'ASN', 'ATN', 'BGET', 'COS', 'COUNT',
                     'DEG', 'ERL', 'ERR', 'EVAL', 'EXP', 'EXT', 'FALSE', 'FN',
                     'GET', 'INKEY', 'INSTR', 'INT', 'LEN', 'LN', 'LOG', 'NOT',
                     'OPENUP', 'OPENOUT', 'PI', 'POINT', 'POS', 'RAD', 'RND',
                     'SGN', 'SIN', 'SQR', 'TAN', 'TO', 'TRUE', 'USR', 'VAL',
                     'VPOS', 'CHR$', 'GET$', 'INKEY$', 'LEFT$', 'MID$',
                     'RIGHT$', 'STR$', 'STRING$', 'EOF', 'PTR', 'PAGE', 'TIME',
                     'LOMEM', 'HIMEM', 'SOUND', 'BPUT', 'CALL', 'CHAIN', 'CLEAR',
                     'CLOSE', 'CLG', 'CLS', 'DATA', 'DEF', 'DIM', 'DRAW', 'END',
                     'ENDPROC', 'ENVELOPE', 'FOR', 'GOSUB', 'GOTO', 'GCOL', 'IF',
                     'INPUT', 'LET', 'LOCAL', 'MODE', 'MOVE', 'NEXT', 'ON',
                     'VDU', 'PLOT', 'PRINT', 'PROC', 'READ', 'REM', 'REPEAT',
                     'REPORT', 'RESTORE', 'RETURN', 'RUN', 'STOP', 'COLOUR',
                     'TRACE', 'UNTIL', 'WIDTH', 'OSCLI']
    basic5_keywords = ['WHEN', 'OF', 'ENDCASE', 'ENDIF', 'ENDWHILE', 'CASE',
                       'CIRCLE', 'FILL', 'ORIGIN', 'POINT', 'RECTANGLE', 'SWAP',
                       'WHILE', 'WAIT', 'MOUSE', 'QUIT', 'SYS', 'INSTALL',
                       'LIBRARY', 'TINT', 'ELLIPSE', 'BEATS', 'TEMPO', 'VOICES',
                       'VOICE', 'STEREO', 'OVERLAY', 'APPEND', 'AUTO', 'CRUNCH',
                       'DELETE', 'EDIT', 'HELP', 'LIST', 'LOAD', 'LVAR', 'NEW',
                       'OLD', 'RENUMBER', 'SAVE', 'TEXTLOAD', 'TEXTSAVE',
                       'TWIN', 'TWINO', 'INSTALL', 'SUM', 'BEAT']
    name = 'BBC Basic'
    aliases = ['bbcbasic']
    filenames = ['*.bbc']
    url = 'https://www.bbcbasic.co.uk/bbcbasic.html'
            (r"[0-9]+", Name.Label),
            (r"(\*)([^\n]*)",
             bygroups(Keyword.Pseudo, Comment.Special)),
            default('code'),
        'code': [
            (r"(REM)([^\n]*)",
             bygroups(Keyword.Declaration, Comment.Single)),
            (r'\n', Whitespace, 'root'),
            (r':', Comment.Preproc),
            # Some special cases to make functions come out nicer
            (r'(DEF)(\s*)(FN|PROC)([A-Za-z_@][\w@]*)',
             bygroups(Keyword.Declaration, Whitespace,
                      Keyword.Declaration, Name.Function)),
            (r'(FN|PROC)([A-Za-z_@][\w@]*)',
             bygroups(Keyword, Name.Function)),
            (r'(GOTO|GOSUB|THEN|RESTORE)(\s*)(\d+)',
             bygroups(Keyword, Whitespace, Name.Label)),
            (r'(TRUE|FALSE)', Keyword.Constant),
            (r'(PAGE|LOMEM|HIMEM|TIME|WIDTH|ERL|ERR|REPORT\$|POS|VPOS|VOICES)',
             Keyword.Pseudo),
            (words(base_keywords), Keyword),
            (words(basic5_keywords), Keyword),
            ('%[01]{1,32}', Number.Bin),
            ('&[0-9a-f]{1,8}', Number.Hex),
            (r'[+-]?[0-9]+\.[0-9]*(E[+-]?[0-9]+)?', Number.Float),
            (r'[+-]?\.[0-9]+(E[+-]?[0-9]+)?', Number.Float),
            (r'[+-]?[0-9]+E[+-]?[0-9]+', Number.Float),
            (r'[+-]?\d+', Number.Integer),
            (r'([A-Za-z_@][\w@]*[%$]?)', Name.Variable),
            (r'([+\-]=|[$!|?+\-*/%^=><();]|>=|<=|<>|<<|>>|>>>|,)', Operator),
            (r'\n', Error, 'root'),  # Unterminated string
        if text.startswith('10REM >') or text.startswith('REM >'):
This module provides :class:`GitIgnoreBasicPattern` which implements Git's
`gitignore`_ patterns as documented. This differs from how Git actually behaves
when including files in excluded directories.
.. _`gitignore`: https://git-scm.com/docs/gitignore
from pathspec import util
	assert_unreachable,
	override)  # Added in 3.12.
	GitIgnorePatternError,
	_BYTES_ENCODING,
	_GitIgnoreBasePattern)
class GitIgnoreBasicPattern(_GitIgnoreBasePattern):
	The :class:`GitIgnoreBasicPattern` class represents a compiled gitignore
	pattern as documented. This is registered as "gitignore".
	def __normalize_segments(
		is_dir_pattern: bool,
		pattern_segs: list[str],
	) -> tuple[Optional[list[str]], Optional[str]]:
		Normalize the pattern segments to make processing easier.
		*is_dir_pattern* (:class:`bool`) is whether the pattern is a directory
		pattern (i.e., ends with a slash '/').
		*pattern_segs* (:class:`list` of :class:`str`) contains the pattern
		segments. This may be modified in place.
		Returns a :class:`tuple` containing either:
		- The normalized segments (:class:`list` of :class:`str`; or :data:`None`).
		- The regular expression override (:class:`str` or :data:`None`).
		if not pattern_segs[0]:
			# A pattern beginning with a slash ('/') should match relative to the root
			# directory. Remove the empty first segment to make the pattern relative
			# to root.
			del pattern_segs[0]
		elif len(pattern_segs) == 1 or (len(pattern_segs) == 2 and not pattern_segs[1]):
			# A single segment pattern with or without a trailing slash ('/') will
			# match any descendant path. This is equivalent to "**/{pattern}". Prepend
			# double-asterisk segment to make pattern relative to root.
			if pattern_segs[0] != '**':
				pattern_segs.insert(0, '**')
			# A pattern without a beginning slash ('/') but contains at least one
			# prepended directory (e.g., "dir/{pattern}") should match relative to the
			# root directory. No segment modification is needed.
		if not pattern_segs:
			# After normalization, we end up with no pattern at all. This must be
			# because the pattern is invalid.
			raise ValueError("Pattern normalized to nothing.")
		if not pattern_segs[-1]:
			# A pattern ending with a slash ('/') will match all descendant paths if
			# it is a directory but not if it is a regular file. This is equivalent to
			# "{pattern}/**". Set empty last segment to a double-asterisk to include
			# all descendants.
			pattern_segs[-1] = '**'
		# EDGE CASE: Collapse duplicate double-asterisk sequences (i.e., '**/**').
		# Iterate over the segments in reverse order and remove the duplicate double
		# asterisks as we go.
		for i in range(len(pattern_segs) - 1, 0, -1):
			prev = pattern_segs[i-1]
			seg = pattern_segs[i]
			if prev == '**' and seg == '**':
				del pattern_segs[i]
		seg_count = len(pattern_segs)
		if seg_count == 1 and pattern_segs[0] == '**':
			if is_dir_pattern:
				# The pattern "**/" will be normalized to "**", but it should match
				# everything except for files in the root. Special case this pattern.
				return (None, '/')
				# The pattern "**" will match every path. Special case this pattern.
				return (None, '.')
			seg_count == 2
			and pattern_segs[0] == '**'
			and pattern_segs[1] == '*'
			# The pattern "*" will be normalized to "**/*" and will match every
			# path. Special case this pattern for efficiency.
			seg_count == 3
			and pattern_segs[2] == '**'
			# The pattern "*/" will be normalized to "**/*/**" which will match every
			# file not in the root directory. Special case this pattern for
			# efficiency.
		# No regular expression override, return modified pattern segments.
		return (pattern_segs, None)
	def pattern_to_regex(
		pattern: AnyStr,
	) -> tuple[Optional[AnyStr], Optional[bool]]:
		Convert the pattern into a regular expression.
		*pattern* (:class:`str` or :class:`bytes`) is the pattern to convert into a
		Returns a :class:`tuple` containing:
			-	*pattern* (:class:`str`, :class:`bytes` or :data:`None`) is the
				uncompiled regular expression.
			-	*include* (:class:`bool` or :data:`None`) is whether matched files
				should be included (:data:`True`), excluded (:data:`False`), or is a
				null-operation (:data:`None`).
			pattern_str = pattern
			pattern_str = pattern.decode(_BYTES_ENCODING)
			raise TypeError(f"{pattern=!r} is not a unicode or byte string.")
		original_pattern = pattern_str
		del pattern
		if pattern_str.endswith('\\ '):
			# EDGE CASE: Spaces can be escaped with backslash. If a pattern that ends
			# with a backslash is followed by a space, do not strip from the left.
			# EDGE CASE: Leading spaces should be kept (only trailing spaces should be
			# removed).
			pattern_str = pattern_str.rstrip()
		regex: Optional[str]
		if not pattern_str:
			# A blank pattern is a null-operation (neither includes nor excludes
			# files).
		elif pattern_str.startswith('#'):
			# A pattern starting with a hash ('#') serves as a comment (neither
			# includes nor excludes files). Escape the hash with a backslash to match
			# a literal hash (i.e., '\#').
		if pattern_str.startswith('!'):
			# A pattern starting with an exclamation mark ('!') negates the pattern
			# (exclude instead of include). Escape the exclamation mark with a back
			# slash to match a literal exclamation mark (i.e., '\!').
			# Remove leading exclamation mark.
			pattern_str = pattern_str[1:]
		# Split pattern into segments.
		pattern_segs = pattern_str.split('/')
		# Check whether the pattern is specifically a directory pattern before
		# normalization.
		is_dir_pattern = not pattern_segs[-1]
		if pattern_str == '/':
			# EDGE CASE: A single slash ('/') is not addressed by the gitignore
			# documentation. Git treats it as a no-op (does not match any files). The
			# straight forward interpretation is to treat it as a directory and match
			# every descendant path (equivalent to '**'). Remove the directory pattern
			# flag so that it is treated as '**' instead of '**/'.
			is_dir_pattern = False
		# Normalize pattern to make processing easier.
			pattern_segs, override_regex = cls.__normalize_segments(
				is_dir_pattern, pattern_segs,
			raise GitIgnorePatternError((
				f"Invalid git pattern: {original_pattern!r}"
			)) from e  # GitIgnorePatternError
		if override_regex is not None:
			# Use regex override.
			regex = override_regex
		elif pattern_segs is not None:
			# Build regular expression from pattern.
				regex_parts = cls.__translate_segments(pattern_segs)
			regex = ''.join(regex_parts)
			assert_unreachable((
				f"{override_regex=} and {pattern_segs=} cannot both be null."
			))  # assert_unreachable
		# Encode regex if needed.
		out_regex: AnyStr
		if regex is not None and return_type is bytes:
			out_regex = regex.encode(_BYTES_ENCODING)
			out_regex = regex
		return (out_regex, include)
	def __translate_segments(cls, pattern_segs: list[str]) -> list[str]:
		Translate the pattern segments to regular expressions.
		segments.
		Returns the regular expression parts (:class:`list` of :class:`str`).
		out_parts = []
		need_slash = False
		end = len(pattern_segs) - 1
		for i, seg in enumerate(pattern_segs):
			if seg == '**':
					# A normalized pattern beginning with double-asterisks ('**') will
					# match any leading path segments.
					# - NOTICE: '(?:^|/)' benchmarks slower using p15 (sm=0.9382,
					#   hs=0.9966, re2=0.9337).
					out_parts.append('^(?:.+/)?')
				elif i < end:
					# A pattern with inner double-asterisks ('**') will match multiple (or
					# zero) inner path segments.
					out_parts.append('(?:/.+)?')
					need_slash = True
					assert i == end, (i, end)
					# A normalized pattern ending with double-asterisks ('**') will match
					# any trailing path segments.
					out_parts.append('/')
				# Match path segment.
					# Anchor to root directory.
					out_parts.append('^')
				if need_slash:
				if seg == '*':
					# Match whole path segment.
					out_parts.append('[^/]+')
					# Match segment glob pattern.
					out_parts.append(cls._translate_segment_glob(seg))
				if i == end:
						# A pattern ending with an asterisk ('*') will match a file or
						# directory (without matching descendant paths). E.g., "foo/*"
						# matches "foo/test.json", "foo/bar/", but not "foo/bar/hello.c".
						out_parts.append('/?$')
						# A pattern ending without a slash ('/') will match a file or a
						# directory (with paths underneath it). E.g., "foo" matches "foo",
						# "foo/bar", "foo/bar/baz", etc.
						out_parts.append('(?:/|$)')
		return out_parts
# Register GitIgnoreBasicPattern as "gitignore".
util.register_pattern('gitignore', GitIgnoreBasicPattern)
==========================
Bipartite Graph Algorithms
import networkx as nx
from networkx.algorithms.components import connected_components
from networkx.exception import AmbiguousSolution
    "is_bipartite",
    "is_bipartite_node_set",
    "color",
    "sets",
    "density",
    "degrees",
@nx._dispatchable
def color(G):
    """Returns a two-coloring of the graph.
    Raises an exception if the graph is not bipartite.
    G : NetworkX graph
    color : dictionary
        A dictionary keyed by node with a 1 or 0 as data for each node color.
    NetworkXError
        If the graph is not two-colorable.
    >>> G = nx.path_graph(4)
    >>> c = bipartite.color(G)
    {0: 1, 1: 0, 2: 1, 3: 0}
    You can use this to set a node attribute indicating the bipartite set:
    >>> nx.set_node_attributes(G, c, "bipartite")
    >>> print(G.nodes[0]["bipartite"])
    >>> print(G.nodes[1]["bipartite"])
    if G.is_directed():
        def neighbors(v):
            return itertools.chain.from_iterable([G.predecessors(v), G.successors(v)])
        neighbors = G.neighbors
    color = {}
    for n in G:  # handle disconnected graphs
        if n in color or len(G[n]) == 0:  # skip isolates
        queue = [n]
        color[n] = 1  # nodes seen with color (1 or 0)
        while queue:
            v = queue.pop()
            c = 1 - color[v]  # opposite color of node v
            for w in neighbors(v):
                if w in color:
                    if color[w] == color[v]:
                        raise nx.NetworkXError("Graph is not bipartite.")
                    color[w] = c
                    queue.append(w)
    # color isolates with 0
    color.update(dict.fromkeys(nx.isolates(G), 0))
    return color
def is_bipartite(G):
    """Returns True if graph G is bipartite, False if not.
    >>> print(bipartite.is_bipartite(G))
    color, is_bipartite_node_set
        color(G)
    except nx.NetworkXError:
def is_bipartite_node_set(G, nodes):
    """Returns True if nodes and G/nodes are a bipartition of G.
    nodes: list or container
      Check if nodes are a one of a bipartite set.
    >>> X = set([1, 3])
    >>> bipartite.is_bipartite_node_set(G, X)
    An exception is raised if the input nodes are not distinct, because in this
    case some bipartite algorithms will yield incorrect results.
    For connected graphs the bipartite sets are unique.  This function handles
    disconnected graphs.
    S = set(nodes)
    if len(S) < len(nodes):
        # this should maybe just return False?
        raise AmbiguousSolution(
            "The input node set contains duplicates.\n"
            "This may lead to incorrect results when using it in bipartite algorithms.\n"
            "Consider using set(nodes) as the input"
    for CC in (G.subgraph(c).copy() for c in connected_components(G)):
        X, Y = sets(CC)
            (X.issubset(S) and Y.isdisjoint(S)) or (Y.issubset(S) and X.isdisjoint(S))
def sets(G, top_nodes=None):
    """Returns bipartite node sets of graph G.
    Raises an exception if the graph is not bipartite or if the input
    graph is disconnected and thus more than one valid solution exists.
    See :mod:`bipartite documentation <networkx.algorithms.bipartite>`
    for further details on how bipartite graphs are handled in NetworkX.
    top_nodes : container, optional
      Container with all nodes in one bipartite node set. If not supplied
      it will be computed. But if more than one solution exists an exception
      will be raised.
    X : set
      Nodes from one side of the bipartite graph.
    Y : set
      Nodes from the other side.
    AmbiguousSolution
      Raised if the input bipartite graph is disconnected and no container
      with all nodes in one bipartite set is provided. When determining
      the nodes in each bipartite set more than one valid solution is
      possible if the input graph is disconnected.
      Raised if the input graph is not bipartite.
    >>> X, Y = bipartite.sets(G)
    >>> list(X)
    >>> list(Y)
    [1, 3]
        is_connected = nx.is_weakly_connected
        is_connected = nx.is_connected
    if top_nodes is not None:
        X = set(top_nodes)
        Y = set(G) - X
        if not is_connected(G):
            msg = "Disconnected graph: Ambiguous solution for bipartite sets."
            raise nx.AmbiguousSolution(msg)
        c = color(G)
        X = {n for n, is_top in c.items() if is_top}
        Y = {n for n, is_top in c.items() if not is_top}
    return (X, Y)
@nx._dispatchable(graphs="B")
def density(B, nodes):
    """Returns density of bipartite graph B.
    B : NetworkX graph
      Nodes in one node set of the bipartite graph.
    d : float
       The bipartite density
    >>> G = nx.complete_bipartite_graph(3, 2)
    >>> X = set([0, 1, 2])
    >>> bipartite.density(G, X)
    >>> Y = set([3, 4])
    >>> bipartite.density(G, Y)
    The container of nodes passed as argument must contain all nodes
    in one of the two bipartite node sets to avoid ambiguity in the
    case of disconnected graphs.
    n = len(B)
    m = nx.number_of_edges(B)
    nb = len(nodes)
    nt = n - nb
    if m == 0:  # includes cases n==0 and n==1
        d = 0.0
        if B.is_directed():
            d = m / (2 * nb * nt)
            d = m / (nb * nt)
@nx._dispatchable(graphs="B", edge_attrs="weight")
def degrees(B, nodes, weight=None):
    """Returns the degrees of the two node sets in the bipartite graph B.
    weight : string or None, optional (default=None)
       The edge attribute that holds the numerical value used as a weight.
       If None, then each edge has weight 1.
       The degree is the sum of the edge weights adjacent to the node.
    (degX,degY) : tuple of dictionaries
       The degrees of the two bipartite sets as dictionaries keyed by node.
    >>> degX, degY = bipartite.degrees(G, Y)
    >>> dict(degX)
    {0: 2, 1: 2, 2: 2}
    color, density
    bottom = set(nodes)
    top = set(B) - bottom
    return (B.degree(top, weight), B.degree(bottom, weight))
Discrete Fourier Transforms - basic.py
from . import pypocketfft as pfft
from .helper import (_asfarray, _init_nd_shape_and_axes, _datacopied,
                     _fix_shape, _fix_shape_1d, _normalization,
                     _workers)
def c2c(forward, x, n=None, axis=-1, norm=None, overwrite_x=False,
        workers=None, *, plan=None):
    """ Return discrete Fourier transform of real or complex sequence. """
    if plan is not None:
        raise NotImplementedError('Passing a precomputed plan is not yet '
                                  'supported by scipy.fft functions')
    tmp = _asfarray(x)
    overwrite_x = overwrite_x or _datacopied(tmp, x)
    norm = _normalization(norm, forward)
    workers = _workers(workers)
        tmp, copied = _fix_shape_1d(tmp, n, axis)
        overwrite_x = overwrite_x or copied
    elif tmp.shape[axis] < 1:
        message = f"invalid number of data points ({tmp.shape[axis]}) specified"
        raise ValueError(message)
    out = (tmp if overwrite_x and tmp.dtype.kind == 'c' else None)
    return pfft.c2c(tmp, (axis,), forward, norm, out, workers)
fft = functools.partial(c2c, True)
fft.__name__ = 'fft'
ifft = functools.partial(c2c, False)
ifft.__name__ = 'ifft'
def r2c(forward, x, n=None, axis=-1, norm=None, overwrite_x=False,
    Discrete Fourier transform of a real sequence.
    if not np.isrealobj(tmp):
        raise TypeError("x must be a real sequence")
        tmp, _ = _fix_shape_1d(tmp, n, axis)
        raise ValueError(f"invalid number of data points ({tmp.shape[axis]}) specified")
    # Note: overwrite_x is not utilised
    return pfft.r2c(tmp, (axis,), forward, norm, None, workers)
rfft = functools.partial(r2c, True)
rfft.__name__ = 'rfft'
ihfft = functools.partial(r2c, False)
ihfft.__name__ = 'ihfft'
def c2r(forward, x, n=None, axis=-1, norm=None, overwrite_x=False,
    Return inverse discrete Fourier transform of real sequence x.
    # TODO: Optimize for hermitian and real?
    if np.isrealobj(tmp):
        tmp = tmp + 0.j
    # Last axis utilizes hermitian symmetry
        n = (tmp.shape[axis] - 1) * 2
        if n < 1:
            raise ValueError(f"Invalid number of data points ({n}) specified")
        tmp, _ = _fix_shape_1d(tmp, (n//2) + 1, axis)
    # Note: overwrite_x is not utilized
    return pfft.c2r(tmp, (axis,), n, forward, norm, None, workers)
hfft = functools.partial(c2r, True)
hfft.__name__ = 'hfft'
irfft = functools.partial(c2r, False)
irfft.__name__ = 'irfft'
def hfft2(x, s=None, axes=(-2,-1), norm=None, overwrite_x=False, workers=None,
          *, plan=None):
    2-D discrete Fourier transform of a Hermitian sequence
    return hfftn(x, s, axes, norm, overwrite_x, workers)
def ihfft2(x, s=None, axes=(-2,-1), norm=None, overwrite_x=False, workers=None,
    2-D discrete inverse Fourier transform of a Hermitian sequence
    return ihfftn(x, s, axes, norm, overwrite_x, workers)
def c2cn(forward, x, s=None, axes=None, norm=None, overwrite_x=False,
    Return multidimensional discrete Fourier transform.
    shape, axes = _init_nd_shape_and_axes(tmp, s, axes)
    if len(axes) == 0:
    tmp, copied = _fix_shape(tmp, shape, axes)
    return pfft.c2c(tmp, axes, forward, norm, out, workers)
fftn = functools.partial(c2cn, True)
fftn.__name__ = 'fftn'
ifftn = functools.partial(c2cn, False)
ifftn.__name__ = 'ifftn'
def r2cn(forward, x, s=None, axes=None, norm=None, overwrite_x=False,
    """Return multidimensional discrete Fourier transform of real input"""
    tmp, _ = _fix_shape(tmp, shape, axes)
        raise ValueError("at least 1 axis must be transformed")
    return pfft.r2c(tmp, axes, forward, norm, None, workers)
rfftn = functools.partial(r2cn, True)
rfftn.__name__ = 'rfftn'
ihfftn = functools.partial(r2cn, False)
ihfftn.__name__ = 'ihfftn'
def c2rn(forward, x, s=None, axes=None, norm=None, overwrite_x=False,
    """Multidimensional inverse discrete fourier transform with real output"""
    noshape = s is None
    shape = list(shape)
    if noshape:
        shape[-1] = (x.shape[axes[-1]] - 1) * 2
    lastsize = shape[-1]
    shape[-1] = (shape[-1] // 2) + 1
    tmp, _ = tuple(_fix_shape(tmp, shape, axes))
    return pfft.c2r(tmp, axes, lastsize, forward, norm, None, workers)
hfftn = functools.partial(c2rn, True)
hfftn.__name__ = 'hfftn'
irfftn = functools.partial(c2rn, False)
irfftn.__name__ = 'irfftn'
def r2r_fftpack(forward, x, n=None, axis=-1, norm=None, overwrite_x=False):
    """FFT of a real sequence, returning fftpack half complex format"""
    workers = _workers(None)
    if tmp.dtype.kind == 'c':
        raise TypeError('x must be a real sequence')
    out = (tmp if overwrite_x else None)
    return pfft.r2r_fftpack(tmp, (axis,), forward, forward, norm, out, workers)
rfft_fftpack = functools.partial(r2r_fftpack, True)
rfft_fftpack.__name__ = 'rfft_fftpack'
irfft_fftpack = functools.partial(r2r_fftpack, False)
irfft_fftpack.__name__ = 'irfft_fftpack'
# This file is not meant for public use and will be removed in SciPy v2.0.0.
# Use the `scipy.fftpack` namespace for importing the functions
# included below.
from scipy._lib.deprecation import _sub_module_deprecation
__all__ = [  # noqa: F822
    'fft','ifft','fftn','ifftn','rfft','irfft',
    'fft2','ifft2'
    return _sub_module_deprecation(sub_package="fftpack", module="basic",
                                   private_modules=["_basic"], all=__all__,
                                   attribute=name)
# Use the `scipy.linalg` namespace for importing the functions
    'solve', 'solve_triangular', 'solveh_banded', 'solve_banded',
    'solve_toeplitz', 'solve_circulant', 'inv', 'det', 'lstsq',
    'pinv', 'pinvh', 'matrix_balance', 'matmul_toeplitz',
    'get_lapack_funcs', 'LinAlgError', 'LinAlgWarning',
    return _sub_module_deprecation(sub_package="linalg", module="basic",
# Use the `scipy.special` namespace for importing the functions
    'ai_zeros',
    'assoc_laguerre',
    'bei_zeros',
    'beip_zeros',
    'ber_zeros',
    'bernoulli',
    'berp_zeros',
    'bi_zeros',
    'clpmn',
    'comb',
    'digamma',
    'diric',
    'erf_zeros',
    'euler',
    'factorial',
    'factorial2',
    'factorialk',
    'fresnel_zeros',
    'fresnelc_zeros',
    'fresnels_zeros',
    'h1vp',
    'h2vp',
    'hankel1',
    'hankel2',
    'iv',
    'ivp',
    'jn_zeros',
    'jnjnp_zeros',
    'jnp_zeros',
    'jnyn_zeros',
    'jv',
    'jvp',
    'kei_zeros',
    'keip_zeros',
    'kelvin_zeros',
    'ker_zeros',
    'kerp_zeros',
    'kv',
    'kvp',
    'lmbda',
    'lpmn',
    'lpn',
    'lqmn',
    'lqn',
    'mathieu_a',
    'mathieu_b',
    'mathieu_even_coef',
    'mathieu_odd_coef',
    'obl_cv_seq',
    'pbdn_seq',
    'pbdv_seq',
    'pbvv_seq',
    'perm',
    'polygamma',
    'pro_cv_seq',
    'psi',
    'riccati_jn',
    'riccati_yn',
    'sinc',
    'y0_zeros',
    'y1_zeros',
    'y1p_zeros',
    'yn_zeros',
    'ynp_zeros',
    'yv',
    'yvp',
    'zeta'
    return _sub_module_deprecation(sub_package="special", module="basic",
                                   private_modules=["_basic", "_ufuncs"], all=__all__,
# pylint: disable=function-redefined
from prompt_toolkit.filters import (
    emacs_insert_mode,
    has_selection,
    in_paste_mode,
    is_multiline,
    vi_insert_mode,
from prompt_toolkit.key_binding.key_processor import KeyPress, KeyPressEvent
from prompt_toolkit.keys import Keys
from ..key_bindings import KeyBindings
from .named_commands import get_by_name
    "load_basic_bindings",
E = KeyPressEvent
def if_no_repeat(event: E) -> bool:
    """Callable that returns True when the previous event was delivered to
    another handler."""
    return not event.is_repeat
def has_text_before_cursor() -> bool:
    return bool(get_app().current_buffer.text)
def in_quoted_insert() -> bool:
    return get_app().quoted_insert
def load_basic_bindings() -> KeyBindings:
    key_bindings = KeyBindings()
    insert_mode = vi_insert_mode | emacs_insert_mode
    handle = key_bindings.add
    @handle("c-a")
    @handle("c-b")
    @handle("c-c")
    @handle("c-d")
    @handle("c-e")
    @handle("c-f")
    @handle("c-g")
    @handle("c-h")
    @handle("c-i")
    @handle("c-j")
    @handle("c-k")
    @handle("c-l")
    @handle("c-m")
    @handle("c-n")
    @handle("c-o")
    @handle("c-p")
    @handle("c-q")
    @handle("c-r")
    @handle("c-s")
    @handle("c-t")
    @handle("c-u")
    @handle("c-v")
    @handle("c-w")
    @handle("c-x")
    @handle("c-y")
    @handle("c-z")
    @handle("f1")
    @handle("f2")
    @handle("f3")
    @handle("f4")
    @handle("f5")
    @handle("f6")
    @handle("f7")
    @handle("f8")
    @handle("f9")
    @handle("f10")
    @handle("f11")
    @handle("f12")
    @handle("f13")
    @handle("f14")
    @handle("f15")
    @handle("f16")
    @handle("f17")
    @handle("f18")
    @handle("f19")
    @handle("f20")
    @handle("f21")
    @handle("f22")
    @handle("f23")
    @handle("f24")
    @handle("c-@")  # Also c-space.
    @handle("c-\\")
    @handle("c-]")
    @handle("c-^")
    @handle("c-_")
    @handle("backspace")
    @handle("up")
    @handle("down")
    @handle("right")
    @handle("left")
    @handle("s-up")
    @handle("s-down")
    @handle("s-right")
    @handle("s-left")
    @handle("home")
    @handle("end")
    @handle("s-home")
    @handle("s-end")
    @handle("delete")
    @handle("s-delete")
    @handle("c-delete")
    @handle("pageup")
    @handle("pagedown")
    @handle("s-tab")
    @handle("tab")
    @handle("c-s-left")
    @handle("c-s-right")
    @handle("c-s-home")
    @handle("c-s-end")
    @handle("c-left")
    @handle("c-right")
    @handle("c-up")
    @handle("c-down")
    @handle("c-home")
    @handle("c-end")
    @handle("insert")
    @handle("s-insert")
    @handle("c-insert")
    @handle("<sigint>")
    @handle(Keys.Ignore)
    def _ignore(event: E) -> None:
        First, for any of these keys, Don't do anything by default. Also don't
        catch them in the 'Any' handler which will insert them as data.
        If people want to insert these characters as a literal, they can always
        do by doing a quoted insert. (ControlQ in emacs mode, ControlV in Vi
        mode.)
    # Readline-style bindings.
    handle("home")(get_by_name("beginning-of-line"))
    handle("end")(get_by_name("end-of-line"))
    handle("left")(get_by_name("backward-char"))
    handle("right")(get_by_name("forward-char"))
    handle("c-up")(get_by_name("previous-history"))
    handle("c-down")(get_by_name("next-history"))
    handle("c-l")(get_by_name("clear-screen"))
    handle("c-k", filter=insert_mode)(get_by_name("kill-line"))
    handle("c-u", filter=insert_mode)(get_by_name("unix-line-discard"))
    handle("backspace", filter=insert_mode, save_before=if_no_repeat)(
        get_by_name("backward-delete-char")
    handle("delete", filter=insert_mode, save_before=if_no_repeat)(
        get_by_name("delete-char")
    handle("c-delete", filter=insert_mode, save_before=if_no_repeat)(
    handle(Keys.Any, filter=insert_mode, save_before=if_no_repeat)(
        get_by_name("self-insert")
    handle("c-t", filter=insert_mode)(get_by_name("transpose-chars"))
    handle("c-i", filter=insert_mode)(get_by_name("menu-complete"))
    handle("s-tab", filter=insert_mode)(get_by_name("menu-complete-backward"))
    # Control-W should delete, using whitespace as separator, while M-Del
    # should delete using [^a-zA-Z0-9] as a boundary.
    handle("c-w", filter=insert_mode)(get_by_name("unix-word-rubout"))
    handle("pageup", filter=~has_selection)(get_by_name("previous-history"))
    handle("pagedown", filter=~has_selection)(get_by_name("next-history"))
    # CTRL keys.
    handle("c-d", filter=has_text_before_cursor & insert_mode)(
    @handle("enter", filter=insert_mode & is_multiline)
    def _newline(event: E) -> None:
        Newline (in case of multiline input.
        event.current_buffer.newline(copy_margin=not in_paste_mode())
    def _newline2(event: E) -> None:
        By default, handle \n as if it were a \r (enter).
        (It appears that some terminals send \n instead of \r when pressing
        enter. - at least the Linux subsystem for Windows.)
        event.key_processor.feed(KeyPress(Keys.ControlM, "\r"), first=True)
    # Delete the word before the cursor.
    def _go_up(event: E) -> None:
        event.current_buffer.auto_up(count=event.arg)
    def _go_down(event: E) -> None:
        event.current_buffer.auto_down(count=event.arg)
    @handle("delete", filter=has_selection)
    def _cut(event: E) -> None:
        data = event.current_buffer.cut_selection()
        event.app.clipboard.set_data(data)
    # Global bindings.
    def _insert_ctrl_z(event: E) -> None:
        By default, control-Z should literally insert Ctrl-Z.
        (Ansi Ctrl-Z, code 26 in MSDOS means End-Of-File.
        In a Python REPL for instance, it's possible to type
        Control-Z followed by enter to quit.)
        When the system bindings are loaded and suspend-to-background is
        supported, that will override this binding.
        event.current_buffer.insert_text(event.data)
    @handle(Keys.BracketedPaste)
    def _paste(event: E) -> None:
        Pasting from clipboard.
        # Be sure to use \n as line ending.
        # Some terminals (Like iTerm2) seem to paste \r\n line endings in a
        # bracketed paste. See: https://github.com/ipython/ipython/issues/9737
        data = data.replace("\r\n", "\n")
        data = data.replace("\r", "\n")
        event.current_buffer.insert_text(data)
    @handle(Keys.Any, filter=in_quoted_insert, eager=True)
    def _insert_text(event: E) -> None:
        Handle quoted insert.
        event.current_buffer.insert_text(event.data, overwrite=False)
        event.app.quoted_insert = False
    return key_bindings
