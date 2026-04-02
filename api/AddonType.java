 * This class defines an add-on type.
public class AddonType {
    public static final AddonType AUTOMATION = new AddonType("automation", "Automation");
    public static final AddonType BINDING = new AddonType("binding", "Bindings");
    public static final AddonType MISC = new AddonType("misc", "Misc");
    public static final AddonType PERSISTENCE = new AddonType("persistence", "Persistence");
    public static final AddonType TRANSFORMATION = new AddonType("transformation", "Transformations");
    public static final AddonType UI = new AddonType("ui", "User Interfaces");
    public static final AddonType VOICE = new AddonType("voice", "Voice");
    public static final List<AddonType> DEFAULT_TYPES = List.of(AUTOMATION, BINDING, MISC, PERSISTENCE, TRANSFORMATION,
            UI, VOICE);
     * Creates a new type instance with the given id and label
     * @param label
    public AddonType(String id, String label) {
     * The id of the type
     * The label of the type to be used for headers (likely to be plural form)
        final int prime = 31;
        int result = 1;
        result = prime * result + id.hashCode();
        if ((obj == null) || (getClass() != obj.getClass())) {
        return id.equals(((AddonType) obj).id);
