 * Interface for builders for {@link ChannelType}s of kind STATE
public interface StateChannelTypeBuilder extends ChannelTypeBuilder<StateChannelTypeBuilder> {
     * Sets the {@link StateDescriptionFragment} for the {@link ChannelType}
     * @param stateDescriptionFragment StateDescriptionFragment for the ChannelType
    StateChannelTypeBuilder withStateDescriptionFragment(@Nullable StateDescriptionFragment stateDescriptionFragment);
     * Sets the {@link AutoUpdatePolicy} for the {@link ChannelType}
     * @param autoUpdatePolicy the AutoUpdatePolicy for the ChannelType
     * @return this builder
    StateChannelTypeBuilder withAutoUpdatePolicy(@Nullable AutoUpdatePolicy autoUpdatePolicy);
     * Sets the list of {@link CommandOption}s for the {@link ChannelType}
     * @param commandDescription the list of {@link CommandOption}s
    StateChannelTypeBuilder withCommandDescription(@Nullable CommandDescription commandDescription);
     * Sets the unitHint for the {@link ChannelType}
     * @param unitHint the unit hint
    StateChannelTypeBuilder withUnitHint(@Nullable String unitHint);
