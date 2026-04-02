    internal sealed class QueryInstancesJob : QueryJobBase
        private readonly string _wqlQuery;
        private readonly bool _useEnumerateInstances;
        internal QueryInstancesJob(CimJobContext jobContext, CimQuery cimQuery, string wqlCondition)
            Dbg.Assert(wqlCondition != null, "Caller should verify that wqlCondition is not null");
            var wqlQueryBuilder = new StringBuilder();
            wqlQueryBuilder.Append("SELECT * FROM ");
            wqlQueryBuilder.Append(this.JobContext.ClassName);
            wqlQueryBuilder.Append(' ');
            wqlQueryBuilder.Append(wqlCondition);
            _wqlQuery = wqlQueryBuilder.ToString();
            if (string.IsNullOrWhiteSpace(wqlCondition))
                _useEnumerateInstances = true;
                if (jobContext.CmdletInvocationContext.CmdletDefinitionContext.UseEnumerateInstancesInsteadOfWql)
            IObservable<CimInstance> observable;
            if (_useEnumerateInstances)
                observable = this.JobContext.Session.EnumerateInstancesAsync(
                observable = this.JobContext.Session.QueryInstancesAsync(
                    "WQL",
                    _wqlQuery,
                return this.FailSafeDescription;
                    CmdletizationResources.CimJob_SafeQueryDescription,
                    _wqlQuery);
