 * Tests for the ExecUtil
public class ExecUtilTest {
    private static final Duration TIMEOUT = Duration.ofSeconds(10);
    public void testBasicExecuteCommandLine() {
        if (isWindowsSystem()) {
            ExecUtil.executeCommandLine("cmd", "/c", "dir");
            ExecUtil.executeCommandLine("ls");
    public void testBasicExecuteCommandLineAndWaitResponse() {
            result = ExecUtil.executeCommandLineAndWaitResponse(TIMEOUT, "cmd", "/c", "dir");
            result = ExecUtil.executeCommandLineAndWaitResponse(TIMEOUT, "ls");
        assertNotNull(result);
        assertNotEquals("", result);
    public void testExecuteCommandLineAndWaitResponseWithArguments() {
            result = ExecUtil.executeCommandLineAndWaitResponse(TIMEOUT, "cmd", "/c", "echo", "test");
            result = ExecUtil.executeCommandLineAndWaitResponse(TIMEOUT, "echo", "'test'");
        assertNotEquals("test", result);
    private boolean isWindowsSystem() {
        String osName = System.getProperty("os.name").toLowerCase();
        return osName.contains("windows");
    public void testExecuteCommandLineAndWaitStdErrRedirection() {
            result = ExecUtil.executeCommandLineAndWaitResponse(TIMEOUT, "cmd", "/c", "dir", "xxx.xxx", "1>", "nul");
            result = ExecUtil.executeCommandLineAndWaitResponse(TIMEOUT, "ls", "xxx.xxx");
