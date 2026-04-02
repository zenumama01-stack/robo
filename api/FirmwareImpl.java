package org.openhab.core.thing.internal.firmware;
import static org.openhab.core.thing.Thing.*;
import java.security.DigestInputStream;
import org.openhab.core.thing.binding.firmware.FirmwareRestriction;
 * Default implementation of {@link Firmware}.
 * @author Dimitar Ivanov - FirmwareUID is replaced by ThingTypeUID and firmware version
public final class FirmwareImpl implements Firmware {
    public static final String PROPERTY_REQUIRES_FACTORY_RESET = "requiresFactoryReset";
    private final Logger logger = LoggerFactory.getLogger(FirmwareImpl.class);
    private final @Nullable String vendor;
    private final @Nullable String model;
    private final boolean modelRestricted;
    private final Version version;
    private final @Nullable Version prerequisiteVersion;
    private final FirmwareRestriction firmwareRestriction;
    private final @Nullable String changelog;
    private final @Nullable URL onlineChangelog;
    private final @Nullable transient InputStream inputStream;
    private final @Nullable String md5Hash;
    private final Map<String, String> properties;
    private transient byte @Nullable [] bytes;
     * Constructs new firmware by the given meta information.
     * @param thingTypeUID thing type UID, that this firmware is associated with (not null)
     * @param vendor the vendor of the firmware (can be null)
     * @param model the model of the firmware (can be null)
     * @param modelRestricted whether the firmware is restricted to a particular model
     * @param description the description of the firmware (can be null)
     * @param version the version of the firmware (not null)
     * @param prerequisiteVersion the prerequisite version of the firmware (can be null)
     * @param firmwareRestriction {@link FirmwareRestriction} for applying an additional restriction on
     *            the firmware (can be null). If null, a default function will be used to return always true
     * @param changelog the changelog of the firmware (can be null)
     * @param onlineChangelog the URL the an online changelog of the firmware (can be null)
     * @param inputStream the input stream for the binary content of the firmware (can be null)
     * @param md5Hash the MD5 hash value of the firmware (can be null)
     * @param properties the immutable properties of the firmware (can be null)
     * @throws IllegalArgumentException if the ThingTypeUID or the firmware version are null
    public FirmwareImpl(ThingTypeUID thingTypeUID, @Nullable String vendor, @Nullable String model,
            boolean modelRestricted, @Nullable String description, String version, @Nullable String prerequisiteVersion,
            @Nullable FirmwareRestriction firmwareRestriction, @Nullable String changelog,
            @Nullable URL onlineChangelog, @Nullable InputStream inputStream, @Nullable String md5Hash,
            @Nullable Map<String, String> properties) {
        ParameterChecks.checkNotNull(thingTypeUID, "ThingTypeUID");
        ParameterChecks.checkNotNullOrEmpty(version, "Firmware version");
        this.version = new Version(version);
        this.prerequisiteVersion = prerequisiteVersion != null ? new Version(prerequisiteVersion) : null;
        this.firmwareRestriction = firmwareRestriction != null ? firmwareRestriction : t -> true;
        this.properties = properties != null ? Collections.unmodifiableMap(properties) : Map.of();
        return thingTypeUID;
    public @Nullable String getVendor() {
        return vendor;
    public @Nullable String getModel() {
    public boolean isModelRestricted() {
        return modelRestricted;
        return version.toString();
    public @Nullable String getPrerequisiteVersion() {
        return (prerequisiteVersion != null) ? prerequisiteVersion.toString() : null;
    public FirmwareRestriction getFirmwareRestriction() {
        return firmwareRestriction;
    public @Nullable String getChangelog() {
        return changelog;
    public @Nullable URL getOnlineChangelog() {
        return onlineChangelog;
    public @Nullable InputStream getInputStream() {
    public @Nullable String getMd5Hash() {
        return md5Hash;
    public synchronized byte @Nullable [] getBytes() {
        if (inputStream == null) {
        if (bytes == null) {
                MessageDigest md = MessageDigest.getInstance("MD5");
                try (DigestInputStream dis = new DigestInputStream(inputStream, md)) {
                    bytes = dis.readAllBytes();
                } catch (IOException ioEx) {
                    logger.error("Cannot read firmware {}.", this, ioEx);
                byte[] digest = md.digest();
                if (md5Hash != null && digest != null) {
                    StringBuilder digestString = new StringBuilder();
                    for (byte b : digest) {
                        digestString.append(String.format("%02x", b));
                    if (!md5Hash.contentEquals(digestString)) {
                        bytes = null;
                                String.format("Invalid MD5 checksum. Expected %s, but was %s.", md5Hash, digestString));
                logger.error("Cannot calculate MD5 checksum.", e);
    public boolean isSuccessorVersion(@Nullable String firmwareVersion) {
        return version.compare(new Version(firmwareVersion)) > 0;
    public boolean isSuitableFor(Thing thing) {
        return hasSameThingType(thing) && hasRequiredModel(thing) && firmwareOnThingIsHighEnough(thing)
                && firmwareRestriction.apply(thing);
    public int compareTo(Firmware firmware) {
        return -version.compare(new Version(firmware.getVersion()));
    private boolean hasSameThingType(Thing thing) {
        return Objects.equals(this.getThingTypeUID(), thing.getThingTypeUID());
    private boolean hasRequiredModel(Thing thing) {
        if (isModelRestricted()) {
            return Objects.equals(this.getModel(), thing.getProperties().get(PROPERTY_MODEL_ID));
    private boolean firmwareOnThingIsHighEnough(Thing thing) {
        if (prerequisiteVersion == null) {
            String firmwareOnThing = thing.getProperties().get(PROPERTY_FIRMWARE_VERSION);
            return firmwareOnThing != null && new Version(firmwareOnThing).compare(prerequisiteVersion) >= 0;
    private static class Version {
        private static final int NO_INT = -1;
        private final String versionString;
        private final String[] parts;
        private Version(String versionString) {
            this.versionString = versionString;
            this.parts = versionString.split("-|_|\\.");
        private int compare(@Nullable Version theVersion) {
            if (theVersion == null) {
            int max = Math.max(parts.length, theVersion.parts.length);
            for (int i = 0; i < max; i++) {
                String partA = i < parts.length ? parts[i] : null;
                String partB = i < theVersion.parts.length ? theVersion.parts[i] : null;
                Integer intA = partA != null && isInt(partA) ? Integer.parseInt(partA) : NO_INT;
                Integer intB = partB != null && isInt(partB) ? Integer.parseInt(partB) : NO_INT;
                if (intA != NO_INT && intB != NO_INT) {
                    if (intA < intB) {
                    if (intA > intB) {
                } else if (partA == null || partB == null) {
                    if (partA == null) {
                    if (partB == null) {
                    int result = partA.compareTo(partB);
        private boolean isInt(String s) {
            return s.matches("^-?\\d+$");
            return versionString.hashCode();
        public boolean equals(@Nullable Object other) {
            if (other == null) {
            } else if (!(other instanceof Version)) {
                return Objects.equals(this.versionString, ((Version) other).versionString);
        result = prime * result + ((changelog == null) ? 0 : changelog.hashCode());
        result = prime * result + ((md5Hash == null) ? 0 : md5Hash.hashCode());
        result = prime * result + ((model == null) ? 0 : model.hashCode());
        result = prime * result + Boolean.hashCode(modelRestricted);
        result = prime * result + ((onlineChangelog == null) ? 0 : onlineChangelog.hashCode());
        result = prime * result + ((prerequisiteVersion == null) ? 0 : prerequisiteVersion.hashCode());
        result = prime * result + ((thingTypeUID == null) ? 0 : thingTypeUID.hashCode());
        result = prime * result + ((vendor == null) ? 0 : vendor.hashCode());
        result = prime * result + ((version == null) ? 0 : version.hashCode());
        result = prime * result + ((properties == null) ? 0 : properties.hashCode());
        FirmwareImpl other = (FirmwareImpl) obj;
        if (changelog == null) {
            if (other.changelog != null) {
        } else if (!changelog.equals(other.changelog)) {
        if (md5Hash == null) {
            if (other.md5Hash != null) {
        } else if (!md5Hash.equals(other.md5Hash)) {
            if (other.model != null) {
        } else if (!model.equals(other.model)) {
        if (modelRestricted != other.modelRestricted) {
        if (onlineChangelog == null) {
            if (other.onlineChangelog != null) {
        } else if (!onlineChangelog.equals(other.onlineChangelog)) {
            if (other.prerequisiteVersion != null) {
        } else if (!prerequisiteVersion.equals(other.prerequisiteVersion)) {
            if (other.thingTypeUID != null) {
        } else if (!thingTypeUID.equals(other.thingTypeUID)) {
        if (vendor == null) {
            if (other.vendor != null) {
        } else if (!vendor.equals(other.vendor)) {
        if (version == null) {
            if (other.version != null) {
        } else if (!version.equals(other.version)) {
            if (other.properties != null) {
        } else if (!properties.equals(other.properties)) {
        return "FirmwareImpl [thingTypeUID=" + thingTypeUID + ", vendor=" + vendor + ", model=" + model
                + ", modelRestricted=" + modelRestricted + ", description=" + description + ", version=" + version
                + ", prerequisiteVersion=" + prerequisiteVersion + ", changelog=" + changelog + ", onlineChangelog="
                + onlineChangelog + ", md5Hash=" + md5Hash + ", properties=" + properties + "]";
