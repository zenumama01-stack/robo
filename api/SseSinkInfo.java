 * The specific information we need to hold for a SSE sink.
public class SseSinkInfo {
    public final String subscriptionId;
    public final SitemapSubscriptionService subscriptions;
    public SseSinkInfo(String subscriptionId, SitemapSubscriptionService subscriptions) {
        this.subscriptionId = subscriptionId;
