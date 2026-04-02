 * A DTO representing a {@link User}.
@Schema(name = "User")
public class UserDTO {
    public Collection<String> roles;
    public UserDTO(User user) {
        this.name = user.getName();
        this.roles = user.getRoles();
