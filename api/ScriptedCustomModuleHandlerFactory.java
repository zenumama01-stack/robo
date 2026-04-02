 * The {@link ScriptedCustomModuleHandlerFactory} is used in combination with the
 * {@link ScriptedCustomModuleTypeProvider} to allow scripts to define custom types in the RuleManager. These
 * registered types can then be used publicly from any Rule-Editor.
 * This class provides the handlers from the script to the RuleManager. As Jsr223 languages have different needs, it
 * allows these handlers to be defined in different ways.
@Component(immediate = true, service = { ScriptedCustomModuleHandlerFactory.class, ModuleHandlerFactory.class })
public class ScriptedCustomModuleHandlerFactory extends AbstractScriptedModuleHandlerFactory {
    private final HashMap<String, ScriptedHandler> typesHandlers = new HashMap<>();
        return typesHandlers.keySet();
        ScriptedHandler scriptedHandler = typesHandlers.get(module.getTypeUID());
        return getModuleHandler(module, scriptedHandler);
    public void addModuleHandler(String uid, ScriptedHandler scriptedHandler) {
        typesHandlers.put(uid, scriptedHandler);
    public void removeModuleHandler(String uid) {
        typesHandlers.remove(uid);
