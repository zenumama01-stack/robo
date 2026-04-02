using System.Diagnostics.PerformanceData;
namespace System.Management.Automation.PerformanceData
    /// An abstract class that forms the base class for any Counter Set type.
    /// A Counter Set Instance is required to register a given performance counter category
    /// with PSPerfCountersMgr.
    public abstract class CounterSetInstanceBase : IDisposable
        /// An instance of counterSetRegistrarBase type encapsulates all the information
        /// about a counter set and its associated counters.
        protected CounterSetRegistrarBase _counterSetRegistrarBase;
        // NOTE: Check whether the following dictionaries need to be concurrent
        // because there would be only 1 thread creating the instance,
        // and that instance would then be shared by multiple threads for data access.
        // Those threads won't modify/manipulate the dictionary, but they would only access it.
        /// Dictionary mapping counter name to id.
        protected ConcurrentDictionary<string, int> _counterNameToIdMapping;
        /// Dictionary mapping counter id to counter type.
        protected ConcurrentDictionary<int, CounterType> _counterIdToTypeMapping;
        protected CounterSetInstanceBase(CounterSetRegistrarBase counterSetRegistrarInst)
            this._counterSetRegistrarBase = counterSetRegistrarInst;
            _counterNameToIdMapping = new ConcurrentDictionary<string, int>();
            _counterIdToTypeMapping = new ConcurrentDictionary<int, CounterType>();
            CounterInfo[] counterInfoArray = this._counterSetRegistrarBase.CounterInfoArray;
            for (int i = 0; i < counterInfoArray.Length; i++)
                this._counterIdToTypeMapping.TryAdd(counterInfoArray[i].Id, counterInfoArray[i].Type);
                if (!string.IsNullOrWhiteSpace(counterInfoArray[i].Name))
                    this._counterNameToIdMapping.TryAdd(counterInfoArray[i].Name, counterInfoArray[i].Id);
        /// Method that retrieves the target counter id.
        /// NOTE: If isNumerator is true, then input counter id is returned.
        /// But, if isNumerator is false, then a check is made on the input
        /// counter's type to ensure that denominator is indeed value for such a counter.
        protected bool RetrieveTargetCounterIdIfValid(int counterId, bool isNumerator, out int targetCounterId)
            targetCounterId = counterId;
            if (isNumerator == false)
                bool isDenominatorValid = false;
                CounterType counterType = this._counterIdToTypeMapping[counterId];
                switch (counterType)
                    case CounterType.MultiTimerPercentageActive:
                    case CounterType.MultiTimerPercentageActive100Ns:
                    case CounterType.MultiTimerPercentageNotActive:
                    case CounterType.MultiTimerPercentageNotActive100Ns:
                    case CounterType.RawFraction32:
                    case CounterType.RawFraction64:
                    case CounterType.SampleFraction:
                    case CounterType.AverageCount64:
                    case CounterType.AverageTimer32:
                        isDenominatorValid = true;
                if (isDenominatorValid == false)
                    InvalidOperationException invalidOperationException =
                            "Denominator for update not valid for the given counter id {0}",
                            counterId));
                    _tracer.TraceException(invalidOperationException);
                targetCounterId = counterId + 1;
        /// If isNumerator is true, then updates the numerator component
        /// of target counter 'counterId' by a value given by 'stepAmount'.
        /// Otherwise, updates the denominator component by 'stepAmount'.
        public abstract bool UpdateCounterByValue(
            int counterId,
            long stepAmount,
            bool isNumerator);
        /// of target counter 'counterName' by a value given by 'stepAmount'.
            string counterName,
        /// If isNumerator is true, then sets the numerator component of target
        /// counter 'counterId' to 'counterValue'.
        /// Otherwise, sets the denominator component to 'counterValue'.
        public abstract bool SetCounterValue(
            long counterValue,
        /// Counter 'counterName' to 'counterValue'.
        /// This method retrieves the counter value associated with counter 'counterId'
        /// based on isNumerator parameter.
        public abstract bool GetCounterValue(int counterId, bool isNumerator, out long counterValue);
        /// This method retrieves the counter value associated with counter 'counterName'
        public abstract bool GetCounterValue(string counterName, bool isNumerator, out long counterValue);
        /// An abstract method that will be implemented by the derived type
        /// so as to dispose the appropriate counter set instance.
        public abstract void Dispose();
        /// Resets the target counter 'counterId' to 0. If the given
        /// counter has both numerator and denominator components, then
        /// they both are set to 0.
        public bool ResetCounter(
            int counterId)
            this.SetCounterValue(counterId, 0, true);
            this.SetCounterValue(counterId, 0, false);
        /// Resets the target counter 'counterName' to 0. If the given
        public void ResetCounter(string counterName)
            this.SetCounterValue(counterName, 0, true);
            this.SetCounterValue(counterName, 0, false);
    /// PSCounterSetInstance is a thin wrapper
    /// on System.Diagnostics.PerformanceData.CounterSetInstance.
    public class PSCounterSetInstance : CounterSetInstanceBase
        private bool _Disposed;
        private CounterSet _CounterSet;
        private CounterSetInstance _CounterSetInstance;
        private void CreateCounterSetInstance()
            _CounterSet =
                new CounterSet(
                    base._counterSetRegistrarBase.ProviderId,
                    base._counterSetRegistrarBase.CounterSetId,
                    base._counterSetRegistrarBase.CounterSetInstType);
            // Add the counters to the counter set definition.
            foreach (CounterInfo counterInfo in base._counterSetRegistrarBase.CounterInfoArray)
                if (counterInfo.Name == null)
                    _CounterSet.AddCounter(counterInfo.Id, counterInfo.Type);
                    _CounterSet.AddCounter(counterInfo.Id, counterInfo.Type, counterInfo.Name);
            string instanceName = PSPerfCountersMgr.Instance.GetCounterSetInstanceName();
            // Create an instance of the counter set (contains the counter data).
            _CounterSetInstance = _CounterSet.CreateCounterSetInstance(instanceName);
        private void UpdateCounterByValue(CounterData TargetCounterData, long stepAmount)
            Debug.Assert(TargetCounterData != null);
            if (stepAmount == -1)
                TargetCounterData.Decrement();
            else if (stepAmount == 1)
                TargetCounterData.Increment();
                TargetCounterData.IncrementBy(stepAmount);
        /// Constructor for creating an instance of PSCounterSetInstance.
        public PSCounterSetInstance(CounterSetRegistrarBase counterSetRegBaseObj)
            : base(counterSetRegBaseObj)
            CreateCounterSetInstance();
        #region Destructor
        /// This destructor will run only if the Dispose method
        /// does not get called.
        /// It gives the base class opportunity to finalize.
        ~PSCounterSetInstance()
            if (!_Disposed)
                    _CounterSetInstance.Dispose();
                    _CounterSet.Dispose();
                _Disposed = true;
        /// Dispose Method implementation for IDisposable interface.
        #region CounterSetInstanceBase Overrides
        public override bool UpdateCounterByValue(int counterId, long stepAmount, bool isNumerator)
            if (_Disposed)
                ObjectDisposedException objectDisposedException =
                    new ObjectDisposedException("PSCounterSetInstance");
                _tracer.TraceException(objectDisposedException);
            int targetCounterId;
            if (base.RetrieveTargetCounterIdIfValid(counterId, isNumerator, out targetCounterId))
                CounterData targetCounterData = _CounterSetInstance.Counters[targetCounterId];
                if (targetCounterData != null)
                    this.UpdateCounterByValue(targetCounterData, stepAmount);
                        "Lookup for counter corresponding to counter id {0} failed",
        public override bool UpdateCounterByValue(string counterName, long stepAmount, bool isNumerator)
            // retrieve counter id associated with the counter name
            if (counterName == null)
                ArgumentNullException argNullException = new ArgumentNullException(nameof(counterName));
                _tracer.TraceException(argNullException);
                int targetCounterId = this._counterNameToIdMapping[counterName];
                return this.UpdateCounterByValue(targetCounterId, stepAmount, isNumerator);
            catch (KeyNotFoundException)
                    "Lookup for counter corresponding to counter name {0} failed",
                    counterName));
        /// If isNumerator is true, then sets the numerator component
        /// of target counter 'counterId' to 'counterValue'.
        public override bool SetCounterValue(int counterId, long counterValue, bool isNumerator)
                    targetCounterData.Value = counterValue;
        /// of target counter 'counterName' by a value given by 'counterValue'.
        public override bool SetCounterValue(string counterName, long counterValue, bool isNumerator)
                return this.SetCounterValue(targetCounterId, counterValue, isNumerator);
        public override bool GetCounterValue(int counterId, bool isNumerator, out long counterValue)
            counterValue = -1;
                    counterValue = targetCounterData.Value;
        public override bool GetCounterValue(string counterName, bool isNumerator, out long counterValue)
                return this.GetCounterValue(targetCounterId, isNumerator, out counterValue);
