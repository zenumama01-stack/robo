import org.eclipse.xtext.parser.IEncodingProvider;
 * {@link IEncodingProvider} implementation for scripts.
 * It makes sure that synthetic resources are interpreted as UTF-8 because they will be handed in as strings and turned
 * into UTF-8 encoded streams by the script engine.
public class ScriptEncodingProvider implements IEncodingProvider {
    public String getEncoding(URI uri) {
        if (uri.toString().startsWith("__synthetic")) {
            return StandardCharsets.UTF_8.name();
        return Charset.defaultCharset().name();
