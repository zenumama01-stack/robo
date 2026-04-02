package org.openhab.core.config.core.validation;
 * The {@link ConfigDescriptionValidator} validates a given set of {@link Configuration} parameters against a
public interface ConfigDescriptionValidator {
    void validate(Map<String, Object> configurationParameters, URI configDescriptionURI);
