 * The {@link NodeName} interface defines common features for all {@code Node}* classes.
 * Each {@code Node}* class has to return its node name.
public interface NodeName {
     * Returns the name of the node this object belongs to.
     * @return the name of the node this object belongs to (not empty)
    String getNodeName();
