 * The {@link ThingTypeMigrationService} describes a service to change the thing type
 * of a given {@link Thing}.
public interface ThingTypeMigrationService {
     * Changes the type of a given {@link Thing}.
     * @param thing {@link Thing} whose type should be changed
     * @param thingTypeUID new {@link ThingTypeUID}
     * @param configuration new configuration
     * @throws RuntimeException, if the new thing type is not registered in the registry
    void migrateThingType(Thing thing, ThingTypeUID thingTypeUID, @Nullable Configuration configuration);
