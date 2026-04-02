 * PostgreSQL implementation of the BaseAutoDocDriver
 * Uses pg driver for database connectivity
import { Pool, PoolClient, PoolConfig } from 'pg';
  AutoDocValueDistribution,
 * PostgreSQL driver implementation
@RegisterClass(BaseAutoDocDriver, 'PostgreSQL')
export class PostgreSQLDriver extends BaseAutoDocDriver {
  private pool: Pool | null = null;
  private pgConfig: PoolConfig;
    // Map generic config to PostgreSQL specific config
    this.pgConfig = {
      port: config.port || 5432,
      ssl: config.ssl,
      connectionTimeoutMillis: config.connectionTimeout ?? 30000,
      max: config.maxConnections ?? 10,
      min: config.minConnections ?? 0,
      idleTimeoutMillis: config.idleTimeoutMillis ?? 30000
    this.pool = new Pool(this.pgConfig);
          version() as version,
          current_database() as db
        c.column_name,
        c.data_type,
        c.udt_name,
        CASE WHEN c.is_nullable = 'YES' THEN true ELSE false END as is_nullable,
        c.character_maximum_length,
        c.numeric_precision,
        c.numeric_scale,
        c.column_default,
        CASE WHEN pk.column_name IS NOT NULL THEN true ELSE false END as is_primary_key,
        CASE WHEN fk.column_name IS NOT NULL THEN true ELSE false END as is_foreign_key,
        cc.check_clause as check_constraint
      FROM information_schema.columns c
          kcu.table_schema,
          kcu.table_name,
          kcu.column_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu
          ON tc.constraint_name = kcu.constraint_name
          AND tc.table_schema = kcu.table_schema
        WHERE tc.constraint_type = 'PRIMARY KEY'
      ) pk ON c.table_schema = pk.table_schema
        AND c.table_name = pk.table_name
        AND c.column_name = pk.column_name
        WHERE tc.constraint_type = 'FOREIGN KEY'
      ) fk ON c.table_schema = fk.table_schema
        AND c.table_name = fk.table_name
        AND c.column_name = fk.column_name
          ccu.table_schema,
          ccu.table_name,
          ccu.column_name,
          cc.check_clause
        FROM information_schema.check_constraints cc
        JOIN information_schema.constraint_column_usage ccu
          ON cc.constraint_name = ccu.constraint_name
          AND cc.constraint_schema = ccu.constraint_schema
      ) cc ON c.table_schema = cc.table_schema
        AND c.table_name = cc.table_name
        AND c.column_name = cc.column_name
      WHERE c.table_schema = $1 AND c.table_name = $2
      ORDER BY c.ordinal_position
      udt_name: string;
      is_nullable: boolean;
      character_maximum_length: number | null;
      numeric_precision: number | null;
      numeric_scale: number | null;
      column_default: string | null;
      is_primary_key: boolean;
      is_foreign_key: boolean;
      check_constraint: string | null;
      dataType: this.normalizeDataType(row.data_type, row.udt_name),
      isNullable: row.is_nullable,
      isPrimaryKey: row.is_primary_key,
      isForeignKey: row.is_foreign_key,
      checkConstraint: row.check_constraint || undefined,
      defaultValue: row.column_default || undefined,
      maxLength: row.character_maximum_length || undefined,
      precision: row.numeric_precision || undefined,
      scale: row.numeric_scale || undefined
    // PostgreSQL uses pg_description for comments
        COALESCE(a.attname, '') as column_name,
        d.description
      FROM pg_description d
      JOIN pg_class c ON d.objoid = c.oid
      JOIN pg_namespace n ON c.relnamespace = n.oid
      LEFT JOIN pg_attribute a ON d.objoid = a.attrelid AND d.objsubid = a.attnum
      WHERE n.nspname = $1
        AND c.relname = $2
        AND d.description IS NOT NULL
      target: row.column_name ? 'column' : 'table',
      ORDER BY RANDOM()
        const result = params
          ? await this.pool!.query(query, params)
          data: result.rows as T[],
          rowCount: result.rowCount || 0
    return `"${identifier}"`;
    let whereClause = "WHERE t.table_type = 'BASE TABLE'";
    whereClause += this.buildSchemaFilterClause(schemaFilter, 't.table_schema');
    whereClause += this.buildTableFilterClause(tableFilter, 't.table_name');
    // Exclude system schemas by default
    if (!schemaFilter.include || schemaFilter.include.length === 0) {
      whereClause += ` AND t.table_schema NOT IN ('pg_catalog', 'information_schema')`;
        t.table_schema as schema_name,
        t.table_name as table_name,
        COALESCE(s.n_live_tup, 0) as row_count
      FROM information_schema.tables t
      LEFT JOIN pg_stat_user_tables s
        ON t.table_schema = s.schemaname
        AND t.table_name = s.relname
      ORDER BY t.table_schema, t.table_name
        kcu.column_name,
        ccu.table_schema as referenced_schema,
        ccu.table_name as referenced_table,
        ccu.column_name as referenced_column,
        tc.constraint_name
        ON ccu.constraint_name = tc.constraint_name
        AND ccu.table_schema = tc.table_schema
        AND tc.table_schema = $1
        AND tc.table_name = $2
        kcu.ordinal_position,
      ORDER BY kcu.ordinal_position
      ordinalPosition: row.ordinal_position,
        COUNT(*) - COUNT(${this.escapeIdentifier(columnName)}) as null_count
      distinct_count: string;
      total_count: string;
      null_count: string;
        AVG(${this.escapeIdentifier(columnName)}::NUMERIC) as avg_value,
      avg_value: string;
      std_dev: string;
        avg: row.avg_value ? Number(row.avg_value) : undefined,
        stdDev: row.std_dev ? Number(row.std_dev) : undefined
      avg_length: string;
        avgLength: row.avg_length ? Number(row.avg_length) : undefined,
        maxLength: row.max_length,
        minLength: row.min_length
      'ETIMEDOUT'
    return transientMessages.some(msg => message.includes(msg.toLowerCase()));
   * Normalize PostgreSQL data types to generic format
  private normalizeDataType(dataType: string, udtName: string): string {
    // Map PostgreSQL types to more generic names
      'character varying': 'varchar',
      'character': 'char',
      'timestamp without time zone': 'timestamp',
      'timestamp with time zone': 'timestamptz',
      'time without time zone': 'time',
      'time with time zone': 'timetz',
      'double precision': 'float8',
      'integer': 'int4',
      'bigint': 'int8',
      'smallint': 'int2'
    return typeMap[dataType.toLowerCase()] || udtName || dataType;
        column_name as name,
        data_type as type,
        CASE WHEN is_nullable = 'YES' THEN true ELSE false END as nullable
      FROM information_schema.columns
      WHERE table_schema = $1
        AND table_name = $2
        AND column_name = $3
      nullable: boolean;
      nullable: result.data[0].nullable
        WITH source_sample AS (
        target_values AS (
        FROM source_sample ss
        LEFT JOIN target_values tv ON ss.value = tv.value
        total_source: string;
        matching_count: string;
      const whereClause = escapedColumns.map(col => `${col} IS NOT NULL`).join(' AND ');
        WITH sampled_data AS (
        grouped_data AS (
          FROM sampled_data
        FROM grouped_data
      const result = await this.executeQuery<{ duplicate_count: string }>(query);
