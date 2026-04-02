package org.openhab.core.model.script.scoping;
 * This is a special class loader that tries to resolve classes from available {@link ActionService}s,
 * if the class cannot be resolved from the normal classpath.
public final class ActionClassLoader extends ClassLoader {
    public ActionClassLoader(ClassLoader cl) {
        super(cl);
    public Class<?> loadClass(String name) throws ClassNotFoundException {
            return getParent().loadClass(name);
            for (ActionService actionService : ScriptServiceUtil.getActionServices()) {
                if (actionService.getActionClassName().equals(name)) {
                    return actionService.getActionClass();
            for (ThingActions actions : ScriptServiceUtil.getThingActions()) {
                if (actions.getClass().getName().equals(name)) {
                    return actions.getClass();
        throw new ClassNotFoundException();
