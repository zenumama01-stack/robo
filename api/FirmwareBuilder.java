import org.openhab.core.thing.internal.firmware.FirmwareImpl;
 * The builder to create a {@link Firmware}.
 * @author Dimitar Ivanov - Extracted as separate class for Firmware, introduced firmware restriction
 *         function
public final class FirmwareBuilder {
    private final ThingTypeUID thingTypeUID;
    private @Nullable String vendor;
    private @Nullable String model;
    private boolean modelRestricted;
    private @Nullable String prerequisiteVersion;
    private @Nullable FirmwareRestriction firmwareRestriction;
    private @Nullable String changelog;
    private @Nullable URL onlineChangelog;
    private @Nullable transient InputStream inputStream;
    private @Nullable String md5Hash;
    public static FirmwareBuilder create(ThingTypeUID thingTypeUID, String firmwareVersion) {
        return new FirmwareBuilder(thingTypeUID, firmwareVersion);
     * Creates a new builder.
     * @param thingTypeUID the thing type UID that is associated with this firmware (not null)
     * @param firmwareVersion the version of the firmware to be created (not null)
     * @throws IllegalArgumentException if given firmware version is null or empty; if the thing type UID is null
    private FirmwareBuilder(ThingTypeUID thingTypeUID, String firmwareVersion) {
        checkNotNull(thingTypeUID, "ThingTypeUID");
        checkNotNullOrEmpty(firmwareVersion, "Firmware version");
        this.version = firmwareVersion;
        this.properties = new HashMap<>();
     * Adds the vendor to the builder.
     * @param vendor the vendor to be added to the builder (can be null)
    public FirmwareBuilder withVendor(@Nullable String vendor) {
        this.vendor = vendor;
     * Adds the model to the builder.
     * @param model the model to be added to the builder (can be null)
    public FirmwareBuilder withModel(@Nullable String model) {
        this.model = model;
     * Sets the modelRestricted flag in the builder.
     * @param modelRestricted the modelRestricted flag to be added to the builder
    public FirmwareBuilder withModelRestricted(boolean modelRestricted) {
        this.modelRestricted = modelRestricted;
     * Adds the description to the builder.
     * @param description the description to be added to the builder (can be null)
    public FirmwareBuilder withDescription(@Nullable String description) {
     * Adds the prerequisite version to the builder.
     * @param prerequisiteVersion the prerequisite version to be added to the builder (can be null)
    public FirmwareBuilder withPrerequisiteVersion(@Nullable String prerequisiteVersion) {
        this.prerequisiteVersion = prerequisiteVersion;
     * Adds the changelog to the builder.
     * @param changelog the changelog to be added to the builder (can be null)
    public FirmwareBuilder withChangelog(@Nullable String changelog) {
        this.changelog = changelog;
     * Adds the online changelog to the builder.
     * @param onlineChangelog the online changelog to be added to the builder (can be null)
    public FirmwareBuilder withOnlineChangelog(@Nullable URL onlineChangelog) {
        this.onlineChangelog = onlineChangelog;
     * Adds the input stream for the binary content to the builder.
     * @param inputStream the input stream for the binary content to be added to the builder (can be null)
    public FirmwareBuilder withInputStream(@Nullable InputStream inputStream) {
        this.inputStream = inputStream;
     * Adds the properties to the builder.
     * @param properties the properties to be added to the builder (not null)
     * @throws IllegalArgumentException if the given properties are null
    public FirmwareBuilder withProperties(Map<String, String> properties) {
        checkNotNull(properties, "Properties");
     * Adds the given md5 hash value to the builder.
     * @param md5Hash the md5 hash value to be added to the builder (can be null)
    public FirmwareBuilder withMd5Hash(@Nullable String md5Hash) {
        this.md5Hash = md5Hash;
     * An additional restriction can be applied on the firmware by providing a
     * {@link FirmwareRestriction} function.
     * @param firmwareRestriction a {@link FirmwareRestriction} for applying an additional
     *            restriction function on the firmware (not null)
     * @throws IllegalArgumentException if the given function is null
    public FirmwareBuilder withFirmwareRestriction(FirmwareRestriction firmwareRestriction) {
        checkNotNull(firmwareRestriction, "Firmware restriction function");
        this.firmwareRestriction = firmwareRestriction;
    private void checkNotNull(@Nullable Object object, String argumentName) {
            throw new IllegalArgumentException(argumentName + " must not be null.");
    private void checkNotNullOrEmpty(@Nullable String string, String argumentName) {
            throw new IllegalArgumentException(argumentName + " must not be null or empty.");
     * Builds the firmware.
     * @return the firmware instance based on this builder
     * @throws IllegalArgumentException when the model restricted property ({@link #withModelRestricted(boolean)}) is
     *             set to true, but the model ({@link #withModel(String)}) is not set
    public Firmware build() {
        if (modelRestricted && (model == null || model.isEmpty())) {
            throw new IllegalArgumentException("Cannot create model restricted firmware without model");
        return new FirmwareImpl(thingTypeUID, vendor, model, modelRestricted, description, version, prerequisiteVersion,
                firmwareRestriction, changelog, onlineChangelog, inputStream, md5Hash, properties);
