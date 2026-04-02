package org.openhab.core.internal.types;
 * The {@link CommandDescriptionImpl} groups state command properties.
public class CommandDescriptionImpl implements CommandDescription {
    private final List<CommandOption> commandOptions;
    public CommandDescriptionImpl() {
        commandOptions = new ArrayList<>();
     * Adds a {@link CommandOption} to this {@link CommandDescriptionImpl}.
     * @param commandOption a commandOption to be added to this {@link CommandDescriptionImpl}.
    public void addCommandOption(CommandOption commandOption) {
        commandOptions.add(commandOption);
        return Collections.unmodifiableList(commandOptions);
        result = prime * result + commandOptions.hashCode();
        CommandDescriptionImpl other = (CommandDescriptionImpl) obj;
        return commandOptions.equals(other.commandOptions);
        return "CommandDescription [commandOptions=" + commandOptions + "]";
