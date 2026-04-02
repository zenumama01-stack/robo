import javax.xml.bind.JAXBContext;
import javax.xml.bind.JAXBException;
import javax.xml.bind.Unmarshaller;
import org.openhab.core.thing.internal.update.dto.XmlAddChannel;
import org.openhab.core.thing.internal.update.dto.XmlInstructionSet;
import org.openhab.core.thing.internal.update.dto.XmlThingType;
import org.openhab.core.thing.internal.update.dto.XmlUpdateChannel;
import org.openhab.core.thing.internal.update.dto.XmlUpdateDescriptions;
 * The {@link ThingUpdateInstructionReaderImpl} is an implementation of {@link ThingUpdateInstructionReader}
@Component(service = ThingUpdateInstructionReader.class)
public class ThingUpdateInstructionReaderImpl implements ThingUpdateInstructionReader {
    private final Logger logger = LoggerFactory.getLogger(ThingUpdateInstructionReaderImpl.class);
    public ThingUpdateInstructionReaderImpl(@Reference BundleResolver bundleResolver,
            @Reference ChannelTypeRegistry channelTypeRegistry,
            @Reference ConfigDescriptionRegistry configDescriptionRegistry) {
    public Map<UpdateInstructionKey, List<ThingUpdateInstruction>> readForFactory(ThingHandlerFactory factory) {
        Bundle bundle = bundleResolver.resolveBundle(factory.getClass());
                    "Could not get bundle for '{}', thing type updates will fail. If this occurs outside of tests, it is a bug.",
                    factory.getClass());
        Map<UpdateInstructionKey, List<ThingUpdateInstruction>> updateInstructions = new HashMap<>();
        Enumeration<URL> entries = bundle.findEntries("OH-INF/update", "*.xml", true);
            while (entries.hasMoreElements()) {
                URL url = entries.nextElement();
                    JAXBContext context = JAXBContext.newInstance(XmlUpdateDescriptions.class);
                    Unmarshaller u = context.createUnmarshaller();
                    XmlUpdateDescriptions updateDescriptions = (XmlUpdateDescriptions) u.unmarshal(url);
                    for (XmlThingType thingType : updateDescriptions.getThingType()) {
                        ThingTypeUID thingTypeUID = new ThingTypeUID(thingType.getUid());
                        UpdateInstructionKey key = new UpdateInstructionKey(factory, thingTypeUID);
                        List<ThingUpdateInstruction> instructions = new ArrayList<>();
                        List<XmlInstructionSet> instructionSets = thingType.getInstructionSet().stream()
                                .sorted(Comparator.comparing(XmlInstructionSet::getTargetVersion)).toList();
                        for (XmlInstructionSet instructionSet : instructionSets) {
                            int targetVersion = instructionSet.getTargetVersion();
                            for (Object instruction : instructionSet.getInstructions()) {
                                if (instruction instanceof XmlAddChannel addChannelType) {
                                    instructions.add(new UpdateChannelInstructionImpl(targetVersion, addChannelType,
                                            channelTypeRegistry, configDescriptionRegistry));
                                } else if (instruction instanceof XmlUpdateChannel updateChannelType) {
                                    instructions.add(new UpdateChannelInstructionImpl(targetVersion, updateChannelType,
                                } else if (instruction instanceof XmlRemoveChannel removeChannelType) {
                                    instructions
                                            .add(new RemoveChannelInstructionImpl(targetVersion, removeChannelType));
                                    logger.warn("Instruction type '{}' is unknown.", instruction.getClass());
                        updateInstructions.put(key, instructions);
                    logger.trace("Reading update instructions from '{}'", url.getPath());
                } catch (IllegalArgumentException | JAXBException e) {
                    logger.warn("Failed to parse update instructions from '{}':", url, e);
        return updateInstructions;
