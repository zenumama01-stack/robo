    /// Describes whether to report errors when a given filter doesnt match any objects.
    public enum BehaviorOnNoMatch
        /// Default behavior is to be consistent with the built-in cmdlets:
        /// - When a wildcard is specified, then no errors are reported (i.e. Get-Process -Name noSuchProcess*)
        /// - When no wildcard is specified, then errors are reported (i.e. Get-Process -Name noSuchProcess)
        /// Note that the following conventions are adopted:
        /// - Min/max queries
        ///   (<see cref="QueryBuilder.FilterByMinPropertyValue(string,object,BehaviorOnNoMatch)"/> and
        ///    <see cref="QueryBuilder.FilterByMaxPropertyValue(string,object,BehaviorOnNoMatch)"/>)
        ///   are treated as wildcards
        /// - Exclusions
        ///   (<see cref="QueryBuilder.ExcludeByProperty(string,System.Collections.IEnumerable,bool,BehaviorOnNoMatch)"/>)
        /// - Associations
        ///   (<see cref="QueryBuilder.FilterByAssociatedInstance(object,string,string,string,BehaviorOnNoMatch)"/>)
        ///   are treated as not a wildcard.
        /// <c>ReportErrors</c> forces reporting of errors that in other circumstances would be reported if no objects matched the filters.
        ReportErrors,
        /// <c>SilentlyContinue</c> suppresses errors that in other circumstances would be reported if no objects matched the filters.
        SilentlyContinue,
    /// QueryBuilder supports building of object model queries in an object-model-agnostic way.
    public abstract class QueryBuilder
        /// <see langword="true"/> if <paramref name="allowedPropertyValues"/> should be treated as a <see cref="string"/> containing a wildcard pattern;
        public virtual void FilterByProperty(string propertyName, IEnumerable allowedPropertyValues, bool wildcardsEnabled, BehaviorOnNoMatch behaviorOnNoMatch)
        public virtual void ExcludeByProperty(string propertyName, IEnumerable excludedPropertyValues, bool wildcardsEnabled, BehaviorOnNoMatch behaviorOnNoMatch)
        public virtual void FilterByMinPropertyValue(string propertyName, object minPropertyValue, BehaviorOnNoMatch behaviorOnNoMatch)
        public virtual void FilterByMaxPropertyValue(string propertyName, object maxPropertyValue, BehaviorOnNoMatch behaviorOnNoMatch)
        public virtual void FilterByAssociatedInstance(object associatedInstance, string associationName, string sourceRole, string resultRole, BehaviorOnNoMatch behaviorOnNoMatch)
        public virtual void AddQueryOption(string optionName, object optionValue)
