 * The {@link org.openhab.core.persistence.dto.PersistenceFilterDTO} is used for transferring persistence filter
 * configurations
@Schema(name = "PersistenceFilter")
public class PersistenceFilterDTO {
    // threshold and time
    public BigDecimal value;
    // threshold
    public Boolean relative;
    // threshold, include/exclude
    // include/exclude
    public BigDecimal lower;
    public BigDecimal upper;
    // equals/not equals
    public List<String> values;
    // equals/not equals, include/exclude
    public Boolean inverted;
