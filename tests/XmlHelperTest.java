public class XmlHelperTest {
    public void whenUIDContainsDotShouldBeconvcertedToColon() {
        assertThat(XmlHelper.getSystemUID("system.test"), is("system:test"));
    public void whenNoPrefixIsGivenShouldPrependSystemPrefix() {
        assertThat(XmlHelper.getSystemUID("test"), is("system:test"));
