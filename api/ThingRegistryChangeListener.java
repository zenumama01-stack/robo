 * {@link ThingRegistryChangeListener} can be implemented to listen for things
 * being added or removed. The listener must be added and removed via
 * {@link ThingRegistry#addRegistryChangeListener} and
 * {@link ThingRegistry#removeRegistryChangeListener}.
 * @see ThingRegistry
public interface ThingRegistryChangeListener extends RegistryChangeListener<Thing> {
