class EchoArg(bb.Struct):
    Contains the arguments to be sent to the Dropbox servers.
    :ivar check.EchoArg.query: The string that you'd like to be echoed back to
        '_query_value',
    _has_required_fields = False
                 query=None):
        self._query_value = bb.NOT_SET
            self.query = query
    query = bb.Attribute("query")
        super(EchoArg, self)._process_custom_annotations(annotation_type, field_path, processor)
EchoArg_validator = bv.Struct(EchoArg)
class EchoResult(bb.Struct):
    EchoResult contains the result returned from the Dropbox servers.
    :ivar check.EchoResult.result: If everything worked correctly, this would be
        the same as query.
        '_result_value',
                 result=None):
        self._result_value = bb.NOT_SET
        if result is not None:
            self.result = result
    result = bb.Attribute("result")
        super(EchoResult, self)._process_custom_annotations(annotation_type, field_path, processor)
EchoResult_validator = bv.Struct(EchoResult)
EchoArg.query.validator = bv.String(max_length=500)
EchoArg._all_field_names_ = set(['query'])
EchoArg._all_fields_ = [('query', EchoArg.query.validator)]
EchoResult.result.validator = bv.String()
EchoResult._all_field_names_ = set(['result'])
EchoResult._all_fields_ = [('result', EchoResult.result.validator)]
EchoArg.query.default = ''
EchoResult.result.default = ''
app = bb.Route(
    'app',
    EchoArg_validator,
    EchoResult_validator,
user = bb.Route(
    'user',
    'app': app,
    'user': user,
from django.core.checks.registry import registry
from django.core.management.base import BaseCommand, CommandError
from django.db import connections
class Command(BaseCommand):
    help = "Checks the entire Django project for potential problems."
    requires_system_checks = []
        parser.add_argument("args", metavar="app_label", nargs="*")
            "--tag",
            dest="tags",
            help="Run only checks labeled with given tag.",
            "--list-tags",
                "List available tags. Specify --deploy to include available deployment "
                "tags."
            "--deploy",
            help="Check deployment settings.",
            "--fail-level",
            default="ERROR",
            choices=["CRITICAL", "ERROR", "WARNING", "INFO", "DEBUG"],
                "Message level that will cause the command to exit with a "
                "non-zero status. Default is ERROR."
            "--database",
            choices=tuple(connections),
            dest="databases",
            help="Run database related checks against these aliases.",
        include_deployment_checks = options["deploy"]
        if options["list_tags"]:
                "\n".join(sorted(registry.tags_available(include_deployment_checks)))
        if app_labels:
            app_configs = None
        tags = options["tags"]
                invalid_tag = next(
                    for tag in tags
                    if not checks.tag_exists(tag, include_deployment_checks)
                # no invalid tags
                    'There is no system check with the "%s" tag.' % invalid_tag
        self.check(
            display_num_errors=True,
            fail_level=getattr(checks, options["fail_level"]),
            databases=options["databases"],
"""distutils.command.check
Implements the Distutils 'check' command.
from email.utils import getaddresses
from distutils.errors import DistutilsSetupError
    # docutils is installed
    from docutils.utils import Reporter
    from docutils.parsers.rst import Parser
    from docutils import frontend
    from docutils import nodes
    class SilentReporter(Reporter):
            report_level,
            halt_level,
            debug=0,
            encoding='ascii',
            error_handler='replace',
            self.messages = []
                source, report_level, halt_level, stream, debug, encoding, error_handler
        def system_message(self, level, message, *children, **kwargs):
            self.messages.append((level, message, children, kwargs))
            return nodes.system_message(
                message, level=level, type=self.levels[level], *children, **kwargs
    HAS_DOCUTILS = True
    # Catch all exceptions because exceptions besides ImportError probably
    # indicate that docutils is not ported to Py3k.
    HAS_DOCUTILS = False
class check(Command):
    """This command checks the meta-data of the package."""
    description = "perform some checks on the package"
        ('metadata', 'm', 'Verify meta-data'),
            'restructuredtext',
            'r',
                'Checks if long string meta-data syntax '
                'are reStructuredText-compliant'
        ('strict', 's', 'Will exit with an error if a check fails'),
    boolean_options = ['metadata', 'restructuredtext', 'strict']
        """Sets default values for options."""
        self.restructuredtext = 0
        self.metadata = 1
        self.strict = 0
        self._warnings = 0
    def warn(self, msg):
        """Counts the number of warnings that occurs."""
        self._warnings += 1
        return Command.warn(self, msg)
        """Runs the command."""
        # perform the various tests
            self.check_metadata()
        if self.restructuredtext:
            if HAS_DOCUTILS:
                self.check_restructuredtext()
            elif self.strict:
                raise DistutilsSetupError('The docutils package is needed.')
        # let's raise an error in strict mode, if we have at least
        # one warning
        if self.strict and self._warnings > 0:
            raise DistutilsSetupError('Please correct your package.')
    def check_metadata(self):
        """Ensures that all required elements of meta-data are supplied.
        Required fields:
            name, version
        Warns if any are missing.
        metadata = self.distribution.metadata
        for attr in 'name', 'version':
            if not getattr(metadata, attr, None):
                missing.append(attr)
            self.warn("missing required meta-data: %s" % ', '.join(missing))
    def check_restructuredtext(self):
        """Checks if the long string fields are reST-compliant."""
        data = self.distribution.get_long_description()
        for warning in self._check_rst_data(data):
            line = warning[-1].get('line')
            if line is None:
                warning = warning[1]
                warning = '%s (line %s)' % (warning[1], line)
            self.warn(warning)
    def _check_rst_data(self, data):
        """Returns warnings when the provided data doesn't compile."""
        # the include and csv_table directives need this to be a path
        source_path = self.distribution.script_name or 'setup.py'
        parser = Parser()
        settings = frontend.OptionParser(components=(Parser,)).get_default_values()
        settings.tab_width = 4
        settings.pep_references = None
        settings.rfc_references = None
        reporter = SilentReporter(
            source_path,
            settings.report_level,
            settings.halt_level,
            stream=settings.warning_stream,
            debug=settings.debug,
            encoding=settings.error_encoding,
            error_handler=settings.error_encoding_error_handler,
        document = nodes.document(settings, reporter, source=source_path)
        document.note_source(source_path, -1)
            parser.parse(data, document)
            reporter.messages.append(
                (-1, 'Could not finish the parsing: %s.' % e, '', {})
        return reporter.messages
from pip._internal.metadata import get_default_environment
from pip._internal.operations.check import (
    check_package_set,
    check_unsupported,
    create_package_set_from_installed,
from pip._internal.utils.compatibility_tags import get_supported
from pip._internal.utils.misc import write_output
class CheckCommand(Command):
    """Verify installed packages have compatible dependencies."""
      %prog [options]"""
        package_set, parsing_probs = create_package_set_from_installed()
        missing, conflicting = check_package_set(package_set)
        unsupported = list(
            check_unsupported(
                get_default_environment().iter_installed_distributions(),
                get_supported(),
        for project_name in missing:
            version = package_set[project_name].version
            for dependency in missing[project_name]:
                write_output(
                    "%s %s requires %s, which is not installed.",
                    project_name,
                    dependency[0],
        for project_name in conflicting:
            for dep_name, dep_version, req in conflicting[project_name]:
                    "%s %s has requirement %s, but you have %s %s.",
                    req,
                    dep_name,
                    dep_version,
        for package in unsupported:
                "%s %s is not supported on this platform",
                package.raw_name,
                package.version,
        if missing or conflicting or parsing_probs or unsupported:
            write_output("No broken requirements found.")
"""Validation of dependencies of packages"""
from collections.abc import Generator, Iterable
from pip._vendor.packaging.tags import Tag, parse_tag
from pip._internal.distributions import make_distribution_for_install_requirement
class PackageDetails(NamedTuple):
    version: Version
    dependencies: list[Requirement]
# Shorthands
PackageSet = dict[NormalizedName, PackageDetails]
Missing = tuple[NormalizedName, Requirement]
Conflicting = tuple[NormalizedName, Version, Requirement]
MissingDict = dict[NormalizedName, list[Missing]]
ConflictingDict = dict[NormalizedName, list[Conflicting]]
CheckResult = tuple[MissingDict, ConflictingDict]
ConflictDetails = tuple[PackageSet, CheckResult]
def create_package_set_from_installed() -> tuple[PackageSet, bool]:
    """Converts a list of distributions into a PackageSet."""
    package_set = {}
    problems = False
    env = get_default_environment()
    for dist in env.iter_installed_distributions(local_only=False, skip=()):
        name = dist.canonical_name
            dependencies = list(dist.iter_dependencies())
            package_set[name] = PackageDetails(dist.version, dependencies)
        except (OSError, ValueError) as e:
            # Don't crash on unreadable or broken metadata.
            logger.warning("Error parsing dependencies of %s: %s", name, e)
            problems = True
    return package_set, problems
def check_package_set(
    package_set: PackageSet, should_ignore: Callable[[str], bool] | None = None
) -> CheckResult:
    """Check if a package set is consistent
    If should_ignore is passed, it should be a callable that takes a
    package name and returns a boolean.
    missing = {}
    conflicting = {}
    for package_name, package_detail in package_set.items():
        # Info about dependencies of package_name
        missing_deps: set[Missing] = set()
        conflicting_deps: set[Conflicting] = set()
        if should_ignore and should_ignore(package_name):
        for req in package_detail.dependencies:
            name = canonicalize_name(req.name)
            # Check if it's missing
            if name not in package_set:
                missed = True
                if req.marker is not None:
                    missed = req.marker.evaluate({"extra": ""})
                if missed:
                    missing_deps.add((name, req))
            # Check if there's a conflict
            version = package_set[name].version
            if not req.specifier.contains(version, prereleases=True):
                conflicting_deps.add((name, version, req))
        if missing_deps:
            missing[package_name] = sorted(missing_deps, key=str)
        if conflicting_deps:
            conflicting[package_name] = sorted(conflicting_deps, key=str)
    return missing, conflicting
def check_install_conflicts(to_install: list[InstallRequirement]) -> ConflictDetails:
    """For checking if the dependency graph would be consistent after \
    installing given requirements
    # Start from the current state
    package_set, _ = create_package_set_from_installed()
    # Install packages
    would_be_installed = _simulate_installation_of(to_install, package_set)
    # Only warn about directly-dependent packages; create a whitelist of them
    whitelist = _create_whitelist(would_be_installed, package_set)
        package_set,
        check_package_set(
            package_set, should_ignore=lambda name: name not in whitelist
def check_unsupported(
    packages: Iterable[BaseDistribution],
    supported_tags: Iterable[Tag],
) -> Generator[BaseDistribution, None, None]:
    for p in packages:
            wheel_file = p.read_text("WHEEL")
            wheel_tags: frozenset[Tag] = reduce(
                frozenset.union,
                map(parse_tag, Parser().parsestr(wheel_file).get_all("Tag", [])),
                frozenset(),
            if wheel_tags.isdisjoint(supported_tags):
                yield p
def _simulate_installation_of(
    to_install: list[InstallRequirement], package_set: PackageSet
) -> set[NormalizedName]:
    """Computes the version of packages after installing to_install."""
    # Keep track of packages that were installed
    installed = set()
    # Modify it as installing requirement_set would (assuming no errors)
    for inst_req in to_install:
        abstract_dist = make_distribution_for_install_requirement(inst_req)
        dist = abstract_dist.get_metadata_distribution()
        package_set[name] = PackageDetails(dist.version, list(dist.iter_dependencies()))
        installed.add(name)
    return installed
def _create_whitelist(
    would_be_installed: set[NormalizedName], package_set: PackageSet
    packages_affected = set(would_be_installed)
    for package_name in package_set:
        if package_name in packages_affected:
        for req in package_set[package_name].dependencies:
            if canonicalize_name(req.name) in packages_affected:
                packages_affected.add(package_name)
    return packages_affected
