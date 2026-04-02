import com.thoughtworks.xstream.io.xml.StaxDriver;
 * The {@link XmlDocumentReader} is an abstract class used to read XML documents
 * of a certain type and converts them to its according objects.
 * This class uses {@code XStream} and {@code StAX} to parse and convert the XML document.
 * @author Wouter Born - Configure XStream security
public abstract class XmlDocumentReader<@NonNull T> {
    protected static final String[] DEFAULT_ALLOWED_TYPES_WILDCARD = new String[] { "org.openhab.core.**" };
    private final XStream xstream = new XStream(new StaxDriver());
     * The default constructor of this class initializes the {@code XStream} object by calling:
     * <ol>
     * <li>{@link #configureSecurity}</li>
     * <li>{@link #registerConverters}</li>
     * <li>{@link #registerAliases}</li>
     * </ol>
    public XmlDocumentReader() {
        configureSecurity(xstream);
        registerConverters(xstream);
        registerAliases(xstream);
     * Sets the classloader for {@link XStream}.
     * @param classLoader the classloader to set (must not be null)
    protected void setClassLoader(ClassLoader classLoader) {
        xstream.setClassLoader(classLoader);
     * Configures the security of the {@link XStream} instance to protect against vulnerabilities.
     * @param xstream the XStream object to be configured
     * @see <a href="https://x-stream.github.io/security.html">XStream - Security Aspects</a>
    protected void configureSecurity(XStream xstream) {
        xstream.allowTypesByWildcard(DEFAULT_ALLOWED_TYPES_WILDCARD);
     * Registers any {@link Converter}s at the {@link XStream} instance.
    protected abstract void registerConverters(XStream xstream);
     * Registers any aliases at the {@link XStream} instance.
    protected abstract void registerAliases(XStream xstream);
     * Reads the XML document containing a specific XML tag from the specified {@link URL} and converts it to the
     * according object.
     * This method returns {@code null} if the given URL is {@code null}.
     * @param xmlURL the URL pointing to the XML document to be read (could be null)
     * @return the conversion result object (could be null)
     * @throws ConversionException if the specified document contains invalid content
    public @Nullable T readFromXML(URL xmlURL) throws ConversionException {
        return (@Nullable T) xstream.fromXML(xmlURL);
     * Reads the XML document containing a specific XML tag from the specified xml string and converts it to the
     * @param xml a string containing the XML document to be read.
     * @return the conversion result object (could be null).
     * @throws XStreamException if the object cannot be deserialized.
    public @Nullable T readFromXML(String xml) throws ConversionException {
        return (@Nullable T) xstream.fromXML(xml);
