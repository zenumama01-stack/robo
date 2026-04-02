 * {@link DefaultAbstractManagedProvider} is a specific {@link AbstractManagedProvider} implementation, where the stored
 * element is
 * the same as the element of the provider. So no transformation is needed.
 * Therefore only two generic parameters are needed instead of three.
public abstract class DefaultAbstractManagedProvider<@NonNull E extends Identifiable<K>, @NonNull K>
        extends AbstractManagedProvider<E, K, E> {
    public DefaultAbstractManagedProvider(final StorageService storageService) {
    protected @Nullable E toElement(String key, E element) {
    protected E toPersistableElement(E element) {
