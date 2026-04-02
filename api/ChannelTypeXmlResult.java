 * The {@link ChannelTypeXmlResult} is an intermediate XML conversion result object which
 * contains a {@link ChannelType} object.
public class ChannelTypeXmlResult {
    private ConfigDescription configDescription;
    private boolean system;
    public ChannelTypeXmlResult(ChannelType channelType, ConfigDescription configDescription) {
        this(channelType, configDescription, false);
    public ChannelTypeXmlResult(ChannelType channelType, ConfigDescription configDescription, boolean system) {
        this.configDescription = configDescription;
        this.system = system;
    public ChannelType toChannelType() {
    public ConfigDescription getConfigDescription() {
    public boolean isSystem() {
        return system;
        return "ChannelTypeXmlResult [channelType=" + channelType + ", configDescription=" + configDescription + "]";
