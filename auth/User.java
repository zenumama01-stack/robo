 * A user represents an individual, physical person using the system.
public interface User extends Principal, Identifiable<String> {
     * Gets the roles attributed to the user.
     * @see Role
     * @return role attributed to the user
    Set<String> getRoles();
