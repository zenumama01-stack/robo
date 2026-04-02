 * The {@link AddonInfoXmlResult} is an intermediate XML conversion result object which
 * contains a mandatory {@link AddonInfo} and an optional {@link ConfigDescription} object.
 * If a {@link ConfigDescription} object exists, it must be added to the according
 * {@link org.openhab.core.config.core.ConfigDescriptionProvider}.
public record AddonInfoXmlResult(AddonInfo addonInfo, @Nullable ConfigDescription configDescription) {
