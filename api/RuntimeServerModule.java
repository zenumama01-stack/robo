import org.eclipse.lsp4j.services.LanguageServer;
import org.eclipse.xtext.ide.ExecutorServiceProvider;
import org.eclipse.xtext.ide.server.DefaultProjectDescriptionFactory;
import org.eclipse.xtext.ide.server.ILanguageServerShutdownAndExitHandler;
import org.eclipse.xtext.ide.server.IMultiRootWorkspaceConfigFactory;
import org.eclipse.xtext.ide.server.IProjectDescriptionFactory;
import org.eclipse.xtext.ide.server.MultiRootWorkspaceConfigFactory;
import org.eclipse.xtext.ide.server.concurrent.IRequestManager;
import org.eclipse.xtext.ide.server.concurrent.RequestManager;
import org.eclipse.xtext.resource.IContainer;
import org.eclipse.xtext.resource.containers.ProjectDescriptionBasedContainerManager;
import com.google.inject.AbstractModule;
 * This class configures the injector for the Language Server.
public class RuntimeServerModule extends AbstractModule {
    public RuntimeServerModule(ScriptServiceUtil scriptServiceUtil, ScriptEngine scriptEngine) {
    protected void configure() {
        binder().bind(ExecutorService.class).toProvider(ExecutorServiceProvider.class);
        bind(UriExtensions.class).toInstance(new MappingUriExtensions(OpenHAB.getConfigFolder()));
        bind(IRequestManager.class).to(RequestManager.class);
        bind(LanguageServer.class).to(LanguageServerImpl.class);
        bind(IResourceServiceProvider.Registry.class).toProvider(new RegistryProvider(scriptServiceUtil, scriptEngine));
        bind(IMultiRootWorkspaceConfigFactory.class).to(MultiRootWorkspaceConfigFactory.class);
        bind(IProjectDescriptionFactory.class).to(DefaultProjectDescriptionFactory.class);
        bind(IContainer.Manager.class).to(ProjectDescriptionBasedContainerManager.class);
        bind(ILanguageServerShutdownAndExitHandler.class).to(ILanguageServerShutdownAndExitHandler.NullImpl.class);
