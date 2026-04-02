public class SystemFollowProfileTest {
        SystemFollowProfile profile = new SystemFollowProfile(callbackMock);
    public void testOnUpdate() {
        profile.onStateUpdateFromItem(OnOffType.ON);
