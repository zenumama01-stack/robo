import org.openhab.core.magic.binding.MagicService;
 * Configuration holder object for {@link MagicService}
public class MagicServiceConfig {
    public String text;
    public boolean bool;
    public BigDecimal decimal;
    public Integer integer;
    public String textAdvanced;
    public boolean booleanAdvanced;
    public BigDecimal decimalAdvanced;
    public Integer integerAdvanced;
    public String requiredTextParameter;
    public String verifiedTextParameter;
    public String selectLimited;
    public String selectVariable;
    public List<String> multiselectTextLimit;
    public List<BigDecimal> multiselectIntegerLimit;
    public BigDecimal selectDecimalLimit;
        for (Field field : this.getClass().getDeclaredFields()) {
                value = field.get(this);
                b.append("MagicService config ");
                b.append(field.getName());
                b.append(" = ");
                b.append(value);
