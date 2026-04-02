import 'vitest';
interface MJCustomMatchers<R = unknown> {
  /** Asserts that a value is a valid MJ entity with a non-empty ID */
  toBeValidEntity(): R;
  /** Asserts that a RunView result has Success: true */
  toHaveSucceeded(): R;
  /** Asserts that an entity-like object has a specific field by name */
  toHaveEntityField(fieldName: string): R;
declare module 'vitest' {
  // eslint-disable-next-line @typescript-eslint/no-empty-interface
  interface Assertion<T = unknown> extends MJCustomMatchers<T> {}
  interface AsymmetricMatchersContaining extends MJCustomMatchers {}
  TestAPI,
  SuiteAPI,
  TestFunction,
  SuiteFunction,
  ExpectStatic,
} from "vitest"
type MetaTest = TestFunction
type MetaSuite = SuiteFunction
interface ConditionalTestAPI extends TestAPI {
  ifMac: ConditionalTestAPI
  ifWindows: ConditionalTestAPI
  ifLinux: ConditionalTestAPI
  ifNotMac: ConditionalTestAPI
  ifNotWindows: ConditionalTestAPI
  ifNotLinux: ConditionalTestAPI
  ifEnv: (envKey: boolean | string | undefined) => ConditionalTestAPI
  ifLazyTrue: (truthy: () => boolean | Promise<boolean>) => ConditionalTestAPI
  heavy: ConditionalTestAPI
interface ConditionalSuiteAPI extends SuiteAPI {
  ifMac: ConditionalSuiteAPI
  ifWindows: ConditionalSuiteAPI
  ifLinux: ConditionalSuiteAPI
  ifNotMac: ConditionalSuiteAPI
  ifNotWindows: ConditionalSuiteAPI
  ifNotLinux: ConditionalSuiteAPI
  ifEnv: (envKey: boolean | string | undefined) => ConditionalSuiteAPI
  ifLazyTrue: (truthy: () => boolean | Promise<boolean>) => ConditionalSuiteAPI
  heavy: ConditionalSuiteAPI
interface ConditionalSkipAPI extends TestAPI["skip"] {
  ifMac: ConditionalSkipAPI
  ifWindows: ConditionalSkipAPI
  ifLinux: ConditionalSkipAPI
  ifNotMac: ConditionalSkipAPI
  ifNotWindows: ConditionalSkipAPI
  ifNotLinux: ConditionalSkipAPI
export declare const test: ConditionalTestAPI
export declare const describe: ConditionalSuiteAPI
export declare const skip: ConditionalSkipAPI
export declare const expect: ExpectStatic
declare module "vitest" {
  interface TestOptions {
      platform?: "mac" | "win" | "linux"
      platformNot?: "mac" | "win" | "linux"
      ci?: boolean
      [key: string]: any
declare global {
  const it: ConditionalTestAPI
  const test: ConditionalTestAPI
  const describe: ConditionalSuiteAPI
