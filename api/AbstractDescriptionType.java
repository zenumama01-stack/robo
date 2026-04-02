package org.openhab.core.thing.type;
 * The {@link AbstractDescriptionType} class is the base class for a {@link ThingType},
 * a {@link BridgeType} a {@link ChannelType} or a {@link ChannelGroupType}.
 * This class contains only properties and methods accessing them.
public abstract class AbstractDescriptionType implements Identifiable<UID> {
    private final UID uid;
    private final @Nullable URI configDescriptionURI;
     * @param uid the unique identifier which identifies the according type within
     *            the overall system (must neither be null, nor empty)
     * @param label the human-readable label for the according type
     *            (must neither be null nor empty)
     * @param description the human-readable description for the according type
     *            (could be null or empty)
     * @param configDescriptionURI the {@link URI} that references the {@link ConfigDescription} of this type
     * @throws IllegalArgumentException if the UID is null, or the label is null or empty
    protected AbstractDescriptionType(UID uid, String label, @Nullable String description,
            throw new IllegalArgumentException("The label must neither be null nor empty!");
     * Returns the unique identifier which identifies the according type within the overall system.
     * @return the unique identifier which identifies the according type within
     *         the overall system (neither null, nor empty)
    public UID getUID() {
        return this.uid;
     * Returns the human-readable label for the according type.
     * @return the human-readable label for the according type (neither null, nor empty)
     * Returns the human-readable description for the according type.
     * @return the human-readable description for the according type (could be null or empty)
        return this.description;
     * Returns the link to a concrete {@link ConfigDescription}.
     * @return the link to a concrete ConfigDescription
    public @Nullable URI getConfigDescriptionURI() {
