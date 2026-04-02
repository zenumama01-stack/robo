 * Implementation of a safe EMF caller..
public class SafeEMFImpl implements SafeEMF {
    public synchronized <T> T call(Supplier<T> func) {
        return func.get();
    public synchronized void call(Runnable func) {
        func.run();
