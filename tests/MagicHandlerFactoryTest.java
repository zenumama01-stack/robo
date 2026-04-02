 * Tests cases for {@link MagicHandlerFactory}.
public class MagicHandlerFactoryTest {
    private @NonNullByDefault({}) MagicHandlerFactory factory;
        factory = new MagicHandlerFactory(mock(MagicDynamicCommandDescriptionProvider.class),
                mock(MagicDynamicStateDescriptionProvider.class));
    public void shoudlReturnNullForUnknownThingTypeUID() {
        Thing thing = mock(Thing.class);
        when(thing.getThingTypeUID()).thenReturn(new ThingTypeUID("anyBinding:someThingType"));
        assertThat(factory.createHandler(thing), is(nullValue()));
    public void shoudlReturnColorLightHandler() {
        when(thing.getThingTypeUID()).thenReturn(MagicBindingConstants.THING_TYPE_COLOR_LIGHT);
        assertThat(factory.createHandler(thing), is(instanceOf(MagicColorLightHandler.class)));
    public void shoudlReturnDimmableLightHandler() {
        when(thing.getThingTypeUID()).thenReturn(MagicBindingConstants.THING_TYPE_DIMMABLE_LIGHT);
        assertThat(factory.createHandler(thing), is(instanceOf(MagicDimmableLightHandler.class)));
    public void shoudlReturnOnOffLightHandler() {
        when(thing.getThingTypeUID()).thenReturn(MagicBindingConstants.THING_TYPE_ON_OFF_LIGHT);
        assertThat(factory.createHandler(thing), is(instanceOf(MagicOnOffLightHandler.class)));
