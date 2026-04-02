package org.openhab.core.events;
 * Abstract implementation of the {@link Event} interface.
public abstract class AbstractEvent implements Event {
    public static final String ACTOR_SEPARATOR = "$";
    public static final String DELEGATION_SEPARATOR = "=>";
    public static final String DELEGATION_ESCAPE = "__";
    private final String payload;
    private final @Nullable String source;
     * Must be called in subclass constructor to create a new event.
    protected AbstractEvent(String topic, String payload, @Nullable String source) {
    public String getPayload() {
    public @Nullable String getSource() {
        result = prime * result + payload.hashCode();
        result = prime * result + (source instanceof String local ? local.hashCode() : 0);
        result = prime * result + topic.hashCode();
        AbstractEvent other = (AbstractEvent) obj;
        if (!payload.equals(other.payload)) {
        String localSource = source;
        if (localSource == null) {
            if (other.source != null) {
        } else if (!localSource.equals(other.source)) {
        if (!topic.equals(other.topic)) {
     * Utility method to build a source string from a bundle and an optional actor.
     * Bundle names may not contain the actor separator.
     * The actor, if present, will be replaced with `__` to disallow the delegation separator.
     * Consequently, `__` will be doubled as an escape sequence.
     * @param bundle the bundle (such as org.openhab.core.thing or org.openhab.binding.matter)
     * @param actor the actor
     * @return the final source string
    public static String buildSource(String bundle, @Nullable String actor) {
        if (bundle.contains(ACTOR_SEPARATOR)) {
            throw new IllegalArgumentException("Bundle must not contain the actor separator '" + ACTOR_SEPARATOR + "'");
        if (bundle.contains(DELEGATION_SEPARATOR)) {
                    "Bundle must not contain the delegation separator '" + DELEGATION_SEPARATOR + "'");
        if (actor == null || actor.isEmpty()) {
        actor = actor.replace(DELEGATION_ESCAPE, DELEGATION_ESCAPE + DELEGATION_ESCAPE);
        actor = actor.replace(DELEGATION_SEPARATOR, DELEGATION_ESCAPE);
        return bundle + ACTOR_SEPARATOR + actor;
     * Utility method to build a delegated source string from an original source and a bundle
     * @param originalSource the original source (may be null)
    public static String buildDelegatedSource(@Nullable String originalSource, String bundle) {
        if (originalSource == null || originalSource.isEmpty()) {
        return originalSource + DELEGATION_SEPARATOR + bundle;
     * Utility method to build a delegated source string from an original source, a bundle and an optional actor.
    public static String buildDelegatedSource(@Nullable String originalSource, String bundle, @Nullable String actor) {
        return buildDelegatedSource(originalSource, buildSource(bundle, actor));
