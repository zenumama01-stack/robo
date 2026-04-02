package org.openhab.core.internal.i18n;
import static org.openhab.core.library.unit.MetricPrefix.HECTO;
import java.time.DateTimeException;
import javax.measure.quantity.Acceleration;
import javax.measure.quantity.AmountOfSubstance;
import javax.measure.quantity.Angle;
import javax.measure.quantity.Area;
import javax.measure.quantity.CatalyticActivity;
import javax.measure.quantity.ElectricCapacitance;
import javax.measure.quantity.ElectricCharge;
import javax.measure.quantity.ElectricConductance;
import javax.measure.quantity.ElectricInductance;
import javax.measure.quantity.ElectricResistance;
import javax.measure.quantity.Energy;
import javax.measure.quantity.Force;
import javax.measure.quantity.Frequency;
import javax.measure.quantity.Illuminance;
import javax.measure.quantity.LuminousFlux;
import javax.measure.quantity.LuminousIntensity;
import javax.measure.quantity.MagneticFlux;
import javax.measure.quantity.MagneticFluxDensity;
import javax.measure.quantity.Mass;
import javax.measure.quantity.Power;
import javax.measure.quantity.RadiationDoseAbsorbed;
import javax.measure.quantity.RadiationDoseEffective;
import javax.measure.quantity.Radioactivity;
import javax.measure.quantity.SolidAngle;
import javax.measure.quantity.Speed;
import javax.measure.quantity.Time;
import javax.measure.quantity.Volume;
import org.openhab.core.i18n.LocationProvider;
import org.openhab.core.library.dimension.ArealDensity;
import org.openhab.core.library.dimension.CalorificValue;
import org.openhab.core.library.dimension.Currency;
import org.openhab.core.library.dimension.DataAmount;
import org.openhab.core.library.dimension.DataTransferRate;
import org.openhab.core.library.dimension.Density;
import org.openhab.core.library.dimension.ElectricConductivity;
import org.openhab.core.library.dimension.EmissionIntensity;
import org.openhab.core.library.dimension.EnergyPrice;
import org.openhab.core.library.dimension.Intensity;
import org.openhab.core.library.dimension.RadiantExposure;
import org.openhab.core.library.dimension.RadiationDoseRate;
import org.openhab.core.library.dimension.RadiationSpecificActivity;
import org.openhab.core.library.dimension.VolumePrice;
import org.openhab.core.library.dimension.VolumetricFlowRate;
import org.openhab.core.library.unit.CurrencyUnits;
 * The {@link I18nProviderImpl} is a concrete implementation of the {@link TranslationProvider}, {@link LocaleProvider},
 * and {@link LocationProvider} service interfaces.
 * This implementation uses the i18n mechanism of Java ({@link java.util.ResourceBundle}) to translate a
 * given key into text. The
 * resources must be placed under the specific directory {@link LanguageResourceBundleManager#RESOURCE_DIRECTORY} within
 * the certain modules. Each module is tracked in the platform by using the {@link ResourceBundleTracker} and managed by
 * using one certain {@link LanguageResourceBundleManager} which is responsible for the translation.
 * It reads a user defined configuration to set a locale and a location for this installation. Options for the
 * parameters "language", "region", "variant" and "timezone" are provided by the I18nConfigOptionsProvider.
 * @author Christoph Weitkamp - Added price per volume
@Component(immediate = true, configurationPid = I18nProviderImpl.CONFIGURATION_PID, property = {
        Constants.SERVICE_PID + "=org.openhab.i18n", //
        "service.config.label=Regional Settings", //
        "service.config.category=system", //
        "service.config.description.uri=system:i18n" })
public class I18nProviderImpl
        implements TranslationProvider, LocaleProvider, LocationProvider, TimeZoneProvider, UnitProvider {
    private final Logger logger = LoggerFactory.getLogger(I18nProviderImpl.class);
    public static final String CONFIGURATION_PID = "org.openhab.i18n";
    // LocaleProvider
    public static final String LANGUAGE = "language";
    public static final String SCRIPT = "script";
    public static final String REGION = "region";
    public static final String VARIANT = "variant";
    private @Nullable Locale locale;
    // TranslationProvider
    private final ResourceBundleTracker resourceBundleTracker;
    // LocationProvider
    static final String LOCATION = "location";
    private @Nullable PointType location;
    // TimeZoneProvider
    static final String TIMEZONE = "timezone";
    private @Nullable ZoneId timeZone;
    // UnitProvider
    static final String MEASUREMENT_SYSTEM = "measurementSystem";
    private @Nullable SystemOfUnits measurementSystem;
    private static final Map<Class<? extends Quantity<?>>, Map<SystemOfUnits, Unit<? extends Quantity<?>>>> DIMENSION_MAP = getDimensionMap();
    public I18nProviderImpl(ComponentContext componentContext) {
        getDimensionMap();
        modified((Map<String, Object>) componentContext.getProperties());
        this.resourceBundleTracker = new ResourceBundleTracker(componentContext.getBundleContext(), this);
        this.resourceBundleTracker.open();
        this.resourceBundleTracker.close();
        final String language = toStringOrNull(config.get(LANGUAGE));
        final String script = toStringOrNull(config.get(SCRIPT));
        final String region = toStringOrNull(config.get(REGION));
        final String variant = toStringOrNull(config.get(VARIANT));
        final String location = toStringOrNull(config.get(LOCATION));
        final String zoneId = toStringOrNull(config.get(TIMEZONE));
        final String measurementSystem = toStringOrNull(config.get(MEASUREMENT_SYSTEM));
        setTimeZone(zoneId);
        setLocation(location);
        setLocale(language, script, region, variant);
        setMeasurementSystem(measurementSystem);
    private void setMeasurementSystem(@Nullable String measurementSystem) {
        SystemOfUnits oldMeasurementSystem = this.measurementSystem;
        final String ms;
        if (measurementSystem == null || measurementSystem.isEmpty()) {
            ms = "";
            ms = measurementSystem;
        final SystemOfUnits newMeasurementSystem;
        switch (ms) {
            case SIUnits.MEASUREMENT_SYSTEM_NAME -> newMeasurementSystem = SIUnits.getInstance();
            case ImperialUnits.MEASUREMENT_SYSTEM_NAME -> newMeasurementSystem = ImperialUnits.getInstance();
                logger.debug("Error setting measurement system for value '{}'.", measurementSystem);
                newMeasurementSystem = null;
        this.measurementSystem = newMeasurementSystem;
        if (oldMeasurementSystem != null && newMeasurementSystem == null) {
            logger.info("Measurement system is not set, falling back to locale based system.");
        } else if (newMeasurementSystem != null && !newMeasurementSystem.equals(oldMeasurementSystem)) {
            logger.info("Measurement system set to '{}'.", newMeasurementSystem.getName());
    private void setLocale(@Nullable String language, @Nullable String script, @Nullable String region,
            @Nullable String variant) {
        Locale oldLocale = this.locale;
        if (language == null || language.isEmpty()) {
            // at least the language must be defined otherwise the system default locale is used
            logger.debug("No language set, setting locale to 'null'.");
            locale = null;
            if (oldLocale != null) {
                logger.info("Locale is not set, falling back to the default locale");
        final Locale.Builder builder = new Locale.Builder();
            builder.setLanguage(language);
            logger.warn("Language ({}) is invalid. Cannot create locale, keep old one.", language, ex);
            builder.setScript(script);
            logger.warn("Script ({}) is invalid. Skip it.", script, ex);
            builder.setRegion(region);
            logger.warn("Region ({}) is invalid. Skip it.", region, ex);
            builder.setVariant(variant);
            logger.warn("Variant ({}) is invalid. Skip it.", variant, ex);
        final Locale newLocale = builder.build();
        locale = newLocale;
        if (!newLocale.equals(oldLocale)) {
            logger.info("Locale set to '{}'.", newLocale);
    private @Nullable String toStringOrNull(@Nullable Object value) {
        return value == null ? null : value.toString();
    private void setLocation(final @Nullable String location) {
        PointType oldLocation = this.location;
        PointType newLocation;
        if (location == null || location.isEmpty()) {
            newLocation = null;
                newLocation = PointType.valueOf(location);
                newLocation = oldLocation;
                // preserve old location or null if none was set before
                logger.warn("Could not set new location: {}, keeping old one, error message: {}", location,
        if (!Objects.equals(newLocation, oldLocation)) {
            this.location = newLocation;
            logger.info("Location set to '{}'.", newLocation);
    private void setTimeZone(final @Nullable String zoneId) {
        ZoneId oldTimeZone = this.timeZone;
        if (zoneId == null || zoneId.isBlank()) {
            timeZone = null;
                timeZone = ZoneId.of(zoneId);
            } catch (DateTimeException e) {
                logger.warn("Error setting time zone '{}', falling back to the default time zone: {}", zoneId,
        if (oldTimeZone != null && this.timeZone == null) {
            logger.info("Time zone is not set, falling back to the default time zone.");
        } else if (this.timeZone instanceof ZoneId zId && !zId.equals(oldTimeZone)) {
            logger.info("Time zone set to '{}'.", this.timeZone);
    public @Nullable PointType getLocation() {
        final ZoneId timeZone = this.timeZone;
        if (timeZone == null) {
            return ZoneId.systemDefault();
        return timeZone;
        final Locale locale = this.locale;
            return Locale.getDefault();
        LanguageResourceBundleManager languageResource = this.resourceBundleTracker.getLanguageResource(bundle);
        if (languageResource != null) {
            String text = languageResource.getText(key, locale);
            if (text != null) {
        String text = getText(bundle, key, defaultText, locale);
                return MessageFormat.format(text, arguments);
            logger.warn("Failed to format message '{}' with parameters {}. This is a bug.", text, arguments);
        Map<SystemOfUnits, Unit<? extends Quantity<?>>> map = DIMENSION_MAP.get(dimension);
        if (map == null) {
            throw new IllegalArgumentException("Dimension " + dimension.getName() + " is unknown. This is a bug.");
        Unit<T> unit = (Unit<T>) map.get(getMeasurementSystem());
        return Objects.requireNonNull(unit);
        final SystemOfUnits measurementSystem = this.measurementSystem;
        if (measurementSystem != null) {
            return measurementSystem;
        // Only US and Liberia use the Imperial System.
        if (Locale.US.equals(locale) || Locale.forLanguageTag("en-LR").equals(locale)) {
            return ImperialUnits.getInstance();
        return Set.copyOf(getDimensionMap().keySet());
    public static Map<Class<? extends Quantity<?>>, Map<SystemOfUnits, Unit<? extends Quantity<?>>>> getDimensionMap() {
        Map<Class<? extends Quantity<?>>, Map<SystemOfUnits, Unit<? extends Quantity<?>>>> dimensionMap = new HashMap<>();
        addDefaultUnit(dimensionMap, Acceleration.class, Units.METRE_PER_SQUARE_SECOND);
        addDefaultUnit(dimensionMap, AmountOfSubstance.class, Units.MOLE);
        addDefaultUnit(dimensionMap, Angle.class, Units.DEGREE_ANGLE, Units.DEGREE_ANGLE);
        addDefaultUnit(dimensionMap, Area.class, SIUnits.SQUARE_METRE, ImperialUnits.SQUARE_FOOT);
        addDefaultUnit(dimensionMap, ArealDensity.class, Units.DOBSON_UNIT);
        addDefaultUnit(dimensionMap, CalorificValue.class, Units.KILOWATT_HOUR_PER_CUBICMETRE);
        addDefaultUnit(dimensionMap, CatalyticActivity.class, Units.KATAL);
        addDefaultUnit(dimensionMap, Currency.class, CurrencyUnits.BASE_CURRENCY);
        addDefaultUnit(dimensionMap, DataAmount.class, Units.BYTE);
        addDefaultUnit(dimensionMap, DataTransferRate.class, Units.MEGABIT_PER_SECOND);
        addDefaultUnit(dimensionMap, Density.class, Units.KILOGRAM_PER_CUBICMETRE);
        addDefaultUnit(dimensionMap, Dimensionless.class, Units.ONE);
        addDefaultUnit(dimensionMap, ElectricCapacitance.class, Units.FARAD);
        addDefaultUnit(dimensionMap, ElectricCharge.class, Units.COULOMB);
        addDefaultUnit(dimensionMap, ElectricConductance.class, Units.SIEMENS);
        addDefaultUnit(dimensionMap, ElectricConductivity.class, Units.SIEMENS_PER_METRE);
        addDefaultUnit(dimensionMap, ElectricCurrent.class, Units.AMPERE);
        addDefaultUnit(dimensionMap, ElectricInductance.class, Units.HENRY);
        addDefaultUnit(dimensionMap, ElectricPotential.class, Units.VOLT);
        addDefaultUnit(dimensionMap, ElectricResistance.class, Units.OHM);
        addDefaultUnit(dimensionMap, EmissionIntensity.class, Units.GRAM_PER_KILOWATT_HOUR);
        addDefaultUnit(dimensionMap, Energy.class, Units.KILOWATT_HOUR);
        addDefaultUnit(dimensionMap, EnergyPrice.class, CurrencyUnits.BASE_ENERGY_PRICE);
        addDefaultUnit(dimensionMap, Force.class, Units.NEWTON);
        addDefaultUnit(dimensionMap, Frequency.class, Units.HERTZ);
        addDefaultUnit(dimensionMap, Illuminance.class, Units.LUX);
        addDefaultUnit(dimensionMap, Intensity.class, Units.IRRADIANCE);
        addDefaultUnit(dimensionMap, Length.class, SIUnits.METRE, ImperialUnits.INCH);
        addDefaultUnit(dimensionMap, LuminousFlux.class, Units.LUMEN);
        addDefaultUnit(dimensionMap, LuminousIntensity.class, Units.CANDELA);
        addDefaultUnit(dimensionMap, MagneticFlux.class, Units.WEBER);
        addDefaultUnit(dimensionMap, MagneticFluxDensity.class, Units.TESLA);
        addDefaultUnit(dimensionMap, Mass.class, SIUnits.KILOGRAM, ImperialUnits.POUND);
        addDefaultUnit(dimensionMap, Power.class, Units.WATT);
        addDefaultUnit(dimensionMap, Pressure.class, HECTO(SIUnits.PASCAL), ImperialUnits.INCH_OF_MERCURY);
        addDefaultUnit(dimensionMap, RadiationDoseRate.class, Units.SIEVERT_PER_HOUR);
        addDefaultUnit(dimensionMap, RadiationDoseAbsorbed.class, Units.GRAY);
        addDefaultUnit(dimensionMap, RadiationDoseEffective.class, Units.SIEVERT);
        addDefaultUnit(dimensionMap, RadiationSpecificActivity.class, Units.BECQUEREL_PER_CUBIC_METRE);
        addDefaultUnit(dimensionMap, RadiantExposure.class, Units.JOULE_PER_SQUARE_METRE);
        addDefaultUnit(dimensionMap, Radioactivity.class, Units.BECQUEREL);
        addDefaultUnit(dimensionMap, SolidAngle.class, Units.STERADIAN);
        addDefaultUnit(dimensionMap, Speed.class, SIUnits.KILOMETRE_PER_HOUR, ImperialUnits.MILES_PER_HOUR);
        addDefaultUnit(dimensionMap, Temperature.class, SIUnits.CELSIUS, ImperialUnits.FAHRENHEIT);
        addDefaultUnit(dimensionMap, Time.class, Units.SECOND);
        addDefaultUnit(dimensionMap, Volume.class, SIUnits.CUBIC_METRE, ImperialUnits.GALLON_LIQUID_US);
        addDefaultUnit(dimensionMap, VolumePrice.class, CurrencyUnits.PRICE_PER_LITRE);
        addDefaultUnit(dimensionMap, VolumePrice.class, CurrencyUnits.PRICE_PER_CUBIC_METRE);
        addDefaultUnit(dimensionMap, VolumePrice.class, CurrencyUnits.PRICE_PER_GALLON_LIQUID_US);
        addDefaultUnit(dimensionMap, VolumetricFlowRate.class, Units.LITRE_PER_MINUTE, ImperialUnits.GALLON_PER_MINUTE);
        return dimensionMap;
    private static <T extends Quantity<T>> void addDefaultUnit(
            Map<Class<? extends Quantity<?>>, Map<SystemOfUnits, Unit<? extends Quantity<?>>>> dimensionMap,
            Class<T> dimension, Unit<T> siUnit, Unit<T> imperialUnit) {
        dimensionMap.put(dimension, Map.of(SIUnits.getInstance(), siUnit, ImperialUnits.getInstance(), imperialUnit));
            Class<T> dimension, Unit<T> unit) {
        dimensionMap.put(dimension, Map.of(SIUnits.getInstance(), unit, ImperialUnits.getInstance(), unit));
