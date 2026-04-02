    /// To make it easier to specify a version, we add some conversions that wouldn't happen otherwise:
    ///   * A simple integer, i.e. 2;
    ///   * A string without a dot, i.e. "2".
    internal class ArgumentToVersionTransformationAttribute : ArgumentTransformationAttribute
            object version = PSObject.Base(inputData);
            if (version is string versionStr)
                if (TryConvertFromString(versionStr, out var convertedVersion))
                    return convertedVersion;
                if (versionStr.Contains('.'))
                    // If the string contains a '.', let the Version constructor handle the conversion.
            if (version is double)
                // The conversion to int below is wrong, but the usual conversions will turn
                // the double into a string, so just return the original object.
            if (LanguagePrimitives.TryConvertTo<int>(version, out var majorVersion))
                return new Version(majorVersion, 0);
        protected virtual bool TryConvertFromString(string versionString, [NotNullWhen(true)] out Version? version)
            version = null;
