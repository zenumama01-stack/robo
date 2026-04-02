package org.openhab.core.addon.marketplace.test;
 * The {@link TestAddonHandler} is a
public class TestAddonHandler implements MarketplaceAddonHandler {
    private static final Set<String> SUPPORTED_ADDON_TYPES = Set.of("binding", "automation");
    public static final String TEST_ADDON_CONTENT_TYPE = "testAddonContentType";
    private final Set<String> installedAddons = new HashSet<>();
    private boolean isReady = true;
    public void setReady(boolean ready) {
        isReady = ready;
        return SUPPORTED_ADDON_TYPES.contains(type) && TEST_ADDON_CONTENT_TYPE.equals(contentType);
        if (!isReady) {
            // this is to catch illegal calls to the service in tests
            throw new IllegalStateException();
        return installedAddons.contains(id);
        if (addon.getUid().endsWith(":" + INSTALL_EXCEPTION_ADDON)) {
            throw new MarketplaceHandlerException("Installation failed", null);
        installedAddons.add(addon.getUid());
        if (addon.getUid().endsWith(":" + UNINSTALL_EXCEPTION_ADDON)) {
            throw new MarketplaceHandlerException("Uninstallation failed", null);
        installedAddons.remove(addon.getUid());
