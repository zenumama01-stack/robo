public class MappingUriExtensionsTest {
    public @TempDir @NonNullByDefault({}) File folder;
    private @NonNullByDefault({}) File confFolder;
                { "conf", //
                        "file:///q:/conf", //
                        "" }, //
                        "file:///q:", //
                        "file://fqdn/openhab-conf", //
                        "file://fqdn/openhab-conf/conf", //
                { null, //
                        "file:///asdf/conf/items/test.items", //
                        "file:///asdf/conf", //
                        "items/test.items" }, //
                        "file:///asdf/items/test.items", //
                        "file:///asdf", //
                        "file://fqdn/openhab-conf/conf/items/test.items", //
                        "file://fqdn/openhab-conf/items/test.items", //
                        "items/test.items" },//
        confFolder = new File(folder, "conf");
        File itemsFolder = new File(confFolder, "items");
        itemsFolder.mkdirs();
        File itemsFile = new File(itemsFolder, "test.items");
        itemsFile.deleteOnExit();
        itemsFile.createNewFile();
    public void testGuessClientPath(String conf, String request, String expectedClientPath, String expectedUriPath) {
        MappingUriExtensions mapper = createMapper(conf);
        String clientPath = mapper.guessClientPath(request);
        assertEquals(expectedClientPath, clientPath);
    public void testToUri(String conf, String request, String expectedClientPath, String expectedUriPath) {
        URI clientPath = mapper.toUri(request);
        assertEquals(confFolder.toPath().toUri().toString() + expectedUriPath, clientPath.toString());
    private MappingUriExtensions createMapper(String conf) {
        return new MappingUriExtensions(conf) {
            protected String calcServerLocation(@Nullable String configFolder) {
                // ensure test execution is independent from the current working directory
                return removeTrailingSlash(confFolder.toPath().toUri().toString());
