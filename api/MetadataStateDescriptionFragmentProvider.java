import static org.openhab.core.internal.items.MetadataCommandDescriptionProvider.parseValueLabelPair;
 * A {@link StateDescriptionFragment} provider from items' metadata
@Component(service = StateDescriptionFragmentProvider.class)
public class MetadataStateDescriptionFragmentProvider implements StateDescriptionFragmentProvider {
    private final Logger logger = LoggerFactory.getLogger(MetadataStateDescriptionFragmentProvider.class);
    public static final String STATEDESCRIPTION_METADATA_NAMESPACE = "stateDescription";
    private final Integer rank;
    public MetadataStateDescriptionFragmentProvider(final @Reference MetadataRegistry metadataRegistry,
            rank = 1; // takes precedence over other providers usually ranked 0
        Metadata metadata = metadataRegistry.get(new MetadataKey(STATEDESCRIPTION_METADATA_NAMESPACE, itemName));
                Object pattern = metadata.getConfiguration().get("pattern");
                    builder.withPattern((String) pattern);
                Object min = metadata.getConfiguration().get("min");
                    builder.withMinimum(getBigDecimal(min));
                Object max = metadata.getConfiguration().get("max");
                    builder.withMaximum(getBigDecimal(max));
                Object step = metadata.getConfiguration().get("step");
                    builder.withStep(getBigDecimal(step));
                Object readOnly = metadata.getConfiguration().get("readOnly");
                    builder.withReadOnly(getBoolean(readOnly));
                if (metadata.getConfiguration().get("options") instanceof Object options) {
                    List<StateOption> stateOptions = Stream.of(options.toString().split(",")).map(o -> {
                            return new StateOption(pair[0], pair[1]);
                            return new StateOption(o.trim(), null);
                    builder.withOptions(stateOptions);
                logger.warn("Unable to parse the stateDescription from metadata for item {}, ignoring it", itemName);
    private BigDecimal getBigDecimal(Object value) {
        BigDecimal ret = null;
        if (value instanceof BigDecimal decimal) {
            ret = decimal;
        } else if (value instanceof String string) {
            ret = new BigDecimal(string);
        } else if (value instanceof BigInteger integer) {
            ret = new BigDecimal(integer);
            ret = new BigDecimal(number.doubleValue());
            throw new ClassCastException(
                    "Not possible to coerce [" + value + "] from class " + value.getClass() + " into a BigDecimal.");
    private Boolean getBoolean(Object value) {
        Boolean ret = null;
            ret = boolean1;
            ret = Boolean.parseBoolean(string);
                    "Not possible to coerce [" + value + "] from class " + value.getClass() + " into a Boolean.");
