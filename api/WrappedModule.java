public class WrappedModule<@NonNull M extends Module, H extends ModuleHandler> {
    private final M module;
    private @Nullable H handler;
    protected WrappedModule(final M module) {
    public M unwrap() {
     * This method gets handler which is responsible for handling of this module.
     * @return handler of the module or null.
    public @Nullable H getModuleHandler() {
     * This method sets handler of the module.
     * @param handler the new handler
    public void setModuleHandler(final @Nullable H handler) {
