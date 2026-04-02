 * The {@link ThingImplTypeMigrator} implements a {@link TypeMigrator} for stored things
public class ThingImplTypeMigrator implements TypeMigrator {
        return "org.openhab.core.thing.internal.ThingImpl";
        segmentUidToStringUid(newValue, "uid", "UID");
        segmentUidToStringUid(newValue, "bridgeUID");
        segmentUidToStringUid(newValue, "thingTypeUID");
        for (JsonElement jsonElement : newValue.get("channels").getAsJsonArray()) {
            JsonObject channel = jsonElement.getAsJsonObject();
            channel.add("itemType", channel.remove("acceptedItemType"));
            channel.add("configuration", channel.remove("configuration").getAsJsonObject().get("properties"));
            segmentUidToStringUid(channel, "uid");
            segmentUidToStringUid(channel, "channelTypeUID");
        newValue.add("configuration", newValue.remove("configuration").getAsJsonObject().get("properties"));
        newValue.addProperty("isBridge", false);
    private void segmentUidToStringUid(JsonObject object, String name) {
        segmentUidToStringUid(object, name, name);
    private void segmentUidToStringUid(JsonObject object, String oldName, String newName) {
        JsonElement element = object.remove(oldName);
            Spliterator<JsonElement> segments = element.getAsJsonObject().get("segments").getAsJsonArray()
                    .spliterator();
            String uid = StreamSupport.stream(segments, false).map(JsonElement::getAsString)
                    .collect(Collectors.joining(":"));
            object.addProperty(newName, uid);
