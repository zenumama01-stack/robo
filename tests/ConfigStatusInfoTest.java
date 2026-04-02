 * Testing the {@link ConfigStatusInfo}.
public class ConfigStatusInfoTest {
    private static final String PARAM3 = "param3";
    private static final String PARAM4 = "param4";
    private static final String PARAM5 = "param5";
    private static final String PARAM6 = "param6";
    private static final String INFO1 = "info1";
    private static final String INFO2 = "info2";
    private static final String WARNING1 = "warning1";
    private static final String WARNING2 = "warning2";
    private static final String ERROR1 = "error1";
    private static final String ERROR2 = "error2";
    private static final ConfigStatusMessage MSG1 = ConfigStatusMessage.Builder.information(PARAM1)
            .withMessageKeySuffix(INFO1).build();
    private static final ConfigStatusMessage MSG2 = ConfigStatusMessage.Builder.information(PARAM2)
            .withMessageKeySuffix(INFO2).withStatusCode(1).build();
    private static final ConfigStatusMessage MSG3 = ConfigStatusMessage.Builder.warning(PARAM3)
            .withMessageKeySuffix(WARNING1).build();
    private static final ConfigStatusMessage MSG4 = ConfigStatusMessage.Builder.warning(PARAM4)
            .withMessageKeySuffix(WARNING2).withStatusCode(1).build();
    private static final ConfigStatusMessage MSG5 = ConfigStatusMessage.Builder.error(PARAM5)
            .withMessageKeySuffix(ERROR1).build();
    private static final ConfigStatusMessage MSG6 = ConfigStatusMessage.Builder.pending(PARAM6)
            .withMessageKeySuffix(ERROR2).withStatusCode(1).build();
    private static final List<ConfigStatusMessage> ALL = List.of(MSG1, MSG2, MSG3, MSG4, MSG5, MSG6);
    public void assertCorrectConfigErrorHandlingForEmptyResultObject() {
        assertThat(info.getConfigStatusMessages().size(), is(0));
    public void assertCorrectConfigStatusInfoHandlingUusingConstructor() {
        assertConfigStatusInfo(new ConfigStatusInfo(ALL));
    public void assertCorrectConfigErrorHandlingUsingAddConfigErrors() {
        info.add(ALL);
        assertConfigStatusInfo(info);
    public void assertCorrectConfigErrorHandlingUsingAddConfigError() {
        for (ConfigStatusMessage configStatusMessage : ALL) {
            info.add(configStatusMessage);
    private void assertConfigStatusInfo(ConfigStatusInfo info) {
        assertThat(info.getConfigStatusMessages().size(), is(ALL.size()));
        assertThat(info.getConfigStatusMessages(), hasItems(MSG1, MSG2, MSG3, MSG4, MSG5, MSG6));
        assertThat(info.getConfigStatusMessages(Type.INFORMATION).size(), is(2));
        assertThat(info.getConfigStatusMessages(Type.INFORMATION), hasItems(MSG1, MSG2));
        assertThat(info.getConfigStatusMessages(Type.WARNING).size(), is(2));
        assertThat(info.getConfigStatusMessages(Type.WARNING), hasItems(MSG3, MSG4));
        assertThat(info.getConfigStatusMessages(Type.ERROR).size(), is(1));
        assertThat(info.getConfigStatusMessages(Type.ERROR), hasItems(MSG5));
        assertThat(info.getConfigStatusMessages(Type.PENDING).size(), is(1));
        assertThat(info.getConfigStatusMessages(Type.PENDING), hasItems(MSG6));
        assertThat(info.getConfigStatusMessages(Type.INFORMATION, Type.WARNING).size(), is(4));
        assertThat(info.getConfigStatusMessages(Type.INFORMATION, Type.WARNING), hasItems(MSG1, MSG2, MSG3, MSG4));
        assertThat(info.getConfigStatusMessages(PARAM1).size(), is(1));
        assertThat(info.getConfigStatusMessages(PARAM1), hasItem(MSG1));
        assertThat(info.getConfigStatusMessages(PARAM2).size(), is(1));
        assertThat(info.getConfigStatusMessages(PARAM2), hasItem(MSG2));
        assertThat(info.getConfigStatusMessages(PARAM3, PARAM4).size(), is(2));
        assertThat(info.getConfigStatusMessages(PARAM3, PARAM4), hasItems(MSG3, MSG4));
        assertThat(info.getConfigStatusMessages(Set.of(Type.INFORMATION, Type.WARNING), Set.of(PARAM1, PARAM6)).size(),
                is(5));
        assertThat(info.getConfigStatusMessages(Set.of(Type.INFORMATION, Type.WARNING), Set.of(PARAM1, PARAM6)),
                hasItems(MSG1, MSG2, MSG3, MSG4, MSG6));
        assertThat(info.getConfigStatusMessages("unknown").size(), is(0));
