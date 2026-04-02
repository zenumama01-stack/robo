namespace System.Management.Automation.Language
    /// This type is introduced to provide a way to pass null into a .NET method that has a string parameter.
    public class NullString
        // Private member for instance.
        /// This overrides ToString() method and returns null.
        /// This returns the singleton instance of NullString.
        public static NullString Value { get; } = new NullString();
        #region private Constructor
        /// This is a private constructor, meaning no outsiders have access.
        private NullString()
        #endregion private Constructor
