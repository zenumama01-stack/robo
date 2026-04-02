import org.openhab.core.io.net.exec.ExecUtil;
 * This class provides static methods that can be used in automation rules for
 * executing commands on command line.
 * @author Pauli Anttila
public class Exec {
     * @param commandLine
     *            the command line to execute
        ExecUtil.executeCommandLine(commandLine);
     * @param timeout
     *            timeout for execution, if null will wait indefinitely
     * @return response data from executed command line
    public static String executeCommandLine(Duration timeout, String... commandLine) {
        return ExecUtil.executeCommandLineAndWaitResponse(timeout, commandLine);
