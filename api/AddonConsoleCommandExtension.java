package org.openhab.core.io.console.internal.extension;
 * Console command extension to manage add-ons
public class AddonConsoleCommandExtension extends AbstractConsoleCommandExtension {
    private static final String SUBCMD_SERVICES = "services";
    private static final String SUBCMD_INSTALL = "install";
    private static final String SUBCMD_UNINSTALL = "uninstall";
            List.of(SUBCMD_LIST, SUBCMD_SERVICES, SUBCMD_INSTALL, SUBCMD_UNINSTALL), false);
    private class AddonConsoleCommandCompleter implements ConsoleCommandCompleter {
    private final Map<String, AddonService> addonServices = new ConcurrentHashMap<>();
    public AddonConsoleCommandExtension() {
        super("addons", "Manage add-ons.");
    public void bindAddonService(AddonService addonService) {
        addonServices.put(addonService.getId(), addonService);
    public void unbindAddonService(AddonService addonService) {
        addonServices.remove(addonService.getId());
        return List.of(buildCommandUsage(SUBCMD_SERVICES, "list all available add-on services"),
                buildCommandUsage(SUBCMD_LIST + " [<serviceId>]",
                        "lists names of all add-ons (from the named service, if given)"),
                buildCommandUsage(SUBCMD_INSTALL + " <addonUid>", "installs the given add-on"),
                buildCommandUsage(SUBCMD_UNINSTALL + " <addonUid>", "uninstalls the given add-on"));
                case SUBCMD_SERVICES:
                    listServices(console);
                    listAddons(console, (args.length < 2) ? "" : args[1]);
                case SUBCMD_INSTALL:
                    if (args.length == 2) {
                        installAddon(console, args[1]);
                        console.println("Specify the UID of the add-on to install: " + getCommand() + " "
                                + SUBCMD_INSTALL + " <addonUid>");
                case SUBCMD_UNINSTALL:
                        uninstallAddon(console, args[1]);
                        console.println("Specify the UID of the add-on to uninstall: " + getCommand() + " "
                                + SUBCMD_UNINSTALL + " <addonUid>");
                    console.println("Unknown command '" + subCommand + "'");
        return new AddonConsoleCommandCompleter();
    private void listServices(Console console) {
        addonServices.values().forEach(s -> console.println(String.format("%-20s %s", s.getId(), s.getName())));
    private void listAddons(Console console, String serviceId) {
        List<Addon> addons;
        if (serviceId.isBlank()) {
            addons = addonServices.values().stream().map(s -> s.getAddons(null)).flatMap(List::stream).toList();
            AddonService service = addonServices.get(serviceId);
            if (service == null) {
                console.println("Add-on service '" + serviceId + "' is not known.");
            addons = service.getAddons(null);
        addons.forEach(addon -> console.println(String.format("%s %-45s %-20s %s", addon.isInstalled() ? "i" : " ",
                addon.getUid(), addon.getVersion().isBlank() ? "not set" : addon.getVersion(), addon.getLabel())));
    private void installAddon(Console console, String addonUid) {
        String[] parts = addonUid.split(":");
        String serviceId = parts.length == 2 ? parts[0] : "karaf";
        String addonId = parts.length == 2 ? parts[1] : parts[0];
            console.println("Could not find requested add-on service. Add-on " + addonUid + " not installed.");
            Addon addon = service.getAddon(addonId, null);
                console.println("Could not find add-on in add-on service. Add-on " + addonUid + " not installed.");
            } else if (addon.isInstalled()) {
                console.println("Add-on " + addonUid + " is already installed.");
                service.install(addonId);
                console.println("Installed " + addonUid + ".");
    private void uninstallAddon(Console console, String addonUid) {
            console.println("Could not find requested add-on service. Add-on " + addonUid + " not uninstalled.");
                console.println("Could not find add-on in add-on service. Add-on " + addonUid + " not uninstalled.");
            } else if (!addon.isInstalled()) {
                console.println("Add-on " + addonUid + " is not installed.");
                service.uninstall(addonId);
                console.println("Uninstalled " + addonUid + ".");
