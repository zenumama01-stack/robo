 * The {@link SitemapRegistry} is the central place to store sitemaps.
 * Sitemaps are registered through {@link SitemapProvider}.
public interface SitemapRegistry extends Registry<Sitemap, String> {
     * Add a sitemap provider to the registry.
    public void addSitemapProvider(Provider<Sitemap> provider);
     * Remove a sitemap provider from the registry.
    public void removeSitemapProvider(Provider<Sitemap> provider);
