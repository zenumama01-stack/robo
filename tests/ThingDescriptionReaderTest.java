 * Tests reading thing descriptions from XML using the {@link ThingDescriptionReader}.
public class ThingDescriptionReaderTest {
    public void readFromXML() throws Exception {
        URL url = Path.of("src/test/resources/thing/thing-types.xml").toUri().toURL();
        ThingDescriptionReader reader = new ThingDescriptionReader();
        List<?> types = Objects.requireNonNull(reader.readFromXML(url));
        List<ThingTypeXmlResult> thingTypeXmlResults = new ArrayList<>();
        List<ChannelGroupTypeXmlResult> channelGroupTypeXmlResults = new ArrayList<>();
        List<ChannelTypeXmlResult> channelTypeXmlResults = new ArrayList<>();
            if (type instanceof ThingTypeXmlResult result) {
                thingTypeXmlResults.add(result);
            } else if (type instanceof ChannelGroupTypeXmlResult result) {
                channelGroupTypeXmlResults.add(result);
            } else if (type instanceof ChannelTypeXmlResult result) {
                channelTypeXmlResults.add(result);
        assertThat(thingTypeXmlResults.size(), is(1));
        ThingTypeXmlResult thingTypeXmlResult = thingTypeXmlResults.getFirst();
        assertThat(thingTypeXmlResult.getUID().toString(), is("hue:lamp"));
        assertThat(thingTypeXmlResult.label, is("HUE Lamp"));
        assertThat(thingTypeXmlResult.description, is("My own great HUE Lamp."));
        assertThat(thingTypeXmlResult.semanticEquipmentTag, is("LightBulb"));
        List<String> supportedBridgeTypeUIDs = thingTypeXmlResult.getBuilder().build().getSupportedBridgeTypeUIDs();
        assertThat(supportedBridgeTypeUIDs.size(), is(2));
        assertTrue(supportedBridgeTypeUIDs.contains("hue:bridge"));
        assertTrue(supportedBridgeTypeUIDs.contains("mqtt:broker"));
        assertThat(channelGroupTypeXmlResults.size(), is(1));
        ChannelGroupTypeXmlResult channelGroupTypeXmlResult = channelGroupTypeXmlResults.getFirst();
        ChannelGroupType channelGroupType = channelGroupTypeXmlResult.toChannelGroupType();
        assertThat(channelGroupTypeXmlResult.getUID().toString(), is("hue:alarm_system"));
        assertThat(channelGroupType.getLabel(), is("Alarm System"));
        assertThat(channelGroupType.getDescription(), is("The alarm system."));
        assertThat(channelTypeXmlResults.size(), is(5));
        ChannelType channelType = channelTypeXmlResults.getFirst().toChannelType();
        assertThat(channelType.getUID().toString(), is("hue:color"));
        assertThat(channelType.getItemType(), is("Color"));
        assertThat(channelType.getLabel(), is("Color"));
        assertThat(channelType.getDescription(), is("The color channel allows to control the color of a light. "
                + "It is also possible to dim values and switch the light on and off."));
        assertThat(channelType.getCategory(), is("ColorLight"));
        assertThat(channelType.getTags().size(), is(2));
        assertTrue(channelType.getTags().contains("Control"));
        assertTrue(channelType.getTags().contains("Light"));
        channelType = channelTypeXmlResults.get(1).toChannelType();
        assertThat(channelType.getUID().toString(), is("hue:brightness"));
        assertThat(channelType.getItemType(), is("Dimmer"));
        assertThat(channelType.getLabel(), is("Brightness"));
        assertThat(channelType.getDescription(),
                is("The brightness channel allows to control the brightness of a light. "
                        + "It is also possible to switch the light on and off."));
        assertThat(channelType.getCategory(), is("Light"));
        channelType = channelTypeXmlResults.get(2).toChannelType();
        assertThat(channelType.getUID().toString(), is("hue:color_temperature"));
        assertThat(channelType.getLabel(), is("Color Temperature"));
        assertThat(channelType.getDescription(), is(
                "The color temperature channel allows to set the color temperature of a light from 0 (cold) to 100 (warm)."));
        assertThat(channelType.getCategory(), is(nullValue()));
        assertTrue(channelType.getTags().contains("ColorTemperature"));
        channelType = channelTypeXmlResults.get(3).toChannelType();
        assertThat(channelType.getUID().toString(), is("hue:alarm"));
        assertThat(channelType.getItemType(), is("Number"));
        assertThat(channelType.getLabel(), is("Alarm System"));
        assertThat(channelType.getDescription(), is("The light blinks if alarm is set."));
        assertThat(channelType.getTags().size(), is(0));
        channelType = channelTypeXmlResults.get(4).toChannelType();
        assertThat(channelType.getUID().toString(), is("hue:motion"));
        assertThat(channelType.getItemType(), is(nullValue()));
        assertThat(channelType.getLabel(), is("Motion Sensor"));
        assertThat(channelType.getDescription(), is("The sensor detecting motion."));
        assertThat(channelType.getCategory(), is("Motion"));
        assertTrue(channelType.getTags().contains("Status"));
        assertTrue(channelType.getTags().contains("Presence"));
