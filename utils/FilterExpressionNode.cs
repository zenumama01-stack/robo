    /// The FilterExpressionNode class is the base class for derived
    /// FilterExpressionNodes. FilterExpressionNodes are used to
    /// construct a logical evaluation tree which holds FilterRules as
    /// its operands.
    public abstract class FilterExpressionNode : IEvaluate
        /// In derived classes, this evaluation will return a true or
        /// false result based upon some criteria.
        /// True if the criteria is met, false otherwise.
        public abstract bool Evaluate(object item);
        /// Retrieves all elements of the specified type within the entire expression tree.
        /// <typeparam name="T">The type of the items to find.</typeparam>
        /// <returns>All elements of the specified type within the entire expression tree.</returns>
        public ICollection<T> FindAll<T>()
            var ts = new List<T>();
            var operandNode = this as FilterExpressionOperandNode;
            if (operandNode != null)
                if (typeof(T).IsInstanceOfType(operandNode.Rule))
                    object obj = operandNode.Rule;
                    ts.Add((T)obj);
            var operatorAndNode = this as FilterExpressionAndOperatorNode;
            if (operatorAndNode != null)
                foreach (var childNode in operatorAndNode.Children)
                    ts.AddRange(childNode.FindAll<T>());
            var operatorOrNode = this as FilterExpressionOrOperatorNode;
            if (operatorOrNode != null)
                foreach (var childNode in operatorOrNode.Children)
            return ts;
