 * The {@link XmlDocumentProviderFactory} is responsible to create {@link XmlDocumentProvider} instances for any certain
 * module. The factory is <i>not</i> responsible to clean-up any
 * created providers.
 * The {@link XmlDocumentProviderFactory} is used by the {@link XmlDocumentBundleTracker} to create for each module an
 * own {@link XmlDocumentProvider} instance to process any result objects from the XML conversion.
 * @see XmlDocumentProvider
public interface XmlDocumentProviderFactory<@NonNull T> {
     * Creates an XML document provider for the specified module which is used to process
     * any result objects from the XML conversion.
     * @param bundle the module for which the provider must be created (must not be null)
     * @return the created provider for the specified module (must not be null)
    XmlDocumentProvider<T> createDocumentProvider(Bundle bundle);
