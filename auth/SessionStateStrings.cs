    /// Holds the #defines for any special strings used in session state.
    internal static class StringLiterals
        /// The separator used in provider base paths. The format is
        /// providerId::providerPath.
        internal const string ProviderPathSeparator = "::";
        /// Porting note: IO.Path.DirectorySeparatorChar is correct for all platforms. On Windows,
        /// it is '\', and on Linux, it is '/', as expected.
        internal static readonly char DefaultPathSeparator = System.IO.Path.DirectorySeparatorChar;
        internal static readonly string DefaultPathSeparatorString = DefaultPathSeparator.ToString();
        /// Porting note: we do not use .NET's AlternatePathSeparatorChar here because it correctly
        /// states that both the default and alternate are '/' on Linux. However, for PowerShell to
        /// be "slash agnostic", we need to use the assumption that a '\' is the alternate path
        /// separator on Linux.
        internal static readonly char AlternatePathSeparator = Platform.IsWindows ? '/' : '\\';
        internal static readonly string AlternatePathSeparatorString = AlternatePathSeparator.ToString();
        /// The default path prefix for remote paths. This is to mimic
        /// UNC paths in the file system.
        internal const string DefaultRemotePathPrefix = "\\\\";
        /// The alternate path prefix for remote paths. This is to mimic
        internal const string AlternateRemotePathPrefix = "//";
        /// The character used in a path to indicate the home location.
        internal const string HomePath = "~";
        /// Name of the global variable table in Variable scopes of session state.
        internal const string Global = "GLOBAL";
        /// Name of the current scope variable table of session state.
        internal const string Local = "LOCAL";
        /// When prefixing a variable "private" makes the variable
        /// only visible in the current scope.
        internal const string Private = "PRIVATE";
        /// When prefixing a variable "script" makes the variable
        /// global to the script but not global to the entire session.
        internal const string Script = "SCRIPT";
        /// Session state string used as resource name in exceptions.
        internal const string SessionState = "SessionState";
        /// The file extension (including the dot) of an PowerShell script file.
        internal const string PowerShellScriptFileExtension = ".ps1";
        /// The file extension (including the dot) of an PowerShell module file.
        internal const string PowerShellModuleFileExtension = ".psm1";
        /// The file extension (including the dot) of an Mof file.
        internal const string PowerShellMofFileExtension = ".mof";
        /// The file extension (including the dot) of a PowerShell cmdletization file.
        internal const string PowerShellCmdletizationFileExtension = ".cdxml";
        /// The file extension (including the dot) of a PowerShell declarative session configuration file.
        internal const string PowerShellDISCFileExtension = ".pssc";
        /// The file extension (including the dot) of a PowerShell role capability file.
        internal const string PowerShellRoleCapabilityFileExtension = ".psrc";
        /// The file extension (including the dot) of an PowerShell data file.
        internal const string PowerShellDataFileExtension = ".psd1";
        /// The file extension (including the dot) of an workflow dependent assembly.
        internal const string PowerShellILAssemblyExtension = ".dll";
        /// The file extension (including the dot) of an workflow dependent Ngen assembly.
        internal const string PowerShellNgenAssemblyExtension = ".ni.dll";
        /// The file extension (including the dot) of an executable file.
        internal const string PowerShellILExecutableExtension = ".exe";
        internal const string PowerShellConsoleFileExtension = ".psc1";
        /// The default verb/noun separator for a command. verb-noun or verb/noun.
        internal const char CommandVerbNounSeparator = '-';
        /// The default verb to try if the command was not resolved.
        internal const string DefaultCommandVerb = "get";
        /// The default extension for a help file relative to its code assembly name.
        internal const string HelpFileExtension = "-Help.xml";
        /// The language representation of null.
        internal const string DollarNull = "$null";
        internal const string Null = "null";
        /// The language representation of false.
        internal const string False = "false";
        /// The language representation of true.
        internal const string True = "true";
        /// The escape character used in the language.
        internal const char EscapeCharacter = '`';
        /// The default cmdlet adapter for cmdletization / cdxml modules.
        internal const string DefaultCmdletAdapter = "Microsoft.PowerShell.Cmdletization.Cim.CimCmdletAdapter, Microsoft.PowerShell.Commands.Management, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
