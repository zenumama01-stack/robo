 * Fixture-Based Linter Test Suite Runner
 * Runs linter validation tests against real component specs loaded from fixtures.
 * This is the primary test runner for testing against actual production components.
 *   npm test:fixtures                    # Run fixture tests
 *   npm test:fixtures -- --verbose       # Run with detailed output
import { runTests, SuiteResult } from './infrastructure/test-runner';
  setContextUser,
  registerBulkBrokenTests,
  registerBulkFixedTests,
  registerBulkValidTests,
  displayFixtureStats
} from './tests/fixture-tests';
  console.log('║              Fixture-Based Linter Test Suite Runner                         ║');
    // Load real context user from database
    contextUser = await getContextUser();
    // Initialize ComponentMetadataEngine with context user
    // Set context user for fixture tests
    setContextUser(contextUser);
    // Display fixture statistics
    await displayFixtureStats();
    console.log('Running Fixture Tests...');
    // Register all fixture-based tests (must await since they load fixtures)
    await registerBulkBrokenTests();
    await registerBulkFixedTests();
    await registerBulkValidTests();
      console.log('  🎉 All fixture tests passed!\n');
