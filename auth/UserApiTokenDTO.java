 * A DTO representing a user API token, without the sensible information.
@Schema(name = "UserApiToken")
public class UserApiTokenDTO {
    public Date createdTime;
    public UserApiTokenDTO(String name, Date createdTime, String scope) {
        this.createdTime = createdTime;
