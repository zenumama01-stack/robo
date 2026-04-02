package org.openhab.core.thing.internal.type;
 * Abstract base class with common methods for {@link ChannelTypeBuilder}
abstract class AbstractChannelTypeBuilder<@NonNull T extends ChannelTypeBuilder<T>> implements ChannelTypeBuilder<T> {
    protected final ChannelTypeUID channelTypeUID;
    protected final String label;
    protected boolean advanced;
    protected @Nullable String category;
    protected final Set<String> tags = new HashSet<>();
    protected @Nullable URI configDescriptionURI;
    protected AbstractChannelTypeBuilder(ChannelTypeUID channelTypeUID, String label) {
        if (label.isEmpty()) {
            throw new IllegalArgumentException("Label for a ChannelType must not be empty.");
    public T isAdvanced(boolean advanced) {
        return (T) this;
    public T withDescription(String description) {
    public T withCategory(String category) {
    public T withTag(String tag) {
        this.tags.add(tag);
    public T withTags(Collection<String> tags) {
    public T withTags(SemanticTag... tags) {
        this.tags.addAll(Stream.of(tags).map(t -> t.getName()).toList());
    public T withConfigDescriptionURI(URI configDescriptionURI) {
    public abstract ChannelType build();
