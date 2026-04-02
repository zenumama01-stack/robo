 * Can be implemented to point one config description URI to another.
public interface ConfigDescriptionAliasProvider {
     * Get an alias for the given config description URI (if applicable)
     * @param configDescriptionURI the original config description URI
     * @return the alias or {@code null} if this handler does not apply to the given URI.
    URI getAlias(URI configDescriptionURI);
