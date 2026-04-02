package org.openhab.core.common.registry;
 * {@link AbstractManagedProvider} is an abstract implementation for the {@link ManagedProvider} interface and can be
 * used as base class for {@link ManagedProvider} implementations. It uses the {@link StorageService} to persist the
 * elements.
 * It provides the possibility to transform the element into another java class, that can be persisted. This is needed,
 * if the original element class is not directly persistable. If the element type can be persisted directly the
 * {@link DefaultAbstractManagedProvider} can be used as base class.
 *            type of the element
 * @param <K>
 *            type of the element key
 * @param <PE>
 *            type of the persistable element
public abstract class AbstractManagedProvider<@NonNull E extends Identifiable<K>, @NonNull K, @NonNull PE>
        extends AbstractProvider<E> implements ManagedProvider<E, K> {
    private volatile Storage<PE> storage;
    protected final Logger logger = LoggerFactory.getLogger(AbstractManagedProvider.class);
    protected AbstractManagedProvider(final StorageService storageService) {
        storage = storageService.getStorage(getStorageName(), this.getClass().getClassLoader());
    public void add(E element) {
        String keyAsString = getKeyAsString(element);
        if (storage.get(keyAsString) != null) {
                    "Cannot add element, because an element with same UID (" + keyAsString + ") already exists.");
        storage.put(keyAsString, toPersistableElement(element));
        logger.debug("Added new element {} to {}.", keyAsString, this.getClass().getSimpleName());
    public Collection<E> getAll() {
        return (Collection<E>) storage.getKeys().stream().map(this::getElement).filter(Objects::nonNull).toList();
    public @Nullable E get(K key) {
        return getElement(keyToString(key));
    private @Nullable E getElement(String key) {
        PE persistableElement = storage.get(key);
        return persistableElement != null ? toElement(key, persistableElement) : null;
    public @Nullable E remove(K key) {
        String keyAsString = keyToString(key);
        PE persistableElement = storage.remove(keyAsString);
        if (persistableElement != null) {
            E element = toElement(keyAsString, persistableElement);
                logger.debug("Removed element {} from {}.", keyAsString, this.getClass().getSimpleName());
    public @Nullable E update(E element) {
        String key = getKeyAsString(element);
        if (storage.get(key) != null) {
            PE persistableElement = storage.put(key, toPersistableElement(element));
                E oldElement = toElement(key, persistableElement);
                    oldElement = element;
                logger.debug("Updated element {} in {}.", key, this.getClass().getSimpleName());
            logger.warn("Could not update element with key {} in {}, because it does not exist.", key,
    private String getKeyAsString(E element) {
        return keyToString(element.getUID());
     * Returns the name of storage, that is used to persist the elements.
     * @return name of the storage
    protected abstract String getStorageName();
     * Transforms the key into a string representation.
     * @param key key
     * @return string representation of the key
    protected abstract String keyToString(K key);
     * Converts the persistable element into the original element.
     * @param persistableElement persistable element
     * @return original element
    protected abstract @Nullable E toElement(String key, PE persistableElement);
     * Converts the original element into an element that can be persisted.
     * @param element original element
     * @return persistable element
    protected abstract PE toPersistableElement(E element);
