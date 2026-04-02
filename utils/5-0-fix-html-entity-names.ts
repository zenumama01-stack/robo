export default class V50FixHtmlEntityNames extends Command {
    static description = `[v5.0 Migration] Scan Angular HTML template files for hardcoded entity names that need "MJ: " prefix updates.
Uses targeted regex patterns to find entity name references in template expressions
and attribute values. Detects method calls like navigateToEntity('Actions'),
OpenEntityRecord('Entities', id), and attribute values like RowsEntityName="Users".
Runs in dry-run mode by default; use --fix to apply.
The rename map is built dynamically from entity_subclasses.ts by parsing all
@RegisterClass(BaseEntity, 'MJ: XYZ') decorators (~272 entries).`;
            description: 'Dry-run scan of Angular templates',
            command: '<%= config.bin %> <%= command.id %> --path packages/Angular/',
            description: 'Apply fixes to HTML templates',
            command: '<%= config.bin %> <%= command.id %> --path packages/Angular/ --fix',
            description: 'Scan with verbose output',
            command: '<%= config.bin %> <%= command.id %> --path packages/ -v',
            description: 'Scan a single template file',
            command: '<%= config.bin %> <%= command.id %> --path packages/Angular/Explorer/dashboards/src/Actions/actions-dashboard.component.html',
            description: 'File or directory to scan. Accepts a single .html file or a directory (scanned recursively). Defaults to the current working directory.',
        const { scanHtmlEntityNames } = await import('@memberjunction/codegen-lib');
        type ScanResult = Awaited<ReturnType<typeof scanHtmlEntityNames>>;
        const { flags } = await this.parse(V50FixHtmlEntityNames);
        const result: ScanResult = await scanHtmlEntityNames({
            this.log(`\nScanned ${result.FilesScanned} files, found ${result.Findings.length} entity name(s) needing update`);
