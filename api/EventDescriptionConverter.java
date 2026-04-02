 * The {@link EventDescriptionConverter} is a concrete implementation of the {@code XStream}
 * {@link com.thoughtworks.xstream.converters.Converter} interface
 * used to convert a event description within an XML document into an {@link EventDescription} object.
 * This converter converts {@code state} XML tags.
public class EventDescriptionConverter extends GenericUnmarshaller<EventDescription> {
    public EventDescriptionConverter() {
        super(EventDescription.class);
    private List<EventOption> toListOfEventOptions(NodeList nodeList) throws ConversionException {
        if ("options".equals(nodeList.getNodeName())) {
            List<EventOption> eventOptions = new ArrayList<>();
            for (Object nodeObject : nodeList.getList()) {
                eventOptions.add(toEventOption((NodeValue) nodeObject));
            return eventOptions;
        throw new ConversionException("Unknown type '" + nodeList.getNodeName() + "'!");
    private EventOption toEventOption(NodeValue nodeValue) throws ConversionException {
        if ("option".equals(nodeValue.getNodeName())) {
            if ((attributes != null) && (attributes.containsKey("value"))) {
                value = attributes.get("value");
                throw new ConversionException("The node 'option' requires the attribute 'value'!");
            label = (String) nodeValue.getValue();
            return new EventOption(value, label);
        throw new ConversionException("Unknown type in the list of 'options'!");
    public final @Nullable Object unmarshal(HierarchicalStreamReader reader, UnmarshallingContext context) {
        List<EventOption> eventOptions = null;
        NodeList optionNodes = (NodeList) nodeIterator.next();
        if (optionNodes != null) {
            eventOptions = toListOfEventOptions(optionNodes);
        return new EventDescription(eventOptions);
