package org.openhab.core.magic.internal.metadata;
import static org.openhab.core.config.core.ConfigDescriptionParameterBuilder.create;
 * Describes the metadata for the "magic" namespace.
public class MagicMetadataProvider implements MetadataConfigDescriptionProvider {
    public String getNamespace() {
        return "magic";
    public @Nullable String getDescription(@Nullable Locale locale) {
        return "Make items magic";
    public @Nullable List<ParameterOption> getParameterOptions(@Nullable Locale locale) {
                new ParameterOption("just", "Just Magic"), //
                new ParameterOption("pure", "Pure Magic") //
    public @Nullable List<ConfigDescriptionParameter> getParameters(String value, @Nullable Locale locale) {
            case "just":
                        create("electric", Type.BOOLEAN).withLabel("Use Electricity").build() //
            case "pure":
                        create("spell", Type.TEXT).withLabel("Spell").withDescription("The exact spell to use").build(), //
                        create("price", Type.DECIMAL).withLabel("Price")
                                .withDescription("...because magic always comes with a price").build(), //
                        create("power", Type.INTEGER).withLabel("Power").withLimitToOptions(true).withOptions( //
                                List.of( //
                                        new ParameterOption("0", "Very High"), //
                                        new ParameterOption("1", "Incredible"), //
                                        new ParameterOption("2", "Insane"), //
                                        new ParameterOption("3", "Ludicrous") //
                                )).build() //
