 * Database connection configuration and management for MemberJunction CodeGen.
 * Provides SQL Server connection setup using mssql package with configuration
 * from the main config file.
import { configInfo } from './config';
/** Extract database connection parameters from configuration */
const { dbDatabase, dbHost, codeGenPassword, dbPort, codeGenLogin, dbInstanceName, dbTrustServerCertificate } = configInfo;
 * SQL Server configuration object for mssql package.
 * Configured with extended timeout for code generation operations
 * which can involve long-running schema analysis queries.
export const sqlConfig: mssql.config = {
  /** Database username for authentication */
  user: codeGenLogin,
  /** Database password for authentication */
  password: codeGenPassword,
  /** Database server hostname or IP */
  /** Target database name */
  /** Database server port (typically 1433) */
    /** Extended timeout for long-running code generation queries */
    requestTimeout: 120000,
    /** Enable encrypted connections */
    /** SQL Server instance name if using named instances */
    instanceName: dbInstanceName && dbInstanceName.trim().length > 0 ? dbInstanceName : undefined,
    /** Whether to trust the server certificate */
    trustServerCertificate: dbTrustServerCertificate === 'Y',
 * Cached connection pool instance for reuse across code generation operations
let _pool: mssql.ConnectionPool;
 * Gets or creates a SQL Server connection pool for database operations.
 * Uses singleton pattern to reuse the same connection pool across multiple
 * code generation operations for better performance.
 * @returns Promise resolving to the mssql ConnectionPool instance
 * @throws Error if connection fails
export async function MSSQLConnection(): Promise<mssql.ConnectionPool> {
  if (!_pool) {
    _pool = await mssql.connect(sqlConfig);
  return _pool;
