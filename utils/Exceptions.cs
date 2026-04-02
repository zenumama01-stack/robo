using System.Management.Automation.SecurityAccountsManager;
using Microsoft.PowerShell.LocalAccounts;
    /// Base class for cmdlet-specific exceptions.
    public class LocalAccountsException : Exception
        /// Gets the <see cref="System.Management.Automation.ErrorCategory"/>
        /// value for this exception.
        public ErrorCategory ErrorCategory
        /// Gets the target object for this exception. This is used as
        /// the TargetObject member of a PowerShell
        /// <see cref="System.Management.Automation.ErrorRecord"/> object.
        public object Target
        /// Gets the error name. This is used as the ErrorId parameter when
        /// constructing a PowerShell <see cref="System.Management.Automation.ErrorRecord"/>
        /// oject.
        public string ErrorName
                string exname = "Exception";
                var exlen = exname.Length;
                var name = this.GetType().Name;
                if (name.EndsWith(exname, StringComparison.OrdinalIgnoreCase) && name.Length > exlen)
                    name = name.Substring(0, name.Length - exlen);
        internal LocalAccountsException(string message, object target, ErrorCategory errorCategory)
            ErrorCategory = errorCategory;
        /// Compliance Constructor.
        public LocalAccountsException() : base() { }
        public LocalAccountsException(string message) : base(message) { }
        public LocalAccountsException(string message, Exception ex) : base(message, ex) { }
        /// <param name="ctx"></param>
        protected LocalAccountsException(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
    /// Exception indicating an error occurred during one of the internal
    /// operations such as opening or closing a handle.
    public class InternalException : LocalAccountsException
        /// Gets the NTSTATUS code for this exception.
        public UInt32 StatusCode
        internal InternalException(UInt32 ntStatus,
                                   object target,
                                   ErrorCategory errorCategory = ErrorCategory.NotSpecified)
            : base(message, target, errorCategory)
            StatusCode = ntStatus;
            : this(ntStatus,
                   StringUtil.Format(Strings.UnspecifiedErrorNtStatus, ntStatus),
                   errorCategory)
        public InternalException() : base() { }
        public InternalException(string message) : base(message) { }
        public InternalException(string message, Exception ex) : base(message, ex) { }
        protected InternalException(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
    /// Exception indicating an error occurred when a native function
    /// is called that returns a Win32 error code as opposed to an
    /// NT Status code.
    public class Win32InternalException : LocalAccountsException
        /// The Win32 error code for this exception.
        public int NativeErrorCode
        internal Win32InternalException(int errorCode,
            NativeErrorCode = errorCode;
            : this(errorCode,
                   StringUtil.Format(Strings.UnspecifiedErrorWin32Error, errorCode),
        public Win32InternalException() : base() {}
        public Win32InternalException(string message) : base(message) { }
        public Win32InternalException(string message, Exception ex) : base(message, ex) { }
        protected Win32InternalException(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
    /// Exception indicating an invalid password.
    public class InvalidPasswordException : LocalAccountsException
        /// Generates with a default invalid password message.
        public InvalidPasswordException()
            : base(Strings.InvalidPassword, null, ErrorCategory.InvalidArgument)
        /// Generates the exception with the specified message.
        public InvalidPasswordException(string message)
            : base(message, null, ErrorCategory.InvalidArgument)
        /// Creates a message from the specified error code.
        /// <param name="errorCode"></param>
        public InvalidPasswordException(uint errorCode)
            : base(StringUtil.GetSystemMessage(errorCode), null, ErrorCategory.InvalidArgument)
        public InvalidPasswordException(string message, Exception ex) : base(message, ex) { }
        protected InvalidPasswordException(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
    /// Exception thrown when invalid parameter pairing is detected.
    public class InvalidParametersException : LocalAccountsException
        /// Creates InvalidParametersException using the specified message.
        public InvalidParametersException(string message)
        internal InvalidParametersException(string parameterA, string parameterB)
            : this(StringUtil.Format(Strings.InvalidParameterPair, parameterA, parameterB))
        public InvalidParametersException() : base() { }
        public InvalidParametersException(string message, Exception ex) : base(message, ex) { }
        protected InvalidParametersException(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
    /// Exception indicating permission denied.
    public class AccessDeniedException : LocalAccountsException
        internal AccessDeniedException(object target)
            : base(Strings.AccessDenied, target, ErrorCategory.PermissionDenied)
        public AccessDeniedException() : base() { }
        public AccessDeniedException(string message) : base(message) { }
        public AccessDeniedException(string message, Exception ex) : base(message, ex) { }
        protected AccessDeniedException(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
    /// Exception indicating that the name of a user or group is invalid.
    public class InvalidNameException : LocalAccountsException
        internal InvalidNameException(string name, object target)
            : base(StringUtil.Format(Strings.InvalidName, name), target, ErrorCategory.InvalidArgument)
        public InvalidNameException() : base() { }
        public InvalidNameException(string message) : base(message) { }
        public InvalidNameException(string message, Exception ex) : base(message, ex) { }
        protected InvalidNameException(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
    /// Exception indicating that the specified name is already in use.
    public class NameInUseException : LocalAccountsException
        internal NameInUseException(string name, object target)
            : base(StringUtil.Format(Strings.NameInUse, name), target, ErrorCategory.InvalidArgument)
        public NameInUseException() : base() { }
        public NameInUseException(string message) : base(message) { }
        public NameInUseException(string message, Exception ex) : base(message, ex) { }
        protected NameInUseException(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
    /// Exception indicating that an entity of some kind was not found.
    /// Also serves as a base class for more specific object-not-found errors.
    public class NotFoundException : LocalAccountsException
        internal NotFoundException(string message, object target)
          : base(message, target, ErrorCategory.ObjectNotFound)
        public NotFoundException() : base() { }
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string message, Exception ex) : base(message, ex) { }
        protected NotFoundException(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
    /// Exception indicating that a principal was not Found.
    public class PrincipalNotFoundException : NotFoundException
        internal PrincipalNotFoundException(string principal, object target)
            : base(StringUtil.Format(Strings.PrincipalNotFound, principal), target)
        public PrincipalNotFoundException() : base() { }
        public PrincipalNotFoundException(string message) : base(message) { }
        public PrincipalNotFoundException(string message, Exception ex) : base(message, ex) { }
        protected PrincipalNotFoundException(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
    /// Exception indicating that a group was not found.
    public class GroupNotFoundException : NotFoundException
        internal GroupNotFoundException(string group, object target)
            : base(StringUtil.Format(Strings.GroupNotFound, group), target)
        public GroupNotFoundException() : base() { }
        public GroupNotFoundException(string message) : base(message) { }
        public GroupNotFoundException(string message, Exception ex) : base(message, ex) { }
        protected GroupNotFoundException(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
    /// Exception indicating that a user was not found.
    public class UserNotFoundException : NotFoundException
        internal UserNotFoundException(string user, object target)
            : base(StringUtil.Format(Strings.UserNotFound, user), target)
        public UserNotFoundException() : base() { }
        public UserNotFoundException(string message) : base(message) { }
        public UserNotFoundException(string message, Exception ex) : base(message, ex) { }
        protected UserNotFoundException(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
    /// Exception indicating that a group member was not found.
    public class MemberNotFoundException : NotFoundException
        internal MemberNotFoundException(string member, string group)
            : base(StringUtil.Format(Strings.MemberNotFound, member, group), member)
        public MemberNotFoundException() : base() { }
        public MemberNotFoundException(string message) : base(message) { }
        public MemberNotFoundException(string message, Exception ex) : base(message, ex) { }
        protected MemberNotFoundException(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
    /// Exception indicating that an entity of some kind already exists.
    /// Also serves as a base class for more specific object-exists errors.
    public class ObjectExistsException : LocalAccountsException
        internal ObjectExistsException(string message, object target)
            : base(message, target, ErrorCategory.ResourceExists)
        public ObjectExistsException() : base() { }
        public ObjectExistsException(string message) : base(message) { }
        public ObjectExistsException(string message, Exception ex) : base(message, ex) { }
        protected ObjectExistsException(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
    /// Exception indicating that a group already exists.
    public class GroupExistsException : ObjectExistsException
        internal GroupExistsException(string group, object target)
            : base(StringUtil.Format(Strings.GroupExists, group), target)
        public GroupExistsException() : base() { }
        public GroupExistsException(string message) : base(message) { }
        public GroupExistsException(string message, Exception ex) : base(message, ex) { }
        protected GroupExistsException(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
    public class UserExistsException : ObjectExistsException
        internal UserExistsException(string user, object target)
            : base(StringUtil.Format(Strings.UserExists, user), target)
        public UserExistsException() : base() { }
        public UserExistsException(string message) : base(message) { }
        public UserExistsException(string message, Exception ex) : base(message, ex) { }
        protected UserExistsException(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
    /// Exception indicating that an object already exists as a group member.
    public class MemberExistsException : ObjectExistsException
        internal MemberExistsException(string member, string group, object target)
            : base(StringUtil.Format(Strings.MemberExists, member, group), target)
        public MemberExistsException() : base() { }
        public MemberExistsException(string message) : base(message) { }
        public MemberExistsException(string message, Exception ex) : base(message, ex) { }
        protected MemberExistsException(SerializationInfo info, StreamingContext ctx) : base(info, ctx) { }
