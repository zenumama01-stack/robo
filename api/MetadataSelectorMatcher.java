 * Matches available metadata namespaces against the given namespace list or regular expression.
@Component(service = MetadataSelectorMatcher.class)
public class MetadataSelectorMatcher {
    public MetadataSelectorMatcher(final @Reference MetadataRegistry metadataRegistry) {
     * Filter existing metadata namespaces against the given namespaceSelector. The given String might consist of a
     * comma separated list of namespaces as well as a regular expression.
     * @param namespaceSelector a comma separated list of namespaces or a regular expression.
     * @param locale the locale for config descriptions with the scheme "metadata".
     * @return a {@link Set} of matching namespaces.
    public Set<String> filterNamespaces(@Nullable String namespaceSelector, @Nullable Locale locale) {
        if (namespaceSelector == null || namespaceSelector.isEmpty()) {
            Set<String> originalNamespaces = Arrays.stream(namespaceSelector.split(",")) //
                    .filter(n -> !metadataRegistry.isInternalNamespace(n)) //
                    .map(String::trim) //
            Set<String> allMetadataNamespaces = metadataRegistry.getAll().stream() //
                    .map(metadata -> metadata.getUID().getNamespace()) //
            String namespacePattern = String.join("|", originalNamespaces);
            Pattern pattern = Pattern.compile("(" + namespacePattern + ")$");
            Set<String> metadataNamespaces = allMetadataNamespaces.stream() //
                    .filter(pattern.asPredicate()).collect(Collectors.toSet());
            // merge metadata namespaces and namespaces from the namespace selector:
            Set<String> result = new HashSet<>(originalNamespaces);
            result.addAll(metadataNamespaces);
            // filter all name spaces which do not match the UID segment pattern (this will be the regex tokens):
            return result.stream().filter(AbstractUID::isValid).collect(Collectors.toSet());
