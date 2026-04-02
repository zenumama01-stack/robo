 * The {@link ThingTypeXmlProvider} is responsible managing any created {@link ThingType} objects by a
 * {@link ThingDescriptionReader} for a certain
 * bundle.
 * This implementation registers each {@link ThingType} object at the {@link ThingTypeProvider} which is itself
 * registered as service at the <i>OSGi</i> service registry. If a configuration section is found, a
 * {@link ConfigDescription} object is registered at the {@link ConfigDescriptionProvider} which is itself registered as
 * service at the <i>OSGi</i> service registry.
 * The {@link ThingTypeXmlProvider} uses an internal cache consisting of {@link #thingTypeRefs},
 * {@link #channelGroupTypeRefs}, {@link #channelTypeRefs}. This cache is used to merge
 * first the {@link ChannelType} definitions with the {@link ChannelGroupTypeXmlResult} objects to create valid
 * {@link ChannelGroupType} objects. After that the {@link ChannelType} and the {@link ChannelGroupType} definitions are
 * used to merge with the {@link ThingTypeXmlResult} objects to create valid {@link ThingType} objects. After the merge
 * process has been finished, the cache is cleared again. The merge process is started when {@link #addingFinished()} is
 * invoked from the according {@link XmlDocumentBundleTracker}.
public class ThingTypeXmlProvider implements XmlDocumentProvider<List<?>> {
    private final Logger logger = LoggerFactory.getLogger(ThingTypeXmlProvider.class);
    private final XmlThingTypeProvider thingTypeProvider;
    // temporary cache
    private final List<ThingTypeXmlResult> thingTypeRefs = new ArrayList<>(10);
    private final List<ChannelGroupTypeXmlResult> channelGroupTypeRefs = new ArrayList<>(10);
    private final List<ChannelTypeXmlResult> channelTypeRefs = new ArrayList<>(10);
    private final XmlChannelTypeProvider channelTypeProvider;
    private final XmlChannelGroupTypeProvider channelGroupTypeProvider;
    public ThingTypeXmlProvider(Bundle bundle, AbstractXmlConfigDescriptionProvider configDescriptionProvider,
            XmlThingTypeProvider thingTypeProvider, XmlChannelTypeProvider channelTypeProvider,
            XmlChannelGroupTypeProvider channelGroupTypeProvider) throws IllegalArgumentException {
        this.thingTypeProvider = thingTypeProvider;
        this.channelTypeProvider = channelTypeProvider;
        this.channelGroupTypeProvider = channelGroupTypeProvider;
    public synchronized void addingObject(List<?> types) {
        for (Object type : types) {
            if (type instanceof ThingTypeXmlResult typeResult) {
                addConfigDescription(typeResult.getConfigDescription());
                thingTypeRefs.add(typeResult);
            } else if (type instanceof ChannelGroupTypeXmlResult typeResult) {
                channelGroupTypeRefs.add(typeResult);
            } else if (type instanceof ChannelTypeXmlResult typeResult) {
                channelTypeRefs.add(typeResult);
                throw new ConversionException("Unknown data type for '" + type + "'!");
    private void addConfigDescription(@Nullable ConfigDescription configDescription) {
                logger.error("Could not register ConfigDescription: {}", configDescription.getUID(), e);
    public synchronized void addingFinished() {
        // create channel types
        for (ChannelTypeXmlResult type : channelTypeRefs) {
            ChannelType channelType = type.toChannelType();
                channelTypeProvider.add(bundle, channelType);
                logger.error("Could not register ChannelType: {}", channelType.getUID(), e);
        // create channel group types
        for (ChannelGroupTypeXmlResult type : channelGroupTypeRefs) {
                channelGroupTypeProvider.add(bundle, type.toChannelGroupType());
                logger.error("Could not register ChannelGroupType: {}", type.getUID(), e);
        // create thing and bridge types
        for (ThingTypeXmlResult type : thingTypeRefs) {
                thingTypeProvider.add(bundle, type.toThingType());
                logger.error("Could not register ThingType: {}", type.getUID(), e);
        // release temporary cache
        thingTypeRefs.clear();
        channelGroupTypeRefs.clear();
        channelTypeRefs.clear();
        thingTypeProvider.removeAll(bundle);
        channelGroupTypeProvider.removeAll(bundle);
        channelTypeProvider.removeAll(bundle);
        configDescriptionProvider.removeAll(bundle);
