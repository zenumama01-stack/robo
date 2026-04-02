    /// The RemoteHostMethodId enum.
    internal enum RemoteHostMethodId
        // Host read-only properties.
        GetName = 1,
        GetVersion = 2,
        GetInstanceId = 3,
        GetCurrentCulture = 4,
        GetCurrentUICulture = 5,
        // Host methods.
        SetShouldExit = 6,
        EnterNestedPrompt = 7,
        ExitNestedPrompt = 8,
        NotifyBeginApplication = 9,
        NotifyEndApplication = 10,
        // Host UI methods.
        ReadLine = 11,
        ReadLineAsSecureString = 12,
        Write1 = 13,
        Write2 = 14,
        WriteLine1 = 15,
        WriteLine2 = 16,
        WriteLine3 = 17,
        WriteErrorLine = 18,
        WriteDebugLine = 19,
        WriteProgress = 20,
        WriteVerboseLine = 21,
        WriteWarningLine = 22,
        Prompt = 23,
        PromptForCredential1 = 24,
        PromptForCredential2 = 25,
        PromptForChoice = 26,
        // Host Raw UI read-write properties.
        GetForegroundColor = 27,
        SetForegroundColor = 28,
        GetBackgroundColor = 29,
        SetBackgroundColor = 30,
        GetCursorPosition = 31,
        SetCursorPosition = 32,
        GetWindowPosition = 33,
        SetWindowPosition = 34,
        GetCursorSize = 35,
        SetCursorSize = 36,
        GetBufferSize = 37,
        SetBufferSize = 38,
        GetWindowSize = 39,
        SetWindowSize = 40,
        GetWindowTitle = 41,
        SetWindowTitle = 42,
        // Host Raw UI read-only properties.
        GetMaxWindowSize = 43,
        GetMaxPhysicalWindowSize = 44,
        GetKeyAvailable = 45,
        // Host Raw UI methods.
        ReadKey = 46,
        FlushInputBuffer = 47,
        SetBufferContents1 = 48,
        SetBufferContents2 = 49,
        GetBufferContents = 50,
        ScrollBufferContents = 51,
        // IHostSupportsInteractiveSession methods.
        PushRunspace = 52,
        PopRunspace = 53,
        // IHostSupportsInteractiveSession read-only properties.
        GetIsRunspacePushed = 54,
        GetRunspace = 55,
        // IHostSupportsMultipleChoiceSelection
        PromptForChoiceMultipleSelection = 56,
    /// Stores information about remote host methods. By storing information
    /// in this data structure we only need to transport enums on the wire.
    internal class RemoteHostMethodInfo
        /// Interface type.
        internal Type InterfaceType { get; }
        /// Name.
        /// Return type.
        internal Type ReturnType { get; }
        /// Parameter types.
        internal Type[] ParameterTypes { get; }
        /// Create a new instance of RemoteHostMethodInfo.
        internal RemoteHostMethodInfo(
            InterfaceType = interfaceType;
        /// Look up.
        internal static RemoteHostMethodInfo LookUp(RemoteHostMethodId methodId)
                case RemoteHostMethodId.GetName:
                    return new RemoteHostMethodInfo(
                        typeof(PSHost),
                        "get_Name",
                        Array.Empty<Type>());
                case RemoteHostMethodId.GetVersion:
                        "get_Version",
                case RemoteHostMethodId.GetInstanceId:
                        "get_InstanceId",
                case RemoteHostMethodId.GetCurrentCulture:
                        "get_CurrentCulture",
                        typeof(System.Globalization.CultureInfo),
                case RemoteHostMethodId.GetCurrentUICulture:
                        "get_CurrentUICulture",
                case RemoteHostMethodId.SetShouldExit:
                        "SetShouldExit",
                        new Type[] { typeof(int) });
                case RemoteHostMethodId.EnterNestedPrompt:
                        "EnterNestedPrompt",
                case RemoteHostMethodId.ExitNestedPrompt:
                        "ExitNestedPrompt",
                case RemoteHostMethodId.NotifyBeginApplication:
                        "NotifyBeginApplication",
                case RemoteHostMethodId.NotifyEndApplication:
                        "NotifyEndApplication",
                        typeof(PSHostUserInterface),
                        "ReadLine",
                        "ReadLineAsSecureString",
                        typeof(System.Security.SecureString),
                case RemoteHostMethodId.Write1:
                        "Write",
                        new Type[] { typeof(string) });
                case RemoteHostMethodId.Write2:
                        new Type[] { typeof(ConsoleColor), typeof(ConsoleColor), typeof(string) });
                case RemoteHostMethodId.WriteLine1:
                        "WriteLine",
                case RemoteHostMethodId.WriteLine2:
                case RemoteHostMethodId.WriteLine3:
                case RemoteHostMethodId.WriteErrorLine:
                        "WriteErrorLine",
                case RemoteHostMethodId.WriteDebugLine:
                        "WriteDebugLine",
                case RemoteHostMethodId.WriteProgress:
                        "WriteProgress",
                        new Type[] { typeof(long), typeof(ProgressRecord) });
                case RemoteHostMethodId.WriteVerboseLine:
                        "WriteVerboseLine",
                case RemoteHostMethodId.WriteWarningLine:
                        "WriteWarningLine",
                        "Prompt",
                        typeof(Dictionary<string, PSObject>),
                        new Type[] { typeof(string), typeof(string), typeof(System.Collections.ObjectModel.Collection<FieldDescription>) });
                        "PromptForCredential",
                        typeof(PSCredential),
                        new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) });
                        new Type[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(PSCredentialTypes), typeof(PSCredentialUIOptions) });
                        "PromptForChoice",
                        new Type[] { typeof(string), typeof(string), typeof(System.Collections.ObjectModel.Collection<ChoiceDescription>), typeof(int) });
                        typeof(IHostUISupportsMultipleChoiceSelection),
                        typeof(Collection<int>),
                        new Type[] { typeof(string), typeof(string), typeof(Collection<ChoiceDescription>), typeof(IEnumerable<int>) });
                // Host raw UI read-write properties.
                case RemoteHostMethodId.GetForegroundColor:
                        typeof(PSHostRawUserInterface),
                        "get_ForegroundColor",
                        typeof(ConsoleColor),
                case RemoteHostMethodId.SetForegroundColor:
                        "set_ForegroundColor",
                        new Type[] { typeof(ConsoleColor) });
                case RemoteHostMethodId.GetBackgroundColor:
                        "get_BackgroundColor",
                case RemoteHostMethodId.SetBackgroundColor:
                        "set_BackgroundColor",
                case RemoteHostMethodId.GetCursorPosition:
                        "get_CursorPosition",
                        typeof(Coordinates),
                case RemoteHostMethodId.SetCursorPosition:
                        "set_CursorPosition",
                        new Type[] { typeof(Coordinates) });
                case RemoteHostMethodId.GetWindowPosition:
                        "get_WindowPosition",
                case RemoteHostMethodId.SetWindowPosition:
                        "set_WindowPosition",
                case RemoteHostMethodId.GetCursorSize:
                        "get_CursorSize",
                case RemoteHostMethodId.SetCursorSize:
                        "set_CursorSize",
                case RemoteHostMethodId.GetBufferSize:
                        "get_BufferSize",
                        typeof(Size),
                case RemoteHostMethodId.SetBufferSize:
                        "set_BufferSize",
                        new Type[] { typeof(Size) });
                case RemoteHostMethodId.GetWindowSize:
                        "get_WindowSize",
                case RemoteHostMethodId.SetWindowSize:
                        "set_WindowSize",
                case RemoteHostMethodId.GetWindowTitle:
                        "get_WindowTitle",
                case RemoteHostMethodId.SetWindowTitle:
                        "set_WindowTitle",
                // Host raw UI read-only properties.
                case RemoteHostMethodId.GetMaxWindowSize:
                        "get_MaxWindowSize",
                case RemoteHostMethodId.GetMaxPhysicalWindowSize:
                        "get_MaxPhysicalWindowSize",
                case RemoteHostMethodId.GetKeyAvailable:
                        "get_KeyAvailable",
                // Host raw UI methods.
                        "ReadKey",
                        typeof(KeyInfo),
                        new Type[] { typeof(ReadKeyOptions) });
                case RemoteHostMethodId.FlushInputBuffer:
                        "FlushInputBuffer",
                case RemoteHostMethodId.SetBufferContents1:
                        "SetBufferContents",
                        new Type[] { typeof(Rectangle), typeof(BufferCell) });
                case RemoteHostMethodId.SetBufferContents2:
                        new Type[] { typeof(Coordinates), typeof(BufferCell[,]) });
                case RemoteHostMethodId.GetBufferContents:
                        "GetBufferContents",
                        typeof(BufferCell[,]),
                        new Type[] { typeof(Rectangle) });
                case RemoteHostMethodId.ScrollBufferContents:
                        "ScrollBufferContents",
                        new Type[] { typeof(Rectangle), typeof(Coordinates), typeof(Rectangle), typeof(BufferCell) });
                case RemoteHostMethodId.PushRunspace:
                        typeof(IHostSupportsInteractiveSession),
                        "PushRunspace",
                        new Type[] { typeof(System.Management.Automation.Runspaces.Runspace) });
                case RemoteHostMethodId.PopRunspace:
                        "PopRunspace",
                // IHostSupportsInteractiveSession properties.
                case RemoteHostMethodId.GetIsRunspacePushed:
                        "get_IsRunspacePushed",
                case RemoteHostMethodId.GetRunspace:
                        "get_Runspace",
                        typeof(System.Management.Automation.Runspaces.Runspace),
                    Dbg.Assert(false, "All RemoteHostMethodId's should be handled. This code should not be reached.");
