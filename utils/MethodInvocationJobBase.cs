    internal abstract class MethodInvocationJobBase<T> : CimChildJobBase<T>
        internal MethodInvocationJobBase(CimJobContext jobContext, bool passThru, string methodSubject, MethodInvocationInfo methodInvocationInfo)
                : base(jobContext)
            Dbg.Assert(methodInvocationInfo != null, "Caller should verify methodInvocationInfo != null");
            Dbg.Assert(methodSubject != null, "Caller should verify methodSubject != null");
            _passThru = passThru;
            MethodSubject = methodSubject;
            _methodInvocationInfo = methodInvocationInfo;
        private readonly bool _passThru;
        private readonly MethodInvocationInfo _methodInvocationInfo;
            get { return _methodInvocationInfo.MethodName; }
        private const string CustomOperationOptionPrefix = "cim:operationOption:";
        private IEnumerable<MethodParameter> GetMethodInputParametersCore(Func<MethodParameter, bool> filter)
            IEnumerable<MethodParameter> inputParameters = _methodInvocationInfo.Parameters.Where(filter);
            var result = new List<MethodParameter>();
            foreach (MethodParameter inputParameter in inputParameters)
                object cimValue = CimSensitiveValueConverter.ConvertFromDotNetToCim(inputParameter.Value);
                Type cimType = CimSensitiveValueConverter.GetCimType(inputParameter.ParameterType);
                CimValueConverter.AssertIntrinsicCimType(cimType);
                result.Add(new MethodParameter
                    Name = inputParameter.Name,
                    ParameterType = cimType,
                    Bindings = inputParameter.Bindings,
                    Value = cimValue,
                    IsValuePresent = inputParameter.IsValuePresent
        internal IEnumerable<MethodParameter> GetMethodInputParameters()
            var allMethodParameters = this.GetMethodInputParametersCore(static p => !p.Name.StartsWith(CustomOperationOptionPrefix, StringComparison.OrdinalIgnoreCase));
            var methodParametersWithInputValue = allMethodParameters.Where(static p => p.IsValuePresent);
            return methodParametersWithInputValue;
        internal IEnumerable<CimInstance> GetCimInstancesFromArguments()
            return _methodInvocationInfo.GetArgumentsOfType<CimInstance>();
            IDictionary<string, object> result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            IEnumerable<MethodParameter> customOptions = this
                .GetMethodInputParametersCore(static p => p.Name.StartsWith(CustomOperationOptionPrefix, StringComparison.OrdinalIgnoreCase));
            foreach (MethodParameter customOption in customOptions)
                if (customOption.Value == null)
                result.Add(customOption.Name.Substring(CustomOperationOptionPrefix.Length), customOption.Value);
            return CimCustomOptionsDictionary.Create(result);
        internal IEnumerable<MethodParameter> GetMethodOutputParameters()
            IEnumerable<MethodParameter> allParameters_plus_returnValue = _methodInvocationInfo.Parameters;
            if (_methodInvocationInfo.ReturnValue != null)
                allParameters_plus_returnValue = allParameters_plus_returnValue.Append(_methodInvocationInfo.ReturnValue);
            var outParameters = allParameters_plus_returnValue
                .Where(static p => ((p.Bindings & (MethodParameterBindings.Out | MethodParameterBindings.Error)) != 0));
            return outParameters;
        internal string MethodSubject { get; }
        internal bool ShouldProcess()
            if (!this.JobContext.CmdletInvocationContext.CmdletDefinitionContext.ClientSideShouldProcess)
            bool shouldProcess;
            if (!this.JobContext.SupportsShouldProcess)
                shouldProcess = true;
                string target = this.MethodSubject;
                string action = this.MethodName;
                CimResponseType cimResponseType = this.ShouldProcess(target, action);
                switch (cimResponseType)
                    case CimResponseType.Yes:
                    case CimResponseType.YesToAll:
                        shouldProcess = false;
            if (!shouldProcess)
                this.SetCompletedJobState(JobState.Completed, null);
            return shouldProcess;
        #region PassThru functionality
        internal abstract object PassThruObject { get; }
        internal bool IsPassThruObjectNeeded()
            return (_passThru) && (!this.DidUserSuppressTheOperation) && (!this.JobHadErrors);
                        if (this.IsPassThruObjectNeeded())
                            object passThruObject = this.PassThruObject;
                            if (passThruObject != null)
                                this.WriteObject(passThruObject);
        #region Job descriptions
                    CmdletizationResources.CimJob_MethodDescription,
                    this.MethodName);
                    CmdletizationResources.CimJob_SafeMethodDescription,
