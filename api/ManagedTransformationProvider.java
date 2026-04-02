 * The {@link ManagedTransformationProvider} implements a {@link TransformationProvider} for
 * managed transformations stored in a JSON database
@Component(service = { TransformationProvider.class, ManagedTransformationProvider.class }, immediate = true)
public class ManagedTransformationProvider extends
        AbstractManagedProvider<Transformation, String, PersistedTransformation> implements TransformationProvider {
    public ManagedTransformationProvider(final @Reference StorageService storageService) {
        return Transformation.class.getName();
    protected @Nullable Transformation toElement(String key, PersistedTransformation persistableElement) {
        return new Transformation(persistableElement.uid, persistableElement.label, persistableElement.type,
                persistableElement.configuration);
    protected PersistedTransformation toPersistableElement(Transformation element) {
        return new PersistedTransformation(element);
    public void add(Transformation element) {
        checkConfiguration(element);
        super.add(element);
    public @Nullable Transformation update(Transformation element) {
        return super.update(element);
    private static void checkConfiguration(Transformation element) {
        Matcher matcher = TransformationRegistry.CONFIG_UID_PATTERN.matcher(element.getUID());
                    "The transformation configuration UID '" + element.getUID() + "' is invalid.");
        if (!Objects.equals(element.getType(), matcher.group("type"))) {
            throw new IllegalArgumentException("The transformation configuration UID '" + element.getUID()
                    + "' is not matching the type '" + element.getType() + "'.");
    public static class PersistedTransformation {
        public @NonNullByDefault({}) String uid;
        public @NonNullByDefault({}) Map<String, String> configuration;
        protected PersistedTransformation() {
            // default constructor for deserialization
        public PersistedTransformation(Transformation configuration) {
            this.uid = configuration.getUID();
            this.label = configuration.getLabel();
            this.type = configuration.getType();
            this.configuration = configuration.getConfiguration();
