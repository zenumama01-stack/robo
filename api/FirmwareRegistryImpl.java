 * Default implementation of {@link FirmwareRegistry}.
 * @author Dimitar Ivanov - The firmwares are provided by thing and version
@Component(immediate = true, service = FirmwareRegistry.class)
public final class FirmwareRegistryImpl implements FirmwareRegistry {
    private final Logger logger = LoggerFactory.getLogger(FirmwareRegistryImpl.class);
    private final List<FirmwareProvider> firmwareProviders = new CopyOnWriteArrayList<>();
    public FirmwareRegistryImpl(final @Reference LocaleProvider localeProvider) {
    public @Nullable Firmware getFirmware(Thing thing, String firmwareVersion) {
        return getFirmware(thing, firmwareVersion, localeProvider.getLocale());
    public @Nullable Firmware getFirmware(Thing thing, String firmwareVersion, @Nullable Locale locale) {
        ParameterChecks.checkNotNull(thing, "Thing");
        ParameterChecks.checkNotNullOrEmpty(firmwareVersion, "Firmware version");
        Locale loc = locale != null ? locale : localeProvider.getLocale();
        for (FirmwareProvider firmwareProvider : firmwareProviders) {
                Firmware firmware = firmwareProvider.getFirmware(thing, firmwareVersion, loc);
                if (firmware != null && firmware.isSuitableFor(thing)) {
                    return firmware;
                        "Unexpected exception occurred for firmware provider {} while getting firmware with version {} for thing {}",
                        firmwareProvider.getClass().getSimpleName(), firmwareVersion, thing.getThingTypeUID(), e);
    public @Nullable Firmware getLatestFirmware(Thing thing) {
        return getLatestFirmware(thing, localeProvider.getLocale());
    public @Nullable Firmware getLatestFirmware(Thing thing, @Nullable Locale locale) {
        Collection<Firmware> firmwares = getFirmwares(thing, loc);
        Optional<Firmware> first = firmwares.stream().findFirst();
        // Used as workaround for the NonNull annotation implied to .isElse()
        if (first.isPresent()) {
            return first.get();
    public Collection<Firmware> getFirmwares(Thing thing) {
        return getFirmwares(thing, localeProvider.getLocale());
    public Collection<Firmware> getFirmwares(Thing thing, @Nullable Locale locale) {
        Set<Firmware> firmwares = new TreeSet<>();
                Collection<Firmware> result = firmwareProvider.getFirmwares(thing, loc);
                    result.stream().filter(firmware -> firmware.isSuitableFor(thing)).forEach(firmwares::add);
                        "Unexpected exception occurred for firmware provider {} while getting firmwares for thing {}.",
                        firmwareProvider.getClass().getSimpleName(), thing.getUID(), e);
        return Collections.unmodifiableCollection(firmwares);
    protected void addFirmwareProvider(FirmwareProvider firmwareProvider) {
        firmwareProviders.add(firmwareProvider);
    protected void removeFirmwareProvider(FirmwareProvider firmwareProvider) {
        firmwareProviders.remove(firmwareProvider);
