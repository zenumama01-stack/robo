package org.openhab.core.thing.dto;
 * This is a data transfer object that is used to serialize things.
 * @author Stefan Bußweiler - Added new thing status handling
 * @author Simon Kaufmann - Added label
public abstract class AbstractThingDTO {
    public String bridgeUID;
    public Map<String, String> properties;
    public String UID;
    public String thingTypeUID;
    public String semanticEquipmentTag;
    protected AbstractThingDTO() {
    protected AbstractThingDTO(String thingTypeUID, String uid, String label, String bridgeUID,
            Map<String, Object> configuration, Map<String, String> properties, String location,
            String semanticEquipmentTag) {
        this.UID = uid;
