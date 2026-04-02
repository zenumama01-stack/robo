package org.openhab.core.config.discovery.addon;
 * This is a {@link AddonFinder} interface for classes that find add-ons that are suggested to be installed.
public interface AddonFinder {
     * The framework calls this method to scan through the candidate list of {@link AddonInfo} and return a subset of
     * those that it suggests to be installed.
    Set<AddonInfo> getSuggestedAddons();
     * The framework calls this method to provide a list of {@link AddonInfo} elements which contain potential
     * candidates that this finder can iterate over in order to detect which ones to return via the
     * {@code getSuggestedAddons()} method.
     * @param candidates a list of AddonInfo candidates.
    void setAddonCandidates(List<AddonInfo> candidates);
     * This method should be called from the framework to allow a finder to stop searching for add-ons and do cleanup.
    void unsetAddonCandidates();
