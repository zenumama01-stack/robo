 * The {@link PersistenceCronStrategyDTO} is used for transferring persistence cron
 * strategies
@Schema(name = "PersistenceCronStrategy")
public class PersistenceCronStrategyDTO extends PersistenceStrategyDTO {
    public String cronExpression = "";
    public PersistenceCronStrategyDTO() {
        this("", "");
    public PersistenceCronStrategyDTO(String name, String cronExpression) {
        this.cronExpression = cronExpression;
