package org.openhab.core.model.thing.internal;
import org.eclipse.emf.common.util.BasicEList;
import org.openhab.core.model.thing.thing.ModelBridge;
import org.openhab.core.model.thing.thing.ModelThing;
import org.openhab.core.model.thing.thing.ThingModel;
import org.openhab.core.thing.binding.builder.BridgeBuilder;
 * Test the GenericThingProvider for different {@link ThingHandlerFactory}s for Bridge and Thing.
public class GenericThingProviderMultipleBundlesTest {
    private static final String BRIDGE_ID = "myBridge";
    private static final ThingTypeUID BRIDGE_TYPE_UID = new ThingTypeUID("test:bridge");
    private static final ThingUID BRIDGE_UID = new ThingUID(BRIDGE_TYPE_UID, BRIDGE_ID);
    private static final String THING_ID = "myThing";
    private static final ThingTypeUID THING_TYPE_UID = new ThingTypeUID("test:thing");
    private static final ThingUID THING_UID = new ThingUID(THING_TYPE_UID, THING_ID);
    private static final String BUNDLE_NAME = "myBundle";
    private static final String TEST_MODEL_THINGS = "testModel.things";
    private GenericThingProvider thingProvider;
    private ThingHandlerFactory bridgeHandlerFactory;
    private ThingHandlerFactory thingHandlerFactory;
        thingProvider = new GenericThingProvider();
        Bundle bundle = mock(Bundle.class);
        when(bundle.getSymbolicName()).thenReturn(BUNDLE_NAME);
        BundleResolver bundleResolver = mock(BundleResolver.class);
        when(bundleResolver.resolveBundle(any(Class.class))).thenReturn(bundle);
        thingProvider.setBundleResolver(bundleResolver);
        ModelRepository modelRepository = mock(ModelRepository.class);
        ThingModel thingModel = mock(ThingModel.class);
        EList<ModelThing> dslThings = createModelBridge();
        when(thingModel.getThings()).thenReturn(dslThings);
        when(modelRepository.getModel(TEST_MODEL_THINGS)).thenReturn(thingModel);
        thingProvider.setModelRepository(modelRepository);
        // configure bridgeHandlerFactory to accept the bridge type UID and create a bridge:
        bridgeHandlerFactory = mock(ThingHandlerFactory.class);
        when(bridgeHandlerFactory.supportsThingType(BRIDGE_TYPE_UID)).thenReturn(true);
        when(bridgeHandlerFactory.createThing(eq(BRIDGE_TYPE_UID), any(Configuration.class), eq(BRIDGE_UID), eq(null)))
                .thenReturn(BridgeBuilder.create(BRIDGE_TYPE_UID, BRIDGE_ID).build());
        thingProvider.addThingHandlerFactory(bridgeHandlerFactory);
        // configure thingHandlerFactory to accept the thing type UID and create a thing:
        thingHandlerFactory = mock(ThingHandlerFactory.class);
        when(thingHandlerFactory.supportsThingType(THING_TYPE_UID)).thenReturn(true);
        when(thingHandlerFactory.createThing(eq(THING_TYPE_UID), any(Configuration.class), eq(THING_UID),
                eq(BRIDGE_UID))).thenReturn(ThingBuilder.create(THING_TYPE_UID, THING_ID).build());
        thingProvider.addThingHandlerFactory(thingHandlerFactory);
    private EList<ModelThing> createModelBridge() {
        ModelBridge bridge = mock(ModelBridge.class);
        when(bridge.getId()).thenReturn(BRIDGE_UID.toString());
        when(bridge.getProperties()).thenReturn(new BasicEList<>(0));
        when(bridge.getChannels()).thenReturn(new BasicEList<>(0));
        EList<ModelThing> modelThings = createModelThing();
        when(bridge.getThings()).thenReturn(modelThings);
        BasicEList<ModelThing> result = new BasicEList<>();
        result.add(bridge);
    private EList<ModelThing> createModelThing() {
        ModelThing thing = mock(ModelThing.class);
        // simulate a nested defined thing model with only type id & id given:
        when(thing.getThingTypeId()).thenReturn("thing");
        when(thing.getThingId()).thenReturn(THING_ID);
        when(thing.getBridgeUID()).thenReturn(BRIDGE_UID.toString());
        when(thing.getProperties()).thenReturn(new BasicEList<>(0));
        when(thing.getChannels()).thenReturn(new BasicEList<>(0));
        result.add(thing);
    public void testDifferentHandlerFactoriesForBridgeAndThing() {
        thingProvider.onReadyMarkerAdded(new ReadyMarker("openhab.xmlThingTypes", BUNDLE_NAME));
        thingProvider.modelChanged(TEST_MODEL_THINGS, EventType.ADDED);
        verify(bridgeHandlerFactory).createThing(eq(BRIDGE_TYPE_UID), any(Configuration.class), eq(BRIDGE_UID),
                eq(null));
        verify(thingHandlerFactory).createThing(eq(THING_TYPE_UID), any(Configuration.class), eq(THING_UID),
                eq(BRIDGE_UID));
