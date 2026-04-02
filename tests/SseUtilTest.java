public class SseUtilTest {
    public void testValidInvalidFilters() {
        // invalid
        assertThat(SseUtil.isValidTopicFilter("openhab/.*"), is(false));
        assertThat(SseUtil.isValidTopicFilter("openhab/\\w*/"), is(false));
        assertThat(SseUtil.isValidTopicFilter("sm.*/test/"), is(false));
        assertThat(SseUtil.isValidTopicFilter("openhab.*"), is(false));
        assertThat(SseUtil.isValidTopicFilter("openhab"), is(true));
        assertThat(SseUtil.isValidTopicFilter(""), is(true));
        assertThat(SseUtil.isValidTopicFilter(", openhab/*"), is(true));
        assertThat(SseUtil.isValidTopicFilter("openhab,qivicon"), is(true));
        assertThat(SseUtil.isValidTopicFilter("openhab , qivicon"), is(true));
        assertThat(SseUtil.isValidTopicFilter("openhab,    qivicon"), is(true));
        assertThat(SseUtil.isValidTopicFilter("openhab/test"), is(true));
        assertThat(SseUtil.isValidTopicFilter("openhab/test/test/test/test/test"), is(true));
        assertThat(SseUtil.isValidTopicFilter("openhab/test/test/test/test/test,    openhab/test/test/test/test/test"),
                SseUtil.isValidTopicFilter(
                        "openhab/test/test/test/test/test,    openhab/test/test/test/test/test, openhab,qivicon"),
        assertThat(SseUtil.isValidTopicFilter("////////////"), is(true));
        assertThat(SseUtil.isValidTopicFilter("*/added"), is(true));
        assertThat(SseUtil.isValidTopicFilter("*added"), is(true));
        assertThat(SseUtil.isValidTopicFilter("openhab/test/test:test:123/test"), is(true));
        assertThat(SseUtil.isValidTopicFilter("openhab/test/test-test-123-test:test:123/test"), is(true));
    public void testFilterMatchers() {
        List<String> regexes = SseUtil
                .convertToRegex("openhab/*/test/test/test/test,    openhab/test/*/test/test/test, openhab/*,qivicon/*");
        assertThat("openhab/test/test/test/test/test".matches(regexes.getFirst()), is(true));
        assertThat("openhab/asdf/test/test/test/test".matches(regexes.getFirst()), is(true));
        assertThat("openhab/asdf/ASDF/test/test/test".matches(regexes.getFirst()), is(false));
        assertThat("openhab/test/test/test/test/test".matches(regexes.get(1)), is(true));
        assertThat("openhab/asdf/test/test/test/test".matches(regexes.get(1)), is(false));
        assertThat("openhab/asdf/ASDF/test/test/test".matches(regexes.get(1)), is(false));
        assertThat("openhab/test/test/test/test/test".matches(regexes.get(2)), is(true));
        assertThat("openhab/asdf/test/test/test/test".matches(regexes.get(2)), is(true));
        assertThat("openhab/asdf/ASDF/test/test/test".matches(regexes.get(2)), is(true));
        assertThat("openhab/test/test/test/test/test".matches(regexes.get(3)), is(false));
        assertThat("openhab/asdf/test/test/test/test".matches(regexes.get(3)), is(false));
        assertThat("qivicon/asdf/ASDF/test/test/test".matches(regexes.get(3)), is(true));
    public void testMoreFilterMatchers() {
        List<String> regexes = SseUtil.convertToRegex(",    *, openhab/items/*/added, openhab/items/*/*");
        assertThat("openhab/asdf/ASDF/test/test/test".matches(regexes.getFirst()), is(true));
        assertThat("openhab/test/test/test/test/test".matches(regexes.get(1)), is(false));
        assertThat("openhab/items/anyitem/added".matches(regexes.get(1)), is(true));
        assertThat("openhab/items/anyitem/removed".matches(regexes.get(1)), is(false));
        assertThat("openhab/items/anyitem/added".matches(regexes.get(2)), is(true));
        assertThat("openhab/items/anyitem/removed".matches(regexes.get(2)), is(true));
        assertThat("openhab/items/anyitem/updated".matches(regexes.get(2)), is(true));
        assertThat("openhab/things/anything/updated".matches(regexes.get(2)), is(false));
    public void testEvenMoreFilterMatchers() {
        List<String> regexes = SseUtil.convertToRegex("");
        regexes = SseUtil.convertToRegex("*/added");
        assertThat("openhab/items/anyitem/added".matches(regexes.getFirst()), is(true));
        assertThat("openhab/items/anyitem/removed".matches(regexes.getFirst()), is(false));
        regexes = SseUtil.convertToRegex("*added");
        regexes = SseUtil.convertToRegex("openhab/items/*/state");
        assertThat("openhab/items/anyitem/state".matches(regexes.getFirst()), is(true));
        assertThat("openhab/items/anyitem/statechanged".matches(regexes.getFirst()), is(false));
