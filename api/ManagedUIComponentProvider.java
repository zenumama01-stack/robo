package org.openhab.core.ui.internal.components;
import org.openhab.core.ui.components.UIComponentProvider;
 * A namespace-specific {@link ManagedProvider} for UI components.
 * @author Jan N. Klug - Refactored to component factory
@Component(factory = "org.openhab.core.ui.component.provider.factory")
public class ManagedUIComponentProvider extends AbstractProvider<RootUIComponent>
        implements ManagedProvider<RootUIComponent, String>, UIComponentProvider {
    private final String namespace;
    private final Storage<RootUIComponent> storage;
    public ManagedUIComponentProvider(@Reference StorageService storageService, Map<String, Object> config) {
        String namespace = ConfigParser.valueAs(config.get(CONFIG_NAMESPACE), String.class);
            throw new IllegalStateException("'ui.namespace' must not be null in service configuration");
        this.storage = storageService.getStorage("uicomponents_" + namespace.replace(':', '_'),
    public Collection<RootUIComponent> getAll() {
        List<RootUIComponent> components = new ArrayList<>();
        for (RootUIComponent component : storage.getValues()) {
            if (component != null) {
                components.add(component);
    public void add(RootUIComponent element) {
        if (element.getUID().isEmpty()) {
            throw new IllegalArgumentException("Invalid UID");
        if (storage.get(element.getUID()) != null) {
            throw new IllegalArgumentException("Cannot add UI component to namespace " + namespace
                    + ", because a component with same UID (" + element.getUID() + ") already exists.");
        storage.put(element.getUID(), element);
        notifyListenersAboutAddedElement(element);
    public @Nullable RootUIComponent remove(String key) {
        RootUIComponent element = storage.remove(key);
            notifyListenersAboutRemovedElement(element);
    public @Nullable RootUIComponent update(RootUIComponent element) {
            RootUIComponent oldElement = storage.put(element.getUID(), element);
                notifyListenersAboutUpdatedElement(oldElement, element);
                return oldElement;
            throw new IllegalArgumentException("Cannot update UI component " + element.getUID() + " in namespace "
                    + namespace + " because it doesn't exist.");
    public @Nullable RootUIComponent get(String key) {
        if (key.isEmpty()) {
        return namespace;
