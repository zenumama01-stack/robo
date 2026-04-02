    internal static class UnsafeNativeMethods
        private const string FormatMessageDllName = "api-ms-win-core-localization-l1-2-0.dll";
        private const string EventProviderDllName = "api-ms-win-eventing-provider-l1-1-0.dll";
        private const string WEVTAPI = "wevtapi.dll";
        private static readonly IntPtr s_NULL = IntPtr.Zero;
        // WinError.h codes:
        internal const int ERROR_SUCCESS = 0x0;
        internal const int ERROR_FILE_NOT_FOUND = 0x2;
        internal const int ERROR_PATH_NOT_FOUND = 0x3;
        internal const int ERROR_ACCESS_DENIED = 0x5;
        internal const int ERROR_INVALID_HANDLE = 0x6;
        // Can occurs when filled buffers are trying to flush to disk, but disk IOs are not fast enough.
        // This happens when the disk is slow and event traffic is heavy.
        // Eventually, there are no more free (empty) buffers and the event is dropped.
        internal const int ERROR_NOT_ENOUGH_MEMORY = 0x8;
        internal const int ERROR_INVALID_DRIVE = 0xF;
        internal const int ERROR_NO_MORE_FILES = 0x12;
        internal const int ERROR_NOT_READY = 0x15;
        internal const int ERROR_BAD_LENGTH = 0x18;
        internal const int ERROR_SHARING_VIOLATION = 0x20;
        internal const int ERROR_LOCK_VIOLATION = 0x21;  // 33
        internal const int ERROR_HANDLE_EOF = 0x26;  // 38
        internal const int ERROR_FILE_EXISTS = 0x50;
        internal const int ERROR_INVALID_PARAMETER = 0x57;  // 87
        internal const int ERROR_BROKEN_PIPE = 0x6D;  // 109
        internal const int ERROR_INSUFFICIENT_BUFFER = 0x7A;  // 122
        internal const int ERROR_INVALID_NAME = 0x7B;
        internal const int ERROR_BAD_PATHNAME = 0xA1;
        internal const int ERROR_ALREADY_EXISTS = 0xB7;
        internal const int ERROR_ENVVAR_NOT_FOUND = 0xCB;
        internal const int ERROR_FILENAME_EXCED_RANGE = 0xCE;  // filename too long
        internal const int ERROR_PIPE_BUSY = 0xE7;  // 231
        internal const int ERROR_NO_DATA = 0xE8;  // 232
        internal const int ERROR_PIPE_NOT_CONNECTED = 0xE9;  // 233
        internal const int ERROR_MORE_DATA = 0xEA;
        internal const int ERROR_NO_MORE_ITEMS = 0x103;  // 259
        internal const int ERROR_PIPE_CONNECTED = 0x217;  // 535
        internal const int ERROR_PIPE_LISTENING = 0x218;  // 536
        internal const int ERROR_OPERATION_ABORTED = 0x3E3;  // 995; For IO Cancellation
        internal const int ERROR_IO_PENDING = 0x3E5;  // 997
        internal const int ERROR_NOT_FOUND = 0x490;  // 1168
        // The event size is larger than the allowed maximum (64k - header).
        internal const int ERROR_ARITHMETIC_OVERFLOW = 0x216;  // 534
        internal const int ERROR_RESOURCE_LANG_NOT_FOUND = 0x717;  // 1815
        // Event log specific codes:
        internal const int ERROR_EVT_MESSAGE_NOT_FOUND = 15027;
        internal const int ERROR_EVT_MESSAGE_ID_NOT_FOUND = 15028;
        internal const int ERROR_EVT_UNRESOLVED_VALUE_INSERT = 15029;
        internal const int ERROR_EVT_UNRESOLVED_PARAMETER_INSERT = 15030;
        internal const int ERROR_EVT_MAX_INSERTS_REACHED = 15031;
        internal const int ERROR_EVT_MESSAGE_LOCALE_NOT_FOUND = 15033;
        internal const int ERROR_MUI_FILE_NOT_FOUND = 15100;
        // ErrorCode & format
        // for win32 error message formatting
        private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
        [DllImport(FormatMessageDllName, CharSet = CharSet.Unicode, BestFitMapping = false)]
        internal static extern int FormatMessage(int dwFlags, IntPtr lpSource,
            int dwMessageId, int dwLanguageId, StringBuilder lpBuffer,
            int nSize, IntPtr va_list_arguments);
        // Gets an error message for a Win32 error code.
        internal static string GetMessage(int errorCode)
            StringBuilder sb = new(512);
            int result = UnsafeNativeMethods.FormatMessage(FORMAT_MESSAGE_IGNORE_INSERTS |
                FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_ARGUMENT_ARRAY,
                UnsafeNativeMethods.s_NULL, errorCode, 0, sb, sb.Capacity, UnsafeNativeMethods.s_NULL);
                // result is the # of characters copied to the StringBuilder on NT,
                // but on Win9x, it appears to be the number of MBCS buffer.
                // Just give up and return the String as-is...
                string s = sb.ToString();
                return "UnknownError_Num " + errorCode;
        // ETW Methods
        // Callback
        internal unsafe delegate void EtwEnableCallback(
            [In] ref Guid sourceId,
            [In] byte level,
            [In] long matchAnyKeywords,
            [In] long matchAllKeywords,
        // Registration APIs
        [DllImport(EventProviderDllName, ExactSpelling = true, EntryPoint = "EventRegister", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        internal static extern unsafe uint EventRegister(
                    [In] in Guid providerId,
                    [In] EtwEnableCallback enableCallback,
                    [In] void* callbackContext,
                    [In][Out] ref long registrationHandle
        [DllImport(EventProviderDllName, ExactSpelling = true, EntryPoint = "EventUnregister", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        internal static extern int EventUnregister([In] long registrationHandle);
        // Control (Is Enabled) APIs
        [DllImport(EventProviderDllName, ExactSpelling = true, EntryPoint = "EventEnabled", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        internal static extern int EventEnabled([In] long registrationHandle, [In] in System.Diagnostics.Eventing.EventDescriptor eventDescriptor);
        [DllImport(EventProviderDllName, ExactSpelling = true, EntryPoint = "EventProviderEnabled", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        internal static extern int EventProviderEnabled([In] long registrationHandle, [In] byte level, [In] long keywords);
        // Writing (Publishing/Logging) APIs
        [DllImport(EventProviderDllName, ExactSpelling = true, EntryPoint = "EventWrite", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        internal static extern unsafe uint EventWrite(
                [In] long registrationHandle,
                [In] in EventDescriptor eventDescriptor,
                [In] uint userDataCount,
                [In] void* userData
                [In] EventDescriptor* eventDescriptor,
        [DllImport(EventProviderDllName, ExactSpelling = true, EntryPoint = "EventWriteTransfer", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        internal static extern unsafe uint EventWriteTransfer(
                [In] Guid* activityId,
                [In] Guid* relatedActivityId,
        [DllImport(EventProviderDllName, ExactSpelling = true, EntryPoint = "EventWriteString", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        internal static extern unsafe uint EventWriteString(
                [In] long keywords,
                [In] char* message
        // ActivityId Control APIs
        [DllImport(EventProviderDllName, ExactSpelling = true, EntryPoint = "EventActivityIdControl", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        internal static extern unsafe uint EventActivityIdControl([In] int ControlCode, [In][Out] ref Guid ActivityId);
        // EventLog
        internal enum EvtQueryFlags
            EvtQueryChannelPath = 0x1,
            EvtQueryFilePath = 0x2,
            EvtQueryForwardDirection = 0x100,
            EvtQueryReverseDirection = 0x200,
            EvtQueryTolerateQueryErrors = 0x1000
        /// Evt Variant types.
        internal enum EvtVariantType
            EvtVarTypeNull = 0,
            EvtVarTypeString = 1,
            EvtVarTypeAnsiString = 2,
            EvtVarTypeSByte = 3,
            EvtVarTypeByte = 4,
            EvtVarTypeInt16 = 5,
            EvtVarTypeUInt16 = 6,
            EvtVarTypeInt32 = 7,
            EvtVarTypeUInt32 = 8,
            EvtVarTypeInt64 = 9,
            EvtVarTypeUInt64 = 10,
            EvtVarTypeSingle = 11,
            EvtVarTypeDouble = 12,
            EvtVarTypeBoolean = 13,
            EvtVarTypeBinary = 14,
            EvtVarTypeGuid = 15,
            EvtVarTypeSizeT = 16,
            EvtVarTypeFileTime = 17,
            EvtVarTypeSysTime = 18,
            EvtVarTypeSid = 19,
            EvtVarTypeHexInt32 = 20,
            EvtVarTypeHexInt64 = 21,
            // these types used internally
            EvtVarTypeEvtHandle = 32,
            EvtVarTypeEvtXml = 35,
            // Array = 128
            EvtVarTypeStringArray = 129,
            EvtVarTypeUInt32Array = 136
        internal enum EvtMasks
            EVT_VARIANT_TYPE_MASK = 0x7f,
            EVT_VARIANT_TYPE_ARRAY = 128
        internal struct SystemTime
        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
        internal struct EvtVariant
            public uint UInteger;
            public int Integer;
            public byte UInt8;
            public short Short;
            public ushort UShort;
            public uint Bool;
            public byte ByteVal;
            public byte SByte;
            public ulong ULong;
            public long Long;
            public float Single;
            public double Double;
            public IntPtr StringVal;
            public IntPtr AnsiString;
            public IntPtr SidVal;
            public IntPtr Binary;
            public IntPtr Reference;
            public IntPtr Handle;
            public IntPtr GuidReference;
            public ulong FileTime;
            public IntPtr SystemTime;
            public IntPtr SizeT;
            public uint Count;   // number of elements (not length) in bytes.
        internal enum EvtEventPropertyId
            EvtEventQueryIDs = 0,
            EvtEventPath = 1
        /// The query flags to get information about query.
        internal enum EvtQueryPropertyId
            EvtQueryNames = 0,   // String;   // Variant will be array of EvtVarTypeString
            EvtQueryStatuses = 1 // UInt32;   // Variant will be Array of EvtVarTypeUInt32
        /// Publisher Metadata properties.
        internal enum EvtPublisherMetadataPropertyId
            EvtPublisherMetadataPublisherGuid = 0,      // EvtVarTypeGuid
            EvtPublisherMetadataResourceFilePath = 1,       // EvtVarTypeString
            EvtPublisherMetadataParameterFilePath = 2,      // EvtVarTypeString
            EvtPublisherMetadataMessageFilePath = 3,        // EvtVarTypeString
            EvtPublisherMetadataHelpLink = 4,               // EvtVarTypeString
            EvtPublisherMetadataPublisherMessageID = 5,     // EvtVarTypeUInt32
            EvtPublisherMetadataChannelReferences = 6,      // EvtVarTypeEvtHandle, ObjectArray
            EvtPublisherMetadataChannelReferencePath = 7,   // EvtVarTypeString
            EvtPublisherMetadataChannelReferenceIndex = 8,  // EvtVarTypeUInt32
            EvtPublisherMetadataChannelReferenceID = 9,     // EvtVarTypeUInt32
            EvtPublisherMetadataChannelReferenceFlags = 10,  // EvtVarTypeUInt32
            EvtPublisherMetadataChannelReferenceMessageID = 11, // EvtVarTypeUInt32
            EvtPublisherMetadataLevels = 12,                 // EvtVarTypeEvtHandle, ObjectArray
            EvtPublisherMetadataLevelName = 13,              // EvtVarTypeString
            EvtPublisherMetadataLevelValue = 14,             // EvtVarTypeUInt32
            EvtPublisherMetadataLevelMessageID = 15,         // EvtVarTypeUInt32
            EvtPublisherMetadataTasks = 16,                  // EvtVarTypeEvtHandle, ObjectArray
            EvtPublisherMetadataTaskName = 17,               // EvtVarTypeString
            EvtPublisherMetadataTaskEventGuid = 18,          // EvtVarTypeGuid
            EvtPublisherMetadataTaskValue = 19,              // EvtVarTypeUInt32
            EvtPublisherMetadataTaskMessageID = 20,          // EvtVarTypeUInt32
            EvtPublisherMetadataOpcodes = 21,                // EvtVarTypeEvtHandle, ObjectArray
            EvtPublisherMetadataOpcodeName = 22,             // EvtVarTypeString
            EvtPublisherMetadataOpcodeValue = 23,            // EvtVarTypeUInt32
            EvtPublisherMetadataOpcodeMessageID = 24,        // EvtVarTypeUInt32
            EvtPublisherMetadataKeywords = 25,               // EvtVarTypeEvtHandle, ObjectArray
            EvtPublisherMetadataKeywordName = 26,            // EvtVarTypeString
            EvtPublisherMetadataKeywordValue = 27,           // EvtVarTypeUInt64
            EvtPublisherMetadataKeywordMessageID = 28        // EvtVarTypeUInt32
            // EvtPublisherMetadataPropertyIdEND
        internal enum EvtChannelReferenceFlags
            EvtChannelReferenceImported = 1
        internal enum EvtEventMetadataPropertyId
            EventMetadataEventID,        // EvtVarTypeUInt32
            EventMetadataEventVersion,   // EvtVarTypeUInt32
            EventMetadataEventChannel,   // EvtVarTypeUInt32
            EventMetadataEventLevel,     // EvtVarTypeUInt32
            EventMetadataEventOpcode,    // EvtVarTypeUInt32
            EventMetadataEventTask,      // EvtVarTypeUInt32
            EventMetadataEventKeyword,   // EvtVarTypeUInt64
            EventMetadataEventMessageID, // EvtVarTypeUInt32
            EventMetadataEventTemplate   // EvtVarTypeString
            // EvtEventMetadataPropertyIdEND
        // CHANNEL CONFIGURATION
        internal enum EvtChannelConfigPropertyId
            EvtChannelConfigEnabled = 0,            // EvtVarTypeBoolean
            EvtChannelConfigIsolation,              // EvtVarTypeUInt32, EVT_CHANNEL_ISOLATION_TYPE
            EvtChannelConfigType,                   // EvtVarTypeUInt32, EVT_CHANNEL_TYPE
            EvtChannelConfigOwningPublisher,        // EvtVarTypeString
            EvtChannelConfigClassicEventlog,        // EvtVarTypeBoolean
            EvtChannelConfigAccess,                 // EvtVarTypeString
            EvtChannelLoggingConfigRetention,       // EvtVarTypeBoolean
            EvtChannelLoggingConfigAutoBackup,      // EvtVarTypeBoolean
            EvtChannelLoggingConfigMaxSize,         // EvtVarTypeUInt64
            EvtChannelLoggingConfigLogFilePath,     // EvtVarTypeString
            EvtChannelPublishingConfigLevel,        // EvtVarTypeUInt32
            EvtChannelPublishingConfigKeywords,     // EvtVarTypeUInt64
            EvtChannelPublishingConfigControlGuid,  // EvtVarTypeGuid
            EvtChannelPublishingConfigBufferSize,   // EvtVarTypeUInt32
            EvtChannelPublishingConfigMinBuffers,   // EvtVarTypeUInt32
            EvtChannelPublishingConfigMaxBuffers,   // EvtVarTypeUInt32
            EvtChannelPublishingConfigLatency,      // EvtVarTypeUInt32
            EvtChannelPublishingConfigClockType,    // EvtVarTypeUInt32, EVT_CHANNEL_CLOCK_TYPE
            EvtChannelPublishingConfigSidType,      // EvtVarTypeUInt32, EVT_CHANNEL_SID_TYPE
            EvtChannelPublisherList,                // EvtVarTypeString | EVT_VARIANT_TYPE_ARRAY
            EvtChannelConfigPropertyIdEND
        // LOG INFORMATION
        internal enum EvtLogPropertyId
            EvtLogCreationTime = 0,             // EvtVarTypeFileTime
            EvtLogLastAccessTime,               // EvtVarTypeFileTime
            EvtLogLastWriteTime,                // EvtVarTypeFileTime
            EvtLogFileSize,                     // EvtVarTypeUInt64
            EvtLogAttributes,                   // EvtVarTypeUInt32
            EvtLogNumberOfLogRecords,           // EvtVarTypeUInt64
            EvtLogOldestRecordNumber,           // EvtVarTypeUInt64
            EvtLogFull,                         // EvtVarTypeBoolean
        internal enum EvtExportLogFlags
            EvtExportLogChannelPath = 1,
            EvtExportLogFilePath = 2,
            EvtExportLogTolerateQueryErrors = 0x1000
        // RENDERING
        internal enum EvtRenderContextFlags
            EvtRenderContextValues = 0,      // Render specific properties
            EvtRenderContextSystem = 1,      // Render all system properties (System)
            EvtRenderContextUser = 2         // Render all user properties (User/EventData)
        internal enum EvtRenderFlags
            EvtRenderEventValues = 0,       // Variants
            EvtRenderEventXml = 1,          // XML
            EvtRenderBookmark = 2           // Bookmark
        internal enum EvtFormatMessageFlags
            EvtFormatMessageEvent = 1,
            EvtFormatMessageLevel = 2,
            EvtFormatMessageTask = 3,
            EvtFormatMessageOpcode = 4,
            EvtFormatMessageKeyword = 5,
            EvtFormatMessageChannel = 6,
            EvtFormatMessageProvider = 7,
            EvtFormatMessageId = 8,
            EvtFormatMessageXml = 9
        internal enum EvtSystemPropertyId
            EvtSystemProviderName = 0,          // EvtVarTypeString
            EvtSystemProviderGuid,              // EvtVarTypeGuid
            EvtSystemEventID,                   // EvtVarTypeUInt16
            EvtSystemQualifiers,                // EvtVarTypeUInt16
            EvtSystemLevel,                     // EvtVarTypeUInt8
            EvtSystemTask,                      // EvtVarTypeUInt16
            EvtSystemOpcode,                    // EvtVarTypeUInt8
            EvtSystemKeywords,                  // EvtVarTypeHexInt64
            EvtSystemTimeCreated,               // EvtVarTypeFileTime
            EvtSystemEventRecordId,             // EvtVarTypeUInt64
            EvtSystemActivityID,                // EvtVarTypeGuid
            EvtSystemRelatedActivityID,         // EvtVarTypeGuid
            EvtSystemProcessID,                 // EvtVarTypeUInt32
            EvtSystemThreadID,                  // EvtVarTypeUInt32
            EvtSystemChannel,                   // EvtVarTypeString
            EvtSystemComputer,                  // EvtVarTypeString
            EvtSystemUserID,                    // EvtVarTypeSid
            EvtSystemVersion,                   // EvtVarTypeUInt8
            EvtSystemPropertyIdEND
        // SESSION
        internal enum EvtLoginClass
            EvtRpcLogin = 1
        internal struct EvtRpcLogin
            public string Server;
            public string User;
            public CoTaskMemUnicodeSafeHandle Password;
            public int Flags;
        // SEEK
        internal enum EvtSeekFlags
            EvtSeekRelativeToFirst = 1,
            EvtSeekRelativeToLast = 2,
            EvtSeekRelativeToCurrent = 3,
            EvtSeekRelativeToBookmark = 4,
            EvtSeekOriginMask = 7,
            EvtSeekStrict = 0x10000
        [DllImport(WEVTAPI, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        internal static extern EventLogHandle EvtQuery(
                            EventLogHandle session,
                            [MarshalAs(UnmanagedType.LPWStr)] string path,
                            [MarshalAs(UnmanagedType.LPWStr)] string query,
                            int flags);
        [DllImport(WEVTAPI, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool EvtSeek(
                            EventLogHandle resultSet,
                            long position,
                            EventLogHandle bookmark,
                            [MarshalAs(UnmanagedType.I4)] EvtSeekFlags flags
        internal static extern bool EvtNext(
                            EventLogHandle queryHandle,
                            int eventSize,
                            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] events,
                            int flags,
                            ref int returned);
        internal static extern bool EvtCancel(EventLogHandle handle);
        [DllImport(WEVTAPI)]
        internal static extern bool EvtClose(IntPtr handle);
        [DllImport(WEVTAPI, EntryPoint = "EvtClose", SetLastError = true)]
        public static extern bool EvtClose(
                            IntPtr eventHandle
        internal static extern bool EvtGetEventInfo(
                            EventLogHandle eventHandle,
                            // int propertyId
                            [MarshalAs(UnmanagedType.I4)] EvtEventPropertyId propertyId,
                            IntPtr bufferPtr,
                            out int bufferUsed
        internal static extern bool EvtGetQueryInfo(
                            [MarshalAs(UnmanagedType.I4)] EvtQueryPropertyId propertyId,
                            IntPtr buffer,
                            ref int bufferRequired
        // PUBLISHER METADATA
        internal static extern EventLogHandle EvtOpenPublisherMetadata(
                            [MarshalAs(UnmanagedType.LPWStr)] string publisherId,
                            [MarshalAs(UnmanagedType.LPWStr)] string logFilePath,
                            int locale,
                            int flags
        internal static extern bool EvtGetPublisherMetadataProperty(
                            EventLogHandle publisherMetadataHandle,
                            [MarshalAs(UnmanagedType.I4)] EvtPublisherMetadataPropertyId propertyId,
                            int publisherMetadataPropertyBufferSize,
                            IntPtr publisherMetadataPropertyBuffer,
                            out int publisherMetadataPropertyBufferUsed
        // NEW
        internal static extern bool EvtGetObjectArraySize(
                            EventLogHandle objectArray,
                            out int objectArraySize
        internal static extern bool EvtGetObjectArrayProperty(
                            int propertyId,
                            int arrayIndex,
                            int propertyValueBufferSize,
                            IntPtr propertyValueBuffer,
                            out int propertyValueBufferUsed
        // NEW 2
        internal static extern EventLogHandle EvtOpenEventMetadataEnum(
                            EventLogHandle publisherMetadata,
        // public static extern IntPtr EvtNextEventMetadata(
        internal static extern EventLogHandle EvtNextEventMetadata(
                            EventLogHandle eventMetadataEnum,
        internal static extern bool EvtGetEventMetadataProperty(
                            EventLogHandle eventMetadata,
                            [MarshalAs(UnmanagedType.I4)] EvtEventMetadataPropertyId propertyId,
                            int eventMetadataPropertyBufferSize,
                            IntPtr eventMetadataPropertyBuffer,
                            out int eventMetadataPropertyBufferUsed
        // Channel Configuration Native Api
        internal static extern EventLogHandle EvtOpenChannelEnum(
        internal static extern bool EvtNextChannelPath(
                            EventLogHandle channelEnum,
                            int channelPathBufferSize,
                            // StringBuilder channelPathBuffer,
                            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder channelPathBuffer,
                            out int channelPathBufferUsed
        internal static extern EventLogHandle EvtOpenPublisherEnum(
        internal static extern bool EvtNextPublisherId(
                            EventLogHandle publisherEnum,
                            int publisherIdBufferSize,
                            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder publisherIdBuffer,
                            out int publisherIdBufferUsed
        internal static extern EventLogHandle EvtOpenChannelConfig(
                            [MarshalAs(UnmanagedType.LPWStr)] string channelPath,
        internal static extern bool EvtSaveChannelConfig(
                            EventLogHandle channelConfig,
        internal static extern bool EvtSetChannelConfigProperty(
                            [MarshalAs(UnmanagedType.I4)] EvtChannelConfigPropertyId propertyId,
                            ref EvtVariant propertyValue
        internal static extern bool EvtGetChannelConfigProperty(
        // Log Information Native API
        internal static extern EventLogHandle EvtOpenLog(
                            [MarshalAs(UnmanagedType.I4)] PathType flags
        internal static extern bool EvtGetLogInfo(
                            EventLogHandle log,
                            [MarshalAs(UnmanagedType.I4)] EvtLogPropertyId propertyId,
        // LOG MANIPULATION
        internal static extern bool EvtExportLog(
                            [MarshalAs(UnmanagedType.LPWStr)] string targetFilePath,
        internal static extern bool EvtArchiveExportedLog(
        internal static extern bool EvtClearLog(
        internal static extern EventLogHandle EvtCreateRenderContext(
                            int valuePathsCount,
                            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)]
                                string[] valuePaths,
                            [MarshalAs(UnmanagedType.I4)] EvtRenderContextFlags flags
        internal static extern bool EvtRender(
                            EventLogHandle context,
                            EvtRenderFlags flags,
                            int buffSize,
                            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder buffer,
                            out int buffUsed,
                            out int propCount
        [DllImport(WEVTAPI, EntryPoint = "EvtRender", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        internal struct EvtStringVariant
            [MarshalAs(UnmanagedType.LPWStr), FieldOffset(0)]
            public string StringVal;
            public uint Count;
        internal static extern bool EvtFormatMessage(
                             uint messageId,
                             int valueCount,
                             EvtStringVariant[] values,
                             [MarshalAs(UnmanagedType.I4)] EvtFormatMessageFlags flags,
        [DllImport(WEVTAPI, EntryPoint = "EvtFormatMessage", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool EvtFormatMessageBuffer(
                             IntPtr values,
        internal static extern EventLogHandle EvtOpenSession(
                            [MarshalAs(UnmanagedType.I4)] EvtLoginClass loginClass,
                            ref EvtRpcLogin login,
        // BOOKMARK
        [DllImport(WEVTAPI, EntryPoint = "EvtCreateBookmark", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern EventLogHandle EvtCreateBookmark(
                            [MarshalAs(UnmanagedType.LPWStr)] string bookmarkXml
        internal static extern bool EvtUpdateBookmark(
                            EventLogHandle eventHandle
