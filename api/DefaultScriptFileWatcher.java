import org.openhab.core.automation.module.script.ScriptEngineManager;
import org.openhab.core.automation.module.script.rulesupport.loader.AbstractScriptFileWatcher;
import org.openhab.core.service.StartLevelService;
import org.openhab.core.service.WatchService;
 * The {@link DefaultScriptFileWatcher} watches the jsr223 directory for files. If a new/modified file is detected, the
 * script is read and passed to the {@link ScriptEngineManager}.
 * @author Jonathan Gilbert - initial contribution
public class DefaultScriptFileWatcher extends AbstractScriptFileWatcher {
    private static final String FILE_DIRECTORY = "automation" + File.separator + "jsr223";
    public DefaultScriptFileWatcher(
            final @Reference(target = WatchService.CONFIG_WATCHER_FILTER) WatchService watchService,
            final @Reference ScriptEngineManager manager, final @Reference ReadyService readyService,
            final @Reference StartLevelService startLevelService) {
        super(watchService, manager, readyService, startLevelService, FILE_DIRECTORY, true);
