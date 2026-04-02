 * Make sure static blocks from {@link SIUnits} & {@link ImperialUnits} are executed to initialize the unit parser.
public class UnitInitializer {
        Units.getInstance();
        SIUnits.getInstance();
        ImperialUnits.getInstance();
        CurrencyUnits.getInstance();
    public static void init() {
        // make sure the static block gets executed.
