    /// A struct that encapsulates the information pertaining to a given counter
    /// like name,type and id.
    public struct CounterInfo
        public CounterInfo(int id, CounterType type, string name)
        public CounterInfo(int id, CounterType type)
        /// Getter for Counter Name property.
        /// Getter for Counter Id property.
        /// Getter for Counter Type property.
        public CounterType Type { get; }
    /// An abstract class that forms the base class for any CounterSetRegistrar type.
    /// Any client that needs to register a new type of perf counter category with the
    /// PSPerfCountersMgr, should create an instance of CounterSetRegistrarBase's
    /// derived non-abstract type.
    /// The created instance is then passed to PSPerfCounterMgr's AddCounterSetInstance()
    public abstract class CounterSetRegistrarBase
        /// A reference to the encapsulated counter set instance.
        protected CounterSetInstanceBase _counterSetInstanceBase;
        /// Method that creates an instance of the CounterSetInstanceBase's derived type.
        /// This method is invoked by the PSPerfCountersMgr to retrieve the appropriate
        /// instance of CounterSet to register with its internal datastructure.
        protected abstract CounterSetInstanceBase CreateCounterSetInstance();
        /// Constructor that creates an instance of CounterSetRegistrarBase derived type
        /// based on Provider Id, counterSetId, counterSetInstanceType, a collection
        /// with counters information and an optional counterSetName.
        protected CounterSetRegistrarBase(
            Guid providerId,
            Guid counterSetId,
            CounterSetInstanceType counterSetInstType,
            CounterInfo[] counterInfoArray,
            string counterSetName = null)
            ProviderId = providerId;
            CounterSetId = counterSetId;
            CounterSetInstType = counterSetInstType;
            CounterSetName = counterSetName;
            if (counterInfoArray is null || counterInfoArray.Length == 0)
                throw new ArgumentNullException(nameof(counterInfoArray));
            CounterInfoArray = new CounterInfo[counterInfoArray.Length];
                CounterInfoArray[i] =
                    new CounterInfo(
                        counterInfoArray[i].Id,
                        counterInfoArray[i].Type,
                        counterInfoArray[i].Name
            this._counterSetInstanceBase = null;
            CounterSetRegistrarBase srcCounterSetRegistrarBase)
            ArgumentNullException.ThrowIfNull(srcCounterSetRegistrarBase);
            ProviderId = srcCounterSetRegistrarBase.ProviderId;
            CounterSetId = srcCounterSetRegistrarBase.CounterSetId;
            CounterSetInstType = srcCounterSetRegistrarBase.CounterSetInstType;
            CounterSetName = srcCounterSetRegistrarBase.CounterSetName;
            CounterInfo[] counterInfoArrayRef = srcCounterSetRegistrarBase.CounterInfoArray;
            CounterInfoArray = new CounterInfo[counterInfoArrayRef.Length];
            for (int i = 0; i < counterInfoArrayRef.Length; i++)
                        counterInfoArrayRef[i].Id,
                        counterInfoArrayRef[i].Type,
                        counterInfoArrayRef[i].Name);
        /// Getter method for ProviderId property.
        public Guid ProviderId { get; }
        /// Getter method for CounterSetId property.
        public Guid CounterSetId { get; }
        /// Getter method for CounterSetName property.
        public string CounterSetName { get; }
        /// Getter method for CounterSetInstanceType property.
        public CounterSetInstanceType CounterSetInstType { get; }
        /// Getter method for array of counters information property.
        public CounterInfo[] CounterInfoArray { get; }
        /// Getter method that returns an instance of the CounterSetInstanceBase's
        /// derived type.
        public CounterSetInstanceBase CounterSetInstance
            get { return _counterSetInstanceBase ?? (_counterSetInstanceBase = CreateCounterSetInstance()); }
        /// Method that disposes the referenced instance of the CounterSetInstanceBase's derived type.
        /// This method is invoked by the PSPerfCountersMgr to dispose the appropriate
        /// instance of CounterSet from its internal datastructure as part of PSPerfCountersMgr
        /// cleanup procedure.
        public abstract void DisposeCounterSetInstance();
    /// PSCounterSetRegistrar implements the abstract methods of CounterSetRegistrarBase.
    /// PSPerfCountersMgr, should create an instance of PSCounterSetRegistrar.
    public class PSCounterSetRegistrar : CounterSetRegistrarBase
        /// Constructor that creates an instance of PSCounterSetRegistrar.
        public PSCounterSetRegistrar(
            : base(providerId, counterSetId, counterSetInstType, counterInfoArray, counterSetName)
        /// Copy Constructor.
            PSCounterSetRegistrar srcPSCounterSetRegistrar)
            : base(srcPSCounterSetRegistrar)
            ArgumentNullException.ThrowIfNull(srcPSCounterSetRegistrar);
        #region CounterSetRegistrarBase Overrides
        protected override CounterSetInstanceBase CreateCounterSetInstance()
            return new PSCounterSetInstance(this);
        public override void DisposeCounterSetInstance()
            base._counterSetInstanceBase.Dispose();
