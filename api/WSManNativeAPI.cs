    internal static class WSManNativeApi
        internal const uint INFINITE = 0xFFFFFFFF;
        internal const string PS_CREATION_XML_TAG = "creationXml";
        internal const string PS_CONNECT_XML_TAG = "connectXml";
        internal const string PS_CONNECTRESPONSE_XML_TAG = "connectResponseXml";
        internal const string PS_XML_NAMESPACE = "http://schemas.microsoft.com/powershell";
        internal const string WSMAN_STREAM_ID_STDOUT = "stdout";
        internal const string WSMAN_STREAM_ID_PROMPTRESPONSE = "pr";
        internal const string WSMAN_STREAM_ID_STDIN = "stdin";
        internal const string ResourceURIPrefix = @"http://schemas.microsoft.com/powershell/";
        internal const string NoProfile = "WINRS_NOPROFILE";
        internal const string CodePage = "WINRS_CODEPAGE";
        internal static readonly Version WSMAN_STACK_VERSION = new Version(3, 0);
        internal const int WSMAN_FLAG_REQUESTED_API_VERSION_1_1 = 1;
        // WSMan's default max env size in V2
        internal const int WSMAN_DEFAULT_MAX_ENVELOPE_SIZE_KB_V2 = 150;
        // WSMan's default max env size in V3
        internal const int WSMAN_DEFAULT_MAX_ENVELOPE_SIZE_KB_V3 = 500;
        #region WSMan errors
        /// The WinRM service cannot process the request because the request needs to be sent
        /// to a different machine.
        /// Use the redirect information to send the request to a new machine.
        /// 0x8033819B from sdk\inc\wsmerror.h.
        internal const int ERROR_WSMAN_REDIRECT_REQUESTED = -2144108135;
        /// The WS-Management service cannot process the request. The resource URI is missing or
        ///  it has an incorrect format. Check the documentation or use the following command for
        /// information on how to construct a resource URI: "winrm help uris".
        internal const int ERROR_WSMAN_INVALID_RESOURCE_URI = -2144108485;
        /// The WinRM service cannon re-connect the session because the session is no longer
        /// associated with this transportmanager object.
        internal const int ERROR_WSMAN_INUSE_CANNOT_RECONNECT = -2144108083;
        /// Sending data to a remote command failed with the following error message: The client
        /// cannot connect to the destination specified in the request. Verify that the service on
        /// the destination is running and is accepting requests. Consult the logs and documentation
        /// for the WS-Management service running on the destination, most commonly IIS or WinRM.
        /// If the destination is the WinRM service, run the following command on the destination to
        /// analyze and configure the WinRM service:
        internal const int ERROR_WSMAN_SENDDATA_CANNOT_CONNECT = -2144108526;
        /// Sending data to a remote command failed with the following error message: The WinRM client
        /// cannot complete the operation within the time specified. Check if the machine name is valid
        /// and is reachable over the network and firewall exception for Windows Remote Management service
        /// is enabled.
        internal const int ERROR_WSMAN_SENDDATA_CANNOT_COMPLETE = -2144108250;
        internal const int ERROR_WSMAN_ACCESS_DENIED = 5;
        internal const int ERROR_WSMAN_OUTOF_MEMORY = 14;
        internal const int ERROR_WSMAN_NETWORKPATH_NOTFOUND = 53;
        internal const int ERROR_WSMAN_OPERATION_ABORTED = 995;
        internal const int ERROR_WSMAN_SHUTDOWN_INPROGRESS = 1115;
        internal const int ERROR_WSMAN_AUTHENTICATION_FAILED = 1311;
        internal const int ERROR_WSMAN_NO_LOGON_SESSION_EXIST = 1312;
        internal const int ERROR_WSMAN_LOGON_FAILURE = 1326;
        internal const int ERROR_WSMAN_IMPROPER_RESPONSE = 1722;
        internal const int ERROR_WSMAN_INCORRECT_PROTOCOLVERSION = -2141974624;
        internal const int ERROR_WSMAN_URL_NOTAVAILABLE = -2144108269;
        internal const int ERROR_WSMAN_INVALID_AUTHENTICATION = -2144108274;
        internal const int ERROR_WSMAN_CANNOT_CONNECT_INVALID = -2144108080;
        internal const int ERROR_WSMAN_CANNOT_CONNECT_MISMATCH = -2144108090;
        internal const int ERROR_WSMAN_CANNOT_CONNECT_RUNASFAILED = -2144108065;
        internal const int ERROR_WSMAN_CREATEFAILED_INVALIDNAME = -2144108094;
        internal const int ERROR_WSMAN_TARGETSESSION_DOESNOTEXIST = -2144108453;
        internal const int ERROR_WSMAN_REMOTESESSION_DISALLOWED = -2144108116;
        internal const int ERROR_WSMAN_REMOTECONNECTION_DISALLOWED = -2144108061;
        internal const int ERROR_WSMAN_INVALID_RESOURCE_URI2 = -2144108542;
        internal const int ERROR_WSMAN_CORRUPTED_CONFIG = -2144108539;
        internal const int ERROR_WSMAN_URI_LIMIT = -2144108499;
        internal const int ERROR_WSMAN_CLIENT_KERBEROS_DISABLED = -2144108318;
        internal const int ERROR_WSMAN_SERVER_NOTTRUSTED = -2144108316;
        internal const int ERROR_WSMAN_WORKGROUP_NO_KERBEROS = -2144108276;
        internal const int ERROR_WSMAN_EXPLICIT_CREDENTIALS_REQUIRED = -2144108315;
        internal const int ERROR_WSMAN_REDIRECT_LOCATION_INVALID = -2144108105;
        internal const int ERROR_WSMAN_BAD_METHOD = -2144108428;
        internal const int ERROR_WSMAN_HTTP_SERVICE_UNAVAILABLE = -2144108270;
        internal const int ERROR_WSMAN_HTTP_SERVICE_ERROR = -2144108176;
        internal const int ERROR_WSMAN_COMPUTER_NOTFOUND = -2144108103;
        internal const int ERROR_WSMAN_TARGET_UNKNOWN = -2146893053;
        internal const int ERROR_WSMAN_CANNOTUSE_IP = -2144108101;
        #region MarshalledObject
        /// A struct holding marshalled data (IntPtr). This is
        /// created to supply IDisposable pattern to safely
        /// release the unmanaged pointer.
        internal struct MarshalledObject : IDisposable
            private IntPtr _dataPtr;
            /// Constructs a MarshalledObject with the supplied
            /// ptr.
            /// <param name="dataPtr"></param>
            internal MarshalledObject(IntPtr dataPtr)
                _dataPtr = dataPtr;
            /// Gets the unmanaged ptr.
            internal IntPtr DataPtr { get { return _dataPtr; } }
            /// Creates a MarshalledObject for the specified object.
            /// Must be a value type.
            /// <returns>MarshalledObject.</returns>
            internal static MarshalledObject Create<T>(T obj)
                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
                Marshal.StructureToPtr(obj, ptr, false);
                // Now create the MarshalledObject and return.
                MarshalledObject result = new MarshalledObject();
                result._dataPtr = ptr;
            /// Dispose the unmanaged IntPtr.
                if (_dataPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(_dataPtr);
                    _dataPtr = IntPtr.Zero;
            /// Implicit cast to IntPtr.
            public static implicit operator IntPtr(MarshalledObject obj)
                return obj._dataPtr;
        #region WSMan_Authentication_Credentials
        /// Different Authentication Mechanisms supported by WSMan.
        /// TODO: By the look of it, this appears like a Flags enum.
        /// Need to confirm the behavior with WSMan.
        /// Please keep in sync with WSManAuthenticationMechanism
        /// from C:\e\win7_powershell\admin\monad\nttargets\assemblies\logging\ETW\Manifests\Microsoft-Windows-PowerShell-Instrumentation.man
        internal enum WSManAuthenticationMechanism : int
            /// Use the default authentication.
            WSMAN_FLAG_DEFAULT_AUTHENTICATION = 0x0,
            /// Use no authentication for a remote operation.
            WSMAN_FLAG_NO_AUTHENTICATION = 0x1,
            WSMAN_FLAG_AUTH_DIGEST = 0x2,
            WSMAN_FLAG_AUTH_NEGOTIATE = 0x4,
            WSMAN_FLAG_AUTH_BASIC = 0x8,
            WSMAN_FLAG_AUTH_KERBEROS = 0x10,
            WSMAN_FLAG_AUTH_CLIENT_CERTIFICATE = 0x20,
            WSMAN_FLAG_AUTH_CREDSSP = 0x80,
        /// This is used to represent _WSMAN_AUTHENTICATION_CREDENTIALS
        /// native structure. _WSMAN_AUTHENTICATION_CREDENTIALS has a union
        /// member which cannot be easily represented in managed code.
        /// So created an interface and each union member is represented
        /// with a different structure.
        internal abstract class BaseWSManAuthenticationCredentials : IDisposable
            // used to get Marshalled data of the class.
            public abstract MarshalledObject GetMarshalledObject();
        /// Used to supply _WSMAN_USERNAME_PASSWORD_CREDS type credentials for
        /// WSManCreateSession.
        internal class WSManUserNameAuthenticationCredentials : BaseWSManAuthenticationCredentials
            internal struct WSManUserNameCredentialStruct
                internal WSManAuthenticationMechanism authenticationMechanism;
                internal string userName;
                /// Making password secure.
                internal IntPtr password;
            private WSManUserNameCredentialStruct _cred;
            private MarshalledObject _data;
            internal WSManUserNameAuthenticationCredentials()
                _cred = new WSManUserNameCredentialStruct();
                _data = MarshalledObject.Create<WSManUserNameCredentialStruct>(_cred);
            /// Constructs an WSManUserNameAuthenticationCredentials object.
            /// It is upto the caller to verify if <paramref name="name"/>
            /// and <paramref name="pwd"/> are valid. This API wont complain
            /// if they are Empty or Null.
            /// user name.
            /// <param name="pwd">
            /// password.
            /// <param name="authMechanism">
            /// can be 0 (the user did not specify an authentication mechanism,
            /// WSMan client will choose between Kerberos and Negotiate only);
            /// if it is not 0, it must be one of the values from
            /// WSManAuthenticationMechanism enumeration.
            internal WSManUserNameAuthenticationCredentials(string name,
                System.Security.SecureString pwd, WSManAuthenticationMechanism authMechanism)
                _cred.authenticationMechanism = authMechanism;
                _cred.userName = name;
                if (pwd != null)
                    _cred.password = Marshal.SecureStringToCoTaskMemUnicode(pwd);
            /// Gets a structure representation (used for marshalling)
            internal WSManUserNameCredentialStruct CredentialStruct
                get { return _cred; }
            /// Marshalled Data.
            public override MarshalledObject GetMarshalledObject()
                return _data;
            /// Dispose of the resources.
                if (_cred.password != IntPtr.Zero)
                    Marshal.ZeroFreeCoTaskMemUnicode(_cred.password);
                    _cred.password = IntPtr.Zero;
                _data.Dispose();
        internal class WSManCertificateThumbprintCredentials : BaseWSManAuthenticationCredentials
            private struct WSManThumbprintStruct
                internal string certificateThumbprint;
                /// This is provided for padding as underlying WSMan's implementation
                /// uses a union, we need to pad up unused fields.
                internal IntPtr reserved;
            /// Constructs an WSManCertificateThumbprintCredentials object.
            /// It is upto the caller to verify if <paramref name="thumbPrint"/>
            /// is valid. This API wont complain if it is Empty or Null.
            /// <param name="thumbPrint"></param>
            internal WSManCertificateThumbprintCredentials(string thumbPrint)
                WSManThumbprintStruct cred = new WSManThumbprintStruct();
                cred.authenticationMechanism = WSManAuthenticationMechanism.WSMAN_FLAG_AUTH_CLIENT_CERTIFICATE;
                cred.certificateThumbprint = thumbPrint;
                cred.reserved = IntPtr.Zero;
                _data = MarshalledObject.Create<WSManThumbprintStruct>(cred);
                // data is of struct type..so there is no need to set it to null..
        #region WSMan Session Options
        /// Enum representing native WSManSessionOption enum.
        internal enum WSManSessionOption : int
            #region TimeOuts
            /// Int - default timeout in ms that applies to all operations on the client side.
            WSMAN_OPTION_DEFAULT_OPERATION_TIMEOUTMS = 1,
            /// Int - Robust connection maximum retry time in minutes.
            WSMAN_OPTION_MAX_RETRY_TIME = 11,
            /// Int - timeout in ms for WSManCreateShellEx operations.
            WSMAN_OPTION_TIMEOUTMS_CREATE_SHELL = 12,
            /// Int - timeout in ms for WSManReceiveShellOutputEx operations.
            WSMAN_OPTION_TIMEOUTMS_RECEIVE_SHELL_OUTPUT = 14,
            /// Int - timeout in ms for WSManSendShellInputEx operations.
            WSMAN_OPTION_TIMEOUTMS_SEND_SHELL_INPUT = 15,
            /// Int - timeout in ms for WSManSignalShellEx operations.
            WSMAN_OPTION_TIMEOUTMS_SIGNAL_SHELL = 16,
            /// Int - timeout in ms for WSManCloseShellOperationEx operations.
            WSMAN_OPTION_TIMEOUTMS_CLOSE_SHELL_OPERATION = 17,
            #region Connection Options
            /// Int - 1 to not validate the CA on the server certificate; 0 - default.
            WSMAN_OPTION_SKIP_CA_CHECK = 18,
            /// Int - 1 to not validate the CN on the server certificate; 0 - default.
            WSMAN_OPTION_SKIP_CN_CHECK = 19,
            /// Int - 1 to not encrypt the messages; 0 - default.
            WSMAN_OPTION_UNENCRYPTED_MESSAGES = 20,
            /// Int - 1 Send all network packets for remote operations in UTF16; 0 - default is UTF8.
            WSMAN_OPTION_UTF16 = 21,
            /// Int - 1 When using negotiate, include port number in the connection SPN; 0 - default.
            WSMAN_OPTION_ENABLE_SPN_SERVER_PORT = 22,
            /// Int - Used when not talking to the main OS on a machine but, for instance, a BMC
            /// 1 Identify this machine to the server by including the MachineID header; 0 - default.
            WSMAN_OPTION_MACHINE_ID = 23,
            /// Int -1 Enables host process to be created with interactive token.
            WSMAN_OPTION_USE_INTERACTIVE_TOKEN = 34,
            #region Locale
            /// String - RFC 3066 language code.
            WSMAN_OPTION_LOCALE = 25,
            WSMAN_OPTION_UI_LANGUAGE = 26,
            #region Other
            /// Int - max SOAP envelope size (kb) - default 150kb from winrm config
            /// (see 'winrm help config' for more details); the client SOAP packet size cannot surpass
            /// this value; this value will be also sent to the server in the SOAP request as a
            /// MaxEnvelopeSize header; the server will use min(MaxEnvelopeSizeKb from server configuration,
            /// MaxEnvelopeSize value from SOAP).
            WSMAN_OPTION_MAX_ENVELOPE_SIZE_KB = 28,
            /// Int (read only) - max data size (kb) provided by the client, guaranteed by
            /// the winrm client implementation to fit into one SOAP packet; this is an
            /// approximate value calculated based on the WSMAN_OPTION_MAX_ENVELOPE_SIZE_KB (default 150kb),
            /// the maximum possible size of the SOAP headers and the overhead of the base64
            /// encoding which is specific to WSManSendShellInput API; this option can be used
            /// with WSManGetSessionOptionAsDword API; it cannot be used with WSManSetSessionOption API.
            WSMAN_OPTION_SHELL_MAX_DATA_SIZE_PER_MESSAGE_KB = 29,
            /// String -
            WSMAN_OPTION_REDIRECT_LOCATION = 30,
            /// DWORD  - 1 to not validate the revocation status on the server certificate; 0 - default.
            WSMAN_OPTION_SKIP_REVOCATION_CHECK = 31,
            /// DWORD  - 1 to allow default credentials for Negotiate (this is for SSL only); 0 - default.
            WSMAN_OPTION_ALLOW_NEGOTIATE_IMPLICIT_CREDENTIALS = 32,
            /// DWORD - When using just a machine name in the connection string use an SSL connection.
            /// 0 means HTTP, 1 means HTTPS.  Default is 0.
            WSMAN_OPTION_USE_SSL = 33
        /// Enum representing WSMan Shell specific options.
        internal enum WSManShellFlag : int
            /// Turn off compression for Send/Receive operations.  By default compression is
            /// turned on, but if communicating with a down-level box it may be necessary to
            /// do this.  Other reasons for turning it off is due to the extra memory consumption
            /// and CPU utilization that is used as a result of compression.
            WSMAN_FLAG_NO_COMPRESSION = 1,
            /// Enable the service to drop operation output when running disconnected.
            WSMAN_FLAG_SERVER_BUFFERING_MODE_DROP = 0x4,
            /// Enable the service to block operation progress when output buffers are full.
            WSMAN_FLAG_SERVER_BUFFERING_MODE_BLOCK = 0x8,
            /// Enable receive call to not immediately retrieve results. Only applicable for Receive calls on commands.
            WSMAN_FLAG_RECEIVE_DELAY_OUTPUT_STREAM = 0X10
        #region WSManData
        /// Types of supported WSMan data.
        /// PowerShell uses only Text and DWORD (in some places).
        internal enum WSManDataType : uint
            WSMAN_DATA_NONE = 0,
            WSMAN_DATA_TYPE_TEXT = 1,
            WSMAN_DATA_TYPE_BINARY = 2,
            WSMAN_DATA_TYPE_WS_XML_READER = 3,
            WSMAN_DATA_TYPE_DWORD = 4
        internal class WSManDataStruct
            internal uint type;
            internal WSManBinaryOrTextDataStruct binaryOrTextData;
        internal class WSManBinaryOrTextDataStruct
            internal int bufferLength;
            internal IntPtr data;
        /// Used to supply WSMAN_DATA_BINARY/WSMAN_DATA_TEXT type in place of _WSMAN_DATA.
        internal class WSManData_ManToUn : IDisposable
            private readonly WSManDataStruct _internalData;
            private IntPtr _marshalledObject = IntPtr.Zero;
            private IntPtr _marshalledBuffer = IntPtr.Zero;
            /// Constructs a WSMAN_DATA_BINARY object. This is used to send
            /// data to remote end.
            internal WSManData_ManToUn(byte[] data)
                Dbg.Assert(data != null, "Data cannot be null");
                _internalData = new WSManDataStruct();
                _internalData.binaryOrTextData = new WSManBinaryOrTextDataStruct();
                _internalData.binaryOrTextData.bufferLength = data.Length;
                _internalData.type = (uint)WSManDataType.WSMAN_DATA_TYPE_BINARY;
                IntPtr dataToSendPtr = Marshal.AllocHGlobal(_internalData.binaryOrTextData.bufferLength);
                _internalData.binaryOrTextData.data = dataToSendPtr;
                _marshalledBuffer = dataToSendPtr; // Stored directly to enable graceful clean up during finalizer scenarios
                Marshal.Copy(data, 0, _internalData.binaryOrTextData.data, _internalData.binaryOrTextData.bufferLength);
                _marshalledObject = Marshal.AllocHGlobal(Marshal.SizeOf<WSManDataStruct>());
                Marshal.StructureToPtr(_internalData, _marshalledObject, false);
            /// Constructs a WSMAN_DATA_TEXT object. This is used to send data
            /// to remote end.
            internal WSManData_ManToUn(string data)
                _internalData.type = (uint)WSManDataType.WSMAN_DATA_TYPE_TEXT;
                // marshal text data
                _internalData.binaryOrTextData.data = Marshal.StringToHGlobalUni(data);
                _marshalledBuffer = _internalData.binaryOrTextData.data; // Stored directly to enable graceful clean up during finalizer scenarios
            /// Gets the type of data.
            internal uint Type
                get { return _internalData.type; }
                set { _internalData.type = value; }
            /// Gets the buffer length of data.
            internal int BufferLength
                get { return _internalData.binaryOrTextData.bufferLength; }
                set { _internalData.binaryOrTextData.bufferLength = value; }
            /// Free unmanaged resources. All users of this class should call Dispose rather than
            /// depending on the finalizer to clean it up.
                // Managed objects should not be deleted when this is called via the finalizer
                // because they may have been collected already. To prevent leaking the marshalledBuffer
                // pointer, we are storing its value as a private member of the class, just like marshalledObject.
                if (_marshalledBuffer != IntPtr.Zero)
                    Marshal.FreeHGlobal(_marshalledBuffer);
                    _marshalledBuffer = IntPtr.Zero;
                if (_marshalledObject != IntPtr.Zero)
                    Marshal.FreeHGlobal(_marshalledObject);
                    _marshalledObject = IntPtr.Zero;
            /// Finalizes an instance of the <see cref="WSManData_ManToUn"/> class.
            ~WSManData_ManToUn()
            /// Implicit IntPtr conversion.
            public static implicit operator IntPtr(WSManData_ManToUn data)
                    return data._marshalledObject;
        internal class WSManData_UnToMan
            private uint _type;
            private int _bufferLength;
                get { return _bufferLength; }
                set { _bufferLength = value; }
            private string _text;
            internal string Text
                    if (this.Type == (uint)WSManDataType.WSMAN_DATA_TYPE_TEXT)
            private byte[] _data;
            internal byte[] Data
                    if (this.Type == (uint)WSManDataType.WSMAN_DATA_TYPE_BINARY)
                        return Array.Empty<byte>();
            /// Converts the unmanaged structure to a managed class object.
            /// <param name="dataStruct"></param>
            internal static WSManData_UnToMan UnMarshal(WSManDataStruct dataStruct)
                WSManData_UnToMan newData = new WSManData_UnToMan();
                newData._type = dataStruct.type;
                newData._bufferLength = dataStruct.binaryOrTextData.bufferLength;
                switch (dataStruct.type)
                    case (uint)WSManNativeApi.WSManDataType.WSMAN_DATA_TYPE_TEXT:
                        if (dataStruct.binaryOrTextData.bufferLength > 0)
                            string tempText = Marshal.PtrToStringUni(dataStruct.binaryOrTextData.data, dataStruct.binaryOrTextData.bufferLength);
                            newData._text = tempText;
                    case (uint)WSManNativeApi.WSManDataType.WSMAN_DATA_TYPE_BINARY:
                            // copy data from unmanaged heap to managed heap.
                            byte[] dataRecvd = new byte[dataStruct.binaryOrTextData.bufferLength];
                            Marshal.Copy(
                                dataStruct.binaryOrTextData.data,
                                dataRecvd,
                                dataStruct.binaryOrTextData.bufferLength);
                            newData._data = dataRecvd;
                return newData;
            /// Converts the unmanaged pointer to a managed class object.
            /// <param name="unmanagedData"></param>
            internal static WSManData_UnToMan UnMarshal(IntPtr unmanagedData)
                WSManData_UnToMan result = null;
                if (unmanagedData != IntPtr.Zero)
                    WSManDataStruct resultInternal = Marshal.PtrToStructure<WSManDataStruct>(unmanagedData);
                    result = WSManData_UnToMan.UnMarshal(resultInternal);
        /// Used to supply a DWORD data in place of _WSMAN_DATA.
        internal struct WSManDataDWord
            private readonly WSManDataType _type;
            private WSManDWordDataInternal _dwordData;
            /// Constructs a WSMAN_DATA_DWORD object.
            internal WSManDataDWord(int data)
                _dwordData = new WSManDWordDataInternal();
                _dwordData.number = data;
                _type = WSManDataType.WSMAN_DATA_TYPE_DWORD;
            /// Creates an unmanaged ptr which holds the class data.
            /// This unmanaged ptr can be used with WSMan native API.
            internal MarshalledObject Marshal()
                return MarshalledObject.Create<WSManDataDWord>(this);
            /// This struct is created to honor struct boundaries between
            /// x86,amd64 and ia64. WSMan defines a generic WSMAN_DATA
            /// structure that addresses DWORD, binary, text data.
            private struct WSManDWordDataInternal
                internal int number;
        #region WSManShellStartupInfo / WSManOptionSet / WSManCommandArgSet / WSManProxyInfo
        /// WSMan allows multiple streams within a shell but powershell is
        /// using only 1 stream for input and 1 stream for output to allow
        /// sequencing of data. Because of this the following structure will
        /// have only one string to hold stream information.
        internal struct WSManStreamIDSetStruct
            internal int streamIDsCount;
            internal IntPtr streamIDs;
        internal struct WSManStreamIDSet_ManToUn
            private WSManStreamIDSetStruct _streamSetInfo;
            /// <param name="streamIds"></param>
            internal WSManStreamIDSet_ManToUn(string[] streamIds)
                Dbg.Assert(streamIds != null, "stream ids cannot be null or empty");
                int sizeOfIntPtr = Marshal.SizeOf<IntPtr>();
                _streamSetInfo = new WSManStreamIDSetStruct();
                _streamSetInfo.streamIDsCount = streamIds.Length;
                _streamSetInfo.streamIDs = Marshal.AllocHGlobal(sizeOfIntPtr * streamIds.Length);
                for (int index = 0; index < streamIds.Length; index++)
                    IntPtr streamAddress = Marshal.StringToHGlobalUni(streamIds[index]);
                    Marshal.WriteIntPtr(_streamSetInfo.streamIDs, index * sizeOfIntPtr, streamAddress);
                _data = MarshalledObject.Create<WSManStreamIDSetStruct>(_streamSetInfo);
            /// Free resources.
                if (_streamSetInfo.streamIDs != IntPtr.Zero)
                    for (int index = 0; index < _streamSetInfo.streamIDsCount; index++)
                        IntPtr streamAddress = IntPtr.Zero;
                        streamAddress = Marshal.ReadIntPtr(_streamSetInfo.streamIDs, index * sizeOfIntPtr);
                        if (streamAddress != IntPtr.Zero)
                            Marshal.FreeHGlobal(streamAddress);
                            streamAddress = IntPtr.Zero;
                    Marshal.FreeHGlobal(_streamSetInfo.streamIDs);
                    _streamSetInfo.streamIDs = IntPtr.Zero;
            public static implicit operator IntPtr(WSManStreamIDSet_ManToUn obj)
                return obj._data.DataPtr;
        internal class WSManStreamIDSet_UnToMan
            internal string[] streamIDs;
            internal static WSManStreamIDSet_UnToMan UnMarshal(IntPtr unmanagedData)
                WSManStreamIDSet_UnToMan result = null;
                    WSManStreamIDSetStruct resultInternal = Marshal.PtrToStructure<WSManStreamIDSetStruct>(unmanagedData);
                    result = new WSManStreamIDSet_UnToMan();
                    string[] idsArray = null;
                    if (resultInternal.streamIDsCount > 0)
                        idsArray = new string[resultInternal.streamIDsCount];
                        IntPtr[] ptrs = new IntPtr[resultInternal.streamIDsCount];
                        Marshal.Copy(resultInternal.streamIDs, ptrs, 0, resultInternal.streamIDsCount); // Marshal the array of string pointers
                        for (int i = 0; i < resultInternal.streamIDsCount; i++)
                            idsArray[i] = Marshal.PtrToStringUni(ptrs[i]); // Marshal the string pointers into strings
                         * // TODO: Why didn't this work? It looks more efficient
                        int sizeInBytes = Marshal.SizeOf<IntPtr>();
                        IntPtr perElementPtr = resultInternal.streamIDs;
                            IntPtr p = IntPtr.Add(perElementPtr, (i * sizeInBytes));
                            idsArray[i] = Marshal.PtrToStringUni(p);
                    result.streamIDs = idsArray;
                    result.streamIDsCount = resultInternal.streamIDsCount;
        /// Managed to Unmanaged: Option struct used to pass optional information with WSManCreateShellEx .
        /// Unmanaged to Managed: Included in WSManPluginRequest.
        internal struct WSManOption
            /// Underlying type = PCWSTR.
            internal string value;
            /// Underlying type = BOOL.
            internal bool mustComply;
        /// Unmanaged to Managed: WSMAN_OPERATION_INFO includes the struct directly, so this cannot be made internal to WsmanOptionSet.
        internal struct WSManOptionSetStruct
            internal int optionsCount;
            /// Pointer to an array of WSManOption objects.
            internal IntPtr options;
            internal bool optionsMustUnderstand;
        /// Option set struct used to pass optional information
        /// with WSManCreateShellEx.
        internal struct WSManOptionSet : IDisposable
            #region Managed to Unmanaged
            private WSManOptionSetStruct _optionSet;
            /// Options to construct this OptionSet with.
            internal WSManOptionSet(WSManOption[] options)
                Dbg.Assert(options != null, "options cannot be null");
                int sizeOfOption = Marshal.SizeOf<WSManOption>();
                _optionSet = new WSManOptionSetStruct();
                _optionSet.optionsCount = options.Length;
                _optionSet.optionsMustUnderstand = true;
                _optionSet.options = Marshal.AllocHGlobal(sizeOfOption * options.Length);
                for (int index = 0; index < options.Length; index++)
                    // Look at the structure of native WSManOptionSet.. Options is a pointer..
                    // In C-Style array individual elements are continuous..so I am building
                    // continuous array elements here.
                    Marshal.StructureToPtr(options[index], (IntPtr)(_optionSet.options.ToInt64() + (sizeOfOption * index)), false);
                _data = MarshalledObject.Create<WSManOptionSetStruct>(_optionSet);
                // For conformity:
                this.optionsCount = 0;
                this.options = null;
                this.optionsMustUnderstand = false;
                if (_optionSet.options != IntPtr.Zero)
                    Marshal.FreeHGlobal(_optionSet.options);
                    _optionSet.options = IntPtr.Zero;
                // dispose option set
            /// Implicit IntPtr cast.
            /// <param name="optionSet"></param>
            public static implicit operator IntPtr(WSManOptionSet optionSet)
                return optionSet._data.DataPtr;
            #region Unmanaged to Managed
            internal WSManOption[] options;
            internal static WSManOptionSet UnMarshal(IntPtr unmanagedData)
                if (unmanagedData == IntPtr.Zero)
                    return new WSManOptionSet();
                    WSManOptionSetStruct resultInternal = Marshal.PtrToStructure<WSManOptionSetStruct>(unmanagedData);
                    return UnMarshal(resultInternal);
            /// <param name="resultInternal"></param>
            internal static WSManOptionSet UnMarshal(WSManOptionSetStruct resultInternal)
                WSManOption[] tempOptions = null;
                if (resultInternal.optionsCount > 0)
                    tempOptions = new WSManOption[resultInternal.optionsCount];
                    int sizeInBytes = Marshal.SizeOf<WSManOption>();
                    IntPtr perElementPtr = resultInternal.options;
                    for (int i = 0; i < resultInternal.optionsCount; i++)
                        tempOptions[i] = Marshal.PtrToStructure<WSManOption>(p);
                WSManOptionSet result = new WSManOptionSet();
                result.optionsCount = resultInternal.optionsCount;
                result.options = tempOptions;
                result.optionsMustUnderstand = resultInternal.optionsMustUnderstand;
        internal struct WSManCommandArgSet : IDisposable
            internal struct WSManCommandArgSetInternal
                internal int argsCount;
                internal IntPtr args;
            private WSManCommandArgSetInternal _internalData;
            internal WSManCommandArgSet(byte[] firstArgument)
                _internalData = new WSManCommandArgSetInternal();
                _internalData.argsCount = 1;
                _internalData.args = Marshal.AllocHGlobal(Marshal.SizeOf<IntPtr>());
                // argument set takes only strings..but powershell's serialized pipeline might contain
                // \0 (null characters) which are unacceptable in WSMan. So we are converting to Base64
                // here. The server will convert this back to original string.
                string base64EncodedArgument = Convert.ToBase64String(firstArgument);
                IntPtr firstArgAddress = Marshal.StringToHGlobalUni(base64EncodedArgument);
                Marshal.WriteIntPtr(_internalData.args, firstArgAddress);
                _data = MarshalledObject.Create<WSManCommandArgSet.WSManCommandArgSetInternal>(_internalData);
                this.args = null;
                this.argsCount = 0;
                IntPtr firstArgAddress = Marshal.ReadIntPtr(_internalData.args);
                if (firstArgAddress != IntPtr.Zero)
                    Marshal.FreeHGlobal(firstArgAddress);
                Marshal.FreeHGlobal(_internalData.args);
            public static implicit operator IntPtr(WSManCommandArgSet obj)
            internal string[] args;
            /// Since this is a structure, it must be non-null. This differs in behavior from all the other UnMarshals
            /// that are classes since they can be null.
            /// TODO: Do I need to worry about intermediate null characters in the arguments? The managed to unmanaged does!
            internal static WSManCommandArgSet UnMarshal(IntPtr unmanagedData)
                WSManCommandArgSet result = new WSManCommandArgSet();
                    WSManCommandArgSetInternal resultInternal = Marshal.PtrToStructure<WSManCommandArgSetInternal>(unmanagedData);
                    string[] tempArgs = null;
                    if (resultInternal.argsCount > 0)
                        tempArgs = new string[resultInternal.argsCount];
                        IntPtr[] ptrs = new IntPtr[resultInternal.argsCount];
                        Marshal.Copy(resultInternal.args, ptrs, 0, resultInternal.argsCount); // Marshal the array of string pointers
                        for (int i = 0; i < resultInternal.argsCount; i++)
                            tempArgs[i] = Marshal.PtrToStringUni(ptrs[i]); // Marshal the string pointers into strings
                    result.argsCount = resultInternal.argsCount;
                    result.args = tempArgs;
        internal struct WSManShellDisconnectInfo : IDisposable
            private struct WSManShellDisconnectInfoInternal
                /// New idletimeout for the server shell that overrides the original idletimeout specified in WSManCreateShell.
                internal uint idleTimeoutMs;
            private WSManShellDisconnectInfoInternal _internalInfo;
            internal MarshalledObject data;
            #region Constructor / Other methods
            internal WSManShellDisconnectInfo(uint serverIdleTimeOut)
                _internalInfo = new WSManShellDisconnectInfoInternal();
                _internalInfo.idleTimeoutMs = serverIdleTimeOut;
                data = MarshalledObject.Create<WSManShellDisconnectInfoInternal>(_internalInfo);
            /// Disposes the object.
                data.Dispose();
            /// Implicit IntPtr.
            /// <param name="disconnectInfo"></param>
            public static implicit operator IntPtr(WSManShellDisconnectInfo disconnectInfo)
                return disconnectInfo.data.DataPtr;
        internal struct WSManShellStartupInfoStruct
            /// PowerShell always uses one stream. So no need to expand this.
            /// Maps to WSManStreamIDSet.
            internal IntPtr inputStreamSet;
            internal IntPtr outputStreamSet;
            /// Idle timeout.
            /// Working directory of the shell.
            internal string workingDirectory;
            /// Environment variables available to the shell.
            /// Maps to WSManEnvironmentVariableSet.
            internal IntPtr environmentVariableSet;
        /// Managed to unmanaged representation of WSMAN_SHELL_STARTUP_INFO.
        /// It converts managed values into an unmanaged compatible WSManShellStartupInfoStruct that
        /// is marshalled into unmanaged memory.
        internal struct WSManShellStartupInfo_ManToUn : IDisposable
            private WSManShellStartupInfoStruct _internalInfo;
            /// Creates a startup info with 1 startup option.
            /// The startup option is intended to specify the version.
            /// <param name="inputStreamSet">
            /// <param name="outputStreamSet">
            /// <param name="serverIdleTimeOut">
            internal WSManShellStartupInfo_ManToUn(WSManStreamIDSet_ManToUn inputStreamSet, WSManStreamIDSet_ManToUn outputStreamSet, uint serverIdleTimeOut, string name)
                _internalInfo = new WSManShellStartupInfoStruct();
                _internalInfo.inputStreamSet = inputStreamSet;
                _internalInfo.outputStreamSet = outputStreamSet;
                // WSMan uses %USER_PROFILE% as the default working directory.
                _internalInfo.workingDirectory = null;
                _internalInfo.environmentVariableSet = IntPtr.Zero;
                _internalInfo.name = name;
                data = MarshalledObject.Create<WSManShellStartupInfoStruct>(_internalInfo);
            /// <param name="startupInfo"></param>
            public static implicit operator IntPtr(WSManShellStartupInfo_ManToUn startupInfo)
                return startupInfo.data.DataPtr;
        /// Unmanaged to managed representation of WSMAN_SHELL_STARTUP_INFO.
        /// It unmarshals the unmanaged struct into this object for use by managed code.
        internal class WSManShellStartupInfo_UnToMan
            internal WSManStreamIDSet_UnToMan inputStreamSet;
            internal WSManStreamIDSet_UnToMan outputStreamSet;
            internal uint idleTimeoutMS;
            internal WSManEnvironmentVariableSet environmentVariableSet;
            internal static WSManShellStartupInfo_UnToMan UnMarshal(IntPtr unmanagedData)
                WSManShellStartupInfo_UnToMan result = null;
                    WSManShellStartupInfoStruct resultInternal = Marshal.PtrToStructure<WSManShellStartupInfoStruct>(unmanagedData);
                    result = new WSManShellStartupInfo_UnToMan();
                    result.inputStreamSet = WSManStreamIDSet_UnToMan.UnMarshal(resultInternal.inputStreamSet);
                    result.outputStreamSet = WSManStreamIDSet_UnToMan.UnMarshal(resultInternal.outputStreamSet);
                    result.idleTimeoutMS = resultInternal.idleTimeoutMs;
                    result.workingDirectory = resultInternal.workingDirectory; // TODO: Special marshaling required here?
                    result.environmentVariableSet = WSManEnvironmentVariableSet.UnMarshal(resultInternal.environmentVariableSet);
                    result.name = resultInternal.name;
        /// Managed representation of WSMAN_ENVIRONMENT_VARIABLE_SET.
        /// It wraps WSManEnvironmentVariableSetInternal and UnMarshals the unmanaged
        /// data into the object.
        internal class WSManEnvironmentVariableSet
            internal uint varsCount;
            internal WSManEnvironmentVariableInternal[] vars;
            internal static WSManEnvironmentVariableSet UnMarshal(IntPtr unmanagedData)
                WSManEnvironmentVariableSet result = null;
                    WSManEnvironmentVariableSetInternal resultInternal = Marshal.PtrToStructure<WSManEnvironmentVariableSetInternal>(unmanagedData);
                    result = new WSManEnvironmentVariableSet();
                    WSManEnvironmentVariableInternal[] varsArray = null;
                    if (resultInternal.varsCount > 0)
                        varsArray = new WSManEnvironmentVariableInternal[resultInternal.varsCount];
                        int sizeInBytes = Marshal.SizeOf<WSManEnvironmentVariableInternal>();
                        IntPtr perElementPtr = resultInternal.vars;
                        for (int i = 0; i < resultInternal.varsCount; i++)
                            varsArray[i] = Marshal.PtrToStructure<WSManEnvironmentVariableInternal>(p);
                    result.vars = varsArray;
                    result.varsCount = resultInternal.varsCount;
            private struct WSManEnvironmentVariableSetInternal
                internal IntPtr vars; // Array of WSManEnvironmentVariableInternal structs
            internal struct WSManEnvironmentVariableInternal
        /// Proxy Info used with WSManCreateSession.
        internal class WSManProxyInfo : IDisposable
            private struct WSManProxyInfoInternal
                public int proxyAccessType;
                public WSManUserNameAuthenticationCredentials.WSManUserNameCredentialStruct proxyAuthCredentialsStruct;
            /// <param name="proxyAccessType"></param>
            /// <param name="authCredentials"></param>
            internal WSManProxyInfo(ProxyAccessType proxyAccessType,
                WSManUserNameAuthenticationCredentials authCredentials)
                WSManProxyInfoInternal internalInfo = new WSManProxyInfoInternal();
                internalInfo.proxyAccessType = (int)proxyAccessType;
                internalInfo.proxyAuthCredentialsStruct = new WSManUserNameAuthenticationCredentials.WSManUserNameCredentialStruct();
                internalInfo.proxyAuthCredentialsStruct.authenticationMechanism = WSManAuthenticationMechanism.WSMAN_FLAG_DEFAULT_AUTHENTICATION;
                if (authCredentials != null)
                    internalInfo.proxyAuthCredentialsStruct = authCredentials.CredentialStruct;
                _data = MarshalledObject.Create<WSManProxyInfoInternal>(internalInfo);
                // data is of struct type..so no need to set it to null.
            /// <param name="proxyInfo"></param>
            public static implicit operator IntPtr(WSManProxyInfo proxyInfo)
                return proxyInfo._data.DataPtr;
        #region WSMan Shell Async
        /// Flags used by all callback functions: WSMAN_COMPLETION_FUNCTION,
        /// WSMAN_SUBSCRIPTION_COMPLETION_FUNCTION and WSMAN_SHELL_COMPLETION_FUNCTION.
        internal enum WSManCallbackFlags
            // Flag that marks the end of any single step of multistep operation
            WSMAN_FLAG_CALLBACK_END_OF_OPERATION = 0x1,
            // WSMAN_SHELL_COMPLETION_FUNCTION API specific flags
            //  end of a particular stream; it is used for optimization purposes if the shell
            //  knows that no more output will occur for this stream; in some conditions this
            //  cannot be determined.
            WSMAN_FLAG_CALLBACK_END_OF_STREAM = 0x8,
            // Flag that if present on CreateShell callback indicates that it supports disconnect
            WSMAN_FLAG_CALLBACK_SHELL_SUPPORTS_DISCONNECT = 0x20,
            // Marks the end of an auto-disconnect operation.
            WSMAN_FLAG_CALLBACK_SHELL_AUTODISCONNECTED = 0x40,
            // Network failure notification.
            WSMAN_FLAG_CALLBACK_NETWORK_FAILURE_DETECTED = 0x100,
            // Network connection retry notification.
            WSMAN_FLAG_CALLBACK_RETRYING_AFTER_NETWORK_FAILURE = 0x200,
            // Network retry succeeded, connection re-established notification.
            WSMAN_FLAG_CALLBACK_RECONNECTED_AFTER_NETWORK_FAILURE = 0x400,
            // Retries failed, now auto-disconnecting.
            WSMAN_FLAG_CALLBACK_SHELL_AUTODISCONNECTING = 0x800,
            // Internal error during retries.  Cannot auto-disconnect.  Shell failure.
            WSMAN_FLAG_CALLBACK_RETRY_ABORTED_DUE_TO_INTERNAL_ERROR = 0x1000,
            // Flag that indicates for a receive operation that a delay stream request has been processed
            WSMAN_FLAG_RECEIVE_DELAY_STREAM_REQUEST_PROCESSED = 0X2000
        /// Completion function used by all Shell functions. Returns error->code != 0 upon error;
        /// use error->errorDetail structure for extended error informations; the callback is
        /// called for each shell operation; after a WSManReceiveShellOutput operation is initiated,
        /// the callback is called for each output stream element or if error; the underlying
        /// implementation handles the polling of stream data from the command or shell.
        /// If WSMAN_COMMAND_STATE_DONE state is received, no more streams will be received from the command,
        /// so the command can be closed using WSManCloseShellOperationEx(command).
        /// If error->code != 0, the result is guaranteed to be NULL. The error and result objects are
        /// allocated and owned by the WSMan client stack; they are valid during the callback only; the user
        /// has to synchronously copy the data in the callback. This callback function will use the current
        /// access token, whether it is a process or impersonation token.
        /// <param name="operationContext">
        /// user supplied operation context.
        /// one or more flags from WSManCallbackFlags
        /// <param name="error">
        /// error allocated and owned by the winrm stack; valid in the callback only;
        /// <param name="shellOperationHandle">
        /// shell handle associated with the user context
        /// <param name="commandOperationHandle">
        /// command handle associated with the user context
        /// <param name="operationHandle">
        /// operation handle associated with the user context
        /// output data from command/shell; allocated internally and owned by the winrm stack.
        /// valid only within this function.
        /// See WSManReceiveDataResult.
        internal delegate void WSManShellCompletionFunction(
            IntPtr operationContext,
            IntPtr error,
            IntPtr shellOperationHandle,
            IntPtr commandOperationHandle,
            IntPtr operationHandle,
            IntPtr data
        /// Struct which holds reference to the callback(delegate) passed to WSMan
        /// API.
        internal struct WSManShellAsyncCallback
            // GC handle which prevents garbage collector from collecting this delegate.
            private readonly IntPtr _asyncCallback;
            internal WSManShellAsyncCallback(WSManShellCompletionFunction callback)
                // if a delegate is re-located by a garbage collection, it will not affect
                // the underlaying managed callback, so Alloc is used to add a reference
                // to the delegate, allowing relocation of the delegate, but preventing
                // disposal. Using GCHandle without pinning reduces fragmentation potential
                // of the managed heap.
                _gcHandle = GCHandle.Alloc(callback);
                _asyncCallback = Marshal.GetFunctionPointerForDelegate(callback);
            public static implicit operator IntPtr(WSManShellAsyncCallback callback)
                return callback._asyncCallback;
        /// Used in different WSMan functions to supply async callback.
        internal class WSManShellAsync
            internal struct WSManShellAsyncInternal
                internal IntPtr operationContext;
                internal IntPtr asyncCallback;
            private WSManShellAsyncInternal _internalData;
            internal WSManShellAsync(IntPtr context, WSManShellAsyncCallback callback)
                _internalData = new WSManShellAsyncInternal();
                _internalData.operationContext = context;
                _internalData.asyncCallback = callback;
                _data = MarshalledObject.Create<WSManShellAsyncInternal>(_internalData);
            public static implicit operator IntPtr(WSManShellAsync async)
                return async._data;
        /// Used in the shell completion function delegate to refer to error.
        internal struct WSManError
            internal int errorCode;
            /// Extended error description from the fault;
            internal string errorDetail;
            /// Language for error description (RFC 3066 language code); it can be NULL.
            internal string language;
            /// Machine id; it can be NULL.
            internal string machineName;
            /// Constructs a WSManError from the unmanaged pointer.
            /// This involves copying data from unmanaged memory to managed heap.
            /// <param name="unmanagedData">
            /// Pointer to unmanaged data.
            internal static WSManError UnMarshal(IntPtr unmanagedData)
                return Marshal.PtrToStructure<WSManError>(unmanagedData);
        internal class WSManCreateShellDataResult
            private struct WSManCreateShellDataResultInternal
                internal WSManDataStruct data;
            internal string data;
            internal static WSManCreateShellDataResult UnMarshal(IntPtr unmanagedData)
                WSManCreateShellDataResult result = new WSManCreateShellDataResult();
                    WSManCreateShellDataResultInternal resultInternal = Marshal.PtrToStructure<WSManCreateShellDataResultInternal>(unmanagedData);
                    string connectData = null;
                    if (resultInternal.data.textData.textLength > 0)
                        connectData = Marshal.PtrToStringUni(resultInternal.data.textData.text, resultInternal.data.textData.textLength);
                    result.data = connectData;
            // The following structures are created to honor struct boundaries
            // on x86,amd64 and ia64
            private struct WSManDataStruct
                internal WSManTextDataInternal textData;
            private struct WSManTextDataInternal
                internal int textLength;
                internal IntPtr text;
        internal class WSManConnectDataResult
            private struct WSManConnectDataResultInternal
            internal static WSManConnectDataResult UnMarshal(IntPtr unmanagedData)
                WSManConnectDataResultInternal resultInternal = Marshal.PtrToStructure<WSManConnectDataResultInternal>(unmanagedData);
                WSManConnectDataResult result = new WSManConnectDataResult();
        /// Used in the shell completion function delegate to refer to the data.
        internal class WSManReceiveDataResult
            /// The actual data.
            internal byte[] data;
            /// Stream the data belongs to.
            internal string stream;
            /// Constructs a WSManReceiveDataResult from the unmanaged pointer.
            /// Currently PowerShell supports only binary data on the wire, so this
            /// method asserts if the data is not binary.
            internal static WSManReceiveDataResult UnMarshal(IntPtr unmanagedData)
                WSManReceiveDataResultInternal result1 =
                    Marshal.PtrToStructure<WSManReceiveDataResultInternal>(unmanagedData);
                byte[] dataRecvd = null;
                if (result1.data.binaryData.bufferLength > 0)
                    dataRecvd = new byte[result1.data.binaryData.bufferLength];
                    Marshal.Copy(result1.data.binaryData.buffer,
                        result1.data.binaryData.bufferLength);
                Dbg.Assert(result1.data.type == (uint)WSManDataType.WSMAN_DATA_TYPE_BINARY,
                    "ReceiveDataResult can receive only binary data");
                WSManReceiveDataResult result = new WSManReceiveDataResult();
                result.data = dataRecvd;
                result.stream = result1.streamId;
            private struct WSManReceiveDataResultInternal
                internal string streamId;
                internal string commandState;
                internal int exitCode;
                internal WSManBinaryDataInternal binaryData;
            private struct WSManBinaryDataInternal
                internal IntPtr buffer;
        #region Plugin API Structure Definitions
        /// This is the managed representation of the WSMAN_PLUGIN_REQUEST struct.
        internal class WSManPluginRequest
            /// Unmarshalled WSMAN_SENDER_DETAILS struct.
            internal WSManSenderDetails senderDetails;
            internal string locale;
            internal string resourceUri;
            /// Unmarshalled WSMAN_OPERATION_INFO struct.
            internal WSManOperationInfo operationInfo;
            /// Kept around to allow direct access to shutdownNotification and its handle.
            private WSManPluginRequestInternal _internalDetails;
            /// Volatile value that should be read directly from its unmanaged location.
            /// TODO: Does "volatile" still apply when accessing it in managed code.
            internal bool shutdownNotification
                get { return _internalDetails.shutdownNotification; }
            /// Left untouched in unmanaged memory because it is passed directly to
            /// RegisterWaitForSingleObject().
            internal IntPtr shutdownNotificationHandle
                get { return _internalDetails.shutdownNotificationHandle; }
            /// Copy of the unmanagedData value used to create the structure.
            internal IntPtr unmanagedHandle;
            internal static WSManPluginRequest UnMarshal(IntPtr unmanagedData)
                // Dbg.Assert(IntPtr.Zero != unmanagedData, "unmanagedData must be non-null. This means WinRM sent a bad pointer.");
                WSManPluginRequest result = null;
                    WSManPluginRequestInternal resultInternal = Marshal.PtrToStructure<WSManPluginRequestInternal>(unmanagedData);
                    result = new WSManPluginRequest();
                    result.senderDetails = WSManSenderDetails.UnMarshal(resultInternal.senderDetails);
                    result.locale = resultInternal.locale;
                    result.resourceUri = resultInternal.resourceUri;
                    result.operationInfo = WSManOperationInfo.UnMarshal(resultInternal.operationInfo);
                    result._internalDetails = resultInternal;
                    result.unmanagedHandle = unmanagedData;
            /// Representation of WSMAN_PLUGIN_REQUEST.
            private struct WSManPluginRequestInternal
                /// WSManSenderDetails.
                internal IntPtr senderDetails;
                /// WSManOperationInfo.
                internal IntPtr operationInfo;
                internal bool shutdownNotification;
                internal IntPtr shutdownNotificationHandle;
        internal class WSManSenderDetails
            internal string senderName;
            internal string authenticationMechanism;
            internal WSManCertificateDetails certificateDetails;
            internal IntPtr clientToken; // TODO: How should this be marshalled?????
            internal string httpUrl;
            internal static WSManSenderDetails UnMarshal(IntPtr unmanagedData)
                WSManSenderDetails result = null;
                    WSManSenderDetailsInternal resultInternal = Marshal.PtrToStructure<WSManSenderDetailsInternal>(unmanagedData);
                    result = new WSManSenderDetails();
                    result.senderName = resultInternal.senderName;
                    result.authenticationMechanism = resultInternal.authenticationMechanism;
                    result.certificateDetails = WSManCertificateDetails.UnMarshal(resultInternal.certificateDetails);
                    result.clientToken = resultInternal.clientToken; // TODO: UnMarshaling needed here!!!!
                    result.httpUrl = resultInternal.httpUrl;
            /// Managed representation of WSMAN_SENDER_DETAILS.
            private struct WSManSenderDetailsInternal
                /// WSManCertificateDetails.
                internal IntPtr certificateDetails;
                internal IntPtr clientToken;
        internal class WSManCertificateDetails
            internal string subject;
            internal string issuerName;
            internal string issuerThumbprint;
            internal string subjectName;
            internal static WSManCertificateDetails UnMarshal(IntPtr unmanagedData)
                WSManCertificateDetails result = null;
                    WSManCertificateDetailsInternal resultInternal = Marshal.PtrToStructure<WSManCertificateDetailsInternal>(unmanagedData);
                    result = new WSManCertificateDetails();
                    result.subject = resultInternal.subject;
                    result.issuerName = resultInternal.issuerName;
                    result.issuerThumbprint = resultInternal.issuerThumbprint;
                    result.subjectName = resultInternal.subjectName;
            /// Managed representation of WSMAN_CERTIFICATE_DETAILS.
            private struct WSManCertificateDetailsInternal
        internal class WSManOperationInfo
            internal WSManFragmentInternal fragment;
            internal WSManFilterInternal filter;
            internal WSManSelectorSet selectorSet;
            internal WSManOptionSet optionSet;
            internal static WSManOperationInfo UnMarshal(IntPtr unmanagedData)
                WSManOperationInfo result = null;
                    WSManOperationInfoInternal resultInternal = Marshal.PtrToStructure<WSManOperationInfoInternal>(unmanagedData);
                    result = new WSManOperationInfo();
                    result.fragment = resultInternal.fragment;
                    result.filter = resultInternal.filter;
                    result.selectorSet = WSManSelectorSet.UnMarshal(resultInternal.selectorSet);
                    result.optionSet = WSManOptionSet.UnMarshal(resultInternal.optionSet);
            /// Managed representation of WSMAN_OPERATION_INFO.
            /// selectorSet and optionSet are handled differently because they are structs that contain pointers to arrays of structs.
            /// Most other data structures in the API point to structures using IntPtr rather than including the actual structure.
            private struct WSManOperationInfoInternal
                internal WSManSelectorSet.WSManSelectorSetStruct selectorSet;
                internal WSManOptionSetStruct optionSet;
            /// Managed representation of WSMAN_FRAGMENT.
            internal struct WSManFragmentInternal
                internal string path;
                internal string dialect;
            /// Managed representation of WSMAN_FILTER.
            internal struct WSManFilterInternal
                internal string filter;
        internal class WSManSelectorSet
            internal int numberKeys;
            internal WSManKeyStruct[] keys;
            internal static WSManSelectorSet UnMarshal(WSManSelectorSetStruct resultInternal)
                WSManKeyStruct[] tempKeys = null;
                if (resultInternal.numberKeys > 0)
                    tempKeys = new WSManKeyStruct[resultInternal.numberKeys];
                    int sizeInBytes = Marshal.SizeOf<WSManKeyStruct>();
                    IntPtr perElementPtr = resultInternal.keys;
                    for (int i = 0; i < resultInternal.numberKeys; i++)
                        tempKeys[i] = Marshal.PtrToStructure<WSManKeyStruct>(p);
                WSManSelectorSet result = new WSManSelectorSet();
                result.numberKeys = resultInternal.numberKeys;
                result.keys = tempKeys;
            /// Managed representation of WSMAN_SELECTOR_SET.
            internal struct WSManSelectorSetStruct
                /// Array of WSManKeyStruct structures.
                internal IntPtr keys;
            /// Managed representation of WSMAN_OPTION_SET.
            internal struct WSManKeyStruct
                internal string key;
        #region DllImports ClientAPI
        internal const string WSManClientApiDll = @"WsmSvc.dll";
        internal const string WSManProviderApiDll = @"WsmSvc.dll";
        internal const string WSManClientApiDll = @"libpsrpclient";
        internal const string WSManProviderApiDll = @"libpsrpomiprov";
        /// This API is used to initialize the WinRM client;
        /// It can be used by different clients on the same process, ie svchost.exe.
        /// Returns a nonzero error code upon failure.
        /// <param name="wsManAPIHandle">
        [DllImport(WSManNativeApi.WSManClientApiDll, SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern int WSManInitialize(int flags,
          [In, Out] ref IntPtr wsManAPIHandle);
        /// This API deinitializes the Winrm client stack; all operations will
        /// finish before this API will return; this is a sync call;
        /// it is highly recommended that all operations are explicitly cancelled
        /// and all sessions are closed before calling this API
        /// Returns non zero error code upon failure.
        /// <param name="wsManAPIHandle"></param>
        internal static extern int WSManDeinitialize(IntPtr wsManAPIHandle, int flags);
        /// Creates a session which can be used to perform subsequent operations
        /// Returns a non zero error code upon failure.
        /// <param name="connection">
        /// if NULL, then connection will default to 127.0.0.1
        /// <param name="authenticationCredentials">
        /// can be null.
        /// <param name="proxyInfo">
        /// <param name="wsManSessionHandle"></param>
        internal static extern int WSManCreateSession(IntPtr wsManAPIHandle,
            [MarshalAs(UnmanagedType.LPWStr)] string connection,
            IntPtr authenticationCredentials,
            IntPtr proxyInfo,
            [In, Out] ref IntPtr wsManSessionHandle);
        /// Frees memory of session and closes all related operations before returning;
        /// this is sync call it is recommended that all pending operations are either
        /// completed or cancelled before calling this API. Returns a non zero error
        /// code upon failure.
        internal static extern void WSManCloseSession(IntPtr wsManSessionHandle,
        /// WSManSetSessionOption API - set session options
        /// <param name="option"></param>
        /// An int (DWORD) data.
        internal static int WSManSetSessionOption(IntPtr wsManSessionHandle,
            WSManSessionOption option,
            WSManDataDWord data)
            MarshalledObject marshalObj = data.Marshal();
            using (marshalObj)
                return WSManSetSessionOption(wsManSessionHandle, option, marshalObj.DataPtr);
        internal static extern int WSManSetSessionOption(IntPtr wsManSessionHandle,
            IntPtr data);
        /// WSManGetSessionOptionAsDword API - get a session option. Returns a non
        /// zero error code upon failure.
        /// <returns>Zero on success, otherwise the error code.</returns>
        internal static extern int WSManGetSessionOptionAsDword(IntPtr wsManSessionHandle,
            out int value);
        /// Function that retrieves a WSMan session option as string. Thread.CurrentUICulture
        /// will be used as the language code to get the error message in.
        /// <param name="option">Session option to get.</param>
        internal static string WSManGetSessionOptionAsString(IntPtr wsManAPIHandle,
            WSManSessionOption option)
            Dbg.Assert(wsManAPIHandle != IntPtr.Zero, "wsManAPIHandle cannot be null.");
            // The error code taken from winerror.h used for getting buffer length.
            string returnval = string.Empty;
            int bufferSize = 0;
            // calculate buffer size required
            if (WSManGetSessionOptionAsString(wsManAPIHandle,
                option, 0, null, out bufferSize) != ERROR_INSUFFICIENT_BUFFER)
                return returnval;
            // calculate space required to store output.
            // StringBuilder will not work for this case as CLR
            // does not copy the entire string if there are delimiters ('\0')
            // in the middle of a string.
            int bufferSizeInBytes = bufferSize * 2;
            byte[] msgBufferPtr = new byte[bufferSizeInBytes];
            // Now get the actual value
            int messageLength;
                    option, bufferSizeInBytes, msgBufferPtr, out messageLength) != 0)
                returnval = Encoding.Unicode.GetString(msgBufferPtr, 0, bufferSizeInBytes);
            catch (System.Text.DecoderFallbackException)
        private static extern int WSManGetSessionOptionAsString(IntPtr wsManSessionHandle,
            int optionLength,
            byte[] optionAsString,
            out int optionLengthUsed);
        /// Creates a shell on the remote end.
        /// <param name="wsManSessionHandle">
        /// Session in which the shell is created.
        /// <param name="resourceUri">
        /// The resource Uri to use to create the shell.
        /// <param name="startupInfo">
        /// startup information to be passed to the shell.
        /// <param name="optionSet">
        /// Options to be passed with CreateShell
        /// <param name="openContent">
        /// any content that is used by the remote shell to startup.
        /// <param name="asyncCallback">
        /// callback to notify when the create operation completes.
        /// An out parameter referencing a WSMan shell operation handle
        /// for this shell.
        internal static void WSManCreateShellEx(IntPtr wsManSessionHandle,
            string resourceUri,
            WSManShellStartupInfo_ManToUn startupInfo,
            WSManOptionSet optionSet,
            WSManData_ManToUn openContent,
            IntPtr asyncCallback,
            ref IntPtr shellOperationHandle)
            WSManCreateShellExInternal(wsManSessionHandle, flags, resourceUri, shellId, startupInfo, optionSet,
                    openContent, asyncCallback, ref shellOperationHandle);
        [DllImport(WSManNativeApi.WSManClientApiDll, EntryPoint = "WSManCreateShellEx", SetLastError = false, CharSet = CharSet.Unicode)]
        private static extern void WSManCreateShellExInternal(IntPtr wsManSessionHandle,
            [MarshalAs(UnmanagedType.LPWStr)] string resourceUri,
            [MarshalAs(UnmanagedType.LPWStr)] string shellId,
            IntPtr startupInfo,
            IntPtr optionSet,
            IntPtr openContent,
            [In, Out] ref IntPtr shellOperationHandle);
        /// <param name="connectXml"></param>
        /// <param name="asyncCallback"></param>
        /// <param name="shellOperationHandle"></param>
        [DllImport(WSManNativeApi.WSManClientApiDll, EntryPoint = "WSManConnectShell", SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern void WSManConnectShellEx(IntPtr wsManSessionHandle,
            IntPtr connectXml,
        [DllImport(WSManNativeApi.WSManClientApiDll, EntryPoint = "WSManDisconnectShell", SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern void WSManDisconnectShellEx(IntPtr wsManSessionHandle,
            IntPtr disconnectInfo,
            IntPtr asyncCallback);
        [DllImport(WSManNativeApi.WSManClientApiDll, EntryPoint = "WSManReconnectShell", SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern void WSManReconnectShellEx(IntPtr wsManSessionHandle,
        [DllImport(WSManNativeApi.WSManClientApiDll, EntryPoint = "WSManReconnectShellCommand", SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern void WSManReconnectShellCommandEx(IntPtr wsManCommandHandle,
        /// Starts a command on the remote end.
        /// Shell handle in which the command is created and run.
        /// <param name="commandId"></param>
        /// <param name="commandLine">
        /// command line for the command.
        /// <param name="commandArgSet">
        /// arguments for the command.
        /// options.
        /// callback to notify when the operation completes.
        /// for this command.
        [DllImport(WSManNativeApi.WSManClientApiDll, EntryPoint = "WSManRunShellCommandEx", SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern void WSManRunShellCommandEx(IntPtr shellOperationHandle,
            string commandId,
            string commandLine,
            IntPtr commandArgSet,
            ref IntPtr commandOperationHandle);
        [DllImport(WSManNativeApi.WSManClientApiDll, EntryPoint = "WSManConnectShellCommand", SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern void WSManConnectShellCommandEx(IntPtr shellOperationHandle,
            string commandID,
        /// Registers a callback with WSMan to receive output from the remote end.
        /// If commandOperationHandle is null, then the receive callback is registered
        /// for shell. It is enough to register the callback only once. WSMan will
        /// keep on calling this callback as and when it has data for a particular
        /// command + shell. There will be only 1 callback active per command or per shell.
        /// So if there are multiple commands active, then there can be 1 callback active
        /// for each of them.
        /// TODO: How to unregister the callback.
        /// Shell Operation Handle.
        /// Command Operation Handle. If null, the receive request corresponds
        /// to the shell.
        /// <param name="desiredStreamSet"></param>
        /// callback which receives the data asynchronously.
        /// <param name="receiveOperationHandle">
        /// handle to use to cancel the operation.
        [DllImport(WSManNativeApi.WSManClientApiDll, EntryPoint = "WSManReceiveShellOutput", SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern void WSManReceiveShellOutputEx(IntPtr shellOperationHandle,
            IntPtr desiredStreamSet,
            [In, Out] ref IntPtr receiveOperationHandle);
        /// Send data to the remote end.
        /// Command Operation Handle. If null, the send request corresponds
        /// <param name="streamId"></param>
        /// <param name="streamData"></param>
        /// <param name="sendOperationHandle">
        internal static void WSManSendShellInputEx(IntPtr shellOperationHandle,
            [MarshalAs(UnmanagedType.LPWStr)] string streamId,
            WSManData_ManToUn streamData,
            ref IntPtr sendOperationHandle)
            WSManSendShellInputExInternal(shellOperationHandle, commandOperationHandle, flags, streamId,
                    streamData, false, asyncCallback, ref sendOperationHandle);
        [DllImport(WSManNativeApi.WSManClientApiDll, EntryPoint = "WSManSendShellInput", SetLastError = false, CharSet = CharSet.Unicode)]
        private static extern void WSManSendShellInputExInternal(IntPtr shellOperationHandle,
            IntPtr streamData,
            bool endOfStream,
            [In, Out] ref IntPtr sendOperationHandle);
        /// Closes a shell or a command; if the callback associated with the operation
        /// is pending and have not completed when WSManCloseShellOperationEx is called,
        /// the function waits for the callback to finish; If the operation was not finished,
        /// the operation is cancelled and the operation callback is called with
        /// WSMAN_ERROR_OPERATION_ABORTED error; then the WSManCloseShellOperationEx callback
        /// is called with WSMAN_FLAG_CALLBACK_END_OF_OPERATION flag as result of this operation.
        /// <param name="shellHandle">
        /// Shell handle to Close.
        internal static extern void WSManCloseShell(IntPtr shellHandle,
        /// Closes a command (signals the termination of a command); the WSManCloseCommand callback
        /// <param name="cmdHandle">
        /// Command handle to Close.
        internal static extern void WSManCloseCommand(IntPtr cmdHandle,
        /// Sends a signal. If <paramref name="cmdOperationHandle"/> is null, then the signal will
        /// be sent to shell.
        /// <param name="cmdOperationHandle"></param>
        /// <param name="code"></param>
        /// <param name="signalOperationHandle"></param>
        [DllImport(WSManNativeApi.WSManClientApiDll, EntryPoint = "WSManSignalShell", SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern void WSManSignalShellEx(IntPtr shellOperationHandle,
            IntPtr cmdOperationHandle,
            string code,
            [In, Out] ref IntPtr signalOperationHandle);
        /// Closes an asynchronous operation; if the callback associated with the operation
        /// is pending and have not completed when WSManCloseOperation is called, then
        /// the function marks the operation for deletion and returns; If the callback was not called,
        /// WSMAN_ERROR_OPERATION_ABORTED error; the operation handle is freed in all cases
        /// after the callback returns.
        /// <param name="operationHandle"></param>
        internal static extern void WSManCloseOperation(IntPtr operationHandle, int flags);
        /// Function that retrieves WSMan error messages with a particular error code. Thread.CurrentUICulture
        internal static string WSManGetErrorMessage(IntPtr wsManAPIHandle, int errorCode)
            // get language code.
            string langCode = CultureInfo.CurrentUICulture.Name;
            if (WSManGetErrorMessage(wsManAPIHandle,
                    0, langCode, errorCode, 0, null, out bufferSize) != ERROR_INSUFFICIENT_BUFFER)
                    0, langCode, errorCode, bufferSizeInBytes, msgBufferPtr, out messageLength) != 0)
        /// Function that retrieves WSMan error messages with a particular error code and a language code.
        /// The handle returned by WSManInitialize API call. It cannot be NULL.
        /// Reserved for future use. It must be 0.
        /// <param name="languageCode">
        /// Defines the RFC 3066 language code name that should be used to localize the error. It can be NULL.
        /// if not specified, the thread's UI language will be used.
        /// <param name="errorCode">
        /// Represents the error code for the requested error message. This error code can be a hexadecimal or
        /// decimal component from WSManagement component, WinHttp component or other Windows operating system
        /// components.
        /// <param name="messageLength">
        /// Represents the size of the output message buffer in characters, including the NULL terminator.
        /// If 0, then the "message" parameter must be NULL; in this case the function will return
        /// ERROR_INSUFFICIENT_BUFFER error and the "messageLengthUsed" parameter will be set to the number
        /// of characters needed, including NULL terminator.
        /// Represents the output buffer to store the message in. It must be allocated/deallocated by the client.
        /// The buffer must be big enough to store the message plus the NULL terminator otherwise an
        /// ERROR_INSUFFICIENT_BUFFER error will be returned and the "messageLengthUsed" parameter will be set
        /// to the number of characters needed, including NULL terminator. If NULL, then the "messageLength" parameter
        /// must be NULL; in this case the function will return ERROR_INSUFFICIENT_BUFFER error and the "messageLengthUsed"
        /// parameter will be set to the number of characters needed, including NULL terminator.
        /// <param name="messageLengthUsed">
        /// Represents the effective number of characters written to the output buffer, including the NULL terminator.
        /// It cannot be NULL. If both "messageLength" and "message" parameters are 0, the function will return ERROR_INSUFFICIENT_BUFFER
        /// and "messageLengthUsed" parameter will be set to the number of characters needed, including NULL terminator
        internal static extern int WSManGetErrorMessage(IntPtr wsManAPIHandle,
            string languageCode,
            int errorCode,
            int messageLength,
            byte[] message,
            out int messageLengthUsed);
        #region DllImports PluginAPI
        /// Gets operational information for items such as time-outs and data restrictions that
        /// are associated with the operation.
        /// <param name="requestDetails">Specifies the resource URI, options, locale, shutdown flag, and handle for the request.</param>
        /// <param name="flags">Specifies the options that are available for retrieval.</param>
        /// <param name="data">Specifies the result object (WSMAN_DATA).</param>
        [DllImport(WSManNativeApi.WSManProviderApiDll, SetLastError = false, CharSet = CharSet.Unicode)]
        internal static extern int WSManPluginGetOperationParameters(
            IntPtr requestDetails,
            [In, Out, MarshalAs(UnmanagedType.LPStruct)] WSManDataStruct data);
        // [In, Out] ref IntPtr data);
        /// Reports the completion of an operation by all operation entry points except for the
        /// WSManPluginStartup and WSManPluginShutdown methods.
        /// <param name="errorCode">Reports any failure in the operation. Terminates on non-NO_ERROR status.</param>
        /// <param name="extendedInformation">XML document containing extra error information.</param>
        internal static extern int WSManPluginOperationComplete(
            [MarshalAs(UnmanagedType.LPWStr)] string extendedInformation);
        internal enum WSManFlagReceive : int
            /// No more data on this stream.  Only valid when a stream is specified.
            WSMAN_FLAG_RECEIVE_RESULT_NO_MORE_DATA = 1,
            /// Send the data as soon as possible.  Normally data is held onto in
            /// order to maximise the size of the response packet.  This should
            /// only be used if a request/response style of data is needed between
            /// the send and receive data streams.
            WSMAN_FLAG_RECEIVE_FLUSH = 2,
            /// Data reported is at a boundary. Plugins usually serialize and fragment
            /// output data objects and push them along the receive byte stream.
            /// If the current data chunk being reported is an end fragment of the
            /// data object current processed, plugins would set this flag.
            WSMAN_FLAG_RECEIVE_RESULT_DATA_BOUNDARY = 4
        internal const string WSMAN_SHELL_NAMESPACE = "http://schemas.microsoft.com/wbem/wsman/1/windows/shell";
        internal const string WSMAN_COMMAND_STATE_DONE = WSMAN_SHELL_NAMESPACE + "/CommandState/Done";
        internal const string WSMAN_COMMAND_STATE_PENDING = WSMAN_SHELL_NAMESPACE + "/CommandState/Pending";
        internal const string WSMAN_COMMAND_STATE_RUNNING = WSMAN_SHELL_NAMESPACE + "/CommandState/Running";
        /// Reports results for the WSMAN_PLUGIN_RECEIVE plug-in call and is used by most shell
        /// plug-ins that return results. After all of the data is received, the
        /// WSManPluginOperationComplete method must be called.
        /// <param name="stream">Specifies the stream that the data is associated with.</param>
        /// <param name="streamResult">A pointer to a WSMAN_DATA structure that specifies the result object that is returned to the client.</param>
        /// <param name="commandState">Specifies the state of the command. It must be set to a value specified by the plugin.</param>
        /// <param name="exitCode">Only set when the commandState is terminating.</param>
        internal static extern int WSManPluginReceiveResult(
            [MarshalAs(UnmanagedType.LPWStr)] string stream,
            IntPtr streamResult,
            [MarshalAs(UnmanagedType.LPWStr)] string commandState,
            int exitCode);
        /// Reports shell and command context back to the Windows Remote Management (WinRM)
        /// infrastructure so that further operations can be performed against the shell and/or
        /// command. This method is called only for WSManPluginShell and WSManPluginCommand plug-in
        /// entry points.
        /// <param name="context">Defines the value to pass into all future shell and command operations. Represents either the shell or the command.</param>
        internal static extern int WSManPluginReportContext(
            IntPtr context);
        /// Registers the shutdown callback.
        /// <param name="shutdownCallback">Callback to be executed on shutdown.</param>
        /// <param name="shutdownContext"></param>
        internal static extern void WSManPluginRegisterShutdownCallback(
            IntPtr shutdownCallback,
            IntPtr shutdownContext);
    /// Interface to enable stubbing of the WSManNativeApi PInvoke calls for
    /// unit testing.
    /// Note: It is implemented as a class to avoid exposing it outside the module.
    internal interface IWSManNativeApiFacade
        // TODO: Expand this to cover the rest of the API once I prove that it works!
        int WSManPluginGetOperationParameters(
            WSManNativeApi.WSManDataStruct data);
        int WSManPluginOperationComplete(
            string extendedInformation);
        int WSManPluginReceiveResult(
            string? stream,
            string commandState,
        int WSManPluginReportContext(
        void WSManPluginRegisterShutdownCallback(
    /// Concrete implementation of the PInvoke facade for use in the production code.
    internal class WSManNativeApiFacade : IWSManNativeApiFacade
        int IWSManNativeApiFacade.WSManPluginGetOperationParameters(
            WSManNativeApi.WSManDataStruct data)
            return WSManNativeApi.WSManPluginGetOperationParameters(requestDetails, flags, data);
        int IWSManNativeApiFacade.WSManPluginOperationComplete(
            string extendedInformation)
            return WSManNativeApi.WSManPluginOperationComplete(requestDetails, flags, errorCode, extendedInformation);
        int IWSManNativeApiFacade.WSManPluginReceiveResult(
            int exitCode)
            return WSManNativeApi.WSManPluginReceiveResult(requestDetails, flags, stream, streamResult, commandState, exitCode);
        int IWSManNativeApiFacade.WSManPluginReportContext(
            IntPtr context)
            return WSManNativeApi.WSManPluginReportContext(requestDetails, flags, context);
        void IWSManNativeApiFacade.WSManPluginRegisterShutdownCallback(
            IntPtr shutdownContext)
            WSManNativeApi.WSManPluginRegisterShutdownCallback(requestDetails, shutdownCallback, shutdownContext);
