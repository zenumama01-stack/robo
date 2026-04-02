 * The {@link ConfigDescriptionParameterGroup} specifies information about parameter groups.
 * A parameter group is used to group a number of parameters together so they can
 * be displayed together in the UI (eg in a single tab).
 * A {@link ConfigDescriptionParameter} instance must also contain the groupName. It should be permissible to use the
 * groupId in the {@link ConfigDescriptionParameter} without supplying a corresponding
 * {@link ConfigDescriptionParameterGroup} - in this way the UI can group the parameters together, but doesn't have the
 * group information.
public class ConfigDescriptionParameterGroup {
    private final @Nullable String context;
    private final boolean advanced;
     * Create a Parameter Group. A group is used by the user interface to display groups
     * of parameters together.
     * @param name the name, used to link the group, to the parameter
     * @param context a context string. Can be used to provide some context to the group
     * @param advanced a flag that is set to true if this group contains advanced settings
     * @param label the human readable group label
     * @param description a description that can be provided to the user
    ConfigDescriptionParameterGroup(String name, @Nullable String context, Boolean advanced, @Nullable String label,
     * Get the name of the group.
     * @return groupName as string
     * Get the context of the group.
     * @return group context as a string
     * Gets the advanced flag for this group.
     * @return advanced flag - true if the group contains advanced properties
     * Get the human readable label of the group
     * @return group label as a string
     * Get the human readable description of the parameter group
     * @return group description as a string
        return this.getClass().getSimpleName() + " [groupId=\"" + name + "\", context=\"" + context + "\", advanced=\""
                + advanced + "\"" + (label != null ? ", label=\"" + label + "\"" : "")
                + (description != null ? ", description=\"" + description + "\"" : "") + "]";
