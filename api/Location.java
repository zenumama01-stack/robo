 * This is the super interface for all types that represent a Location.
public interface Location extends Tag {
    static String name() {
        return "Location";
    Location isPartOf();
