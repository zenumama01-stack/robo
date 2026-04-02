 * This is a java bean that is used to define system information for the REST interface.
public class SystemInfoBean {
    public final SystemInfo systemInfo;
    public static class SystemInfo {
        public final String configFolder = OpenHAB.getConfigFolder();
        public final String userdataFolder = OpenHAB.getUserDataFolder();
        public final @Nullable String logFolder = System.getProperty("openhab.logdir");
        public final @Nullable String javaVersion = System.getProperty("java.version");
        public final @Nullable String javaVendor = System.getProperty("java.vendor");
        public final @Nullable String javaVendorVersion = System.getProperty("java.vendor.version");
        public final @Nullable String osName = System.getProperty("os.name");
        public final @Nullable String osVersion = System.getProperty("os.version");
        public final @Nullable String osArchitecture = System.getProperty("os.arch");
        public final int availableProcessors = Runtime.getRuntime().availableProcessors();
        public final long freeMemory = Runtime.getRuntime().freeMemory();
        public final long totalMemory = Runtime.getRuntime().totalMemory();
        public final long uptime = ManagementFactory.getRuntimeMXBean().getUptime() / 1000;
        public final int startLevel;
        public SystemInfo(int startLevel) {
    public SystemInfoBean(int startLevel) {
        systemInfo = new SystemInfo(startLevel);
