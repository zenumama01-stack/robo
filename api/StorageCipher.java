 * This allows the encryption and decryption to be performed before saving to storage.
public interface StorageCipher {
     * A unique cipher identifier per each implementation of StorageCipher.
     * It allows the OAuthStoreHandler to choose which cipher implementation to use.
     * This is particularly important when old ciphers becomes out-dated and
     * need to be replaced by new implementations.
     * @return unique identifier
    String getUniqueCipherId();
     * Encrypt the plainText, then produce a base64 encoded cipher text
     * @param plainText
     * @return base64 encoded( encrypted( text ) )
     * @throws GeneralSecurityException all security-related exception
    String encrypt(@Nullable String plainText) throws GeneralSecurityException;
     * Decrypt the base64 encoded cipher text.
     * @param base64CipherText This should be the result from the {@link #encrypt(String)}
     * @return plain text
    String decrypt(@Nullable String base64CipherText) throws GeneralSecurityException;
