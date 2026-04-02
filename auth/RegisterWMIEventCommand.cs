    /// Registers for an event on an object.
    [Cmdlet(VerbsLifecycle.Register, "WmiEvent", DefaultParameterSetName = "class",
        HelpUri = "https://go.microsoft.com/fwlink/?LinkID=135245", RemotingCapability = RemotingCapability.OwnedByCommand)]
    public class RegisterWmiEventCommand : ObjectEventRegistrationBase
        /// The WMI namespace to use.
        [Alias("NS")]
        public string Namespace { get; set; } = "root\\cimv2";
        /// The credential to use.
        /// The ComputerName in which to query.
        public string Class { get; set; } = null;
        public string Query { get; set; } = null;
        /// Timeout in milliseconds.
        [Alias("TimeoutMSec")]
        public Int64 Timeout
                return _timeOut;
                _timeOut = value;
        private Int64 _timeOut = 0;
        #endregion parameters
        private string BuildEventQuery(string objectName)
            StringBuilder returnValue = new StringBuilder("select * from ");
            returnValue.Append(objectName);
        private string GetScopeString(string computer, string namespaceParameter)
            StringBuilder returnValue = new StringBuilder("\\\\");
            returnValue.Append('\\');
        #endregion helper functions
            string wmiQuery = this.Query;
            if (this.Class != null)
                // Validate class format
                for (int i = 0; i < this.Class.Length; i++)
                    if (char.IsLetterOrDigit(this.Class[i]) || this.Class[i].Equals('_'))
                    errorRecord.ErrorDetails = new ErrorDetails(this, "WmiResources", "WmiInvalidClass");
                wmiQuery = BuildEventQuery(this.Class);
            ConnectionOptions conOptions = new ConnectionOptions();
            if (this.Credential != null)
                System.Net.NetworkCredential cred = this.Credential.GetNetworkCredential();
                if (string.IsNullOrEmpty(cred.Domain))
                    conOptions.Username = cred.UserName;
                    conOptions.Username = cred.Domain + "\\" + cred.UserName;
                conOptions.Password = cred.Password;
            ManagementScope scope = new ManagementScope(GetScopeString(ComputerName, this.Namespace), conOptions);
            EventWatcherOptions evtOptions = new EventWatcherOptions();
            if (_timeoutSpecified)
                evtOptions.Timeout = new TimeSpan(_timeOut * 10000);
            ManagementEventWatcher watcher = new ManagementEventWatcher(scope, new EventQuery(wmiQuery), evtOptions);
            return "EventArrived";
        /// Processes the event subscriber after the base class has registered.
            // event watcher.
                newSubscriber.Unsubscribed += new PSEventUnsubscribedEventHandler(newSubscriber_Unsubscribed);
        private void newSubscriber_Unsubscribed(object sender, PSEventUnsubscribedEventArgs e)
            ManagementEventWatcher watcher = sender as ManagementEventWatcher;
            if (watcher != null)
                watcher.Stop();
