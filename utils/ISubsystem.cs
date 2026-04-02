namespace System.Management.Automation.Subsystem
    /// Define the kinds of subsystems.
    /// This enum uses power of 2 as the values for the enum elements, so as to make sure
    /// the bitwise 'or' operation of the elements always results in an invalid value.
    public enum SubsystemKind : uint
        /// Component that provides predictive suggestions to commandline input.
        CommandPredictor = 1,
        /// Cross platform desired state configuration component.
        CrossPlatformDsc = 2,
        /// Component that provides feedback when a command fails interactively.
        FeedbackProvider = 4,
    /// Define the base interface to implement a subsystem.
    /// The API contracts for specific subsystems are defined within the specific interfaces/abstract classes that implements this interface.
    /// A user should not directly implement <see cref="ISubsystem"/>, but instead should derive from one of the concrete subsystem interfaces or abstract classes.
    /// The instance of a type that only implements 'ISubsystem' cannot be registered to the <see cref="SubsystemManager"/>.
    public interface ISubsystem
        /// Gets the unique identifier for a subsystem implementation.
        Guid Id { get; }
        /// Gets the name of a subsystem implementation.
        string Name { get; }
        /// Gets the description of a subsystem implementation.
        string Description { get; }
        /// Gets a dictionary that contains the functions to be defined at the global scope of a PowerShell session.
        /// Key: function name; Value: function script.
        Dictionary<string, string>? FunctionsToDefine { get; }
