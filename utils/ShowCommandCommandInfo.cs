namespace Microsoft.PowerShell.Commands.ShowCommandExtension
    /// Implements a facade around CommandInfo and its deserialized counterpart.
    public class ShowCommandCommandInfo
        /// Initializes a new instance of the <see cref="ShowCommandCommandInfo"/> class
        /// with the specified <see cref="CommandInfo"/>.
        /// <param name="other">
        /// The object to wrap.
        public ShowCommandCommandInfo(CommandInfo other)
            ArgumentNullException.ThrowIfNull(other);
            this.Name = other.Name;
            this.ModuleName = other.ModuleName;
            this.CommandType = other.CommandType;
            this.Definition = other.Definition;
            // In a runspace with restricted security settings we catch
            // PSSecurityException when accessing ParameterSets because
            // ExternalScript commands may be evaluated.
                this.ParameterSets =
                    other.ParameterSets
                        .Select(static x => new ShowCommandParameterSetInfo(x))
                        .ToList()
                        .AsReadOnly();
            catch (PSSecurityException)
                // Since we can't access the parameter sets of this command,
                // populate the ParameterSets property with an empty list
                // so that consumers don't trip on a null value.
                this.ParameterSets = new List<ShowCommandParameterSetInfo>().AsReadOnly();
            catch (ParseException)
                // Could not parse the given command so don't continue initializing it
            if (other.Module != null)
                this.Module = new ShowCommandModuleInfo(other.Module);
        /// with the specified <see cref="PSObject"/>.
        public ShowCommandCommandInfo(PSObject other)
            this.Name = other.Members["Name"].Value as string;
            this.ModuleName = other.Members["ModuleName"].Value as string;
            this.Definition = other.Members["Definition"].Value as string;
            this.ParameterSets = other.Members["ParameterSets"].Value as ICollection<ShowCommandParameterSetInfo>;
            if (this.ParameterSets != null)
                // Simple case - the objects are still live because they came from in-proc. Just cast them back
                this.CommandType = (CommandTypes)(other.Members["CommandType"].Value);
                this.Module = other.Members["Module"].Value as ShowCommandModuleInfo;
                // Objects came in their deserialized form - recreate the object graph
                this.CommandType = (CommandTypes)((other.Members["CommandType"].Value as PSObject).BaseObject);
                var parameterSets = (other.Members["ParameterSets"].Value as PSObject).BaseObject as System.Collections.ArrayList;
                this.ParameterSets = GetObjectEnumerable(parameterSets).Cast<PSObject>().Select(static x => new ShowCommandParameterSetInfo(x)).ToList().AsReadOnly();
                if (other.Members["Module"]?.Value is PSObject)
                    this.Module = new ShowCommandModuleInfo(other.Members["Module"].Value as PSObject);
        /// Builds a strongly typed IEnumerable{object} out of an IEnumerable.
        /// The object to enumerate.
        internal static IEnumerable<object> GetObjectEnumerable(System.Collections.IEnumerable enumerable)
            foreach (object obj in enumerable)
                yield return obj;
        /// A string representing the definition of the command.
        /// A string representing module the command belongs to.
        public string ModuleName { get; }
        /// A reference to the module the command came from.
        public ShowCommandModuleInfo Module { get; }
        /// An enumeration of the command types this command belongs to.
        public CommandTypes CommandType { get; }
        public ICollection<ShowCommandParameterSetInfo> ParameterSets { get; }
