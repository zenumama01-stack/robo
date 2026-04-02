    /// FormObjectCollection used in HtmlWebResponseObject.
    public class FormObjectCollection : Collection<FormObject>
        /// Gets the FormObject from the key.
        public FormObject? this[string key]
                FormObject? form = null;
                foreach (FormObject f in this)
                    if (string.Equals(key, f.Id, StringComparison.OrdinalIgnoreCase))
                        form = f;
                return form;
