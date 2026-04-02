    /// CabinetExtractor interface, implemented by CabinetExtractor
    /// in native code to handle the extraction of cabinet files.
    internal abstract class ICabinetExtractor : IDisposable
        /// Extracts a cabinet file.
        /// <param name="cabinetName">Cabinet file name.</param>
        /// <param name="srcPath">Cabinet directory name, must be back slash terminated.</param>
        /// <param name="destPath">Destination directory name, must be back slash terminated.</param>
        internal abstract bool Extract(string cabinetName, string srcPath, string destPath);
        #region IDisposable Interface
        // This is a special case of the IDisposable pattern because the resource
        // to be disposed is managed by the derived class. The implementation here
        // enables derived classes to handle it cleanly.
        /// Flag: Has Dispose already been called?
        /// Disposes the instance, callable by consumers.
                // Nothing to do since the resource has already been disposed.
            // If this class had to free objects:
            // Free managed objects if disposing == true;
            // Free unmanaged objects regardless.
        ~ICabinetExtractor()
    /// Abstract class which defines a CabinetExtractor loader. An implementation
    /// of this class will be instantiated onetime from the C++/CLI
    /// assembly using reflection.
    /// <remarks>The C++/CLI implementation of this class needs to be
    /// static</remarks>
    internal abstract class ICabinetExtractorLoader
        internal virtual ICabinetExtractor GetCabinetExtractor() { return null; }
    /// Used to create a CabinetExtractor class.
    internal static class CabinetExtractorFactory
        private static readonly ICabinetExtractorLoader s_cabinetLoader;
        internal static readonly ICabinetExtractor EmptyExtractor = new EmptyCabinetExtractor();
        static CabinetExtractorFactory()
            s_cabinetLoader = CabinetExtractorLoader.GetInstance();
        /// Provider a CabinetExtractor instance.
        /// <returns>Tracer instance.</returns>
        internal static ICabinetExtractor GetCabinetExtractor()
            if (s_cabinetLoader != null)
                return s_cabinetLoader.GetCabinetExtractor();
                return EmptyExtractor;
    /// Dummy cabinet extractor implementation.
    internal sealed class EmptyCabinetExtractor : ICabinetExtractor
        internal override bool Extract(string cabinetName, string srcPath, string destPath)
            // its intentional that this method has no definition
        /// Disposes the instance.
            // it's intentional that this method has no definition since there is nothing to dispose.
            // If a resource is added to this class, it should implement IDisposable for derived classes.
