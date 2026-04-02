import javax.crypto.SecretKeyFactory;
import javax.crypto.spec.PBEKeySpec;
import org.openhab.core.auth.UserProvider;
 * The implementation of a {@link UserRegistry} for {@link ManagedUser} entities.
@Component(service = UserRegistry.class, immediate = true)
public class UserRegistryImpl extends AbstractRegistry<User, String, UserProvider> implements UserRegistry {
    private final Logger logger = LoggerFactory.getLogger(UserRegistryImpl.class);
    private static final int PASSWORD_ITERATIONS = 65536;
    private static final int APITOKEN_ITERATIONS = 1024;
    private static final String APITOKEN_PREFIX = "oh";
    private static final int KEY_LENGTH = 512;
    private static final String ALGORITHM = "PBKDF2WithHmacSHA512";
    private static final SecureRandom RAND = new SecureRandom();
    public UserRegistryImpl(BundleContext context, Map<String, Object> properties) {
        super(UserProvider.class);
        super.activate(context);
    protected void setManagedProvider(ManagedUserProvider managedProvider) {
    protected void unsetManagedProvider(ManagedUserProvider managedProvider) {
        super.removeProvider(managedProvider);
    public User register(String username, String password, Set<String> roles) {
        String passwordSalt = generateSalt(KEY_LENGTH / 8).get();
        String passwordHash = hash(password, passwordSalt, PASSWORD_ITERATIONS).get();
        ManagedUser user = new ManagedUser(username, passwordSalt, passwordHash);
        user.setRoles(new HashSet<>(roles));
        super.add(user);
    private Optional<String> generateSalt(final int length) {
        if (length < 1) {
            logger.error("error in generateSalt: length must be > 0");
        byte[] salt = new byte[length];
        RAND.nextBytes(salt);
        return Optional.of(Base64.getEncoder().encodeToString(salt));
    private Optional<String> hash(String password, String salt, int iterations) {
        char[] chars = password.toCharArray();
        byte[] bytes = salt.getBytes();
        PBEKeySpec spec = new PBEKeySpec(chars, bytes, iterations, KEY_LENGTH);
        Arrays.fill(chars, Character.MIN_VALUE);
            SecretKeyFactory fac = SecretKeyFactory.getInstance(ALGORITHM);
            byte[] securePassword = fac.generateSecret(spec).getEncoded();
            return Optional.of(Base64.getEncoder().encodeToString(securePassword));
        } catch (NoSuchAlgorithmException | InvalidKeySpecException e) {
            logger.error("Exception encountered while hashing", e);
            spec.clearPassword();
        if (credentials instanceof UsernamePasswordCredentials usernamePasswordCreds) {
            User user = get(usernamePasswordCreds.getUsername());
                throw new AuthenticationException("User not found: " + usernamePasswordCreds.getUsername());
            String hashedPassword = hash(usernamePasswordCreds.getPassword(), managedUser.getPasswordSalt(),
                    PASSWORD_ITERATIONS).get();
            if (!hashedPassword.equals(managedUser.getPasswordHash())) {
                throw new AuthenticationException("Wrong password for user " + usernamePasswordCreds.getUsername());
            return new Authentication(managedUser.getName(), managedUser.getRoles().stream().toArray(String[]::new));
        } else if (credentials instanceof UserApiTokenCredentials apiTokenCreds) {
            String[] apiTokenParts = apiTokenCreds.getApiToken().split("\\.");
            if (apiTokenParts.length != 3 || !APITOKEN_PREFIX.equals(apiTokenParts[0])) {
                throw new AuthenticationException("Invalid API token format");
            for (User user : getAll()) {
                    for (UserApiToken userApiToken : authenticatedUser.getApiTokens()) {
                        // only check if the name in the token matches
                        if (!userApiToken.getName().equals(apiTokenParts[1])) {
                        String[] existingTokenHashAndSalt = userApiToken.getApiToken().split(":");
                        String incomingTokenHash = hash(apiTokenCreds.getApiToken(), existingTokenHashAndSalt[1],
                                APITOKEN_ITERATIONS).get();
                        if (incomingTokenHash.equals(existingTokenHashAndSalt[0])) {
                            return new Authentication(authenticatedUser.getName(),
                                    authenticatedUser.getRoles().toArray(String[]::new), userApiToken.getScope());
            throw new AuthenticationException("Unknown API token");
        throw new IllegalArgumentException("Invalid credential type");
    public void changePassword(User user, String newPassword) {
        if (!(user instanceof ManagedUser)) {
            throw new IllegalArgumentException("User is not managed: " + user.getName());
        String passwordHash = hash(newPassword, passwordSalt, PASSWORD_ITERATIONS).get();
        managedUser.setPasswordSalt(passwordSalt);
        managedUser.setPasswordHash(passwordHash);
        update(user);
    public void addUserSession(User user, UserSession session) {
            throw new IllegalArgumentException("User authentication is not managed by openHAB: " + user.getName());
        authenticatedUser.getSessions().add(session);
    public void removeUserSession(User user, UserSession session) {
        authenticatedUser.getSessions().remove(session);
    public void clearSessions(User user) {
        authenticatedUser.getSessions().clear();
    public String addUserApiToken(User user, String name, String scope) {
        if (!name.matches("[a-zA-Z0-9]*")) {
            throw new IllegalArgumentException("API token name format invalid, alphanumeric characters only");
        String tokenSalt = generateSalt(KEY_LENGTH / 8).get();
        byte[] rnd = new byte[64];
        RAND.nextBytes(rnd);
        String token = APITOKEN_PREFIX + "." + name + "."
                + Base64.getEncoder().encodeToString(rnd).replaceAll("(\\+|/|=)", "");
        String tokenHash = hash(token, tokenSalt, APITOKEN_ITERATIONS).get();
        UserApiToken userApiToken = new UserApiToken(name, tokenHash + ":" + tokenSalt, scope);
        authenticatedUser.getApiTokens().add(userApiToken);
    public void removeUserApiToken(User user, UserApiToken userApiToken) {
        authenticatedUser.getApiTokens().remove(userApiToken);
    public @Nullable User update(User element) {
        String key = element.getName();
        Provider<User> provider = getProvider(key);
        // If the provider of this element is a ManagedProvider,
        // invoke the update method of that provider instead of the default one
        // This allows for registering additional ManagedProviders, e.g., for providing LDAP users
        if (provider instanceof ManagedProvider<User, ?> managedProvider) {
            return managedProvider.update(element);
        return (UsernamePasswordCredentials.class.isAssignableFrom(type));
