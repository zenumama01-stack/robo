** native EVT_HANDLE - obtained from EventLog Native Methods.
    // marked as Safe since the only real operation that is performed
    // by this class is NativeWrapper.EvtClose and that is protected
    // by a full Demand() before doing any work.
    internal sealed class EventLogHandle : SafeHandle
        // Called by P/Invoke when returning SafeHandles
        private EventLogHandle()
        internal EventLogHandle(IntPtr handle, bool ownsHandle)
            NativeWrapper.EvtClose(handle);
        // DONT compare EventLogHandle with EventLogHandle.Zero
        public static EventLogHandle Zero
                return new EventLogHandle();
