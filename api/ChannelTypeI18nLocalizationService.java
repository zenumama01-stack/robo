import org.openhab.core.thing.type.TriggerChannelTypeBuilder;
 * This OSGi service could be used to localize a {@link ChannelType} using the I18N mechanism of the openHAB
@Component(service = ChannelTypeI18nLocalizationService.class)
public class ChannelTypeI18nLocalizationService {
    public ChannelTypeI18nLocalizationService(final @Reference TranslationProvider i18nProvider) {
    public @Nullable String createLocalizedStatePattern(final Bundle bundle, String pattern,
            final ChannelTypeUID channelTypeUID, final @Nullable Locale locale) {
        return thingTypeI18nUtil.getChannelStatePattern(bundle, channelTypeUID, pattern, locale);
    public List<StateOption> createLocalizedStateOptions(final Bundle bundle, List<StateOption> stateOptions,
        List<StateOption> localizedOptions = new ArrayList<>();
        for (final StateOption stateOption : stateOptions) {
            String optionLabel = stateOption.getLabel();
            if (optionLabel != null) {
                optionLabel = thingTypeI18nUtil.getChannelStateOption(bundle, channelTypeUID, stateOption.getValue(),
                        optionLabel, locale);
            localizedOptions.add(new StateOption(stateOption.getValue(), optionLabel));
    public @Nullable StateDescriptionFragment createLocalizedStateDescriptionFragment(final Bundle bundle,
            final @Nullable StateDescription state, final ChannelTypeUID channelTypeUID,
        String localizedPattern = state.getPattern();
            localizedPattern = createLocalizedStatePattern(bundle, localizedPattern, channelTypeUID, locale);
        List<StateOption> localizedOptions = createLocalizedStateOptions(bundle, state.getOptions(), channelTypeUID,
        StateDescriptionFragmentBuilder builder = StateDescriptionFragmentBuilder.create(state);
        return builder.withOptions(localizedOptions).build();
    public List<CommandOption> createLocalizedCommandOptions(final Bundle bundle, List<CommandOption> commandOptions,
        List<CommandOption> localizedOptions = new ArrayList<>();
        for (final CommandOption commandOption : commandOptions) {
            String optionLabel = commandOption.getLabel();
                optionLabel = thingTypeI18nUtil.getChannelCommandOption(bundle, channelTypeUID,
                        commandOption.getCommand(), optionLabel, locale);
            localizedOptions.add(new CommandOption(commandOption.getCommand(), optionLabel));
    public @Nullable CommandDescription createLocalizedCommandDescription(final Bundle bundle,
            final @Nullable CommandDescription command, final ChannelTypeUID channelTypeUID,
        if (command == null) {
        List<CommandOption> localizedOptions = createLocalizedCommandOptions(bundle, command.getCommandOptions(),
        CommandDescriptionBuilder commandDescriptionBuilder = CommandDescriptionBuilder.create();
        return commandDescriptionBuilder.withCommandOptions(localizedOptions).build();
    public ChannelType createLocalizedChannelType(Bundle bundle, ChannelType channelType, @Nullable Locale locale) {
        ChannelTypeUID channelTypeUID = channelType.getUID();
        String defaultLabel = channelType.getLabel();
        String label = thingTypeI18nUtil.getChannelLabel(bundle, channelTypeUID, defaultLabel, locale);
        String description = thingTypeI18nUtil.getChannelDescription(bundle, channelTypeUID,
                channelType.getDescription(), locale);
        final ChannelTypeBuilder<?> builder;
        switch (channelType.getKind()) {
            case STATE:
                StateDescriptionFragment stateDescriptionFragment = createLocalizedStateDescriptionFragment(bundle,
                        channelType.getState(), channelTypeUID, locale);
                CommandDescription command = createLocalizedCommandDescription(bundle,
                        channelType.getCommandDescription(), channelTypeUID, locale);
                    throw new IllegalArgumentException("If the kind is 'state', the item type must be set!");
                String unitHint = channelType.getUnitHint();
                builder = ChannelTypeBuilder.state(channelTypeUID, label == null ? defaultLabel : label, itemType)
                        .withUnitHint(unitHint).withStateDescriptionFragment(stateDescriptionFragment)
                        .withAutoUpdatePolicy(channelType.getAutoUpdatePolicy()).withCommandDescription(command);
            case TRIGGER:
                EventDescription eventDescription = channelType.getEvent();
                TriggerChannelTypeBuilder triggerBuilder = ChannelTypeBuilder.trigger(channelTypeUID,
                        label == null ? defaultLabel : label);
                if (eventDescription != null) {
                    triggerBuilder.withEventDescription(eventDescription);
                builder = triggerBuilder;
                throw new IllegalArgumentException("Kind must not be null or empty!");
        String category = channelType.getCategory();
        URI configDescriptionURI = channelType.getConfigDescriptionURI();
            builder.withConfigDescriptionURI(configDescriptionURI);
        return builder.isAdvanced(channelType.isAdvanced()).withTags(channelType.getTags()).build();
