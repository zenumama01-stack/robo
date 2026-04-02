 * The {@link ThingUpdateInstruction} is an interface that can be implemented to perform updates on things when the
 * thing-type changes.
public interface ThingUpdateInstruction {
     * Get the (final) thing type version that the {@link Thing} will be updated to after all
     * {@link ThingUpdateInstruction}s with the same version have been applied.
     * @return the thing-type version (always > 0)
    int getThingTypeVersion();
     * Perform the update in this instruction for a given {@link Thing} using the given {@link ThingBuilder}
     * Note: the thing type version is not updated as there may be several instructions to perform for a single version.
     * @param thing the thing that should be updated
     * @param thingBuilder the thing builder to use
    void perform(Thing thing, ThingBuilder thingBuilder);
     * Check if this update is needed for a {@link Thing} with the given version
     * @param currentThingTypeVersion the current thing type version of the {@link Thing}
     * @return <code>true</code> if this instruction should be applied, <code>false</code> otherwise
    static Predicate<ThingUpdateInstruction> applies(int currentThingTypeVersion) {
        return i -> i.getThingTypeVersion() > currentThingTypeVersion;
