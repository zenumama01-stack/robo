public class InstanceUUIDTest {
    public void sameUUID() {
        String uuid1 = InstanceUUID.get();
        String uuid2 = InstanceUUID.get();
        assertEquals(uuid1, uuid2);
    public void readFromPersistedFile() throws IOException {
        // we first need to remove the cached value
        InstanceUUID.uuid = null;
        Path path = Path.of(OpenHAB.getUserDataFolder(), InstanceUUID.UUID_FILE_NAME);
        Files.createDirectories(path.getParent());
        Files.write(path, "123".getBytes());
        String uuid = InstanceUUID.get();
        assertEquals("123", uuid);
    public void ignoreEmptyFile() throws IOException {
        Files.write(path, "".getBytes());
        assertNotEquals("", uuid);
