 * The {@link VolatileStorageService} returns {@link VolatileStorage}s
 * which stores their data in-memory.
public class VolatileStorageService implements StorageService {
    Map<String, Storage> storages = new ConcurrentHashMap<>();
    public synchronized <T> Storage<T> getStorage(String name) {
        Storage<T> storage = storages.get(name);
        if (storage == null) {
            storages.put(name, storage);
        return storage;
        return getStorage(name);
