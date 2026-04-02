package org.openhab.core.automation.internal.parser.gson;
import org.openhab.core.config.core.ConfigurationDeserializer;
import org.openhab.core.config.core.ConfigurationSerializer;
import org.openhab.core.config.core.OrderingMapSerializer;
import org.openhab.core.config.core.OrderingSetSerializer;
 * Abstract class that can be used by the parsers for the different entity types.
 * @author Ana Dimova - add Instance Creators
 * @author Sami Salonen - add sorting for maps and sets for minimal diffs
 * @param <T> the type of the entities to parse
public abstract class AbstractGSONParser<T> implements Parser<T> {
    // A Gson instance to use by the parsers
    protected static Gson gson = new GsonBuilder() //
            .setDateFormat(DateTimeType.DATE_PATTERN_JSON_COMPAT) //
            .registerTypeAdapter(CompositeActionType.class, new ActionInstanceCreator()) //
            .registerTypeAdapter(CompositeConditionType.class, new ConditionInstanceCreator()) //
            .registerTypeAdapter(CompositeTriggerType.class, new TriggerInstanceCreator()) //
            .registerTypeAdapter(Configuration.class, new ConfigurationDeserializer()) //
            .registerTypeAdapter(Configuration.class, new ConfigurationSerializer()) //
            .registerTypeHierarchyAdapter(Map.class, new OrderingMapSerializer()) //
            .registerTypeHierarchyAdapter(Set.class, new OrderingSetSerializer()) //
    public void serialize(Set<T> dataObjects, OutputStreamWriter writer) throws Exception {
        gson.toJson(dataObjects, writer);
