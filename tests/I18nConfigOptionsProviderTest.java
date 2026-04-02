import org.hamcrest.collection.IsEmptyCollection;
 * Tests for {@link I18nConfigOptionsProvider}
public class I18nConfigOptionsProviderTest {
    private final I18nConfigOptionsProvider provider = new I18nConfigOptionsProvider();
    private final URI uriI18N = URI.create("system:i18n");
    private final ParameterOption empty = new ParameterOption("", "");
    private final ParameterOption expectedLangEN = new ParameterOption("en", "English");
    private final ParameterOption expectedLangFR = new ParameterOption("en", "anglais");
    private final ParameterOption expectedCntryEN = new ParameterOption("US", "United States");
    private final ParameterOption expectedCntryFRJava8 = new ParameterOption("US", "Etats-Unis");
    private final ParameterOption expectedCntryFRJava9 = new ParameterOption("US", "États-Unis");
    public void testLanguage() {
        assertThat(provider.getParameterOptions(uriI18N, "language", null, Locale.US), hasItem(expectedLangEN));
        assertThat(provider.getParameterOptions(uriI18N, "language", null, Locale.US), not(hasItem(empty)));
        assertThat(provider.getParameterOptions(uriI18N, "language", null, Locale.FRENCH), hasItem(expectedLangFR));
        assertThat(provider.getParameterOptions(uriI18N, "language", null, Locale.FRENCH), not(hasItem(empty)));
        assertThat(provider.getParameterOptions(uriI18N, "language", null, null), not(IsEmptyCollection.empty()));
    public void testRegion() {
        assertThat(provider.getParameterOptions(uriI18N, "region", null, Locale.US), hasItem(expectedCntryEN));
        assertThat(provider.getParameterOptions(uriI18N, "region", null, Locale.US), not(hasItem(empty)));
        assertThat(provider.getParameterOptions(uriI18N, "region", null, Locale.FRENCH),
                anyOf(hasItem(expectedCntryFRJava8), hasItem(expectedCntryFRJava9)));
        assertThat(provider.getParameterOptions(uriI18N, "region", null, Locale.FRENCH), not(hasItem(empty)));
        assertThat(provider.getParameterOptions(uriI18N, "region", null, null), not(IsEmptyCollection.empty()));
    public void testUnknownParameter() {
        assertThat(provider.getParameterOptions(uriI18N, "unknown", null, Locale.US), nullValue());
