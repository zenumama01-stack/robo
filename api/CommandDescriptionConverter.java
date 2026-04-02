 * The {@link CommandDescriptionConverter} is a concrete implementation of the {@code XStream} {@link Converter}
 * interface used to convert a command description within an XML document into a {@link CommandDescription} object.
 * This converter converts {@code command} XML tags.
public class CommandDescriptionConverter extends GenericUnmarshaller<CommandDescription> {
    public CommandDescriptionConverter() {
        super(CommandDescription.class);
    public final @Nullable CommandDescription unmarshal(HierarchicalStreamReader reader, UnmarshallingContext context) {
        NodeList nodes = (NodeList) context.convertAnother(context, NodeList.class);
        NodeIterator nodeIterator = new NodeIterator(nodes.getList());
        NodeList commandOptionsNode = (NodeList) nodeIterator.next();
        if (commandOptionsNode != null) {
            if ("options".equals(commandOptionsNode.getNodeName())) {
                for (Object coNodeObject : commandOptionsNode.getList()) {
                    NodeValue optionsNode = (NodeValue) coNodeObject;
                    if ("option".equals(optionsNode.getNodeName())) {
                        String name = (String) optionsNode.getValue();
                        String command = optionsNode.getAttributes().get("value");
                        if (name != null && command != null) {
                            commandDescriptionBuilder.withCommandOption(new CommandOption(command, name));
                        throw new ConversionException("The 'options' node must only contain 'option' nodes!");
                return commandDescriptionBuilder.build();
