    /// This provider is the data accessor for environment variables. It uses
    [CmdletProvider(EnvironmentProvider.ProviderName, ProviderCapabilities.ShouldProcess)]
    public sealed class EnvironmentProvider : SessionStateProviderBase
        public const string ProviderName = "Environment";
        /// The constructor for the provider that exposes environment variables to the user
        public EnvironmentProvider()
            string description = SessionStateStrings.EnvironmentDriveDescription;
            PSDriveInfo envDrive =
                    DriveNames.EnvironmentDrive,
            drives.Add(envDrive);
        /// Gets a environment variable from session state.
        /// The name of the environment variable to retrieve.
        /// A DictionaryEntry that represents the value of the environment variable.
            string value = Environment.GetEnvironmentVariable(name);
                result = new DictionaryEntry(name, value);
        /// Sets the environment variable of the specified name to the specified value.
        /// The name of the environment variable to set.
        /// The new value for the environment variable.
                Environment.SetEnvironmentVariable(name, null);
                // First see if we got a DictionaryEntry which represents
                // an item for this provider. If so, use the value from
                // the dictionary entry.
                if (value is DictionaryEntry)
                    value = ((DictionaryEntry)value).Value;
                if (stringValue == null)
                    // try using ETS to convert to a string.
                    PSObject wrappedObject = PSObject.AsPSObject(value);
                    stringValue = wrappedObject.ToString();
                Environment.SetEnvironmentVariable(name, stringValue);
                DictionaryEntry item = new DictionaryEntry(name, stringValue);
                if (writeItem)
                    WriteItemObject(item, name, false);
        /// Removes the specified environment variable from session state.
        /// The name of the environment variable to remove from session state.
        /// Gets a flattened view of the environment variables in session state.
        /// An IDictionary representing the flattened view of the environment variables in
            // Environment variables are case-sensitive on Unix and
            // case-insensitive on Windows
            Dictionary<string, DictionaryEntry> providerTable =
                new Dictionary<string, DictionaryEntry>(StringComparer.Ordinal);
                new Dictionary<string, DictionaryEntry>(StringComparer.OrdinalIgnoreCase);
            // The environment variables returns a dictionary of keys and values that are
            // both strings. We want to return a dictionary with the key as a string and
            // the value as the DictionaryEntry containing both the name and env variable
            IDictionary environmentTable = Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry entry in environmentTable)
                if (!providerTable.TryAdd((string)entry.Key, entry))
                {   // Windows only: duplicate key (variable name that differs only in case)
                    // NOTE: Even though this shouldn't happen, it can, e.g. when npm
                    //       creates duplicate environment variables that differ only in case -
                    //       see https://github.com/PowerShell/PowerShell/issues/6305.
                    //       However, because retrieval *by name* later is invariably
                    //       case-INsensitive, in effect only a *single* variable exists.
                    //       We simply ask Environment.GetEnvironmentVariable() for the effective value
                    //       and use that as the only entry, because for a given key 'foo' (and all its case variations),
                    //       that is guaranteed to match what $env:FOO and [environment]::GetEnvironmentVariable('foo') return.
                    //       (If, by contrast, we just used `entry` as-is every time a duplicate is encountered,
                    //        it could - intermittently - represent a value *other* than the effective one.)
                    string effectiveValue = Environment.GetEnvironmentVariable((string)entry.Key);
                    if (((string)entry.Value).Equals(effectiveValue, StringComparison.Ordinal))
                    { // We've found the effective definition.
                        // Note: We *recreate* the entry so that the specific name casing of the
                        //       effective definition is also reflected. However, if the case variants
                        //       define the same value, it is unspecified which name variant is reflected
                        //       in Get-Item env: output; given the always case-insensitive nature of the retrieval,
                        //       that shouldn't matter.
                        providerTable.Remove((string)entry.Key);
                        providerTable.Add((string)entry.Key, entry);
            return providerTable;
        /// Gets the Value property of the DictionaryEntry item.
        /// The item to get the value from.
            if (item is DictionaryEntry)
                value = ((DictionaryEntry)item).Value;
    public class EnvironmentProvider : IEnvironment
        public string GetEnvironmentVariable(string variable) => Environment.GetEnvironmentVariable(variable);
