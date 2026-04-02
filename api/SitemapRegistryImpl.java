 * The {@link SitemapRegistryImpl} implements the {@link SitemapRegistry}
@Component(service = SitemapRegistry.class, immediate = true)
public class SitemapRegistryImpl extends AbstractRegistry<Sitemap, String, SitemapProvider> implements SitemapRegistry {
    public SitemapRegistryImpl() {
        super(null);
    public void addSitemapProvider(Provider<Sitemap> provider) {
        addProvider(provider);
    public void removeSitemapProvider(Provider<Sitemap> provider) {
        removeProvider(provider);
