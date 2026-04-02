package org.openhab.core.semantics;
 * This is the super interface for all types that represent an Equipment.
 * The interface describes the relations to other entity types.
public interface Equipment extends Tag {
    Equipment isPartOf();
    Location hasLocation();
