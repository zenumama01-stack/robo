 * This class holds a strategy to persist items.
public class PersistenceStrategy {
    public static class Globals {
        public static final PersistenceStrategy UPDATE = new PersistenceStrategy("everyUpdate");
        public static final PersistenceStrategy CHANGE = new PersistenceStrategy("everyChange");
        public static final PersistenceStrategy RESTORE = new PersistenceStrategy("restoreOnStartup");
        public static final PersistenceStrategy FORECAST = new PersistenceStrategy("forecast");
        public static final Map<String, PersistenceStrategy> STRATEGIES = Map.of( //
                UPDATE.name, UPDATE, //
                CHANGE.name, CHANGE, //
                RESTORE.name, RESTORE, //
                FORECAST.name, FORECAST);
    public PersistenceStrategy(final String name) {
        if (!(obj instanceof final PersistenceStrategy other)) {
