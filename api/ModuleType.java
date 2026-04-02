 * This class provides common functionality for creating {@link ModuleType} instances. Each {@link ModuleType} instance
 * defines the meta-information needed for creation of a {@link Module} instance which is a building block for a
 * {@link Rule}. The meta-information describes the {@link org.openhab.core.config.core.Configuration} of a
 * {@link Module} providing list with {@link ConfigDescriptionParameter}s, {@link Input}s and {@link Output}s of a
 * {@link Module}. Each {@link ModuleType} instance owns a unique id which is used as reference in the {@link Module}s,
 * to find their meta-information.
 * Whether the {@link ModuleType}s can be used by anyone, depends from their {@link Visibility} value, but they can be
 * modified only by their creator.
public abstract class ModuleType implements Identifiable<String> {
     * Holds the {@link ModuleType}'s identifier, specified by its creator or randomly generated, and is used as
     * reference in the {@link Module}s, to find their meta-information.
     * Determines whether the {@link ModuleType}s can be used by anyone if they are {@link Visibility#VISIBLE} or only
     * by their creator if they are {@link Visibility#HIDDEN}.
     * Defines categories that fit the particular {@link ModuleType} and which can serve as criteria for searching or
     * filtering of {@link ModuleType}s.
     * Defines short and accurate, human-readable label of the {@link ModuleType}.
     * Defines detailed, human-readable description of usage of {@link ModuleType} and its benefits.
     * Describes meta-data for the configuration of the future {@link Module} instances.
     * Creates a {@link ModuleType} instance. This constructor is responsible to initialize common base properties of
     * the {@link ModuleType}s.
     * @param uid the {@link ModuleType}'s identifier, or {@code null} if a random identifier should be
     * @param configDescriptions describing meta-data for the configuration of the future {@link Module} instances
    public ModuleType(@Nullable String uid, @Nullable List<ConfigDescriptionParameter> configDescriptions) {
        this(uid, configDescriptions, null, null, null, null);
     * Creates a {@link ModuleType} instance. This constructor is responsible to initialize all common properties of
     * @param configDescriptions describing meta-data for the configuration of the future {@link Module} instances.
     * @param label a short and accurate, human-readable label of the {@link ModuleType}.
     * @param description a detailed, human-readable description of usage of {@link ModuleType} and its benefits.
     * @param tags defines categories that fit the {@link ModuleType} and which can serve as criteria for
     * @param visibility determines whether the {@link ModuleType} can be used by anyone if it is
    public ModuleType(@Nullable String uid, @Nullable List<ConfigDescriptionParameter> configDescriptions,
     * Gets the {@link ModuleType}. It can be specified by the {@link ModuleType}'s creator, or randomly generated.
     * @return an identifier of this {@link ModuleType}. Can't be {@code null}.
     * Gets the meta-data for the configuration of the future {@link Module} instances.
     * @return a {@link Set} of meta-information configuration descriptions.
     * Gets the assigned to the {@link ModuleType} - {@link #tags}.
     * @return a set of tags, assigned to this {@link ModuleType}.
     * Gets the label of the {@link ModuleType}. The label is a short and accurate, human-readable label of the
     * {@link ModuleType}.
     * @return the {@link ModuleType}'s {@link #label}. Can be {@code null}.
     * Gets the description of the {@link ModuleType}. The description is a short and understandable description of that
     * for what can be used the {@link ModuleType}.
     * @return the {@link ModuleType}'s {@link #description}. Can be {@code null}.
     * Gets the visibility of the {@link ModuleType}. The visibility determines whether the {@link ModuleType}s can be
     * used by anyone if they are {@link Visibility#VISIBLE} or only by their creator if they are
     * {@link Visibility#HIDDEN}. The default visibility is {@link Visibility#VISIBLE}.
     * @return the {@link #visibility} of the {@link ModuleType}.
        ModuleType other = (ModuleType) obj;
