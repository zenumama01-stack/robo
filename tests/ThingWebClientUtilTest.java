public class ThingWebClientUtilTest {
    private ThingUID uid1 = new ThingUID("mycroft", "mycroft", "yy");
    private ThingUID uid2 = new ThingUID("mycroft", "mycroft", "myInstance");
    private ThingUID uid3 = new ThingUID("mycroft", "mycroft", "myPersonalInstance");
    private ThingUID uid4 = new ThingUID("amazonechocontrol", "account", "myAccount");
    public void testBuildWebClientConsumerNameWhenThingUidSizeIsOk() {
        String name = ThingWebClientUtil.buildWebClientConsumerName(uid1, null);
        assertThat(name, is("mycroft-mycroft-yy"));
    public void testBuildWebClientConsumerNameWhenPrefixAndThingUidSizeIsOk() {
        String name = ThingWebClientUtil.buildWebClientConsumerName(uid1, "x-");
        assertThat(name, is("x-mycroft-mycroft-yy"));
    public void testBuildWebClientConsumerNameWhenPrefixIsTooBig() {
        String name = ThingWebClientUtil.buildWebClientConsumerName(uid1, "xxxx-");
        assertThat(name, is("xxxx-mycro-yy"));
    public void testBuildWebClientConsumerNameWhenThingUidIsTooBig() {
        String name = ThingWebClientUtil.buildWebClientConsumerName(uid2, null);
        assertThat(name, is("mycroft-myInstance"));
    public void testBuildWebClientConsumerNameWhenThingIdIsTooBig() {
        String name = ThingWebClientUtil.buildWebClientConsumerName(uid3, null);
        String hashCode = ThingWebClientUtil.buildHashCode(uid3, 12);
        assertThat(name, is("mycroft-" + hashCode));
    public void testBuildWebClientConsumerNameWhenBindingIdIsTooBig() {
        String name = ThingWebClientUtil.buildWebClientConsumerName(uid4, null);
        assertThat(name, is("amazonecho-myAccount"));
