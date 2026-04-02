    /// Returns:
    ///     - the thread's current culture
    ///     - culture by name
    ///     - list of all supported cultures.
    [Cmdlet(VerbsCommon.Get, "Culture", DefaultParameterSetName = CurrentCultureParameterSet, HelpUri = "https://go.microsoft.com/fwlink/?LinkID=2097017")]
    [OutputType(typeof(System.Globalization.CultureInfo))]
    public sealed class GetCultureCommand : PSCmdlet
        private const string CurrentCultureParameterSet = "CurrentCulture";
        private const string ListAvailableParameterSet = "ListAvailable";
        /// Gets or sets culture names for which CultureInfo values are returned.
        /// Empty string matches Invariant culture.
        [Parameter(ParameterSetName = NameParameterSet, Position = 0, ValueFromPipeline = true)]
        [ValidateSet(typeof(ValidateCultureNamesGenerator))]
        /// Gets or sets a switch to return current culture with user overrides (by default).
        /// With the switch on, we return current culture without user overrides.
        [Parameter(ParameterSetName = CurrentCultureParameterSet)]
        public SwitchParameter NoUserOverrides { get; set; }
        /// Gets or sets a switch to list all available cultures.
        [Parameter(ParameterSetName = ListAvailableParameterSet)]
        /// Output:
            CultureInfo ci;
                case CurrentCultureParameterSet:
                    if (NoUserOverrides)
                        ci = CultureInfo.GetCultureInfo(Host.CurrentCulture.Name);
                        ci = Host.CurrentCulture;
                    WriteObject(ci);
                case NameParameterSet:
                        foreach (var cultureName in Name)
                            if (!NoUserOverrides && string.Equals(cultureName, Host.CurrentCulture.Name, StringComparison.CurrentCultureIgnoreCase))
                                ci = CultureInfo.GetCultureInfo(cultureName);
                    catch (CultureNotFoundException exc)
                        WriteError(new ErrorRecord(exc, "ItemNotFoundException", ErrorCategory.ObjectNotFound, Name));
                case ListAvailableParameterSet:
                    foreach (var cultureInfo in CultureInfo.GetCultures(CultureTypes.AllCultures))
                        WriteObject(cultureInfo);
    /// Get list of valid culture names for ValidateSet attribute.
    public class ValidateCultureNamesGenerator : IValidateSetValuesGenerator
        string[] IValidateSetValuesGenerator.GetValidValues()
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            var result = new List<string>(cultures.Length);
            foreach (var cultureInfo in cultures)
                result.Add(cultureInfo.Name);
            return result.ToArray();
