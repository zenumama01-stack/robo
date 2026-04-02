import type { config as SQLConfig } from 'mssql';
import { configInfo, dbDatabase, dbHost, dbPassword, dbPort, dbUsername, dbInstanceName, dbTrustServerCertificate } from './config.js';
 * Create MSSQL configuration object with all necessary settings
 * Follows the same pattern as MJServer for consistency
const createMSSQLConfig = (): SQLConfig => {
  const mssqlConfig: SQLConfig = {
      max: configInfo.databaseSettings.connectionPool?.max ?? 50,
      min: configInfo.databaseSettings.connectionPool?.min ?? 5,
      idleTimeoutMillis: configInfo.databaseSettings.connectionPool?.idleTimeoutMillis ?? 30000,
      acquireTimeoutMillis: configInfo.databaseSettings.connectionPool?.acquireTimeoutMillis ?? 30000,
    mssqlConfig.options = {
      ...mssqlConfig.options,
      instanceName: dbInstanceName,
  if (dbTrustServerCertificate !== null && dbTrustServerCertificate !== undefined) {
  return mssqlConfig;
export default createMSSQLConfig;const createMSSQLConfig = (): sql.config => {
  const mssqlConfig: sql.config = {
  //console.log({ mssqlConfig: { ...mssqlConfig, password: '***' } });
export default createMSSQLConfig;const createMSSQLConfig = (): sql.config => {
export default createMSSQLConfig;const createMSSQLConfig = (): sql.config => {
