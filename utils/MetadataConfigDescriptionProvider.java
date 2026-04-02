package org.openhab.core.config.core.metadata;
 * A {@link MetadataConfigDescriptionProvider} implementation can be registered as an OSGi service in order to give
 * guidance to UIs what metadata namespaces should be available and what metadata properties are expected.
 * It will be tracked by the framework and the given information will be translated into config descriptions.
 * Every extension which deals with specific metadata (in its own namespace) is expected to provide an implementation of
 * this interface.
public interface MetadataConfigDescriptionProvider {
     * Get the identifier of the metadata namespace
     * @return the metadata namespace
    String getNamespace();
     * Get the human-readable description of the metadata namespace
     * Overriding this method is optional - it will default to the namespace identifier.
     * @param locale a locale, if available
     * @return the metadata namespace description
    String getDescription(@Nullable Locale locale);
     * Get all valid options if the main metadata value should be restricted to certain values.
     * @return a list of parameter options or {@code null}
    List<ParameterOption> getParameterOptions(@Nullable Locale locale);
     * Get the config descriptions for all expected parameters.
     * This list may depend on the current "main" value
     * @param value the current "main" value
     * @return a list of config description parameters or {@code null}
    List<ConfigDescriptionParameter> getParameters(String value, @Nullable Locale locale);
