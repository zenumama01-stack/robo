    /// The IEvaluate interface provides the most basic
    /// support for the evaluation of an item against
    /// criteria defined in a derived class.
    public interface IEvaluate
        /// Gets a values indicating whether the supplied item has meet the
        /// criteria rule specified by the rule.
        /// <param name="item">
        /// The item to evaluate.
        /// Returns true if the item meets the criteria. False otherwise.
        bool Evaluate(object item);
