"""Schema processing for discovery based APIs
 schema = \"\"\"{
   "Foo": {
     "etag": {
      "description": "ETag of the collection."
     "kind": {
      "description": "Type of the collection ('calendar#acl').",
      "default": "calendar#acl"
     "nextPageToken": {
      "description": "Token used to access the next
         page of this result. Omitted if no further results are available."
 }\"\"\"
   "nextPageToken": "A String", # Token used to access the
   "kind": "A String", # Type of the collection ('calendar#acl').
   "etag": "A String", # ETag of the collection.
# TODO(jcgregorio) support format, enum, minimum, maximum
class Schemas(object):
    """Schemas for an API."""
    def __init__(self, discovery):
        self.schemas = discovery.get("schemas", {})
        # Cache of pretty printed schemas.
        self.pretty = {}
    def _prettyPrintByName(self, name, seen=None, dent=0):
        """Get pretty printed object prototype from the schema name.
        if seen is None:
            seen = []
        if name in seen:
            # Do not fall into an infinite loop over recursive definitions.
            return "# Object with schema name: %s" % name
        seen.append(name)
        if name not in self.pretty:
            self.pretty[name] = _SchemaToStruct(
                self.schemas[name], seen, dent=dent
            ).to_str(self._prettyPrintByName)
        seen.pop()
        return self.pretty[name]
    def prettyPrintByName(self, name):
        # Return with trailing comma and newline removed.
        return self._prettyPrintByName(name, seen=[], dent=0)[:-2]
    def _prettyPrintSchema(self, schema, seen=None, dent=0):
        """Get pretty printed object prototype of schema.
        return _SchemaToStruct(schema, seen, dent=dent).to_str(self._prettyPrintByName)
    def prettyPrintSchema(self, schema):
        return self._prettyPrintSchema(schema, dent=0)[:-2]
    def get(self, name, default=None):
        """Get deserialized JSON schema from the schema name.
        return self.schemas.get(name, default)
class _SchemaToStruct(object):
    """Convert schema to a prototype object."""
    def __init__(self, schema, seen, dent=0):
        # The result of this parsing kept as list of strings.
        self.value = []
        # The final value of the parsing.
        self.string = None
        # The parsed JSON schema.
        self.schema = schema
        # Indentation level.
        self.dent = dent
        # Method that when called returns a prototype object for the schema with
        # the given name.
        self.from_cache = None
        # List of names of schema already seen while parsing.
        self.seen = seen
    def emit(self, text):
        """Add text as a line to the output.
        self.value.extend(["  " * self.dent, text, "\n"])
    def emitBegin(self, text):
        """Add text to the output, but with no line terminator.
        self.value.extend(["  " * self.dent, text])
    def emitEnd(self, text, comment):
        """Add text and comment to the output with line terminator.
        if comment:
            divider = "\n" + "  " * (self.dent + 2) + "# "
            lines = comment.splitlines()
            lines = [x.rstrip() for x in lines]
            comment = divider.join(lines)
            self.value.extend([text, " # ", comment, "\n"])
            self.value.extend([text, "\n"])
    def indent(self):
        """Increase indentation level."""
        self.dent += 1
    def undent(self):
        """Decrease indentation level."""
        self.dent -= 1
    def _to_str_impl(self, schema):
        """Prototype object based on the schema, in Python code with comments.
        stype = schema.get("type")
        if stype == "object":
            self.emitEnd("{", schema.get("description", ""))
            self.indent()
            if "properties" in schema:
                properties = schema.get("properties", {})
                sorted_properties = OrderedDict(sorted(properties.items()))
                for pname, pschema in sorted_properties.items():
                    self.emitBegin('"%s": ' % pname)
                    self._to_str_impl(pschema)
            elif "additionalProperties" in schema:
                self.emitBegin('"a_key": ')
                self._to_str_impl(schema["additionalProperties"])
            self.undent()
            self.emit("},")
        elif "$ref" in schema:
            schemaName = schema["$ref"]
            description = schema.get("description", "")
            s = self.from_cache(schemaName, seen=self.seen)
            parts = s.splitlines()
            self.emitEnd(parts[0], description)
            for line in parts[1:]:
                self.emit(line.rstrip())
        elif stype == "boolean":
            value = schema.get("default", "True or False")
            self.emitEnd("%s," % str(value), schema.get("description", ""))
        elif stype == "string":
            value = schema.get("default", "A String")
            self.emitEnd('"%s",' % str(value), schema.get("description", ""))
        elif stype == "integer":
            value = schema.get("default", "42")
        elif stype == "number":
            value = schema.get("default", "3.14")
        elif stype == "null":
            self.emitEnd("None,", schema.get("description", ""))
        elif stype == "any":
            self.emitEnd('"",', schema.get("description", ""))
        elif stype == "array":
            self.emitEnd("[", schema.get("description"))
            self.emitBegin("")
            self._to_str_impl(schema["items"])
            self.emit("],")
            self.emit("Unknown type! %s" % stype)
            self.emitEnd("", "")
        self.string = "".join(self.value)
        return self.string
    def to_str(self, from_cache):
        self.from_cache = from_cache
        return self._to_str_impl(self.schema)
from django.contrib.gis.db.models import GeometryField
from django.db import OperationalError
from django.db.backends.mysql.schema import DatabaseSchemaEditor
logger = logging.getLogger("django.contrib.gis")
class MySQLGISSchemaEditor(DatabaseSchemaEditor):
    sql_add_spatial_index = "CREATE SPATIAL INDEX %(index)s ON %(table)s(%(column)s)"
    def quote_value(self, value):
        if isinstance(value, self.connection.ops.Adapter):
            return super().quote_value(str(value))
        return super().quote_value(value)
    def _field_indexes_sql(self, model, field):
        if isinstance(field, GeometryField) and field.spatial_index and not field.null:
            with self.connection.cursor() as cursor:
                supports_spatial_index = (
                    self.connection.introspection.supports_spatial_index(
                        cursor, model._meta.db_table
            sql = self._create_spatial_index_sql(model, field)
            if supports_spatial_index:
                return [sql]
                    f"Cannot create SPATIAL INDEX {sql}. Only MyISAM, Aria, and InnoDB "
                    f"support them.",
        return super()._field_indexes_sql(model, field)
    def remove_field(self, model, field):
            sql = self._delete_spatial_index_sql(model, field)
                self.execute(sql)
                    "Couldn't remove spatial index: %s (may be expected "
                    "if your storage engine doesn't support them).",
        super().remove_field(model, field)
    def _alter_field(
        old_field,
        new_field,
        old_type,
        new_type,
        old_db_params,
        new_db_params,
        strict=False,
        super()._alter_field(
            strict=strict,
        old_field_spatial_index = (
            isinstance(old_field, GeometryField)
            and old_field.spatial_index
            and not old_field.null
        new_field_spatial_index = (
            isinstance(new_field, GeometryField)
            and new_field.spatial_index
            and not new_field.null
        if not old_field_spatial_index and new_field_spatial_index:
            self.execute(self._create_spatial_index_sql(model, new_field))
        elif old_field_spatial_index and not new_field_spatial_index:
            self.execute(self._delete_spatial_index_sql(model, old_field))
    def _create_spatial_index_name(self, model, field):
        return "%s_%s_id" % (model._meta.db_table, field.column)
    def _create_spatial_index_sql(self, model, field):
        index_name = self._create_spatial_index_name(model, field)
        qn = self.connection.ops.quote_name
        return self.sql_add_spatial_index % {
            "index": qn(index_name),
            "table": qn(model._meta.db_table),
            "column": qn(field.column),
    def _delete_spatial_index_sql(self, model, field):
        return self._delete_index_sql(model, index_name)
from django.db.backends.oracle.schema import DatabaseSchemaEditor
from django.db.backends.utils import strip_quotes, truncate_name
class OracleGISSchemaEditor(DatabaseSchemaEditor):
    sql_add_geometry_metadata = """
        INSERT INTO USER_SDO_GEOM_METADATA
            ("TABLE_NAME", "COLUMN_NAME", "DIMINFO", "SRID")
            %(table)s,
            %(column)s,
            MDSYS.SDO_DIM_ARRAY(
                MDSYS.SDO_DIM_ELEMENT('LONG', %(dim0)s, %(dim2)s, %(tolerance)s),
                MDSYS.SDO_DIM_ELEMENT('LAT', %(dim1)s, %(dim3)s, %(tolerance)s)
            %(srid)s
        )"""
    sql_add_spatial_index = (
        "CREATE INDEX %(index)s ON %(table)s(%(column)s) "
        "INDEXTYPE IS MDSYS.SPATIAL_INDEX"
    sql_clear_geometry_table_metadata = (
        "DELETE FROM USER_SDO_GEOM_METADATA WHERE TABLE_NAME = %(table)s"
    sql_clear_geometry_field_metadata = (
        "DELETE FROM USER_SDO_GEOM_METADATA WHERE TABLE_NAME = %(table)s "
        "AND COLUMN_NAME = %(column)s"
        self.geometry_sql = []
    def geo_quote_name(self, name):
        return self.connection.ops.geo_quote_name(name)
        if isinstance(field, GeometryField) and field.spatial_index:
            return [self._create_spatial_index_sql(model, field)]
    def column_sql(self, model, field, include_default=False):
        column_sql = super().column_sql(model, field, include_default)
        if isinstance(field, GeometryField):
            self.geometry_sql.append(
                self.sql_add_geometry_metadata
                    "table": self.geo_quote_name(model._meta.db_table),
                    "column": self.geo_quote_name(field.column),
                    "dim0": field._extent[0],
                    "dim1": field._extent[1],
                    "dim2": field._extent[2],
                    "dim3": field._extent[3],
                    "tolerance": field._tolerance,
                    "srid": field.srid,
        return column_sql
    def create_model(self, model):
        super().create_model(model)
        self.run_geometry_sql()
    def delete_model(self, model):
        super().delete_model(model)
        self.execute(
            self.sql_clear_geometry_table_metadata
    def add_field(self, model, field):
        super().add_field(model, field)
                self.sql_clear_geometry_field_metadata
            if field.spatial_index:
                self.execute(self._delete_spatial_index_sql(model, field))
    def run_geometry_sql(self):
        for sql in self.geometry_sql:
            isinstance(old_field, GeometryField) and old_field.spatial_index
            isinstance(new_field, GeometryField) and new_field.spatial_index
        # Oracle doesn't allow object names > 30 characters. Use this scheme
        # instead of self._create_index_name() for backwards compatibility.
        return truncate_name(
            "%s_%s_id" % (strip_quotes(model._meta.db_table), field.column), 30
            "index": self.quote_name(index_name),
            "table": self.quote_name(model._meta.db_table),
            "column": self.quote_name(field.column),
from django.db.backends.postgresql.schema import DatabaseSchemaEditor
from django.db.models.expressions import Col, Func
class PostGISSchemaEditor(DatabaseSchemaEditor):
    geom_index_type = "GIST"
    geom_index_ops_nd = "GIST_GEOMETRY_OPS_ND"
    rast_index_template = "ST_ConvexHull(%(expressions)s)"
    sql_alter_column_to_3d = (
        "ALTER COLUMN %(column)s TYPE %(type)s USING ST_Force3D(%(column)s)::%(type)s"
    sql_alter_column_to_2d = (
        "ALTER COLUMN %(column)s TYPE %(type)s USING ST_Force2D(%(column)s)::%(type)s"
    def _field_should_be_indexed(self, model, field):
        if getattr(field, "spatial_index", False):
        return super()._field_should_be_indexed(model, field)
    def _create_index_sql(self, model, *, fields=None, **kwargs):
        if fields is None or len(fields) != 1 or not hasattr(fields[0], "geodetic"):
            return super()._create_index_sql(model, fields=fields, **kwargs)
        return self._create_spatial_index_sql(model, fields[0], **kwargs)
    def _alter_column_type_sql(
        self, table, old_field, new_field, new_type, old_collation, new_collation
        Special case when dimension changed.
        if not hasattr(old_field, "dim") or not hasattr(new_field, "dim"):
            return super()._alter_column_type_sql(
                table, old_field, new_field, new_type, old_collation, new_collation
        if old_field.dim == 2 and new_field.dim == 3:
            sql_alter = self.sql_alter_column_to_3d
        elif old_field.dim == 3 and new_field.dim == 2:
            sql_alter = self.sql_alter_column_to_2d
            sql_alter = self.sql_alter_column_type
                sql_alter
                    "column": self.quote_name(new_field.column),
                    "type": new_type,
                    "collation": "",
        return self._create_index_name(model._meta.db_table, [field.column], "_id")
    def _create_spatial_index_sql(self, model, field, **kwargs):
        expressions = None
        opclasses = None
        fields = [field]
        if field.geom_type == "RASTER":
            # For raster fields, wrap index creation SQL statement with
            # ST_ConvexHull. Indexes on raster columns are based on the convex
            # hull of the raster.
            expressions = Func(Col(None, field), template=self.rast_index_template)
        elif field.dim > 2 and not field.geography:
            # Use "nd" ops which are fast on multidimensional cases
            opclasses = [self.geom_index_ops_nd]
        if not (name := kwargs.get("name")):
            name = self._create_spatial_index_name(model, field)
        return super()._create_index_sql(
            fields=fields,
            using=" USING %s" % self.geom_index_type,
            opclasses=opclasses,
            expressions=expressions,
from django.db import DatabaseError
from django.db.backends.sqlite3.schema import DatabaseSchemaEditor
class SpatialiteSchemaEditor(DatabaseSchemaEditor):
    sql_add_geometry_column = (
        "SELECT AddGeometryColumn(%(table)s, %(column)s, %(srid)s, "
        "%(geom_type)s, %(dim)s, %(null)s)"
    sql_add_spatial_index = "SELECT CreateSpatialIndex(%(table)s, %(column)s)"
    sql_drop_spatial_index = "DROP TABLE idx_%(table)s_%(column)s"
    sql_recover_geometry_metadata = (
        "SELECT RecoverGeometryColumn(%(table)s, %(column)s, %(srid)s, "
        "%(geom_type)s, %(dim)s)"
    sql_remove_geometry_metadata = "SELECT DiscardGeometryColumn(%(table)s, %(column)s)"
    sql_discard_geometry_columns = (
        "DELETE FROM %(geom_table)s WHERE f_table_name = %(table)s"
    sql_update_geometry_columns = (
        "UPDATE %(geom_table)s SET f_table_name = %(new_table)s "
        "WHERE f_table_name = %(old_table)s"
    geometry_tables = [
        "geometry_columns",
        "geometry_columns_auth",
        "geometry_columns_time",
        "geometry_columns_statistics",
        if not isinstance(field, GeometryField):
            return super().column_sql(model, field, include_default)
        # Geometry columns are created by the `AddGeometryColumn` function
            self.sql_add_geometry_column
                "geom_type": self.geo_quote_name(field.geom_type),
                "dim": field.dim,
                "null": int(not field.null),
                self.sql_add_spatial_index
    def remove_geometry_metadata(self, model, field):
            self.sql_remove_geometry_metadata
            self.sql_drop_spatial_index
                "table": model._meta.db_table,
                "column": field.column,
        # Create geometry columns
    def delete_model(self, model, **kwargs):
        # Drop spatial metadata (dropping the table does not automatically
        # remove them)
        for field in model._meta.local_fields:
                self.remove_geometry_metadata(model, field)
        # Make sure all geom stuff is gone
        for geom_table in self.geometry_tables:
                    self.sql_discard_geometry_columns
                        "geom_table": geom_table,
            except DatabaseError:
        super().delete_model(model, **kwargs)
            # Populate self.geometry_sql
            self.column_sql(model, field)
        # NOTE: If the field is a geometry field, the table is just recreated,
        # the parent's remove_field can't be used cause it will skip the
        # recreation if the field does not have a database type. Geometry
        # fields do not have a db type cause they are added and removed via
        # stored procedures.
            self._remake_table(model, delete_field=field)
    def alter_db_table(self, model, old_db_table, new_db_table):
        if old_db_table == new_db_table or (
            self.connection.features.ignores_table_name_case
            and old_db_table.lower() == new_db_table.lower()
        # Remove geometry-ness from temp table
                        "table": self.quote_name(old_db_table),
        # Alter table
        super().alter_db_table(model, old_db_table, new_db_table)
        # Repoint any straggler names
                    self.sql_update_geometry_columns
                        "old_table": self.quote_name(old_db_table),
                        "new_table": self.quote_name(new_db_table),
        # Re-add geometry-ness and rename spatial index tables
                    self.sql_recover_geometry_metadata
                        "table": self.geo_quote_name(new_db_table),
                    self.sql_rename_table
                        "old_table": self.quote_name(
                            "idx_%s_%s" % (old_db_table, field.column)
                        "new_table": self.quote_name(
                            "idx_%s_%s" % (new_db_table, field.column)
from itertools import chain
from django.core.exceptions import FieldError
from django.db.backends.ddl_references import (
    Columns,
    Expressions,
    ForeignKeyName,
    IndexName,
    Table,
from django.db.backends.utils import names_digest, split_identifier, truncate_name
from django.db.models import Deferrable, Index
from django.db.models.fields.composite import CompositePrimaryKey
from django.db.models.sql import Query
from django.db.transaction import TransactionManagementError, atomic
logger = logging.getLogger("django.db.backends.schema")
def _is_relevant_relation(relation, altered_field):
    When altering the given field, must constraints on its model from the given
    relation be temporarily dropped?
    field = relation.field
    if field.many_to_many:
        # M2M reverse field
    if altered_field.primary_key and field.to_fields == [None]:
        # Foreign key constraint on the primary key, which is being altered.
    # Is the constraint targeting the field being altered?
    return altered_field.name in field.to_fields
def _all_related_fields(model):
    # Related fields must be returned in a deterministic order.
        model._meta._get_fields(
            forward=False,
            reverse=True,
            include_hidden=True,
            include_parents=False,
        key=operator.attrgetter("name"),
def _related_non_m2m_objects(old_field, new_field):
    # Filter out m2m objects from reverse relations.
    # Return (old_relation, new_relation) tuples.
    related_fields = zip(
            obj
            for obj in _all_related_fields(old_field.model)
            if _is_relevant_relation(obj, old_field)
            for obj in _all_related_fields(new_field.model)
            if _is_relevant_relation(obj, new_field)
    for old_rel, new_rel in related_fields:
        yield old_rel, new_rel
        yield from _related_non_m2m_objects(
            old_rel.remote_field,
            new_rel.remote_field,
class BaseDatabaseSchemaEditor:
    This class and its subclasses are responsible for emitting schema-changing
    statements to the databases - model creation/removal/alteration, field
    renaming, index fiddling, and so on.
    # Overrideable SQL templates
    sql_create_table = "CREATE TABLE %(table)s (%(definition)s)"
    sql_rename_table = "ALTER TABLE %(old_table)s RENAME TO %(new_table)s"
    sql_retablespace_table = "ALTER TABLE %(table)s SET TABLESPACE %(new_tablespace)s"
    sql_delete_table = "DROP TABLE %(table)s CASCADE"
    sql_create_column = "ALTER TABLE %(table)s ADD COLUMN %(column)s %(definition)s"
    sql_alter_column = "ALTER TABLE %(table)s %(changes)s"
    sql_alter_column_type = "ALTER COLUMN %(column)s TYPE %(type)s%(collation)s"
    sql_alter_column_null = "ALTER COLUMN %(column)s DROP NOT NULL"
    sql_alter_column_not_null = "ALTER COLUMN %(column)s SET NOT NULL"
    sql_alter_column_default = "ALTER COLUMN %(column)s SET DEFAULT %(default)s"
    sql_alter_column_no_default = "ALTER COLUMN %(column)s DROP DEFAULT"
    sql_alter_column_no_default_null = sql_alter_column_no_default
    sql_delete_column = "ALTER TABLE %(table)s DROP COLUMN %(column)s"
    sql_rename_column = (
        "ALTER TABLE %(table)s RENAME COLUMN %(old_column)s TO %(new_column)s"
    sql_update_with_default = (
        "UPDATE %(table)s SET %(column)s = %(default)s WHERE %(column)s IS NULL"
    sql_unique_constraint = "UNIQUE (%(columns)s)%(deferrable)s"
    sql_check_constraint = "CHECK (%(check)s)"
    sql_delete_constraint = "ALTER TABLE %(table)s DROP CONSTRAINT %(name)s"
    sql_constraint = "CONSTRAINT %(name)s %(constraint)s"
    sql_pk_constraint = "PRIMARY KEY (%(columns)s)"
    sql_create_check = "ALTER TABLE %(table)s ADD CONSTRAINT %(name)s CHECK (%(check)s)"
    sql_delete_check = sql_delete_constraint
    sql_create_unique = (
        "ALTER TABLE %(table)s ADD CONSTRAINT %(name)s "
        "UNIQUE%(nulls_distinct)s (%(columns)s)%(deferrable)s"
    sql_delete_unique = sql_delete_constraint
    sql_create_fk = (
        "ALTER TABLE %(table)s ADD CONSTRAINT %(name)s FOREIGN KEY (%(column)s) "
        "REFERENCES %(to_table)s (%(to_column)s)%(on_delete_db)s%(deferrable)s"
    sql_create_inline_fk = None
    sql_create_column_inline_fk = None
    sql_delete_fk = sql_delete_constraint
    sql_create_index = (
        "CREATE INDEX %(name)s ON %(table)s "
        "(%(columns)s)%(include)s%(extra)s%(condition)s"
    sql_create_unique_index = (
        "CREATE UNIQUE INDEX %(name)s ON %(table)s "
        "(%(columns)s)%(include)s%(nulls_distinct)s%(condition)s"
    sql_rename_index = "ALTER INDEX %(old_name)s RENAME TO %(new_name)s"
    sql_delete_index = "DROP INDEX %(name)s"
    sql_create_pk = (
        "ALTER TABLE %(table)s ADD CONSTRAINT %(name)s PRIMARY KEY (%(columns)s)"
    sql_delete_pk = sql_delete_constraint
    sql_delete_procedure = "DROP PROCEDURE %(procedure)s"
    sql_alter_table_comment = "COMMENT ON TABLE %(table)s IS %(comment)s"
    sql_alter_column_comment = "COMMENT ON COLUMN %(table)s.%(column)s IS %(comment)s"
    def __init__(self, connection, collect_sql=False, atomic=True):
        self.collect_sql = collect_sql
        if self.collect_sql:
            self.collected_sql = []
        self.atomic_migration = self.connection.features.can_rollback_ddl and atomic
    # State-managing methods
        self.deferred_sql = []
        if self.atomic_migration:
            self.atomic = atomic(self.connection.alias)
            self.atomic.__enter__()
            for sql in self.deferred_sql:
                self.execute(sql, None)
            self.atomic.__exit__(exc_type, exc_value, traceback)
    # Core utility functions
    def execute(self, sql, params=()):
        """Execute the given SQL statement, with optional parameters."""
        # Don't perform the transactional DDL check if SQL is being collected
        # as it's not going to be executed anyway.
            not self.collect_sql
            and self.connection.in_atomic_block
            and not self.connection.features.can_rollback_ddl
                "Executing DDL statements while in a transaction on databases "
                "that can't perform a rollback is prohibited."
        # Account for non-string statement objects.
        sql = str(sql)
        # Log the command we're running, then run it
            "%s; (params %r)", sql, params, extra={"params": params, "sql": sql}
            ending = "" if sql.rstrip().endswith(";") else ";"
            if params is not None:
                self.collected_sql.append(
                    (sql % tuple(map(self.quote_value, params))) + ending
                self.collected_sql.append(sql + ending)
                cursor.execute(sql, params)
    def quote_name(self, name):
        return self.connection.ops.quote_name(name)
    def table_sql(self, model):
        """Take a model and return its table definition."""
        # Add any unique_togethers (always deferred, as some fields might be
        # created afterward, like geometry fields with some backends).
        for field_names in model._meta.unique_together:
            fields = [model._meta.get_field(field) for field in field_names]
            self.deferred_sql.append(self._create_unique_sql(model, fields))
        # Create column SQL, add FK deferreds if needed.
        column_sqls = []
        params = []
            # SQL.
            definition, extra_params = self.column_sql(model, field)
            if definition is None:
            # Check constraints can go on the column SQL here.
            db_params = field.db_parameters(connection=self.connection)
            if db_params["check"]:
                definition += " " + self.sql_check_constraint % db_params
            # Autoincrement SQL (for backends with inline variant).
            col_type_suffix = field.db_type_suffix(connection=self.connection)
            if col_type_suffix:
                definition += " %s" % col_type_suffix
            params.extend(extra_params)
            # FK.
            if field.remote_field and field.db_constraint:
                to_table = field.remote_field.model._meta.db_table
                to_column = field.remote_field.model._meta.get_field(
                    field.remote_field.field_name
                ).column
                if self.sql_create_inline_fk:
                    definition += " " + self.sql_create_inline_fk % {
                        "to_table": self.quote_name(to_table),
                        "to_column": self.quote_name(to_column),
                        "on_delete_db": self._create_on_delete_sql(model, field),
                elif self.connection.features.supports_foreign_keys:
                    self.deferred_sql.append(
                        self._create_fk_sql(
                            model, field, "_fk_%(to_table)s_%(to_column)s"
            # Add the SQL to our big list.
            column_sqls.append(
                "%s %s"
                    self.quote_name(field.column),
                    definition,
            # Autoincrement SQL (for backends with post table definition
            # variant).
            if field.get_internal_type() in (
                "AutoField",
                "BigAutoField",
                "SmallAutoField",
                autoinc_sql = self.connection.ops.autoinc_sql(
                    model._meta.db_table, field.column
                if autoinc_sql:
                    self.deferred_sql.extend(autoinc_sql)
        # The BaseConstraint DDL creation methods such as constraint_sql(),
        # create_sql(), and delete_sql(), were not designed in a way that
        # separate SQL from parameters which make their generated SQL unfit to
        # be used in a context where parametrization is delegated to the
        constraint_sqls = []
            # If parameters are present (e.g. a DEFAULT clause on backends that
            # allow parametrization) defer constraint creation so they are not
            # mixed with SQL meant to be parametrized.
            for constraint in model._meta.constraints:
                self.deferred_sql.append(constraint.create_sql(model, self))
            constraint_sqls.extend(
                constraint.constraint_sql(model, self)
                for constraint in model._meta.constraints
        pk = model._meta.pk
        if isinstance(pk, CompositePrimaryKey):
            constraint_sqls.append(self._pk_constraint_sql(pk.columns))
        sql = self.sql_create_table % {
            "definition": ", ".join(
                str(statement)
                for statement in (*column_sqls, *constraint_sqls)
                if statement
        if model._meta.db_tablespace:
            tablespace_sql = self.connection.ops.tablespace_sql(
                model._meta.db_tablespace
            if tablespace_sql:
                sql += " " + tablespace_sql
        return sql, params
    # Field <-> database mapping functions
    def _iter_column_sql(
        self, column_db_type, params, model, field, field_db_params, include_default
        yield column_db_type
        if collation := field_db_params.get("collation"):
            yield self._collate_sql(collation)
        # Work out nullability.
        null = field.null
        # Add database default.
        if field.has_db_default():
            default_sql, default_params = self.db_default_sql(field)
            yield f"DEFAULT {default_sql}"
            params.extend(default_params)
            include_default = False
        # Include a default value, if requested.
        include_default = (
            include_default
            and not self.skip_default(field)
            # Don't include a default value if it's a nullable field and the
            # default cannot be dropped in the ALTER COLUMN statement (e.g.
            # MySQL longtext and longblob).
            not (null and self.skip_default_on_alter(field))
        if include_default:
            default_value = self.effective_default(field)
            if default_value is not None:
                column_default = "DEFAULT " + self._column_default_sql(field)
                if self.connection.features.requires_literal_defaults:
                    # Some databases can't take defaults as a parameter
                    # (Oracle, SQLite). If this is the case, the individual
                    # schema backend should implement prepare_default().
                    yield column_default % self.prepare_default(default_value)
                    yield column_default
                    params.append(default_value)
        # Oracle treats the empty string ('') as null, so coerce the null
        # option whenever '' is a possible value.
            field.empty_strings_allowed
            and not field.primary_key
            and self.connection.features.interprets_empty_strings_as_nulls
            null = True
        if field.generated:
            generated_sql, generated_params = self._column_generated_sql(field)
            params.extend(generated_params)
            yield generated_sql
        elif not null:
            yield "NOT NULL"
        elif not self.connection.features.implied_column_null:
            yield "NULL"
            yield "PRIMARY KEY"
        elif field.unique:
            yield "UNIQUE"
        # Optionally add the tablespace if it's an implicitly indexed column.
        tablespace = field.db_tablespace or model._meta.db_tablespace
            tablespace
            and self.connection.features.supports_tablespaces
            and field.unique
            yield self.connection.ops.tablespace_sql(tablespace, inline=True)
        if self.connection.features.supports_comments_inline and field.db_comment:
            yield self._comment_sql(field.db_comment)
        Return the column definition for a field. The field must already have
        had set_attributes_from_name() called.
        # Get the column's type and use that as the basis of the SQL.
        field_db_params = field.db_parameters(connection=self.connection)
        column_db_type = field_db_params["type"]
        # Check for fields that aren't actually columns (e.g. M2M).
        if column_db_type is None:
            " ".join(
                # This appends to the params being returned.
                self._iter_column_sql(
                    column_db_type,
                    field_db_params,
                    include_default,
    def skip_default(self, field):
        Some backends don't accept default values for certain columns types
        (i.e. MySQL longtext and longblob).
    def skip_default_on_alter(self, field):
        (i.e. MySQL longtext and longblob) in the ALTER COLUMN statement.
    def prepare_default(self, value):
        Only used for backends which have requires_literal_defaults feature
            "subclasses of BaseDatabaseSchemaEditor for backends which have "
            "requires_literal_defaults must provide a prepare_default() method"
    def _column_default_sql(self, field):
        Return the SQL to use in a DEFAULT clause. The resulting string should
        contain a '%s' placeholder for a default value.
        return "%s"
    def db_default_sql(self, field):
        """Return the sql and params for the field's database default."""
        from django.db.models.expressions import Value
        db_default = field._db_default_expression
        sql = (
            self._column_default_sql(field) if isinstance(db_default, Value) else "(%s)"
        query = Query(model=field.model)
        compiler = query.get_compiler(connection=self.connection)
        default_sql, params = compiler.compile(db_default)
            # Some databases don't support parameterized defaults (Oracle,
            # SQLite). If this is the case, the individual schema backend
            # should implement prepare_default().
            default_sql %= tuple(self.prepare_default(p) for p in params)
        return sql % default_sql, params
    def _column_generated_persistency_sql(self, field):
        """Return the SQL to define the persistency of generated fields."""
        return "STORED" if field.db_persist else "VIRTUAL"
    def _column_generated_sql(self, field):
        """Return the SQL to use in a GENERATED ALWAYS clause."""
        expression_sql, params = field.generated_sql(self.connection)
        persistency_sql = self._column_generated_persistency_sql(field)
            expression_sql = expression_sql % tuple(self.quote_value(p) for p in params)
            params = ()
        return f"GENERATED ALWAYS AS ({expression_sql}) {persistency_sql}", params
    def _effective_default(field):
        # This method allows testing its logic without a connection.
        if field.has_default():
            default = field.get_default()
        elif field.generated:
        elif not field.null and field.blank and field.empty_strings_allowed:
            if field.get_internal_type() == "BinaryField":
                default = b""
                default = ""
        elif getattr(field, "auto_now", False) or getattr(field, "auto_now_add", False):
            internal_type = field.get_internal_type()
            if internal_type == "DateTimeField":
                default = timezone.now()
                default = datetime.now()
                if internal_type == "DateField":
                    default = default.date()
                elif internal_type == "TimeField":
                    default = default.time()
    def effective_default(self, field):
        """Return a field's effective database default value."""
        return field.get_db_prep_save(self._effective_default(field), self.connection)
        Return a quoted version of the value so it's safe to use in an SQL
        string. This is not safe against injection from user code; it is
        intended only for use in making SQL scripts or preparing default values
        for particularly tricky backends (defaults are not user-defined,
        though, so this is safe).
        Create a table and any accompanying indexes or unique constraints for
        the given `model`.
        sql, params = self.table_sql(model)
        # Prevent using [] as params, in the case a literal '%' is used in the
        # definition on backends that don't support parametrized DDL.
        self.execute(sql, params or None)
        if self.connection.features.supports_comments:
            # Add table comment.
            if model._meta.db_table_comment:
                self.alter_db_table_comment(model, None, model._meta.db_table_comment)
            # Add column comments.
            if not self.connection.features.supports_comments_inline:
                    if field.db_comment:
                        field_db_params = field.db_parameters(
                            connection=self.connection
                        field_type = field_db_params["type"]
                            *self._alter_column_comment_sql(
                                model, field, field_type, field.db_comment
        # Add any field index (deferred as SQLite _remake_table needs it).
        self.deferred_sql.extend(self._model_indexes_sql(model))
        # Make M2M tables
        for field in model._meta.local_many_to_many:
                self.create_model(field.remote_field.through)
        """Delete a model from the database."""
        # Handle auto-created intermediary models
                self.delete_model(field.remote_field.through)
        # Delete the table
            self.sql_delete_table
        # Remove all deferred statements referencing the deleted table.
        for sql in list(self.deferred_sql):
            if isinstance(sql, Statement) and sql.references_table(
                model._meta.db_table
                self.deferred_sql.remove(sql)
    def add_index(self, model, index):
        """Add an index on a model."""
            index.contains_expressions
            and not self.connection.features.supports_expression_indexes
        # Index.create_sql returns interpolated SQL which makes params=None a
        # necessity to avoid escaping attempts on execution.
        self.execute(index.create_sql(model, self), params=None)
    def remove_index(self, model, index):
        """Remove an index from a model."""
        self.execute(index.remove_sql(model, self))
    def rename_index(self, model, old_index, new_index):
        if self.connection.features.can_rename_index:
                self._rename_index_sql(model, old_index.name, new_index.name),
            self.remove_index(model, old_index)
            self.add_index(model, new_index)
    def add_constraint(self, model, constraint):
        """Add a constraint to a model."""
        sql = constraint.create_sql(model, self)
        if sql:
            # Constraint.create_sql returns interpolated SQL which makes
            # params=None a necessity to avoid escaping attempts on execution.
            self.execute(sql, params=None)
    def remove_constraint(self, model, constraint):
        """Remove a constraint from a model."""
        sql = constraint.remove_sql(model, self)
    def alter_unique_together(self, model, old_unique_together, new_unique_together):
        Deal with a model changing its unique_together. The input
        unique_togethers must be doubly-nested, not the single-nested
        ["foo", "bar"] format.
        olds = {tuple(fields) for fields in old_unique_together}
        news = {tuple(fields) for fields in new_unique_together}
        # Deleted uniques
        for fields in olds.difference(news):
            self._delete_composed_index(
                {"unique": True, "primary_key": False},
                self.sql_delete_unique,
        # Created uniques
        for field_names in news.difference(olds):
            self.execute(self._create_unique_sql(model, fields))
    def alter_index_together(self, model, old_index_together, new_index_together):
        Deal with a model changing its index_together. The input
        index_togethers must be doubly-nested, not the single-nested
        olds = {tuple(fields) for fields in old_index_together}
        news = {tuple(fields) for fields in new_index_together}
        # Deleted indexes
                {"index": True, "unique": False},
                self.sql_delete_index,
        # Created indexes
            self.execute(self._create_index_sql(model, fields=fields, suffix="_idx"))
    def _delete_composed_index(self, model, fields, constraint_kwargs, sql):
        meta_constraint_names = {
            constraint.name for constraint in model._meta.constraints
        meta_index_names = {constraint.name for constraint in model._meta.indexes}
        columns = [model._meta.get_field(field).column for field in fields]
        constraint_names = self._constraint_names(
            exclude=meta_constraint_names | meta_index_names,
            **constraint_kwargs,
            constraint_kwargs.get("unique") is True
            and constraint_names
            and self.connection.features.allows_multiple_constraints_on_same_fields
            # Constraint matching the unique_together name.
            default_name = str(
                self._unique_constraint_name(model._meta.db_table, columns, quote=False)
            if default_name in constraint_names:
                constraint_names = [default_name]
        if len(constraint_names) != 1:
                "Found wrong number (%s) of constraints for %s(%s)"
                    len(constraint_names),
                    model._meta.db_table,
                    ", ".join(columns),
        self.execute(self._delete_constraint_sql(sql, model, constraint_names[0]))
        """Rename the table a model points to."""
        # Rename all references to the old table name.
            if isinstance(sql, Statement):
                sql.rename_table_references(old_db_table, new_db_table)
    def alter_db_table_comment(self, model, old_db_table_comment, new_db_table_comment):
        if self.sql_alter_table_comment and self.connection.features.supports_comments:
                self.sql_alter_table_comment
                    "comment": self.quote_value(new_db_table_comment or ""),
    def alter_db_tablespace(self, model, old_db_tablespace, new_db_tablespace):
        """Move a model's table between tablespaces."""
            self.sql_retablespace_table
                "old_tablespace": self.quote_name(old_db_tablespace),
                "new_tablespace": self.quote_name(new_db_tablespace),
        Create a field on a model. Usually involves adding a column, but may
        involve adding a table instead (for M2M fields).
        # Special-case implicit M2M tables
        if field.many_to_many and field.remote_field.through._meta.auto_created:
            return self.create_model(field.remote_field.through)
        # Get the column's definition
        definition, params = self.column_sql(model, field, include_default=True)
        # It might not actually have a column behind it
        if col_type_suffix := field.db_type_suffix(connection=self.connection):
            definition += f" {col_type_suffix}"
        # Check constraints can go on the column SQL here
            field.remote_field
            and self.connection.features.supports_foreign_keys
            and field.db_constraint
            constraint_suffix = "_fk_%(to_table)s_%(to_column)s"
            # Add FK constraint inline, if supported.
            if self.sql_create_column_inline_fk:
                namespace, _ = split_identifier(model._meta.db_table)
                definition += " " + self.sql_create_column_inline_fk % {
                    "name": self._fk_constraint_name(model, field, constraint_suffix),
                    "namespace": (
                        "%s." % self.quote_name(namespace) if namespace else ""
                    "deferrable": self.connection.ops.deferrable_sql(),
            # Otherwise, add FK constraints later.
                    self._create_fk_sql(model, field, constraint_suffix)
        # Build the SQL and run it
        sql = self.sql_create_column % {
            "definition": definition,
        # Drop the default if we need to
            not field.has_db_default()
            and not self.skip_default_on_alter(field)
            and self.effective_default(field) is not None
            changes_sql, params = self._alter_column_default_sql(
                model, None, field, drop=True
            sql = self.sql_alter_column % {
                "changes": changes_sql,
            self.execute(sql, params)
        # Add field comment, if required.
            field.db_comment
            and self.connection.features.supports_comments
            and not self.connection.features.supports_comments_inline
            field_type = db_params["type"]
        # Add an index, if required
        self.deferred_sql.extend(self._field_indexes_sql(model, field))
        # Reset connection if required
        if self.connection.features.connection_persists_old_columns:
            self.connection.close()
        Remove a field from a model. Usually involves deleting a column,
        but for M2Ms may involve deleting a table.
            return self.delete_model(field.remote_field.through)
        if field.db_parameters(connection=self.connection)["type"] is None:
        # Drop any FK constraints, MySQL requires explicit deletion
            fk_names = self._constraint_names(model, [field.column], foreign_key=True)
            for fk_name in fk_names:
                self.execute(self._delete_fk_sql(model, fk_name))
        # Delete the column
        sql = self.sql_delete_column % {
        # Remove all deferred statements referencing the deleted column.
            if isinstance(sql, Statement) and sql.references_column(
    def alter_field(self, model, old_field, new_field, strict=False):
        Allow a field's type, uniqueness, nullability, default, column,
        constraints, etc. to be modified.
        `old_field` is required to compute the necessary changes.
        If `strict` is True, raise errors if the old column does not match
        `old_field` precisely.
        if not self._field_should_be_altered(old_field, new_field):
        # Ensure this field is even column-based
        old_db_params = old_field.db_parameters(connection=self.connection)
        old_type = old_db_params["type"]
        new_db_params = new_field.db_parameters(connection=self.connection)
        new_type = new_db_params["type"]
        modifying_generated_field = False
        if (old_type is None and old_field.remote_field is None) or (
            new_type is None and new_field.remote_field is None
                "Cannot alter field %s into %s - they do not properly define "
                "db_type (are you using a badly-written custom field?)"
                % (old_field, new_field),
            old_type is None
            and new_type is None
                old_field.remote_field.through
                and new_field.remote_field.through
                and old_field.remote_field.through._meta.auto_created
                and new_field.remote_field.through._meta.auto_created
            return self._alter_many_to_many(model, old_field, new_field, strict)
                and not old_field.remote_field.through._meta.auto_created
                and not new_field.remote_field.through._meta.auto_created
            # Both sides have through models; this is a no-op.
        elif old_type is None or new_type is None:
                "Cannot alter field %s into %s - they are not compatible types "
                "(you cannot alter to or from M2M fields, or add or remove "
                "through= on M2M fields)" % (old_field, new_field)
        elif old_field.generated != new_field.generated or (
            new_field.generated and old_field.db_persist != new_field.db_persist
            modifying_generated_field = True
        elif new_field.generated:
                old_field_sql = old_field.generated_sql(self.connection)
            except FieldError:
                # Field used in a generated field was renamed.
                new_field_sql = new_field.generated_sql(self.connection)
                modifying_generated_field = old_field_sql != new_field_sql
                db_features = self.connection.features
                # Some databases (e.g. Oracle) don't allow altering a data type
                # for generated columns.
                    not modifying_generated_field
                    and old_type != new_type
                    and not db_features.supports_alter_generated_column_data_type
        if modifying_generated_field:
                f"Modifying GeneratedFields is not supported - the field {new_field} "
                "must be removed and re-added with the new definition."
        self._alter_field(
            strict,
    def _field_db_check(self, field, field_db_params):
        # Always check constraints with the same mocked column name to avoid
        # recreating constraints when the column is renamed.
        check_constraints = self.connection.data_type_check_constraints
        data = field.db_type_parameters(self.connection)
        data["column"] = "__column_name__"
            return check_constraints[field.get_internal_type()] % data
        """Perform a "physical" (non-ManyToMany) field update."""
        # Drop any FK constraints, we'll remake them later
        fks_dropped = set()
            self.connection.features.supports_foreign_keys
            and old_field.remote_field
            and old_field.db_constraint
            and self._field_should_be_altered(
                ignore={"db_comment"},
            fk_names = self._constraint_names(
                model, [old_field.column], foreign_key=True
            if strict and len(fk_names) != 1:
                    "Found wrong number (%s) of foreign key constraints for %s.%s"
                        len(fk_names),
                        old_field.column,
                fks_dropped.add((old_field.column,))
        # Has unique been removed?
        if old_field.unique and (
            not new_field.unique or self._field_became_primary_key(old_field, new_field)
            # Find the unique constraint for this field
                [old_field.column],
                primary_key=False,
                exclude=meta_constraint_names,
            if strict and len(constraint_names) != 1:
                    "Found wrong number (%s) of unique constraints for %s.%s"
            for constraint_name in constraint_names:
                self.execute(self._delete_unique_sql(model, constraint_name))
        # Drop incoming FK constraints if the field is a primary key or unique,
        # which might be a to_field target, and things are going to change.
        old_collation = old_db_params.get("collation")
        new_collation = new_db_params.get("collation")
        drop_foreign_keys = (
                (old_field.primary_key and new_field.primary_key)
                or (old_field.unique and new_field.unique)
            and ((old_type != new_type) or (old_collation != new_collation))
        if drop_foreign_keys:
            # '_meta.related_field' also contains M2M reverse fields, these
            # will be filtered out
            for _old_rel, new_rel in _related_non_m2m_objects(old_field, new_field):
                rel_fk_names = self._constraint_names(
                    new_rel.related_model, [new_rel.field.column], foreign_key=True
                for fk_name in rel_fk_names:
                    self.execute(self._delete_fk_sql(new_rel.related_model, fk_name))
        # Removed an index? (no strict check, as multiple indexes are possible)
        # Remove indexes if db_index switched to False or a unique constraint
        # will now be used in lieu of an index. The following lines from the
        # truth table show all True cases; the rest are False:
        #      old_field    |     new_field
        # db_index | unique | db_index | unique
        # -------------------------------------
        # True     | False  | False    | False
        # True     | False  | False    | True
        # True     | False  | True     | True
            old_field.db_index
            and not old_field.unique
            and (not new_field.db_index or new_field.unique)
            # Find the index for this field
            meta_index_names = {index.name for index in model._meta.indexes}
            # Retrieve only BTREE indexes since this is what's created with
            # db_index=True.
            index_names = self._constraint_names(
                index=True,
                type_=Index.suffix,
                exclude=meta_index_names,
            for index_name in index_names:
                # The only way to check if an index was created with
                # db_index=True or with Index(['field'], name='foo')
                # is to look at its name (refs #28053).
                self.execute(self._delete_index_sql(model, index_name))
        # Change check constraints?
        old_db_check = self._field_db_check(old_field, old_db_params)
        new_db_check = self._field_db_check(new_field, new_db_params)
        if old_db_check != new_db_check and old_db_check:
                    "Found wrong number (%s) of check constraints for %s.%s"
                self.execute(self._delete_check_sql(model, constraint_name))
        # Have they renamed the column?
        if old_field.column != new_field.column:
                self._rename_field_sql(
                    model._meta.db_table, old_field, new_field, new_type
            # Rename all references to the renamed column.
                    sql.rename_column_references(
                        model._meta.db_table, old_field.column, new_field.column
        # Next, start accumulating actions to do
        null_actions = []
        post_actions = []
        # Type suffix change? (e.g. auto increment).
        old_type_suffix = old_field.db_type_suffix(connection=self.connection)
        new_type_suffix = new_field.db_type_suffix(connection=self.connection)
        # Type, collation, or comment change?
            old_type != new_type
            or old_type_suffix != new_type_suffix
            or old_collation != new_collation
                self.connection.features.supports_comments
                and old_field.db_comment != new_field.db_comment
            fragment, other_actions = self._alter_column_type_sql(
                model, old_field, new_field, new_type, old_collation, new_collation
            actions.append(fragment)
            post_actions.extend(other_actions)
        if new_field.has_db_default():
                not old_field.has_db_default()
                or new_field.db_default != old_field.db_default
                actions.append(
                    self._alter_column_database_default_sql(model, old_field, new_field)
        elif old_field.has_db_default():
                self._alter_column_database_default_sql(
                    model, old_field, new_field, drop=True
        # When changing a column NULL constraint to NOT NULL with a given
        # default value, we need to perform 4 steps:
        #  1. Add a default for new incoming writes
        #  2. Update existing NULL rows with new default
        #  3. Replace NULL constraint with NOT NULL
        #  4. Drop the default again.
        # Default change?
        needs_database_default = False
        if old_field.null and not new_field.null and not new_field.has_db_default():
            old_default = self.effective_default(old_field)
            new_default = self.effective_default(new_field)
                not self.skip_default_on_alter(new_field)
                and old_default != new_default
                and new_default is not None
                needs_database_default = True
                    self._alter_column_default_sql(model, old_field, new_field)
        # Nullability change?
        if old_field.null != new_field.null:
            fragment = self._alter_column_null_sql(model, old_field, new_field)
            if fragment:
                null_actions.append(fragment)
        # Only if we have a default and there is a change from NULL to NOT NULL
        four_way_default_alteration = (
            new_field.has_default() or new_field.has_db_default()
        ) and (old_field.null and not new_field.null)
        if actions or null_actions:
            if not four_way_default_alteration:
                # If we don't have to do a 4-way default alteration we can
                # directly run a (NOT) NULL alteration
                actions += null_actions
            # Combine actions together if we can (e.g. postgres)
            if self.connection.features.supports_combined_alters and actions:
                sql, params = tuple(zip(*actions))
                actions = [(", ".join(sql), tuple(chain(*params)))]
            # Apply those actions
            for sql, params in actions:
                    self.sql_alter_column
                        "changes": sql,
            if four_way_default_alteration:
                if not new_field.has_db_default():
                    default_sql = "%s"
                    params = [new_default]
                    default_sql, params = self.db_default_sql(new_field)
                # Update existing rows with default value
                    self.sql_update_with_default
                        "default": default_sql,
                # Since we didn't run a NOT NULL change before we need to do it
                # now
                for sql, params in null_actions:
        if post_actions:
            for sql, params in post_actions:
        # If primary_key changed to False, delete the primary key constraint.
        if old_field.primary_key and not new_field.primary_key:
            self._delete_primary_key(model, strict)
        # Added a unique?
        if self._unique_should_be_added(old_field, new_field):
            self.execute(self._create_unique_sql(model, [new_field]))
        # Added an index? Add an index if db_index switched to True or a unique
        # constraint will no longer be used in lieu of an index. The following
        # lines from the truth table show all True cases; the rest are False:
        # False    | False  | True     | False
        # False    | True   | True     | False
        # True     | True   | True     | False
            (not old_field.db_index or old_field.unique)
            and new_field.db_index
            and not new_field.unique
            self.execute(self._create_index_sql(model, fields=[new_field]))
        # Type alteration on primary key? Then we need to alter the column
        # referring to us.
        rels_to_update = []
            rels_to_update.extend(_related_non_m2m_objects(old_field, new_field))
        # Changed to become primary key?
        if self._field_became_primary_key(old_field, new_field):
            # Make the new one
            self.execute(self._create_primary_key_sql(model, new_field))
            # Update all referencing columns
        # Handle our type alters on the other end of rels from the PK stuff
        # above
        for old_rel, new_rel in rels_to_update:
            rel_db_params = new_rel.field.db_parameters(connection=self.connection)
            rel_type = rel_db_params["type"]
            rel_collation = rel_db_params.get("collation")
            old_rel_db_params = old_rel.field.db_parameters(connection=self.connection)
            old_rel_collation = old_rel_db_params.get("collation")
                new_rel.related_model,
                old_rel.field,
                new_rel.field,
                rel_type,
                old_rel_collation,
                rel_collation,
                    "table": self.quote_name(new_rel.related_model._meta.db_table),
                    "changes": fragment[0],
                fragment[1],
            for sql, params in other_actions:
        # Does it have a foreign key?
            and new_field.remote_field
                fks_dropped or not old_field.remote_field or not old_field.db_constraint
            and new_field.db_constraint
                self._create_fk_sql(model, new_field, "_fk_%(to_table)s_%(to_column)s")
        # Rebuild FKs that pointed to us if we previously had to drop them
            for _, rel in rels_to_update:
                if rel.field.db_constraint:
                        self._create_fk_sql(rel.related_model, rel.field, "_fk")
        # Does it have check constraints we need to add?
        if old_db_check != new_db_check and new_db_check:
            constraint_name = self._create_index_name(
                model._meta.db_table, [new_field.column], suffix="_check"
                self._create_check_sql(model, constraint_name, new_db_params["check"])
        # (Django usually does not use in-database defaults)
        if needs_database_default:
    def _alter_column_null_sql(self, model, old_field, new_field):
        Hook to specialize column null alteration.
        Return a (sql, params) fragment to set a column to null or non-null
        as required by new_field, or None if no changes are required.
            self.connection.features.interprets_empty_strings_as_nulls
            and new_field.empty_strings_allowed
            # The field is nullable in the database anyway, leave it alone.
                self.sql_alter_column_null
                if new_field.null
                else self.sql_alter_column_not_null
                sql
                    "type": new_db_params["type"],
    def _alter_column_default_sql(self, model, old_field, new_field, drop=False):
        Hook to specialize column default alteration.
        Return a (sql, params) fragment to add or drop (depending on the drop
        argument) a default to new_field's column.
        default = self._column_default_sql(new_field)
        if drop:
        elif self.connection.features.requires_literal_defaults:
            # Some databases (Oracle) can't take defaults as a parameter
            # If this is the case, the SchemaEditor for that database should
            # implement prepare_default().
            default = self.prepare_default(new_default)
            if new_field.null:
                sql = self.sql_alter_column_no_default_null
                sql = self.sql_alter_column_no_default
            sql = self.sql_alter_column_default
    def _alter_column_database_default_sql(
        self, model, old_field, new_field, drop=False
        Hook to specialize column database default alteration.
            default_sql = ""
        self, model, old_field, new_field, new_type, old_collation, new_collation
        Hook to specialize column type alteration for different backends,
        for cases when a creation type is different to an alteration type
        (e.g. SERIAL in PostgreSQL, PostGIS fields).
        Return a 2-tuple of: an SQL fragment of (sql, params) to insert into
        an ALTER TABLE statement and a list of extra (sql, params) tuples to
        run once the field is altered.
        other_actions = []
        if collate_sql := self._collate_sql(
            new_collation, old_collation, model._meta.db_table
            collate_sql = f" {collate_sql}"
            collate_sql = ""
        # Comment change?
        comment_sql = ""
        if self.connection.features.supports_comments and not new_field.many_to_many:
            if old_field.db_comment != new_field.db_comment:
                # PostgreSQL and Oracle can't execute 'ALTER COLUMN ...' and
                # 'COMMENT ON ...' at the same time.
                sql, params = self._alter_column_comment_sql(
                    model, new_field, new_type, new_field.db_comment
                    other_actions.append((sql, params))
            if new_field.db_comment:
                comment_sql = self._comment_sql(new_field.db_comment)
                self.sql_alter_column_type
                    "collation": collate_sql,
                    "comment": comment_sql,
            other_actions,
    def _alter_column_comment_sql(self, model, new_field, new_type, new_db_comment):
            self.sql_alter_column_comment
                "comment": self._comment_sql(new_db_comment),
    def _comment_sql(self, comment):
        return self.quote_value(comment or "")
    def _alter_many_to_many(self, model, old_field, new_field, strict):
        """Alter M2Ms to repoint their to= endpoints."""
        # Rename the through table
            old_field.remote_field.through._meta.db_table
            != new_field.remote_field.through._meta.db_table
            self.alter_db_table(
                old_field.remote_field.through,
                old_field.remote_field.through._meta.db_table,
                new_field.remote_field.through._meta.db_table,
        # Repoint the FK to the other side
        self.alter_field(
            new_field.remote_field.through,
            # The field that points to the target model is needed, so we can
            # tell alter_field to change it - this is m2m_reverse_field_name()
            # (as opposed to m2m_field_name(), which points to our model).
            old_field.remote_field.through._meta.get_field(
                old_field.m2m_reverse_field_name()
            new_field.remote_field.through._meta.get_field(
                new_field.m2m_reverse_field_name()
            # for self-referential models we need to alter field from the other
            # end too
            old_field.remote_field.through._meta.get_field(old_field.m2m_field_name()),
            new_field.remote_field.through._meta.get_field(new_field.m2m_field_name()),
    def _create_index_name(self, table_name, column_names, suffix=""):
        Generate a unique name for an index/unique constraint.
        The name is divided into 3 parts: the table name, the column names,
        and a unique digest and suffix.
        _, table_name = split_identifier(table_name)
        hash_suffix_part = "%s%s" % (
            names_digest(table_name, *column_names, length=8),
            suffix,
        max_length = self.connection.ops.max_name_length() or 200
        # If everything fits into max_length, use that name.
        index_name = "%s_%s_%s" % (table_name, "_".join(column_names), hash_suffix_part)
        if len(index_name) <= max_length:
            return index_name
        # Shorten a long suffix.
        if len(hash_suffix_part) > max_length / 3:
            hash_suffix_part = hash_suffix_part[: max_length // 3]
        other_length = (max_length - len(hash_suffix_part)) // 2 - 1
        index_name = "%s_%s_%s" % (
            table_name[:other_length],
            "_".join(column_names)[:other_length],
            hash_suffix_part,
        # Prepend D if needed to prevent the name from starting with an
        # underscore or a number (not permitted on Oracle).
        if index_name[0] == "_" or index_name[0].isdigit():
            index_name = "D%s" % index_name[:-1]
    def _get_index_tablespace_sql(self, model, fields, db_tablespace=None):
        if db_tablespace is None:
            if len(fields) == 1 and fields[0].db_tablespace:
                db_tablespace = fields[0].db_tablespace
            elif settings.DEFAULT_INDEX_TABLESPACE:
                db_tablespace = settings.DEFAULT_INDEX_TABLESPACE
            elif model._meta.db_tablespace:
                db_tablespace = model._meta.db_tablespace
        if db_tablespace is not None:
            return " " + self.connection.ops.tablespace_sql(db_tablespace)
    def _index_condition_sql(self, condition):
        if condition:
            return " WHERE " + condition
    def _index_include_sql(self, model, columns):
        if not columns or not self.connection.features.supports_covering_indexes:
        return Statement(
            " INCLUDE (%(columns)s)",
            columns=Columns(model._meta.db_table, columns, self.quote_name),
    def _create_index_sql(
        suffix="",
        using="",
        db_tablespace=None,
        col_suffixes=(),
        sql=None,
        opclasses=(),
        condition=None,
        include=None,
        expressions=None,
        Return the SQL statement to create the index for one or several fields
        or expressions. `sql` can be specified if the syntax differs from the
        standard (GIS indexes, ...).
        fields = fields or []
        expressions = expressions or []
        compiler = Query(model, alias_cols=False).get_compiler(
            connection=self.connection,
        tablespace_sql = self._get_index_tablespace_sql(
            model, fields, db_tablespace=db_tablespace
        columns = [field.column for field in fields]
        sql_create_index = sql or self.sql_create_index
        table = model._meta.db_table
        def create_index_name(*args, **kwargs):
            nonlocal name
                name = self._create_index_name(*args, **kwargs)
            return self.quote_name(name)
            sql_create_index,
            table=Table(table, self.quote_name),
            name=IndexName(table, columns, suffix, create_index_name),
            columns=(
                self._index_columns(table, columns, col_suffixes, opclasses)
                if columns
                else Expressions(table, expressions, compiler, self.quote_value)
            extra=tablespace_sql,
            condition=self._index_condition_sql(condition),
            include=self._index_include_sql(model, include),
    def _delete_index_sql(self, model, name, sql=None):
        statement = Statement(
            sql or self.sql_delete_index,
            table=Table(model._meta.db_table, self.quote_name),
            name=self.quote_name(name),
        # Remove all deferred statements referencing the deleted index.
        table_name = statement.parts["table"].table
        index_name = statement.parts["name"]
            if isinstance(sql, Statement) and sql.references_index(
                table_name, index_name
        return statement
    def _rename_index_sql(self, model, old_name, new_name):
            self.sql_rename_index,
            old_name=self.quote_name(old_name),
            new_name=self.quote_name(new_name),
    def _create_on_delete_sql(self, model, field):
        remote_field = field.remote_field
            return remote_field.on_delete.on_delete_sql(self)
    def _index_columns(self, table, columns, col_suffixes, opclasses):
        return Columns(table, columns, self.quote_name, col_suffixes=col_suffixes)
    def _model_indexes_sql(self, model):
        Return a list of all index SQL statements (field indexes, Meta.indexes)
        for the specified model.
        if not model._meta.managed or model._meta.proxy or model._meta.swapped:
            output.extend(self._field_indexes_sql(model, field))
        for index in model._meta.indexes:
                not index.contains_expressions
                or self.connection.features.supports_expression_indexes
                output.append(index.create_sql(model, self))
        Return a list of all index SQL statements for the specified field.
        if self._field_should_be_indexed(model, field):
            output.append(self._create_index_sql(model, fields=[field]))
    def _field_should_be_altered(self, old_field, new_field, ignore=None):
        if (not (old_field.concrete or old_field.many_to_many)) and (
            not (new_field.concrete or new_field.many_to_many)
        ignore = ignore or set()
        _, old_path, old_args, old_kwargs = old_field.deconstruct()
        _, new_path, new_args, new_kwargs = new_field.deconstruct()
        # Don't alter when:
        # - changing only a field name (unless it's a many-to-many)
        # - changing an attribute that doesn't affect the schema
        # - changing an attribute in the provided set of ignored attributes
        # - adding only a db_column and the column name is not changed
        # - db_table does not change for model referenced by foreign keys
        for attr in ignore.union(old_field.non_db_attrs):
            old_kwargs.pop(attr, None)
        for attr in ignore.union(new_field.non_db_attrs):
            new_kwargs.pop(attr, None)
            not new_field.many_to_many
            and old_field.remote_field.model._meta.db_table
            == new_field.remote_field.model._meta.db_table
            old_kwargs.pop("to", None)
            new_kwargs.pop("to", None)
        # db_default can take many forms but result in the same SQL.
            old_kwargs.get("db_default")
            and new_kwargs.get("db_default")
            and self.db_default_sql(old_field) == self.db_default_sql(new_field)
            old_kwargs.pop("db_default")
            new_kwargs.pop("db_default")
            old_field.concrete
            and new_field.concrete
            and (self.quote_name(old_field.column) != self.quote_name(new_field.column))
            old_field.many_to_many
            and new_field.many_to_many
            and old_field.name != new_field.name
        return (old_path, old_args, old_kwargs) != (new_path, new_args, new_kwargs)
        return field.db_index and not field.unique
    def _field_became_primary_key(self, old_field, new_field):
        return not old_field.primary_key and new_field.primary_key
    def _unique_should_be_added(self, old_field, new_field):
            not new_field.primary_key
            and new_field.unique
            and (not old_field.unique or old_field.primary_key)
    def _rename_field_sql(self, table, old_field, new_field, new_type):
        return self.sql_rename_column % {
            "table": self.quote_name(table),
            "old_column": self.quote_name(old_field.column),
            "new_column": self.quote_name(new_field.column),
    def _create_fk_sql(self, model, field, suffix):
        table = Table(model._meta.db_table, self.quote_name)
        name = self._fk_constraint_name(model, field, suffix)
        column = Columns(model._meta.db_table, [field.column], self.quote_name)
        to_table = Table(field.target_field.model._meta.db_table, self.quote_name)
        to_column = Columns(
            field.target_field.model._meta.db_table,
            [field.target_field.column],
            self.quote_name,
        deferrable = self.connection.ops.deferrable_sql()
            self.sql_create_fk,
            table=table,
            column=column,
            to_table=to_table,
            to_column=to_column,
            deferrable=deferrable,
            on_delete_db=self._create_on_delete_sql(model, field),
    def _fk_constraint_name(self, model, field, suffix):
        def create_fk_name(*args, **kwargs):
            return self.quote_name(self._create_index_name(*args, **kwargs))
        return ForeignKeyName(
            [field.column],
            split_identifier(field.target_field.model._meta.db_table)[1],
            create_fk_name,
    def _delete_fk_sql(self, model, name):
        return self._delete_constraint_sql(self.sql_delete_fk, model, name)
    def _deferrable_constraint_sql(self, deferrable):
        if deferrable is None:
        if deferrable == Deferrable.DEFERRED:
            return " DEFERRABLE INITIALLY DEFERRED"
        if deferrable == Deferrable.IMMEDIATE:
            return " DEFERRABLE INITIALLY IMMEDIATE"
    def _unique_index_nulls_distinct_sql(self, nulls_distinct):
        if nulls_distinct is False:
            return " NULLS NOT DISTINCT"
        elif nulls_distinct is True:
            return " NULLS DISTINCT"
    def _unique_supported(
        deferrable=None,
        nulls_distinct=None,
            (not condition or self.connection.features.supports_partial_indexes)
                not deferrable
                or self.connection.features.supports_deferrable_unique_constraints
            and (not include or self.connection.features.supports_covering_indexes)
                not expressions or self.connection.features.supports_expression_indexes
                nulls_distinct is None
                or self.connection.features.supports_nulls_distinct_unique_constraints
    def _unique_sql(
        opclasses=None,
        if not self._unique_supported(
            condition=condition,
            nulls_distinct=nulls_distinct,
            condition
            or include
            or opclasses
            or expressions
            or nulls_distinct is not None
            # Databases support conditional, covering, functional unique,
            # and nulls distinct constraints via a unique index.
            sql = self._create_unique_sql(
                self.deferred_sql.append(sql)
        constraint = self.sql_unique_constraint % {
            "columns": ", ".join([self.quote_name(field.column) for field in fields]),
            "deferrable": self._deferrable_constraint_sql(deferrable),
        return self.sql_constraint % {
            "name": self.quote_name(name),
            "constraint": constraint,
    def _create_unique_sql(
            name = self._unique_constraint_name(table, columns, quote=True)
            name = self.quote_name(name)
        if condition or include or opclasses or expressions:
            sql = self.sql_create_unique_index
            sql = self.sql_create_unique
        if columns:
            columns = self._index_columns(
                table, columns, col_suffixes=(), opclasses=opclasses
            columns = Expressions(table, expressions, compiler, self.quote_value)
            columns=columns,
            deferrable=self._deferrable_constraint_sql(deferrable),
            nulls_distinct=self._unique_index_nulls_distinct_sql(nulls_distinct),
    def _unique_constraint_name(self, table, columns, quote=True):
        if quote:
            def create_unique_name(*args, **kwargs):
            create_unique_name = self._create_index_name
        return IndexName(table, columns, "_uniq", create_unique_name)
    def _delete_unique_sql(
            sql = self.sql_delete_index
            sql = self.sql_delete_unique
        return self._delete_constraint_sql(sql, model, name)
    def _check_sql(self, name, check):
            "constraint": self.sql_check_constraint % {"check": check},
    def _create_check_sql(self, model, name, check):
        if not self.connection.features.supports_table_check_constraints:
            self.sql_create_check,
            check=check,
    def _delete_check_sql(self, model, name):
        return self._delete_constraint_sql(self.sql_delete_check, model, name)
    def _delete_constraint_sql(self, template, model, name):
    def _constraint_names(
        column_names=None,
        unique=None,
        primary_key=None,
        index=None,
        foreign_key=None,
        check=None,
        type_=None,
        exclude=None,
        """Return all constraint names matching the columns and conditions."""
        if column_names is not None:
            column_names = [
                    self.connection.introspection.identifier_converter(
                        truncate_name(name, self.connection.ops.max_name_length())
                    if self.connection.features.truncates_names
                    else self.connection.introspection.identifier_converter(name)
                for name in column_names
            constraints = self.connection.introspection.get_constraints(
        for name, infodict in constraints.items():
            if column_names is None or column_names == infodict["columns"]:
                if unique is not None and infodict["unique"] != unique:
                if primary_key is not None and infodict["primary_key"] != primary_key:
                if index is not None and infodict["index"] != index:
                if check is not None and infodict["check"] != check:
                if foreign_key is not None and not infodict["foreign_key"]:
                if type_ is not None and infodict["type"] != type_:
                if not exclude or name not in exclude:
                    result.append(name)
    def _pk_constraint_sql(self, columns):
        return self.sql_pk_constraint % {
            "columns": ", ".join(self.quote_name(column) for column in columns)
    def _delete_primary_key(self, model, strict=False):
        constraint_names = self._constraint_names(model, primary_key=True)
                "Found wrong number (%s) of PK constraints for %s"
            self.execute(self._delete_primary_key_sql(model, constraint_name))
    def _create_primary_key_sql(self, model, field):
            self.sql_create_pk,
            name=self.quote_name(
                self._create_index_name(
                    model._meta.db_table, [field.column], suffix="_pk"
            columns=Columns(model._meta.db_table, [field.column], self.quote_name),
    def _delete_primary_key_sql(self, model, name):
        return self._delete_constraint_sql(self.sql_delete_pk, model, name)
    def _collate_sql(self, collation, old_collation=None, table_name=None):
        return "COLLATE " + self.quote_name(collation) if collation else ""
    def remove_procedure(self, procedure_name, param_types=()):
        sql = self.sql_delete_procedure % {
            "procedure": self.quote_name(procedure_name),
            "param_types": ",".join(param_types),
from django.db.backends.base.schema import BaseDatabaseSchemaEditor
from django.db.models import NOT_PROVIDED, F, UniqueConstraint
class DatabaseSchemaEditor(BaseDatabaseSchemaEditor):
    sql_rename_table = "RENAME TABLE %(old_table)s TO %(new_table)s"
    sql_alter_column_null = "MODIFY %(column)s %(type)s NULL"
    sql_alter_column_not_null = "MODIFY %(column)s %(type)s NOT NULL"
    sql_alter_column_type = "MODIFY %(column)s %(type)s%(collation)s%(comment)s"
    sql_alter_column_no_default_null = "ALTER COLUMN %(column)s SET DEFAULT NULL"
    sql_delete_unique = "ALTER TABLE %(table)s DROP INDEX %(name)s"
    sql_create_column_inline_fk = (
        ", ADD CONSTRAINT %(name)s FOREIGN KEY (%(column)s) "
        "REFERENCES %(to_table)s(%(to_column)s)%(on_delete_db)s"
    sql_delete_fk = "ALTER TABLE %(table)s DROP FOREIGN KEY %(name)s"
    sql_delete_index = "DROP INDEX %(name)s ON %(table)s"
    sql_rename_index = "ALTER TABLE %(table)s RENAME INDEX %(old_name)s TO %(new_name)s"
    sql_delete_pk = "ALTER TABLE %(table)s DROP PRIMARY KEY"
    sql_create_index = "CREATE INDEX %(name)s ON %(table)s (%(columns)s)%(extra)s"
    sql_alter_table_comment = "ALTER TABLE %(table)s COMMENT = %(comment)s"
    sql_alter_column_comment = None
    def sql_delete_check(self):
        if self.connection.mysql_is_mariadb:
            # The name of the column check constraint is the same as the field
            # name on MariaDB. Adding IF EXISTS clause prevents migrations
            # crash. Constraint is removed during a "MODIFY" column statement.
            return "ALTER TABLE %(table)s DROP CONSTRAINT IF EXISTS %(name)s"
        return "ALTER TABLE %(table)s DROP CHECK %(name)s"
        self.connection.ensure_connection()
        # MySQLdb escapes to string, PyMySQL to bytes.
        quoted = self.connection.connection.escape(
            value, self.connection.connection.encoders
        if isinstance(value, str) and isinstance(quoted, bytes):
            quoted = quoted.decode()
        return quoted
    def _is_limited_data_type(self, field):
        db_type = field.db_type(self.connection)
            db_type is not None
            and db_type.lower() in self.connection._limited_data_types
    def _is_text_or_blob(self, field):
        return db_type and db_type.lower().endswith(("blob", "text"))
        default_is_empty = self.effective_default(field) in ("", b"")
        if default_is_empty and self._is_text_or_blob(field):
        if self.skip_default(field):
        if self._is_limited_data_type(field) and not self.connection.mysql_is_mariadb:
            # MySQL doesn't support defaults for BLOB and TEXT in the
            # ALTER COLUMN statement.
        if not self.connection.mysql_is_mariadb and self._is_limited_data_type(field):
            # MySQL supports defaults for BLOB and TEXT columns only if the
            # default value is written as an expression i.e. in parentheses.
            return "(%s)"
        return super()._column_default_sql(field)
        # Simulate the effect of a one-off default.
        # field.default may be unhashable, so a set isn't used for "in" check.
        if self.skip_default(field) and field.default not in (None, NOT_PROVIDED):
            effective_default = self.effective_default(field)
                "UPDATE %(table)s SET %(column)s = %%s"
                [effective_default],
            isinstance(constraint, UniqueConstraint)
            and constraint.create_sql(model, self) is not None
            self._create_missing_fk_index(
                fields=constraint.fields,
                expressions=constraint.expressions,
        super().remove_constraint(model, constraint)
            fields=[field_name for field_name, _ in index.fields_orders],
            expressions=index.expressions,
        super().remove_index(model, index)
        if not super()._field_should_be_indexed(model, field):
        storage = self.connection.introspection.get_storage_engine(
            self.connection.cursor(), model._meta.db_table
        # No need to create an index for ForeignKey fields except if
        # db_constraint=False because the index from that constraint won't be
            storage == "InnoDB"
            and field.get_internal_type() == "ForeignKey"
        return not self._is_limited_data_type(field)
    def _create_missing_fk_index(
        MySQL can remove an implicit FK index on a field when that field is
        covered by another index like a unique_together. "covered" here means
        that the more complex index has the FK field as its first field (see
        https://bugs.mysql.com/bug.php?id=37910).
        Manually create an implicit FK index to make it possible to remove the
        composed index.
        first_field_name = None
            first_field_name = fields[0]
            expressions
            and self.connection.features.supports_expression_indexes
            and isinstance(expressions[0], F)
            and LOOKUP_SEP not in expressions[0].name
            first_field_name = expressions[0].name
        if not first_field_name:
        first_field = model._meta.get_field(first_field_name)
        if first_field.get_internal_type() == "ForeignKey":
            column = self.connection.introspection.identifier_converter(
                first_field.column
                constraint_names = [
                    for name, infodict in self.connection.introspection.get_constraints(
                    ).items()
                    if infodict["index"] and infodict["columns"][0] == column
            # There are no other indexes that starts with the FK field, only
            # the index that is expected to be deleted.
            if len(constraint_names) == 1:
                    self._create_index_sql(model, fields=[first_field], suffix="")
    def _delete_composed_index(self, model, fields, *args):
        self._create_missing_fk_index(model, fields=fields)
        return super()._delete_composed_index(model, fields, *args)
    def _set_field_new_type(self, field, new_type):
        Keep the NULL and DEFAULT properties of the old field. If it has
        changed, it will be handled separately.
            default_sql, params = self.db_default_sql(field)
            default_sql %= tuple(self.quote_value(p) for p in params)
            new_type += f" DEFAULT {default_sql}"
        if field.null:
            new_type += " NULL"
            new_type += " NOT NULL"
        return new_type
        new_type = self._set_field_new_type(old_field, new_type)
            return super()._field_db_check(field, field_db_params)
        # On MySQL, check constraints with the column name as it requires
        # explicit recreation when the column is renamed.
        return field_db_params["check"]
        return super()._rename_field_sql(table, old_field, new_field, new_type)
        # Comment is alter when altering the column type.
        return "", []
        comment_sql = super()._comment_sql(comment)
        return f" COMMENT {comment_sql}"
            return super()._alter_column_null_sql(model, old_field, new_field)
        type_sql = self._set_field_new_type(new_field, new_db_params["type"])
            "MODIFY %(column)s %(type)s"
                "type": type_sql,
from django.db.backends.base.schema import (
    BaseDatabaseSchemaEditor,
    _related_non_m2m_objects,
from django.utils.duration import duration_iso_string
    sql_create_column = "ALTER TABLE %(table)s ADD %(column)s %(definition)s"
    sql_alter_column_type = "MODIFY %(column)s %(type)s%(collation)s"
    sql_alter_column_null = "MODIFY %(column)s NULL"
    sql_alter_column_not_null = "MODIFY %(column)s NOT NULL"
    sql_alter_column_default = "MODIFY %(column)s DEFAULT %(default)s"
    sql_alter_column_no_default = "MODIFY %(column)s DEFAULT NULL"
        "CONSTRAINT %(name)s REFERENCES %(to_table)s(%(to_column)s)%(on_delete_db)"
        "s%(deferrable)s"
    sql_delete_table = "DROP TABLE %(table)s CASCADE CONSTRAINTS"
        if isinstance(value, (datetime.date, datetime.time, datetime.datetime)):
            return "'%s'" % value
        elif isinstance(value, datetime.timedelta):
            return "'%s'" % duration_iso_string(value)
        elif isinstance(value, str):
            return "'%s'" % value.replace("'", "''")
        elif isinstance(value, (bytes, bytearray, memoryview)):
            return "'%s'" % value.hex()
            return "1" if value else "0"
        # If the column is an identity column, drop the identity before
        # removing the field.
        if self._is_identity_column(model._meta.db_table, field.column):
            self._drop_identity(model._meta.db_table, field.column)
        # Run superclass action
        # Clean up manually created sequence.
            DECLARE
                i INTEGER;
                SELECT COUNT(1) INTO i FROM USER_SEQUENCES
                    WHERE SEQUENCE_NAME = '%(sq_name)s';
                IF i = 1 THEN
                    EXECUTE IMMEDIATE 'DROP SEQUENCE "%(sq_name)s"';
                END IF;
        /"""
                "sq_name": self.connection.ops._get_no_autofield_sequence_name(
            super().alter_field(model, old_field, new_field, strict)
        except DatabaseError as e:
            description = str(e)
            # If we're changing type to an unsupported type we need a
            # SQLite-ish workaround
            if "ORA-22858" in description or "ORA-22859" in description:
                self._alter_field_type_workaround(model, old_field, new_field)
            # If an identity column is changing to a non-numeric type, drop the
            # identity first.
            elif "ORA-30675" in description:
                self._drop_identity(model._meta.db_table, old_field.column)
                self.alter_field(model, old_field, new_field, strict)
            # If a primary key column is changing to an identity column, drop
            # the primary key first.
            elif "ORA-30673" in description and old_field.primary_key:
                self._delete_primary_key(model, strict=True)
            # If a collation is changing on a primary key, drop the primary key
            elif "ORA-43923" in description and old_field.primary_key:
                # Restore a primary key, if needed.
                if new_field.primary_key:
    def _alter_field_type_workaround(self, model, old_field, new_field):
        Oracle refuses to change from some type to other type.
        What we need to do instead is:
        - Add a nullable version of the desired field with a temporary name. If
          the new column is an auto field, then the temporary column can't be
          nullable.
        - Update the table to transfer values from old to new
        - Drop old column
        - Rename the new column and possibly drop the nullable property
        # Make a new field that's like the new one but with a temporary
        # column name.
        new_temp_field = copy.deepcopy(new_field)
        new_temp_field.null = new_field.get_internal_type() not in (
        new_temp_field.column = self._generate_temp_name(new_field.column)
        # Add it
        self.add_field(model, new_temp_field)
        # Explicit data type conversion
        # https://docs.oracle.com/en/database/oracle/oracle-database/21/sqlrf
        # /Data-Type-Comparison-Rules.html#GUID-D0C5A47E-6F93-4C2D-9E49-4F2B86B359DD
        new_value = self.quote_name(old_field.column)
        old_type = old_field.db_type(self.connection)
        if re.match("^N?CLOB", old_type):
            new_value = "TO_CHAR(%s)" % new_value
            old_type = "VARCHAR2"
        if re.match("^N?VARCHAR2", old_type):
            new_internal_type = new_field.get_internal_type()
            if new_internal_type == "DateField":
                new_value = "TO_DATE(%s, 'YYYY-MM-DD')" % new_value
            elif new_internal_type == "DateTimeField":
                new_value = "TO_TIMESTAMP(%s, 'YYYY-MM-DD HH24:MI:SS.FF')" % new_value
            elif new_internal_type == "TimeField":
                # TimeField are stored as TIMESTAMP with a 1900-01-01 date
                # part.
                new_value = "CONCAT('1900-01-01 ', %s)" % new_value
        # Transfer values across
            "UPDATE %s set %s=%s"
                self.quote_name(model._meta.db_table),
                self.quote_name(new_temp_field.column),
                new_value,
        # Drop the old field
        self.remove_field(model, old_field)
        # Rename and possibly make the new field NOT NULL
        super().alter_field(model, new_temp_field, new_field)
        # Recreate foreign key (if necessary) because the old field is not
        # passed to the alter_field() and data types of new_temp_field and
        # new_field always match.
        new_type = new_field.db_type(self.connection)
        ) and old_type != new_type:
            for _, rel in _related_non_m2m_objects(new_temp_field, new_field):
        auto_field_types = {"AutoField", "BigAutoField", "SmallAutoField"}
        # Drop the identity if migrating away from AutoField.
            old_field.get_internal_type() in auto_field_types
            and new_field.get_internal_type() not in auto_field_types
            and self._is_identity_column(model._meta.db_table, new_field.column)
            self._drop_identity(model._meta.db_table, new_field.column)
    def normalize_name(self, name):
        Get the properly shortened and uppercased identifier as returned by
        quote_name() but without the quotes.
        nn = self.quote_name(name)
        if nn[0] == '"' and nn[-1] == '"':
            nn = nn[1:-1]
        return nn
    def _generate_temp_name(self, for_name):
        """Generate temporary names for workarounds that need temp columns."""
        suffix = hex(hash(for_name)).upper()[1:]
        return self.normalize_name(for_name + "_" + suffix)
        return self.quote_value(value)
        create_index = super()._field_should_be_indexed(model, field)
        return create_index
    def _is_identity_column(self, table_name, column_name):
        if not column_name:
                    CASE WHEN identity_column = 'YES' THEN 1 ELSE 0 END
                FROM user_tab_cols
                WHERE table_name = %s AND
                      column_name = %s
                [self.normalize_name(table_name), self.normalize_name(column_name)],
            return row[0] if row else False
    def _drop_identity(self, table_name, column_name):
            "ALTER TABLE %(table)s MODIFY %(column)s DROP IDENTITY"
                "table": self.quote_name(table_name),
                "column": self.quote_name(column_name),
    def _get_default_collation(self, table_name):
                SELECT default_collation FROM user_tables WHERE table_name = %s
                [self.normalize_name(table_name)],
            return cursor.fetchone()[0]
        if collation is None and old_collation is not None:
            collation = self._get_default_collation(table_name)
        return super()._collate_sql(collation, old_collation, table_name)
        return "MATERIALIZED" if field.db_persist else "VIRTUAL"
from django.db.backends.ddl_references import IndexColumns
from django.db.backends.postgresql.psycopg_any import sql
from django.db.backends.utils import strip_quotes
    # Setting all constraints to IMMEDIATE to allow changing data in the same
    # transaction.
        "; SET CONSTRAINTS ALL IMMEDIATE"
    sql_alter_sequence_type = "ALTER SEQUENCE IF EXISTS %(sequence)s AS %(type)s"
    sql_delete_sequence = "DROP SEQUENCE IF EXISTS %(sequence)s CASCADE"
        "CREATE INDEX %(name)s ON %(table)s%(using)s "
    sql_create_index_concurrently = (
        "CREATE INDEX CONCURRENTLY %(name)s ON %(table)s%(using)s "
    sql_delete_index = "DROP INDEX IF EXISTS %(name)s"
    sql_delete_index_concurrently = "DROP INDEX CONCURRENTLY IF EXISTS %(name)s"
    # Setting the constraint to IMMEDIATE to allow changing data in the same
        "CONSTRAINT %(name)s REFERENCES %(to_table)s(%(to_column)s)%(on_delete_db)s"
        "%(deferrable)s; SET CONSTRAINTS %(namespace)s%(name)s IMMEDIATE"
    # Setting the constraint to IMMEDIATE runs any deferred checks to allow
    # dropping it in the same transaction.
    sql_delete_fk = (
        "SET CONSTRAINTS %(name)s IMMEDIATE; "
        "ALTER TABLE %(table)s DROP CONSTRAINT %(name)s"
    sql_delete_procedure = "DROP FUNCTION %(procedure)s(%(param_types)s)"
        # Merge the query client-side, as PostgreSQL won't do it server-side.
        sql = self.connection.ops.compose_sql(str(sql), params)
        # Don't let the superclass touch anything.
        return super().execute(sql, None)
    sql_add_identity = (
        "ALTER TABLE %(table)s ALTER COLUMN %(column)s ADD "
        "GENERATED BY DEFAULT AS IDENTITY"
    sql_drop_indentity = (
        "ALTER TABLE %(table)s ALTER COLUMN %(column)s DROP IDENTITY IF EXISTS"
        return sql.quote(value, self.connection.connection)
        output = super()._field_indexes_sql(model, field)
        like_index_statement = self._create_like_index_sql(model, field)
        if like_index_statement is not None:
            output.append(like_index_statement)
    def _field_data_type(self, field):
        if field.is_relation:
            return field.rel_db_type(self.connection)
        return self.connection.data_types.get(
            field.get_internal_type(),
            field.db_type(self.connection),
    def _field_base_data_types(self, field):
        # Yield base data types for array fields.
        if field.base_field.get_internal_type() == "ArrayField":
            yield from self._field_base_data_types(field.base_field)
            yield self._field_data_type(field.base_field)
    def _create_like_index_sql(self, model, field):
        Return the statement to create an index with varchar operator pattern
        when the column type is 'varchar' or 'text', otherwise return None.
        db_type = field.db_type(connection=self.connection)
        if db_type is not None and (field.db_index or field.unique):
            # Fields with database column types of `varchar` and `text` need
            # a second index that specifies their operator class, which is
            # needed when performing correct LIKE queries outside the
            # C locale. See #12234.
            # The same doesn't apply to array fields such as varchar[size]
            # and text[size], so skip them.
            if "[" in db_type:
            # Non-deterministic collations on Postgresql don't support indexes
            # for operator classes varchar_pattern_ops/text_pattern_ops.
            collation_name = getattr(field, "db_collation", None)
            if not collation_name and field.is_relation:
                collation_name = getattr(field.target_field, "db_collation", None)
            if collation_name and not self._is_collation_deterministic(collation_name):
            if db_type.startswith("varchar"):
                return self._create_index_sql(
                    fields=[field],
                    suffix="_like",
                    opclasses=["varchar_pattern_ops"],
            elif db_type.startswith("text"):
                    opclasses=["text_pattern_ops"],
    def _using_sql(self, new_field, old_field):
        if new_field.generated:
        using_sql = " USING %(column)s::%(type)s"
        old_internal_type = old_field.get_internal_type()
        if new_internal_type == "ArrayField" and new_internal_type == old_internal_type:
            # Compare base data types for array fields.
            if list(self._field_base_data_types(old_field)) != list(
                self._field_base_data_types(new_field)
                return using_sql
        elif self._field_data_type(old_field) != self._field_data_type(new_field):
    def _get_sequence_name(self, table, column):
            for sequence in self.connection.introspection.get_sequences(cursor, table):
                if sequence["column"] == column:
                    return sequence["name"]
    def _is_changing_type_of_indexed_text_column(self, old_field, old_type, new_type):
        return (old_field.db_index or old_field.unique) and (
            (old_type.startswith("varchar") and not new_type.startswith("varchar"))
            or (old_type.startswith("text") and not new_type.startswith("text"))
            or (old_type.startswith("citext") and not new_type.startswith("citext"))
        # Drop indexes on varchar/text/citext columns that are changing to a
        # different type.
        if self._is_changing_type_of_indexed_text_column(old_field, old_type, new_type):
            index_name = self._create_index_name(
                model._meta.db_table, [old_field.column], suffix="_like"
        self.sql_alter_column_type = (
            "ALTER COLUMN %(column)s TYPE %(type)s%(collation)s"
        # Cast when data type changed.
        if using_sql := self._using_sql(new_field, old_field):
            self.sql_alter_column_type += using_sql
        # Make ALTER TYPE with IDENTITY make sense.
        table = strip_quotes(model._meta.db_table)
        auto_field_types = {
        old_is_auto = old_internal_type in auto_field_types
        new_is_auto = new_internal_type in auto_field_types
        if new_is_auto and not old_is_auto:
            column = strip_quotes(new_field.column)
                        "column": self.quote_name(column),
                        self.sql_add_identity
        elif old_is_auto and not new_is_auto:
            # Drop IDENTITY if exists (pre-Django 4.1 serial columns don't have
            # it).
                self.sql_drop_indentity
                    "column": self.quote_name(strip_quotes(new_field.column)),
            fragment, _ = super()._alter_column_type_sql(
            # Drop the sequence if exists (Django 4.1+ identity columns don't
            # have it).
            if sequence_name := self._get_sequence_name(table, column):
                other_actions = [
                        self.sql_delete_sequence
                            "sequence": self.quote_name(sequence_name),
            return fragment, other_actions
        elif new_is_auto and old_is_auto and old_internal_type != new_internal_type:
            db_types = {
            # Alter the sequence type if exists (Django 4.1+ identity columns
            # don't have it).
                        self.sql_alter_sequence_type
                            "type": db_types[new_internal_type],
        # Added an index? Create any PostgreSQL-specific indexes.
            (not (old_field.db_index or old_field.unique) and new_field.db_index)
            or (not old_field.unique and new_field.unique)
                self._is_changing_type_of_indexed_text_column(
                    old_field, old_type, new_type
            like_index_statement = self._create_like_index_sql(model, new_field)
                self.execute(like_index_statement)
        # Removed an index? Drop any PostgreSQL-specific indexes.
        if old_field.unique and not (new_field.db_index or new_field.unique):
            index_to_remove = self._create_index_name(
            self.execute(self._delete_index_sql(model, index_to_remove))
        if opclasses:
            return IndexColumns(
                col_suffixes=col_suffixes,
        return super()._index_columns(table, columns, col_suffixes, opclasses)
    def add_index(self, model, index, concurrently=False):
            index.create_sql(model, self, concurrently=concurrently), params=None
    def remove_index(self, model, index, concurrently=False):
        self.execute(index.remove_sql(model, self, concurrently=concurrently))
    def _delete_index_sql(self, model, name, sql=None, concurrently=False):
        sql = sql or (
            self.sql_delete_index_concurrently
            if concurrently
            else self.sql_delete_index
        return super()._delete_index_sql(model, name, sql)
        concurrently=False,
            self.sql_create_index
            if not concurrently
            else self.sql_create_index_concurrently
            suffix=suffix,
            db_tablespace=db_tablespace,
            sql=sql,
    def _is_collation_deterministic(self, collation_name):
                SELECT collisdeterministic
                FROM pg_collation
                WHERE collname = %s
                [collation_name],
            return row[0] if row else None
from decimal import Decimal
from django.apps.registry import Apps
from django.db.backends.ddl_references import Statement
from django.db.models import CompositePrimaryKey, UniqueConstraint
    sql_delete_table = "DROP TABLE %(table)s"
    sql_create_fk = None
    sql_create_inline_fk = (
        "REFERENCES %(to_table)s (%(to_column)s)%(on_delete_db)s DEFERRABLE INITIALLY "
        "DEFERRED"
    sql_create_column_inline_fk = sql_create_inline_fk
    sql_create_unique = "CREATE UNIQUE INDEX %(name)s ON %(table)s (%(columns)s)"
    sql_delete_unique = "DROP INDEX %(name)s"
    sql_alter_table_comment = None
        # Some SQLite schema alterations need foreign key constraints to be
        # disabled. Enforce it here for the duration of the schema edition.
        if not self.connection.disable_constraint_checking():
                "SQLite schema editor cannot be used while foreign key "
                "constraint checks are enabled. Make sure to disable them "
                "before entering a transaction.atomic() context because "
                "SQLite does not support disabling them in the middle of "
                "a multi-statement transaction."
        return super().__enter__()
        self.connection.check_constraints()
        super().__exit__(exc_type, exc_value, traceback)
        self.connection.enable_constraint_checking()
        # The backend "mostly works" without this function and there are use
        # cases for compiling Python without the sqlite3 libraries (e.g.
        # security hardening).
            value = sqlite3.adapt(value)
        except sqlite3.ProgrammingError:
        # Manual emulation of SQLite parameter quoting
        if isinstance(value, bool):
        elif isinstance(value, (Decimal, float, int)):
            return "NULL"
            # Bytes are only allowed for BLOB fields, encoded as string
            # literals containing hexadecimal data and preceded by a single "X"
            # character.
            return "X'%s'" % value.hex()
                "Cannot quote parameter value %r of type %s" % (value, type(value))
    def _remake_table(
        self, model, create_field=None, delete_field=None, alter_fields=None
        Shortcut to transform a model from old_model into new_model
        This follows the correct procedure to perform non-rename or column
        addition operations based on SQLite's documentation
        https://www.sqlite.org/lang_altertable.html#caution
        The essential steps are:
          1. Create a table with the updated definition called "new__app_model"
          2. Copy the data from the existing "app_model" table to the new table
          3. Drop the "app_model" table
          4. Rename the "new__app_model" table to "app_model"
          5. Restore any index of the previous "app_model" table.
        # Self-referential fields must be recreated rather than copied from
        # the old model to ensure their remote_field.field_name doesn't refer
        # to an altered field.
        def is_self_referential(f):
            return f.is_relation and f.remote_field.model is model
        # Work out the new fields dict / mapping
            f.name: f.clone() if is_self_referential(f) else f
            for f in model._meta.local_concrete_fields
        # Since CompositePrimaryKey is not a concrete field (column is None),
        # it's not copied by default.
            body[pk.name] = pk.clone()
        # Since mapping might mix column names and default values,
        # its values must be already quoted.
        mapping = {
            f.column: self.quote_name(f.column)
            if f.generated is False
        # This maps field names (not columns) for things like unique_together
        rename_mapping = {}
        # If any of the new or altered fields is introducing a new PK,
        # remove the old one
        restore_pk_field = None
        alter_fields = alter_fields or []
        if getattr(create_field, "primary_key", False) or any(
            getattr(new_field, "primary_key", False) for _, new_field in alter_fields
            for name, field in list(body.items()):
                if field.primary_key and not any(
                    # Do not remove the old primary key when an altered field
                    # that introduces a primary key is the same field.
                    name == new_field.name
                    for _, new_field in alter_fields
                    field.primary_key = False
                    restore_pk_field = field
                    if field.auto_created:
                        del body[name]
                        del mapping[field.column]
        # Add in any created fields
        if create_field:
            body[create_field.name] = create_field
            # Choose a default and insert it into the copy map
                not create_field.has_db_default()
                and not create_field.generated
                and create_field.concrete
                mapping[create_field.column] = self.prepare_default(
                    self.effective_default(create_field)
        # Add in any altered fields
        for alter_field in alter_fields:
            old_field, new_field = alter_field
            body.pop(old_field.name, None)
            mapping.pop(old_field.column, None)
            body[new_field.name] = new_field
            rename_mapping[old_field.name] = new_field.name
            if old_field.null and not new_field.null:
                    default = self.prepare_default(self.effective_default(new_field))
                    default, _ = self.db_default_sql(new_field)
                case_sql = "coalesce(%(col)s, %(default)s)" % {
                    "col": self.quote_name(old_field.column),
                mapping[new_field.column] = case_sql
                mapping[new_field.column] = self.quote_name(old_field.column)
        # Remove any deleted fields
        if delete_field:
            del body[delete_field.name]
            mapping.pop(delete_field.column, None)
            # Remove any implicit M2M tables
                delete_field.many_to_many
                and delete_field.remote_field.through._meta.auto_created
                return self.delete_model(delete_field.remote_field.through)
        # Work inside a new app registry
        apps = Apps()
        # Work out the new value of unique_together, taking renames into
        # account
        unique_together = [
            [rename_mapping.get(n, n) for n in unique]
            for unique in model._meta.unique_together
        indexes = model._meta.indexes
            indexes = [
                index for index in indexes if delete_field.name not in index.fields
        constraints = list(model._meta.constraints)
        # Provide isolated instances of the fields to the new model body so
        # that the existing model's internals aren't interfered with when
        # the dummy model is constructed.
        body_copy = copy.deepcopy(body)
        # Construct a new model with the new fields to allow self referential
        # primary key to resolve to. This model won't ever be materialized as a
        # table and solely exists for foreign key reference resolution
        # purposes. This wouldn't be required if the schema editor was
        # operating on model states instead of rendered models.
        meta_contents = {
            "app_label": model._meta.app_label,
            "db_table": model._meta.db_table,
            "unique_together": unique_together,
            "indexes": indexes,
            "constraints": constraints,
            "apps": apps,
        meta = type("Meta", (), meta_contents)
        body_copy["Meta"] = meta
        body_copy["__module__"] = model.__module__
        type(model._meta.object_name, model.__bases__, body_copy)
        # Construct a model with a renamed table name.
            "db_table": "new__%s" % strip_quotes(model._meta.db_table),
        new_model = type("New%s" % model._meta.object_name, model.__bases__, body_copy)
        # Remove the automatically recreated default primary key, if it has
        # been deleted.
        if delete_field and delete_field.attname == new_model._meta.pk.attname:
            auto_pk = new_model._meta.pk
            delattr(new_model, auto_pk.attname)
            new_model._meta.local_fields.remove(auto_pk)
            new_model.pk = None
        # Create a new table with the updated schema.
        self.create_model(new_model)
        # Copy data from the old table into the new table
            "INSERT INTO %s (%s) SELECT %s FROM %s"
                self.quote_name(new_model._meta.db_table),
                ", ".join(self.quote_name(x) for x in mapping),
                ", ".join(mapping.values()),
        # Delete the old table to make way for the new
        self.delete_model(model, handle_autom2m=False)
        # Rename the new table to take way for the old
            new_model,
            new_model._meta.db_table,
        # Run deferred SQL on correct table
        # Fix any PK-removed field
        if restore_pk_field:
            restore_pk_field.primary_key = True
    def delete_model(self, model, handle_autom2m=True):
        if handle_autom2m:
            # Delete the table (and only that)
        """Create a field on a model."""
        # Special-case implicit M2M tables.
        elif isinstance(field, CompositePrimaryKey):
            # If a CompositePrimaryKey field was added, the existing primary
            # key field had to be altered too, resulting in an AddField,
            # AlterField migration. The table cannot be re-created on AddField,
            # it would result in a duplicate primary key error.
            # Primary keys and unique fields are not supported in ALTER TABLE
            # ADD COLUMN.
            field.primary_key
            or field.unique
            or not field.null
            # Fields with default values cannot by handled by ALTER TABLE ADD
            # COLUMN statement because DROP DEFAULT is not supported in
            # ALTER TABLE.
            or self.effective_default(field) is not None
            # Fields with non-constant defaults cannot by handled by ALTER
            # TABLE ADD COLUMN statement.
            or (field.has_db_default() and not isinstance(field.db_default, Value))
            self._remake_table(model, create_field=field)
        # M2M fields are a special case
            # For implicit M2M tables, delete the auto-created table
            # For explicit "through" M2M fields, do nothing
            # Primary keys, unique fields, indexed fields, and foreign keys are
            # not supported in ALTER TABLE DROP COLUMN.
            not field.primary_key
            and not field.unique
            and not field.db_index
            and not (field.remote_field and field.db_constraint)
        # For everything else, remake.
        # Use "ALTER TABLE ... RENAME COLUMN" if only the column name
        # changed and there aren't any constraints.
            old_field.column != new_field.column
            and self.column_sql(model, old_field) == self.column_sql(model, new_field)
            and not (
                old_field.remote_field
                or new_field.remote_field
            return self.execute(
        # Alter by remaking table
        self._remake_table(model, alter_fields=[(old_field, new_field)])
        # Rebuild tables with FKs pointing to this field.
        if new_field.unique and (
            old_type != new_type or old_collation != new_collation
            related_models = set()
            opts = new_field.model._meta
            for remote_field in opts.related_objects:
                # Ignore self-relationship since the table was already rebuilt.
                if remote_field.related_model == model:
                if not remote_field.many_to_many:
                    if remote_field.field_name == new_field.name:
                        related_models.add(remote_field.related_model)
                elif new_field.primary_key and remote_field.through._meta.auto_created:
                    related_models.add(remote_field.through)
                for many_to_many in opts.many_to_many:
                    # Ignore self-relationship since the table was already
                    # rebuilt.
                    if many_to_many.related_model == model:
                    if many_to_many.remote_field.through._meta.auto_created:
                        related_models.add(many_to_many.remote_field.through)
            for related_model in related_models:
                self._remake_table(related_model)
            == new_field.remote_field.through._meta.db_table
            # The field name didn't change, but some options did, so we have to
            # propagate this altering.
            self._remake_table(
                alter_fields=[
                        # The field that points to the target model is needed,
                        # so that table can be remade with the new m2m field -
                        # this is m2m_reverse_field_name().
                        # The field that points to the model itself is needed,
                        # so that table can be remade with the new self field -
                        # this is m2m_field_name().
                            old_field.m2m_field_name()
                            new_field.m2m_field_name()
        # Make a new through table
        self.create_model(new_field.remote_field.through)
        # Copy the data across
                self.quote_name(new_field.remote_field.through._meta.db_table),
                ", ".join(
                        new_field.m2m_column_name(),
                        new_field.m2m_reverse_name(),
                        old_field.m2m_column_name(),
                        old_field.m2m_reverse_name(),
                self.quote_name(old_field.remote_field.through._meta.db_table),
        # Delete the old through table
        self.delete_model(old_field.remote_field.through)
        if isinstance(constraint, UniqueConstraint) and (
            constraint.condition
            or constraint.contains_expressions
            or constraint.include
            or constraint.deferrable
            super().add_constraint(model, constraint)
            self._remake_table(model)
    def _collate_sql(self, collation):
        return "COLLATE " + collation
"""Module contains typedefs that are used with `Runnable` objects."""
from typing import TYPE_CHECKING, Any, Literal
class EventData(TypedDict, total=False):
    """Data associated with a streaming event."""
    input: Any
    """The input passed to the `Runnable` that generated the event.
    Inputs will sometimes be available at the *START* of the `Runnable`, and
    sometimes at the *END* of the `Runnable`.
    If a `Runnable` is able to stream its inputs, then its input by definition
    won't be known until the *END* of the `Runnable` when it has finished streaming
    its inputs.
    error: NotRequired[BaseException]
    """The error that occurred during the execution of the `Runnable`.
    This field is only available if the `Runnable` raised an exception.
    output: Any
    """The output of the `Runnable` that generated the event.
    Outputs will only be available at the *END* of the `Runnable`.
    For most `Runnable` objects, this field can be inferred from the `chunk` field,
    though there might be some exceptions for special a cased `Runnable` (e.g., like
    chat models), which may return more information.
    chunk: Any
    """A streaming chunk from the output that generated the event.
    chunks support addition in general, and adding them up should result
    in the output of the `Runnable` that generated the event.
    tool_call_id: NotRequired[str | None]
    """The tool call ID associated with the tool execution.
    This field is available for the `on_tool_error` event and can be used to
    link errors to specific tool calls in stateless agent implementations.
class BaseStreamEvent(TypedDict):
    """Streaming event.
    Schema of a streaming event which is produced from the `astream_events` method.
        events = [event async for event in chain.astream_events("hello")]
        # (where some fields have been omitted for brevity):
    """Event names are of the format: `on_[runnable_type]_(start|stream|end)`.
    Runnable types are one of:
    - **llm** - used by non chat models
    - **chat_model** - used by chat models
    - **prompt** --  e.g., `ChatPromptTemplate`
    - **tool** -- from tools defined via `@tool` decorator or inheriting
        from `Tool`/`BaseTool`
    - **chain** - most `Runnable` objects are of this type
    Further, the events are categorized as one of:
    - **start** - when the `Runnable` starts
    - **stream** - when the `Runnable` is streaming
    - **end* - when the `Runnable` ends
    start, stream and end are associated with slightly different `data` payload.
    Please see the documentation for `EventData` for more details.
    """An randomly generated ID to keep track of the execution of the given `Runnable`.
    Each child `Runnable` that gets invoked as part of the execution of a parent
    `Runnable` is assigned its own unique ID.
    tags: NotRequired[list[str]]
    """Tags associated with the `Runnable` that generated this event.
    Tags are always inherited from parent `Runnable` objects.
    Tags can either be bound to a `Runnable` using `.with_config({"tags":  ["hello"]})`
    or passed at run time using `.astream_events(..., {"tags": ["hello"]})`.
    metadata: NotRequired[dict[str, Any]]
    """Metadata associated with the `Runnable` that generated this event.
    Metadata can either be bound to a `Runnable` using
        `.with_config({"metadata": { "foo": "bar" }})`
    or passed at run time using
        `.astream_events(..., {"metadata": {"foo": "bar"}})`.
    parent_ids: Sequence[str]
    """A list of the parent IDs associated with this event.
    Root Events will have an empty list.
    For example, if a `Runnable` A calls `Runnable` B, then the event generated by
    `Runnable` B will have `Runnable` A's ID in the `parent_ids` field.
    The order of the parent IDs is from the root parent to the immediate parent.
    Only supported as of v2 of the astream events API. v1 will return an empty list.
class StandardStreamEvent(BaseStreamEvent):
    """A standard stream event that follows LangChain convention for event data."""
    data: EventData
    """Event data.
    The contents of the event data depend on the event type.
    """The name of the `Runnable` that generated the event."""
class CustomStreamEvent(BaseStreamEvent):
    """Custom stream event created by the user."""
    # Overwrite the event field to be more specific.
    event: Literal["on_custom_event"]  # type: ignore[misc]
    """The event type."""
    """User defined name for the event."""
    data: Any
    """The data associated with the event. Free form and can be anything."""
StreamEvent = StandardStreamEvent | CustomStreamEvent
from langchain_core.agents import AgentAction
class AgentScratchPadChatPromptTemplate(ChatPromptTemplate):
    """Chat prompt template for the agent scratchpad."""
    def _construct_agent_scratchpad(
        intermediate_steps: list[tuple[AgentAction, str]],
        if len(intermediate_steps) == 0:
        thoughts = ""
        for action, observation in intermediate_steps:
            thoughts += action.log
            thoughts += f"\nObservation: {observation}\nThought: "
            f"This was your previous work "
            f"(but I haven't seen any of it! I only see what "
            f"you return as final answer):\n{thoughts}"
        intermediate_steps = kwargs.pop("intermediate_steps")
        kwargs["agent_scratchpad"] = self._construct_agent_scratchpad(
            intermediate_steps,
from pydantic import BaseModel, ConfigDict
class AttributeInfo(BaseModel):
    """Information about a data source attribute."""
"""Interfaces to be implemented by general evaluators."""
from warnings import warn
class EvaluatorType(str, Enum):
    """The types of the evaluators."""
    QA = "qa"
    """Question answering evaluator, which grades answers to questions
    directly using an LLM."""
    COT_QA = "cot_qa"
    """Chain of thought question answering evaluator, which grades
    answers to questions using
    chain of thought 'reasoning'."""
    CONTEXT_QA = "context_qa"
    """Question answering evaluator that incorporates 'context' in the response."""
    PAIRWISE_STRING = "pairwise_string"
    """The pairwise string evaluator, which predicts the preferred prediction from
    between two models."""
    SCORE_STRING = "score_string"
    """The scored string evaluator, which gives a score between 1 and 10
    to a prediction."""
    LABELED_PAIRWISE_STRING = "labeled_pairwise_string"
    """The labeled pairwise string evaluator, which predicts the preferred prediction
    from between two models based on a ground truth reference label."""
    LABELED_SCORE_STRING = "labeled_score_string"
    """The labeled scored string evaluator, which gives a score between 1 and 10
    to a prediction based on a ground truth reference label."""
    AGENT_TRAJECTORY = "trajectory"
    """The agent trajectory evaluator, which grades the agent's intermediate steps."""
    CRITERIA = "criteria"
    """The criteria evaluator, which evaluates a model based on a
    custom set of criteria without any reference labels."""
    LABELED_CRITERIA = "labeled_criteria"
    """The labeled criteria evaluator, which evaluates a model based on a
    custom set of criteria, with a reference label."""
    STRING_DISTANCE = "string_distance"
    """Compare predictions to a reference answer using string edit distances."""
    EXACT_MATCH = "exact_match"
    """Compare predictions to a reference answer using exact matching."""
    REGEX_MATCH = "regex_match"
    """Compare predictions to a reference answer using regular expressions."""
    PAIRWISE_STRING_DISTANCE = "pairwise_string_distance"
    """Compare predictions based on string edit distances."""
    EMBEDDING_DISTANCE = "embedding_distance"
    """Compare a prediction to a reference label using embedding distance."""
    PAIRWISE_EMBEDDING_DISTANCE = "pairwise_embedding_distance"
    """Compare two predictions using embedding distance."""
    JSON_VALIDITY = "json_validity"
    """Check if a prediction is valid JSON."""
    JSON_EQUALITY = "json_equality"
    """Check if a prediction is equal to a reference JSON."""
    JSON_EDIT_DISTANCE = "json_edit_distance"
    """Compute the edit distance between two JSON strings after canonicalization."""
    JSON_SCHEMA_VALIDATION = "json_schema_validation"
    """Check if a prediction is valid JSON according to a JSON schema."""
class LLMEvalChain(Chain):
    """A base class for evaluators that use an LLM."""
    def from_llm(cls, llm: BaseLanguageModel, **kwargs: Any) -> LLMEvalChain:
        """Create a new evaluator from an LLM."""
class _EvalArgsMixin:
    """Mixin for checking evaluation arguments."""
    def requires_reference(self) -> bool:
        """Whether this evaluator requires a reference label."""
    def requires_input(self) -> bool:
        """Whether this evaluator requires an input string."""
    def _skip_input_warning(self) -> str:
        """Warning to show when input is ignored."""
        return f"Ignoring input in {self.__class__.__name__}, as it is not expected."
    def _skip_reference_warning(self) -> str:
        """Warning to show when reference is ignored."""
            f"Ignoring reference in {self.__class__.__name__}, as it is not expected."
    def _check_evaluation_args(
        reference: str | None = None,
        input_: str | None = None,
        """Check if the evaluation arguments are valid.
            reference: The reference label.
            input_: The input string.
            ValueError: If the evaluator requires an input string but none is provided,
                or if the evaluator requires a reference label but none is provided.
        if self.requires_input and input_ is None:
            msg = f"{self.__class__.__name__} requires an input string."
        if input_ is not None and not self.requires_input:
            warn(self._skip_input_warning, stacklevel=3)
        if self.requires_reference and reference is None:
            msg = f"{self.__class__.__name__} requires a reference string."
        if reference is not None and not self.requires_reference:
            warn(self._skip_reference_warning, stacklevel=3)
class StringEvaluator(_EvalArgsMixin, ABC):
    """String evaluator interface.
    Grade, tag, or otherwise evaluate predictions relative to their inputs
    and/or reference labels.
    def evaluation_name(self) -> str:
    def _evaluate_strings(
        prediction: str | Any,
        reference: str | Any | None = None,
        input: str | Any | None = None,  # noqa: A002
        """Evaluate Chain or LLM output, based on optional input and label.
            prediction: The LLM or chain prediction to evaluate.
            reference: The reference label to evaluate against.
            input: The input to consider during evaluation.
            **kwargs: Additional keyword arguments, including callbacks, tags, etc.
            The evaluation results containing the score or value.
            It is recommended that the dictionary contain the following keys:
                 - score: the score of the evaluation, if applicable.
                 - value: the string value of the evaluation, if applicable.
                 - reasoning: the reasoning for the evaluation, if applicable.
    async def _aevaluate_strings(
        """Asynchronously evaluate Chain or LLM output, based on optional input and label.
            self._evaluate_strings,
            reference=reference,
    def evaluate_strings(
        prediction: str,
        input: str | None = None,  # noqa: A002
        self._check_evaluation_args(reference=reference, input_=input)
        return self._evaluate_strings(
    async def aevaluate_strings(
        return await self._aevaluate_strings(
class PairwiseStringEvaluator(_EvalArgsMixin, ABC):
    """Compare the output of two models (or two outputs of the same model)."""
    def _evaluate_string_pairs(
        prediction_b: str,
        """Evaluate the output string pairs.
            prediction: The output string from the first model.
            prediction_b: The output string from the second model.
            reference: The expected output / reference string.
            input: The input string.
            **kwargs: Additional keyword arguments, such as callbacks and optional reference strings.
            `dict` containing the preference, scores, and/or other information.
    async def _aevaluate_string_pairs(
        """Asynchronously evaluate the output string pairs.
            self._evaluate_string_pairs,
            prediction_b=prediction_b,
    def evaluate_string_pairs(
        return self._evaluate_string_pairs(
    async def aevaluate_string_pairs(
        return await self._aevaluate_string_pairs(
class AgentTrajectoryEvaluator(_EvalArgsMixin, ABC):
    """Interface for evaluating agent trajectories."""
    def _evaluate_agent_trajectory(
        agent_trajectory: Sequence[tuple[AgentAction, str]],
        input: str,  # noqa: A002
        """Evaluate a trajectory.
            prediction: The final predicted response.
            agent_trajectory:
                The intermediate steps forming the agent trajectory.
            input: The input to the agent.
            reference: The reference answer.
            The evaluation result.
    async def _aevaluate_agent_trajectory(
        """Asynchronously evaluate a trajectory.
            self._evaluate_agent_trajectory,
            agent_trajectory=agent_trajectory,
    def evaluate_agent_trajectory(
        return self._evaluate_agent_trajectory(
    async def aevaluate_agent_trajectory(
        return await self._aevaluate_agent_trajectory(
    from langchain_community.vectorstores.redis.schema import (
        FlatVectorField,
        HNSWVectorField,
        NumericFieldSchema,
        RedisDistanceMetric,
        RedisField,
        RedisModel,
        RedisVectorField,
        TagFieldSchema,
        TextFieldSchema,
        read_schema,
    "RedisDistanceMetric": "langchain_community.vectorstores.redis.schema",
    "RedisField": "langchain_community.vectorstores.redis.schema",
    "TextFieldSchema": "langchain_community.vectorstores.redis.schema",
    "TagFieldSchema": "langchain_community.vectorstores.redis.schema",
    "NumericFieldSchema": "langchain_community.vectorstores.redis.schema",
    "RedisVectorField": "langchain_community.vectorstores.redis.schema",
    "FlatVectorField": "langchain_community.vectorstores.redis.schema",
    "HNSWVectorField": "langchain_community.vectorstores.redis.schema",
    "RedisModel": "langchain_community.vectorstores.redis.schema",
    "read_schema": "langchain_community.vectorstores.redis.schema",
    "FlatVectorField",
    "HNSWVectorField",
    "NumericFieldSchema",
    "RedisDistanceMetric",
    "RedisField",
    "RedisModel",
    "RedisVectorField",
    "TagFieldSchema",
    "TextFieldSchema",
    "read_schema",
"""The `schema` module is a backport module from V1."""
from datetime import date, datetime, time, timedelta
    FrozenSet,
    SHAPE_DEQUE,
    SHAPE_FROZENSET,
    SHAPE_GENERIC,
    SHAPE_ITERABLE,
    SHAPE_LIST,
    SHAPE_SEQUENCE,
    SHAPE_SET,
    SHAPE_SINGLETON,
    SHAPE_TUPLE,
    SHAPE_TUPLE_ELLIPSIS,
    FieldInfo,
from pydantic.v1.networks import AnyUrl, EmailStr
from pydantic.v1.types import (
    ConstrainedDecimal,
    ConstrainedFloat,
    ConstrainedFrozenSet,
    ConstrainedInt,
    ConstrainedList,
    ConstrainedSet,
    ConstrainedStr,
    SecretBytes,
    StrictBytes,
    StrictStr,
    conbytes,
    condecimal,
    confloat,
    confrozenset,
    conint,
    conlist,
    conset,
    constr,
    all_literal_values,
    get_sub_types,
    is_callable_type,
    is_none_type,
from pydantic.v1.utils import ROOT_KEY, get_model, lenient_issubclass
    from pydantic.v1.dataclasses import Dataclass
default_prefix = '#/definitions/'
default_ref_template = '#/definitions/{model}'
TypeModelOrEnum = Union[Type['BaseModel'], Type[Enum]]
TypeModelSet = Set[TypeModelOrEnum]
def _apply_modify_schema(
    modify_schema: Callable[..., None], field: Optional[ModelField], field_schema: Dict[str, Any]
    sig = signature(modify_schema)
    args = set(sig.parameters.keys())
    if 'field' in args or 'kwargs' in args:
        modify_schema(field_schema, field=field)
        modify_schema(field_schema)
def schema(
    models: Sequence[Union[Type['BaseModel'], Type['Dataclass']]],
    description: Optional[str] = None,
    ref_prefix: Optional[str] = None,
    ref_template: str = default_ref_template,
    Process a list of models and generate a single JSON Schema with all of them defined in the ``definitions``
    top-level JSON key, including their sub-models.
    :param models: a list of models to include in the generated JSON Schema
    :param by_alias: generate the schemas using the aliases defined, if any
    :param title: title for the generated schema that includes the definitions
    :param description: description for the generated schema
    :param ref_prefix: the JSON Pointer prefix for schema references with ``$ref``, if None, will be set to the
      default of ``#/definitions/``. Update it if you want the schemas to reference the definitions somewhere
      else, e.g. for OpenAPI use ``#/components/schemas/``. The resulting generated schemas will still be at the
      top-level key ``definitions``, so you can extract them from there. But all the references will have the set
    :param ref_template: Use a ``string.format()`` template for ``$ref`` instead of a prefix. This can be useful
      for references that cannot be represented by ``ref_prefix`` such as a definition stored in another file. For
      a sibling json file in a ``/schemas`` directory use ``"/schemas/${model}.json#"``.
    :return: dict with the JSON Schema with a ``definitions`` top-level key including the schema definitions for
      the models and sub-models passed in ``models``.
    clean_models = [get_model(model) for model in models]
    flat_models = get_flat_models_from_models(clean_models)
    definitions = {}
    output_schema: Dict[str, Any] = {}
        output_schema['title'] = title
        output_schema['description'] = description
    for model in clean_models:
        m_schema, m_definitions, m_nested_models = model_process_schema(
            ref_prefix=ref_prefix,
        definitions.update(m_definitions)
        model_name = model_name_map[model]
        definitions[model_name] = m_schema
        output_schema['definitions'] = definitions
    return output_schema
def model_schema(
    model: Union[Type['BaseModel'], Type['Dataclass']],
    Generate a JSON Schema for one model. With all the sub-models defined in the ``definitions`` top-level
    JSON key.
    :param model: a Pydantic model (a class that inherits from BaseModel)
    :param ref_template: Use a ``string.format()`` template for ``$ref`` instead of a prefix. This can be useful for
      references that cannot be represented by ``ref_prefix`` such as a definition stored in another file. For a
      sibling json file in a ``/schemas`` directory use ``"/schemas/${model}.json#"``.
    :return: dict with the JSON Schema for the passed ``model``
    model = get_model(model)
    flat_models = get_flat_models_from_model(model)
    m_schema, m_definitions, nested_models = model_process_schema(
        model, by_alias=by_alias, model_name_map=model_name_map, ref_prefix=ref_prefix, ref_template=ref_template
    if model_name in nested_models:
        # model_name is in Nested models, it has circular references
        m_definitions[model_name] = m_schema
        m_schema = get_schema_ref(model_name, ref_prefix, ref_template, False)
    if m_definitions:
        m_schema.update({'definitions': m_definitions})
    return m_schema
def get_field_info_schema(field: ModelField, schema_overrides: bool = False) -> Tuple[Dict[str, Any], bool]:
    # If no title is explicitly set, we don't set title in the schema for enums.
    # The behaviour is the same as `BaseModel` reference, where the default title
    # is in the definitions part of the schema.
    schema_: Dict[str, Any] = {}
    if field.field_info.title or not lenient_issubclass(field.type_, Enum):
        schema_['title'] = field.field_info.title or field.alias.title().replace('_', ' ')
    if field.field_info.title:
        schema_overrides = True
    if field.field_info.description:
        schema_['description'] = field.field_info.description
    if not field.required and field.default is not None and not is_callable_type(field.outer_type_):
        schema_['default'] = encode_default(field.default)
    return schema_, schema_overrides
def field_schema(
    field: ModelField,
    model_name_map: Dict[TypeModelOrEnum, str],
    known_models: Optional[TypeModelSet] = None,
) -> Tuple[Dict[str, Any], Dict[str, Any], Set[str]]:
    Process a Pydantic field and return a tuple with a JSON Schema for it as the first item.
    Also return a dictionary of definitions with models as keys and their schemas as values. If the passed field
    is a model and has sub-models, and those sub-models don't have overrides (as ``title``, ``default``, etc), they
    will be included in the definitions and referenced in the schema instead of included recursively.
    :param field: a Pydantic ``ModelField``
    :param by_alias: use the defined alias (if any) in the returned schema
    :param model_name_map: used to generate the JSON Schema references to other models included in the definitions
    :param ref_prefix: the JSON Pointer prefix to use for references to other schemas, if None, the default of
      #/definitions/ will be used
    :param known_models: used to solve circular references
    :return: tuple of the schema for this field and additional definitions
    s, schema_overrides = get_field_info_schema(field)
    validation_schema = get_field_schema_validations(field)
    if validation_schema:
        s.update(validation_schema)
    f_schema, f_definitions, f_nested_models = field_type_schema(
        schema_overrides=schema_overrides,
        known_models=known_models or set(),
    # $ref will only be returned when there are no schema_overrides
    if '$ref' in f_schema:
        return f_schema, f_definitions, f_nested_models
        s.update(f_schema)
        return s, f_definitions, f_nested_models
numeric_types = (int, float, Decimal)
_str_types_attrs: Tuple[Tuple[str, Union[type, Tuple[type, ...]], str], ...] = (
    ('max_length', numeric_types, 'maxLength'),
    ('min_length', numeric_types, 'minLength'),
    ('regex', str, 'pattern'),
_numeric_types_attrs: Tuple[Tuple[str, Union[type, Tuple[type, ...]], str], ...] = (
    ('gt', numeric_types, 'exclusiveMinimum'),
    ('lt', numeric_types, 'exclusiveMaximum'),
    ('ge', numeric_types, 'minimum'),
    ('le', numeric_types, 'maximum'),
    ('multiple_of', numeric_types, 'multipleOf'),
def get_field_schema_validations(field: ModelField) -> Dict[str, Any]:
    Get the JSON Schema validation keywords for a ``field`` with an annotation of
    a Pydantic ``FieldInfo`` with validation arguments.
    f_schema: Dict[str, Any] = {}
    if lenient_issubclass(field.type_, Enum):
        # schema is already updated by `enum_process_schema`; just update with field extra
        if field.field_info.extra:
            f_schema.update(field.field_info.extra)
        return f_schema
    if lenient_issubclass(field.type_, (str, bytes)):
        for attr_name, t, keyword in _str_types_attrs:
            attr = getattr(field.field_info, attr_name, None)
            if isinstance(attr, t):
                f_schema[keyword] = attr
    if lenient_issubclass(field.type_, numeric_types) and not issubclass(field.type_, bool):
        for attr_name, t, keyword in _numeric_types_attrs:
    if field.field_info is not None and field.field_info.const:
        f_schema['const'] = field.default
    modify_schema = getattr(field.outer_type_, '__modify_schema__', None)
    if modify_schema:
        _apply_modify_schema(modify_schema, field, f_schema)
def get_model_name_map(unique_models: TypeModelSet) -> Dict[TypeModelOrEnum, str]:
    Process a set of models and generate unique names for them to be used as keys in the JSON Schema
    definitions. By default the names are the same as the class name. But if two models in different Python
    modules have the same name (e.g. "users.Model" and "items.Model"), the generated names will be
    based on the Python module path for those conflicting models to prevent name collisions.
    :param unique_models: a Python set of models
    :return: dict mapping models to names
    name_model_map = {}
    conflicting_names: Set[str] = set()
    for model in unique_models:
        model_name = normalize_name(model.__name__)
        if model_name in conflicting_names:
            model_name = get_long_model_name(model)
            name_model_map[model_name] = model
        elif model_name in name_model_map:
            conflicting_names.add(model_name)
            conflicting_model = name_model_map.pop(model_name)
            name_model_map[get_long_model_name(conflicting_model)] = conflicting_model
            name_model_map[get_long_model_name(model)] = model
    return {v: k for k, v in name_model_map.items()}
def get_flat_models_from_model(model: Type['BaseModel'], known_models: Optional[TypeModelSet] = None) -> TypeModelSet:
    Take a single ``model`` and generate a set with itself and all the sub-models in the tree. I.e. if you pass
    model ``Foo`` (subclass of Pydantic ``BaseModel``) as ``model``, and it has a field of type ``Bar`` (also
    subclass of ``BaseModel``) and that model ``Bar`` has a field of type ``Baz`` (also subclass of ``BaseModel``),
    the return value will be ``set([Foo, Bar, Baz])``.
    :param model: a Pydantic ``BaseModel`` subclass
    :return: a set with the initial model and all its sub-models
    known_models = known_models or set()
    flat_models: TypeModelSet = set()
    flat_models.add(model)
    known_models |= flat_models
    fields = cast(Sequence[ModelField], model.__fields__.values())
    flat_models |= get_flat_models_from_fields(fields, known_models=known_models)
def get_flat_models_from_field(field: ModelField, known_models: TypeModelSet) -> TypeModelSet:
    Take a single Pydantic ``ModelField`` (from a model) that could have been declared as a subclass of BaseModel
    (so, it could be a submodel), and generate a set with its model and all the sub-models in the tree.
    I.e. if you pass a field that was declared to be of type ``Foo`` (subclass of BaseModel) as ``field``, and that
    model ``Foo`` has a field of type ``Bar`` (also subclass of ``BaseModel``) and that model ``Bar`` has a field of
    type ``Baz`` (also subclass of ``BaseModel``), the return value will be ``set([Foo, Bar, Baz])``.
    :return: a set with the model used in the declaration for this field, if any, and all its sub-models
    field_type = field.type_
    if lenient_issubclass(getattr(field_type, '__pydantic_model__', None), BaseModel):
        field_type = field_type.__pydantic_model__
    if field.sub_fields and not lenient_issubclass(field_type, BaseModel):
        flat_models |= get_flat_models_from_fields(field.sub_fields, known_models=known_models)
    elif lenient_issubclass(field_type, BaseModel) and field_type not in known_models:
        flat_models |= get_flat_models_from_model(field_type, known_models=known_models)
    elif lenient_issubclass(field_type, Enum):
        flat_models.add(field_type)
def get_flat_models_from_fields(fields: Sequence[ModelField], known_models: TypeModelSet) -> TypeModelSet:
    Take a list of Pydantic  ``ModelField``s (from a model) that could have been declared as subclasses of ``BaseModel``
    (so, any of them could be a submodel), and generate a set with their models and all the sub-models in the tree.
    I.e. if you pass a the fields of a model ``Foo`` (subclass of ``BaseModel``) as ``fields``, and on of them has a
    field of type ``Bar`` (also subclass of ``BaseModel``) and that model ``Bar`` has a field of type ``Baz`` (also
    subclass of ``BaseModel``), the return value will be ``set([Foo, Bar, Baz])``.
    :param fields: a list of Pydantic ``ModelField``s
    :return: a set with any model declared in the fields, and all their sub-models
        flat_models |= get_flat_models_from_field(field, known_models=known_models)
def get_flat_models_from_models(models: Sequence[Type['BaseModel']]) -> TypeModelSet:
    Take a list of ``models`` and generate a set with them and all their sub-models in their trees. I.e. if you pass
    a list of two models, ``Foo`` and ``Bar``, both subclasses of Pydantic ``BaseModel`` as models, and ``Bar`` has
    a field of type ``Baz`` (also subclass of ``BaseModel``), the return value will be ``set([Foo, Bar, Baz])``.
        flat_models |= get_flat_models_from_model(model)
def get_long_model_name(model: TypeModelOrEnum) -> str:
    return f'{model.__module__}__{model.__qualname__}'.replace('.', '__')
def field_type_schema(
    ref_template: str,
    schema_overrides: bool = False,
    known_models: TypeModelSet,
    Used by ``field_schema()``, you probably should be using that function.
    Take a single ``field`` and generate the schema for its type only, not including additional
    information as title, etc. Also return additional schema definitions, from sub-models.
    from pydantic.v1.main import BaseModel  # noqa: F811
    nested_models: Set[str] = set()
    f_schema: Dict[str, Any]
    if field.shape in {
        items_schema, f_definitions, f_nested_models = field_singleton_schema(
            known_models=known_models,
        definitions.update(f_definitions)
        nested_models.update(f_nested_models)
        f_schema = {'type': 'array', 'items': items_schema}
        if field.shape in {SHAPE_SET, SHAPE_FROZENSET}:
            f_schema['uniqueItems'] = True
    elif field.shape in MAPPING_LIKE_SHAPES:
        f_schema = {'type': 'object'}
        key_field = cast(ModelField, field.key_field)
        regex = getattr(key_field.type_, 'regex', None)
        if regex:
            # Dict keys have a regex pattern
            # items_schema might be a schema or empty dict, add it either way
            f_schema['patternProperties'] = {ConstrainedStr._get_pattern(regex): items_schema}
        if items_schema:
            # The dict values are not simply Any, so they need a schema
            f_schema['additionalProperties'] = items_schema
    elif field.shape == SHAPE_TUPLE or (field.shape == SHAPE_GENERIC and not issubclass(field.type_, BaseModel)):
        sub_schema = []
        sub_fields = cast(List[ModelField], field.sub_fields)
        for sf in sub_fields:
            sf_schema, sf_definitions, sf_nested_models = field_type_schema(
                sf,
            definitions.update(sf_definitions)
            nested_models.update(sf_nested_models)
            sub_schema.append(sf_schema)
        sub_fields_len = len(sub_fields)
        if field.shape == SHAPE_GENERIC:
            all_of_schemas = sub_schema[0] if sub_fields_len == 1 else {'type': 'array', 'items': sub_schema}
            f_schema = {'allOf': [all_of_schemas]}
            f_schema = {
                'type': 'array',
                'minItems': sub_fields_len,
                'maxItems': sub_fields_len,
            if sub_fields_len >= 1:
                f_schema['items'] = sub_schema
        assert field.shape in {SHAPE_SINGLETON, SHAPE_GENERIC}, field.shape
        f_schema, f_definitions, f_nested_models = field_singleton_schema(
    # check field type to avoid repeated calls to the same __modify_schema__ method
    if field.type_ != field.outer_type_:
            field_type = field.outer_type_
        modify_schema = getattr(field_type, '__modify_schema__', None)
    return f_schema, definitions, nested_models
def model_process_schema(
    model: TypeModelOrEnum,
    field: Optional[ModelField] = None,
    Used by ``model_schema()``, you probably should be using that function.
    Take a single ``model`` and generate its schema. Also return additional schema definitions, from sub-models. The
    sub-models of the returned schema will be referenced, but their definitions will not be included in the schema. All
    the definitions are returned as the second value.
    from inspect import getdoc, signature
    if lenient_issubclass(model, Enum):
        model = cast(Type[Enum], model)
        s = enum_process_schema(model, field=field)
        return s, {}, set()
    model = cast(Type['BaseModel'], model)
    s = {'title': model.__config__.title or model.__name__}
    doc = getdoc(model)
    if doc:
        s['description'] = doc
    known_models.add(model)
    m_schema, m_definitions, nested_models = model_type_schema(
    s.update(m_schema)
    schema_extra = model.__config__.schema_extra
    if callable(schema_extra):
        if len(signature(schema_extra).parameters) == 1:
            schema_extra(s)
            schema_extra(s, model)
        s.update(schema_extra)
    return s, m_definitions, nested_models
def model_type_schema(
    model: Type['BaseModel'],
    You probably should be using ``model_schema()``, this function is indirectly used by that function.
    Take a single ``model`` and generate the schema for its type only, not including additional
    properties = {}
    required = []
    definitions: Dict[str, Any] = {}
    for k, f in model.__fields__.items():
            f_schema, f_definitions, f_nested_models = field_schema(
        except SkipField as skip:
            warnings.warn(skip.message, UserWarning)
        if by_alias:
            properties[f.alias] = f_schema
            if f.required:
                required.append(f.alias)
            properties[k] = f_schema
                required.append(k)
    if ROOT_KEY in properties:
        out_schema = properties[ROOT_KEY]
        out_schema['title'] = model.__config__.title or model.__name__
        out_schema = {'type': 'object', 'properties': properties}
            out_schema['required'] = required
    if model.__config__.extra == 'forbid':
        out_schema['additionalProperties'] = False
    return out_schema, definitions, nested_models
def enum_process_schema(enum: Type[Enum], *, field: Optional[ModelField] = None) -> Dict[str, Any]:
    Take a single `enum` and generate its schema.
    This is similar to the `model_process_schema` function, but applies to ``Enum`` objects.
    schema_: Dict[str, Any] = {
        'title': enum.__name__,
        # Python assigns all enums a default docstring value of 'An enumeration', so
        # all enums will have a description field even if not explicitly provided.
        'description': inspect.cleandoc(enum.__doc__ or 'An enumeration.'),
        # Add enum values and the enum field type to the schema.
        'enum': [item.value for item in cast(Iterable[Enum], enum)],
    add_field_type_to_schema(enum, schema_)
    modify_schema = getattr(enum, '__modify_schema__', None)
        _apply_modify_schema(modify_schema, field, schema_)
    return schema_
def field_singleton_sub_fields_schema(
    This function is indirectly used by ``field_schema()``, you probably should be using that function.
    Take a list of Pydantic ``ModelField`` from the declaration of a type with parameters, and generate their
    schema. I.e., fields used as "type parameters", like ``str`` and ``int`` in ``Tuple[str, int]``.
    if len(sub_fields) == 1:
        return field_type_schema(
            sub_fields[0],
        s: Dict[str, Any] = {}
        # https://github.com/OAI/OpenAPI-Specification/blob/master/versions/3.0.2.md#discriminator-object
        field_has_discriminator: bool = field.discriminator_key is not None
        if field_has_discriminator:
            assert field.sub_fields_mapping is not None
            discriminator_models_refs: Dict[str, Union[str, Dict[str, Any]]] = {}
            for discriminator_value, sub_field in field.sub_fields_mapping.items():
                if isinstance(discriminator_value, Enum):
                    discriminator_value = str(discriminator_value.value)
                # sub_field is either a `BaseModel` or directly an `Annotated` `Union` of many
                if is_union(get_origin(sub_field.type_)):
                    sub_models = get_sub_types(sub_field.type_)
                    discriminator_models_refs[discriminator_value] = {
                        model_name_map[sub_model]: get_schema_ref(
                            model_name_map[sub_model], ref_prefix, ref_template, False
                        for sub_model in sub_models
                    sub_field_type = sub_field.type_
                    if hasattr(sub_field_type, '__pydantic_model__'):
                        sub_field_type = sub_field_type.__pydantic_model__
                    discriminator_model_name = model_name_map[sub_field_type]
                    discriminator_model_ref = get_schema_ref(discriminator_model_name, ref_prefix, ref_template, False)
                    discriminator_models_refs[discriminator_value] = discriminator_model_ref['$ref']
            s['discriminator'] = {
                'propertyName': field.discriminator_alias if by_alias else field.discriminator_key,
                'mapping': discriminator_models_refs,
        sub_field_schemas = []
            sub_schema, sub_definitions, sub_nested_models = field_type_schema(
            definitions.update(sub_definitions)
            if schema_overrides and 'allOf' in sub_schema:
                # if the sub_field is a referenced schema we only need the referenced
                # object. Otherwise we will end up with several allOf inside anyOf/oneOf.
                # See https://github.com/pydantic/pydantic/issues/1209
                sub_schema = sub_schema['allOf'][0]
            if sub_schema.keys() == {'discriminator', 'oneOf'}:
                # we don't want discriminator information inside oneOf choices, this is dealt with elsewhere
                sub_schema.pop('discriminator')
            sub_field_schemas.append(sub_schema)
            nested_models.update(sub_nested_models)
        s['oneOf' if field_has_discriminator else 'anyOf'] = sub_field_schemas
        return s, definitions, nested_models
# Order is important, e.g. subclasses of str must go before str
# this is used only for standard library types, custom types should use __modify_schema__ instead
field_class_to_schema: Tuple[Tuple[Any, Dict[str, Any]], ...] = (
    (Path, {'type': 'string', 'format': 'path'}),
    (datetime, {'type': 'string', 'format': 'date-time'}),
    (date, {'type': 'string', 'format': 'date'}),
    (time, {'type': 'string', 'format': 'time'}),
    (timedelta, {'type': 'number', 'format': 'time-delta'}),
    (IPv4Network, {'type': 'string', 'format': 'ipv4network'}),
    (IPv6Network, {'type': 'string', 'format': 'ipv6network'}),
    (IPv4Interface, {'type': 'string', 'format': 'ipv4interface'}),
    (IPv6Interface, {'type': 'string', 'format': 'ipv6interface'}),
    (IPv4Address, {'type': 'string', 'format': 'ipv4'}),
    (IPv6Address, {'type': 'string', 'format': 'ipv6'}),
    (Pattern, {'type': 'string', 'format': 'regex'}),
    (str, {'type': 'string'}),
    (bytes, {'type': 'string', 'format': 'binary'}),
    (bool, {'type': 'boolean'}),
    (int, {'type': 'integer'}),
    (float, {'type': 'number'}),
    (Decimal, {'type': 'number'}),
    (UUID, {'type': 'string', 'format': 'uuid'}),
    (dict, {'type': 'object'}),
    (list, {'type': 'array', 'items': {}}),
    (tuple, {'type': 'array', 'items': {}}),
    (set, {'type': 'array', 'items': {}, 'uniqueItems': True}),
    (frozenset, {'type': 'array', 'items': {}, 'uniqueItems': True}),
json_scheme = {'type': 'string', 'format': 'json-string'}
def add_field_type_to_schema(field_type: Any, schema_: Dict[str, Any]) -> None:
    Update the given `schema` with the type-specific metadata for the given `field_type`.
    This function looks through `field_class_to_schema` for a class that matches the given `field_type`,
    and then modifies the given `schema` with the information from that type.
    for type_, t_schema in field_class_to_schema:
        # Fallback for `typing.Pattern` and `re.Pattern` as they are not a valid class
        if lenient_issubclass(field_type, type_) or field_type is type_ is Pattern:
            schema_.update(t_schema)
def get_schema_ref(name: str, ref_prefix: Optional[str], ref_template: str, schema_overrides: bool) -> Dict[str, Any]:
    if ref_prefix:
        schema_ref = {'$ref': ref_prefix + name}
        schema_ref = {'$ref': ref_template.format(model=name)}
    return {'allOf': [schema_ref]} if schema_overrides else schema_ref
def field_singleton_schema(  # noqa: C901 (ignore complexity)
    This function is indirectly used by ``field_schema()``, you should probably be using that function.
    Take a single Pydantic ``ModelField``, and return its schema and any additional definitions from sub-models.
    # Recurse into this field if it contains sub_fields and is NOT a
    # BaseModel OR that BaseModel is a const
    if field.sub_fields and (
        (field.field_info and field.field_info.const) or not lenient_issubclass(field_type, BaseModel)
        return field_singleton_sub_fields_schema(
    if field_type is Any or field_type is object or field_type.__class__ == TypeVar or get_origin(field_type) is type:
        return {}, definitions, nested_models  # no restrictions
    if is_none_type(field_type):
        return {'type': 'null'}, definitions, nested_models
    if is_callable_type(field_type):
        raise SkipField(f'Callable {field.name} was excluded from schema since JSON schema has no equivalent type.')
    if is_literal_type(field_type):
        values = tuple(x.value if isinstance(x, Enum) else x for x in all_literal_values(field_type))
        if len({v.__class__ for v in values}) > 1:
            return field_schema(
                multitypes_literal_field_for_schema(values, field),
        # All values have the same type
        field_type = values[0].__class__
        f_schema['enum'] = list(values)
        add_field_type_to_schema(field_type, f_schema)
        enum_name = model_name_map[field_type]
        f_schema, schema_overrides = get_field_info_schema(field, schema_overrides)
        f_schema.update(get_schema_ref(enum_name, ref_prefix, ref_template, schema_overrides))
        definitions[enum_name] = enum_process_schema(field_type, field=field)
    elif is_namedtuple(field_type):
        sub_schema, *_ = model_process_schema(
            field_type.__pydantic_model__,
        items_schemas = list(sub_schema['properties'].values())
        f_schema.update(
                'items': items_schemas,
                'minItems': len(items_schemas),
                'maxItems': len(items_schemas),
    elif not hasattr(field_type, '__pydantic_model__'):
    if f_schema:
    # Handle dataclass-based models
    if issubclass(field_type, BaseModel):
        model_name = model_name_map[field_type]
        if field_type not in known_models:
            sub_schema, sub_definitions, sub_nested_models = model_process_schema(
                field_type,
            definitions[model_name] = sub_schema
            nested_models.add(model_name)
        schema_ref = get_schema_ref(model_name, ref_prefix, ref_template, schema_overrides)
        return schema_ref, definitions, nested_models
    # For generics with no args
    args = get_args(field_type)
    if args is not None and not args and Generic in field_type.__bases__:
    raise ValueError(f'Value not declarable with JSON Schema, field: {field}')
def multitypes_literal_field_for_schema(values: Tuple[Any, ...], field: ModelField) -> ModelField:
    To support `Literal` with values of different types, we split it into multiple `Literal` with same type
    e.g. `Literal['qwe', 'asd', 1, 2]` becomes `Union[Literal['qwe', 'asd'], Literal[1, 2]]`
    literal_distinct_types = defaultdict(list)
    for v in values:
        literal_distinct_types[v.__class__].append(v)
    distinct_literals = (Literal[tuple(same_type_values)] for same_type_values in literal_distinct_types.values())
    return ModelField(
        name=field.name,
        type_=Union[tuple(distinct_literals)],  # type: ignore
        class_validators=field.class_validators,
        model_config=field.model_config,
        default=field.default,
        required=field.required,
        alias=field.alias,
        field_info=field.field_info,
def encode_default(dft: Any) -> Any:
    if isinstance(dft, BaseModel) or is_dataclass(dft):
        dft = cast('dict[str, Any]', pydantic_encoder(dft))
    if isinstance(dft, dict):
        return {encode_default(k): encode_default(v) for k, v in dft.items()}
    elif isinstance(dft, Enum):
        return dft.value
    elif isinstance(dft, (int, float, str)):
        return dft
    elif isinstance(dft, (list, tuple)):
        t = dft.__class__
        seq_args = (encode_default(v) for v in dft)
        return t(*seq_args) if is_namedtuple(t) else t(seq_args)
    elif dft is None:
        return pydantic_encoder(dft)
_map_types_constraint: Dict[Any, Callable[..., type]] = {int: conint, float: confloat, Decimal: condecimal}
def get_annotation_from_field_info(
    annotation: Any, field_info: FieldInfo, field_name: str, validate_assignment: bool = False
) -> Type[Any]:
    Get an annotation with validation implemented for numbers and strings based on the field_info.
    :param annotation: an annotation from a field specification, as ``str``, ``ConstrainedStr``
    :param field_info: an instance of FieldInfo, possibly with declarations for validations and JSON Schema
    :param field_name: name of the field for use in error messages
    :param validate_assignment: default False, flag for BaseModel Config value of validate_assignment
    :return: the same ``annotation`` if unmodified or a new annotation with validation in place
    constraints = field_info.get_constraints()
    used_constraints: Set[str] = set()
    if constraints:
        annotation, used_constraints = get_annotation_with_constraints(annotation, field_info)
    if validate_assignment:
        used_constraints.add('allow_mutation')
    unused_constraints = constraints - used_constraints
    if unused_constraints:
            f'On field "{field_name}" the following field constraints are set but not enforced: '
            f'{", ".join(unused_constraints)}. '
            f'\nFor more details see https://docs.pydantic.dev/usage/schema/#unenforced-field-constraints'
def get_annotation_with_constraints(annotation: Any, field_info: FieldInfo) -> Tuple[Type[Any], Set[str]]:  # noqa: C901
    Get an annotation with used constraints implemented for numbers and strings based on the field_info.
    :return: the same ``annotation`` if unmodified or a new annotation along with the used constraints.
    def go(type_: Any) -> Type[Any]:
            is_literal_type(type_)
            or isinstance(type_, ForwardRef)
            or lenient_issubclass(type_, (ConstrainedList, ConstrainedSet, ConstrainedFrozenSet))
        origin = get_origin(type_)
            args: Tuple[Any, ...] = get_args(type_)
            if any(isinstance(a, ForwardRef) for a in args):
                # forward refs cause infinite recursion below
                return go(args[0])
                return Union[tuple(go(a) for a in args)]  # type: ignore
            if issubclass(origin, List) and (
                field_info.min_items is not None
                or field_info.max_items is not None
                or field_info.unique_items is not None
                used_constraints.update({'min_items', 'max_items', 'unique_items'})
                return conlist(
                    go(args[0]),
                    min_items=field_info.min_items,
                    max_items=field_info.max_items,
                    unique_items=field_info.unique_items,
            if issubclass(origin, Set) and (field_info.min_items is not None or field_info.max_items is not None):
                used_constraints.update({'min_items', 'max_items'})
                return conset(go(args[0]), min_items=field_info.min_items, max_items=field_info.max_items)
            if issubclass(origin, FrozenSet) and (field_info.min_items is not None or field_info.max_items is not None):
                return confrozenset(go(args[0]), min_items=field_info.min_items, max_items=field_info.max_items)
            for t in (Tuple, List, Set, FrozenSet, Sequence):
                if issubclass(origin, t):  # type: ignore
                    return t[tuple(go(a) for a in args)]  # type: ignore
            if issubclass(origin, Dict):
                return Dict[args[0], go(args[1])]  # type: ignore
        attrs: Optional[Tuple[str, ...]] = None
        constraint_func: Optional[Callable[..., type]] = None
        if isinstance(type_, type):
            if issubclass(type_, (SecretStr, SecretBytes)):
                attrs = ('max_length', 'min_length')
                def constraint_func(**kw: Any) -> Type[Any]:  # noqa: F811
                    return type(type_.__name__, (type_,), kw)
            elif issubclass(type_, str) and not issubclass(type_, (EmailStr, AnyUrl)):
                attrs = ('max_length', 'min_length', 'regex')
                if issubclass(type_, StrictStr):
                    def constraint_func(**kw: Any) -> Type[Any]:
                    constraint_func = constr
            elif issubclass(type_, bytes):
                if issubclass(type_, StrictBytes):
                    constraint_func = conbytes
            elif issubclass(type_, numeric_types) and not issubclass(
                type_,
                # Is numeric type
                attrs = ('gt', 'lt', 'ge', 'le', 'multiple_of')
                if issubclass(type_, float):
                    attrs += ('allow_inf_nan',)
                if issubclass(type_, Decimal):
                    attrs += ('max_digits', 'decimal_places')
                numeric_type = next(t for t in numeric_types if issubclass(type_, t))  # pragma: no branch
                constraint_func = _map_types_constraint[numeric_type]
            used_constraints.update(set(attrs))
                attr_name: attr
                for attr_name, attr in ((attr_name, getattr(field_info, attr_name)) for attr_name in attrs)
                if attr is not None
                constraint_func = cast(Callable[..., type], constraint_func)
                return constraint_func(**kwargs)
    return go(annotation), used_constraints
def normalize_name(name: str) -> str:
    Normalizes the given name. This can be applied to either a model *or* enum.
    return re.sub(r'[^a-zA-Z0-9.\-_]', '_', name)
class SkipField(Exception):
    Utility exception used to exclude fields from schema.
"""Schema definitions for Focus export data."""
import polars as pl
# see: https://focus.finops.org/focus-specification/v1-2/
FOCUS_NORMALIZED_SCHEMA = pl.Schema(
        ("BilledCost", pl.Decimal(18, 6)),
        ("BillingAccountId", pl.String),
        ("BillingAccountName", pl.String),
        ("BillingCurrency", pl.String),
        ("BillingPeriodStart", pl.Datetime(time_unit="us")),
        ("BillingPeriodEnd", pl.Datetime(time_unit="us")),
        ("ChargeCategory", pl.String),
        ("ChargeClass", pl.String),
        ("ChargeDescription", pl.String),
        ("ChargeFrequency", pl.String),
        ("ChargePeriodStart", pl.Datetime(time_unit="us")),
        ("ChargePeriodEnd", pl.Datetime(time_unit="us")),
        ("ConsumedQuantity", pl.Decimal(18, 6)),
        ("ConsumedUnit", pl.String),
        ("ContractedCost", pl.Decimal(18, 6)),
        ("ContractedUnitPrice", pl.Decimal(18, 6)),
        ("EffectiveCost", pl.Decimal(18, 6)),
        ("InvoiceIssuerName", pl.String),
        ("ListCost", pl.Decimal(18, 6)),
        ("ListUnitPrice", pl.Decimal(18, 6)),
        ("PricingCategory", pl.String),
        ("PricingQuantity", pl.Decimal(18, 6)),
        ("PricingUnit", pl.String),
        ("ProviderName", pl.String),
        ("PublisherName", pl.String),
        ("RegionId", pl.String),
        ("RegionName", pl.String),
        ("ResourceId", pl.String),
        ("ResourceName", pl.String),
        ("ResourceType", pl.String),
        ("ServiceCategory", pl.String),
        ("ServiceSubcategory", pl.String),
        ("ServiceName", pl.String),
        ("SubAccountId", pl.String),
        ("SubAccountName", pl.String),
        ("SubAccountType", pl.String),
        ("Tags", pl.Object),
__all__ = ["FOCUS_NORMALIZED_SCHEMA"]
