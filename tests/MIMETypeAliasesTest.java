 * The {@link MIMETypeAliasesTest} contains tests for the {@link MIMETypeAliases} class.
public class MIMETypeAliasesTest {
    public void testAliasToType() {
        assertThat(MIMETypeAliases.aliasToType("a"), is("a"));
        assertThat(MIMETypeAliases.aliasToType(""), is(""));
        assertThat(MIMETypeAliases.aliasToType("DSL"), is("application/vnd.openhab.dsl.rule"));
        assertThat(MIMETypeAliases.aliasToType("RuleDSL"), is("application/vnd.openhab.dsl.rule"));
        assertThat(MIMETypeAliases.aliasToType("Rule DSL"), is("application/vnd.openhab.dsl.rule"));
        assertThat(MIMETypeAliases.aliasToType("Groovy"), is("application/x-groovy"));
        assertThat(MIMETypeAliases.aliasToType("JavaScript"), is("application/javascript"));
        assertThat(MIMETypeAliases.aliasToType("JS"), is("application/javascript"));
        assertThat(MIMETypeAliases.aliasToType("GraalJS"), is("application/javascript"));
        assertThat(MIMETypeAliases.aliasToType("Python"), is("application/python"));
        assertThat(MIMETypeAliases.aliasToType("Python3"), is("application/python"));
        assertThat(MIMETypeAliases.aliasToType("PY"), is("application/python"));
        assertThat(MIMETypeAliases.aliasToType("PY3"), is("application/python"));
        assertThat(MIMETypeAliases.aliasToType("Jython"), is("application/x-python2"));
        assertThat(MIMETypeAliases.aliasToType("JythonPY"), is("application/x-python2"));
        assertThat(MIMETypeAliases.aliasToType("PY2"), is("application/x-python2"));
        assertThat(MIMETypeAliases.aliasToType("Ruby"), is("application/x-ruby"));
        assertThat(MIMETypeAliases.aliasToType("RB"), is("application/x-ruby"));
        assertThat(MIMETypeAliases.aliasToType("NashornJS"), is("application/javascript;version=ECMAScript-5.1"));
        assertThat(MIMETypeAliases.aliasToType("ECMAScript5.1"), is("application/javascript;version=ECMAScript-5.1"));
        assertThat(MIMETypeAliases.aliasToType("ECMAScript 5.1"), is("application/javascript;version=ECMAScript-5.1"));
    public void testMimeTypeToAlias() {
        assertThat(MIMETypeAliases.mimeTypeToAlias("a"), is("a"));
        assertThat(MIMETypeAliases.mimeTypeToAlias(""), is(""));
        assertThat(MIMETypeAliases.mimeTypeToAlias("application/vnd.openhab.dsl.rule"), is("DSL"));
        assertThat(MIMETypeAliases.mimeTypeToAlias("application/x-groovy"), is("Groovy"));
        assertThat(MIMETypeAliases.mimeTypeToAlias("application/javascript"), is("JavaScript"));
        assertThat(MIMETypeAliases.mimeTypeToAlias("application/python"), is("Python"));
        assertThat(MIMETypeAliases.mimeTypeToAlias("application/x-python2"), is("Jython"));
        assertThat(MIMETypeAliases.mimeTypeToAlias("application/x-ruby"), is("Ruby"));
        assertThat(MIMETypeAliases.mimeTypeToAlias("application/javascript;version=ECMAScript-5.1"), is("NashornJS"));
