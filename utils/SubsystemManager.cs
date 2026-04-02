using System.Management.Automation.Subsystem.Prediction;
    /// Class used to manage subsystems.
    public static class SubsystemManager
        private static readonly ReadOnlyCollection<SubsystemInfo> s_subsystems;
        private static readonly ReadOnlyDictionary<Type, SubsystemInfo> s_subSystemTypeMap;
        private static readonly ReadOnlyDictionary<SubsystemKind, SubsystemInfo> s_subSystemKindMap;
        static SubsystemManager()
            var subsystems = new SubsystemInfo[]
                SubsystemInfo.Create<ICommandPredictor>(
                    SubsystemKind.CommandPredictor,
                    allowUnregistration: true,
                    allowMultipleRegistration: true),
                SubsystemInfo.Create<ICrossPlatformDsc>(
                    SubsystemKind.CrossPlatformDsc,
                    allowMultipleRegistration: false),
                SubsystemInfo.Create<IFeedbackProvider>(
                    SubsystemKind.FeedbackProvider,
            var subSystemTypeMap = new Dictionary<Type, SubsystemInfo>(subsystems.Length);
            var subSystemKindMap = new Dictionary<SubsystemKind, SubsystemInfo>(subsystems.Length);
            foreach (var subsystem in subsystems)
                subSystemTypeMap.Add(subsystem.SubsystemType, subsystem);
                subSystemKindMap.Add(subsystem.Kind, subsystem);
            s_subsystems = new ReadOnlyCollection<SubsystemInfo>(subsystems);
            s_subSystemTypeMap = new ReadOnlyDictionary<Type, SubsystemInfo>(subSystemTypeMap);
            s_subSystemKindMap = new ReadOnlyDictionary<SubsystemKind, SubsystemInfo>(subSystemKindMap);
            // Register built-in suggestion providers.
            RegisterSubsystem(SubsystemKind.FeedbackProvider, new GeneralCommandErrorFeedback());
        #region internal - Retrieve subsystem proxy object
        /// Get the proxy object registered for a specific subsystem.
        /// Return null when the given subsystem is not registered.
        /// Design point:
        /// The implementation proxy object is not supposed to expose to users.
        /// Users shouldn't depend on a implementation proxy object directly, but instead should depend on PowerShell APIs.
        /// <para/>
        /// Example: if a user want to use prediction functionality, he/she should use the PowerShell prediction API instead of
        /// directly interacting with the implementation proxy object of `IPrediction`.
        /// <typeparam name="TConcreteSubsystem">The concrete subsystem base type.</typeparam>
        /// <returns>The most recently registered implementation object of the concrete subsystem.</returns>
        internal static TConcreteSubsystem? GetSubsystem<TConcreteSubsystem>()
            if (s_subSystemTypeMap.TryGetValue(typeof(TConcreteSubsystem), out SubsystemInfo? subsystemInfo))
                var subsystemInfoImpl = (SubsystemInfoImpl<TConcreteSubsystem>)subsystemInfo;
                return subsystemInfoImpl.GetImplementation();
                    SubsystemStrings.SubsystemTypeUnknown,
                    typeof(TConcreteSubsystem).FullName));
        /// Get all the proxy objects registered for a specific subsystem.
        /// Return an empty collection when the given subsystem is not registered.
        /// <returns>A readonly collection of all implementation objects registered for the concrete subsystem.</returns>
        internal static ReadOnlyCollection<TConcreteSubsystem> GetSubsystems<TConcreteSubsystem>()
                return subsystemInfoImpl.GetAllImplementations();
        #region public - Subsystem metadata
        /// Get the information about all subsystems.
        /// <returns>A readonly collection of all <see cref="SubsystemInfo"/> objects.</returns>
        public static ReadOnlyCollection<SubsystemInfo> GetAllSubsystemInfo()
            return s_subsystems;
        /// Get the information about a subsystem by the subsystem type.
        /// <param name="subsystemType">The base type of a specific concrete subsystem.</param>
        /// <returns>The <see cref="SubsystemInfo"/> object that represents the concrete subsystem.</returns>
        public static SubsystemInfo GetSubsystemInfo(Type subsystemType)
            ArgumentNullException.ThrowIfNull(subsystemType);
            if (s_subSystemTypeMap.TryGetValue(subsystemType, out SubsystemInfo? subsystemInfo))
                return subsystemInfo;
                subsystemType == typeof(ISubsystem)
                    ? SubsystemStrings.MustUseConcreteSubsystemType
                    : StringUtil.Format(
                        subsystemType.FullName),
                nameof(subsystemType));
        /// Get the information about a subsystem by the subsystem kind.
        /// <param name="kind">A specific <see cref="SubsystemKind"/>.</param>
        public static SubsystemInfo GetSubsystemInfo(SubsystemKind kind)
            if (s_subSystemKindMap.TryGetValue(kind, out SubsystemInfo? subsystemInfo))
                    SubsystemStrings.SubsystemKindUnknown,
                    kind.ToString()),
                nameof(kind));
        #region public - Subsystem registration
        /// Subsystem registration.
        /// <typeparam name="TImplementation">The implementation type of that concrete subsystem.</typeparam>
        /// <param name="proxy">An instance of the implementation.</param>
        public static void RegisterSubsystem<TConcreteSubsystem, TImplementation>(TImplementation proxy)
            where TImplementation : class, TConcreteSubsystem
            ArgumentNullException.ThrowIfNull(proxy);
            RegisterSubsystem(GetSubsystemInfo(typeof(TConcreteSubsystem)), proxy);
        /// Register an implementation for a subsystem.
        /// <param name="kind">The target <see cref="SubsystemKind"/> of the registration.</param>
        public static void RegisterSubsystem(SubsystemKind kind, ISubsystem proxy)
            SubsystemInfo info = GetSubsystemInfo(kind);
            if (!info.SubsystemType.IsAssignableFrom(proxy.GetType()))
                        SubsystemStrings.ConcreteSubsystemNotImplemented,
                        kind.ToString(),
                        info.SubsystemType.Name),
                    nameof(proxy));
            RegisterSubsystem(info, proxy);
        private static void RegisterSubsystem(SubsystemInfo subsystemInfo, ISubsystem proxy)
            if (proxy.Id == Guid.Empty)
                        SubsystemStrings.EmptyImplementationId,
                        subsystemInfo.Kind.ToString()),
            if (string.IsNullOrEmpty(proxy.Name))
                        SubsystemStrings.NullOrEmptyImplementationName,
            if (string.IsNullOrEmpty(proxy.Description))
                        SubsystemStrings.NullOrEmptyImplementationDescription,
            if (subsystemInfo.RequiredCmdlets.Count > 0 || subsystemInfo.RequiredFunctions.Count > 0)
                // Process 'proxy.CmdletImplementationAssembly' and 'proxy.FunctionsToDefine'
                // Functions are added to global scope.
                // Cmdlets are loaded in a way like a snapin, making the 'Source' of the cmdlets to be 'Microsoft.PowerShell.Core'.
                // For example, let's say the Job adapter is made a subsystem, then all `*-Job` cmdlets will be moved out of S.M.A
                // into a subsystem implementation DLL. After registration, all `*-Job` cmdlets should be back in the
                // 'Microsoft.PowerShell.Core' namespace to keep backward compatibility.
                // Both cmdlets and functions are added to the default InitialSessionState used for creating a new Runspace,
                // so the subsystem works for all subsequent new runspaces after it's registered.
                // Take the Job adapter subsystem as an instance again, so when creating another Runspace after the registration,
                // all '*-Job' cmdlets should be available in the 'Microsoft.PowerShell.Core' namespace by default.
            subsystemInfo.RegisterImplementation(proxy);
        #region public - Subsystem unregistration
        /// Subsystem unregistration.
        /// Throw 'InvalidOperationException' when called for subsystems that cannot be unregistered.
        /// <typeparam name="TConcreteSubsystem">The base type of the target concrete subsystem of the un-registration.</typeparam>
        /// <param name="id">The Id of the implementation to be unregistered.</param>
        public static void UnregisterSubsystem<TConcreteSubsystem>(Guid id)
            UnregisterSubsystem(GetSubsystemInfo(typeof(TConcreteSubsystem)), id);
        /// <param name="kind">The target <see cref="SubsystemKind"/> of the un-registration.</param>
        public static void UnregisterSubsystem(SubsystemKind kind, Guid id)
            UnregisterSubsystem(GetSubsystemInfo(kind), id);
        private static void UnregisterSubsystem(SubsystemInfo subsystemInfo, Guid id)
                throw new NotSupportedException("NotSupported yet: unregister subsystem that introduced new cmdlets/functions.");
            ISubsystem impl = subsystemInfo.UnregisterImplementation(id);
            if (impl is IDisposable disposable)
                    // It's OK to ignore all exceptions when disposing the object.
