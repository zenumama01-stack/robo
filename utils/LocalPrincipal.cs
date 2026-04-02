    /// Defines the source of a Principal.
    public enum PrincipalSource
        /// The principal source is unknown or could not be determined.
        /// The principal is sourced from the local Windows Security Accounts Manager.
        /// The principal is sourced from an Active Directory domain.
        ActiveDirectory,
        /// The principal is sourced from Azure Active Directory.
        AzureAD,
        /// The principal is a Microsoft Account, such as
        /// <b>MicrosoftAccount\user@domain.com</b>
        MicrosoftAccount
    /// Represents a Principal. Serves as a base class for Users and Groups.
    public class LocalPrincipal
        /// The account name of the Principal.
        /// The Security Identifier that uniquely identifies the Principal/
        public SecurityIdentifier SID { get; set; }
        /// Indicates the account store from which the principal is sourced.
        /// One of the PrincipalSource enumerations.
        public PrincipalSource? PrincipalSource { get; set; }
        /// The object class that represents this principal.
        /// This can be User or Group.
        public string ObjectClass { get; set; }
        /// Initializes a new LocalPrincipal object.
        public LocalPrincipal()
        /// Initializes a new LocalPrincipal object with the specified name.
        /// <param name="name">Name of the new LocalPrincipal.</param>
        public LocalPrincipal(string name)
        /// Provides a string representation of the Principal.
        /// A string, in SDDL form, representing the Principal.
