import org.openhab.core.auth.ManagedUser;
import org.openhab.core.auth.User;
import org.openhab.core.auth.UserApiToken;
 * Console command extension to manage users, sessions and API tokens
public class UserConsoleCommandExtension extends AbstractConsoleCommandExtension {
    private static final String SUBCMD_CHANGEPASSWORD = "changePassword";
    private static final String SUBCMD_LISTAPITOKENS = "listApiTokens";
    private static final String SUBCMD_ADDAPITOKEN = "addApiToken";
    private static final String SUBCMD_RMAPITOKEN = "rmApiToken";
    private static final String SUBCMD_CLEARSESSIONS = "clearSessions";
    private final UserRegistry userRegistry;
    public UserConsoleCommandExtension(final @Reference UserRegistry userRegistry) {
        super("users", "Access the user registry.");
        this.userRegistry = userRegistry;
        return List.of(buildCommandUsage(SUBCMD_LIST, "lists all users"),
                buildCommandUsage(SUBCMD_ADD + " <userId> <password> <role>",
                        "adds a new user with the specified role"),
                buildCommandUsage(SUBCMD_REMOVE + " <userId>", "removes the given user"),
                buildCommandUsage(SUBCMD_CHANGEPASSWORD + " <userId> <newPassword>", "changes the password of a user"),
                buildCommandUsage(SUBCMD_LISTAPITOKENS, "lists the API tokens for all users"),
                buildCommandUsage(SUBCMD_ADDAPITOKEN + " <userId> <tokenName> <scope>",
                        "adds a new API token on behalf of the specified user for the specified scope"),
                buildCommandUsage(SUBCMD_RMAPITOKEN + " <userId> <tokenName>",
                        "removes (revokes) the specified API token"),
                buildCommandUsage(SUBCMD_CLEARSESSIONS + " <userId>",
                        "clear the refresh tokens associated with the user (will sign the user out of all sessions)"));
                    userRegistry.getAll().forEach(user -> console.println(user.toString()));
                    if (args.length == 4) {
                        User existingUser = userRegistry.get(args[1]);
                        if (existingUser == null) {
                            User newUser = userRegistry.register(args[1], args[2], Set.of(args[3]));
                            console.println(newUser.toString());
                            console.println("User created.");
                            console.println("The user already exists.");
                        console.printUsage(findUsage(SUBCMD_ADD));
                        User user = userRegistry.get(args[1]);
                        if (user != null) {
                            userRegistry.remove(user.getName());
                            console.println("User removed.");
                            console.println("User not found.");
                        console.printUsage(findUsage(SUBCMD_REMOVE));
                case SUBCMD_CHANGEPASSWORD:
                    if (args.length == 3) {
                            userRegistry.changePassword(user, args[2]);
                            console.println("Password changed.");
                        console.printUsage(findUsage(SUBCMD_CHANGEPASSWORD));
                case SUBCMD_LISTAPITOKENS:
                    userRegistry.getAll().forEach(user -> {
                        ManagedUser managedUser = (ManagedUser) user;
                        if (!managedUser.getApiTokens().isEmpty()) {
                            managedUser.getApiTokens()
                                    .forEach(t -> console.println("user=" + user.toString() + ", " + t.toString()));
                case SUBCMD_ADDAPITOKEN:
                        ManagedUser user = (ManagedUser) userRegistry.get(args[1]);
                            Optional<UserApiToken> userApiToken = user.getApiTokens().stream()
                                    .filter(t -> args[2].equals(t.getName())).findAny();
                            if (userApiToken.isEmpty()) {
                                String tokenString = userRegistry.addUserApiToken(user, args[2], args[3]);
                                console.println(tokenString);
                                console.println("Cannot create API token: another one with the same name was found.");
                        console.printUsage(findUsage(SUBCMD_ADDAPITOKEN));
                case SUBCMD_RMAPITOKEN:
                            if (userApiToken.isPresent()) {
                                userRegistry.removeUserApiToken(user, userApiToken.get());
                                console.println("API token revoked.");
                                console.println("No matching API token found.");
                        console.printUsage(findUsage(SUBCMD_RMAPITOKEN));
                case SUBCMD_CLEARSESSIONS:
                            userRegistry.clearSessions(user);
                            console.println("User sessions cleared.");
                        console.printUsage(findUsage(SUBCMD_CLEARSESSIONS));
    private String findUsage(String cmd) {
        return getUsages().stream().filter(u -> u.contains(cmd)).findAny().get();
