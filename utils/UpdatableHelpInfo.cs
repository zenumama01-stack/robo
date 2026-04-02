    /// Represents each supported culture.
    internal class CultureSpecificUpdatableHelp
        /// <param name="version">Version info.</param>
        internal CultureSpecificUpdatableHelp(CultureInfo culture, Version version)
            Debug.Assert(version != null);
            Debug.Assert(culture != null);
            Culture = culture;
        /// Culture version.
        internal Version Version { get; set; }
        /// Supported culture.
        internal CultureInfo Culture { get; set; }
        /// Enumerates fallback chain (parents) of the culture, including itself.
        /// <param name="culture">Culture to enumerate</param>
        /// en-GB => { en-GB, en }
        /// zh-Hans-CN => { zh-Hans-CN, zh-Hans, zh }.
        /// <returns>An enumerable list of culture names.</returns>
        internal static IEnumerable<string> GetCultureFallbackChain(CultureInfo culture)
            // We use just names instead because comparing two CultureInfo objects
            // can fail if they are created using different means
            while (culture != null)
                if (string.IsNullOrEmpty(culture.Name))
                yield return culture.Name;
        /// Checks if a culture is supported.
        /// <param name="cultureName">Name of the culture to check.</param>
        /// <returns>True if supported, false if not.</returns>
        internal bool IsCultureSupported(string cultureName)
            Debug.Assert(cultureName != null, $"{nameof(cultureName)} may not be null");
            return GetCultureFallbackChain(Culture).Any(fallback => fallback == cultureName);
    /// This class represents the HelpInfo metadata XML.
    internal class UpdatableHelpInfo
        /// <param name="unresolvedUri">Unresolved help content URI.</param>
        /// <param name="cultures">Supported UI cultures.</param>
        internal UpdatableHelpInfo(string unresolvedUri, CultureSpecificUpdatableHelp[] cultures)
            Debug.Assert(cultures != null);
            UnresolvedUri = unresolvedUri;
            HelpContentUriCollection = new Collection<UpdatableHelpUri>();
            UpdatableHelpItems = cultures;
        /// Unresolved URI.
        internal string UnresolvedUri { get; }
        /// Link to the actual help content.
        internal Collection<UpdatableHelpUri> HelpContentUriCollection { get; }
        /// Supported UI cultures.
        internal CultureSpecificUpdatableHelp[] UpdatableHelpItems { get; }
        /// Checks if the other HelpInfo has a newer version.
        /// <param name="helpInfo">HelpInfo object to check.</param>
        /// <param name="culture">Culture to check.</param>
        /// <returns>True if the other HelpInfo is newer, false if not.</returns>
        internal bool IsNewerVersion(UpdatableHelpInfo helpInfo, CultureInfo culture)
            Debug.Assert(helpInfo != null);
            Version v1 = helpInfo.GetCultureVersion(culture);
            Version v2 = GetCultureVersion(culture);
            Debug.Assert(v1 != null);
            if (v2 == null)
            return v1 > v2;
            return UpdatableHelpItems.Any(item => item.IsCultureSupported(cultureName));
        /// Gets a string representation of the supported cultures.
        /// <returns>Supported cultures in string.</returns>
        internal string GetSupportedCultures()
            if (UpdatableHelpItems.Length == 0)
                return StringUtil.Format(HelpDisplayStrings.None);
            for (int i = 0; i < UpdatableHelpItems.Length; i++)
                sb.Append(UpdatableHelpItems[i].Culture.Name);
                if (i != (UpdatableHelpItems.Length - 1))
                    sb.Append(" | ");
        /// Gets the culture version.
        /// <returns>Culture version.</returns>
        internal Version GetCultureVersion(CultureInfo culture)
            foreach (CultureSpecificUpdatableHelp updatableHelpItem in UpdatableHelpItems)
                if (string.Equals(updatableHelpItem.Culture.Name, culture.Name,
                    return updatableHelpItem.Version;
