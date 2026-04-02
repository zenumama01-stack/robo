 * A {@link User} sourced from a managed {@link UserProvider}.
public class ManagedUser implements AuthenticatedUser {
    private String passwordHash;
    private String passwordSalt;
    private Set<String> roles = new HashSet<>();
    private @Nullable PendingToken pendingToken = null;
    private List<UserSession> sessions = new ArrayList<>();
    private List<UserApiToken> apiTokens = new ArrayList<>();
     * Constructs a user with a password hash and salt provided by the caller.
     * @param passwordSalt the salt to compute the password hash
     * @param passwordHash the result of the hashing of the salted password
    public ManagedUser(String name, String passwordSalt, String passwordHash) {
        this.passwordSalt = passwordSalt;
        this.passwordHash = passwordHash;
     * Gets the password hash.
     * @return the password hash
    public String getPasswordHash() {
        return passwordHash;
     * Alters the password salt.
     * @param passwordSalt the new password salt
    public void setPasswordSalt(String passwordSalt) {
     * Alters the password hash.
     * @param passwordHash the new password hash
    public void setPasswordHash(String passwordHash) {
     * Gets the password salt.
     * @return the password salt
    public String getPasswordSalt() {
        return passwordSalt;
     * Alters the user's account name
     * @param name the new account name
     * Alters the user's set of roles.
     * @param roles the new roles
    public void setRoles(Set<String> roles) {
    public @Nullable PendingToken getPendingToken() {
        return pendingToken;
    public void setPendingToken(@Nullable PendingToken pendingToken) {
        this.pendingToken = pendingToken;
    public List<UserSession> getSessions() {
    public void setSessions(List<UserSession> sessions) {
        this.sessions = sessions;
    public List<UserApiToken> getApiTokens() {
        return apiTokens;
    public void setApiTokens(List<UserApiToken> apiTokens) {
        this.apiTokens = apiTokens;
        return name + " (" + String.join(", ", roles.stream().toArray(String[]::new)) + ")";
