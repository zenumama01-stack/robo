package org.openhab.core.io.net.exec;
import java.io.StringWriter;
import java.lang.ProcessBuilder.Redirect;
 * Some common methods to execute commands on command line.
 * @author Kai Kreuzer - added exception logging
 * @author Connor Petty - replaced delimiter usage with argument array
public class ExecUtil {
    private static Logger logger = LoggerFactory.getLogger(ExecUtil.class);
    private static ExecutorService executor = ThreadPoolManager.getPool("ExecUtil");
     * Executes <code>commandLine</code>.
     * A possible {@link IOException} gets logged but no further processing is done.
     * @param commandLine the command line to execute
    public static void executeCommandLine(String... commandLine) {
            new ProcessBuilder(commandLine).redirectError(Redirect.DISCARD).redirectOutput(Redirect.DISCARD).start();
            logger.warn("Error occurred when executing commandLine '{}'", commandLine, e);
     * Executes <code>commandLine</code> and return its result.
     * @param timeout the max time to wait for a process to finish, null to wait indefinitely
     * @return response data from executed command line or <code>null</code> if a timeout or error occurred
    public static @Nullable String executeCommandLineAndWaitResponse(@Nullable Duration timeout,
            String... commandLine) {
        Process processTemp = null;
        Future<String> outputFuture = null;
        cleanup: try {
            Process process = processTemp = new ProcessBuilder(commandLine).redirectErrorStream(true).start();
            outputFuture = executor.submit(() -> {
                try (InputStream inputStream = process.getInputStream();
                        BufferedReader reader = new BufferedReader(new InputStreamReader(inputStream))) {
                    StringWriter output = new StringWriter();
                    reader.transferTo(output);
                    return output.toString();
            if (timeout == null) {
                process.waitFor();
            } else if (!process.waitFor(timeout.toMillis(), TimeUnit.MILLISECONDS)) {
                logger.warn("Timeout occurred when executing commandLine '{}'", Arrays.toString(commandLine));
                break cleanup;
            return outputFuture.get();
        } catch (ExecutionException e) {
                logger.warn("Error occurred when executing commandLine '{}'", Arrays.toString(commandLine),
                        e.getCause());
                logger.warn("Error occurred when executing commandLine '{}'", Arrays.toString(commandLine));
            logger.debug("commandLine '{}' was interrupted", Arrays.toString(commandLine), e);
                logger.warn("Failed to execute commandLine '{}'", Arrays.toString(commandLine), e);
                logger.warn("Failed to execute commandLine '{}'", Arrays.toString(commandLine));
        if (processTemp != null && processTemp.isAlive()) {
            processTemp.destroyForcibly();
        if (outputFuture != null) {
            outputFuture.cancel(true);
