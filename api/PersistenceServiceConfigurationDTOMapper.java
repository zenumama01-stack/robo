import static org.openhab.core.persistence.strategy.PersistenceStrategy.Globals.STRATEGIES;
import org.openhab.core.persistence.dto.PersistenceFilterDTO;
import org.openhab.core.persistence.dto.PersistenceItemConfigurationDTO;
 * The {@link PersistenceServiceConfigurationDTOMapper} is a utility class to map persistence configurations for storage
public class PersistenceServiceConfigurationDTOMapper {
    private PersistenceServiceConfigurationDTOMapper() {
        // prevent initialization
    public static PersistenceServiceConfigurationDTO map(
            PersistenceServiceConfiguration persistenceServiceConfiguration) {
        PersistenceServiceConfigurationDTO dto = new PersistenceServiceConfigurationDTO();
        dto.serviceId = persistenceServiceConfiguration.getUID();
        dto.configs = persistenceServiceConfiguration.getConfigs().stream()
                .map(PersistenceServiceConfigurationDTOMapper::mapPersistenceItemConfig).toList();
        dto.aliases = Map.copyOf(persistenceServiceConfiguration.getAliases());
        dto.cronStrategies = filterList(persistenceServiceConfiguration.getStrategies(), PersistenceCronStrategy.class,
                PersistenceServiceConfigurationDTOMapper::mapPersistenceCronStrategy);
        dto.thresholdFilters = filterList(persistenceServiceConfiguration.getFilters(),
                PersistenceThresholdFilter.class,
                PersistenceServiceConfigurationDTOMapper::mapPersistenceThresholdFilter);
        dto.timeFilters = filterList(persistenceServiceConfiguration.getFilters(), PersistenceTimeFilter.class,
                PersistenceServiceConfigurationDTOMapper::mapPersistenceTimeFilter);
        dto.equalsFilters = filterList(persistenceServiceConfiguration.getFilters(), PersistenceEqualsFilter.class,
                PersistenceServiceConfigurationDTOMapper::mapPersistenceEqualsFilter);
        dto.includeFilters = filterList(persistenceServiceConfiguration.getFilters(), PersistenceIncludeFilter.class,
                PersistenceServiceConfigurationDTOMapper::mapPersistenceIncludeFilter);
    public static PersistenceServiceConfiguration map(PersistenceServiceConfigurationDTO dto) {
        Map<String, PersistenceStrategy> strategyMap = dto.cronStrategies.stream()
                .collect(Collectors.toMap(e -> e.name, e -> new PersistenceCronStrategy(e.name, e.cronExpression)));
        Map<String, PersistenceFilter> filterMap = Stream.of(
                dto.thresholdFilters.stream()
                        .map(f -> new PersistenceThresholdFilter(f.name, f.value, f.unit, f.relative)),
                dto.timeFilters.stream().map(f -> new PersistenceTimeFilter(f.name, f.value.intValue(), f.unit)),
                dto.equalsFilters.stream().map(f -> new PersistenceEqualsFilter(f.name, f.values, f.inverted)),
                dto.includeFilters.stream()
                        .map(f -> new PersistenceIncludeFilter(f.name, f.lower, f.upper, f.unit, f.inverted)))
                .flatMap(Function.identity()).collect(Collectors.toMap(PersistenceFilter::getName, e -> e));
        List<PersistenceItemConfiguration> configs = dto.configs.stream().map(config -> {
            List<PersistenceConfig> items = config.items.stream()
                    .map(PersistenceServiceConfigurationDTOMapper::stringToPersistenceConfig).toList();
            List<PersistenceStrategy> strategies = config.strategies.stream()
                    .map(str -> stringToPersistenceStrategy(str, strategyMap, dto.serviceId)).toList();
            List<PersistenceFilter> filters = config.filters.stream()
                    .map(str -> stringToPersistenceFilter(str, filterMap, dto.serviceId)).toList();
            return new PersistenceItemConfiguration(items, strategies, filters);
        Map<String, String> aliases = Map.copyOf(dto.aliases);
        return new PersistenceServiceConfiguration(dto.serviceId, configs, aliases, strategyMap.values(),
                filterMap.values());
    private static <T, R> Collection<R> filterList(Collection<? super T> list, Class<T> clazz, Function<T, R> mapper) {
        return list.stream().filter(clazz::isInstance).map(clazz::cast).map(mapper).toList();
    private static PersistenceConfig stringToPersistenceConfig(String string) {
        if ("*".equals(string)) {
            return new PersistenceAllConfig();
        } else if (string.endsWith("*")) {
            if (string.startsWith("!")) {
                return new PersistenceGroupExcludeConfig(string.substring(1, string.length() - 1));
            return new PersistenceGroupConfig(string.substring(0, string.length() - 1));
                return new PersistenceItemExcludeConfig(string.substring(1));
            return new PersistenceItemConfig(string);
    private static PersistenceStrategy stringToPersistenceStrategy(String string,
            Map<String, PersistenceStrategy> strategyMap, String serviceId) {
        PersistenceStrategy strategy = strategyMap.get(string);
        if (strategy != null) {
            return strategy;
        strategy = STRATEGIES.get(string);
        throw new IllegalArgumentException("Strategy '" + string + "' unknown for service '" + serviceId + "'");
    private static PersistenceFilter stringToPersistenceFilter(String string, Map<String, PersistenceFilter> filterMap,
            String serviceId) {
        PersistenceFilter filter = filterMap.get(string);
        if (filter != null) {
        throw new IllegalArgumentException("Filter '" + string + "' unknown for service '" + serviceId + "'");
    public static String persistenceConfigToString(PersistenceConfig config) {
        if (config instanceof PersistenceAllConfig) {
            return "*";
        } else if (config instanceof PersistenceGroupConfig persistenceGroupConfig) {
            return persistenceGroupConfig.getGroup() + "*";
        } else if (config instanceof PersistenceItemConfig persistenceItemConfig) {
            return persistenceItemConfig.getItem();
        } else if (config instanceof PersistenceGroupExcludeConfig persistenceGroupExcludeConfig) {
            return "!" + persistenceGroupExcludeConfig.getGroup() + "*";
        } else if (config instanceof PersistenceItemExcludeConfig persistenceItemExcludeConfig) {
            return "!" + persistenceItemExcludeConfig.getItem();
        throw new IllegalArgumentException("Unknown persistence config class " + config.getClass());
    private static PersistenceItemConfigurationDTO mapPersistenceItemConfig(PersistenceItemConfiguration config) {
        PersistenceItemConfigurationDTO itemDto = new PersistenceItemConfigurationDTO();
        itemDto.items = config.items().stream().map(PersistenceServiceConfigurationDTOMapper::persistenceConfigToString)
        itemDto.strategies = config.strategies().stream().map(PersistenceStrategy::getName).toList();
        itemDto.filters = config.filters().stream().map(PersistenceFilter::getName).toList();
        return itemDto;
    private static PersistenceCronStrategyDTO mapPersistenceCronStrategy(PersistenceCronStrategy cronStrategy) {
        PersistenceCronStrategyDTO cronStrategyDTO = new PersistenceCronStrategyDTO();
        cronStrategyDTO.name = cronStrategy.getName();
        cronStrategyDTO.cronExpression = cronStrategy.getCronExpression();
        return cronStrategyDTO;
    private static PersistenceFilterDTO mapPersistenceThresholdFilter(PersistenceThresholdFilter thresholdFilter) {
        PersistenceFilterDTO filterDTO = new PersistenceFilterDTO();
        filterDTO.name = thresholdFilter.getName();
        filterDTO.value = thresholdFilter.getValue();
        filterDTO.unit = thresholdFilter.getUnit();
        filterDTO.relative = thresholdFilter.isRelative();
        return filterDTO;
    private static PersistenceFilterDTO mapPersistenceTimeFilter(PersistenceTimeFilter persistenceTimeFilter) {
        filterDTO.name = persistenceTimeFilter.getName();
        filterDTO.value = new BigDecimal(persistenceTimeFilter.getValue());
        filterDTO.unit = persistenceTimeFilter.getUnit();
    private static PersistenceFilterDTO mapPersistenceEqualsFilter(PersistenceEqualsFilter persistenceEqualsFilter) {
        filterDTO.name = persistenceEqualsFilter.getName();
        filterDTO.values = persistenceEqualsFilter.getValues().stream().toList();
        filterDTO.inverted = persistenceEqualsFilter.getInverted();
    private static PersistenceFilterDTO mapPersistenceIncludeFilter(PersistenceIncludeFilter persistenceIncludeFilter) {
        filterDTO.name = persistenceIncludeFilter.getName();
        filterDTO.lower = persistenceIncludeFilter.getLower();
        filterDTO.upper = persistenceIncludeFilter.getUpper();
        filterDTO.unit = persistenceIncludeFilter.getUnit();
        filterDTO.inverted = persistenceIncludeFilter.getInverted();
