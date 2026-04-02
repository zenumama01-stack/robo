package org.openhab.core.io.rest;
 * Utilities for mapping/transforming DTOs.
public interface DTOMapper {
    <@NonNull T> Stream<T> limitToFields(Stream<T> itemStream, @Nullable String fields);
