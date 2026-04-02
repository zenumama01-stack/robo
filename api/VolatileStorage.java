package org.openhab.core.test.storage;
 * A {@link Storage} implementation which stores it's data in-memory.
 * @author Kai Kreuzer - improved return values
public class VolatileStorage<T> implements Storage<T> {
    Map<String, T> storage = new ConcurrentHashMap<>();
        return storage.put(key, value);
        return storage.remove(key);
        return storage.containsKey(key);
        return storage.keySet();
        return storage.values();
