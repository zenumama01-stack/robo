 * Represents a call to the dynamic proxy which wraps a {@link Callable} and tracks the executing thread.
class Invocation implements Callable<Object> {
    private final @Nullable Object @Nullable [] args;
    private final AbstractInvocationHandler<?> invocationHandler;
    private final Deque<Invocation> invocationStack = new LinkedList<>();
    private Thread thread;
    Invocation(AbstractInvocationHandler<?> invocationHandler, Method method, @Nullable Object @Nullable [] args) {
        this.invocationHandler = invocationHandler;
        this.invocationStack.push(this);
    Thread getThread() {
    public Object call() throws Exception {
        thread = Thread.currentThread();
        return invocationHandler.invokeDirect(this);
    Method getMethod() {
    Object[] getArgs() {
        return invocationHandler.getTimeout();
        return invocationHandler.getIdentifier();
    AbstractInvocationHandler<?> getInvocationHandler() {
        return invocationHandler;
        return "invocation of '" + method.getName() + "()' on '" + invocationHandler.getTarget() + "'";
    Deque<Invocation> getInvocationStack() {
        return invocationStack;
