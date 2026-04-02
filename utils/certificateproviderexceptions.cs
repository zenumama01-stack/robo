    /// Defines the base class for exceptions thrown by the
    /// certificate provider when the specified item cannot be located.
    public class CertificateProviderItemNotFoundException : SystemException
        /// Initializes a new instance of the CertificateProviderItemNotFoundException
        /// class with the default message.
        public CertificateProviderItemNotFoundException() : base()
        /// class with the specified message.
        /// The message to be included in the exception.
        public CertificateProviderItemNotFoundException(string message) : base(message)
        /// class with the specified message, and inner exception.
        /// The inner exception to be included in the exception.
        public CertificateProviderItemNotFoundException(string message,
        /// class with the specified serialization information, and context.
        /// The serialization information.
        /// The streaming context.
        protected CertificateProviderItemNotFoundException(SerializationInfo info,
        /// class with the specified inner exception.
        internal CertificateProviderItemNotFoundException(Exception innerException)
            : base(innerException.Message, innerException)
    /// Defines the exception thrown by the certificate provider
    /// when the specified X509 certificate cannot be located.
    public class CertificateNotFoundException
              : CertificateProviderItemNotFoundException
        /// Initializes a new instance of the CertificateNotFoundException
        public CertificateNotFoundException()
        public CertificateNotFoundException(string message)
        public CertificateNotFoundException(string message,
        protected CertificateNotFoundException(SerializationInfo info,
        internal CertificateNotFoundException(Exception innerException)
    /// when the specified X509 store cannot be located.
    public class CertificateStoreNotFoundException
        /// Initializes a new instance of the CertificateStoreNotFoundException
        public CertificateStoreNotFoundException()
        protected CertificateStoreNotFoundException(SerializationInfo info,
        public CertificateStoreNotFoundException(string message)
        public CertificateStoreNotFoundException(string message,
        internal CertificateStoreNotFoundException(Exception innerException)
    /// when the specified X509 store location cannot be located.
    public class CertificateStoreLocationNotFoundException
        /// Initializes a new instance of the CertificateStoreLocationNotFoundException
        public CertificateStoreLocationNotFoundException()
        protected CertificateStoreLocationNotFoundException(SerializationInfo info,
        public CertificateStoreLocationNotFoundException(string message)
        public CertificateStoreLocationNotFoundException(string message,
        internal CertificateStoreLocationNotFoundException(Exception innerException)
