 * A DTO representing an add-on service.
public class AddonServiceDTO {
    String id;
    Set<AddonType> addonTypes;
    public AddonServiceDTO(String id, String name, Set<AddonType> addonTypes) {
        this.addonTypes = addonTypes;
