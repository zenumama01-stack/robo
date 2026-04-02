package org.openhab.core.auth.oauth2client.internal.cipher;
import java.security.NoSuchAlgorithmException;
import java.security.SecureRandom;
import javax.crypto.Cipher;
import javax.crypto.KeyGenerator;
import javax.crypto.SecretKey;
import javax.crypto.spec.IvParameterSpec;
import javax.crypto.spec.SecretKeySpec;
 * This is a symmetric key encryption service for encrypting the OAuth tokens.
public class SymmetricKeyCipher implements StorageCipher {
    public static final String CIPHER_ID = "SymmetricKeyCipher";
    public static final String PID = CIPHER_ID;
    private final Logger logger = LoggerFactory.getLogger(SymmetricKeyCipher.class);
    private static final String ENCRYPTION_ALGO = "AES";
    private static final String ENCRYPTION_ALGO_MODE_WITH_PADDING = "AES/CBC/PKCS5Padding";
    private static final String PROPERTY_KEY_ENCRYPTION_KEY_BASE64 = "ENCRYPTION_KEY";
    private static final int ENCRYPTION_KEY_SIZE_BITS = 128; // do not use high grade encryption due to export limit
    private static final int IV_BYTE_SIZE = 16;
    private final ConfigurationAdmin configurationAdmin;
    private final SecretKey encryptionKey;
    private final SecureRandom random = new SecureRandom();
     * Activate will try to load the encryption key. If an existing encryption key does not exists,
     * it will generate a new one and save to {@code org.osgi.service.cm.ConfigurationAdmin}
     * @throws NoSuchAlgorithmException When encryption algorithm is not available {@code #ENCRYPTION_ALGO}
     * @throws IOException if access to persistent storage fails (@code org.osgi.service.cm.ConfigurationAdmin)
    public SymmetricKeyCipher(final @Reference ConfigurationAdmin configurationAdmin)
            throws NoSuchAlgorithmException, IOException {
        // load or generate the encryption key
        encryptionKey = getOrGenerateEncryptionKey();
    public String getUniqueCipherId() {
        return CIPHER_ID;
    public @Nullable String encrypt(@Nullable String plainText) throws GeneralSecurityException {
        if (plainText == null) {
        // Generate IV
        byte[] iv = new byte[IV_BYTE_SIZE];
        random.nextBytes(iv);
        Cipher cipherEnc = Cipher.getInstance(ENCRYPTION_ALGO_MODE_WITH_PADDING);
        cipherEnc.init(Cipher.ENCRYPT_MODE, encryptionKey, new IvParameterSpec(iv));
        byte[] encryptedBytes = cipherEnc.doFinal(plainText.getBytes(StandardCharsets.UTF_8));
        byte[] encryptedBytesWithIV = new byte[encryptedBytes.length + IV_BYTE_SIZE];
        // copy iv to the start of array
        System.arraycopy(iv, 0, encryptedBytesWithIV, 0, IV_BYTE_SIZE);
        // append encrypted text to tail
        System.arraycopy(encryptedBytes, 0, encryptedBytesWithIV, IV_BYTE_SIZE, encryptedBytes.length);
        return Base64.getEncoder().encodeToString(encryptedBytesWithIV);
    public @Nullable String decrypt(@Nullable String base64CipherText) throws GeneralSecurityException {
        if (base64CipherText == null) {
        // base64 decode the base64CipherText
        byte[] decodedCipherTextWithIV = Base64.getDecoder().decode(base64CipherText);
        // Read IV
        System.arraycopy(decodedCipherTextWithIV, 0, iv, 0, IV_BYTE_SIZE);
        byte[] cipherTextBytes = new byte[decodedCipherTextWithIV.length - IV_BYTE_SIZE];
        System.arraycopy(decodedCipherTextWithIV, IV_BYTE_SIZE, cipherTextBytes, 0, cipherTextBytes.length);
        Cipher cipherDec = Cipher.getInstance(ENCRYPTION_ALGO_MODE_WITH_PADDING);
        cipherDec.init(Cipher.DECRYPT_MODE, encryptionKey, new IvParameterSpec(iv));
        byte[] decryptedBytes = cipherDec.doFinal(cipherTextBytes);
        return new String(decryptedBytes, StandardCharsets.UTF_8);
    private static SecretKey generateEncryptionKey() throws NoSuchAlgorithmException {
        KeyGenerator keygen = KeyGenerator.getInstance(ENCRYPTION_ALGO);
        keygen.init(ENCRYPTION_KEY_SIZE_BITS);
        return keygen.generateKey();
    private SecretKey getOrGenerateEncryptionKey() throws NoSuchAlgorithmException, IOException {
        Configuration configuration = configurationAdmin.getConfiguration(PID);
        String encryptionKeyInBase64;
            properties = new Hashtable<>();
        if (properties.get(PROPERTY_KEY_ENCRYPTION_KEY_BASE64) == null) {
            SecretKey secretKey = generateEncryptionKey();
            encryptionKeyInBase64 = new String(Base64.getEncoder().encode(secretKey.getEncoded()));
            // Put encryption key back into config
            properties.put(PROPERTY_KEY_ENCRYPTION_KEY_BASE64, encryptionKeyInBase64);
            configuration.update(properties);
            logger.debug("Encryption key generated");
            return secretKey;
        // encryption key already present in config
        encryptionKeyInBase64 = (String) properties.get(PROPERTY_KEY_ENCRYPTION_KEY_BASE64);
        byte[] encKeyBytes = Base64.getDecoder().decode(encryptionKeyInBase64);
        // 128 bit key/ 8 bit = 16 bytes length
        logger.debug("Encryption key loaded");
        return new SecretKeySpec(encKeyBytes, 0, ENCRYPTION_KEY_SIZE_BITS / 8, ENCRYPTION_ALGO);
