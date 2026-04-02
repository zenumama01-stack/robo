 * Tests cases for {@link MagicService}.
public class MagicServiceImplTest {
    private static final String PARAMETER_NAME = "select_decimal_limit";
    private @NonNullByDefault({}) MagicService magicService;
        magicService = new MagicServiceImpl();
    public void shouldProvideConfigOptionsForURIAndParameterName() {
        Collection<ParameterOption> parameterOptions = magicService.getParameterOptions(MagicService.CONFIG_URI,
                PARAMETER_NAME, null, null);
        assertThat(parameterOptions, hasSize(3));
    public void shouldProvidemtpyListForInvalidURI() {
        Collection<ParameterOption> parameterOptions = magicService.getParameterOptions(URI.create("system.audio"),
        assertNull(parameterOptions);
    public void shouldProvidemtpyListForInvalidParameterName() {
                "some_param_name", null, null);
