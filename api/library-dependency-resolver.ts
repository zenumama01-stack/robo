 * @fileoverview Library dependency resolution utilities
 * Provides dependency graph construction, cycle detection, topological sorting, and version resolution
  DependencyGraph,
  DependencyNode,
  LoadOrderResult,
  ParsedDependency,
  ParsedVersion,
  VersionRange,
  VersionRangeType,
  VersionRequirement,
  DependencyResolutionOptions
} from '../types/dependency-types';
 * Resolves library dependencies and determines load order
export class LibraryDependencyResolver {
  constructor(options?: DependencyResolutionOptions) {
    this.debug = options?.debug || false;
   * Parse dependencies from JSON string
   * @param json - JSON string containing dependencies
   * @returns Map of library name to version specification
  parseDependencies(json: string | null): Map<string, string> {
    const dependencies = new Map<string, string>();
    if (!json || json.trim() === '') {
      if (typeof parsed === 'object' && parsed !== null) {
        for (const [name, version] of Object.entries(parsed)) {
          if (typeof version === 'string') {
            dependencies.set(name, version);
      console.error('Failed to parse dependencies JSON:', error);
   * Build a dependency graph from a collection of libraries
   * @param libraries - All available libraries
   * @returns Dependency graph structure
  buildDependencyGraph(libraries: MJComponentLibraryEntity[]): DependencyGraph {
    const nodes = new Map<string, DependencyNode>();
    const roots = new Set<string>();
    // First pass: create nodes for all libraries
    for (const library of libraries) {
      const dependencies = this.parseDependencies(library.Dependencies);
      nodes.set(library.Name, {
        library,
        dependents: new Set()
      // Initially assume all are roots
      roots.add(library.Name);
    // Second pass: establish dependency relationships
    for (const [name, node] of nodes) {
      for (const [depName, depVersion] of node.dependencies) {
        const depNode = nodes.get(depName);
          // This library depends on depName, so depName is not a root
          roots.delete(depName);
          // Add this library as a dependent of depName
          depNode.dependents.add(name);
        } else if (this.debug) {
          console.warn(`Dependency '${depName}' not found for library '${name}'`);
    return { nodes, roots };
   * Detect circular dependencies in the graph
   * @returns Array of cycles found
  detectCycles(graph: DependencyGraph): string[][] {
    const path: string[] = [];
    const detectCyclesUtil = (nodeName: string): void => {
      visited.add(nodeName);
      recursionStack.add(nodeName);
      path.push(nodeName);
      const node = graph.nodes.get(nodeName);
        for (const [depName] of node.dependencies) {
          if (!visited.has(depName)) {
            detectCyclesUtil(depName);
          } else if (recursionStack.has(depName)) {
            const cycleStartIndex = path.indexOf(depName);
            const cycle = [...path.slice(cycleStartIndex), depName];
      path.pop();
      recursionStack.delete(nodeName);
    // Check all nodes
    for (const nodeName of graph.nodes.keys()) {
      if (!visited.has(nodeName)) {
        detectCyclesUtil(nodeName);
   * Perform topological sort to determine load order
   * @returns Sorted array of libraries to load in order
  topologicalSort(graph: DependencyGraph): MJComponentLibraryEntity[] {
    const result: MJComponentLibraryEntity[] = [];
    const tempMarked = new Set<string>();
    const visit = (nodeName: string): boolean => {
      if (tempMarked.has(nodeName)) {
        // Cycle detected
      if (visited.has(nodeName)) {
      tempMarked.add(nodeName);
          if (graph.nodes.has(depName)) {
            if (!visit(depName)) {
        result.push(node.library);
      tempMarked.delete(nodeName);
    // Start with all nodes
        if (!visit(nodeName)) {
          console.error(`Cycle detected involving library: ${nodeName}`);
   * Parse a semver version string
   * @param version - Version string (e.g., "1.2.3")
   * @returns Parsed version object
  private parseVersion(version: string): ParsedVersion | null {
    const match = version.match(/^(\d+)\.(\d+)\.(\d+)(?:-([^+]+))?(?:\+(.+))?$/);
      major: parseInt(match[1], 10),
      minor: parseInt(match[2], 10),
      patch: parseInt(match[3], 10),
      prerelease: match[4],
      build: match[5]
   * Parse a version specification into a range
   * @param spec - Version specification (e.g., "^1.2.3", "~1.2.0", "1.2.3")
   * @returns Parsed version range
  private parseVersionRange(spec: string): VersionRange {
    if (spec === '*' || spec === '' || spec === 'latest') {
      return { type: 'any', raw: spec };
    // Tilde range (~1.2.3)
    if (spec.startsWith('~')) {
      const version = this.parseVersion(spec.substring(1));
        type: 'tilde',
        version: version || undefined,
        raw: spec
    // Caret range (^1.2.3)
    if (spec.startsWith('^')) {
        type: 'caret',
    // Exact version
    const version = this.parseVersion(spec);
    if (version) {
        type: 'exact',
    // Default to any if we can't parse
   * Check if a version satisfies a version range
   * @param version - Version to check
   * @param range - Version range specification
   * @returns True if version satisfies range
  private versionSatisfiesRange(version: ParsedVersion, range: VersionRange): boolean {
    if (range.type === 'any') {
    if (!range.version) {
    const rangeVer = range.version;
    switch (range.type) {
      case 'exact':
        return version.major === rangeVer.major &&
               version.minor === rangeVer.minor &&
               version.patch === rangeVer.patch;
      case 'tilde': // ~1.2.3 means >=1.2.3 <1.3.0
        if (version.major !== rangeVer.major) return false;
        if (version.minor !== rangeVer.minor) return false;
        return version.patch >= rangeVer.patch;
      case 'caret': // ^1.2.3 means >=1.2.3 <2.0.0 (for 1.x.x)
        if (rangeVer.major === 0) {
          // ^0.x.y behaves like ~0.x.y
          if (version.major !== 0) return false;
          if (rangeVer.minor === 0) {
            // ^0.0.x means exactly 0.0.x
            return version.minor === 0 && version.patch === rangeVer.patch;
          // ^0.x.y means >=0.x.y <0.(x+1).0
        // Normal caret for major > 0
        if (version.minor < rangeVer.minor) return false;
        if (version.minor === rangeVer.minor) {
   * Compare two versions for sorting
   * @returns -1 if a < b, 0 if a === b, 1 if a > b
  private compareVersions(a: ParsedVersion, b: ParsedVersion): number {
    if (a.major !== b.major) return a.major - b.major;
    if (a.minor !== b.minor) return a.minor - b.minor;
    if (a.patch !== b.patch) return a.patch - b.patch;
    // Handle prerelease versions
    if (!a.prerelease && b.prerelease) return 1; // a is newer
    if (a.prerelease && !b.prerelease) return -1; // b is newer
    if (a.prerelease && b.prerelease) {
      return a.prerelease.localeCompare(b.prerelease);
   * Resolve version conflicts using NPM-style resolution
   * @param requirements - Array of version requirements
   * @param availableLibraries - All available libraries to choose from
   * @returns Resolved version information
  resolveVersionConflicts(
    requirements: VersionRequirement[],
    availableLibraries: MJComponentLibraryEntity[]
  ): ResolvedVersion {
    if (requirements.length === 0) {
      throw new Error('No version requirements provided');
    const libraryName = requirements[0].library;
    // Find all available versions for this library
    const availableVersions = availableLibraries
      .filter(lib => lib.Name === libraryName && lib.Version)
      .map(lib => ({
        library: lib,
        version: this.parseVersion(lib.Version!)
      .filter(item => item.version !== null)
      .sort((a, b) => this.compareVersions(b.version!, a.version!)); // Sort descending
    if (availableVersions.length === 0) {
      throw new Error(`No versions available for library '${libraryName}'`);
    // Parse all requirements
    const ranges = requirements.map(req => ({
      ...req,
      range: this.parseVersionRange(req.versionSpec)
    // Find the highest version that satisfies all requirements
    for (const { library, version } of availableVersions) {
      let satisfiesAll = true;
      const satisfiedRequirements: VersionRequirement[] = [];
      for (const req of ranges) {
        if (this.versionSatisfiesRange(version!, req.range)) {
          satisfiedRequirements.push(req);
          satisfiesAll = false;
            `Version ${library.Version} does not satisfy '${req.versionSpec}' required by ${req.requestedBy}`
      if (satisfiesAll) {
          library: libraryName,
          version: library.Version!,
          satisfies: requirements,
          warnings: warnings.length > 0 ? warnings : undefined
    // No version satisfies all requirements
    // Return the latest version with warnings
    const latest = availableVersions[0];
      `Could not find a version that satisfies all requirements. Using ${latest.library.Version}`
      version: latest.library.Version!,
      satisfies: [],
   * Get the load order for a set of requested libraries
   * @param requestedLibs - Library names requested
   * @param allLibs - All available libraries
   * @param options - Resolution options
   * @returns Load order result
  getLoadOrder(
    requestedLibs: string[],
    allLibs: MJComponentLibraryEntity[],
    options?: DependencyResolutionOptions
  ): LoadOrderResult {
    // Filter out null, undefined, and non-string values from requestedLibs
    const validRequestedLibs = requestedLibs.filter(lib => {
      if (!lib || typeof lib !== 'string') {
        const warning = `Invalid library name: ${lib} (type: ${typeof lib})`;
        if (this.debug || options?.debug) {
          console.warn(`⚠️ ${warning}`);
      console.log('🔍 Getting load order for requested libraries:');
      console.log('  📝 Requested (raw):', requestedLibs);
      console.log('  📝 Requested (valid):', validRequestedLibs);
      console.log('  📚 Total available libraries:', allLibs.length);
    // Build a map for quick lookup (case-insensitive)
    const libMap = new Map<string, MJComponentLibraryEntity[]>();
    for (const lib of allLibs) {
      if (!lib?.Name) {
        warnings.push(`Library with missing name found in available libraries`);
      const key = lib.Name.toLowerCase();
      if (!libMap.has(key)) {
        libMap.set(key, []);
      libMap.get(key)!.push(lib);
    // Collect all libraries needed (requested + their dependencies)
    const needed = new Set<string>();
    const toProcess = [...validRequestedLibs];
    const versionRequirements = new Map<string, VersionRequirement[]>();
    const maxDepth = options?.maxDepth || 10;
    while (toProcess.length > 0 && depth < maxDepth) {
      // Extra safety check for null/undefined
      if (!current || typeof current !== 'string') {
        const warning = `Unexpected invalid library name during processing: ${current}`;
      if (processed.has(current)) continue;
      processed.add(current);
      needed.add(current);
      // Find the library (case-insensitive lookup)
      const libVersions = libMap.get(current.toLowerCase());
      if (!libVersions || libVersions.length === 0) {
          console.log(`    ❌ Library '${current}' not found in available libraries`);
        errors.push(`Library '${current}' not found`);
      // For now, use the first version found (should be resolved properly)
      const lib = libVersions[0];
      const deps = this.parseDependencies(lib.Dependencies);
      if ((this.debug || options?.debug) && deps.size > 0) {
        console.log(`    📌 ${current} requires:`, Array.from(deps.entries()));
      // Process dependencies
      for (const [depName, depVersion] of deps) {
        needed.add(depName);
        // Track version requirements
        if (!versionRequirements.has(depName)) {
          versionRequirements.set(depName, []);
        versionRequirements.get(depName)!.push({
          library: depName,
          versionSpec: depVersion,
          requestedBy: current
        if (!processed.has(depName)) {
          toProcess.push(depName);
    if (depth >= maxDepth) {
      warnings.push(`Maximum dependency depth (${maxDepth}) reached`);
    // Resolve version conflicts for each library
    const resolvedLibraries: MJComponentLibraryEntity[] = [];
    for (const libName of needed) {
      const requirements = versionRequirements.get(libName) || [];
      const versions = libMap.get(libName.toLowerCase()) || [];
      if (versions.length === 0) {
        errors.push(`Library '${libName}' not found`);
      if (requirements.length > 0) {
          const resolved = this.resolveVersionConflicts(requirements, versions);
          const selectedLib = versions.find(lib => lib.Version === resolved.version);
          if (selectedLib) {
            resolvedLibraries.push(selectedLib);
            if (resolved.warnings) {
              warnings.push(...resolved.warnings);
          errors.push(error.message);
          // Use first available version as fallback
          resolvedLibraries.push(versions[0]);
        // No specific requirements, use the first (default) version
    // Build dependency graph with resolved libraries
    const graph = this.buildDependencyGraph(resolvedLibraries);
    const cycles = this.detectCycles(graph);
    if (cycles.length > 0) {
      errors.push(`Circular dependencies detected: ${cycles.map(c => c.join(' -> ')).join(', ')}`);
        cycles,
    const sorted = this.topologicalSort(graph);
      console.log('✅ Load order determined:', sorted.map(lib => `${lib.Name}@${lib.Version}`));
      order: sorted,
      errors: errors.length > 0 ? errors : undefined,
   * Get direct dependencies of a library
   * @param library - Library to get dependencies for
   * @returns Map of dependency names to version specs
  getDirectDependencies(library: MJComponentLibraryEntity): Map<string, string> {
    return this.parseDependencies(library.Dependencies);
   * Get all transitive dependencies of a library
   * @param libraryName - Library name to analyze
   * @param maxDepth - Maximum depth to traverse
   * @returns Set of all dependency names (including transitive)
  getTransitiveDependencies(
    libraryName: string,
    const dependencies = new Set<string>();
    const toProcess = [libraryName];
    // Build lookup map (case-insensitive)
    const libMap = new Map<string, MJComponentLibraryEntity>();
      libMap.set(lib.Name.toLowerCase(), lib);
      const lib = libMap.get(current.toLowerCase());
      if (!lib) continue;
      for (const [depName] of deps) {
        dependencies.add(depName);
