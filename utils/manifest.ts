export default class CodeGenManifest extends Command {
    static description = `Generate a class registration manifest to prevent tree-shaking.
MemberJunction uses @RegisterClass decorators with a dynamic class factory.
Modern bundlers (ESBuild, Vite) cannot detect dynamic instantiation and will
tree-shake these classes out of production builds. This command scans the
dependency tree for all @RegisterClass-decorated classes and emits a manifest
file with static imports that the bundler cannot eliminate.
Typically used as a prebuild/prestart script for MJAPI and MJExplorer. For
MJ distribution users, pre-built manifests ship inside @memberjunction/server-bootstrap
and @memberjunction/ng-bootstrap -- use --exclude-packages @memberjunction to
generate a supplemental manifest covering only your own application classes.`;
            description: 'Generate manifest with default output path',
            command: '<%= config.bin %> <%= command.id %> --appDir ./packages/MJAPI --output ./packages/MJAPI/src/generated/class-registrations-manifest.ts',
            description: 'Generate manifest for a specific application directory',
            command: '<%= config.bin %> <%= command.id %> --exclude-packages @memberjunction',
            description: 'Exclude MJ packages (use pre-built bootstrap manifests instead)',
            command: '<%= config.bin %> <%= command.id %> --filter BaseEngine --filter BaseAction --verbose',
            description: 'Only include specific base classes with detailed progress',
            description: 'Output file path for the generated manifest. The file will contain named imports and a CLASS_REGISTRATIONS array.',
            default: './src/generated/class-registrations-manifest.ts',
        appDir: Flags.string({
            description: 'Root directory of the application whose package.json dependency tree will be scanned. Defaults to the current working directory.',
        filter: Flags.string({
            description: 'Only include classes extending this base class. Can be repeated (e.g., --filter BaseEngine --filter BaseAction).',
        'exclude-packages': Flags.string({
            description: 'Skip packages whose name starts with this prefix. Useful for excluding @memberjunction packages when using pre-built bootstrap manifests. Can be repeated.',
            description: 'Suppress all output except errors.',
            description: 'Show detailed progress including per-package scanning info and skipped classes.',
        const { generateClassRegistrationsManifest } = await import('@memberjunction/codegen-lib');
        const { flags } = await this.parse(CodeGenManifest);
        const excludePackages = flags['exclude-packages'];
        const result = await generateClassRegistrationsManifest({
            outputPath: flags.output,
            appDir: flags.appDir || process.cwd(),
            filterBaseClasses: flags.filter && flags.filter.length > 0 ? flags.filter : undefined,
            excludePackages: excludePackages && excludePackages.length > 0 ? excludePackages : undefined,
            this.error(`Manifest generation failed:\n${result.errors.map(e => `  - ${e}`).join('\n')}`);
            if (result.ManifestChanged) {
                this.log(`[class-manifest] Updated: ${result.classes.length} classes from ${result.packages.length} packages (${result.totalDepsWalked} deps walked)`);
                this.log(`[class-manifest] No changes detected (${result.classes.length} classes, ${result.packages.length} packages)`);
