 * The {@link ThingUpdateInstructionReader} is used to read instructions for a given {@link ThingHandlerFactory} and
 * create a list of {@link ThingUpdateInstruction}s
public interface ThingUpdateInstructionReader {
    Map<UpdateInstructionKey, List<ThingUpdateInstruction>> readForFactory(ThingHandlerFactory factory);
    record UpdateInstructionKey(ThingHandlerFactory factory, ThingTypeUID thingTypeId) {
