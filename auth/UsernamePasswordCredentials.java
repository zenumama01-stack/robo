 * Credentials which represent user name and password.
 * @author Kai Kreuzer - Added JavaDoc
public class UsernamePasswordCredentials implements Credentials {
    private final String username;
    private final String password;
     * @param username name of the user
     * @param password password of the user
    public UsernamePasswordCredentials(String username, String password) {
     * Retrieves the user name
     * @return the username
     * Retrieves the password
     * @return the password
    public String getPassword() {
        return username + ":" + password.replaceAll(".", "*");
