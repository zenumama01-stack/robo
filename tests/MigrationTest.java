import static org.mockito.Mockito.spy;
import org.openhab.core.storage.json.internal.migration.RenamingTypeMigrator;
 * The {@link MigrationTest} is a
public class MigrationTest {
    private static final String OBJECT_KEY = "foo";
    private static final String OBJECT_VALUE = "bar";
        // store old class
        OldNameClass oldNameInstance = new OldNameClass(OBJECT_VALUE);
        JsonStorage<OldNameClass> storage = new JsonStorage<>(tmpFile, this.getClass().getClassLoader(), 0, 0, 0,
        storage.put(OBJECT_KEY, oldNameInstance);
    public void testRenameClassMigration() throws TypeMigrationException {
        TypeMigrator typeMigrator = spy(
                new RenamingTypeMigrator(OldNameClass.class.getName(), NewNameClass.class.getName()));
        // read new class
        JsonStorage<NewNameClass> storage1 = new JsonStorage<>(tmpFile, this.getClass().getClassLoader(), 0, 0, 0,
                List.of(typeMigrator));
        NewNameClass newNameInstance = storage1.get(OBJECT_KEY);
        verify(typeMigrator).getOldType();
        verify(typeMigrator).getNewType();
        verify(typeMigrator).migrate(any());
        Objects.requireNonNull(newNameInstance);
        assertThat(OBJECT_VALUE, is(newNameInstance.value));
        // ensure type migrations are stored
        storage1.flush();
        newNameInstance = storage1.get(OBJECT_KEY);
        verifyNoMoreInteractions(typeMigrator);
    public void testRenameFieldMigration() throws TypeMigrationException {
        TypeMigrator typeMigrator = spy(new OldToNewFieldMigrator());
        JsonStorage<NewFieldClass> storage1 = new JsonStorage<>(tmpFile, this.getClass().getClassLoader(), 0, 0, 0,
        NewFieldClass newNameInstance = storage1.get(OBJECT_KEY);
        assertThat(OBJECT_VALUE, is(newNameInstance.val));
    private static class OldNameClass {
        public OldNameClass(String value) {
    private static class NewNameClass {
        public NewNameClass(String value) {
    private static class NewFieldClass {
        public String val;
        public NewFieldClass(String value) {
            this.val = value;
    private static class OldToNewFieldMigrator implements TypeMigrator {
            return OldNameClass.class.getName();
            return NewFieldClass.class.getName();
            JsonObject newElement = oldValue.getAsJsonObject();
            JsonElement element = newElement.remove("value");
            newElement.add("val", element);
            return newElement;
