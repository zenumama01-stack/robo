public class HostFragmentMappingUtil {
    private static Map<Bundle, List<Bundle>> hostFragmentMapping = new HashMap<>();
    static PackageAdmin pkgAdmin;
    static Set<Entry<Bundle, List<Bundle>>> getMapping() {
        return hostFragmentMapping.entrySet();
     * This method is used to get the host bundles of the parameter which is a fragment bundle.
     * @param fragment an OSGi fragment bundle.
     * @return a list with the hosts of the <code>fragment</code> parameter.
    static List<Bundle> returnHostBundles(Bundle fragment) {
        List<Bundle> hosts = new ArrayList<>();
        Bundle[] bundles = pkgAdmin.getHosts(fragment);
        if (bundles != null) {
            hosts = Arrays.asList(bundles);
            for (Entry<Bundle, List<Bundle>> entry : hostFragmentMapping.entrySet()) {
                if (entry.getValue().contains(fragment)) {
                    hosts.add(entry.getKey());
        return hosts;
    static List<Bundle> fillHostFragmentMapping(Bundle host) {
        List<Bundle> fragments = new ArrayList<>();
        Bundle[] bundles = pkgAdmin.getFragments(host);
            fragments = Arrays.asList(bundles);
        synchronized (hostFragmentMapping) {
            hostFragmentMapping.put(host, fragments);
        return fragments;
    static void fillHostFragmentMapping(List<Bundle> hosts) {
        for (Bundle host : hosts) {
            fillHostFragmentMapping(host);
    static boolean needToProcessFragment(Bundle fragment, List<Bundle> hosts) {
        if (hosts.isEmpty()) {
                List<Bundle> fragments = hostFragmentMapping.get(host);
                if (fragments != null && fragments.contains(fragment)) {
    static boolean isFragmentBundle(Bundle bundle) {
        PackageAdmin pkgAdmin = HostFragmentMappingUtil.pkgAdmin;
        if (pkgAdmin == null) {
        return pkgAdmin.getBundleType(bundle) == PackageAdmin.BUNDLE_TYPE_FRAGMENT;
