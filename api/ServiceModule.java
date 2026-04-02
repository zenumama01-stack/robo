import org.openhab.core.model.script.engine.IActionServiceProvider;
import org.openhab.core.model.script.engine.IThingActionsProvider;
import org.openhab.core.model.script.internal.engine.ServiceTrackerActionServiceProvider;
import org.openhab.core.model.script.internal.engine.ServiceTrackerThingActionsProvider;
import org.openhab.core.model.script.script.impl.ScriptImpl;
import com.google.inject.Binder;
import com.google.inject.Module;
 * Guice module that binds openHAB services
public class ServiceModule implements Module {
    public ServiceModule(ScriptServiceUtil scriptServiceUtil, ScriptEngine scriptEngine) {
    public void configure(Binder binder) {
        binder.bind(ItemRegistry.class).toInstance(scriptServiceUtil.getItemRegistryInstance());
        binder.bind(ThingRegistry.class).toInstance(scriptServiceUtil.getThingRegistryInstance());
        binder.bind(ModelRepository.class).toInstance(scriptServiceUtil.getModelRepositoryInstance());
        binder.bind(ScriptEngine.class).toInstance(scriptEngine);
        binder.bind(IActionServiceProvider.class)
                .toInstance(new ServiceTrackerActionServiceProvider(scriptServiceUtil));
        binder.bind(IThingActionsProvider.class).toInstance(new ServiceTrackerThingActionsProvider(scriptServiceUtil));
        binder.bind(Script.class).to(ScriptImpl.class);
