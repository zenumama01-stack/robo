 * {@link ThingTypeI18nUtil} uses the {@link TranslationProvider} to resolve
 * the localized texts. It automatically infers the key if the default text is
 * not a constant.
 * @author Laurent Garnier - add translation for channel group label and channel group description
 * @author Christoph Weitkamp - fix localized label and description for channel definition
public class ThingTypeI18nUtil {
    public ThingTypeI18nUtil(TranslationProvider i18nProvider) {
    public @Nullable String getChannelDescription(Bundle bundle, ChannelTypeUID channelTypeUID,
        String key = I18nUtil.stripConstantOr(defaultDescription, () -> inferChannelKey(channelTypeUID, "description"));
    public @Nullable String getChannelDescription(Bundle bundle, ThingTypeUID thingTypeUID, ChannelDefinition channel,
                () -> inferThingTypeKey(thingTypeUID, channel, "description"));
    public @Nullable String getChannelGroupDescription(Bundle bundle, ChannelGroupTypeUID channelGroupTypeUID,
                () -> inferChannelGroupKey(channelGroupTypeUID, "description"));
    public @Nullable String getChannelDescription(Bundle bundle, ChannelGroupTypeUID channelGroupTypeUID,
            ChannelDefinition channel, @Nullable String defaultLabel, @Nullable Locale locale) {
                () -> inferChannelGroupKey(channelGroupTypeUID, channel, "description"));
    public @Nullable String getChannelGroupDescription(Bundle bundle, ThingTypeUID thingTypeUID,
            ChannelGroupDefinition channelGroup, @Nullable String defaultDescription, @Nullable Locale locale) {
                () -> inferThingTypeKey(thingTypeUID, channelGroup, "description"));
    public @Nullable String getChannelLabel(Bundle bundle, ChannelTypeUID channelTypeUID, @Nullable String defaultLabel,
        String key = I18nUtil.stripConstantOr(defaultLabel, () -> inferChannelKey(channelTypeUID, "label"));
    public @Nullable String getChannelLabel(Bundle bundle, ChannelGroupTypeUID channelGroupTypeUID,
                () -> inferChannelGroupKey(channelGroupTypeUID, channel, "label"));
    public @Nullable String getChannelLabel(Bundle bundle, ThingTypeUID thingTypeUID, ChannelDefinition channel,
        String key = I18nUtil.stripConstantOr(defaultLabel, () -> inferThingTypeKey(thingTypeUID, channel, "label"));
    public @Nullable String getChannelGroupLabel(Bundle bundle, ChannelGroupTypeUID channelGroupTypeUID,
        String key = I18nUtil.stripConstantOr(defaultLabel, () -> inferChannelGroupKey(channelGroupTypeUID, "label"));
    public @Nullable String getChannelGroupLabel(Bundle bundle, ThingTypeUID thingTypeUID,
            ChannelGroupDefinition channelGroup, @Nullable String defaultLabel, @Nullable Locale locale) {
                () -> inferThingTypeKey(thingTypeUID, channelGroup, "label"));
    public @Nullable String getChannelStateOption(Bundle bundle, ChannelTypeUID channelTypeUID, String optionValue,
            @Nullable String defaultOptionLabel, @Nullable Locale locale) {
        String key = I18nUtil.stripConstantOr(defaultOptionLabel,
                () -> inferChannelKey(channelTypeUID, "state.option." + optionValue));
        return i18nProvider.getText(bundle, key, defaultOptionLabel, locale);
    public @Nullable String getChannelCommandOption(Bundle bundle, ChannelTypeUID channelTypeUID, String optionValue,
                () -> inferChannelKey(channelTypeUID, "command.option." + optionValue));
    public @Nullable String getChannelStatePattern(Bundle bundle, ChannelTypeUID channelTypeUID,
        String key = I18nUtil.stripConstantOr(defaultPattern, () -> inferChannelKey(channelTypeUID, "state.pattern"));
        return i18nProvider.getText(bundle, key, defaultPattern, locale);
    public @Nullable String getDescription(Bundle bundle, ThingTypeUID thingTypeUID,
        String key = I18nUtil.stripConstantOr(defaultDescription, () -> inferThingTypeKey(thingTypeUID, "description"));
    public @Nullable String getLabel(Bundle bundle, ThingTypeUID thingTypeUID, @Nullable String defaultLabel,
        String key = I18nUtil.stripConstantOr(defaultLabel, () -> inferThingTypeKey(thingTypeUID, "label"));
    private String inferChannelKey(ChannelTypeUID channelTypeUID, String lastSegment) {
        return "channel-type." + channelTypeUID.getBindingId() + "." + channelTypeUID.getId() + "." + lastSegment;
    private String inferChannelGroupKey(ChannelGroupTypeUID channelGroupTypeUID, String lastSegment) {
        return "channel-group-type." + channelGroupTypeUID.getBindingId() + "." + channelGroupTypeUID.getId() + "."
                + lastSegment;
    private String inferChannelGroupKey(ChannelGroupTypeUID channelGroupTypeUID, ChannelDefinition channel,
        return "channel-group-type." + channelGroupTypeUID.getBindingId() + "." + channelGroupTypeUID.getId()
                + ".channel." + channel.getId() + "." + lastSegment;
    private String inferThingTypeKey(ThingTypeUID thingTypeUID, String lastSegment) {
        return "thing-type." + thingTypeUID.getBindingId() + "." + thingTypeUID.getId() + "." + lastSegment;
    private String inferThingTypeKey(ThingTypeUID thingTypeUID, ChannelGroupDefinition channelGroup,
        return "thing-type." + thingTypeUID.getBindingId() + "." + thingTypeUID.getId() + ".group."
                + channelGroup.getId() + "." + lastSegment;
    private String inferThingTypeKey(ThingTypeUID thingTypeUID, ChannelDefinition channel, String lastSegment) {
        return "thing-type." + thingTypeUID.getBindingId() + "." + thingTypeUID.getId() + ".channel." + channel.getId()
                + "." + lastSegment;
