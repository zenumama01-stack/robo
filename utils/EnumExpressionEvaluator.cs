    /// Takes as input a collection of strings and builds an expression tree from the input.
    /// At the evaluation stage, it walks down the tree and evaluates the result.
    public sealed class FlagsExpression<T> where T : struct, IConvertible
        /// Construct the expression from a single string.
        /// <param name="expression">
        /// The specified flag attribute expression string.
        public FlagsExpression(string expression)
            if (!typeof(T).IsEnum)
                throw InterpreterError.NewInterpreterException(expression, typeof(RuntimeException),
                    null, "InvalidGenericType", EnumExpressionEvaluatorStrings.InvalidGenericType);
            _underType = Enum.GetUnderlyingType(typeof(T));
            if (string.IsNullOrWhiteSpace(expression))
                    null, "EmptyInputString", EnumExpressionEvaluatorStrings.EmptyInputString);
            List<Token> tokenList = TokenizeInput(expression);
            // Append an OR at the end of the list for construction
            tokenList.Add(new Token(TokenKind.Or));
            CheckSyntaxError(tokenList);
            Root = ConstructExpressionTree(tokenList);
        /// Construct the tree from an object collection when arguments are comma separated.
        /// If valid, all elements are OR separated.
        /// The array of specified flag attribute subexpression strings.
        public FlagsExpression(object[] expression)
                throw InterpreterError.NewInterpreterException(null, typeof(ArgumentNullException),
            foreach (string inputClause in expression)
                if (string.IsNullOrWhiteSpace(inputClause))
            List<Token> tokenList = new List<Token>();
            foreach (string orClause in expression)
                tokenList.AddRange(TokenizeInput(orClause));
            // Unnecessary OR at the end not removed for tree construction
            Debug.Assert(tokenList.Count > 0, "Input must not all be white characters.");
        #region parser tokens
        internal enum TokenKind
            Identifier,
            And,
            Or,
            Not
        internal class Token
            public TokenKind Kind { get; set; }
            internal Token(TokenKind kind)
                Kind = kind;
                switch (kind)
                    case TokenKind.Or:
                        Text = "OR";
                    case TokenKind.And:
                        Text = "AND";
                    case TokenKind.Not:
                        Text = "NOT";
                        Debug.Fail("Invalid token kind passed in.");
            internal Token(string identifier)
                Kind = TokenKind.Identifier;
                Text = identifier;
        #region tree nodes
        /// Abstract base type for other types of nodes in the tree.
        internal abstract class Node
            // Only used in internal nodes holding operators.
            public Node Operand1 { get; set; }
            internal abstract bool Eval(object val);
            internal abstract bool ExistEnum(object enumVal);
        /// OR node for attributes separated by a comma.
        internal class OrNode : Node
            public Node Operand2 { get; set; }
            public OrNode(Node n)
                Operand2 = n;
            internal override bool Eval(object val)
                // bitwise OR
                bool satisfy = Operand1.Eval(val) || Operand2.Eval(val);
                return satisfy;
            internal override bool ExistEnum(object enumVal)
                bool exist = Operand1.ExistEnum(enumVal) || Operand2.ExistEnum(enumVal);
                return exist;
        /// AND node for attributes separated by a plus(+) operator.
        internal class AndNode : Node
            public AndNode(Node n)
                // bitwise AND
                bool satisfy = Operand1.Eval(val) && Operand2.Eval(val);
        /// NOT node for attribute preceded by an exclamation(!) operator.
        internal class NotNode : Node
                // bitwise NOT
                bool satisfy = !(Operand1.Eval(val));
                bool exist = Operand1.ExistEnum(enumVal);
        /// Leaf nodes of the expression tree.
        internal class OperandNode : Node
            internal object _operandValue;
            public object OperandValue
                    return _operandValue;
                    _operandValue = value;
            /// Takes a string value and converts to corresponding enum value.
            /// The string value should be checked at parsing stage prior to
            /// tree construction to ensure it is valid.
            internal OperandNode(string enumString)
                Type enumType = typeof(T);
                Type underType = Enum.GetUnderlyingType(enumType);
                FieldInfo enumItem = enumType.GetField(enumString);
                _operandValue = LanguagePrimitives.ConvertTo(enumItem.GetValue(enumType), underType, CultureInfo.InvariantCulture);
                Type underType = Enum.GetUnderlyingType(typeof(T));
                // bitwise AND checking
                bool satisfy = false;
                if (isUnsigned(underType))
                    ulong valueToCheck = (ulong)LanguagePrimitives.ConvertTo(val, typeof(ulong), CultureInfo.InvariantCulture);
                    ulong operandValue = (ulong)LanguagePrimitives.ConvertTo(_operandValue, typeof(ulong), CultureInfo.InvariantCulture);
                    satisfy = (operandValue == (valueToCheck & operandValue));
                // allow for negative enum value input (though it's not recommended practice for flags attribute)
                    long valueToCheck = (long)LanguagePrimitives.ConvertTo(val, typeof(long), CultureInfo.InvariantCulture);
                    long operandValue = (long)LanguagePrimitives.ConvertTo(_operandValue, typeof(long), CultureInfo.InvariantCulture);
                bool exist = false;
                    ulong valueToCheck = (ulong)LanguagePrimitives.ConvertTo(enumVal, typeof(ulong), CultureInfo.InvariantCulture);
                    exist = valueToCheck == (valueToCheck & operandValue);
                    long valueToCheck = (long)LanguagePrimitives.ConvertTo(enumVal, typeof(long), CultureInfo.InvariantCulture);
            private static bool isUnsigned(Type type)
                return (type == typeof(ulong) || type == typeof(uint) || type == typeof(ushort) || type == typeof(byte));
        private readonly Type _underType = null;
        internal Node Root { get; set; } = null;
        /// Evaluate a given flag enum value against the expression.
        /// The flag enum value to be evaluated.
        /// Whether the enum value satisfy the expression.
        public bool Evaluate(T value)
            object val = LanguagePrimitives.ConvertTo(value, _underType, CultureInfo.InvariantCulture);
            return Root.Eval(val);
        /// Given an enum element, check if the element is present in the expression tree,
        /// which is also present in the input expression.
        /// <param name="flagName">
        /// The enum element to be examined.
        /// Whether the enum element is present in the expression.
        /// The enum value passed in should be a single enum element value,
        /// not a flag enum value with multiple bits set.
        internal bool ExistsInExpression(T flagName)
            object val = LanguagePrimitives.ConvertTo(flagName, _underType, CultureInfo.InvariantCulture);
            exist = Root.ExistEnum(val);
        #region parser methods
        /// Takes a string of input tokenize into a list of ordered tokens.
        /// <param name="input">
        /// The input argument string,
        /// could be partial input (one element from the argument collection).
        /// A generic list of tokenized input.
        private static List<Token> TokenizeInput(string input)
            int _offset = 0;
            while (_offset < input.Length)
                FindNextToken(input, ref _offset);
                if (_offset < input.Length)
                    tokenList.Add(GetNextToken(input, ref _offset));
            return tokenList;
        /// Find the start of the next token, skipping white spaces.
        /// Input string
        /// <param name="_offset">
        /// Current offset position for the string parser.
        private static void FindNextToken(string input, ref int _offset)
                char cc = input[_offset++];
                if (!char.IsWhiteSpace(cc))
                    _offset--;
        /// Given the start (offset) of the next token, traverse through
        /// the string to find the next token, stripping correctly
        /// enclosed quotes.
        /// The next token on the input string
        private static Token GetNextToken(string input, ref int _offset)
            // bool singleQuoted = false;
            // bool doubleQuoted = false;
            bool readingIdentifier = false;
                if ((cc == ',') || (cc == '+') || (cc == '!'))
                    if (!readingIdentifier)
                        sb.Append(cc);
                    readingIdentifier = true;
            string result = sb.ToString().Trim();
            // If resulting identifier is enclosed in paired quotes,
            // remove the only the first pair of quotes from the string
            if (result.Length >= 2 &&
                ((result[0] == '\'' && result[result.Length - 1] == '\'') ||
                (result[0] == '\"' && result[result.Length - 1] == '\"')))
                result = result.Substring(1, result.Length - 2);
            result = result.Trim();
            // possible empty token because white spaces are enclosed in quotation marks.
            if (string.IsNullOrWhiteSpace(result))
                throw InterpreterError.NewInterpreterException(input, typeof(RuntimeException),
                    null, "EmptyTokenString", EnumExpressionEvaluatorStrings.EmptyTokenString,
                    EnumMinimumDisambiguation.EnumAllValues(typeof(T)));
            else if (result[0] == '(')
                int matchIndex = input.IndexOf(')', _offset);
                if (result[result.Length - 1] == ')' || matchIndex >= 0)
                        null, "NoIdentifierGroupingAllowed", EnumExpressionEvaluatorStrings.NoIdentifierGroupingAllowed);
            if (result.Equals(","))
                return (new Token(TokenKind.Or));
            else if (result.Equals("+"))
                return (new Token(TokenKind.And));
            else if (result.Equals("!"))
                return (new Token(TokenKind.Not));
                return (new Token(result));
        /// Checks syntax errors on input expression,
        /// as well as performing disambiguation for identifiers.
        /// <param name="tokenList">
        /// A list of tokenized input.
        private static void CheckSyntaxError(List<Token> tokenList)
            // Initialize, assuming preceded by OR
            TokenKind previous = TokenKind.Or;
            for (int i = 0; i < tokenList.Count; i++)
                Token token = tokenList[i];
                // Not allowed: ... AND/OR AND/OR ...
                // Allowed: ... AND/OR NOT/ID ...
                if (previous == TokenKind.Or || previous == TokenKind.And)
                    if ((token.Kind == TokenKind.Or) || (token.Kind == TokenKind.And))
                        throw InterpreterError.NewInterpreterException(null, typeof(RuntimeException),
                            null, "SyntaxErrorUnexpectedBinaryOperator", EnumExpressionEvaluatorStrings.SyntaxErrorUnexpectedBinaryOperator);
                // Not allowed: ... NOT AND/OR/NOT ...
                // Allowed: ... NOT ID ...
                else if (previous == TokenKind.Not)
                    if (token.Kind != TokenKind.Identifier)
                            null, "SyntaxErrorIdentifierExpected", EnumExpressionEvaluatorStrings.SyntaxErrorIdentifierExpected);
                // Not allowed: ... ID NOT/ID ...
                // Allowed: ... ID AND/OR ...
                else if (previous == TokenKind.Identifier)
                    if ((token.Kind == TokenKind.Identifier) || (token.Kind == TokenKind.Not))
                            null, "SyntaxErrorBinaryOperatorExpected", EnumExpressionEvaluatorStrings.SyntaxErrorBinaryOperatorExpected);
                if (token.Kind == TokenKind.Identifier)
                    string text = token.Text;
                    token.Text = EnumMinimumDisambiguation.EnumDisambiguate(text, typeof(T));
                previous = token.Kind;
        /// Takes a list of tokenized input and create the corresponding expression tree.
        /// Tokenized list of the input string.
        private static Node ConstructExpressionTree(List<Token> tokenList)
            bool notFlag = false;
            Queue<Node> andQueue = new Queue<Node>();
            Queue<Node> orQueue = new Queue<Node>();
                TokenKind kind = token.Kind;
                if (kind == TokenKind.Identifier)
                    Node idNode = new OperandNode(token.Text);
                    if (notFlag)    // identifier preceded by NOT
                        Node notNode = new NotNode();
                        notNode.Operand1 = idNode;
                        notFlag = false;
                        andQueue.Enqueue(notNode);
                        andQueue.Enqueue(idNode);
                else if (kind == TokenKind.Not)
                    notFlag = true;
                else if (kind == TokenKind.And)
                else if (kind == TokenKind.Or)
                    // Dequeue all nodes from AND queue,
                    // create the AND tree, then add to the OR queue.
                    Node andCurrent = andQueue.Dequeue();
                    while (andQueue.Count > 0)
                        Node andNode = new AndNode(andCurrent);
                        andNode.Operand1 = andQueue.Dequeue();
                        andCurrent = andNode;
                    orQueue.Enqueue(andCurrent);
            // Dequeue all nodes from OR queue,
            // create the OR tree (final expression tree)
            Node orCurrent = orQueue.Dequeue();
            while (orQueue.Count > 0)
                Node orNode = new OrNode(orCurrent);
                orNode.Operand1 = orQueue.Dequeue();
                orCurrent = orNode;
            return orCurrent;
