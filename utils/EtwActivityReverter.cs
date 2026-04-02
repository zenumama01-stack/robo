    ///     An object that can be used to revert the ETW activity ID of the current thread
    ///     to its original value.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Etw")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Reverter")]
    public interface IEtwActivityReverter :
        IDisposable
        ///     Reverts the ETW activity ID of the current thread to its original value.
        ///     <para>Calling <see cref="IDisposable.Dispose"/> has the same effect as
        ///         calling this method and is useful in the C# "using" syntax.</para>
        void RevertCurrentActivityId();
    internal class EtwActivityReverter :
        IEtwActivityReverter
        private readonly IEtwEventCorrelator _correlator;
        private readonly Guid _oldActivityId;
        public EtwActivityReverter(IEtwEventCorrelator correlator, Guid oldActivityId)
            _correlator = correlator;
            _oldActivityId = oldActivityId;
        public void RevertCurrentActivityId()
                _correlator.CurrentActivityId = _oldActivityId;
