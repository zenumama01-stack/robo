 * Database setup for linter tests
 * Initializes database connection and loads context user
import * as dotenv from 'dotenv';
import * as sql from 'mssql';
dotenv.config({ path: path.join(__dirname, '../../.env') });
let systemUser: UserInfo | null = null;
 * Initialize database connection and metadata
 * Note: This initializes ComponentMetadataEngine which is required for
 * dependency validation rules in the linter. The linter needs to resolve
 * component dependencies from the registry to validate props and events.
export async function initializeDatabase(): Promise<void> {
  console.log(`🔄 Initializing database connection --- Host: ${process.env.DB_HOST}; Database: ${process.env.DB_DATABASE}`);
  // Create SQL Server connection pool configuration
    server: process.env.DB_HOST || 'localhost',
    port: parseInt(process.env.DB_PORT || '1433', 10),
    user: process.env.DB_USERNAME || '',
    password: process.env.DB_PASSWORD || '',
    database: process.env.DB_DATABASE || '',
    connectionTimeout: 30000,
      trustServerCertificate: parseBooleanEnv(process.env.DB_TRUST_SERVER_CERTIFICATE),
  // Add instance name if provided
  if (process.env.DB_INSTANCE_NAME) {
      instanceName: process.env.DB_INSTANCE_NAME,
  // Create and connect the pool
  connectionPool = new sql.ConnectionPool(mssqlConfig);
  console.log(`✅ Database connection pool connected`);
  // Create provider configuration
  const mjCoreSchema = process.env.MJ_CORE_SCHEMA || '__mj';
  const config = new SQLServerProviderConfigData(
    mjCoreSchema,
    0 // checkRefreshIntervalSeconds - disable auto refresh for tests
  // Initialize SQL Server client - this sets Metadata.Provider
  // Verify metadata is initialized
  console.log(`✅ Metadata initialized with ${md?.Entities ? md.Entities.length : 0} entities`);
 * Get the SYSTEM_USER context user for tests
export async function getContextUser(): Promise<UserInfo> {
  if (!isInitialized) {
    throw new Error('Database not initialized. Call initializeDatabase() first.');
  if (systemUser) {
  const testEmail = process.env.TEST_USER_EMAIL || 'not.set@nowhere.com';
  // Try to find user in UserCache (already populated during initialization)
  systemUser = UserCache.Instance.Users.find(
    (user: UserInfo) => user.Email.toLowerCase() === testEmail.toLowerCase()
    console.warn(`Failed to load context user ${testEmail}, using an existing user with the Owner role instead`);
    // Fallback: find any user with Owner role
      (user: UserInfo) => user.Type.trim().toLowerCase() === 'owner'
        `No existing users found in the database with the Owner role, cannot proceed with tests.\n` +
        `Make sure the database has at least one user with Owner role.`
  console.log(`✅ Context user loaded: ${systemUser.Name} (${systemUser.Email})`);
 * Initialize ComponentMetadataEngine with context user
 * Must be called AFTER getContextUser()
export async function initializeComponentEngine(contextUser: UserInfo): Promise<void> {
    await ComponentMetadataEngine.Instance.Config(false, contextUser, Metadata.Provider);
    console.warn(`⚠️  ComponentMetadataEngine initialization failed:`, error);
    console.warn(`   Dependency validation rules may not work correctly`);
 * Clean up database connection
export async function cleanupDatabase(): Promise<void> {
  if (connectionPool && connectionPool.connected) {
    console.log(`✅ Database connection pool closed`);
  systemUser = null;
