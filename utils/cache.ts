import { LRUCache } from 'lru-cache';
const oneHourMs = 60 * 60 * 1000;
export const authCache = new LRUCache({
  max: 50000,
  ttl: oneHourMs,
  ttlAutopurge: false,
import * as fs from "fs"
import { CACHE_FILE, SupportedPlatforms } from "./smart-config"
export interface TestStats {
  platformRuns?: Record<
    SupportedPlatforms,
      runs: number
      fails: number
      avgMs: number
  heavy?: boolean
  unstable?: boolean
  hasHeavyTests?: boolean
interface SmartCache {
  tests: Record<string, TestStats>
  files: Record<string, FileStats>
export function loadCache(): SmartCache {
  // Check if file exists BEFORE trying to read it
  if (!fs.existsSync(CACHE_FILE)) {
    console.log(`[loadCache] Cache file does not exist, starting fresh`)
    return { tests: {}, files: {} }
    const content = fs.readFileSync(CACHE_FILE, "utf8")
    const cache = JSON.parse(content)
    const testCount = Object.keys(cache.tests || {}).length
    const fileCount = Object.keys(cache.files || {}).length
    console.log(`[loadCache] ✓ Loaded cache - Tests: ${testCount}, Files: ${fileCount}`)
    console.error(`[loadCache] ✗ Error reading/parsing cache:`, err.message)
    console.error(`[loadCache] Starting with fresh cache`)
export function saveCache(cache: SmartCache) {
  console.log(`[saveCache] Saving cache - Tests: ${testCount}, Files: ${fileCount}`)
  console.log(`[saveCache] Target: ${CACHE_FILE}`)
  const dir = path.dirname(CACHE_FILE)
    console.log(`[saveCache] Creating directory: ${dir}`)
    fs.mkdirSync(dir, { recursive: true })
    // Write to temp file first for atomic write
    const tempFile = `${CACHE_FILE}.tmp`
    const content = JSON.stringify(cache, null, 2)
    fs.writeFileSync(tempFile, content, "utf8")
    fs.renameSync(tempFile, CACHE_FILE)
    console.log(`[saveCache] ✓ Cache saved successfully (${(content.length / 1024).toFixed(2)} KB)`)
    console.error(`[saveCache] ✗ Error saving cache:`, err.message)
    throw err
