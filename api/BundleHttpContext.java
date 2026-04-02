package org.openhab.core.io.http.internal;
 * A bundle specific http context - delegates security and mime type handling to "parent" context.
class BundleHttpContext extends DelegatingHttpContext {
    BundleHttpContext(HttpContext delegate, Bundle bundle) {
        super(delegate);
    public URL getResource(String name) {
        if (name != null) {
            String resourceName;
            if (name.startsWith("/")) {
                resourceName = name.substring(1);
                resourceName = name;
            return bundle.getResource(resourceName);
