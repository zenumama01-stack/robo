using Microsoft.Win32.SafeHandles;
namespace Microsoft.PowerShell.Commands;
/// JobProcessCollection is a helper class used by Start-Process -Wait cmdlet to monitor the
/// child processes created by the main process hosted by the Start-process cmdlet.
internal sealed class JobProcessCollection : IDisposable
    /// Stores the initialisation state of the job and completion port.
    private bool? _initStatus;
    /// JobObjectHandle is a reference to the job object used to track
    /// the child processes created by the main process hosted by the Start-Process cmdlet.
    private Interop.Windows.SafeJobHandle? _jobObject;
    /// The completion port handle that is used to monitor job events.
    private Interop.Windows.SafeIoCompletionPort? _completionPort;
    /// Initializes a new instance of the <see cref="JobProcessCollection"/> class.
    public JobProcessCollection()
    /// Initializes the job and IO completion port and adds the process to the
    /// job object.
    /// <param name="process">The process to add to the job.</param>
    /// <returns>Whether the job creation and assignment worked or not.</returns>
    public bool AssignProcessToJobObject(SafeProcessHandle process)
        => InitializeJob() && Interop.Windows.AssignProcessToJobObject(_jobObject, process);
    /// Blocks the current thread until all processes in the job have exited.
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public void WaitForExit(CancellationToken cancellationToken)
        if (_completionPort is null)
        using var cancellationRegistration = cancellationToken.Register(() =>
            Interop.Windows.PostQueuedCompletionStatus(
                _completionPort,
                Interop.Windows.JOB_OBJECT_MSG_ACTIVE_PROCESS_ZERO);
        int completionCode = 0;
            Interop.Windows.GetQueuedCompletionStatus(
                Interop.Windows.INFINITE,
                out completionCode);
        while (completionCode != Interop.Windows.JOB_OBJECT_MSG_ACTIVE_PROCESS_ZERO);
        cancellationToken.ThrowIfCancellationRequested();
    [MemberNotNullWhen(true, [nameof(_jobObject), nameof(_completionPort)])]
    private bool InitializeJob()
        if (_initStatus.HasValue)
            return _initStatus.Value;
        if (_jobObject is null)
            _jobObject = Interop.Windows.CreateJobObject();
            if (_jobObject.IsInvalid)
                _initStatus = false;
                _jobObject.Dispose();
                _jobObject = null;
            _completionPort = Interop.Windows.CreateIoCompletionPort();
            if (_completionPort.IsInvalid)
                _completionPort.Dispose();
                _completionPort = null;
        _initStatus = Interop.Windows.SetInformationJobObject(
            _jobObject,
            _completionPort);
        _jobObject?.Dispose();
        _completionPort?.Dispose();
