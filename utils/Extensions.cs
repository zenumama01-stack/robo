namespace System.Management.Automation.SecurityAccountsManager.Extensions
    /// Provides extension methods for the Cmdlet class.
    internal static class CmdletExtensions
        /// Attempt to create a SID from a string.
        /// <param name="cmdlet">The cmdlet being extended with this method.</param>
        /// <param name="s">The string to be converted to a SID.</param>
        /// <param name="allowSidConstants">
        /// A boolean indicating whether SID constants, such as "BA", are considered.
        /// A <see cref="SecurityIdentifier"/> object if the conversion was successful,
        /// null otherwise.
        internal static SecurityIdentifier TrySid(this Cmdlet cmdlet,
                                                  string s,
                                                  bool allowSidConstants = false)
            if (!allowSidConstants)
                if (!(s.Length > 2 && s.StartsWith("S-", StringComparison.Ordinal) && char.IsDigit(s[2])))
            SecurityIdentifier sid = null;
                sid = new SecurityIdentifier(s);
                // do nothing here, just fall through to the return
            return sid;
    /// Provides extension methods for the PSCmdlet class.
    internal static class PSExtensions
        /// Determine if a given parameter was provided to the cmdlet.
        /// The <see cref="PSCmdlet"/> object to check.
        /// <param name="parameterName">
        /// A string containing the name of the parameter. This should be in the
        /// same letter-casing as the defined parameter.
        /// True if the specified parameter was given on the cmdlet invocation,
        internal static bool HasParameter(this PSCmdlet cmdlet, string parameterName)
            var invocation = cmdlet.MyInvocation;
            if (invocation != null)
                var parameters = invocation.BoundParameters;
                if (parameters != null)
                    // PowerShell sets the parameter names in the BoundParameters dictionary
                    // to their "proper" casing, so we don't have to do a case-insensitive search.
                    if (parameters.ContainsKey(parameterName))
    /// Provides extension methods for the SecurityIdentifier class.
    internal static class SidExtensions
        /// Get the Relative ID (RID) from a <see cref="SecurityIdentifier"/> object.
        /// <param name="sid">The SecurityIdentifier containing the desired Relative ID.</param>
        /// A UInt32 value containing the Relative ID in the SecurityIdentifier.
        internal static UInt32 GetRid(this SecurityIdentifier sid)
            byte[] sidBinary = new byte[sid.BinaryLength];
            sid.GetBinaryForm(sidBinary, 0);
            return System.BitConverter.ToUInt32(sidBinary, sidBinary.Length-4);
        /// Gets the Identifier Authority portion of a <see cref="SecurityIdentifier"/>
        /// <param name="sid">The SecurityIdentifier containing the desired Authority.</param>
        /// A long integer value containing the SecurityIdentifier's Identifier Authority value.
        /// This method is used primarily for determining the Source of a Principal.
        /// The Win32 API LsaLookupUserAccountType function does not (yet) properly
        /// identify MicrosoftAccount principals.
        internal static long GetIdentifierAuthority(this SecurityIdentifier sid)
            // The Identifier Authority is six bytes wide,
            // in big-endian format, starting at the third byte
            long authority = (long) (((long)sidBinary[2]) << 40) +
                                    (((long)sidBinary[3]) << 32) +
                                    (((long)sidBinary[4]) << 24) +
                                    (((long)sidBinary[5]) << 16) +
                                    (((long)sidBinary[6]) <<  8) +
                                    (((long)sidBinary[7])      );
            return authority;
        internal static bool IsMsaAccount(this SecurityIdentifier sid)
            return sid.GetIdentifierAuthority() == 11;
    internal static class SecureStringExtensions
        /// Extension method to extract clear text from a
        /// <see cref="System.Security.SecureString"/> object.
        /// <param name="str">
        /// This SecureString object, containing encrypted text.
        /// A string containing the SecureString object's original text.
        internal static string AsString(this SecureString str)
            IntPtr buffer = SecureStringMarshal.SecureStringToCoTaskMemUnicode(str);
            string clear = Marshal.PtrToStringUni(buffer);
            Marshal.ZeroFreeCoTaskMemUnicode(buffer);
            var bstr = Marshal.SecureStringToBSTR(str);
            string clear = Marshal.PtrToStringAuto(bstr);
            Marshal.ZeroFreeBSTR(bstr);
            return clear;
    internal static class ExceptionExtensions
        internal static ErrorRecord MakeErrorRecord(this Exception ex,
                                                    object target = null)
            return new ErrorRecord(ex, errorId, errorCategory, target);
        internal static ErrorRecord MakeErrorRecord(this Exception ex, object target = null)
            // This part is somewhat less than beautiful, but it prevents
            // having to have multiple exception handlers in every cmdlet command.
            var exTemp = ex as LocalAccountsException;
            if (exTemp != null)
                return MakeErrorRecord(exTemp, target ?? exTemp.Target);
            return new ErrorRecord(ex,
                                   Strings.UnspecifiedError,
                                   ErrorCategory.NotSpecified,
                                   target);
        internal static ErrorRecord MakeErrorRecord(this LocalAccountsException ex, object target = null)
            return ex.MakeErrorRecord(ex.ErrorName, ex.ErrorCategory, target ?? ex.Target);
﻿// Licensed to the .NET Foundation under one or more agreements.
// See the LICENSE file in the project root for more information.
using BenchmarkDotNet.Reports;
    public static class SummaryExtensions
        public static int ToExitCode(this IEnumerable<Summary> summaries)
            // an empty summary means that initial filtering and validation did not allow to run
            if (!summaries.Any())
            // if anything has failed, it's an error
            if (summaries.Any(summary => summary.HasCriticalValidationErrors || summary.Reports.Any(report => !report.BuildResult.IsBuildSuccess || !report.AllMeasurements.Any())))
