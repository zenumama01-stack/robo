package org.openhab.core.io.dto;
 * A DTO that can handle the deserialization from "tree form" itself, allowing more flexible, multi-step processing.
 * @param <D> the DTO type.
 * @param <M> the mapper/helper type, e.g. {@code ObjectMapper}.
 * @param <N> the tree-node type, e.g. {@code JsonNode}.
public interface ModularDTO<D, M, N> {
     * Deserializes the specified node into a DTO object.
     * @param node the node to deserialize.
     * @param mapper the mapper/helper object to use for deserialization.
     * @return The resulting DTO instance.
     * @throws SerializationException If an error occurs during deserialization.
    D toDto(@NonNull N node, @NonNull M mapper) throws SerializationException;
