 * Represents a generic {@link User} with a set of roles
public class GenericUser implements User {
    protected String name;
    protected Set<String> roles;
     * Constructs a user attributed with a set of roles.
     * @param name the username (account name)
     * @param roles the roles attributed to this user
    public GenericUser(String name, Set<String> roles) {
     * Constructs a user with no roles.
    public GenericUser(String name) {
        this(name, new HashSet<>());
        return "User (name=" + name + ", roles=" + String.join(", ", roles) + ")";
