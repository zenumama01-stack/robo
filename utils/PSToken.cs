    Contents:    PowerShell token interface for syntax editors
    Classes:     System.Management.Automation.PSToken
    /// This is public class for representing a powershell token.
    /// There is already an internal class Token for representing the token.
    /// This class wraps the internal Token class for providing limited information
    /// to syntax editor.
    public sealed class PSToken
        internal PSToken(Token token)
            Type = GetPSTokenType(token);
            _extent = token.Extent;
            if (token is StringToken)
                _content = ((StringToken)token).Value;
            else if (token is VariableToken)
                _content = ((VariableToken)token).VariablePath.ToString();
        internal PSToken(IScriptExtent extent)
            Type = PSTokenType.Position;
            _extent = extent;
        /// Resulting text for the token.
        /// The text here represents the content of token. It can be the same as
        /// the text chunk within script resulting into this token, but usually is not
        /// the case.
        /// For example, -name in following command result into a parameter token.
        ///     get-process -name foo
        /// Text property in this case is 'name' instead of '-name'.
                return _content ?? _extent.Text;
        private readonly string _content;
        #region Token Type
        /// Map a V3 token to a V2 PSTokenType.
        /// <param name="token">The V3 token.</param>
        /// <returns>The V2 PSTokenType.</returns>
        public static PSTokenType GetPSTokenType(Token token)
            if ((token.TokenFlags & TokenFlags.CommandName) != 0)
                return PSTokenType.Command;
            if ((token.TokenFlags & TokenFlags.MemberName) != 0)
                return PSTokenType.Member;
            if ((token.TokenFlags & TokenFlags.AttributeName) != 0)
                return PSTokenType.Attribute;
            if ((token.TokenFlags & TokenFlags.TypeName) != 0)
                return PSTokenType.Type;
            return s_tokenKindMapping[(int)token.Kind];
        /// Token type.
        public PSTokenType Type { get; }
        private static readonly PSTokenType[] s_tokenKindMapping = new PSTokenType[]
            #region Flags for unclassified tokens
            /*              Unknown */ PSTokenType.Unknown,
            /*             Variable */ PSTokenType.Variable,
            /*     SplattedVariable */ PSTokenType.Variable,
            /*            Parameter */ PSTokenType.CommandParameter,
            /*               Number */ PSTokenType.Number,
            /*                Label */ PSTokenType.LoopLabel,
            /*           Identifier */ PSTokenType.CommandArgument,
            /*              Generic */ PSTokenType.CommandArgument,
            /*              Newline */ PSTokenType.NewLine,
            /*     LineContinuation */ PSTokenType.LineContinuation,
            /*              Comment */ PSTokenType.Comment,
            /*           EndOfInput */ PSTokenType.Unknown,
            #endregion Flags for unclassified tokens
            #region Flags for strings
            /*        StringLiteral */ PSTokenType.String,
            /*     StringExpandable */ PSTokenType.String,
            /*    HereStringLiteral */ PSTokenType.String,
            /* HereStringExpandable */ PSTokenType.String,
            #endregion Flags for strings
            #region Flags for punctuators
            /*               LParen */ PSTokenType.GroupStart,
            /*               RParen */ PSTokenType.GroupEnd,
            /*               LCurly */ PSTokenType.GroupStart,
            /*               RCurly */ PSTokenType.GroupEnd,
            /*             LBracket */ PSTokenType.Operator,
            /*             RBracket */ PSTokenType.Operator,
            /*              AtParen */ PSTokenType.GroupStart,
            /*              AtCurly */ PSTokenType.GroupStart,
            /*          DollarParen */ PSTokenType.GroupStart,
            /*                 Semi */ PSTokenType.StatementSeparator,
            #endregion Flags for punctuators
            #region Flags for operators
            /*               AndAnd */ PSTokenType.Operator,
            /*                 OrOr */ PSTokenType.Operator,
            /*            Ampersand */ PSTokenType.Operator,
            /*                 Pipe */ PSTokenType.Operator,
            /*                Comma */ PSTokenType.Operator,
            /*           MinusMinus */ PSTokenType.Operator,
            /*             PlusPlus */ PSTokenType.Operator,
            /*               DotDot */ PSTokenType.Operator,
            /*           ColonColon */ PSTokenType.Operator,
            /*                  Dot */ PSTokenType.Operator,
            /*              Exclaim */ PSTokenType.Operator,
            /*             Multiply */ PSTokenType.Operator,
            /*               Divide */ PSTokenType.Operator,
            /*                  Rem */ PSTokenType.Operator,
            /*                 Plus */ PSTokenType.Operator,
            /*                Minus */ PSTokenType.Operator,
            /*               Equals */ PSTokenType.Operator,
            /*           PlusEquals */ PSTokenType.Operator,
            /*          MinusEquals */ PSTokenType.Operator,
            /*       MultiplyEquals */ PSTokenType.Operator,
            /*         DivideEquals */ PSTokenType.Operator,
            /*      RemainderEquals */ PSTokenType.Operator,
            /*          Redirection */ PSTokenType.Operator,
            /*        RedirectInStd */ PSTokenType.Operator,
            /*               Format */ PSTokenType.Operator,
            /*                  Not */ PSTokenType.Operator,
            /*                 Bnot */ PSTokenType.Operator,
            /*                  And */ PSTokenType.Operator,
            /*                   Or */ PSTokenType.Operator,
            /*                  Xor */ PSTokenType.Operator,
            /*                 Band */ PSTokenType.Operator,
            /*                  Bor */ PSTokenType.Operator,
            /*                 Bxor */ PSTokenType.Operator,
            /*                 Join */ PSTokenType.Operator,
            /*                  Ieq */ PSTokenType.Operator,
            /*                  Ine */ PSTokenType.Operator,
            /*                  Ige */ PSTokenType.Operator,
            /*                  Igt */ PSTokenType.Operator,
            /*                  Ilt */ PSTokenType.Operator,
            /*                  Ile */ PSTokenType.Operator,
            /*                Ilike */ PSTokenType.Operator,
            /*             Inotlike */ PSTokenType.Operator,
            /*               Imatch */ PSTokenType.Operator,
            /*            Inotmatch */ PSTokenType.Operator,
            /*             Ireplace */ PSTokenType.Operator,
            /*            Icontains */ PSTokenType.Operator,
            /*         Inotcontains */ PSTokenType.Operator,
            /*                  Iin */ PSTokenType.Operator,
            /*               Inotin */ PSTokenType.Operator,
            /*               Isplit */ PSTokenType.Operator,
            /*                  Ceq */ PSTokenType.Operator,
            /*                  Cne */ PSTokenType.Operator,
            /*                  Cge */ PSTokenType.Operator,
            /*                  Cgt */ PSTokenType.Operator,
            /*                  Clt */ PSTokenType.Operator,
            /*                  Cle */ PSTokenType.Operator,
            /*                Clike */ PSTokenType.Operator,
            /*             Cnotlike */ PSTokenType.Operator,
            /*               Cmatch */ PSTokenType.Operator,
            /*            Cnotmatch */ PSTokenType.Operator,
            /*             Creplace */ PSTokenType.Operator,
            /*            Ccontains */ PSTokenType.Operator,
            /*         Cnotcontains */ PSTokenType.Operator,
            /*                  Cin */ PSTokenType.Operator,
            /*               Cnotin */ PSTokenType.Operator,
            /*               Csplit */ PSTokenType.Operator,
            /*                   Is */ PSTokenType.Operator,
            /*                IsNot */ PSTokenType.Operator,
            /*                   As */ PSTokenType.Operator,
            /*      PostFixPlusPlus */ PSTokenType.Operator,
            /*    PostFixMinusMinus */ PSTokenType.Operator,
            /*                  Shl */ PSTokenType.Operator,
            /*                  Shr */ PSTokenType.Operator,
            /*    Reserved slot 1   */ PSTokenType.Unknown,
            /*    Reserved slot 2   */ PSTokenType.Unknown,
            /*    Reserved slot 3   */ PSTokenType.Unknown,
            /*    Reserved slot 4   */ PSTokenType.Unknown,
            /*    Reserved slot 5   */ PSTokenType.Unknown,
            /*    Reserved slot 6   */ PSTokenType.Unknown,
            /*    Reserved slot 7   */ PSTokenType.Unknown,
            /*    Reserved slot 8   */ PSTokenType.Unknown,
            /*    Reserved slot 9   */ PSTokenType.Unknown,
            /*    Reserved slot 10  */ PSTokenType.Unknown,
            /*    Reserved slot 11  */ PSTokenType.Unknown,
            /*    Reserved slot 12  */ PSTokenType.Unknown,
            /*    Reserved slot 13  */ PSTokenType.Unknown,
            /*    Reserved slot 14  */ PSTokenType.Unknown,
            /*    Reserved slot 15  */ PSTokenType.Unknown,
            /*    Reserved slot 16  */ PSTokenType.Unknown,
            /*    Reserved slot 17  */ PSTokenType.Unknown,
            /*    Reserved slot 18  */ PSTokenType.Unknown,
            /*    Reserved slot 19  */ PSTokenType.Unknown,
            /*    Reserved slot 20  */ PSTokenType.Unknown,
            #endregion Flags for operators
            #region Flags for keywords
            /*                Begin */ PSTokenType.Keyword,
            /*                Break */ PSTokenType.Keyword,
            /*                Catch */ PSTokenType.Keyword,
            /*                Class */ PSTokenType.Keyword,
            /*             Continue */ PSTokenType.Keyword,
            /*                 Data */ PSTokenType.Keyword,
            /*               Define */ PSTokenType.Keyword,
            /*                   Do */ PSTokenType.Keyword,
            /*         Dynamicparam */ PSTokenType.Keyword,
            /*                 Else */ PSTokenType.Keyword,
            /*               ElseIf */ PSTokenType.Keyword,
            /*                  End */ PSTokenType.Keyword,
            /*                 Exit */ PSTokenType.Keyword,
            /*               Filter */ PSTokenType.Keyword,
            /*              Finally */ PSTokenType.Keyword,
            /*                  For */ PSTokenType.Keyword,
            /*              Foreach */ PSTokenType.Keyword,
            /*                 From */ PSTokenType.Keyword,
            /*             Function */ PSTokenType.Keyword,
            /*                   If */ PSTokenType.Keyword,
            /*                   In */ PSTokenType.Keyword,
            /*                Param */ PSTokenType.Keyword,
            /*              Process */ PSTokenType.Keyword,
            /*               Return */ PSTokenType.Keyword,
            /*               Switch */ PSTokenType.Keyword,
            /*                Throw */ PSTokenType.Keyword,
            /*                 Trap */ PSTokenType.Keyword,
            /*                  Try */ PSTokenType.Keyword,
            /*                Until */ PSTokenType.Keyword,
            /*                Using */ PSTokenType.Keyword,
            /*                  Var */ PSTokenType.Keyword,
            /*                While */ PSTokenType.Keyword,
            /*             Workflow */ PSTokenType.Keyword,
            /*             Parallel */ PSTokenType.Keyword,
            /*             Sequence */ PSTokenType.Keyword,
            /*         InlineScript */ PSTokenType.Keyword,
            /*        Configuration */ PSTokenType.Keyword,
            /*       DynamicKeyword */ PSTokenType.Keyword,
            /*               Public */ PSTokenType.Keyword,
            /*              Private */ PSTokenType.Keyword,
            /*               Static */ PSTokenType.Keyword,
            /*            Interface */ PSTokenType.Keyword,
            /*                 Enum */ PSTokenType.Keyword,
            /*            Namespace */ PSTokenType.Keyword,
            /*               Module */ PSTokenType.Keyword,
            /*                 Type */ PSTokenType.Keyword,
            /*             Assembly */ PSTokenType.Keyword,
            /*              Command */ PSTokenType.Keyword,
            /*               Hidden */ PSTokenType.Keyword,
            /*                 Base */ PSTokenType.Keyword,
            /*              Default */ PSTokenType.Keyword,
            #endregion Flags for keywords
            /*            LastToken */ PSTokenType.Unknown,
        #region Position Information
        private readonly IScriptExtent _extent;
        /// Offset of token start in script buffer.
        public int Start
            get { return _extent.StartOffset; }
        /// Offset of token end in script buffer.
                return _extent.EndOffset - _extent.StartOffset;
        /// Line number of token start.
        /// StartLine, StartColumn, EndLine, and EndColumn are 1-based,
        /// i.e., first line has a line number 1 and first character in
        /// a line has column number 1.
        public int StartLine { get { return _extent.StartLineNumber; } }
        /// Position of token start in start line.
        public int StartColumn { get { return _extent.StartColumnNumber; } }
        /// Line number of token end.
        public int EndLine { get { return _extent.EndLineNumber; } }
        /// Position of token end in end line.
        public int EndColumn { get { return _extent.EndColumnNumber; } }
    /// PowerShell token types.
    public enum PSTokenType
        /// Unknown token.
        /// Command.
        /// For example, 'get-process' in
        ///     <c><code>get-process -name foo</code></c>
        /// Command Parameter.
        /// For example, '-name' in
        CommandParameter,
        /// Command Argument.
        /// For example, 'foo' in
        CommandArgument,
        /// Number.
        /// For example, 12 in
        ///     <c><code>$a=12</code></c>
        Number,
        /// String.
        /// For example, "12" in
        ///     <c><code>$a="12"</code></c>
        /// Variable.
        /// For example, $a in
        /// Property name or method name.
        /// For example, Name in
        ///     <c><code>$a.Name</code></c>
        Member,
        /// Loop label.
        /// For example, :loop in
        /// <c><code>
        ///     :loop
        ///     foreach($a in $b)
        ///         $a
        /// </code></c>
        LoopLabel,
        /// Attributes.
        /// For example, Mandatory in
        ///     <c><code>param([Mandatory] $a)</code></c>
        Attribute,
        /// Types.
        /// For example, [string] in
        ///     <c><code>$a = [string] 12</code></c>
        /// Operators.
        /// For example, + in
        ///     <c><code>$a = 1 + 2</code></c>
        Operator,
        /// Group Starter.
        /// For example, { in
        ///     if ($a -gt 4)
        ///         $a++;
        GroupStart,
        /// Group Ender.
        /// For example, } in
        GroupEnd,
        /// Keyword.
        /// For example, if in
        Keyword,
        /// Comment.
        /// For example, #here in
        ///     #here
        Comment,
        /// Statement separator. This is ';'
        /// For example, ; in
        StatementSeparator,
        /// New line. This is '\n'
        /// For example, \n in
        NewLine,
        /// Line continuation.
        /// For example, ` in
        ///     get-command -name `
        ///     foo
        LineContinuation,
        /// Position token.
        /// Position tokens are bogus tokens generated for identifying a location
        /// in the script.
        Position
