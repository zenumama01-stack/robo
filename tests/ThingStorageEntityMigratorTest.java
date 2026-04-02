 * The {@link ThingStorageEntityMigratorTest} contains tests for the ThingImpl and BridgeImpl migrators
public class ThingStorageEntityMigratorTest {
        inputMap = readDatabase(Path.of("src/test/resources/thingMigration-input.json"));
        resultMap = readDatabase(Path.of("src/test/resources/thingMigration-result.json"));
        return Stream.of(Arguments.of("deconz:deconz:00313E041ED0", new BridgeImplTypeMigrator(), true),
                Arguments.of("http:url:0a500ec3d8", new ThingImplTypeMigrator(), false));
    public void typeMigration(String thingUid, TypeMigrator migrator, boolean isBridge) throws TypeMigrationException {
        assertThat(entityValue.getAsJsonObject().get("isBridge"), nullValue());
        assertThat(newEntityValue.getAsJsonObject().get("isBridge").getAsBoolean(), is(isBridge));
