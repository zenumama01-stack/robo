 * The {@link ThingDescriptionConverter} is a concrete implementation of the {@code XStream} {@link Converter} interface
 * used to convert a list of type information within an XML document
 * into a {@link ThingDescriptionList} object.
 * This converter converts {@code thing-descriptions} XML tags.
public class ThingDescriptionConverter extends GenericUnmarshaller<ThingDescriptionList> {
    public ThingDescriptionConverter() {
        super(ThingDescriptionList.class);
                new String[][] { { "bindingId", "true" }, { "schemaLocation", "false" } });
        String bindingId = attributes.get("bindingId");
        context.put("thing-descriptions.bindingId", bindingId);
        List<?> typeList = (List<?>) context.convertAnother(context, List.class);
        return new ThingDescriptionList(typeList);
