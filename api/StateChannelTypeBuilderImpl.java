 * StateChannelTypeBuilder to create {@link ChannelType}s of kind STATE
public class StateChannelTypeBuilderImpl extends AbstractChannelTypeBuilder<StateChannelTypeBuilder>
        implements StateChannelTypeBuilder {
    private static class StateChannelTypeImpl extends ChannelType {
        private StateChannelTypeImpl(ChannelTypeUID uid, boolean advanced, String itemType, @Nullable String unitHint,
                String label, @Nullable String description, @Nullable String category, @Nullable Set<String> tags,
                @Nullable StateDescription state, @Nullable CommandDescription commandDescription,
                @Nullable URI configDescriptionURI, @Nullable AutoUpdatePolicy autoUpdatePolicy)
            super(uid, advanced, itemType, unitHint, ChannelKind.STATE, label, description, category, tags, state,
                    commandDescription, null, configDescriptionURI, autoUpdatePolicy);
    private final String itemType;
    private @Nullable String unitHint;
    private @Nullable StateDescriptionFragment stateDescriptionFragment;
    private @Nullable CommandDescription commandDescription;
    public StateChannelTypeBuilderImpl(ChannelTypeUID channelTypeUID, String label, String itemType) {
        super(channelTypeUID, label);
        if (itemType.isBlank()) {
            throw new IllegalArgumentException("Supported itemType for a ChannelType must not be empty.");
    public StateChannelTypeBuilder withUnitHint(@Nullable String unitHint) {
    public StateChannelTypeBuilder withStateDescriptionFragment(
            @Nullable StateDescriptionFragment stateDescriptionFragment) {
        this.stateDescriptionFragment = stateDescriptionFragment;
    public StateChannelTypeBuilder withAutoUpdatePolicy(@Nullable AutoUpdatePolicy autoUpdatePolicy) {
    public StateChannelTypeBuilder withCommandDescription(@Nullable CommandDescription commandDescription) {
    public ChannelType build() {
        return new StateChannelTypeImpl(channelTypeUID, advanced, itemType, unitHint, label, description, category,
                tags, stateDescriptionFragment != null ? stateDescriptionFragment.toStateDescription() : null,
                commandDescription, configDescriptionURI, autoUpdatePolicy);
