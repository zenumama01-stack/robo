package org.openhab.core.test.internal.java;
import static java.util.stream.Collectors.*;
import org.osgi.service.component.runtime.ServiceComponentRuntime;
import org.osgi.service.component.runtime.dto.ComponentConfigurationDTO;
import org.osgi.service.component.runtime.dto.ComponentDescriptionDTO;
import org.osgi.service.component.runtime.dto.ReferenceDTO;
 * Utility class to analyze and print possible reasons for a service being not present.
public class MissingServiceAnalyzer {
    private final @Nullable PrintStream ps;
    public MissingServiceAnalyzer(@Nullable PrintStream ps, BundleContext bundleContext) {
        this.ps = ps;
    public <T> void printMissingServiceDetails(Class<T> clazz) {
        ServiceReference<ServiceComponentRuntime> scrReference = bundleContext
                .getServiceReference(ServiceComponentRuntime.class);
        if (scrReference != null) {
            ServiceComponentRuntime scr = bundleContext.getService(scrReference);
            if (scr != null) {
                ps.println("Components implementing " + clazz.getName() + ":");
                printUnsatisfiedServices(scr, clazz.getName(), "");
            ps.println("SCR is not started! Add the SCR bundle to your launch config.");
    private <T> void printUnsatisfiedServices(ServiceComponentRuntime scr, String interfaceName, String prefix) {
        Bundle[] allBundlesArrays = getAllBundles();
        List<ComponentDescriptionDTO> descriptions = getComponentDescriptions(scr, interfaceName, allBundlesArrays);
        if (descriptions.isEmpty()) {
            ps.println(prefix + "No component implementing " + interfaceName + " is currently registered.");
            ps.println(
                    "Make sure to add the appropriate bundle and set 'Default Auto-Start=true' in the launch config.");
            for (ComponentDescriptionDTO description : descriptions) {
                Collection<ComponentConfigurationDTO> configurations = scr.getComponentConfigurationDTOs(description);
                for (ComponentConfigurationDTO configuration : configurations) {
                    ps.println(prefix + configuration.id + " [" + getState(configuration.state) + "] "
                            + description.implementationClass + " in " + description.bundle.symbolicName);
                    for (ReferenceDTO ref : getUnsatisfiedReferences(description, configuration)) {
                        ps.println(prefix + "\t" + ref.name + " (" + ref.interfaceName + ")");
                        printUnsatisfiedServices(scr, ref.interfaceName, prefix + "\t\t");
    private List<ReferenceDTO> getUnsatisfiedReferences(ComponentDescriptionDTO description,
            ComponentConfigurationDTO configuration) {
        Set<String> unsatisfiedRefNames = Stream.of(configuration.unsatisfiedReferences)//
                .map(ref -> ref.name) //
        return Stream.of(description.references) //
                .filter(ref -> unsatisfiedRefNames.contains(ref.name)) //
    private List<ComponentDescriptionDTO> getComponentDescriptions(ServiceComponentRuntime scr, String interfaceName,
            Bundle[] allBundlesArrays) {
        return scr.getComponentDescriptionDTOs(allBundlesArrays).stream()
                .filter(description -> Stream.of(description.serviceInterfaces).anyMatch(s -> s.equals(interfaceName)))
    private Bundle[] getAllBundles() {
        List<Bundle> allBundles = Arrays.stream(bundleContext.getBundles())
                .filter(b -> b.getHeaders().get(Constants.FRAGMENT_HOST) == null).toList();
        return allBundles.toArray(new Bundle[allBundles.size()]);
    private String getState(int state) {
        return switch (state) {
            case ComponentConfigurationDTO.UNSATISFIED_CONFIGURATION -> "UNSATISFIED_CONFIGURATION";
            case ComponentConfigurationDTO.UNSATISFIED_REFERENCE -> "UNSATISFIED_REFERENCE";
            case ComponentConfigurationDTO.SATISFIED -> "SATISFIED";
            case ComponentConfigurationDTO.ACTIVE -> "ACTIVE";
            default -> state + "";
