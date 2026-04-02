 * The {@link ChannelGroupTypeUID} represents a unique identifier for channel group types.
 * @author Michael Grammling - Initial contribution.
public class ChannelGroupTypeUID extends UID {
     * @param channelGroupUid the UID for the channel group
    public ChannelGroupTypeUID(String channelGroupUid) {
     * @param bindingId the binding ID
     * @param id the identifier of the channel group
    public ChannelGroupTypeUID(String bindingId, String id) {
        super(bindingId, id);
     * @return the identifier of the channel group (neither null, nor empty)
