 * {@link ThingHelper} provides a utility method to create and bind items.
 * @author Andre Fuechsel - graceful creation of items and links
 *         ThingTypeDescription
 * @author Dennis Nobel - Removed createAndBindItems method
 * @author Kai Kreuzer - Added merge method
public class ThingHelper {
     * Indicates whether two {@link Thing}s are technical equal.
     * @param a Thing object
     * @param b another Thing object
     * @return true whether a and b are equal, otherwise false
    public static boolean equals(Thing a, Thing b) {
        if (!a.getUID().equals(b.getUID())) {
        // bridge
        if (!Objects.equals(a.getBridgeUID(), b.getBridgeUID())) {
        // configuration
        if (!Objects.equals(a.getConfiguration(), b.getConfiguration())) {
        // label
        if (!Objects.equals(a.getLabel(), b.getLabel())) {
        if (!Objects.equals(a.getLocation(), b.getLocation())) {
        // semantic equipment tag
        if (!Objects.equals(a.getSemanticEquipmentTag(), b.getSemanticEquipmentTag())) {
        // channels
        Set<Channel> channelsOfA = new HashSet<>(a.getChannels());
        Set<Channel> channelsOfB = new HashSet<>(b.getChannels());
        return channelsOfA.equals(channelsOfB);
    public static void addChannelsToThing(Thing thing, Collection<Channel> channels) {
        Collection<Channel> mutableChannels = thing.getChannels();
        ensureUniqueChannels(mutableChannels, channels);
            ((ThingImpl) thing).addChannel(channel);
     * Ensures that there are no duplicate channels in the array (i.e. not using the same ChannelUID)
     * @param channels the channels to check
     * @throws IllegalArgumentException in case there are duplicate channels found
    public static void ensureUniqueChannels(final Channel[] channels) {
        ensureUniqueChannels(Arrays.stream(channels).iterator(), new HashSet<>(channels.length));
     * Ensures that there are no duplicate channels in the collection (i.e. not using the same ChannelUID)
    public static void ensureUniqueChannels(final Collection<Channel> channels) {
        ensureUniqueChannels(channels.iterator(), new HashSet<>(channels.size()));
     * Ensures that there are no duplicate channels in the collection plus the additional one (i.e. not using the same
     * ChannelUID)
     * @param channels the {@link List} of channels to check
     * @param channel an additional channel
    public static void ensureUniqueChannels(final Collection<Channel> channels, final Channel channel) {
        ensureUniqueChannels(channels, Set.of(channel));
    private static void ensureUniqueChannels(final Collection<Channel> channels1, final Collection<Channel> channels2) {
        ensureUniqueChannels(channels1.iterator(),
                ensureUniqueChannels(channels2.iterator(), new HashSet<>(channels1.size() + channels2.size())));
    private static Set<UID> ensureUniqueChannels(final Iterator<Channel> channels, final Set<UID> ids) {
        while (channels.hasNext()) {
            final Channel channel = channels.next();
            if (!ids.add(channel.getUID())) {
                throw new IllegalArgumentException("Duplicate channels " + channel.getUID().getAsString());
     * Merges the content of a ThingDTO with an existing Thing.
     * Where ever the DTO has null values, the content of the original Thing is kept.
     * Where ever the DTO has non-null values, these are used.
     * In consequence, care must be taken when the content of a list (like configuration, properties or channels) is to
     * be updated - the DTO must contain the full list, otherwise entries will be deleted.
     * @param thing the Thing instance to merge the new content into
     * @param updatedContents a DTO which carries the updated content
     * @return A Thing instance, which is the result of the merge
    public static Thing merge(Thing thing, ThingDTO updatedContents) {
        ThingBuilder builder;
            builder = BridgeBuilder.create(thing.getThingTypeUID(), thing.getUID());
            builder = ThingBuilder.create(thing.getThingTypeUID(), thing.getUID());
        // Update the label
        if (updatedContents.label != null) {
            builder.withLabel(updatedContents.label);
            builder.withLabel(thing.getLabel());
        // Update the location
        if (updatedContents.location != null) {
            builder.withLocation(updatedContents.location);
            builder.withLocation(thing.getLocation());
        // update bridge UID
        if (updatedContents.bridgeUID != null) {
            builder.withBridge(new ThingUID(updatedContents.bridgeUID));
            builder.withBridge(thing.getBridgeUID());
        // update thing configuration
        if (updatedContents.configuration != null && !updatedContents.configuration.keySet().isEmpty()) {
            builder.withConfiguration(new Configuration(updatedContents.configuration));
            builder.withConfiguration(thing.getConfiguration());
        // update thing properties
        if (updatedContents.properties != null) {
            builder.withProperties(updatedContents.properties);
            builder.withProperties(thing.getProperties());
        // Update the channels
        if (updatedContents.channels != null) {
            for (ChannelDTO channelDTO : updatedContents.channels) {
                builder.withChannel(ChannelDTOMapper.map(channelDTO));
            builder.withChannels(thing.getChannels());
        // Update the semantic equipment tag
        if (updatedContents.semanticEquipmentTag != null) {
            builder.withSemanticEquipmentTag(updatedContents.semanticEquipmentTag);
            builder.withSemanticEquipmentTag(thing.getSemanticEquipmentTag());
        Thing mergedThing = builder.build();
        // keep all child things in place on a merged bridge
        if (mergedThing instanceof BridgeImpl mergedBridge && thing instanceof Bridge bridge) {
            for (Thing child : bridge.getThings()) {
                mergedBridge.addThing(child);
        return mergedThing;
