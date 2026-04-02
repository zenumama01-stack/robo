"""Starting template for Google App Engine applications.
Use this project as a starting point if you are just beginning to build a Google
App Engine project. Remember to download the OAuth 2.0 client secrets which can
be obtained from the Developer Console <https://code.google.com/apis/console/>
and save them as 'client_secrets.json' in the project directory.
from oauth2client.contrib import appengine
import webapp2
import jinja2
JINJA_ENVIRONMENT = jinja2.Environment(
    loader=jinja2.FileSystemLoader(os.path.dirname(__file__)),
    autoescape=True,
    extensions=["jinja2.ext.autoescape"],
# CLIENT_SECRETS, name of a file containing the OAuth 2.0 information for this
# Console <http://code.google.com/apis/console>
CLIENT_SECRETS = os.path.join(os.path.dirname(__file__), "client_secrets.json")
# Helpful message to display in the browser if the CLIENT_SECRETS file
# is missing.
MISSING_CLIENT_SECRETS_MESSAGE = (
<h1>Warning: Please configure OAuth 2.0</h1>
To make this sample run you will need to populate the client_secrets.json file
found at:
<code>%s</code>.
<p>with information found on the <a
href="https://code.google.com/apis/console">APIs Console</a>.
    % CLIENT_SECRETS
http = httplib2.Http(memcache)
service = discovery.build("plus", "v1", http=http)
decorator = appengine.oauth2decorator_from_clientsecrets(
    CLIENT_SECRETS,
    scope="https://www.googleapis.com/auth/plus.me",
    message=MISSING_CLIENT_SECRETS_MESSAGE,
class MainHandler(webapp2.RequestHandler):
    @decorator.oauth_aware
    def get(self):
            "url": decorator.authorize_url(),
            "has_credentials": decorator.has_credentials(),
        template = JINJA_ENVIRONMENT.get_template("grant.html")
        self.response.write(template.render(variables))
class AboutHandler(webapp2.RequestHandler):
    @decorator.oauth_required
            http = decorator.http()
            user = service.people().get(userId="me").execute(http=http)
            text = "Hello, %s!" % user["displayName"]
            template = JINJA_ENVIRONMENT.get_template("welcome.html")
            self.response.write(template.render({"text": text}))
            self.redirect("/")
app = webapp2.WSGIApplication(
        ("/", MainHandler),
        ("/about", AboutHandler),
        (decorator.callback_path, decorator.callback_handler()),
    debug=True,
"""Simple command-line example for Custom Search.
Command-line application that does a search.
    # Build a service object for interacting with the API. Visit
    # the Google APIs Console <http://code.google.com/apis/console>
    # to get an API key for your own application.
    service = build(
        "customsearch", "v1", developerKey="<YOUR DEVELOPER KEY>"
    res = (
        service.cse()
            q="lectures",
            cx="017576662512468239146:omuauf_lfve",
    pprint.pprint(res)
"""Simple command-line example for The Google Search
API for Shopping.
Command-line application that does a search for products.
__author__ = "aherrman@google.com (Andy Herrman)"
# Uncomment the next line to get very detailed logging
# httplib2.debuglevel = 4
    p = build("shopping", "v1", developerKey="<YOUR DEVELOPER KEY>")
    # Search over all public offers:
    print("Searching all public offers.")
        p.products().list(country="US", source="public", q="android t-shirt").execute()
    print_items(res["items"])
    # Search over a specific merchant's offers:
    print("Searching Google Store.")
        p.products()
            q="android t-shirt",
            restrictBy="accountId:5968952",
    # Remember the Google Id of the last product
    googleId = res["items"][0]["product"]["googleId"]
    # Get data for the single public offer:
    print("Getting data for offer %s" % googleId)
            accountId="5968952",
            productIdType="gid",
            productId=googleId,
    print_item(res)
def print_item(item):
    """Displays a single item: title, merchant, link."""
    product = item["product"]
        "- %s [%s] (%s)"
        % (product["title"], product["author"]["name"], product["link"])
def print_items(items):
    """Displays a number of items."""
        print_item(item)
"""This application produces formatted listings for Google Cloud
   Storage buckets.
It takes a bucket name in the URL path and does an HTTP GET on the
corresponding Google Cloud Storage URL to obtain a listing of the bucket
contents. For example, if this app is invoked with the URI
http://bucket-list.appspot.com/foo, it would remove the bucket name 'foo',
append it to the Google Cloud Storage service URI and send a GET request to
the resulting URI. The bucket listing is returned in an XML document, which is
prepended with a reference to an XSLT style sheet for human readable
presentation.
More information about using Google App Engine apps and service accounts to
call Google APIs can be found here:
<https://developers.google.com/accounts/docs/OAuth2ServiceAccount>
<http://code.google.com/appengine/docs/python/appidentity/overview.html>
__author__ = "marccohen@google.com (Marc Cohen)"
from google.appengine.ext import webapp
from google.appengine.ext.webapp.util import run_wsgi_app
from oauth2client.contrib.appengine import AppAssertionCredentials
# Constants for the XSL stylesheet and the Google Cloud Storage URI.
XSL = '\n<?xml-stylesheet href="/listing.xsl" type="text/xsl"?>\n'
URI = "http://commondatastorage.googleapis.com"
# Obtain service account credentials and authorize HTTP connection.
credentials = AppAssertionCredentials(
    scope="https://www.googleapis.com/auth/devstorage.read_write"
http = credentials.authorize(httplib2.Http(memcache))
class MainHandler(webapp.RequestHandler):
            # Derive desired bucket name from path after domain name.
            bucket = self.request.path
            if bucket[-1] == "/":
                # Trim final slash, if necessary.
                bucket = bucket[:-1]
            # Send HTTP request to Google Cloud Storage to obtain bucket listing.
            resp, content = http.request(URI + bucket, "GET")
            if resp.status != 200:
                # If error getting bucket listing, raise exception.
                err = (
                    "Error: "
                    + str(resp.status)
                    + ", bucket: "
                    + bucket
                    + ", response: "
                    + str(content)
                raise Exception(err)
            # Edit returned bucket listing XML to insert a reference to our style
            # sheet for nice formatting and send results to client.
            content = re.sub("(<ListBucketResult)", XSL + "\\1", content)
            self.response.headers["Content-Type"] = "text/xml"
            self.response.out.write(content)
            self.response.headers["Content-Type"] = "text/plain"
            self.response.set_status(404)
            self.response.out.write(str(e))
    application = webapp.WSGIApplication(
            (".*", MainHandler),
    run_wsgi_app(application)
from webapp2_extras import jinja2
from oauth2client.contrib.appengine import OAuth2Decorator
import settings
decorator = OAuth2Decorator(
    client_id=settings.CLIENT_ID,
    client_secret=settings.CLIENT_SECRET,
    scope=settings.SCOPE,
service = build("tasks", "v1")
    def render_response(self, template, **context):
        renderer = jinja2.get_jinja2(app=self.app)
        rendered_value = renderer.render_template(template, **context)
        self.response.write(rendered_value)
        if decorator.has_credentials():
                service.tasks().list(tasklist="@default").execute(http=decorator.http())
            tasks = result.get("items", [])
            for task in tasks:
                task["title_short"] = truncate(task["title"], 26)
            self.render_response("index.html", tasks=tasks)
            url = decorator.authorize_url()
            self.render_response("index.html", tasks=[], authorize_url=url)
def truncate(s, l):
    return s[:l] + "..." if len(s) > l else s
application = webapp2.WSGIApplication(
"""Simple command-line example for Translate.
Command-line application that translates some text.
        "translate", "v2", developerKey="<YOUR DEVELOPER KEY>"
        service.translations()
        .list(source="en", target="fr", q=["flower", "car"])
from fastapi import FastAPI
app = FastAPI()
@app.get("/")
async def read_main():
    return {"msg": "Hello World"}
from typing import Annotated
from fastapi import FastAPI, Header, HTTPException
fake_secret_token = "coneofsilence"
fake_db = {
    "foo": {"id": "foo", "title": "Foo", "description": "There goes my hero"},
    "bar": {"id": "bar", "title": "Bar", "description": "The bartenders"},
    description: str | None = None
@app.get("/items/{item_id}", response_model=Item)
async def read_main(item_id: str, x_token: Annotated[str, Header()]):
    if x_token != fake_secret_token:
        raise HTTPException(status_code=400, detail="Invalid X-Token header")
    if item_id not in fake_db:
        raise HTTPException(status_code=404, detail="Item not found")
    return fake_db[item_id]
@app.post("/items/")
async def create_item(item: Item, x_token: Annotated[str, Header()]) -> Item:
    if item.id in fake_db:
        raise HTTPException(status_code=409, detail="Item already exists")
    fake_db[item.id] = item.model_dump()
async def read_main(item_id: str, x_token: str = Header()):
async def create_item(item: Item, x_token: str = Header()) -> Item:
async def root():
    return {"message": "Tomato"}
from fastapi import Depends, FastAPI
from .dependencies import get_query_token, get_token_header
from .internal import admin
from .routers import items, users
app = FastAPI(dependencies=[Depends(get_query_token)])
app.include_router(users.router)
app.include_router(items.router)
app.include_router(
    admin.router,
    prefix="/admin",
    tags=["admin"],
    dependencies=[Depends(get_token_header)],
    responses={418: {"description": "I'm a teapot"}},
    return {"message": "Hello Bigger Applications!"}
from .config import settings
@app.get("/info")
async def info():
        "app_name": settings.app_name,
        "admin_email": settings.admin_email,
        "items_per_user": settings.items_per_user,
from .config import Settings
@lru_cache
def get_settings():
    return Settings()
async def info(settings: Annotated[Settings, Depends(get_settings)]):
async def info(settings: Settings = Depends(get_settings)):
from . import config
    return config.Settings()
async def info(settings: Annotated[config.Settings, Depends(get_settings)]):
async def info(settings: config.Settings = Depends(get_settings)):
import http
from fastapi import FastAPI, Path, Query
external_docs = {
    "description": "External API documentation.",
    "url": "https://docs.example.com/api-general",
app = FastAPI(openapi_external_docs=external_docs)
@app.api_route("/api_route")
def non_operation():
    return {"message": "Hello World"}
def non_decorated_route():
app.add_api_route("/non_decorated_route", non_decorated_route)
@app.get("/text")
def get_text():
    return "Hello World"
@app.get("/path/{item_id}")
def get_id(item_id):
    return item_id
@app.get("/path/str/{item_id}")
def get_str_id(item_id: str):
@app.get("/path/int/{item_id}")
def get_int_id(item_id: int):
@app.get("/path/float/{item_id}")
def get_float_id(item_id: float):
@app.get("/path/bool/{item_id}")
def get_bool_id(item_id: bool):
@app.get("/path/param/{item_id}")
def get_path_param_id(item_id: str | None = Path()):
@app.get("/path/param-minlength/{item_id}")
def get_path_param_min_length(item_id: str = Path(min_length=3)):
@app.get("/path/param-maxlength/{item_id}")
def get_path_param_max_length(item_id: str = Path(max_length=3)):
@app.get("/path/param-min_maxlength/{item_id}")
def get_path_param_min_max_length(item_id: str = Path(max_length=3, min_length=2)):
@app.get("/path/param-gt/{item_id}")
def get_path_param_gt(item_id: float = Path(gt=3)):
@app.get("/path/param-gt0/{item_id}")
def get_path_param_gt0(item_id: float = Path(gt=0)):
@app.get("/path/param-ge/{item_id}")
def get_path_param_ge(item_id: float = Path(ge=3)):
@app.get("/path/param-lt/{item_id}")
def get_path_param_lt(item_id: float = Path(lt=3)):
@app.get("/path/param-lt0/{item_id}")
def get_path_param_lt0(item_id: float = Path(lt=0)):
@app.get("/path/param-le/{item_id}")
def get_path_param_le(item_id: float = Path(le=3)):
@app.get("/path/param-lt-gt/{item_id}")
def get_path_param_lt_gt(item_id: float = Path(lt=3, gt=1)):
@app.get("/path/param-le-ge/{item_id}")
def get_path_param_le_ge(item_id: float = Path(le=3, ge=1)):
@app.get("/path/param-lt-int/{item_id}")
def get_path_param_lt_int(item_id: int = Path(lt=3)):
@app.get("/path/param-gt-int/{item_id}")
def get_path_param_gt_int(item_id: int = Path(gt=3)):
@app.get("/path/param-le-int/{item_id}")
def get_path_param_le_int(item_id: int = Path(le=3)):
@app.get("/path/param-ge-int/{item_id}")
def get_path_param_ge_int(item_id: int = Path(ge=3)):
@app.get("/path/param-lt-gt-int/{item_id}")
def get_path_param_lt_gt_int(item_id: int = Path(lt=3, gt=1)):
@app.get("/path/param-le-ge-int/{item_id}")
def get_path_param_le_ge_int(item_id: int = Path(le=3, ge=1)):
@app.get("/query")
def get_query(query):
    return f"foo bar {query}"
@app.get("/query/optional")
def get_query_optional(query=None):
    if query is None:
        return "foo bar"
@app.get("/query/int")
def get_query_type(query: int):
@app.get("/query/int/optional")
def get_query_type_optional(query: int | None = None):
@app.get("/query/int/default")
def get_query_type_int_default(query: int = 10):
@app.get("/query/param")
def get_query_param(query=Query(default=None)):
@app.get("/query/param-required")
def get_query_param_required(query=Query()):
@app.get("/query/param-required/int")
def get_query_param_required_type(query: int = Query()):
@app.get("/enum-status-code", status_code=http.HTTPStatus.CREATED)
def get_enum_status_code():
@app.get("/query/frozenset")
def get_query_type_frozenset(query: frozenset[int] = Query(...)):
    return ",".join(map(str, sorted(query)))
@app.get("/query/list")
def get_query_list(device_ids: list[int] = Query()) -> list[int]:
    return device_ids
@app.get("/query/list-default")
def get_query_list_default(device_ids: list[int] = Query(default=[])) -> list[int]:
from . import a, b
app.include_router(a.router, prefix="/a")
app.include_router(b.router, prefix="/b")
from django.contrib.admin import FieldListFilter
from django.contrib.admin.exceptions import (
    DisallowedModelAdminLookup,
    DisallowedModelAdminToField,
    IS_FACETS_VAR,
    IS_POPUP_VAR,
    SOURCE_MODEL_VAR,
    TO_FIELD_VAR,
    IncorrectLookupParameters,
    build_q_object_from_lookup_parameters,
    get_fields_from_path,
    prepare_lookup_value,
    ImproperlyConfigured,
    SuspiciousOperation,
from django.core.paginator import InvalidPage
from django.db.models import F, Field, ManyToOneRel, OrderBy
from django.db.models.expressions import Combinable
from django.utils.timezone import make_aware
# Changelist settings
ALL_VAR = "all"
ORDER_VAR = "o"
PAGE_VAR = "p"
SEARCH_VAR = "q"
ERROR_FLAG = "e"
IGNORED_PARAMS = (
    ALL_VAR,
    ORDER_VAR,
    SEARCH_VAR,
class ChangeListSearchForm(forms.Form):
        # Populate "fields" dynamically because SEARCH_VAR is a variable:
        self.fields = {
            SEARCH_VAR: forms.CharField(required=False, strip=False),
class ChangeList:
    search_form_class = ChangeListSearchForm
        list_filter,
        date_hierarchy,
        search_fields,
        list_select_related,
        list_per_page,
        list_max_show_all,
        list_editable,
        model_admin,
        search_help_text,
        self.lookup_opts = self.opts
        self.root_queryset = model_admin.get_queryset(request)
        self.list_display = list_display
        self.list_display_links = list_display_links
        self.list_filter = list_filter
        self.has_filters = None
        self.has_active_filters = None
        self.clear_all_filters_qs = None
        self.date_hierarchy = date_hierarchy
        self.search_fields = search_fields
        self.list_select_related = list_select_related
        self.list_per_page = list_per_page
        self.list_max_show_all = list_max_show_all
        self.preserved_filters = model_admin.get_preserved_filters(request)
        self.sortable_by = sortable_by
        self.search_help_text = search_help_text
        self.formset = None
        # Get search parameters from the query string.
        _search_form = self.search_form_class(request.GET)
        if not _search_form.is_valid():
            for error in _search_form.errors.values():
                messages.error(request, ", ".join(error))
        self.query = _search_form.cleaned_data.get(SEARCH_VAR) or ""
            self.page_num = int(request.GET.get(PAGE_VAR, 1))
            self.page_num = 1
        self.show_all = ALL_VAR in request.GET
        self.is_popup = IS_POPUP_VAR in request.GET
        self.add_facets = model_admin.show_facets is ShowFacets.ALWAYS or (
            model_admin.show_facets is ShowFacets.ALLOW and IS_FACETS_VAR in request.GET
        self.is_facets_optional = model_admin.show_facets is ShowFacets.ALLOW
        to_field = request.GET.get(TO_FIELD_VAR)
        if to_field and not model_admin.to_field_allowed(request, to_field):
        self.to_field = to_field
        self.params = dict(request.GET.items())
        self.filter_params = dict(request.GET.lists())
        if PAGE_VAR in self.params:
            del self.params[PAGE_VAR]
            del self.filter_params[PAGE_VAR]
        if ERROR_FLAG in self.params:
            del self.params[ERROR_FLAG]
            del self.filter_params[ERROR_FLAG]
        self.remove_facet_link = self.get_query_string(remove=[IS_FACETS_VAR])
        self.add_facet_link = self.get_query_string({IS_FACETS_VAR: True})
        if self.is_popup:
            self.list_editable = ()
            self.list_editable = list_editable
        self.queryset = self.get_queryset(request)
        self.get_results(request)
            title = gettext("Select %s")
        elif self.model_admin.has_change_permission(request):
            title = gettext("Select %s to change")
            title = gettext("Select %s to view")
        self.title = title % self.opts.verbose_name
        self.pk_attname = self.lookup_opts.pk.attname
        return "<%s: model=%s model_admin=%s>" % (
            self.__class__.__qualname__,
            self.model.__qualname__,
            self.model_admin.__class__.__qualname__,
    def get_filters_params(self, params=None):
        Return all params except IGNORED_PARAMS.
        params = params or self.filter_params
        lookup_params = params.copy()  # a dictionary of the query string
        # Remove all the parameters that are globally and systematically
        # ignored.
        for ignored in IGNORED_PARAMS:
            if ignored in lookup_params:
                del lookup_params[ignored]
        return lookup_params
    def get_filters(self, request):
        lookup_params = self.get_filters_params()
        has_active_filters = False
        for key, value_list in lookup_params.items():
            for value in value_list:
                if not self.model_admin.lookup_allowed(key, value, request):
                    raise DisallowedModelAdminLookup(f"Filtering by {key} not allowed")
        filter_specs = []
        for list_filter in self.list_filter:
            lookup_params_count = len(lookup_params)
            if callable(list_filter):
                # This is simply a custom list filter class.
                spec = list_filter(request, lookup_params, self.model, self.model_admin)
                field_path = None
                if isinstance(list_filter, (tuple, list)):
                    # This is a custom FieldListFilter class for a given field.
                    field, field_list_filter_class = list_filter
                    # This is simply a field name, so use the default
                    # FieldListFilter class that has been registered for the
                    # type of the given field.
                    field, field_list_filter_class = list_filter, FieldListFilter.create
                if not isinstance(field, Field):
                    field_path = field
                    field = get_fields_from_path(self.model, field_path)[-1]
                spec = field_list_filter_class(
                    lookup_params,
                    field_path=field_path,
                # field_list_filter_class removes any lookup_params it
                # processes. If that happened, check if duplicates should be
                # removed.
                if lookup_params_count > len(lookup_params):
                    may_have_duplicates |= lookup_spawns_duplicates(
                        self.lookup_opts,
                        field_path,
            if spec and spec.has_output():
                filter_specs.append(spec)
                    has_active_filters = True
        if self.date_hierarchy:
            # Create bounded lookup parameters so that the query is more
            # efficient.
            year = lookup_params.pop("%s__year" % self.date_hierarchy, None)
            if year is not None:
                month = lookup_params.pop("%s__month" % self.date_hierarchy, None)
                day = lookup_params.pop("%s__day" % self.date_hierarchy, None)
                    from_date = datetime(
                        int(year[-1]),
                        int(month[-1] if month is not None else 1),
                        int(day[-1] if day is not None else 1),
                    raise IncorrectLookupParameters(e) from e
                if day:
                    to_date = from_date + timedelta(days=1)
                elif month:
                    # In this branch, from_date will always be the first of a
                    # month, so advancing 32 days gives the next month.
                    to_date = (from_date + timedelta(days=32)).replace(day=1)
                    to_date = from_date.replace(year=from_date.year + 1)
                if settings.USE_TZ:
                    from_date = make_aware(from_date)
                    to_date = make_aware(to_date)
                lookup_params.update(
                        "%s__gte" % self.date_hierarchy: [from_date],
                        "%s__lt" % self.date_hierarchy: [to_date],
        # At this point, all the parameters used by the various ListFilters
        # have been removed from lookup_params, which now only contains other
        # parameters passed via the query string. We now loop through the
        # remaining parameters both to ensure that all the parameters are valid
        # fields and to determine if at least one of them spawns duplicates. If
        # the lookup parameters aren't real fields, then bail out.
            for key, value in lookup_params.items():
                lookup_params[key] = prepare_lookup_value(key, value)
                may_have_duplicates |= lookup_spawns_duplicates(self.lookup_opts, key)
                filter_specs,
                bool(filter_specs),
                may_have_duplicates,
                has_active_filters,
        except FieldDoesNotExist as e:
    def get_query_string(self, new_params=None, remove=None):
        if new_params is None:
            new_params = {}
        if remove is None:
            remove = []
        p = self.filter_params.copy()
        for r in remove:
            for k in list(p):
                if k.startswith(r):
                    del p[k]
        for k, v in new_params.items():
                if k in p:
                p[k] = v
        return "?%s" % urlencode(sorted(p.items()), doseq=True)
    def get_results(self, request):
        paginator = self.model_admin.get_paginator(
            request, self.queryset, self.list_per_page
        # Get the number of objects, with admin filters applied.
        result_count = paginator.count
        # Get the total number of objects, with no admin filters applied.
        # Note this isn't necessarily the same as result_count in the case of
        # no filtering. Filters defined in list_filters may still apply some
        # default filtering which may be removed with query parameters.
        if self.model_admin.show_full_result_count:
            full_result_count = self.root_queryset.count()
            full_result_count = None
        can_show_all = result_count <= self.list_max_show_all
        multi_page = result_count > self.list_per_page
        # Get the list of objects to display on this page.
        if (self.show_all and can_show_all) or not multi_page:
            result_list = self.queryset._clone()
                result_list = paginator.page(self.page_num).object_list
            except InvalidPage:
                raise IncorrectLookupParameters
        self.result_count = result_count
        self.show_full_result_count = self.model_admin.show_full_result_count
        # Admin actions are shown if there is at least one entry
        # or if entries are not counted because show_full_result_count is
        # disabled
        self.show_admin_actions = not self.show_full_result_count or bool(
            full_result_count
        self.full_result_count = full_result_count
        self.result_list = result_list
        self.can_show_all = can_show_all
        self.multi_page = multi_page
        self.paginator = paginator
    def _get_default_ordering(self):
        ordering = []
        if self.model_admin.ordering:
            ordering = self.model_admin.ordering
        elif self.lookup_opts.ordering:
            ordering = self.lookup_opts.ordering
        return ordering
    def get_ordering_field(self, field_name):
        Return the proper model field name corresponding to the given
        field_name to use for ordering. field_name may either be the name of a
        proper model field, possibly across relations, or the name of a method
        (on the admin or model) or a callable with the 'admin_order_field'
        attribute. Return None if no proper model field name can be matched.
            field = self.lookup_opts.get_field(field_name)
            return field.name
            # See whether field_name is a name of a non-field
            # that allows sorting.
            if callable(field_name):
                attr = field_name
            elif hasattr(self.model_admin, field_name):
                attr = getattr(self.model_admin, field_name)
                    attr = getattr(self.model, field_name)
                    if LOOKUP_SEP in field_name:
                        return field_name
            if isinstance(attr, property) and hasattr(attr, "fget"):
                attr = attr.fget
            return getattr(attr, "admin_order_field", None)
    def get_ordering(self, request, queryset):
        Return the list of ordering fields for the change list.
        First check the get_ordering() method in model admin, then check
        the object's default ordering. Then, any manually-specified ordering
        from the query string overrides anything. Finally, a deterministic
        order is guaranteed by calling _get_deterministic_ordering() with the
        constructed ordering.
        params = self.params
        ordering = list(
            self.model_admin.get_ordering(request) or self._get_default_ordering()
        if params.get(ORDER_VAR):
            # Clear ordering and used params
            order_params = params[ORDER_VAR].split(".")
            for p in order_params:
                    none, pfx, idx = p.rpartition("-")
                    field_name = self.list_display[int(idx)]
                    order_field = self.get_ordering_field(field_name)
                    if not order_field:
                        continue  # No 'admin_order_field', skip it
                    if isinstance(order_field, OrderBy):
                        if pfx == "-":
                            order_field = order_field.copy()
                            order_field.reverse_ordering()
                        ordering.append(order_field)
                    elif hasattr(order_field, "resolve_expression"):
                        # order_field is an expression.
                        ordering.append(
                            order_field.desc() if pfx == "-" else order_field.asc()
                    # reverse order if order_field has already "-" as prefix
                    elif pfx == "-" and order_field.startswith(pfx):
                        ordering.append(order_field.removeprefix(pfx))
                        ordering.append(pfx + order_field)
                except (IndexError, ValueError):
                    continue  # Invalid ordering specified, skip it.
        # Add the given query's ordering fields, if any.
        ordering.extend(queryset.query.order_by)
        if queryset.order_by(*ordering).totally_ordered:
        return ordering + ["-pk"]
    def get_ordering_field_columns(self):
        Return a dictionary of ordering field column numbers and asc/desc.
        # We must cope with more than one column having the same underlying
        # sort field, so we base things on column numbers.
        ordering = self._get_default_ordering()
        ordering_fields = {}
        if ORDER_VAR not in self.params:
            # for ordering specified on ModelAdmin or model Meta, we don't know
            # the right column numbers absolutely, because there might be more
            # than one column associated with that ordering, so we guess.
            for field in ordering:
                if isinstance(field, (Combinable, OrderBy)):
                    if not isinstance(field, OrderBy):
                        field = field.asc()
                    if isinstance(field.expression, F):
                        order_type = "desc" if field.descending else "asc"
                        field = field.expression.name
                elif field.startswith("-"):
                    field = field.removeprefix("-")
                    order_type = "desc"
                    order_type = "asc"
                for index, attr in enumerate(self.list_display):
                    if self.get_ordering_field(attr) == field:
                        ordering_fields[index] = order_type
            for p in self.params[ORDER_VAR].split("."):
                    idx = int(idx)
                    continue  # skip it
                ordering_fields[idx] = "desc" if pfx == "-" else "asc"
        return ordering_fields
    def get_queryset(self, request, exclude_parameters=None):
        # First, we collect all the declared list filters.
            self.filter_specs,
            self.has_filters,
            remaining_lookup_params,
            filters_may_have_duplicates,
            self.has_active_filters,
        ) = self.get_filters(request)
        # Then, we let every list filter modify the queryset to its liking.
        qs = self.root_queryset
        for filter_spec in self.filter_specs:
                exclude_parameters is None
                or filter_spec.expected_parameters() != exclude_parameters
                new_qs = filter_spec.queryset(request, qs)
                if new_qs is not None:
                    qs = new_qs
            # Finally, we apply the remaining lookup parameters from the query
            # string (i.e. those that haven't already been processed by the
            # filters).
            q_object = build_q_object_from_lookup_parameters(remaining_lookup_params)
            qs = qs.filter(q_object)
        except (SuspiciousOperation, ImproperlyConfigured):
            # Allow certain types of errors to be re-raised as-is so that the
            # caller can treat them in a special way.
            # Every other error is caught with a naked except, because we don't
            # have any other way of validating lookup parameters. They might be
            # invalid if the keyword arguments are incorrect, or if the values
            # are not in the correct type, so we might get FieldError,
            # ValueError, ValidationError, or ?.
            raise IncorrectLookupParameters(e)
        if not qs.query.select_related:
            qs = self.apply_select_related(qs)
        # Set ordering.
        ordering = self.get_ordering(request, qs)
        # Apply search results
        qs, search_may_have_duplicates = self.model_admin.get_search_results(
            qs,
            self.query,
        # Set query string for clearing all filters.
        self.clear_all_filters_qs = self.get_query_string(
            new_params=remaining_lookup_params,
            remove=self.get_filters_params(),
        # Remove duplicates from results, if necessary
        if filters_may_have_duplicates | search_may_have_duplicates:
            return qs.distinct()
    def apply_select_related(self, qs):
        if self.list_select_related is True:
            return qs.select_related()
        if self.list_select_related is False:
            if self.has_related_field_in_list_display():
        if self.list_select_related:
            return qs.select_related(*self.list_select_related)
    def has_related_field_in_list_display(self):
        for field_name in self.list_display:
                if isinstance(field.remote_field, ManyToOneRel):
                    # <FK>_id field names don't require a join.
                    if field_name != field.attname:
    def url_for_result(self, result):
        pk = getattr(result, self.pk_attname)
            "admin:%s_%s_change" % (self.opts.app_label, self.opts.model_name),
            args=(quote(pk),),
EN: Abstract Factory Design Pattern
Intent: Lets you produce families of related objects without specifying their
concrete classes.
RU: Паттерн Абстрактная Фабрика
Назначение: Предоставляет интерфейс для создания семейств связанных или
зависимых объектов без привязки к их конкретным классам.
class AbstractFactory(ABC):
    EN: The Abstract Factory interface declares a set of methods that return
    different abstract products. These products are called a family and are
    related by a high-level theme or concept. Products of one family are usually
    able to collaborate among themselves. A family of products may have several
    variants, but the products of one variant are incompatible with products of
    RU: Интерфейс Абстрактной Фабрики объявляет набор методов, которые
    возвращают различные абстрактные продукты. Эти продукты называются
    семейством и связаны темой или концепцией высокого уровня. Продукты одного
    семейства обычно могут взаимодействовать между собой. Семейство продуктов
    может иметь несколько вариаций, но продукты одной вариации несовместимы с
    продуктами другой.
    def create_product_a(self) -> AbstractProductA:
    def create_product_b(self) -> AbstractProductB:
class ConcreteFactory1(AbstractFactory):
    EN: Concrete Factories produce a family of products that belong to a single
    variant. The factory guarantees that resulting products are compatible. Note
    that signatures of the Concrete Factory's methods return an abstract
    product, while inside the method a concrete product is instantiated.
    RU: Конкретная Фабрика производит семейство продуктов одной вариации.
    Фабрика гарантирует совместимость полученных продуктов. Обратите внимание,
    что сигнатуры методов Конкретной Фабрики возвращают абстрактный продукт, в
    то время как внутри метода создается экземпляр конкретного продукта.
        return ConcreteProductA1()
        return ConcreteProductB1()
class ConcreteFactory2(AbstractFactory):
    EN: Each Concrete Factory has a corresponding product variant.
    RU: Каждая Конкретная Фабрика имеет соответствующую вариацию продукта.
        return ConcreteProductA2()
        return ConcreteProductB2()
class AbstractProductA(ABC):
    EN: Each distinct product of a product family should have a base interface.
    All variants of the product must implement this interface.
    RU: Каждый отдельный продукт семейства продуктов должен иметь базовый
    интерфейс. Все вариации продукта должны реализовывать этот интерфейс.
    def useful_function_a(self) -> str:
EN: Concrete Products are created by corresponding Concrete Factories.
RU: Конкретные продукты создаются соответствующими Конкретными Фабриками.
class ConcreteProductA1(AbstractProductA):
        return "The result of the product A1."
class ConcreteProductA2(AbstractProductA):
        return "The result of the product A2."
class AbstractProductB(ABC):
    EN: Here's the the base interface of another product. All products can
    interact with each other, but proper interaction is possible only between
    products of the same concrete variant.
    RU: Базовый интерфейс другого продукта. Все продукты могут взаимодействовать
    друг с другом, но правильное взаимодействие возможно только между продуктами
    одной и той же конкретной вариации.
    def useful_function_b(self) -> None:
        EN: Product B is able to do its own thing...
        RU: Продукт B способен работать самостоятельно...
    def another_useful_function_b(self, collaborator: AbstractProductA) -> None:
        EN: ...but it also can collaborate with the ProductA.
        The Abstract Factory makes sure that all products it creates are of the
        same variant and thus, compatible.
        RU: ...а также взаимодействовать с Продуктами A той же вариации.
        Абстрактная Фабрика гарантирует, что все продукты, которые она создает,
        имеют одинаковую вариацию и, следовательно, совместимы.
RU: Конкретные Продукты создаются соответствующими Конкретными Фабриками.
class ConcreteProductB1(AbstractProductB):
    def useful_function_b(self) -> str:
        return "The result of the product B1."
    EN: The variant, Product B1, is only able to work correctly with the
    variant, Product A1. Nevertheless, it accepts any instance of
    AbstractProductA as an argument.
    RU: Продукт B1 может корректно работать только с Продуктом A1. Тем не менее,
    он принимает любой экземпляр Абстрактного Продукта А в качестве аргумента.
    def another_useful_function_b(self, collaborator: AbstractProductA) -> str:
        result = collaborator.useful_function_a()
        return f"The result of the B1 collaborating with the ({result})"
class ConcreteProductB2(AbstractProductB):
        return "The result of the product B2."
    def another_useful_function_b(self, collaborator: AbstractProductA):
        EN: The variant, Product B2, is only able to work correctly with the
        variant, Product A2. Nevertheless, it accepts any instance of
        RU: Продукт B2 может корректно работать только с Продуктом A2. Тем не
        менее, он принимает любой экземпляр Абстрактного Продукта А в качестве
        аргумента.
        return f"The result of the B2 collaborating with the ({result})"
def client_code(factory: AbstractFactory) -> None:
    EN: The client code works with factories and products only through abstract
    types: AbstractFactory and AbstractProduct. This lets you pass any factory
    or product subclass to the client code without breaking it.
    RU: Клиентский код работает с фабриками и продуктами только через
    абстрактные типы: Абстрактная Фабрика и Абстрактный Продукт. Это позволяет
    передавать любой подкласс фабрики или продукта клиентскому коду, не нарушая
    его.
    product_a = factory.create_product_a()
    product_b = factory.create_product_b()
    print(f"{product_b.useful_function_b()}")
    print(f"{product_b.another_useful_function_b(product_a)}", end="")
    EN: The client code can work with any concrete factory class.
    RU: Клиентский код может работать с любым конкретным классом фабрики.
    print("Client: Testing client code with the first factory type:")
    client_code(ConcreteFactory1())
    print("Client: Testing the same client code with the second factory type:")
    client_code(ConcreteFactory2())
EN: Adapter Design Pattern
Intent: Provides a unified interface that allows objects with incompatible
interfaces to collaborate.
RU: Паттерн Адаптер
Назначение: Позволяет объектам с несовместимыми интерфейсами работать вместе.
class Target:
    EN: The Target defines the domain-specific interface used by the client
    code.
    RU: Целевой класс объявляет интерфейс, с которым может работать клиентский
    код.
    def request(self) -> str:
        return "Target: The default target's behavior."
class Adaptee:
    EN: The Adaptee contains some useful behavior, but its interface is
    incompatible with the existing client code. The Adaptee needs some
    adaptation before the client code can use it.
    RU: Адаптируемый класс содержит некоторое полезное поведение, но его
    интерфейс несовместим с существующим клиентским кодом. Адаптируемый класс
    нуждается в некоторой доработке, прежде чем клиентский код сможет его
    использовать.
    def specific_request(self) -> str:
        return ".eetpadA eht fo roivaheb laicepS"
class Adapter(Target, Adaptee):
    EN: The Adapter makes the Adaptee's interface compatible with the Target's
    interface via multiple inheritance.
    RU: Адаптер делает интерфейс Адаптируемого класса совместимым с целевым
    интерфейсом благодаря множественному наследованию.
        return f"Adapter: (TRANSLATED) {self.specific_request()[::-1]}"
def client_code(target: "Target") -> None:
    EN: The client code supports all classes that follow the Target interface.
    RU: Клиентский код поддерживает все классы, использующие интерфейс Target.
    print(target.request(), end="")
    print("Client: I can work just fine with the Target objects:")
    target = Target()
    client_code(target)
    adaptee = Adaptee()
    print("Client: The Adaptee class has a weird interface. "
          "See, I don't understand it:")
    print(f"Adaptee: {adaptee.specific_request()}", end="\n\n")
    print("Client: But I can work with it via the Adapter:")
    adapter = Adapter()
    client_code(adapter)
class Adapter(Target):
    interface via composition.
    интерфейсом благодаря агрегации.
    def __init__(self, adaptee: Adaptee) -> None:
        self.adaptee = adaptee
        return f"Adapter: (TRANSLATED) {self.adaptee.specific_request()[::-1]}"
def client_code(target: Target) -> None:
    adapter = Adapter(adaptee)
EN: Bridge Design Pattern
Intent: Lets you split a large class or a set of closely related classes into
two separate hierarchies—abstraction and implementation—which can be developed
independently of each other.
           /     \                        A         N
         Aa      Ab        ===>        /     \     / \
        / \     /  \                 Aa(N) Ab(N)  1   2
      Aa1 Aa2  Ab1 Ab2
RU: Паттерн Мост
Назначение: Разделяет один или несколько классов на две отдельные иерархии —
абстракцию и реализацию, позволяя изменять их независимо друг от друга.
class Abstraction:
    EN: The Abstraction defines the interface for the "control" part of the two
    class hierarchies. It maintains a reference to an object of the
    Implementation hierarchy and delegates all of the real work to this object.
    RU: Абстракция устанавливает интерфейс для «управляющей» части двух иерархий
    классов. Она содержит ссылку на объект из иерархии Реализации и делегирует
    ему всю настоящую работу.
    def __init__(self, implementation: Implementation) -> None:
        self.implementation = implementation
    def operation(self) -> str:
        return (f"Abstraction: Base operation with:\n"
                f"{self.implementation.operation_implementation()}")
class ExtendedAbstraction(Abstraction):
    EN: You can extend the Abstraction without changing the Implementation
    classes.
    RU: Можно расширить Абстракцию без изменения классов Реализации.
        return (f"ExtendedAbstraction: Extended operation with:\n"
class Implementation(ABC):
    EN: The Implementation defines the interface for all implementation classes.
    It doesn't have to match the Abstraction's interface. In fact, the two
    interfaces can be entirely different. Typically the Implementation interface
    provides only primitive operations, while the Abstraction defines higher-
    level operations based on those primitives.
    RU: Реализация устанавливает интерфейс для всех классов реализации. Он не
    должен соответствовать интерфейсу Абстракции. На практике оба интерфейса
    могут быть совершенно разными. Как правило, интерфейс Реализации
    предоставляет только примитивные операции, в то время как Абстракция
    определяет операции более высокого уровня, основанные на этих примитивах.
    def operation_implementation(self) -> str:
EN: Each Concrete Implementation corresponds to a specific platform and
implements the Implementation interface using that platform's API.
RU: Каждая Конкретная Реализация соответствует определённой платформе и
реализует интерфейс Реализации с использованием API этой платформы.
class ConcreteImplementationA(Implementation):
        return "ConcreteImplementationA: Here's the result on the platform A."
class ConcreteImplementationB(Implementation):
        return "ConcreteImplementationB: Here's the result on the platform B."
def client_code(abstraction: Abstraction) -> None:
    EN: Except for the initialization phase, where an Abstraction object gets
    linked with a specific Implementation object, the client code should only
    depend on the Abstraction class. This way the client code can support any
    abstraction-implementation combination.
    RU: За исключением этапа инициализации, когда объект Абстракции связывается
    с определённым объектом Реализации, клиентский код должен зависеть только от
    класса Абстракции. Таким образом, клиентский код может поддерживать любую
    комбинацию абстракции и реализации.
    print(abstraction.operation(), end="")
    EN: The client code should be able to work with any pre-configured
    RU: Клиентский код должен работать с любой предварительно сконфигурированной
    комбинацией абстракции и реализации.
    implementation = ConcreteImplementationA()
    abstraction = Abstraction(implementation)
    client_code(abstraction)
    implementation = ConcreteImplementationB()
    abstraction = ExtendedAbstraction(implementation)
EN: Builder Design Pattern
Intent: Lets you construct complex objects step by step. The pattern allows you
to produce different types and representations of an object using the same
construction code.
RU: Паттерн Строитель
Назначение: Позволяет создавать сложные объекты пошагово. Строитель даёт
возможность использовать один и тот же код строительства для получения разных
представлений объектов.
class Builder(ABC):
    EN: The Builder interface specifies methods for creating the different parts
    of the Product objects.
    RU: Интерфейс Строителя объявляет создающие методы для различных частей
    объектов Продуктов.
    def product(self) -> None:
    def produce_part_a(self) -> None:
    def produce_part_b(self) -> None:
    def produce_part_c(self) -> None:
class ConcreteBuilder1(Builder):
    EN: The Concrete Builder classes follow the Builder interface and provide
    specific implementations of the building steps. Your program may have
    several variations of Builders, implemented differently.
    RU: Классы Конкретного Строителя следуют интерфейсу Строителя и
    предоставляют конкретные реализации шагов построения. Ваша программа может
    иметь несколько вариантов Строителей, реализованных по-разному.
        EN: A fresh builder instance should contain a blank product object,
        which is used in further assembly.
        RU: Новый экземпляр строителя должен содержать пустой объект продукта,
        который используется в дальнейшей сборке.
    def reset(self) -> None:
        self._product = Product1()
    def product(self) -> Product1:
        EN: Concrete Builders are supposed to provide their own methods for
        retrieving results. That's because various types of builders may create
        entirely different products that don't follow the same interface.
        Therefore, such methods cannot be declared in the base Builder interface
        (at least in a statically typed programming language).
        Usually, after returning the end result to the client, a builder
        instance is expected to be ready to start producing another product.
        That's why it's a usual practice to call the reset method at the end of
        the `getProduct` method body. However, this behavior is not mandatory,
        and you can make your builders wait for an explicit reset call from the
        client code before disposing of the previous result.
        RU: Конкретные Строители должны предоставить свои собственные методы
        получения результатов. Это связано с тем, что различные типы строителей
        могут создавать совершенно разные продукты с разными интерфейсами.
        Поэтому такие методы не могут быть объявлены в базовом интерфейсе
        Строителя (по крайней мере, в статически типизированном языке
        программирования).
        Как правило, после возвращения конечного результата клиенту, экземпляр
        строителя должен быть готов к началу производства следующего продукта.
        Поэтому обычной практикой является вызов метода сброса в конце тела
        метода getProduct. Однако такое поведение не является обязательным, вы
        можете заставить своих строителей ждать явного запроса на сброс из кода
        клиента, прежде чем избавиться от предыдущего результата.
        product = self._product
        return product
        self._product.add("PartA1")
        self._product.add("PartB1")
        self._product.add("PartC1")
class Product1():
    EN: It makes sense to use the Builder pattern only when your products are
    quite complex and require extensive configuration.
    Unlike in other creational patterns, different concrete builders can produce
    unrelated products. In other words, results of various builders may not
    always follow the same interface.
    RU: Имеет смысл использовать паттерн Строитель только тогда, когда ваши
    продукты достаточно сложны и требуют обширной конфигурации.
    В отличие от других порождающих паттернов, различные конкретные строители
    могут производить несвязанные продукты. Другими словами, результаты
    различных строителей могут не всегда следовать одному и тому же интерфейсу.
        self.parts = []
    def add(self, part: Any) -> None:
        self.parts.append(part)
    def list_parts(self) -> None:
        print(f"Product parts: {', '.join(self.parts)}", end="")
class Director:
    EN: The Director is only responsible for executing the building steps in a
    particular sequence. It is helpful when producing products according to a
    specific order or configuration. Strictly speaking, the Director class is
    optional, since the client can control builders directly.
    RU: Директор отвечает только за выполнение шагов построения в определённой
    последовательности. Это полезно при производстве продуктов в определённом
    порядке или особой конфигурации. Строго говоря, класс Директор необязателен,
    так как клиент может напрямую управлять строителями.
        self._builder = None
    def builder(self) -> Builder:
        return self._builder
    @builder.setter
    def builder(self, builder: Builder) -> None:
        EN: The Director works with any builder instance that the client code
        passes to it. This way, the client code may alter the final type of the
        newly assembled product.
        RU: Директор работает с любым экземпляром строителя, который передаётся
        ему клиентским кодом. Таким образом, клиентский код может изменить
        конечный тип вновь собираемого продукта.
        self._builder = builder
    EN: The Director can construct several product variations using the same
    building steps.
    RU: Директор может строить несколько вариаций продукта, используя одинаковые
    шаги построения.
    def build_minimal_viable_product(self) -> None:
        self.builder.produce_part_a()
    def build_full_featured_product(self) -> None:
        self.builder.produce_part_b()
        self.builder.produce_part_c()
    EN: The client code creates a builder object, passes it to the director and
    then initiates the construction process. The end result is retrieved from
    the builder object.
    RU: Клиентский код создаёт объект-строитель, передаёт его директору, а затем
    инициирует процесс построения. Конечный результат извлекается из
    объекта-строителя.
    director = Director()
    builder = ConcreteBuilder1()
    director.builder = builder
    print("Standard basic product: ")
    director.build_minimal_viable_product()
    builder.product.list_parts()
    print("Standard full featured product: ")
    director.build_full_featured_product()
    # EN: Remember, the Builder pattern can be used without a Director class.
    # RU: Помните, что паттерн Строитель можно использовать без класса Директор.
    print("Custom product: ")
    builder.produce_part_a()
    builder.produce_part_b()
EN: Chain of Responsibility Design Pattern
Intent: Lets you pass requests along a chain of handlers. Upon receiving a
request, each handler decides either to process the request or to pass it to the
next handler in the chain.
RU: Паттерн Цепочка обязанностей
Назначение: Позволяет передавать запросы последовательно по цепочке
обработчиков. Каждый последующий обработчик решает, может ли он обработать
запрос сам и стоит ли передавать запрос дальше по цепи.
from typing import Any, Optional
class Handler(ABC):
    EN: The Handler interface declares a method for building the chain of
    handlers. It also declares a method for executing a request.
    RU: Интерфейс Обработчика объявляет метод построения цепочки обработчиков.
    Он также объявляет метод для выполнения запроса.
    def set_next(self, handler: Handler) -> Handler:
    def handle(self, request) -> Optional[str]:
class AbstractHandler(Handler):
    EN: The default chaining behavior can be implemented inside a base handler
    class.
    RU: Поведение цепочки по умолчанию может быть реализовано внутри базового
    класса обработчика.
    _next_handler: Handler = None
        self._next_handler = handler
        # EN: Returning a handler from here will let us link handlers in a
        # convenient way like this:
        # monkey.set_next(squirrel).set_next(dog)
        # RU: Возврат обработчика отсюда позволит связать обработчики простым
        # способом, вот так:
    def handle(self, request: Any) -> str:
        if self._next_handler:
            return self._next_handler.handle(request)
EN: All Concrete Handlers either handle a request or pass it to the next handler
in the chain.
RU: Все Конкретные Обработчики либо обрабатывают запрос, либо передают его
следующему обработчику в цепочке.
class MonkeyHandler(AbstractHandler):
        if request == "Banana":
            return f"Monkey: I'll eat the {request}"
            return super().handle(request)
class SquirrelHandler(AbstractHandler):
        if request == "Nut":
            return f"Squirrel: I'll eat the {request}"
class DogHandler(AbstractHandler):
        if request == "MeatBall":
            return f"Dog: I'll eat the {request}"
def client_code(handler: Handler) -> None:
    EN: The client code is usually suited to work with a single handler. In most
    cases, it is not even aware that the handler is part of a chain.
    RU: Обычно клиентский код приспособлен для работы с единственным
    обработчиком. В большинстве случаев клиенту даже неизвестно, что этот
    обработчик является частью цепочки.
    for food in ["Nut", "Banana", "Cup of coffee"]:
        print(f"\nClient: Who wants a {food}?")
        result = handler.handle(food)
            print(f"  {result}", end="")
            print(f"  {food} was left untouched.", end="")
    monkey = MonkeyHandler()
    squirrel = SquirrelHandler()
    dog = DogHandler()
    monkey.set_next(squirrel).set_next(dog)
    # EN: The client should be able to send a request to any handler, not just
    # the first one in the chain.
    # RU: Клиент должен иметь возможность отправлять запрос любому обработчику,
    # а не только первому в цепочке.
    print("Chain: Monkey > Squirrel > Dog")
    client_code(monkey)
    print("Subchain: Squirrel > Dog")
    client_code(squirrel)
EN: Command Design Pattern
Intent: Turns a request into a stand-alone object that contains all information
about the request. This transformation lets you parameterize methods with
different requests, delay or queue a request's execution, and support undoable
operations.
RU: Паттерн Команда
Назначение: Превращает запросы в объекты, позволяя передавать их как аргументы
при вызове методов, ставить запросы в очередь, логировать их, а также
поддерживать отмену операций.
class Command(ABC):
    EN: The Command interface declares a method for executing a command.
    RU: Интерфейс Команды объявляет метод для выполнения команд.
    def execute(self) -> None:
class SimpleCommand(Command):
    EN: Some commands can implement simple operations on their own.
    RU: Некоторые команды способны выполнять простые операции самостоятельно.
    def __init__(self, payload: str) -> None:
        self._payload = payload
        print(f"SimpleCommand: See, I can do simple things like printing"
              f"({self._payload})")
class ComplexCommand(Command):
    EN: However, some commands can delegate more complex operations to other
    objects, called "receivers."
    RU: Но есть и команды, которые делегируют более сложные операции другим
    объектам, называемым «получателями».
    def __init__(self, receiver: Receiver, a: str, b: str) -> None:
        EN: Complex commands can accept one or several receiver objects along
        with any context data via the constructor.
        RU: Сложные команды могут принимать один или несколько
        объектов-получателей вместе с любыми данными о контексте через
        конструктор.
        self._receiver = receiver
        self._a = a
        self._b = b
        EN: Commands can delegate to any methods of a receiver.
        RU: Команды могут делегировать выполнение любым методам получателя.
        print("ComplexCommand: Complex stuff should be done by a receiver object", end="")
        self._receiver.do_something(self._a)
        self._receiver.do_something_else(self._b)
class Receiver:
    EN: The Receiver classes contain some important business logic. They know
    how to perform all kinds of operations, associated with carrying out a
    request. In fact, any class may serve as a Receiver.
    RU: Классы Получателей содержат некую важную бизнес-логику. Они умеют
    выполнять все виды операций, связанных с выполнением запроса. Фактически,
    любой класс может выступать Получателем.
    def do_something(self, a: str) -> None:
        print(f"\nReceiver: Working on ({a}.)", end="")
    def do_something_else(self, b: str) -> None:
        print(f"\nReceiver: Also working on ({b}.)", end="")
class Invoker:
    EN: The Invoker is associated with one or several commands. It sends a
    request to the command.
    RU: Отправитель связан с одной или несколькими командами. Он отправляет
    запрос команде.
    _on_start = None
    _on_finish = None
    EN: Initialize commands.
    RU: Инициализация команд.
    def set_on_start(self, command: Command):
        self._on_start = command
    def set_on_finish(self, command: Command):
        self._on_finish = command
    def do_something_important(self) -> None:
        EN: The Invoker does not depend on concrete command or receiver classes.
        The Invoker passes a request to a receiver indirectly, by executing a
        RU: Отправитель не зависит от классов конкретных команд и получателей.
        Отправитель передаёт запрос получателю косвенно, выполняя команду.
        print("Invoker: Does anybody want something done before I begin?")
        if isinstance(self._on_start, Command):
            self._on_start.execute()
        print("Invoker: ...doing something really important...")
        print("Invoker: Does anybody want something done after I finish?")
        if isinstance(self._on_finish, Command):
            self._on_finish.execute()
    EN: The client code can parameterize an invoker with any commands.
    RU: Клиентский код может параметризовать отправителя любыми командами.
    invoker = Invoker()
    invoker.set_on_start(SimpleCommand("Say Hi!"))
    receiver = Receiver()
    invoker.set_on_finish(ComplexCommand(
        receiver, "Send email", "Save report"))
    invoker.do_something_important()
EN: Composite Design Pattern
Intent: Lets you compose objects into tree structures and then work with these
structures as if they were individual objects.
RU: Паттерн Компоновщик
Назначение: Позволяет сгруппировать объекты в древовидную структуру, а затем
работать с ними так, как будто это единичный объект.
class Component(ABC):
    EN: The base Component class declares common operations for both simple and
    complex objects of a composition.
    RU: Базовый класс Компонент объявляет общие операции как для простых, так и
    для сложных объектов структуры.
    def parent(self) -> Component:
        return self._parent
    @parent.setter
    def parent(self, parent: Component):
        EN: Optionally, the base Component can declare an interface for setting
        and accessing a parent of the component in a tree structure. It can also
        provide some default implementation for these methods.
        RU: При необходимости базовый Компонент может объявить интерфейс для
        установки и получения родителя компонента в древовидной структуре. Он
        также может предоставить некоторую реализацию по умолчанию для этих
        методов.
        self._parent = parent
    EN: In some cases, it would be beneficial to define the child-management
    operations right in the base Component class. This way, you won't need to
    expose any concrete component classes to the client code, even during the
    object tree assembly. The downside is that these methods will be empty for
    the leaf-level components.
    RU: В некоторых случаях целесообразно определить операции управления
    потомками прямо в базовом классе Компонент. Таким образом, вам не нужно
    будет предоставлять конкретные классы компонентов клиентскому коду, даже во
    время сборки дерева объектов. Недостаток такого подхода в том, что эти
    методы будут пустыми для компонентов уровня листа.
    def add(self, component: Component) -> None:
    def remove(self, component: Component) -> None:
    def is_composite(self) -> bool:
        EN: You can provide a method that lets the client code figure out
        whether a component can bear children.
        RU: Вы можете предоставить метод, который позволит клиентскому коду
        понять, может ли компонент иметь вложенные объекты.
        EN: The base Component may implement some default behavior or leave it
        to concrete classes (by declaring the method containing the behavior as
        "abstract").
        RU: Базовый Компонент может сам реализовать некоторое поведение по
        умолчанию или поручить это конкретным классам, объявив метод, содержащий
        поведение абстрактным.
class Leaf(Component):
    EN: The Leaf class represents the end objects of a composition. A leaf can't
    have any children.
    Usually, it's the Leaf objects that do the actual work, whereas Composite
    objects only delegate to their sub-components.
    RU: Класс Лист представляет собой конечные объекты структуры. Лист не может
    иметь вложенных компонентов.
    Обычно объекты Листьев выполняют фактическую работу, тогда как объекты
    Контейнера лишь делегируют работу своим подкомпонентам.
        return "Leaf"
class Composite(Component):
    EN: The Composite class represents the complex components that may have
    children. Usually, the Composite objects delegate the actual work to their
    children and then "sum-up" the result.
    RU: Класс Контейнер содержит сложные компоненты, которые могут иметь
    вложенные компоненты. Обычно объекты Контейнеры делегируют фактическую
    работу своим детям, а затем «суммируют» результат.
        self._children: List[Component] = []
    EN: A composite object can add or remove other components (both simple or
    complex) to or from its child list.
    RU: Объект контейнера может как добавлять компоненты в свой список вложенных
    компонентов, так и удалять их, как простые, так и сложные.
        self._children.append(component)
        component.parent = self
        self._children.remove(component)
        component.parent = None
        EN: The Composite executes its primary logic in a particular way. It
        traverses recursively through all its children, collecting and summing
        their results. Since the composite's children pass these calls to their
        children and so forth, the whole object tree is traversed as a result.
        RU: Контейнер выполняет свою основную логику особым образом. Он проходит
        рекурсивно через всех своих детей, собирая и суммируя их результаты.
        Поскольку потомки контейнера передают эти вызовы своим потомкам и так
        далее, в результате обходится всё дерево объектов.
        for child in self._children:
            results.append(child.operation())
        return f"Branch({'+'.join(results)})"
def client_code(component: Component) -> None:
    EN: The client code works with all of the components via the base interface.
    RU: Клиентский код работает со всеми компонентами через базовый интерфейс.
    print(f"RESULT: {component.operation()}", end="")
def client_code2(component1: Component, component2: Component) -> None:
    EN: Thanks to the fact that the child-management operations are declared in
    the base Component class, the client code can work with any component,
    simple or complex, without depending on their concrete classes.
    RU: Благодаря тому, что операции управления потомками объявлены в базовом
    классе Компонента, клиентский код может работать как с простыми, так и со
    сложными компонентами, вне зависимости от их конкретных классов.
    if component1.is_composite():
        component1.add(component2)
    print(f"RESULT: {component1.operation()}", end="")
    # EN: This way the client code can support the simple leaf components...
    # RU: Таким образом, клиентский код может поддерживать простые
    # компоненты-листья...
    simple = Leaf()
    print("Client: I've got a simple component:")
    client_code(simple)
    # EN: ...as well as the complex composites.
    # RU: ...а также сложные контейнеры.
    tree = Composite()
    branch1 = Composite()
    branch1.add(Leaf())
    branch2 = Composite()
    branch2.add(Leaf())
    tree.add(branch1)
    tree.add(branch2)
    print("Client: Now I've got a composite tree:")
    client_code(tree)
    print("Client: I don't need to check the components classes even when managing the tree:")
    client_code2(tree, simple)
EN: Decorator Design Pattern
Intent: Lets you attach new behaviors to objects by placing these objects inside
special wrapper objects that contain the behaviors.
RU: Паттерн Декоратор
Назначение: Позволяет динамически добавлять объектам новую функциональность,
оборачивая их в полезные «обёртки».
class Component():
    EN: The base Component interface defines operations that can be altered by
    decorators.
    RU: Базовый интерфейс Компонента определяет поведение, которое изменяется
    декораторами.
class ConcreteComponent(Component):
    EN: Concrete Components provide default implementations of the operations.
    There might be several variations of these classes.
    RU: Конкретные Компоненты предоставляют реализации поведения по умолчанию.
    Может быть несколько вариаций этих классов.
        return "ConcreteComponent"
class Decorator(Component):
    EN: The base Decorator class follows the same interface as the other
    components. The primary purpose of this class is to define the wrapping
    interface for all concrete decorators. The default implementation of the
    wrapping code might include a field for storing a wrapped component and the
    means to initialize it.
    RU: Базовый класс Декоратора следует тому же интерфейсу, что и другие
    компоненты. Основная цель этого класса - определить интерфейс обёртки для
    всех конкретных декораторов. Реализация кода обёртки по умолчанию может
    включать в себя поле для хранения завёрнутого компонента и средства его
    инициализации.
    _component: Component = None
    def __init__(self, component: Component) -> None:
        self._component = component
    def component(self) -> Component:
        EN: The Decorator delegates all work to the wrapped component.
        RU: Декоратор делегирует всю работу обёрнутому компоненту.
        return self._component
        return self._component.operation()
class ConcreteDecoratorA(Decorator):
    EN: Concrete Decorators call the wrapped object and alter its result in some
    RU: Конкретные Декораторы вызывают обёрнутый объект и изменяют его результат
    некоторым образом.
        EN: Decorators may call parent implementation of the operation, instead
        of calling the wrapped object directly. This approach simplifies
        extension of decorator classes.
        RU: Декораторы могут вызывать родительскую реализацию операции, вместо
        того, чтобы вызвать обёрнутый объект напрямую. Такой подход упрощает
        расширение классов декораторов.
        return f"ConcreteDecoratorA({self.component.operation()})"
class ConcreteDecoratorB(Decorator):
    EN: Decorators can execute their behavior either before or after the call to
    a wrapped object.
    RU: Декораторы могут выполнять своё поведение до или после вызова обёрнутого
    объекта.
        return f"ConcreteDecoratorB({self.component.operation()})"
    EN: The client code works with all objects using the Component interface.
    This way it can stay independent of the concrete classes of components it
    works with.
    RU: Клиентский код работает со всеми объектами, используя интерфейс
    Компонента. Таким образом, он остаётся независимым от конкретных классов
    компонентов, с которыми работает.
    # EN: This way the client code can support both simple components...
    # RU: Таким образом, клиентский код может поддерживать как простые
    # компоненты...
    simple = ConcreteComponent()
    # EN: ...as well as decorated ones.
    # Note how decorators can wrap not only simple components but the other
    # decorators as well.
    # RU: ...так и декорированные.
    # Обратите внимание, что декораторы могут обёртывать не только простые
    # компоненты, но и другие декораторы.
    decorator1 = ConcreteDecoratorA(simple)
    decorator2 = ConcreteDecoratorB(decorator1)
    print("Client: Now I've got a decorated component:")
    client_code(decorator2)
EN: Facade Design Pattern
Intent: Provides a simplified interface to a library, a framework, or any other
complex set of classes.
RU: Паттерн Фасад
Назначение: Предоставляет простой интерфейс к сложной системе классов,
библиотеке или фреймворку.
class Facade:
    EN: The Facade class provides a simple interface to the complex logic of one
    or several subsystems. The Facade delegates the client requests to the
    appropriate objects within the subsystem. The Facade is also responsible for
    managing their lifecycle. All of this shields the client from the undesired
    complexity of the subsystem.
    RU: Класс Фасада предоставляет простой интерфейс для сложной логики одной
    или нескольких подсистем. Фасад делегирует запросы клиентов соответствующим
    объектам внутри подсистемы. Фасад также отвечает за управление их жизненным
    циклом. Все это защищает клиента от нежелательной сложности подсистемы.
    def __init__(self, subsystem1: Subsystem1, subsystem2: Subsystem2) -> None:
        EN: Depending on your application's needs, you can provide the Facade
        with existing subsystem objects or force the Facade to create them on
        its own.
        RU: В зависимости от потребностей вашего приложения вы можете
        предоставить Фасаду существующие объекты подсистемы или заставить Фасад
        создать их самостоятельно.
        self._subsystem1 = subsystem1 or Subsystem1()
        self._subsystem2 = subsystem2 or Subsystem2()
        EN: The Facade's methods are convenient shortcuts to the sophisticated
        functionality of the subsystems. However, clients get only to a fraction
        of a subsystem's capabilities.
        RU: Методы Фасада удобны для быстрого доступа к сложной функциональности
        подсистем. Однако клиенты получают только часть возможностей подсистемы.
        results.append("Facade initializes subsystems:")
        results.append(self._subsystem1.operation1())
        results.append(self._subsystem2.operation1())
        results.append("Facade orders subsystems to perform the action:")
        results.append(self._subsystem1.operation_n())
        results.append(self._subsystem2.operation_z())
        return "\n".join(results)
class Subsystem1:
    EN: The Subsystem can accept requests either from the facade or client
    directly. In any case, to the Subsystem, the Facade is yet another client,
    and it's not a part of the Subsystem.
    RU: Подсистема может принимать запросы либо от фасада, либо от клиента
    напрямую. В любом случае, для Подсистемы Фасад – это ещё один клиент, и он
    не является частью Подсистемы.
    def operation1(self) -> str:
        return "Subsystem1: Ready!"
    def operation_n(self) -> str:
        return "Subsystem1: Go!"
class Subsystem2:
    EN: Some facades can work with multiple subsystems at the same time.
    RU: Некоторые фасады могут работать с разными подсистемами одновременно.
        return "Subsystem2: Get ready!"
    def operation_z(self) -> str:
        return "Subsystem2: Fire!"
def client_code(facade: Facade) -> None:
    EN: The client code works with complex subsystems through a simple interface
    provided by the Facade. When a facade manages the lifecycle of the
    subsystem, the client might not even know about the existence of the
    subsystem. This approach lets you keep the complexity under control.
    RU: Клиентский код работает со сложными подсистемами через простой
    интерфейс, предоставляемый Фасадом. Когда фасад управляет жизненным циклом
    подсистемы, клиент может даже не знать о существовании подсистемы. Такой
    подход позволяет держать сложность под контролем.
    print(facade.operation(), end="")
    # EN: The client code may have some of the subsystem's objects already
    # created. In this case, it might be worthwhile to initialize the Facade
    # with these objects instead of letting the Facade create new instances.
    # RU: В клиентском коде могут быть уже созданы некоторые объекты подсистемы.
    # В этом случае может оказаться целесообразным инициализировать Фасад с
    # этими объектами вместо того, чтобы позволить Фасаду создавать новые
    # экземпляры.
    subsystem1 = Subsystem1()
    subsystem2 = Subsystem2()
    facade = Facade(subsystem1, subsystem2)
    client_code(facade)
EN: Factory Method Design Pattern
Intent: Provides an interface for creating objects in a superclass, but allows
subclasses to alter the type of objects that will be created.
RU: Паттерн Фабричный Метод
Назначение: Определяет общий интерфейс для создания объектов в суперклассе,
позволяя подклассам изменять тип создаваемых объектов.
class Creator(ABC):
    EN: The Creator class declares the factory method that is supposed to return
    an object of a Product class. The Creator's subclasses usually provide the
    implementation of this method.
    RU: Класс Создатель объявляет фабричный метод, который должен возвращать
    объект класса Продукт. Подклассы Создателя обычно предоставляют реализацию
    этого метода.
    def factory_method(self):
        EN: Note that the Creator may also provide some default implementation
        of the factory method.
        RU: Обратите внимание, что Создатель может также обеспечить реализацию
        фабричного метода по умолчанию.
    def some_operation(self) -> str:
        EN: Also note that, despite its name, the Creator's primary
        responsibility is not creating products. Usually, it contains some core
        business logic that relies on Product objects, returned by the factory
        method. Subclasses can indirectly change that business logic by
        overriding the factory method and returning a different type of product
        from it.
        RU: Также заметьте, что, несмотря на название, основная обязанность
        Создателя не заключается в создании продуктов. Обычно он содержит
        некоторую базовую бизнес-логику, которая основана на объектах Продуктов,
        возвращаемых фабричным методом. Подклассы могут косвенно изменять эту
        бизнес-логику, переопределяя фабричный метод и возвращая из него другой
        тип продукта.
        # EN: Call the factory method to create a Product object.
        # RU: Вызываем фабричный метод, чтобы получить объект-продукт.
        product = self.factory_method()
        # EN: Now, use the product.
        # RU: Далее, работаем с этим продуктом.
        result = f"Creator: The same creator's code has just worked with {product.operation()}"
EN: Concrete Creators override the factory method in order to change the
resulting product's type.
RU: Конкретные Создатели переопределяют фабричный метод для того, чтобы изменить
тип результирующего продукта.
class ConcreteCreator1(Creator):
    EN: Note that the signature of the method still uses the abstract product
    type, even though the concrete product is actually returned from the method.
    This way the Creator can stay independent of concrete product classes.
    RU: Обратите внимание, что сигнатура метода по-прежнему использует тип
    абстрактного продукта, хотя фактически из метода возвращается конкретный
    продукт. Таким образом, Создатель может оставаться независимым от конкретных
    классов продуктов.
    def factory_method(self) -> Product:
        return ConcreteProduct1()
class ConcreteCreator2(Creator):
        return ConcreteProduct2()
class Product(ABC):
    EN: The Product interface declares the operations that all concrete products
    must implement.
    RU: Интерфейс Продукта объявляет операции, которые должны выполнять все
    конкретные продукты.
EN: Concrete Products provide various implementations of the Product interface.
RU: Конкретные Продукты предоставляют различные реализации интерфейса Продукта.
class ConcreteProduct1(Product):
        return "{Result of the ConcreteProduct1}"
class ConcreteProduct2(Product):
        return "{Result of the ConcreteProduct2}"
def client_code(creator: Creator) -> None:
    EN: The client code works with an instance of a concrete creator, albeit
    through its base interface. As long as the client keeps working with the
    creator via the base interface, you can pass it any creator's subclass.
    RU: Клиентский код работает с экземпляром конкретного создателя, хотя и
    через его базовый интерфейс. Пока клиент продолжает работать с создателем
    через базовый интерфейс, вы можете передать ему любой подкласс создателя.
    print(f"Client: I'm not aware of the creator's class, but it still works.\n"
          f"{creator.some_operation()}", end="")
    print("App: Launched with the ConcreteCreator1.")
    client_code(ConcreteCreator1())
    print("App: Launched with the ConcreteCreator2.")
    client_code(ConcreteCreator2())
EN: Flyweight Design Pattern
Intent: Lets you fit more objects into the available amount of RAM by sharing
common parts of state between multiple objects, instead of keeping all of the
data in each object.
RU: Паттерн Легковес
Назначение: Позволяет вместить бóльшее количество объектов в отведённую
оперативную память. Легковес экономит память, разделяя общее состояние объектов
между собой, вместо хранения одинаковых данных в каждом объекте.
class Flyweight():
    EN: The Flyweight stores a common portion of the state (also called
    intrinsic state) that belongs to multiple real business entities. The
    Flyweight accepts the rest of the state (extrinsic state, unique for each
    entity) via its method parameters.
    RU: Легковес хранит общую часть состояния (также называемую внутренним
    состоянием), которая принадлежит нескольким реальным бизнес-объектам.
    Легковес принимает оставшуюся часть состояния (внешнее состояние, уникальное
    для каждого объекта) через его параметры метода.
    def __init__(self, shared_state: str) -> None:
        self._shared_state = shared_state
    def operation(self, unique_state: str) -> None:
        s = json.dumps(self._shared_state)
        u = json.dumps(unique_state)
        print(f"Flyweight: Displaying shared ({s}) and unique ({u}) state.", end="")
class FlyweightFactory():
    EN: The Flyweight Factory creates and manages the Flyweight objects. It
    ensures that flyweights are shared correctly. When the client requests a
    flyweight, the factory either returns an existing instance or creates a new
    one, if it doesn't exist yet.
    RU: Фабрика Легковесов создает объекты-Легковесы и управляет ими. Она
    обеспечивает правильное разделение легковесов. Когда клиент запрашивает
    легковес, фабрика либо возвращает существующий экземпляр, либо создает
    новый, если он ещё не существует.
    _flyweights: Dict[str, Flyweight] = {}
    def __init__(self, initial_flyweights: Dict) -> None:
        for state in initial_flyweights:
            self._flyweights[self.get_key(state)] = Flyweight(state)
    def get_key(self, state: Dict) -> str:
        EN: Returns a Flyweight's string hash for a given state.
        RU: Возвращает хеш строки Легковеса для данного состояния.
        return "_".join(sorted(state))
    def get_flyweight(self, shared_state: Dict) -> Flyweight:
        EN: Returns an existing Flyweight with a given state or creates a new
        one.
        RU: Возвращает существующий Легковес с заданным состоянием или создает
        новый.
        key = self.get_key(shared_state)
        if not self._flyweights.get(key):
            print("FlyweightFactory: Can't find a flyweight, creating new one.")
            self._flyweights[key] = Flyweight(shared_state)
            print("FlyweightFactory: Reusing existing flyweight.")
        return self._flyweights[key]
    def list_flyweights(self) -> None:
        count = len(self._flyweights)
        print(f"FlyweightFactory: I have {count} flyweights:")
        print("\n".join(map(str, self._flyweights.keys())), end="")
def add_car_to_police_database(
    factory: FlyweightFactory, plates: str, owner: str,
    brand: str, model: str, color: str
    print("\n\nClient: Adding a car to database.")
    flyweight = factory.get_flyweight([brand, model, color])
    # EN: The client code either stores or calculates extrinsic state and passes
    # it to the flyweight's methods.
    # RU: Клиентский код либо сохраняет, либо вычисляет внешнее состояние и
    # передает его методам легковеса.
    flyweight.operation([plates, owner])
    EN: The client code usually creates a bunch of pre-populated flyweights in
    the initialization stage of the application.
    RU: Клиентский код обычно создает кучу предварительно заполненных легковесов
    на этапе инициализации приложения.
    factory = FlyweightFactory([
        ["Chevrolet", "Camaro2018", "pink"],
        ["Mercedes Benz", "C300", "black"],
        ["Mercedes Benz", "C500", "red"],
        ["BMW", "M5", "red"],
        ["BMW", "X6", "white"],
    factory.list_flyweights()
    add_car_to_police_database(
        factory, "CL234IR", "James Doe", "BMW", "M5", "red")
        factory, "CL234IR", "James Doe", "BMW", "X1", "red")
EN: Iterator Design Pattern
Intent: Lets you traverse elements of a collection without exposing its
underlying representation (list, stack, tree, etc.).
RU: Паттерн Итератор
Назначение: Даёт возможность последовательно обходить элементы составных
объектов, не раскрывая их внутреннего представления.
EN: To create an iterator in Python, there are two abstract classes from the
built-in `collections` module - Iterable,Iterator. We need to implement the
`__iter__()` method in the iterated object (collection), and the `__next__ ()`
method in theiterator.
RU: Для создания итератора в Python есть два абстрактных класса из встроенного
модуля collections - Iterable, Iterator. Нужно реализовать метод __iter__() в
итерируемом объекте (списке), а метод __next__() в итераторе.
class AlphabeticalOrderIterator(Iterator):
    EN: Concrete Iterators implement various traversal algorithms. These classes
    store the current traversal position at all times.
    RU: Конкретные Итераторы реализуют различные алгоритмы обхода. Эти классы
    постоянно хранят текущее положение обхода.
    EN: `_position` attribute stores the current traversal position. An iterator
    may have a lot of other fields for storing iteration state, especially when
    it is supposed to work with a particular kind of collection.
    RU: Атрибут _position хранит текущее положение обхода. У итератора может
    быть множество других полей для хранения состояния итерации, особенно когда
    он должен работать с определённым типом коллекции.
    _position: int = None
    EN: This attribute indicates the traversal direction.
    RU: Этот атрибут указывает направление обхода.
    _reverse: bool = False
    def __init__(self, collection: WordsCollection, reverse: bool = False) -> None:
        self._collection = collection
        self._reverse = reverse
        self._sorted_items = None  # Will be set on first __next__ call
        self._position = 0
    def __next__(self) -> Any:
        EN: Optimization: sorting happens only when the first items is actually
        RU: Оптимизация: сортировка происходит только тогда, когда первый элемент
        впервые запрашивается.
        if self._sorted_items is None:
            self._sorted_items = sorted(self._collection._collection)
            if self._reverse:
                self._sorted_items = list(reversed(self._sorted_items))
        EN: The __next__() method must return the next item in the sequence. On
        reaching the end, and in subsequent calls, it must raise StopIteration.
        RU: Метод __next __() должен вернуть следующий элемент в
        последовательности. При достижении конца коллекции и в последующих
        вызовах должно вызываться исключение StopIteration.
        if self._position >= len(self._sorted_items):
        value = self._sorted_items[self._position]
        self._position += 1
class WordsCollection(Iterable):
    EN: Concrete Collections provide one or several methods for retrieving fresh
    iterator instances, compatible with the collection class.
    RU: Конкретные Коллекции предоставляют один или несколько методов для
    получения новых экземпляров итератора, совместимых с классом коллекции.
    def __init__(self, collection: list[Any] | None = None) -> None:
        self._collection = collection or []
    def __getitem__(self, index: int) -> Any:
        return self._collection[index]
    def __iter__(self) -> AlphabeticalOrderIterator:
        EN: The __iter__() method returns the iterator object itself, by default
        we return the iterator in ascending order.
        RU: Метод __iter__() возвращает объект итератора, по умолчанию мы
        возвращаем итератор с сортировкой по возрастанию.
        return AlphabeticalOrderIterator(self)
    def get_reverse_iterator(self) -> AlphabeticalOrderIterator:
        return AlphabeticalOrderIterator(self, True)
    def add_item(self, item: Any) -> None:
        self._collection.append(item)
    # EN: The client code may or may not know about the Concrete Iterator or
    # Collection classes, depending on the level of indirection you want to keep
    # in your program.
    # RU: Клиентский код может знать или не знать о Конкретном Итераторе или
    # классах Коллекций, в зависимости от уровня косвенности, который вы хотите
    # сохранить в своей программе.
    collection = WordsCollection()
    collection.add_item("B")
    collection.add_item("A")
    collection.add_item("C")
    print("Straight traversal:")
    print("\n".join(collection))
    print("")
    print("Reverse traversal:")
    print("\n".join(collection.get_reverse_iterator()), end="")
EN: Mediator Design Pattern
Intent: Lets you reduce chaotic dependencies between objects. The pattern
restricts direct communications between the objects and forces them to
collaborate only via a mediator object.
RU: Паттерн Посредник
Назначение: Позволяет уменьшить связанность множества классов между собой,
благодаря перемещению этих связей в один класс-посредник.
class Mediator(ABC):
    EN: The Mediator interface declares a method used by components to notify
    the mediator about various events. The Mediator may react to these events
    and pass the execution to other components.
    RU: Интерфейс Посредника предоставляет метод, используемый компонентами для
    уведомления посредника о различных событиях. Посредник может реагировать на
    эти события и передавать исполнение другим компонентам.
    def notify(self, sender: object, event: str) -> None:
class ConcreteMediator(Mediator):
    def __init__(self, component1: Component1, component2: Component2) -> None:
        self._component1 = component1
        self._component1.mediator = self
        self._component2 = component2
        self._component2.mediator = self
        if event == "A":
            print("Mediator reacts on A and triggers following operations:")
            self._component2.do_c()
        elif event == "D":
            print("Mediator reacts on D and triggers following operations:")
            self._component1.do_b()
class BaseComponent:
    EN: The Base Component provides the basic functionality of storing a
    mediator's instance inside component objects.
    RU: Базовый Компонент обеспечивает базовую функциональность хранения
    экземпляра посредника внутри объектов компонентов.
    def __init__(self, mediator: Mediator = None) -> None:
        self._mediator = mediator
    def mediator(self) -> Mediator:
        return self._mediator
    @mediator.setter
    def mediator(self, mediator: Mediator) -> None:
EN: Concrete Components implement various functionality. They don't depend on
other components. They also don't depend on any concrete mediator classes.
RU: Конкретные Компоненты реализуют различную функциональность. Они не зависят
от других компонентов. Они также не зависят от каких-либо конкретных классов
посредников.
class Component1(BaseComponent):
    def do_a(self) -> None:
        print("Component 1 does A.")
        self.mediator.notify(self, "A")
    def do_b(self) -> None:
        print("Component 1 does B.")
        self.mediator.notify(self, "B")
class Component2(BaseComponent):
    def do_c(self) -> None:
        print("Component 2 does C.")
        self.mediator.notify(self, "C")
    def do_d(self) -> None:
        print("Component 2 does D.")
        self.mediator.notify(self, "D")
    # EN: The client code.
    # RU: Клиентский код.
    c1 = Component1()
    c2 = Component2()
    mediator = ConcreteMediator(c1, c2)
    print("Client triggers operation A.")
    c1.do_a()
    print("\n", end="")
    print("Client triggers operation D.")
    c2.do_d()
EN: Memento Design Pattern
Intent: Lets you save and restore the previous state of an object without
revealing the details of its implementation.
RU: Паттерн Снимок
Назначение: Фиксирует и восстанавливает внутреннее состояние объекта таким
образом, чтобы в дальнейшем объект можно было восстановить в этом состоянии без
нарушения инкапсуляции.
from random import sample
from string import ascii_letters
class Originator:
    EN: The Originator holds some important state that may change over time. It
    also defines a method for saving the state inside a memento and another
    method for restoring the state from it.
    RU: Создатель содержит некоторое важное состояние, которое может со временем
    меняться. Он также объявляет метод сохранения состояния внутри снимка и
    метод восстановления состояния из него.
    _state = None
    EN: For the sake of simplicity, the originator's state is stored inside a
    single variable.
    RU: Для удобства состояние создателя хранится внутри одной переменной.
    def __init__(self, state: str) -> None:
        self._state = state
        print(f"Originator: My initial state is: {self._state}")
    def do_something(self) -> None:
        EN: The Originator's business logic may affect its internal state.
        Therefore, the client should backup the state before launching methods
        of the business logic via the save() method.
        RU: Бизнес-логика Создателя может повлиять на его внутреннее состояние.
        Поэтому клиент должен выполнить резервное копирование состояния с
        помощью метода save перед запуском методов бизнес-логики.
        print("Originator: I'm doing something important.")
        self._state = self._generate_random_string(30)
        print(f"Originator: and my state has changed to: {self._state}")
    def _generate_random_string(length: int = 10) -> str:
        return "".join(sample(ascii_letters, length))
    def save(self) -> Memento:
        EN: Saves the current state inside a memento.
        RU: Сохраняет текущее состояние внутри снимка.
        return ConcreteMemento(self._state)
    def restore(self, memento: Memento) -> None:
        EN: Restores the Originator's state from a memento object.
        RU: Восстанавливает состояние Создателя из объекта снимка.
        self._state = memento.get_state()
        print(f"Originator: My state has changed to: {self._state}")
class Memento(ABC):
    EN: The Memento interface provides a way to retrieve the memento's metadata,
    such as creation date or name. However, it doesn't expose the Originator's
    RU: Интерфейс Снимка предоставляет способ извлечения метаданных снимка,
    таких как дата создания или название. Однако он не раскрывает состояние
    Создателя.
    def get_name(self) -> str:
    def get_date(self) -> str:
class ConcreteMemento(Memento):
        self._date = str(datetime.now())[:19]
    def get_state(self) -> str:
        EN: The Originator uses this method when restoring its state.
        RU: Создатель использует этот метод, когда восстанавливает своё
        состояние.
        return self._state
        EN: The rest of the methods are used by the Caretaker to display
        metadata.
        RU: Остальные методы используются Опекуном для отображения метаданных.
        return f"{self._date} / ({self._state[0:9]}...)"
        return self._date
class Caretaker:
    EN: The Caretaker doesn't depend on the Concrete Memento class. Therefore,
    it doesn't have access to the originator's state, stored inside the memento.
    It works with all mementos via the base Memento interface.
    RU: Опекун не зависит от класса Конкретного Снимка. Таким образом, он не
    имеет доступа к состоянию создателя, хранящемуся внутри снимка. Он работает
    со всеми снимками через базовый интерфейс Снимка.
    def __init__(self, originator: Originator) -> None:
        self._mementos = []
        self._originator = originator
    def backup(self) -> None:
        print("\nCaretaker: Saving Originator's state...")
        self._mementos.append(self._originator.save())
    def undo(self) -> None:
        if not len(self._mementos):
        memento = self._mementos.pop()
        print(f"Caretaker: Restoring state to: {memento.get_name()}")
            self._originator.restore(memento)
            self.undo()
    def show_history(self) -> None:
        print("Caretaker: Here's the list of mementos:")
        for memento in self._mementos:
            print(memento.get_name())
    originator = Originator("Super-duper-super-puper-super.")
    caretaker = Caretaker(originator)
    caretaker.backup()
    originator.do_something()
    caretaker.show_history()
    print("\nClient: Now, let's rollback!\n")
    caretaker.undo()
    print("\nClient: Once more!\n")
EN: Observer Design Pattern
Intent: Lets you define a subscription mechanism to notify multiple objects
about any events that happen to the object they're observing.
Note that there's a lot of different terms with similar meaning associated with
this pattern. Just remember that the Subject is also called the Publisher and
the Observer is often called the Subscriber and vice versa. Also the verbs
"observe", "listen" or "track" usually mean the same thing.
RU: Паттерн Наблюдатель
Назначение: Создаёт механизм подписки, позволяющий одним объектам следить и
реагировать на события, происходящие в других объектах.
Обратите внимание, что существует множество различных терминов с похожими
значениями, связанных с этим паттерном. Просто помните, что Субъекта также
называют Издателем, а Наблюдателя часто называют Подписчиком и наоборот. Также
глаголы «наблюдать», «слушать» или «отслеживать» обычно означают одно и то же.
from random import randrange
class Subject(ABC):
    EN: The Subject interface declares a set of methods for managing
    subscribers.
    RU: Интерфейс издателя объявляет набор методов для управлениями
    подписчиками.
    def attach(self, observer: Observer) -> None:
        EN: Attach an observer to the subject.
        RU: Присоединяет наблюдателя к издателю.
    def detach(self, observer: Observer) -> None:
        EN: Detach an observer from the subject.
        RU: Отсоединяет наблюдателя от издателя.
    def notify(self) -> None:
        EN: Notify all observers about an event.
        RU: Уведомляет всех наблюдателей о событии.
class ConcreteSubject(Subject):
    EN: The Subject owns some important state and notifies observers when the
    state changes.
    RU: Издатель владеет некоторым важным состоянием и оповещает наблюдателей о
    его изменениях.
    _state: int = None
    EN: For the sake of simplicity, the Subject's state, essential to all
    subscribers, is stored in this variable.
    RU: Для удобства в этой переменной хранится состояние Издателя, необходимое
    всем подписчикам.
    _observers: List[Observer] = []
    EN: List of subscribers. In real life, the list of subscribers can be stored
    more comprehensively (categorized by event type, etc.).
    RU: Список подписчиков. В реальной жизни список подписчиков может храниться
    в более подробном виде (классифицируется по типу события и т.д.)
        print("Subject: Attached an observer.")
        self._observers.append(observer)
        self._observers.remove(observer)
    EN: The subscription management methods.
    RU: Методы управления подпиской.
        EN: Trigger an update in each subscriber.
        RU: Запуск обновления в каждом подписчике.
        print("Subject: Notifying observers...")
        for observer in self._observers:
            observer.update(self)
    def some_business_logic(self) -> None:
        EN: Usually, the subscription logic is only a fraction of what a Subject
        can really do. Subjects commonly hold some important business logic,
        that triggers a notification method whenever something important is
        about to happen (or after it).
        RU: Обычно логика подписки – только часть того, что делает Издатель.
        Издатели часто содержат некоторую важную бизнес-логику, которая
        запускает метод уведомления всякий раз, когда должно произойти что-то
        важное (или после этого).
        print("\nSubject: I'm doing something important.")
        self._state = randrange(0, 10)
        print(f"Subject: My state has just changed to: {self._state}")
        self.notify()
class Observer(ABC):
    EN: The Observer interface declares the update method, used by subjects.
    RU: Интерфейс Наблюдателя объявляет метод уведомления, который издатели
    используют для оповещения своих подписчиков.
    def update(self, subject: Subject) -> None:
        EN: Receive update from subject.
        RU: Получить обновление от субъекта.
EN: Concrete Observers react to the updates issued by the Subject they had been
RU: Конкретные Наблюдатели реагируют на обновления, выпущенные Издателем, к
которому они прикреплены.
class ConcreteObserverA(Observer):
        if subject._state < 3:
            print("ConcreteObserverA: Reacted to the event")
class ConcreteObserverB(Observer):
        if subject._state == 0 or subject._state >= 2:
            print("ConcreteObserverB: Reacted to the event")
    subject = ConcreteSubject()
    observer_a = ConcreteObserverA()
    subject.attach(observer_a)
    observer_b = ConcreteObserverB()
    subject.attach(observer_b)
    subject.some_business_logic()
    subject.detach(observer_a)
class SelfReferencingEntity:
        self.parent = None
    def set_parent(self, parent):
        self.parent = parent
class SomeComponent:
    Python provides its own interface of Prototype via `copy.copy` and
    `copy.deepcopy` functions. And any class that wants to implement custom
    implementations have to override `__copy__` and `__deepcopy__` member
    functions.
    def __init__(self, some_int, some_list_of_objects, some_circular_ref):
        self.some_int = some_int
        self.some_list_of_objects = some_list_of_objects
        self.some_circular_ref = some_circular_ref
        Create a shallow copy. This method will be called whenever someone calls
        `copy.copy` with this object and the returned value is returned as the
        new shallow copy.
        # First, let's create copies of the nested objects.
        some_list_of_objects = copy.copy(self.some_list_of_objects)
        some_circular_ref = copy.copy(self.some_circular_ref)
        # Then, let's clone the object itself, using the prepared clones of the
        # nested objects.
        new = self.__class__(
            self.some_int, some_list_of_objects, some_circular_ref
        new.__dict__.update(self.__dict__)
    def __deepcopy__(self, memo=None):
        Create a deep copy. This method will be called whenever someone calls
        `copy.deepcopy` with this object and the returned value is returned as
        the new deep copy.
        What is the use of the argument `memo`?
        Memo is the dictionary that is used by the `deepcopy` library to prevent
        infinite recursive copies in instances of circular references. Pass it
        to all the `deepcopy` calls you make in the `__deepcopy__` implementation
        to prevent infinite recursions.
        if memo is None:
            memo = {}
        some_list_of_objects = copy.deepcopy(self.some_list_of_objects, memo)
        some_circular_ref = copy.deepcopy(self.some_circular_ref, memo)
        new.__dict__ = copy.deepcopy(self.__dict__, memo)
    list_of_objects = [1, {1, 2, 3}, [1, 2, 3]]
    circular_ref = SelfReferencingEntity()
    component = SomeComponent(23, list_of_objects, circular_ref)
    circular_ref.set_parent(component)
    shallow_copied_component = copy.copy(component)
    # Let's change the list in shallow_copied_component and see if it changes in
    # component.
    shallow_copied_component.some_list_of_objects.append("another object")
    if component.some_list_of_objects[-1] == "another object":
            "Adding elements to `shallow_copied_component`'s "
            "some_list_of_objects adds it to `component`'s "
            "some_list_of_objects."
            "some_list_of_objects doesn't add it to `component`'s "
    # Let's change the set in the list of objects.
    component.some_list_of_objects[1].add(4)
    if 4 in shallow_copied_component.some_list_of_objects[1]:
            "Changing objects in the `component`'s some_list_of_objects "
            "changes that object in `shallow_copied_component`'s "
            "doesn't change that object in `shallow_copied_component`'s "
    deep_copied_component = copy.deepcopy(component)
    # Let's change the list in deep_copied_component and see if it changes in
    deep_copied_component.some_list_of_objects.append("one more object")
    if component.some_list_of_objects[-1] == "one more object":
            "Adding elements to `deep_copied_component`'s "
    component.some_list_of_objects[1].add(10)
    if 10 in deep_copied_component.some_list_of_objects[1]:
            "changes that object in `deep_copied_component`'s "
            "doesn't change that object in `deep_copied_component`'s "
        f"id(deep_copied_component.some_circular_ref.parent): "
        f"{id(deep_copied_component.some_circular_ref.parent)}"
        f"id(deep_copied_component.some_circular_ref.parent.some_circular_ref.parent): "
        f"{id(deep_copied_component.some_circular_ref.parent.some_circular_ref.parent)}"
        "^^ This shows that deepcopied objects contain same reference, they "
        "are not cloned repeatedly."
EN: Proxy Design Pattern
Intent: Provide a surrogate or placeholder for another object to control access
to the original object or to add other responsibilities.
RU: Паттерн Заместитель
Назначение: Позволяет подставлять вместо реальных объектов специальные
объекты-заменители. Эти объекты перехватывают вызовы к оригинальному объекту,
позволяя сделать что-то до или после передачи вызова оригиналу.
    EN: The Subject interface declares common operations for both RealSubject
    and the Proxy. As long as the client works with RealSubject using this
    interface, you'll be able to pass it a proxy instead of a real subject.
    RU: Интерфейс Субъекта объявляет общие операции как для Реального Субъекта,
    так и для Заместителя. Пока клиент работает с Реальным Субъектом, используя
    этот интерфейс, вы сможете передать ему заместителя вместо реального
    субъекта.
    def request(self) -> None:
class RealSubject(Subject):
    EN: The RealSubject contains some core business logic. Usually, RealSubjects
    are capable of doing some useful work which may also be very slow or
    sensitive - e.g. correcting input data. A Proxy can solve these issues
    without any changes to the RealSubject's code.
    RU: Реальный Субъект содержит некоторую базовую бизнес-логику. Как правило,
    Реальные Субъекты способны выполнять некоторую полезную работу, которая к
    тому же может быть очень медленной или точной – например, коррекция входных
    данных. Заместитель может решить эти задачи без каких-либо изменений в коде
    Реального Субъекта.
        print("RealSubject: Handling request.")
class Proxy(Subject):
    EN: The Proxy has an interface identical to the RealSubject.
    RU: Интерфейс Заместителя идентичен интерфейсу Реального Субъекта.
    def __init__(self, real_subject: RealSubject) -> None:
        self._real_subject = real_subject
        EN: The most common applications of the Proxy pattern are lazy loading,
        caching, controlling the access, logging, etc. A Proxy can perform one
        of these things and then, depending on the result, pass the execution to
        the same method in a linked RealSubject object.
        RU: Наиболее распространёнными областями применения паттерна Заместитель
        являются ленивая загрузка, кэширование, контроль доступа, ведение
        журнала и т.д. Заместитель может выполнить одну из этих задач, а затем,
        в зависимости от результата, передать выполнение одноимённому методу в
        связанном объекте класса Реального Субъекта.
        if self.check_access():
            self._real_subject.request()
            self.log_access()
    def check_access(self) -> bool:
        print("Proxy: Checking access prior to firing a real request.")
    def log_access(self) -> None:
        print("Proxy: Logging the time of request.", end="")
def client_code(subject: Subject) -> None:
    EN: The client code is supposed to work with all objects (both subjects and
    proxies) via the Subject interface in order to support both real subjects
    and proxies. In real life, however, clients mostly work with their real
    subjects directly. In this case, to implement the pattern more easily, you
    can extend your proxy from the real subject's class.
    RU: Клиентский код должен работать со всеми объектами (как с реальными, так
    и заместителями) через интерфейс Субъекта, чтобы поддерживать как реальные
    субъекты, так и заместителей. В реальной жизни, однако, клиенты в основном
    работают с реальными субъектами напрямую. В этом случае, для более простой
    реализации паттерна, можно расширить заместителя из класса реального
    subject.request()
    print("Client: Executing the client code with a real subject:")
    real_subject = RealSubject()
    client_code(real_subject)
    print("Client: Executing the same client code with a proxy:")
    proxy = Proxy(real_subject)
    client_code(proxy)
EN: Singleton Design Pattern
Intent: Lets you ensure that a class has only one instance, while providing a
global access point to this instance. One instance per each subclass (if any).
RU: Паттерн Одиночка
Назначение: Гарантирует, что у класса есть только один экземпляр, и
предоставляет к нему глобальную точку доступа. У каждого наследника класса тоже
будет по одному экземпляру.
class SingletonMeta(type):
    EN: The Singleton class can be implemented in different ways in Python. Some
    possible methods include: base class, decorator, metaclass. We will use the
    metaclass because it is best suited for this purpose.
    RU: В Python класс Одиночка можно реализовать по-разному. Возможные
    способы включают себя базовый класс, декоратор, метакласс. Мы воспользуемся
    метаклассом, поскольку он лучше всего подходит для этой цели.
    _instances = {}
    def __call__(cls, *args, **kwargs):
        EN: Possible changes to the value of the `__init__` argument do not
        affect the returned instance.
        RU: Данная реализация не учитывает возможное изменение передаваемых
        аргументов в `__init__`.
        if cls not in cls._instances:
            instance = super().__call__(*args, **kwargs)
            cls._instances[cls] = instance
        return cls._instances[cls]
class Singleton(metaclass=SingletonMeta):
    def some_business_logic(self):
        EN: Finally, any singleton should define some business logic, which can
        be executed on its instance.
        RU: Наконец, любой одиночка должен содержать некоторую бизнес-логику,
        которая может быть выполнена на его экземпляре.
    s1 = Singleton()
    s2 = Singleton()
    if id(s1) == id(s2):
        print("Singleton works, both variables contain the same instance.")
        print("Singleton failed, variables contain different instances.")
from threading import Lock, Thread
    EN: This is a thread-safe implementation of Singleton.
    RU: Это потокобезопасная реализация класса Singleton.
    _lock: Lock = Lock()
    We now have a lock object that will be used to synchronize
    threads during first access to the Singleton.
    RU: У нас теперь есть объект-блокировка для синхронизации потоков во
    время первого доступа к Одиночке.
        # EN: Now, imagine that the program has just been launched.
        # Since there's no Singleton instance yet, multiple threads can
        # simultaneously pass the previous conditional and reach this
        # point almost at the same time. The first of them will acquire
        # lock and will proceed further, while the rest will wait here.
        # RU: Теперь представьте, что программа была только-только
        # запущена. Объекта-одиночки ещё никто не создавал, поэтому
        # несколько потоков вполне могли одновременно пройти через
        # предыдущее условие и достигнуть блокировки. Самый быстрый
        # поток поставит блокировку и двинется внутрь секции, пока
        # другие будут здесь его ожидать.
        with cls._lock:
            # EN: The first thread to acquire the lock, reaches this
            # conditional, goes inside and creates the Singleton
            # instance. Once it leaves the lock block, a thread that
            # might have been waiting for the lock release may then
            # enter this section. But since the Singleton field is
            # already initialized, the thread won't create a new
            # RU: Первый поток достигает этого условия и проходит внутрь,
            # создавая объект-одиночку. Как только этот поток покинет
            # секцию и освободит блокировку, следующий поток может
            # снова установить блокировку и зайти внутрь. Однако теперь
            # экземпляр одиночки уже будет создан и поток не сможет
            # пройти через это условие, а значит новый объект не будет
            # создан.
    EN: We'll use this property to prove that our Singleton really works.
    RU: Мы используем это поле, чтобы доказать, что наш Одиночка
    действительно работает.
    def __init__(self, value: str) -> None:
def test_singleton(value: str) -> None:
    singleton = Singleton(value)
    print(singleton.value)
    print("If you see the same value, then singleton was reused (yay!)\n"
          "If you see different values, "
          "then 2 singletons were created (booo!!)\n\n"
          "RESULT:\n")
    process1 = Thread(target=test_singleton, args=("FOO",))
    process2 = Thread(target=test_singleton, args=("BAR",))
    process1.start()
    process2.start()
EN: State Design Pattern
Intent: Lets an object alter its behavior when its internal state changes. It
appears as if the object changed its class.
RU: Паттерн Состояние
Назначение: Позволяет объектам менять поведение в зависимости от своего
состояния. Извне создаётся впечатление, что изменился класс объекта.
class Context:
    EN: The Context defines the interface of interest to clients. It also
    maintains a reference to an instance of a State subclass, which represents
    the current state of the Context.
    RU: Контекст определяет интерфейс, представляющий интерес для клиентов. Он
    также хранит ссылку на экземпляр подкласса Состояния, который отображает
    текущее состояние Контекста.
    EN: A reference to the current state of the Context.
    RU: Ссылка на текущее состояние Контекста.
    def __init__(self, state: State) -> None:
        self.transition_to(state)
    def transition_to(self, state: State):
        EN: The Context allows changing the State object at runtime.
        RU: Контекст позволяет изменять объект Состояния во время выполнения.
        print(f"Context: Transition to {type(state).__name__}")
        self._state.context = self
    EN: The Context delegates part of its behavior to the current State object.
    RU: Контекст делегирует часть своего поведения текущему объекту Состояния.
    def request1(self):
        self._state.handle1()
    def request2(self):
        self._state.handle2()
class State(ABC):
    EN: The base State class declares methods that all Concrete State should
    implement and also provides a backreference to the Context object,
    associated with the State. This backreference can be used by States to
    transition the Context to another State.
    RU: Базовый класс Состояния объявляет методы, которые должны реализовать все
    Конкретные Состояния, а также предоставляет обратную ссылку на объект
    Контекст, связанный с Состоянием. Эта обратная ссылка может использоваться
    Состояниями для передачи Контекста другому Состоянию.
    def context(self) -> Context:
        return self._context
    @context.setter
    def context(self, context: Context) -> None:
    def handle1(self) -> None:
    def handle2(self) -> None:
EN: Concrete States implement various behaviors, associated with a state of the
Context.
RU: Конкретные Состояния реализуют различные модели поведения, связанные с
состоянием Контекста.
class ConcreteStateA(State):
        print("ConcreteStateA handles request1.")
        print("ConcreteStateA wants to change the state of the context.")
        self.context.transition_to(ConcreteStateB())
        print("ConcreteStateA handles request2.")
class ConcreteStateB(State):
        print("ConcreteStateB handles request1.")
        print("ConcreteStateB handles request2.")
        print("ConcreteStateB wants to change the state of the context.")
        self.context.transition_to(ConcreteStateA())
    context = Context(ConcreteStateA())
    context.request1()
    context.request2()
EN: Strategy Design Pattern
Intent: Lets you define a family of algorithms, put each of them into a separate
class, and make their objects interchangeable.
RU: Паттерн Стратегия
Назначение: Определяет семейство схожих алгоритмов и помещает каждый из них в
собственный класс, после чего алгоритмы можно взаимозаменять прямо во время
исполнения программы.
class Context():
    EN: The Context defines the interface of interest to clients.
    RU: Контекст определяет интерфейс, представляющий интерес для клиентов.
    def __init__(self, strategy: Strategy) -> None:
        EN: Usually, the Context accepts a strategy through the constructor, but
        also provides a setter to change it at runtime.
        RU: Обычно Контекст принимает стратегию через конструктор, а также
        предоставляет сеттер для её изменения во время выполнения.
        self._strategy = strategy
    def strategy(self) -> Strategy:
        EN: The Context maintains a reference to one of the Strategy objects.
        The Context does not know the concrete class of a strategy. It should
        work with all strategies via the Strategy interface.
        RU: Контекст хранит ссылку на один из объектов Стратегии. Контекст не
        знает конкретного класса стратегии. Он должен работать со всеми
        стратегиями через интерфейс Стратегии.
        return self._strategy
    @strategy.setter
    def strategy(self, strategy: Strategy) -> None:
        EN: Usually, the Context allows replacing a Strategy object at runtime.
        RU: Обычно Контекст позволяет заменить объект Стратегии во время
        выполнения.
    def do_some_business_logic(self) -> None:
        EN: The Context delegates some work to the Strategy object instead of
        implementing multiple versions of the algorithm on its own.
        RU: Вместо того, чтобы самостоятельно реализовывать множественные версии
        алгоритма, Контекст делегирует некоторую работу объекту Стратегии.
        print("Context: Sorting data using the strategy (not sure how it'll do it)")
        result = self._strategy.do_algorithm(["a", "b", "c", "d", "e"])
        print(",".join(result))
class Strategy(ABC):
    EN: The Strategy interface declares operations common to all supported
    versions of some algorithm.
    The Context uses this interface to call the algorithm defined by Concrete
    Strategies.
    RU: Интерфейс Стратегии объявляет операции, общие для всех поддерживаемых
    версий некоторого алгоритма.
    Контекст использует этот интерфейс для вызова алгоритма, определённого
    Конкретными Стратегиями.
    def do_algorithm(self, data: List):
EN: Concrete Strategies implement the algorithm while following the base
Strategy interface. The interface makes them interchangeable in the Context.
RU: Конкретные Стратегии реализуют алгоритм, следуя базовому интерфейсу
Стратегии. Этот интерфейс делает их взаимозаменяемыми в Контексте.
class ConcreteStrategyA(Strategy):
    def do_algorithm(self, data: List) -> List:
        return sorted(data)
class ConcreteStrategyB(Strategy):
        return reversed(sorted(data))
    # EN: The client code picks a concrete strategy and passes it to the
    # context. The client should be aware of the differences between strategies
    # in order to make the right choice.
    # RU: Клиентский код выбирает конкретную стратегию и передаёт её в контекст.
    # Клиент должен знать о различиях между стратегиями, чтобы сделать
    # правильный выбор.
    context = Context(ConcreteStrategyA())
    print("Client: Strategy is set to normal sorting.")
    context.do_some_business_logic()
    print("Client: Strategy is set to reverse sorting.")
    context.strategy = ConcreteStrategyB()
EN: Template Method Design Pattern
Intent: Defines the skeleton of an algorithm in the superclass but lets
subclasses override specific steps of the algorithm without changing its
structure.
RU: Паттерн Шаблонный метод
Назначение: Определяет общую схему алгоритма, перекладывая реализацию некоторых
шагов на подклассы. Шаблонный метод позволяет подклассам переопределять
отдельные шаги алгоритма без изменения структуры алгоритма.
class AbstractClass(ABC):
    EN: The Abstract Class defines a template method that contains a skeleton of
    some algorithm, composed of calls to (usually) abstract primitive
    Concrete subclasses should implement these operations, but leave the
    template method itself intact.
    RU: Абстрактный Класс определяет шаблонный метод, содержащий скелет
    некоторого алгоритма, состоящего из вызовов (обычно) абстрактных примитивных
    операций.
    Конкретные подклассы должны реализовать эти операции, но оставить сам
    шаблонный метод без изменений.
    def template_method(self) -> None:
        EN: The template method defines the skeleton of an algorithm.
        RU: Шаблонный метод определяет скелет алгоритма.
        self.base_operation1()
        self.required_operations1()
        self.base_operation2()
        self.hook1()
        self.required_operations2()
        self.base_operation3()
        self.hook2()
    # EN: These operations already have implementations.
    # RU: Эти операции уже имеют реализации.
    def base_operation1(self) -> None:
        print("AbstractClass says: I am doing the bulk of the work")
    def base_operation2(self) -> None:
        print("AbstractClass says: But I let subclasses override some operations")
    def base_operation3(self) -> None:
        print("AbstractClass says: But I am doing the bulk of the work anyway")
    # EN: These operations have to be implemented in subclasses.
    # RU: А эти операции должны быть реализованы в подклассах.
    def required_operations1(self) -> None:
    def required_operations2(self) -> None:
    # EN: These are "hooks." Subclasses may override them, but it's not
    # mandatory since the hooks already have default (but empty) implementation.
    # Hooks provide additional extension points in some crucial places of the
    # RU: Это «хуки». Подклассы могут переопределять их, но это не обязательно,
    # поскольку у хуков уже есть стандартная (но пустая) реализация. Хуки
    # предоставляют дополнительные точки расширения в некоторых критических
    # местах алгоритма.
    def hook1(self) -> None:
    def hook2(self) -> None:
class ConcreteClass1(AbstractClass):
    EN: Concrete classes have to implement all abstract operations of the base
    class. They can also override some operations with a default implementation.
    RU: Конкретные классы должны реализовать все абстрактные операции базового
    класса. Они также могут переопределить некоторые операции с реализацией по
    умолчанию.
        print("ConcreteClass1 says: Implemented Operation1")
        print("ConcreteClass1 says: Implemented Operation2")
class ConcreteClass2(AbstractClass):
    EN: Usually, concrete classes override only a fraction of base class'
    RU: Обычно конкретные классы переопределяют только часть операций базового
    класса.
        print("ConcreteClass2 says: Implemented Operation1")
        print("ConcreteClass2 says: Implemented Operation2")
        print("ConcreteClass2 says: Overridden Hook1")
def client_code(abstract_class: AbstractClass) -> None:
    EN: The client code calls the template method to execute the algorithm.
    Client code does not have to know the concrete class of an object it works
    with, as long as it works with objects through the interface of their base
    RU: Клиентский код вызывает шаблонный метод для выполнения алгоритма.
    Клиентский код не должен знать конкретный класс объекта, с которым работает,
    при условии, что он работает с объектами через интерфейс их базового класса.
    abstract_class.template_method()
    print("Same client code can work with different subclasses:")
    client_code(ConcreteClass1())
    client_code(ConcreteClass2())
EN: Visitor Design Pattern
Intent: Lets you separate algorithms from the objects on which they operate.
RU: Паттерн Посетитель
Назначение: Позволяет создавать новые операции, не меняя классы объектов, над
которыми эти операции могут выполняться.
    EN: The Component interface declares an `accept` method that should take the
    base visitor interface as an argument.
    RU: Интерфейс Компонента объявляет метод accept, который в качестве
    аргумента может получать любой объект, реализующий интерфейс посетителя.
    def accept(self, visitor: Visitor) -> None:
class ConcreteComponentA(Component):
    EN: Each Concrete Component must implement the `accept` method in such a way
    that it calls the visitor's method corresponding to the component's class.
    RU: Каждый Конкретный Компонент должен реализовать метод accept таким
    образом, чтобы он вызывал метод посетителя, соответствующий классу
    компонента.
        EN: Note that we're calling `visitConcreteComponentA`, which matches the
        current class name. This way we let the visitor know the class of the
        component it works with.
        RU: Обратите внимание, мы вызываем visitConcreteComponentA, что
        соответствует названию текущего класса. Таким образом мы позволяем
        посетителю узнать, с каким классом компонента он работает.
        visitor.visit_concrete_component_a(self)
    def exclusive_method_of_concrete_component_a(self) -> str:
        EN: Concrete Components may have special methods that don't exist in
        their base class or interface. The Visitor is still able to use these
        methods since it's aware of the component's concrete class.
        RU: Конкретные Компоненты могут иметь особые методы, не объявленные в их
        базовом классе или интерфейсе. Посетитель всё же может использовать эти
        методы, поскольку он знает о конкретном классе компонента.
        return "A"
class ConcreteComponentB(Component):
    EN: Same here: visitConcreteComponentB => ConcreteComponentB
    RU: То же самое здесь: visitConcreteComponentB => ConcreteComponentB
    def accept(self, visitor: Visitor):
        visitor.visit_concrete_component_b(self)
    def special_method_of_concrete_component_b(self) -> str:
        return "B"
class Visitor(ABC):
    EN: The Visitor Interface declares a set of visiting methods that correspond
    to component classes. The signature of a visiting method allows the visitor
    to identify the exact class of the component that it's dealing with.
    RU: Интерфейс Посетителя объявляет набор методов посещения, соответствующих
    классам компонентов. Сигнатура метода посещения позволяет посетителю
    определить конкретный класс компонента, с которым он имеет дело.
    def visit_concrete_component_a(self, element: ConcreteComponentA) -> None:
    def visit_concrete_component_b(self, element: ConcreteComponentB) -> None:
EN: Concrete Visitors implement several versions of the same algorithm, which
can work with all concrete component classes.
You can experience the biggest benefit of the Visitor pattern when using it with
a complex object structure, such as a Composite tree. In this case, it might be
helpful to store some intermediate state of the algorithm while executing
visitor's methods over various objects of the structure.
RU: Конкретные Посетители реализуют несколько версий одного и того же алгоритма,
которые могут работать со всеми классами конкретных компонентов.
Максимальную выгоду от паттерна Посетитель вы почувствуете, используя его со
сложной структурой объектов, такой как дерево Компоновщика. В этом случае было
бы полезно хранить некоторое промежуточное состояние алгоритма при выполнении
методов посетителя над различными объектами структуры.
class ConcreteVisitor1(Visitor):
    def visit_concrete_component_a(self, element) -> None:
        print(f"{element.exclusive_method_of_concrete_component_a()} + ConcreteVisitor1")
    def visit_concrete_component_b(self, element) -> None:
        print(f"{element.special_method_of_concrete_component_b()} + ConcreteVisitor1")
class ConcreteVisitor2(Visitor):
        print(f"{element.exclusive_method_of_concrete_component_a()} + ConcreteVisitor2")
        print(f"{element.special_method_of_concrete_component_b()} + ConcreteVisitor2")
def client_code(components: List[Component], visitor: Visitor) -> None:
    EN: The client code can run visitor operations over any set of elements
    without figuring out their concrete classes. The accept operation directs a
    call to the appropriate operation in the visitor object.
    RU: Клиентский код может выполнять операции посетителя над любым набором
    элементов, не выясняя их конкретных классов. Операция принятия направляет
    вызов к соответствующей операции в объекте посетителя.
    for component in components:
        component.accept(visitor)
    components = [ConcreteComponentA(), ConcreteComponentB()]
    print("The client code works with all visitors via the base Visitor interface:")
    visitor1 = ConcreteVisitor1()
    client_code(components, visitor1)
    print("It allows the same client code to work with different types of visitors:")
    visitor2 = ConcreteVisitor2()
    client_code(components, visitor2)
"""Primary application entrypoint."""
# Do not import and use main() directly! Using it directly is actively
# discouraged by pip's maintainers. The name, location and behavior of
# this function is subject to change, so calling it directly is not
# portable across different pip versions.
# In addition, running pip in-process is unsupported and unsafe. This is
# elaborated in detail at
# https://pip.pypa.io/en/stable/user_guide/#using-pip-from-your-program.
# That document also provides suggestions that should work for nearly
# all users that are considering importing and using main() directly.
# However, we know that certain users will still want to invoke pip
# in-process. If you understand and accept the implications of using pip
# in an unsupported manner, the best approach is to use runpy to avoid
# depending on the exact location of this entry point.
# The following example shows how to use runpy to invoke pip in that
# case:
#     sys.argv = ["pip", your, args, here]
#     runpy.run_module("pip", run_name="__main__")
# Note that this will exit the process after running, unlike a direct
# call to main. As it is not safe to do any processing after calling
# main, this should not be an issue in practice.
    # NOTE: Lazy imports to speed up import of this module,
    # which is imported from the pip console script. This doesn't
    # speed up normal pip execution, but might be important in the future
    # if we use ``multiprocessing`` module,
    # which imports __main__ for each spawned subprocess.
    from pip._internal.cli.autocompletion import autocomplete
    from pip._internal.cli.main_parser import parse_command
    from pip._internal.commands import create_command
    from pip._internal.exceptions import PipError
    from pip._internal.utils import deprecation
    if args is None:
        args = sys.argv[1:]
    # Suppress the pkg_resources deprecation warning
    # Note - we use a module of .*pkg_resources to cover
    # the normal case (pip._vendor.pkg_resources) and the
    # devendored case (a bare pkg_resources)
        action="ignore", category=DeprecationWarning, module=".*pkg_resources"
    # Configure our deprecation warnings to be sent through loggers
    deprecation.install_warning_logger()
    autocomplete()
        cmd_name, cmd_args = parse_command(args)
    except PipError as exc:
        sys.stderr.write(f"ERROR: {exc}")
        sys.stderr.write(os.linesep)
    # Needed for locale.getpreferredencoding(False) to work
    # in pip._internal.utils.encoding.auto_decode
        locale.setlocale(locale.LC_ALL, "")
    except locale.Error as e:
        # setlocale can apparently crash if locale are uninitialized
        logger.debug("Ignoring error %s when setting locale", e)
    command = create_command(cmd_name, isolated=("--isolated" in cmd_args))
    return command.main(cmd_args)
from . import SUPPORTED_SHELLS, __version__, add_argument_to, complete
def get_main_parser():
    parser = argparse.ArgumentParser(prog="shtab")
    parser.add_argument("parser", help="importable parser (or function returning parser)")
    parser.add_argument("--version", action="version", version="%(prog)s " + __version__)
    parser.add_argument("-s", "--shell", default=SUPPORTED_SHELLS[0], choices=SUPPORTED_SHELLS)
    parser.add_argument("-o", "--output", default='-', help="output file (- for stdout)",
                        type=Path)
    parser.add_argument("--prefix", help="prepended to generated functions to avoid clashes")
    parser.add_argument("--preamble", help="prepended to generated script")
    parser.add_argument("--prog", help="custom program name (overrides `parser.prog`)")
        "--error-unimportable",
        help="raise errors if `parser` is not found in $PYTHONPATH",
    parser.add_argument("--verbose", dest="loglevel", action="store_const", default=logging.INFO,
                        const=logging.DEBUG, help="Log debug information")
    add_argument_to(parser, "--print-own-completion", help="print shtab's own completion")
    parser = get_main_parser()
    logging.basicConfig(level=args.loglevel)
    log.debug(args)
    module, other_parser = args.parser.rsplit(".", 1)
    if sys.path and sys.path[0]:
        # not blank so not searching curdir
        sys.path.insert(1, os.curdir)
        module = import_module(module)
        if args.error_unimportable:
        log.debug(str(err))
    other_parser = getattr(module, other_parser)
    if callable(other_parser):
        other_parser = other_parser()
    if args.prog:
        other_parser.prog = args.prog
    def _open(out_path):
        if str(out_path) in ("-", "stdout"):
            yield sys.stdout
            with out_path.open('w') as fd:
                yield fd
    with _open(args.output) as fd:
            complete(other_parser, shell=args.shell, root_prefix=args.prefix
                     or args.parser.split(".", 1)[0], preamble=args.preamble), file=fd)
from typing import IO, Dict, Iterable, Iterator, Mapping, Optional, Tuple, Union
from .parser import Binding, parse_stream
from .variables import parse_variables
# A type alias for a string path to be used for the paths in this file.
# These paths may flow to `open()` and `shutil.move()`; `shutil.move()`
# only accepts string paths, not byte paths or file descriptors. See
# https://github.com/python/typeshed/pull/6832.
StrPath = Union[str, "os.PathLike[str]"]
def _load_dotenv_disabled() -> bool:
    Determine if dotenv loading has been disabled.
    if "PYTHON_DOTENV_DISABLED" not in os.environ:
    value = os.environ["PYTHON_DOTENV_DISABLED"].casefold()
    return value in {"1", "true", "t", "yes", "y"}
def with_warn_for_invalid_lines(mappings: Iterator[Binding]) -> Iterator[Binding]:
    for mapping in mappings:
        if mapping.error:
                "python-dotenv could not parse statement starting at line %s",
                mapping.original.line,
        yield mapping
class DotEnv:
        dotenv_path: Optional[StrPath],
        stream: Optional[IO[str]] = None,
        encoding: Optional[str] = None,
        interpolate: bool = True,
        override: bool = True,
        self.dotenv_path: Optional[StrPath] = dotenv_path
        self.stream: Optional[IO[str]] = stream
        self._dict: Optional[Dict[str, Optional[str]]] = None
        self.verbose: bool = verbose
        self.encoding: Optional[str] = encoding
        self.interpolate: bool = interpolate
        self.override: bool = override
    def _get_stream(self) -> Iterator[IO[str]]:
        if self.dotenv_path and _is_file_or_fifo(self.dotenv_path):
            with open(self.dotenv_path, encoding=self.encoding) as stream:
                yield stream
        elif self.stream is not None:
            yield self.stream
                    "python-dotenv could not find configuration file %s.",
                    self.dotenv_path or ".env",
            yield io.StringIO("")
    def dict(self) -> Dict[str, Optional[str]]:
        """Return dotenv as dict"""
        if self._dict:
            return self._dict
        raw_values = self.parse()
        if self.interpolate:
            self._dict = OrderedDict(
                resolve_variables(raw_values, override=self.override)
            self._dict = OrderedDict(raw_values)
    def parse(self) -> Iterator[Tuple[str, Optional[str]]]:
        with self._get_stream() as stream:
            for mapping in with_warn_for_invalid_lines(parse_stream(stream)):
                if mapping.key is not None:
                    yield mapping.key, mapping.value
    def set_as_environment_variables(self) -> bool:
        Load the current dotenv as system environment variable.
        if not self.dict():
        for k, v in self.dict().items():
            if k in os.environ and not self.override:
                os.environ[k] = v
    def get(self, key: str) -> Optional[str]:
        """ """
        data = self.dict()
        if key in data:
            return data[key]
            logger.warning("Key %s not found in %s.", key, self.dotenv_path)
def get_key(
    dotenv_path: StrPath,
    key_to_get: str,
    encoding: Optional[str] = "utf-8",
    Get the value of a given key from the given .env.
    Returns `None` if the key isn't found or doesn't have a value.
    return DotEnv(dotenv_path, verbose=True, encoding=encoding).get(key_to_get)
def rewrite(
    path: StrPath,
    encoding: Optional[str],
) -> Iterator[Tuple[IO[str], IO[str]]]:
    pathlib.Path(path).touch()
    with tempfile.NamedTemporaryFile(mode="w", encoding=encoding, delete=False) as dest:
            with open(path, encoding=encoding) as source:
                yield (source, dest)
        except BaseException as err:
            error = err
        shutil.move(dest.name, path)
        os.unlink(dest.name)
        raise error from None
def set_key(
    key_to_set: str,
    value_to_set: str,
    quote_mode: str = "always",
    export: bool = False,
) -> Tuple[Optional[bool], str, str]:
    Adds or Updates a key/value to the given .env
    If the .env path given doesn't exist, fails instead of risking creating
    an orphan .env somewhere in the filesystem
    if quote_mode not in ("always", "auto", "never"):
        raise ValueError(f"Unknown quote_mode: {quote_mode}")
    quote = quote_mode == "always" or (
        quote_mode == "auto" and not value_to_set.isalnum()
        value_out = "'{}'".format(value_to_set.replace("'", "\\'"))
        value_out = value_to_set
    if export:
        line_out = f"export {key_to_set}={value_out}\n"
        line_out = f"{key_to_set}={value_out}\n"
    with rewrite(dotenv_path, encoding=encoding) as (source, dest):
        replaced = False
        missing_newline = False
        for mapping in with_warn_for_invalid_lines(parse_stream(source)):
            if mapping.key == key_to_set:
                dest.write(line_out)
                replaced = True
                dest.write(mapping.original.string)
                missing_newline = not mapping.original.string.endswith("\n")
        if not replaced:
            if missing_newline:
                dest.write("\n")
    return True, key_to_set, value_to_set
def unset_key(
    key_to_unset: str,
) -> Tuple[Optional[bool], str]:
    Removes a given key from the given `.env` file.
    If the .env path given doesn't exist, fails.
    If the given key doesn't exist in the .env, fails.
    if not os.path.exists(dotenv_path):
        logger.warning("Can't delete from %s - it doesn't exist.", dotenv_path)
        return None, key_to_unset
    removed = False
            if mapping.key == key_to_unset:
                removed = True
    if not removed:
            "Key %s not removed from %s - key doesn't exist.", key_to_unset, dotenv_path
    return removed, key_to_unset
def resolve_variables(
    values: Iterable[Tuple[str, Optional[str]]],
    override: bool,
) -> Mapping[str, Optional[str]]:
    new_values: Dict[str, Optional[str]] = {}
    for name, value in values:
            atoms = parse_variables(value)
            env: Dict[str, Optional[str]] = {}
                env.update(os.environ)  # type: ignore
                env.update(new_values)
            result = "".join(atom.resolve(env) for atom in atoms)
        new_values[name] = result
    return new_values
def _walk_to_root(path: str) -> Iterator[str]:
    Yield directories starting from the given directory up to the root
    if not os.path.exists(path):
        raise IOError("Starting path not found")
    if os.path.isfile(path):
        path = os.path.dirname(path)
    last_dir = None
    current_dir = os.path.abspath(path)
    while last_dir != current_dir:
        yield current_dir
        parent_dir = os.path.abspath(os.path.join(current_dir, os.path.pardir))
        last_dir, current_dir = current_dir, parent_dir
def find_dotenv(
    filename: str = ".env",
    raise_error_if_not_found: bool = False,
    usecwd: bool = False,
    Search in increasingly higher folders for the given file
    Returns path to the file if found, or an empty string otherwise
    def _is_interactive():
        """Decide whether this is running in a REPL or IPython notebook"""
        if hasattr(sys, "ps1") or hasattr(sys, "ps2"):
            main = __import__("__main__", None, None, fromlist=["__file__"])
        return not hasattr(main, "__file__")
    def _is_debugger():
        return sys.gettrace() is not None
    if usecwd or _is_interactive() or _is_debugger() or getattr(sys, "frozen", False):
        # Should work without __file__, e.g. in REPL or IPython notebook.
        path = os.getcwd()
        # will work for .py files
        frame = sys._getframe()
        current_file = __file__
        while frame.f_code.co_filename == current_file or not os.path.exists(
            frame.f_code.co_filename
            assert frame.f_back is not None
            frame = frame.f_back
        frame_filename = frame.f_code.co_filename
        path = os.path.dirname(os.path.abspath(frame_filename))
    for dirname in _walk_to_root(path):
        check_path = os.path.join(dirname, filename)
        if _is_file_or_fifo(check_path):
            return check_path
    if raise_error_if_not_found:
        raise IOError("File not found")
def load_dotenv(
    dotenv_path: Optional[StrPath] = None,
    override: bool = False,
    """Parse a .env file and then load all the variables found as environment variables.
        dotenv_path: Absolute or relative path to .env file.
        stream: Text stream (such as `io.StringIO`) with .env content, used if
            `dotenv_path` is `None`.
        verbose: Whether to output a warning the .env file is missing.
        override: Whether to override the system environment variables with the variables
            from the `.env` file.
        encoding: Encoding to be used to read the file.
        Bool: True if at least one environment variable is set else False
    If both `dotenv_path` and `stream` are `None`, `find_dotenv()` is used to find the
    .env file with it's default parameters. If you need to change the default parameters
    of `find_dotenv()`, you can explicitly call `find_dotenv()` and pass the result
    to this function as `dotenv_path`.
    If the environment variable `PYTHON_DOTENV_DISABLED` is set to a truthy value,
    .env loading is disabled.
    if _load_dotenv_disabled():
            "python-dotenv: .env loading disabled by PYTHON_DOTENV_DISABLED environment variable"
    if dotenv_path is None and stream is None:
        dotenv_path = find_dotenv()
    dotenv = DotEnv(
        dotenv_path=dotenv_path,
        interpolate=interpolate,
        override=override,
    return dotenv.set_as_environment_variables()
def dotenv_values(
) -> Dict[str, Optional[str]]:
    Parse a .env file and return its content as a dict.
    The returned dict will have `None` values for keys without values in the .env file.
    For example, `foo=bar` results in `{"foo": "bar"}` whereas `foo` alone results in
    `{"foo": None}`
        dotenv_path: Absolute or relative path to the .env file.
        stream: `StringIO` object with .env content, used if `dotenv_path` is `None`.
        verbose: Whether to output a warning if the .env file is missing.
    .env file.
    return DotEnv(
        override=True,
    ).dict()
def _is_file_or_fifo(path: StrPath) -> bool:
    Return True if `path` exists and is either a regular file or a FIFO.
        st = os.stat(path)
    except (FileNotFoundError, OSError):
    return stat.S_ISFIFO(st.st_mode)
class Doc:
    """Define the documentation of a type annotation using `Annotated`, to be
        used in class attributes, function and method parameters, return values,
        and variables.
    The value should be a positional-only string literal to allow static tools
    like editors and documentation generators to use it.
    This complements docstrings.
    The string value passed is available in the attribute `documentation`.
    ```Python
    def hi(name: Annotated[str, Doc("Who to say hi to")]) -> None:
        print(f"Hi, {name}!")
    def __init__(self, documentation: str, /) -> None:
        self.documentation = documentation
        return f"Doc({self.documentation!r})"
        return hash(self.documentation)
        if not isinstance(other, Doc):
        return self.documentation == other.documentation
from scipy.optimize import (
    Bounds,
    NonlinearConstraint,
    OptimizeResult,
from .framework import TrustRegion
from .problem import (
    ObjectiveFunction,
    BoundConstraints,
    LinearConstraints,
    NonlinearConstraints,
    Problem,
    MaxEvalError,
    TargetSuccess,
    CallbackSuccess,
    FeasibleSuccess,
    exact_1d_array,
from .settings import (
    ExitStatus,
    Constants,
    DEFAULT_OPTIONS,
    DEFAULT_CONSTANTS,
    PRINT_OPTIONS,
def minimize(
    fun,
    args=(),
    bounds=None,
    constraints=(),
    callback=None,
    Minimize a scalar function using the COBYQA method.
    The Constrained Optimization BY Quadratic Approximations (COBYQA) method is
    a derivative-free optimization method designed to solve general nonlinear
    optimization problems. A complete description of COBYQA is given in [3]_.
    fun : {callable, None}
        Objective function to be minimized.
            ``fun(x, *args) -> float``
        where ``x`` is an array with shape (n,) and `args` is a tuple. If `fun`
        is ``None``, the objective function is assumed to be the zero function,
        resulting in a feasibility problem.
    x0 : array_like, shape (n,)
        Initial guess.
    args : tuple, optional
        Extra arguments passed to the objective function.
    bounds : {`scipy.optimize.Bounds`, array_like, shape (n, 2)}, optional
        Bound constraints of the problem. It can be one of the cases below.
        #. An instance of `scipy.optimize.Bounds`. For the time being, the
           argument ``keep_feasible`` is disregarded, and all the constraints
           are considered unrelaxable and will be enforced.
        #. An array with shape (n, 2). The bound constraints for ``x[i]`` are
           ``bounds[i][0] <= x[i] <= bounds[i][1]``. Set ``bounds[i][0]`` to
           :math:`-\infty` if there is no lower bound, and set ``bounds[i][1]``
           to :math:`\infty` if there is no upper bound.
        The COBYQA method always respect the bound constraints.
    constraints : {Constraint, list}, optional
        General constraints of the problem. It can be one of the cases below.
        #. An instance of `scipy.optimize.LinearConstraint`. The argument
           ``keep_feasible`` is disregarded.
        #. An instance of `scipy.optimize.NonlinearConstraint`. The arguments
           ``jac``, ``hess``, ``keep_feasible``, ``finite_diff_rel_step``, and
           ``finite_diff_jac_sparsity`` are disregarded.
        #. A list, each of whose elements are described in the cases above.
    callback : callable, optional
        A callback executed at each objective function evaluation. The method
        terminates if a ``StopIteration`` exception is raised by the callback
        function. Its signature can be one of the following:
            ``callback(intermediate_result)``
        where ``intermediate_result`` is a keyword parameter that contains an
        instance of `scipy.optimize.OptimizeResult`, with attributes ``x``
        and ``fun``, being the point at which the objective function is
        evaluated and the value of the objective function, respectively. The
        name of the parameter must be ``intermediate_result`` for the callback
        to be passed an instance of `scipy.optimize.OptimizeResult`.
        Alternatively, the callback function can have the signature:
            ``callback(xk)``
        where ``xk`` is the point at which the objective function is evaluated.
        Introspection is used to determine which of the signatures to invoke.
    options : dict, optional
        Options passed to the solver. Accepted keys are:
            disp : bool, optional
                Whether to print information about the optimization procedure.
                Default is ``False``.
            maxfev : int, optional
                Maximum number of function evaluations. Default is ``500 * n``.
            maxiter : int, optional
                Maximum number of iterations. Default is ``1000 * n``.
            target : float, optional
                Target on the objective function value. The optimization
                procedure is terminated when the objective function value of a
                feasible point is less than or equal to this target. Default is
                ``-numpy.inf``.
            feasibility_tol : float, optional
                Tolerance on the constraint violation. If the maximum
                constraint violation at a point is less than or equal to this
                tolerance, the point is considered feasible. Default is
                ``numpy.sqrt(numpy.finfo(float).eps)``.
            radius_init : float, optional
                Initial trust-region radius. Typically, this value should be in
                the order of one tenth of the greatest expected change to `x0`.
                Default is ``1.0``.
            radius_final : float, optional
                Final trust-region radius. It should indicate the accuracy
                required in the final values of the variables. Default is
                ``1e-6``.
            nb_points : int, optional
                Number of interpolation points used to build the quadratic
                models of the objective and constraint functions. Default is
                ``2 * n + 1``.
            scale : bool, optional
                Whether to scale the variables according to the bounds. Default
            filter_size : int, optional
                Maximum number of points in the filter. The filter is used to
                select the best point returned by the optimization procedure.
                Default is ``sys.maxsize``.
            store_history : bool, optional
                Whether to store the history of the function evaluations.
            history_size : int, optional
                Maximum number of function evaluations to store in the history.
            debug : bool, optional
                Whether to perform additional checks during the optimization
                procedure. This option should be used only for debugging
                purposes and is highly discouraged to general users. Default is
        Other constants (from the keyword arguments) are described below. They
        are not intended to be changed by general users. They should only be
        changed by users with a deep understanding of the algorithm, who want
        to experiment with different settings.
    `scipy.optimize.OptimizeResult`
        Result of the optimization procedure, with the following fields:
            message : str
                Description of the cause of the termination.
            success : bool
                Whether the optimization procedure terminated successfully.
            status : int
                Termination status of the optimization procedure.
            x : `numpy.ndarray`, shape (n,)
                Solution point.
            fun : float
                Objective function value at the solution point.
            maxcv : float
                Maximum constraint violation at the solution point.
                Number of function evaluations.
            nit : int
                Number of iterations.
        If ``store_history`` is True, the result also has the following fields:
            fun_history : `numpy.ndarray`, shape (nfev,)
                History of the objective function values.
            maxcv_history : `numpy.ndarray`, shape (nfev,)
                History of the maximum constraint violations.
        A description of the termination statuses is given below.
        .. list-table::
            :widths: 25 75
            :header-rows: 1
            * - Exit status
              - Description
            * - 0
              - The lower bound for the trust-region radius has been reached.
            * - 1
              - The target objective function value has been reached.
            * - 2
              - All variables are fixed by the bound constraints.
            * - 3
              - The callback requested to stop the optimization procedure.
            * - 4
              - The feasibility problem received has been solved successfully.
            * - 5
              - The maximum number of function evaluations has been exceeded.
            * - 6
              - The maximum number of iterations has been exceeded.
            * - -1
              - The bound constraints are infeasible.
            * - -2
              - A linear algebra error occurred.
    Other Parameters
    decrease_radius_factor : float, optional
        Factor by which the trust-region radius is reduced when the reduction
        ratio is low or negative. Default is ``0.5``.
    increase_radius_factor : float, optional
        Factor by which the trust-region radius is increased when the reduction
        ratio is large. Default is ``numpy.sqrt(2.0)``.
    increase_radius_threshold : float, optional
        Threshold that controls the increase of the trust-region radius when
        the reduction ratio is large. Default is ``2.0``.
    decrease_radius_threshold : float, optional
        Threshold used to determine whether the trust-region radius should be
        reduced to the resolution. Default is ``1.4``.
    decrease_resolution_factor : float, optional
        Factor by which the resolution is reduced when the current value is far
        from its final value. Default is ``0.1``.
    large_resolution_threshold : float, optional
        Threshold used to determine whether the resolution is far from its
        final value. Default is ``250.0``.
    moderate_resolution_threshold : float, optional
        Threshold used to determine whether the resolution is close to its
        final value. Default is ``16.0``.
    low_ratio : float, optional
        Threshold used to determine whether the reduction ratio is low. Default
        is ``0.1``.
    high_ratio : float, optional
        Threshold used to determine whether the reduction ratio is high.
        Default is ``0.7``.
    very_low_ratio : float, optional
        Threshold used to determine whether the reduction ratio is very low.
        This is used to determine whether the models should be reset. Default
        is ``0.01``.
    penalty_increase_threshold : float, optional
        Threshold used to determine whether the penalty parameter should be
        increased. Default is ``1.5``.
    penalty_increase_factor : float, optional
        Factor by which the penalty parameter is increased. Default is ``2.0``.
    short_step_threshold : float, optional
        Factor used to determine whether the trial step is too short. Default
        is ``0.5``.
    low_radius_factor : float, optional
        Factor used to determine which interpolation point should be removed
        from the interpolation set at each iteration. Default is ``0.1``.
    byrd_omojokun_factor : float, optional
        Factor by which the trust-region radius is reduced for the computations
        of the normal step in the Byrd-Omojokun composite-step approach.
        Default is ``0.8``.
    threshold_ratio_constraints : float, optional
        Threshold used to determine which constraints should be taken into
        account when decreasing the penalty parameter. Default is ``2.0``.
    large_shift_factor : float, optional
        Factor used to determine whether the point around which the quadratic
        models are built should be updated. Default is ``10.0``.
    large_gradient_factor : float, optional
        Factor used to determine whether the models should be reset. Default is
        ``10.0``.
    resolution_factor : float, optional
        Factor by which the resolution is decreased. Default is ``2.0``.
    improve_tcg : bool, optional
        Whether to improve the steps computed by the truncated conjugate
        gradient method when the trust-region boundary is reached. Default is
        ``True``.
    .. [1] J. Nocedal and S. J. Wright. *Numerical Optimization*. Springer Ser.
       Oper. Res. Financ. Eng. Springer, New York, NY, USA, second edition,
       2006. `doi:10.1007/978-0-387-40065-5
       <https://doi.org/10.1007/978-0-387-40065-5>`_.
    .. [2] M. J. D. Powell. A direct search optimization method that models the
       objective and constraint functions by linear interpolation. In S. Gomez
       and J.-P. Hennart, editors, *Advances in Optimization and Numerical
       Analysis*, volume 275 of Math. Appl., pages 51--67. Springer, Dordrecht,
       Netherlands, 1994. `doi:10.1007/978-94-015-8330-5_4
       <https://doi.org/10.1007/978-94-015-8330-5_4>`_.
    .. [3] T. M. Ragonneau. *Model-Based Derivative-Free Optimization Methods
       and Software*. PhD thesis, Department of Applied Mathematics, The Hong
       Kong Polytechnic University, Hong Kong, China, 2022. URL:
       https://theses.lib.polyu.edu.hk/handle/200/12294.
    To demonstrate how to use `minimize`, we first minimize the Rosenbrock
    function implemented in `scipy.optimize` in an unconstrained setting.
        np.set_printoptions(precision=3, suppress=True)
    >>> from cobyqa import minimize
    >>> from scipy.optimize import rosen
    To solve the problem using COBYQA, run:
    >>> x0 = [1.3, 0.7, 0.8, 1.9, 1.2]
    >>> res = minimize(rosen, x0)
    >>> res.x
    array([1., 1., 1., 1., 1.])
    To see how bound and constraints are handled using `minimize`, we solve
    Example 16.4 of [1]_, defined as
        \begin{aligned}
            \min_{x \in \mathbb{R}^2}   & \quad (x_1 - 1)^2 + (x_2 - 2.5)^2\\
            \text{s.t.}                 & \quad -x_1 + 2x_2 \le 2,\\
                                        & \quad x_1 + 2x_2 \le 6,\\
                                        & \quad x_1 - 2x_2 \le 2,\\
                                        & \quad x_1 \ge 0,\\
                                        & \quad x_2 \ge 0.
        \end{aligned}
    >>> from scipy.optimize import Bounds, LinearConstraint
    Its objective function can be implemented as:
    >>> def fun(x):
    ...     return (x[0] - 1.0)**2 + (x[1] - 2.5)**2
    This problem can be solved using `minimize` as:
    >>> x0 = [2.0, 0.0]
    >>> bounds = Bounds([0.0, 0.0], np.inf)
    >>> constraints = LinearConstraint([
    ...     [-1.0, 2.0],
    ...     [1.0, 2.0],
    ...     [1.0, -2.0],
    ... ], -np.inf, [2.0, 6.0, 2.0])
    >>> res = minimize(fun, x0, bounds=bounds, constraints=constraints)
    array([1.4, 1.7])
    To see how nonlinear constraints are handled, we solve Problem (F) of [2]_,
    defined as
            \min_{x \in \mathbb{R}^2}   & \quad -x_1 - x_2\\
            \text{s.t.}                 & \quad x_1^2 - x_2 \le 0,\\
                                        & \quad x_1^2 + x_2^2 \le 1.
    >>> from scipy.optimize import NonlinearConstraint
    Its objective and constraint functions can be implemented as:
    ...     return -x[0] - x[1]
    >>> def cub(x):
    ...     return [x[0]**2 - x[1], x[0]**2 + x[1]**2]
    >>> x0 = [1.0, 1.0]
    >>> constraints = NonlinearConstraint(cub, -np.inf, [0.0, 1.0])
    >>> res = minimize(fun, x0, constraints=constraints)
    array([0.707, 0.707])
    Finally, to see how to supply linear and nonlinear constraints
    simultaneously, we solve Problem (G) of [2]_, defined as
            \min_{x \in \mathbb{R}^3}   & \quad x_3\\
            \text{s.t.}                 & \quad 5x_1 - x_2 + x_3 \ge 0,\\
                                        & \quad -5x_1 - x_2 + x_3 \ge 0,\\
                                        & \quad x_1^2 + x_2^2 + 4x_2 \le x_3.
    Its objective and nonlinear constraint functions can be implemented as:
    ...     return x[2]
    ...     return x[0]**2 + x[1]**2 + 4.0*x[1] - x[2]
    >>> x0 = [1.0, 1.0, 1.0]
    >>> constraints = [
    ...     LinearConstraint(
    ...         [[5.0, -1.0, 1.0], [-5.0, -1.0, 1.0]],
    ...         [0.0, 0.0],
    ...         np.inf,
    ...     ),
    ...     NonlinearConstraint(cub, -np.inf, 0.0),
    array([ 0., -3., -3.])
    # Get basic options that are needed for the initialization.
        options = dict(options)
    verbose = options.get(Options.VERBOSE, DEFAULT_OPTIONS[Options.VERBOSE])
    verbose = bool(verbose)
    feasibility_tol = options.get(
        Options.FEASIBILITY_TOL,
        DEFAULT_OPTIONS[Options.FEASIBILITY_TOL],
    feasibility_tol = float(feasibility_tol)
    scale = options.get(Options.SCALE, DEFAULT_OPTIONS[Options.SCALE])
    scale = bool(scale)
    store_history = options.get(
        Options.STORE_HISTORY,
        DEFAULT_OPTIONS[Options.STORE_HISTORY],
    store_history = bool(store_history)
    if Options.HISTORY_SIZE in options and options[Options.HISTORY_SIZE] <= 0:
        raise ValueError("The size of the history must be positive.")
    history_size = options.get(
        Options.HISTORY_SIZE,
        DEFAULT_OPTIONS[Options.HISTORY_SIZE],
    history_size = int(history_size)
    if Options.FILTER_SIZE in options and options[Options.FILTER_SIZE] <= 0:
        raise ValueError("The size of the filter must be positive.")
    filter_size = options.get(
        Options.FILTER_SIZE,
        DEFAULT_OPTIONS[Options.FILTER_SIZE],
    filter_size = int(filter_size)
    debug = options.get(Options.DEBUG, DEFAULT_OPTIONS[Options.DEBUG])
    debug = bool(debug)
    # Initialize the objective function.
    if not isinstance(args, tuple):
    obj = ObjectiveFunction(fun, verbose, debug, *args)
    # Initialize the bound constraints.
    if not hasattr(x0, "__len__"):
        x0 = [x0]
    n_orig = len(x0)
    bounds = BoundConstraints(_get_bounds(bounds, n_orig))
    # Initialize the constraints.
    linear_constraints, nonlinear_constraints = _get_constraints(constraints)
    linear = LinearConstraints(linear_constraints, n_orig, debug)
    nonlinear = NonlinearConstraints(nonlinear_constraints, verbose, debug)
    # Initialize the problem (and remove the fixed variables).
    pb = Problem(
        bounds,
        linear,
        nonlinear,
        callback,
        feasibility_tol,
        scale,
        store_history,
        history_size,
        filter_size,
    _set_default_options(options, pb.n)
    constants = _set_default_constants(**kwargs)
    # Initialize the models and skip the computations whenever possible.
    if not pb.bounds.is_feasible:
        # The bound constraints are infeasible.
        return _build_result(
            pb,
            ExitStatus.INFEASIBLE_ERROR,
    elif pb.n == 0:
        # All variables are fixed by the bound constraints.
            ExitStatus.FIXED_SUCCESS,
        print("Starting the optimization procedure.")
        print(f"Initial trust-region radius: {options[Options.RHOBEG]}.")
        print(f"Final trust-region radius: {options[Options.RHOEND]}.")
            f"Maximum number of function evaluations: "
            f"{options[Options.MAX_EVAL]}."
        print(f"Maximum number of iterations: {options[Options.MAX_ITER]}.")
        framework = TrustRegion(pb, options, constants)
    except TargetSuccess:
        # The target on the objective function value has been reached
            ExitStatus.TARGET_SUCCESS,
    except CallbackSuccess:
        # The callback raised a StopIteration exception.
            ExitStatus.CALLBACK_SUCCESS,
    except FeasibleSuccess:
        # The feasibility problem has been solved successfully.
            ExitStatus.FEASIBLE_SUCCESS,
    except MaxEvalError:
        # The maximum number of function evaluations has been exceeded.
            ExitStatus.MAX_ITER_WARNING,
    except np.linalg.LinAlgError:
        # The construction of the initial interpolation set failed.
            ExitStatus.LINALG_ERROR,
    # Start the optimization procedure.
    n_iter = 0
    k_new = None
    n_short_steps = 0
    n_very_short_steps = 0
    n_alt_models = 0
        # Stop the optimization procedure if the maximum number of iterations
        # has been exceeded. We do not write the main loop as a for loop
        # because we want to access the number of iterations outside the loop.
        if n_iter >= options[Options.MAX_ITER]:
            status = ExitStatus.MAX_ITER_WARNING
        n_iter += 1
        # Update the point around which the quadratic models are built.
            np.linalg.norm(
                framework.x_best - framework.models.interpolation.x_base
            >= constants[Constants.LARGE_SHIFT_FACTOR] * framework.radius
            framework.shift_x_base(options)
        # Evaluate the trial step.
        radius_save = framework.radius
        normal_step, tangential_step = framework.get_trust_region_step(options)
        step = normal_step + tangential_step
        s_norm = np.linalg.norm(step)
        # If the trial step is too short, we do not attempt to evaluate the
        # objective and constraint functions. Instead, we reduce the
        # trust-region radius and check whether the resolution should be
        # enhanced and whether the geometry of the interpolation set should be
        # improved. Otherwise, we entertain a classical iteration. The
        # criterion for performing an exceptional jump is taken from NEWUOA.
            s_norm
            <= constants[Constants.SHORT_STEP_THRESHOLD] * framework.resolution
            framework.radius *= constants[Constants.DECREASE_RESOLUTION_FACTOR]
            if radius_save > framework.resolution:
                n_short_steps += 1
                n_very_short_steps += 1
                if s_norm > 0.1 * framework.resolution:
            enhance_resolution = n_short_steps >= 5 or n_very_short_steps >= 3
            if enhance_resolution:
                improve_geometry = False
                    k_new, dist_new = framework.get_index_to_remove()
                    status = ExitStatus.LINALG_ERROR
                improve_geometry = dist_new > max(
                    framework.radius,
                    constants[Constants.RESOLUTION_FACTOR]
                    * framework.resolution,
            # Increase the penalty parameter if necessary.
            same_best_point = framework.increase_penalty(step)
            if same_best_point:
                # Evaluate the objective and constraint functions.
                    fun_val, cub_val, ceq_val = _eval(
                        framework,
                    status = ExitStatus.TARGET_SUCCESS
                    success = True
                    status = ExitStatus.FEASIBLE_SUCCESS
                    status = ExitStatus.CALLBACK_SUCCESS
                    status = ExitStatus.MAX_EVAL_WARNING
                # Perform a second-order correction step if necessary.
                merit_old = framework.merit(
                    framework.x_best,
                    framework.fun_best,
                    framework.cub_best,
                    framework.ceq_best,
                merit_new = framework.merit(
                    framework.x_best + step, fun_val, cub_val, ceq_val
                    pb.type == "nonlinearly constrained"
                    and merit_new > merit_old
                    and np.linalg.norm(normal_step)
                    > constants[Constants.BYRD_OMOJOKUN_FACTOR] ** 2.0
                    * framework.radius
                    soc_step = framework.get_second_order_correction_step(
                        step, options
                    if np.linalg.norm(soc_step) > 0.0:
                        step += soc_step
                # Calculate the reduction ratio.
                ratio = framework.get_reduction_ratio(
                    fun_val,
                    cub_val,
                    ceq_val,
                # Choose an interpolation point to remove.
                    k_new = framework.get_index_to_remove(
                        framework.x_best + step
                # Update the interpolation set.
                    ill_conditioned = framework.models.update_interpolation(
                        k_new, framework.x_best + step, fun_val, cub_val,
                        ceq_val
                framework.set_best_index()
                # Update the trust-region radius.
                framework.update_radius(step, ratio)
                # Attempt to replace the models by the alternative ones.
                if framework.radius <= framework.resolution:
                    if ratio >= constants[Constants.VERY_LOW_RATIO]:
                        n_alt_models += 1
                        grad = framework.models.fun_grad(framework.x_best)
                            grad_alt = framework.models.fun_alt_grad(
                                framework.x_best
                        if np.linalg.norm(grad) < constants[
                            Constants.LARGE_GRADIENT_FACTOR
                        ] * np.linalg.norm(grad_alt):
                        if n_alt_models >= 3:
                                framework.models.reset_models()
                # Update the Lagrange multipliers.
                framework.set_multipliers(framework.x_best + step)
                # Check whether the resolution should be enhanced.
                improve_geometry = (
                    ill_conditioned
                    or ratio <= constants[Constants.LOW_RATIO]
                    and dist_new
                    > max(
                enhance_resolution = (
                    radius_save <= framework.resolution
                    and ratio <= constants[Constants.LOW_RATIO]
                    and not improve_geometry
                # When increasing the penalty parameter, the best point so far
                # may change. In this case, we restart the iteration.
                enhance_resolution = False
        # Reduce the resolution if necessary.
            if framework.resolution <= options[Options.RHOEND]:
                status = ExitStatus.RADIUS_SUCCESS
            framework.enhance_resolution(options)
            framework.decrease_penalty()
                maxcv_val = pb.maxcv(
                    framework.x_best, framework.cub_best, framework.ceq_best
                _print_step(
                    f"New trust-region radius: {framework.resolution}",
                    pb.build_x(framework.x_best),
                    maxcv_val,
                    pb.n_eval,
                    n_iter,
        # Improve the geometry of the interpolation set if necessary.
        if improve_geometry:
                step = framework.get_geometry_step(k_new, options)
                fun_val, cub_val, ceq_val = _eval(pb, framework, step, options)
                framework.models.update_interpolation(
                    k_new,
                    framework.x_best + step,
        framework.penalty,
def _get_bounds(bounds, n):
    Uniformize the bounds.
    if bounds is None:
        return Bounds(np.full(n, -np.inf), np.full(n, np.inf))
    elif isinstance(bounds, Bounds):
        if bounds.lb.shape != (n,) or bounds.ub.shape != (n,):
            raise ValueError(f"The bounds must have {n} elements.")
        return Bounds(bounds.lb, bounds.ub)
    elif hasattr(bounds, "__len__"):
        bounds = np.asarray(bounds)
        if bounds.shape != (n, 2):
                "The shape of the bounds is not compatible with "
                "the number of variables."
        return Bounds(bounds[:, 0], bounds[:, 1])
            "The bounds must be an instance of "
            "scipy.optimize.Bounds or an array-like object."
def _get_constraints(constraints):
    Extract the linear and nonlinear constraints.
    if isinstance(constraints, dict) or not hasattr(constraints, "__len__"):
        constraints = (constraints,)
    # Extract the linear and nonlinear constraints.
    linear_constraints = []
    nonlinear_constraints = []
    for constraint in constraints:
        if isinstance(constraint, LinearConstraint):
            lb = exact_1d_array(
                constraint.lb,
                "The lower bound of the linear constraints must be a vector.",
            ub = exact_1d_array(
                constraint.ub,
                "The upper bound of the linear constraints must be a vector.",
            linear_constraints.append(
                LinearConstraint(
                    constraint.A,
                    *np.broadcast_arrays(lb, ub),
        elif isinstance(constraint, NonlinearConstraint):
                "The lower bound of the "
                "nonlinear constraints must be a "
                "vector.",
                "The upper bound of the "
            nonlinear_constraints.append(
                NonlinearConstraint(
                    constraint.fun,
        elif isinstance(constraint, dict):
            if "type" not in constraint or constraint["type"] not in (
                "eq",
                "ineq",
                raise ValueError('The constraint type must be "eq" or "ineq".')
            if "fun" not in constraint or not callable(constraint["fun"]):
                raise ValueError("The constraint function must be callable.")
                    "fun": constraint["fun"],
                    "type": constraint["type"],
                    "args": constraint.get("args", ()),
                "The constraints must be instances of "
                "scipy.optimize.LinearConstraint, "
                "scipy.optimize.NonlinearConstraint, or dict."
    return linear_constraints, nonlinear_constraints
def _set_default_options(options, n):
    Set the default options.
    if Options.RHOBEG in options and options[Options.RHOBEG] <= 0.0:
        raise ValueError("The initial trust-region radius must be positive.")
    if Options.RHOEND in options and options[Options.RHOEND] < 0.0:
        raise ValueError("The final trust-region radius must be nonnegative.")
    if Options.RHOBEG in options and Options.RHOEND in options:
        if options[Options.RHOBEG] < options[Options.RHOEND]:
                "The initial trust-region radius must be greater "
                "than or equal to the final trust-region radius."
    elif Options.RHOBEG in options:
        options[Options.RHOEND.value] = np.min(
                DEFAULT_OPTIONS[Options.RHOEND],
                options[Options.RHOBEG],
    elif Options.RHOEND in options:
        options[Options.RHOBEG.value] = np.max(
                DEFAULT_OPTIONS[Options.RHOBEG],
                options[Options.RHOEND],
        options[Options.RHOBEG.value] = DEFAULT_OPTIONS[Options.RHOBEG]
        options[Options.RHOEND.value] = DEFAULT_OPTIONS[Options.RHOEND]
    options[Options.RHOBEG.value] = float(options[Options.RHOBEG])
    options[Options.RHOEND.value] = float(options[Options.RHOEND])
    if Options.NPT in options and options[Options.NPT] <= 0:
        raise ValueError("The number of interpolation points must be "
                         "positive.")
        Options.NPT in options
        and options[Options.NPT] > ((n + 1) * (n + 2)) // 2
            f"The number of interpolation points must be at most "
            f"{((n + 1) * (n + 2)) // 2}."
    options.setdefault(Options.NPT.value, DEFAULT_OPTIONS[Options.NPT](n))
    options[Options.NPT.value] = int(options[Options.NPT])
    if Options.MAX_EVAL in options and options[Options.MAX_EVAL] <= 0:
            "The maximum number of function evaluations must be positive."
    options.setdefault(
        Options.MAX_EVAL.value,
        np.max(
                DEFAULT_OPTIONS[Options.MAX_EVAL](n),
                options[Options.NPT] + 1,
    options[Options.MAX_EVAL.value] = int(options[Options.MAX_EVAL])
    if Options.MAX_ITER in options and options[Options.MAX_ITER] <= 0:
        raise ValueError("The maximum number of iterations must be positive.")
        Options.MAX_ITER.value,
        DEFAULT_OPTIONS[Options.MAX_ITER](n),
    options[Options.MAX_ITER.value] = int(options[Options.MAX_ITER])
    options.setdefault(Options.TARGET.value, DEFAULT_OPTIONS[Options.TARGET])
    options[Options.TARGET.value] = float(options[Options.TARGET])
        Options.FEASIBILITY_TOL.value,
    options[Options.FEASIBILITY_TOL.value] = float(
        options[Options.FEASIBILITY_TOL]
    options.setdefault(Options.VERBOSE.value, DEFAULT_OPTIONS[Options.VERBOSE])
    options[Options.VERBOSE.value] = bool(options[Options.VERBOSE])
    options.setdefault(Options.SCALE.value, DEFAULT_OPTIONS[Options.SCALE])
    options[Options.SCALE.value] = bool(options[Options.SCALE])
        Options.FILTER_SIZE.value,
    options[Options.FILTER_SIZE.value] = int(options[Options.FILTER_SIZE])
        Options.STORE_HISTORY.value,
    options[Options.STORE_HISTORY.value] = bool(options[Options.STORE_HISTORY])
        Options.HISTORY_SIZE.value,
    options[Options.HISTORY_SIZE.value] = int(options[Options.HISTORY_SIZE])
    options.setdefault(Options.DEBUG.value, DEFAULT_OPTIONS[Options.DEBUG])
    options[Options.DEBUG.value] = bool(options[Options.DEBUG])
    # Check whether they are any unknown options.
    for key in options:
        if key not in Options.__members__.values():
            warnings.warn(f"Unknown option: {key}.", RuntimeWarning, 3)
def _set_default_constants(**kwargs):
    Set the default constants.
    constants = dict(kwargs)
    constants.setdefault(
        Constants.DECREASE_RADIUS_FACTOR.value,
        DEFAULT_CONSTANTS[Constants.DECREASE_RADIUS_FACTOR],
    constants[Constants.DECREASE_RADIUS_FACTOR.value] = float(
        constants[Constants.DECREASE_RADIUS_FACTOR]
        constants[Constants.DECREASE_RADIUS_FACTOR] <= 0.0
        or constants[Constants.DECREASE_RADIUS_FACTOR] >= 1.0
            "The constant decrease_radius_factor must be in the interval "
            "(0, 1)."
        Constants.INCREASE_RADIUS_THRESHOLD.value,
        DEFAULT_CONSTANTS[Constants.INCREASE_RADIUS_THRESHOLD],
    constants[Constants.INCREASE_RADIUS_THRESHOLD.value] = float(
        constants[Constants.INCREASE_RADIUS_THRESHOLD]
    if constants[Constants.INCREASE_RADIUS_THRESHOLD] <= 1.0:
            "The constant increase_radius_threshold must be greater than 1."
        Constants.INCREASE_RADIUS_FACTOR in constants
        and constants[Constants.INCREASE_RADIUS_FACTOR] <= 1.0
            "The constant increase_radius_factor must be greater than 1."
        Constants.DECREASE_RADIUS_THRESHOLD in constants
        and constants[Constants.DECREASE_RADIUS_THRESHOLD] <= 1.0
            "The constant decrease_radius_threshold must be greater than 1."
        and Constants.DECREASE_RADIUS_THRESHOLD in constants
            constants[Constants.DECREASE_RADIUS_THRESHOLD]
            >= constants[Constants.INCREASE_RADIUS_FACTOR]
                "The constant decrease_radius_threshold must be "
                "less than increase_radius_factor."
    elif Constants.INCREASE_RADIUS_FACTOR in constants:
        constants[Constants.DECREASE_RADIUS_THRESHOLD.value] = np.min(
                DEFAULT_CONSTANTS[Constants.DECREASE_RADIUS_THRESHOLD],
                0.5 * (1.0 + constants[Constants.INCREASE_RADIUS_FACTOR]),
    elif Constants.DECREASE_RADIUS_THRESHOLD in constants:
        constants[Constants.INCREASE_RADIUS_FACTOR.value] = np.max(
                DEFAULT_CONSTANTS[Constants.INCREASE_RADIUS_FACTOR],
                2.0 * constants[Constants.DECREASE_RADIUS_THRESHOLD],
        constants[Constants.INCREASE_RADIUS_FACTOR.value] = DEFAULT_CONSTANTS[
            Constants.INCREASE_RADIUS_FACTOR
        constants[Constants.DECREASE_RADIUS_THRESHOLD.value] = (
            DEFAULT_CONSTANTS[Constants.DECREASE_RADIUS_THRESHOLD])
        Constants.DECREASE_RESOLUTION_FACTOR.value,
        DEFAULT_CONSTANTS[Constants.DECREASE_RESOLUTION_FACTOR],
    constants[Constants.DECREASE_RESOLUTION_FACTOR.value] = float(
        constants[Constants.DECREASE_RESOLUTION_FACTOR]
        constants[Constants.DECREASE_RESOLUTION_FACTOR] <= 0.0
        or constants[Constants.DECREASE_RESOLUTION_FACTOR] >= 1.0
            "The constant decrease_resolution_factor must be in the interval "
        Constants.LARGE_RESOLUTION_THRESHOLD in constants
        and constants[Constants.LARGE_RESOLUTION_THRESHOLD] <= 1.0
            "The constant large_resolution_threshold must be greater than 1."
        Constants.MODERATE_RESOLUTION_THRESHOLD in constants
        and constants[Constants.MODERATE_RESOLUTION_THRESHOLD] <= 1.0
            "The constant moderate_resolution_threshold must be greater than "
            "1."
        and Constants.MODERATE_RESOLUTION_THRESHOLD in constants
            constants[Constants.MODERATE_RESOLUTION_THRESHOLD]
            > constants[Constants.LARGE_RESOLUTION_THRESHOLD]
                "The constant moderate_resolution_threshold "
                "must be at most large_resolution_threshold."
    elif Constants.LARGE_RESOLUTION_THRESHOLD in constants:
        constants[Constants.MODERATE_RESOLUTION_THRESHOLD.value] = np.min(
                DEFAULT_CONSTANTS[Constants.MODERATE_RESOLUTION_THRESHOLD],
                constants[Constants.LARGE_RESOLUTION_THRESHOLD],
    elif Constants.MODERATE_RESOLUTION_THRESHOLD in constants:
        constants[Constants.LARGE_RESOLUTION_THRESHOLD.value] = np.max(
                DEFAULT_CONSTANTS[Constants.LARGE_RESOLUTION_THRESHOLD],
                constants[Constants.MODERATE_RESOLUTION_THRESHOLD],
        constants[Constants.LARGE_RESOLUTION_THRESHOLD.value] = (
            DEFAULT_CONSTANTS[Constants.LARGE_RESOLUTION_THRESHOLD]
        constants[Constants.MODERATE_RESOLUTION_THRESHOLD.value] = (
            DEFAULT_CONSTANTS[Constants.MODERATE_RESOLUTION_THRESHOLD]
    if Constants.LOW_RATIO in constants and (
        constants[Constants.LOW_RATIO] <= 0.0
        or constants[Constants.LOW_RATIO] >= 1.0
            "The constant low_ratio must be in the interval (0, 1)."
    if Constants.HIGH_RATIO in constants and (
        constants[Constants.HIGH_RATIO] <= 0.0
        or constants[Constants.HIGH_RATIO] >= 1.0
            "The constant high_ratio must be in the interval (0, 1)."
    if Constants.LOW_RATIO in constants and Constants.HIGH_RATIO in constants:
        if constants[Constants.LOW_RATIO] > constants[Constants.HIGH_RATIO]:
                "The constant low_ratio must be at most high_ratio."
    elif Constants.LOW_RATIO in constants:
        constants[Constants.HIGH_RATIO.value] = np.max(
                DEFAULT_CONSTANTS[Constants.HIGH_RATIO],
                constants[Constants.LOW_RATIO],
    elif Constants.HIGH_RATIO in constants:
        constants[Constants.LOW_RATIO.value] = np.min(
                DEFAULT_CONSTANTS[Constants.LOW_RATIO],
                constants[Constants.HIGH_RATIO],
        constants[Constants.LOW_RATIO.value] = DEFAULT_CONSTANTS[
            Constants.LOW_RATIO
        constants[Constants.HIGH_RATIO.value] = DEFAULT_CONSTANTS[
            Constants.HIGH_RATIO
        Constants.VERY_LOW_RATIO.value,
        DEFAULT_CONSTANTS[Constants.VERY_LOW_RATIO],
    constants[Constants.VERY_LOW_RATIO.value] = float(
        constants[Constants.VERY_LOW_RATIO]
        constants[Constants.VERY_LOW_RATIO] <= 0.0
        or constants[Constants.VERY_LOW_RATIO] >= 1.0
            "The constant very_low_ratio must be in the interval (0, 1)."
        Constants.PENALTY_INCREASE_THRESHOLD in constants
        and constants[Constants.PENALTY_INCREASE_THRESHOLD] < 1.0
            "The constant penalty_increase_threshold must be "
            "greater than or equal to 1."
        Constants.PENALTY_INCREASE_FACTOR in constants
        and constants[Constants.PENALTY_INCREASE_FACTOR] <= 1.0
            "The constant penalty_increase_factor must be greater than 1."
        and Constants.PENALTY_INCREASE_FACTOR in constants
            constants[Constants.PENALTY_INCREASE_FACTOR]
            < constants[Constants.PENALTY_INCREASE_THRESHOLD]
                "The constant penalty_increase_factor must be "
                "greater than or equal to "
                "penalty_increase_threshold."
    elif Constants.PENALTY_INCREASE_THRESHOLD in constants:
        constants[Constants.PENALTY_INCREASE_FACTOR.value] = np.max(
                DEFAULT_CONSTANTS[Constants.PENALTY_INCREASE_FACTOR],
                constants[Constants.PENALTY_INCREASE_THRESHOLD],
    elif Constants.PENALTY_INCREASE_FACTOR in constants:
        constants[Constants.PENALTY_INCREASE_THRESHOLD.value] = np.min(
                DEFAULT_CONSTANTS[Constants.PENALTY_INCREASE_THRESHOLD],
                constants[Constants.PENALTY_INCREASE_FACTOR],
        constants[Constants.PENALTY_INCREASE_THRESHOLD.value] = (
            DEFAULT_CONSTANTS[Constants.PENALTY_INCREASE_THRESHOLD]
        constants[Constants.PENALTY_INCREASE_FACTOR.value] = DEFAULT_CONSTANTS[
            Constants.PENALTY_INCREASE_FACTOR
        Constants.SHORT_STEP_THRESHOLD.value,
        DEFAULT_CONSTANTS[Constants.SHORT_STEP_THRESHOLD],
    constants[Constants.SHORT_STEP_THRESHOLD.value] = float(
        constants[Constants.SHORT_STEP_THRESHOLD]
        constants[Constants.SHORT_STEP_THRESHOLD] <= 0.0
        or constants[Constants.SHORT_STEP_THRESHOLD] >= 1.0
            "The constant short_step_threshold must be in the interval (0, 1)."
        Constants.LOW_RADIUS_FACTOR.value,
        DEFAULT_CONSTANTS[Constants.LOW_RADIUS_FACTOR],
    constants[Constants.LOW_RADIUS_FACTOR.value] = float(
        constants[Constants.LOW_RADIUS_FACTOR]
        constants[Constants.LOW_RADIUS_FACTOR] <= 0.0
        or constants[Constants.LOW_RADIUS_FACTOR] >= 1.0
            "The constant low_radius_factor must be in the interval (0, 1)."
        Constants.BYRD_OMOJOKUN_FACTOR.value,
        DEFAULT_CONSTANTS[Constants.BYRD_OMOJOKUN_FACTOR],
    constants[Constants.BYRD_OMOJOKUN_FACTOR.value] = float(
        constants[Constants.BYRD_OMOJOKUN_FACTOR]
        constants[Constants.BYRD_OMOJOKUN_FACTOR] <= 0.0
        or constants[Constants.BYRD_OMOJOKUN_FACTOR] >= 1.0
            "The constant byrd_omojokun_factor must be in the interval (0, 1)."
        Constants.THRESHOLD_RATIO_CONSTRAINTS.value,
        DEFAULT_CONSTANTS[Constants.THRESHOLD_RATIO_CONSTRAINTS],
    constants[Constants.THRESHOLD_RATIO_CONSTRAINTS.value] = float(
        constants[Constants.THRESHOLD_RATIO_CONSTRAINTS]
    if constants[Constants.THRESHOLD_RATIO_CONSTRAINTS] <= 1.0:
            "The constant threshold_ratio_constraints must be greater than 1."
        Constants.LARGE_SHIFT_FACTOR.value,
        DEFAULT_CONSTANTS[Constants.LARGE_SHIFT_FACTOR],
    constants[Constants.LARGE_SHIFT_FACTOR.value] = float(
        constants[Constants.LARGE_SHIFT_FACTOR]
    if constants[Constants.LARGE_SHIFT_FACTOR] < 0.0:
        raise ValueError("The constant large_shift_factor must be "
                         "nonnegative.")
        Constants.LARGE_GRADIENT_FACTOR.value,
        DEFAULT_CONSTANTS[Constants.LARGE_GRADIENT_FACTOR],
    constants[Constants.LARGE_GRADIENT_FACTOR.value] = float(
        constants[Constants.LARGE_GRADIENT_FACTOR]
    if constants[Constants.LARGE_GRADIENT_FACTOR] <= 1.0:
            "The constant large_gradient_factor must be greater than 1."
        Constants.RESOLUTION_FACTOR.value,
        DEFAULT_CONSTANTS[Constants.RESOLUTION_FACTOR],
    constants[Constants.RESOLUTION_FACTOR.value] = float(
    if constants[Constants.RESOLUTION_FACTOR] <= 1.0:
            "The constant resolution_factor must be greater than 1."
        Constants.IMPROVE_TCG.value,
        DEFAULT_CONSTANTS[Constants.IMPROVE_TCG],
    constants[Constants.IMPROVE_TCG.value] = bool(
        constants[Constants.IMPROVE_TCG]
        if key not in Constants.__members__.values():
            warnings.warn(f"Unknown constant: {key}.", RuntimeWarning, 3)
    return constants
def _eval(pb, framework, step, options):
    Evaluate the objective and constraint functions.
    if pb.n_eval >= options[Options.MAX_EVAL]:
        raise MaxEvalError
    x_eval = framework.x_best + step
    fun_val, cub_val, ceq_val = pb(x_eval, framework.penalty)
    r_val = pb.maxcv(x_eval, cub_val, ceq_val)
        fun_val <= options[Options.TARGET]
        and r_val <= options[Options.FEASIBILITY_TOL]
        raise TargetSuccess
    if pb.is_feasibility and r_val <= options[Options.FEASIBILITY_TOL]:
        raise FeasibleSuccess
    return fun_val, cub_val, ceq_val
def _build_result(pb, penalty, success, status, n_iter, options):
    Build the result of the optimization process.
    # Build the result.
    x, fun, maxcv = pb.best_eval(penalty)
    success = success and np.isfinite(fun) and np.isfinite(maxcv)
    if status not in [ExitStatus.TARGET_SUCCESS, ExitStatus.FEASIBLE_SUCCESS]:
        success = success and maxcv <= options[Options.FEASIBILITY_TOL]
    result = OptimizeResult()
    result.message = {
        ExitStatus.RADIUS_SUCCESS: "The lower bound for the trust-region "
                                   "radius has been reached",
        ExitStatus.TARGET_SUCCESS: "The target objective function value has "
                                   "been reached",
        ExitStatus.FIXED_SUCCESS: "All variables are fixed by the bound "
        ExitStatus.CALLBACK_SUCCESS: "The callback requested to stop the "
                                     "optimization procedure",
        ExitStatus.FEASIBLE_SUCCESS: "The feasibility problem received has "
                                     "been solved successfully",
        ExitStatus.MAX_EVAL_WARNING: "The maximum number of function "
                                     "evaluations has been exceeded",
        ExitStatus.MAX_ITER_WARNING: "The maximum number of iterations has "
                                     "been exceeded",
        ExitStatus.INFEASIBLE_ERROR: "The bound constraints are infeasible",
        ExitStatus.LINALG_ERROR: "A linear algebra error occurred",
    }.get(status, "Unknown exit status")
    result.success = success
    result.status = status.value
    result.x = pb.build_x(x)
    result.fun = fun
    result.maxcv = maxcv
    result.nfev = pb.n_eval
    result.nit = n_iter
    if options[Options.STORE_HISTORY]:
        result.fun_history = pb.fun_history
        result.maxcv_history = pb.maxcv_history
    # Print the result if requested.
    if options[Options.VERBOSE]:
            result.message,
            result.x,
            result.fun,
            result.maxcv,
            result.nfev,
            result.nit,
def _print_step(message, pb, x, fun_val, r_val, n_eval, n_iter):
    Print information about the current state of the optimization process.
    print(f"{message}.")
    print(f"Number of function evaluations: {n_eval}.")
    print(f"Number of iterations: {n_iter}.")
    if not pb.is_feasibility:
        print(f"Least value of {pb.fun_name}: {fun_val}.")
    print(f"Maximum constraint violation: {r_val}.")
    with np.printoptions(**PRINT_OPTIONS):
        print(f"Corresponding point: {x}.")
from collections.abc import Callable, Generator, Iterable, Mapping, MutableMapping
from typing import Any, Literal, overload
from . import helpers, presets
from .common import normalize_url, utils
from .parser_block import ParserBlock
from .parser_core import ParserCore
from .parser_inline import ParserInline
from .renderer import RendererHTML, RendererProtocol
from .rules_core.state_core import StateCore
from .token import Token
from .utils import EnvType, OptionsDict, OptionsType, PresetType
    import linkify_it
    linkify_it = None
_PRESETS: dict[str, PresetType] = {
    "default": presets.default.make(),
    "js-default": presets.js_default.make(),
    "zero": presets.zero.make(),
    "commonmark": presets.commonmark.make(),
    "gfm-like": presets.gfm_like.make(),
class MarkdownIt:
        config: str | PresetType = "commonmark",
        options_update: Mapping[str, Any] | None = None,
        renderer_cls: Callable[[MarkdownIt], RendererProtocol] = RendererHTML,
        """Main parser class
        :param config: name of configuration to load or a pre-defined dictionary
        :param options_update: dictionary that will be merged into ``config["options"]``
        :param renderer_cls: the class to load as the renderer:
            ``self.renderer = renderer_cls(self)
        # add modules
        self.utils = utils
        self.helpers = helpers
        # initialise classes
        self.inline = ParserInline()
        self.block = ParserBlock()
        self.core = ParserCore()
        self.renderer = renderer_cls(self)
        self.linkify = linkify_it.LinkifyIt() if linkify_it else None
        # set the configuration
        if options_update and not isinstance(options_update, Mapping):
            # catch signature change where renderer_cls was not used as a key-word
                f"options_update should be a mapping: {options_update}"
                "\n(Perhaps you intended this to be the renderer_cls?)"
        self.configure(config, options_update=options_update)
        return f"{self.__class__.__module__}.{self.__class__.__name__}()"
    def __getitem__(self, name: Literal["inline"]) -> ParserInline: ...
    def __getitem__(self, name: Literal["block"]) -> ParserBlock: ...
    def __getitem__(self, name: Literal["core"]) -> ParserCore: ...
    def __getitem__(self, name: Literal["renderer"]) -> RendererProtocol: ...
    def __getitem__(self, name: str) -> Any: ...
    def __getitem__(self, name: str) -> Any:
            "inline": self.inline,
            "block": self.block,
            "core": self.core,
            "renderer": self.renderer,
        }[name]
    def set(self, options: OptionsType) -> None:
        """Set parser options (in the same format as in constructor).
        Probably, you will never need it, but you can change options after constructor call.
        __Note:__ To achieve the best possible performance, don't modify a
        `markdown-it` instance options on the fly. If you need multiple configurations
        it's best to create multiple instances and initialize each with separate config.
        self.options = OptionsDict(options)
    def configure(
        self, presets: str | PresetType, options_update: Mapping[str, Any] | None = None
    ) -> MarkdownIt:
        """Batch load of all options and component settings.
        This is an internal method, and you probably will not need it.
        But if you will - see available presets and data structure
        [here](https://github.com/markdown-it/markdown-it/tree/master/lib/presets)
        We strongly recommend to use presets instead of direct config loads.
        That will give better compatibility with next versions.
        if isinstance(presets, str):
            if presets not in _PRESETS:
                raise KeyError(f"Wrong `markdown-it` preset '{presets}', check name")
            config = _PRESETS[presets]
            config = presets
        if not config:
            raise ValueError("Wrong `markdown-it` config, can't be empty")
        options = config.get("options", {}) or {}
        if options_update:
            options = {**options, **options_update}  # type: ignore
        self.set(options)  # type: ignore
        if "components" in config:
            for name, component in config["components"].items():
                rules = component.get("rules", None)
                if rules:
                    self[name].ruler.enableOnly(rules)
                rules2 = component.get("rules2", None)
                if rules2:
                    self[name].ruler2.enableOnly(rules2)
    def get_all_rules(self) -> dict[str, list[str]]:
        """Return the names of all active rules."""
        rules = {
            chain: self[chain].ruler.get_all_rules()
            for chain in ["core", "block", "inline"]
        rules["inline2"] = self.inline.ruler2.get_all_rules()
    def get_active_rules(self) -> dict[str, list[str]]:
            chain: self[chain].ruler.get_active_rules()
        rules["inline2"] = self.inline.ruler2.get_active_rules()
    def enable(
        self, names: str | Iterable[str], ignoreInvalid: bool = False
        """Enable list or rules. (chainable)
        :param names: rule name or list of rule names to enable.
        :param ignoreInvalid: set `true` to ignore errors when rule not found.
        It will automatically find appropriate components,
        containing rules with given names. If rule not found, and `ignoreInvalid`
        not set - throws exception.
            md = MarkdownIt().enable(['sub', 'sup']).disable('smartquotes')
        if isinstance(names, str):
            names = [names]
        for chain in ["core", "block", "inline"]:
            result.extend(self[chain].ruler.enable(names, True))
        result.extend(self.inline.ruler2.enable(names, True))
        missed = [name for name in names if name not in result]
        if missed and not ignoreInvalid:
            raise ValueError(f"MarkdownIt. Failed to enable unknown rule(s): {missed}")
    def disable(
        """The same as [[MarkdownIt.enable]], but turn specified rules off. (chainable)
        :param names: rule name or list of rule names to disable.
            result.extend(self[chain].ruler.disable(names, True))
        result.extend(self.inline.ruler2.disable(names, True))
            raise ValueError(f"MarkdownIt. Failed to disable unknown rule(s): {missed}")
    def reset_rules(self) -> Generator[None, None, None]:
        """A context manager, that will reset the current enabled rules on exit."""
        chain_rules = self.get_active_rules()
        for chain, rules in chain_rules.items():
            if chain != "inline2":
                self[chain].ruler.enableOnly(rules)
        self.inline.ruler2.enableOnly(chain_rules["inline2"])
    def add_render_rule(
        self, name: str, function: Callable[..., Any], fmt: str = "html"
        """Add a rule for rendering a particular Token type.
        Only applied when ``renderer.__output__ == fmt``
        if self.renderer.__output__ == fmt:
            self.renderer.rules[name] = function.__get__(self.renderer)  # type: ignore
    def use(
        self, plugin: Callable[..., None], *params: Any, **options: Any
        """Load specified plugin with given params into current parser instance. (chainable)
        It's just a sugar to call `plugin(md, params)` with curring.
            def func(tokens, idx):
                tokens[idx].content = tokens[idx].content.replace('foo', 'bar')
            md = MarkdownIt().use(plugin, 'foo_replace', 'text', func)
        plugin(self, *params, **options)
    def parse(self, src: str, env: EnvType | None = None) -> list[Token]:
        """Parse the source string to a token stream
        :param src: source string
        :param env: environment sandbox
        Parse input string and return list of block tokens (special token type
        "inline" will contain list of inline tokens).
        `env` is used to pass data between "distributed" rules and return additional
        metadata like reference info, needed for the renderer. It also can be used to
        inject data in specific cases. Usually, you will be ok to pass `{}`,
        and then pass updated object to renderer.
        env = {} if env is None else env
        if not isinstance(env, MutableMapping):
            raise TypeError(f"Input data should be a MutableMapping, not {type(env)}")
        if not isinstance(src, str):
            raise TypeError(f"Input data should be a string, not {type(src)}")
        state = StateCore(src, self, env)
        self.core.process(state)
        return state.tokens
    def render(self, src: str, env: EnvType | None = None) -> Any:
        """Render markdown string into html. It does all magic for you :).
        :returns: The output of the loaded renderer
        `env` can be used to inject additional metadata (`{}` by default).
        But you will not need it with high probability. See also comment
        in [[MarkdownIt.parse]].
        return self.renderer.render(self.parse(src, env), self.options, env)
    def parseInline(self, src: str, env: EnvType | None = None) -> list[Token]:
        """The same as [[MarkdownIt.parse]] but skip all block rules.
        It returns the
        block tokens list with the single `inline` element, containing parsed inline
        tokens in `children` property. Also updates `env` object.
            raise TypeError(f"Input data should be an MutableMapping, not {type(env)}")
        state.inlineMode = True
    def renderInline(self, src: str, env: EnvType | None = None) -> Any:
        """Similar to [[MarkdownIt.render]] but for single paragraph content.
        Similar to [[MarkdownIt.render]] but for single paragraph content. Result
        will NOT be wrapped into `<p>` tags.
        return self.renderer.render(self.parseInline(src, env), self.options, env)
    # link methods
    def validateLink(self, url: str) -> bool:
        """Validate if the URL link is allowed in output.
        This validator can prohibit more than really needed to prevent XSS.
        It's a tradeoff to keep code simple and to be secure by default.
        Note: the url should be normalized at this point, and existing entities decoded.
        return normalize_url.validateLink(url)
    def normalizeLink(self, url: str) -> str:
        """Normalize destination URLs in links
            [label]:   destination   'title'
                    ^^^^^^^^^^^
        return normalize_url.normalizeLink(url)
    def normalizeLinkText(self, link: str) -> str:
        """Normalize autolink content
            <destination>
        return normalize_url.normalizeLinkText(link)
from typing import TYPE_CHECKING, AsyncGenerator, Callable, Generator, Optional, Set, Tuple, Union
from ._rust_notify import RustNotify
from .filters import DefaultFilter
__all__ = 'watch', 'awatch', 'Change', 'FileChange'
logger = logging.getLogger('watchfiles.main')
class Change(IntEnum):
    Enum representing the type of change that occurred.
    added = 1
    """A new file or directory was added."""
    modified = 2
    """A file or directory was modified, can be either a metadata or data change."""
    deleted = 3
    """A file or directory was deleted."""
    def raw_str(self) -> str:
FileChange = Tuple[Change, str]
A tuple representing a file change, first element is a [`Change`][watchfiles.Change] member, second is the path
of the file or directory that changed.
    import trio
    AnyEvent = Union[anyio.Event, asyncio.Event, trio.Event]
    class AbstractEvent(Protocol):
        def is_set(self) -> bool: ...
def watch(
    *paths: Union[Path, str],
    watch_filter: Optional[Callable[['Change', str], bool]] = DefaultFilter(),
    debounce: int = 1_600,
    step: int = 50,
    stop_event: Optional['AbstractEvent'] = None,
    rust_timeout: int = 5_000,
    yield_on_timeout: bool = False,
    debug: Optional[bool] = None,
    raise_interrupt: bool = True,
    force_polling: Optional[bool] = None,
    poll_delay_ms: int = 300,
    recursive: bool = True,
    ignore_permission_denied: Optional[bool] = None,
) -> Generator[Set[FileChange], None, None]:
    Watch one or more paths and yield a set of changes whenever files change.
    The paths watched can be directories or files, directories are watched recursively - changes in subdirectories
    are also detected.
    #### Force polling
    Notify will fall back to file polling if it can't use file system notifications, but we also force Notify
    to use polling if the `force_polling` argument is `True`; if `force_polling` is unset (or `None`), we enable
    force polling thus:
    * if the `WATCHFILES_FORCE_POLLING` environment variable exists and is not empty:
        * if the value is `false`, `disable` or `disabled`, force polling is disabled
        * otherwise, force polling is enabled
    * otherwise, we enable force polling only if we detect we're running on WSL (Windows Subsystem for Linux)
    It is also possible to change the poll delay between iterations, it can be changed to maintain a good response time
    and an appropiate CPU consumption using the `poll_delay_ms` argument, we change poll delay thus:
    * if file polling is enabled and the `WATCHFILES_POLL_DELAY_MS` env var exists and it is numeric, we use that
    * otherwise, we use the argument value
        *paths: filesystem paths to watch.
        watch_filter: callable used to filter out changes which are not important, you can either use a raw callable
            or a [`BaseFilter`][watchfiles.BaseFilter] instance,
            defaults to an instance of [`DefaultFilter`][watchfiles.DefaultFilter]. To keep all changes, use `None`.
        debounce: maximum time in milliseconds to group changes over before yielding them.
        step: time to wait for new changes in milliseconds, if no changes are detected in this time, and
            at least one change has been detected, the changes are yielded.
        stop_event: event to stop watching, if this is set, the generator will stop iteration,
            this can be anything with an `is_set()` method which returns a bool, e.g. `threading.Event()`.
        rust_timeout: maximum time in milliseconds to wait in the rust code for changes, `0` means no timeout.
        yield_on_timeout: if `True`, the generator will yield upon timeout in rust even if no changes are detected.
        debug: whether to print information about all filesystem changes in rust to stdout, if `None` will use the
            `WATCHFILES_DEBUG` environment variable.
        raise_interrupt: whether to re-raise `KeyboardInterrupt`s, or suppress the error and just stop iterating.
        force_polling: See [Force polling](#force-polling) above.
        poll_delay_ms: delay between polling for changes, only used if `force_polling=True`.
        recursive: if `True`, watch for changes in sub-directories recursively, otherwise watch only for changes in the
            top-level directory, default is `True`.
        ignore_permission_denied: if `True`, will ignore permission denied errors, otherwise will raise them by default.
            Setting the `WATCHFILES_IGNORE_PERMISSION_DENIED` environment variable will set this value too.
        The generator yields sets of [`FileChange`][watchfiles.main.FileChange]s.
    ```py title="Example of watch usage"
    from watchfiles import watch
    for changes in watch('./first/dir', './second/dir', raise_interrupt=False):
        print(changes)
    force_polling = _default_force_polling(force_polling)
    poll_delay_ms = _default_poll_delay_ms(poll_delay_ms)
    ignore_permission_denied = _default_ignore_permission_denied(ignore_permission_denied)
    debug = _default_debug(debug)
    with RustNotify(
        [str(p) for p in paths], debug, force_polling, poll_delay_ms, recursive, ignore_permission_denied
    ) as watcher:
            raw_changes = watcher.watch(debounce, step, rust_timeout, stop_event)
            if raw_changes == 'timeout':
                if yield_on_timeout:
                    yield set()
                    logger.debug('rust notify timeout, continuing')
            elif raw_changes == 'signal':
                if raise_interrupt:
                    logger.warning('KeyboardInterrupt caught, stopping watch')
            elif raw_changes == 'stop':
                changes = _prep_changes(raw_changes, watch_filter)
                    _log_changes(changes)
                    yield changes
                    logger.debug('all changes filtered out, raw_changes=%s', raw_changes)
async def awatch(  # C901
    watch_filter: Optional[Callable[[Change, str], bool]] = DefaultFilter(),
    stop_event: Optional['AnyEvent'] = None,
    rust_timeout: Optional[int] = None,
    raise_interrupt: Optional[bool] = None,
) -> AsyncGenerator[Set[FileChange], None]:
    Asynchronous equivalent of [`watch`][watchfiles.watch] using threads to wait for changes.
    Arguments match those of [`watch`][watchfiles.watch] except `stop_event`.
    All async methods use [anyio](https://anyio.readthedocs.io/en/latest/) to run the event loop.
    Unlike [`watch`][watchfiles.watch] `KeyboardInterrupt` cannot be suppressed by `awatch` so they need to be caught
    where `asyncio.run` or equivalent is called.
        watch_filter: matches the same argument of [`watch`][watchfiles.watch].
        debounce: matches the same argument of [`watch`][watchfiles.watch].
        step: matches the same argument of [`watch`][watchfiles.watch].
        stop_event: `anyio.Event` which can be used to stop iteration, see example below.
        rust_timeout: matches the same argument of [`watch`][watchfiles.watch], except that `None` means
            use `1_000` on Windows and `5_000` on other platforms thus helping with exiting on `Ctrl+C` on Windows,
            see [#110](https://github.com/samuelcolvin/watchfiles/issues/110).
        yield_on_timeout: matches the same argument of [`watch`][watchfiles.watch].
        debug: matches the same argument of [`watch`][watchfiles.watch].
        raise_interrupt: This is deprecated, `KeyboardInterrupt` will cause this coroutine to be cancelled and then
            be raised by the top level `asyncio.run` call or equivalent, and should be caught there.
            See [#136](https://github.com/samuelcolvin/watchfiles/issues/136)
        force_polling: if true, always use polling instead of file system notifications, default is `None` where
            `force_polling` is set to `True` if the `WATCHFILES_FORCE_POLLING` environment variable exists.
            `poll_delay_ms` can be changed via the `WATCHFILES_POLL_DELAY_MS` environment variable.
    ```py title="Example of awatch usage"
    from watchfiles import awatch
    async def main():
        async for changes in awatch('./first/dir', './second/dir'):
            print('stopped via KeyboardInterrupt')
    ```py title="Example of awatch usage with a stop event"
        stop_event = asyncio.Event()
        async def stop_soon():
            stop_event.set()
        stop_soon_task = asyncio.create_task(stop_soon())
        async for changes in awatch('/path/to/dir', stop_event=stop_event):
        # cleanup by awaiting the (now complete) stop_soon_task
        await stop_soon_task
    if raise_interrupt is not None:
            'raise_interrupt is deprecated, KeyboardInterrupt will cause this coroutine to be cancelled and then '
            'be raised by the top level asyncio.run call or equivalent, and should be caught there. See #136.',
    if stop_event is None:
        stop_event_: AnyEvent = anyio.Event()
        stop_event_ = stop_event
        timeout = _calc_async_timeout(rust_timeout)
        CancelledError = anyio.get_cancelled_exc_class()
            async with anyio.create_task_group() as tg:
                    raw_changes = await anyio.to_thread.run_sync(watcher.watch, debounce, step, timeout, stop_event_)
                except (CancelledError, KeyboardInterrupt):
                    stop_event_.set()
                    # suppressing KeyboardInterrupt wouldn't stop it getting raised by the top level asyncio.run call
                tg.cancel_scope.cancel()
                # in theory the watch thread should never get a signal
                raise RuntimeError('watch thread unexpectedly received a signal')
def _prep_changes(
    raw_changes: Set[Tuple[int, str]], watch_filter: Optional[Callable[[Change, str], bool]]
) -> Set[FileChange]:
    # if we wanted to be really snazzy, we could move this into rust
    changes = {(Change(change), path) for change, path in raw_changes}
    if watch_filter:
        changes = {c for c in changes if watch_filter(c[0], c[1])}
    return changes
def _log_changes(changes: Set[FileChange]) -> None:
    if logger.isEnabledFor(logging.INFO):  # pragma: no branch
        count = len(changes)
        plural = '' if count == 1 else 's'
        if logger.isEnabledFor(logging.DEBUG):
            logger.debug('%d change%s detected: %s', count, plural, changes)
            logger.info('%d change%s detected', count, plural)
def _calc_async_timeout(timeout: Optional[int]) -> int:
    see https://github.com/samuelcolvin/watchfiles/issues/110
            return 1_000
            return 5_000
def _default_force_polling(force_polling: Optional[bool]) -> bool:
    See docstring for `watch` above for details.
    See samuelcolvin/watchfiles#167 and samuelcolvin/watchfiles#187 for discussion and rationale.
    if force_polling is not None:
        return force_polling
    env_var = os.getenv('WATCHFILES_FORCE_POLLING')
    if env_var:
        return env_var.lower() not in {'false', 'disable', 'disabled'}
        return _auto_force_polling()
def _default_poll_delay_ms(poll_delay_ms: int) -> int:
    env_var = os.getenv('WATCHFILES_POLL_DELAY_MS')
    if env_var and env_var.isdecimal():
        return int(env_var)
        return poll_delay_ms
def _default_debug(debug: Optional[bool]) -> bool:
    if debug is not None:
    env_var = os.getenv('WATCHFILES_DEBUG')
    return bool(env_var)
def _auto_force_polling() -> bool:
    Whether to auto-enable force polling, it should be enabled automatically only on WSL.
    See samuelcolvin/watchfiles#187 for discussion.
    uname = platform.uname()
    return 'microsoft-standard' in uname.release.lower() and uname.system.lower() == 'linux'
def _default_ignore_permission_denied(ignore_permission_denied: Optional[bool]) -> bool:
    if ignore_permission_denied is not None:
        return ignore_permission_denied
    env_var = os.getenv('WATCHFILES_IGNORE_PERMISSION_DENIED')
"""Logic for creating models."""
# Because `dict` is in the local namespace of the `BaseModel` class, we use `Dict` for annotations.
# TODO v3 fallback to `dict` when the deprecated `dict` method gets removed.
# ruff: noqa: UP035
from collections.abc import Generator, Mapping
from pydantic_core import PydanticUndefined, ValidationError
from typing_extensions import Self, TypeAlias, Unpack
from . import PydanticDeprecatedSince20, PydanticDeprecatedSince211
from ._internal import (
    _config,
    _decorators,
    _fields,
    _forward_ref,
    _generics,
    _mock_val_ser,
    _model_construction,
    _namespace_utils,
    _repr,
    _typing_extra,
    _utils,
from .aliases import AliasChoices, AliasPath
from .config import ConfigDict, ExtraValues
from .errors import PydanticUndefinedAnnotation, PydanticUserError
from .json_schema import DEFAULT_REF_TEMPLATE, GenerateJsonSchema, JsonSchemaMode, JsonSchemaValue, model_json_schema
from .plugin._schema_validator import PluggableSchemaValidator
    from inspect import Signature
    from pydantic_core import CoreSchema, SchemaSerializer, SchemaValidator
    from ._internal._namespace_utils import MappingNamespace
    from ._internal._utils import AbstractSetIntStr, MappingIntStrAny
    from .deprecated.parse import Protocol as DeprecatedParseProtocol
    from .fields import ComputedFieldInfo, FieldInfo, ModelPrivateAttr
__all__ = 'BaseModel', 'create_model'
# Keep these type aliases available at runtime:
TupleGenerator: TypeAlias = Generator[tuple[str, Any], None, None]
# NOTE: In reality, `bool` should be replaced by `Literal[True]` but mypy fails to correctly apply bidirectional
# type inference (e.g. when using `{'a': {'b': True}}`):
# NOTE: Keep this type alias in sync with the stub definition in `pydantic-core`:
IncEx: TypeAlias = Union[set[int], set[str], Mapping[int, Union['IncEx', bool]], Mapping[str, Union['IncEx', bool]]]
_object_setattr = _model_construction.object_setattr
def _check_frozen(model_cls: type[BaseModel], name: str, value: Any) -> None:
    if model_cls.model_config.get('frozen'):
        error_type = 'frozen_instance'
    elif getattr(model_cls.__pydantic_fields__.get(name), 'frozen', False):
        error_type = 'frozen_field'
    raise ValidationError.from_exception_data(
        model_cls.__name__, [{'type': error_type, 'loc': (name,), 'input': value}]
def _model_field_setattr_handler(model: BaseModel, name: str, val: Any) -> None:
    model.__dict__[name] = val
    model.__pydantic_fields_set__.add(name)
def _private_setattr_handler(model: BaseModel, name: str, val: Any) -> None:
    if getattr(model, '__pydantic_private__', None) is None:
        # While the attribute should be present at this point, this may not be the case if
        # users do unusual stuff with `model_post_init()` (which is where the  `__pydantic_private__`
        # is initialized, by wrapping the user-defined `model_post_init()`), e.g. if they mock
        # the `model_post_init()` call. Ideally we should find a better way to init private attrs.
        object.__setattr__(model, '__pydantic_private__', {})
    model.__pydantic_private__[name] = val  # pyright: ignore[reportOptionalSubscript]
_SIMPLE_SETATTR_HANDLERS: Mapping[str, Callable[[BaseModel, str, Any], None]] = {
    'model_field': _model_field_setattr_handler,
    'validate_assignment': lambda model, name, val: model.__pydantic_validator__.validate_assignment(model, name, val),  # pyright: ignore[reportAssignmentType]
    'private': _private_setattr_handler,
    'cached_property': lambda model, name, val: model.__dict__.__setitem__(name, val),
    'extra_known': lambda model, name, val: _object_setattr(model, name, val),
class BaseModel(metaclass=_model_construction.ModelMetaclass):
        [Models](../concepts/models.md)
    A base class for creating Pydantic models.
        __class_vars__: The names of the class variables defined on the model.
        __private_attributes__: Metadata about the private attributes of the model.
        __signature__: The synthesized `__init__` [`Signature`][inspect.Signature] of the model.
        __pydantic_complete__: Whether model building is completed, or if there are still undefined fields.
        __pydantic_core_schema__: The core schema of the model.
        __pydantic_custom_init__: Whether the model has a custom `__init__` function.
        __pydantic_decorators__: Metadata containing the decorators defined on the model.
            This replaces `Model.__validators__` and `Model.__root_validators__` from Pydantic V1.
        __pydantic_generic_metadata__: Metadata for generic models; contains data used for a similar purpose to
            __args__, __origin__, __parameters__ in typing-module generics. May eventually be replaced by these.
        __pydantic_parent_namespace__: Parent namespace of the model, used for automatic rebuilding of models.
        __pydantic_post_init__: The name of the post-init method for the model, if defined.
        __pydantic_root_model__: Whether the model is a [`RootModel`][pydantic.root_model.RootModel].
        __pydantic_serializer__: The `pydantic-core` `SchemaSerializer` used to dump instances of the model.
        __pydantic_validator__: The `pydantic-core` `SchemaValidator` used to validate instances of the model.
        __pydantic_fields__: A dictionary of field names and their corresponding [`FieldInfo`][pydantic.fields.FieldInfo] objects.
        __pydantic_computed_fields__: A dictionary of computed field names and their corresponding [`ComputedFieldInfo`][pydantic.fields.ComputedFieldInfo] objects.
        __pydantic_extra__: A dictionary containing extra values, if [`extra`][pydantic.config.ConfigDict.extra]
            is set to `'allow'`.
        __pydantic_fields_set__: The names of fields explicitly set during instantiation.
        __pydantic_private__: Values of private attributes set on the model instance.
    # Note: Many of the below class vars are defined in the metaclass, but we define them here for type checking purposes.
    model_config: ClassVar[ConfigDict] = ConfigDict()
    Configuration for the model, should be a dictionary conforming to [`ConfigDict`][pydantic.config.ConfigDict].
    __class_vars__: ClassVar[set[str]]
    """The names of the class variables defined on the model."""
    __private_attributes__: ClassVar[Dict[str, ModelPrivateAttr]]  # noqa: UP006
    """Metadata about the private attributes of the model."""
    __signature__: ClassVar[Signature]
    """The synthesized `__init__` [`Signature`][inspect.Signature] of the model."""
    __pydantic_complete__: ClassVar[bool] = False
    """Whether model building is completed, or if there are still undefined fields."""
    __pydantic_core_schema__: ClassVar[CoreSchema]
    """The core schema of the model."""
    __pydantic_custom_init__: ClassVar[bool]
    """Whether the model has a custom `__init__` method."""
    # Must be set for `GenerateSchema.model_schema` to work for a plain `BaseModel` annotation.
    __pydantic_decorators__: ClassVar[_decorators.DecoratorInfos] = _decorators.DecoratorInfos()
    """Metadata containing the decorators defined on the model.
    This replaces `Model.__validators__` and `Model.__root_validators__` from Pydantic V1."""
    __pydantic_generic_metadata__: ClassVar[_generics.PydanticGenericMetadata]
    """Metadata for generic models; contains data used for a similar purpose to
    __args__, __origin__, __parameters__ in typing-module generics. May eventually be replaced by these."""
    __pydantic_parent_namespace__: ClassVar[Dict[str, Any] | None] = None  # noqa: UP006
    """Parent namespace of the model, used for automatic rebuilding of models."""
    __pydantic_post_init__: ClassVar[None | Literal['model_post_init']]
    """The name of the post-init method for the model, if defined."""
    __pydantic_root_model__: ClassVar[bool] = False
    """Whether the model is a [`RootModel`][pydantic.root_model.RootModel]."""
    __pydantic_serializer__: ClassVar[SchemaSerializer]
    """The `pydantic-core` `SchemaSerializer` used to dump instances of the model."""
    __pydantic_validator__: ClassVar[SchemaValidator | PluggableSchemaValidator]
    """The `pydantic-core` `SchemaValidator` used to validate instances of the model."""
    __pydantic_fields__: ClassVar[Dict[str, FieldInfo]]  # noqa: UP006
    """A dictionary of field names and their corresponding [`FieldInfo`][pydantic.fields.FieldInfo] objects.
    This replaces `Model.__fields__` from Pydantic V1.
    __pydantic_setattr_handlers__: ClassVar[Dict[str, Callable[[BaseModel, str, Any], None]]]  # noqa: UP006
    """`__setattr__` handlers. Memoizing the handlers leads to a dramatic performance improvement in `__setattr__`"""
    __pydantic_computed_fields__: ClassVar[Dict[str, ComputedFieldInfo]]  # noqa: UP006
    """A dictionary of computed field names and their corresponding [`ComputedFieldInfo`][pydantic.fields.ComputedFieldInfo] objects."""
    __pydantic_extra__: Dict[str, Any] | None = _model_construction.NoInitField(init=False)  # noqa: UP006
    """A dictionary containing extra values, if [`extra`][pydantic.config.ConfigDict.extra] is set to `'allow'`."""
    __pydantic_fields_set__: set[str] = _model_construction.NoInitField(init=False)
    """The names of fields explicitly set during instantiation."""
    __pydantic_private__: Dict[str, Any] | None = _model_construction.NoInitField(init=False)  # noqa: UP006
    """Values of private attributes set on the model instance."""
        # Prevent `BaseModel` from being instantiated directly
        # (defined in an `if not TYPE_CHECKING` block for clarity and to avoid type checking errors):
        __pydantic_core_schema__ = _mock_val_ser.MockCoreSchema(
            'Pydantic models should inherit from BaseModel, BaseModel cannot be instantiated directly',
            code='base-model-instantiated',
        __pydantic_validator__ = _mock_val_ser.MockValSer(
            val_or_ser='validator',
        __pydantic_serializer__ = _mock_val_ser.MockValSer(
            val_or_ser='serializer',
    __slots__ = '__dict__', '__pydantic_fields_set__', '__pydantic_extra__', '__pydantic_private__'
    def __init__(self, /, **data: Any) -> None:
        """Create a new model by parsing and validating input data from keyword arguments.
        Raises [`ValidationError`][pydantic_core.ValidationError] if the input data cannot be
        validated to form a valid model.
        `self` is explicitly positional-only to allow `self` as a field name.
        # `__tracebackhide__` tells pytest and some other tools to omit this function from tracebacks
        validated_self = self.__pydantic_validator__.validate_python(data, self_instance=self)
        if self is not validated_self:
                'A custom validator is returning a value other than `self`.\n'
                "Returning anything other than `self` from a top level model validator isn't supported when validating via `__init__`.\n"
                'See the `model_validator` docs (https://docs.pydantic.dev/latest/concepts/validators/#model-validators) for more details.',
    # The following line sets a flag that we use to determine when `__init__` gets overridden by the user
    __init__.__pydantic_base_init__ = True  # pyright: ignore[reportFunctionMemberAccess]
    @_utils.deprecated_instance_property
    def model_fields(cls) -> dict[str, FieldInfo]:
        """A mapping of field names to their respective [`FieldInfo`][pydantic.fields.FieldInfo] instances.
            Accessing this attribute from a model instance is deprecated, and will not work in Pydantic V3.
            Instead, you should access this attribute from the model class.
        return getattr(cls, '__pydantic_fields__', {})
    def model_computed_fields(cls) -> dict[str, ComputedFieldInfo]:
        """A mapping of computed field names to their respective [`ComputedFieldInfo`][pydantic.fields.ComputedFieldInfo] instances.
        return getattr(cls, '__pydantic_computed_fields__', {})
    def model_extra(self) -> dict[str, Any] | None:
        """Get extra fields set during validation.
            A dictionary of extra fields, or `None` if `config.extra` is not set to `"allow"`.
        return self.__pydantic_extra__
        """Returns the set of fields that have been explicitly set on this model instance.
            A set of strings representing the fields that have been set,
                i.e. that were not filled from defaults.
        return self.__pydantic_fields_set__
    def model_construct(cls, _fields_set: set[str] | None = None, **values: Any) -> Self:  # noqa: C901
        """Creates a new instance of the `Model` class with validated data.
        Creates a new model setting `__dict__` and `__pydantic_fields_set__` from trusted or pre-validated data.
        Default values are respected, but no other validation is performed.
            `model_construct()` generally respects the `model_config.extra` setting on the provided model.
            That is, if `model_config.extra == 'allow'`, then all extra passed values are added to the model instance's `__dict__`
            and `__pydantic_extra__` fields. If `model_config.extra == 'ignore'` (the default), then all extra passed values are ignored.
            Because no validation is performed with a call to `model_construct()`, having `model_config.extra == 'forbid'` does not result in
            an error if extra values are passed, but they will be ignored.
            _fields_set: A set of field names that were originally explicitly set during instantiation. If provided,
                this is directly used for the [`model_fields_set`][pydantic.BaseModel.model_fields_set] attribute.
                Otherwise, the field names from the `values` argument will be used.
            values: Trusted or pre-validated data dictionary.
            A new instance of the `Model` class with validated data.
        m = cls.__new__(cls)
        fields_values: dict[str, Any] = {}
        fields_set = set()
        for name, field in cls.__pydantic_fields__.items():
            if field.alias is not None and field.alias in values:
                fields_values[name] = values.pop(field.alias)
                fields_set.add(name)
            if (name not in fields_set) and (field.validation_alias is not None):
                validation_aliases: list[str | AliasPath] = (
                    field.validation_alias.choices
                    if isinstance(field.validation_alias, AliasChoices)
                    else [field.validation_alias]
                for alias in validation_aliases:
                    if isinstance(alias, str) and alias in values:
                        fields_values[name] = values.pop(alias)
                    elif isinstance(alias, AliasPath):
                        value = alias.search_dict_for_path(values)
                        if value is not PydanticUndefined:
                            fields_values[name] = value
            if name not in fields_set:
                if name in values:
                    fields_values[name] = values.pop(name)
                elif not field.is_required():
                    fields_values[name] = field.get_default(call_default_factory=True, validated_data=fields_values)
            _fields_set = fields_set
        _extra: dict[str, Any] | None = values if cls.model_config.get('extra') == 'allow' else None
        _object_setattr(m, '__dict__', fields_values)
        _object_setattr(m, '__pydantic_fields_set__', _fields_set)
        if not cls.__pydantic_root_model__:
            _object_setattr(m, '__pydantic_extra__', _extra)
        if cls.__pydantic_post_init__:
            m.model_post_init(None)
            # update private attributes with values set
            if hasattr(m, '__pydantic_private__') and m.__pydantic_private__ is not None:
                for k, v in values.items():
                    if k in m.__private_attributes__:
                        m.__pydantic_private__[k] = v
        elif not cls.__pydantic_root_model__:
            # Note: if there are any private attributes, cls.__pydantic_post_init__ would exist
            # Since it doesn't, that means that `__pydantic_private__` should be set to None
            _object_setattr(m, '__pydantic_private__', None)
    def model_copy(self, *, update: Mapping[str, Any] | None = None, deep: bool = False) -> Self:
            [`model_copy`](../concepts/models.md#model-copy)
        Returns a copy of the model.
            The underlying instance's [`__dict__`][object.__dict__] attribute is copied. This
            might have unexpected side effects if you store anything in it, on top of the model
            fields (e.g. the value of [cached properties][functools.cached_property]).
            update: Values to change/add in the new model. Note: the data is not validated
                before creating the new model. You should trust this data.
            deep: Set to `True` to make a deep copy of the model.
            New model instance.
        copied = self.__deepcopy__() if deep else self.__copy__()
        if update:
            if self.model_config.get('extra') == 'allow':
                for k, v in update.items():
                    if k in self.__pydantic_fields__:
                        copied.__dict__[k] = v
                        if copied.__pydantic_extra__ is None:
                            copied.__pydantic_extra__ = {}
                        copied.__pydantic_extra__[k] = v
                copied.__dict__.update(update)
            copied.__pydantic_fields_set__.update(update.keys())
        return copied
        mode: Literal['json', 'python'] | str = 'python',
        warnings: bool | Literal['none', 'warn', 'error'] = True,
            [`model_dump`](../concepts/serialization.md#python-mode)
        return self.__pydantic_serializer__.to_python(
            by_alias=by_alias,
            exclude_computed_fields=exclude_computed_fields,
            round_trip=round_trip,
            fallback=fallback,
            serialize_as_any=serialize_as_any,
            [`model_dump_json`](../concepts/serialization.md#json-mode)
            ensure_ascii: If `True`, the output is guaranteed to have all incoming non-ASCII characters escaped.
                If `False` (the default), these characters will be output as-is.
            include: Field(s) to include in the JSON output.
            exclude: Field(s) to exclude from the JSON output.
        return self.__pydantic_serializer__.to_json(
    def model_json_schema(
        by_alias: bool = True,
        ref_template: str = DEFAULT_REF_TEMPLATE,
        schema_generator: type[GenerateJsonSchema] = GenerateJsonSchema,
        mode: JsonSchemaMode = 'validation',
        union_format: Literal['any_of', 'primitive_type_array'] = 'any_of',
        """Generates a JSON schema for a model class.
            by_alias: Whether to use attribute aliases or not.
            ref_template: The reference template.
            union_format: The format to use when combining schemas from unions together. Can be one of:
                - `'any_of'`: Use the [`anyOf`](https://json-schema.org/understanding-json-schema/reference/combining#anyOf)
                keyword to combine schemas (the default).
                - `'primitive_type_array'`: Use the [`type`](https://json-schema.org/understanding-json-schema/reference/type)
                keyword as an array of strings, containing each type of the combination. If any of the schemas is not a primitive
                type (`string`, `boolean`, `null`, `integer` or `number`) or contains constraints/metadata, falls back to
                `any_of`.
            schema_generator: To override the logic used to generate the JSON schema, as a subclass of
                `GenerateJsonSchema` with your desired modifications
            mode: The mode in which to generate the schema.
            The JSON schema for the given model class.
        return model_json_schema(
            ref_template=ref_template,
            union_format=union_format,
            schema_generator=schema_generator,
    def model_parametrized_name(cls, params: tuple[type[Any], ...]) -> str:
        """Compute the class name for parametrizations of generic classes.
        This method can be overridden to achieve a custom naming scheme for generic BaseModels.
            params: Tuple of types of the class. Given a generic class
                `Model` with 2 type variables and a concrete model `Model[str, int]`,
                the value `(str, int)` would be passed to `params`.
            String representing the new class where `params` are passed to `cls` as type variables.
            TypeError: Raised when trying to generate concrete names for non-generic models.
        if not issubclass(cls, Generic):
            raise TypeError('Concrete names should only be generated for generic models.')
        # Any strings received should represent forward references, so we handle them specially below.
        # If we eventually move toward wrapping them in a ForwardRef in __class_getitem__ in the future,
        # we may be able to remove this special case.
        param_names = [param if isinstance(param, str) else _repr.display_as_type(param) for param in params]
        params_component = ', '.join(param_names)
        return f'{cls.__name__}[{params_component}]'
    def model_post_init(self, context: Any, /) -> None:
        """Override this method to perform additional initialization after `__init__` and `model_construct`.
        This is useful if you want to do some validation that requires the entire model to be initialized.
    def model_rebuild(
        force: bool = False,
        raise_errors: bool = True,
        _parent_namespace_depth: int = 2,
        _types_namespace: MappingNamespace | None = None,
        """Try to rebuild the pydantic-core schema for the model.
        This may be necessary when one of the annotations is a ForwardRef which could not be resolved during
        the initial attempt to build the schema, and automatic rebuilding fails.
            force: Whether to force the rebuilding of the model schema, defaults to `False`.
            raise_errors: Whether to raise errors, defaults to `True`.
            _parent_namespace_depth: The depth level of the parent namespace, defaults to 2.
            _types_namespace: The types namespace, defaults to `None`.
            Returns `None` if the schema is already "complete" and rebuilding was not required.
            If rebuilding _was_ required, returns `True` if rebuilding was successful, otherwise `False`.
        already_complete = cls.__pydantic_complete__
        if already_complete and not force:
        cls.__pydantic_complete__ = False
        for attr in ('__pydantic_core_schema__', '__pydantic_validator__', '__pydantic_serializer__'):
            if attr in cls.__dict__ and not isinstance(getattr(cls, attr), _mock_val_ser.MockValSer):
                # Deleting the validator/serializer is necessary as otherwise they can get reused in
                # pydantic-core. We do so only if they aren't mock instances, otherwise — as `model_rebuild()`
                # isn't thread-safe — concurrent model instantiations can lead to the parent validator being used.
                # Same applies for the core schema that can be reused in schema generation.
                delattr(cls, attr)
        if _types_namespace is not None:
            rebuild_ns = _types_namespace
        elif _parent_namespace_depth > 0:
            rebuild_ns = _typing_extra.parent_frame_namespace(parent_depth=_parent_namespace_depth, force=True) or {}
            rebuild_ns = {}
        parent_ns = _model_construction.unpack_lenient_weakvaluedict(cls.__pydantic_parent_namespace__) or {}
        ns_resolver = _namespace_utils.NsResolver(
            parent_namespace={**rebuild_ns, **parent_ns},
        return _model_construction.complete_model_class(
            _config.ConfigWrapper(cls.model_config, check=False),
            ns_resolver,
            raise_errors=raise_errors,
            # If the model was already complete, we don't need to call the hook again.
            call_on_complete_hook=not already_complete,
    def model_validate(
        extra: ExtraValues | None = None,
        from_attributes: bool | None = None,
        by_name: bool | None = None,
        """Validate a pydantic model instance.
            obj: The object to validate.
            strict: Whether to enforce types strictly.
            extra: Whether to ignore, allow, or forbid extra data during model validation.
                See the [`extra` configuration value][pydantic.ConfigDict.extra] for details.
            from_attributes: Whether to extract data from object attributes.
            context: Additional context to pass to the validator.
            by_alias: Whether to use the field's alias when validating against the provided input data.
            by_name: Whether to use the field's name when validating against the provided input data.
            ValidationError: If the object could not be validated.
            The validated model instance.
        if by_alias is False and by_name is not True:
                'At least one of `by_alias` or `by_name` must be set to True.',
                code='validate-by-alias-and-name-false',
        return cls.__pydantic_validator__.validate_python(
            from_attributes=from_attributes,
            by_name=by_name,
    def model_validate_json(
        json_data: str | bytes | bytearray,
            [JSON Parsing](../concepts/json.md#json-parsing)
        Validate the given JSON data against the Pydantic model.
            json_data: The JSON data to validate.
            context: Extra variables to pass to the validator.
            The validated Pydantic model.
            ValidationError: If `json_data` is not a JSON string or the object could not be validated.
        return cls.__pydantic_validator__.validate_json(
            json_data, strict=strict, extra=extra, context=context, by_alias=by_alias, by_name=by_name
    def model_validate_strings(
        """Validate the given object with string data against the Pydantic model.
            obj: The object containing string data to validate.
        return cls.__pydantic_validator__.validate_strings(
            obj, strict=strict, extra=extra, context=context, by_alias=by_alias, by_name=by_name
    def __get_pydantic_core_schema__(cls, source: type[BaseModel], handler: GetCoreSchemaHandler, /) -> CoreSchema:
        # This warning is only emitted when calling `super().__get_pydantic_core_schema__` from a model subclass.
        # In the generate schema logic, this method (`BaseModel.__get_pydantic_core_schema__`) is special cased to
        # *not* be called if not overridden.
            'The `__get_pydantic_core_schema__` method of the `BaseModel` class is deprecated. If you are calling '
            '`super().__get_pydantic_core_schema__` when overriding the method on a Pydantic model, consider using '
            '`handler(source)` instead. However, note that overriding this method on models can lead to unexpected '
            'side effects.',
        # Logic copied over from `GenerateSchema._model_schema`:
        schema = cls.__dict__.get('__pydantic_core_schema__')
        if schema is not None and not isinstance(schema, _mock_val_ser.MockCoreSchema):
            return cls.__pydantic_core_schema__
        return handler(source)
        core_schema: CoreSchema,
        handler: GetJsonSchemaHandler,
    ) -> JsonSchemaValue:
        """Hook into generating the model's JSON schema.
            core_schema: A `pydantic-core` CoreSchema.
                You can ignore this argument and call the handler with a new CoreSchema,
                wrap this CoreSchema (`{'type': 'nullable', 'schema': current_schema}`),
                or just call the handler with the original schema.
            handler: Call into Pydantic's internal JSON schema generation.
                This will raise a `pydantic.errors.PydanticInvalidForJsonSchema` if JSON schema
                generation fails.
                Since this gets called by `BaseModel.model_json_schema` you can override the
                `schema_generator` argument to that function to change JSON schema generation globally
                for a type.
            A JSON schema, as a Python object.
        return handler(core_schema)
    def __pydantic_init_subclass__(cls, **kwargs: Any) -> None:
        """This is intended to behave just like `__init_subclass__`, but is called by `ModelMetaclass`
        only after basic class initialization is complete. In particular, attributes like `model_fields` will
        be present when this is called, but forward annotations are not guaranteed to be resolved yet,
        meaning that creating an instance of the class may fail.
        This is necessary because `__init_subclass__` will always be called by `type.__new__`,
        and it would require a prohibitively large refactor to the `ModelMetaclass` to ensure that
        `type.__new__` was called in such a manner that the class would already be sufficiently initialized.
        This will receive the same `kwargs` that would be passed to the standard `__init_subclass__`, namely,
        any kwargs passed to the class definition that aren't used internally by Pydantic.
            **kwargs: Any keyword arguments passed to the class definition that aren't used internally
                by Pydantic.
            You may want to override [`__pydantic_on_complete__()`][pydantic.main.BaseModel.__pydantic_on_complete__]
            instead, which is called once the class and its fields are fully initialized and ready for validation.
    def __pydantic_on_complete__(cls) -> None:
        """This is called once the class and its fields are fully initialized and ready to be used.
        This typically happens when the class is created (just before
        [`__pydantic_init_subclass__()`][pydantic.main.BaseModel.__pydantic_init_subclass__] is called on the superclass),
        except when forward annotations are used that could not immediately be resolved.
        In that case, it will be called later, when the model is rebuilt automatically or explicitly using
        [`model_rebuild()`][pydantic.main.BaseModel.model_rebuild].
    def __class_getitem__(
        cls, typevar_values: type[Any] | tuple[type[Any], ...]
    ) -> type[BaseModel] | _forward_ref.PydanticRecursiveRef:
        cached = _generics.get_cached_generic_type_early(cls, typevar_values)
        if cls is BaseModel:
            raise TypeError('Type parameters should be placed on typing.Generic, not BaseModel')
        if not hasattr(cls, '__parameters__'):
            raise TypeError(f'{cls} cannot be parametrized because it does not inherit from typing.Generic')
        if not cls.__pydantic_generic_metadata__['parameters'] and Generic not in cls.__bases__:
            raise TypeError(f'{cls} is not a generic class')
        if not isinstance(typevar_values, tuple):
            typevar_values = (typevar_values,)
        # For a model `class Model[T, U, V = int](BaseModel): ...` parametrized with `(str, bool)`,
        # this gives us `{T: str, U: bool, V: int}`:
        typevars_map = _generics.map_generic_model_arguments(cls, typevar_values)
        # We also update the provided args to use defaults values (`(str, bool)` becomes `(str, bool, int)`):
        typevar_values = tuple(v for v in typevars_map.values())
        if _utils.all_identical(typevars_map.keys(), typevars_map.values()) and typevars_map:
            submodel = cls  # if arguments are equal to parameters it's the same object
            _generics.set_cached_generic_type(cls, typevar_values, submodel)
            parent_args = cls.__pydantic_generic_metadata__['args']
            if not parent_args:
                args = typevar_values
                args = tuple(_generics.replace_types(arg, typevars_map) for arg in parent_args)
            origin = cls.__pydantic_generic_metadata__['origin'] or cls
            model_name = origin.model_parametrized_name(args)
            params = tuple(
                dict.fromkeys(_generics.iter_contained_typevars(typevars_map.values()))
            )  # use dict as ordered set
            with _generics.generic_recursion_self_type(origin, args) as maybe_self_type:
                cached = _generics.get_cached_generic_type_late(cls, typevar_values, origin, args)
                if maybe_self_type is not None:
                    return maybe_self_type
                # Attempt to rebuild the origin in case new types have been defined
                    # depth 2 gets you above this __class_getitem__ call.
                    # Note that we explicitly provide the parent ns, otherwise
                    # `model_rebuild` will use the parent ns no matter if it is the ns of a module.
                    # We don't want this here, as this has unexpected effects when a model
                    # is being parametrized during a forward annotation evaluation.
                    parent_ns = _typing_extra.parent_frame_namespace(parent_depth=2) or {}
                    origin.model_rebuild(_types_namespace=parent_ns)
                except PydanticUndefinedAnnotation:
                    # It's okay if it fails, it just means there are still undefined types
                    # that could be evaluated later.
                submodel = _generics.create_generic_submodel(model_name, origin, args, params)
                _generics.set_cached_generic_type(cls, typevar_values, submodel, origin, args)
        return submodel
    def __copy__(self) -> Self:
        """Returns a shallow copy of the model."""
        _object_setattr(m, '__dict__', copy(self.__dict__))
        _object_setattr(m, '__pydantic_extra__', copy(self.__pydantic_extra__))
        _object_setattr(m, '__pydantic_fields_set__', copy(self.__pydantic_fields_set__))
        if not hasattr(self, '__pydantic_private__') or self.__pydantic_private__ is None:
            _object_setattr(
                m,
                '__pydantic_private__',
                {k: v for k, v in self.__pydantic_private__.items() if v is not PydanticUndefined},
    def __deepcopy__(self, memo: dict[int, Any] | None = None) -> Self:
        """Returns a deep copy of the model."""
        _object_setattr(m, '__dict__', deepcopy(self.__dict__, memo=memo))
        _object_setattr(m, '__pydantic_extra__', deepcopy(self.__pydantic_extra__, memo=memo))
        # This next line doesn't need a deepcopy because __pydantic_fields_set__ is a set[str],
        # and attempting a deepcopy would be marginally slower.
                deepcopy({k: v for k, v in self.__pydantic_private__.items() if v is not PydanticUndefined}, memo=memo),
        # We put `__getattr__` in a non-TYPE_CHECKING block because otherwise, mypy allows arbitrary attribute access
        # The same goes for __setattr__ and __delattr__, see: https://github.com/pydantic/pydantic/issues/8643
        def __getattr__(self, item: str) -> Any:
            private_attributes = object.__getattribute__(self, '__private_attributes__')
            if item in private_attributes:
                attribute = private_attributes[item]
                if hasattr(attribute, '__get__'):
                    return attribute.__get__(self, type(self))  # type: ignore
                    # Note: self.__pydantic_private__ cannot be None if self.__private_attributes__ has items
                    return self.__pydantic_private__[item]  # type: ignore
                    raise AttributeError(f'{type(self).__name__!r} object has no attribute {item!r}') from exc
                # `__pydantic_extra__` can fail to be set if the model is not yet fully initialized.
                # See `BaseModel.__repr_args__` for more details
                    pydantic_extra = object.__getattribute__(self, '__pydantic_extra__')
                    pydantic_extra = None
                if pydantic_extra and item in pydantic_extra:
                    return pydantic_extra[item]
                    if hasattr(self.__class__, item):
                        return super().__getattribute__(item)  # Raises AttributeError if appropriate
                        # this is the current error
                        raise AttributeError(f'{type(self).__name__!r} object has no attribute {item!r}')
        def __setattr__(self, name: str, value: Any) -> None:
            if (setattr_handler := self.__pydantic_setattr_handlers__.get(name)) is not None:
                setattr_handler(self, name, value)
            # if None is returned from _setattr_handler, the attribute was set directly
            elif (setattr_handler := self._setattr_handler(name, value)) is not None:
                setattr_handler(self, name, value)  # call here to not memo on possibly unknown fields
                self.__pydantic_setattr_handlers__[name] = setattr_handler  # memoize the handler for faster access
        def _setattr_handler(self, name: str, value: Any) -> Callable[[BaseModel, str, Any], None] | None:
            """Get a handler for setting an attribute on the model instance.
                A handler for setting an attribute on the model instance. Used for memoization of the handler.
                Memoizing the handlers leads to a dramatic performance improvement in `__setattr__`
                Returns `None` when memoization is not safe, then the attribute is set directly.
            if name in cls.__class_vars__:
                    f'{name!r} is a ClassVar of `{cls.__name__}` and cannot be set on an instance. '
                    f'If you want to set a value on the class, use `{cls.__name__}.{name} = value`.'
            elif not _fields.is_valid_field_name(name):
                if (attribute := cls.__private_attributes__.get(name)) is not None:
                    if hasattr(attribute, '__set__'):
                        return lambda model, _name, val: attribute.__set__(model, val)
                        return _SIMPLE_SETATTR_HANDLERS['private']
                    _object_setattr(self, name, value)
                    return None  # Can not return memoized handler with possibly freeform attr names
            attr = getattr(cls, name, None)
            # NOTE: We currently special case properties and `cached_property`, but we might need
            # to generalize this to all data/non-data descriptors at some point. For non-data descriptors
            # (such as `cached_property`), it isn't obvious though. `cached_property` caches the value
            # to the instance's `__dict__`, but other non-data descriptors might do things differently.
            if isinstance(attr, cached_property):
                return _SIMPLE_SETATTR_HANDLERS['cached_property']
            _check_frozen(cls, name, value)
            # We allow properties to be set only on non frozen models for now (to match dataclasses).
            # This can be changed if it ever gets requested.
            if isinstance(attr, property):
                return lambda model, _name, val: attr.__set__(model, val)
            elif cls.model_config.get('validate_assignment'):
                return _SIMPLE_SETATTR_HANDLERS['validate_assignment']
            elif name not in cls.__pydantic_fields__:
                if cls.model_config.get('extra') != 'allow':
                    # TODO - matching error
                    raise ValueError(f'"{cls.__name__}" object has no field "{name}"')
                    # attribute does not exist, so put it in extra
                    self.__pydantic_extra__[name] = value
                    # attribute _does_ exist, and was not in extra, so update it
                    return _SIMPLE_SETATTR_HANDLERS['extra_known']
                return _SIMPLE_SETATTR_HANDLERS['model_field']
        def __delattr__(self, item: str) -> Any:
            if item in self.__private_attributes__:
                attribute = self.__private_attributes__[item]
                if hasattr(attribute, '__delete__'):
                    attribute.__delete__(self)  # type: ignore
                    del self.__pydantic_private__[item]  # type: ignore
                    raise AttributeError(f'{cls.__name__!r} object has no attribute {item!r}') from exc
            # Allow cached properties to be deleted (even if the class is frozen):
            attr = getattr(cls, item, None)
                return object.__delattr__(self, item)
            _check_frozen(cls, name=item, value=None)
            if item in self.__pydantic_fields__:
                object.__delattr__(self, item)
            elif self.__pydantic_extra__ is not None and item in self.__pydantic_extra__:
                del self.__pydantic_extra__[item]
        # Because we make use of `@dataclass_transform()`, `__replace__` is already synthesized by
        # type checkers, so we define the implementation in this `if not TYPE_CHECKING:` block:
        def __replace__(self, **changes: Any) -> Self:
            return self.model_copy(update=changes)
    def __getstate__(self) -> dict[Any, Any]:
        private = self.__pydantic_private__
            private = {k: v for k, v in private.items() if v is not PydanticUndefined}
            '__dict__': self.__dict__,
            '__pydantic_extra__': self.__pydantic_extra__,
            '__pydantic_fields_set__': self.__pydantic_fields_set__,
            '__pydantic_private__': private,
    def __setstate__(self, state: dict[Any, Any]) -> None:
        _object_setattr(self, '__pydantic_fields_set__', state.get('__pydantic_fields_set__', {}))
        _object_setattr(self, '__pydantic_extra__', state.get('__pydantic_extra__', {}))
        _object_setattr(self, '__pydantic_private__', state.get('__pydantic_private__', {}))
        _object_setattr(self, '__dict__', state.get('__dict__', {}))
        def __eq__(self, other: Any) -> bool:
            if isinstance(other, BaseModel):
                # When comparing instances of generic types for equality, as long as all field values are equal,
                # only require their generic origin types to be equal, rather than exact type equality.
                # This prevents headaches like MyGeneric(x=1) != MyGeneric[Any](x=1).
                self_type = self.__pydantic_generic_metadata__['origin'] or self.__class__
                other_type = other.__pydantic_generic_metadata__['origin'] or other.__class__
                # Perform common checks first
                    self_type == other_type
                    and getattr(self, '__pydantic_private__', None) == getattr(other, '__pydantic_private__', None)
                    and self.__pydantic_extra__ == other.__pydantic_extra__
                # We only want to compare pydantic fields but ignoring fields is costly.
                # We'll perform a fast check first, and fallback only when needed
                # See GH-7444 and GH-7825 for rationale and a performance benchmark
                # First, do the fast (and sometimes faulty) __dict__ comparison
                if self.__dict__ == other.__dict__:
                    # If the check above passes, then pydantic fields are equal, we can return early
                # We don't want to trigger unnecessary costly filtering of __dict__ on all unequal objects, so we return
                # early if there are no keys to ignore (we would just return False later on anyway)
                model_fields = type(self).__pydantic_fields__.keys()
                if self.__dict__.keys() <= model_fields and other.__dict__.keys() <= model_fields:
                # If we reach here, there are non-pydantic-fields keys, mapped to unequal values, that we need to ignore
                # Resort to costly filtering of the __dict__ objects
                # We use operator.itemgetter because it is much faster than dict comprehensions
                # NOTE: Contrary to standard python class and instances, when the Model class has a default value for an
                # attribute and the model instance doesn't have a corresponding attribute, accessing the missing attribute
                # raises an error in BaseModel.__getattr__ instead of returning the class attribute
                # So we can use operator.itemgetter() instead of operator.attrgetter()
                getter = operator.itemgetter(*model_fields) if model_fields else lambda _: _utils._SENTINEL
                    return getter(self.__dict__) == getter(other.__dict__)
                    # In rare cases (such as when using the deprecated BaseModel.copy() method),
                    # the __dict__ may not contain all model fields, which is how we can get here.
                    # getter(self.__dict__) is much faster than any 'safe' method that accounts
                    # for missing keys, and wrapping it in a `try` doesn't slow things down much
                    # in the common case.
                    self_fields_proxy = _utils.SafeGetItemProxy(self.__dict__)
                    other_fields_proxy = _utils.SafeGetItemProxy(other.__dict__)
                    return getter(self_fields_proxy) == getter(other_fields_proxy)
            # other instance is not a BaseModel
                return NotImplemented  # delegate to the other item in the comparison
        # We put `__init_subclass__` in a TYPE_CHECKING block because, even though we want the type-checking benefits
        # described in the signature of `__init_subclass__` below, we don't want to modify the default behavior of
        # subclass initialization.
        def __init_subclass__(cls, **kwargs: Unpack[ConfigDict]):
            """This signature is included purely to help type-checkers check arguments to class declaration, which
            provides a way to conveniently set model_config key/value pairs.
            class MyModel(BaseModel, extra='allow'): ...
            However, this may be deceiving, since the _actual_ calls to `__init_subclass__` will not receive any
            of the config arguments, and will only receive any keyword arguments passed during class initialization
            that are _not_ expected keys in ConfigDict. (This is due to the way `ModelMetaclass.__new__` works.)
                **kwargs: Keyword arguments passed to the class definition, which set model_config
                You may want to override `__pydantic_init_subclass__` instead, which behaves similarly but is called
                *after* the class is fully initialized.
    def __iter__(self) -> TupleGenerator:
        """So `dict(model)` works."""
        yield from [(k, v) for (k, v) in self.__dict__.items() if not k.startswith('_')]
        extra = self.__pydantic_extra__
        if extra:
            yield from extra.items()
        return f'{self.__repr_name__()}({self.__repr_str__(", ")})'
    def __repr_args__(self) -> _repr.ReprArgs:
        # Eagerly create the repr of computed fields, as this may trigger access of cached properties and as such
        # modify the instance's `__dict__`. If we don't do it now, it could happen when iterating over the `__dict__`
        # below if the instance happens to be referenced in a field, and would modify the `__dict__` size *during* iteration.
        computed_fields_repr_args = [
            (k, getattr(self, k)) for k, v in self.__pydantic_computed_fields__.items() if v.repr
            field = self.__pydantic_fields__.get(k)
            if field and field.repr:
                if v is not self:
                    yield k, v
                    yield k, self.__repr_recursion__(v)
        # This can happen if a `ValidationError` is raised during initialization and the instance's
        # repr is generated as part of the exception handling. Therefore, we use `getattr` here
        # with a fallback, even though the type hints indicate the attribute will always be present.
        if pydantic_extra is not None:
            yield from ((k, v) for k, v in pydantic_extra.items())
        yield from computed_fields_repr_args
    # take logic from `_repr.Representation` without the side effects of inheritance, see #5740
    __repr_name__ = _repr.Representation.__repr_name__
    __repr_recursion__ = _repr.Representation.__repr_recursion__
    __repr_str__ = _repr.Representation.__repr_str__
    __pretty__ = _repr.Representation.__pretty__
    __rich_repr__ = _repr.Representation.__rich_repr__
        return self.__repr_str__(' ')
    # ##### Deprecated methods from v1 #####
    @typing_extensions.deprecated(
        'The `__fields__` attribute is deprecated, use the `model_fields` class property instead.', category=None
    def __fields__(self) -> dict[str, FieldInfo]:
            'The `__fields__` attribute is deprecated, use the `model_fields` class property instead.',
            category=PydanticDeprecatedSince20,
        return getattr(type(self), '__pydantic_fields__', {})
        'The `__fields_set__` attribute is deprecated, use `model_fields_set` instead.',
    def __fields_set__(self) -> set[str]:
    @typing_extensions.deprecated('The `dict` method is deprecated; use `model_dump` instead.', category=None)
    def dict(  # noqa: D102
        by_alias: bool = False,
    ) -> Dict[str, Any]:  # noqa UP006
            'The `dict` method is deprecated; use `model_dump` instead.',
    @typing_extensions.deprecated('The `json` method is deprecated; use `model_dump_json` instead.', category=None)
    def json(  # noqa: D102
        encoder: Callable[[Any], Any] | None = PydanticUndefined,  # type: ignore[assignment]
        models_as_dict: bool = PydanticUndefined,  # type: ignore[assignment]
        **dumps_kwargs: Any,
            'The `json` method is deprecated; use `model_dump_json` instead.',
        if encoder is not PydanticUndefined:
            raise TypeError('The `encoder` argument is no longer supported; use field serializers instead.')
        if models_as_dict is not PydanticUndefined:
            raise TypeError('The `models_as_dict` argument is no longer supported; use a model serializer instead.')
        if dumps_kwargs:
            raise TypeError('`dumps_kwargs` keyword arguments are no longer supported.')
    @typing_extensions.deprecated('The `parse_obj` method is deprecated; use `model_validate` instead.', category=None)
    def parse_obj(cls, obj: Any) -> Self:  # noqa: D102
            'The `parse_obj` method is deprecated; use `model_validate` instead.',
        return cls.model_validate(obj)
        'The `parse_raw` method is deprecated; if your data is JSON use `model_validate_json`, '
        'otherwise load the data then use `model_validate` instead.',
    def parse_raw(  # noqa: D102
        b: str | bytes,
        content_type: str | None = None,
        encoding: str = 'utf8',
        proto: DeprecatedParseProtocol | None = None,
        allow_pickle: bool = False,
    ) -> Self:  # pragma: no cover
        from .deprecated import parse
            obj = parse.load_str_bytes(
                b,
                proto=proto,
                allow_pickle=allow_pickle,
        except (ValueError, TypeError) as exc:
            # try to match V1
            if isinstance(exc, UnicodeDecodeError):
                type_str = 'value_error.unicodedecode'
            elif isinstance(exc, json.JSONDecodeError):
                type_str = 'value_error.jsondecode'
            elif isinstance(exc, ValueError):
                type_str = 'value_error'
                type_str = 'type_error'
            # ctx is missing here, but since we've added `input` to the error, we're not pretending it's the same
            error: pydantic_core.InitErrorDetails = {
                # The type: ignore on the next line is to ignore the requirement of LiteralString
                'type': pydantic_core.PydanticCustomError(type_str, str(exc)),  # type: ignore
                'loc': ('__root__',),
                'input': b,
            raise pydantic_core.ValidationError.from_exception_data(cls.__name__, [error])
        'The `parse_file` method is deprecated; load the data from file, then if your data is JSON '
        'use `model_validate_json`, otherwise `model_validate` instead.',
    def parse_file(  # noqa: D102
        path: str | Path,
        obj = parse.load_file(
        return cls.parse_obj(obj)
        'The `from_orm` method is deprecated; set '
        "`model_config['from_attributes']=True` and use `model_validate` instead.",
    def from_orm(cls, obj: Any) -> Self:  # noqa: D102
        if not cls.model_config.get('from_attributes', None):
                'You must set the config attribute `from_attributes=True` to use from_orm', code=None
    @typing_extensions.deprecated('The `construct` method is deprecated; use `model_construct` instead.', category=None)
    def construct(cls, _fields_set: set[str] | None = None, **values: Any) -> Self:  # noqa: D102
            'The `construct` method is deprecated; use `model_construct` instead.',
        return cls.model_construct(_fields_set=_fields_set, **values)
        'The `copy` method is deprecated; use `model_copy` instead. '
        'See the docstring of `BaseModel.copy` for details about how to handle `include` and `exclude`.',
        include: AbstractSetIntStr | MappingIntStrAny | None = None,
        exclude: AbstractSetIntStr | MappingIntStrAny | None = None,
        update: Dict[str, Any] | None = None,  # noqa UP006
        deep: bool = False,
        """Returns a copy of the model.
        !!! warning "Deprecated"
            This method is now deprecated; use `model_copy` instead.
        If you need `include` or `exclude`, use:
        ```python {test="skip" lint="skip"}
        data = self.model_dump(include=include, exclude=exclude, round_trip=True)
        data = {**data, **(update or {})}
        copied = self.model_validate(data)
            include: Optional set or mapping specifying which fields to include in the copied model.
            exclude: Optional set or mapping specifying which fields to exclude in the copied model.
            update: Optional dictionary of field-value pairs to override field values in the copied model.
            deep: If True, the values of fields that are Pydantic models will be deep-copied.
            A copy of the model with included, excluded and updated fields as specified.
        from .deprecated import copy_internals
            copy_internals._iter(
                self, to_dict=False, by_alias=False, include=include, exclude=exclude, exclude_unset=False
            **(update or {}),
        if self.__pydantic_private__ is None:
            private = None
            private = {k: v for k, v in self.__pydantic_private__.items() if v is not PydanticUndefined}
        if self.__pydantic_extra__ is None:
            extra: dict[str, Any] | None = None
            extra = self.__pydantic_extra__.copy()
            for k in list(self.__pydantic_extra__):
                if k not in values:  # k was in the exclude
                    extra.pop(k)
            for k in list(values):
                if k in self.__pydantic_extra__:  # k must have come from extra
                    extra[k] = values.pop(k)
        # new `__pydantic_fields_set__` can have unset optional fields with a set value in `update` kwarg
            fields_set = self.__pydantic_fields_set__ | update.keys()
            fields_set = set(self.__pydantic_fields_set__)
        # removing excluded fields from `__pydantic_fields_set__`
        if exclude:
            fields_set -= set(exclude)
        return copy_internals._copy_and_set_values(self, values, fields_set, extra, private, deep=deep)
    @typing_extensions.deprecated('The `schema` method is deprecated; use `model_json_schema` instead.', category=None)
    def schema(  # noqa: D102
        cls, by_alias: bool = True, ref_template: str = DEFAULT_REF_TEMPLATE
            'The `schema` method is deprecated; use `model_json_schema` instead.',
        return cls.model_json_schema(by_alias=by_alias, ref_template=ref_template)
        'The `schema_json` method is deprecated; use `model_json_schema` and json.dumps instead.',
    def schema_json(  # noqa: D102
        cls, *, by_alias: bool = True, ref_template: str = DEFAULT_REF_TEMPLATE, **dumps_kwargs: Any
    ) -> str:  # pragma: no cover
        from .deprecated.json import pydantic_encoder
            cls.model_json_schema(by_alias=by_alias, ref_template=ref_template),
            default=pydantic_encoder,
            **dumps_kwargs,
    @typing_extensions.deprecated('The `validate` method is deprecated; use `model_validate` instead.', category=None)
    def validate(cls, value: Any) -> Self:  # noqa: D102
            'The `validate` method is deprecated; use `model_validate` instead.',
        return cls.model_validate(value)
        'The `update_forward_refs` method is deprecated; use `model_rebuild` instead.',
    def update_forward_refs(cls, **localns: Any) -> None:  # noqa: D102
        if localns:  # pragma: no cover
            raise TypeError('`localns` arguments are not longer accepted.')
        cls.model_rebuild(force=True)
        'The private method `_iter` will be removed and should no longer be used.', category=None
    def _iter(self, *args: Any, **kwargs: Any) -> Any:
            'The private method `_iter` will be removed and should no longer be used.',
        return copy_internals._iter(self, *args, **kwargs)
        'The private method `_copy_and_set_values` will be removed and should no longer be used.',
    def _copy_and_set_values(self, *args: Any, **kwargs: Any) -> Any:
        return copy_internals._copy_and_set_values(self, *args, **kwargs)
        'The private method `_get_value` will be removed and should no longer be used.',
    def _get_value(cls, *args: Any, **kwargs: Any) -> Any:
        return copy_internals._get_value(cls, *args, **kwargs)
        'The private method `_calculate_keys` will be removed and should no longer be used.',
    def _calculate_keys(self, *args: Any, **kwargs: Any) -> Any:
        return copy_internals._calculate_keys(self, *args, **kwargs)
ModelT = TypeVar('ModelT', bound=BaseModel)
def create_model(
    __config__: ConfigDict | None = None,
    __doc__: str | None = None,
    __base__: None = None,
    __module__: str = __name__,
    __validators__: dict[str, Callable[..., Any]] | None = None,
    __cls_kwargs__: dict[str, Any] | None = None,
    __qualname__: str | None = None,
    **field_definitions: Any | tuple[str, Any],
) -> type[BaseModel]: ...
    __base__: type[ModelT] | tuple[type[ModelT], ...],
) -> type[ModelT]: ...
def create_model(  # noqa: C901
    __base__: type[ModelT] | tuple[type[ModelT], ...] | None = None,
    __module__: str | None = None,
    # TODO PEP 747: replace `Any` by the TypeForm:
) -> type[ModelT]:
        [Dynamic Model Creation](../concepts/models.md#dynamic-model-creation)
    Dynamically creates and returns a new Pydantic model, in other words, `create_model` dynamically creates a
    subclass of [`BaseModel`][pydantic.BaseModel].
        This function may execute arbitrary code contained in field annotations, if string references need to be evaluated.
        See [Security implications of introspecting annotations](https://docs.python.org/3/library/annotationlib.html#annotationlib-security) for more information.
        model_name: The name of the newly created model.
        __config__: The configuration of the new model.
        __doc__: The docstring of the new model.
        __base__: The base class or classes for the new model.
        __module__: The name of the module that the model belongs to;
            if `None`, the value is taken from `sys._getframe(1)`
        __validators__: A dictionary of methods that validate fields. The keys are the names of the validation methods to
            be added to the model, and the values are the validation methods themselves. You can read more about functional
            validators [here](https://docs.pydantic.dev/2.9/concepts/validators/#field-validators).
        __cls_kwargs__: A dictionary of keyword arguments for class creation, such as `metaclass`.
        __qualname__: The qualified name of the newly created model.
        **field_definitions: Field definitions of the new model. Either:
            - a single element, representing the type annotation of the field.
            - a two-tuple, the first element being the type and the second element the assigned value
              (either a default or the [`Field()`][pydantic.Field] function).
        The new [model][pydantic.BaseModel].
        PydanticUserError: If `__base__` and `__config__` are both passed.
    if __base__ is None:
        __base__ = (cast('type[ModelT]', BaseModel),)
    elif not isinstance(__base__, tuple):
        __base__ = (__base__,)
    __cls_kwargs__ = __cls_kwargs__ or {}
    fields: dict[str, Any] = {}
    annotations: dict[str, Any] = {}
    for f_name, f_def in field_definitions.items():
        if isinstance(f_def, tuple):
            if len(f_def) != 2:
                    f'Field definition for {f_name!r} should a single element representing the type or a two-tuple, the first element '
                    'being the type and the second element the assigned value (either a default or the `Field()` function).',
                    code='create-model-field-definitions',
            annotations[f_name] = f_def[0]
            fields[f_name] = f_def[1]
            annotations[f_name] = f_def
    if __module__ is None:
        __module__ = f.f_globals['__name__']
    namespace: dict[str, Any] = {'__annotations__': annotations, '__module__': __module__}
        namespace['__doc__'] = __doc__
    if __qualname__ is not None:
        namespace['__qualname__'] = __qualname__
    if __validators__:
        namespace.update(__validators__)
    namespace.update(fields)
    if __config__:
        namespace['model_config'] = __config__
    resolved_bases = types.resolve_bases(__base__)
    meta, ns, kwds = types.prepare_class(model_name, resolved_bases, kwds=__cls_kwargs__)
    if resolved_bases is not __base__:
        ns['__orig_bases__'] = __base__
    namespace.update(ns)
    return meta(
        resolved_bases,
        __pydantic_reset_parent_namespace__=False,
        _create_model_module=__module__,
        **kwds,
from abc import ABCMeta
from types import FunctionType, prepare_class, resolve_bases
    AbstractSet,
    no_type_check,
from typing_extensions import dataclass_transform
from pydantic.v1.class_validators import ValidatorGroup, extract_root_validators, extract_validators, inherit_validators
from pydantic.v1.config import BaseConfig, Extra, inherit_config, prepare_config
from pydantic.v1.error_wrappers import ErrorWrapper, ValidationError
from pydantic.v1.errors import ConfigError, DictError, ExtraError, MissingError
from pydantic.v1.fields import (
    MAPPING_LIKE_SHAPES,
    ModelPrivateAttr,
    PrivateAttr,
    is_finalvar_with_default_val,
from pydantic.v1.json import custom_pydantic_encoder, pydantic_encoder
from pydantic.v1.parse import Protocol, load_file, load_str_bytes
from pydantic.v1.schema import default_ref_template, model_schema
from pydantic.v1.types import PyObject, StrBytes
from pydantic.v1.typing import (
    AnyCallable,
    is_classvar,
    is_namedtuple,
    resolve_annotations,
    update_model_forward_refs,
from pydantic.v1.utils import (
    DUNDER_ATTRIBUTES,
    ROOT_KEY,
    ClassAttribute,
    GetterDict,
    Representation,
    ValueItems,
    generate_model_signature,
    is_valid_field,
    is_valid_private_name,
    sequence_like,
    smart_deepcopy,
    unique_list,
    validate_field_name,
    from pydantic.v1.class_validators import ValidatorListDict
    from pydantic.v1.types import ModelOrDc
        AbstractSetIntStr,
        AnyClassMethod,
        CallableGenerator,
        DictAny,
        DictStrAny,
        MappingIntStrAny,
        ReprArgs,
        SetStr,
        TupleGenerator,
    Model = TypeVar('Model', bound='BaseModel')
__all__ = 'BaseModel', 'create_model', 'validate_model'
def validate_custom_root_type(fields: Dict[str, ModelField]) -> None:
        raise ValueError(f'{ROOT_KEY} cannot be mixed with other fields')
def generate_hash_function(frozen: bool) -> Optional[Callable[[Any], int]]:
    def hash_function(self_: Any) -> int:
        return hash(self_.__class__) + hash(tuple(self_.__dict__.values()))
    return hash_function if frozen else None
# If a field is of type `Callable`, its default value should be a function and cannot to ignored.
ANNOTATED_FIELD_UNTOUCHED_TYPES: Tuple[Any, ...] = (property, type, classmethod, staticmethod)
# When creating a `BaseModel` instance, we bypass all the methods, properties... added to the model
UNTOUCHED_TYPES: Tuple[Any, ...] = (FunctionType,) + ANNOTATED_FIELD_UNTOUCHED_TYPES
# Note `ModelMetaclass` refers to `BaseModel`, but is also used to *create* `BaseModel`, so we need to add this extra
# (somewhat hacky) boolean to keep track of whether we've created the `BaseModel` class yet, and therefore whether it's
# safe to refer to it. If it *hasn't* been created, we assume that the `__new__` call we're in the middle of is for
# the `BaseModel` class, since that's defined immediately after the metaclass.
_is_base_model_class_defined = False
@dataclass_transform(kw_only_default=True, field_specifiers=(Field,))
class ModelMetaclass(ABCMeta):
    @no_type_check  # noqa C901
    def __new__(mcs, name, bases, namespace, **kwargs):  # noqa C901
        fields: Dict[str, ModelField] = {}
        config = BaseConfig
        validators: 'ValidatorListDict' = {}
        pre_root_validators, post_root_validators = [], []
        private_attributes: Dict[str, ModelPrivateAttr] = {}
        base_private_attributes: Dict[str, ModelPrivateAttr] = {}
        slots: SetStr = namespace.get('__slots__', ())
        slots = {slots} if isinstance(slots, str) else set(slots)
        class_vars: SetStr = set()
        hash_func: Optional[Callable[[Any], int]] = None
        for base in reversed(bases):
            if _is_base_model_class_defined and issubclass(base, BaseModel) and base != BaseModel:
                fields.update(smart_deepcopy(base.__fields__))
                config = inherit_config(base.__config__, config)
                validators = inherit_validators(base.__validators__, validators)
                pre_root_validators += base.__pre_root_validators__
                post_root_validators += base.__post_root_validators__
                base_private_attributes.update(base.__private_attributes__)
                class_vars.update(base.__class_vars__)
                hash_func = base.__hash__
        resolve_forward_refs = kwargs.pop('__resolve_forward_refs__', True)
        allowed_config_kwargs: SetStr = {
            key
            for key in dir(config)
            if not (key.startswith('__') and key.endswith('__'))  # skip dunder methods and attributes
        config_kwargs = {key: kwargs.pop(key) for key in kwargs.keys() & allowed_config_kwargs}
        config_from_namespace = namespace.get('Config')
        if config_kwargs and config_from_namespace:
            raise TypeError('Specifying config in two places is ambiguous, use either Config attribute or class kwargs')
        config = inherit_config(config_from_namespace, config, **config_kwargs)
        validators = inherit_validators(extract_validators(namespace), validators)
        vg = ValidatorGroup(validators)
        for f in fields.values():
            f.set_config(config)
            extra_validators = vg.get_validators(f.name)
            if extra_validators:
                f.class_validators.update(extra_validators)
                # re-run prepare to add extra validators
                f.populate_validators()
        prepare_config(config, name)
        untouched_types = ANNOTATED_FIELD_UNTOUCHED_TYPES
        def is_untouched(v: Any) -> bool:
            return isinstance(v, untouched_types) or v.__class__.__name__ == 'cython_function_or_method'
        if (namespace.get('__module__'), namespace.get('__qualname__')) != ('pydantic.main', 'BaseModel'):
            annotations = resolve_annotations(namespace.get('__annotations__', {}), namespace.get('__module__', None))
            # annotation only fields need to come first in fields
            for ann_name, ann_type in annotations.items():
                if is_classvar(ann_type):
                    class_vars.add(ann_name)
                elif is_finalvar_with_default_val(ann_type, namespace.get(ann_name, Undefined)):
                elif is_valid_field(ann_name):
                    validate_field_name(bases, ann_name)
                    value = namespace.get(ann_name, Undefined)
                    allowed_types = get_args(ann_type) if is_union(get_origin(ann_type)) else (ann_type,)
                        is_untouched(value)
                        and ann_type != PyObject
                        and not any(
                            lenient_issubclass(get_origin(allowed_type), Type) for allowed_type in allowed_types
                    fields[ann_name] = ModelField.infer(
                        name=ann_name,
                        annotation=ann_type,
                        class_validators=vg.get_validators(ann_name),
                elif ann_name not in namespace and config.underscore_attrs_are_private:
                    private_attributes[ann_name] = PrivateAttr()
            untouched_types = UNTOUCHED_TYPES + config.keep_untouched
            for var_name, value in namespace.items():
                can_be_changed = var_name not in class_vars and not is_untouched(value)
                if isinstance(value, ModelPrivateAttr):
                    if not is_valid_private_name(var_name):
                        raise NameError(
                            f'Private attributes "{var_name}" must not be a valid field name; '
                            f'Use sunder or dunder names, e. g. "_{var_name}" or "__{var_name}__"'
                    private_attributes[var_name] = value
                elif config.underscore_attrs_are_private and is_valid_private_name(var_name) and can_be_changed:
                    private_attributes[var_name] = PrivateAttr(default=value)
                elif is_valid_field(var_name) and var_name not in annotations and can_be_changed:
                    validate_field_name(bases, var_name)
                    inferred = ModelField.infer(
                        name=var_name,
                        annotation=annotations.get(var_name, Undefined),
                        class_validators=vg.get_validators(var_name),
                    if var_name in fields:
                        if lenient_issubclass(inferred.type_, fields[var_name].type_):
                            inferred.type_ = fields[var_name].type_
                                f'The type of {name}.{var_name} differs from the new default value; '
                                f'if you wish to change the type of this field, please use a type annotation'
                    fields[var_name] = inferred
        _custom_root_type = ROOT_KEY in fields
        if _custom_root_type:
            validate_custom_root_type(fields)
        vg.check_for_unused()
        if config.json_encoders:
            json_encoder = partial(custom_pydantic_encoder, config.json_encoders)
            json_encoder = pydantic_encoder
        pre_rv_new, post_rv_new = extract_root_validators(namespace)
        if hash_func is None:
            hash_func = generate_hash_function(config.frozen)
        exclude_from_namespace = fields | private_attributes.keys() | {'__slots__'}
        new_namespace = {
            '__config__': config,
            '__fields__': fields,
            '__exclude_fields__': {
                name: field.field_info.exclude for name, field in fields.items() if field.field_info.exclude is not None
            or None,
            '__include_fields__': {
                name: field.field_info.include for name, field in fields.items() if field.field_info.include is not None
            '__validators__': vg.validators,
            '__pre_root_validators__': unique_list(
                pre_root_validators + pre_rv_new,
                name_factory=lambda v: v.__name__,
            '__post_root_validators__': unique_list(
                post_root_validators + post_rv_new,
                name_factory=lambda skip_on_failure_and_v: skip_on_failure_and_v[1].__name__,
            '__schema_cache__': {},
            '__json_encoder__': staticmethod(json_encoder),
            '__custom_root_type__': _custom_root_type,
            '__private_attributes__': {**base_private_attributes, **private_attributes},
            '__slots__': slots | private_attributes.keys(),
            '__hash__': hash_func,
            '__class_vars__': class_vars,
            **{n: v for n, v in namespace.items() if n not in exclude_from_namespace},
        cls = super().__new__(mcs, name, bases, new_namespace, **kwargs)
        # set __signature__ attr only for model class, but not for its instances
        cls.__signature__ = ClassAttribute('__signature__', generate_model_signature(cls.__init__, fields, config))
        if not _is_base_model_class_defined:
            # Cython does not understand the `if TYPE_CHECKING:` condition in the
            # BaseModel's body (where annotations are set), so clear them manually:
            getattr(cls, '__annotations__', {}).clear()
        if resolve_forward_refs:
            cls.__try_update_forward_refs__()
        # preserve `__set_name__` protocol defined in https://peps.python.org/pep-0487
        # for attributes not in `new_namespace` (e.g. private attributes)
        for name, obj in namespace.items():
            if name not in new_namespace:
                set_name = getattr(obj, '__set_name__', None)
                if callable(set_name):
                    set_name(cls, name)
    def __instancecheck__(self, instance: Any) -> bool:
        Avoid calling ABC _abc_subclasscheck unless we're pretty sure.
        See #3829 and python/cpython#92810
        return hasattr(instance, '__post_root_validators__') and super().__instancecheck__(instance)
object_setattr = object.__setattr__
class BaseModel(Representation, metaclass=ModelMetaclass):
        # populated by the metaclass, defined here to help IDEs only
        __fields__: ClassVar[Dict[str, ModelField]] = {}
        __include_fields__: ClassVar[Optional[Mapping[str, Any]]] = None
        __exclude_fields__: ClassVar[Optional[Mapping[str, Any]]] = None
        __validators__: ClassVar[Dict[str, AnyCallable]] = {}
        __pre_root_validators__: ClassVar[List[AnyCallable]]
        __post_root_validators__: ClassVar[List[Tuple[bool, AnyCallable]]]
        __config__: ClassVar[Type[BaseConfig]] = BaseConfig
        __json_encoder__: ClassVar[Callable[[Any], Any]] = lambda x: x
        __schema_cache__: ClassVar['DictAny'] = {}
        __custom_root_type__: ClassVar[bool] = False
        __signature__: ClassVar['Signature']
        __private_attributes__: ClassVar[Dict[str, ModelPrivateAttr]]
        __class_vars__: ClassVar[SetStr]
        __fields_set__: ClassVar[SetStr] = set()
    Config = BaseConfig
    __slots__ = ('__dict__', '__fields_set__')
    __doc__ = ''  # Null out the Representation docstring
    def __init__(__pydantic_self__, **data: Any) -> None:
        Create a new model by parsing and validating input data from keyword arguments.
        Raises ValidationError if the input data cannot be parsed to form a valid model.
        # Uses something other than `self` the first arg to allow "self" as a settable attribute
        values, fields_set, validation_error = validate_model(__pydantic_self__.__class__, data)
        if validation_error:
            raise validation_error
            object_setattr(__pydantic_self__, '__dict__', values)
                'Model values must be a dict; you may not have returned a dictionary from a root validator'
        object_setattr(__pydantic_self__, '__fields_set__', fields_set)
        __pydantic_self__._init_private_attributes()
    @no_type_check
    def __setattr__(self, name, value):  # noqa: C901 (ignore complexity)
        if name in self.__private_attributes__ or name in DUNDER_ATTRIBUTES:
            return object_setattr(self, name, value)
        if self.__config__.extra is not Extra.allow and name not in self.__fields__:
            raise ValueError(f'"{self.__class__.__name__}" object has no field "{name}"')
        elif not self.__config__.allow_mutation or self.__config__.frozen:
            raise TypeError(f'"{self.__class__.__name__}" is immutable and does not support item assignment')
        elif name in self.__fields__ and self.__fields__[name].final:
                f'"{self.__class__.__name__}" object "{name}" field is final and does not support reassignment'
        elif self.__config__.validate_assignment:
            new_values = {**self.__dict__, name: value}
            for validator in self.__pre_root_validators__:
                    new_values = validator(self.__class__, new_values)
                except (ValueError, TypeError, AssertionError) as exc:
                    raise ValidationError([ErrorWrapper(exc, loc=ROOT_KEY)], self.__class__)
            known_field = self.__fields__.get(name, None)
            if known_field:
                # We want to
                # - make sure validators are called without the current value for this field inside `values`
                # - keep other values (e.g. submodels) untouched (using `BaseModel.dict()` will change them into dicts)
                # - keep the order of the fields
                if not known_field.field_info.allow_mutation:
                    raise TypeError(f'"{known_field.name}" has allow_mutation set to False and cannot be assigned')
                dict_without_original_value = {k: v for k, v in self.__dict__.items() if k != name}
                value, error_ = known_field.validate(value, dict_without_original_value, loc=name, cls=self.__class__)
                if error_:
                    raise ValidationError([error_], self.__class__)
                    new_values[name] = value
            for skip_on_failure, validator in self.__post_root_validators__:
                if skip_on_failure and errors:
                    errors.append(ErrorWrapper(exc, loc=ROOT_KEY))
                raise ValidationError(errors, self.__class__)
            # update the whole __dict__ as other values than just `value`
            # may be changed (e.g. with `root_validator`)
            object_setattr(self, '__dict__', new_values)
            self.__dict__[name] = value
        self.__fields_set__.add(name)
    def __getstate__(self) -> 'DictAny':
        private_attrs = ((k, getattr(self, k, Undefined)) for k in self.__private_attributes__)
            '__fields_set__': self.__fields_set__,
            '__private_attribute_values__': {k: v for k, v in private_attrs if v is not Undefined},
    def __setstate__(self, state: 'DictAny') -> None:
        object_setattr(self, '__dict__', state['__dict__'])
        object_setattr(self, '__fields_set__', state['__fields_set__'])
        for name, value in state.get('__private_attribute_values__', {}).items():
            object_setattr(self, name, value)
    def _init_private_attributes(self) -> None:
        for name, private_attr in self.__private_attributes__.items():
            default = private_attr.get_default()
            if default is not Undefined:
                object_setattr(self, name, default)
    def dict(
        include: Optional[Union['AbstractSetIntStr', 'MappingIntStrAny']] = None,
        exclude: Optional[Union['AbstractSetIntStr', 'MappingIntStrAny']] = None,
        skip_defaults: Optional[bool] = None,
    ) -> 'DictStrAny':
        if skip_defaults is not None:
                f'{self.__class__.__name__}.dict(): "skip_defaults" is deprecated and replaced by "exclude_unset"',
            exclude_unset = skip_defaults
        return dict(
            self._iter(
                to_dict=True,
    def json(
        encoder: Optional[Callable[[Any], Any]] = None,
        models_as_dict: bool = True,
        Generate a JSON representation of the model, `include` and `exclude` arguments as per `dict()`.
        `encoder` is an optional function to supply as `default` to json.dumps(), other arguments as per `json.dumps()`.
                f'{self.__class__.__name__}.json(): "skip_defaults" is deprecated and replaced by "exclude_unset"',
        encoder = cast(Callable[[Any], Any], encoder or self.__json_encoder__)
        # We don't directly call `self.dict()`, which does exactly this with `to_dict=True`
        # because we want to be able to keep raw `BaseModel` instances and not as `dict`.
        # This allows users to write custom JSON encoders for given `BaseModel` classes.
        data = dict(
                to_dict=models_as_dict,
        if self.__custom_root_type__:
            data = data[ROOT_KEY]
        return self.__config__.json_dumps(data, default=encoder, **dumps_kwargs)
    def _enforce_dict_if_root(cls, obj: Any) -> Any:
        if cls.__custom_root_type__ and (
            not (isinstance(obj, dict) and obj.keys() == {ROOT_KEY})
            and not (isinstance(obj, BaseModel) and obj.__fields__.keys() == {ROOT_KEY})
            or cls.__fields__[ROOT_KEY].shape in MAPPING_LIKE_SHAPES
            return {ROOT_KEY: obj}
    def parse_obj(cls: Type['Model'], obj: Any) -> 'Model':
        obj = cls._enforce_dict_if_root(obj)
        if not isinstance(obj, dict):
                obj = dict(obj)
                exc = TypeError(f'{cls.__name__} expected dict not {obj.__class__.__name__}')
                raise ValidationError([ErrorWrapper(exc, loc=ROOT_KEY)], cls) from e
        return cls(**obj)
    def parse_raw(
        cls: Type['Model'],
        b: StrBytes,
        content_type: str = None,
        proto: Protocol = None,
    ) -> 'Model':
            obj = load_str_bytes(
                json_loads=cls.__config__.json_loads,
        except (ValueError, TypeError, UnicodeDecodeError) as e:
            raise ValidationError([ErrorWrapper(e, loc=ROOT_KEY)], cls)
        path: Union[str, Path],
        obj = load_file(
    def from_orm(cls: Type['Model'], obj: Any) -> 'Model':
        if not cls.__config__.orm_mode:
            raise ConfigError('You must have the config attribute orm_mode=True to use from_orm')
        obj = {ROOT_KEY: obj} if cls.__custom_root_type__ else cls._decompose_class(obj)
        values, fields_set, validation_error = validate_model(cls, obj)
        object_setattr(m, '__dict__', values)
        object_setattr(m, '__fields_set__', fields_set)
        m._init_private_attributes()
    def construct(cls: Type['Model'], _fields_set: Optional['SetStr'] = None, **values: Any) -> 'Model':
        Creates a new model setting __dict__ and __fields_set__ from trusted or pre-validated data.
        Behaves as if `Config.extra = 'allow'` was set since it adds all passed values
        fields_values: Dict[str, Any] = {}
        for name, field in cls.__fields__.items():
            if field.alt_alias and field.alias in values:
                fields_values[name] = values[field.alias]
            elif name in values:
                fields_values[name] = values[name]
            elif not field.required:
                fields_values[name] = field.get_default()
        fields_values.update(values)
        object_setattr(m, '__dict__', fields_values)
            _fields_set = set(values.keys())
        object_setattr(m, '__fields_set__', _fields_set)
    def _copy_and_set_values(self: 'Model', values: 'DictStrAny', fields_set: 'SetStr', *, deep: bool) -> 'Model':
        if deep:
            # chances of having empty dict here are quite low for using smart_deepcopy
            values = deepcopy(values)
        for name in self.__private_attributes__:
            value = getattr(self, name, Undefined)
            if value is not Undefined:
                    value = deepcopy(value)
                object_setattr(m, name, value)
        self: 'Model',
        update: Optional['DictStrAny'] = None,
        Duplicate a model, optionally choose which fields to include, exclude and change.
        :param include: fields to include in new model
        :param exclude: fields to exclude from new model, as with values this takes precedence over include
        :param update: values to change/add in the new model. Note: the data is not validated before creating
            the new model: you should trust this data
        :param deep: set to `True` to make a deep copy of the model
        :return: new model instance
            self._iter(to_dict=False, by_alias=False, include=include, exclude=exclude, exclude_unset=False),
        # new `__fields_set__` can have unset optional fields with a set value in `update` kwarg
            fields_set = self.__fields_set__ | update.keys()
            fields_set = set(self.__fields_set__)
        return self._copy_and_set_values(values, fields_set, deep=deep)
    def schema(cls, by_alias: bool = True, ref_template: str = default_ref_template) -> 'DictStrAny':
        cached = cls.__schema_cache__.get((by_alias, ref_template))
        s = model_schema(cls, by_alias=by_alias, ref_template=ref_template)
        cls.__schema_cache__[(by_alias, ref_template)] = s
    def schema_json(
        cls, *, by_alias: bool = True, ref_template: str = default_ref_template, **dumps_kwargs: Any
        from pydantic.v1.json import pydantic_encoder
        return cls.__config__.json_dumps(
            cls.schema(by_alias=by_alias, ref_template=ref_template), default=pydantic_encoder, **dumps_kwargs
    def __get_validators__(cls) -> 'CallableGenerator':
    def validate(cls: Type['Model'], value: Any) -> 'Model':
        if isinstance(value, cls):
            copy_on_model_validation = cls.__config__.copy_on_model_validation
            # whether to deep or shallow copy the model on validation, None means do not copy
            deep_copy: Optional[bool] = None
            if copy_on_model_validation not in {'deep', 'shallow', 'none'}:
                # Warn about deprecated behavior
                    "`copy_on_model_validation` should be a string: 'deep', 'shallow' or 'none'", DeprecationWarning
                if copy_on_model_validation:
                    deep_copy = False
            if copy_on_model_validation == 'shallow':
                # shallow copy
            elif copy_on_model_validation == 'deep':
                # deep copy
                deep_copy = True
            if deep_copy is None:
                return value._copy_and_set_values(value.__dict__, value.__fields_set__, deep=deep_copy)
        value = cls._enforce_dict_if_root(value)
            return cls(**value)
        elif cls.__config__.orm_mode:
            return cls.from_orm(value)
                value_as_dict = dict(value)
                raise DictError() from e
            return cls(**value_as_dict)
    def _decompose_class(cls: Type['Model'], obj: Any) -> GetterDict:
        if isinstance(obj, GetterDict):
        return cls.__config__.getter_dict(obj)
    def _get_value(
        v: Any,
        to_dict: bool,
        by_alias: bool,
        include: Optional[Union['AbstractSetIntStr', 'MappingIntStrAny']],
        exclude: Optional[Union['AbstractSetIntStr', 'MappingIntStrAny']],
        exclude_unset: bool,
        exclude_defaults: bool,
        exclude_none: bool,
        if isinstance(v, BaseModel):
            if to_dict:
                v_dict = v.dict(
                if ROOT_KEY in v_dict:
                    return v_dict[ROOT_KEY]
                return v_dict
                return v.copy(include=include, exclude=exclude)
        value_exclude = ValueItems(v, exclude) if exclude else None
        value_include = ValueItems(v, include) if include else None
        if isinstance(v, dict):
                k_: cls._get_value(
                    v_,
                    to_dict=to_dict,
                    include=value_include and value_include.for_element(k_),
                    exclude=value_exclude and value_exclude.for_element(k_),
                for k_, v_ in v.items()
                if (not value_exclude or not value_exclude.is_excluded(k_))
                and (not value_include or value_include.is_included(k_))
        elif sequence_like(v):
            seq_args = (
                cls._get_value(
                    include=value_include and value_include.for_element(i),
                    exclude=value_exclude and value_exclude.for_element(i),
                for i, v_ in enumerate(v)
                if (not value_exclude or not value_exclude.is_excluded(i))
                and (not value_include or value_include.is_included(i))
            return v.__class__(*seq_args) if is_namedtuple(v.__class__) else v.__class__(seq_args)
        elif isinstance(v, Enum) and getattr(cls.Config, 'use_enum_values', False):
            return v.value
    def __try_update_forward_refs__(cls, **localns: Any) -> None:
        Same as update_forward_refs but will not raise exception
        when forward references are not defined.
        update_model_forward_refs(cls, cls.__fields__.values(), cls.__config__.json_encoders, localns, (NameError,))
    def update_forward_refs(cls, **localns: Any) -> None:
        Try to update ForwardRefs on fields based on this Model, globalns and localns.
        update_model_forward_refs(cls, cls.__fields__.values(), cls.__config__.json_encoders, localns)
    def __iter__(self) -> 'TupleGenerator':
        so `dict(model)` works
        yield from self.__dict__.items()
    def _iter(
        to_dict: bool = False,
    ) -> 'TupleGenerator':
        # Merge field set excludes with explicit exclude parameter with explicit overriding field set options.
        # The extra "is not None" guards are not logically necessary but optimizes performance for the simple case.
        if exclude is not None or self.__exclude_fields__ is not None:
            exclude = ValueItems.merge(self.__exclude_fields__, exclude)
        if include is not None or self.__include_fields__ is not None:
            include = ValueItems.merge(self.__include_fields__, include, intersect=True)
        allowed_keys = self._calculate_keys(
            include=include, exclude=exclude, exclude_unset=exclude_unset  # type: ignore
        if allowed_keys is None and not (to_dict or by_alias or exclude_unset or exclude_defaults or exclude_none):
            # huge boost for plain _iter()
        value_exclude = ValueItems(self, exclude) if exclude is not None else None
        value_include = ValueItems(self, include) if include is not None else None
        for field_key, v in self.__dict__.items():
            if (allowed_keys is not None and field_key not in allowed_keys) or (exclude_none and v is None):
            if exclude_defaults:
                model_field = self.__fields__.get(field_key)
                if not getattr(model_field, 'required', True) and getattr(model_field, 'default', _missing) == v:
            if by_alias and field_key in self.__fields__:
                dict_key = self.__fields__[field_key].alias
                dict_key = field_key
            if to_dict or value_include or value_exclude:
                v = self._get_value(
                    v,
                    include=value_include and value_include.for_element(field_key),
                    exclude=value_exclude and value_exclude.for_element(field_key),
            yield dict_key, v
    def _calculate_keys(
        include: Optional['MappingIntStrAny'],
        exclude: Optional['MappingIntStrAny'],
    ) -> Optional[AbstractSet[str]]:
        if include is None and exclude is None and exclude_unset is False:
        keys: AbstractSet[str]
        if exclude_unset:
            keys = self.__fields_set__.copy()
            keys = self.__dict__.keys()
        if include is not None:
            keys &= include.keys()
            keys -= update.keys()
            keys -= {k for k, v in exclude.items() if ValueItems.is_true(v)}
        return keys
            return self.dict() == other.dict()
            return self.dict() == other
    def __repr_args__(self) -> 'ReprArgs':
            (k, v)
            for k, v in self.__dict__.items()
            if k not in DUNDER_ATTRIBUTES and (k not in self.__fields__ or self.__fields__[k].field_info.repr)
_is_base_model_class_defined = True
    __model_name: str,
    __config__: Optional[Type[BaseConfig]] = None,
    __validators__: Dict[str, 'AnyClassMethod'] = None,
    __cls_kwargs__: Dict[str, Any] = None,
    **field_definitions: Any,
) -> Type['BaseModel']:
    __base__: Union[Type['Model'], Tuple[Type['Model'], ...]],
) -> Type['Model']:
    __base__: Union[None, Type['Model'], Tuple[Type['Model'], ...]] = None,
    __slots__: Optional[Tuple[str, ...]] = None,
    Dynamically create a model.
    :param __model_name: name of the created model
    :param __config__: config class to use for the new model
    :param __base__: base class for the new model to inherit from
    :param __module__: module of the created model
    :param __validators__: a dict of method names and @validator class methods
    :param __cls_kwargs__: a dict for class creation
    :param __slots__: Deprecated, `__slots__` should not be passed to `create_model`
    :param field_definitions: fields of the model (or extra fields if a base is supplied)
        in the format `<name>=(<type>, <default default>)` or `<name>=<default value>, e.g.
        `foobar=(str, ...)` or `foobar=123`, or, for complex use-cases, in the format
        `<name>=<Field>` or `<name>=(<type>, <FieldInfo>)`, e.g.
        `foo=Field(datetime, default_factory=datetime.utcnow, alias='bar')` or
        `foo=(str, FieldInfo(title='Foo'))`
    if __slots__ is not None:
        # __slots__ will be ignored from here on
        warnings.warn('__slots__ should not be passed to create_model', RuntimeWarning)
    if __base__ is not None:
        if __config__ is not None:
            raise ConfigError('to avoid confusion __config__ and __base__ cannot be used together')
        if not isinstance(__base__, tuple):
        __base__ = (cast(Type['Model'], BaseModel),)
    fields = {}
        if not is_valid_field(f_name):
            warnings.warn(f'fields may not start with an underscore, ignoring "{f_name}"', RuntimeWarning)
                f_annotation, f_value = f_def
                    'field definitions should either be a tuple of (<type>, <default>) or just a '
                    'default value, unfortunately this means tuples as '
                    'default values are not allowed'
            f_annotation, f_value = None, f_def
        if f_annotation:
            annotations[f_name] = f_annotation
        fields[f_name] = f_value
    namespace: 'DictStrAny' = {'__annotations__': annotations, '__module__': __module__}
        namespace['Config'] = inherit_config(__config__, BaseConfig)
    resolved_bases = resolve_bases(__base__)
    meta, ns, kwds = prepare_class(__model_name, resolved_bases, kwds=__cls_kwargs__)
    return meta(__model_name, resolved_bases, namespace, **kwds)
_missing = object()
def validate_model(  # noqa: C901 (ignore complexity)
    model: Type[BaseModel], input_data: 'DictStrAny', cls: 'ModelOrDc' = None
) -> Tuple['DictStrAny', 'SetStr', Optional[ValidationError]]:
    validate data against a model.
    # input_data names, possibly alias
    names_used = set()
    # field names, never aliases
    config = model.__config__
    check_extra = config.extra is not Extra.ignore
    cls_ = cls or model
    for validator in model.__pre_root_validators__:
            input_data = validator(cls_, input_data)
            return {}, set(), ValidationError([ErrorWrapper(exc, loc=ROOT_KEY)], cls_)
    for name, field in model.__fields__.items():
        value = input_data.get(field.alias, _missing)
        using_name = False
        if value is _missing and config.allow_population_by_field_name and field.alt_alias:
            value = input_data.get(field.name, _missing)
            using_name = True
        if value is _missing:
            if field.required:
                errors.append(ErrorWrapper(MissingError(), loc=field.alias))
            if not config.validate_all and not field.validate_always:
                values[name] = value
            if check_extra:
                names_used.add(field.name if using_name else field.alias)
        v_, errors_ = field.validate(value, values, loc=field.alias, cls=cls_)
        if isinstance(errors_, ErrorWrapper):
            errors.append(errors_)
        elif isinstance(errors_, list):
            values[name] = v_
        if isinstance(input_data, GetterDict):
            extra = input_data.extra_keys() - names_used
            extra = input_data.keys() - names_used
            fields_set |= extra
            if config.extra is Extra.allow:
                for f in extra:
                    values[f] = input_data[f]
                for f in sorted(extra):
                    errors.append(ErrorWrapper(ExtraError(), loc=f))
    for skip_on_failure, validator in model.__post_root_validators__:
            values = validator(cls_, values)
        return values, fields_set, ValidationError(errors, cls_)
        return values, fields_set, None
import pathspec
from .dump import dump  # noqa: F401
from .parsers import PARSERS
    # Parse command line arguments
    parser.add_argument("pattern", nargs="?", help="the pattern to search for")
    parser.add_argument("filenames", nargs="*", help="the files to display", default=".")
    parser.add_argument("--encoding", default="utf8", help="file encoding")
    parser.add_argument("--languages", action="store_true", help="show supported languages")
    parser.add_argument("-i", "--ignore-case", action="store_true", help="ignore case distinctions")
    parser.add_argument("--color", action="store_true", help="force color printing", default=None)
        "--no-color", action="store_false", help="disable color printing", dest="color"
    parser.add_argument("--no-gitignore", action="store_true", help="ignore .gitignore file")
    parser.add_argument("--verbose", action="store_true", help="enable verbose output")
    parser.add_argument("-n", "--line-number", action="store_true", help="display line numbers")
    # If stdout is not a terminal, set color to False
    if args.color is None:
        args.color = os.isatty(1)
    # If --languages is provided, print the parsers table and exit
    if args.languages:
        for ext, lang in sorted(PARSERS.items()):
            print(f"{ext}: {lang}")
    elif not args.pattern:
        print("Please provide a pattern to search for")
    gitignore = None
    if not args.no_gitignore:
        for parent in Path("./xxx").resolve().parents:
            potential_gitignore = parent / ".gitignore"
            if potential_gitignore.exists():
                gitignore = potential_gitignore
    if gitignore:
        with gitignore.open() as f:
            spec = pathspec.PathSpec.from_lines("gitwildmatch", f)
        spec = pathspec.PathSpec.from_lines("gitwildmatch", [])
    for fname in enumerate_files(args.filenames, spec):
        process_filename(fname, args)
def enumerate_files(fnames, spec, use_spec=False):
    for fname in fnames:
        fname = Path(fname)
        # oddly, Path('.').name == "" so we will recurse it
        if fname.name.startswith(".") or use_spec and spec.match_file(fname):
        if fname.is_file():
            yield str(fname)
        if fname.is_dir():
            for sub_fnames in enumerate_files(fname.iterdir(), spec, True):
                yield sub_fnames
def process_filename(filename, args):
        with open(filename, "r", encoding=args.encoding) as file:
            code = file.read()
        tc = TreeContext(
            filename, code, color=args.color, verbose=args.verbose, line_number=args.line_number
    loi = tc.grep(args.pattern, args.ignore_case)
    if not loi:
    tc.add_lines_of_interest(loi)
    tc.add_context()
    print(f"{filename}:")
    print(tc.format(), end="")
    res = main()
    sys.exit(res)
from traceback import FrameSummary, StackSummary
from typing import Annotated, Any, Callable, Optional, Union
from typer._types import TyperChoice
from ._typing import get_args, get_origin, is_literal_type, is_union, literal_values
from .completion import get_completion_inspect_parameters
    DEFAULT_MARKUP_MODE,
    HAS_RICH,
    MarkupMode,
    TyperArgument,
    TyperCommand,
    TyperGroup,
    TyperOption,
    AnyType,
    ArgumentInfo,
    CommandFunctionType,
    CommandInfo,
    DefaultPlaceholder,
    DeveloperExceptionConfig,
    FileBinaryRead,
    FileBinaryWrite,
    FileTextWrite,
    OptionInfo,
    ParameterInfo,
    ParamMeta,
    TyperInfo,
    TyperPath,
_original_except_hook = sys.excepthook
_typer_developer_exception_attr_name = "__typer_developer_exception__"
def except_hook(
    exc_type: type[BaseException], exc_value: BaseException, tb: Optional[TracebackType]
    exception_config: Union[DeveloperExceptionConfig, None] = getattr(
        exc_value, _typer_developer_exception_attr_name, None
    standard_traceback = os.getenv(
        "TYPER_STANDARD_TRACEBACK", os.getenv("_TYPER_STANDARD_TRACEBACK")
        standard_traceback
        or not exception_config
        or not exception_config.pretty_exceptions_enable
        _original_except_hook(exc_type, exc_value, tb)
    typer_path = os.path.dirname(__file__)
    click_path = os.path.dirname(click.__file__)  # ty: ignore[no-matching-overload]
    internal_dir_names = [typer_path, click_path]
    exc = exc_value
    if HAS_RICH:
        from . import rich_utils
        rich_tb = rich_utils.get_traceback(exc, exception_config, internal_dir_names)
        console_stderr = rich_utils._get_rich_console(stderr=True)
        console_stderr.print(rich_tb)
    tb_exc = traceback.TracebackException.from_exception(exc)
    stack: list[FrameSummary] = []
    for frame in tb_exc.stack:
        if any(frame.filename.startswith(path) for path in internal_dir_names):
            if not exception_config.pretty_exceptions_short:
                # Hide the line for internal libraries, Typer and Click
                stack.append(
                    traceback.FrameSummary(
                        filename=frame.filename,
                        lineno=frame.lineno,
                        name=frame.name,
                        line="",
            stack.append(frame)
    # Type ignore ref: https://github.com/python/typeshed/pull/8244
    final_stack_summary = StackSummary.from_list(stack)
    tb_exc.stack = final_stack_summary
    for line in tb_exc.format():
        print(line, file=sys.stderr)
def get_install_completion_arguments() -> tuple[click.Parameter, click.Parameter]:
    install_param, show_param = get_completion_inspect_parameters()
    click_install_param, _ = get_click_param(install_param)
    click_show_param, _ = get_click_param(show_param)
    return click_install_param, click_show_param
class Typer:
    `Typer` main class, the main entrypoint to use Typer.
    [Typer docs for First Steps](https://typer.tiangolo.com/tutorial/typer-app/).
    import typer
    app = typer.Typer()
        name: Annotated[
            Optional[str],
                The name of this application.
                Mostly used to set the name for [subcommands](https://typer.tiangolo.com/tutorial/subcommands/), in which case it can be overridden by `add_typer(name=...)`.
                app = typer.Typer(name="users")
        ] = Default(None),
        cls: Annotated[
            Optional[type[TyperGroup]],
                The class of this app. Mainly used when [using the Click library underneath](https://typer.tiangolo.com/tutorial/using-click/). Can usually be left at the default value `None`.
                Otherwise, should be a subtype of `TyperGroup`.
                app = typer.Typer(cls=TyperGroup)
        invoke_without_command: Annotated[
                By setting this to `True`, you can make sure a callback is executed even when no subcommand is provided.
                app = typer.Typer(invoke_without_command=True)
        ] = Default(False),
        no_args_is_help: Annotated[
                If this is set to `True`, running a command without any arguments will automatically show the help page.
                app = typer.Typer(no_args_is_help=True)
        subcommand_metavar: Annotated[
                **Note**: you probably shouldn't use this parameter, it is inherited
                from Click and supported for compatibility.
                How to represent the subcommand argument in help.
        chain: Annotated[
                Allow passing more than one subcommand argument.
        result_callback: Annotated[
            Optional[Callable[..., Any]],
                A function to call after the group's and subcommand's callbacks.
        # Command
        context_settings: Annotated[
            Optional[dict[Any, Any]],
                Pass configurations for the [context](https://typer.tiangolo.com/tutorial/commands/context/).
                Available configurations can be found in the docs for Click's `Context` [here](https://click.palletsprojects.com/en/stable/api/#context).
                app = typer.Typer(context_settings={"help_option_names": ["-h", "--help"]})
        callback: Annotated[
                Add a callback to the main Typer app. Can be overridden with `@app.callback()`.
                See [the tutorial about callbacks](https://typer.tiangolo.com/tutorial/commands/callback/) for more details.
                def callback():
                    print("Running a command")
                app = typer.Typer(callback=callback)
        help: Annotated[
                Help text for the main Typer app.
                See [the tutorial about name and help](https://typer.tiangolo.com/tutorial/subcommands/name-and-help) for different ways of setting a command's help,
                and which one takes priority.
                app = typer.Typer(help="Some help.")
        epilog: Annotated[
                Text that will be printed right after the help text.
                app = typer.Typer(epilog="May the force be with you")
        short_help: Annotated[
                A shortened version of the help text that can be used e.g. in the help table listing subcommands.
                When not defined, the normal `help` text will be used instead.
                app = typer.Typer(help="A lot of explanation about user management", short_help="user management")
        options_metavar: Annotated[
                In the example usage string of the help text for a command, the default placeholder for various arguments is `[OPTIONS]`.
                Set `options_metavar` to change this into a different string.
                app = typer.Typer(options_metavar="[OPTS]")
        ] = Default("[OPTIONS]"),
        add_help_option: Annotated[
                By default each command registers a `--help` option. This can be disabled by this parameter.
        ] = Default(True),
        hidden: Annotated[
                Hide this command from help outputs. `False` by default.
                app = typer.Typer(hidden=True)
        deprecated: Annotated[
                Mark this command as being deprecated in the help text. `False` by default.
                app = typer.Typer(deprecated=True)
        add_completion: Annotated[
                Toggle whether or not to add the `--install-completion` and `--show-completion` options to the app.
                Set to `True` by default.
                app = typer.Typer(add_completion=False)
        # Rich settings
        rich_markup_mode: Annotated[
                Enable markup text if you have Rich installed. This can be set to `"markdown"`, `"rich"`, or `None`.
                By default, `rich_markup_mode` is `None` if Rich is not installed, and `"rich"` if it is installed.
                See [the tutorial on help formatting](https://typer.tiangolo.com/tutorial/commands/help/#rich-markdown-and-markup) for more information.
                app = typer.Typer(rich_markup_mode="rich")
        ] = DEFAULT_MARKUP_MODE,
        rich_help_panel: Annotated[
            Union[str, None],
                Set the panel name of the command when the help is printed with Rich.
                app = typer.Typer(rich_help_panel="Utils and Configs")
        suggest_commands: Annotated[
                As of version 0.20.0, Typer provides [support for mistyped command names](https://typer.tiangolo.com/tutorial/commands/help/#suggest-commands) by printing helpful suggestions.
                You can turn this setting off with `suggest_commands`:
                app = typer.Typer(suggest_commands=False)
        pretty_exceptions_enable: Annotated[
                If you want to disable [pretty exceptions with Rich](https://typer.tiangolo.com/tutorial/exceptions/#exceptions-with-rich),
                you can set `pretty_exceptions_enable` to `False`. When doing so, you will see the usual standard exception trace.
                app = typer.Typer(pretty_exceptions_enable=False)
        pretty_exceptions_show_locals: Annotated[
                If Rich is installed, [error messages](https://typer.tiangolo.com/tutorial/exceptions/#exceptions-and-errors)
                will be nicely printed.
                If you set `pretty_exceptions_show_locals=True` it will also include the values of local variables for easy debugging.
                However, if such a variable contains delicate information, you should consider leaving `pretty_exceptions_show_locals=False`
                (the default) to `False` to enhance security.
                app = typer.Typer(pretty_exceptions_show_locals=True)
        pretty_exceptions_short: Annotated[
                By default, [pretty exceptions formatted with Rich](https://typer.tiangolo.com/tutorial/exceptions/#exceptions-with-rich) hide the long stack trace.
                If you want to show the full trace instead, you can set the parameter `pretty_exceptions_short` to `False`:
                app = typer.Typer(pretty_exceptions_short=False)
        self._add_completion = add_completion
        self.rich_markup_mode: MarkupMode = rich_markup_mode
        self.rich_help_panel = rich_help_panel
        self.suggest_commands = suggest_commands
        self.pretty_exceptions_enable = pretty_exceptions_enable
        self.pretty_exceptions_show_locals = pretty_exceptions_show_locals
        self.pretty_exceptions_short = pretty_exceptions_short
        self.info = TyperInfo(
            cls=cls,
            invoke_without_command=invoke_without_command,
            no_args_is_help=no_args_is_help,
            subcommand_metavar=subcommand_metavar,
            result_callback=result_callback,
            context_settings=context_settings,
            help=help,
            epilog=epilog,
            short_help=short_help,
            options_metavar=options_metavar,
            add_help_option=add_help_option,
            hidden=hidden,
            deprecated=deprecated,
        self.registered_groups: list[TyperInfo] = []
        self.registered_commands: list[CommandInfo] = []
        self.registered_callback: Optional[TyperInfo] = None
                Help text for the command.
                Set `options_metavar` to change this into a different string. When `None`, the default value will be used.
                Mark this command as deprecated in the help text. `False` by default.
    ) -> Callable[[CommandFunctionType], CommandFunctionType]:
        Using the decorator `@app.callback`, you can declare the CLI parameters for the main CLI application.
        [Typer docs for Callbacks](https://typer.tiangolo.com/tutorial/commands/callback/).
        state = {"verbose": False}
        @app.callback()
        def main(verbose: bool = False):
                print("Will write verbose output")
                state["verbose"] = True
        @app.command()
        def delete(username: str):
            # define subcommand
        def decorator(f: CommandFunctionType) -> CommandFunctionType:
            self.registered_callback = TyperInfo(
                callback=f,
                options_metavar=(
                    options_metavar or self._info_val_str("options_metavar")
                rich_help_panel=rich_help_panel,
    def command(
                The name of this command.
            Optional[type[TyperCommand]],
                The class of this command. Mainly used when [using the Click library underneath](https://typer.tiangolo.com/tutorial/using-click/). Can usually be left at the default value `None`.
                Otherwise, should be a subtype of `TyperCommand`.
                Mark this command as deprecated in the help outputs. `False` by default.
        Using the decorator `@app.command`, you can define a subcommand of the previously defined Typer app.
        [Typer docs for Commands](https://typer.tiangolo.com/tutorial/commands/).
        def create():
            print("Creating user: Hiro Hamada")
        def delete():
            print("Deleting user: Hiro Hamada")
            cls = TyperCommand
            self.registered_commands.append(
                CommandInfo(
    def add_typer(
        typer_instance: "Typer",
                The name of this subcommand.
                See [the tutorial about name and help](https://typer.tiangolo.com/tutorial/subcommands/name-and-help) for different ways of setting a command's name,
                The class of this subcommand. Mainly used when [using the Click library underneath](https://typer.tiangolo.com/tutorial/using-click/). Can usually be left at the default value `None`.
                Add a callback to this app.
                Help text for the subcommand.
        Add subcommands to the main app using `app.add_typer()`.
        Subcommands may be defined in separate modules, ensuring clean separation of code by functionality.
        [Typer docs for SubCommands](https://typer.tiangolo.com/tutorial/subcommands/add-typer/).
        from .add import app as add_app
        from .delete import app as delete_app
        app.add_typer(add_app)
        app.add_typer(delete_app)
        self.registered_groups.append(
            TyperInfo(
                typer_instance,
    def __call__(self, *args: Any, **kwargs: Any) -> Any:
        if sys.excepthook != except_hook:
            sys.excepthook = except_hook
            return get_command(self)(*args, **kwargs)
            # Set a custom attribute to tell the hook to show nice exceptions for user
            # code. An alternative/first implementation was a custom exception with
            # raise custom_exc from e
            # but that means the last error shown is the custom exception, not the
            # actual error. This trick improves developer experience by showing the
            # actual error last.
                _typer_developer_exception_attr_name,
                DeveloperExceptionConfig(
                    pretty_exceptions_enable=self.pretty_exceptions_enable,
                    pretty_exceptions_show_locals=self.pretty_exceptions_show_locals,
                    pretty_exceptions_short=self.pretty_exceptions_short,
    def _info_val_str(self, name: str) -> str:
        val = getattr(self.info, name)
        val_str = val.value if isinstance(val, DefaultPlaceholder) else val
        assert isinstance(val_str, str)
        return val_str
def get_group(typer_instance: Typer) -> TyperGroup:
    group = get_group_from_info(
        TyperInfo(typer_instance),
        pretty_exceptions_short=typer_instance.pretty_exceptions_short,
        rich_markup_mode=typer_instance.rich_markup_mode,
        suggest_commands=typer_instance.suggest_commands,
def get_command(typer_instance: Typer) -> click.Command:
    if typer_instance._add_completion:
        click_install_param, click_show_param = get_install_completion_arguments()
        typer_instance.registered_callback
        or typer_instance.info.callback
        or typer_instance.registered_groups
        or len(typer_instance.registered_commands) > 1
        # Create a Group
        click_command: click.Command = get_group(typer_instance)
            click_command.params.append(click_install_param)
            click_command.params.append(click_show_param)
        return click_command
    elif len(typer_instance.registered_commands) == 1:
        # Create a single Command
        single_command = typer_instance.registered_commands[0]
        if not single_command.context_settings and not isinstance(
            typer_instance.info.context_settings, DefaultPlaceholder
            single_command.context_settings = typer_instance.info.context_settings
        click_command = get_command_from_info(
            single_command,
        "Could not get a command for this Typer instance"
def solve_typer_info_help(typer_info: TyperInfo) -> str:
    # Priority 1: Explicit value was set in app.add_typer()
    if not isinstance(typer_info.help, DefaultPlaceholder):
        return inspect.cleandoc(typer_info.help or "")
    # Priority 2: Explicit value was set in sub_app.callback()
    if typer_info.typer_instance and typer_info.typer_instance.registered_callback:
        callback_help = typer_info.typer_instance.registered_callback.help
        if not isinstance(callback_help, DefaultPlaceholder):
            return inspect.cleandoc(callback_help or "")
    # Priority 3: Explicit value was set in sub_app = typer.Typer()
    if typer_info.typer_instance and typer_info.typer_instance.info:
        instance_help = typer_info.typer_instance.info.help
        if not isinstance(instance_help, DefaultPlaceholder):
            return inspect.cleandoc(instance_help or "")
    # Priority 4: Implicit inference from callback docstring in app.add_typer()
    if typer_info.callback:
        doc = inspect.getdoc(typer_info.callback)
            return doc
    # Priority 5: Implicit inference from callback docstring in @app.callback()
        callback = typer_info.typer_instance.registered_callback.callback
        if not isinstance(callback, DefaultPlaceholder):
            doc = inspect.getdoc(callback or "")
    # Priority 6: Implicit inference from callback docstring in typer.Typer()
        instance_callback = typer_info.typer_instance.info.callback
        if not isinstance(instance_callback, DefaultPlaceholder):
            doc = inspect.getdoc(instance_callback)
    # Value not set, use the default
    return typer_info.help.value
def solve_typer_info_defaults(typer_info: TyperInfo) -> TyperInfo:
    for name, value in typer_info.__dict__.items():
        # Priority 1: Value was set in app.add_typer()
        if not isinstance(value, DefaultPlaceholder):
        # Priority 2: Value was set in @subapp.callback()
            callback_value = getattr(
                typer_info.typer_instance.registered_callback,  # type: ignore
            if not isinstance(callback_value, DefaultPlaceholder):
                values[name] = callback_value
        # Priority 3: Value set in subapp = typer.Typer()
            instance_value = getattr(
                typer_info.typer_instance.info,  # type: ignore
            if not isinstance(instance_value, DefaultPlaceholder):
                values[name] = instance_value
        values[name] = value.value
    values["help"] = solve_typer_info_help(typer_info)
    return TyperInfo(**values)
def get_group_from_info(
    group_info: TyperInfo,
    pretty_exceptions_short: bool,
    suggest_commands: bool,
    rich_markup_mode: MarkupMode,
) -> TyperGroup:
    assert group_info.typer_instance, (
        "A Typer instance is needed to generate a Click Group"
    commands: dict[str, click.Command] = {}
    for command_info in group_info.typer_instance.registered_commands:
        command = get_command_from_info(
            command_info=command_info,
            pretty_exceptions_short=pretty_exceptions_short,
            rich_markup_mode=rich_markup_mode,
        if command.name:
            commands[command.name] = command
    for sub_group_info in group_info.typer_instance.registered_groups:
        sub_group = get_group_from_info(
            sub_group_info,
            suggest_commands=suggest_commands,
        if sub_group.name:
            commands[sub_group.name] = sub_group
            if sub_group.callback:
                    "The 'callback' parameter is not supported by Typer when using `add_typer` without a name",
            for sub_command_name, sub_command in sub_group.commands.items():
                commands[sub_command_name] = sub_command
    solved_info = solve_typer_info_defaults(group_info)
        convertors,
        context_param_name,
    ) = get_params_convertors_ctx_param_name_from_function(solved_info.callback)
    cls = solved_info.cls or TyperGroup
    assert issubclass(cls, TyperGroup), f"{cls} should be a subclass of {TyperGroup}"
    group = cls(
        name=solved_info.name or "",
        commands=commands,
        invoke_without_command=solved_info.invoke_without_command,
        no_args_is_help=solved_info.no_args_is_help,
        subcommand_metavar=solved_info.subcommand_metavar,
        chain=solved_info.chain,
        result_callback=solved_info.result_callback,
        context_settings=solved_info.context_settings,
        callback=get_callback(
            callback=solved_info.callback,
            convertors=convertors,
            context_param_name=context_param_name,
        help=solved_info.help,
        epilog=solved_info.epilog,
        short_help=solved_info.short_help,
        options_metavar=solved_info.options_metavar,
        add_help_option=solved_info.add_help_option,
        hidden=solved_info.hidden,
        deprecated=solved_info.deprecated,
        rich_help_panel=solved_info.rich_help_panel,
def get_command_name(name: str) -> str:
    return name.lower().replace("_", "-")
def get_params_convertors_ctx_param_name_from_function(
    callback: Optional[Callable[..., Any]],
) -> tuple[list[Union[click.Argument, click.Option]], dict[str, Any], Optional[str]]:
    convertors = {}
    context_param_name = None
    if callback:
        parameters = get_params_from_function(callback)
        for param_name, param in parameters.items():
            if lenient_issubclass(param.annotation, click.Context):
                context_param_name = param_name
            click_param, convertor = get_click_param(param)
            if convertor:
                convertors[param_name] = convertor
            params.append(click_param)
    return params, convertors, context_param_name
def get_command_from_info(
    command_info: CommandInfo,
) -> click.Command:
    assert command_info.callback, "A command must have a callback function"
    name = command_info.name or get_command_name(command_info.callback.__name__)  # ty: ignore[possibly-missing-attribute]
    use_help = command_info.help
    if use_help is None:
        use_help = inspect.getdoc(command_info.callback)
        use_help = inspect.cleandoc(use_help)
    ) = get_params_convertors_ctx_param_name_from_function(command_info.callback)
    cls = command_info.cls or TyperCommand
    command = cls(
        context_settings=command_info.context_settings,
            callback=command_info.callback,
        params=params,  # type: ignore
        help=use_help,
        epilog=command_info.epilog,
        short_help=command_info.short_help,
        options_metavar=command_info.options_metavar,
        add_help_option=command_info.add_help_option,
        no_args_is_help=command_info.no_args_is_help,
        hidden=command_info.hidden,
        deprecated=command_info.deprecated,
        rich_help_panel=command_info.rich_help_panel,
def determine_type_convertor(type_: Any) -> Optional[Callable[[Any], Any]]:
    convertor: Optional[Callable[[Any], Any]] = None
    if lenient_issubclass(type_, Path):
        convertor = param_path_convertor
    if lenient_issubclass(type_, Enum):
        convertor = generate_enum_convertor(type_)
    return convertor
def param_path_convertor(value: Optional[str] = None) -> Optional[Path]:
        # allow returning any subclass of Path created by an annotated parser without converting
        # it back to a Path
        return value if isinstance(value, Path) else Path(value)
def generate_enum_convertor(enum: type[Enum]) -> Callable[[Any], Any]:
    val_map = {str(val.value): val for val in enum}
    def convertor(value: Any) -> Any:
            val = str(value)
            if val in val_map:
                key = val_map[val]
                return enum(key)
def generate_list_convertor(
    convertor: Optional[Callable[[Any], Any]], default_value: Optional[Any]
) -> Callable[[Optional[Sequence[Any]]], Optional[list[Any]]]:
    def internal_convertor(value: Optional[Sequence[Any]]) -> Optional[list[Any]]:
        if (value is None) or (default_value is None and len(value) == 0):
        return [convertor(v) if convertor else v for v in value]
    return internal_convertor
def generate_tuple_convertor(
    types: Sequence[Any],
) -> Callable[[Optional[tuple[Any, ...]]], Optional[tuple[Any, ...]]]:
    convertors = [determine_type_convertor(type_) for type_ in types]
    def internal_convertor(
        param_args: Optional[tuple[Any, ...]],
    ) -> Optional[tuple[Any, ...]]:
        if param_args is None:
            convertor(arg) if convertor else arg
            for (convertor, arg) in zip(convertors, param_args)
def get_callback(
    callback: Optional[Callable[..., Any]] = None,
    params: Sequence[click.Parameter] = [],
    convertors: Optional[dict[str, Callable[[str], Any]]] = None,
    context_param_name: Optional[str] = None,
) -> Optional[Callable[..., Any]]:
    use_convertors = convertors or {}
    if not callback:
    use_params: dict[str, Any] = {}
    for param_name in parameters:
        use_params[param_name] = None
        if param.name:
            use_params[param.name] = param.default
    def wrapper(**kwargs: Any) -> Any:
        _rich_traceback_guard = pretty_exceptions_short  # noqa: F841
            if k in use_convertors:
                use_params[k] = use_convertors[k](v)
                use_params[k] = v
        if context_param_name:
            use_params[context_param_name] = click.get_current_context()
        return callback(**use_params)
    update_wrapper(wrapper, callback)
def get_click_type(
    *, annotation: Any, parameter_info: ParameterInfo
) -> click.ParamType:
    if parameter_info.click_type is not None:
        return parameter_info.click_type
    elif parameter_info.parser is not None:
        return click.types.FuncParamType(parameter_info.parser)
    elif annotation is str:
        return click.STRING
    elif annotation is int:
        if parameter_info.min is not None or parameter_info.max is not None:
            min_ = None
            max_ = None
            if parameter_info.min is not None:
                min_ = int(parameter_info.min)
            if parameter_info.max is not None:
                max_ = int(parameter_info.max)
            return click.IntRange(min=min_, max=max_, clamp=parameter_info.clamp)
            return click.INT
    elif annotation is float:
            return click.FloatRange(
                min=parameter_info.min,
                max=parameter_info.max,
                clamp=parameter_info.clamp,
            return click.FLOAT
    elif annotation is bool:
        return click.BOOL
    elif annotation == UUID:
        return click.UUID
    elif annotation == datetime:
        return click.DateTime(formats=parameter_info.formats)
        annotation == Path
        or parameter_info.allow_dash
        or parameter_info.path_type
        or parameter_info.resolve_path
        return TyperPath(
            exists=parameter_info.exists,
            file_okay=parameter_info.file_okay,
            dir_okay=parameter_info.dir_okay,
            writable=parameter_info.writable,
            readable=parameter_info.readable,
            resolve_path=parameter_info.resolve_path,
            allow_dash=parameter_info.allow_dash,
            path_type=parameter_info.path_type,
    elif lenient_issubclass(annotation, FileTextWrite):
        return click.File(
            mode=parameter_info.mode or "w",
            encoding=parameter_info.encoding,
            errors=parameter_info.errors,
            lazy=parameter_info.lazy,
            atomic=parameter_info.atomic,
    elif lenient_issubclass(annotation, FileText):
            mode=parameter_info.mode or "r",
    elif lenient_issubclass(annotation, FileBinaryRead):
            mode=parameter_info.mode or "rb",
    elif lenient_issubclass(annotation, FileBinaryWrite):
            mode=parameter_info.mode or "wb",
    elif lenient_issubclass(annotation, Enum):
        # The custom TyperChoice is only needed for Click < 8.2.0, to parse the
        # command line values matching them to the enum values. Click 8.2.0 added
        # support for enum values but reading enum names.
        # Passing here the list of enum values (instead of just the enum) accounts for
        # Click < 8.2.0.
        return TyperChoice(
            [item.value for item in annotation],
            case_sensitive=parameter_info.case_sensitive,
    elif is_literal_type(annotation):
        return click.Choice(
            literal_values(annotation),
    raise RuntimeError(f"Type not yet supported: {annotation}")  # pragma: no cover
    cls: Any, class_or_tuple: Union[AnyType, tuple[AnyType, ...]]
def get_click_param(
    param: ParamMeta,
) -> tuple[Union[click.Argument, click.Option], Any]:
    # First, find out what will be:
    # * ParamInfo (ArgumentInfo or OptionInfo)
    # * default_value
    # * required
    default_value = None
    required = False
    if isinstance(param.default, ParameterInfo):
        parameter_info = param.default
        if parameter_info.default == Required:
            required = True
            default_value = parameter_info.default
    elif param.default == Required or param.default is param.empty:
        parameter_info = ArgumentInfo()
        default_value = param.default
        parameter_info = OptionInfo()
    if param.annotation is not param.empty:
        annotation = param.annotation
        annotation = str
    main_type = annotation
    is_list = False
    is_tuple = False
    parameter_type: Any = None
    is_flag = None
    origin = get_origin(main_type)
        # Handle SomeType | None and Optional[SomeType]
            for type_ in get_args(main_type):
                if type_ is NoneType:
                types.append(type_)
            assert len(types) == 1, "Typer Currently doesn't support Union types"
            main_type = types[0]
        # Handle Tuples and Lists
        if lenient_issubclass(origin, list):
            main_type = get_args(main_type)[0]
            assert not get_origin(main_type), (
                "List types with complex sub-types are not currently supported"
            is_list = True
        elif lenient_issubclass(origin, tuple):
                assert not get_origin(type_), (
                    "Tuple types with complex sub-types are not currently supported"
                types.append(
                    get_click_type(annotation=type_, parameter_info=parameter_info)
            parameter_type = tuple(types)
            is_tuple = True
    if parameter_type is None:
        parameter_type = get_click_type(
            annotation=main_type, parameter_info=parameter_info
    convertor = determine_type_convertor(main_type)
    if is_list:
        convertor = generate_list_convertor(
            convertor=convertor, default_value=default_value
    if is_tuple:
        convertor = generate_tuple_convertor(get_args(main_type))
    if isinstance(parameter_info, OptionInfo):
        if main_type is bool:
            is_flag = True
            # Click doesn't accept a flag of type bool, only None, and then it sets it
            # to bool internally
            parameter_type = None
        default_option_name = get_command_name(param.name)
        if is_flag:
            default_option_declaration = (
                f"--{default_option_name}/--no-{default_option_name}"
            default_option_declaration = f"--{default_option_name}"
        param_decls = [param.name]
        if parameter_info.param_decls:
            param_decls.extend(parameter_info.param_decls)
            param_decls.append(default_option_declaration)
            TyperOption(
                # Option
                param_decls=param_decls,
                show_default=parameter_info.show_default,
                prompt=parameter_info.prompt,
                confirmation_prompt=parameter_info.confirmation_prompt,
                prompt_required=parameter_info.prompt_required,
                hide_input=parameter_info.hide_input,
                is_flag=is_flag,
                multiple=is_list,
                count=parameter_info.count,
                allow_from_autoenv=parameter_info.allow_from_autoenv,
                type=parameter_type,
                help=parameter_info.help,
                hidden=parameter_info.hidden,
                show_choices=parameter_info.show_choices,
                show_envvar=parameter_info.show_envvar,
                # Parameter
                default=default_value,
                callback=get_param_callback(
                    callback=parameter_info.callback, convertor=convertor
                metavar=parameter_info.metavar,
                expose_value=parameter_info.expose_value,
                is_eager=parameter_info.is_eager,
                envvar=parameter_info.envvar,
                shell_complete=parameter_info.shell_complete,
                autocompletion=get_param_completion(parameter_info.autocompletion),
                rich_help_panel=parameter_info.rich_help_panel,
            convertor,
    elif isinstance(parameter_info, ArgumentInfo):
        nargs = None
            nargs = -1
            TyperArgument(
                # Argument
                nargs=nargs,
                # TyperArgument
    raise AssertionError("A click.Parameter should be returned")  # pragma: no cover
def get_param_callback(
    convertor: Optional[Callable[..., Any]] = None,
    ctx_name = None
    click_param_name = None
    value_name = None
    untyped_names: list[str] = []
    for param_name, param_sig in parameters.items():
        if lenient_issubclass(param_sig.annotation, click.Context):
            ctx_name = param_name
        elif lenient_issubclass(param_sig.annotation, click.Parameter):
            click_param_name = param_name
            untyped_names.append(param_name)
    # Extract value param name first
    if untyped_names:
        value_name = untyped_names.pop()
    # If context and Click param were not typed (old/Click callback style) extract them
        if ctx_name is None:
            ctx_name = untyped_names.pop(0)
        if click_param_name is None:
                click_param_name = untyped_names.pop(0)
            raise click.ClickException(
                "Too many CLI parameter callback function parameters"
    def wrapper(ctx: click.Context, param: click.Parameter, value: Any) -> Any:
        if ctx_name:
            use_params[ctx_name] = ctx
        if click_param_name:
            use_params[click_param_name] = param
        if value_name:
                use_value = convertor(value)
                use_value = value
            use_params[value_name] = use_value
def get_param_completion(
    args_name = None
    incomplete_name = None
    unassigned_params = list(parameters.values())
    for param_sig in unassigned_params[:]:
        origin = get_origin(param_sig.annotation)
            ctx_name = param_sig.name
            unassigned_params.remove(param_sig)
        elif lenient_issubclass(origin, list):
            args_name = param_sig.name
        elif lenient_issubclass(param_sig.annotation, str):
            incomplete_name = param_sig.name
    # If there are still unassigned parameters (not typed), extract by name
        if ctx_name is None and param_sig.name == "ctx":
        elif args_name is None and param_sig.name == "args":
        elif incomplete_name is None and param_sig.name == "incomplete":
    if unassigned_params:
        show_params = " ".join([param.name for param in unassigned_params])
            f"Invalid autocompletion callback parameters: {show_params}"
    def wrapper(ctx: click.Context, args: list[str], incomplete: Optional[str]) -> Any:
        if args_name:
            use_params[args_name] = args
        if incomplete_name:
            use_params[incomplete_name] = incomplete
    function: Annotated[
        Callable[..., Any],
            The function that should power this CLI application.
    This function converts a given function to a CLI application with `Typer()` and executes it.
    def main(name: str):
        print(f"Hello {name}")
        typer.run(main)
    app = Typer(add_completion=False)
    app.command()(function)
    app()
def _is_macos() -> bool:
    return platform.system() == "Darwin"
def _is_linux_or_bsd() -> bool:
    return "BSD" in platform.system()
def launch(
    url: Annotated[
            URL or filename of the thing to launch.
    wait: Annotated[
            Wait for the program to exit before returning. This only works if the launched program blocks.
            In particular, `xdg-open` on Linux does not block.
    locate: Annotated[
            If this is set to `True`, then instead of launching the application associated with the URL, it will attempt to
            launch a file manager with the file located. This might have weird effects if the URL does not point to the filesystem.
    This function launches the given URL (or filename) in the default
    viewer application for this file type.  If this is an executable, it
    might launch the executable in a new session.  The return value is
    the exit code of the launched application.  Usually, `0` indicates
    success.
    This function handles url in different operating systems separately:
     - On macOS (Darwin), it uses the `open` command.
     - On Linux and BSD, it uses `xdg-open` if available.
     - On Windows (and other OSes), it uses the standard webbrowser module.
    The function avoids, when possible, using the webbrowser module on Linux and macOS
    to prevent spammy terminal messages from some browsers (e.g., Chrome).
        typer.launch("https://typer.tiangolo.com/")
        typer.launch("/my/downloaded/file", locate=True)
        if _is_macos():
            return subprocess.Popen(
                ["open", url], stdout=subprocess.DEVNULL, stderr=subprocess.STDOUT
            ).wait()
        has_xdg_open = _is_linux_or_bsd() and shutil.which("xdg-open") is not None
        if has_xdg_open:
                ["xdg-open", url], stdout=subprocess.DEVNULL, stderr=subprocess.STDOUT
        webbrowser.open(url)
        return click.launch(url)
#  Thank you ! We ❤️ you! - Krrish & Ishaan
from concurrent import futures
from concurrent.futures import FIRST_COMPLETED, ThreadPoolExecutor, wait
from litellm._uuid import uuid
    from aiohttp import ClientSession
from typing_extensions import overload
import litellm
# client must be imported from litellm as it's a decorator used at function definition time
from litellm import client
# Other utils are imported directly to avoid circular imports
from litellm.utils import exception_type, get_litellm_params, get_optional_params
# Logging is imported lazily when needed to avoid loading litellm_logging at import time
    from litellm.litellm_core_utils.litellm_logging import Logging
    DEFAULT_MOCK_RESPONSE_COMPLETION_TOKEN_COUNT,
    DEFAULT_MOCK_RESPONSE_PROMPT_TOKEN_COUNT,
from litellm.exceptions import LiteLLMUnknownProvider
from litellm.litellm_core_utils.asyncify import run_async_function
from litellm.litellm_core_utils.audio_utils.utils import (
    calculate_request_duration,
    get_audio_file_for_health_check,
from litellm.litellm_core_utils.dd_tracing import tracer
from litellm.litellm_core_utils.get_provider_specific_headers import (
    ProviderSpecificHeaderUtils,
from litellm.litellm_core_utils.health_check_utils import (
    _create_health_check_response,
    _filter_model_params,
from litellm.litellm_core_utils.litellm_logging import Logging as LiteLLMLoggingObj
from litellm.litellm_core_utils.mock_functions import (
    mock_embedding,
    mock_image_generation,
from litellm.litellm_core_utils.prompt_templates.common_utils import (
    get_content_from_model_response,
from litellm.llms.base_llm import BaseConfig, BaseImageGenerationConfig
from litellm.llms.base_llm.base_model_iterator import (
    convert_model_response_to_streaming,
from litellm.llms.bedrock.common_utils import BedrockModelInfo
from litellm.llms.cohere.common_utils import CohereModelInfo
from litellm.llms.openai_like.json_loader import JSONProviderRegistry
from litellm.llms.vertex_ai.common_utils import (
    VertexAIModelRoute,
    get_vertex_ai_model_route,
from litellm.realtime_api.main import _realtime_health_check
from litellm.secret_managers.main import get_secret_bool, get_secret_str
from litellm.types.router import GenericLiteLLMParams
    ModelResponseStream,
    RawRequestTypedDict,
    StreamingChoices,
from litellm.utils import (
    Choices,
    CustomStreamWrapper,
    EmbeddingResponse,
    ProviderConfigManager,
    TextChoices,
    TextCompletionResponse,
    TextCompletionStreamWrapper,
    TranscriptionResponse,
    Usage,
    _get_model_info_helper,
    add_provider_specific_params_to_optional_params,
    async_mock_completion_streaming_obj,
    convert_to_model_response_object,
    create_pretrained_tokenizer,
    create_tokenizer,
    get_api_key,
    get_llm_provider,
    get_non_default_completion_params,
    get_non_default_transcription_params,
    get_optional_params_embeddings,
    get_optional_params_image_gen,
    get_optional_params_transcription,
    get_requester_metadata,
    get_secret,
    get_standard_openai_params,
    mock_completion_streaming_obj,
    pre_process_non_default_params,
    read_config_args,
    should_run_mock_completion,
    supports_httpx_timeout,
    token_counter,
    validate_and_fix_openai_messages,
    validate_and_fix_openai_tools,
    validate_chat_completion_tool_choice,
    validate_openai_optional_params,
from ._logging import verbose_logger
from .caching.caching import disable_cache, enable_cache, update_cache
from .litellm_core_utils.core_helpers import safe_deep_copy
from .litellm_core_utils.fallback_utils import (
    async_completion_with_fallbacks,
    completion_with_fallbacks,
from .litellm_core_utils.prompt_templates.common_utils import (
    get_completion_messages,
    update_messages_with_model_file_ids,
from .litellm_core_utils.prompt_templates.factory import (
    custom_prompt,
    function_call_prompt,
    map_system_message_pt,
    ollama_pt,
    prompt_factory,
    stringify_json_tool_call_content,
from .litellm_core_utils.streaming_chunk_builder_utils import ChunkProcessor
from .llms.anthropic.chat import AnthropicChatCompletion
from .llms.azure.audio_transcriptions import AzureAudioTranscription
from .llms.azure.azure import AzureChatCompletion, _check_dynamic_azure_params
from .llms.azure.chat.o_series_handler import AzureOpenAIO1ChatCompletion
from .llms.azure.completion.handler import AzureTextCompletion
from .llms.azure_ai.anthropic.handler import AzureAnthropicChatCompletion
from .llms.azure_ai.embed import AzureAIEmbedding
from .llms.bedrock.chat import BedrockConverseLLM, BedrockLLM
from .llms.bedrock.embed.embedding import BedrockEmbedding
from .llms.bedrock.image_edit.handler import BedrockImageEdit
from .llms.bedrock.image_generation.image_handler import BedrockImageGeneration
from .llms.bytez.chat.transformation import BytezChatConfig
from .llms.clarifai.chat.transformation import ClarifaiConfig
from .llms.codestral.completion.handler import CodestralTextCompletion
from .llms.cohere.embed import handler as cohere_embed
from .llms.custom_httpx.aiohttp_handler import BaseLLMAIOHTTPHandler
from .llms.custom_httpx.llm_http_handler import BaseLLMHTTPHandler
from .llms.custom_llm import CustomLLM, custom_chat_llm_router
from .llms.databricks.embed.handler import DatabricksEmbeddingHandler
from .llms.deprecated_providers import aleph_alpha, palm
from .llms.gemini.common_utils import get_api_key_from_env
from .llms.groq.chat.handler import GroqChatCompletion
from .llms.heroku.chat.transformation import HerokuChatConfig
from .llms.huggingface.embedding.handler import HuggingFaceEmbedding
from .llms.lemonade.chat.transformation import LemonadeChatConfig
from .llms.nlp_cloud.chat.handler import completion as nlp_cloud_chat_completion
from .llms.oci.chat.transformation import OCIChatConfig
from .llms.ollama.completion import handler as ollama
from .llms.oobabooga.chat import oobabooga
from .llms.openai.completion.handler import OpenAITextCompletion
from .llms.openai.image_variations.handler import OpenAIImageVariationsHandler
from .llms.openai.openai import OpenAIChatCompletion
from .llms.openai.transcriptions.handler import OpenAIAudioTranscription
from .llms.openai_like.chat.handler import OpenAILikeChatHandler
from .llms.openai_like.embedding.handler import OpenAILikeEmbeddingHandler
from .llms.ovhcloud.chat.transformation import OVHCloudChatConfig
from .llms.petals.completion import handler as petals_handler
from .llms.predibase.chat.handler import PredibaseChatCompletion
from .llms.replicate.chat.handler import completion as replicate_chat_completion
from .llms.sagemaker.chat.handler import SagemakerChatHandler
from .llms.sagemaker.completion.handler import SagemakerLLM
from .llms.sap.chat.handler import GenAIHubOrchestration
from .llms.vertex_ai import vertex_ai_non_gemini
from .llms.vertex_ai.gemini.vertex_and_google_ai_studio_gemini import VertexLLM
from .llms.vertex_ai.gemini_embeddings.batch_embed_content_handler import (
    GoogleBatchEmbeddings,
from .llms.vertex_ai.image_generation.image_generation_handler import (
    VertexImageGeneration,
from .llms.vertex_ai.multimodal_embeddings.embedding_handler import (
    VertexMultimodalEmbedding,
from .llms.vertex_ai.vertex_ai_partner_models.main import VertexAIPartnerModels
from .llms.vertex_ai.vertex_embeddings.embedding_handler import VertexEmbedding
from .llms.vertex_ai.vertex_gemma_models.main import VertexAIGemmaModels
from .llms.vertex_ai.vertex_model_garden.main import VertexAIModelGardenModels
from .llms.vllm.completion import handler as vllm_handler
from .llms.watsonx.chat.handler import WatsonXChatHandler
from .llms.watsonx.common_utils import IBMWatsonXMixin
from .types.llms.anthropic import AnthropicThinkingParam
from .types.llms.openai import (
    ChatCompletionAssistantMessage,
    ChatCompletionModality,
    ChatCompletionPredictionContentParam,
    ChatCompletionUserMessage,
    HttpxBinaryResponseContent,
    OpenAIModerationResponse,
    OpenAIWebSearchOptions,
from .types.utils import (
    AdapterCompletionStreamWrapper,
    ChatCompletionMessageToolCall,
    CompletionTokensDetails,
    HiddenParams,
    LlmProviders,
    PromptTokensDetails,
    ProviderSpecificHeader,
    all_litellm_params,
####### ENVIRONMENT VARIABLES ###################
openai_chat_completions = OpenAIChatCompletion()
openai_text_completions = OpenAITextCompletion()
openai_audio_transcriptions = OpenAIAudioTranscription()
openai_image_variations = OpenAIImageVariationsHandler()
groq_chat_completions = GroqChatCompletion()
sap_gen_ai_hub_chat_completions = GenAIHubOrchestration()
sap_gen_ai_hub_emb = GenAIHubOrchestration()
azure_ai_embedding = AzureAIEmbedding()
anthropic_chat_completions = AnthropicChatCompletion()
azure_anthropic_chat_completions = AzureAnthropicChatCompletion()
azure_chat_completions = AzureChatCompletion()
azure_o1_chat_completions = AzureOpenAIO1ChatCompletion()
azure_text_completions = AzureTextCompletion()
azure_audio_transcriptions = AzureAudioTranscription()
huggingface_embed = HuggingFaceEmbedding()
predibase_chat_completions = PredibaseChatCompletion()
codestral_text_completions = CodestralTextCompletion()
bedrock_converse_chat_completion = BedrockConverseLLM()
bedrock_embedding = BedrockEmbedding()
bedrock_image_generation = BedrockImageGeneration()
bedrock_image_edit = BedrockImageEdit()
vertex_chat_completion = VertexLLM()
vertex_embedding = VertexEmbedding()
vertex_multimodal_embedding = VertexMultimodalEmbedding()
vertex_image_generation = VertexImageGeneration()
google_batch_embeddings = GoogleBatchEmbeddings()
vertex_partner_models_chat_completion = VertexAIPartnerModels()
vertex_gemma_chat_completion = VertexAIGemmaModels()
vertex_model_garden_chat_completion = VertexAIModelGardenModels()
# vertex_text_to_speech is now replaced by VertexAITextToSpeechConfig
sagemaker_llm = SagemakerLLM()
watsonx_chat_completion = WatsonXChatHandler()
openai_like_embedding = OpenAILikeEmbeddingHandler()
openai_like_chat_completion = OpenAILikeChatHandler()
databricks_embedding = DatabricksEmbeddingHandler()
base_llm_http_handler = BaseLLMHTTPHandler()
base_llm_aiohttp_handler = BaseLLMAIOHTTPHandler()
sagemaker_chat_completion = SagemakerChatHandler()
bytez_transformation = BytezChatConfig()
heroku_transformation = HerokuChatConfig()
oci_transformation = OCIChatConfig()
ovhcloud_transformation = OVHCloudChatConfig()
lemonade_transformation = LemonadeChatConfig()
MOCK_RESPONSE_TYPE = Union[str, Exception, dict, ModelResponse, ModelResponseStream]
####### COMPLETION ENDPOINTS ################
class LiteLLM:
        api_key=None,
        organization: Optional[str] = None,
        timeout: Optional[float] = 600,
        max_retries: Optional[int] = litellm.num_retries,
        default_headers: Optional[Mapping[str, str]] = None,
        self.params = locals()
        self.chat = Chat(self.params, router_obj=None)
class Chat:
    def __init__(self, params, router_obj: Optional[Any]):
        if self.params.get("acompletion", False) is True:
            self.params.pop("acompletion")
            self.completions: Union[AsyncCompletions, Completions] = AsyncCompletions(
                self.params, router_obj=router_obj
            self.completions = Completions(self.params, router_obj=router_obj)
class Completions:
        self.router_obj = router_obj
    def create(self, messages, model=None, **kwargs):
            self.params[k] = v
        model = model or self.params.get("model")
        if self.router_obj is not None:
            response = self.router_obj.completion(
                model=model, messages=messages, **self.params
            response = completion(model=model, messages=messages, **self.params)
class AsyncCompletions:
    async def create(self, messages, model=None, **kwargs):
            response = await self.router_obj.acompletion(
            response = await acompletion(model=model, messages=messages, **self.params)
@tracer.wrap()
@client
async def acompletion(  # noqa: PLR0915
    # Optional OpenAI params: see https://platform.openai.com/docs/api-reference/chat/create
    messages: List = [],
    functions: Optional[List] = None,
    function_call: Optional[str] = None,
    timeout: Optional[Union[float, int]] = None,
    stream_options: Optional[dict] = None,
    stop=None,
    max_completion_tokens: Optional[int] = None,
    modalities: Optional[List[ChatCompletionModality]] = None,
    prediction: Optional[ChatCompletionPredictionContentParam] = None,
    audio: Optional[ChatCompletionAudioParam] = None,
    logit_bias: Optional[dict] = None,
    user: Optional[str] = None,
    # openai v1.0+ new params
    response_format: Optional[Union[dict, Type[BaseModel]]] = None,
    tools: Optional[List] = None,
    tool_choice: Optional[Union[str, dict]] = None,
    parallel_tool_calls: Optional[bool] = None,
    deployment_id=None,
    reasoning_effort: Optional[
        Literal["none", "minimal", "low", "medium", "high", "xhigh", "default"]
    verbosity: Optional[Literal["low", "medium", "high"]] = None,
    safety_identifier: Optional[str] = None,
    service_tier: Optional[str] = None,
    # set api_base, api_version, api_key
    api_version: Optional[str] = None,
    model_list: Optional[list] = None,  # pass in a list of api_base,keys, etc.
    extra_headers: Optional[dict] = None,
    # Optional liteLLM function params
    thinking: Optional[AnthropicThinkingParam] = None,
    web_search_options: Optional[OpenAIWebSearchOptions] = None,
    # Session management
    shared_session: Optional["ClientSession"] = None,
) -> Union[ModelResponse, CustomStreamWrapper]:
    Asynchronously executes a litellm.completion() call for any of litellm supported llms (example gpt-4, gpt-3.5-turbo, claude-2, command-nightly)
        model (str): The name of the language model to use for text completion. see all supported LLMs: https://docs.litellm.ai/docs/providers/
        messages (List): A list of message objects representing the conversation context (default is an empty list).
        OPTIONAL PARAMS
        functions (List, optional): A list of functions to apply to the conversation messages (default is an empty list).
        function_call (str, optional): The name of the function to call within the conversation (default is an empty string).
        temperature (float, optional): The temperature parameter for controlling the randomness of the output (default is 1.0).
        top_p (float, optional): The top-p parameter for nucleus sampling (default is 1.0).
        n (int, optional): The number of completions to generate (default is 1).
        stream (bool, optional): If True, return a streaming response (default is False).
        stream_options (dict, optional): A dictionary containing options for the streaming response. Only use this if stream is True.
        stop(string/list, optional): - Up to 4 sequences where the LLM API will stop generating further tokens.
        max_tokens (integer, optional): The maximum number of tokens in the generated completion (default is infinity).
        max_completion_tokens (integer, optional): An upper bound for the number of tokens that can be generated for a completion, including visible output tokens and reasoning tokens.
        modalities (List[ChatCompletionModality], optional): Output types that you would like the model to generate for this request. You can use `["text", "audio"]`
        prediction (ChatCompletionPredictionContentParam, optional): Configuration for a Predicted Output, which can greatly improve response times when large parts of the model response are known ahead of time. This is most common when you are regenerating a file with only minor changes to most of the content.
        audio (ChatCompletionAudioParam, optional): Parameters for audio output. Required when audio output is requested with modalities: ["audio"]
        presence_penalty (float, optional): It is used to penalize new tokens based on their existence in the text so far.
        frequency_penalty: It is used to penalize new tokens based on their frequency in the text so far.
        logit_bias (dict, optional): Used to modify the probability of specific tokens appearing in the completion.
        user (str, optional):  A unique identifier representing your end-user. This can help the LLM provider to monitor and detect abuse.
        metadata (dict, optional): Pass in additional metadata to tag your completion calls - eg. prompt version, details, etc.
        api_base (str, optional): Base URL for the API (default is None).
        api_version (str, optional): API version (default is None).
        api_key (str, optional): API key (default is None).
        model_list (list, optional): List of api base, version, keys
        timeout (float, optional): The maximum execution time in seconds for the completion request.
        LITELLM Specific Params
        mock_response (str, optional): If provided, return a mock completion response for testing or debugging purposes (default is None).
        custom_llm_provider (str, optional): Used for Non-OpenAI LLMs, Example usage for bedrock, set model="amazon.titan-tg1-large" and custom_llm_provider="bedrock"
        ModelResponse: A response object containing the generated completion and associated metadata.
        - This function is an asynchronous version of the `completion` function.
        - The `completion` function is called using `run_in_executor` to execute synchronously in the event loop.
        - If `stream` is True, the function returns an async generator that yields completion lines.
    fallbacks = kwargs.get("fallbacks", None)
    mock_timeout = kwargs.get("mock_timeout", None)
    if mock_timeout is True:
        await _handle_mock_timeout_async(mock_timeout, timeout, model)
    custom_llm_provider = kwargs.get("custom_llm_provider", None)
    ## PROMPT MANAGEMENT HOOKS ##
    #########################################################
    litellm_logging_obj = kwargs.get("litellm_logging_obj", None)
    if isinstance(litellm_logging_obj, LiteLLMLoggingObj) and (
        litellm_logging_obj.should_run_prompt_management_hooks(
            prompt_id=kwargs.get("prompt_id", None),
            non_default_params=kwargs,
        ) = await litellm_logging_obj.async_get_chat_completion_prompt(
            prompt_variables=kwargs.get("prompt_variables", None),
            prompt_label=kwargs.get("prompt_label", None),
            prompt_version=kwargs.get("prompt_version", None),
        # if the chat completion logging hook removed all tools,
        # set tools to None
        # eg. in certain cases when users send vector stores as tools
        # we don't want the tools to go to the upstream llm
        # relevant issue: https://github.com/BerriAI/litellm/issues/11404
        if tools is not None and len(tools) == 0:
            tools = None
    # Log shared session usage
    if shared_session is not None:
        verbose_logger.debug(
            f"🔄 SHARED SESSION: acompletion called with shared_session (ID: {id(shared_session)})"
            "🔄 NO SHARED SESSION: acompletion called without shared_session"
    # Adjusted to use explicit arguments instead of *args and **kwargs
    completion_kwargs = {
        "deployment_id": deployment_id,
        "base_url": base_url,
        "api_version": api_version,
        "api_key": api_key,
        "model_list": model_list,
        "extra_headers": extra_headers,
        "acompletion": True,  # assuming this is a required parameter
        "thinking": thinking,
        "shared_session": shared_session,
    if custom_llm_provider is None:
        _, custom_llm_provider, _, _ = get_llm_provider(
            custom_llm_provider=custom_llm_provider,
            api_base=completion_kwargs.get("base_url", None),
    fallbacks = fallbacks or litellm.model_fallbacks
    if fallbacks is not None:
        response = await async_completion_with_fallbacks(
            **completion_kwargs, kwargs={"fallbacks": fallbacks, **kwargs}
                "No response from fallbacks. Got none. Turn on `litellm.set_verbose=True` to see more details."
    ### APPLY MOCK DELAY ###
    mock_delay = kwargs.get("mock_delay")
    mock_response = kwargs.get("mock_response")
    mock_tool_calls = kwargs.get("mock_tool_calls")
    mock_timeout = kwargs.get("mock_timeout")
    if mock_delay and should_run_mock_completion(
        mock_response=mock_response,
        mock_tool_calls=mock_tool_calls,
        mock_timeout=mock_timeout,
        await asyncio.sleep(mock_delay)
        # Use a partial function to pass your keyword arguments
        func = partial(completion, **completion_kwargs, **kwargs)
        # Add the context to the function
        ctx = contextvars.copy_context()
        func_with_context = partial(ctx.run, func)
        init_response = await loop.run_in_executor(None, func_with_context)
        if isinstance(init_response, dict) or isinstance(
            init_response, ModelResponse
        ):  ## CACHING SCENARIO
            if isinstance(init_response, dict):
                response = ModelResponse(**init_response)
            response = init_response
        elif asyncio.iscoroutine(init_response):
            response = await init_response
            response = init_response  # type: ignore
            custom_llm_provider == "text-completion-openai"
            or custom_llm_provider == "text-completion-codestral"
        ) and isinstance(response, TextCompletionResponse):
            response = litellm.OpenAITextCompletionConfig().convert_to_chat_model_response_object(
                response_object=response,
                model_response_object=litellm.ModelResponse(),
        if isinstance(response, CustomStreamWrapper):
            response.set_logging_event_loop(
                loop=loop
            )  # sets the logging event loop if the user does sync streaming (e.g. on proxy for sagemaker calls)
        custom_llm_provider = custom_llm_provider or "openai"
        raise exception_type(
            original_exception=e,
            completion_kwargs=completion_kwargs,
            extra_kwargs=kwargs,
async def _async_streaming(response, model, custom_llm_provider, args):
        print_verbose(f"received response in _async_streaming: {response}")
            response = await response
        async for line in response:
            print_verbose(f"line in async streaming: {line}")
def _handle_mock_potential_exceptions(
    mock_response: Union[str, Exception],
    custom_llm_provider: Optional[str] = None,
    if isinstance(mock_response, Exception):
        if isinstance(mock_response, openai.APIError):
            raise mock_response
        raise litellm.MockException(
            status_code=getattr(mock_response, "status_code", 500),  # type: ignore
            message=getattr(mock_response, "text", str(mock_response)),
            llm_provider=getattr(
                mock_response, "llm_provider", custom_llm_provider or "openai"
            ),  # type: ignore
            model=model,  # type: ignore
            request=httpx.Request(method="POST", url="https://api.openai.com/v1/"),
    elif isinstance(mock_response, str) and mock_response == "litellm.RateLimitError":
        raise litellm.RateLimitError(
            message="this is a mock rate limit error",
        isinstance(mock_response, str)
        and mock_response == "litellm.ContextWindowExceededError"
        raise litellm.ContextWindowExceededError(
            message="this is a mock context window exceeded error",
        and mock_response == "litellm.InternalServerError"
        raise litellm.InternalServerError(
            message="this is a mock internal server error",
    elif isinstance(mock_response, str) and mock_response.startswith(
        "Exception: content_filter_policy"
            message=mock_response,
            llm_provider="azure",
def _handle_mock_timeout(
    mock_timeout: Optional[bool],
    timeout: Optional[Union[float, str, httpx.Timeout]],
    if mock_timeout is True and timeout is not None:
        _sleep_for_timeout(timeout)
        raise litellm.Timeout(
            message="This is a mock timeout error",
            llm_provider="openai",
async def _handle_mock_timeout_async(
        await _sleep_for_timeout_async(timeout)
def _sleep_for_timeout(timeout: Union[float, str, httpx.Timeout]):
    if isinstance(timeout, float):
    elif isinstance(timeout, str):
        time.sleep(float(timeout))
    elif isinstance(timeout, httpx.Timeout) and timeout.connect is not None:
        time.sleep(timeout.connect)
async def _sleep_for_timeout_async(timeout: Union[float, str, httpx.Timeout]):
        await asyncio.sleep(timeout)
        await asyncio.sleep(float(timeout))
        await asyncio.sleep(timeout.connect)
def mock_completion(
    messages: List,
    stream: Optional[bool] = False,
    mock_response: Optional[MOCK_RESPONSE_TYPE] = "This is a mock request",
    mock_tool_calls: Optional[List] = None,
    mock_timeout: Optional[bool] = False,
    logging=None,
    custom_llm_provider=None,
    timeout: Optional[Union[float, str, httpx.Timeout]] = None,
    Generate a mock completion response for testing or debugging purposes.
    This is a helper function that simulates the response structure of the OpenAI completion API.
        model (str): The name of the language model for which the mock response is generated.
        messages (List): A list of message objects representing the conversation context.
        stream (bool, optional): If True, returns a mock streaming response (default is False).
        mock_response (str, optional): The content of the mock response (default is "This is a mock request").
        mock_timeout (bool, optional): If True, the mock response will be a timeout error (default is False).
        timeout (float, optional): The timeout value to use for the mock response (default is None).
        **kwargs: Additional keyword arguments that can be used but are not required.
        litellm.ModelResponse: A ModelResponse simulating a completion response with the specified model, messages, and mock response.
        Exception: If an error occurs during the generation of the mock completion response.
        - This function is intended for testing or debugging purposes to generate mock completion responses.
        - If 'stream' is True, it returns a response that mimics the behavior of a streaming completion.
        is_acompletion = kwargs.get("acompletion") or False
        if mock_response is None:
            mock_response = "This is a mock request"
        _handle_mock_timeout(mock_timeout=mock_timeout, timeout=timeout, model=model)
        ## LOGGING
        if logging is not None:
            logging.pre_call(
                input=messages,
                api_key="mock-key",
        if isinstance(mock_response, str) or isinstance(mock_response, Exception):
            _handle_mock_potential_exceptions(
        mock_response = cast(
            Union[str, dict, ModelResponse, ModelResponseStream], mock_response
        )  # after this point, mock_response is a string, dict, ModelResponse, or ModelResponseStream
        if isinstance(mock_response, str) and mock_response.startswith(
            "Exception: mock_streaming_error"
            mock_response = litellm.MockException(
                message="This is a mock error raised mid-stream",
                llm_provider="anthropic",
                status_code=529,
        time_delay = kwargs.get("mock_delay", None)
        if time_delay is not None and not is_acompletion:
            time.sleep(time_delay)
        if isinstance(mock_response, dict):
            return ModelResponse(**mock_response)
        if isinstance(mock_response, ModelResponse):
            # convert to ModelResponseStream
            mock_response = convert_model_response_to_streaming(mock_response)  # type: ignore
        model_response: Union[ModelResponse, ModelResponseStream] = ModelResponse()
        if stream is True:
            model_response = ModelResponseStream()
            # don't try to access stream object,
            if kwargs.get("acompletion", False) is True:
                return CustomStreamWrapper(
                    completion_stream=async_mock_completion_streaming_obj(
                        model_response, mock_response=mock_response, model=model, n=n
                    custom_llm_provider="openai",
                    logging_obj=logging,
                completion_stream=mock_completion_streaming_obj(
        if isinstance(mock_response, litellm.MockException):
        # At this point, mock_response must be a string (all other types have been handled or returned early)
        mock_response = cast(str, mock_response)
            model_response.choices[0].message.content = mock_response  # type: ignore
            _all_choices = []
                _choice = litellm.utils.Choices(
                    message=litellm.utils.Message(
                        content=mock_response, role="assistant"
                _all_choices.append(_choice)
            model_response.choices = _all_choices  # type: ignore
        model_response.created = int(time.time())
        model_response.model = model
        if mock_tool_calls:
            model_response.choices[0].message.tool_calls = [  # type: ignore
                ChatCompletionMessageToolCall(**tool_call)
                for tool_call in mock_tool_calls
            model_response,
            "usage",
            Usage(
                prompt_tokens=DEFAULT_MOCK_RESPONSE_PROMPT_TOKEN_COUNT,
                completion_tokens=DEFAULT_MOCK_RESPONSE_COMPLETION_TOKEN_COUNT,
                total_tokens=DEFAULT_MOCK_RESPONSE_PROMPT_TOKEN_COUNT
                + DEFAULT_MOCK_RESPONSE_COMPLETION_TOKEN_COUNT,
            _, custom_llm_provider, _, _ = litellm.utils.get_llm_provider(model=model)
            model_response._hidden_params["custom_llm_provider"] = custom_llm_provider
            # dont let setting a hidden param block a mock_respose
            logging.post_call(
                api_key="my-secret-key",
                original_response="my-original-response",
        return model_response
        if isinstance(e, openai.APIError):
        raise Exception("Mock completion response failed - {}".format(e))
def responses_api_bridge_check(
    custom_llm_provider: str,
) -> Tuple[dict, str]:
    model_info: Dict[str, Any] = {}
        model_info = cast(
            _get_model_info_helper(
        if model_info.get("mode") is None and model.startswith("responses/"):
            model = model.replace("responses/", "")
            mode = "responses"
            model_info["mode"] = mode
        if web_search_options is not None and custom_llm_provider == "xai":
            model_info["mode"] = "responses"
        verbose_logger.debug("Error getting model info: {}".format(e))
        if model.startswith(
            "responses/"
        ):  # handle azure models - `azure/responses/<deployment-name>`
    return model_info, model
def _should_allow_input_examples(
    custom_llm_provider: Optional[str], model: str
    if custom_llm_provider == "anthropic":
        custom_llm_provider == "azure_ai"
        or custom_llm_provider == "bedrock"
        or custom_llm_provider == "vertex_ai"
        return "claude" in model.lower()
def _drop_input_examples_from_tool(tool: dict) -> dict:
    tool_copy = tool.copy()
    tool_copy.pop("input_examples", None)
    function = tool_copy.get("function")
    if isinstance(function, dict):
        function = function.copy()
        function.pop("input_examples", None)
        tool_copy["function"] = function
    return tool_copy
def _drop_input_examples_from_tools(
    tools: Optional[List[dict]],
) -> Optional[List[dict]]:
    if tools is None:
    cleaned_tools: List[dict] = []
            cleaned_tools.append(_drop_input_examples_from_tool(tool))
            cleaned_tools.append(tool)
    return cleaned_tools
def completion(  # type: ignore # noqa: PLR0915
    # soon to be deprecated params by OpenAI
    Perform a completion() using any of litellm supported llms (example gpt-4, gpt-3.5-turbo, claude-2, command-nightly)
        stream_options (dict, optional): A dictionary containing options for the streaming response. Only set this when you set stream: true.
        modalities (List[ChatCompletionModality], optional): Output types that you would like the model to generate for this request.. You can use `["text", "audio"]`
        logprobs (bool, optional): Whether to return log probabilities of the output tokens or not. If true, returns the log probabilities of each output token returned in the content of message
        top_logprobs (int, optional): An integer between 0 and 5 specifying the number of most likely tokens to return at each token position, each with an associated log probability. logprobs must be set to true if this parameter is used.
        extra_headers (dict, optional): Additional headers to include in the request.
        max_retries (int, optional): The number of retries to attempt (default is 0).
        - This function is used to perform completions() using the specified language model.
        - It supports various optional parameters for customizing the completion behavior.
        - If 'mock_response' is provided, a mock completion response is returned for testing or debugging.
    ### VALIDATE Request ###
        raise ValueError("model param not passed in.")
    # validate messages
    messages = validate_and_fix_openai_messages(messages=messages)
    tools = validate_and_fix_openai_tools(tools=tools)
    # validate tool_choice
    tool_choice = validate_chat_completion_tool_choice(tool_choice=tool_choice)
    # validate optional params
    stop = validate_openai_optional_params(stop=stop)
    ######### unpacking kwargs #####################
    args = locals()
    skip_mcp_handler = kwargs.pop("_skip_mcp_handler", False)
    if not skip_mcp_handler and tools:
        from litellm.responses.mcp.chat_completions_handler import (
            acompletion_with_mcp,
        from litellm.responses.mcp.litellm_proxy_mcp_handler import (
            LiteLLM_Proxy_MCP_Handler,
        from litellm.types.llms.openai import ToolParam
        # Check if MCP tools are present (following responses pattern)
        # Cast tools to Optional[Iterable[ToolParam]] for type checking
        tools_for_mcp = cast(Optional[Iterable[ToolParam]], tools)
        if LiteLLM_Proxy_MCP_Handler._should_use_litellm_mcp_gateway(
            tools=tools_for_mcp
            # Return coroutine - acompletion will await it
            # completion() can return a coroutine when MCP tools are present, which acompletion() awaits
            return acompletion_with_mcp(  # type: ignore[return-value]
                deployment_id=deployment_id,
                model_list=model_list,
                thinking=thinking,
                shared_session=shared_session,
    api_base = kwargs.get("api_base", None)
    mock_response: Optional[MOCK_RESPONSE_TYPE] = kwargs.get("mock_response", None)
    mock_tool_calls = kwargs.get("mock_tool_calls", None)
    mock_timeout = cast(Optional[bool], kwargs.get("mock_timeout", None))
    force_timeout = kwargs.get("force_timeout", 600)  ## deprecated
    logger_fn = kwargs.get("logger_fn", None)
    verbose = kwargs.get("verbose", False)
    id = kwargs.get("id", None)
    metadata = kwargs.get("metadata", None)
    model_info = kwargs.get("model_info", None)
    proxy_server_request = kwargs.get("proxy_server_request", None)
    provider_specific_header = cast(
        Optional[ProviderSpecificHeader], kwargs.get("provider_specific_header", None)
    headers = kwargs.get("headers", None) or extra_headers
    ensure_alternating_roles: Optional[bool] = kwargs.get(
        "ensure_alternating_roles", None
    user_continue_message: Optional[ChatCompletionUserMessage] = kwargs.get(
        "user_continue_message", None
    assistant_continue_message: Optional[ChatCompletionAssistantMessage] = kwargs.get(
        "assistant_continue_message", None
        headers.update(extra_headers)
    # Inject proxy auth headers if configured
    if litellm.proxy_auth is not None:
            proxy_headers = litellm.proxy_auth.get_auth_headers()
            headers.update(proxy_headers)
            verbose_logger.warning(f"Failed to get proxy auth headers: {e}")
    num_retries = kwargs.get(
        "num_retries", None
    )  ## alt. param for 'max_retries'. Use this to pass retries w/ instructor.
    max_retries = kwargs.get("max_retries", None)
    cooldown_time = kwargs.get("cooldown_time", None)
    context_window_fallback_dict = kwargs.get("context_window_fallback_dict", None)
    organization = kwargs.get("organization", None)
    ### VERIFY SSL ###
    ssl_verify = kwargs.get("ssl_verify", None)
    ### CUSTOM MODEL COST ###
    input_cost_per_token = kwargs.get("input_cost_per_token", None)
    output_cost_per_token = kwargs.get("output_cost_per_token", None)
    input_cost_per_second = kwargs.get("input_cost_per_second", None)
    output_cost_per_second = kwargs.get("output_cost_per_second", None)
    ### CUSTOM PROMPT TEMPLATE ###
    initial_prompt_value = kwargs.get("initial_prompt_value", None)
    roles = kwargs.get("roles", None)
    final_prompt_value = kwargs.get("final_prompt_value", None)
    bos_token = kwargs.get("bos_token", None)
    eos_token = kwargs.get("eos_token", None)
    preset_cache_key = kwargs.get("preset_cache_key", None)
    hf_model_name = kwargs.get("hf_model_name", None)
    supports_system_message = kwargs.get("supports_system_message", None)
    base_model = kwargs.get("base_model", None)
    ### DISABLE FLAGS ###
    disable_add_transform_inline_image_block = kwargs.get(
        "disable_add_transform_inline_image_block", None
    ### TEXT COMPLETION CALLS ###
    text_completion = kwargs.get("text_completion", False)
    atext_completion = kwargs.get("atext_completion", False)
    ### ASYNC CALLS ###
    acompletion = kwargs.get("acompletion", False)
    client = kwargs.get("client", None)
    ### Admin Controls ###
    no_log = kwargs.get("no-log", False)
    ### PROMPT MANAGEMENT ###
    prompt_id = cast(Optional[str], kwargs.get("prompt_id", None))
    prompt_variables = cast(Optional[dict], kwargs.get("prompt_variables", None))
    ### COPY MESSAGES ### - related issue https://github.com/BerriAI/litellm/discussions/4489
    messages = get_completion_messages(
        ensure_alternating_roles=ensure_alternating_roles or False,
        user_continue_message=user_continue_message,
        assistant_continue_message=assistant_continue_message,
    ######## end of unpacking kwargs ###########
    non_default_params = get_non_default_completion_params(kwargs=kwargs)
    litellm_params = {}  # used to prevent unbound var errors
            prompt_id=prompt_id, non_default_params=non_default_params
            optional_params,
        ) = litellm_logging_obj.get_chat_completion_prompt(
            non_default_params=non_default_params,
            prompt_id=prompt_id,
            prompt_variables=prompt_variables,
            api_base = base_url
        if num_retries is not None:
            max_retries = num_retries
        logging: LiteLLMLoggingObj = cast(LiteLLMLoggingObj, litellm_logging_obj)
            return completion_with_fallbacks(**args)
        if model_list is not None:
            deployments = [
                m["litellm_params"] for m in model_list if m["model_name"] == model
            return litellm.batch_completion_models(deployments=deployments, **args)
        if litellm.model_alias_map and model in litellm.model_alias_map:
            model = litellm.model_alias_map[
            ]  # update the model to the actual value if an alias has been passed in
        model_response = ModelResponse()
        setattr(model_response, "usage", litellm.Usage())
            kwargs.get("azure", False) is True
        ):  # don't remove flag check, to remain backwards compatible for repos like Codium
            custom_llm_provider = "azure"
        if deployment_id is not None:  # azure llms
            model = deployment_id
        model, custom_llm_provider, dynamic_api_key, api_base = get_llm_provider(
            api_base=api_base,
        if not _should_allow_input_examples(
            custom_llm_provider=custom_llm_provider, model=model
            tools = _drop_input_examples_from_tools(tools=tools)
        if provider_specific_header is not None:
            headers.update(
                ProviderSpecificHeaderUtils.get_provider_specific_headers(
                    provider_specific_header=provider_specific_header,
        if model_response is not None and hasattr(model_response, "_hidden_params"):
            model_response._hidden_params["region_name"] = kwargs.get(
                "aws_region_name", None
            )  # support region-based pricing for bedrock
        ### TIMEOUT LOGIC ###
        timeout = timeout or kwargs.get("request_timeout", 600) or 600
        # set timeout for 10 minutes by default
        if isinstance(timeout, httpx.Timeout) and not supports_httpx_timeout(
            custom_llm_provider
            timeout = timeout.read or 600  # default 10 min timeout
        elif not isinstance(timeout, httpx.Timeout):
            timeout = float(timeout)  # type: ignore
        ### REGISTER CUSTOM MODEL PRICING -- IF GIVEN ###
        if input_cost_per_token is not None and output_cost_per_token is not None:
            litellm.register_model(
                    f"{custom_llm_provider}/{model}": {
                        "input_cost_per_token": input_cost_per_token,
                        "output_cost_per_token": output_cost_per_token,
                        "litellm_provider": custom_llm_provider,
            input_cost_per_second is not None
        ):  # time based pricing just needs cost in place
            output_cost_per_second = output_cost_per_second
                        "input_cost_per_second": input_cost_per_second,
                        "output_cost_per_second": output_cost_per_second,
        ### BUILD CUSTOM PROMPT TEMPLATE -- IF GIVEN ###
        custom_prompt_dict = {}  # type: ignore
            initial_prompt_value
            or roles
            or final_prompt_value
            or bos_token
            or eos_token
            custom_prompt_dict = {model: {}}
            if initial_prompt_value:
                custom_prompt_dict[model]["initial_prompt_value"] = initial_prompt_value
                custom_prompt_dict[model]["roles"] = roles
            if final_prompt_value:
                custom_prompt_dict[model]["final_prompt_value"] = final_prompt_value
            if bos_token:
                custom_prompt_dict[model]["bos_token"] = bos_token
            if eos_token:
                custom_prompt_dict[model]["eos_token"] = eos_token
        if kwargs.get("model_file_id_mapping"):
            messages = update_messages_with_model_file_ids(
                model_id=kwargs.get("model_info", {}).get("id", None),
                model_file_id_mapping=cast(
                    Dict[str, Dict[str, str]], kwargs.get("model_file_id_mapping")
        provider_config: Optional[BaseConfig] = None
        if custom_llm_provider is not None and custom_llm_provider in [
            provider.value for provider in LlmProviders
            provider_config = ProviderConfigManager.get_provider_chat_config(
                model=model, provider=LlmProviders(custom_llm_provider)
        if provider_config is not None:
            messages = provider_config.translate_developer_role_to_system_role(
                messages=messages
            supports_system_message is not None
            and isinstance(supports_system_message, bool)
            and supports_system_message is False
            messages = map_system_message_pt(messages=messages)
        if dynamic_api_key is not None:
            api_key = dynamic_api_key
        # check if user passed in any of the OpenAI optional params
        optional_param_args = {
            # params to identify the model
            "custom_llm_provider": custom_llm_provider,
            "max_retries": max_retries,
            "allowed_openai_params": kwargs.get("allowed_openai_params"),
        optional_params = get_optional_params(
            **optional_param_args, **non_default_params
        processed_non_default_params = pre_process_non_default_params(
            passed_params=optional_param_args,
            special_params=non_default_params,
            additional_drop_params=kwargs.get("additional_drop_params"),
            remove_sensitive_keys=True,
            add_provider_specific_params=True,
            provider_config=provider_config,
        if litellm.add_function_to_prompt and optional_params.get(
            "functions_unsupported_model", None
        ):  # if user opts to add it to prompt, when API doesn't support function calling
            functions_unsupported_model = optional_params.pop(
                "functions_unsupported_model"
            messages = function_call_prompt(
                messages=messages, functions=functions_unsupported_model
        # For logging - save the values of the litellm-specific params passed in
        litellm_params = get_litellm_params(
            acompletion=acompletion,
            force_timeout=force_timeout,
            logger_fn=logger_fn,
            litellm_call_id=kwargs.get("litellm_call_id", None),
            model_alias_map=litellm.model_alias_map,
            completion_call_id=id,
            model_info=model_info,
            proxy_server_request=proxy_server_request,
            preset_cache_key=preset_cache_key,
            no_log=no_log,
            input_cost_per_second=input_cost_per_second,
            input_cost_per_token=input_cost_per_token,
            output_cost_per_second=output_cost_per_second,
            output_cost_per_token=output_cost_per_token,
            cooldown_time=cooldown_time,
            text_completion=kwargs.get("text_completion"),
            azure_ad_token_provider=kwargs.get("azure_ad_token_provider"),
            user_continue_message=kwargs.get("user_continue_message"),
            base_model=base_model,
            litellm_trace_id=kwargs.get("litellm_trace_id"),
            litellm_session_id=kwargs.get("litellm_session_id"),
            hf_model_name=hf_model_name,
            custom_prompt_dict=custom_prompt_dict,
            litellm_metadata=kwargs.get("litellm_metadata"),
            disable_add_transform_inline_image_block=disable_add_transform_inline_image_block,
            drop_params=kwargs.get("drop_params"),
            ssl_verify=ssl_verify,
            merge_reasoning_content_in_choices=kwargs.get(
                "merge_reasoning_content_in_choices", None
            use_litellm_proxy=kwargs.get("use_litellm_proxy", False),
            azure_ad_token=kwargs.get("azure_ad_token"),
            tenant_id=kwargs.get("tenant_id"),
            client_id=kwargs.get("client_id"),
            client_secret=kwargs.get("client_secret"),
            azure_username=kwargs.get("azure_username"),
            azure_password=kwargs.get("azure_password"),
            azure_scope=kwargs.get("azure_scope"),
            litellm_request_debug=kwargs.get("litellm_request_debug", False),
            tpm=kwargs.get("tpm"),
            rpm=kwargs.get("rpm"),
        cast(LiteLLMLoggingObj, logging).update_environment_variables(
            optional_params=processed_non_default_params,  # [IMPORTANT] - using processed_non_default_params ensures consistent params logged to langfuse for finetuning / eval datasets.
            litellm_params=litellm_params,
        if mock_response or mock_tool_calls or mock_timeout:
            kwargs.pop("mock_timeout", None)  # remove for any fallbacks triggered
            return mock_completion(
                logging=logging,
                mock_delay=kwargs.get("mock_delay", None),
        ## RESPONSES API BRIDGE LOGIC ## - check if model has 'mode: responses' in litellm.model_cost map
        model_info, model = responses_api_bridge_check(
            model=model, custom_llm_provider=custom_llm_provider, web_search_options=web_search_options
        if model_info.get("mode") == "responses":
            from litellm.completion_extras import responses_api_bridge
            return responses_api_bridge.completion(
                model_response=model_response,
                optional_params=optional_params,
                timeout=timeout,  # type: ignore
                client=client,  # pass AsyncOpenAI, OpenAI client
                encoding=_get_encoding(),
        if custom_llm_provider == "azure":
            # azure configs
            ## check dynamic params ##
            dynamic_params = False
            if client is not None and (
                isinstance(client, openai.AzureOpenAI)
                or isinstance(client, openai.AsyncAzureOpenAI)
                dynamic_params = _check_dynamic_azure_params(
                    azure_client_params={"api_version": api_version},
                    azure_client=client,
            api_type = get_secret("AZURE_API_TYPE") or "azure"
            api_base = api_base or litellm.api_base or get_secret("AZURE_API_BASE")
            api_version = (
                api_version
                or litellm.api_version
                or get_secret_str("AZURE_API_VERSION")
                or litellm.AZURE_DEFAULT_API_VERSION
                api_key
                or litellm.api_key
                or litellm.azure_key
                or get_secret_str("AZURE_OPENAI_API_KEY")
                or get_secret_str("AZURE_API_KEY")
            azure_ad_token = optional_params.get("extra_body", {}).pop(
                "azure_ad_token", None
            ) or get_secret_str("AZURE_AD_TOKEN")
            azure_ad_token_provider = litellm_params.get(
                "azure_ad_token_provider", None
            headers = headers or litellm.headers
                optional_params["extra_headers"] = extra_headers
            if max_retries is not None:
                optional_params["max_retries"] = max_retries
            if litellm.AzureOpenAIO1Config().is_o_series_model(model=model):
                ## LOAD CONFIG - if set
                config = litellm.AzureOpenAIO1Config.get_config()
                        k not in optional_params
                    ):  # completion(top_k=3) > azure_config(top_k=3) <- allows for dynamic variables to be passed in
                        optional_params[k] = v
                response = azure_o1_chat_completions.completion(
                    dynamic_params=dynamic_params,
                    print_verbose=print_verbose,
                    client=client,  # pass AsyncAzureOpenAI, AzureOpenAI client
                config = litellm.AzureOpenAIConfig.get_config()
                ## COMPLETION CALL
                response = azure_chat_completions.completion(
                    api_type=api_type,
            if optional_params.get("stream", False):
                    original_response=response,
                    additional_args={
                        "headers": headers,
                        "api_base": api_base,
        elif custom_llm_provider == "azure_text":
            api_type = get_secret_str("AZURE_API_TYPE") or "azure"
            api_base = api_base or litellm.api_base or get_secret_str("AZURE_API_BASE")
            if api_base is None:
                    "api_base is required for Azure OpenAI LLM provider. Either set it dynamically or set the AZURE_API_BASE environment variable."
            response = azure_text_completions.completion(
                api_version=cast(str, api_version),
            if optional_params.get("stream", False) or acompletion is True:
        elif custom_llm_provider == "deepseek":
                response = base_llm_http_handler.completion(
                ## LOGGING - log the original exception returned
                    original_response=str(e),
                    additional_args={"headers": headers},
        elif custom_llm_provider == "azure_ai":
            from litellm.llms.azure_ai.common_utils import AzureFoundryModelInfo
            azure_ai_route = AzureFoundryModelInfo.get_azure_ai_route(model)
            # Check if this is an agents route - model format: azure_ai/agents/<agent_id>
            if azure_ai_route == "agents":
                from litellm.llms.azure_ai.agents import AzureAIAgentsConfig
                api_base = AzureFoundryModelInfo.get_api_base(api_base)
                        "Azure AI Agents requests require an api_base. "
                        "Set `api_base` or the AZURE_AI_API_BASE env var."
                api_key = AzureFoundryModelInfo.get_api_key(api_key)
                response = AzureAIAgentsConfig.completion(
                    headers=headers or litellm.headers,
            # Check if this is a Claude model - route to Azure Anthropic handler
            elif "claude" in model.lower():
                # Use Azure Anthropic handler for Claude models
                        "Azure Anthropic requests require an api_base. "
                # Ensure the URL ends with /v1/messages for Anthropic
                if api_base:
                    api_base = api_base.rstrip("/")
                    if not api_base.endswith("/v1/messages"):
                        if "/anthropic" in api_base:
                            parts = api_base.split("/anthropic", 1)
                            api_base = parts[0] + "/anthropic"
                            api_base = api_base + "/anthropic"
                        api_base = api_base + "/v1/messages"
                response = azure_anthropic_chat_completions.completion(
                    custom_prompt_dict=litellm.custom_prompt_dict,
                response = response
                # Non-Claude models use standard Azure AI flow
                # set API KEY
                ## FOR COHERE
                if "command-r" in model:  # make sure tool call in messages are str
                    messages = stringify_json_tool_call_content(messages=messages)
            or "ft:babbage-002" in model
            or "ft:davinci-002" in model  # support for finetuned completion models
            or custom_llm_provider
            in litellm.openai_text_completion_compatible_providers
            and kwargs.get("text_completion") is True
            openai.api_type = "openai"
            api_base = (
                api_base
                or litellm.api_base
                or get_secret("OPENAI_BASE_URL")
                or get_secret("OPENAI_API_BASE")
                or "https://api.openai.com/v1"
                or litellm.openai_key
                or get_secret("OPENAI_API_KEY")
            config = litellm.OpenAITextCompletionConfig.get_config()
                ):  # completion(top_k=3) > openai_text_config(top_k=3) <- allows for dynamic variables to be passed in
            if litellm.organization:
                openai.organization = litellm.organization
                len(messages) > 0
                and "content" in messages[0]
                and isinstance(messages[0]["content"], list)
                # text-davinci-003 can accept a string or array, if it's an array, assume the array is set in messages[0]['content']
                # https://platform.openai.com/docs/api-reference/completions/create
                prompt = messages[0]["content"]
                prompt = " ".join([message["content"] for message in messages])  # type: ignore
            _response = openai_text_completions.completion(
                optional_params.get("stream", False) is False
                and acompletion is False
                and text_completion is False
                # convert to chat completion response
                _response = litellm.OpenAITextCompletionConfig().convert_to_chat_model_response_object(
                    response_object=_response, model_response_object=model_response
                    original_response=_response,
            response = _response
        elif custom_llm_provider == "fireworks_ai":
        elif custom_llm_provider == "heroku":
        elif custom_llm_provider == "ragflow":
            ## COMPLETION CALL - RAGFlow uses HTTP handler to support custom URL paths
        elif custom_llm_provider == "xai":
        elif custom_llm_provider == "groq":
                api_base  # for deepinfra/perplexity/anyscale/groq/friendliai we check in get_llm_provider and pass in the api base from there
                or get_secret("GROQ_API_BASE")
                or "https://api.groq.com/openai/v1"
                or litellm.api_key  # for deepinfra/perplexity/anyscale/friendliai we check in get_llm_provider and pass in the api key from there
                or litellm.groq_key
                or get_secret("GROQ_API_KEY")
            config = litellm.GroqChatConfig.get_config()
                ):  # completion(top_k=3) > openai_config(top_k=3) <- allows for dynamic variables to be passed in
                logging_obj=logging,  # model call logging done inside the class as we make need to modify I/O to fit aleph alpha's requirements
        elif custom_llm_provider == "a2a":
            # A2A (Agent-to-Agent) Protocol
            # Resolve agent configuration from registry if model format is "a2a/<agent-name>"
            api_base, api_key, headers = litellm.A2AConfig.resolve_agent_config_from_registry(
            # Fall back to environment variables and defaults
            api_base = api_base or litellm.api_base or get_secret_str("A2A_API_BASE")
                    "api_base is required for A2A provider. "
                    "Either provide api_base parameter, set A2A_API_BASE environment variable, "
                    "or register the agent in the proxy with model='a2a/<agent-name>'."
        elif custom_llm_provider == "gigachat":
            # GigaChat - Sber AI's LLM (Russia)
                or litellm.gigachat_key
                or get_secret("GIGACHAT_API_KEY")
                or get_secret("GIGACHAT_CREDENTIALS")
            headers = headers or litellm.headers or {}
        elif custom_llm_provider == "sap":
            config = litellm.GenAIHubOrchestrationConfig.get_config()
            response = sap_gen_ai_hub_chat_completions.completion(
        elif custom_llm_provider == "aiohttp_openai":
            # NEW aiohttp provider for 10-100x higher RPS
            response = base_llm_aiohttp_handler.completion(
        elif custom_llm_provider == "cometapi":
                or litellm.cometapi_key
                or get_secret_str("COMETAPI_KEY")
                or get_secret_str("COMETAPI_API_BASE")
                or "https://api.cometapi.com/v1"
                input=messages, api_key=api_key, original_response=response
        elif custom_llm_provider == "minimax":
            api_key = api_key or get_secret_str("MINIMAX_API_KEY") or litellm.api_key
                or get_secret_str("MINIMAX_API_BASE")
                or "https://api.minimax.io/v1"
        elif custom_llm_provider == "hosted_vllm":
                api_base or litellm.api_base or get_secret_str("HOSTED_VLLM_API_BASE")
            model in litellm.open_ai_chat_completion_models
            or custom_llm_provider == "custom_openai"
            or custom_llm_provider == "deepinfra"
            or custom_llm_provider == "perplexity"
            or custom_llm_provider == "nvidia_nim"
            or custom_llm_provider == "cerebras"
            or custom_llm_provider == "baseten"
            or custom_llm_provider == "sambanova"
            or custom_llm_provider == "volcengine"
            or custom_llm_provider == "anyscale"
            or custom_llm_provider == "openai"
            or custom_llm_provider == "together_ai"
            or custom_llm_provider == "nebius"
            or custom_llm_provider == "wandb"
            or custom_llm_provider == "clarifai"
            or custom_llm_provider in litellm.openai_compatible_providers
            or JSONProviderRegistry.exists(
            )  # JSON-configured providers
            or "ft:gpt-3.5-turbo" in model  # finetune gpt-3.5-turbo
        ):  # allow user to make an openai call with a custom base
            # note: if a user sets a custom base - we should ensure this works
            # allow for the setting of dynamic and stateful api-bases
            organization = (
                organization
                or litellm.organization
                or get_secret("OPENAI_ORGANIZATION")
                or None  # default - https://github.com/openai/openai-python/blob/284c1799070c723c6a553337134148a7ab088dd8/openai/util.py#L105
            openai.organization = organization
            # Add GitHub Copilot headers (same as /responses endpoint does)
            if custom_llm_provider == "github_copilot":
                from litellm.llms.github_copilot.common_utils import (
                    get_copilot_default_headers,
                from litellm.llms.github_copilot.authenticator import Authenticator
                copilot_auth = Authenticator()
                copilot_api_key = copilot_auth.get_api_key()
                copilot_headers = get_copilot_default_headers(copilot_api_key)
                if extra_headers:
                    copilot_headers.update(extra_headers)
                extra_headers = copilot_headers
                litellm.enable_preview_features and metadata is not None
            ):  # [PREVIEW] allow metadata to be passed to OPENAI
                openai_metadata = get_requester_metadata(metadata)
                if openai_metadata is not None:
                    optional_params["metadata"] = openai_metadata
            config = litellm.OpenAIConfig.get_config()
            use_base_llm_http_handler = get_secret_bool(
                "EXPERIMENTAL_OPENAI_BASE_LLM_HTTP_HANDLER"
                if use_base_llm_http_handler:
                    response = openai_chat_completions.completion(
        elif custom_llm_provider == "mistral":
            api_key = api_key or litellm.api_key or get_secret("MISTRAL_API_KEY")
                or get_secret("MISTRAL_API_BASE")
                or "https://api.mistral.ai/v1"
            "replicate" in model
            or custom_llm_provider == "replicate"
            or model in litellm.replicate_models
            # Setting the relevant API KEY for replicate, replicate defaults to using os.environ.get("REPLICATE_API_TOKEN")
            replicate_key = (
                or litellm.replicate_key
                or get_secret("REPLICATE_API_KEY")
                or get_secret("REPLICATE_API_TOKEN")
                or get_secret("REPLICATE_API_BASE")
                or "https://api.replicate.com/v1"
            custom_prompt_dict = custom_prompt_dict or litellm.custom_prompt_dict
            model_response = replicate_chat_completion(  # type: ignore
                encoding=_get_encoding(),  # for calculating input/output tokens
                api_key=replicate_key,
            if optional_params.get("stream", False) is True:
                    original_response=model_response,
            response = model_response
            "clarifai" in model
            or model in litellm.clarifai_models
            pass  # Deprecated - handled in the openai compatible provider section above
        elif custom_llm_provider == "anthropic_text":
                or litellm.anthropic_key
                or os.environ.get("ANTHROPIC_API_KEY")
                or get_secret("ANTHROPIC_API_BASE")
                or get_secret("ANTHROPIC_BASE_URL")
                or "https://api.anthropic.com/v1/complete"
            # Check if we should disable automatic URL suffix appending
            disable_url_suffix = get_secret_bool("LITELLM_ANTHROPIC_DISABLE_URL_SUFFIX")
                api_base is not None
                and not disable_url_suffix
                and not api_base.endswith("/v1/complete")
                api_base += "/v1/complete"
            elif disable_url_suffix:
                    "LITELLM_ANTHROPIC_DISABLE_URL_SUFFIX is set, skipping /v1/complete suffix"
                custom_llm_provider="anthropic_text",
        elif custom_llm_provider == "anthropic":
            # call /messages
            # default route for all anthropic models
                or "https://api.anthropic.com/v1/messages"
                and not api_base.endswith("/v1/messages")
                api_base += "/v1/messages"
                    "LITELLM_ANTHROPIC_DISABLE_URL_SUFFIX is set, skipping /v1/messages suffix"
            response = anthropic_chat_completions.completion(
        elif custom_llm_provider == "nlp_cloud":
            nlp_cloud_key = (
                or litellm.nlp_cloud_key
                or get_secret("NLP_CLOUD_API_KEY")
                or get_secret("NLP_CLOUD_API_BASE")
                or "https://api.nlpcloud.io/v1/gpu/"
            response = nlp_cloud_chat_completion(
                api_key=nlp_cloud_key,
            if "stream" in optional_params and optional_params["stream"] is True:
                response = CustomStreamWrapper(
                    custom_llm_provider="nlp_cloud",
        elif custom_llm_provider == "aleph_alpha":
            aleph_alpha_key = (
                or litellm.aleph_alpha_key
                or get_secret("ALEPH_ALPHA_API_KEY")
                or get_secret("ALEPHALPHA_API_KEY")
                or get_secret("ALEPH_ALPHA_API_BASE")
                or "https://api.aleph-alpha.com/complete"
            model_response = aleph_alpha.completion(
                default_max_tokens_to_sample=litellm.max_tokens,
                api_key=aleph_alpha_key,
                    custom_llm_provider="aleph_alpha",
        elif custom_llm_provider == "cohere_chat" or custom_llm_provider == "cohere":
            cohere_key = (
                or litellm.cohere_key
                or get_secret_str("COHERE_API_KEY")
                or get_secret_str("CO_API_KEY")
            cohere_route = CohereModelInfo.get_cohere_route(model)
            verbose_logger.debug(f"Cohere route: {cohere_route}")
            # Set API base based on route
            if cohere_route == "v2":
                    or get_secret_str("COHERE_API_BASE")
                    or "https://api.cohere.com/v2/chat"
                # Remove v2/ prefix from model name for the actual API call
                if "v2/" in model:
                    model = model.replace("v2/", "")
                    or "https://api.cohere.ai/v1/chat"
            verbose_logger.debug(f"Model: {model}, API Base: {api_base}")
            verbose_logger.debug(f"Provider Config: {provider_config}")
                custom_llm_provider="cohere_chat",
                api_key=cohere_key,
        elif custom_llm_provider == "maritalk":
            maritalk_key = (
                or litellm.maritalk_key
                or get_secret("MARITALK_API_KEY")
                or get_secret("MARITALK_API_BASE")
                or "https://chat.maritaca.ai/api"
            model_response = openai_like_chat_completion.completion(
                api_key=maritalk_key,
                custom_llm_provider="maritalk",
        elif custom_llm_provider == "amazon_nova":
                or litellm.amazon_nova_api_key
                or get_secret_str("AMAZON_NOVA_API_KEY")
                or get_secret_str("AMAZON_NOVA_API_BASE")
                or "https://api.nova.amazon.com/v1"
            response = openai_like_chat_completion.completion(
        elif custom_llm_provider == "huggingface":
            huggingface_key = (
                or litellm.huggingface_key
                or os.environ.get("HF_TOKEN")
                or os.environ.get("HUGGINGFACE_API_KEY")
            hf_headers = headers or litellm.headers
                headers=hf_headers,
                api_key=huggingface_key,
        elif custom_llm_provider == "oci":
        elif custom_llm_provider == "compactifai":
                api_key or get_secret_str("COMPACTIFAI_API_KEY") or litellm.api_key
            api_base = api_base or "https://api.compactif.ai/v1"
        elif custom_llm_provider == "oobabooga":
            custom_llm_provider = "oobabooga"
            model_response = oobabooga.completion(
                api_base=api_base,  # type: ignore
                    custom_llm_provider="oobabooga",
        elif custom_llm_provider == "databricks":
                api_base  # for databricks we check in get_llm_provider and pass in the api base from there
                or os.getenv("DATABRICKS_API_BASE")
                or litellm.api_key  # for databricks we check in get_llm_provider and pass in the api key from there
                or litellm.databricks_key
                or get_secret("DATABRICKS_API_KEY")
                    custom_llm_provider="databricks",
        elif custom_llm_provider == "datarobot":
        elif custom_llm_provider == "openrouter":
                or get_secret_str("OPENROUTER_API_BASE")
                or "https://openrouter.ai/api/v1"
                or litellm.openrouter_key
                or get_secret_str("OPENROUTER_API_KEY")
                or get_secret_str("OR_API_KEY")
            openrouter_site_url = get_secret("OR_SITE_URL") or "https://litellm.ai"
            openrouter_app_name = get_secret("OR_APP_NAME") or "liteLLM"
            openrouter_headers = {
                "HTTP-Referer": openrouter_site_url,
                "X-Title": openrouter_app_name,
            _headers = headers or litellm.headers
                openrouter_headers.update(_headers)
            headers = openrouter_headers
            ## Load Config
            config = litellm.OpenrouterConfig.get_config()
                if k == "extra_body":
                    # we use openai 'extra_body' to pass openrouter specific params - transforms, route, models
                    if "extra_body" in optional_params:
                        optional_params[k].update(v)
                elif k not in optional_params:
            data = {"model": model, "messages": messages, **optional_params}
                custom_llm_provider="openrouter",
                input=messages, api_key=openai.api_key, original_response=response
        elif custom_llm_provider == "vercel_ai_gateway":
                or get_secret_str("VERCEL_AI_GATEWAY_API_BASE")
                or "https://ai-gateway.vercel.sh/v1"
                api_key or litellm.api_key or get_secret("VERCEL_AI_GATEWAY_API_KEY")
            vercel_site_url = get_secret("VERCEL_SITE_URL") or "https://litellm.ai"
            vercel_app_name = get_secret("VERCEL_APP_NAME") or "liteLLM"
            vercel_headers = {
                "http-referer": vercel_site_url,
                "x-title": vercel_app_name,
                vercel_headers.update(_headers)
            headers = vercel_headers
            config = litellm.VercelAIGatewayConfig.get_config()
                    # we use openai 'extra_body' to pass vercel specific params - providerOptions
                custom_llm_provider="vercel_ai_gateway",
            custom_llm_provider == "together_ai"
            or ("togethercomputer" in model)
            or (model in litellm.together_ai_models)
            Deprecated. We now do together ai calls via the openai client - https://docs.together.ai/docs/openai-api-compatibility
        elif custom_llm_provider == "palm":
                "Palm was decommisioned on October 2024. Please use the `gemini/` route for Gemini Google AI Studio Models. Announcement: https://ai.google.dev/palm_docs/palm?hl=en"
        elif custom_llm_provider == "vertex_ai_beta" or custom_llm_provider == "gemini":
            vertex_ai_project = (
                optional_params.pop("vertex_project", None)
                or optional_params.pop("vertex_ai_project", None)
                or litellm.vertex_project
                or get_secret("VERTEXAI_PROJECT")
            vertex_ai_location = (
                optional_params.pop("vertex_location", None)
                or optional_params.pop("vertex_ai_location", None)
                or litellm.vertex_location
                or get_secret("VERTEXAI_LOCATION")
            vertex_credentials = (
                optional_params.pop("vertex_credentials", None)
                or optional_params.pop("vertex_ai_credentials", None)
                or get_secret("VERTEXAI_CREDENTIALS")
            gemini_api_key = (
                or get_api_key_from_env()
                or get_secret("PALM_API_KEY")  # older palm api key should also work
            api_base = api_base or litellm.api_base or get_secret("GEMINI_API_BASE")
            new_params = safe_deep_copy(optional_params or {})
            response = vertex_chat_completion.completion(  # type: ignore
                optional_params=new_params,
                litellm_params=litellm_params,  # type: ignore
                vertex_location=vertex_ai_location,
                vertex_project=vertex_ai_project,
                vertex_credentials=vertex_credentials,
                gemini_api_key=gemini_api_key,
                custom_llm_provider=custom_llm_provider,  # type: ignore
        elif custom_llm_provider == "vertex_ai":
            api_base = api_base or litellm.api_base or get_secret("VERTEXAI_API_BASE")
            model_route = get_vertex_ai_model_route(
                model=model, litellm_params=litellm_params
            if model_route == VertexAIModelRoute.PARTNER_MODELS:
                model_response = vertex_partner_models_chat_completion.completion(
            elif model_route == VertexAIModelRoute.GEMINI:
                model_response = vertex_chat_completion.completion(  # type: ignore
                    gemini_api_key=None,
            elif model_route == VertexAIModelRoute.GEMMA:
                # Vertex Gemma Models with custom prediction endpoint
                model_response = vertex_gemma_chat_completion.completion(
            elif model_route == VertexAIModelRoute.MODEL_GARDEN:
                # Vertex Model Garden - OpenAI compatible models
                model_response = vertex_model_garden_chat_completion.completion(
            elif model_route == VertexAIModelRoute.AGENT_ENGINE:
                # Vertex AI Agent Engine (Reasoning Engines)
                from litellm.llms.vertex_ai.agent_engine.transformation import (
                    VertexAgentEngineConfig,
                vertex_agent_engine_config = VertexAgentEngineConfig()
                # Update litellm_params with vertex credentials
                litellm_params["vertex_project"] = vertex_ai_project
                litellm_params["vertex_location"] = vertex_ai_location
                litellm_params["vertex_credentials"] = vertex_credentials
                model_response = base_llm_http_handler.completion(
                    custom_llm_provider="vertex_ai",
                    provider_config=vertex_agent_engine_config,
                    headers=headers or {},
            else:  # VertexAIModelRoute.NON_GEMINI
                model_response = vertex_ai_non_gemini.completion(
                    "stream" in optional_params
                    and optional_params["stream"] is True
        elif custom_llm_provider == "predibase":
            tenant_id = (
                optional_params.pop("tenant_id", None)
                or optional_params.pop("predibase_tenant_id", None)
                or litellm.predibase_tenant_id
                or get_secret("PREDIBASE_TENANT_ID")
            if tenant_id is None:
                    "Missing Predibase Tenant ID - Required for making the request. Set dynamically (e.g. `completion(..tenant_id=<MY-ID>)`) or in env - `PREDIBASE_TENANT_ID`."
                or optional_params.pop("api_base", None)
                or optional_params.pop("base_url", None)
                or get_secret("PREDIBASE_API_BASE")
                or litellm.predibase_key
                or get_secret("PREDIBASE_API_KEY")
            _model_response = predibase_chat_completions.completion(
                tenant_id=tenant_id,
                return _model_response
            response = _model_response
        elif custom_llm_provider == "text-completion-codestral":
                or "https://codestral.mistral.ai/v1/fim/completions"
            api_key = api_key or litellm.api_key or get_secret("CODESTRAL_API_KEY")
            text_completion_model_response = litellm.TextCompletionResponse(
                stream=stream
            _model_response = codestral_text_completions.completion(  # type: ignore
                model_response=text_completion_model_response,
        elif custom_llm_provider == "sagemaker_chat":
            # boto3 reads keys from .env
                custom_llm_provider="sagemaker_chat",
            ## RESPONSE OBJECT
        elif custom_llm_provider == "sagemaker":
            model_response = sagemaker_llm.completion(
        elif custom_llm_provider == "bedrock":
            if "aws_bedrock_client" in optional_params:
                verbose_logger.warning(
                    "'aws_bedrock_client' is a deprecated param. Please move to another auth method - https://docs.litellm.ai/docs/providers/bedrock#boto3---authentication."
                # Extract credentials for legacy boto3 client and pass thru to httpx
                aws_bedrock_client = optional_params.pop("aws_bedrock_client")
                creds = aws_bedrock_client._get_credentials().get_frozen_credentials()
                if creds.access_key:
                    optional_params["aws_access_key_id"] = creds.access_key
                if creds.secret_key:
                    optional_params["aws_secret_access_key"] = creds.secret_key
                if creds.token:
                    optional_params["aws_session_token"] = creds.token
                    "aws_region_name" not in optional_params
                    or optional_params["aws_region_name"] is None
                    optional_params["aws_region_name"] = (
                        aws_bedrock_client.meta.region_name
            bedrock_route = BedrockModelInfo.get_bedrock_route(model)
            if bedrock_route == "converse":
                model = model.replace("converse/", "")
                response = bedrock_converse_chat_completion.completion(
                    extra_headers=headers,  # Use merged headers instead of original extra_headers
            elif bedrock_route == "converse_like":
                model = model.replace("converse_like/", "")
                    custom_llm_provider="bedrock",
        elif custom_llm_provider == "watsonx":
            response = watsonx_chat_completion.completion(
                custom_llm_provider="watsonx",
        elif custom_llm_provider == "watsonx_text":
                or optional_params.pop("apikey", None)
                or get_secret_str("WATSONX_APIKEY")
                or get_secret_str("WATSONX_API_KEY")
                or get_secret_str("WX_API_KEY")
                or optional_params.pop(
                    optional_params.pop(
                        "api_base", optional_params.pop("base_url", None)
                or get_secret_str("WATSONX_API_BASE")
                or get_secret_str("WATSONX_URL")
                or get_secret_str("WX_URL")
                or get_secret_str("WML_URL")
            wx_credentials = optional_params.pop(
                "wx_credentials",
                    "watsonx_credentials", None
                ),  # follow {provider}_credentials, same as vertex ai
            if wx_credentials is not None:
                api_base = wx_credentials.get("url", api_base)
                api_key = wx_credentials.get(
                    "apikey", wx_credentials.get("api_key", api_key)
                token = wx_credentials.get(
                    "token",
                    wx_credentials.get(
                        "watsonx_token", None
                    ),  # follow format of {provider}_token, same as azure - e.g. 'azure_ad_token=..'
            if token is not None:
                optional_params["token"] = token
                custom_llm_provider="watsonx_text",
        elif custom_llm_provider == "vllm":
            model_response = vllm_handler.completion(
                "stream" in optional_params and optional_params["stream"] is True
            ):  ## [BETA]
                    custom_llm_provider="vllm",
        elif custom_llm_provider == "ollama":
                litellm.api_base
                or api_base
                or get_secret("OLLAMA_API_BASE")
                or "http://localhost:11434"
            if api_key is not None and "Authorization" not in headers:
                headers["Authorization"] = f"Bearer {api_key}"
                custom_llm_provider="ollama",
        elif custom_llm_provider == "ollama_chat":
                or litellm.ollama_key
                or os.environ.get("OLLAMA_API_KEY")
                custom_llm_provider="ollama_chat",
        elif custom_llm_provider == "triton":
            api_base = litellm.api_base or api_base
        elif custom_llm_provider == "cloudflare":
                or litellm.cloudflare_api_key
                or get_secret("CLOUDFLARE_API_KEY")
            account_id = get_secret("CLOUDFLARE_ACCOUNT_ID")
                or get_secret("CLOUDFLARE_API_BASE")
                or f"https://api.cloudflare.com/client/v4/accounts/{account_id}/ai/run/"
                custom_llm_provider="cloudflare",
        elif custom_llm_provider == "petals" or model in litellm.petals_models:
            api_base = api_base or litellm.api_base
            custom_llm_provider = "petals"
            stream = optional_params.pop("stream", False)
            model_response = petals_handler.completion(
            if stream is True:  ## [BETA]
                # Fake streaming for petals
                resp_string = model_response["choices"][0]["message"]["content"]
                    resp_string,
                    custom_llm_provider="petals",
        elif custom_llm_provider == "snowflake" or model in litellm.snowflake_models:
                client = (
                    HTTPHandler(timeout=timeout) if stream is False else None
                )  # Keep this here, otherwise, the httpx.client closes and streaming is impossible
        elif custom_llm_provider == "gradient_ai":
                custom_llm_provider="gradient_ai",
        elif custom_llm_provider == "bytez":
                or litellm.bytez_key
                or get_secret_str("BYTEZ_API_KEY")
                provider_config=bytez_transformation,
        elif custom_llm_provider == "lemonade":
                or litellm.lemonade_key
                or get_secret_str("LEMONADE_API_KEY")
                provider_config=lemonade_transformation,
        elif custom_llm_provider == "ovhcloud" or model in litellm.ovhcloud_models:
                or litellm.ovhcloud_key
                or get_secret_str("OVHCLOUD_API_KEY")
                or get_secret_str("OVHCLOUD_API_BASE")
                or "https://oai.endpoints.kepler.ai.cloud.ovh.net/v1"
                provider_config=ovhcloud_transformation,
        elif custom_llm_provider == "custom":
            url = litellm.api_base or api_base or ""
            if url is None or url == "":
                    "api_base not set. Set api_base or litellm.api_base for custom endpoints"
            assume input to custom LLM api bases follow this format:
            resp = litellm.module_level_client.post(
                api_base,
                    'model': 'meta-llama/Llama-2-13b-hf', # model name
                    'params': {
                        'prompt': ["The capital of France is P"],
                        'max_tokens': 32,
                        'temperature': 0.7,
                        'top_p': 1.0,
                        'top_k': 40,
                    "params": {
                        "prompt": [prompt],
                        "top_k": kwargs.get("top_k"),
                    **kwargs.get("extra_body", {}),
            response_json = resp.json()
            assume all responses from custom api_bases of this format:
                'data': [
                        'prompt': 'The capital of France is P',
                        'output': ['The capital of France is PARIS.\nThe capital of France is PARIS.\nThe capital of France is PARIS.\nThe capital of France is PARIS.\nThe capital of France is PARIS.\nThe capital of France is PARIS.\nThe capital of France is PARIS.\nThe capital of France is PARIS.\nThe capital of France is PARIS.\nThe capital of France is PARIS.\nThe capital of France is PARIS.\nThe capital of France is PARIS.\nThe capital of France is PARIS.\nThe capital of France'],
                        'params': {'temperature': 0.7, 'top_k': 40, 'top_p': 1}}],
                        'message': 'ok'
            string_response = response_json["data"][0]["output"][0]
            model_response.choices[0].message.content = string_response  # type: ignore
            custom_llm_provider in litellm._custom_providers
        ):  # Assume custom LLM provider
            # Get the Custom Handler
            custom_handler: Optional[CustomLLM] = None
            for item in litellm.custom_provider_map:
                if item["provider"] == custom_llm_provider:
                    custom_handler = item["custom_handler"]
            if custom_handler is None:
                raise LiteLLMUnknownProvider(
            ## ROUTE LLM CALL ##
            handler_fn = custom_chat_llm_router(
                async_fn=acompletion, stream=stream, custom_llm=custom_handler
            ## CALL FUNCTION
            response = handler_fn(
                    completion_stream=response,
        elif custom_llm_provider == "langgraph":
            # LangGraph - Agent Runtime Provider
            from litellm.llms.langgraph.chat.transformation import LangGraphConfig
            ) = LangGraphConfig()._get_openai_compatible_provider_info(
                api_base=api_base or litellm.api_base,
                api_key=api_key or litellm.api_key,
        ## Map to OpenAI Exception
            completion_kwargs=args,
def completion_with_retries(*args, **kwargs):
    Executes a litellm.completion() with 3 retries
            f"tenacity import failed please run `pip install tenacity`. Error{e}"
    num_retries = kwargs.pop("num_retries", 3)
    # reset retries in .completion()
    kwargs["max_retries"] = 0
    kwargs["num_retries"] = 0
    retry_strategy: Literal["exponential_backoff_retry", "constant_retry"] = kwargs.pop(
        "retry_strategy", "constant_retry"
    original_function = kwargs.pop("original_function", completion)
    if retry_strategy == "exponential_backoff_retry":
        retryer = tenacity.Retrying(
            wait=tenacity.wait_exponential(multiplier=1, max=10),
            stop=tenacity.stop_after_attempt(num_retries),
            reraise=True,
            stop=tenacity.stop_after_attempt(num_retries), reraise=True
    return retryer(original_function, *args, **kwargs)
async def acompletion_with_retries(*args, **kwargs):
    [DEPRECATED]. Use 'acompletion' or router.acompletion instead!
    retry_strategy = kwargs.pop("retry_strategy", "constant_retry")
        retryer = tenacity.AsyncRetrying(
    return await retryer(original_function, *args, **kwargs)
def responses_with_retries(*args, **kwargs):
    Executes a litellm.responses() with retries
    from litellm.responses.main import responses
    # reset retries in .responses()
    original_function = kwargs.pop("original_function", responses)
async def aresponses_with_retries(*args, **kwargs):
    Executes a litellm.aresponses() with retries
    from litellm.responses.main import aresponses
    original_function = kwargs.pop("original_function", aresponses)
### EMBEDDING ENDPOINTS ####################
async def aembedding(*args, **kwargs) -> EmbeddingResponse:
    Asynchronously calls the `embedding` function with the given arguments and keyword arguments.
    - `args` (tuple): Positional arguments to be passed to the `embedding` function.
    - `kwargs` (dict): Keyword arguments to be passed to the `embedding` function.
    - `response` (Any): The response returned by the `embedding` function.
    model = args[0] if len(args) > 0 else kwargs["model"]
    ### PASS ARGS TO Embedding ###
    kwargs["aembedding"] = True
        func = partial(embedding, *args, **kwargs)
            api_base=kwargs.get("api_base", None),
        # Await normally
        response: Optional[EmbeddingResponse] = None
            response = EmbeddingResponse(**init_response)
        elif isinstance(init_response, EmbeddingResponse):  ## CACHING SCENARIO
            response = await init_response  # type: ignore
            and isinstance(response, EmbeddingResponse)
            and hasattr(response, "_hidden_params")
            response._hidden_params["custom_llm_provider"] = custom_llm_provider
                "Unable to get Embedding Response. Please pass a valid llm_provider."
# Overload for when aembedding=True (returns coroutine)
def embedding(
    input=[],
    # Optional params
    encoding_format: Optional[str] = None,
    timeout=600,  # default to 10 minutes
    api_base: Optional[str] = None,
    api_type: Optional[str] = None,
    caching: bool = False,
    litellm_call_id=None,
    logger_fn=None,
    aembedding: Literal[True],
) -> Coroutine[Any, Any, EmbeddingResponse]: 
# Overload for when aembedding=False or not specified (returns EmbeddingResponse)
    aembedding: Literal[False] = False,
) -> EmbeddingResponse: 
def embedding(  # noqa: PLR0915
) -> Union[EmbeddingResponse, Coroutine[Any, Any, EmbeddingResponse]]:
    Embedding function that calls an API to generate embeddings for the given input.
    - model: The embedding model to use.
    - input: The input for which embeddings are to be generated.
    - encoding_format: Optional[str] The format to return the embeddings in. Can be either `float` or `base64`
    - dimensions: The number of dimensions the resulting output embeddings should have. Only supported in text-embedding-3 and later models.
    - timeout: The timeout value for the API call, default 10 mins
    - litellm_call_id: The call ID for litellm logging.
    - litellm_logging_obj: The litellm logging object.
    - logger_fn: The logger function.
    - api_base: Optional. The base URL for the API.
    - api_version: Optional. The version of the API.
    - api_key: Optional. The API key to use.
    - api_type: Optional. The type of the API.
    - caching: A boolean indicating whether to enable caching.
    - custom_llm_provider: The custom llm provider.
    - response: The response received from the API call.
    - exception_type: If an exception occurs during the API call.
    azure = kwargs.get("azure", None)
    client = kwargs.pop("client", None)
    shared_session = kwargs.get("shared_session", None)
    litellm_logging_obj: LiteLLMLoggingObj = kwargs.get("litellm_logging_obj")  # type: ignore
    mock_response: Optional[List[float]] = kwargs.get("mock_response", None)  # type: ignore
    azure_ad_token_provider = kwargs.get("azure_ad_token_provider", None)
    aembedding: Optional[bool] = kwargs.get("aembedding", None)
    extra_headers = kwargs.get("extra_headers", None)
    openai_params = [
        "dimensions",
        "request_timeout",
        "api_base",
        "api_version",
        "api_key",
        "deployment_id",
        "default_headers",
        "max_retries",
        "encoding_format",
    litellm_params = [
        "aembedding",
        "extra_headers",
    ] + all_litellm_params
    default_params = openai_params + litellm_params
    non_default_params = {
        k: v for k, v in kwargs.items() if k not in default_params
    }  # model-specific params - pass them straight to the model/provider
    optional_params = get_optional_params_embeddings(
        dimensions=dimensions,
        encoding_format=encoding_format,
        **non_default_params,
    if input_cost_per_second is not None:  # time based pricing just needs cost in place
        output_cost_per_second = output_cost_per_second or 0.0
    litellm_params_dict = get_litellm_params(**kwargs)
    logging: LiteLLMLoggingObj = litellm_logging_obj  # type: ignore
    logging.update_environment_variables(
        litellm_params=litellm_params_dict,
    if mock_response is not None:
        return mock_embedding(model=model, mock_response=mock_response)
        response: Optional[
            Union[EmbeddingResponse, Coroutine[Any, Any, EmbeddingResponse]]
        if azure is True or custom_llm_provider == "azure":
            azure_ad_token = optional_params.pop(
                    "No API Base provided for Azure OpenAI LLM provider. Set 'AZURE_API_BASE' in .env"
            ## EMBEDDING CALL
            response = azure_chat_completions.embedding(
                model_response=EmbeddingResponse(),
                aembedding=aembedding,
                headers=headers or extra_headers,
        elif custom_llm_provider == "github_copilot":
            api_key = api_key or litellm.api_key
            response = base_llm_http_handler.embedding(
            custom_llm_provider == "openai"
            or custom_llm_provider == "litellm_proxy"
            or (model in litellm.open_ai_embedding_models and custom_llm_provider is None)
                or get_secret_str("OPENAI_BASE_URL")
                or get_secret_str("OPENAI_API_BASE")
            openai.organization = (
                litellm.organization
                or get_secret_str("OPENAI_ORGANIZATION")
                or get_secret_str("OPENAI_API_KEY")
            if headers is not None and headers != {}:
                optional_params["extra_headers"] = headers
            if encoding_format is not None:
                optional_params["encoding_format"] = encoding_format
                # Omiting causes openai sdk to add default value of "float"
                optional_params["encoding_format"] = None
            api_version = None
            response = openai_chat_completions.embedding(
            api_base = api_base or litellm.api_base or get_secret("DATABRICKS_API_BASE")  # type: ignore
            response = databricks_embedding.embedding(
                api_key = litellm.api_key or get_secret_str("HOSTED_VLLM_API_KEY")
            custom_llm_provider == "openai_like"
            or custom_llm_provider == "llamafile"
            or custom_llm_provider == "lm_studio"
                api_base or litellm.api_base or get_secret_str("OPENAI_LIKE_API_BASE")
                    or litellm.openai_like_key
                    or get_secret_str("OPENAI_LIKE_API_KEY")
            response = openai_like_embedding.embedding(
        elif custom_llm_provider == "cohere" or custom_llm_provider == "cohere_chat":
            # Use the merged headers variable (already merged at the top of the function)
            # Don't overwrite it with just extra_headers
                or get_secret_str("VERCEL_AI_GATEWAY_API_KEY")
                or get_secret_str("VERCEL_OIDC_TOKEN")
                or get_secret("HUGGINGFACE_API_KEY")
            response = huggingface_embed.embedding(
                encoding=_get_encoding(),  # type: ignore
            if isinstance(input, str):
                transformed_input = [input]
                transformed_input = input
            response = bedrock_embedding.embeddings(
                input=transformed_input,
                litellm_params={},
                    "api_base is required for triton. Please pass `api_base`"
        elif custom_llm_provider == "gemini":
            gemini_api_key = api_key or get_api_key_from_env() or litellm.api_key
            api_base = api_base or litellm.api_base or get_secret_str("GEMINI_API_BASE")
            response = google_batch_embeddings.batch_embeddings(  # type: ignore
                vertex_project=None,
                vertex_location=None,
                vertex_credentials=None,
                custom_llm_provider="gemini",
                api_key=gemini_api_key,
                or get_secret_str("VERTEXAI_PROJECT")
                or get_secret_str("VERTEX_PROJECT")
                or get_secret_str("VERTEXAI_LOCATION")
                or get_secret_str("VERTEX_LOCATION")
                or get_secret_str("VERTEXAI_CREDENTIALS")
                or get_secret_str("VERTEX_CREDENTIALS")
                or get_secret_str("VERTEXAI_API_BASE")
                or get_secret_str("VERTEX_API_BASE")
                "image" in optional_params
                or "video" in optional_params
                or model
                in vertex_multimodal_embedding.SUPPORTED_MULTIMODAL_EMBEDDING_MODELS
                # multimodal embedding is supported on vertex httpx
                response = vertex_multimodal_embedding.multimodal_embedding(
                response = vertex_embedding.embedding(
            response = oobabooga.embedding(
                or get_secret_str("OLLAMA_API_BASE")
                input = [input]
            if not all(isinstance(item, str) for item in input):
                raise litellm.BadRequestError(
                    message=f"Invalid input for ollama embeddings. input={input}",
                    llm_provider="ollama",  # type: ignore
            ollama_embeddings_fn = (
                ollama.ollama_aembeddings
                if aembedding is True
                else ollama.ollama_embeddings
            response = ollama_embeddings_fn(  # type: ignore
                prompts=input,
            response = sagemaker_llm.embedding(
            api_key = api_key or litellm.api_key or get_secret_str("MISTRAL_API_KEY")
                api_key or litellm.api_key or get_secret_str("FIREWORKS_AI_API_KEY")
        elif custom_llm_provider == "nebius":
            api_key = api_key or litellm.api_key or get_secret_str("NEBIUS_API_KEY")
                or get_secret_str("NEBIUS_API_BASE")
                or "api.studio.nebius.ai/v1"
        elif custom_llm_provider == "wandb":
            api_key = api_key or litellm.api_key or get_secret_str("WANDB_API_KEY")
                or get_secret_str("WANDB_API_BASE")
                or "https://api.inference.wandb.ai/v1"
        elif custom_llm_provider == "sambanova":
            api_key = api_key or litellm.api_key or get_secret_str("SAMBANOVA_API_KEY")
                or get_secret_str("SAMBANOVA_API_BASE")
                or "https://api.sambanova.ai/v1"
        elif custom_llm_provider == "voyage":
        elif custom_llm_provider == "infinity":
            credentials = IBMWatsonXMixin.get_watsonx_credentials(
                optional_params=optional_params, api_key=api_key, api_base=api_base
            api_key = credentials["api_key"]
            api_base = credentials["api_base"]
            if "token" in credentials:
                optional_params["token"] = credentials["token"]
        elif custom_llm_provider == "xinference":
                or get_secret_str("XINFERENCE_API_KEY")
                or "stub-xinference-key"
            )  # xinference does not need an api key, pass a stub key if user did not set one
                or get_secret_str("XINFERENCE_API_BASE")
                or "http://127.0.0.1:9997/v1"
                or get_secret_str("AZURE_AI_API_BASE")
                or get_secret_str("AZURE_AI_API_KEY")
            response = azure_ai_embedding.embedding(
        elif custom_llm_provider == "jina_ai":
        elif custom_llm_provider == "volcengine":
            volcengine_key = (
                or get_secret_str("ARK_API_KEY")
                or get_secret_str("VOLCENGINE_API_KEY")
            if volcengine_key is None:
                    "Missing API key for Volcengine. Set ARK_API_KEY or VOLCENGINE_API_KEY environment variable or pass api_key parameter."
            if extra_headers is not None and isinstance(extra_headers, dict):
                headers = extra_headers
                api_key=volcengine_key,
        elif custom_llm_provider == "ovhcloud":
            api_key = api_key or litellm.api_key or get_secret_str("OVHCLOUD_API_KEY")
        elif custom_llm_provider in litellm._custom_providers:
            handler_fn = (
                custom_handler.embedding
                if not aembedding
                else custom_handler.aembedding
        elif custom_llm_provider == "snowflake":
            api_key = api_key or get_secret_str("SNOWFLAKE_JWT")
                or get_secret_str("GIGACHAT_CREDENTIALS")
                or get_secret_str("GIGACHAT_API_KEY")
                litellm_params={"ssl_verify": kwargs.get("ssl_verify", None)},
        litellm_logging_obj.post_call(
###### Text Completion ################
async def atext_completion(
    *args, **kwargs
) -> Union[TextCompletionResponse, TextCompletionStreamWrapper]:
    Implemented to handle async streaming for the text completion endpoint
    ### PASS ARGS TO COMPLETION ###
    kwargs["acompletion"] = True
    custom_llm_provider = None
        func = partial(text_completion, *args, **kwargs)
            init_response, TextCompletionResponse
                response = TextCompletionResponse(**init_response)
            kwargs.get("stream", False) is True
            or isinstance(response, TextCompletionStreamWrapper)
            or isinstance(response, CustomStreamWrapper)
        ):  # return an async generator
            return TextCompletionStreamWrapper(
                completion_stream=_async_streaming(
                stream_options=kwargs.get("stream_options"),
            ## OpenAI / Azure Text Completion Returns here
            if isinstance(response, TextCompletionResponse):
            text_completion_response = TextCompletionResponse()
            text_completion_response = litellm.utils.LiteLLMResponseObjectHandler.convert_chat_to_text_completion(
                text_completion_response=text_completion_response,
            return text_completion_response
def text_completion(  # noqa: PLR0915
    prompt: Union[
        str, List[Union[str, List[Union[str, List[int]]]]]
    ],  # Required: The prompt(s) to generate completions for.
    model: Optional[str] = None,  # Optional: either `model` or `engine` can be set
    best_of: Optional[
    ] = None,  # Optional: Generates best_of completions server-side.
    echo: Optional[
    ] = None,  # Optional: Echo back the prompt in addition to the completion.
    frequency_penalty: Optional[
    ] = None,  # Optional: Penalize new tokens based on their existing frequency.
    logit_bias: Optional[
        Dict[int, int]
    ] = None,  # Optional: Modify the likelihood of specified tokens.
    logprobs: Optional[
    ] = None,  # Optional: Include the log probabilities on the most likely tokens.
    max_tokens: Optional[
    ] = None,  # Optional: The maximum number of tokens to generate in the completion.
    n: Optional[
    ] = None,  # Optional: How many completions to generate for each prompt.
    presence_penalty: Optional[
    ] = None,  # Optional: Penalize new tokens based on whether they appear in the text so far.
    stop: Optional[
        Union[str, List[str]]
    ] = None,  # Optional: Sequences where the API will stop generating further tokens.
    stream: Optional[bool] = None,  # Optional: Whether to stream back partial progress.
    suffix: Optional[
    ] = None,  # Optional: The suffix that comes after a completion of inserted text.
    temperature: Optional[float] = None,  # Optional: Sampling temperature to use.
    top_p: Optional[float] = None,  # Optional: Nucleus sampling parameter.
    user: Optional[
    ] = None,  # Optional: A unique identifier representing your end-user.
    Generate text completions using the OpenAI API.
        model (str): ID of the model to use.
        prompt (Union[str, List[Union[str, List[Union[str, List[int]]]]]): The prompt(s) to generate completions for.
        best_of (Optional[int], optional): Generates best_of completions server-side. Defaults to 1.
        echo (Optional[bool], optional): Echo back the prompt in addition to the completion. Defaults to False.
        frequency_penalty (Optional[float], optional): Penalize new tokens based on their existing frequency. Defaults to 0.
        logit_bias (Optional[Dict[int, int]], optional): Modify the likelihood of specified tokens. Defaults to None.
        logprobs (Optional[int], optional): Include the log probabilities on the most likely tokens. Defaults to None.
        max_tokens (Optional[int], optional): The maximum number of tokens to generate in the completion. Defaults to 16.
        n (Optional[int], optional): How many completions to generate for each prompt. Defaults to 1.
        presence_penalty (Optional[float], optional): Penalize new tokens based on whether they appear in the text so far. Defaults to 0.
        stop (Optional[Union[str, List[str]]], optional): Sequences where the API will stop generating further tokens. Defaults to None.
        stream (Optional[bool], optional): Whether to stream back partial progress. Defaults to False.
        suffix (Optional[str], optional): The suffix that comes after a completion of inserted text. Defaults to None.
        temperature (Optional[float], optional): Sampling temperature to use. Defaults to 1.
        top_p (Optional[float], optional): Nucleus sampling parameter. Defaults to 1.
        user (Optional[str], optional): A unique identifier representing your end-user.
        TextCompletionResponse: A response object containing the generated completion and associated metadata.
        Your example of how to use this function goes here.
    if "engine" in kwargs:
        _engine = kwargs["engine"]
        if model is None and isinstance(_engine, str):
            # only use engine when model not passed
            model = _engine
        kwargs.pop("engine")
    optional_params: Dict[str, Any] = {}
    # default values for all optional params are none, litellm only passes them to the llm when they are set to non None values
    if best_of is not None:
        optional_params["best_of"] = best_of
    if echo is not None:
        optional_params["echo"] = echo
    if frequency_penalty is not None:
        optional_params["frequency_penalty"] = frequency_penalty
    if logit_bias is not None:
        optional_params["logit_bias"] = logit_bias
    if logprobs is not None:
        optional_params["logprobs"] = logprobs
    if max_tokens is not None:
        optional_params["max_tokens"] = max_tokens
        optional_params["n"] = n
    if presence_penalty is not None:
        optional_params["presence_penalty"] = presence_penalty
        optional_params["stop"] = stop
        optional_params["stream"] = stream
    if stream_options is not None:
        optional_params["stream_options"] = stream_options
    if suffix is not None:
        optional_params["suffix"] = suffix
    if temperature is not None:
        optional_params["temperature"] = temperature
    if top_p is not None:
        optional_params["top_p"] = top_p
        optional_params["user"] = user
    if api_base is not None:
        optional_params["api_base"] = api_base
    if api_version is not None:
        optional_params["api_version"] = api_version
    if api_key is not None:
        optional_params["api_key"] = api_key
    if custom_llm_provider is not None:
        optional_params["custom_llm_provider"] = custom_llm_provider
    # get custom_llm_provider
    _model, custom_llm_provider, dynamic_api_key, api_base = get_llm_provider(
    if custom_llm_provider == "huggingface":
        # if echo == True, for TGI llms we need to set top_n_tokens to 3
        if echo is True:
            # for tgi llms
            if "top_n_tokens" not in kwargs:
                kwargs["top_n_tokens"] = 3
        # processing prompt - users can pass raw tokens to OpenAI Completion()
        if isinstance(prompt, list):
            tokenizer = tiktoken.encoding_for_model("text-davinci-003")
            ## if it's a 2d list - each element in the list is a text_completion() request
            if len(prompt) > 0 and isinstance(prompt[0], list):
                responses = [None for x in prompt]  # init responses
                def process_prompt(i, individual_prompt):
                    decoded_prompt = tokenizer.decode(individual_prompt)
                    all_params = {**kwargs, **optional_params}
                    response: TextCompletionResponse = text_completion(  # type: ignore
                        prompt=decoded_prompt,
                        num_retries=3,  # ensure this does not fail for the batch
                        **all_params,
                    text_completion_response["id"] = response.get("id", None)
                    text_completion_response["object"] = "text_completion"
                    text_completion_response["created"] = response.get("created", None)
                    text_completion_response["model"] = response.get("model", None)
                    return response["choices"][0]
                with concurrent.futures.ThreadPoolExecutor() as executor:
                    completed_futures = [
                        executor.submit(process_prompt, i, individual_prompt)
                        for i, individual_prompt in enumerate(prompt)
                    for i, future in enumerate(
                        concurrent.futures.as_completed(completed_futures)
                        responses[i] = future.result()
                    text_completion_response.choices = responses  # type: ignore
    # else:
    # check if non default values passed in for best_of, echo, logprobs, suffix
    # these are the params supported by Completion() but not ChatCompletion
    # default case, non OpenAI requests go through here
    # handle prompt formatting if prompt is a string vs. list of strings
    if isinstance(prompt, list) and len(prompt) > 0 and isinstance(prompt[0], str):
        for p in prompt:
            message = {"role": "user", "content": p}
            messages.append(message)
    elif isinstance(prompt, str):
        messages = [{"role": "user", "content": prompt}]
            or custom_llm_provider == "azure"
            or custom_llm_provider == "azure_text"
            or custom_llm_provider == "text-completion-openai"
        and isinstance(prompt, list)
        and len(prompt) > 0
        and (isinstance(prompt[0], list) or isinstance(prompt[0], int))
        # Support for token IDs as prompt (list of integers or list of lists of integers)
        messages = [{"role": "user", "content": prompt}]  # type: ignore
            f"Unmapped prompt format. Your prompt is neither a list of strings nor a string. prompt={prompt}. File an issue - https://github.com/BerriAI/litellm/issues"
    kwargs.pop("prompt", None)
    if _model is not None and (
    ):  # for openai compatible endpoints - e.g. vllm, call the native /v1/completions endpoint for text completion calls
        if _model not in litellm.open_ai_chat_completion_models:
            model = "text-completion-openai/" + _model
            optional_params.pop("custom_llm_provider", None)
        raise ValueError("model is not set. Set either via 'model' or 'engine' param.")
    kwargs["text_completion"] = True
    response = completion(
        **optional_params,
        stream is True
        or kwargs.get("stream", False) is True
        response = TextCompletionStreamWrapper(
    elif isinstance(response, TextCompletionStreamWrapper):
    # OpenAI Text / Azure Text will return here
    text_completion_response = (
        litellm.utils.LiteLLMResponseObjectHandler.convert_chat_to_text_completion(
###### Adapter Completion ################
async def aadapter_completion(
    *, adapter_id: str, **kwargs
) -> Optional[Union[BaseModel, AdapterCompletionStreamWrapper]]:
    Implemented to handle async calls for adapter_completion()
        translation_obj: Optional[CustomLogger] = None
        for item in litellm.adapters:
            if item["id"] == adapter_id:
                translation_obj = item["adapter"]
        if translation_obj is None:
                "No matching adapter given. Received 'adapter_id'={}, litellm.adapters={}".format(
                    adapter_id, litellm.adapters
        new_kwargs = translation_obj.translate_completion_input_params(kwargs=kwargs)
        response: Union[ModelResponse, CustomStreamWrapper] = await acompletion(**new_kwargs)  # type: ignore
        translated_response: Optional[
            Union[BaseModel, AdapterCompletionStreamWrapper]
        if isinstance(response, ModelResponse):
            translated_response = translation_obj.translate_completion_output_params(
                response=response
            translated_response = (
                translation_obj.translate_completion_output_params_streaming(
                    completion_stream=response
        return translated_response
async def aadapter_generate_content(
) -> Union[Dict[str, Any], AsyncIterator[bytes]]:
    from litellm.google_genai.adapters.handler import GenerateContentToCompletionHandler
    coro = cast(
        Coroutine[Any, Any, Union[Dict[str, Any], AsyncIterator[bytes]]],
        GenerateContentToCompletionHandler.generate_content_handler(
            **kwargs, _is_async=True
def adapter_completion(
    response: Union[ModelResponse, CustomStreamWrapper] = completion(**new_kwargs)  # type: ignore
    translated_response: Optional[Union[BaseModel, AdapterCompletionStreamWrapper]] = (
    elif isinstance(response, CustomStreamWrapper) or inspect.isgenerator(response):
##### Moderation #######################
def moderation(
    input: str, model: Optional[str] = None, api_key: Optional[str] = None, **kwargs
) -> OpenAIModerationResponse:
    # only supports open ai for now
    # Extract api_base from kwargs
    openai_client = kwargs.get("client", None)
    if openai_client is None:
            openai_client = openai.OpenAI(api_key=api_key, base_url=api_base)
            openai_client = openai.OpenAI(api_key=api_key)
    if model is not None:
        response = openai_client.moderations.create(input=input, model=model)
        response = openai_client.moderations.create(input=input)
    response_dict: Dict = response.model_dump()
    return litellm.utils.LiteLLMResponseObjectHandler.convert_to_moderation_response(
        response_object=response_dict,
async def amoderation(
    optional_params = GenericLiteLLMParams(**kwargs)
    litellm_logging_obj: Optional[LiteLLMLoggingObj] = kwargs.get(
        "litellm_logging_obj", None
    _dynamic_api_base = None
            custom_llm_provider,
            _dynamic_api_key,
            _dynamic_api_base,
        ) = litellm.get_llm_provider(
            model=model or "",
            api_base=optional_params.api_base,
            api_key=optional_params.api_key,
    except litellm.BadRequestError:
        # `model` is optional field for moderation - get_llm_provider will throw BadRequestError if model is not set / not recognized
    if openai_client is None or not isinstance(openai_client, AsyncOpenAI):
        # call helper to get OpenAI client
        # _get_openai_client maintains in-memory caching logic for OpenAI clients
        _openai_client: AsyncOpenAI = openai_chat_completions._get_openai_client(  # type: ignore
            is_async=True,
            api_base=optional_params.api_base or _dynamic_api_base,
        _openai_client = openai_client
    # update litellm_logging_obj with environment variables
    custom_llm_provider = custom_llm_provider or litellm.LlmProviders.OPENAI.value
    if litellm_logging_obj is not None:
        litellm_logging_obj.update_environment_variables(
            user=kwargs.get("user", None),
            optional_params={},
            litellm_params={
        response = await _openai_client.moderations.create(input=input, model=model)
        response = await _openai_client.moderations.create(input=input)
##### Transcription #######################
async def atranscription(*args, **kwargs) -> TranscriptionResponse:
    Calls openai + azure whisper endpoints.
    Allows router to load balance between them
    ### PASS ARGS TO Image Generation ###
    kwargs["atranscription"] = True
    file = kwargs.get("file", None)
        func = partial(transcription, *args, **kwargs)
            model=model, api_base=kwargs.get("api_base", None)
            response = TranscriptionResponse(**init_response)
        elif isinstance(init_response, TranscriptionResponse):  ## CACHING SCENARIO
            # Call the synchronous function using run_in_executor
            response = await loop.run_in_executor(None, func_with_context)
        if not isinstance(response, TranscriptionResponse):
                f"Invalid response from transcription provider, expected TranscriptionResponse, but got {type(response)}"
        # Calculate and add duration if response is missing it
            and not isinstance(response, Coroutine)
            and file is not None
            # Check if response is missing duration
            existing_duration = getattr(response, "duration", None)
            if existing_duration is None:
                calculated_duration = calculate_request_duration(file)
                if calculated_duration is not None:
                    setattr(response, "duration", calculated_duration)
def transcription(
    ## OPTIONAL OPENAI PARAMS ##
    language: Optional[str] = None,
    response_format: Optional[
        Literal["json", "text", "srt", "verbose_json", "vtt"]
    timestamp_granularities: Optional[List[Literal["word", "segment"]]] = None,
    temperature: Optional[int] = None,  # openai defaults this to 0
    ## LITELLM PARAMS ##
) -> Union[TranscriptionResponse, Coroutine[Any, Any, TranscriptionResponse]]:
    litellm_call_id = kwargs.get("litellm_call_id", None)
    atranscription = kwargs.pop("atranscription", False)
    kwargs.pop("tags", [])
    non_default_params = get_non_default_transcription_params(kwargs)
    client: Optional[
            openai.AsyncOpenAI,
            openai.OpenAI,
            openai.AzureOpenAI,
            openai.AsyncAzureOpenAI,
    ] = kwargs.pop("client", None)
    if litellm_logging_obj:
        litellm_logging_obj.model_call_details["client"] = str(client)
    if max_retries is None:
        max_retries = openai.DEFAULT_MAX_RETRIES
    model_response = litellm.utils.TranscriptionResponse()
    optional_params = get_optional_params_transcription(
        timestamp_granularities=timestamp_granularities,
            "litellm_call_id": litellm_call_id,
            "proxy_server_request": proxy_server_request,
            "model_info": model_info,
            "preset_cache_key": None,
            "stream_response": {},
        Union[TranscriptionResponse, Coroutine[Any, Any, TranscriptionResponse]]
    provider_config = ProviderConfigManager.get_provider_audio_transcription_config(
        provider=LlmProviders(custom_llm_provider),
            api_version or litellm.api_version or get_secret_str("AZURE_API_VERSION")
        azure_ad_token = kwargs.pop("azure_ad_token", None) or get_secret_str(
            "AZURE_AD_TOKEN"
        response = azure_audio_transcriptions.audio_transcriptions(
            audio_file=file,
            atranscription=atranscription,
            logging_obj=litellm_logging_obj,
    elif custom_llm_provider == "openai" or (
        custom_llm_provider in litellm.openai_compatible_providers
        api_key = api_key or litellm.api_key or litellm.openai_key or get_secret("OPENAI_API_KEY")  # type: ignore
        response = openai_audio_transcriptions.audio_transcriptions(
    elif provider_config is not None:
        response = base_llm_http_handler.audio_transcriptions(
            client=(
                client
                if client is not None
                    isinstance(client, HTTPHandler)
                    or isinstance(client, AsyncHTTPHandler)
    if response is not None and not isinstance(response, Coroutine):
        raise ValueError("Unmapped provider passed in. Unable to get the response.")
async def aspeech(*args, **kwargs) -> HttpxBinaryResponseContent:
    Calls openai tts endpoints.
    kwargs["aspeech"] = True
        func = partial(speech, *args, **kwargs)
        if asyncio.iscoroutine(init_response):
        return response  # type: ignore
def speech(  # noqa: PLR0915
    voice: Optional[Union[str, dict]] = None,
    project: Optional[str] = None,
    metadata: Optional[dict] = None,
    timeout: Optional[Union[float, httpx.Timeout]] = None,
    response_format: Optional[str] = None,
    speed: Optional[int] = None,
    instructions: Optional[str] = None,
    client=None,
    aspeech: Optional[bool] = None,
) -> Union[HttpxBinaryResponseContent, Coroutine[Any, Any, HttpxBinaryResponseContent]]:
    user = kwargs.get("user", None)
    litellm_call_id: Optional[str] = kwargs.get("litellm_call_id", None)
        model=model, custom_llm_provider=custom_llm_provider, api_base=api_base
    optional_params = {}
    if response_format is not None:
        optional_params["response_format"] = response_format
        optional_params["speed"] = speed  # type: ignore
    if instructions is not None:
        optional_params["instructions"] = instructions
        timeout = litellm.request_timeout
        max_retries = litellm.num_retries or openai.DEFAULT_MAX_RETRIES
    # Get provider-specific text-to-speech config and map parameters
    text_to_speech_provider_config = (
        ProviderConfigManager.get_provider_text_to_speech_config(
            provider=litellm.LlmProviders(custom_llm_provider),
    # Map OpenAI params to provider-specific params if config exists
    if text_to_speech_provider_config is not None:
        voice, optional_params = text_to_speech_provider_config.map_openai_params(
            voice=voice,
            drop_params=False,
            kwargs=kwargs,
    logging_obj: LiteLLMLoggingObj = cast(
        LiteLLMLoggingObj, kwargs.get("litellm_logging_obj")
    logging_obj.update_environment_variables(
        Coroutine[Any, Any, HttpxBinaryResponseContent],
        if voice is None or not (isinstance(voice, str)):
                message="'voice' is required to be passed as a string for OpenAI TTS",
                llm_provider=custom_llm_provider,
            or litellm.api_key  # for deepinfra/perplexity/anyscale we check in get_llm_provider and pass in the api key from there
        project = (
            project
            or litellm.project
            or get_secret("OPENAI_PROJECT")
        response = openai_chat_completions.audio_speech(
            aspeech=aspeech,
    elif custom_llm_provider == "azure":
        # Check if this is Azure Speech Service (Cognitive Services TTS)
        if model.startswith("speech/"):
            from litellm.llms.azure.text_to_speech.transformation import (
                AzureAVATextToSpeechConfig,
            # Azure AVA (Cognitive Services) Text-to-Speech
            if text_to_speech_provider_config is None:
                    message="Azure Speech Service configuration not found",
            # Cast to specific Azure config type to access dispatch method
            azure_config = cast(
                AzureAVATextToSpeechConfig, text_to_speech_provider_config
            response = azure_config.dispatch_text_to_speech(  # type: ignore
                litellm_params_dict=litellm_params_dict,
                logging_obj=logging_obj,
                base_llm_http_handler=base_llm_http_handler,
                aspeech=aspeech or False,
            # Azure OpenAI TTS
                    message="'voice' is required to be passed as a string for Azure TTS",
            api_base = api_base or litellm.api_base or get_secret("AZURE_API_BASE")  # type: ignore
            api_version = api_version or litellm.api_version or get_secret("AZURE_API_VERSION")  # type: ignore
                or get_secret("AZURE_OPENAI_API_KEY")
                or get_secret("AZURE_API_KEY")
            azure_ad_token: Optional[str] = optional_params.get("extra_body", {}).pop(  # type: ignore
            ) or get_secret(
            response = azure_chat_completions.audio_speech(
    elif custom_llm_provider == "elevenlabs":
        from litellm.llms.elevenlabs.text_to_speech.transformation import (
            ElevenLabsTextToSpeechConfig,
            text_to_speech_provider_config = ElevenLabsTextToSpeechConfig()
        elevenlabs_config = cast(
            ElevenLabsTextToSpeechConfig, text_to_speech_provider_config
        voice_id = voice if isinstance(voice, str) else None
        if voice_id is None or not voice_id.strip():
                message="'voice' must resolve to an ElevenLabs voice id for ElevenLabs TTS",
        voice_id = voice_id.strip()
        query_params = kwargs.pop(
            ElevenLabsTextToSpeechConfig.ELEVENLABS_QUERY_PARAMS_KEY, None
        if isinstance(query_params, dict):
            litellm_params_dict[
                ElevenLabsTextToSpeechConfig.ELEVENLABS_QUERY_PARAMS_KEY
            ] = query_params
        litellm_params_dict[ElevenLabsTextToSpeechConfig.ELEVENLABS_VOICE_ID_KEY] = (
            voice_id
            litellm_params_dict["api_base"] = api_base
            litellm_params_dict["api_key"] = api_key
        response = base_llm_http_handler.text_to_speech_handler(
            voice=voice_id,
            text_to_speech_provider_config=elevenlabs_config,
            text_to_speech_optional_params=optional_params,
            _is_async=aspeech or False,
    elif custom_llm_provider == "vertex_ai" or custom_llm_provider == "vertex_ai_beta":
        from litellm.llms.vertex_ai.text_to_speech.transformation import (
            VertexAITextToSpeechConfig,
        generic_optional_params = GenericLiteLLMParams(**kwargs)
        # Handle Gemini models separately (they use speech_to_completion_bridge)
        if "gemini" in model:
            from .endpoints.speech.speech_to_completion_bridge.handler import (
                speech_to_completion_bridge_handler,
            return speech_to_completion_bridge_handler.speech(
        # Vertex AI Text-to-Speech (Google Cloud TTS)
            text_to_speech_provider_config = VertexAITextToSpeechConfig()
        # Cast to specific Vertex AI config type to access dispatch method
        vertex_config = cast(VertexAITextToSpeechConfig, text_to_speech_provider_config)
        # Store Vertex AI specific params in litellm_params_dict
        litellm_params_dict.update(
                "vertex_project": generic_optional_params.vertex_project,
                "vertex_location": generic_optional_params.vertex_location,
                "vertex_credentials": generic_optional_params.vertex_credentials,
        response = vertex_config.dispatch_text_to_speech(
            api_base=generic_optional_params.api_base,
            api_key=None,  # Vertex AI uses OAuth, not API key
    elif custom_llm_provider == "runwayml":
        from litellm.llms.runwayml.text_to_speech.transformation import (
            RunwayMLTextToSpeechConfig,
        # RunwayML Text-to-Speech
                message="RunwayML Text-to-Speech configuration not found",
        # Cast to specific RunwayML config type to access dispatch method
        runwayml_config = cast(
            RunwayMLTextToSpeechConfig, text_to_speech_provider_config
        response = runwayml_config.dispatch_text_to_speech(  # type: ignore
        from litellm.llms.minimax.text_to_speech.transformation import (
            MinimaxTextToSpeechConfig,
        # MiniMax Text-to-Speech
            text_to_speech_provider_config = MinimaxTextToSpeechConfig()
        minimax_config = cast(MinimaxTextToSpeechConfig, text_to_speech_provider_config)
        # Convert voice to string if it's a dict (minimax handler expects Optional[str])
        voice_str: Optional[str] = None
        if isinstance(voice, str):
            voice_str = voice
        elif isinstance(voice, dict):
            # Extract voice_id from dict if needed
            voice_str = voice.get("voice_id") or voice.get("id") or voice.get("name")
            voice=voice_str,
            text_to_speech_provider_config=minimax_config,
    elif custom_llm_provider == "aws_polly":
        from litellm.llms.aws_polly.text_to_speech.transformation import (
            AWSPollyTextToSpeechConfig,
        # AWS Polly Text-to-Speech
            text_to_speech_provider_config = AWSPollyTextToSpeechConfig()
        # Cast to specific AWS Polly config type to access dispatch method
        aws_polly_config = cast(
            AWSPollyTextToSpeechConfig, text_to_speech_provider_config
        response = aws_polly_config.dispatch_text_to_speech(
            "Unable to map the custom llm provider={} to a known provider={}.".format(
                custom_llm_provider, litellm.provider_list
##### Health Endpoints #######################
async def ahealth_check(
    model_params: dict,
    mode: Optional[
            "chat",
            "completion",
            "embedding",
            "audio_speech",
            "audio_transcription",
            "video_generation",
            "rerank",
            "realtime",
            "responses",
    ] = "chat",
    input: Optional[List] = None,
    Support health checks for different providers. Return remaining rate limit, etc.
            "x-ratelimit-remaining-requests": int,
            "x-ratelimit-remaining-tokens": int,
            "x-ms-region": str,
    from litellm.litellm_core_utils.cached_imports import get_litellm_logging_class
    from litellm.litellm_core_utils.health_check_helpers import HealthCheckHelpers
    # Use cached import helper to lazy-load Logging class (only loads when function is called)
    Logging = get_litellm_logging_class()
    # Map modes to their corresponding health check calls
    # Init request with tracking information
    litellm_logging_obj = Logging(
        model="",
        call_type="acompletion",
        litellm_call_id=str(uuid.uuid4()),
        start_time=datetime.datetime.now(),
        function_id=str(uuid.uuid4()),
        log_raw_request_response=True,
    model_params["litellm_logging_obj"] = litellm_logging_obj
    model_params = (
        HealthCheckHelpers._update_model_params_with_health_check_tracking_information(
            model_params=model_params
        model: Optional[str] = model_params.get("model", None)
            raise Exception("model not set")
        if model in litellm.model_cost and mode is None:
            mode = litellm.model_cost[model].get("mode")
        custom_llm_provider_from_params = model_params.get("custom_llm_provider", None)
        api_base_from_params = model_params.get("api_base", None)
        api_key_from_params = model_params.get("api_key", None)
        model, custom_llm_provider, _, _ = get_llm_provider(
            custom_llm_provider=custom_llm_provider_from_params,
            api_base=api_base_from_params,
            api_key=api_key_from_params,
        model_params["cache"] = {
            "no-cache": True
        }  # don't used cached responses for making health check calls
        mode = mode or "chat"
        if "*" in model:
            return await HealthCheckHelpers.ahealth_check_wildcard_models(
                model_params=model_params,
                litellm_logging_obj=litellm_logging_obj,
        mode_handlers = HealthCheckHelpers.get_mode_handlers(
        if mode in mode_handlers:
            _response = await mode_handlers[mode]()
            # Only process headers for chat mode
            _response_headers: dict = (
                getattr(_response, "_hidden_params", {}).get("headers", {}) or {}
            return _create_health_check_response(_response_headers)
                f"Mode {mode} not supported. See modes here: https://docs.litellm.ai/docs/proxy/health"
        stack_trace = traceback.format_exc()
        if isinstance(stack_trace, str):
            stack_trace = stack_trace[:1000]
        if mode is None:
                "error": f"error:{str(e)}. Missing `mode`. Set the `mode` for the model - https://docs.litellm.ai/docs/proxy/health#embedding-models  \nstacktrace: {stack_trace}"
        error_to_return = str(e) + "\nstack trace: " + stack_trace
        raw_request_typed_dict = litellm_logging_obj.model_call_details.get(
            "raw_request_typed_dict"
            "error": error_to_return,
            "raw_request_typed_dict": raw_request_typed_dict,
####### HELPER FUNCTIONS ################
## Set verbose to true -> ```litellm.set_verbose = True```
def print_verbose(print_statement):
        verbose_logger.debug(print_statement)
        if litellm.set_verbose:
            print(print_statement)  # noqa
def config_completion(**kwargs):
    if litellm.config_path is not None:
        config_args = read_config_args(litellm.config_path)
        # overwrite any args passed in with config args
        return completion(**kwargs, **config_args)
            "No config path set, please set a config path using `litellm.config_path = 'path/to/config.json'`"
def stream_chunk_builder_text_completion(
    chunks: list, messages: Optional[List] = None
) -> TextCompletionResponse:
    id = chunks[0]["id"]
    object = chunks[0]["object"]
    created = chunks[0]["created"]
    model = chunks[0]["model"]
    system_fingerprint = chunks[0].get("system_fingerprint", None)
    finish_reason = chunks[-1]["choices"][0]["finish_reason"]
    logprobs = chunks[-1]["choices"][0]["logprobs"]
        "id": id,
        "object": object,
        "created": created,
        "system_fingerprint": system_fingerprint,
        "choices": [
                "text": None,
                "finish_reason": finish_reason,
        "usage": {
            "prompt_tokens": None,
            "completion_tokens": None,
            "total_tokens": None,
    content_list = []
        choices = chunk["choices"]
        for choice in choices:
                choice is not None
                and hasattr(choice, "text")
                and choice.get("text") is not None
                _choice = choice.get("text")
                content_list.append(_choice)
    # Combine the "content" strings into a single string || combine the 'function' strings into a single string
    combined_content = "".join(content_list)
    # Update the "content" field within the response dictionary
    response["choices"][0]["text"] = combined_content
    if len(combined_content) > 0:
    # # Update usage information if needed
        response["usage"]["prompt_tokens"] = token_counter(
            model=model, messages=messages
    ):  # don't allow this failing to block a complete streaming response from being returned
        print_verbose("token_counter failed, assuming prompt tokens is 0")
        response["usage"]["prompt_tokens"] = 0
    response["usage"]["completion_tokens"] = token_counter(
        text=combined_content,
        count_response_tokens=True,  # count_response_tokens is a Flag to tell token counter this is a response, No need to add extra tokens we do for input messages
    response["usage"]["total_tokens"] = (
        response["usage"]["prompt_tokens"] + response["usage"]["completion_tokens"]
    return TextCompletionResponse(**response)
def stream_chunk_builder(  # noqa: PLR0915
    chunks: list,
    messages: Optional[list] = None,
    end_time=None,
    logging_obj: Optional["Logging"] = None,
) -> Optional[Union[ModelResponse, TextCompletionResponse]]:
        if chunks is None:
            raise litellm.APIError(
                status_code=500,
                message="Error building chunks for logging/streaming usage calculation",
                llm_provider="",
        if not chunks:
        processor = ChunkProcessor(chunks, messages)
        chunks = processor.chunks
        ### BASE-CASE ###
        if len(chunks) == 0:
        ## Route to the text completion logic
            chunks[0]["choices"][0], litellm.utils.TextChoices
        ):  # route to the text completion logic
            return stream_chunk_builder_text_completion(
                chunks=chunks, messages=messages
        # Initialize the response dictionary
        response = processor.build_base_response(chunks)
            chunk
            for chunk in chunks
            if len(chunk["choices"]) > 0
            and "tool_calls" in chunk["choices"][0]["delta"]
            and chunk["choices"][0]["delta"]["tool_calls"] is not None
        if len(tool_call_chunks) > 0:
            tool_calls_list = processor.get_combined_tool_content(tool_call_chunks)
            _choice = cast(Choices, response.choices[0])
            _choice.message.content = None
            _choice.message.tool_calls = tool_calls_list
        function_call_chunks = [
            and "function_call" in chunk["choices"][0]["delta"]
            and chunk["choices"][0]["delta"]["function_call"] is not None
        if len(function_call_chunks) > 0:
            _choice.message.function_call = (
                processor.get_combined_function_call_content(function_call_chunks)
        content_chunks = [
            and "content" in chunk["choices"][0]["delta"]
            and chunk["choices"][0]["delta"]["content"] is not None
        if len(content_chunks) > 0:
            response["choices"][0]["message"]["content"] = (
                processor.get_combined_content(content_chunks)
        thinking_blocks = [
            and "thinking_blocks" in chunk["choices"][0]["delta"]
            and chunk["choices"][0]["delta"]["thinking_blocks"] is not None
        if len(thinking_blocks) > 0:
            response["choices"][0]["message"]["thinking_blocks"] = (
                processor.get_combined_thinking_content(thinking_blocks)
        reasoning_chunks = [
            and "reasoning_content" in chunk["choices"][0]["delta"]
            and chunk["choices"][0]["delta"]["reasoning_content"] is not None
        if len(reasoning_chunks) > 0:
            response["choices"][0]["message"]["reasoning_content"] = (
                processor.get_combined_reasoning_content(reasoning_chunks)
        annotation_chunks = [
            and "annotations" in chunk["choices"][0]["delta"]
            and chunk["choices"][0]["delta"]["annotations"] is not None
        if len(annotation_chunks) > 0:
            annotations = annotation_chunks[0]["choices"][0]["delta"]["annotations"]
            response["choices"][0]["message"]["annotations"] = annotations
        audio_chunks = [
            and "audio" in chunk["choices"][0]["delta"]
            and chunk["choices"][0]["delta"]["audio"] is not None
        if len(audio_chunks) > 0:
            _choice.message.audio = processor.get_combined_audio_content(audio_chunks)
        # Handle image chunks from models like gemini-2.5-flash-image
        # See: https://github.com/BerriAI/litellm/issues/19478
        image_chunks = [
            and "images" in chunk["choices"][0]["delta"]
            and chunk["choices"][0]["delta"]["images"] is not None
        if len(image_chunks) > 0:
            # Images come complete in a single chunk, collect all images from all chunks
            all_images = []
            for chunk in image_chunks:
                all_images.extend(chunk["choices"][0]["delta"]["images"])
            response["choices"][0]["message"]["images"] = all_images
        # Combine provider_specific_fields from streaming chunks (e.g., web_search_results, citations)
        # See: https://github.com/BerriAI/litellm/issues/17737
        provider_specific_chunks = [
            and "provider_specific_fields" in chunk["choices"][0]["delta"]
            and chunk["choices"][0]["delta"]["provider_specific_fields"] is not None
        if len(provider_specific_chunks) > 0:
            combined_provider_fields: Dict[str, Any] = {}
            for chunk in provider_specific_chunks:
                fields = chunk["choices"][0]["delta"]["provider_specific_fields"]
                if isinstance(fields, dict):
                    for key, value in fields.items():
                        if key not in combined_provider_fields:
                            combined_provider_fields[key] = value
                        elif isinstance(value, list) and isinstance(
                            combined_provider_fields[key], list
                            # For lists like web_search_results, take the last (most complete) one
            if combined_provider_fields:
                _choice.message.provider_specific_fields = combined_provider_fields
        completion_output = get_content_from_model_response(response)
        reasoning_tokens = processor.count_reasoning_tokens(response)
        usage = processor.calculate_usage(
            chunks=chunks,
            completion_output=completion_output,
            reasoning_tokens=reasoning_tokens,
        setattr(response, "usage", usage)
        # Add cost to usage object if include_cost_in_streaming_usage is True
        if litellm.include_cost_in_streaming_usage and logging_obj is not None:
                usage, "cost", logging_obj._response_cost_calculator(result=response)
        verbose_logger.exception(
            "litellm.main.py::stream_chunk_builder() - Exception occurred - {}".format(
                str(e)
# Cache for encoding to avoid repeated __getattr__ calls
_encoding_cache: Optional[Any] = None
def _get_encoding():
    """Get encoding, loading it lazily if needed."""
    global _encoding_cache
    if _encoding_cache is None:
        # Access via module to trigger __getattr__ if not cached
        _encoding_cache = sys.modules[__name__].encoding
    return _encoding_cache
    """Lazy import handler for main module"""
        # Use _get_default_encoding which properly sets TIKTOKEN_CACHE_DIR
        # before loading tiktoken, ensuring the local cache is used
        # instead of downloading from the internet
        from litellm._lazy_imports import _get_default_encoding
        _encoding = _get_default_encoding()
        # Cache it in the module's __dict__ for subsequent accesses
        sys.modules[__name__].__dict__["encoding"] = _encoding
        _encoding_cache = _encoding
        return _encoding
LiteLLM A2A SDK functions.
Provides standalone functions with @client decorator for LiteLLM logging integration.
from typing import TYPE_CHECKING, Any, AsyncIterator, Coroutine, Dict, Optional, Union
from litellm._logging import verbose_logger, verbose_proxy_logger
from litellm.a2a_protocol.streaming_iterator import A2AStreamingIterator
from litellm.a2a_protocol.utils import A2ARequestUtils
from litellm.constants import DEFAULT_A2A_AGENT_TIMEOUT
from litellm.llms.custom_httpx.http_handler import (
    get_async_httpx_client,
    httpxSpecialProvider,
from litellm.utils import client
# Runtime imports with availability check
A2A_SDK_AVAILABLE = False
A2ACardResolver: Any = None
_A2AClient: Any = None
    from a2a.client import A2AClient as _A2AClient  # type: ignore[no-redef]
    A2A_SDK_AVAILABLE = True
# Import our custom card resolver that supports multiple well-known paths
from litellm.a2a_protocol.card_resolver import LiteLLMA2ACardResolver
from litellm.a2a_protocol.exception_mapping_utils import (
    handle_a2a_localhost_retry,
    map_a2a_exception,
from litellm.a2a_protocol.exceptions import A2ALocalhostURLError
# Use our custom resolver instead of the default A2A SDK resolver
A2ACardResolver = LiteLLMA2ACardResolver
def _set_usage_on_logging_obj(
    prompt_tokens: int,
    completion_tokens: int,
    Set usage on litellm_logging_obj for standard logging payload.
        kwargs: The kwargs dict containing litellm_logging_obj
        prompt_tokens: Number of input tokens
        completion_tokens: Number of output tokens
    litellm_logging_obj = kwargs.get("litellm_logging_obj")
        usage = litellm.Usage(
            prompt_tokens=prompt_tokens,
            completion_tokens=completion_tokens,
            total_tokens=prompt_tokens + completion_tokens,
        litellm_logging_obj.model_call_details["usage"] = usage
def _set_agent_id_on_logging_obj(
    agent_id: Optional[str],
    Set agent_id on litellm_logging_obj for SpendLogs tracking.
        agent_id: The A2A agent ID
    if agent_id is None:
        # Set agent_id directly on model_call_details (same pattern as custom_llm_provider)
        litellm_logging_obj.model_call_details["agent_id"] = agent_id
def _get_a2a_model_info(a2a_client: Any, kwargs: Dict[str, Any]) -> str:
    Extract agent info and set model/custom_llm_provider for cost tracking.
    Sets model info on the litellm_logging_obj if available.
    Returns the agent name for logging.
    agent_name = "unknown"
    # Try to get agent card from our stored attribute first, then fallback to SDK attribute
    agent_card = getattr(a2a_client, "_litellm_agent_card", None)
    if agent_card is None:
        agent_card = getattr(a2a_client, "agent_card", None)
    if agent_card is not None:
        agent_name = getattr(agent_card, "name", "unknown") or "unknown"
    # Build model string
    model = f"a2a_agent/{agent_name}"
    custom_llm_provider = "a2a_agent"
    # Set on litellm_logging_obj if available (for standard logging payload)
        litellm_logging_obj.model = model
        litellm_logging_obj.custom_llm_provider = custom_llm_provider
        litellm_logging_obj.model_call_details["model"] = model
        litellm_logging_obj.model_call_details[
            "custom_llm_provider"
        ] = custom_llm_provider
    return agent_name
async def asend_message(
    a2a_client: Optional["A2AClientType"] = None,
    request: Optional["SendMessageRequest"] = None,
    litellm_params: Optional[Dict[str, Any]] = None,
    agent_id: Optional[str] = None,
    Async: Send a message to an A2A agent.
    Uses the @client decorator for LiteLLM logging and tracking.
    If litellm_params contains custom_llm_provider, routes through the completion bridge.
        a2a_client: An initialized a2a.client.A2AClient instance (optional if using completion bridge)
        request: SendMessageRequest from a2a.types (optional if using completion bridge with api_base)
        api_base: API base URL (required for completion bridge, optional for standard A2A)
        litellm_params: Optional dict with custom_llm_provider, model, etc. for completion bridge
        agent_id: Optional agent ID for tracking in SpendLogs
        **kwargs: Additional arguments passed to the client decorator
        LiteLLMSendMessageResponse (wraps a2a SendMessageResponse with _hidden_params)
    Example (standard A2A):
        from litellm.a2a_protocol import asend_message, create_a2a_client
        a2a_client = await create_a2a_client(base_url="http://localhost:10001")
                message={"role": "user", "parts": [{"kind": "text", "text": "Hello!"}], "messageId": uuid4().hex}
        response = await asend_message(a2a_client=a2a_client, request=request)
    Example (completion bridge with LangGraph):
            api_base="http://localhost:2024",
            litellm_params={"custom_llm_provider": "langgraph", "model": "agent"},
    litellm_params = litellm_params or {}
    custom_llm_provider = litellm_params.get("custom_llm_provider")
    # Route through completion bridge if custom_llm_provider is set
            raise ValueError("request is required for completion bridge")
        # api_base is optional for providers that derive endpoint from model (e.g., bedrock/agentcore)
            f"A2A using completion bridge: provider={custom_llm_provider}, api_base={api_base}"
        from litellm.a2a_protocol.litellm_completion_bridge.handler import (
            A2ACompletionBridgeHandler,
        # Extract params from request
        params = (
            request.params.model_dump(mode="json")
            if hasattr(request.params, "model_dump")
            else dict(request.params)
        response_dict = await A2ACompletionBridgeHandler.handle_non_streaming(
            request_id=str(request.id),
        # Convert to LiteLLMSendMessageResponse
        return LiteLLMSendMessageResponse.from_dict(response_dict)
    # Standard A2A client flow
        raise ValueError("request is required")
    # Create A2A client if not provided but api_base is available
    if a2a_client is None:
                "Either a2a_client or api_base is required for standard A2A flow"
        trace_id = str(uuid.uuid4())
        extra_headers = {"X-LiteLLM-Trace-Id": trace_id}
        if agent_id:
            extra_headers["X-LiteLLM-Agent-Id"] = agent_id
        a2a_client = await create_a2a_client(base_url=api_base, extra_headers=extra_headers)
    # Type assertion: a2a_client is guaranteed to be non-None here
    assert a2a_client is not None
    agent_name = _get_a2a_model_info(a2a_client, kwargs)
    verbose_logger.info(f"A2A send_message request_id={request.id}, agent={agent_name}")
    # Get agent card URL for localhost retry logic
    agent_card = getattr(a2a_client, "_litellm_agent_card", None) or getattr(
        a2a_client, "agent_card", None
    card_url = getattr(agent_card, "url", None) if agent_card else None
    # Retry loop: if connection fails due to localhost URL in agent card, retry with fixed URL
    a2a_response = None
    for _ in range(2):  # max 2 attempts: original + 1 retry
            a2a_response = await a2a_client.send_message(request)
            break  # success, exit retry loop
        except A2ALocalhostURLError as e:
            # Localhost URL error - fix and retry
            a2a_client = handle_a2a_localhost_retry(
                error=e,
                agent_card=agent_card,
                a2a_client=a2a_client,
                is_streaming=False,
            card_url = agent_card.url if agent_card else None
            # Map exception - will raise A2ALocalhostURLError if applicable
                map_a2a_exception(e, card_url, api_base, model=agent_name)
            except A2ALocalhostURLError as localhost_err:
                    error=localhost_err,
                # Re-raise the mapped exception
    verbose_logger.info(f"A2A send_message completed, request_id={request.id}")
    # a2a_response is guaranteed to be set if we reach here (loop breaks on success or raises)
    assert a2a_response is not None
    # Wrap in LiteLLM response type for _hidden_params support
    response = LiteLLMSendMessageResponse.from_a2a_response(a2a_response)
    # Calculate token usage from request and response
    response_dict = a2a_response.model_dump(mode="json", exclude_none=True)
        prompt_tokens,
        completion_tokens,
    ) = A2ARequestUtils.calculate_usage_from_request_response(
        response_dict=response_dict,
    # Set usage on logging obj for standard logging payload
    _set_usage_on_logging_obj(
    # Set agent_id on logging obj for SpendLogs tracking
    _set_agent_id_on_logging_obj(kwargs=kwargs, agent_id=agent_id)
def send_message(
    a2a_client: "A2AClientType",
    request: "SendMessageRequest",
) -> Union[LiteLLMSendMessageResponse, Coroutine[Any, Any, LiteLLMSendMessageResponse]]:
    Sync: Send a message to an A2A agent.
        a2a_client: An initialized a2a.client.A2AClient instance
        request: SendMessageRequest from a2a.types
        loop = None
        return asend_message(a2a_client=a2a_client, request=request, **kwargs)
        return asyncio.run(
            asend_message(a2a_client=a2a_client, request=request, **kwargs)
def _build_streaming_logging_obj(
    request: "SendStreamingMessageRequest",
    agent_name: str,
    litellm_params: Optional[Dict[str, Any]],
    metadata: Optional[Dict[str, Any]],
    proxy_server_request: Optional[Dict[str, Any]],
) -> Logging:
    """Build logging object for streaming A2A requests."""
    logging_obj = Logging(
        messages=[{"role": "user", "content": "streaming-request"}],
        call_type="asend_message_streaming",
        litellm_call_id=str(request.id),
        function_id=str(request.id),
    logging_obj.model = model
    logging_obj.custom_llm_provider = "a2a_agent"
    logging_obj.model_call_details["model"] = model
    logging_obj.model_call_details["custom_llm_provider"] = "a2a_agent"
        logging_obj.model_call_details["agent_id"] = agent_id
    _litellm_params = litellm_params.copy() if litellm_params else {}
        _litellm_params["metadata"] = metadata
    if proxy_server_request:
        _litellm_params["proxy_server_request"] = proxy_server_request
    logging_obj.litellm_params = _litellm_params
    logging_obj.optional_params = _litellm_params
    logging_obj.model_call_details["litellm_params"] = _litellm_params
    logging_obj.model_call_details["metadata"] = metadata or {}
    return logging_obj
async def asend_message_streaming(
    request: Optional["SendStreamingMessageRequest"] = None,
    metadata: Optional[Dict[str, Any]] = None,
    proxy_server_request: Optional[Dict[str, Any]] = None,
    Async: Send a streaming message to an A2A agent.
        request: SendStreamingMessageRequest from a2a.types
        api_base: API base URL (required for completion bridge)
        metadata: Optional metadata dict (contains user_api_key, user_id, team_id, etc.)
        proxy_server_request: Optional proxy server request data
        SendStreamingMessageResponse chunks from the agent
        from litellm.a2a_protocol import asend_message_streaming
        from a2a.types import SendStreamingMessageRequest, MessageSendParams
        request = SendStreamingMessageRequest(
        async for chunk in asend_message_streaming(
            print(chunk)
            f"A2A streaming using completion bridge: provider={custom_llm_provider}"
        async for chunk in A2ACompletionBridgeHandler.handle_streaming(
        a2a_client = await create_a2a_client(base_url=api_base)
    verbose_logger.info(f"A2A send_message_streaming request_id={request.id}")
    # Build logging object for streaming completion callbacks
    agent_name = getattr(agent_card, "name", "unknown") if agent_card else "unknown"
    logging_obj = _build_streaming_logging_obj(
        agent_name=agent_name,
        agent_id=agent_id,
    # Connection errors in streaming typically occur on first chunk iteration
    first_chunk = True
    for attempt in range(2):  # max 2 attempts: original + 1 retry
        stream = a2a_client.send_message_streaming(request)
        iterator = A2AStreamingIterator(
                if first_chunk:
                    first_chunk = False  # connection succeeded
            return  # stream completed successfully
            # Only retry on first chunk, not mid-stream
            if first_chunk and attempt == 0:
                    is_streaming=True,
            # Only map exception on first chunk
async def create_a2a_client(
) -> "A2AClientType":
    Create an A2A client for the given agent URL.
    This resolves the agent card and returns a ready-to-use A2A client.
    The client can be reused for multiple requests.
        An initialized a2a.client.A2AClient instance
        from litellm.a2a_protocol import create_a2a_client, asend_message
        # Create client once
        client = await create_a2a_client(base_url="http://localhost:10001")
        # Reuse for multiple requests
        response1 = await asend_message(a2a_client=client, request=request1)
        response2 = await asend_message(a2a_client=client, request=request2)
    if not A2A_SDK_AVAILABLE:
            "The 'a2a' package is required for A2A agent invocation. "
            "Install it with: pip install a2a-sdk"
    verbose_logger.info(f"Creating A2A client for {base_url}")
    # Use LiteLLM's cached httpx client
    http_handler = get_async_httpx_client(
        llm_provider=httpxSpecialProvider.A2A,
        params={"timeout": timeout},
    httpx_client = http_handler.client
        httpx_client.headers.update(extra_headers)
        verbose_proxy_logger.debug(f"A2A client created with extra_headers={extra_headers}")
    # Resolve agent card
    resolver = A2ACardResolver(
        httpx_client=httpx_client,
    agent_card = await resolver.get_agent_card()
        f"Resolved agent card: {agent_card.name if hasattr(agent_card, 'name') else 'unknown'}"
    # Create A2A client
    a2a_client = _A2AClient(
    # Store agent_card on client for later retrieval (SDK doesn't expose it)
    a2a_client._litellm_agent_card = agent_card  # type: ignore[attr-defined]
    verbose_logger.info(f"A2A client created for {base_url}")
    return a2a_client
async def aget_agent_card(
    timeout: float = DEFAULT_A2A_AGENT_TIMEOUT,
) -> "AgentCard":
    Fetch the agent card from an A2A agent.
        AgentCard from the A2A agent
    verbose_logger.info(f"Fetching agent card from {base_url}")
        f"Fetched agent card: {agent_card.name if hasattr(agent_card, 'name') else 'unknown'}"
    return agent_card
## Main file for assistants API logic
from typing import Any, Coroutine, Dict, Iterable, List, Literal, Optional, Union
from openai import AsyncOpenAI, OpenAI
from openai.types.beta.assistant import Assistant
from openai.types.beta.assistant_deleted import AssistantDeleted
    exception_type,
from ..llms.azure.assistants import AzureAssistantsAPI
from ..llms.openai.openai import OpenAIAssistantsAPI
from ..types.llms.openai import *
from ..types.router import *
from .utils import get_optional_params_add_message
openai_assistants_api = OpenAIAssistantsAPI()
azure_assistants_api = AzureAssistantsAPI()
### ASSISTANTS ###
async def aget_assistants(
    custom_llm_provider: Literal["openai", "azure"],
    client: Optional[AsyncOpenAI] = None,
) -> AsyncCursorPage[Assistant]:
    ### PASS ARGS TO GET ASSISTANTS ###
    kwargs["aget_assistants"] = True
        func = partial(get_assistants, custom_llm_provider, client, **kwargs)
        _, custom_llm_provider, _, _ = get_llm_provider(  # type: ignore
            model="", custom_llm_provider=custom_llm_provider
def get_assistants(
    client: Optional[Any] = None,
    aget_assistants: Optional[bool] = kwargs.pop("aget_assistants", None)
    if aget_assistants is not None and not isinstance(aget_assistants, bool):
            "Invalid value passed in for aget_assistants. Only bool or None allowed"
    optional_params = GenericLiteLLMParams(
        api_key=api_key, api_base=api_base, api_version=api_version, **kwargs
    timeout = optional_params.timeout or kwargs.get("request_timeout", 600) or 600
        timeout is not None
        and isinstance(timeout, httpx.Timeout)
        and supports_httpx_timeout(custom_llm_provider) is False
        read_timeout = timeout.read or 600
        timeout = read_timeout  # default 10 min timeout
    elif timeout is not None and not isinstance(timeout, httpx.Timeout):
    elif timeout is None:
        timeout = 600.0
    response: Optional[SyncCursorPage[Assistant]] = None
            optional_params.api_base  # for deepinfra/perplexity/anyscale/groq we check in get_llm_provider and pass in the api base from there
            or os.getenv("OPENAI_BASE_URL")
            or os.getenv("OPENAI_API_BASE")
            optional_params.organization
            or os.getenv("OPENAI_ORGANIZATION", None)
            optional_params.api_key
            or os.getenv("OPENAI_API_KEY")
        response = openai_assistants_api.get_assistants(
            max_retries=optional_params.max_retries,
            aget_assistants=aget_assistants,  # type: ignore
            optional_params.api_base or litellm.api_base or get_secret("AZURE_API_BASE")
            optional_params.api_version
        extra_body = optional_params.get("extra_body", {})
            azure_ad_token = extra_body.pop("azure_ad_token", None)
            azure_ad_token = get_secret("AZURE_AD_TOKEN")  # type: ignore
        response = azure_assistants_api.get_assistants(
        raise litellm.exceptions.BadRequestError(
            message="LiteLLM doesn't support {} for 'get_assistants'. Only 'openai' is supported.".format(
            model="n/a",
            response=httpx.Response(
                content="Unsupported provider",
                request=httpx.Request(method="create_thread", url="https://github.com/BerriAI/litellm"),  # type: ignore
async def acreate_assistants(
    kwargs["async_create_assistants"] = True
    model = kwargs.pop("model", None)
        kwargs["client"] = client
        func = partial(create_assistants, custom_llm_provider, model, **kwargs)
def create_assistants(
    tools: Optional[List[Dict[str, Any]]] = None,
    tool_resources: Optional[Dict[str, Any]] = None,
    metadata: Optional[Dict[str, str]] = None,
    response_format: Optional[Union[str, Dict[str, str]]] = None,
) -> Union[Assistant, Coroutine[Any, Any, Assistant]]:
    async_create_assistants: Optional[bool] = kwargs.pop(
        "async_create_assistants", None
    if async_create_assistants is not None and not isinstance(
        async_create_assistants, bool
            "Invalid value passed in for async_create_assistants. Only bool or None allowed"
    create_assistant_data = {
    # only send params that are not None
        k: v for k, v in create_assistant_data.items() if v is not None
    response: Optional[Union[Coroutine[Any, Any, Assistant], Assistant]] = None
        response = openai_assistants_api.create_assistants(
            create_assistant_data=create_assistant_data,
            async_create_assistants=async_create_assistants,  # type: ignore
        if isinstance(client, OpenAI):
            client = None  # only pass client if it's AzureOpenAI
        response = azure_assistants_api.create_assistants(
            async_create_assistants=async_create_assistants,
            message="LiteLLM doesn't support {} for 'create_assistants'. Only 'openai' is supported.".format(
        raise litellm.exceptions.InternalServerError(
            message="No response returned from 'create_assistants'",
async def adelete_assistant(
    kwargs["async_delete_assistants"] = True
        func = partial(delete_assistant, custom_llm_provider, **kwargs)
def delete_assistant(
) -> Union[AssistantDeleted, Coroutine[Any, Any, AssistantDeleted]]:
    async_delete_assistants: Optional[bool] = kwargs.pop(
        "async_delete_assistants", None
    if async_delete_assistants is not None and not isinstance(
        async_delete_assistants, bool
            "Invalid value passed in for async_delete_assistants. Only bool or None allowed"
        Union[AssistantDeleted, Coroutine[Any, Any, AssistantDeleted]]
            optional_params.api_base
            or None
        response = openai_assistants_api.delete_assistant(
            async_delete_assistants=async_delete_assistants,
        response = azure_assistants_api.delete_assistant(
            message="LiteLLM doesn't support {} for 'delete_assistant'. Only 'openai' is supported.".format(
                    method="delete_assistant", url="https://github.com/BerriAI/litellm"
            message="No response returned from 'delete_assistant'",
### THREADS ###
async def acreate_thread(
    custom_llm_provider: Literal["openai", "azure"], **kwargs
    kwargs["acreate_thread"] = True
        func = partial(create_thread, custom_llm_provider, **kwargs)
def create_thread(
    messages: Optional[Iterable[OpenAICreateThreadParamsMessage]] = None,
    tool_resources: Optional[OpenAICreateThreadParamsToolResources] = None,
    client: Optional[OpenAI] = None,
    - get the llm provider
    - if openai - route it there
    - pass through relevant params
    from litellm import create_thread
    create_thread(
        ### OPTIONAL ###
        messages =  {
            "content": "Hello, what is AI?"
            "content": "How does AI work? Explain it in simple terms."
    acreate_thread = kwargs.get("acreate_thread", None)
    response: Optional[Thread] = None
        response = openai_assistants_api.create_thread(
            acreate_thread=acreate_thread,
        api_version: Optional[str] = (
        response = azure_assistants_api.create_thread(
            message="LiteLLM doesn't support {} for 'create_thread'. Only 'openai' is supported.".format(
async def aget_thread(
    kwargs["aget_thread"] = True
        func = partial(get_thread, custom_llm_provider, thread_id, client, **kwargs)
def get_thread(
    """Get the thread object, given a thread_id"""
    aget_thread = kwargs.pop("aget_thread", None)
        response = openai_assistants_api.get_thread(
            aget_thread=aget_thread,
        response = azure_assistants_api.get_thread(
            message="LiteLLM doesn't support {} for 'get_thread'. Only 'openai' is supported.".format(
### MESSAGES ###
async def a_add_message(
    attachments: Optional[List[Attachment]] = None,
) -> OpenAIMessage:
    kwargs["a_add_message"] = True
        func = partial(
            add_message,
            thread_id,
            attachments,
def add_message(
    ### COMMON OBJECTS ###
    a_add_message = kwargs.pop("a_add_message", None)
    _message_data = MessageData(
        role=role, content=content, attachments=attachments, metadata=metadata
    message_data = get_optional_params_add_message(
        role=_message_data["role"],
        content=_message_data["content"],
        attachments=_message_data["attachments"],
        metadata=_message_data["metadata"],
    response: Optional[OpenAIMessage] = None
        response = openai_assistants_api.add_message(
            message_data=message_data,
            a_add_message=a_add_message,
        response = azure_assistants_api.add_message(
async def aget_messages(
) -> AsyncCursorPage[OpenAIMessage]:
    kwargs["aget_messages"] = True
            get_messages,
def get_messages(
) -> SyncCursorPage[OpenAIMessage]:
    aget_messages = kwargs.pop("aget_messages", None)
    response: Optional[SyncCursorPage[OpenAIMessage]] = None
        response = openai_assistants_api.get_messages(
            aget_messages=aget_messages,
        response = azure_assistants_api.get_messages(
            message="LiteLLM doesn't support {} for 'get_messages'. Only 'openai' is supported.".format(
### RUNS ###
def arun_thread_stream(
    event_handler: Optional[AssistantEventHandler] = None,
    kwargs["arun_thread"] = True
    return run_thread(stream=True, event_handler=event_handler, **kwargs)  # type: ignore
async def arun_thread(
    additional_instructions: Optional[str] = None,
    tools: Optional[Iterable[AssistantToolParam]] = None,
            run_thread,
            assistant_id,
            additional_instructions,
def run_thread_stream(
def run_thread(
    event_handler: Optional[AssistantEventHandler] = None,  # for stream=True calls
    """Run a given thread + assistant."""
    arun_thread = kwargs.pop("arun_thread", None)
    response: Optional[Run] = None
        response = openai_assistants_api.run_thread(
            arun_thread=arun_thread,
            event_handler=event_handler,
        azure_ad_token = None
        response = azure_assistants_api.run_thread(
            api_base=str(api_base) if api_base is not None else None,
            api_key=str(api_key) if api_key is not None else None,
            api_version=str(api_version) if api_version is not None else None,
            azure_ad_token=str(azure_ad_token) if azure_ad_token is not None else None,
            message="LiteLLM doesn't support {} for 'run_thread'. Only 'openai' is supported.".format(
from litellm._logging import print_verbose
from litellm.utils import get_optional_params
from ..llms.vllm.completion import handler as vllm_handler
def batch_completion(
    request_timeout: Optional[int] = None,
    timeout: Optional[int] = 600,
    max_workers: Optional[int] = 100,
    Batch litellm.completion function for a given model.
        model (str): The model to use for generating completions.
        messages (List, optional): List of messages to use as input for generating completions. Defaults to [].
        functions (List, optional): List of functions to use as input for generating completions. Defaults to [].
        function_call (str, optional): The function call to use as input for generating completions. Defaults to "".
        temperature (float, optional): The temperature parameter for generating completions. Defaults to None.
        top_p (float, optional): The top-p parameter for generating completions. Defaults to None.
        n (int, optional): The number of completions to generate. Defaults to None.
        stream (bool, optional): Whether to stream completions or not. Defaults to None.
        stop (optional): The stop parameter for generating completions. Defaults to None.
        max_tokens (float, optional): The maximum number of tokens to generate. Defaults to None.
        presence_penalty (float, optional): The presence penalty for generating completions. Defaults to None.
        frequency_penalty (float, optional): The frequency penalty for generating completions. Defaults to None.
        logit_bias (dict, optional): The logit bias for generating completions. Defaults to {}.
        user (str, optional): The user string for generating completions. Defaults to "".
        deployment_id (optional): The deployment ID for generating completions. Defaults to None.
        request_timeout (int, optional): The request timeout for generating completions. Defaults to None.
        max_workers (int,optional): The maximum number of threads to use for parallel processing.
        list: A list of completion results.
    batch_messages = messages
    completions = []
    model = model
    if model.split("/", 1)[0] in litellm.provider_list:
        custom_llm_provider = model.split("/", 1)[0]
        model = model.split("/", 1)[1]
    if custom_llm_provider == "vllm":
        results = vllm_handler.batch_completions(
            messages=batch_messages,
    # all non VLLM models for batch completion models
        def chunks(lst, n):
            """Yield successive n-sized chunks from lst."""
            for i in range(0, len(lst), n):
                yield lst[i : i + n]
        with ThreadPoolExecutor(max_workers=max_workers) as executor:
            for sub_batch in chunks(batch_messages, 100):
                for message_list in sub_batch:
                    kwargs_modified = args.copy()
                    kwargs_modified.pop("max_workers")
                    kwargs_modified["messages"] = message_list
                    original_kwargs = {}
                    if "kwargs" in kwargs_modified:
                        original_kwargs = kwargs_modified.pop("kwargs")
                    future = executor.submit(
                        litellm.completion, **kwargs_modified, **original_kwargs
                    completions.append(future)
        # Retrieve the results from the futures
        # results = [future.result() for future in completions]
        # return exceptions if any
        for future in completions:
                results.append(exc)
# send one request to multiple models
# return as soon as one of the llms responds
def batch_completion_models(*args, **kwargs):
    Send a request to multiple language models concurrently and return the response
    as soon as one of the models responds.
        *args: Variable-length positional arguments passed to the completion function.
        **kwargs: Additional keyword arguments:
            - models (str or list of str): The language models to send requests to.
            - Other keyword arguments to be passed to the completion function.
        str or None: The response from one of the language models, or None if no response is received.
        This function utilizes a ThreadPoolExecutor to parallelize requests to multiple models.
        It sends requests concurrently and returns the response from the first model that responds.
    if "model" in kwargs:
        kwargs.pop("model")
    if "models" in kwargs:
        models = kwargs["models"]
        kwargs.pop("models")
        futures = {}
        with ThreadPoolExecutor(max_workers=len(models)) as executor:
                futures[model] = executor.submit(
                    litellm.completion, *args, model=model, **kwargs
            for model, future in sorted(
                futures.items(), key=lambda x: models.index(x[0])
                if future.result() is not None:
                    return future.result()
    elif "deployments" in kwargs:
        deployments = kwargs["deployments"]
        kwargs.pop("deployments")
        kwargs.pop("model_list")
        nested_kwargs = kwargs.pop("kwargs", {})
        with ThreadPoolExecutor(max_workers=len(deployments)) as executor:
            for deployment in deployments:
                        key not in deployment
                    ):  # don't override deployment values e.g. model name, api base, etc.
                        deployment[key] = kwargs[key]
                kwargs = {**deployment, **nested_kwargs}
                futures[deployment["model"]] = executor.submit(
                    litellm.completion, **kwargs
                # wait for the first returned future
                print_verbose("\n\n waiting for next result\n\n")
                done, _ = wait(futures.values(), return_when=FIRST_COMPLETED)
                print_verbose(f"done list\n{done}")
                for future in done:
                        result = future.result()
                        # if model 1 fails, continue with response from model 2, model3
                            "\n\ngot an exception, ignoring, removing from futures"
                        print_verbose(futures)
                        new_futures = {}
                        for key, value in futures.items():
                            if future == value:
                                print_verbose(f"removing key{key}")
                                new_futures[key] = value
                        futures = new_futures
                        print_verbose(f"new futures{futures}")
                print_verbose("\n\ndone looping through futures\n\n")
    return None  # If no response is received from any model
def batch_completion_models_all_responses(*args, **kwargs):
    Send a request to multiple language models concurrently and return a list of responses
    from all models that respond.
        list: A list of responses from the language models that responded.
        It sends requests concurrently and collects responses from all models that respond.
    # ANSI escape codes for colored output
        raise Exception("'models' param not in kwargs")
    with concurrent.futures.ThreadPoolExecutor(max_workers=len(models)) as executor:
        for idx, model in enumerate(models):
            future = executor.submit(litellm.completion, *args, model=model, **kwargs)
                responses.append(future.result())
    return responses
Main File for Batches API implementation
https://platform.openai.com/docs/api-reference/batch
- create_batch()
- retrieve_batch()
- cancel_batch()
- list_batch()
from typing import Any, Coroutine, Dict, Literal, Optional, Union, cast
from openai.types.batch import BatchRequestCounts
from litellm.llms.anthropic.batches.handler import AnthropicBatchesHandler
from litellm.llms.azure.batches.handler import AzureBatchesAPI
from litellm.llms.bedrock.batches.handler import BedrockBatchesHandler
from litellm.llms.custom_httpx.llm_http_handler import BaseLLMHTTPHandler
from litellm.llms.openai.openai import OpenAIBatchesAPI
from litellm.llms.vertex_ai.batches.handler import VertexAIBatchPrediction
from litellm.secret_managers.main import get_secret_str
    CancelBatchRequest,
    CreateBatchRequest,
    RetrieveBatchRequest,
    OPENAI_COMPATIBLE_BATCH_AND_FILES_PROVIDERS,
    LiteLLMBatch,
openai_batches_instance = OpenAIBatchesAPI()
azure_batches_instance = AzureBatchesAPI()
vertex_ai_batches_instance = VertexAIBatchPrediction(gcs_bucket_name="")
anthropic_batches_instance = AnthropicBatchesHandler()
#################################################
def _resolve_timeout(
    optional_params: GenericLiteLLMParams,
    default_timeout: float = 600.0,
    Resolve timeout value from various sources and handle httpx.Timeout objects.
        optional_params: GenericLiteLLMParams object containing timeout
        kwargs: Additional kwargs that may contain request_timeout
        custom_llm_provider: Provider name for httpx timeout support check
        default_timeout: Default timeout value to use
        Resolved timeout as float
        optional_params.timeout
        or kwargs.get("request_timeout", default_timeout)
        or default_timeout
    # Handle httpx.Timeout objects
    if isinstance(timeout, httpx.Timeout):
        if supports_httpx_timeout(custom_llm_provider) is False:
            # Extract read timeout for providers that don't support httpx.Timeout
            read_timeout = timeout.read or default_timeout
            return float(read_timeout)
            # For providers that support httpx.Timeout, we still need to return a float
            # This case might need to be handled differently based on the actual use case
            return float(timeout.read or default_timeout)
    # Handle None case
        return float(default_timeout)
    # Handle numeric values (int, float, string representations)
    return float(timeout)
async def acreate_batch(
    endpoint: Literal["/v1/chat/completions", "/v1/embeddings", "/v1/completions"],
    custom_llm_provider: Literal["openai", "azure", "vertex_ai", "bedrock", "hosted_vllm"] = "openai",
    extra_body: Optional[Dict[str, str]] = None,
) -> LiteLLMBatch:
    Async: Creates and executes a batch from an uploaded file of request
    LiteLLM Equivalent of POST: https://api.openai.com/v1/batches
        kwargs["acreate_batch"] = True
            create_batch,
            completion_window,
            input_file_id,
            extra_headers,
            extra_body,
def create_batch(
) -> Union[LiteLLMBatch, Coroutine[Any, Any, LiteLLMBatch]]:
    Creates and executes a batch from an uploaded file of request
        model: Optional[str] = kwargs.get("model", None)
                model, _, _, _ = get_llm_provider(
                f"litellm.batches.main.py::create_batch() - Error inferring custom_llm_provider - {str(e)}"
        _is_async = kwargs.pop("acreate_batch", False) is True
        litellm_params = dict(GenericLiteLLMParams(**kwargs))
        litellm_logging_obj: LiteLLMLoggingObj = cast(
            LiteLLMLoggingObj, kwargs.get("litellm_logging_obj", None)
        timeout = _resolve_timeout(optional_params, kwargs, custom_llm_provider)
            optional_params=optional_params.model_dump(),
                **optional_params.model_dump(exclude_unset=True),
        _create_batch_request = CreateBatchRequest(
            completion_window=completion_window,
            endpoint=endpoint,
            input_file_id=input_file_id,
            provider_config = ProviderConfigManager.get_provider_batches_config(
            provider_config = None
            response = base_llm_http_handler.create_batch(
                create_batch_data=_create_batch_request,
                headers=extra_headers or {},
                _is_async=_is_async,
                    and isinstance(client, (HTTPHandler, AsyncHTTPHandler))
        if custom_llm_provider in OPENAI_COMPATIBLE_BATCH_AND_FILES_PROVIDERS:
            # for deepinfra/perplexity/anyscale/groq we check in get_llm_provider and pass in the api base from there
            response = openai_batches_instance.create_batch(
                or get_secret_str("AZURE_API_BASE")
                extra_body.pop("azure_ad_token", None)
                get_secret_str("AZURE_AD_TOKEN")  # type: ignore
            response = azure_batches_instance.create_batch(
            api_base = optional_params.api_base or ""
                optional_params.vertex_project
                optional_params.vertex_location
            vertex_credentials = optional_params.vertex_credentials or get_secret_str(
                "VERTEXAI_CREDENTIALS"
            response = vertex_ai_batches_instance.create_batch(
                message="LiteLLM doesn't support custom_llm_provider={} for 'create_batch'".format(
                    request=httpx.Request(method="create_batch", url="https://github.com/BerriAI/litellm"),  # type: ignore
async def aretrieve_batch(
    custom_llm_provider: Literal["openai", "azure", "vertex_ai", "bedrock", "hosted_vllm", "anthropic"] = "openai",
    Async: Retrieves a batch.
    LiteLLM Equivalent of GET https://api.openai.com/v1/batches/{batch_id}
        kwargs["aretrieve_batch"] = True
            retrieve_batch,
def _handle_retrieve_batch_providers_without_provider_config(
    timeout: Union[float, httpx.Timeout],
    _retrieve_batch_request: RetrieveBatchRequest,
    _is_async: bool,
    logging_obj: Optional[Any] = None,
        response = openai_batches_instance.retrieve_batch(
            retrieve_batch_data=_retrieve_batch_request,
        response = azure_batches_instance.retrieve_batch(
        response = vertex_ai_batches_instance.retrieve_batch(
            or get_secret_str("ANTHROPIC_API_BASE")
            or get_secret_str("ANTHROPIC_API_KEY")
        response = anthropic_batches_instance.retrieve_batch(
            message="LiteLLM doesn't support {} for 'create_batch'. Only 'openai' is supported.".format(
def retrieve_batch(
        _retrieve_batch_request = RetrieveBatchRequest(
        _is_async = kwargs.pop("aretrieve_batch", False) is True
        # Check if this is an async invoke ARN (different from regular batch ARN)
        # Async invoke ARNs have format: arn:aws(-[^:]+)?:bedrock:[a-z0-9-]{1,20}:[0-9]{12}:async-invoke/[a-z0-9]{12}
            batch_id.startswith("arn:aws")
            and ":bedrock:" in batch_id
            and ":async-invoke/" in batch_id
            # Handle async invoke status check
            # Remove aws_region_name from kwargs to avoid duplicate parameter
            async_kwargs = kwargs.copy()
            async_kwargs.pop("aws_region_name", None)
            return BedrockBatchesHandler._handle_async_invoke_status(
                aws_region_name=kwargs.get("aws_region_name", "us-east-1"),
                **async_kwargs,
        # Try to use provider config first (for providers like bedrock)
            response = base_llm_http_handler.retrieve_batch(
                logging_obj=litellm_logging_obj
                or LiteLLMLoggingObj(
                    model=model or f"{custom_llm_provider}/unknown",
                    call_type="batch_retrieve",
                    litellm_call_id="batch_retrieve_" + batch_id,
                    function_id="batch_retrieve",
        # Handle providers without provider config
        return _handle_retrieve_batch_providers_without_provider_config(
            _retrieve_batch_request=_retrieve_batch_request,
async def alist_batches(
    after: Optional[str] = None,
    limit: Optional[int] = None,
    custom_llm_provider: Literal["openai", "azure", "hosted_vllm", "vertex_ai"] = "openai",
    Async: List your organization's batches.
        kwargs["alist_batches"] = True
            list_batches,
def list_batches(
    Lists batches
    List your organization's batches.
        _is_async = kwargs.pop("alist_batches", False) is True
            response = openai_batches_instance.list_batches(
                after=after,
                limit=limit,
            api_base = optional_params.api_base or litellm.api_base or get_secret_str("AZURE_API_BASE")  # type: ignore
            response = azure_batches_instance.list_batches(
            response = vertex_ai_batches_instance.list_batches(
                message="LiteLLM doesn't support {} for 'list_batch'. Supported providers: openai, azure, vertex_ai.".format(
async def acancel_batch(
    custom_llm_provider: Literal["openai", "azure"] = "openai",
    Async: Cancels a batch.
    LiteLLM Equivalent of POST https://api.openai.com/v1/batches/{batch_id}/cancel
        kwargs["acancel_batch"] = True
        # Preserve model parameter - only pop from kwargs if it exists there
        # (to avoid passing it twice), otherwise keep the function parameter value
        model = kwargs.pop("model", None) or model
            cancel_batch,
def cancel_batch(
    custom_llm_provider: Union[Literal["openai", "azure"], str] = "openai",
    Cancels a batch.
                f"litellm.batches.main.py::cancel_batch() - Error inferring custom_llm_provider - {str(e)}"
        _cancel_batch_request = CancelBatchRequest(
        _is_async = kwargs.pop("acancel_batch", False) is True
            response = openai_batches_instance.cancel_batch(
                cancel_batch_data=_cancel_batch_request,
            response = azure_batches_instance.cancel_batch(
                message="LiteLLM doesn't support {} for 'cancel_batch'. Only 'openai' and 'azure' are supported.".format(
                    request=httpx.Request(method="cancel_batch", url="https://github.com/BerriAI/litellm"),  # type: ignore
def _handle_async_invoke_status(
    batch_id: str, aws_region_name: str, logging_obj=None, **kwargs
) -> "LiteLLMBatch":
    Handle async invoke status check for AWS Bedrock.
        batch_id: The async invoke ARN
        aws_region_name: AWS region name
        **kwargs: Additional parameters
        dict: Status information including status, output_file_id (S3 URL), etc.
    from litellm.llms.bedrock.embed.embedding import BedrockEmbedding
    async def _async_get_status():
        # Create embedding handler instance
        embedding_handler = BedrockEmbedding()
        # Get the status of the async invoke job
        status_response = await embedding_handler._get_async_invoke_status(
            invocation_arn=batch_id,
            aws_region_name=aws_region_name,
        # Transform response to a LiteLLMBatch object
        from litellm.types.llms.openai import BatchJobStatus
        from litellm.types.utils import LiteLLMBatch
        # Normalize status to lowercase (AWS returns 'Completed', 'Failed', etc.)
        aws_status_raw = status_response.get("status", "")
        aws_status_lower = aws_status_raw.lower()
        # Map AWS status values to LiteLLM expected values
        status_mapping: dict[str, BatchJobStatus] = {
            "completed": "completed",
            "failed": "failed",
            "inprogress": "in_progress",
            "in_progress": "in_progress",
        normalized_status: BatchJobStatus = status_mapping.get(aws_status_lower, "failed")  # Default to "failed" if unknown status
        # Get output S3 URI safely
        output_s3_uri = ""
            output_s3_uri = status_response["outputDataConfig"]["s3OutputDataConfig"]["s3Uri"]
        # Use BedrockBatchesConfig's timestamp parsing method (expects raw AWS status string)
        created_at, in_progress_at, completed_at, failed_at, _, _ = BedrockBatchesConfig()._parse_timestamps_and_status(status_response, aws_status_raw)
        result = LiteLLMBatch(
            id=status_response["invocationArn"],
            object="batch",
            status=normalized_status,
            created_at=created_at or int(time.time()),  # Provide default timestamp if None
            in_progress_at=in_progress_at,
            completed_at=completed_at,
            failed_at=failed_at,
            request_counts=BatchRequestCounts(
                total=1,
                completed=1 if normalized_status == "completed" else 0,
                failed=1 if normalized_status == "failed" else 0,
            metadata=dict(
                    "output_file_id": output_s3_uri,
                    "failure_message": status_response.get("failureMessage") or "",
                    "model_arn": status_response["modelArn"],
            endpoint="/v1/embeddings",
            input_file_id="",
    # Since this function is called from within an async context via run_in_executor,
    # we need to create a new event loop in a thread to avoid conflicts
    def run_in_thread():
        new_loop = asyncio.new_event_loop()
        asyncio.set_event_loop(new_loop)
            return new_loop.run_until_complete(_async_get_status())
            new_loop.close()
        future = executor.submit(run_in_thread)
from typing import Any, Coroutine, Dict, List, Literal, Optional, Union, overload
from litellm.constants import request_timeout as DEFAULT_REQUEST_TIMEOUT
from litellm.containers.utils import ContainerRequestUtils
from litellm.main import base_llm_http_handler
from litellm.types.containers.main import (
    ContainerCreateOptionalRequestParams,
    ContainerFileListResponse,
    ContainerFileObject,
    ContainerListOptionalRequestParams,
    ContainerObject,
    DeleteContainerResult,
from litellm.types.llms.openai import FileTypes
from litellm.types.utils import CallTypes
from litellm.utils import ProviderConfigManager, client
    "aupload_container_file",
    "upload_container_file",
##### Container Create #######################
async def acreate_container(
    expires_after: Optional[Dict[str, Any]] = None,
    file_ids: Optional[List[str]] = None,
    # LiteLLM specific params,
    custom_llm_provider: Literal["openai"] = "openai",
    extra_headers: Optional[Dict[str, Any]] = None,
    extra_query: Optional[Dict[str, Any]] = None,
    extra_body: Optional[Dict[str, Any]] = None,
) -> ContainerObject:
    """Asynchronously calls the `create_container` function with the given arguments and keyword arguments.
    - `name` (str): Name of the container to create
    - `expires_after` (Optional[Dict[str, Any]]): Container expiration time settings
    - `file_ids` (Optional[List[str]]): IDs of files to copy to the container
    - `timeout` (int): Request timeout in seconds
    - `custom_llm_provider` (Optional[Literal["openai"]]): The LLM provider to use
    - `extra_headers` (Optional[Dict[str, Any]]): Additional headers
    - `extra_query` (Optional[Dict[str, Any]]): Additional query parameters
    - `extra_body` (Optional[Dict[str, Any]]): Additional body parameters
    - `kwargs` (dict): Additional keyword arguments
    - `response` (ContainerObject): The created container object
    local_vars = locals()
        kwargs["async_call"] = True
            expires_after=expires_after,
        raise litellm.exception_type(
            completion_kwargs=local_vars,
# Overload for when acreate_container=True (returns Coroutine)
def create_container(
    acreate_container: Literal[True],
) -> Coroutine[Any, Any, ContainerObject]:
    acreate_container: Literal[False] = False,
    Coroutine[Any, Any, ContainerObject],
    """Create a container using the OpenAI Container API.
    Currently supports OpenAI
    response = litellm.create_container(
        name="My Container",
        litellm_logging_obj: LiteLLMLoggingObj = kwargs.pop("litellm_logging_obj")  # type: ignore
        litellm_call_id: Optional[str] = kwargs.get("litellm_call_id")
        _is_async = kwargs.pop("async_call", False) is True
        # Check for mock response first
            if isinstance(mock_response, str):
                mock_response = json.loads(mock_response)
            response = ContainerObject(**mock_response)
        # get llm provider logic
        # Pass credential params explicitly since they're named args, not in kwargs
        litellm_params = GenericLiteLLMParams(
        # get provider config
        container_provider_config: Optional[BaseContainerConfig] = (
            ProviderConfigManager.get_provider_container_config(
        if container_provider_config is None:
            raise ValueError(f"container operations are not supported for {custom_llm_provider}")
        local_vars.update(kwargs)
        # Get ContainerCreateOptionalRequestParams with only valid parameters
        container_create_optional_params: ContainerCreateOptionalRequestParams = (
            ContainerRequestUtils.get_requested_container_create_optional_param(local_vars)
        # Get optional parameters for the container API
        container_create_request_params: Dict = (
            ContainerRequestUtils.get_optional_params_container_create(
                container_provider_config=container_provider_config,
                container_create_optional_params=container_create_optional_params,
        # Pre Call logging
            optional_params=dict(container_create_request_params),
                **container_create_request_params,
        # Set the correct call type for container creation
        litellm_logging_obj.call_type = CallTypes.create_container.value
        return base_llm_http_handler.container_create_handler(
            container_create_request_params=container_create_request_params,
            timeout=timeout or DEFAULT_REQUEST_TIMEOUT,
##### Container List #######################
async def alist_containers(
    order: Optional[str] = None,
) -> ContainerListResponse:
    """Asynchronously list containers.
    - `after` (Optional[str]): A cursor for pagination
    - `limit` (Optional[int]): Number of items to return (1-100, default 20)
    - `order` (Optional[str]): Sort order ('asc' or 'desc', default 'desc')
    - `custom_llm_provider` (Literal["openai"]): The LLM provider to use
    - `response` (ContainerListResponse): The list of containers
            order=order,
def list_containers(
    alist_containers: Literal[True],
) -> Coroutine[Any, Any, ContainerListResponse]:
    alist_containers: Literal[False] = False,
    Coroutine[Any, Any, ContainerListResponse],
    """List containers using the OpenAI Container API.
            response = ContainerListResponse(**mock_response)
            raise ValueError(f"Container provider config not found for provider: {custom_llm_provider}")
        # Get container list request parameters
        container_list_optional_params: ContainerListOptionalRequestParams = (
            ContainerRequestUtils.get_requested_container_list_optional_param(local_vars)
            optional_params=dict(container_list_optional_params),
                **container_list_optional_params,
        # Set the correct call type
        litellm_logging_obj.call_type = CallTypes.list_containers.value
        return base_llm_http_handler.container_list_handler(
##### Container Retrieve #######################
async def aretrieve_container(
    """Asynchronously retrieve a container.
    - `container_id` (str): The ID of the container to retrieve
    - `response` (ContainerObject): The container object
            container_id=container_id,
def retrieve_container(
    aretrieve_container: Literal[True],
    aretrieve_container: Literal[False] = False,
    """Retrieve a container using the OpenAI Container API.
        litellm_logging_obj.call_type = CallTypes.retrieve_container.value
        return base_llm_http_handler.container_retrieve_handler(
##### Container Delete #######################
async def adelete_container(
) -> DeleteContainerResult:
    """Asynchronously delete a container.
    - `container_id` (str): The ID of the container to delete
    - `response` (DeleteContainerResult): The deletion result
def delete_container(
    adelete_container: Literal[True],
) -> Coroutine[Any, Any, DeleteContainerResult]:
    adelete_container: Literal[False] = False,
    Coroutine[Any, Any, DeleteContainerResult],
    """Delete a container using the OpenAI Container API.
            response = DeleteContainerResult(**mock_response)
        litellm_logging_obj.call_type = CallTypes.delete_container.value
        return base_llm_http_handler.container_delete_handler(
##### Container Files List #######################
async def alist_container_files(
) -> ContainerFileListResponse:
    """Asynchronously list files in a container.
    - `container_id` (str): The ID of the container
    - `response` (ContainerFileListResponse): The list of container files
def list_container_files(
    timeout=600,
    alist_container_files: Literal[True],
) -> Coroutine[Any, Any, ContainerFileListResponse]:
    alist_container_files: Literal[False] = False,
    Coroutine[Any, Any, ContainerFileListResponse],
    """List files in a container using the OpenAI Container API.
            response = ContainerFileListResponse(**mock_response)
            optional_params={"container_id": container_id, "after": after, "limit": limit, "order": order},
        litellm_logging_obj.call_type = CallTypes.list_container_files.value
        return base_llm_http_handler.container_file_list_handler(
##### Container File Upload #######################
async def aupload_container_file(
) -> ContainerFileObject:
    """Asynchronously upload a file to a container.
    This endpoint allows uploading files directly to a container session,
    supporting various file types like CSV, Excel, Python scripts, etc.
    - `container_id` (str): The ID of the container to upload the file to
    - `file` (FileTypes): The file to upload. Can be:
        - A tuple of (filename, content, content_type)
        - A tuple of (filename, content)
        - A file-like object with read() method
        - Bytes
        - A string path to a file
    - `response` (ContainerFileObject): The uploaded file object
    # Upload a CSV file
    response = await litellm.aupload_container_file(
        container_id="container_abc123",
        file=("data.csv", open("data.csv", "rb").read(), "text/csv"),
            upload_container_file,
def upload_container_file(
    aupload_container_file: Literal[True],
) -> Coroutine[Any, Any, ContainerFileObject]:
    aupload_container_file: Literal[False] = False,
    Coroutine[Any, Any, ContainerFileObject],
    """Upload a file to a container using the OpenAI Container API.
    supporting various file types like CSV, Excel, Python scripts, JSON, etc.
    This is useful when /chat/completions or /responses sends files to the
    container but the input file type is limited to PDF. This endpoint lets
    you work with other file types.
    response = litellm.upload_container_file(
    # Upload a Python script
        file=("script.py", b"print('hello world')", "text/x-python"),
    from litellm.llms.custom_httpx.container_handler import generic_container_handler
            response = ContainerFileObject(**mock_response)
            optional_params={"container_id": container_id},
        litellm_logging_obj.call_type = CallTypes.upload_container_file.value
        return generic_container_handler.handle(
            endpoint_name="upload_container_file",
Main File for Files API implementation
https://platform.openai.com/docs/api-reference/files
import uuid as uuid_module
from litellm import get_secret_str
from litellm.litellm_core_utils.get_llm_provider_logic import get_llm_provider
from litellm.llms.anthropic.files.handler import AnthropicFilesHandler
from litellm.llms.azure.files.handler import AzureOpenAIFilesAPI
from litellm.llms.bedrock.files.handler import BedrockFilesHandler
from litellm.llms.openai.openai import FileDeleted, FileObject, OpenAIFilesAPI
from litellm.llms.vertex_ai.files.handler import VertexAIFilesHandler
    CreateFileRequest,
    FileContentRequest,
    FileExpiresAfter,
    OpenAIFileObject,
from litellm.types.router import *
openai_files_instance = OpenAIFilesAPI()
azure_files_instance = AzureOpenAIFilesAPI()
vertex_ai_files_instance = VertexAIFilesHandler()
bedrock_files_instance = BedrockFilesHandler()
anthropic_files_instance = AnthropicFilesHandler()
async def acreate_file(
    purpose: Literal["assistants", "batch", "fine-tune"],
    expires_after: Optional[FileExpiresAfter] = None,
    custom_llm_provider: Literal["openai", "azure", "gemini", "vertex_ai", "bedrock", "hosted_vllm", "manus"] = "openai",
) -> OpenAIFileObject:
    Async: Files are used to upload documents that can be used with features like Assistants, Fine-tuning, and Batch API.
    LiteLLM Equivalent of POST: POST https://api.openai.com/v1/files
        kwargs["acreate_file"] = True
        call_args = {
            "extra_body": extra_body,
        func = partial(create_file, **call_args)
def create_file(
    custom_llm_provider: Optional[Literal["openai", "azure", "gemini", "vertex_ai", "bedrock", "hosted_vllm", "manus"]] = None,
) -> Union[OpenAIFileObject, Coroutine[Any, Any, OpenAIFileObject]]:
    Files are used to upload documents that can be used with features like Assistants, Fine-tuning, and Batch API.
    Specify either provider_list or custom_llm_provider.
        _is_async = kwargs.pop("acreate_file", False) is True
        litellm_params_dict = dict(**kwargs)
        logging_obj = cast(
            Optional[LiteLLMLoggingObj], kwargs.get("litellm_logging_obj")
            raise ValueError("logging_obj is required")
        client = kwargs.get("client")
            and supports_httpx_timeout(cast(str, custom_llm_provider)) is False
        if expires_after is not None:
            _create_file_request = CreateFileRequest(
        provider_config = ProviderConfigManager.get_provider_files_config(
            response = base_llm_http_handler.create_file(
                create_file_data=_create_file_request,
        elif custom_llm_provider in OPENAI_COMPATIBLE_BATCH_AND_FILES_PROVIDERS:
            response = openai_files_instance.create_file(
            response = azure_files_instance.create_file(
            response = vertex_ai_files_instance.create_file(
                message="LiteLLM doesn't support {} for 'create_file'. Only ['openai', 'azure', 'vertex_ai', 'manus'] are supported.".format(
                    request=httpx.Request(method="create_file", url="https://github.com/BerriAI/litellm"),  # type: ignore
async def afile_retrieve(
    custom_llm_provider: Literal["openai", "azure", "gemini", "hosted_vllm", "manus"] = "openai",
    Async: Get file contents
    LiteLLM Equivalent of GET https://api.openai.com/v1/files
        kwargs["is_async"] = True
            file_retrieve,
        return OpenAIFileObject(**response.model_dump())
def file_retrieve(
    custom_llm_provider: Literal["openai", "azure", "hosted_vllm", "manus"] = "openai",
        _is_async = kwargs.pop("is_async", False) is True
            response = openai_files_instance.retrieve_file(
            response = azure_files_instance.retrieve_file(
            # Try using provider config pattern (for Manus, Bedrock, etc.)
                litellm_params_dict["api_key"] = optional_params.api_key
                litellm_params_dict["api_base"] = optional_params.api_base
                logging_obj = kwargs.get("litellm_logging_obj")
                        Logging as LiteLLMLoggingObj,
                    logging_obj = LiteLLMLoggingObj(
                        call_type="afile_retrieve" if _is_async else "file_retrieve",
                        start_time=time.time(),
                        litellm_call_id=kwargs.get("litellm_call_id", str(uuid_module.uuid4())),
                        function_id=str(kwargs.get("id") or ""),
                response = base_llm_http_handler.retrieve_file(
                    message="LiteLLM doesn't support {} for 'file_retrieve'. Only 'openai', 'azure', and 'manus' are supported.".format(
        return cast(FileObject, response)
# Delete file
async def afile_delete(
    custom_llm_provider: Literal["openai", "azure", "gemini", "manus"] = "openai",
) -> Coroutine[Any, Any, FileObject]:
    Async: Delete file
    LiteLLM Equivalent of DELETE https://api.openai.com/v1/files
            file_delete,
        return cast(FileDeleted, response)  # type: ignore
def file_delete(
    custom_llm_provider: Union[Literal["openai", "azure", "gemini", "manus"], str] = "openai",
    Delete file
            response = openai_files_instance.delete_file(
            response = azure_files_instance.delete_file(
                        call_type="afile_delete" if _is_async else "file_delete",
                response = base_llm_http_handler.delete_file(
                    message="LiteLLM doesn't support {} for 'file_delete'. Only 'openai', 'azure', 'gemini', and 'manus' are supported.".format(
        return cast(FileDeleted, response)
# List files
async def afile_list(
    custom_llm_provider: Literal["openai", "azure", "manus"] = "openai",
    purpose: Optional[str] = None,
    Async: List files
            file_list,
def file_list(
    List files
        # Check if provider has a custom files config (e.g., Manus, Bedrock, Vertex AI)
                    call_type="afile_list" if _is_async else "file_list",
                    function_id=str(kwargs.get("id", "")),
            response = base_llm_http_handler.list_files(
            response = openai_files_instance.list_files(
            response = azure_files_instance.list_files(
                message="LiteLLM doesn't support {} for 'file_list'. Only 'openai', 'azure', and 'manus' are supported.".format(
                    request=httpx.Request(method="file_list", url="https://github.com/BerriAI/litellm"),  # type: ignore
async def afile_content(
    custom_llm_provider: Literal["openai", "azure", "vertex_ai", "bedrock", "hosted_vllm", "anthropic", "manus"] = "openai",
) -> HttpxBinaryResponseContent:
        kwargs["afile_content"] = True
            file_content,
def file_content(
    custom_llm_provider: Optional[
        Union[Literal["openai", "azure", "vertex_ai", "bedrock", "hosted_vllm", "anthropic", "manus"], str]
        _file_content_request = FileContentRequest(
        _is_async = kwargs.pop("afile_content", False) is True
        # Check if this is an Anthropic batch results request
            response = anthropic_files_instance.file_content(
                file_content_request=_file_content_request,
            response = openai_files_instance.file_content(
            response = azure_files_instance.file_content(
            response = vertex_ai_files_instance.file_content(
            response = bedrock_files_instance.file_content(
                optional_params=litellm_params_dict,
                message="LiteLLM doesn't support {} for 'file_content'. Supported providers are 'openai', 'azure', 'vertex_ai', 'bedrock', 'manus'.".format(
Main File for Fine Tuning API implementation
https://platform.openai.com/docs/api-reference/fine-tuning
- fine_tuning.jobs.create()
- fine_tuning.jobs.list()
- client.fine_tuning.jobs.list_events()
from typing import Any, Coroutine, Dict, Literal, Optional, Union
from litellm.llms.azure.fine_tuning.handler import AzureOpenAIFineTuningAPI
from litellm.llms.openai.fine_tuning.handler import OpenAIFineTuningAPI
from litellm.llms.vertex_ai.fine_tuning.handler import VertexFineTuningAPI
from litellm.types.llms.openai import FineTuningJobCreate, Hyperparameters
from litellm.types.utils import LiteLLMFineTuningJob
from litellm.utils import client, supports_httpx_timeout
openai_fine_tuning_apis_instance = OpenAIFineTuningAPI()
azure_fine_tuning_apis_instance = AzureOpenAIFineTuningAPI()
vertex_fine_tuning_apis_instance = VertexFineTuningAPI()
async def acreate_fine_tuning_job(
    hyperparameters: Optional[dict] = {},
    suffix: Optional[str] = None,
    validation_file: Optional[str] = None,
    integrations: Optional[List[str]] = None,
    custom_llm_provider: Literal["openai", "azure", "vertex_ai"] = "openai",
) -> LiteLLMFineTuningJob:
        "inside acreate_fine_tuning_job model=%s and kwargs=%s", model, kwargs
        kwargs["acreate_fine_tuning_job"] = True
            create_fine_tuning_job,
            training_file,
            hyperparameters,
            validation_file,
            integrations,
def create_fine_tuning_job(
) -> Union[LiteLLMFineTuningJob, Coroutine[Any, Any, LiteLLMFineTuningJob]]:
    Creates a fine-tuning job which begins the process of creating a new model from a given dataset.
    Response includes details of the enqueued job including job status and the name of the fine-tuned models once complete
        _is_async = kwargs.pop("acreate_fine_tuning_job", False) is True
        # handle hyperparameters
        hyperparameters = hyperparameters or {}  # original hyperparameters
        _oai_hyperparameters: Hyperparameters = Hyperparameters(
            **hyperparameters
        )  # Typed Hyperparameters for OpenAI Spec
        # OpenAI
            create_fine_tuning_job_data = FineTuningJobCreate(
                training_file=training_file,
                hyperparameters=_oai_hyperparameters,
                validation_file=validation_file,
                integrations=integrations,
            create_fine_tuning_job_data_dict = create_fine_tuning_job_data.model_dump(
                exclude_none=True
            response = openai_fine_tuning_apis_instance.create_fine_tuning_job(
                api_version=optional_params.api_version,
                create_fine_tuning_job_data=create_fine_tuning_job_data_dict,
                client=kwargs.get(
                    "client", None
                ),  # note, when we add this to `GenericLiteLLMParams` it impacts a lot of other tests + linting
        # Azure OpenAI
            response = azure_fine_tuning_apis_instance.create_fine_tuning_job(
                organization=optional_params.organization,
            response = vertex_fine_tuning_apis_instance.create_fine_tuning_job(
                create_fine_tuning_job_data=create_fine_tuning_job_data,
                original_hyperparameters=hyperparameters,
        verbose_logger.error("got exception in create_fine_tuning_job=%s", str(e))
async def acancel_fine_tuning_job(
    Async: Immediately cancel a fine-tune job.
        kwargs["acancel_fine_tuning_job"] = True
            cancel_fine_tuning_job,
            fine_tuning_job_id,
def cancel_fine_tuning_job(
        _is_async = kwargs.pop("acancel_fine_tuning_job", False) is True
            response = openai_fine_tuning_apis_instance.cancel_fine_tuning_job(
                fine_tuning_job_id=fine_tuning_job_id,
                client=kwargs.get("client", None),
            api_base = optional_params.api_base or litellm.api_base or get_secret("AZURE_API_BASE")  # type: ignore
            response = azure_fine_tuning_apis_instance.cancel_fine_tuning_job(
async def alist_fine_tuning_jobs(
    Async: List your organization's fine-tuning jobs
        kwargs["alist_fine_tuning_jobs"] = True
            list_fine_tuning_jobs,
def list_fine_tuning_jobs(
    Params:
    - after: Optional[str] = None, Identifier for the last job from the previous pagination request.
    - limit: Optional[int] = None, Number of fine-tuning jobs to retrieve. Defaults to 20
        _is_async = kwargs.pop("alist_fine_tuning_jobs", False) is True
            response = openai_fine_tuning_apis_instance.list_fine_tuning_jobs(
                get_secret("AZURE_AD_TOKEN")  # type: ignore
            response = azure_fine_tuning_apis_instance.list_fine_tuning_jobs(
async def aretrieve_fine_tuning_job(
    Async: Get info about a fine-tuning job.
        kwargs["aretrieve_fine_tuning_job"] = True
            retrieve_fine_tuning_job,
def retrieve_fine_tuning_job(
        _is_async = kwargs.pop("aretrieve_fine_tuning_job", False) is True
            response = openai_fine_tuning_apis_instance.retrieve_fine_tuning_job(
            response = azure_fine_tuning_apis_instance.retrieve_fine_tuning_job(
                message="LiteLLM doesn't support {} for 'retrieve_fine_tuning_job'. Only 'openai' and 'azure' are supported.".format(
                    request=httpx.Request(method="retrieve_fine_tuning_job", url="https://github.com/BerriAI/litellm"),  # type: ignore
from typing import TYPE_CHECKING, Any, ClassVar, Dict, Iterator, Optional, Union
from litellm.constants import request_timeout
# Import the adapter for fallback to completion format
    from litellm.types.google_genai.main import (
        GenerateContentConfigDict,
        GenerateContentContentListUnionDict,
        GenerateContentResponse,
        ToolConfigDict,
    GenerateContentConfigDict = Any
    GenerateContentContentListUnionDict = Any
    GenerateContentResponse = Any
    ToolConfigDict = Any
# Initialize any necessary instances or variables here
class GenerateContentSetupResult(BaseModel):
    """Internal Type - Result of setting up a generate content call"""
    request_body: Dict[str, Any]
    generate_content_provider_config: Optional[BaseGoogleGenAIGenerateContentConfig]
    generate_content_config_dict: Dict[str, Any]
    litellm_params: GenericLiteLLMParams
    litellm_logging_obj: LiteLLMLoggingObj
    litellm_call_id: Optional[str]
class GenerateContentHelper:
    """Helper class for Google GenAI generate content operations"""
    def mock_generate_content_response(
        mock_response: str = "This is a mock response from Google GenAI generate_content.",
        """Mock response for generate_content for testing purposes"""
            "text": mock_response,
            "candidates": [
                    "content": {"parts": [{"text": mock_response}], "role": "model"},
                    "finishReason": "STOP",
                    "safetyRatings": [],
            "usageMetadata": {
                "promptTokenCount": 10,
                "candidatesTokenCount": 20,
                "totalTokenCount": 30,
    def setup_generate_content_call(
        contents: GenerateContentContentListUnionDict,
        config: Optional[GenerateContentConfigDict] = None,
        tools: Optional[ToolConfigDict] = None,
    ) -> GenerateContentSetupResult:
        Common setup logic for generate_content calls
            model: The model name
            contents: The content to generate from
            config: Optional configuration
            custom_llm_provider: Optional custom LLM provider
            tools: Optional tools
            **kwargs: Additional keyword arguments
            GenerateContentSetupResult containing all setup information
            "litellm_logging_obj"
        litellm_params = GenericLiteLLMParams(**kwargs)
        ## MOCK RESPONSE LOGIC (only for non-streaming)
            not kwargs.get("stream", False)
            and litellm_params.mock_response
            and isinstance(litellm_params.mock_response, str)
            raise ValueError("Mock response should be handled by caller")
            dynamic_api_key,
            dynamic_api_base,
        if litellm_params.custom_llm_provider is None:
            litellm_params.custom_llm_provider = custom_llm_provider
        generate_content_provider_config: Optional[
            BaseGoogleGenAIGenerateContentConfig
        ] = ProviderConfigManager.get_provider_google_genai_generate_content_config(
        if generate_content_provider_config is None:
            # Use adapter to transform to completion format when provider config is None
            # Signal that we should use the adapter by returning special result
            if litellm_logging_obj is None:
                raise ValueError("litellm_logging_obj is required, but got None")
            return GenerateContentSetupResult(
                request_body={},  # Will be handled by adapter
                generate_content_provider_config=None,  # type: ignore
                generate_content_config_dict=dict(config or {}),
                litellm_call_id=litellm_call_id,
        #########################################################################################
        # Construct request body
        # Create Google Optional Params Config
        generate_content_config_dict = (
            generate_content_provider_config.map_generate_content_optional_params(
                generate_content_config_dict=config or {},
        # Extract systemInstruction from kwargs to pass to transform
        system_instruction = kwargs.get("systemInstruction") or kwargs.get("system_instruction")
        request_body = (
            generate_content_provider_config.transform_generate_content_request(
                contents=contents,
                generate_content_config_dict=generate_content_config_dict,
                system_instruction=system_instruction,
            optional_params=dict(generate_content_config_dict),
            request_body=request_body,
            generate_content_provider_config=generate_content_provider_config,
async def agenerate_content(
    Async: Generate content using Google GenAI
        kwargs["agenerate_content"] = True
        # Handle generationConfig parameter from kwargs for backward compatibility
        if "generationConfig" in kwargs and config is None:
            config = kwargs.pop("generationConfig")
        # get custom llm provider so we can use this for mapping exceptions
            _, custom_llm_provider, _, _ = litellm.get_llm_provider(
def generate_content(
    Generate content using Google GenAI
        _is_async = kwargs.pop("agenerate_content", False)
        if litellm_params.mock_response and isinstance(
            litellm_params.mock_response, str
            return GenerateContentHelper.mock_generate_content_response(
                mock_response=litellm_params.mock_response
        # Setup the call
        setup_result = GenerateContentHelper.setup_generate_content_call(
        # Extract systemInstruction from kwargs to pass to handler
        # Check if we should use the adapter (when provider config is None)
        if setup_result.generate_content_provider_config is None:
            # Use the adapter to convert to completion format
            return GenerateContentToCompletionHandler.generate_content_handler(
                contents=contents,  # type: ignore
                config=setup_result.generate_content_config_dict,
                litellm_params=setup_result.litellm_params,
        # Call the standard handler
        response = base_llm_http_handler.generate_content_handler(
            model=setup_result.model,
            generate_content_provider_config=setup_result.generate_content_provider_config,
            generate_content_config_dict=setup_result.generate_content_config_dict,
            custom_llm_provider=setup_result.custom_llm_provider,
            logging_obj=setup_result.litellm_logging_obj,
            timeout=timeout or request_timeout,
            client=kwargs.get("client"),
            litellm_metadata=kwargs.get("litellm_metadata", {}),
async def agenerate_content_stream(
    Async: Generate content using Google GenAI with streaming response
        kwargs["agenerate_content_stream"] = True
                model=model, api_base=local_vars.get("base_url", None)
            if "stream" in kwargs:
                kwargs.pop("stream", None)
                await GenerateContentToCompletionHandler.async_generate_content_handler(
        # Call the handler with async enabled and streaming
        # Return the coroutine directly for the router to handle
        return await base_llm_http_handler.generate_content_handler(
            _is_async=True,
def generate_content_stream(
    Generate content using Google GenAI with streaming response
        # Remove any async-related flags since this is the sync function
        _is_async = kwargs.pop("agenerate_content_stream", False)
        # Call the handler with streaming enabled (sync version)
        return base_llm_http_handler.generate_content_handler(
    from litellm.images.utils import ImageEditRequestUtils
# client is imported from litellm as it's a decorator
from litellm.constants import DEFAULT_IMAGE_ENDPOINT_MODEL
from litellm.litellm_core_utils.mock_functions import mock_image_generation
from litellm.llms.base_llm import BaseImageEditConfig, BaseImageGenerationConfig
from litellm.llms.custom_llm import CustomLLM
from litellm.utils import exception_type, get_litellm_params
#################### Initialize provider clients ####################
llm_http_handler: BaseLLMHTTPHandler = BaseLLMHTTPHandler()
from openai.types.audio.transcription_create_params import FileTypes  # type: ignore
from litellm.main import (
    azure_chat_completions,
    base_llm_aiohttp_handler,
    base_llm_http_handler,
    bedrock_image_edit,
    bedrock_image_generation,
    openai_chat_completions,
    openai_image_variations,
###########################################
from litellm.types.images.main import ImageEditOptionalRequestParams
from litellm.types.llms.openai import ImageGenerationRequestQuality
    LITELLM_IMAGE_VARIATION_PROVIDERS,
# Cache for ImageEditRequestUtils to avoid repeated __getattr__ calls
_ImageEditRequestUtils_cache: Optional["ImageEditRequestUtils"] = None
def _get_ImageEditRequestUtils() -> "ImageEditRequestUtils":
    """Get ImageEditRequestUtils, loading it lazily if needed."""
    global _ImageEditRequestUtils_cache
    if _ImageEditRequestUtils_cache is None:
        module = importlib.import_module(__name__)
        _ImageEditRequestUtils_cache = module.ImageEditRequestUtils
    assert _ImageEditRequestUtils_cache is not None  # Type narrowing for type checker
    return _ImageEditRequestUtils_cache
##### Image Generation #######################
async def aimage_generation(*args, **kwargs) -> ImageResponse:
    Asynchronously calls the `image_generation` function with the given arguments and keyword arguments.
    - `args` (tuple): Positional arguments to be passed to the `image_generation` function.
    - `kwargs` (dict): Keyword arguments to be passed to the `image_generation` function.
    - `response` (Any): The response returned by the `image_generation` function.
    kwargs["aimg_generation"] = True
        func = partial(image_generation, *args, **kwargs)
        response: Optional[ImageResponse] = None
            response = ImageResponse(**init_response)
        elif isinstance(init_response, ImageResponse):  ## CACHING SCENARIO
                "Unable to get Image Response. Please pass a valid llm_provider."
# Overload for when aimg_generation=True (returns Coroutine)
def image_generation(
    quality: Optional[Union[str, ImageGenerationRequestQuality]] = None,
    aimg_generation: Literal[True],
) -> Coroutine[Any, Any, ImageResponse]: 
# Overload for when aimg_generation=False or not specified (returns ImageResponse)
    aimg_generation: Literal[False] = False,
) -> ImageResponse: 
def image_generation(  # noqa: PLR0915
    Coroutine[Any, Any, ImageResponse],
    Maps the https://api.openai.com/v1/images/generations endpoint.
    Currently supports just Azure + OpenAI.
        aimg_generation = kwargs.get("aimg_generation", False)
        mock_response: Optional[str] = kwargs.get("mock_response", None)  # type: ignore
        metadata = kwargs.get("metadata", {})
        headers: dict = kwargs.get("headers", None) or {}
        model_response: ImageResponse = litellm.utils.ImageResponse()
        dynamic_api_key: Optional[str] = None
        if model is not None or custom_llm_provider is not None:
            model = "dall-e-2"
            custom_llm_provider = "openai"  # default to dall-e-2 on openai
        model_response._hidden_params["model"] = model
            "quality",
        litellm_params = all_litellm_params
        image_generation_config: Optional[BaseImageGenerationConfig] = None
            image_generation_config = (
                ProviderConfigManager.get_provider_image_generation_config(
                    model=base_model or model,
        optional_params = get_optional_params_image_gen(
            quality=quality,
            provider_config=image_generation_config,
        logging: Logging = litellm_logging_obj
                "azure": False,
                "logger_fn": logger_fn,
        if "custom_llm_provider" not in logging.model_call_details:
            logging.model_call_details["custom_llm_provider"] = custom_llm_provider
            return mock_image_generation(model=model, mock_response=mock_response)
            # Create azure_ad_token_provider from tenant_id, client_id, client_secret if not already provided
            if azure_ad_token_provider is None:
                from litellm.llms.azure.common_utils import (
                    get_azure_ad_token_from_entra_id,
                # Extract Azure AD credentials from litellm_params
                tenant_id = litellm_params_dict.get("tenant_id")
                client_id = litellm_params_dict.get("client_id")
                client_secret = litellm_params_dict.get("client_secret")
                azure_scope = litellm_params_dict.get("azure_scope") or "https://cognitiveservices.azure.com/.default"
                # Create token provider if credentials are available
                if tenant_id and client_id and client_secret:
                    azure_ad_token_provider = get_azure_ad_token_from_entra_id(
                        scope=azure_scope,
            # Only add api-key header if api_key is not None
            # Azure AD authentication will use Authorization header instead
                default_headers["api-key"] = api_key
            for k, v in default_headers.items():
                if k not in headers:
                    headers[k] = v
            model_response = azure_chat_completions.image_generation(
                aimg_generation=aimg_generation,
        # Providers using llm_http_handler
        elif custom_llm_provider in (
            litellm.LlmProviders.RECRAFT,
            litellm.LlmProviders.AIML,
            litellm.LlmProviders.GEMINI,
            litellm.LlmProviders.FAL_AI,
            litellm.LlmProviders.STABILITY,
            litellm.LlmProviders.RUNWAYML,
            litellm.LlmProviders.VERTEX_AI,
            litellm.LlmProviders.OPENROUTER
            if image_generation_config is None:
                    f"image generation config is not supported for {custom_llm_provider}"
            # Resolve api_base from litellm.api_base if not explicitly provided
            _api_base = api_base or litellm.api_base
            litellm_params_dict["api_base"] = _api_base
            return llm_http_handler.image_generation_handler(
                image_generation_provider_config=image_generation_config,
                image_generation_optional_request_params=optional_params,
                azure_ad_token=None,
            or custom_llm_provider == LlmProviders.LITELLM_PROXY.value
            # Forward OpenAI organization if present (set by proxy pre-call utils)
            organization: Optional[str] = kwargs.get("organization", None)
            model_response = openai_chat_completions.image_generation(
                api_key=api_key or dynamic_api_key,
                raise Exception("Model needs to be set for bedrock")
            model_response = bedrock_image_generation.image_generation(  # type: ignore
            if aimg_generation is True:
                async_custom_client: Optional[AsyncHTTPHandler] = None
                if client is not None and isinstance(client, AsyncHTTPHandler):
                    async_custom_client = client
                model_response = custom_handler.aimage_generation(  # type: ignore
                    client=async_custom_client,
                custom_client: Optional[HTTPHandler] = None
                if client is not None and isinstance(client, HTTPHandler):
                    custom_client = client
                model_response = custom_handler.image_generation(
                    client=custom_client,
            completion_kwargs=locals(),
async def aimage_variation(*args, **kwargs) -> ImageResponse:
    Asynchronously calls the `image_variation` function with the given arguments and keyword arguments.
    - `args` (tuple): Positional arguments to be passed to the `image_variation` function.
    - `kwargs` (dict): Keyword arguments to be passed to the `image_variation` function.
    - `response` (Any): The response returned by the `image_variation` function.
    model = kwargs.get("model", None)
        func = partial(image_variation, *args, **kwargs)
        if custom_llm_provider is None and model is not None:
            init_response, ImageResponse
                init_response = ImageResponse(**init_response)
def image_variation(
    model: str = "dall-e-2",  # set to dall-e-2 by default - like OpenAI.
    n: int = 1,
    response_format: Literal["url", "b64_json"] = "url",
    # get non-default params
    # get logging object
    litellm_logging_obj = cast(LiteLLMLoggingObj, kwargs.get("litellm_logging_obj"))
    # get the litellm params
    litellm_params = get_litellm_params(**kwargs)
    # get the custom llm provider
        custom_llm_provider=litellm_params.get("custom_llm_provider", None),
        api_base=litellm_params.get("api_base", None),
        api_key=litellm_params.get("api_key", None),
    # route to the correct provider w/ the params
        llm_provider = LlmProviders(custom_llm_provider)
        image_variation_provider = LITELLM_IMAGE_VARIATION_PROVIDERS(llm_provider)
            f"Invalid image variation provider: {custom_llm_provider}. Supported providers are: {LITELLM_IMAGE_VARIATION_PROVIDERS}"
    model_response = ImageResponse()
        model=model or "",  # openai defaults to dall-e-2
        provider=llm_provider,
            f"image variation provider has no known model info config - required for getting api keys, etc.: {custom_llm_provider}. Supported providers are: {LITELLM_IMAGE_VARIATION_PROVIDERS}"
    api_key = provider_config.get_api_key(litellm_params.get("api_key", None))
    api_base = provider_config.get_api_base(litellm_params.get("api_base", None))
    if image_variation_provider == LITELLM_IMAGE_VARIATION_PROVIDERS.OPENAI:
            raise ValueError("API key is required for OpenAI image variations")
            raise ValueError("API base is required for OpenAI image variations")
        response = openai_image_variations.image_variations(
            timeout=litellm_params.get("timeout", None),
    elif image_variation_provider == LITELLM_IMAGE_VARIATION_PROVIDERS.TOPAZ:
            raise ValueError("API key is required for Topaz image variations")
            raise ValueError("API base is required for Topaz image variations")
        response = base_llm_aiohttp_handler.image_variations(
            timeout=litellm_params.get("timeout", None) or DEFAULT_REQUEST_TIMEOUT,
    # return the response
def image_edit(  # noqa: PLR0915
    image: Optional[Union[FileTypes, List[FileTypes]]] = None,
    prompt: Optional[str]= None,
    mask: Optional[str] = None,
) -> Union[ImageResponse, Coroutine[Any, Any, ImageResponse]]:
    Maps the image edit functionality, similar to OpenAI's images/edits endpoint.
                "async_call",
        litellm_params_list = all_litellm_params
        default_params = openai_params + litellm_params_list
        # add images / or return a single image
        images = image if isinstance(image, list) else ([image] if image is not None else [])
        headers_from_kwargs = kwargs.get("headers")
        merged_extra_headers: Dict[str, Any] = {}
        if isinstance(headers_from_kwargs, dict):
            merged_extra_headers.update(headers_from_kwargs)
        if isinstance(extra_headers, dict):
            merged_extra_headers.update(extra_headers)
        if merged_extra_headers:
            extra_headers = dict(merged_extra_headers)
            model=model or DEFAULT_IMAGE_ENDPOINT_MODEL,
        # Check for custom provider
        if custom_llm_provider in litellm._custom_providers:
            if _is_async:
                if kwargs.get("client") is not None and isinstance(
                    kwargs.get("client"), AsyncHTTPHandler
                    async_custom_client = kwargs.get("client")
                return custom_handler.aimage_edit(
                    image=images,
                    api_key=kwargs.get("api_key"),
                    api_base=kwargs.get("api_base"),
                    kwargs.get("client"), HTTPHandler
                    custom_client = kwargs.get("client")
                return custom_handler.image_edit(
        image_edit_provider_config: Optional[BaseImageEditConfig] = (
            ProviderConfigManager.get_provider_image_edit_config(
        if image_edit_provider_config is None:
            raise ValueError(f"image edit is not supported for {custom_llm_provider}")
        # Get ImageEditOptionalRequestParams with only valid parameters
        image_edit_optional_params: ImageEditOptionalRequestParams = (
            _get_ImageEditRequestUtils().get_requested_image_edit_optional_param(local_vars)
        # Get optional parameters for the responses API
        image_edit_request_params: Dict = (
            _get_ImageEditRequestUtils().get_optional_params_image_edit(
                image_edit_provider_config=image_edit_provider_config,
                image_edit_optional_params=image_edit_optional_params,
            optional_params=dict(image_edit_request_params),
                **image_edit_request_params,
        # Route bedrock to its specific handler (AWS signing required)
        if custom_llm_provider == "bedrock":
            image_edit_request_params.update(non_default_params)
            return bedrock_image_edit.image_edit(  # type: ignore
                optional_params=image_edit_request_params,
                model_response=ImageResponse(),
                aimage_edit=_is_async,
        elif custom_llm_provider == "stability":
            return base_llm_http_handler.image_edit_handler(
            image_edit_optional_request_params=image_edit_request_params,
        # Call the handler with _is_async flag instead of directly calling the async handler
async def aimage_edit(
    image: Union[FileTypes, List[FileTypes]],
    Asynchronously calls the `image_edit` function with the given arguments and keyword arguments.
    - `args` (tuple): Positional arguments to be passed to the `image_edit` function.
    - `kwargs` (dict): Keyword arguments to be passed to the `image_edit` function.
    - `response` (Any): The response returned by the `image_edit` function.
        images = image if isinstance(image, list) else [image]
            image_edit,
            mask=mask,
    """Lazy import handler for images.main module"""
    if name == "ImageEditRequestUtils":
        # Lazy load ImageEditRequestUtils to avoid heavy import from images.utils at module load time
        from .utils import ImageEditRequestUtils as _ImageEditRequestUtils
        module.__dict__["ImageEditRequestUtils"] = _ImageEditRequestUtils
        return _ImageEditRequestUtils
LiteLLM Interactions API - Main Module
Per OpenAPI spec (https://ai.google.dev/static/api/interactions.openapi.json):
- Create interaction: POST /{api_version}/interactions
- Get interaction: GET /{api_version}/interactions/{interaction_id}
- Delete interaction: DELETE /{api_version}/interactions/{interaction_id}
from litellm.interactions.http_handler import interactions_http_handler
from litellm.interactions.utils import (
    InteractionsAPIRequestUtils,
    get_provider_interactions_api_config,
from litellm.types.interactions import (
    CancelInteractionResult,
    DeleteInteractionResult,
    InteractionInput,
    InteractionsAPIResponse,
    InteractionsAPIStreamingResponse,
    InteractionTool,
# ============================================================
# SDK Methods - CREATE INTERACTION
    # Model or Agent (one required per OpenAPI spec)
    agent: Optional[str] = None,
    # Input (required)
    input: Optional[InteractionInput] = None,
    # Tools (for model interactions)
    tools: Optional[List[InteractionTool]] = None,
    # System instruction
    system_instruction: Optional[str] = None,
    # Generation config
    generation_config: Optional[Dict[str, Any]] = None,
    # Streaming
    # Storage
    store: Optional[bool] = None,
    # Background execution
    background: Optional[bool] = None,
    # Response format
    response_modalities: Optional[List[str]] = None,
    response_format: Optional[Dict[str, Any]] = None,
    response_mime_type: Optional[str] = None,
    # Continuation
    previous_interaction_id: Optional[str] = None,
    # Extra params
    # LiteLLM params
) -> Union[InteractionsAPIResponse, AsyncIterator[InteractionsAPIStreamingResponse]]:
    Async: Create a new interaction using Google's Interactions API.
    Per OpenAPI spec, provide either `model` or `agent`.
        model: The model to use (e.g., "gemini-2.5-flash")
        agent: The agent to use (e.g., "deep-research-pro-preview-12-2025")
        input: The input content (string, content object, or list)
        tools: Tools available for the model
        system_instruction: System instruction for the interaction
        generation_config: Generation configuration
        stream: Whether to stream the response
        store: Whether to store the response for later retrieval
        background: Whether to run in background
        response_modalities: Requested response modalities (TEXT, IMAGE, AUDIO)
        response_format: JSON schema for response format
        response_mime_type: MIME type of the response
        previous_interaction_id: ID of previous interaction for continuation
        extra_headers: Additional headers
        extra_body: Additional body parameters
        timeout: Request timeout
        custom_llm_provider: Override the LLM provider
        InteractionsAPIResponse or async iterator for streaming
        kwargs["acreate_interaction"] = True
        if custom_llm_provider is None and model:
        elif custom_llm_provider is None:
            custom_llm_provider = "gemini"
            generation_config=generation_config,
            response_modalities=response_modalities,
            response_mime_type=response_mime_type,
            previous_interaction_id=previous_interaction_id,
    Iterator[InteractionsAPIStreamingResponse],
    Coroutine[Any, Any, Union[InteractionsAPIResponse, AsyncIterator[InteractionsAPIStreamingResponse]]],
    Sync: Create a new interaction using Google's Interactions API.
        InteractionsAPIResponse or iterator for streaming
        _is_async = kwargs.pop("acreate_interaction", False) is True
            custom_llm_provider = custom_llm_provider or "gemini"
        interactions_api_config = get_provider_interactions_api_config(
            provider=custom_llm_provider,
        # Get optional params using utility (similar to responses API pattern)
        optional_params = InteractionsAPIRequestUtils.get_requested_interactions_api_optional_params(
            local_vars
        # Check if this is a bridge provider (litellm_responses) - similar to responses API
        # Either provider is explicitly "litellm_responses" or no config found (bridge to responses)
        if custom_llm_provider == "litellm_responses" or interactions_api_config is None:
            # Bridge to litellm.responses() for non-native providers
            from litellm.interactions.litellm_responses_transformation.handler import (
                LiteLLMResponsesInteractionsHandler,
            handler = LiteLLMResponsesInteractionsHandler()
            return handler.interactions_api_handler(
            optional_params=dict(optional_params),
            litellm_params={"litellm_call_id": litellm_call_id},
        response = interactions_http_handler.create_interaction(
            interactions_api_config=interactions_api_config,
# SDK Methods - GET INTERACTION
    interaction_id: str,
) -> InteractionsAPIResponse:
    """Async: Get an interaction by its ID."""
        kwargs["aget_interaction"] = True
            interaction_id=interaction_id,
            custom_llm_provider=custom_llm_provider or "gemini",
) -> Union[InteractionsAPIResponse, Coroutine[Any, Any, InteractionsAPIResponse]]:
    """Sync: Get an interaction by its ID."""
        _is_async = kwargs.pop("aget_interaction", False) is True
        if interactions_api_config is None:
            raise ValueError(f"Interactions API not supported for: {custom_llm_provider}")
            optional_params={"interaction_id": interaction_id},
        return interactions_http_handler.get_interaction(
# SDK Methods - DELETE INTERACTION
) -> DeleteInteractionResult:
    """Async: Delete an interaction by its ID."""
        kwargs["adelete_interaction"] = True
) -> Union[DeleteInteractionResult, Coroutine[Any, Any, DeleteInteractionResult]]:
    """Sync: Delete an interaction by its ID."""
        _is_async = kwargs.pop("adelete_interaction", False) is True
        return interactions_http_handler.delete_interaction(
# SDK Methods - CANCEL INTERACTION
async def acancel(
) -> CancelInteractionResult:
    """Async: Cancel an interaction by its ID."""
        kwargs["acancel_interaction"] = True
) -> Union[CancelInteractionResult, Coroutine[Any, Any, CancelInteractionResult]]:
    """Sync: Cancel an interaction by its ID."""
        _is_async = kwargs.pop("acancel_interaction", False) is True
        return interactions_http_handler.cancel_interaction(
## API Handler for calling Vertex AI Partner Models
from typing import Callable, Optional, Union
from litellm import LlmProviders
from litellm.types.llms.vertex_ai import VertexPartnerProvider
from litellm.utils import ModelResponse
from ...custom_httpx.llm_http_handler import BaseLLMHTTPHandler
from ..vertex_llm_base import VertexBase
class VertexAIError(Exception):
    def __init__(self, status_code, message):
        self.request = httpx.Request(
            method="POST", url=" https://cloud.google.com/vertex-ai/"
        self.response = httpx.Response(status_code=status_code, request=self.request)
            self.message
class PartnerModelPrefixes(str, Enum):
    META_PREFIX = "meta/"
    DEEPSEEK_PREFIX = "deepseek-ai"
    MISTRAL_PREFIX = "mistral"
    CODERESTAL_PREFIX = "codestral"
    JAMBA_PREFIX = "jamba"
    CLAUDE_PREFIX = "claude"
    QWEN_PREFIX = "qwen"
    GPT_OSS_PREFIX = "openai/gpt-oss-"
    MINIMAX_PREFIX = "minimaxai/"
    MOONSHOT_PREFIX = "moonshotai/"
    ZAI_PREFIX = "zai-org/"
class VertexAIPartnerModels(VertexBase):
    def is_vertex_partner_model(model: str):
        Check if the model string is a Vertex AI Partner Model
        Only use this once you have confirmed that custom_llm_provider is vertex_ai
            bool: True if the model string is a Vertex AI Partner Model, False otherwise
            model.startswith(PartnerModelPrefixes.META_PREFIX)
            or model.startswith(PartnerModelPrefixes.DEEPSEEK_PREFIX)
            or model.startswith(PartnerModelPrefixes.MISTRAL_PREFIX)
            or model.startswith(PartnerModelPrefixes.CODERESTAL_PREFIX)
            or model.startswith(PartnerModelPrefixes.JAMBA_PREFIX)
            or model.startswith(PartnerModelPrefixes.CLAUDE_PREFIX)
            or model.startswith(PartnerModelPrefixes.QWEN_PREFIX)
            or model.startswith(PartnerModelPrefixes.GPT_OSS_PREFIX)
            or model.startswith(PartnerModelPrefixes.MINIMAX_PREFIX)
            or model.startswith(PartnerModelPrefixes.MOONSHOT_PREFIX)
            or model.startswith(PartnerModelPrefixes.ZAI_PREFIX)
    def should_use_openai_handler(model: str):
        OPENAI_LIKE_VERTEX_PROVIDERS = [
            "llama",
            PartnerModelPrefixes.DEEPSEEK_PREFIX,
            PartnerModelPrefixes.QWEN_PREFIX,
            PartnerModelPrefixes.GPT_OSS_PREFIX,
            PartnerModelPrefixes.MINIMAX_PREFIX,
            PartnerModelPrefixes.MOONSHOT_PREFIX,
            PartnerModelPrefixes.ZAI_PREFIX,
        if any(provider in model for provider in OPENAI_LIKE_VERTEX_PROVIDERS):
        custom_prompt_dict: dict,
        headers: Optional[dict],
            import vertexai
            from litellm.llms.anthropic.chat import AnthropicChatCompletion
            from litellm.llms.codestral.completion.handler import (
                CodestralTextCompletion,
            from litellm.llms.openai_like.chat.handler import OpenAILikeChatHandler
            from litellm.llms.vertex_ai.gemini.vertex_and_google_ai_studio_gemini import (
            raise VertexAIError(
                message=f"""vertexai import failed please run `pip install -U "google-cloud-aiplatform>=1.38"`. Got error: {e}""",
            hasattr(vertexai, "preview") or hasattr(vertexai.preview, "language_models")
                message="""Upgrade vertex ai. Run `pip install "google-cloud-aiplatform>=1.38"`""",
            vertex_httpx_logic = VertexLLM()
            access_token, project_id = vertex_httpx_logic._ensure_access_token(
                credentials=vertex_credentials,
                project_id=vertex_project,
            openai_like_chat_completions = OpenAILikeChatHandler()
            codestral_fim_completions = CodestralTextCompletion()
            ## CONSTRUCT API BASE
            stream: bool = optional_params.get("stream", False) or False
            if self.should_use_openai_handler(model):
                partner = VertexPartnerProvider.llama
            elif "mistral" in model or "codestral" in model:
                partner = VertexPartnerProvider.mistralai
            elif "jamba" in model:
                partner = VertexPartnerProvider.ai21
                partner = VertexPartnerProvider.claude
                raise ValueError(f"Unknown partner model: {model}")
            api_base = self.get_complete_vertex_url(
                custom_api_base=api_base,
                vertex_location=vertex_location,
                vertex_project=vertex_project,
                project_id=project_id,
                partner=partner,
            if "codestral" in model or "mistral" in model:
                model = model.split("@")[0]
            if "codestral" in model and litellm_params.get("text_completion") is True:
                optional_params["model"] = model
                return codestral_fim_completions.completion(
                    api_key=access_token,
                headers.update({"Authorization": "Bearer {}".format(access_token)})
                optional_params.update(
                        "anthropic_version": "vertex-2023-10-16",
                        "is_vertex_request": True,
                return anthropic_chat_completions.completion(
                    encoding=encoding,  # for calculating input/output tokens
                    custom_llm_provider=LlmProviders.VERTEX_AI.value,
            elif self.should_use_openai_handler(model):
                return base_llm_http_handler.completion(
                    logging_obj=logging_obj,  # model call logging done inside the class as we make need to modify I/O to fit aleph alpha's requirements
            return openai_like_chat_completions.completion(
                custom_endpoint=True,
            raise VertexAIError(status_code=500, message=str(e))
    async def count_tokens(
        Count tokens for Vertex AI partner models (Anthropic Claude, Mistral, etc.)
            model: The model name (e.g., "claude-3-5-sonnet-20241022")
            messages: List of messages in Anthropic Messages API format
            litellm_params: LiteLLM parameters dict
            vertex_project: Optional Google Cloud project ID
            vertex_location: Optional Vertex AI location
            vertex_credentials: Optional Vertex AI credentials
            Dict containing token count information
            from litellm.llms.vertex_ai.vertex_ai_partner_models.count_tokens.handler import (
                VertexAIPartnerModelsTokenCounter,
            # Prepare request data in Anthropic Messages API format
            # Prepare litellm_params with credentials
            _litellm_params = litellm_params.copy()
            if vertex_project:
                _litellm_params["vertex_project"] = vertex_project
            if vertex_location:
                _litellm_params["vertex_location"] = vertex_location
            if vertex_credentials:
                _litellm_params["vertex_credentials"] = vertex_credentials
            # Call the token counter
            token_counter = VertexAIPartnerModelsTokenCounter()
            result = await token_counter.handle_count_tokens_request(
                request_data=request_data,
                litellm_params=_litellm_params,
API Handler for calling Vertex AI Gemma Models
These models use a custom prediction endpoint format that wraps messages in 'instances'
with @requestFormat: "chatCompletions" and returns responses wrapped in 'predictions'.
response = litellm.completion(
    model="vertex_ai/gemma/gemma-3-12b-it-1222199011122",
    messages=[{"role": "user", "content": "What is machine learning?"}],
    vertex_project="your-project-id",
    vertex_location="us-central1",
Sent to this route when `model` is in the format `vertex_ai/gemma/{MODEL_NAME}`
The API expects a custom endpoint URL format:
https://{ENDPOINT_NUMBER}.{location}-{REGION_NUMBER}.prediction.vertexai.goog/v1/projects/{PROJECT_ID}/locations/{location}/endpoints/{ENDPOINT_ID}:predict
from ..common_utils import VertexAIError, get_vertex_base_model_name
class VertexAIGemmaModels(VertexBase):
        Handles calling Vertex AI Gemma Models
            from litellm.llms.vertex_ai.vertex_gemma_models.transformation import (
                VertexGemmaConfig,
            model = get_vertex_base_model_name(model=model)
            gemma_transformation = VertexGemmaConfig()
            # If api_base is not provided, it should be set as an environment variable
            # or passed explicitly because the endpoint URL is unique per deployment
                    message="api_base is required for Vertex AI Gemma models. Please provide the full endpoint URL.",
            # Check if we need to append :predict
            if not api_base.endswith(":predict"):
                _, api_base = self._check_custom_proxy(
                    endpoint="predict",
                    auth_header=None,
            # If api_base already ends with :predict, use it as-is
            # Use the custom transformation handler for gemma models
            return gemma_transformation.completion(
API Handler for calling Vertex AI Model Garden Models
Most Vertex Model Garden Models are OpenAI compatible - so this handler calls `openai_like_chat_completions`
    model="vertex_ai/openai/5464397967697903616",
    messages=[{"role": "user", "content": "Hello, how are you?"}],
Sent to this route when `model` is in the format `vertex_ai/openai/{MODEL_ID}`
Vertex Documentation for using the OpenAI /chat/completions endpoint: https://github.com/GoogleCloudPlatform/vertex-ai-samples/blob/main/notebooks/community/model_garden/model_garden_pytorch_llama3_deployment.ipynb
from litellm.llms.vertex_ai.common_utils import get_vertex_base_url
def create_vertex_url(
    vertex_location: str,
    vertex_project: str,
    """Return the base url for the vertex garden models"""
    base_url = get_vertex_base_url(vertex_location)
    return f"{base_url}/v1beta1/projects/{vertex_project}/locations/{vertex_location}/endpoints/{model}"
class VertexAIModelGardenModels(VertexBase):
        Handles calling Vertex AI Model Garden Models in OpenAI compatible format
            default_api_base = create_vertex_url(
                vertex_location=vertex_location or "us-central1",
                vertex_project=vertex_project or project_id,
            if len(default_api_base.split(":")) > 1:
                endpoint = default_api_base.split(":")[-1]
                endpoint = ""
                url=default_api_base,
                vertex_api_version="v1beta1",
Main OCR function for LiteLLM.
from typing import Any, Coroutine, Dict, Optional, Union
from litellm.llms.base_llm.ocr.transformation import BaseOCRConfig, OCRResponse
async def aocr(
    document: Dict[str, str],
) -> OCRResponse:
    Async OCR function.
        model: Model name (e.g., "mistral/mistral-ocr-latest")
        document: Document to process in Mistral format:
            {"type": "document_url", "document_url": "https://..."} for PDFs/docs or
            {"type": "image_url", "image_url": "https://..."} for images
        api_key: Optional API key
        api_base: Optional API base URL
        timeout: Optional timeout
        extra_headers: Optional extra headers
        **kwargs: Additional parameters (e.g., include_image_base64, pages, image_limit)
        OCRResponse in Mistral OCR format with pages, model, usage_info, etc.
        # OCR with PDF
        response = await litellm.aocr(
            model="mistral/mistral-ocr-latest",
            document={
                "type": "document_url",
                "document_url": "https://arxiv.org/pdf/2201.04234"
            include_image_base64=True
        # OCR with image
                "image_url": "https://example.com/image.png"
        # OCR with base64 encoded PDF
                "document_url": f"data:application/pdf;base64,{base64_pdf}"
        kwargs["aocr"] = True
        # Get custom llm provider
                model=model, api_base=api_base
            ocr,
            document=document,
                f"Got an unexpected None response from the OCR API: {response}"
def ocr(
) -> Union[OCRResponse, Coroutine[Any, Any, OCRResponse]]:
    Synchronous OCR function.
        response = litellm.ocr(
        # Access pages
        for page in response.pages:
            print(f"Page {page.index}: {page.markdown}")
        _is_async = kwargs.pop("aocr", False) is True
        # Validate document parameter format (Mistral spec)
        if not isinstance(document, dict):
            raise ValueError(f"document must be a dict with 'type' and URL field, got {type(document)}")
        doc_type = document.get("type")
        if doc_type not in ["document_url", "image_url"]:
            raise ValueError(f"Invalid document type: {doc_type}. Must be 'document_url' or 'image_url'")
        model, custom_llm_provider, dynamic_api_key, dynamic_api_base = (
            litellm.get_llm_provider(
        # Update with dynamic values if available
        if dynamic_api_key:
        if dynamic_api_base:
            api_base = dynamic_api_base
        # Get provider config
        ocr_provider_config: Optional[BaseOCRConfig] = (
            ProviderConfigManager.get_provider_ocr_config(
        if ocr_provider_config is None:
                f"OCR is not supported for provider: {custom_llm_provider}"
            f"OCR call - model: {model}, provider: {custom_llm_provider}"
        # Get litellm params using GenericLiteLLMParams (same as responses API)
        # Extract OCR-specific parameters from kwargs
        supported_params = ocr_provider_config.get_supported_ocr_params(model=model)
        for param in supported_params:
            if param in kwargs:
                non_default_params[param] = kwargs.pop(param)
        # Map parameters to provider-specific format
        optional_params = ocr_provider_config.map_ocr_params(
        verbose_logger.debug(f"OCR optional_params after mapping: {optional_params}")
        # Call the handler - pass document dict directly
        response = base_llm_http_handler.ocr(
            document=document,  # Pass the entire document dict
            aocr=_is_async,
            headers=extra_headers,
            provider_config=ocr_provider_config,
            litellm_params=dict(litellm_params),
This module is used to pass through requests to the LLM APIs.
from httpx._types import CookieTypes, QueryParamTypes, RequestFiles
from litellm.passthrough.utils import CommonUtils
async def allm_passthrough_route(
    endpoint: str,
    request_query_params: Optional[dict] = None,
    request_headers: Optional[dict] = None,
    content: Optional[Any] = None,
    data: Optional[dict] = None,
    files: Optional[RequestFiles] = None,
    json: Optional[Any] = None,
    params: Optional[QueryParamTypes] = None,
    cookies: Optional[CookieTypes] = None,
    client: Optional[Union[HTTPHandler, AsyncHTTPHandler]] = None,
) -> Union[httpx.Response, AsyncGenerator[Any, Any]]:
    Async: Reranks a list of documents based on their relevance to the query
        kwargs["allm_passthrough_route"] = True
        model, custom_llm_provider, api_key, api_base = get_llm_provider(
        from litellm.utils import ProviderConfigManager
        provider_config = cast(
            Optional["BasePassthroughConfig"], kwargs.get("provider_config")
        ) or ProviderConfigManager.get_provider_passthrough_config(
            raise Exception(f"Provider {custom_llm_provider} not found")
            llm_passthrough_route,
            request_query_params=request_query_params,
            request_headers=request_headers,
        # Since allm_passthrough_route=True, we always get a coroutine from _async_passthrough_request
            # Only call raise_for_status if it's a Response object (not a generator)
            if isinstance(response, httpx.Response):
            # This shouldn't happen when allm_passthrough_route=True, but handle it for type safety
            raise Exception("Expected coroutine from async passthrough route")
        # For HTTP errors, re-raise as-is to preserve the original error details
        # The caller (e.g., proxy layer) can handle conversion to appropriate response format
        # For other exceptions, use provider-specific error handling
        # Get the provider using the same logic as llm_passthrough_route
        _, resolved_custom_llm_provider, _, _ = get_llm_provider(
        # Get provider config if available
        if resolved_custom_llm_provider:
                    provider=LlmProviders(resolved_custom_llm_provider),
                # If we can't get provider config, pass None
            # If no provider config available, raise the original exception
        raise base_llm_http_handler._handle_error(
            e=e,
def llm_passthrough_route(
    allm_passthrough_route: bool = False,
    httpx.Response,
    Coroutine[Any, Any, httpx.Response],
    Coroutine[Any, Any, Union[httpx.Response, AsyncGenerator[Any, Any]]],
    Generator[Any, Any, Any],
    AsyncGenerator[Any, Any],
    Pass through requests to the LLM APIs.
    Step 1. Build the request
    Step 2. Send the request
    Step 3. Return the response
    from litellm.litellm_core_utils.get_litellm_params import get_litellm_params
    _is_async = allm_passthrough_route
            client = litellm.module_level_aclient
            client = litellm.module_level_client
    litellm_logging_obj = cast("LiteLLMLoggingObj", kwargs.get("litellm_logging_obj"))
    # Add model_id to litellm_params if present in kwargs (for Bedrock Application Inference Profiles)
    if "model_id" in kwargs:
        litellm_params_dict["model_id"] = kwargs["model_id"]
        request_data=data if data else json,
    updated_url, base_target_url = provider_config.get_complete_url(
    # [TODO: Refactor to bedrockpassthroughconfig] need to encode the id of application-inference-profile for bedrock
    if custom_llm_provider == "bedrock" and "application-inference-profile" in endpoint:
        encoded_url_str = CommonUtils.encode_bedrock_runtime_modelid_arn(
            str(updated_url)
        updated_url = httpx.URL(encoded_url_str)
    # Add or update query parameters
    provider_api_key = provider_config.get_api_key(api_key)
    auth_headers = provider_config.validate_environment(
        api_key=provider_api_key,
        api_base=base_target_url,
    headers = BasePassthroughUtils.forward_headers_from_request(
        request_headers=request_headers or {},
        headers=auth_headers,
        forward_headers=False,
    headers, signed_json_body = provider_config.sign_request(
        api_base=str(updated_url),
    ## SWAP MODEL IN JSON BODY [TODO: REFACTOR TO A provider_config.transform_request method]
    if json and isinstance(json, dict) and "model" in json:
        json["model"] = model
    request = client.client.build_request(
        url=updated_url,
        content=signed_json_body,
        data=data if signed_json_body is None else None,
        json=json if signed_json_body is None else None,
    ## IS STREAMING REQUEST
    is_streaming_request = provider_config.is_streaming_request(
        request_data=data or json or {},
    # Update logging object with streaming status
    litellm_logging_obj.stream = is_streaming_request
    ## LOGGING PRE-CALL
    request_data = data if data else json
    litellm_logging_obj.pre_call(
        input=request_data,
            "complete_input_dict": request_data,
            "api_base": str(updated_url),
            # Return the coroutine to be awaited by the caller
            return _async_passthrough_request(
                is_streaming_request=is_streaming_request,
            # Sync path - client.client.send returns Response directly
            response: httpx.Response = client.client.send(request=request, stream=is_streaming_request)  # type: ignore
                hasattr(response, "iter_bytes") and is_streaming_request
            ):  # yield the chunk, so we can store it in the logging object
                return _sync_streaming(response, litellm_logging_obj, provider_config)
                # For non-streaming responses, yield the entire response
async def _async_passthrough_request(
    client: Union[HTTPHandler, AsyncHTTPHandler],
    is_streaming_request: bool,
    litellm_logging_obj: "LiteLLMLoggingObj",
    provider_config: "BasePassthroughConfig",
    Handle async passthrough requests.
    Uses async client to send request and properly handles streaming.
    # client.client.send returns a coroutine for async clients
    response_result = client.client.send(request=request, stream=is_streaming_request)
    # Check if it's a coroutine and await it
    if asyncio.iscoroutine(response_result):
        if is_streaming_request:
            # Pass the coroutine to _async_streaming which will await it
            return _async_streaming(
                response=response_result,
            response = await response_result
        # Fallback for sync-like behavior (shouldn't happen in async path)
        raise Exception("Expected coroutine from async client")
def _sync_streaming(
    from litellm.utils import executor
        raw_bytes: List[bytes] = []
        for chunk in response.iter_bytes():  # type: ignore
            raw_bytes.append(chunk)
            litellm_logging_obj.flush_passthrough_collected_chunks,
            raw_bytes=raw_bytes,
async def _async_streaming(
    response: Coroutine[Any, Any, httpx.Response],
        iter_response = await response
        async for chunk in iter_response.aiter_bytes():  # type: ignore
            litellm_logging_obj.async_flush_passthrough_collected_chunks(
# stdlib imports
# third party imports
from litellm._version import version as litellm_version
from litellm.proxy.client.health import HealthManagementClient
from .commands.auth import get_stored_api_key, login, logout, whoami
from .commands.chat import chat
from .commands.credentials import credentials
from .commands.http import http
from .commands.keys import keys
# local imports
from .commands.models import models
from .commands.teams import teams
from .commands.users import users
from .interface import interactive_shell
def print_version(base_url: str, api_key: Optional[str]):
    """Print CLI and server version info."""
    click.echo(f"LiteLLM Proxy CLI Version: {litellm_version}")
    if base_url:
        click.echo(f"LiteLLM Proxy Server URL: {base_url}")
        health_client = HealthManagementClient(base_url=base_url, api_key=api_key)
        server_version = health_client.get_server_version()
        if server_version:
            click.echo(f"LiteLLM Proxy Server Version: {server_version}")
            click.echo("LiteLLM Proxy Server Version: (unavailable)")
        click.echo(f"Could not retrieve server version: {e}")
@click.group(invoke_without_command=True)
    "--version", "-v", is_flag=True, is_eager=True, expose_value=False,
    help="Show the LiteLLM Proxy CLI and server version and exit.",
    callback=lambda ctx, param, value: (
        print_version(
            ctx.params.get("base_url") or "http://localhost:4000",
            ctx.params.get("api_key")
        or ctx.exit()
    ) if value and not ctx.resilient_parsing else None,
    "--base-url",
    envvar="LITELLM_PROXY_URL",
    show_envvar=True,
    default="http://localhost:4000",
    help="Base URL of the LiteLLM proxy server",
    "--api-key",
    envvar="LITELLM_PROXY_API_KEY",
    help="API key for authentication",
@click.pass_context
def cli(ctx: click.Context, base_url: str, api_key: Optional[str]) -> None:
    """LiteLLM Proxy CLI - Manage your LiteLLM proxy server"""
    ctx.ensure_object(dict)
    # If no API key provided via flag or environment variable, try to load from saved token
        api_key = get_stored_api_key()
    ctx.obj["base_url"] = base_url
    ctx.obj["api_key"] = api_key
    # If no subcommand was invoked, start interactive mode
    if ctx.invoked_subcommand is None:
        interactive_shell(ctx)
@cli.command()
def version(ctx: click.Context):
    """Show the LiteLLM Proxy CLI and server version."""
    print_version(ctx.obj.get("base_url"), ctx.obj.get("api_key"))
# Add authentication commands as top-level commands
cli.add_command(login)
cli.add_command(logout)
cli.add_command(whoami)
# Add the models command group
cli.add_command(models)
# Add the credentials command group
cli.add_command(credentials)
# Add the chat command group
cli.add_command(chat)
# Add the http command group
cli.add_command(http)
# Add the keys command group
cli.add_command(keys)
# Add the teams command group
cli.add_command(teams)
# Add the users command group
cli.add_command(users)
Skills Injection Hook for LiteLLM Proxy
Main hook that orchestrates skill processing:
- Fetches skills from LiteLLM DB
- Injects SKILL.md content into system prompt
- Adds litellm_code_execution tool for automatic code execution
- Handles agentic loop internally when litellm_code_execution is called
For non-Anthropic models (e.g., Bedrock, OpenAI, etc.):
- Skills are converted to OpenAI-style tools
- Skill file content (SKILL.md) is extracted and injected into the system prompt
- litellm_code_execution tool is added - when model calls it, LiteLLM handles
  execution automatically and returns final response with file_ids
    # Simple - LiteLLM handles everything automatically via proxy
    # The container parameter triggers the SkillsInjectionHook
    response = await litellm.acompletion(
        messages=[{"role": "user", "content": "Create a bouncing ball GIF"}],
        container={"skills": [{"skill_id": "litellm:skill_abc123"}]},
    # Response includes file_ids for generated files
from typing import Any, Dict, List, Optional, Union
from litellm.caching.caching import DualCache
from litellm.proxy._types import LiteLLM_SkillsTable, UserAPIKeyAuth
class SkillsInjectionHook(CustomLogger):
    Pre/Post-call hook that processes skills from container.skills parameter.
    Pre-call (async_pre_call_hook):
    - Skills with 'litellm:' prefix are fetched from LiteLLM DB
    - For Anthropic models: native skills pass through, LiteLLM skills converted to tools
    - For non-Anthropic models: LiteLLM skills are converted to tools + execute_code tool
    Post-call (async_post_call_success_deployment_hook):
    - If response has litellm_code_execution tool call, automatically execute code
    - Continue conversation loop until model gives final response
    - Return response with generated files inline
    This hook is called automatically by litellm during completion calls.
        self.optional_params = kwargs
        self.prompt_handler = SkillPromptInjectionHandler()
        self.max_iterations = kwargs.get("max_iterations", DEFAULT_MAX_ITERATIONS)
        self.sandbox_timeout = kwargs.get("sandbox_timeout", DEFAULT_SANDBOX_TIMEOUT)
    async def async_pre_call_hook(
        cache: DualCache,
    ) -> Optional[Union[Exception, str, dict]]:
        Process skills from container.skills before the LLM call.
        1. Check if container.skills exists in request
        2. Separate skills by prefix (litellm: vs native)
        3. Fetch LiteLLM skills from database
        4. For Anthropic: keep native skills in container
        5. For non-Anthropic: convert LiteLLM skills to tools, inject content, add execute_code
        # Only process completion-type calls
        if call_type not in ["completion", "acompletion", "anthropic_messages"]:
        container = data.get("container")
        if not container or not isinstance(container, dict):
        skills = container.get("skills")
        if not skills or not isinstance(skills, list):
        verbose_proxy_logger.debug(f"SkillsInjectionHook: Processing {len(skills)} skills")
        litellm_skills: List[LiteLLM_SkillsTable] = []
        anthropic_skills: List[Dict[str, Any]] = []
        # Separate skills by prefix
        for skill in skills:
            if not isinstance(skill, dict):
            skill_id = skill.get("skill_id", "")
            if skill_id.startswith("litellm_"):
                # Fetch from LiteLLM DB
                db_skill = await self._fetch_skill_from_db(skill_id)
                if db_skill:
                    litellm_skills.append(db_skill)
                        f"SkillsInjectionHook: Skill '{skill_id}' not found in LiteLLM DB"
                # Native Anthropic skill - pass through
                anthropic_skills.append(skill)
        # Check if using messages API spec (anthropic_messages call type)
        # Messages API always uses Anthropic-style tool format
        use_anthropic_format = call_type == "anthropic_messages"
        if len(litellm_skills) > 0:
            data = self._process_for_messages_api(
                litellm_skills=litellm_skills,
                use_anthropic_format=use_anthropic_format,
    def _process_for_messages_api(
        litellm_skills: List[LiteLLM_SkillsTable],
        use_anthropic_format: bool = True,
        Process skills for messages API (Anthropic format tools).
        - Converts skills to Anthropic-style tools (name, description, input_schema)
        - Extracts and injects SKILL.md content into system prompt
        - Adds litellm_code_execution tool for code execution
        - Stores skill files in metadata for sandbox execution
            get_litellm_code_execution_tool_anthropic,
        tools = data.get("tools", [])
        skill_contents: List[str] = []
        all_skill_files: Dict[str, Dict[str, bytes]] = {}
        all_module_paths: List[str] = []
        for skill in litellm_skills:
            # Convert skill to Anthropic-style tool
            tools.append(self.prompt_handler.convert_skill_to_anthropic_tool(skill))
            # Extract skill content from file if available
            content = self.prompt_handler.extract_skill_content(skill)
                skill_contents.append(content)
            # Extract all files for code execution
            skill_files = self.prompt_handler.extract_all_files(skill)
            if skill_files:
                all_skill_files[skill.skill_id] = skill_files
                for path in skill_files.keys():
                    if path.endswith(".py"):
                        all_module_paths.append(path)
            data["tools"] = tools
        # Inject skill content into system prompt
        # For Anthropic messages API, use top-level 'system' param instead of messages array
        if skill_contents:
            data = self.prompt_handler.inject_skill_content_to_messages(
                data, skill_contents, use_anthropic_format=use_anthropic_format
        # Add litellm_code_execution tool if we have skill files
        if all_skill_files:
            code_exec_tool = get_litellm_code_execution_tool_anthropic()
            data["tools"] = data.get("tools", []) + [code_exec_tool]
            # Store skill files in litellm_metadata for automatic code execution
            data["litellm_metadata"] = data.get("litellm_metadata", {})
            data["litellm_metadata"]["_skill_files"] = all_skill_files
            data["litellm_metadata"]["_litellm_code_execution_enabled"] = True
        # Remove container (not supported by underlying providers)
        data.pop("container", None)
            f"SkillsInjectionHook: Messages API - converted {len(litellm_skills)} skills to Anthropic tools, "
            f"injected {len(skill_contents)} skill contents, "
            f"added litellm_code_execution tool with {len(all_module_paths)} modules"
    def _process_non_anthropic_model(
        Process skills for non-Anthropic models (OpenAI format tools).
        - Converts skills to OpenAI-style tools
        - Extracts and injects SKILL.md content
        - Adds execute_code tool for code execution
            # Convert skill to OpenAI-style tool
            tools.append(self.prompt_handler.convert_skill_to_tool(skill))
                # Collect Python module paths
            data = self.prompt_handler.inject_skill_content_to_messages(data, skill_contents)
            data["tools"] = data.get("tools", []) + [get_litellm_code_execution_tool()]
            # Using litellm_metadata instead of metadata to avoid conflicts with user metadata
        # Remove container for non-Anthropic (they don't support it)
            f"SkillsInjectionHook: Non-Anthropic model - converted {len(litellm_skills)} skills to tools, "
            f"added execute_code tool with {len(all_module_paths)} modules"
    async def _fetch_skill_from_db(self, skill_id: str) -> Optional[LiteLLM_SkillsTable]:
        Fetch a skill from the LiteLLM database.
            skill_id: The skill ID (without 'litellm:' prefix)
            LiteLLM_SkillsTable or None if not found
            return await LiteLLMSkillsHandler.fetch_skill_from_db(skill_id)
                f"SkillsInjectionHook: Error fetching skill {skill_id}: {e}"
    def _is_anthropic_model(self, model: str) -> bool:
        Check if the model is an Anthropic model using get_llm_provider.
            model: The model name/identifier
            True if Anthropic model, False otherwise
            return custom_llm_provider == "anthropic"
            # Fallback to simple check if get_llm_provider fails
            return "claude" in model.lower() or model.lower().startswith("anthropic/")
        call_type: Optional[CallTypes],
        Post-call hook to handle automatic code execution.
        Handles both OpenAI format (response.choices) and Anthropic/messages API 
        format (response["content"]).
        If the response contains a tool call (litellm_code_execution or skill tool):
        1. Execute the code in sandbox
        2. Add result to messages
        3. Make another LLM call
        4. Repeat until model gives final response
        5. Return modified response with generated files
        # Check if code execution is enabled for this request
        litellm_metadata = request_data.get("litellm_metadata") or {}
        metadata = request_data.get("metadata") or {}
        code_exec_enabled = (
            litellm_metadata.get("_litellm_code_execution_enabled") or
            metadata.get("_litellm_code_execution_enabled")
        if not code_exec_enabled:
        # Get skill files
        skill_files_by_id = (
            litellm_metadata.get("_skill_files") or
            metadata.get("_skill_files", {})
        all_skill_files: Dict[str, bytes] = {}
        for files_dict in skill_files_by_id.values():
            all_skill_files.update(files_dict)
        if not all_skill_files:
                "SkillsInjectionHook: No skill files found, cannot execute code"
        # Check for tool calls - handle both Anthropic and OpenAI formats
        tool_calls = self._extract_tool_calls(response)
        # Check if any tool call needs execution (litellm_code_execution or skill tool)
        has_executable_tool = False
            tool_name = tc.get("name", "")
            # Execute if it's litellm_code_execution OR a skill tool (skill_xxx)
            if tool_name == LiteLLMInternalTools.CODE_EXECUTION.value or tool_name.startswith("skill_"):
                has_executable_tool = True
        if not has_executable_tool:
            "SkillsInjectionHook: Detected tool call, starting execution loop"
        # Start the agentic loop
        return await self._execute_code_loop_messages_api(
            skill_files=all_skill_files,
    def _extract_tool_calls(self, response: Any) -> List[Dict[str, Any]]:
        """Extract tool calls from response, handling both formats."""
        # Get content - handle both dict and object responses
            content = response.get("content", [])
        elif hasattr(response, "content"):
            content = response.content
        # Anthropic/messages API format: response has "content" list with tool_use blocks
                if isinstance(block, dict) and block.get("type") == "tool_use":
                    tool_calls.append({
                        "id": block.get("id"),
                        "input": block.get("input", {}),
                elif hasattr(block, "type") and getattr(block, "type", None) == "tool_use":
                        "id": getattr(block, "id", None),
                        "name": getattr(block, "name", None),
                        "input": getattr(block, "input", {}),
        # OpenAI format: response has choices[0].message.tool_calls
        if not tool_calls and hasattr(response, "choices") and response.choices:  # type: ignore[union-attr]
            msg = response.choices[0].message  # type: ignore[union-attr]
            if hasattr(msg, "tool_calls") and msg.tool_calls:
                for tc in msg.tool_calls:
                        "id": tc.id,
                        "name": tc.function.name,
                        "input": json.loads(tc.function.arguments) if tc.function.arguments else {},
        return tool_calls
    async def _execute_code_loop_messages_api(
        skill_files: Dict[str, bytes],
        Execute the code execution loop for messages API (Anthropic format).
        Returns the final response with generated files inline.
        from litellm.llms.litellm_proxy.skills.sandbox_executor import (
        # Ensure response is not None
                "SkillsInjectionHook: Response is None, cannot execute code loop"
        model = data.get("model", "")
        messages = list(data.get("messages", []))
        max_tokens = data.get("max_tokens", 4096)
        executor = SkillsSandboxExecutor(timeout=self.sandbox_timeout)
        generated_files: List[Dict[str, Any]] = []
        for iteration in range(self.max_iterations):
            # Extract tool calls from current response
            tool_calls = self._extract_tool_calls(current_response)
            stop_reason = current_response.get("stop_reason") if isinstance(current_response, dict) else getattr(current_response, "stop_reason", None)
            # Get content for assistant message - convert to plain dicts
            raw_content = current_response.get("content", []) if isinstance(current_response, dict) else getattr(current_response, "content", [])
            for block in raw_content or []:
                elif hasattr(block, "model_dump"):
                    content_blocks.append(block.model_dump())
                elif hasattr(block, "__dict__"):
                    content_blocks.append(dict(block.__dict__))
                    content_blocks.append({"type": "text", "text": str(block)})
            # Build assistant message for conversation history (Anthropic format)
            assistant_msg = {"role": "assistant", "content": content_blocks}
            messages.append(assistant_msg)
            # Check if we're done (no tool calls)
            if stop_reason != "tool_use" or not tool_calls:
                    f"SkillsInjectionHook: Loop completed after {iteration + 1} iterations, "
                    f"{len(generated_files)} files generated"
                return self._attach_files_to_response(current_response, generated_files)
            # Process tool calls
            tool_results = []
                tool_id = tc.get("id", "")
                tool_input = tc.get("input", {})
                # Execute if it's litellm_code_execution OR a skill tool
                if tool_name == LiteLLMInternalTools.CODE_EXECUTION.value:
                    code = tool_input.get("code", "")
                    result = await self._execute_code(code, skill_files, executor, generated_files)
                elif tool_name.startswith("skill_"):
                    # Skill tool - execute the skill's code
                    result = await self._execute_skill_tool(tool_name, tool_input, skill_files, executor, generated_files)
                    result = f"Tool '{tool_name}' not handled"
                tool_results.append({
                    "tool_use_id": tool_id,
                    "content": result,
            # Add tool results to messages (Anthropic format)
            messages.append({"role": "user", "content": tool_results})
            # Make next LLM call
                f"SkillsInjectionHook: Making LLM call iteration {iteration + 2}"
                current_response = await litellm.anthropic.acreate(
                if current_response is None:
                        "SkillsInjectionHook: LLM call returned None"
                    return self._attach_files_to_response(response, generated_files)
                    f"SkillsInjectionHook: LLM call failed: {e}"
            f"SkillsInjectionHook: Max iterations ({self.max_iterations}) reached"
    async def _execute_code(
        code: str,
        executor: Any,
        generated_files: List[Dict[str, Any]],
        """Execute code in sandbox and return result string."""
            verbose_proxy_logger.debug(f"SkillsInjectionHook: Executing code ({len(code)} chars)")
            exec_result = executor.execute(code=code, skill_files=skill_files)
            result = exec_result.get("output", "") or ""
            # Collect generated files
            if exec_result.get("files"):
                for f in exec_result["files"]:
                    generated_files.append({
                        "name": f["name"],
                        "mime_type": f["mime_type"],
                        "content_base64": f["content_base64"],
                        "size": len(base64.b64decode(f["content_base64"])),
                    result += f"\n\nGenerated file: {f['name']}"
            if exec_result.get("error"):
                result += f"\n\nError: {exec_result['error']}"
            return result or "Code executed successfully"
            return f"Code execution failed: {str(e)}"
    async def _execute_skill_tool(
        tool_name: str,
        tool_input: Dict[str, Any],
        """Execute a skill tool by generating and running code based on skill content."""
        # Generate code based on available skill modules
        # Look for Python modules in the skill
        python_modules = [p for p in skill_files.keys() if p.endswith(".py") and not p.endswith("__init__.py")]
        # Try to find the main builder/creator module
        main_module = None
        for mod in python_modules:
            if "builder" in mod.lower() or "creator" in mod.lower() or "generator" in mod.lower():
                main_module = mod
        if not main_module and python_modules:
            # Use first non-init module
            main_module = python_modules[0]
        if main_module:
            # Convert path to import: "core/gif_builder.py" -> "core.gif_builder"
            import_path = main_module.replace("/", ".").replace(".py", "")
            # Generate code that imports and uses the module
            code = f"""
# Auto-generated code to execute skill
sys.path.insert(0, '/sandbox')
from {import_path} import *
# Try to find and use a Builder/Creator class
module = __import__('{import_path}', fromlist=[''])
for name, obj in inspect.getmembers(module):
    if inspect.isclass(obj) and name != 'object':
            instance = obj()
            # Try common methods
            if hasattr(instance, 'create'):
                result = instance.create()
            elif hasattr(instance, 'build'):
                result = instance.build()
            elif hasattr(instance, 'generate'):
                result = instance.generate()
            elif hasattr(instance, 'save'):
                instance.save('output.gif')
            print(f'Used {{name}} class')
            print(f'Error with {{name}}: {{e}}')
# List generated files
for f in os.listdir('.'):
    if f.endswith(('.gif', '.png', '.jpg')):
        print(f'Generated: {{f}}')
            # Fallback generic code
print('No executable skill module found')
        return await self._execute_code(code, skill_files, executor, generated_files)
    async def _execute_code_loop(
        Execute the code execution loop until model gives final response.
        # Keys to exclude when passing through to acompletion
        # These are either handled explicitly or are internal LiteLLM fields
        _EXCLUDED_ACOMPLETION_KEYS = frozenset({
            "messages",
            "litellm_metadata",
            "container",
            k: v for k, v in data.items() 
            if k not in _EXCLUDED_ACOMPLETION_KEYS
        current_response: Any = response
            # OpenAI format response has choices[0].message
            assistant_message = current_response.choices[0].message  # type: ignore[union-attr]
            stop_reason = current_response.choices[0].finish_reason  # type: ignore[union-attr]
            # Build assistant message for conversation history
            assistant_msg_dict: Dict[str, Any] = {
                "content": assistant_message.content,
            if assistant_message.tool_calls:
                assistant_msg_dict["tool_calls"] = [
                            "arguments": tc.function.arguments
                    for tc in assistant_message.tool_calls
            messages.append(assistant_msg_dict)
            if stop_reason != "tool_calls" or not assistant_message.tool_calls:
                    f"SkillsInjectionHook: Code execution loop completed after "
                    f"{iteration + 1} iterations, {len(generated_files)} files generated"
                # Attach generated files to response
            for tool_call in assistant_message.tool_calls:
                tool_name = tool_call.function.name
                    tool_result = await self._execute_code_tool(
                        tool_call=tool_call,
                        skill_files=skill_files,
                        executor=executor,
                        generated_files=generated_files,
                    # Non-code-execution tool - cannot handle
                    tool_result = f"Tool '{tool_name}' not handled automatically"
                messages.append({
                    "tool_call_id": tool_call.id,
                    "content": tool_result,
            # Make next LLM call using the messages API
                max_tokens=kwargs.get("max_tokens", 4096),
        # Max iterations reached
    async def _execute_code_tool(
        tool_call: Any,
        """Execute a litellm_code_execution tool call and return result string."""
            args = json.loads(tool_call.function.arguments)
            code = args.get("code", "")
                f"SkillsInjectionHook: Executing code ({len(code)} chars)"
            exec_result = executor.execute(
            # Build tool result content
            tool_result = exec_result.get("output", "") or ""
                tool_result += "\n\nGenerated files:"
                    file_content = base64.b64decode(f["content_base64"])
                        "size": len(file_content),
                    tool_result += f"\n- {f['name']} ({len(file_content)} bytes)"
                        f"SkillsInjectionHook: Generated file {f['name']} "
                        f"({len(file_content)} bytes)"
                tool_result += f"\n\nError:\n{exec_result['error']}"
                f"SkillsInjectionHook: Code execution failed: {e}"
    def _attach_files_to_response(
        Attach generated files to the response object.
        Files are added to response._litellm_generated_files for easy access.
        For dict responses, files are added as a key.
        if not generated_files:
        # Handle dict response (Anthropic/messages API format)
            response["_litellm_generated_files"] = generated_files
                f"SkillsInjectionHook: Attached {len(generated_files)} files to dict response"
        # Handle object response (OpenAI format)
            response._litellm_generated_files = generated_files
        # Also add to model_extra if available (for serialization)
        if hasattr(response, "model_extra"):
            if response.model_extra is None:
                response.model_extra = {}
            response.model_extra["_litellm_generated_files"] = generated_files
            f"SkillsInjectionHook: Attached {len(generated_files)} files to response"
# Global instance for registration
skills_injection_hook = SkillsInjectionHook()
litellm.logging_callback_manager.add_litellm_callback(skills_injection_hook)
RAG Ingest API for LiteLLM.
from litellm.rag.ingestion.base_ingestion import BaseRAGIngestion
from litellm.rag.ingestion.bedrock_ingestion import BedrockRAGIngestion
from litellm.rag.ingestion.gemini_ingestion import GeminiRAGIngestion
from litellm.rag.ingestion.openai_ingestion import OpenAIRAGIngestion
from litellm.rag.ingestion.s3_vectors_ingestion import S3VectorsRAGIngestion
from litellm.rag.rag_query import RAGQuery
from litellm.types.rag import (
    RAGIngestOptions,
    RAGIngestResponse,
from litellm.types.utils import ModelResponse
# Registry of provider-specific ingestion classes
INGESTION_REGISTRY: Dict[str, Type[BaseRAGIngestion]] = {
    "openai": OpenAIRAGIngestion,
    "bedrock": BedrockRAGIngestion,
    "gemini": GeminiRAGIngestion,
    "s3_vectors": S3VectorsRAGIngestion,
def get_ingestion_class(provider: str) -> Type[BaseRAGIngestion]:
    Get the ingestion class for a given provider.
        provider: The vector store provider name (e.g., 'openai')
        The ingestion class for the provider
        ValueError: If provider is not supported
    ingestion_class = INGESTION_REGISTRY.get(provider)
    if ingestion_class is None:
        supported = ", ".join(INGESTION_REGISTRY.keys())
            f"Provider '{provider}' is not supported for RAG ingestion. "
            f"Supported providers: {supported}"
    return ingestion_class
async def _execute_ingest_pipeline(
    ingest_options: RAGIngestOptions,
    file_data: Optional[Tuple[str, bytes, str]] = None,
    file_url: Optional[str] = None,
    file_id: Optional[str] = None,
    router: Optional["Router"] = None,
) -> RAGIngestResponse:
    Execute the RAG ingest pipeline using provider-specific implementation.
        ingest_options: Configuration for the ingest pipeline
        file_data: Tuple of (filename, content_bytes, content_type)
        file_url: URL to fetch file from
        file_id: Existing file ID to use
        router: Optional LiteLLM router for load balancing
        RAGIngestResponse with status and IDs
    # Get provider from vector store config
    vector_store_config = ingest_options.get("vector_store") or {}
    provider = vector_store_config.get("custom_llm_provider", "openai")
    # Get provider-specific ingestion class
    ingestion_class = get_ingestion_class(provider)
    # Create ingestion instance
    ingestion = ingestion_class(
        ingest_options=ingest_options,
        router=router,
    # Execute ingestion pipeline
    return await ingestion.ingest(
        file_data=file_data,
        file_url=file_url,
####### PUBLIC API ###################
async def aingest(
    ingest_options: Dict[str, Any],
    file: Optional[Dict[str, str]] = None,
    Async: Ingest a document into a vector store.
        file: Dict with {filename, content (base64), content_type} - for JSON API
        response = await litellm.aingest(
            ingest_options={
                "vector_store": {
                    "custom_llm_provider": "openai",
                    "litellm_credential_name": "my-openai-creds",  # optional
            file_url="https://example.com/doc.pdf",
        kwargs["aingest"] = True
            ingest,
            custom_llm_provider=ingest_options.get("vector_store", {}).get("custom_llm_provider"),
async def _execute_query_pipeline(
    messages: List[Any],
    retrieval_config: Dict[str, Any],
    rerank: Optional[Dict[str, Any]] = None,
) -> ModelResponse:
    Execute the RAG query pipeline.
    # Extract router from kwargs - use it for completion if available
    # to properly resolve virtual model names
    router: Optional["Router"] = kwargs.pop("router", None)
    # 1. Extract query from last user message
    query_text = RAGQuery.extract_query_from_messages(messages)
    if not query_text:
        raise ValueError("No query found in messages for RAG query")
    # 2. Search vector store
    search_response = await litellm.vector_stores.asearch(
        vector_store_id=retrieval_config["vector_store_id"],
        query=query_text,
        max_num_results=retrieval_config.get("top_k", 10),
        custom_llm_provider=retrieval_config.get("custom_llm_provider", "openai"),
    rerank_response = None
    context_chunks = search_response.get("data", [])
    # 3. Optional rerank
    if rerank and rerank.get("enabled"):
        documents = RAGQuery.extract_documents_from_search(search_response)
        if documents:
            rerank_response = await litellm.arerank(
                model=rerank["model"],
                top_n=rerank.get("top_n", 5),
            context_chunks = RAGQuery.get_top_chunks_from_rerank(
                search_response, rerank_response
    # 4. Build context message and call completion
    context_message = RAGQuery.build_context_message(context_chunks)
    modified_messages = messages[:-1] + [context_message] + [messages[-1]]
    # Use router if available to properly resolve virtual model names
    if router is not None:
        response = await router.acompletion(
            messages=modified_messages,
    # 5. Attach search results to response
    if not stream and isinstance(response, ModelResponse):
        response = RAGQuery.add_search_results_to_response(
            search_results=search_response,
            rerank_results=rerank_response,
    return response  # type: ignore[return-value]
async def aquery(
    Async: Query a RAG pipeline.
        kwargs["aquery"] = True
            retrieval_config=retrieval_config,
            rerank=rerank,
            custom_llm_provider=retrieval_config.get("custom_llm_provider"),
def query(
) -> Union[ModelResponse, Coroutine[Any, Any, ModelResponse]]:
    Query a RAG pipeline.
        _is_async = kwargs.pop("aquery", False) is True
            return _execute_query_pipeline(
            return asyncio.get_event_loop().run_until_complete(
                _execute_query_pipeline(
def ingest(
) -> Union[RAGIngestResponse, Coroutine[Any, Any, RAGIngestResponse]]:
    Ingest a document into a vector store.
        response = litellm.ingest(
            file_data=("doc.txt", b"Hello world", "text/plain"),
        _is_async = kwargs.pop("aingest", False) is True
        router: Optional["Router"] = kwargs.get("router")
        # Convert file dict to file_data tuple if provided
        if file is not None and file_data is None:
            filename = file.get("filename", "document")
            content_b64 = file.get("content", "")
            content_type = file.get("content_type", "application/octet-stream")
            content_bytes = base64.b64decode(content_b64)
            file_data = (filename, content_bytes, content_type)
            return _execute_ingest_pipeline(
                ingest_options=ingest_options,  # type: ignore
                _execute_ingest_pipeline(
"""Abstraction function for OpenAI's realtime API"""
from typing import Any, Optional, cast
from litellm.constants import REALTIME_WEBSOCKET_MAX_MESSAGE_SIZE_BYTES
from litellm.types.realtime import RealtimeQueryParams
from ..litellm_core_utils.get_litellm_params import get_litellm_params
from ..litellm_core_utils.litellm_logging import Logging as LiteLLMLogging
from ..llms.azure.realtime.handler import AzureOpenAIRealtime
from ..llms.bedrock.realtime.handler import BedrockRealtime
from ..llms.custom_httpx.http_handler import get_shared_realtime_ssl_context
from ..llms.openai.realtime.handler import OpenAIRealtime
from ..llms.xai.realtime.handler import XAIRealtime
from ..utils import client as wrapper_client
azure_realtime = AzureOpenAIRealtime()
openai_realtime = OpenAIRealtime()
bedrock_realtime = BedrockRealtime()
xai_realtime = XAIRealtime()
@wrapper_client
async def _arealtime(
    websocket: Any,  # fastapi websocket
    query_params: Optional[RealtimeQueryParams] = None,
    Private function to handle the realtime API call.
    For PROXY use only.
    headers = cast(Optional[dict], kwargs.get("headers"))
    extra_headers = cast(Optional[dict], kwargs.get("extra_headers"))
    litellm_logging_obj: LiteLLMLogging = kwargs.get("litellm_logging_obj")  # type: ignore
    model, _custom_llm_provider, dynamic_api_key, dynamic_api_base = get_llm_provider(
    # Ensure query params use the normalized provider model (no proxy aliases).
    if query_params is not None:
        query_params = {**query_params, "model": model}
        custom_llm_provider=_custom_llm_provider,
    provider_config: Optional[BaseRealtimeConfig] = None
    if _custom_llm_provider in LlmProviders._member_map_.values():
        provider_config = ProviderConfigManager.get_provider_realtime_config(
            provider=LlmProviders(_custom_llm_provider),
        await base_llm_http_handler.async_realtime(
            websocket=websocket,
    elif _custom_llm_provider == "azure":
            dynamic_api_base
            or litellm_params.api_base
            dynamic_api_key
            or litellm_params.api_version
            or "2024-10-01-preview"
        realtime_protocol = (
            kwargs.get("realtime_protocol")
            or "beta"
        await azure_realtime.async_realtime(
            realtime_protocol=realtime_protocol,
    elif _custom_llm_provider == "openai":
            or "https://api.openai.com/"
        await openai_realtime.async_realtime(
    elif _custom_llm_provider == "bedrock":
        # Extract AWS parameters from kwargs
        aws_region_name = kwargs.get("aws_region_name")
        aws_access_key_id = kwargs.get("aws_access_key_id")
        aws_secret_access_key = kwargs.get("aws_secret_access_key")
        aws_session_token = kwargs.get("aws_session_token")
        aws_role_name = kwargs.get("aws_role_name")
        aws_session_name = kwargs.get("aws_session_name")
        aws_profile_name = kwargs.get("aws_profile_name")
        aws_web_identity_token = kwargs.get("aws_web_identity_token")
        aws_sts_endpoint = kwargs.get("aws_sts_endpoint")
        aws_bedrock_runtime_endpoint = kwargs.get("aws_bedrock_runtime_endpoint")
        aws_external_id = kwargs.get("aws_external_id")
        await bedrock_realtime.async_realtime(
            api_base=dynamic_api_base or api_base,
            api_key=dynamic_api_key or api_key,
            aws_bedrock_runtime_endpoint=aws_bedrock_runtime_endpoint,
            aws_external_id=aws_external_id,
    elif _custom_llm_provider == "xai":
            or get_secret_str("XAI_API_BASE")
            or "https://api.x.ai/v1"
            or get_secret_str("XAI_API_KEY")
        await xai_realtime.async_realtime(
async def _realtime_health_check(
    realtime_protocol: Optional[str] = None,
    Health check for realtime API - tries connection to the realtime API websocket
        model: str - model name
        api_base: str - api base
        api_version: Optional[str] - api version
        api_key: str - api key
        custom_llm_provider: str - custom llm provider
        realtime_protocol: Optional[str] - protocol version ("GA"/"v1" for GA path, "beta"/None for beta path)
        bool - True if connection is successful, False otherwise
        Exception - if the connection is not successful
        url = azure_realtime._construct_url(
            api_base=api_base or "",
            api_version=api_version or "2024-10-01-preview",
        url = openai_realtime._construct_url(
            api_base=api_base or "https://api.openai.com/", query_params={"model": model}
        url = xai_realtime._construct_url(
            api_base=api_base or "https://api.x.ai/v1", query_params={"model": model}
    ssl_context = get_shared_realtime_ssl_context()
    async with websockets.connect(  # type: ignore
        additional_headers={
            "api-key": api_key,  # type: ignore
        max_size=REALTIME_WEBSOCKET_MAX_MESSAGE_SIZE_BYTES,
        ssl=ssl_context,
from typing import Any, Coroutine, Dict, List, Literal, Optional, Union
from litellm.llms.bedrock.rerank.handler import BedrockRerankHandler
from litellm.llms.together_ai.rerank.handler import TogetherAIRerank
from litellm.rerank_api.rerank_utils import get_optional_rerank_params
from litellm.secret_managers.main import get_secret, get_secret_str
from litellm.utils import ProviderConfigManager, client, exception_type
together_rerank = TogetherAIRerank()
bedrock_rerank = BedrockRerankHandler()
async def arerank(
    documents: List[Union[str, Dict[str, Any]]],
    custom_llm_provider: Optional[Literal["cohere", "together_ai", "deepinfra", "fireworks_ai", "voyage"]] = None,
    top_n: Optional[int] = None,
    rank_fields: Optional[List[str]] = None,
    return_documents: Optional[bool] = None,
    max_chunks_per_doc: Optional[int] = None,
) -> Union[RerankResponse, Coroutine[Any, Any, RerankResponse]]:
        kwargs["arerank"] = True
            rerank,
            top_n,
            rank_fields,
            return_documents,
            max_chunks_per_doc,
def rerank(  # noqa: PLR0915
            "together_ai",
            "infinity",
            "litellm_proxy",
            "hosted_vllm",
            "deepinfra",
            "fireworks_ai",
            "voyage",
    return_documents: Optional[bool] = True,
    max_tokens_per_doc: Optional[int] = None,
    Reranks a list of documents based on their relevance to the query
    headers: Optional[dict] = kwargs.get("headers")  # type: ignore
        _is_async = kwargs.pop("arerank", False) is True
        # Params that are unique to specific versions of the client for the rerank call
        unique_version_params = {
            "max_chunks_per_doc": max_chunks_per_doc,
            "max_tokens_per_doc": max_tokens_per_doc,
        present_version_params = [
            k for k, v in unique_version_params.items() if v is not None
            _custom_llm_provider,
        rerank_provider_config: BaseRerankConfig = (
            ProviderConfigManager.get_provider_rerank_config(
                provider=litellm.LlmProviders(_custom_llm_provider),
                present_version_params=present_version_params,
        optional_rerank_params: Dict = get_optional_rerank_params(
            rerank_provider_config=rerank_provider_config,
            drop_params=kwargs.get("drop_params") or litellm.drop_params or False,
            top_n=top_n,
            rank_fields=rank_fields,
            return_documents=return_documents,
            max_chunks_per_doc=max_chunks_per_doc,
            max_tokens_per_doc=max_tokens_per_doc,
        verbose_logger.info(f"optional_rerank_params: {optional_rerank_params}")
        if isinstance(optional_params.timeout, str):
            optional_params.timeout = float(optional_params.timeout)
        model_response = RerankResponse()
            optional_params=dict(optional_rerank_params),
        # Implement rerank logic here based on the custom_llm_provider
        if _custom_llm_provider == litellm.LlmProviders.COHERE or _custom_llm_provider == litellm.LlmProviders.LITELLM_PROXY:
            # Implement Cohere rerank logic
            api_key: Optional[str] = (
                dynamic_api_key or optional_params.api_key or litellm.api_key
            api_base: Optional[str] = (
                or optional_params.api_base
                or get_secret("COHERE_API_BASE")  # type: ignore
                or "https://api.cohere.com"
                    "Invalid api base. api_base=None. Set in call or via `COHERE_API_BASE` env var."
            response = base_llm_http_handler.rerank(
                provider_config=rerank_provider_config,
                optional_rerank_params=optional_rerank_params,
                timeout=optional_params.timeout,
                headers=headers or litellm.headers or {},
        elif _custom_llm_provider == litellm.LlmProviders.AZURE_AI:
                dynamic_api_base  # for deepinfra/perplexity/anyscale/groq/friendliai we check in get_llm_provider and pass in the api base from there
                or get_secret("AZURE_AI_API_BASE")  # type: ignore
                api_key=dynamic_api_key or optional_params.api_key,
        elif _custom_llm_provider == litellm.LlmProviders.INFINITY:
            # Implement Infinity rerank logic
            api_key = dynamic_api_key or optional_params.api_key or litellm.api_key
                or get_secret_str("INFINITY_API_BASE")
                    "Invalid api base. api_base=None. Set in call or via `INFINITY_API_BASE` env var."
        elif _custom_llm_provider == litellm.LlmProviders.TOGETHER_AI:
            # Implement Together AI rerank logic
                or optional_params.api_key
                or get_secret("TOGETHERAI_API_KEY")  # type: ignore
                    "TogetherAI API key is required, please set 'TOGETHERAI_API_KEY' in your environment"
            response = together_rerank.rerank(
        elif _custom_llm_provider == litellm.LlmProviders.JINA_AI:
            if dynamic_api_key is None:
                    "Jina AI API key is required, please set 'JINA_AI_API_KEY' in your environment"
                or get_secret("BEDROCK_API_BASE")  # type: ignore
        elif _custom_llm_provider == litellm.LlmProviders.NVIDIA_NIM:
                    "Nvidia NIM API key is required, please set 'NVIDIA_NIM_API_KEY' in your environment"
            # Note: For rerank, the base URL is different from chat/embeddings
            # Rerank uses ai.api.nvidia.com instead of integrate.api.nvidia.com
                or get_secret("NVIDIA_NIM_API_BASE")  # type: ignore
                or "https://ai.api.nvidia.com"  # Default for rerank
        elif _custom_llm_provider == litellm.LlmProviders.BEDROCK:
            # Merge headers and extra_headers if both are provided
            merged_headers = headers or litellm.headers or {}
            extra_headers_from_kwargs = kwargs.get("extra_headers")
            if extra_headers_from_kwargs:
                merged_headers = {**merged_headers, **extra_headers_from_kwargs}
            response = bedrock_rerank.rerank(
                optional_params=optional_params.model_dump(exclude_unset=True),
                extra_headers=merged_headers,
        elif _custom_llm_provider == litellm.LlmProviders.HOSTED_VLLM:
            # Implement Hosted VLLM rerank logic
                or get_secret_str("HOSTED_VLLM_API_KEY")
                or get_secret_str("HOSTED_VLLM_API_BASE")
                    "api_base must be provided for Hosted VLLM rerank. Set in call or via HOSTED_VLLM_API_BASE env var."
        elif _custom_llm_provider == litellm.LlmProviders.DEEPINFRA:
                or get_secret_str("DEEPINFRA_API_KEY")
                or get_secret_str("DEEPINFRA_API_BASE")
                    "api_base must be provided for Deepinfra rerank. Set in call or via DEEPINFRA_API_BASE env var."
        elif _custom_llm_provider == litellm.LlmProviders.FIREWORKS_AI:
                or get_secret_str("FIREWORKS_API_KEY")
                or get_secret_str("FIREWORKS_AI_API_KEY")
                or get_secret_str("FIREWORKSAI_API_KEY")
                or get_secret_str("FIREWORKS_AI_TOKEN")
                or get_secret_str("FIREWORKS_AI_API_BASE")
        elif _custom_llm_provider == litellm.LlmProviders.VOYAGE:
                or get_secret_str("VOYAGE_API_KEY")
                or get_secret_str("VOYAGE_AI_API_KEY")
                or get_secret_str("VOYAGE_API_BASE")
            # Generic handler for all providers that use base_llm_http_handler
            # Provider-specific logic (API key validation, URL generation, etc.)
            # is handled in the respective transformation configs
            # Check if the provider is actually supported
            # If rerank_provider_config is a default CohereRerankConfig but the provider is not Cohere or litellm_proxy,
            # it means the provider is not supported
                    isinstance(rerank_provider_config, litellm.CohereRerankConfig)
                    or isinstance(rerank_provider_config, litellm.CohereRerankV2Config)
                and _custom_llm_provider != "cohere"
                and _custom_llm_provider != "litellm_proxy"
                raise ValueError(f"Unsupported provider: {_custom_llm_provider}")
                api_base=dynamic_api_base or optional_params.api_base,
        # Placeholder return
        verbose_logger.error(f"Error in rerank: {str(e)}")
            model=model, custom_llm_provider=custom_llm_provider, original_exception=e
    update_responses_input_with_model_file_ids,
    update_responses_tools_with_model_file_ids,
from litellm.responses.litellm_completion_transformation.handler import (
    LiteLLMCompletionTransformationHandler,
from litellm.responses.utils import ResponsesAPIRequestUtils
    PromptObject,
    Reasoning,
    ResponseIncludable,
    ResponsesAPIOptionalRequestParams,
    ToolChoice,
# Handle ResponseText import with fallback
    from litellm.types.llms.openai import ResponseText  # type: ignore
    ResponseText = str  # Fallback for ResponseText import
from litellm.types.responses.main import *
    MCPTool = Any
from .streaming_iterator import BaseResponsesAPIStreamingIterator
litellm_completion_transformation_handler = LiteLLMCompletionTransformationHandler()
def mock_responses_api_response(
    mock_response: str = "In a peaceful grove beneath a silver moon, a unicorn named Lumina discovered a hidden pool that reflected the stars. As she dipped her horn into the water, the pool began to shimmer, revealing a pathway to a magical realm of endless night skies. Filled with wonder, Lumina whispered a wish for all who dream to find their own hidden magic, and as she glanced back, her hoofprints sparkled like stardust.",
    return ResponsesAPIResponse(
        **{  # type: ignore
            "id": "resp_67ccd2bed1ec8190b14f964abc0542670bb6a6b452d3795b",
            "object": "response",
            "created_at": 1741476542,
            "error": None,
            "incomplete_details": None,
            "instructions": None,
            "max_output_tokens": None,
            "model": "gpt-4.1-2025-04-14",
            "output": [
                    "id": "msg_67ccd2bf17f0819081ff3bb2cf6508e60bb6a6b452d3795b",
            "parallel_tool_calls": True,
            "previous_response_id": None,
            "reasoning": {"effort": None, "summary": None},
            "store": True,
            "temperature": 1.0,
            "text": {"format": {"type": "text"}},
            "tool_choice": "auto",
            "tools": [],
            "top_p": 1.0,
            "truncation": "disabled",
                "input_tokens": 36,
                "input_tokens_details": {"cached_tokens": 0},
                "output_tokens": 87,
                "output_tokens_details": {"reasoning_tokens": 0},
                "total_tokens": 123,
async def aresponses_api_with_mcp(
    include: Optional[List[ResponseIncludable]] = None,
    max_output_tokens: Optional[int] = None,
    prompt: Optional[PromptObject] = None,
    previous_response_id: Optional[str] = None,
    reasoning: Optional[Reasoning] = None,
    text: Optional["ResponseText"] = None,
    tool_choice: Optional[ToolChoice] = None,
    tools: Optional[Iterable[ToolParam]] = None,
    truncation: Optional[Literal["auto", "disabled"]] = None,
) -> Union[ResponsesAPIResponse, BaseResponsesAPIStreamingIterator]:
    Async version of responses API with MCP integration.
    When MCP tools with server_url="litellm_proxy" are provided, this function will:
    1. Get available tools from the MCP server manager
    2. Insert the tools into the messages/input
    3. Call the standard responses API
    4. If require_approval="never" and tool calls are returned, automatically execute them
    # Parse MCP tools and separate from other tools
        mcp_tools_with_litellm_proxy,
        other_tools,
    ) = LiteLLM_Proxy_MCP_Handler._parse_mcp_tools(tools)
    # Process MCP tools through the complete pipeline (fetch + filter + deduplicate + transform)
    # Extract user_api_key_auth from litellm_metadata (where it's added by add_user_api_key_auth_to_request_metadata)
    user_api_key_auth = kwargs.get("user_api_key_auth") or kwargs.get(
        "litellm_metadata", {}
    ).get("user_api_key_auth")
    # Get original MCP tools (for events) and OpenAI tools (for LLM) by reusing existing methods
        original_mcp_tools,
        tool_server_map,
    ) = await LiteLLM_Proxy_MCP_Handler._process_mcp_tools_without_openai_transform(
        user_api_key_auth=user_api_key_auth,
        mcp_tools_with_litellm_proxy=mcp_tools_with_litellm_proxy,
    openai_tools = LiteLLM_Proxy_MCP_Handler._transform_mcp_tools_to_openai(
        original_mcp_tools
    # Combine with other tools
    all_tools = openai_tools + other_tools if (openai_tools or other_tools) else None
    # Prepare call parameters for reuse
    call_params = {
        "extra_query": extra_query,
    # Handle MCP streaming if requested
    if stream and mcp_tools_with_litellm_proxy:
        # Generate MCP discovery events using the already processed tools
        from litellm.responses.mcp.mcp_streaming_iterator import (
            create_mcp_list_tools_events,
        base_item_id = f"mcp_{uuid.uuid4().hex[:8]}"
        mcp_discovery_events = await create_mcp_list_tools_events(
            base_item_id=base_item_id,
            pre_processed_mcp_tools=original_mcp_tools,
        return LiteLLM_Proxy_MCP_Handler._create_mcp_streaming_response(
            all_tools=all_tools,
            mcp_discovery_events=mcp_discovery_events,
            call_params=call_params,
            tool_server_map=tool_server_map,
    # Determine if we should auto-execute tools
    should_auto_execute = bool(
        mcp_tools_with_litellm_proxy
    ) and LiteLLM_Proxy_MCP_Handler._should_auto_execute_tools(
        mcp_tools_with_litellm_proxy=mcp_tools_with_litellm_proxy
    # Prepare parameters for the initial call
    initial_call_params = LiteLLM_Proxy_MCP_Handler._prepare_initial_call_params(
        call_params=call_params, should_auto_execute=should_auto_execute
    # Make initial response API call
    response = await aresponses(
        tools=all_tools,
        **initial_call_params,
    verbose_logger.debug("Initial response %s", response)
    # Auto-Execute Tools Handling
    # If auto-execute tools is True, then we need to execute the tool calls
    if should_auto_execute and isinstance(
        response, ResponsesAPIResponse
    ):  # type: ignore
        tool_calls = LiteLLM_Proxy_MCP_Handler._extract_tool_calls_from_response(
        if tool_calls:
            user_api_key_auth = kwargs.get("litellm_metadata", {}).get(
                "user_api_key_auth"
            # Extract MCP auth headers from the request to pass to MCP server
            secret_fields: Optional[Dict[str, Any]] = kwargs.get("secret_fields")
                mcp_auth_header,
                mcp_server_auth_headers,
                oauth2_headers,
                raw_headers_from_request,
            ) = ResponsesAPIRequestUtils.extract_mcp_headers_from_request(
                secret_fields=secret_fields,
            tool_results = await LiteLLM_Proxy_MCP_Handler._execute_tool_calls(
                mcp_auth_header=mcp_auth_header,
                mcp_server_auth_headers=mcp_server_auth_headers,
                oauth2_headers=oauth2_headers,
                raw_headers=raw_headers_from_request,
                litellm_call_id=kwargs.get("litellm_call_id"),
            if tool_results:
                follow_up_input = LiteLLM_Proxy_MCP_Handler._create_follow_up_input(
                    response=response, tool_results=tool_results, original_input=input
                # Prepare parameters for follow-up call (restores original stream setting)
                follow_up_call_params = (
                    LiteLLM_Proxy_MCP_Handler._prepare_follow_up_call_params(
                        call_params=call_params, original_stream_setting=stream or False
                # Create tool execution events for streaming if needed
                tool_execution_events = []
                    tool_execution_events = (
                        LiteLLM_Proxy_MCP_Handler._create_tool_execution_events(
                            tool_calls=tool_calls, tool_results=tool_results
                final_response = await LiteLLM_Proxy_MCP_Handler._make_follow_up_call(
                    follow_up_input=follow_up_input,
                    response_id=response.id,
                    **follow_up_call_params,
                # If streaming and we have tool execution events, wrap the response
                    and tool_execution_events
                        hasattr(final_response, "__aiter__")
                        or hasattr(final_response, "__iter__")
                        MCPEnhancedStreamingIterator,
                    final_response = MCPEnhancedStreamingIterator(
                        base_iterator=final_response,
                        mcp_events=tool_execution_events,
                # Add custom output elements to the final response (for non-streaming)
                elif isinstance(final_response, ResponsesAPIResponse):
                    # Fetch MCP tools again for output elements (without OpenAI transformation)
                        mcp_tools_for_output,
                    final_response = (
                        LiteLLM_Proxy_MCP_Handler._add_mcp_output_elements_to_response(
                            response=final_response,
                            mcp_tools_fetched=mcp_tools_for_output,
                            tool_results=tool_results,
                return final_response
async def aresponses(
    text_format: Optional[Union[Type["BaseModel"], dict]] = None,
    Async: Handles responses API requests by reusing the synchronous function
        kwargs["aresponses"] = True
        # Convert text_format to text parameter if provided
        text = ResponsesAPIRequestUtils.convert_text_format_to_text_param(
            text_format=text_format, text=text
            # Update local_vars to include the converted text parameter
            local_vars["text"] = text
            # Update local_vars with detected provider (fixes #19782)
            local_vars["custom_llm_provider"] = custom_llm_provider
        # Update the responses_api_response_id with the model_id
        if isinstance(response, ResponsesAPIResponse):
            response = ResponsesAPIRequestUtils._update_responses_api_response_id_with_model_id(
                responses_api_response=response,
                f"Got an unexpected None response from the Responses API: {response}"
def responses(
    Synchronous version of the Responses API.
    Uses the synchronous HTTP handler to make requests.
        _is_async = kwargs.pop("aresponses", False) is True
        # MOCK RESPONSE LOGIC
            return mock_responses_api_response(
        # Use dynamic credentials from get_llm_provider (e.g., when use_litellm_proxy=True)
            litellm_params.api_key = dynamic_api_key
        if dynamic_api_base is not None:
            litellm_params.api_base = dynamic_api_base
        # Update input and tools with provider-specific file IDs if managed files are used
        model_file_id_mapping = kwargs.get("model_file_id_mapping")
        model_info_id = kwargs.get("model_info", {}).get("id") if isinstance(kwargs.get("model_info"), dict) else None
        input = cast(
            Union[str, ResponseInputParam],
            update_responses_input_with_model_file_ids(
                model_id=model_info_id,
                model_file_id_mapping=model_file_id_mapping,
        local_vars["input"] = input
        # Update tools with provider-specific file IDs if needed
            tools = cast(
                Optional[Iterable[ToolParam]],
                update_responses_tools_with_model_file_ids(
                    tools=cast(Optional[List[Dict[str, Any]]], tools),
            local_vars["tools"] = tools
        # Native MCP Responses API
        if LiteLLM_Proxy_MCP_Handler._should_use_litellm_mcp_gateway(tools=tools):
            return aresponses_api_with_mcp(
        responses_api_provider_config: Optional[
            BaseResponsesAPIConfig
        ] = ProviderConfigManager.get_provider_responses_api_config(
        # Get ResponsesAPIOptionalRequestParams with only valid parameters
        response_api_optional_params: ResponsesAPIOptionalRequestParams = (
            ResponsesAPIRequestUtils.get_requested_response_api_optional_param(
        if responses_api_provider_config is None:
            return litellm_completion_transformation_handler.response_api_handler(
                responses_api_request=response_api_optional_params,
        responses_api_request_params: Dict = (
            ResponsesAPIRequestUtils.get_optional_params_responses_api(
                responses_api_provider_config=responses_api_provider_config,
                response_api_optional_params=response_api_optional_params,
            optional_params=dict(responses_api_request_params),
                **responses_api_request_params,
                "aresponses": _is_async,
        response = base_llm_http_handler.response_api_handler(
            response_api_optional_request_params=responses_api_request_params,
            fake_stream=responses_api_provider_config.should_fake_stream(
                model=model, stream=stream, custom_llm_provider=custom_llm_provider
            shared_session=kwargs.get("shared_session"),
async def adelete_responses(
) -> DeleteResponseResult:
    Async version of the DELETE Responses API
    DELETE /v1/responses/{response_id} endpoint in the responses API
        kwargs["adelete_responses"] = True
        # get custom llm provider from response_id
        decoded_response_id: DecodedResponseId = (
            ResponsesAPIRequestUtils._decode_responses_api_response_id(
        response_id = decoded_response_id.get("response_id") or response_id
        custom_llm_provider = (
            decoded_response_id.get("custom_llm_provider") or custom_llm_provider
            delete_responses,
def delete_responses(
) -> Union[DeleteResponseResult, Coroutine[Any, Any, DeleteResponseResult]]:
    Synchronous version of the DELETE Responses API
        _is_async = kwargs.pop("adelete_responses", False) is True
            raise ValueError("custom_llm_provider is required but passed as None")
                f"DELETE responses is not supported for {custom_llm_provider}"
            optional_params={
                "response_id": response_id,
        response = base_llm_http_handler.delete_response_api_handler(
async def aget_responses(
) -> ResponsesAPIResponse:
    Async: Fetch a response by its ID.
    GET /v1/responses/{response_id} endpoint in the responses API
        response_id: The ID of the response to fetch.
        custom_llm_provider: Optional provider name. If not specified, will be decoded from response_id.
        The response object with complete information about the stored response.
        kwargs["aget_responses"] = True
            get_responses,
def get_responses(
) -> Union[ResponsesAPIResponse, Coroutine[Any, Any, ResponsesAPIResponse]]:
    Fetch a response by its ID.
        _is_async = kwargs.pop("aget_responses", False) is True
                f"GET responses is not supported for {custom_llm_provider}"
        response = base_llm_http_handler.get_responses(
async def alist_input_items(
    before: Optional[str] = None,
    include: Optional[List[str]] = None,
    limit: int = 20,
    order: Literal["asc", "desc"] = "desc",
    """Async: List input items for a response"""
        kwargs["alist_input_items"] = True
        decoded_response_id = (
                response_id=response_id
            list_input_items,
            before=before,
def list_input_items(
) -> Union[Dict, Coroutine[Any, Any, Dict]]:
    """List input items for a response"""
        _is_async = kwargs.pop("alist_input_items", False) is True
                f"list_input_items is not supported for {custom_llm_provider}"
            optional_params={"response_id": response_id},
        response = base_llm_http_handler.list_responses_input_items(
async def acancel_responses(
    Async version of the POST Cancel Responses API
    POST /v1/responses/{response_id}/cancel endpoint in the responses API
        kwargs["acancel_responses"] = True
            cancel_responses,
def cancel_responses(
    Synchronous version of the POST Responses API
        _is_async = kwargs.pop("acancel_responses", False) is True
                f"CANCEL responses is not supported for {custom_llm_provider}"
        response = base_llm_http_handler.cancel_response_api_handler(
async def acompact_responses(
    Async version of the POST Compact Responses API
    POST /v1/responses/compact endpoint in the responses API
    Runs a compaction pass over a conversation, returning encrypted, opaque items.
        kwargs["acompact_responses"] = True
            compact_responses,
def compact_responses(
    Synchronous version of the POST Compact Responses API
        _is_async = kwargs.pop("acompact_responses", False) is True
                f"COMPACT responses is not supported for {custom_llm_provider}"
        # Build optional params for compact endpoint
                allowed_openai_params=None,
        response = base_llm_http_handler.compact_response_api_handler(
Main Search function for LiteLLM.
from typing import Any, Coroutine, Dict, List, Optional, Union
from litellm.llms.base_llm.search.transformation import BaseSearchConfig, SearchResponse
from litellm.types.utils import SearchProviders
from litellm.utils import ProviderConfigManager, client, filter_out_litellm_params
def _build_search_optional_params(
    max_results: Optional[int] = None,
    search_domain_filter: Optional[List[str]] = None,
    max_tokens_per_page: Optional[int] = None,
    country: Optional[str] = None,
    Helper function to build optional_params dict from Perplexity Search API parameters.
        max_results: Maximum number of results (1-20)
        search_domain_filter: List of domains to filter (max 20)
        max_tokens_per_page: Max tokens per page
        country: Country code filter
        Dict with non-None optional parameters
        optional_params["max_results"] = max_results
    if search_domain_filter is not None:
        optional_params["search_domain_filter"] = search_domain_filter
    if max_tokens_per_page is not None:
        optional_params["max_tokens_per_page"] = max_tokens_per_page
        optional_params["country"] = country
    query: Union[str, List[str]],
    search_provider: str,
) -> SearchResponse:
    Async Search function.
        query: Search query (string or list of strings)
        search_provider: Provider name (e.g., "perplexity")
        max_results: Optional maximum number of results (1-20), default 10
        search_domain_filter: Optional list of domains to filter (max 20)
        max_tokens_per_page: Optional max tokens per page, default 1024
        country: Optional country code filter (e.g., 'US', 'GB', 'DE')
        SearchResponse with results list following Perplexity format
        # Basic search
        response = await litellm.asearch(
            query="latest AI developments 2024",
            search_provider="perplexity"
        # Search with options
            query="AI developments",
            search_provider="perplexity",
            max_results=10,
            search_domain_filter=["arxiv.org", "nature.com"],
            max_tokens_per_page=1024,
            country="US"
        # Access results
        for result in response.results:
            print(f"{result.title}: {result.url}")
            print(f"Snippet: {result.snippet}")
        kwargs["asearch"] = True
            search_provider=search_provider,
            max_results=max_results,
            search_domain_filter=search_domain_filter,
            max_tokens_per_page=max_tokens_per_page,
            country=country,
                f"Got an unexpected None response from the Search API: {response}"
        model_name = f"{search_provider}/search"
            model=model_name,
            custom_llm_provider=search_provider,
) -> Union[SearchResponse, Coroutine[Any, Any, SearchResponse]]:
    Synchronous Search function.
        response = litellm.search(
        # Multi-query search
            query=["AI developments", "machine learning trends"],
            if result.date:
                print(f"Date: {result.date}")
        _is_async = kwargs.pop("asearch", False) is True
        # Validate query parameter
        if not isinstance(query, (str, list)):
            raise ValueError(f"query must be a string or list of strings, got {type(query)}")
        if isinstance(query, list) and not all(isinstance(q, str) for q in query):
            raise ValueError("All items in query list must be strings")
        search_provider_config: Optional[BaseSearchConfig] = (
            ProviderConfigManager.get_provider_search_config(
                provider=SearchProviders(search_provider),
        if search_provider_config is None:
                f"Search is not supported for provider: {search_provider}"
            f"Search call - provider: {search_provider}"
        # Build optional_params from explicit parameters
        optional_params = _build_search_optional_params(
        # Filter out internal LiteLLM parameters from kwargs
        filtered_kwargs = filter_out_litellm_params(kwargs=kwargs)
        # Add remaining kwargs to optional_params (for provider-specific params)
        for key, value in filtered_kwargs.items():
            if key not in optional_params:
                optional_params[key] = value
        verbose_logger.debug(f"Search optional_params: {optional_params}")
        # Validate environment and get headers
        headers = search_provider_config.validate_environment(
        # Get complete URL
        complete_url = search_provider_config.get_complete_url(
                "api_base": complete_url,
        # Call the handler
        response = base_llm_http_handler.search(
            api_base=complete_url,
            asearch=_is_async,
            provider_config=search_provider_config,
from litellm.llms.custom_httpx.http_handler import HTTPHandler
from litellm.secret_managers.get_azure_ad_token_provider import (
    get_azure_ad_token_provider,
from litellm.secret_managers.secret_manager_handler import get_secret_from_manager
oidc_cache = DualCache()
def _get_oidc_http_handler(timeout: Optional[httpx.Timeout] = None) -> HTTPHandler:
    Factory function to create HTTPHandler for OIDC requests.
    This function can be mocked in tests.
        timeout: Optional timeout for HTTP requests. Defaults to 600.0 seconds with 5.0 connect timeout.
        HTTPHandler instance configured for OIDC requests.
        timeout = httpx.Timeout(timeout=600.0, connect=5.0)
    return HTTPHandler(timeout=timeout)
######### Secret Manager ############################
# checks if user has passed in a secret manager client
# if passed in then checks the secret there
def str_to_bool(value: Optional[str]) -> Optional[bool]:
    Converts a string to a boolean if it's a recognized boolean string.
    Returns None if the string is not a recognized boolean value.
    :param value: The string to be checked.
    :return: True or False if the string is a recognized boolean, otherwise None.
    true_values = {"true"}
    false_values = {"false"}
    value_lower = value.strip().lower()
    if value_lower in true_values:
    elif value_lower in false_values:
def get_secret_str(
    secret_name: str,
    default_value: Optional[Union[str, bool]] = None,
    Guarantees response from 'get_secret' is either string or none. Used for fixing linting errors.
    value = get_secret(secret_name=secret_name, default_value=default_value)
    if value is not None and not isinstance(value, str):
def get_secret_bool(
    default_value: Optional[bool] = None,
    Guarantees response from 'get_secret' is either boolean or none. Used for fixing linting errors.
        secret_name: The name of the secret to get.
        default_value: The default value to return if the secret is not found.
        The secret value as a boolean or None if the secret is not found.
    _secret_value = get_secret(secret_name, default_value)
    if _secret_value is None:
    elif isinstance(_secret_value, bool):
        return _secret_value
        return str_to_bool(_secret_value)
def get_secret(  # noqa: PLR0915
    key_management_system = litellm._key_management_system
    key_management_settings = litellm._key_management_settings
    secret = None
    if secret_name.startswith("os.environ/"):
        secret_name = secret_name.replace("os.environ/", "")
    # Example: oidc/google/https://bedrock-runtime.us-east-1.amazonaws.com/model/stability.stable-diffusion-xl-v1/invoke
    if secret_name.startswith("oidc/"):
        secret_name_split = secret_name.replace("oidc/", "")
        oidc_provider, oidc_aud = secret_name_split.split("/", 1)
        oidc_aud = "/".join(secret_name_split.split("/")[1:])
        # TODO: Add caching for HTTP requests
        if oidc_provider == "google":
            oidc_token = oidc_cache.get_cache(key=secret_name)
            if oidc_token is not None:
                return oidc_token
            oidc_client = _get_oidc_http_handler()
            # https://cloud.google.com/compute/docs/instances/verifying-instance-identity#request_signature
            response = oidc_client.get(
                "http://metadata.google.internal/computeMetadata/v1/instance/service-accounts/default/identity",
                params={"audience": oidc_aud},
                headers={"Metadata-Flavor": "Google"},
                oidc_token = response.text
                oidc_cache.set_cache(key=secret_name, value=oidc_token, ttl=3600 - 60)
                raise ValueError("Google OIDC provider failed")
        elif oidc_provider == "circleci":
            # https://circleci.com/docs/openid-connect-tokens/
            env_secret = os.getenv("CIRCLE_OIDC_TOKEN")
            if env_secret is None:
                raise ValueError("CIRCLE_OIDC_TOKEN not found in environment")
            return env_secret
        elif oidc_provider == "circleci_v2":
            env_secret = os.getenv("CIRCLE_OIDC_TOKEN_V2")
                raise ValueError("CIRCLE_OIDC_TOKEN_V2 not found in environment")
        elif oidc_provider == "github":
            # https://docs.github.com/en/actions/deployment/security-hardening-your-deployments/configuring-openid-connect-in-cloud-providers#using-custom-actions
            actions_id_token_request_url = os.getenv("ACTIONS_ID_TOKEN_REQUEST_URL")
            actions_id_token_request_token = os.getenv("ACTIONS_ID_TOKEN_REQUEST_TOKEN")
            if actions_id_token_request_url is None or actions_id_token_request_token is None:
                    "ACTIONS_ID_TOKEN_REQUEST_URL or ACTIONS_ID_TOKEN_REQUEST_TOKEN not found in environment"
                actions_id_token_request_url,
                    "Authorization": f"Bearer {actions_id_token_request_token}",
                    "Accept": "application/json; api-version=2.0",
                oidc_token = response.json().get("value", None)
                oidc_cache.set_cache(key=secret_name, value=oidc_token, ttl=300 - 5)
                raise ValueError("Github OIDC provider failed")
        elif oidc_provider == "azure":
            # https://azure.github.io/azure-workload-identity/docs/quick-start.html
            azure_federated_token_file = os.getenv("AZURE_FEDERATED_TOKEN_FILE")
            if azure_federated_token_file is None:
                    "AZURE_FEDERATED_TOKEN_FILE not found in environment will use Azure AD token provider"
                azure_token_provider = get_azure_ad_token_provider(azure_scope=oidc_aud)
                    oidc_token = azure_token_provider()
                    if oidc_token is None:
                        raise ValueError("Azure OIDC provider returned None token")
                    error_msg = f"Azure OIDC provider failed: {str(e)}"
                    verbose_logger.error(error_msg)
                    raise ValueError(error_msg)
            with open(azure_federated_token_file, "r") as f:
                oidc_token = f.read()
        elif oidc_provider == "file":
            # Load token from a file
            with open(oidc_aud, "r") as f:
        elif oidc_provider == "env":
            # Load token directly from an environment variable
            oidc_token = os.getenv(oidc_aud)
                raise ValueError(f"Environment variable {oidc_aud} not found")
        elif oidc_provider == "env_path":
            # Load token from a file path specified in an environment variable
            token_file_path = os.getenv(oidc_aud)
            if token_file_path is None:
            with open(token_file_path, "r") as f:
            raise ValueError("Unsupported OIDC provider")
        if _should_read_secret_from_secret_manager() and litellm.secret_manager_client is not None:
                client = litellm.secret_manager_client
                key_manager = "local"
                if key_management_system is not None:
                    key_manager = key_management_system.value
                if key_management_settings is not None:
                        key_management_settings.hosted_keys is not None
                        and secret_name not in key_management_settings.hosted_keys
                    ):  # allow user to specify which keys to check in hosted key manager
                # Delegate to the secret manager handler
                secret = get_secret_from_manager(
                    key_manager=key_manager,
                    secret_name=secret_name,
                    key_management_settings=key_management_settings,
            except Exception as e:  # check if it's in os.environ
                    f"Defaulting to os.environ value for key={secret_name}. An exception occurred - {str(e)}.\n\n{traceback.format_exc()}"
                secret = os.getenv(secret_name)
                if isinstance(secret, str):
                    secret_value_as_bool = ast.literal_eval(secret)
                    if isinstance(secret_value_as_bool, bool):
                        return secret_value_as_bool
                        return secret
            secret = os.environ.get(secret_name)
            secret_value_as_bool = str_to_bool(secret) if secret is not None else None
            if secret_value_as_bool is not None and isinstance(secret_value_as_bool, bool):
def _should_read_secret_from_secret_manager() -> bool:
    Returns True if the secret manager should be used to read the secret, False otherwise
    - If the secret manager client is not set, return False
    - If the `_key_management_settings` access mode is "read_only" or "read_and_write", return True
    - Otherwise, return False
    if litellm.secret_manager_client is not None:
        if litellm._key_management_settings is not None:
                litellm._key_management_settings.access_mode == "read_only"
                or litellm._key_management_settings.access_mode == "read_and_write"
Main entry point for Skills API operations
Provides create, list, get, and delete operations for skills
from litellm.types.llms.anthropic_skills import (
    CreateSkillRequest,
    DeleteSkillResponse,
    ListSkillsParams,
    ListSkillsResponse,
    Skill,
# Initialize HTTP handler
DEFAULT_ANTHROPIC_API_BASE = "https://api.anthropic.com/v1"
# Initialize LiteLLM skills handler (lazy - only used when custom_llm_provider="litellm")
_litellm_skills_handler = None
def _get_litellm_skills_handler():
    """Lazy initialization of LiteLLM skills handler to avoid import overhead."""
    global _litellm_skills_handler
    if _litellm_skills_handler is None:
        _litellm_skills_handler = LiteLLMSkillsTransformationHandler()
    return _litellm_skills_handler
async def acreate_skill(
    files: Optional[List[Any]] = None,
    display_title: Optional[str] = None,
    Async: Create a new skill
        files: Files to upload for the skill. All files must be in the same top-level directory and must include a SKILL.md file at the root.
        display_title: Optional display title for the skill
        extra_headers: Additional headers for the request
        extra_query: Additional query parameters
        custom_llm_provider: Provider name (e.g., 'anthropic')
        Skill object
        kwargs["acreate_skill"] = True
            display_title=display_title,
def create_skill(
) -> Union[Skill, Coroutine[Any, Any, Skill]]:
    Create a new skill
        _is_async = kwargs.pop("acreate_skill", False) is True
        # Get LiteLLM parameters
        # Determine provider
            custom_llm_provider = "anthropic"
        # Build create request
        create_request: CreateSkillRequest = {}
        if display_title is not None:
            create_request["display_title"] = display_title
            create_request["files"] = files
        # Merge extra_body if provided
        if extra_body:
            create_request.update(extra_body)  # type: ignore
        # Route to LiteLLM DB if custom_llm_provider="litellm_proxy"
        if custom_llm_provider == LlmProviders.LITELLM_PROXY.value:
            return _get_litellm_skills_handler().create_skill_handler(
                metadata=extra_body.get("metadata") if extra_body else None,
                user_id=kwargs.get("user_id"),
        # Get provider config for external providers (Anthropic, etc.)
        skills_api_provider_config: Optional[BaseSkillsAPIConfig] = (
            ProviderConfigManager.get_provider_skills_api_config(
        if skills_api_provider_config is None:
                f"CREATE skill is not supported for {custom_llm_provider}"
        headers = extra_headers or {}
        headers = skills_api_provider_config.validate_environment(
            headers=headers, litellm_params=litellm_params
        # Transform request
        request_body = skills_api_provider_config.transform_create_skill_request(
            create_request=create_request,
        # Get API base and URL
        from litellm.llms.anthropic.common_utils import AnthropicModelInfo
        api_base = AnthropicModelInfo.get_api_base(litellm_params.api_base)
        url = skills_api_provider_config.get_complete_url(
            api_base=api_base, endpoint="skills"
        # Pre-call logging
            optional_params=request_body,
        # Make HTTP request
        response = base_llm_http_handler.create_skill_handler(
            skills_api_provider_config=skills_api_provider_config,
async def alist_skills(
    page: Optional[str] = None,
    source: Optional[str] = None,
) -> ListSkillsResponse:
    Async: List all skills
        limit: Number of results to return per page (max 100, default 20)
        page: Pagination token for fetching a specific page of results
        source: Filter skills by source ('custom' or 'anthropic')
        ListSkillsResponse object
        kwargs["alist_skills"] = True
            page=page,
            source=source,
def list_skills(
) -> Union[ListSkillsResponse, Coroutine[Any, Any, ListSkillsResponse]]:
    List all skills
        _is_async = kwargs.pop("alist_skills", False) is True
            return _get_litellm_skills_handler().list_skills_handler(
                limit=limit or 20,
                offset=0,
            raise ValueError(f"LIST skills is not supported for {custom_llm_provider}")
        # Build list parameters
        list_params: ListSkillsParams = {}
            list_params["limit"] = limit
        if page is not None:
            list_params["page"] = page
            list_params["source"] = source
        # Merge extra_query if provided
        if extra_query:
            list_params.update(extra_query)  # type: ignore
        url, query_params = skills_api_provider_config.transform_list_skills_request(
            list_params=list_params,
            optional_params=query_params,
        response = base_llm_http_handler.list_skills_handler(
async def aget_skill(
    Async: Get a skill by ID
        skill_id: The ID of the skill to fetch
        kwargs["aget_skill"] = True
            skill_id=skill_id,
def get_skill(
    Get a skill by ID
        _is_async = kwargs.pop("aget_skill", False) is True
            return _get_litellm_skills_handler().get_skill_handler(
            raise ValueError(f"GET skill is not supported for {custom_llm_provider}")
        # Get API base
        url, headers = skills_api_provider_config.transform_get_skill_request(
            api_base=api_base or DEFAULT_ANTHROPIC_API_BASE,
            optional_params={"skill_id": skill_id},
        response = base_llm_http_handler.get_skill_handler(
async def adelete_skill(
) -> DeleteSkillResponse:
    Async: Delete a skill by ID
        skill_id: The ID of the skill to delete
        DeleteSkillResponse object
        kwargs["adelete_skill"] = True
def delete_skill(
) -> Union[DeleteSkillResponse, Coroutine[Any, Any, DeleteSkillResponse]]:
    Delete a skill by ID
        _is_async = kwargs.pop("adelete_skill", False) is True
            return _get_litellm_skills_handler().delete_skill_handler(
                f"DELETE skill is not supported for {custom_llm_provider}"
        url, headers = skills_api_provider_config.transform_delete_skill_request(
        response = base_llm_http_handler.delete_skill_handler(
from typing import Any, Dict, List, Literal, Optional
    """Container expiration settings."""
    minutes: int
class ContainerObject(BaseModel):
    """Represents a container object."""
    object: Literal["container"]
    _hidden_params: Dict[str, Any] = {}
            return self.model_dump(**kwargs)
class DeleteContainerResult(BaseModel):
    """Result of a delete container request."""
    object: Literal["container.deleted"]
    """Response object for list containers request."""
    data: List[ContainerObject]
class ContainerCreateOptionalRequestParams(TypedDict, total=False):
    TypedDict for Optional parameters supported by OpenAI's container creation API.
    Params here: https://platform.openai.com/docs/api-reference/containers/create
    expires_after: Optional[Dict[str, Any]]  # ExpiresAfter object
    file_ids: Optional[List[str]]
    extra_headers: Optional[Dict[str, str]]
    extra_body: Optional[Dict[str, str]]
class ContainerCreateRequestParams(ContainerCreateOptionalRequestParams, total=False):
    TypedDict for request parameters supported by OpenAI's container creation API.
class ContainerListOptionalRequestParams(TypedDict, total=False):
    TypedDict for Optional parameters supported by OpenAI's container list API.
    Params here: https://platform.openai.com/docs/api-reference/containers/list
    after: Optional[str]
    order: Optional[str]
    extra_query: Optional[Dict[str, str]]
class ContainerFileObject(BaseModel):
    """Represents a container file object."""
    object: Literal["container.file", "container_file"]  # OpenAI returns "container.file"
    bytes: Optional[int] = None  # Can be null for some files
class ContainerFileListResponse(BaseModel):
    """Response object for list container files request."""
    data: List[ContainerFileObject]
class DeleteContainerFileResponse(BaseModel):
    """Response object for delete container file request."""
    object: Literal["container_file.deleted"]
# Import types from the Google GenAI SDK
from typing import TYPE_CHECKING, Any, Dict, List, Optional, TypeAlias
from litellm.types.llms.openai import BaseLiteLLMOpenAIResponseObject
# During static type-checking we can rely on the real google-genai types.
    from google.genai import types as _genai_types  # type: ignore
    ContentListUnion = _genai_types.ContentListUnion
    ContentListUnionDict = _genai_types.ContentListUnionDict
    GenerateContentConfigOrDict = _genai_types.GenerateContentConfigOrDict
    GoogleGenAIGenerateContentResponse = _genai_types.GenerateContentResponse
    GenerateContentContentListUnionDict = _genai_types.ContentListUnionDict
    GenerateContentConfigDict = _genai_types.GenerateContentConfigDict
    GenerateContentRequestParametersDict = _genai_types._GenerateContentParametersDict
    ToolConfigDict = _genai_types.ToolConfigDict
    class GenerateContentRequestDict(GenerateContentRequestParametersDict):  # type: ignore[misc]
        generationConfig: Optional[Any]
        tools: Optional[ToolConfigDict] # type: ignore[assignment]
    class GenerateContentResponse(GoogleGenAIGenerateContentResponse, BaseLiteLLMOpenAIResponseObject): # type: ignore[misc]
    # Fallback types when google.genai is not available
    ContentListUnion = Any
    ContentListUnionDict = Dict[str, Any]
    GenerateContentConfigOrDict = Dict[str, Any]
    GoogleGenAIGenerateContentResponse = Dict[str, Any]
    GenerateContentContentListUnionDict = Dict[str, Any]
    # Create a proper fallback class that can be instantiated
    class GenerateContentConfigDict(dict):  # type: ignore[misc]
        def __init__(self, **kwargs):  # type: ignore
    class GenerateContentRequestParametersDict(dict):  # type: ignore[misc]
    ToolConfigDict = Dict[str, Any]
            # Extract specific fields
            self.generationConfig = kwargs.get('generationConfig')
            self.tools = kwargs.get('tools')
    class GenerateContentResponse(BaseLiteLLMOpenAIResponseObject): # type: ignore[misc]
            self._hidden_params = kwargs.get('_hidden_params', {})from typing import Any, Dict, List, Literal, Optional, Union
class ImageEditOptionalRequestParams(TypedDict, total=False):
    TypedDict for Optional parameters supported by OpenAI's image edit API.
    Params here: https://platform.openai.com/docs/api-reference/images/createEdit
    mask: Optional[str]
    quality: Optional[Literal["high", "medium", "low", "standard", "auto"]]
    size: Optional[str]
class ImageEditRequestParams(ImageEditOptionalRequestParams, total=False):
    TypedDict for request parameters supported by OpenAI's image edit API.
    image: FileTypes
from openai.types.responses.response_function_tool_call import ResponseFunctionToolCall
from typing_extensions import Any, List, Optional, TypedDict
from litellm.types.llms.base import BaseLiteLLMOpenAIResponseObject
class GenericResponseOutputItemContentAnnotation(BaseLiteLLMOpenAIResponseObject):
    """Annotation for content in a message"""
    start_index: Optional[int]
    end_index: Optional[int]
class OutputText(BaseLiteLLMOpenAIResponseObject):
    """Text output content from an assistant message"""
    type: Optional[str]  # "output_text"
    annotations: Optional[List[GenericResponseOutputItemContentAnnotation]]
class OutputFunctionToolCall(BaseLiteLLMOpenAIResponseObject):
    """A tool call to run a function"""
    arguments: Optional[str]
    type: Optional[str]  # "function_call"
class OutputImageGenerationCall(BaseLiteLLMOpenAIResponseObject):
    """An image generation call output"""
    status: Literal["in_progress", "completed", "incomplete", "failed"]
    result: Optional[str]  # Base64 encoded image data (without data:image prefix)
class GenericResponseOutputItem(BaseLiteLLMOpenAIResponseObject):
    Generic response API output item
    type: str  # "message"
    status: str  # "completed", "in_progress", etc.
    role: str  # "assistant", "user", etc.
    content: List[OutputText]
class DeleteResponseResult(BaseLiteLLMOpenAIResponseObject):
    Result of a delete response request
        "id": "resp_6786a1bec27481909a17d673315b29f6",
        "deleted": true
    object: Optional[str]
    deleted: Optional[bool]
    # Define private attributes using PrivateAttr
    _hidden_params: dict = PrivateAttr(default_factory=dict)
class DecodedResponseId(TypedDict, total=False):
    """Structure representing a decoded response ID"""
from typing import Dict, List, Literal, Optional
from litellm.types.llms.base import LiteLLMPydanticObjectBase
class KeyManagementSystem(enum.Enum):
    GOOGLE_KMS = "google_kms"
    AZURE_KEY_VAULT = "azure_key_vault"
    AWS_SECRET_MANAGER = "aws_secret_manager"
    GOOGLE_SECRET_MANAGER = "google_secret_manager"
    HASHICORP_VAULT = "hashicorp_vault"
    CYBERARK = "cyberark"
    LOCAL = "local"
    AWS_KMS = "aws_kms"
class KeyManagementSettings(LiteLLMPydanticObjectBase):
    hosted_keys: Optional[List] = None
    store_virtual_keys: Optional[bool] = False
    If True, virtual keys created by litellm will be stored in the secret manager
    prefix_for_stored_virtual_keys: str = "litellm/"
    If set, this prefix will be used for stored virtual keys in the secret manager
    access_mode: Literal["read_only", "write_only", "read_and_write"] = "read_only"
    Access mode for the secret manager, when write_only will only use for writing secrets
    primary_secret_name: Optional[str] = None
    If set, will read secrets from this primary secret in the secret manager
    eg. on AWS you can store multiple secret values as K/V pairs in a single secret
    """Optional description attached when creating secrets (visible in AWS console)."""
    tags: Optional[Dict[str, str]] = None
    """Optional tags to attach when creating secrets (e.g. {"Environment": "Prod", "Owner": "AI-Platform"})."""
    custom_secret_manager: Optional[str] = None
    Path to custom secret manager class (e.g. "my_secret_manager.InMemorySecretManager")
    Required when key_management_system is "custom"
    # AWS IAM Role Assumption Settings (for AWS Secret Manager)
    aws_region_name: Optional[str] = None
    """AWS region for Secret Manager operations (e.g., 'us-east-1')"""
    """ARN of IAM role to assume for Secret Manager access (e.g., 'arn:aws:iam::123456789012:role/MyRole')"""
    """Session name for the assumed role session (optional, auto-generated if not provided)"""
    aws_external_id: Optional[str] = None
    """External ID for role assumption (required for cross-account access)"""
    aws_profile_name: Optional[str] = None
    """AWS profile name to use from ~/.aws/credentials"""
    """Web identity token for OIDC/IRSA authentication"""
    aws_sts_endpoint: Optional[str] = None
    """Custom STS endpoint URL (useful for VPC endpoints or testing)"""class VideoObject(BaseModel):
    """Represents a generated video object."""
    created_at: Optional[int] = None
    error: Optional[Dict[str, Any]] = None
    progress: Optional[int] = None
    seconds: Optional[str] = None
    size: Optional[str] = None
    usage: Optional[Dict[str, Any]] = None
class VideoResponse(BaseModel):
    """Response object for video generation requests."""
    data: List[VideoObject]
    hidden_params: Dict[str, Any] = {}
class VideoCreateOptionalRequestParams(TypedDict, total=False):
    TypedDict for Optional parameters supported by OpenAI's video creation API.
    Params here: https://platform.openai.com/docs/api-reference/videos/create
    input_reference: Optional[FileTypes]  # File reference for input image
    seconds: Optional[str]
class VideoCreateRequestParams(VideoCreateOptionalRequestParams, total=False):
    TypedDict for request parameters supported by OpenAI's video creation API.
class DecodedVideoId(TypedDict, total=False):
    """Structure representing a decoded video ID"""
    video_id: str"""LiteLLM SDK functions for managing vector store files."""
from litellm.types.vector_store_files import (
    VectorStoreFileContentResponse,
    VectorStoreFileCreateRequest,
    VectorStoreFileDeleteResponse,
    VectorStoreFileListQueryParams,
    VectorStoreFileListResponse,
    VectorStoreFileObject,
    VectorStoreFileUpdateRequest,
from litellm.vector_store_files.utils import VectorStoreFileRequestUtils
VectorStoreFileAttributeValue = Union[str, int, float, bool]
VectorStoreFileAttributes = Dict[str, VectorStoreFileAttributeValue]
def _ensure_provider(custom_llm_provider: Optional[str]) -> str:
    return custom_llm_provider or "openai"
def _prepare_registry_credentials(
    if litellm.vector_store_registry is None:
        registry_credentials = (
            litellm.vector_store_registry.get_credentials_for_vector_store(
                vector_store_id
        if registry_credentials:
            kwargs.update(registry_credentials)
    attributes: Optional[VectorStoreFileAttributes] = None,
    chunking_strategy: Optional[Dict[str, Any]] = None,
) -> VectorStoreFileObject:
        kwargs["acreate"] = True
    except Exception as e:  # noqa: BLE001
) -> Union[VectorStoreFileObject, Coroutine[Any, Any, VectorStoreFileObject]]:
        _is_async = kwargs.pop("acreate", False) is True
        custom_llm_provider = _ensure_provider(custom_llm_provider)
        _prepare_registry_credentials(vector_store_id=vector_store_id, kwargs=kwargs)
            vector_store_id=vector_store_id, **kwargs
            provider=LlmProviders(custom_llm_provider)
                f"Vector store file create is not supported for {custom_llm_provider}"
        create_request: VectorStoreFileCreateRequest = (
            VectorStoreFileRequestUtils.get_create_request_params(local_vars)
        create_request["file_id"] = file_id
                "vector_store_id": vector_store_id,
                **create_request,
        response = base_llm_http_handler.vector_store_file_create_handler(
            vector_store_files_provider_config=provider_config,
async def alist(
    filter: Optional[str] = None,
) -> VectorStoreFileListResponse:
        kwargs["alist"] = True
            filter=filter,
) -> Union[VectorStoreFileListResponse, Coroutine[Any, Any, VectorStoreFileListResponse]]:
        _is_async = kwargs.pop("alist", False) is True
                f"Vector store file list is not supported for {custom_llm_provider}"
        list_query: VectorStoreFileListQueryParams = (
            VectorStoreFileRequestUtils.get_list_query_params(local_vars)
            optional_params={"vector_store_id": vector_store_id, **list_query},
        response = base_llm_http_handler.vector_store_file_list_handler(
            query_params=list_query,
async def aretrieve(
        kwargs["aretrieve"] = True
        _is_async = kwargs.pop("aretrieve", False) is True
                f"Vector store file retrieve is not supported for {custom_llm_provider}"
        response = base_llm_http_handler.vector_store_file_retrieve_handler(
async def aretrieve_content(
) -> VectorStoreFileContentResponse:
        kwargs["aretrieve_content"] = True
    VectorStoreFileContentResponse, Coroutine[Any, Any, VectorStoreFileContentResponse]
        _is_async = kwargs.pop("aretrieve_content", False) is True
                f"Vector store file content retrieve is not supported for {custom_llm_provider}"
        response = base_llm_http_handler.vector_store_file_content_handler(
    attributes: VectorStoreFileAttributes,
        kwargs["aupdate"] = True
        _is_async = kwargs.pop("aupdate", False) is True
                f"Vector store file update is not supported for {custom_llm_provider}"
        update_request: VectorStoreFileUpdateRequest = (
            VectorStoreFileRequestUtils.get_update_request_params(local_vars)
        update_request["attributes"] = attributes
                **update_request,
        response = base_llm_http_handler.vector_store_file_update_handler(
            update_request=update_request,
) -> VectorStoreFileDeleteResponse:
        kwargs["adelete"] = True
    VectorStoreFileDeleteResponse, Coroutine[Any, Any, VectorStoreFileDeleteResponse]
        _is_async = kwargs.pop("adelete", False) is True
                f"Vector store file delete is not supported for {custom_llm_provider}"
        response = base_llm_http_handler.vector_store_file_delete_handler(
LiteLLM SDK Functions for Creating and Searching Vector Stores
from litellm.types.vector_stores import (
    VectorStoreCreateOptionalRequestParams,
    VectorStoreCreateResponse,
    VectorStoreFileCounts,
    VectorStoreResultContent,
    VectorStoreSearchOptionalRequestParams,
    VectorStoreSearchResult,
from litellm.vector_stores.utils import VectorStoreRequestUtils
def mock_vector_store_search_response(
    mock_results: Optional[List[VectorStoreSearchResult]] = None,
    """Mock response for vector store search"""
    if mock_results is None:
        mock_results = [
            VectorStoreSearchResult(
                score=0.95,
                    VectorStoreResultContent(
                        text="This is a sample search result from the vector store.",
    return VectorStoreSearchResponse(
        object="vector_store.search_results.page",
        search_query="sample query",
        data=mock_results,
def mock_vector_store_create_response(
    mock_response: Optional[VectorStoreCreateResponse] = None,
    """Mock response for vector store create"""
        mock_response = VectorStoreCreateResponse(
            id="vs_mock123",
            object="vector_store",
            created_at=1699061776,
            name="Mock Vector Store",
            file_counts=VectorStoreFileCounts(
                in_progress=0,
                completed=0,
                failed=0,
                cancelled=0,
                total=0,
            expires_after=None,
            expires_at=None,
            last_active_at=None,
    expires_after: Optional[Dict] = None,
    chunking_strategy: Optional[Dict] = None,
) -> VectorStoreCreateResponse:
    Async: Create a vector store.
            custom_llm_provider = "openai"  # Default to OpenAI for vector stores
) -> Union[VectorStoreCreateResponse, Coroutine[Any, Any, VectorStoreCreateResponse]]:
        file_ids: A list of File IDs that the vector store should use.
        expires_after: The expiration policy for the vector store.
        chunking_strategy: The chunking strategy used to chunk the file(s).
        metadata: Set of 16 key-value pairs that can be attached to an object.
        VectorStoreCreateResponse containing the created vector store details.
        ## MOCK RESPONSE LOGIC
            litellm_params.mock_response, dict
            return mock_vector_store_create_response(
                mock_response=VectorStoreCreateResponse(**litellm_params.mock_response)
        # Default to OpenAI for vector stores
            custom_llm_provider = "openai"
        if "/" in custom_llm_provider:
            api_type, custom_llm_provider, _, _ = get_llm_provider(
                model=custom_llm_provider,
                litellm_params=None,
            api_type = None
            custom_llm_provider = custom_llm_provider
        # get provider config - using vector store custom logger for now
        vector_store_provider_config = (
            ProviderConfigManager.get_provider_vector_stores_config(
        if vector_store_provider_config is None:
                f"Vector store create is not supported for {custom_llm_provider}"
        # Get VectorStoreCreateOptionalRequestParams with only valid parameters
        vector_store_create_optional_params: VectorStoreCreateOptionalRequestParams = (
            VectorStoreRequestUtils.get_requested_vector_store_create_optional_param(
                **vector_store_create_optional_params,
        response = base_llm_http_handler.vector_store_create_handler(
            vector_store_create_optional_params=vector_store_create_optional_params,
            vector_store_provider_config=vector_store_provider_config,
    filters: Optional[Dict] = None,
    max_num_results: Optional[int] = None,
    ranking_options: Optional[Dict] = None,
    rewrite_query: Optional[bool] = None,
) -> VectorStoreSearchResponse:
    Async: Search a vector store for relevant chunks based on a query and file attributes filter.
            max_num_results=max_num_results,
            ranking_options=ranking_options,
            rewrite_query=rewrite_query,
) -> Union[VectorStoreSearchResponse, Coroutine[Any, Any, VectorStoreSearchResponse]]:
    Search a vector store for relevant chunks based on a query and file attributes filter.
        vector_store_id: The ID of the vector store to search.
        query: A query string or array for the search.
        filters: Optional filter to apply based on file attributes.
        max_num_results: Maximum number of results to return (1-50, default 10).
        ranking_options: Optional ranking options for search.
        VectorStoreSearchResponse containing the search results.
        # pull credentials from registry if available
        vector_store_id_for_credentials = kwargs.get("vector_store_id", vector_store_id)
            litellm.vector_store_registry is not None
            and vector_store_id_for_credentials is not None
                        vector_store_id_for_credentials
        litellm_params = GenericLiteLLMParams(vector_store_id=vector_store_id, **kwargs)
            litellm_params.mock_response, (str, list)
            mock_results = None
            if isinstance(litellm_params.mock_response, list):
                mock_results = litellm_params.mock_response
            return mock_vector_store_search_response(mock_results=mock_results)
                f"Vector store search is not supported for {custom_llm_provider}"
        # Get VectorStoreSearchOptionalRequestParams with only valid parameters
        vector_store_search_optional_params: VectorStoreSearchOptionalRequestParams = (
            VectorStoreRequestUtils.get_requested_vector_store_search_optional_param(
                local_vars,
            model=api_type,
                **vector_store_search_optional_params,
        response = base_llm_http_handler.vector_store_search_handler(
            vector_store_search_optional_params=vector_store_search_optional_params,
from litellm.constants import DEFAULT_VIDEO_ENDPOINT_MODEL
from litellm.types.utils import CallTypes, FileTypes
from litellm.types.videos.main import (
    VideoCreateOptionalRequestParams,
from litellm.types.videos.utils import decode_video_id_with_provider
from litellm.videos.utils import VideoGenerationRequestUtils
##### Video Generation #######################
async def avideo_generation(
    input_reference: Optional[FileTypes] = None,
    seconds: Optional[str] = None,
) -> VideoObject:
    Asynchronously calls the `video_generation` function with the given arguments and keyword arguments.
    - `prompt` (str): Text prompt that describes the video to generate
    - `model` (Optional[str]): The video generation model to use
    - `input_reference` (Optional[FileTypes]): Optional image reference that guides generation
    - `seconds` (Optional[str]): Clip duration in seconds
    - `size` (Optional[str]): Output resolution formatted as width x height
    - `user` (Optional[str]): A unique identifier representing your end-user
    - `custom_llm_provider` (Optional[str]): The LLM provider to use
    - `response` (VideoResponse): The response returned by the `video_generation` function.
                model=model or DEFAULT_VIDEO_ENDPOINT_MODEL, api_base=local_vars.get("api_base", None)
# Overload for when avideo_generation=True (returns Coroutine)
def video_generation(
    input_reference: Optional[str] = None,
    avideo_generation: Literal[True],
) -> Coroutine[Any, Any, VideoObject]:
    avideo_generation: Literal[False] = False,
def video_generation(  # noqa: PLR0915
    Coroutine[Any, Any, VideoObject],
    Maps the https://api.openai.com/v1/videos endpoint.
        mock_response = kwargs.get("mock_response", None)
            response = VideoObject(**mock_response)
            model=model or DEFAULT_VIDEO_ENDPOINT_MODEL,
        video_generation_provider_config: Optional[BaseVideoConfig] = (
            ProviderConfigManager.get_provider_video_config(
        if video_generation_provider_config is None:
            raise ValueError(f"video generation is not supported for {custom_llm_provider}")
        # Get VideoGenerationOptionalRequestParams with only valid parameters
        video_generation_optional_params: VideoCreateOptionalRequestParams = (
            VideoGenerationRequestUtils.get_requested_video_generation_optional_param(local_vars)
        # Get optional parameters for the video generation API
        video_generation_request_params: Dict = (
            VideoGenerationRequestUtils.get_optional_params_video_generation(
                video_generation_provider_config=video_generation_provider_config,
                video_generation_optional_params=video_generation_optional_params,
            optional_params=dict(video_generation_request_params),
                **video_generation_request_params,
        # Set the correct call type for video generation
        litellm_logging_obj.call_type = CallTypes.create_video.value
        return base_llm_http_handler.video_generation_handler(
            video_generation_optional_request_params=video_generation_request_params,
def video_content(
    Coroutine[Any, Any, bytes],
    Download video content from OpenAI's video API.
        video_id (str): The identifier of the video whose content to download.
        api_key (Optional[str]): The API key to use for authentication.
        api_base (Optional[str]): The base URL for the API.
        timeout (Optional[float]): The timeout for the request in seconds.
        custom_llm_provider (Optional[str]): The LLM provider to use. If not provided, will be auto-detected.
        variant (Optional[str]): Which downloadable asset to return. Defaults to the MP4 video.
        extra_headers (Optional[Dict[str, Any]]): Additional headers to include in the request.
        extra_query (Optional[Dict[str, Any]]): Additional query parameters.
        extra_body (Optional[Dict[str, Any]]): Additional body parameters.
        bytes: The raw video content as bytes.
        video_bytes = litellm.video_content(
            video_id="video_123"
        with open("video.mp4", "wb") as f:
            f.write(video_bytes)
        # Try to decode provider from video_id if not explicitly provided
            custom_llm_provider = decoded.get("custom_llm_provider") or "openai"
        video_provider_config: Optional[BaseVideoConfig] = (
        if video_provider_config is None:
            raise ValueError(f"video support download is not supported for {custom_llm_provider}")
        # For video content download, we don't need complex optional parameter handling
        # Just pass the basic parameters that are relevant for content download
        video_content_request_params: Dict = {
            "video_id": video_id,
            user=kwargs.get("user"),
            optional_params=dict(video_content_request_params),
                **video_content_request_params,
        return base_llm_http_handler.video_content_handler(
            video_id=video_id,
            video_content_provider_config=video_provider_config,
##### Video Content Download #######################
async def avideo_content(
    Asynchronously download video content.
    - `video_id` (str): The identifier of the video whose content to download
    - `timeout` (Optional[float]): The timeout for the request in seconds
    - `bytes`: The raw video content as bytes
##### Video Remix #######################
async def avideo_remix(
    Asynchronously calls the `video_remix` function with the given arguments and keyword arguments.
    - `video_id` (str): The identifier of the completed video to remix
    - `prompt` (str): Updated text prompt that directs the remix generation
    - `response` (VideoObject): The response returned by the `video_remix` function.
# Overload for when avideo_remix=True (returns Coroutine)
def video_remix(
    avideo_remix: Literal[True],
    avideo_remix: Literal[False] = False,
def video_remix(  # noqa: PLR0915
    Maps the https://api.openai.com/v1/videos/{video_id}/remix endpoint.
        video_remix_provider_config: Optional[BaseVideoConfig] = (
        if video_remix_provider_config is None:
            raise ValueError(f"video remix is not supported for {custom_llm_provider}")
        # For video remix, we need the video_id and prompt
        video_remix_request_params: Dict = {
            optional_params=dict(video_remix_request_params),
                **video_remix_request_params,
        # Set the correct call type for video remix
        litellm_logging_obj.call_type = CallTypes.video_remix.value
        return base_llm_http_handler.video_remix_handler(
            video_remix_provider_config=video_remix_provider_config,
##### Video List #######################
async def avideo_list(
) -> List[VideoObject]:
    Asynchronously calls the `video_list` function with the given arguments and keyword arguments.
    - `after` (Optional[str]): Identifier for the last item from the previous pagination request
    - `limit` (Optional[int]): Number of items to retrieve
    - `order` (Optional[str]): Sort order of results by timestamp. Use asc for ascending order or desc for descending order
    - `api_key` (Optional[str]): The API key to use for authentication
    - `response` (Dict[str, Any]): The response returned by the `video_list` function.
                model="", api_base=local_vars.get("api_base", None)
# Overload for when avideo_list=True (returns Coroutine)
def video_list(
    avideo_list: Literal[True],
) -> Coroutine[Any, Any, List[VideoObject]]:
    avideo_list: Literal[False] = False,
def video_list(  # noqa: PLR0915
    List[VideoObject],
    Coroutine[Any, Any, List[VideoObject]],
            return [VideoObject(**item) for item in mock_response]
        # Ensure custom_llm_provider is not None - default to openai if not provided
        video_list_provider_config: Optional[BaseVideoConfig] = (
        if video_list_provider_config is None:
            raise ValueError(f"video list is not supported for {custom_llm_provider}")
        # For video list, we need the query parameters
        video_list_request_params: Dict = {
            optional_params=dict(video_list_request_params),
                **video_list_request_params,
        # Set the correct call type for video list
        litellm_logging_obj.call_type = CallTypes.video_list.value
        return base_llm_http_handler.video_list_handler(
            video_list_provider_config=video_list_provider_config,
##### Video Status/Retrieve #######################
async def avideo_status(
    Asynchronously retrieve video status from OpenAI's video API.
    - `video_id` (str): The identifier of the video whose status to retrieve
    - `model` (Optional[str]): The model to use. If not provided, will be auto-detected
    - `response` (VideoObject): The response returned by the `video_status` function.
# Overload for when avideo_status=True (returns Coroutine)
def video_status(
    avideo_status: Literal[True],
# Overload for when avideo_status=False (returns VideoObject)
    avideo_status: Literal[False] = False,
def video_status(  # noqa: PLR0915
    Retrieve video status from OpenAI's video API.
        video_id (str): The identifier of the video whose status to retrieve.
        timeout (int): The timeout for the request in seconds.
        VideoObject: The video status information.
        # Get video status
        video_status = litellm.video_status(
        print(f"Video status: {video_status.status}")
        print(f"Progress: {video_status.progress}%")
        video_status_provider_config: Optional[BaseVideoConfig] = (
        if video_status_provider_config is None:
            raise ValueError(f"video status is not supported for {custom_llm_provider}")
        # For video status, we need the video_id
        video_status_request_params: Dict = {
            optional_params=dict(video_status_request_params),
                **video_status_request_params,
        # Set the correct call type for video status
        litellm_logging_obj.call_type = CallTypes.video_retrieve.value
        return base_llm_http_handler.video_status_handler(
            video_status_provider_config=video_status_provider_config,
from dataclasses import fields
    git = None
import importlib_resources
import shtab
from aider import __version__, models, urls, utils
from aider.analytics import Analytics
from aider.args import get_parser
from aider.coders import Coder
from aider.coders.base_coder import UnknownEditFormat
from aider.commands import Commands, SwitchCoder
from aider.copypaste import ClipboardWatcher
from aider.deprecated import handle_deprecated_model_args
from aider.format_settings import format_settings, scrub_sensitive_info
from aider.history import ChatSummary
from aider.io import InputOutput
from aider.llm import litellm  # noqa: F401; properly init litellm on launch
from aider.models import ModelSettings
from aider.onboarding import offer_openrouter_oauth, select_default_model
from aider.repo import ANY_GIT_ERROR, GitRepo
from aider.report import report_uncaught_exceptions
from aider.versioncheck import check_version, install_from_main_branch, install_upgrade
from aider.watch import FileWatcher
def check_config_files_for_yes(config_files):
    for config_file in config_files:
        if Path(config_file).exists():
                with open(config_file, "r") as f:
                        if line.strip().startswith("yes:"):
                            print("Configuration error detected.")
                            print(f"The file {config_file} contains a line starting with 'yes:'")
                            print("Please replace 'yes:' with 'yes-always:' in this file.")
    return found
def get_git_root():
    """Try and guess the git repo, since the conf.yml can be at the repo root"""
        repo = git.Repo(search_parent_directories=True)
        return repo.working_tree_dir
    except (git.InvalidGitRepositoryError, FileNotFoundError):
def guessed_wrong_repo(io, git_root, fnames, git_dname):
    """After we parse the args, we can determine the real repo. Did we guess wrong?"""
        check_repo = Path(GitRepo(io, fnames, git_dname).root).resolve()
    except (OSError,) + ANY_GIT_ERROR:
    # we had no guess, rely on the "true" repo result
    if not git_root:
        return str(check_repo)
    git_root = Path(git_root).resolve()
    if check_repo == git_root:
def make_new_repo(git_root, io):
        repo = git.Repo.init(git_root)
        check_gitignore(git_root, io, False)
    except ANY_GIT_ERROR as err:  # issue #1233
        io.tool_error(f"Unable to create git repo in {git_root}")
        io.tool_output(str(err))
    io.tool_output(f"Git repository created in {git_root}")
def setup_git(git_root, io):
    if git is None:
        cwd = Path.cwd()
        cwd = None
    repo = None
    if git_root:
            repo = git.Repo(git_root)
        except ANY_GIT_ERROR:
    elif cwd == Path.home():
        io.tool_warning(
            "You should probably run aider in your project's directory, not your home dir."
    elif cwd and io.confirm_ask(
        "No git repo found, create one to track aider's changes (recommended)?"
        git_root = str(cwd.resolve())
        repo = make_new_repo(git_root, io)
    if not repo:
        user_name = repo.git.config("--get", "user.name") or None
    except git.exc.GitCommandError:
        user_name = None
        user_email = repo.git.config("--get", "user.email") or None
        user_email = None
    if user_name and user_email:
    with repo.config_writer() as git_config:
        if not user_name:
            git_config.set_value("user", "name", "Your Name")
            io.tool_warning('Update git name with: git config user.name "Your Name"')
        if not user_email:
            git_config.set_value("user", "email", "you@example.com")
            io.tool_warning('Update git email with: git config user.email "you@example.com"')
def check_gitignore(git_root, io, ask=True):
        patterns_to_add = []
        if not repo.ignored(".aider"):
            patterns_to_add.append(".aider*")
        env_path = Path(git_root) / ".env"
        if env_path.exists() and not repo.ignored(".env"):
            patterns_to_add.append(".env")
        if not patterns_to_add:
        gitignore_file = Path(git_root) / ".gitignore"
        if gitignore_file.exists():
                content = io.read_text(gitignore_file)
                if not content.endswith("\n"):
                    content += "\n"
                io.tool_error(f"Error when trying to read {gitignore_file}: {e}")
    if ask:
        io.tool_output("You can skip this check with --no-gitignore")
        if not io.confirm_ask(f"Add {', '.join(patterns_to_add)} to .gitignore (recommended)?"):
    content += "\n".join(patterns_to_add) + "\n"
        io.write_text(gitignore_file, content)
        io.tool_output(f"Added {', '.join(patterns_to_add)} to .gitignore")
        io.tool_error(f"Error when trying to write to {gitignore_file}: {e}")
        io.tool_output(
            "Try running with appropriate permissions or manually add these patterns to .gitignore:"
        for pattern in patterns_to_add:
            io.tool_output(f"  {pattern}")
def check_streamlit_install(io):
    return utils.check_pip_install_extra(
        io,
        "streamlit",
        "You need to install the aider browser feature",
        ["aider-chat[browser]"],
def write_streamlit_credentials():
    from streamlit.file_util import get_streamlit_file_path
    # See https://github.com/Aider-AI/aider/issues/772
    credential_path = Path(get_streamlit_file_path()) / "credentials.toml"
    if not os.path.exists(credential_path):
        empty_creds = '[general]\nemail = ""\n'
        os.makedirs(os.path.dirname(credential_path), exist_ok=True)
        with open(credential_path, "w") as f:
            f.write(empty_creds)
        print("Streamlit credentials already exist.")
def launch_gui(args):
    from streamlit.web import cli
    from aider import gui
    print("CONTROL-C to exit...")
    # Necessary so streamlit does not prompt the user for an email address.
    write_streamlit_credentials()
    target = gui.__file__
    st_args = ["run", target]
    st_args += [
        "--browser.gatherUsageStats=false",
        "--runner.magicEnabled=false",
        "--server.runOnSave=false",
    # https://github.com/Aider-AI/aider/issues/2193
    is_dev = "-dev" in str(__version__)
    if is_dev:
        print("Watching for file changes.")
            "--global.developmentMode=false",
            "--server.fileWatcherType=none",
            "--client.toolbarMode=viewer",  # minimal?
    st_args += ["--"] + args
    cli.main(st_args)
    # from click.testing import CliRunner
    # runner = CliRunner()
    # from streamlit.web import bootstrap
    # bootstrap.load_config_options(flag_options={})
    # cli.main_run(target, args)
    # sys.argv = ['streamlit', 'run', '--'] + args
def parse_lint_cmds(lint_cmds, io):
    err = False
    res = dict()
    for lint_cmd in lint_cmds:
        if re.match(r"^[a-z]+:.*", lint_cmd):
            pieces = lint_cmd.split(":")
            lang = pieces[0]
            cmd = lint_cmd[len(lang) + 1 :]
            lang = lang.strip()
            lang = None
            cmd = lint_cmd
        cmd = cmd.strip()
            res[lang] = cmd
            io.tool_error(f'Unable to parse --lint-cmd "{lint_cmd}"')
            io.tool_output('The arg should be "language: cmd --args ..."')
            io.tool_output('For example: --lint-cmd "python: flake8 --select=E9"')
            err = True
def generate_search_path_list(default_file, git_root, command_line_file):
    files.append(Path.home() / default_file)  # homedir
        files.append(Path(git_root) / default_file)  # git root
    files.append(default_file)
    if command_line_file:
        files.append(command_line_file)
    resolved_files = []
            resolved_files.append(Path(fn).resolve())
    files = resolved_files
    files.reverse()
    uniq = []
        if fn not in uniq:
            uniq.append(fn)
    uniq.reverse()
    files = uniq
    files = list(map(str, files))
    files = list(dict.fromkeys(files))
def register_models(git_root, model_settings_fname, io, verbose=False):
    model_settings_files = generate_search_path_list(
        ".aider.model.settings.yml", git_root, model_settings_fname
        files_loaded = models.register_models(model_settings_files)
        if len(files_loaded) > 0:
                io.tool_output("Loaded model settings from:")
                for file_loaded in files_loaded:
                    io.tool_output(f"  - {file_loaded}")  # noqa: E221
        elif verbose:
            io.tool_output("No model settings files loaded")
        io.tool_error(f"Error loading aider model settings: {e}")
        io.tool_output("Searched for model settings files:")
        for file in model_settings_files:
            io.tool_output(f"  - {file}")
def load_dotenv_files(git_root, dotenv_fname, encoding="utf-8"):
    # Standard .env file search path
    dotenv_files = generate_search_path_list(
        ".env",
        git_root,
        dotenv_fname,
    # Explicitly add the OAuth keys file to the beginning of the list
    oauth_keys_file = Path.home() / ".aider" / "oauth-keys.env"
    if oauth_keys_file.exists():
        # Insert at the beginning so it's loaded first (and potentially overridden)
        dotenv_files.insert(0, str(oauth_keys_file.resolve()))
        # Remove duplicates if it somehow got included by generate_search_path_list
        dotenv_files = list(dict.fromkeys(dotenv_files))
    loaded = []
    for fname in dotenv_files:
            if Path(fname).exists():
                load_dotenv(fname, override=True, encoding=encoding)
                loaded.append(fname)
            print(f"OSError loading {fname}: {e}")
            print(f"Error loading {fname}: {e}")
    return loaded
def register_litellm_models(git_root, model_metadata_fname, io, verbose=False):
    model_metadata_files = []
    # Add the resource file path
    resource_metadata = importlib_resources.files("aider.resources").joinpath("model-metadata.json")
    model_metadata_files.append(str(resource_metadata))
    model_metadata_files += generate_search_path_list(
        ".aider.model.metadata.json", git_root, model_metadata_fname
        model_metadata_files_loaded = models.register_litellm_models(model_metadata_files)
        if len(model_metadata_files_loaded) > 0 and verbose:
            io.tool_output("Loaded model metadata from:")
            for model_metadata_file in model_metadata_files_loaded:
                io.tool_output(f"  - {model_metadata_file}")  # noqa: E221
        io.tool_error(f"Error loading model metadata models: {e}")
def sanity_check_repo(repo, io):
    if not repo.repo.working_tree_dir:
        io.tool_error("The git repo does not seem to have a working tree?")
    bad_ver = False
        repo.get_tracked_files()
        if not repo.git_repo_error:
        error_msg = str(repo.git_repo_error)
            "Failed to read the Git repository. This issue is likely caused by a path encoded "
            f'in a format different from the expected encoding "{sys.getfilesystemencoding()}".\n'
            f"Internal error: {str(exc)}"
    except ANY_GIT_ERROR as exc:
        error_msg = str(exc)
        bad_ver = "version in (1, 2)" in error_msg
    except AssertionError as exc:
        bad_ver = True
    if bad_ver:
        io.tool_error("Aider only works with git repos with version number 1 or 2.")
        io.tool_output("You may be able to convert your repo: git update-index --index-version=2")
        io.tool_output("Or run aider --no-git to proceed without using git.")
        io.offer_url(urls.git_index_version, "Open documentation url for more info?")
    io.tool_error("Unable to read git repository, it may be corrupt?")
    io.tool_output(error_msg)
def main(argv=None, input=None, output=None, force_git_root=None, return_coder=False):
    report_uncaught_exceptions()
    if argv is None:
        argv = sys.argv[1:]
        git_root = None
    elif force_git_root:
        git_root = force_git_root
        git_root = get_git_root()
    conf_fname = Path(".aider.conf.yml")
    default_config_files = []
        default_config_files += [conf_fname.resolve()]  # CWD
        git_conf = Path(git_root) / conf_fname  # git root
        if git_conf not in default_config_files:
            default_config_files.append(git_conf)
    default_config_files.append(Path.home() / conf_fname)  # homedir
    default_config_files = list(map(str, default_config_files))
    parser = get_parser(default_config_files, git_root)
        args, unknown = parser.parse_known_args(argv)
        if all(word in str(e) for word in ["bool", "object", "has", "no", "attribute", "strip"]):
            if check_config_files_for_yes(default_config_files):
    if args.verbose:
        print("Config files search order, if no --config:")
        for file in default_config_files:
            exists = "(exists)" if Path(file).exists() else ""
            print(f"  - {file} {exists}")
    default_config_files.reverse()
    # Load the .env file specified in the arguments
    loaded_dotenvs = load_dotenv_files(git_root, args.env_file, args.encoding)
    # Parse again to include any arguments that might have been defined in .env
    if args.shell_completions:
        # Ensure parser.prog is set for shtab, though it should be by default
        parser.prog = "aider"
        print(shtab.complete(parser, shell=args.shell_completions))
        args.git = False
    if args.analytics_disable:
        analytics = Analytics(permanently_disable=True)
        print("Analytics have been permanently disabled.")
    if not args.verify_ssl:
        os.environ["SSL_VERIFY"] = ""
        litellm._load_litellm()
        litellm._lazy_module.client_session = httpx.Client(verify=False)
        litellm._lazy_module.aclient_session = httpx.AsyncClient(verify=False)
        # Set verify_ssl on the model_info_manager
        models.model_info_manager.set_verify_ssl(False)
    if args.timeout:
        models.request_timeout = args.timeout
    if args.dark_mode:
        args.user_input_color = "#32FF32"
        args.tool_error_color = "#FF3333"
        args.tool_warning_color = "#FFFF00"
        args.assistant_output_color = "#00FFFF"
        args.code_theme = "monokai"
    if args.light_mode:
        args.user_input_color = "green"
        args.tool_error_color = "red"
        args.tool_warning_color = "#FFA500"
        args.assistant_output_color = "blue"
        args.code_theme = "default"
    if return_coder and args.yes_always is None:
        args.yes_always = True
    editing_mode = EditingMode.VI if args.vim else EditingMode.EMACS
    def get_io(pretty):
        return InputOutput(
            pretty,
            args.yes_always,
            args.input_history_file,
            args.chat_history_file,
            user_input_color=args.user_input_color,
            tool_output_color=args.tool_output_color,
            tool_warning_color=args.tool_warning_color,
            tool_error_color=args.tool_error_color,
            completion_menu_color=args.completion_menu_color,
            completion_menu_bg_color=args.completion_menu_bg_color,
            completion_menu_current_color=args.completion_menu_current_color,
            completion_menu_current_bg_color=args.completion_menu_current_bg_color,
            assistant_output_color=args.assistant_output_color,
            code_theme=args.code_theme,
            dry_run=args.dry_run,
            encoding=args.encoding,
            line_endings=args.line_endings,
            llm_history_file=args.llm_history_file,
            editingmode=editing_mode,
            fancy_input=args.fancy_input,
            multiline_mode=args.multiline,
            notifications=args.notifications,
            notifications_command=args.notifications_command,
    io = get_io(args.pretty)
        io.rule()
    except UnicodeEncodeError as err:
        if not io.pretty:
        io = get_io(False)
        io.tool_warning("Terminal does not support pretty output (UnicodeDecodeError)")
    # Process any environment variables set via --set-env
    if args.set_env:
        for env_setting in args.set_env:
                name, value = env_setting.split("=", 1)
                os.environ[name.strip()] = value.strip()
                io.tool_error(f"Invalid --set-env format: {env_setting}")
                io.tool_output("Format should be: ENV_VAR_NAME=value")
    # Process any API keys set via --api-key
        for api_setting in args.api_key:
                provider, key = api_setting.split("=", 1)
                env_var = f"{provider.strip().upper()}_API_KEY"
                os.environ[env_var] = key.strip()
                io.tool_error(f"Invalid --api-key format: {api_setting}")
                io.tool_output("Format should be: provider=key")
    if args.anthropic_api_key:
        os.environ["ANTHROPIC_API_KEY"] = args.anthropic_api_key
    if args.openai_api_key:
        os.environ["OPENAI_API_KEY"] = args.openai_api_key
    # Handle deprecated model shortcut args
    handle_deprecated_model_args(args, io)
    if args.openai_api_base:
        os.environ["OPENAI_API_BASE"] = args.openai_api_base
    if args.openai_api_version:
            "--openai-api-version is deprecated, use --set-env OPENAI_API_VERSION=<value>"
        os.environ["OPENAI_API_VERSION"] = args.openai_api_version
    if args.openai_api_type:
        io.tool_warning("--openai-api-type is deprecated, use --set-env OPENAI_API_TYPE=<value>")
        os.environ["OPENAI_API_TYPE"] = args.openai_api_type
    if args.openai_organization_id:
            "--openai-organization-id is deprecated, use --set-env OPENAI_ORGANIZATION=<value>"
        os.environ["OPENAI_ORGANIZATION"] = args.openai_organization_id
    analytics = Analytics(
        logfile=args.analytics_log,
        permanently_disable=args.analytics_disable,
        posthog_host=args.analytics_posthog_host,
        posthog_project_api_key=args.analytics_posthog_project_api_key,
    if args.analytics is not False:
        if analytics.need_to_ask(args.analytics):
                "Aider respects your privacy and never collects your code, chat messages, keys or"
                " personal info."
            io.tool_output(f"For more info: {urls.analytics}")
            disable = not io.confirm_ask(
                "Allow collection of anonymous analytics to help improve aider?"
            analytics.asked_opt_in = True
            if disable:
                analytics.disable(permanently=True)
                io.tool_output("Analytics have been permanently disabled.")
            analytics.save_data()
            io.tool_output()
        # This is a no-op if the user has opted out
        analytics.enable()
    analytics.event("launched")
    if args.gui and not return_coder:
        if not check_streamlit_install(io):
            analytics.event("exit", reason="Streamlit not installed")
        analytics.event("gui session")
        launch_gui(argv)
        analytics.event("exit", reason="GUI session ended")
        for fname in loaded_dotenvs:
            io.tool_output(f"Loaded {fname}")
    all_files = args.files + (args.file or [])
    fnames = [str(Path(fn).resolve()) for fn in all_files]
    read_only_fnames = []
    for fn in args.read or []:
        path = Path(fn).expanduser().resolve()
            read_only_fnames.extend(str(f) for f in path.rglob("*") if f.is_file())
            read_only_fnames.append(str(path))
    if len(all_files) > 1:
        good = True
        for fname in all_files:
            if Path(fname).is_dir():
                io.tool_error(f"{fname} is a directory, not provided alone.")
                good = False
        if not good:
                "Provide either a single directory of a git repo, or a list of one or more files."
            analytics.event("exit", reason="Invalid directory input")
    git_dname = None
    if len(all_files) == 1:
        if Path(all_files[0]).is_dir():
            if args.git:
                git_dname = str(Path(all_files[0]).resolve())
                fnames = []
                io.tool_error(f"{all_files[0]} is a directory, but --no-git selected.")
                analytics.event("exit", reason="Directory with --no-git")
    # We can't know the git repo for sure until after parsing the args.
    # If we guessed wrong, reparse because that changes things like
    # the location of the config.yml and history files.
    if args.git and not force_git_root and git is not None:
        right_repo_root = guessed_wrong_repo(io, git_root, fnames, git_dname)
        if right_repo_root:
            analytics.event("exit", reason="Recursing with correct repo")
            return main(argv, input, output, right_repo_root, return_coder=return_coder)
    if args.just_check_update:
        update_available = check_version(io, just_check=True, verbose=args.verbose)
        analytics.event("exit", reason="Just checking update")
        return 0 if not update_available else 1
    if args.install_main_branch:
        success = install_from_main_branch(io)
        analytics.event("exit", reason="Installed main branch")
        return 0 if success else 1
    if args.upgrade:
        success = install_upgrade(io)
        analytics.event("exit", reason="Upgrade completed")
    if args.check_update:
        check_version(io, verbose=args.verbose)
        git_root = setup_git(git_root, io)
        if args.gitignore:
            check_gitignore(git_root, io)
        show = format_settings(parser, args)
        io.tool_output(show)
    cmd_line = " ".join(sys.argv)
    cmd_line = scrub_sensitive_info(args, cmd_line)
    io.tool_output(cmd_line, log_only=True)
    is_first_run = is_first_run_of_new_version(io, verbose=args.verbose)
    check_and_load_imports(io, is_first_run, verbose=args.verbose)
    register_models(git_root, args.model_settings_file, io, verbose=args.verbose)
    register_litellm_models(git_root, args.model_metadata_file, io, verbose=args.verbose)
    if args.list_models:
        models.print_matching_models(io, args.list_models)
        analytics.event("exit", reason="Listed models")
    # Process any command line aliases
    if args.alias:
        for alias_def in args.alias:
            # Split on first colon only
            parts = alias_def.split(":", 1)
            if len(parts) != 2:
                io.tool_error(f"Invalid alias format: {alias_def}")
                io.tool_output("Format should be: alias:model-name")
                analytics.event("exit", reason="Invalid alias format error")
            alias, model = parts
            models.MODEL_ALIASES[alias.strip()] = model.strip()
    selected_model_name = select_default_model(args, io, analytics)
    if not selected_model_name:
        # Error message and analytics event are handled within select_default_model
        # It might have already offered OAuth if no model/keys were found.
        # If it failed here, we exit.
    args.model = selected_model_name  # Update args with the selected model
    # Check if an OpenRouter model was selected/specified but the key is missing
    if args.model.startswith("openrouter/") and not os.environ.get("OPENROUTER_API_KEY"):
            f"The specified model '{args.model}' requires an OpenRouter API key, which was not"
            " found."
        # Attempt OAuth flow because the specific model needs it
        if offer_openrouter_oauth(io, analytics):
            # OAuth succeeded, the key should now be in os.environ.
            # Check if the key is now present after the flow.
            if os.environ.get("OPENROUTER_API_KEY"):
                    "OpenRouter successfully connected."
                )  # Inform user connection worked
                # This case should ideally not happen if offer_openrouter_oauth succeeded
                # but check defensively.
                io.tool_error(
                    "OpenRouter authentication seemed successful, but the key is still missing."
                analytics.event(
                    "exit",
                    reason="OpenRouter key missing after successful OAuth for specified model",
            # OAuth failed or was declined by the user
                f"Unable to proceed without an OpenRouter API key for model '{args.model}'."
            io.offer_url(urls.models_and_keys, "Open documentation URL for more info?")
                reason="OpenRouter key missing for specified model and OAuth failed/declined",
    main_model = models.Model(
        args.model,
        weak_model=args.weak_model,
        editor_model=args.editor_model,
        editor_edit_format=args.editor_edit_format,
        verbose=args.verbose,
    # Check if deprecated remove_reasoning is set
    if main_model.remove_reasoning is not None:
            "Model setting 'remove_reasoning' is deprecated, please use 'reasoning_tag' instead."
    # Set reasoning effort and thinking tokens if specified
    if args.reasoning_effort is not None:
        # Apply if check is disabled or model explicitly supports it
        if not args.check_model_accepts_settings or (
            main_model.accepts_settings and "reasoning_effort" in main_model.accepts_settings
            main_model.set_reasoning_effort(args.reasoning_effort)
    if args.thinking_tokens is not None:
            main_model.accepts_settings and "thinking_tokens" in main_model.accepts_settings
            main_model.set_thinking_tokens(args.thinking_tokens)
    # Show warnings about unsupported settings that are being ignored
    if args.check_model_accepts_settings:
        settings_to_check = [
            {"arg": args.reasoning_effort, "name": "reasoning_effort"},
            {"arg": args.thinking_tokens, "name": "thinking_tokens"},
        for setting in settings_to_check:
            if setting["arg"] is not None and (
                not main_model.accepts_settings
                or setting["name"] not in main_model.accepts_settings
                    f"Warning: {main_model.name} does not support '{setting['name']}', ignoring."
                    f"Use --no-check-model-accepts-settings to force the '{setting['name']}'"
                    " setting."
    if args.copy_paste and args.edit_format is None:
        if main_model.edit_format in ("diff", "whole", "diff-fenced"):
            main_model.edit_format = "editor-" + main_model.edit_format
        io.tool_output("Model metadata:")
        io.tool_output(json.dumps(main_model.info, indent=4))
        io.tool_output("Model settings:")
        for attr in sorted(fields(ModelSettings), key=lambda x: x.name):
            val = getattr(main_model, attr.name)
            val = json.dumps(val, indent=4)
            io.tool_output(f"{attr.name}: {val}")
    lint_cmds = parse_lint_cmds(args.lint_cmd, io)
    if lint_cmds is None:
        analytics.event("exit", reason="Invalid lint command format")
    if args.show_model_warnings:
        problem = models.sanity_check_models(io, main_model)
        if problem:
            analytics.event("model warning", main_model=main_model)
            io.tool_output("You can skip this check with --no-show-model-warnings")
                io.offer_url(urls.model_warnings, "Open documentation url for more info?")
                analytics.event("exit", reason="Keyboard interrupt during model warnings")
            repo = GitRepo(
                fnames,
                git_dname,
                args.aiderignore,
                models=main_model.commit_message_models(),
                attribute_author=args.attribute_author,
                attribute_committer=args.attribute_committer,
                attribute_commit_message_author=args.attribute_commit_message_author,
                attribute_commit_message_committer=args.attribute_commit_message_committer,
                commit_prompt=args.commit_prompt,
                subtree_only=args.subtree_only,
                git_commit_verify=args.git_commit_verify,
                attribute_co_authored_by=args.attribute_co_authored_by,  # Pass the arg
    if not args.skip_sanity_check_repo:
        if not sanity_check_repo(repo, io):
            analytics.event("exit", reason="Repository sanity check failed")
    if repo and not args.skip_sanity_check_repo:
        num_files = len(repo.get_tracked_files())
        analytics.event("repo", num_files=num_files)
        analytics.event("no-repo")
    commands = Commands(
        voice_language=args.voice_language,
        voice_input_device=args.voice_input_device,
        voice_format=args.voice_format,
        verify_ssl=args.verify_ssl,
        editor=args.editor,
        original_read_only_fnames=read_only_fnames,
    summarizer = ChatSummary(
        [main_model.weak_model, main_model],
        args.max_chat_history_tokens or main_model.max_chat_history_tokens,
    if args.cache_prompts and args.map_refresh == "auto":
        args.map_refresh = "files"
    if not main_model.streaming:
                f"Warning: Streaming is not supported by {main_model.name}. Disabling streaming."
        args.stream = False
    if args.map_tokens is None:
        map_tokens = main_model.get_repo_map_tokens()
        map_tokens = args.map_tokens
    # Track auto-commits configuration
    analytics.event("auto_commits", enabled=bool(args.auto_commits))
        coder = Coder.create(
            main_model=main_model,
            edit_format=args.edit_format,
            io=io,
            fnames=fnames,
            read_only_fnames=read_only_fnames,
            show_diffs=args.show_diffs,
            auto_commits=args.auto_commits,
            dirty_commits=args.dirty_commits,
            map_tokens=map_tokens,
            stream=args.stream,
            use_git=args.git,
            restore_chat_history=args.restore_chat_history,
            auto_lint=args.auto_lint,
            auto_test=args.auto_test,
            lint_cmds=lint_cmds,
            test_cmd=args.test_cmd,
            summarizer=summarizer,
            analytics=analytics,
            map_refresh=args.map_refresh,
            cache_prompts=args.cache_prompts,
            map_mul_no_files=args.map_multiplier_no_files,
            num_cache_warming_pings=args.cache_keepalive_pings,
            suggest_shell_commands=args.suggest_shell_commands,
            chat_language=args.chat_language,
            commit_language=args.commit_language,
            detect_urls=args.detect_urls,
            auto_copy_context=args.copy_paste,
            auto_accept_architect=args.auto_accept_architect,
            add_gitignore_files=args.add_gitignore_files,
    except UnknownEditFormat as err:
        io.tool_error(str(err))
        io.offer_url(urls.edit_formats, "Open documentation about edit formats?")
        analytics.event("exit", reason="Unknown edit format")
        analytics.event("exit", reason="ValueError during coder creation")
    if return_coder:
        analytics.event("exit", reason="Returning coder object")
        return coder
    ignores = []
        ignores.append(str(Path(git_root) / ".gitignore"))
    if args.aiderignore:
        ignores.append(args.aiderignore)
    if args.watch_files:
        file_watcher = FileWatcher(
            coder,
            gitignores=ignores,
            root=str(Path.cwd()) if args.subtree_only else None,
        coder.file_watcher = file_watcher
    if args.copy_paste:
        analytics.event("copy-paste mode")
        ClipboardWatcher(coder.io, verbose=args.verbose)
    coder.show_announcements()
    if args.show_prompts:
        coder.cur_messages += [
            dict(role="user", content="Hello!"),
        messages = coder.format_messages().all_messages()
        utils.show_messages(messages)
        analytics.event("exit", reason="Showed prompts")
    if args.lint:
        coder.commands.cmd_lint(fnames=fnames)
    if args.test:
        if not args.test_cmd:
            io.tool_error("No --test-cmd provided.")
            analytics.event("exit", reason="No test command provided")
        coder.commands.cmd_test(args.test_cmd)
        if io.placeholder:
            coder.run(io.placeholder)
    if args.commit:
            io.tool_output("Dry run enabled, skipping commit.")
            coder.commands.cmd_commit()
    if args.lint or args.test or args.commit:
        analytics.event("exit", reason="Completed lint/test/commit")
    if args.show_repo_map:
        repo_map = coder.get_repo_map()
        if repo_map:
            io.tool_output(repo_map)
        analytics.event("exit", reason="Showed repo map")
    if args.apply:
        content = io.read_text(args.apply)
            analytics.event("exit", reason="Failed to read apply content")
        coder.partial_response_content = content
        # For testing #2879
        # from aider.coders.base_coder import all_fences
        # coder.fence = all_fences[1]
        coder.apply_updates()
        analytics.event("exit", reason="Applied updates")
    if args.apply_clipboard_edits:
        args.edit_format = main_model.editor_edit_format
        args.message = "/paste"
    if args.show_release_notes is True:
        io.tool_output(f"Opening release notes: {urls.release_notes}")
        webbrowser.open(urls.release_notes)
    elif args.show_release_notes is None and is_first_run:
        io.offer_url(
            urls.release_notes,
            "Would you like to see what's new in this version?",
            allow_never=False,
    if git_root and Path.cwd().resolve() != Path(git_root).resolve():
            "Note: in-chat filenames are always relative to the git working dir, not the current"
            " working dir."
        io.tool_output(f"Cur working dir: {Path.cwd()}")
        io.tool_output(f"Git working dir: {git_root}")
    if args.stream and args.cache_prompts:
        io.tool_warning("Cost estimates may be inaccurate when using streaming and caching.")
    if args.load:
        commands.cmd_load(args.load)
    if args.message:
        io.add_to_input_history(args.message)
            coder.run(with_message=args.message)
        except SwitchCoder:
        analytics.event("exit", reason="Completed --message")
    if args.message_file:
            message_from_file = io.read_text(args.message_file)
            coder.run(with_message=message_from_file)
            io.tool_error(f"Message file not found: {args.message_file}")
            analytics.event("exit", reason="Message file not found")
            io.tool_error(f"Error reading message file: {e}")
            analytics.event("exit", reason="Message file IO error")
        analytics.event("exit", reason="Completed --message-file")
    if args.exit:
        analytics.event("exit", reason="Exit flag set")
    analytics.event("cli session", main_model=main_model, edit_format=main_model.edit_format)
            coder.ok_to_warm_cache = bool(args.cache_keepalive_pings)
            coder.run()
            analytics.event("exit", reason="Completed main CLI coder.run")
        except SwitchCoder as switch:
            coder.ok_to_warm_cache = False
            # Set the placeholder if provided
            if hasattr(switch, "placeholder") and switch.placeholder is not None:
                io.placeholder = switch.placeholder
            kwargs = dict(io=io, from_coder=coder)
            kwargs.update(switch.kwargs)
            if "show_announcements" in kwargs:
                del kwargs["show_announcements"]
            coder = Coder.create(**kwargs)
            if switch.kwargs.get("show_announcements") is not False:
def is_first_run_of_new_version(io, verbose=False):
    """Check if this is the first run of a new version/executable combination"""
    installs_file = Path.home() / ".aider" / "installs.json"
    key = (__version__, sys.executable)
    # Never show notes for .dev versions
    if ".dev" in __version__:
            f"Checking imports for version {__version__} and executable {sys.executable}"
        io.tool_output(f"Installs file: {installs_file}")
        if installs_file.exists():
            with open(installs_file, "r") as f:
                installs = json.load(f)
                io.tool_output("Installs file exists and loaded")
            installs = {}
                io.tool_output("Installs file does not exist, creating new dictionary")
        is_first_run = str(key) not in installs
        if is_first_run:
            installs[str(key)] = True
            installs_file.parent.mkdir(parents=True, exist_ok=True)
            with open(installs_file, "w") as f:
                json.dump(installs, f, indent=4)
        return is_first_run
        io.tool_warning(f"Error checking version: {e}")
            io.tool_output(f"Full exception details: {traceback.format_exc()}")
        return True  # Safer to assume it's a first run if we hit an error
def check_and_load_imports(io, is_first_run, verbose=False):
                    "First run for this version and executable, loading imports synchronously"
                load_slow_imports(swallow=False)
                io.tool_output("Error loading required imports. Did you install aider properly?")
                io.offer_url(urls.install_properly, "Open documentation url for more info?")
                io.tool_output("Imports loaded and installs file updated")
                io.tool_output("Not first run, loading imports in background thread")
            thread = threading.Thread(target=load_slow_imports)
            thread.daemon = True
        io.tool_warning(f"Error in loading imports: {e}")
def load_slow_imports(swallow=True):
    # These imports are deferred in various ways to
    # improve startup time.
    # This func is called either synchronously or in a thread
    # depending on whether it's been run before for this version and executable.
        import httpx  # noqa: F401
        import litellm  # noqa: F401
        import networkx  # noqa: F401
        import numpy  # noqa: F401
        if not swallow:
    status = main()
