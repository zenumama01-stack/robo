using Microsoft.PowerShell.Commands;
    /// Enables the user to subscribe to indications using Filter Expression or
    /// Query Expression.
    /// -SourceIdentifier is a name given to the subscription
    /// The Cmdlet should return a PS EventSubscription object that can be used to
    /// cancel the subscription
    /// Should we have the second parameter set with a -Query?
    [Alias("rcie")]
    [Cmdlet(VerbsLifecycle.Register, "CimIndicationEvent", DefaultParameterSetName = CimBaseCommand.ClassNameComputerSet, HelpUri = "https://go.microsoft.com/fwlink/?LinkId=227960")]
    public class RegisterCimIndicationCommand : ObjectEventRegistrationBase
        /// Specifies the NameSpace under which to look for the specified class name.
        /// Default value is root\cimv2
        /// Specifies the Class Name to register the indication on.
                this.SetParameter(value, nameClassName);
        /// The Query Expression to pass.
            ParameterSetName = CimBaseCommand.QueryExpressionSessionSet)]
            ParameterSetName = CimBaseCommand.QueryExpressionComputerSet)]
                this.SetParameter(value, nameQuery);
        [Parameter(ParameterSetName = CimBaseCommand.QueryExpressionComputerSet)]
        [Parameter(ParameterSetName = CimBaseCommand.QueryExpressionSessionSet)]
                this.SetParameter(value, nameQueryDialect);
        [Alias(CimBaseCommand.AliasOT)]
        public CimSession CimSession
                this.SetParameter(value, nameCimSession);
        [Alias(CimBaseCommand.AliasCN, CimBaseCommand.AliasServerName)]
        public string ComputerName
                this.SetParameter(value, nameComputerName);
        private string computername;
        /// Returns the object that generates events to be monitored.
        protected override object GetSourceObject()
            CimIndicationWatcher watcher = null;
            string parameterSetName = null;
                parameterSetName = this.parameterBinder.GetParameterSet();
            string tempQueryExpression = string.Empty;
            switch (parameterSetName)
                case CimBaseCommand.QueryExpressionSessionSet:
                case CimBaseCommand.QueryExpressionComputerSet:
                    tempQueryExpression = this.Query;
                    tempQueryExpression = string.Create(CultureInfo.CurrentCulture, $"Select * from {this.ClassName}");
                        watcher = new CimIndicationWatcher(this.CimSession, this.Namespace, this.QueryDialect, tempQueryExpression, this.OperationTimeoutSec);
                        watcher = new CimIndicationWatcher(this.ComputerName, this.Namespace, this.QueryDialect, tempQueryExpression, this.OperationTimeoutSec);
            watcher?.SetCmdlet(this);
            return watcher;
        /// Returns the event name to be monitored on the input object.
        protected override string GetSourceObjectEventName()
            return "CimIndicationArrived";
            base.EndProcessing();
            // Register for the "Unsubscribed" event so that we can stop the
            // Cimindication event watcher.
            PSEventSubscriber newSubscriber = NewSubscriber;
            if (newSubscriber != null)
                DebugHelper.WriteLog("RegisterCimIndicationCommand::EndProcessing subscribe to Unsubscribed event", 4);
                newSubscriber.Unsubscribed += newSubscriber_Unsubscribed;
        /// Handler to handle unsubscribe event
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void newSubscriber_Unsubscribed(
            object sender, PSEventUnsubscribedEventArgs e)
            CimIndicationWatcher watcher = sender as CimIndicationWatcher;
            watcher?.Stop();
        private readonly ParameterBinder parameterBinder = new(
            parameters, parameterSets);
        /// Set the parameter.
        private void SetParameter(object value, string parameterName)
            this.parameterBinder.SetParameter(parameterName, true);
                                    new ParameterDefinitionEntry(CimBaseCommand.QueryExpressionSessionSet, true),
                                    new ParameterDefinitionEntry(CimBaseCommand.QueryExpressionComputerSet, true),
                                    new ParameterDefinitionEntry(CimBaseCommand.QueryExpressionSessionSet, false),
                                    new ParameterDefinitionEntry(CimBaseCommand.QueryExpressionComputerSet, false),
            {   CimBaseCommand.QueryExpressionSessionSet, new ParameterSetEntry(2)     },
            {   CimBaseCommand.QueryExpressionComputerSet, new ParameterSetEntry(1)     },
