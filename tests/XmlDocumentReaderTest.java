package org.openhab.core.config.xml.util;
import com.acme.Product;
import com.thoughtworks.xstream.security.ForbiddenClassException;
 * Tests {@link XmlDocumentReader}.
public class XmlDocumentReaderTest {
    private static final String OHC_PACKAGE_PREFIX = "org.openhab.core.";
    private static class ConfigDescriptionReader extends XmlDocumentReader<ConfigDescription> {
    private @Nullable ConfigDescription readXML(String xml) throws IOException {
        Path tempFile = Files.createTempFile(null, null);
        tempFile.toFile().deleteOnExit();
        Files.writeString(tempFile, xml);
        return new ConfigDescriptionReader().readFromXML(tempFile.toUri().toURL());
    public void defaultSecurityAllowsDeserializingOHCobjects() throws Exception {
        assertThat(ConfigDescription.class.getPackageName(), startsWith(OHC_PACKAGE_PREFIX));
        URI testURI = URI.create("test:uri");
        ConfigDescription configDescription = ConfigDescriptionBuilder.create(testURI).build();
        String xml = new XStream().toXML(configDescription);
        ConfigDescription readConfigDescription = requireNonNull(readXML(xml));
        assertThat(readConfigDescription.getUID(), is(testURI));
    public void defaultSecurityDisallowsDeserializingNonOHCobjects() throws Exception {
        assertThat(Product.class.getPackageName(), not(startsWith(OHC_PACKAGE_PREFIX)));
        String xml = new XStream().toXML(new Product());
        assertThrows(ForbiddenClassException.class, () -> readXML(xml));
     * @see <a href="https://x-stream.github.io/CVE-2013-7285.html">XStream - CVE-2013-7285</a>
    public void defaultSecurityProtectsAgainstRemoteCodeExecution() throws Exception {
        String xml = """
                <contact class='dynamic-proxy'>
                  <interface>org.openhab.core.Contact</interface>
                  <handler class='java.beans.EventHandler'>
                    <target class='java.lang.ProcessBuilder'>
                      <command>
                        <string>calc.exe</string>
                      </command>
                    </target>
                    <action>start</action>
                  </handler>
                </contact>""";
     * @see <a href="https://x-stream.github.io/CVE-2017-7957.html">XStream - CVE-2017-7957</a>
    public void defaultSecurityProtectsAgainstDenialOfServiceAttacks() throws Exception {
        assertThrows(ForbiddenClassException.class, () -> readXML("<void/>"));
        assertThrows(ForbiddenClassException.class, () -> readXML("<string class='void'>Hello, world!</string>"));
