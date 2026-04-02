package org.openhab.core.storage.json.internal.migration;
 * The {@link BridgeImplTypeMigrator} implements a {@link TypeMigrator} for stored bridges
public class BridgeImplTypeMigrator extends ThingImplTypeMigrator {
    public String getOldType() {
        return "org.openhab.core.thing.internal.BridgeImpl";
    public String getNewType() {
        return "org.openhab.core.thing.internal.ThingStorageEntity";
    public JsonElement migrate(JsonElement oldValue) throws TypeMigrationException {
        JsonElement newValue = super.migrate(oldValue);
        newValue.getAsJsonObject().addProperty("isBridge", true);
