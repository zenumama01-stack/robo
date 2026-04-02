 * User helper utilities for QueryGen CLI operations
 * Get the system user from UserCache
 * The System user is populated in the UserCache when the database provider is initialized.
 * This user is used for CLI operations where no specific user context exists.
 * @returns The System UserInfo object from the cache
 * @throws Error if System user is not found in cache or doesn't have Developer role
      "System user not found in cache. Ensure the database provider is initialized " +
      "before running QueryGen commands (e.g., via 'mj querygen' which initializes the provider)."
      "The System user must have the Developer role to perform QueryGen operations."
