    public class ImportModuleEventArgs : EventArgs
        /// the name for the command belonging to the module to be imported.
        /// the module path or name for the module we want to import.
        private string parentModuleName;
        /// the name of the module that is selected, which can be different from parentModuleName
        /// if "All" is selected
        private string selectedModuleName;
        /// Initializes a new instance of the ImportModuleEventArgs class.
        /// <param name="parentModuleName">The name of the module containing the command.</param>
        /// <param name="selectedModuleName">
        public ImportModuleEventArgs(string commandName, string parentModuleName, string selectedModuleName)
            this.parentModuleName = parentModuleName;
            this.selectedModuleName = selectedModuleName;
        /// Gets the name for the command belonging to the module to be imported.
        /// Gets the module path or name for the module we want to import.
        public string ParentModuleName
            get { return this.parentModuleName; }
        /// Gets the name of the module that is selected, which can be different from parentModuleName
        public string SelectedModuleName
            get { return this.selectedModuleName; }
