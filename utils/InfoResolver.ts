import { Field, ObjectType, Int, Query, Resolver, Ctx, Info as RequestInfo } from 'type-graphql';
import { Public, RequireSystemUser } from '../directives/index.js';
// Use createRequire to import JSON (compatible with module: es2022)
const packageJson = require('../../package.json') as { version: string };
export class ServerInfo {
  IsSystemUser: boolean;
  Platform: string;
  Arch: string;
  CpuModel: string;
  Hostname: string;
@Resolver(ServerInfo)
export class InfoResolver {
  @Query(() => ServerInfo)
  Info(@Ctx() context: AppContext): ServerInfo {
      Version: packageJson.version,
      IsSystemUser: Boolean(context.userPayload.isSystemUser),
      Platform: os.platform(),
      Arch: os.platform(),
      CpuModel: os.cpus()?.[0].model,
      Hostname: os.hostname(),
