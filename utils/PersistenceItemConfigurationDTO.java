 * The {@link org.openhab.core.persistence.dto.PersistenceItemConfigurationDTO} is used for transferring persistence
 * item configurations
@Schema(name = "PersistenceItemConfiguration")
public class PersistenceItemConfigurationDTO {
    public Collection<String> items = List.of();
    public Collection<String> strategies = List.of();
    public Collection<String> filters = List.of();
