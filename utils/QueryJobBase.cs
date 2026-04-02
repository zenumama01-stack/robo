    /// Base job for queries.
    internal abstract class QueryJobBase : CimChildJobBase<CimInstance>
        private readonly CimQuery _cimQuery;
        internal QueryJobBase(CimJobContext jobContext, CimQuery cimQuery)
            Dbg.Assert(cimQuery != null, "Caller should verify cimQuery != null");
            _cimQuery = cimQuery;
                        Dbg.Assert(item != null, "When OnNext is called from our IObservable, item parameter should always be != null");
                        if (!_cimQuery.IsMatchingResult(item))
                        this.WriteObject(item);
                        foreach (ClientSideQuery.NotFoundError notFoundError in _cimQuery.GenerateNotFoundErrors())
                            string errorId = "CmdletizationQuery_NotFound";
                            if (!string.IsNullOrEmpty(notFoundError.PropertyName))
                                errorId = errorId + "_" + notFoundError.PropertyName;
                            CimJobException cimJobException = CimJobException.CreateWithFullControl(
                                notFoundError.ErrorMessageGenerator(this.Description, this.JobContext.ClassName),
                                errorId,
                                ErrorCategory.ObjectNotFound);
                                cimJobException.ErrorRecord.SetTargetObject(notFoundError.PropertyValue);
                            this.WriteError(cimJobException.ErrorRecord);
            return CimCustomOptionsDictionary.Create(_cimQuery.queryOptions);
