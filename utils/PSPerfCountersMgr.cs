    /// Powershell Performance Counters Manager class shall provide a mechanism
    /// for components using SYstem.Management.Automation assembly to register
    /// performance counters with Performance Counters subsystem.
    public class PSPerfCountersMgr
        private static PSPerfCountersMgr s_PSPerfCountersMgrInstance;
        private ConcurrentDictionary<Guid, CounterSetInstanceBase> _CounterSetIdToInstanceMapping;
        private ConcurrentDictionary<string, Guid> _CounterSetNameToIdMapping;
        private PSPerfCountersMgr()
            _CounterSetIdToInstanceMapping = new ConcurrentDictionary<Guid, CounterSetInstanceBase>();
            _CounterSetNameToIdMapping = new ConcurrentDictionary<string, Guid>();
        /// Destructor which will trigger the cleanup of internal data structures and
        /// disposal of counter set instances.
        ~PSPerfCountersMgr()
            RemoveAllCounterSets();
        /// Getter method to retrieve the singleton instance of the PSPerfCountersMgr.
        public static PSPerfCountersMgr Instance
            get { return s_PSPerfCountersMgrInstance ?? (s_PSPerfCountersMgrInstance = new PSPerfCountersMgr()); }
        /// Helper method to generate an instance name for a counter set.
        public string GetCounterSetInstanceName()
            string pid = string.Create(CultureInfo.InvariantCulture, $"{currentProcess.Id}");
        /// Method to determine whether the counter set given by 'counterSetName' is
        /// registered with the system. If true, then counterSetId is populated.
        public bool IsCounterSetRegistered(string counterSetName, out Guid counterSetId)
            counterSetId = new Guid();
            if (counterSetName == null)
                ArgumentNullException argNullException = new ArgumentNullException(nameof(counterSetName));
            return _CounterSetNameToIdMapping.TryGetValue(counterSetName, out counterSetId);
        /// Method to determine whether the counter set given by 'counterSetId' is
        /// registered with the system. If true, then CounterSetInstance is populated.
        public bool IsCounterSetRegistered(Guid counterSetId, out CounterSetInstanceBase counterSetInst)
            return _CounterSetIdToInstanceMapping.TryGetValue(counterSetId, out counterSetInst);
        /// Method to register a counter set with the Performance Counters Manager.
        public bool AddCounterSetInstance(CounterSetRegistrarBase counterSetRegistrarInstance)
            if (counterSetRegistrarInstance == null)
                ArgumentNullException argNullException = new ArgumentNullException("counterSetRegistrarInstance");
            Guid counterSetId = counterSetRegistrarInstance.CounterSetId;
            string counterSetName = counterSetRegistrarInstance.CounterSetName;
            CounterSetInstanceBase counterSetInst = null;
            if (this.IsCounterSetRegistered(counterSetId, out counterSetInst))
                InvalidOperationException invalidOperationException = new InvalidOperationException(
                    "A Counter Set Instance with id '{0}' is already registered",
                    counterSetId));
                if (!string.IsNullOrWhiteSpace(counterSetName))
                    Guid retrievedCounterSetId;
                    // verify that there doesn't exist another counter set with the same name
                    if (this.IsCounterSetRegistered(counterSetName, out retrievedCounterSetId))
                                "A Counter Set Instance with name '{0}' is already registered",
                                counterSetName));
                    _CounterSetNameToIdMapping.TryAdd(counterSetName, counterSetId);
                _CounterSetIdToInstanceMapping.TryAdd(
                    counterSetId,
                    counterSetRegistrarInstance.CounterSetInstance);
            catch (OverflowException overflowException)
                _tracer.TraceException(overflowException);
        /// If IsNumerator is true, then updates the numerator component
        /// of target counter 'counterId' in Counter Set 'counterSetId'
        /// by 'stepAmount'.
        public bool UpdateCounterByValue(
            long stepAmount = 1,
            bool isNumerator = true)
                return counterSetInst.UpdateCounterByValue(counterId, stepAmount, isNumerator);
                        "No Counter Set Instance with id '{0}' is registered",
        /// of target counter 'counterName' in Counter Set 'counterSetId'
                return counterSetInst.UpdateCounterByValue(counterName, stepAmount, isNumerator);
        /// of target counter 'counterId' in Counter Set 'counterSetName'
            string counterSetName,
            Guid counterSetId;
            if (this.IsCounterSetRegistered(counterSetName, out counterSetId))
                CounterSetInstanceBase counterSetInst = _CounterSetIdToInstanceMapping[counterSetId];
        /// of target counter 'counterName' in Counter Set 'counterSetName'
                        "No Counter Set Instance with name {0} is registered",
        /// If IsNumerator is true, then sets the numerator component
        /// to 'counterValue'.
        /// Otherwise, updates the denominator component to 'counterValue'.
        public bool SetCounterValue(
            long counterValue = 1,
                return counterSetInst.SetCounterValue(counterId, counterValue, isNumerator);
                return counterSetInst.SetCounterValue(counterName, counterValue, isNumerator);
                    "No Counter Set Instance with name '{0}' is registered",
        /// NOTE: This method is provided solely for testing purposes.
        internal void RemoveAllCounterSets()
            ICollection<Guid> counterSetIdKeys = _CounterSetIdToInstanceMapping.Keys;
            foreach (Guid counterSetId in counterSetIdKeys)
                CounterSetInstanceBase currentCounterSetInstance = _CounterSetIdToInstanceMapping[counterSetId];
                currentCounterSetInstance.Dispose();
            _CounterSetIdToInstanceMapping.Clear();
            _CounterSetNameToIdMapping.Clear();
