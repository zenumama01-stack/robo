 * Data holder for StateDescriptionFragment creation.
public class StateDescriptionFragmentImpl implements StateDescriptionFragment {
    private static class StateDescriptionImpl extends StateDescription {
        StateDescriptionImpl(@Nullable BigDecimal minimum, @Nullable BigDecimal maximum, @Nullable BigDecimal step,
                @Nullable String pattern, boolean readOnly, @Nullable List<StateOption> options) {
            super(minimum, maximum, step, pattern, readOnly, options);
    private @Nullable BigDecimal minimum;
    private @Nullable BigDecimal maximum;
    private @Nullable Boolean readOnly;
    private @Nullable List<StateOption> options;
     * Create an empty {@link StateDescriptionFragmentImpl}.
    public StateDescriptionFragmentImpl() {
     * Create a {@link StateDescriptionFragmentImpl} and initialize from the given values.
     * @param minimum minimum value of the state
     * @param maximum maximum value of the state
     * @param step step size
     * @param pattern pattern to render the state
     * @param readOnly if the state can be changed by the system
     * @param options predefined list of options
     * @deprecated use {@link org.openhab.core.types.StateDescriptionFragmentBuilder} instead.
    public StateDescriptionFragmentImpl(@Nullable BigDecimal minimum, @Nullable BigDecimal maximum,
            @Nullable BigDecimal step, @Nullable String pattern, @Nullable Boolean readOnly,
            @Nullable List<StateOption> options) {
        this.minimum = minimum;
        this.maximum = maximum;
        this.options = options == null || options.isEmpty() ? List.of() : Collections.unmodifiableList(options);
     * Create a {@link StateDescriptionFragmentImpl} and initialize from the given {@link StateDescription}.
     * Note: State options will only be set if not empty.
     * @param legacy the {@link StateDescription} to initialize from.
    public StateDescriptionFragmentImpl(StateDescription legacy) {
        this.minimum = legacy.getMinimum();
        this.maximum = legacy.getMaximum();
        this.step = legacy.getStep();
        this.pattern = legacy.getPattern();
        this.readOnly = Boolean.valueOf(legacy.isReadOnly());
        if (!legacy.getOptions().isEmpty()) {
            this.options = legacy.getOptions();
     * Copy constructor.
     * @param source the source to copy from.
    public StateDescriptionFragmentImpl(StateDescriptionFragmentImpl source) {
        this.minimum = source.getMinimum();
        this.maximum = source.getMaximum();
        this.step = source.getStep();
        this.pattern = source.getPattern();
        this.readOnly = source.isReadOnly();
        this.options = source.getOptions();
        return minimum;
    public void setMinimum(BigDecimal minimum) {
        return maximum;
    public void setMaximum(BigDecimal maximum) {
    public void setStep(BigDecimal step) {
    public void setPattern(String pattern) {
    public @Nullable Boolean isReadOnly() {
    public void setReadOnly(Boolean readOnly) {
    public @Nullable List<StateOption> getOptions() {
    public void setOptions(List<StateOption> options) {
    public @Nullable StateDescription toStateDescription() {
        if (minimum == null && maximum == null && step == null && readOnly == null && pattern == null
                && options == null) {
        final Boolean ro = readOnly;
        return new StateDescriptionImpl(minimum, maximum, step, pattern, ro != null && ro.booleanValue(), options);
     * Merge the given {@link StateDescriptionFragment}. Set all unset ({@code null}) fields of this instance to the
     * values from the given {@link StateDescriptionFragment}.
     * @param fragment a {@link StateDescriptionFragment} this instance should merge in.
     * @return this instance with the fields merged.
    public StateDescriptionFragment merge(StateDescriptionFragment fragment) {
        if (minimum == null) {
            minimum = fragment.getMinimum();
        if (maximum == null) {
            maximum = fragment.getMaximum();
            step = fragment.getStep();
            pattern = fragment.getPattern();
        if (readOnly == null) {
            readOnly = fragment.isReadOnly();
        List<StateOption> localOptions = options;
        if (localOptions == null || localOptions.isEmpty()) {
            options = fragment.getOptions();
        result = prime * result + Objects.hashCode(minimum);
        result = prime * result + Objects.hashCode(maximum);
        result = prime * result + Objects.hashCode(step);
        result = prime * result + Objects.hashCode(pattern);
        result = prime * result + (readOnly ? 1231 : 1237);
        result = prime * result + Objects.hashCode(options);
        StateDescriptionFragmentImpl other = (StateDescriptionFragmentImpl) obj;
        return Objects.equals(minimum, other.minimum) && Objects.equals(maximum, other.maximum)
                && Objects.equals(step, other.step) && Objects.equals(pattern, other.pattern)
                && Objects.equals(readOnly, other.readOnly) && Objects.equals(options, other.options);
        return "StateDescription [minimum=" + minimum + ", maximum=" + maximum + ", step=" + step + ", pattern="
                + pattern + ", readOnly=" + readOnly + ", channelStateOptions=" + options + "]";
