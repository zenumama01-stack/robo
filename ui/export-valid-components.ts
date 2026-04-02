#!/usr/bin/env ts-node
 * Export Valid Components from Database
 * Queries the MJ: Components entity to find all active root-level components
 * and exports them as fixture files for linter validation.
 * Root-level components are identified by:
 * - Status = 'Active'
 * - ParentComponentID IS NULL (no parent = root level)
import { initializeDatabase, cleanupDatabase } from '../src/infrastructure/database-setup';
async function exportValidComponents() {
  console.log('╔══════════════════════════════════════════════════════════════════════════════╗');
  console.log('║              Export Valid Components from Database                          ║');
  console.log('╚══════════════════════════════════════════════════════════════════════════════╝\n');
    console.log('🔄 Initializing database connection...');
    await initializeDatabase();
    // Create minimal mock contextUser for queries
      ID: '00000000-0000-0000-0000-000000000000',
      Email: 'export@script.com',
      Name: 'Export Script',
      FirstName: 'Export',
      LastName: 'Script'
    // Query for active root-level components
    console.log('🔍 Querying for active root-level components...');
      ExtraFilter: "Status='Active' AND ParentComponentID IS NULL",
      throw new Error(`Failed to query components: ${result.ErrorMessage}`);
    const components = result.Results || [];
    console.log(`✅ Found ${components.length} active root-level components\n`);
    if (components.length === 0) {
      console.log('⚠️  No active root-level components found. Exiting.');
    // Display component list
    console.log('📋 Components found:');
    components.forEach((comp: any, idx: number) => {
      console.log(`   ${idx + 1}. ${comp.Name} (${comp.Type}) - Registry: ${comp.Registry || 'N/A'}`);
    // Export to fixtures directory
    const fixturesDir = path.join(__dirname, '../fixtures/valid-components');
    if (!fs.existsSync(fixturesDir)) {
      fs.mkdirSync(fixturesDir, { recursive: true });
    console.log('💾 Exporting components to fixtures/valid-components/...');
    let exportedCount = 0;
    for (const comp of components) {
      // Skip if no code
      if (!comp.Code || comp.Code.trim() === '') {
        console.log(`   ⚠️  Skipping ${comp.Name} - no code`);
      // Create component spec object
      const spec = {
        name: comp.Name,
        type: comp.Type || 'unknown',
        title: comp.Title || comp.Name,
        description: comp.Description || `Active root-level component: ${comp.Name}`,
        code: comp.Code,
        location: comp.Location || 'embedded',
        functionalRequirements: comp.FunctionalRequirements || '',
        technicalDesign: comp.TechnicalDesign || '',
        exampleUsage: comp.ExampleUsage || `<${comp.Name} />`,
        namespace: comp.Namespace || '',
        version: comp.Version || '1.0.0',
        registry: comp.Registry || '',
        status: comp.Status,
        // Include data requirements if available
        ...(comp.DataRequirementsJSON && {
          dataRequirements: JSON.parse(comp.DataRequirementsJSON)
      // Sanitize filename
      const filename = comp.Name
        .replace(/[^a-zA-Z0-9-]/g, '-')
        .replace(/-+/g, '-')
      const filepath = path.join(fixturesDir, `${filename}.json`);
      fs.writeFileSync(filepath, JSON.stringify(spec, null, 2));
      console.log(`   ✅ Exported: ${filename}.json`);
      exportedCount++;
    console.log('📊 Export Summary');
    console.log(`   Total Components: ${components.length}`);
    console.log(`   Exported: ${exportedCount}`);
    console.log(`   Skipped: ${skippedCount}`);
    console.log(`🎉 Components exported to: ${fixturesDir}`);
    console.log('Next steps:');
    console.log('  1. Run: npm run test:fixtures');
    console.log('  2. Review any violations found');
    console.log('  3. Fix issues in source components');
    console.error('\n❌ ERROR:', error);
    await cleanupDatabase();
// Run export
exportValidComponents().catch((error) => {
