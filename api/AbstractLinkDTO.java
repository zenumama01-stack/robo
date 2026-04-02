package org.openhab.core.thing.link.dto;
 * This is an abstract class for link data transfer object that is used to serialize links.
public abstract class AbstractLinkDTO {
    public String itemName;
    protected AbstractLinkDTO() {
    protected AbstractLinkDTO(String itemName) {
