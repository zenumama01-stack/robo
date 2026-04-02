 * This is a simple data container to keep all details of a service description together.
public class ServiceDescription {
    public String serviceType;
    public String serviceName;
    public int servicePort;
    public Hashtable<String, String> serviceProperties;
     * Constructor for a {@link ServiceDescription}, which takes all details as parameters
     * @param serviceType String service type, like "_openhab-server._tcp.local."
     * @param serviceName String service name, like "openHAB"
     * @param servicePort Int service port, like 8080
     * @param serviceProperties Hashtable service props, like url = "/rest"
    public ServiceDescription(String serviceType, String serviceName, int servicePort,
            Hashtable<String, String> serviceProperties) {
        this.serviceType = serviceType;
        this.serviceName = serviceName;
        this.servicePort = servicePort;
        this.serviceProperties = serviceProperties;
        result = prime * result + ((serviceName == null) ? 0 : serviceName.hashCode());
        result = prime * result + servicePort;
        result = prime * result + ((serviceType == null) ? 0 : serviceType.hashCode());
        ServiceDescription other = (ServiceDescription) obj;
        if (serviceName == null) {
            if (other.serviceName != null) {
        } else if (!serviceName.equals(other.serviceName)) {
        if (servicePort != other.servicePort) {
        if (serviceType == null) {
            if (other.serviceType != null) {
        } else if (!serviceType.equals(other.serviceType)) {
        return "ServiceDescription [serviceType=" + serviceType + ", serviceName=" + serviceName + ", servicePort="
                + servicePort + "]";
