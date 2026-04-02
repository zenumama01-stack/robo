package org.openhab.core.test;
 * @author Tanya Georgieva - Initial contribution
public class AsyncResultWrapper<T> {
    private @Nullable T wrappedObject;
    private boolean isSet = false;
    public void set(T wrappedObject) {
        this.wrappedObject = wrappedObject;
        isSet = true;
    public @Nullable T getWrappedObject() {
        return wrappedObject;
    public boolean isSet() {
        return isSet;
    public void reset() {
        wrappedObject = null;
        isSet = false;
