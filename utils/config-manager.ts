 * @fileoverview Shared configuration manager for MetadataSync commands
 * @module lib/config-manager
 * This module provides a centralized configuration management system that handles
 * loading MJ config from the original working directory, regardless of any
 * directory changes made during command execution.
import { MJConfig } from '../config';
 * Default configuration for MetadataSync
 * Provides database connection settings from environment variables,
 * matching the pattern used by MJServer's DEFAULT_SERVER_CONFIG.
 * This ensures consistent behavior with the MJ ecosystem.
const DEFAULT_SYNC_CONFIG: Partial<MJConfig> = {
  dbEncrypt: process.env.DB_ENCRYPT ?? undefined,
 * Configuration manager singleton for handling MJ configuration
 * Stores the original working directory and MJ configuration to ensure
 * consistent access across all commands, even when the current working
 * directory changes during execution.
export class ConfigManager {
  private static instance: ConfigManager;
  private originalCwd: string | null = null;
  private mjConfig: MJConfig | null = null;
  private configLoaded = false;
    // Original cwd will be set on first access
   * Get the singleton instance of ConfigManager
  static getInstance(): ConfigManager {
    if (!ConfigManager.instance) {
      ConfigManager.instance = new ConfigManager();
    return ConfigManager.instance;
   * Get the original working directory from when the process started
   * @returns The original working directory path
  getOriginalCwd(): string {
    if (!this.originalCwd) {
      // Capture on first access
      this.originalCwd = process.cwd();
    return this.originalCwd;
   * Set the original working directory (for testing or special cases)
   * @param cwd - The working directory to use as original
  setOriginalCwd(cwd: string): void {
    this.originalCwd = cwd;
   * Load MemberJunction configuration
   * Searches for mj.config.cjs starting from the original working directory
   * and walking up the directory tree. Caches the result for subsequent calls.
   * @param forceReload - Force reload the configuration even if cached
  loadMJConfig(forceReload = false): MJConfig | null {
    if (this.configLoaded && !forceReload) {
      return this.mjConfig;
      // Always search from the original working directory
      const searchPath = this.getOriginalCwd();
      const result = explorer.search(searchPath);
      // Merge user config with DEFAULT_SYNC_CONFIG (user config takes precedence)
      // This ensures environment variables are used for database settings
      // when not explicitly set in the config file
      const userConfig = result?.config ?? {};
      this.mjConfig = mergeConfigs(DEFAULT_SYNC_CONFIG, userConfig) as MJConfig;
      this.configLoaded = true;
      console.error('Error loading MJ config:', error);
      this.mjConfig = null;
   * Get the cached MJ configuration
   * @returns The cached MJConfig or null if not loaded
  getMJConfig(): MJConfig | null {
    if (!this.configLoaded) {
      return this.loadMJConfig();
// Export singleton instance for convenience
export const configManager = ConfigManager.getInstance();