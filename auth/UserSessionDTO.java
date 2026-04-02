 * A DTO representing a user session, without the sensible information.
@Schema(name = "UserSession")
public class UserSessionDTO {
    public String sessionId;
    public Date lastRefreshTime;
    public String clientId;
    public UserSessionDTO(String sessionId, Date createdTime, Date lastRefreshTime, String clientId, String scope) {
        this.sessionId = sessionId;
        this.lastRefreshTime = lastRefreshTime;
