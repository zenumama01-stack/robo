 * SQL Server implementation of the BaseAutoDocDriver
 * Uses mssql driver for database connectivity
 * SQL Server driver implementation
@RegisterClass(BaseAutoDocDriver, 'SQLServer')
export class SQLServerDriver extends BaseAutoDocDriver {
  private pool: sql.ConnectionPool | null = null;
  private sqlConfig: sql.config;
    // Map generic config to SQL Server specific config
    this.sqlConfig = {
      server: config.host,
      port: config.port || 1433,
        encrypt: config.encrypt ?? true,
        trustServerCertificate: config.trustServerCertificate ?? false
      connectionTimeout: config.connectionTimeout ?? 30000,
      requestTimeout: config.requestTimeout ?? 30000,
    this.pool = await sql.connect(this.sqlConfig);
          @@VERSION as version,
          DB_NAME() as db
      await this.pool.close();
          // Log error and continue with next table instead of failing entire analysis
            `Failed to introspect ${schemaName}.${tableName}: ${(error as Error).message}`
          console.error('  This table will be skipped. Continuing with remaining tables...');
        c.name as column_name,
        t.name as data_type,
        c.is_nullable,
        c.max_length,
        c.precision,
        c.scale,
        CAST(CASE WHEN pk.column_id IS NOT NULL THEN 1 ELSE 0 END AS BIT) as is_primary_key,
        CAST(CASE WHEN fk.parent_column_id IS NOT NULL THEN 1 ELSE 0 END AS BIT) as is_foreign_key,
        cc.definition as check_constraint,
        dc.definition as default_value
      FROM sys.columns c
      INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
      INNER JOIN sys.tables tbl ON c.object_id = tbl.object_id
      INNER JOIN sys.schemas s ON tbl.schema_id = s.schema_id
        SELECT ic.object_id, ic.column_id
        FROM sys.index_columns ic
        INNER JOIN sys.indexes i ON ic.object_id = i.object_id AND ic.index_id = i.index_id
        WHERE i.is_primary_key = 1
      ) pk ON c.object_id = pk.object_id AND c.column_id = pk.column_id
      LEFT JOIN sys.foreign_key_columns fk ON c.object_id = fk.parent_object_id AND c.column_id = fk.parent_column_id
      LEFT JOIN sys.check_constraints cc ON c.object_id = cc.parent_object_id AND cc.parent_column_id = c.column_id
      LEFT JOIN sys.default_constraints dc ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
      WHERE s.name = '${schemaName}' AND tbl.name = '${tableName}'
      ORDER BY c.column_id
      precision: number;
      maxLength: row.max_length > 0 ? row.max_length : undefined,
      precision: row.precision > 0 ? row.precision : undefined,
      scale: row.scale > 0 ? row.scale : undefined
        ISNULL(c.name, '') as column_name,
        CAST(ep.value AS NVARCHAR(MAX)) as description
      FROM sys.extended_properties ep
      INNER JOIN sys.tables t ON ep.major_id = t.object_id
      INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
      LEFT JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id
      WHERE ep.name = 'MS_Description'
        AND s.name = '${schemaName}'
        AND t.name = '${tableName}'
    return result.success && result.data && result.data.length > 0 ? result.data[0].distinct_count : 0;
      SELECT TOP ${limit}
    return result.success && result.data ? result.data : [];
    // **IMPORTANT**: Limit sample size to max 20 to reduce JSON size
    // User requested: "narrow that down to maybe 10-20 values randomly selected from each col"
      SELECT TOP ${limitedSampleSize} ${this.escapeIdentifier(columnName)} as value
      ORDER BY NEWID()
        const result = await this.pool!.request().query(query);
          data: result.recordset as T[],
          rowCount: result.rowsAffected[0]
    return `[${identifier}]`;
    return `TOP ${limit}`;
    let whereClause = 'WHERE t.is_ms_shipped = 0';
    whereClause += this.buildSchemaFilterClause(schemaFilter, 's.name');
    whereClause += this.buildTableFilterClause(tableFilter, 't.name');
        s.name as schema_name,
        t.name as table_name,
        ISNULL(p.rows, 0) as row_count
      LEFT JOIN sys.partitions p ON t.object_id = p.object_id AND p.index_id IN (0, 1)
      ORDER BY s.name, t.name
        rs.name as referenced_schema,
        rt.name as referenced_table,
        rc.name as referenced_column,
        fkc.name as constraint_name
      FROM sys.foreign_key_columns fk
      INNER JOIN sys.foreign_keys fkc ON fk.constraint_object_id = fkc.object_id
      INNER JOIN sys.tables t ON fk.parent_object_id = t.object_id
      INNER JOIN sys.columns c ON fk.parent_object_id = c.object_id AND fk.parent_column_id = c.column_id
      INNER JOIN sys.tables rt ON fk.referenced_object_id = rt.object_id
      INNER JOIN sys.schemas rs ON rt.schema_id = rs.schema_id
      INNER JOIN sys.columns rc ON fk.referenced_object_id = rc.object_id AND fk.referenced_column_id = rc.column_id
      WHERE s.name = '${schemaName}' AND t.name = '${tableName}'
        ic.key_ordinal as ordinal_position,
        i.name as constraint_name
      INNER JOIN sys.tables t ON i.object_id = t.object_id
      ORDER BY ic.key_ordinal
      const totalCount = row.total_count || 1;
        distinctCount: row.distinct_count,
        uniquenessRatio: row.distinct_count / totalCount,
        nullCount: row.null_count,
        nullPercentage: (row.null_count / totalCount) * 100,
        AVG(CAST(${this.escapeIdentifier(columnName)} AS FLOAT)) as avg_value,
        STDEV(${this.escapeIdentifier(columnName)}) as std_dev
        avg: row.avg_value,
        stdDev: row.std_dev
        AVG(LEN(${this.escapeIdentifier(columnName)})) as avg_length,
        MAX(LEN(${this.escapeIdentifier(columnName)})) as max_length,
        MIN(LEN(${this.escapeIdentifier(columnName)})) as min_length
        avgLength: row.avg_length,
      'transport'
        c.name,
        t.name as type,
        c.is_nullable as nullable
      WHERE s.name = '${schemaName}'
        AND tbl.name = '${tableName}'
        AND c.name = '${columnName}'
          SELECT DISTINCT TOP ${sampleSize}
            ${this.escapeIdentifier(sourceColumn)} as value
      if (row.total_source === 0) {
      return row.matching_count / row.total_source;
          SELECT TOP ${sampleSize}
      return result.data[0].duplicate_count === 0;
