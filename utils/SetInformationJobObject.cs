        internal const int JobObjectAssociateCompletionPortInformation = 7;
        internal const int JOB_OBJECT_MSG_ACTIVE_PROCESS_ZERO = 4;
        internal struct JOBOBJECT_ASSOCIATE_COMPLETION_PORT
            public nint CompletionKey;
            public nint CompletionPort;
        private static partial bool SetInformationJobObject(
            int JobObjectInformationClass,
            ref JOBOBJECT_ASSOCIATE_COMPLETION_PORT lpJobObjectInformation,
            int cbJobObjectInformationLength);
        internal static bool SetInformationJobObject(
            SafeJobHandle jobHandle,
            SafeIoCompletionPort completionPort)
            JOBOBJECT_ASSOCIATE_COMPLETION_PORT objectInfo = new()
                CompletionKey = jobHandle.DangerousGetHandle(),
                CompletionPort = completionPort.DangerousGetHandle(),
            return SetInformationJobObject(
                jobHandle,
                JobObjectAssociateCompletionPortInformation,
                ref objectInfo,
                Marshal.SizeOf<JOBOBJECT_ASSOCIATE_COMPLETION_PORT>());
