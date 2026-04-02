import { ValidationResult, ValidationError, ValidationWarning, FileValidationResult } from '../types/validation';
export class FormattingService {
     * Format validation result as JSON
    public formatValidationResultAsJson(result: ValidationResult): string {
            isValid: result.isValid,
                totalFiles: result.summary.totalFiles,
                totalEntities: result.summary.totalEntities,
                totalErrors: result.summary.totalErrors,
                totalWarnings: result.summary.totalWarnings,
                errorsByType: this.getErrorsByType(result.errors),
                warningsByType: this.getWarningsByType(result.warnings)
            errors: result.errors.map(e => ({
                type: e.type,
                entity: e.entity,
                field: e.field,
                file: e.file,
                suggestion: e.suggestion
            warnings: result.warnings.map(w => ({
                entity: w.entity,
                field: w.field,
                file: w.file,
                message: w.message,
                suggestion: w.suggestion
        return JSON.stringify(output, null, 2);
    private readonly symbols = {
        success: '✓',
        error: '✗',
        warning: '⚠',
        info: 'ℹ',
        arrow: '→',
        bullet: '•',
            horizontal: '─',
            vertical: '│',
            cross: '┼'
     * Format validation result for terminal output
    public formatValidationResult(result: ValidationResult, verbose: boolean = false): string {
        lines.push(this.formatHeader('Validation Report'));
        // Summary box
        lines.push(this.formatSummaryBox(result));
        // File results
        if (result.summary.fileResults.size > 0) {
            lines.push(this.formatSectionHeader('File Results'));
            for (const [file, fileResult] of result.summary.fileResults) {
                const hasIssues = fileResult.errors.length > 0 || fileResult.warnings.length > 0;
                if (!verbose && !hasIssues) continue;
                lines.push(this.formatFileResult(file, fileResult, verbose));
        // Detailed errors
            lines.push(this.formatSectionHeader('Errors'));
            result.errors.forEach((error, index) => {
                lines.push(this.formatError(error, index + 1));
        // Detailed warnings
            lines.push(this.formatSectionHeader('Warnings'));
            result.warnings.forEach((warning, index) => {
                lines.push(this.formatWarning(warning, index + 1));
        // Footer
        lines.push(this.formatFooter(result));
     * Format push/pull summary report
    public formatSyncSummary(
        operation: 'push' | 'pull',
            deleted: number;
            unchanged?: number;
            deferred?: number;
        lines.push(this.formatHeader(`${operation.charAt(0).toUpperCase() + operation.slice(1)} Summary`));
        const total = stats.created + stats.updated + stats.deleted + stats.skipped + (stats.unchanged || 0);
        lines.push(chalk.bold('Operation Statistics:'));
        lines.push(`  ${chalk.green(this.symbols.success)} Created: ${chalk.green(stats.created)}`);
        lines.push(`  ${chalk.blue(this.symbols.info)} Updated: ${chalk.blue(stats.updated)}`);
        if (stats.unchanged !== undefined) {
            lines.push(`  ${chalk.gray('-')} Unchanged: ${chalk.gray(stats.unchanged)}`);
        lines.push(`  ${chalk.red(this.symbols.error)} Deleted: ${chalk.red(stats.deleted)}`);
        lines.push(`  ${chalk.gray('-')} Skipped: ${chalk.gray(stats.skipped)}`);
        if (stats.deferred !== undefined && stats.deferred > 0) {
            lines.push(`  ${chalk.yellow('⏳')} Deferred: ${chalk.yellow(stats.deferred)}`);
        lines.push(`  Total Records: ${chalk.bold(total)}`);
        lines.push(`  Duration: ${chalk.cyan(this.formatDuration(stats.duration))}`);
        if (stats.errors > 0) {
            lines.push(chalk.red(`  ${this.symbols.error} Errors: ${stats.errors}`));
    private formatHeader(title: string): string {
        const width = 60;
        const line = '═'.repeat(width);
        // MemberJunction branding
        const brandingText = 'MemberJunction Metadata Sync';
        const brandingPadding = Math.floor((width - brandingText.length - 2) / 2);
        const titlePadding = Math.floor((width - title.length - 2) / 2);
        return chalk.blue([
            '║' + ' '.repeat(brandingPadding) + brandingText + ' '.repeat(width - brandingPadding - brandingText.length - 2) + '║',
            '║' + ' '.repeat(titlePadding) + title + ' '.repeat(width - titlePadding - title.length - 2) + '║',
            line
        ].join('\n'));
    private formatSectionHeader(title: string): string {
        return chalk.bold.underline(title);
    private formatSummaryBox(result: ValidationResult): string {
        const width = 50;
        lines.push(chalk.gray('┌' + '─'.repeat(width - 2) + '┐'));
        // Basic stats
            ['Files:', result.summary.totalFiles],
            ['Entities:', result.summary.totalEntities],
            ['Errors:', result.summary.totalErrors],
            ['Warnings:', result.summary.totalWarnings]
        items.forEach(([label, value]) => {
            const color = label === 'Errors:' && numValue > 0 ? chalk.red :
                         label === 'Warnings:' && numValue > 0 ? chalk.yellow :
                         chalk.white;
            const line = `${String(label).padEnd(15)} ${color(String(value))}`;
            lines.push(chalk.gray('│ ') + line.padEnd(width - 4) + chalk.gray(' │'));
        // Add separator
        if (result.errors.length > 0 || result.warnings.length > 0) {
            lines.push(chalk.gray('├' + '─'.repeat(width - 2) + '┤'));
        // Error breakdown by type
            lines.push(chalk.gray('│ ') + chalk.bold('Errors by Type:').padEnd(width - 4) + chalk.gray(' │'));
            const errorsByType = this.getErrorsByType(result.errors);
            for (const [type, count] of Object.entries(errorsByType)) {
                const typeText = `  ${type}:`;
                const countText = chalk.red(count.toString());
                const spaceBetween = width - 4 - typeText.length - count.toString().length;
                const line = typeText + ' '.repeat(spaceBetween) + countText;
                lines.push(chalk.gray('│ ') + line + chalk.gray(' │'));
        // Warning breakdown by type
            lines.push(chalk.gray('│ ') + chalk.bold('Warnings by Type:').padEnd(width - 4) + chalk.gray(' │'));
            const warningsByType = this.getWarningsByType(result.warnings);
            for (const [type, count] of Object.entries(warningsByType)) {
                const countText = chalk.yellow(count.toString());
        lines.push(chalk.gray('└' + '─'.repeat(width - 2) + '┘'));
    private formatFileResult(file: string, result: FileValidationResult, verbose: boolean): string {
        const hasErrors = result.errors.length > 0;
        const hasWarnings = result.warnings.length > 0;
        const icon = hasErrors ? chalk.red(this.symbols.error) :
                    hasWarnings ? chalk.yellow(this.symbols.warning) :
                    chalk.green(this.symbols.success);
        const shortPath = this.shortenPath(file);
        lines.push(`${icon} ${chalk.bold(shortPath)}`);
        if (verbose || hasErrors || hasWarnings) {
            lines.push(`  ${chalk.gray(`Entities: ${result.entityCount}`)}`);
            if (hasErrors) {
                lines.push(`  ${chalk.red(`Errors: ${result.errors.length}`)}`);
            if (hasWarnings) {
                lines.push(`  ${chalk.yellow(`Warnings: ${result.warnings.length}`)}`);
    private formatError(error: ValidationError, index: number): string {
        lines.push(chalk.red(`${index}. ${error.message}`));
        if (error.entity) {
            lines.push(chalk.gray(`   Entity: ${error.entity}`));
        if (error.field) {
            lines.push(chalk.gray(`   Field: ${error.field}`));
        lines.push(chalk.gray(`   File: ${this.shortenPath(error.file)}`));
        if (error.suggestion) {
            lines.push(chalk.cyan(`   ${this.symbols.arrow} Suggestion: ${error.suggestion}`));
    private formatWarning(warning: ValidationWarning, index: number): string {
        lines.push(chalk.yellow(`${index}. ${warning.message}`));
        if (warning.entity) {
            lines.push(chalk.gray(`   Entity: ${warning.entity}`));
        if (warning.field) {
            lines.push(chalk.gray(`   Field: ${warning.field}`));
        lines.push(chalk.gray(`   File: ${this.shortenPath(warning.file)}`));
        if (warning.suggestion) {
            lines.push(chalk.cyan(`   ${this.symbols.arrow} Suggestion: ${warning.suggestion}`));
    private formatFooter(result: ValidationResult): string {
        if (result.isValid) {
            lines.push(chalk.green.bold(`${this.symbols.success} Validation passed!`));
            lines.push(chalk.red.bold(`${this.symbols.error} Validation failed with ${result.errors.length} error(s)`));
        // Add documentation link if there are any issues
            lines.push(chalk.gray('For help resolving issues, see:'));
            lines.push(chalk.cyan('https://github.com/MemberJunction/MJ/tree/next/packages/MetadataSync'));
    private shortenPath(filePath: string): string {
        const cwd = process.cwd();
        if (filePath.startsWith(cwd)) {
            return '.' + filePath.slice(cwd.length);
    private formatDuration(ms: number): string {
        return `${Math.floor(ms / 60000)}m ${Math.floor((ms % 60000) / 1000)}s`;
     * Get count of errors by type
    private getErrorsByType(errors: ValidationError[]): Record<string, number> {
        const counts: Record<string, number> = {};
        for (const error of errors) {
            counts[error.type] = (counts[error.type] || 0) + 1;
     * Get count of warnings by type
    private getWarningsByType(warnings: ValidationWarning[]): Record<string, number> {
            counts[warning.type] = (counts[warning.type] || 0) + 1;
     * Format validation result as markdown
    public formatValidationResultAsMarkdown(result: ValidationResult): string {
        const timestamp = new Date();
        const dateStr = timestamp.toLocaleDateString('en-US', { 
        const timeStr = timestamp.toLocaleTimeString('en-US', { 
        // Header with branding
        lines.push('# 🚀 MemberJunction Metadata Sync');
        lines.push('## Validation Report');
        lines.push(`📅 **Date:** ${dateStr}  `);
        lines.push(`🕐 **Time:** ${timeStr}  `);
        lines.push(`📍 **Directory:** \`${process.cwd()}\`  `);
        // Table of Contents
        lines.push('## 📑 Table of Contents');
        lines.push('- [Executive Summary](#executive-summary)');
        lines.push('- [Validation Results](#validation-results)');
            lines.push('- [Issue Analysis](#issue-analysis)');
        lines.push('- [File-by-File Breakdown](#file-by-file-breakdown)');
            lines.push('- [Error Details](#error-details)');
            lines.push('- [Warning Details](#warning-details)');
        lines.push('- [Next Steps](#next-steps)');
        lines.push('- [Resources](#resources)');
        // Executive Summary
        lines.push('## 📊 Executive Summary');
        const statusEmoji = result.isValid ? '✅' : '❌';
        const statusText = result.isValid ? 'PASSED' : 'FAILED';
        const statusColor = result.isValid ? 'green' : 'red';
        lines.push(`### Overall Status: ${statusEmoji} **${statusText}**`);
            lines.push('> 🎉 **Congratulations!** Your metadata validation passed with no errors.');
            lines.push(`> ⚠️ **Action Required:** ${result.errors.length} error(s) need to be resolved before proceeding.`);
        // Quick Stats
        lines.push('### 📈 Quick Statistics');
        lines.push(`**📁 Files Validated:** ${result.summary.totalFiles}  `);
        lines.push(`**📦 Entities Processed:** ${result.summary.totalEntities}  `);
        lines.push(`**❌ Errors Found:** ${result.summary.totalErrors}  `);
        lines.push(`**⚠️ Warnings Found:** ${result.summary.totalWarnings}  `);
        // Validation Results
        lines.push('## 🔍 Validation Results');
        // Issue Analysis
            lines.push('## 📊 Issue Analysis');
                lines.push('### ❌ Error Distribution');
                lines.push('<details>');
                lines.push('<summary>Click to expand error breakdown</summary>');
                    const percentage = ((count / result.errors.length) * 100).toFixed(1);
                    lines.push(`- **${type}**: ${count} errors (${percentage}%)`);
                lines.push('</details>');
                lines.push('### ⚠️ Warning Distribution');
                lines.push('<summary>Click to expand warning breakdown</summary>');
                    const percentage = ((count / result.warnings.length) * 100).toFixed(1);
                    lines.push(`- **${type}**: ${count} warnings (${percentage}%)`);
        // File Results
        lines.push('## 📁 File-by-File Breakdown');
        const sortedFiles = Array.from(result.summary.fileResults.entries())
            .sort(([a], [b]) => {
                // Sort by error count (descending), then warning count, then name
                const aResult = result.summary.fileResults.get(a)!;
                const bResult = result.summary.fileResults.get(b)!;
                if (aResult.errors.length !== bResult.errors.length) {
                    return bResult.errors.length - aResult.errors.length;
                if (aResult.warnings.length !== bResult.warnings.length) {
                    return bResult.warnings.length - aResult.warnings.length;
        for (const [file, fileResult] of sortedFiles) {
            const hasErrors = fileResult.errors.length > 0;
            const hasWarnings = fileResult.warnings.length > 0;
            const icon = hasErrors ? '❌' : hasWarnings ? '⚠️' : '✅';
            const status = hasErrors ? 'Has Errors' : hasWarnings ? 'Has Warnings' : 'Clean';
            lines.push(`<details>`);
            lines.push(`<summary>${icon} <strong>${shortPath}</strong> - ${status}</summary>`);
            lines.push('#### File Statistics');
            lines.push(`- **Entities:** ${fileResult.entityCount}`);
            lines.push(`- **Errors:** ${fileResult.errors.length}`);
            lines.push(`- **Warnings:** ${fileResult.warnings.length}`);
                lines.push('#### Errors in this file:');
                fileResult.errors.forEach((error, idx) => {
                    lines.push(`${idx + 1}. ${error.message}`);
                lines.push('#### Warnings in this file:');
                fileResult.warnings.forEach((warning, idx) => {
                    lines.push(`${idx + 1}. ${warning.message}`);
        // Detailed Errors
            lines.push('## ❌ Error Details');
            lines.push(`> Found ${result.errors.length} error(s) that must be fixed.`);
                lines.push(`### Error ${index + 1}: ${error.message}`);
                lines.push(`**Type:** \`${error.type}\`  `);
                if (error.entity) lines.push(`**Entity:** ${error.entity}  `);
                if (error.field) lines.push(`**Field:** \`${error.field}\`  `);
                lines.push(`**File:** \`${this.shortenPath(error.file)}\`  `);
                lines.push(`**Severity:** ${error.severity}  `);
                    lines.push('> 💡 **Suggestion:** ' + error.suggestion);
        // Detailed Warnings
            lines.push('## ⚠️ Warning Details');
            lines.push(`> Found ${result.warnings.length} warning(s) for your review.`);
            // Group warnings by type for better organization
            const warningsByType = new Map<string, ValidationWarning[]>();
            result.warnings.forEach(warning => {
                if (!warningsByType.has(warning.type)) {
                    warningsByType.set(warning.type, []);
                warningsByType.get(warning.type)!.push(warning);
            for (const [type, warnings] of warningsByType) {
                lines.push(`### Warning Type: \`${type}\``);
                warnings.forEach((warning, index) => {
                    lines.push(`#### ${index + 1}. ${warning.message}`);
                    if (warning.entity) lines.push(`**Entity:** ${warning.entity}  `);
                    if (warning.field) lines.push(`**Field:** \`${warning.field}\`  `);
                    lines.push(`**File:** \`${this.shortenPath(warning.file)}\`  `);
                        lines.push('> 💡 **Suggestion:** ' + warning.suggestion);
        // Next Steps
        lines.push('## 🚀 Next Steps');
            lines.push('### To fix errors:');
            lines.push('1. Review each error in the [Error Details](#error-details) section');
            lines.push('2. Follow the suggestions provided for each error');
            lines.push('3. Run validation again after making changes');
            lines.push('4. Repeat until all errors are resolved');
            lines.push('### To address warnings:');
            lines.push('1. Review warnings in the [Warning Details](#warning-details) section');
            lines.push('2. Determine which warnings are relevant to your use case');
            lines.push('3. Apply suggested fixes where appropriate');
            lines.push('Your metadata is valid and ready to sync! 🎉');
            lines.push('```bash');
            lines.push('# Push your metadata to the database');
            lines.push('mj-sync push');
        // Resources
        lines.push('## 📚 Resources');
        lines.push('- 📖 [MetadataSync Documentation](https://github.com/MemberJunction/MJ/tree/next/packages/MetadataSync)');
        lines.push('- 🐛 [Report Issues](https://github.com/MemberJunction/MJ/issues)');
        lines.push('- 💬 [MemberJunction Community](https://memberjunction.org)');
        lines.push('- 📝 [Validation Rules Guide](https://github.com/MemberJunction/MJ/tree/next/packages/MetadataSync#validation-features)');
        lines.push('<div align="center">');
        lines.push('**Generated by [MemberJunction](https://memberjunction.org) Metadata Sync**');
        lines.push(`<sub>${timestamp.toISOString()}</sub>`);
