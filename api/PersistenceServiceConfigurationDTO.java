 * The {@link PersistenceServiceConfigurationDTO} is used for transferring persistence service configurations
@Schema(name = "PersistenceServiceConfiguration")
public class PersistenceServiceConfigurationDTO {
    public String serviceId = "";
    public Collection<PersistenceItemConfigurationDTO> configs = List.of();
    public Map<String, String> aliases = Map.of();
     * @deprecated This field is kept to enable migration from previous version storage.
     *             It should not be removed as this would make automatic upgrading persistence configurations from an
     *             older version impossible.
    public @Nullable Collection<String> defaults;
    public Collection<PersistenceCronStrategyDTO> cronStrategies = List.of();
    public Collection<PersistenceFilterDTO> thresholdFilters = List.of();
    public Collection<PersistenceFilterDTO> timeFilters = List.of();
    public Collection<PersistenceFilterDTO> equalsFilters = List.of();
    public Collection<PersistenceFilterDTO> includeFilters = List.of();
