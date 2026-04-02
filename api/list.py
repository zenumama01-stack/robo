#: All the potential fields for :class:`List` objects
LIST_FIELDS = [
    "description",
    "follower_count",
    "member_count",
    "name",
    "owner_id",
    "private",
class List(HashableID, DataMapping):
    """The list object contains `Twitter Lists`_ metadata describing the
    referenced List. The List object is the primary object returned in the List
    lookup endpoint. When requesting additional List fields on this endpoint,
    simply use the fields parameter ``list.fields``.
    At the moment, the List object cannot be found as a child object from any
    other data object. However, user objects can be found and expanded in the
    user resource. These objects are available for expansion by adding
    ``owner_id`` to the ``expansions`` query parameter. Use the expansion with
    the field parameter: ``list.fields`` when requesting additional fields to
    complete the primary List object and ``user.fields`` to complete the
    expansion object.
        The JSON data representing the List.
        The unique identifier of this List.
        The name of the List, as defined when creating the List.
        The UTC datetime that the List was created on Twitter.
        A brief description to let users know about the List.
    follower_count : int | None
        Shows how many users follow this List,
    member_count : int | None
        Shows how many members are part of this List.
        Indicates if the List is private.
    owner_id : str | None
        Unique identifier of this List's owner.
    https://developer.twitter.com/en/docs/twitter-api/data-dictionary/object-model/lists
    .. _Twitter Lists: https://help.twitter.com/en/using-twitter/twitter-lists
        "data", "id", "name", "created_at", "description", "follower_count",
        "member_count", "private", "owner_id"
        self.id = data["id"]
        self.name = data["name"]
        self.description = data.get("description")
        self.follower_count = data.get("follower_count")
        self.member_count = data.get("member_count")
        self.private = data.get("private")
        self.owner_id = data.get("owner_id")
        return f"<List id={self.id} name={self.name}>"
        return self.name
from django.core.paginator import InvalidPage, Paginator
from django.db.models import QuerySet
from django.views.generic.base import ContextMixin, TemplateResponseMixin, View
class MultipleObjectMixin(ContextMixin):
    """A mixin for views manipulating multiple objects."""
    allow_empty = True
    queryset = None
    paginate_by = None
    paginate_orphans = 0
    context_object_name = None
    paginator_class = Paginator
    page_kwarg = "page"
        Return the list of items for this view.
        The return value must be an iterable and may be an instance of
        `QuerySet` in which case `QuerySet` specific behavior will be enabled.
            if isinstance(queryset, QuerySet):
                queryset = queryset.all()
        elif self.model is not None:
            queryset = self.model._default_manager.all()
                "%(cls)s is missing a QuerySet. Define "
                "%(cls)s.model, %(cls)s.queryset, or override "
                "%(cls)s.get_queryset()." % {"cls": self.__class__.__name__}
        ordering = self.get_ordering()
            if isinstance(ordering, str):
                ordering = (ordering,)
            queryset = queryset.order_by(*ordering)
    def get_ordering(self):
        """Return the field or fields to use for ordering the queryset."""
        return self.ordering
    def paginate_queryset(self, queryset, page_size):
        """Paginate the queryset, if needed."""
        paginator = self.get_paginator(
            page_size,
            orphans=self.get_paginate_orphans(),
            allow_empty_first_page=self.get_allow_empty(),
        page_kwarg = self.page_kwarg
        page = self.kwargs.get(page_kwarg) or self.request.GET.get(page_kwarg) or 1
            page_number = int(page)
            if page == "last":
                page_number = paginator.num_pages
                raise Http404(
                    _("Page is not “last”, nor can it be converted to an int.")
            page = paginator.page(page_number)
            return (paginator, page, page.object_list, page.has_other_pages())
        except InvalidPage as e:
                _("Invalid page (%(page_number)s): %(message)s")
                % {"page_number": page_number, "message": str(e)}
    def get_paginate_by(self, queryset):
        Get the number of items to paginate by, or ``None`` for no pagination.
        return self.paginate_by
        self, queryset, per_page, orphans=0, allow_empty_first_page=True, **kwargs
        """Return an instance of the paginator for this view."""
        return self.paginator_class(
            per_page,
            orphans=orphans,
            allow_empty_first_page=allow_empty_first_page,
    def get_paginate_orphans(self):
        Return the maximum number of orphans extend the last page by when
        paginating.
        return self.paginate_orphans
    def get_allow_empty(self):
        Return ``True`` if the view should display empty lists and ``False``
        if a 404 should be raised instead.
        return self.allow_empty
    def get_context_object_name(self, object_list):
        """Get the name of the item to be used in the context."""
        if self.context_object_name:
            return self.context_object_name
        elif hasattr(object_list, "model"):
            return "%s_list" % object_list.model._meta.model_name
    def get_context_data(self, *, object_list=None, **kwargs):
        """Get the context for this view."""
        queryset = object_list if object_list is not None else self.object_list
        page_size = self.get_paginate_by(queryset)
        context_object_name = self.get_context_object_name(queryset)
        if page_size:
            paginator, page, queryset, is_paginated = self.paginate_queryset(
                queryset, page_size
                "paginator": paginator,
                "page_obj": page,
                "is_paginated": is_paginated,
                "object_list": queryset,
                "paginator": None,
                "page_obj": None,
                "is_paginated": False,
        if context_object_name is not None:
            context[context_object_name] = queryset
        context.update(kwargs)
        return super().get_context_data(**context)
class BaseListView(MultipleObjectMixin, View):
    Base view for displaying a list of objects.
    This requires subclassing to provide a response mixin.
        self.object_list = self.get_queryset()
        allow_empty = self.get_allow_empty()
        if not allow_empty:
            # When pagination is enabled and object_list is a queryset,
            # it's better to do a cheap query than to load the unpaginated
            # queryset in memory.
            if self.get_paginate_by(self.object_list) is not None and hasattr(
                self.object_list, "exists"
                is_empty = not self.object_list.exists()
                is_empty = not self.object_list
            if is_empty:
                    _("Empty list and “%(class_name)s.allow_empty” is False.")
                        "class_name": self.__class__.__name__,
        context = self.get_context_data()
class MultipleObjectTemplateResponseMixin(TemplateResponseMixin):
    """Mixin for responding with a template and list of objects."""
    template_name_suffix = "_list"
        a list. May not be called if render_to_response is overridden.
            names = super().get_template_names()
            # If template_name isn't specified, it's not a problem --
            # we just start with an empty list.
        # If the list is a queryset, we'll invent a template name based on the
        # app and model name. This name gets put at the end of the template
        # name list so that user-supplied names override the automatically-
        # generated ones.
        if hasattr(self.object_list, "model"):
            opts = self.object_list.model._meta
            names.append(
                "%s/%s%s.html"
                % (opts.app_label, opts.model_name, self.template_name_suffix)
        elif not names:
                "%(cls)s requires either a 'template_name' attribute "
                "or a get_queryset() method that returns a QuerySet."
        return names
class ListView(MultipleObjectTemplateResponseMixin, BaseListView):
    Render some list of objects, set by `self.model` or `self.queryset`.
    `self.queryset` can actually be any iterable of items, not just a queryset.
"""Parsers for list output."""
import csv
from abc import abstractmethod
from typing import TYPE_CHECKING, TypeVar
from langchain_core.output_parsers.transform import BaseTransformOutputParser
def droplastn(
    iter: Iterator[T],  # noqa: A002
    n: int,
) -> Iterator[T]:
    """Drop the last `n` elements of an iterator.
        iter: The iterator to drop elements from.
        n: The number of elements to drop.
        The elements of the iterator, except the last n elements.
    buffer: deque[T] = deque()
    for item in iter:
        buffer.append(item)
        if len(buffer) > n:
            yield buffer.popleft()
class ListOutputParser(BaseTransformOutputParser[list[str]]):
    """Parse the output of a model to a list."""
        return "list"
    def parse(self, text: str) -> list[str]:
        """Parse the output of an LLM call.
            text: The output of an LLM call.
            A list of strings.
    def parse_iter(self, text: str) -> Iterator[re.Match]:
            A match object for each part of the output.
    def _transform(self, input: Iterator[str | BaseMessage]) -> Iterator[list[str]]:
        buffer = ""
        for chunk in input:
            if isinstance(chunk, BaseMessage):
                # Extract text
                chunk_content = chunk.content
                if not isinstance(chunk_content, str):
                buffer += chunk_content
                # Add current chunk to buffer
                buffer += chunk
            # Parse buffer into a list of parts
                done_idx = 0
                # Yield only complete parts
                for m in droplastn(self.parse_iter(buffer), 1):
                    done_idx = m.end()
                    yield [m.group(1)]
                buffer = buffer[done_idx:]
                parts = self.parse(buffer)
                if len(parts) > 1:
                    for part in parts[:-1]:
                        yield [part]
                    buffer = parts[-1]
        # Yield the last part
        for part in self.parse(buffer):
    async def _atransform(
        self, input: AsyncIterator[str | BaseMessage]
    ) -> AsyncIterator[list[str]]:
        async for chunk in input:
class CommaSeparatedListOutputParser(ListOutputParser):
    """Parse the output of a model to a comma-separated list."""
            `["langchain", "output_parsers", "list"]`
        return ["langchain", "output_parsers", "list"]
        """Return the format instructions for the comma-separated list output."""
            "Your response should be a list of comma separated values, "
            "eg: `foo, bar, baz` or `foo,bar,baz`"
            reader = csv.reader(
                StringIO(text), quotechar='"', delimiter=",", skipinitialspace=True
            return [item for sublist in reader for item in sublist]
        except csv.Error:
            # Keep old logic for backup
            return [part.strip() for part in text.split(",")]
        return "comma-separated-list"
class NumberedListOutputParser(ListOutputParser):
    """Parse a numbered list."""
    pattern: str = r"\d+\.\s([^\n]+)"
    """The pattern to match a numbered list item."""
            "Your response should be a numbered list with each item on a new line. "
            "For example: \n\n1. foo\n\n2. bar\n\n3. baz"
        return re.findall(self.pattern, text)
        return re.finditer(self.pattern, text)
        return "numbered-list"
class MarkdownListOutputParser(ListOutputParser):
    """Parse a Markdown list."""
    pattern: str = r"^\s*[-*]\s([^\n]+)$"
    """The pattern to match a Markdown list item."""
        """Return the format instructions for the Markdown list output."""
        return "Your response should be a markdown list, eg: `- foo\n- bar\n- baz`"
        return re.findall(self.pattern, text, re.MULTILINE)
        return re.finditer(self.pattern, text, re.MULTILINE)
        return "markdown-list"
from collections.abc import Generator, Sequence
from email.parser import Parser
from pip._vendor.packaging.version import InvalidVersion, Version
from pip._internal.cli import cmdoptions
from pip._internal.cli.index_command import IndexGroupCommand
from pip._internal.exceptions import CommandError
from pip._internal.metadata import BaseDistribution, get_environment
from pip._internal.models.selection_prefs import SelectionPreferences
from pip._internal.utils.compat import stdlib_pkgs
from pip._internal.utils.misc import tabulate, write_output
    from pip._internal.index.package_finder import PackageFinder
    from pip._internal.network.session import PipSession
    class _DistWithLatestInfo(BaseDistribution):
        """Give the distribution object a couple of extra fields.
        These will be populated during ``get_outdated()``. This is dirty but
        makes the rest of the code much cleaner.
        latest_version: Version
        latest_filetype: str
    _ProcessedDists = Sequence[_DistWithLatestInfo]
class ListCommand(IndexGroupCommand):
    List installed packages, including editables.
    Packages are listed in a case-insensitive sorted order.
            "--outdated",
            help="List outdated packages",
            "-u",
            "--uptodate",
            help="List uptodate packages",
            "-e",
            "--editable",
            help="List editable projects.",
            "--local",
                "If in a virtualenv that has global access, do not list "
                "globally-installed packages."
            dest="user",
            help="Only output packages installed in user-site.",
        self.cmd_opts.add_option(cmdoptions.list_path())
            default="columns",
            choices=("columns", "freeze", "json"),
                "Select the output format among: columns (default), freeze, or json. "
                "The 'freeze' format cannot be used with the --outdated option."
            "--not-required",
            dest="not_required",
            help="List packages that are not dependencies of installed packages.",
            "--exclude-editable",
            dest="include_editable",
            help="Exclude editable package from output.",
            "--include-editable",
            help="Include editable package in output.",
        self.cmd_opts.add_option(cmdoptions.list_exclude())
        index_opts = cmdoptions.make_option_group(cmdoptions.index_group, self.parser)
        selection_opts = cmdoptions.make_option_group(
            cmdoptions.package_selection_group,
            self.parser,
        self.parser.insert_option_group(0, index_opts)
        self.parser.insert_option_group(0, selection_opts)
    def handle_pip_version_check(self, options: Values) -> None:
        if options.outdated or options.uptodate:
            super().handle_pip_version_check(options)
    def _build_package_finder(
        self, options: Values, session: PipSession
    ) -> PackageFinder:
        Create a package finder appropriate to this list command.
        # Lazy import the heavy index modules as most list invocations won't need 'em.
        from pip._internal.index.collector import LinkCollector
        link_collector = LinkCollector.create(session, options=options)
        # Pass allow_yanked=False to ignore yanked versions.
        selection_prefs = SelectionPreferences(
            allow_yanked=False,
            release_control=options.release_control,
        return PackageFinder.create(
            link_collector=link_collector,
            selection_prefs=selection_prefs,
        cmdoptions.check_release_control_exclusive(options)
        if options.outdated and options.uptodate:
            raise CommandError("Options --outdated and --uptodate cannot be combined.")
        if options.outdated and options.list_format == "freeze":
                "List format 'freeze' cannot be used with the --outdated option."
        cmdoptions.check_list_path_option(options)
        skip = set(stdlib_pkgs)
        if options.excludes:
            skip.update(canonicalize_name(n) for n in options.excludes)
        packages: _ProcessedDists = [
            cast("_DistWithLatestInfo", d)
            for d in get_environment(options.path).iter_installed_distributions(
                local_only=options.local,
                user_only=options.user,
                editables_only=options.editable,
                include_editables=options.include_editable,
                skip=skip,
        # get_not_required must be called firstly in order to find and
        # filter out all dependencies correctly. Otherwise a package
        # can't be identified as requirement because some parent packages
        # could be filtered out before.
        if options.not_required:
            packages = self.get_not_required(packages, options)
        if options.outdated:
            packages = self.get_outdated(packages, options)
        elif options.uptodate:
            packages = self.get_uptodate(packages, options)
        self.output_package_listing(packages, options)
    def get_outdated(
        self, packages: _ProcessedDists, options: Values
    ) -> _ProcessedDists:
            dist
            for dist in self.iter_packages_latest_infos(packages, options)
            if dist.latest_version > dist.version
    def get_uptodate(
            if dist.latest_version == dist.version
    def get_not_required(
        dep_keys = {
            canonicalize_name(dep.name)
            for dist in packages
            for dep in (dist.iter_dependencies() or ())
        # Create a set to remove duplicate packages, and cast it to a list
        # to keep the return type consistent with get_outdated and
        # get_uptodate
        return list({pkg for pkg in packages if pkg.canonical_name not in dep_keys})
    def iter_packages_latest_infos(
    ) -> Generator[_DistWithLatestInfo, None, None]:
        with self._build_session(options) as session:
            finder = self._build_package_finder(options, session)
            def latest_info(
                dist: _DistWithLatestInfo,
            ) -> _DistWithLatestInfo | None:
                all_candidates = finder.find_all_candidates(dist.canonical_name)
                if self.should_exclude_prerelease(options, dist.canonical_name):
                    all_candidates = [
                        candidate
                        for candidate in all_candidates
                        if not candidate.version.is_prerelease
                evaluator = finder.make_candidate_evaluator(
                    project_name=dist.canonical_name,
                best_candidate = evaluator.sort_best_candidate(all_candidates)
                if best_candidate is None:
                remote_version = best_candidate.version
                if best_candidate.link.is_wheel:
                    typ = "wheel"
                    typ = "sdist"
                dist.latest_version = remote_version
                dist.latest_filetype = typ
            for dist in map(latest_info, packages):
    def output_package_listing(
        packages = sorted(
            packages,
            key=lambda dist: dist.canonical_name,
        if options.list_format == "columns" and packages:
            data, header = format_for_columns(packages, options)
            self.output_package_listing_columns(data, header)
        elif options.list_format == "freeze":
            for dist in packages:
                    req_string = f"{dist.raw_name}=={dist.version}"
                    req_string = f"{dist.raw_name}==={dist.raw_version}"
                    write_output("%s (%s)", req_string, dist.location)
                    write_output(req_string)
        elif options.list_format == "json":
            write_output(format_for_json(packages, options))
    def output_package_listing_columns(
        self, data: list[list[str]], header: list[str]
        # insert the header first: we need to know the size of column names
        if len(data) > 0:
            data.insert(0, header)
        pkg_strings, sizes = tabulate(data)
        # Create and add a separator.
            pkg_strings.insert(1, " ".join("-" * x for x in sizes))
        for val in pkg_strings:
            write_output(val)
def format_for_columns(
    pkgs: _ProcessedDists, options: Values
) -> tuple[list[list[str]], list[str]]:
    Convert the package data into something usable
    by output_package_listing_columns.
    header = ["Package", "Version"]
    running_outdated = options.outdated
    if running_outdated:
        header.extend(["Latest", "Type"])
    def wheel_build_tag(dist: BaseDistribution) -> str | None:
            wheel_file = dist.read_text("WHEEL")
        return Parser().parsestr(wheel_file).get("Build")
    build_tags = [wheel_build_tag(p) for p in pkgs]
    has_build_tags = any(build_tags)
    if has_build_tags:
        header.append("Build")
    has_editables = any(x.editable for x in pkgs)
    if has_editables:
        header.append("Editable project location")
        header.append("Location")
        header.append("Installer")
    for i, proj in enumerate(pkgs):
        # if we're working on the 'outdated' list, separate out the
        # latest_version and type
        row = [proj.raw_name, proj.raw_version]
            row.append(str(proj.latest_version))
            row.append(proj.latest_filetype)
            row.append(build_tags[i] or "")
            row.append(proj.editable_project_location or "")
            row.append(proj.location or "")
            row.append(proj.installer)
        data.append(row)
    return data, header
def format_for_json(packages: _ProcessedDists, options: Values) -> str:
            version = str(dist.version)
            version = dist.raw_version
        info = {
            "name": dist.raw_name,
            "version": version,
            info["location"] = dist.location or ""
            info["installer"] = dist.installer
            info["latest_version"] = str(dist.latest_version)
            info["latest_filetype"] = dist.latest_filetype
        editable_project_location = dist.editable_project_location
        if editable_project_location:
            info["editable_project_location"] = editable_project_location
        data.append(info)
    return json.dumps(data)
from ..common.utils import isStrSpace
# Search `[-+*][\n ]`, returns next pos after marker on success
# or -1 on fail.
def skipBulletListMarker(state: StateBlock, startLine: int) -> int:
    pos = state.bMarks[startLine] + state.tShift[startLine]
    maximum = state.eMarks[startLine]
        marker = state.src[pos]
    if marker not in ("*", "-", "+"):
    if pos < maximum:
        ch = state.src[pos]
        if not isStrSpace(ch):
            # " -test " - is not a list item
    return pos
# Search `\d+[.)][\n ]`, returns next pos after marker on success
def skipOrderedListMarker(state: StateBlock, startLine: int) -> int:
    start = state.bMarks[startLine] + state.tShift[startLine]
    pos = start
    # List marker should have at least 2 chars (digit + dot)
    if pos + 1 >= maximum:
    ch_ord = ord(ch)
    # /* 0 */  /* 9 */
    if ch_ord < 0x30 or ch_ord > 0x39:
        # EOL -> fail
        if pos >= maximum:
        if ch_ord >= 0x30 and ch_ord <= 0x39:
            # List marker should have no more than 9 digits
            # (prevents integer overflow in browsers)
            if pos - start >= 10:
        # found valid marker
        if ch in (")", "."):
            # " 1.test " - is not a list item
def markTightParagraphs(state: StateBlock, idx: int) -> None:
    level = state.level + 2
    i = idx + 2
    length = len(state.tokens) - 2
    while i < length:
        if state.tokens[i].level == level and state.tokens[i].type == "paragraph_open":
            state.tokens[i + 2].hidden = True
            state.tokens[i].hidden = True
def list_block(state: StateBlock, startLine: int, endLine: int, silent: bool) -> bool:
    LOGGER.debug("entering list: %s, %s, %s, %s", state, startLine, endLine, silent)
    isTerminatingParagraph = False
    tight = True
    if state.is_code_block(startLine):
    # Special case:
    #  - item 1
    #   - item 2
    #    - item 3
    #     - item 4
    #      - this one is a paragraph continuation
        state.listIndent >= 0
        and state.sCount[startLine] - state.listIndent >= 4
        and state.sCount[startLine] < state.blkIndent
    # limit conditions when list can interrupt
    # a paragraph (validation mode only)
    # Next list item should still terminate previous list item
    # This code can fail if plugins use blkIndent as well as lists,
    # but I hope the spec gets fixed long before that happens.
        silent
        and state.parentType == "paragraph"
        and state.sCount[startLine] >= state.blkIndent
        isTerminatingParagraph = True
    # Detect list type and position after marker
    posAfterMarker = skipOrderedListMarker(state, startLine)
    if posAfterMarker >= 0:
        isOrdered = True
        markerValue = int(state.src[start : posAfterMarker - 1])
        # If we're starting a new ordered list right after
        # a paragraph, it should start with 1.
        if isTerminatingParagraph and markerValue != 1:
        posAfterMarker = skipBulletListMarker(state, startLine)
            isOrdered = False
    # If we're starting a new unordered list right after
    # a paragraph, first line should not be empty.
        isTerminatingParagraph
        and state.skipSpaces(posAfterMarker) >= state.eMarks[startLine]
    # We should terminate list on style change. Remember first one to compare.
    markerChar = state.src[posAfterMarker - 1]
    # For validation mode we can terminate immediately
    if silent:
    # Start list
    listTokIdx = len(state.tokens)
    if isOrdered:
        token = state.push("ordered_list_open", "ol", 1)
        if markerValue != 1:
            token.attrs = {"start": markerValue}
        token = state.push("bullet_list_open", "ul", 1)
    token.map = listLines = [startLine, 0]
    token.markup = markerChar
    # Iterate list items
    nextLine = startLine
    prevEmptyEnd = False
    terminatorRules = state.md.block.ruler.getRules("list")
    oldParentType = state.parentType
    state.parentType = "list"
    while nextLine < endLine:
        pos = posAfterMarker
        maximum = state.eMarks[nextLine]
        initial = offset = (
            state.sCount[nextLine]
            + posAfterMarker
            - (state.bMarks[startLine] + state.tShift[startLine])
        while pos < maximum:
            if ch == "\t":
                offset += 4 - (offset + state.bsCount[nextLine]) % 4
            elif ch == " ":
                offset += 1
        contentStart = pos
        # trimming space in "-    \n  3" case, indent is 1 here
        indentAfterMarker = 1 if contentStart >= maximum else offset - initial
        # If we have more than 4 spaces, the indent is 1
        # (the rest is just indented code block)
        if indentAfterMarker > 4:
            indentAfterMarker = 1
        # "  -  test"
        #  ^^^^^ - calculating total length of this thing
        indent = initial + indentAfterMarker
        # Run subparser & write tokens
        token = state.push("list_item_open", "li", 1)
        token.map = itemLines = [startLine, 0]
            token.info = state.src[start : posAfterMarker - 1]
        # change current state, then restore it after parser subcall
        oldTight = state.tight
        oldTShift = state.tShift[startLine]
        oldSCount = state.sCount[startLine]
        #  - example list
        # ^ listIndent position will be here
        #   ^ blkIndent position will be here
        oldListIndent = state.listIndent
        state.listIndent = state.blkIndent
        state.blkIndent = indent
        state.tight = True
        state.tShift[startLine] = contentStart - state.bMarks[startLine]
        state.sCount[startLine] = offset
        if contentStart >= maximum and state.isEmpty(startLine + 1):
            # workaround for this case
            # (list item is empty, list terminates before "foo"):
            # ~~~~~~~~
            #   -
            #     foo
            state.line = min(state.line + 2, endLine)
            # NOTE in list.js this was:
            # state.md.block.tokenize(state, startLine, endLine, True)
            # but  tokeniz does not take the final parameter
            state.md.block.tokenize(state, startLine, endLine)
        # If any of list item is tight, mark list as tight
        if (not state.tight) or prevEmptyEnd:
            tight = False
        # Item become loose if finish with empty line,
        # but we should filter last element, because it means list finish
        prevEmptyEnd = (state.line - startLine) > 1 and state.isEmpty(state.line - 1)
        state.blkIndent = state.listIndent
        state.listIndent = oldListIndent
        state.tShift[startLine] = oldTShift
        state.sCount[startLine] = oldSCount
        state.tight = oldTight
        token = state.push("list_item_close", "li", -1)
        nextLine = startLine = state.line
        itemLines[1] = nextLine
        if nextLine >= endLine:
        contentStart = state.bMarks[startLine]
        # Try to check if list is terminated or continued.
        if state.sCount[nextLine] < state.blkIndent:
        # fail if terminating block found
        terminate = False
        for terminatorRule in terminatorRules:
            if terminatorRule(state, nextLine, endLine, True):
                terminate = True
        if terminate:
        # fail if list has another type
            posAfterMarker = skipOrderedListMarker(state, nextLine)
            if posAfterMarker < 0:
            start = state.bMarks[nextLine] + state.tShift[nextLine]
            posAfterMarker = skipBulletListMarker(state, nextLine)
        if markerChar != state.src[posAfterMarker - 1]:
    # Finalize list
        token = state.push("ordered_list_close", "ol", -1)
        token = state.push("bullet_list_close", "ul", -1)
    listLines[1] = nextLine
    state.line = nextLine
    state.parentType = oldParentType
    # mark paragraphs tight if needed
    if tight:
        markTightParagraphs(state, listTokIdx)
