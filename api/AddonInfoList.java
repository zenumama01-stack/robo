 * DTO containing a list of {@code AddonInfo}
public class AddonInfoList {
    protected @Nullable List<AddonInfo> addons;
    public List<AddonInfo> getAddons() {
        List<AddonInfo> addons = this.addons;
        return addons != null ? addons : List.of();
    public AddonInfoList setAddons(@Nullable List<AddonInfo> addons) {
        this.addons = addons;
