 * This interface describes a provider for a location.
public interface LocationProvider {
     * Provides access to the location of the installation
     * @return location of the current installation or null if the location is not set
    PointType getLocation();
