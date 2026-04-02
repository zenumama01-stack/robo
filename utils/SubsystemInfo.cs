    /// Class used to represent the metadata and state of a subsystem.
    public abstract class SubsystemInfo
        #region "Metadata of a Subsystem (public)"
        /// Gets the kind of a concrete subsystem.
        public SubsystemKind Kind { get; }
        /// Gets the type of a concrete subsystem.
        public Type SubsystemType { get; }
        /// Gets a value indicating whether the subsystem allows to unregister an implementation.
        public bool AllowUnregistration { get; private set; }
        /// Gets a value indicating whether the subsystem allows to have multiple implementations registered.
        public bool AllowMultipleRegistration { get; private set; }
        /// Gets the names of the required cmdlets that have to be implemented by the subsystem implementation.
        public ReadOnlyCollection<string> RequiredCmdlets { get; private set; }
        /// Gets the names of the required functions that have to be implemented by the subsystem implementation.
        public ReadOnlyCollection<string> RequiredFunctions { get; private set; }
        // /// <summary>
        // /// A subsystem may depend on or more other subsystems.
        // /// Maybe add a 'DependsOn' member?
        // /// This can be validated when registering a subsystem implementation,
        // /// to make sure its prerequisites have already been registered.
        // /// </summary>
        // public ReadOnlyCollection<SubsystemKind> DependsOn { get; private set; }
        #region "State of a Subsystem (public)"
        /// Indicate whether there is any implementation registered to the subsystem.
        public bool IsRegistered => _cachedImplInfos.Count > 0;
        /// Get the information about the registered implementations.
        public ReadOnlyCollection<ImplementationInfo> Implementations => _cachedImplInfos;
        #region "private/internal instance members"
        private protected readonly object _syncObj;
        private protected ReadOnlyCollection<ImplementationInfo> _cachedImplInfos;
        private protected SubsystemInfo(SubsystemKind kind, Type subsystemType)
            _syncObj = new object();
            _cachedImplInfos = Utils.EmptyReadOnlyCollection<ImplementationInfo>();
            SubsystemType = subsystemType;
            AllowUnregistration = false;
            AllowMultipleRegistration = false;
            RequiredCmdlets = Utils.EmptyReadOnlyCollection<string>();
            RequiredFunctions = Utils.EmptyReadOnlyCollection<string>();
        private protected abstract void AddImplementation(ISubsystem rawImpl);
        private protected abstract ISubsystem RemoveImplementation(Guid id);
        internal void RegisterImplementation(ISubsystem impl)
            AddImplementation(impl);
            ApplicationInsightsTelemetry.SendUseTelemetry(ApplicationInsightsTelemetry.s_subsystemRegistration, impl.Name);
        internal ISubsystem UnregisterImplementation(Guid id)
            return RemoveImplementation(id);
        #region "Static factory overloads"
        internal static SubsystemInfo Create<TConcreteSubsystem>(SubsystemKind kind)
            where TConcreteSubsystem : class, ISubsystem
            return new SubsystemInfoImpl<TConcreteSubsystem>(kind);
        internal static SubsystemInfo Create<TConcreteSubsystem>(
            SubsystemKind kind,
            bool allowUnregistration,
            bool allowMultipleRegistration) where TConcreteSubsystem : class, ISubsystem
            return new SubsystemInfoImpl<TConcreteSubsystem>(kind)
                AllowUnregistration = allowUnregistration,
                AllowMultipleRegistration = allowMultipleRegistration,
            bool allowMultipleRegistration,
            ReadOnlyCollection<string> requiredCmdlets,
            ReadOnlyCollection<string> requiredFunctions) where TConcreteSubsystem : class, ISubsystem
            if (allowMultipleRegistration &&
                (requiredCmdlets.Count > 0 || requiredFunctions.Count > 0))
                        SubsystemStrings.InvalidSubsystemInfo,
                        kind.ToString()));
                RequiredCmdlets = requiredCmdlets,
                RequiredFunctions = requiredFunctions,
        #region "ImplementationInfo"
        /// Information about an implementation of a subsystem.
        public class ImplementationInfo
            internal ImplementationInfo(SubsystemKind kind, ISubsystem implementation)
                Id = implementation.Id;
                Name = implementation.Name;
                Description = implementation.Description;
                ImplementationType = implementation.GetType();
            public Guid Id { get; }
            /// Gets the kind of subsystem.
            /// Gets the implementation type.
            public Type ImplementationType { get; }
    internal sealed class SubsystemInfoImpl<TConcreteSubsystem> : SubsystemInfo
        private ReadOnlyCollection<TConcreteSubsystem> _registeredImpls;
        internal SubsystemInfoImpl(SubsystemKind kind)
            : base(kind, typeof(TConcreteSubsystem))
            _registeredImpls = Utils.EmptyReadOnlyCollection<TConcreteSubsystem>();
        /// The 'add' and 'remove' operations are implemented in a way to optimize the 'reading' operation,
        /// so that reading is lock-free and allocation-free, at the cost of O(n) copy in 'add' and 'remove'
        /// ('n' is the number of registered implementations).
        /// In the subsystem scenario, registration operations will be minimum, and in most cases, the registered
        /// implementation will never be unregistered, so optimization for reading is more important.
        /// <param name="rawImpl">The subsystem implementation to be added.</param>
        private protected override void AddImplementation(ISubsystem rawImpl)
            lock (_syncObj)
                var impl = (TConcreteSubsystem)rawImpl;
                if (_registeredImpls.Count == 0)
                    _registeredImpls = new ReadOnlyCollection<TConcreteSubsystem>(new[] { impl });
                    _cachedImplInfos = new ReadOnlyCollection<ImplementationInfo>(new[] { new ImplementationInfo(Kind, impl) });
                if (!AllowMultipleRegistration)
                            SubsystemStrings.MultipleRegistrationNotAllowed,
                            Kind.ToString()));
                foreach (TConcreteSubsystem item in _registeredImpls)
                    if (item.Id == impl.Id)
                                SubsystemStrings.ImplementationAlreadyRegistered,
                                impl.Id,
                int newCapacity = _registeredImpls.Count + 1;
                var implList = new List<TConcreteSubsystem>(newCapacity);
                implList.AddRange(_registeredImpls);
                implList.Add(impl);
                var implInfo = new List<ImplementationInfo>(newCapacity);
                implInfo.AddRange(_cachedImplInfos);
                implInfo.Add(new ImplementationInfo(Kind, impl));
                _registeredImpls = new ReadOnlyCollection<TConcreteSubsystem>(implList);
                _cachedImplInfos = new ReadOnlyCollection<ImplementationInfo>(implInfo);
        /// <param name="id">The id of the subsystem implementation to be removed.</param>
        /// <returns>The subsystem implementation that was removed.</returns>
        private protected override ISubsystem RemoveImplementation(Guid id)
            if (!AllowUnregistration)
                        SubsystemStrings.UnregistrationNotAllowed,
                            SubsystemStrings.NoImplementationRegistered,
                for (int i = 0; i < _registeredImpls.Count; i++)
                    if (_registeredImpls[i].Id == id)
                            SubsystemStrings.ImplementationNotFound,
                            id.ToString()));
                ISubsystem target = _registeredImpls[index];
                if (_registeredImpls.Count == 1)
                    int newCapacity = _registeredImpls.Count - 1;
                        if (index == i)
                        implList.Add(_registeredImpls[i]);
                        implInfo.Add(_cachedImplInfos[i]);
        internal TConcreteSubsystem? GetImplementation()
            var localRef = _registeredImpls;
            return localRef.Count > 0 ? localRef[localRef.Count - 1] : null;
        internal ReadOnlyCollection<TConcreteSubsystem> GetAllImplementations()
            return _registeredImpls;
