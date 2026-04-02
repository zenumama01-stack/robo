 * Denotes that there already is a thread occupied by the same context.
class DuplicateExecutionException extends RuntimeException {
    private final Invocation callable;
    DuplicateExecutionException(Invocation invocation) {
        this.callable = invocation;
    public Invocation getCallable() {
        return callable;
