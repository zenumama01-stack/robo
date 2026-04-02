package org.openhab.core.internal.items;
 * A {@link StateDescriptionFragment} provider providing a default state pattern for items of type String,
 * DateTime and Number (with or without dimension).
 * @author Laurent Garnier - initial contribution
@Component(service = { StateDescriptionFragmentProvider.class,
        DefaultStateDescriptionFragmentProvider.class }, immediate = true, property = { "service.ranking:Integer=-2" })
public class DefaultStateDescriptionFragmentProvider implements StateDescriptionFragmentProvider {
    private static final StateDescriptionFragment DEFAULT_STRING = StateDescriptionFragmentBuilder.create()
            .withPattern("%s").build();
    private static final StateDescriptionFragment DEFAULT_DATETIME = StateDescriptionFragmentBuilder.create()
            .withPattern("%1$tY-%1$tm-%1$td %1$tH:%1$tM:%1$tS").build();
    private static final StateDescriptionFragment DEFAULT_NUMBER = StateDescriptionFragmentBuilder.create()
            .withPattern("%.0f").build();
    private static final StateDescriptionFragment DEFAULT_NUMBER_WITH_DIMENSION = StateDescriptionFragmentBuilder
            .create().withPattern("%.0f %unit%").build();
    private final Logger logger = LoggerFactory.getLogger(DefaultStateDescriptionFragmentProvider.class);
    private Integer rank = -2; // takes less precedence than all other providers
    public DefaultStateDescriptionFragmentProvider(Map<String, Object> properties) {
        if (serviceRanking instanceof Integer rankValue) {
            rank = rankValue;
    public void onItemAdded(Item item) {
        logger.trace("onItemAdded {} {}", item.getName(), item.getType());
        if (item instanceof GroupItem group) {
            Item baseItem = group.getBaseItem();
                onItemAdded(baseItem);
        } else if (item.getType().startsWith(CoreItemFactory.NUMBER + ":")) {
            stateDescriptionFragments.put(item.getName(), DEFAULT_NUMBER_WITH_DIMENSION);
            switch (item.getType()) {
                case CoreItemFactory.STRING:
                    stateDescriptionFragments.put(item.getName(), DEFAULT_STRING);
                case CoreItemFactory.DATETIME:
                    stateDescriptionFragments.put(item.getName(), DEFAULT_DATETIME);
                case CoreItemFactory.NUMBER:
                    stateDescriptionFragments.put(item.getName(), DEFAULT_NUMBER);
                    stateDescriptionFragments.remove(item.getName());
    public void onItemRemoved(Item item) {
        logger.trace("onItemRemoved {}", item.getName());
