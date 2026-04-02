 * DTO for serialization of a suggested addon discovery method.
 * @author Andrew Fiddian-Green - Initial contribution
public class AddonDiscoveryMethod {
    private @NonNullByDefault({}) String serviceType;
    private @Nullable List<AddonParameter> parameters;
    private @Nullable List<AddonMatchProperty> matchProperties;
    public String getServiceType() {
        return serviceType.toLowerCase();
    public List<AddonParameter> getParameters() {
        List<AddonParameter> parameters = this.parameters;
        return parameters != null ? parameters : List.of();
    public List<AddonMatchProperty> getMatchProperties() {
        List<AddonMatchProperty> matchProperties = this.matchProperties;
        return matchProperties != null ? matchProperties : List.of();
    public AddonDiscoveryMethod setServiceType(String serviceType) {
        this.serviceType = serviceType.toLowerCase();
    public AddonDiscoveryMethod setParameters(@Nullable List<AddonParameter> parameters) {
    public AddonDiscoveryMethod setMatchProperties(@Nullable List<AddonMatchProperty> matchProperties) {
        this.matchProperties = matchProperties;
        return Objects.hash(serviceType, parameters, matchProperties);
    public boolean equals(@Nullable Object obj) {
        if (this == obj) {
        if (obj == null) {
        if (getClass() != obj.getClass()) {
        AddonDiscoveryMethod other = (AddonDiscoveryMethod) obj;
        return Objects.equals(serviceType, other.serviceType) && Objects.equals(parameters, other.parameters)
                && Objects.equals(matchProperties, other.matchProperties);
