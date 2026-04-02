 * Generates standalone Mermaid diagram files for ERD
export interface MermaidGeneratorOptions {
  includeComments?: boolean;
export class MermaidGenerator {
   * Generate Mermaid ERD diagram
    options: MermaidGeneratorOptions = {}
    // Header comments
    if (options.includeComments !== false) {
      lines.push(`%% Entity Relationship Diagram`);
      lines.push(`%% Database: ${state.database.name}`);
      lines.push(`%% Server: ${state.database.server}`);
      lines.push(`%% Generated: ${new Date().toISOString()}`);
      lines.push(`%% Total Schemas: ${state.schemas.length}`);
      lines.push(`%% Total Tables: ${state.summary.totalTables}`);
      lines.push(`%% Total Columns: ${state.summary.totalColumns}`);
    // Start ERD
    // Process each schema
        lines.push(`    %% Schema: ${schema.name}`);
          lines.push(`    %% ${schema.description}`);
      // Process all tables
      this.appendSchemaEntities(lines, schema, options);
      lines.push(`    %% Relationships`);
    this.appendRelationships(lines, state, options);
   * Append entity definitions for a schema
  private appendSchemaEntities(
    schema: any,
    options: MermaidGeneratorOptions
        // Include description as comment if available
        if (options.includeComments !== false && column.description) {
          const description = column.description.replace(/"/g, '\\"').substring(0, 60);
          lines.push(`        ${type} ${column.name}${constraintStr} %% ${description}`);
   * Append relationship definitions
  private appendRelationships(
    const relationships: Set<string> = new Set();
            // Check if dependent table passes filters
            if (this.tablePassesFilters(state, dep.schema, dep.table, options)) {
              const key = `${dep.table}||--o{${table.name}`;
              if (!relationships.has(key)) {
                relationships.add(key);
    // Add comments for complex relationships
    if (options.includeComments !== false && relationships.size > 0) {
      lines.push(`    %% Relationship Notation:`);
      lines.push(`    %% Parent ||--o{ Child : "describes"`);
      lines.push(`    %% One Parent can have zero or more Children`);
   * Check if a table passes the configured filters
  private tablePassesFilters(
    if (!schema) return false;
    if (!table) return false;
   * Generate an HTML-wrapped version of the Mermaid diagram
   * Useful for standalone rendering
  public generateHtml(
    const mermaidDiagram = this.generate(state, options);
    <title>Database ERD - ${this.escapeHtml(state.database.name)}</title>
    <script src="https://cdn.jsdelivr.net/npm/mermaid/dist/mermaid.min.js"><\/script>
        .metadata {
        .metadata p {
            margin: 0.3rem 0;
            margin: 2rem 0;
        .controls {
            padding: 0.6rem 1.2rem;
        button:hover {
        .download-link {
        .download-link:hover {
        <h1>Database Entity Relationship Diagram</h1>
            <p><strong>Database:</strong> ${this.escapeHtml(state.database.name)}</p>
            <p><strong>Server:</strong> ${this.escapeHtml(state.database.server)}</p>
            <p><strong>Generated:</strong> ${new Date().toLocaleString()}</p>
            <p><strong>Tables:</strong> ${state.summary.totalTables} | <strong>Columns:</strong> ${state.summary.totalColumns}</p>
        <div class="controls">
            <button onclick="downloadMmd()">Download .mmd File</button>
            <button onclick="window.print()">Print / Save as PDF</button>
        <div class="mermaid">
${mermaidDiagram}
        mermaid.initialize({ startOnLoad: true, theme: "default", securityLevel: "loose" });
        mermaid.contentLoaded();
        function downloadMmd() {
            const content = \`${this.escapeLiteral(mermaidDiagram)}\`;
            const element = document.createElement('a');
            element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(content));
            element.setAttribute('download', '${state.database.name.replace(/\\s+/g, '_')}_erd.mmd');
            element.style.display = 'none';
            document.body.appendChild(element);
            element.click();
            document.body.removeChild(element);
  private escapeLiteral(text: string): string {
    return text.replace(/`/g, '\\`').replace(/\$/g, '\\$');
