 * {@link SitemapProvider} should be implemented by any service that provides {@link Sitemap}s to a
 * {@link SitemapRegistry}.
public interface SitemapProvider extends Provider<Sitemap> {
     * Get a sitemap from the provider.
     * @return sitemap
    Sitemap getSitemap(String sitemapName);
     * Get the names of all sitemaps available from the provider
     * @return sitemap names
    Set<String> getSitemapNames();
