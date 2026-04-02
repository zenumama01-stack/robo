 * Generates Markdown documentation
export class MarkdownGenerator {
   * Generate markdown documentation
  public generate(state: DatabaseDocumentation): string {
    lines.push(`# Database Documentation: ${state.database.name}`);
    lines.push(`**Server**: ${state.database.server}`);
    lines.push(`**Generated**: ${new Date().toISOString()}`);
    lines.push(`**Total Iterations**: ${state.summary.totalIterations}`);
      lines.push('## Analysis Summary');
      lines.push(`- **Status**: ${lastRun.status}`);
      lines.push(`- **Iterations**: ${lastRun.iterationsPerformed}`);
      lines.push(`- **Tokens Used**: ${lastRun.totalTokensUsed.toLocaleString()}`);
      lines.push(`- **Estimated Cost**: $${lastRun.estimatedCost.toFixed(2)}`);
      lines.push(`- **AI Model**: ${lastRun.modelUsed}`);
      lines.push(`- **AI Vendor**: ${lastRun.vendor}`);
      lines.push(`- **Temperature**: ${lastRun.temperature}`);
      if (lastRun.topP !== undefined) {
        lines.push(`- **Top P**: ${lastRun.topP}`);
      if (lastRun.topK !== undefined) {
        lines.push(`- **Top K**: ${lastRun.topK}`);
        lines.push(`- **Convergence**: ${lastRun.convergenceReason}`);
    // Seed context
    if (state.seedContext) {
      lines.push('## Database Context');
        lines.push(`**Purpose**: ${state.seedContext.overallPurpose}`);
        lines.push(`**Industry**: ${state.seedContext.industryContext}`);
        lines.push(`**Business Domains**: ${state.seedContext.businessDomains.join(', ')}`);
    // Table of contents - Schemas
    lines.push('## Table of Contents');
      lines.push(`### [${schema.name}](#schema-${this.toAnchor(schema.name)}) (${schema.tables.length} tables)`);
        lines.push(`- [${table.name}](#${this.toAnchor(table.name)})`);
      lines.push(`## Schema: ${schema.name}`);
        lines.push(schema.description);
        lines.push(`**Purpose**: ${schema.inferredPurpose}`);
        lines.push(`**Business Domains**: ${schema.businessDomains.join(', ')}`);
      lines.push('### Entity Relationship Diagram');
      lines.push('```mermaid');
      lines.push('### Tables');
        lines.push(`#### ${table.name}`);
          lines.push(table.description);
        lines.push(`**Row Count**: ${table.rowCount.toLocaleString()}`);
          lines.push(`**Dependency Level**: ${table.dependencyLevel}`);
            lines.push(`**Confidence**: ${(latest.confidence * 100).toFixed(0)}%`);
          lines.push('**Depends On**:');
            const link = `[${dep.schema}.${dep.table}](#${this.toAnchor(dep.table)})`;
            lines.push(`- ${link} (via ${dep.column})`);
          lines.push('**Referenced By**:');
            lines.push(`- ${link}`);
        lines.push('**Columns**:');
        lines.push('| Column | Type | Description |');
        lines.push('|--------|------|-------------|');
          if (column.isPrimaryKey) flags.push('PK');
          if (column.isForeignKey) flags.push('FK');
          if (!column.isNullable) flags.push('NOT NULL');
          const typeInfo = flags.length > 0 ? `${column.dataType} (${flags.join(', ')})` : column.dataType;
          lines.push(`| ${column.name} | ${typeInfo} | ${description} |`);
    // Iteration Analysis Appendix
    lines.push('## Appendix: Iteration Analysis');
    lines.push('This section documents the iterative refinement process used to generate the database documentation, highlighting corrections and improvements discovered through backpropagation.');
    // Generate summary statistics
    const iterationStats = this.collectIterationStats(state);
    if (iterationStats.totalRefinements > 0) {
      lines.push('### Summary');
      lines.push(`- **Total Tables with Refinements**: ${iterationStats.tablesWithRefinements}`);
      lines.push(`- **Total Columns with Refinements**: ${iterationStats.columnsWithRefinements}`);
      lines.push(`- **Total Refinement Iterations**: ${iterationStats.totalRefinements}`);
      lines.push(`- **Refinements Triggered by Backpropagation**: ${iterationStats.backpropRefinements}`);
      // List tables that were refined
      lines.push('### Tables Refined Through Iteration');
          if (table.descriptionIterations && table.descriptionIterations.length > 1) {
            const iterations = table.descriptionIterations.length;
            const lastIteration = table.descriptionIterations[iterations - 1];
            const triggeredBy = lastIteration.triggeredBy || 'unknown';
            lines.push(`#### [${table.name}](#${this.toAnchor(table.name)})`);
            lines.push(`**Iterations**: ${iterations} | **Trigger**: ${triggeredBy}`);
            // Show the evolution of the description
            for (let i = 0; i < table.descriptionIterations.length; i++) {
              const iter = table.descriptionIterations[i];
              lines.push(`**Iteration ${i + 1}** (${iter.triggeredBy || 'initial'}):`);
              lines.push(`> ${iter.description}`);
              if (iter.reasoning) {
                lines.push(`*Reasoning*: ${iter.reasoning}`);
              if (i < table.descriptionIterations.length - 1) {
      // Mermaid sequence diagram
      lines.push('### Iteration Process Visualization');
      lines.push('The following diagram illustrates the analysis workflow and highlights where corrections were made through backpropagation:');
      lines.push(this.generateIterationSequenceDiagram(state));
      lines.push('No iterative refinements were needed - all descriptions were accepted on first analysis.');
   * Generate Mermaid ERD diagram for a schema
          // Format: ParentTable ||--o{ ChildTable : "relationship"
   * Convert a string to a markdown anchor-friendly format
   * Collect iteration statistics from the state
  private collectIterationStats(state: DatabaseDocumentation): {
    tablesWithRefinements: number;
    columnsWithRefinements: number;
    totalRefinements: number;
    backpropRefinements: number;
    let tablesWithRefinements = 0;
    let columnsWithRefinements = 0;
    let totalRefinements = 0;
    let backpropRefinements = 0;
        // Check table iterations
          tablesWithRefinements++;
          totalRefinements += table.descriptionIterations.length - 1;
          // Count backprop triggers
          for (let i = 1; i < table.descriptionIterations.length; i++) {
            if (table.descriptionIterations[i].triggeredBy === 'backpropagation') {
              backpropRefinements++;
        // Check column iterations
          if (column.descriptionIterations && column.descriptionIterations.length > 1) {
            columnsWithRefinements++;
            totalRefinements += column.descriptionIterations.length - 1;
            for (let i = 1; i < column.descriptionIterations.length; i++) {
              if (column.descriptionIterations[i].triggeredBy === 'backpropagation') {
      tablesWithRefinements,
      columnsWithRefinements,
      totalRefinements,
      backpropRefinements
   * Generate a Mermaid sequence diagram showing the iteration process
  private generateIterationSequenceDiagram(state: DatabaseDocumentation): string {
    lines.push('sequenceDiagram');
    lines.push('    participant User');
    lines.push('    participant Analyzer');
    lines.push('    participant AI');
    lines.push('    participant SemanticCheck');
    lines.push('    User->>Analyzer: Start Analysis');
    lines.push('    Analyzer->>AI: Analyze Schema');
    // Track which tables had iterations
    const processedTables: string[] = [];
        if (table.descriptionIterations && table.descriptionIterations.length > 0) {
          // First iteration (initial)
          lines.push(`    AI->>Analyzer: Initial description for ${table.name}`);
          if (iterations > 1) {
            // Subsequent iterations - show refinement
            for (let i = 1; i < iterations; i++) {
              const trigger = iter.triggeredBy || 'unknown';
              if (trigger === 'backpropagation') {
                lines.push(`    Note right of Analyzer: Backpropagation triggered`);
                lines.push(`    Analyzer->>SemanticCheck: Compare iterations for ${table.name}`);
                lines.push(`    SemanticCheck->>Analyzer: Material change detected`);
                lines.push(`    Analyzer->>AI: Refine description (iteration ${i + 1})`);
                lines.push(`    AI->>Analyzer: Updated description`);
                lines.push(`    rect rgb(255, 220, 100)`);
                lines.push(`        Note over Analyzer: ${table.name} refined via backprop`);
                lines.push(`    end`);
            processedTables.push(table.name);
    lines.push('    Analyzer->>User: Analysis Complete');
    if (processedTables.length > 0) {
      lines.push('    Note over User,AI: Tables refined: ' + processedTables.join(', '));
