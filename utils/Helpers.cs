    // Miscellaneous helpers that don't belong anywhere else
    internal static class Helpers
        internal static Expression Convert(Expression expression, Type type)
            if (expression.Type == type)
                return expression;
            if (expression.Type == typeof(void))
                return Expression.Block(expression, Expression.Default(type));
                return Expression.Block(expression, Expression.Empty());
            return Expression.Convert(expression, type);
        [System.Diagnostics.Conditional("DEBUG")]
        internal static void NotNull(object value, [CallerArgumentExpression("value")] string? paramName = null)
            ArgumentNullException.ThrowIfNull(value, paramName);
        internal static void Condition(bool precondition, string paramName)
            if (!precondition)
                throw new ArgumentException(paramName);
