import com.fasterxml.jackson.core.Version;
import com.fasterxml.jackson.databind.AnnotationIntrospector;
import com.fasterxml.jackson.databind.PropertyName;
import com.fasterxml.jackson.databind.introspect.Annotated;
import com.google.gson.annotations.SerializedName;
 * This is a {@link SerializedNameAnnotationIntrospector}, which processes SerializedName annotations.
 * @author Boris Krivonog - Initial contribution
final class SerializedNameAnnotationIntrospector extends AnnotationIntrospector {
    private static final long serialVersionUID = 1L;
    @NonNullByDefault({})
    public PropertyName findNameForDeserialization(Annotated annotated) {
        return Optional.ofNullable(annotated.getAnnotation(SerializedName.class)).map(s -> new PropertyName(s.value()))
                .orElseGet(() -> super.findNameForDeserialization(annotated));
    public List<PropertyName> findPropertyAliases(Annotated annotated) {
        return Optional.ofNullable(annotated.getAnnotation(SerializedName.class))
                .map(s -> Stream.of(s.alternate()).map(PropertyName::new).toList())
                .orElseGet(() -> super.findPropertyAliases(annotated));
    public Version version() {
        return Version.unknownVersion();
