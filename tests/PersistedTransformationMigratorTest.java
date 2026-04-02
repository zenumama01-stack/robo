 * The {@link PersistedTransformationMigratorTest} contains tests for the ThingImpl and BridgeImpl migrators
public class PersistedTransformationMigratorTest {
    private final Gson internalMapper = new GsonBuilder() //
    private @NonNullByDefault({}) Map<String, StorageEntry> inputMap;
    private @NonNullByDefault({}) Map<String, StorageEntry> resultMap;
    public void setup() throws FileNotFoundException {
        inputMap = readDatabase(Path.of("src/test/resources/transformMigration-input.json"));
        resultMap = readDatabase(Path.of("src/test/resources/transformMigration-result.json"));
        assertThat(inputMap.size(), is(2));
        assertThat(resultMap.size(), is(2));
    private static Stream<Arguments> typeMigrationsSource() {
        return Stream.of(Arguments.of("config:map:2480cdc5d0:de", new PersistedTransformationTypeMigrator()),
                Arguments.of("config:script:testTransCfg", new PersistedTransformationTypeMigrator()));
    @MethodSource("typeMigrationsSource")
    public void typeMigration(String thingUid, TypeMigrator migrator) throws TypeMigrationException {
        StorageEntry inputEntry = inputMap.get(thingUid);
        StorageEntry resultEntry = resultMap.get(thingUid);
        assertThat(inputEntry.getEntityClassName(), is(migrator.getOldType()));
        JsonElement entityValue = (JsonElement) inputEntry.getValue();
        JsonElement newEntityValue = migrator.migrate(entityValue);
        assertThat(newEntityValue, is(resultEntry.getValue()));
    private Map<String, StorageEntry> readDatabase(Path path) throws FileNotFoundException {
        final Map<String, StorageEntry> map = new ConcurrentHashMap<>();
        FileReader reader = new FileReader(path.toFile());
            map.putAll(loadedMap);
