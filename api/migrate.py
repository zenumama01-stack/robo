from typing import TYPE_CHECKING, List
from .._errors import CLIError, SilentCLIError
    sub = subparser.add_parser("migrate")
    sub.set_defaults(func=migrate, args_model=MigrateArgs, allow_unknown_args=True)
    sub = subparser.add_parser("grit")
    sub.set_defaults(func=grit, args_model=GritArgs, allow_unknown_args=True)
class GritArgs(BaseModel):
def grit(args: GritArgs) -> None:
    grit_path = install()
        subprocess.check_call([grit_path, *args.unknown_args])
        # stdout and stderr are forwarded by subprocess so an error will already
        # have been displayed
        raise SilentCLIError() from None
class MigrateArgs(BaseModel):
def migrate(args: MigrateArgs) -> None:
        subprocess.check_call([grit_path, "apply", "openai", *args.unknown_args])
# handles downloading the Grit CLI until they provide their own PyPi package
KEYGEN_ACCOUNT = "custodian-dev"
def _cache_dir() -> Path:
    xdg = os.environ.get("XDG_CACHE_HOME")
    if xdg is not None:
        return Path(xdg)
    return Path.home() / ".cache"
def _debug(message: str) -> None:
    if not os.environ.get("DEBUG"):
    sys.stdout.write(f"[DEBUG]: {message}\n")
def install() -> Path:
    """Installs the Grit CLI and returns the location of the binary"""
        raise CLIError("Windows is not supported yet in the migration CLI")
    _debug("Using Grit installer from GitHub")
    platform = "apple-darwin" if sys.platform == "darwin" else "unknown-linux-gnu"
    dir_name = _cache_dir() / "openai-python"
    install_dir = dir_name / ".install"
    target_dir = install_dir / "bin"
    target_path = target_dir / "grit"
    temp_file = target_dir / "grit.tmp"
    if target_path.exists():
        _debug(f"{target_path} already exists")
        return target_path
    _debug(f"Using Grit CLI path: {target_path}")
    target_dir.mkdir(parents=True, exist_ok=True)
    if temp_file.exists():
        temp_file.unlink()
    arch = _get_arch()
    _debug(f"Using architecture {arch}")
    file_name = f"grit-{arch}-{platform}"
    download_url = f"https://github.com/getgrit/gritql/releases/latest/download/{file_name}.tar.gz"
    sys.stdout.write(f"Downloading Grit CLI from {download_url}\n")
    with httpx.Client() as client:
        download_response = client.get(download_url, follow_redirects=True)
        if download_response.status_code != 200:
            raise CLIError(f"Failed to download Grit CLI from {download_url}")
        with open(temp_file, "wb") as file:
            for chunk in download_response.iter_bytes():
                file.write(chunk)
    unpacked_dir = target_dir / "cli-bin"
    unpacked_dir.mkdir(parents=True, exist_ok=True)
    with tarfile.open(temp_file, "r:gz") as archive:
            archive.extractall(unpacked_dir, filter="data")
            archive.extractall(unpacked_dir)
    _move_files_recursively(unpacked_dir, target_dir)
    shutil.rmtree(unpacked_dir)
    os.remove(temp_file)
    os.chmod(target_path, 0o755)
def _move_files_recursively(source_dir: Path, target_dir: Path) -> None:
    for item in source_dir.iterdir():
        if item.is_file():
            item.rename(target_dir / item.name)
        elif item.is_dir():
            _move_files_recursively(item, target_dir)
def _get_arch() -> str:
    architecture = platform.machine().lower()
    # Map the architecture names to Grit equivalents
    arch_map = {
        "x86_64": "x86_64",
        "amd64": "x86_64",
        "armv7l": "aarch64",
        "arm64": "aarch64",
    return arch_map.get(architecture, architecture)
from django.core.management.base import BaseCommand, CommandError, no_translations
from django.core.management.sql import emit_post_migrate_signal, emit_pre_migrate_signal
from django.db import DEFAULT_DB_ALIAS, connections, router
from django.db.backends.utils import truncate_name
from django.db.migrations.autodetector import MigrationAutodetector
from django.db.migrations.loader import AmbiguityError
from django.db.migrations.state import ModelState, ProjectState
from django.utils.module_loading import module_has_submodule
from django.utils.text import Truncator
    autodetector = MigrationAutodetector
    help = (
        "Updates database schema. Manages both apps with migrations and those without."
            "app_label",
            nargs="?",
            help="App label of an application to synchronize the state.",
            "migration_name",
            help="Database state will be brought to the state after that "
            'migration. Use the name "zero" to unapply all migrations.',
            "--noinput",
            "--no-input",
            action="store_false",
            dest="interactive",
            help="Tells Django to NOT prompt the user for input of any kind.",
            default=DEFAULT_DB_ALIAS,
                'Nominates a database to synchronize. Defaults to the "default" '
                "database."
            "--fake",
            help="Mark migrations as run without actually running them.",
            "--fake-initial",
                "Detect if tables already exist and fake-apply initial migrations if "
                "so. Make sure that the current database schema matches your initial "
                "migration before using this flag. Django will only check for an "
                "existing table name."
            "--plan",
            help="Shows a list of the migration actions that will be performed.",
            "--run-syncdb",
            help="Creates tables for apps without migrations.",
            "--check",
            dest="check_unapplied",
                "Exits with a non-zero status if unapplied migrations exist and does "
                "not actually apply migrations."
            "--prune",
            dest="prune",
            help="Delete nonexistent migrations from the django_migrations table.",
        kwargs = super().get_check_kwargs(options)
        return {**kwargs, "databases": [options["database"]]}
    @no_translations
        database = options["database"]
        self.verbosity = options["verbosity"]
        self.interactive = options["interactive"]
        # Import the 'management' module within each installed app, to register
        # dispatcher events.
        for app_config in apps.get_app_configs():
            if module_has_submodule(app_config.module, "management"):
                import_module(".management", app_config.name)
        # Get the database we're operating from
        connection = connections[database]
        # Hook for backends needing any database preparation
        connection.prepare_database()
        # Work out which apps have migrations and which do not
        executor = MigrationExecutor(connection, self.migration_progress_callback)
        # Raise an error if any migrations are applied before their
        # dependencies.
        executor.loader.check_consistent_history(connection)
        # Before anything else, see if there's conflicting apps and drop out
        # hard if there are any
        conflicts = executor.loader.detect_conflicts()
            name_str = "; ".join(
                "%s in %s" % (", ".join(names), app) for app, names in conflicts.items()
                "Conflicting migrations detected; multiple leaf nodes in the "
                "migration graph: (%s).\nTo fix them run "
                "'python manage.py makemigrations --merge'" % name_str
        # If they supplied command line arguments, work out what they mean.
        run_syncdb = options["run_syncdb"]
        target_app_labels_only = True
        if options["app_label"]:
            # Validate app_label.
            app_label = options["app_label"]
                apps.get_app_config(app_label)
            except LookupError as err:
                raise CommandError(str(err))
            if run_syncdb:
                if app_label in executor.loader.migrated_apps:
                        "Can't use run_syncdb with app '%s' as it has migrations."
                        % app_label
            elif app_label not in executor.loader.migrated_apps:
                raise CommandError("App '%s' does not have migrations." % app_label)
        if options["app_label"] and options["migration_name"]:
            migration_name = options["migration_name"]
            if migration_name == "zero":
                targets = [(app_label, None)]
                    migration = executor.loader.get_migration_by_prefix(
                        app_label, migration_name
                except AmbiguityError:
                        "More than one migration matches '%s' in app '%s'. "
                        "Please be more specific." % (migration_name, app_label)
                        "Cannot find a migration matching '%s' from app '%s'."
                        % (migration_name, app_label)
                target = (app_label, migration.name)
                # Partially applied squashed migrations are not included in the
                # graph, use the last replacement instead.
                    target not in executor.loader.graph.nodes
                    and target in executor.loader.replacements
                    incomplete_migration = executor.loader.replacements[target]
                    target = incomplete_migration.replaces[-1]
                targets = [target]
            target_app_labels_only = False
        elif options["app_label"]:
            targets = [
                key for key in executor.loader.graph.leaf_nodes() if key[0] == app_label
            targets = executor.loader.graph.leaf_nodes()
        if options["prune"]:
            if not options["app_label"]:
                    "Migrations can be pruned only when an app is specified."
            if self.verbosity > 0:
                self.stdout.write("Pruning migrations:", self.style.MIGRATE_HEADING)
            to_prune = sorted(
                migration
                for migration in set(executor.loader.applied_migrations)
                - set(executor.loader.disk_migrations)
                if migration[0] == app_label
            squashed_migrations_with_deleted_replaced_migrations = [
                migration_key
                for migration_key, migration_obj in executor.loader.replacements.items()
                if any(replaced in to_prune for replaced in migration_obj.replaces)
            if squashed_migrations_with_deleted_replaced_migrations:
                        "  Cannot use --prune because the following squashed "
                        "migrations have their 'replaces' attributes and may not "
                        "be recorded as applied:"
                for migration in squashed_migrations_with_deleted_replaced_migrations:
                    app, name = migration
                    self.stdout.write(f"    {app}.{name}")
                        "  Re-run 'manage.py migrate' if they are not marked as "
                        "applied, and remove 'replaces' attributes in their "
                        "Migration classes."
                if to_prune:
                    for migration in to_prune:
                                self.style.MIGRATE_LABEL(f"  Pruning {app}.{name}"),
                                ending="",
                        executor.recorder.record_unapplied(app, name)
                            self.stdout.write(self.style.SUCCESS(" OK"))
                elif self.verbosity > 0:
                    self.stdout.write("  No migrations to prune.")
        plan = executor.migration_plan(targets)
        if options["plan"]:
            self.stdout.write("Planned operations:", self.style.MIGRATE_LABEL)
            if not plan:
                self.stdout.write("  No planned migration operations.")
                for migration, backwards in plan:
                    self.stdout.write(str(migration), self.style.MIGRATE_HEADING)
                    for operation in migration.operations:
                        message, is_error = self.describe_operation(
                            operation, backwards
                        style = self.style.WARNING if is_error else None
                        self.stdout.write("    " + message, style)
                if options["check_unapplied"]:
        # At this point, ignore run_syncdb if there aren't any apps to sync.
        run_syncdb = options["run_syncdb"] and executor.loader.unmigrated_apps
        # Print some useful info
        if self.verbosity >= 1:
            self.stdout.write(self.style.MIGRATE_HEADING("Operations to perform:"))
                        self.style.MIGRATE_LABEL(
                            "  Synchronize unmigrated app: %s" % app_label
                        self.style.MIGRATE_LABEL("  Synchronize unmigrated apps: ")
                        + (", ".join(sorted(executor.loader.unmigrated_apps)))
            if target_app_labels_only:
                    self.style.MIGRATE_LABEL("  Apply all migrations: ")
                    + (", ".join(sorted({a for a, n in targets})) or "(none)")
                if targets[0][1] is None:
                        self.style.MIGRATE_LABEL("  Unapply all migrations: ")
                        + str(targets[0][0])
                        self.style.MIGRATE_LABEL("  Target specific migration: ")
                        + "%s, from %s" % (targets[0][1], targets[0][0])
        pre_migrate_state = executor._create_project_state(with_applied_migrations=True)
        pre_migrate_apps = pre_migrate_state.apps
        emit_pre_migrate_signal(
            self.verbosity,
            self.interactive,
            connection.alias,
            stdout=self.stdout,
            apps=pre_migrate_apps,
            plan=plan,
        # Run the syncdb phase.
                    self.style.MIGRATE_HEADING("Synchronizing apps without migrations:")
                self.sync_apps(connection, [app_label])
                self.sync_apps(connection, executor.loader.unmigrated_apps)
        # Migrate!
            self.stdout.write(self.style.MIGRATE_HEADING("Running migrations:"))
                self.stdout.write("  No migrations to apply.")
                # If there's changes that aren't in migrations yet, tell them
                # how to fix it.
                autodetector = self.autodetector(
                    executor.loader.project_state(),
                    ProjectState.from_apps(apps),
                changes = autodetector.changes(graph=executor.loader.graph)
                if changes:
                            "  Your models in app(s): %s have changes that are not "
                            "yet reflected in a migration, and so won't be "
                            "applied." % ", ".join(repr(app) for app in sorted(changes))
                            "  Run 'manage.py makemigrations' to make new "
                            "migrations, and then re-run 'manage.py migrate' to "
                            "apply them."
            fake = False
            fake_initial = False
            fake = options["fake"]
            fake_initial = options["fake_initial"]
        post_migrate_state = executor.migrate(
            targets,
            state=pre_migrate_state.clone(),
            fake=fake,
            fake_initial=fake_initial,
        # post_migrate signals have access to all models. Ensure that all
        # models are reloaded in case any are delayed.
        post_migrate_state.clear_delayed_apps_cache()
        post_migrate_apps = post_migrate_state.apps
        # Re-render models of real apps to include relationships now that
        # we've got a final state. This wouldn't be necessary if real apps
        # models were rendered with relationships in the first place.
        with post_migrate_apps.bulk_update():
            model_keys = []
            for model_state in post_migrate_apps.real_models:
                model_key = model_state.app_label, model_state.name_lower
                model_keys.append(model_key)
                post_migrate_apps.unregister_model(*model_key)
        post_migrate_apps.render_multiple(
            [ModelState.from_model(apps.get_model(*model)) for model in model_keys]
        # Send the post_migrate signal, so individual apps can do whatever they
        # need to do at this point.
        emit_post_migrate_signal(
            apps=post_migrate_apps,
    def migration_progress_callback(self, action, migration=None, fake=False):
            compute_time = self.verbosity > 1
            if action == "apply_start":
                if compute_time:
                    self.start = time.monotonic()
                self.stdout.write("  Applying %s..." % migration, ending="")
                self.stdout.flush()
            elif action == "apply_success":
                elapsed = (
                    " (%.3fs)" % (time.monotonic() - self.start) if compute_time else ""
                if fake:
                    self.stdout.write(self.style.SUCCESS(" FAKED" + elapsed))
                    self.stdout.write(self.style.SUCCESS(" OK" + elapsed))
            elif action == "unapply_start":
                self.stdout.write("  Unapplying %s..." % migration, ending="")
            elif action == "unapply_success":
            elif action == "render_start":
                self.stdout.write("  Rendering model states...", ending="")
            elif action == "render_success":
                self.stdout.write(self.style.SUCCESS(" DONE" + elapsed))
    def sync_apps(self, connection, app_labels):
        """Run the old syncdb-style operation on a list of app_labels."""
        with connection.cursor() as cursor:
            tables = connection.introspection.table_names(cursor)
        # Build the manifest of apps and models that are to be synchronized.
        all_models = [
                app_config.label,
                router.get_migratable_models(
                    app_config, connection.alias, include_auto_created=False
            for app_config in apps.get_app_configs()
            if app_config.models_module is not None and app_config.label in app_labels
        def model_installed(model):
            opts = model._meta
            converter = connection.introspection.identifier_converter
            max_name_length = connection.ops.max_name_length()
            return not (
                (converter(truncate_name(opts.db_table, max_name_length)) in tables)
                    opts.auto_created
                    and converter(opts.auto_created._meta.db_table) in tables
        manifest = {
            app_name: list(filter(model_installed, model_list))
            for app_name, model_list in all_models
        # Create the tables for each model
            self.stdout.write("  Creating tables...")
        with connection.schema_editor() as editor:
            for app_name, model_list in manifest.items():
                for model in model_list:
                    # Never install unmanaged models, etc.
                    if not model._meta.can_migrate(connection):
                    if self.verbosity >= 3:
                            "    Processing %s.%s model"
                            % (app_name, model._meta.object_name)
                            "    Creating table %s" % model._meta.db_table
                    editor.create_model(model)
            # Deferred SQL is executed when exiting the editor's context.
                self.stdout.write("    Running deferred SQL...")
    def describe_operation(operation, backwards):
        """Return a string that describes a migration operation for --plan."""
        prefix = ""
        is_error = False
        if hasattr(operation, "code"):
            code = operation.reverse_code if backwards else operation.code
            action = (code.__doc__ or "") if code else None
        elif hasattr(operation, "sql"):
            action = operation.reverse_sql if backwards else operation.sql
            action = ""
            if backwards:
                prefix = "Undo "
            action = str(action).replace("\n", "")
        elif backwards:
            action = "IRREVERSIBLE"
            is_error = True
        if action:
            action = " -> " + action
        truncated = Truncator(action)
        return prefix + operation.describe() + truncated.chars(40), is_error
