package org.openhab.core.model.persistence.internal;
import org.openhab.core.model.persistence.persistence.AliasConfiguration;
import org.openhab.core.model.persistence.persistence.AllConfig;
import org.openhab.core.model.persistence.persistence.CronStrategy;
import org.openhab.core.model.persistence.persistence.EqualsFilter;
import org.openhab.core.model.persistence.persistence.Filter;
import org.openhab.core.model.persistence.persistence.GroupConfig;
import org.openhab.core.model.persistence.persistence.GroupExcludeConfig;
import org.openhab.core.model.persistence.persistence.IncludeFilter;
import org.openhab.core.model.persistence.persistence.ItemConfig;
import org.openhab.core.model.persistence.persistence.ItemExcludeConfig;
import org.openhab.core.model.persistence.persistence.NotEqualsFilter;
import org.openhab.core.model.persistence.persistence.NotIncludeFilter;
import org.openhab.core.model.persistence.persistence.PersistenceConfiguration;
import org.openhab.core.model.persistence.persistence.PersistenceModel;
import org.openhab.core.model.persistence.persistence.Strategy;
import org.openhab.core.model.persistence.persistence.ThresholdFilter;
import org.openhab.core.model.persistence.persistence.TimeFilter;
import org.openhab.core.persistence.config.PersistenceAllConfig;
import org.openhab.core.persistence.config.PersistenceConfig;
import org.openhab.core.persistence.config.PersistenceGroupConfig;
import org.openhab.core.persistence.config.PersistenceGroupExcludeConfig;
import org.openhab.core.persistence.config.PersistenceItemConfig;
import org.openhab.core.persistence.config.PersistenceItemExcludeConfig;
import org.openhab.core.persistence.filter.PersistenceEqualsFilter;
import org.openhab.core.persistence.filter.PersistenceFilter;
import org.openhab.core.persistence.filter.PersistenceIncludeFilter;
import org.openhab.core.persistence.filter.PersistenceThresholdFilter;
import org.openhab.core.persistence.filter.PersistenceTimeFilter;
import org.openhab.core.persistence.registry.PersistenceServiceConfigurationProvider;
import org.openhab.core.persistence.strategy.PersistenceCronStrategy;
 * This class is the central part of the persistence management and delegation. It reads the persistence
 * models, schedules timers and manages the invocation of {@link PersistenceService}s upon events.
 * @author Markus Rathgeb - Move non-model logic to core.persistence
 * @author Jan N. Klug - Refactored to {@link PersistenceServiceConfigurationProvider}
 * @author Mark Herwege - Separate alias handling
@Component(immediate = true, service = PersistenceServiceConfigurationProvider.class)
public class PersistenceModelManager extends AbstractProvider<PersistenceServiceConfiguration>
        implements ModelRepositoryChangeListener, PersistenceServiceConfigurationProvider {
    private final Logger logger = LoggerFactory.getLogger(PersistenceModelManager.class);
    private final Map<String, PersistenceServiceConfiguration> configurations = new ConcurrentHashMap<>();
    public PersistenceModelManager(@Reference ModelRepository modelRepository) {
        modelRepository.getAllModelNamesOfType("persist")
                .forEach(modelName -> modelChanged(modelName, EventType.ADDED));
                .forEach(modelName -> modelChanged(modelName, EventType.REMOVED));
        if (modelName.endsWith(".persist")) {
            String serviceName = serviceName(modelName);
                PersistenceServiceConfiguration removed = configurations.remove(serviceName);
                if (removed == null) {
                    logger.warn("Service for {} was already removed from registry, ignoring.", modelName);
                    notifyListenersAboutRemovedElement(removed);
                final PersistenceModel model = (PersistenceModel) modelRepository.getModel(modelName);
                    PersistenceServiceConfiguration newConfiguration = new PersistenceServiceConfiguration(serviceName,
                            mapConfigs(model.getConfigs()), mapAliases(model.getAliases()),
                            mapStrategies(model.getStrategies()), mapFilters(model.getFilters()));
                    PersistenceServiceConfiguration oldConfiguration = configurations.put(serviceName,
                            newConfiguration);
                    if (oldConfiguration == null) {
                        if (type != EventType.ADDED) {
                                    "Model {} is inconsistent: An updated event was sent, but there is no old configuration. Adding it now.",
                                    modelName);
                        notifyListenersAboutAddedElement(newConfiguration);
                        if (type != EventType.MODIFIED) {
                                    "Model {} is inconsistent: An added event was sent, but there is an old configuration. Replacing it now.",
                        notifyListenersAboutUpdatedElement(oldConfiguration, newConfiguration);
                            "The model repository reported a {} event for model '{}' but the model could not be found in the repository. ",
                            type, modelName);
    private String serviceName(String modelName) {
        return modelName.substring(0, modelName.length() - ".persist".length());
    private List<PersistenceItemConfiguration> mapConfigs(List<PersistenceConfiguration> configs) {
        final List<PersistenceItemConfiguration> lst = new LinkedList<>();
        for (final PersistenceConfiguration config : configs) {
            lst.add(mapConfig(config));
    private PersistenceItemConfiguration mapConfig(PersistenceConfiguration config) {
        final List<PersistenceConfig> items = new LinkedList<>();
        for (final EObject item : config.getItems()) {
            if (item instanceof AllConfig) {
                items.add(new PersistenceAllConfig());
            } else if (item instanceof GroupConfig groupConfig) {
                items.add(new PersistenceGroupConfig(groupConfig.getGroup()));
            } else if (item instanceof ItemConfig itemConfig) {
                items.add(new PersistenceItemConfig(itemConfig.getItem()));
            } else if (item instanceof GroupExcludeConfig groupExcludeConfig) {
                items.add(new PersistenceGroupExcludeConfig(groupExcludeConfig.getGroupExclude()));
            } else if (item instanceof ItemExcludeConfig itemExcludeConfig) {
                items.add(new PersistenceItemExcludeConfig(itemExcludeConfig.getItemExclude()));
        return new PersistenceItemConfiguration(items, mapStrategies(config.getStrategies()),
                mapFilters(config.getFilters()));
    private Map<String, String> mapAliases(List<AliasConfiguration> aliases) {
        final Map<String, String> map = new HashMap<>();
        for (final AliasConfiguration alias : aliases) {
            map.put(alias.getItem(), alias.getAlias());
    private List<PersistenceStrategy> mapStrategies(List<Strategy> strategies) {
        final List<PersistenceStrategy> lst = new LinkedList<>();
        for (final Strategy strategy : strategies) {
            lst.add(mapStrategy(strategy));
    private PersistenceStrategy mapStrategy(Strategy strategy) {
        if (strategy instanceof CronStrategy cronStrategy) {
            return new PersistenceCronStrategy(strategy.getName(), cronStrategy.getCronExpression());
            return new PersistenceStrategy(strategy.getName());
    private List<PersistenceFilter> mapFilters(List<Filter> filters) {
        final List<PersistenceFilter> lst = new LinkedList<>();
        for (final Filter filter : filters) {
            lst.add(mapFilter(filter));
    private PersistenceFilter mapFilter(Filter filter) {
        if (filter.getDefinition() instanceof TimeFilter timeFilter) {
            return new PersistenceTimeFilter(filter.getName(), timeFilter.getValue(), timeFilter.getUnit());
        } else if (filter.getDefinition() instanceof ThresholdFilter thresholdFilter) {
            return new PersistenceThresholdFilter(filter.getName(), thresholdFilter.getValue(),
                    thresholdFilter.getUnit(), thresholdFilter.isRelative());
        } else if (filter.getDefinition() instanceof EqualsFilter equalsFilter) {
            return new PersistenceEqualsFilter(filter.getName(), equalsFilter.getValues(), false);
        } else if (filter.getDefinition() instanceof NotEqualsFilter notEqualsFilter) {
            return new PersistenceEqualsFilter(filter.getName(), notEqualsFilter.getValues(), true);
        } else if (filter.getDefinition() instanceof IncludeFilter includeFilter) {
            return new PersistenceIncludeFilter(filter.getName(), includeFilter.getLower(), includeFilter.getUpper(),
                    includeFilter.getUnit(), false);
        } else if (filter.getDefinition() instanceof NotIncludeFilter notIncludeFilter) {
            return new PersistenceIncludeFilter(filter.getName(), notIncludeFilter.getLower(),
                    notIncludeFilter.getUpper(), notIncludeFilter.getUnit(), true);
        throw new IllegalArgumentException("Unknown filter type " + filter.getClass());
    public Collection<PersistenceServiceConfiguration> getAll() {
        return List.copyOf(configurations.values());
