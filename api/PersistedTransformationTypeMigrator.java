 * The {@link PersistedTransformationTypeMigrator} implements a {@link TypeMigrator} for stored things
public class PersistedTransformationTypeMigrator implements TypeMigrator {
        return "org.openhab.core.transform.ManagedTransformationConfigurationProvider$PersistedTransformationConfiguration";
        return "org.openhab.core.transform.ManagedTransformationProvider$PersistedTransformation";
        JsonObject newValue = oldValue.deepCopy().getAsJsonObject();
        JsonObject configuration = new JsonObject();
        configuration.addProperty("function", newValue.remove("content").getAsString());
        newValue.remove("language");
        newValue.add("configuration", configuration);
