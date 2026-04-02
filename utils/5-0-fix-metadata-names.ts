export default class V50FixMetadataNames extends Command {
    static description = `[v5.0 Migration] Scan metadata JSON files for entity names that need "MJ: " prefix updates.
Targets the metadata/ directory used by "mj sync". Detects entity name references in
@lookup: directives (both the entity name and lookup value), .mj-sync.json and
.mj-folder.json config files (entity/entityName fields), relatedEntities object keys,
and fields.Name values in Entities-managing folders. Runs in dry-run mode by default;
use --fix to apply.
            description: 'Dry-run scan of the metadata directory',
            command: '<%= config.bin %> <%= command.id %> --path metadata/',
            description: 'Apply fixes to metadata files',
            command: '<%= config.bin %> <%= command.id %> --path metadata/ --fix',
            description: 'Scan a specific subdirectory',
            command: '<%= config.bin %> <%= command.id %> --path metadata/resource-types',
            description: 'Scan and fix a single metadata file',
            command: '<%= config.bin %> <%= command.id %> --path metadata/entities/.audit-related-entities.json --fix',
            description: 'File or directory to scan. Accepts a single .json file or a directory (scanned recursively, including dotfiles like .mj-sync.json). Defaults to the current working directory.',
        const { scanMetadataNames } = await import('@memberjunction/codegen-lib');
        type ScanResult = Awaited<ReturnType<typeof scanMetadataNames>>;
        const { flags } = await this.parse(V50FixMetadataNames);
        const result: ScanResult = await scanMetadataNames({
