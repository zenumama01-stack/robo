    /// Implementation of 'Get-PSSubsystem' cmdlet.
    [Cmdlet(VerbsCommon.Get, "PSSubsystem", DefaultParameterSetName = AllSet)]
    [OutputType(typeof(SubsystemInfo))]
    public sealed class GetPSSubsystemCommand : PSCmdlet
        private const string AllSet = "GetAllSet";
        private const string TypeSet = "GetByTypeSet";
        private const string KindSet = "GetByKindSet";
        /// Gets or sets a concrete subsystem kind.
        [Parameter(Mandatory = true, ParameterSetName = KindSet, ValueFromPipeline = true)]
        public SubsystemKind Kind { get; set; }
        /// Gets or sets the interface or abstract class type of a concrete subsystem.
        [Parameter(Mandatory = true, ParameterSetName = TypeSet, ValueFromPipeline = true)]
        public Type? SubsystemType { get; set; }
        /// ProcessRecord implementation.
                case AllSet:
                    WriteObject(SubsystemManager.GetAllSubsystemInfo(), enumerateCollection: true);
                case KindSet:
                    WriteObject(SubsystemManager.GetSubsystemInfo(Kind));
                case TypeSet:
                    WriteObject(SubsystemManager.GetSubsystemInfo(SubsystemType!));
                    throw new InvalidOperationException("New parameter set is added but the switch statement is not updated.");
