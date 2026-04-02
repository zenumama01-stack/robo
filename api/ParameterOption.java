 * The {@link ParameterOption} specifies one option of a static selection list.
 * A {@link ConfigDescriptionParameter} instance can contain a list of {@link ParameterOption}s to define a static
 * selection list for the parameter value.
public class ParameterOption {
    protected ParameterOption() {
    public ParameterOption(String value, String label) {
        return this.getClass().getSimpleName() + " [value=\"" + value + "\", label=\"" + label + "\"]";
        ParameterOption other = (ParameterOption) obj;
