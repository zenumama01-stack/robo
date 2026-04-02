 * A wrapper that uninstalls a bundle on close.
 * This wrapper allows the usage in try-with-resources blocks.
public class BundleCloseable implements AutoCloseable {
    public BundleCloseable(final Bundle bundle) {
    public void close() throws BundleException {
    public Bundle bundle() {
