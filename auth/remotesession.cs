    /// This abstract class is designed to provide InstanceId and self identification for
    /// client and server remote session classes.
    internal abstract class RemoteSession
        /// This is the unique id of a remote session object.
        internal Guid InstanceId { get; } = new Guid();
        /// This indicates the remote session object is Client, Server or Listener.
        internal abstract RemotingDestination MySelf { get; }
        internal abstract void StartKeyExchange();
        internal abstract void CompleteKeyExchange();
        internal BaseSessionDataStructureHandler BaseSessionDataStructureHandler { get; set; }
