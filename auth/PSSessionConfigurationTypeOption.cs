    /// This is the base class from which other classes should derive.
    /// This class defines the options for the specified configuration type.
    public abstract class PSSessionTypeOption
        /// Returns a xml formatted data that represents the options.
        protected internal virtual string ConstructPrivateData()
        /// Returns a new instance constructed from privateData string.
        protected internal virtual PSSessionTypeOption ConstructObjectFromPrivateData(string privateData)
        /// Copies values from updated.  Only non default values are copies.
        /// <param name="updated"></param>
        protected internal virtual void CopyUpdatedValuesFrom(PSSessionTypeOption updated)
    /// This the abstract class that defines the options for underlying transport layer.
    public abstract class PSTransportOption : ICloneable
        /// Returns all the non-quota options set in this object in a format of xml attributes.
        internal virtual string ConstructOptionsAsXmlAttributes()
        /// Returns all the non-quota options set in this object in a name-value pair (hashtable).
        internal virtual Hashtable ConstructOptionsAsHashtable()
        /// Returns all the quota related options set in this object in a format of xml attributes.
        internal virtual string ConstructQuotas()
        /// Returns all the quota related options in the form of a hashtable.
        internal virtual Hashtable ConstructQuotasAsHashtable()
        /// Sets all the values to default values.
        /// If keepAssigned is true only those values are set
        /// which are unassigned.
        protected internal virtual void LoadFromDefaults(bool keepAssigned)
        /// Clone from ICloneable.
            return this.MemberwiseClone();
