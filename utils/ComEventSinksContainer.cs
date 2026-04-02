    /// ComEventSinksContainer is just a regular list with a finalizer.
    /// This list is usually attached as a custom data for RCW object and
    /// is finalized whenever RCW is finalized.
    internal class ComEventSinksContainer : List<ComEventsSink>, IDisposable
        private ComEventSinksContainer()
        private static readonly object s_comObjectEventSinksKey = new object();
        public static ComEventSinksContainer FromRuntimeCallableWrapper(object rcw, bool createIfNotFound)
            object data = Marshal.GetComObjectData(rcw, s_comObjectEventSinksKey);
            if (data != null || !createIfNotFound)
                return (ComEventSinksContainer)data;
            lock (s_comObjectEventSinksKey)
                data = Marshal.GetComObjectData(rcw, s_comObjectEventSinksKey);
                ComEventSinksContainer comEventSinks = new ComEventSinksContainer();
                if (!Marshal.SetComObjectData(rcw, s_comObjectEventSinksKey, comEventSinks))
                    throw Error.SetComObjectDataFailed();
                return comEventSinks;
            DisposeAll();
        private void DisposeAll()
            foreach (ComEventsSink sink in this)
                ComEventsSink.RemoveAll(sink);
        ~ComEventSinksContainer()
