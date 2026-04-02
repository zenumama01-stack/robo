 * {@link RegistryChangedRunnableListener} can be added to {@link Registry} services, to execute a given
 * {@link Runnable} on all types of changes.
public class RegistryChangedRunnableListener<E> implements RegistryChangeListener<E> {
    final Runnable runnable;
    public RegistryChangedRunnableListener(Runnable runnable) {
    public void added(E element) {
    public void removed(E element) {
    public void updated(E oldElement, E newElement) {
