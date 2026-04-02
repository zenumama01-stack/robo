import { Arg, Ctx, Field, InputType, Int, ObjectType, Query, Resolver, Float } from 'type-graphql';
    TelemetryManager,
    TelemetryCategory,
    TelemetryInsightSeverity
// GraphQL Types for TelemetryEvent
export class MemorySnapshotGQL {
export class TelemetryEventGQL {
    params: string; // JSON stringified
    @Field(() => MemorySnapshotGQL, { nullable: true })
    memoryBefore?: MemorySnapshotGQL;
    memoryAfter?: MemorySnapshotGQL;
// GraphQL Types for TelemetryPattern
export class CallerLocationGQL {
export class TelemetryPatternGQL {
    sampleParams: string; // JSON stringified
    @Field(() => [CallerLocationGQL])
    callerLocations: CallerLocationGQL[];
// GraphQL Types for TelemetryInsight
export class TelemetryInsightGQL {
    metadata?: string; // JSON stringified
// GraphQL Types for Statistics
export class CategoryStatsGQL {
    events: number;
    avgMs: number;
export class TelemetryStatsGQL {
    @Field(() => [CategoryStatsGQL])
    byCategory: CategoryStatsGQL[];
// GraphQL Types for Settings
export class CategoryOverrideGQL {
export class AutoTrimSettingsGQL {
export class DuplicateDetectionSettingsGQL {
export class AnalyzerSettingsGQL {
export class TelemetrySettingsGQL {
    level: string;
    @Field(() => [CategoryOverrideGQL])
    categoryOverrides: CategoryOverrideGQL[];
    @Field(() => AutoTrimSettingsGQL)
    autoTrim: AutoTrimSettingsGQL;
    @Field(() => DuplicateDetectionSettingsGQL)
    duplicateDetection: DuplicateDetectionSettingsGQL;
    @Field(() => AnalyzerSettingsGQL)
    analyzers: AnalyzerSettingsGQL;
// Input Types
export class TelemetryEventFilterInput {
export class TelemetryPatternFilterInput {
    sortBy?: string;
export class TelemetryInsightFilterInput {
    severity?: string;
// Resolver
export class TelemetryResolver {
     * Get telemetry settings
     * Accessible to any authenticated user - telemetry data is not sensitive
    @Query(() => TelemetrySettingsGQL)
    GetServerTelemetrySettings(@Ctx() _context: AppContext): TelemetrySettingsGQL {
        const settings = tm.Settings;
        // Convert categoryOverrides map to array
        const categoryOverrides: CategoryOverrideGQL[] = [];
        if (settings.categoryOverrides) {
            for (const [category, override] of Object.entries(settings.categoryOverrides)) {
                    categoryOverrides.push({
                        enabled: override.enabled,
                        level: override.level
            enabled: settings.enabled,
            level: settings.level,
            categoryOverrides,
                enabled: settings.autoTrim.enabled,
                maxEvents: settings.autoTrim.maxEvents,
                maxAgeMs: settings.autoTrim.maxAgeMs
                enabled: settings.duplicateDetection.enabled,
                windowMs: settings.duplicateDetection.windowMs
                enabled: settings.analyzers.enabled,
                dedupeWindowMs: settings.analyzers.dedupeWindowMs
     * Get telemetry statistics
    @Query(() => TelemetryStatsGQL)
    GetServerTelemetryStats(@Ctx() _context: AppContext): TelemetryStatsGQL {
        const byCategory: CategoryStatsGQL[] = Object.entries(stats.byCategory).map(
            ([category, data]) => ({
                avgMs: data.avgMs
     * Get telemetry events with optional filtering
    @Query(() => [TelemetryEventGQL])
    GetServerTelemetryEvents(
        @Ctx() _context: AppContext,
        @Arg('filter', () => TelemetryEventFilterInput, { nullable: true }) filter?: TelemetryEventFilterInput
    ): TelemetryEventGQL[] {
        const events = tm.GetEvents({
            category: filter?.category as TelemetryCategory | undefined,
            operation: filter?.operation,
            minElapsedMs: filter?.minElapsedMs ?? undefined,
            since: filter?.since ?? undefined,
            limit: filter?.limit ?? undefined
        return events.map(e => ({
            fingerprint: e.fingerprint,
            userId: e.userId,
            params: JSON.stringify(e.params),
            tags: e.tags,
            stackTrace: e.stackTrace,
            memoryBefore: e.memoryBefore,
            memoryAfter: e.memoryAfter,
            parentEventId: e.parentEventId
     * Get telemetry patterns with optional filtering
    @Query(() => [TelemetryPatternGQL])
    GetServerTelemetryPatterns(
        @Arg('filter', () => TelemetryPatternFilterInput, { nullable: true }) filter?: TelemetryPatternFilterInput
    ): TelemetryPatternGQL[] {
        const patterns = tm.GetPatterns({
            minCount: filter?.minCount ?? undefined,
            sortBy: filter?.sortBy as 'count' | 'totalTime' | 'avgTime' | undefined
        return patterns.map(p => {
            // Convert Map to array
            const callerLocations: CallerLocationGQL[] = [];
            p.callerLocations.forEach((count, location) => {
                callerLocations.push({ location, count });
                sampleParams: JSON.stringify(p.sampleParams),
                minElapsedMs: p.minElapsedMs === Infinity ? 0 : p.minElapsedMs,
                callerLocations,
                firstSeen: p.firstSeen,
                lastSeen: p.lastSeen,
                windowStartTime: p.windowStartTime
     * Get telemetry insights with optional filtering
    @Query(() => [TelemetryInsightGQL])
    GetServerTelemetryInsights(
        @Arg('filter', () => TelemetryInsightFilterInput, { nullable: true }) filter?: TelemetryInsightFilterInput
    ): TelemetryInsightGQL[] {
        const insights = tm.GetInsights({
            severity: filter?.severity as TelemetryInsightSeverity | undefined,
            category: filter?.category,
            entityName: filter?.entityName,
        return insights.map(i => ({
            relatedEventIds: i.relatedEventIds,
            metadata: i.metadata ? JSON.stringify(i.metadata) : undefined,
            timestamp: i.timestamp
     * Get duplicate patterns (patterns with count >= threshold)
    GetServerTelemetryDuplicates(
        @Arg('minCount', () => Int, { defaultValue: 2 }) minCount: number
        const patterns = tm.GetDuplicates(minCount);
     * Get currently active (in-progress) events
    GetServerTelemetryActiveEvents(@Ctx() _context: AppContext): TelemetryEventGQL[] {
        const events = tm.GetActiveEvents();
    // Note: Server telemetry settings are configured via mj.config.cjs and cannot be changed at runtime.
    // Use the telemetry config section in mj.config.cjs to enable/disable or change the level.
    //   telemetry: {
    //     enabled: true,
    //     level: 'standard'  // 'minimal' | 'standard' | 'verbose' | 'debug'
    // Or set MJ_TELEMETRY_ENABLED=false environment variable to disable.
