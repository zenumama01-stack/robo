"""Client for discovery based APIs.
A client library for Google's discovery based APIs.
__all__ = ["build", "build_from_document", "fix_method_name", "key2param"]
import collections.abc
# Standard library imports
import copy
from email.generator import BytesGenerator
from email.mime.multipart import MIMEMultipart
from email.mime.nonmultipart import MIMENonMultipart
import http.client as http_client
import keyword
import google.api_core.client_options
from google.auth.exceptions import MutualTLSChannelError
from google.auth.transport import mtls
from google.oauth2 import service_account
# Third-party imports
    from google.api_core import universe
    HAS_UNIVERSE = True
    HAS_UNIVERSE = False
# Local imports
from googleapiclient import _auth, mimeparse
from googleapiclient._helpers import _add_query_parameter, positional
from googleapiclient.errors import (
    HttpError,
    InvalidJsonError,
    MediaUploadSizeError,
    UnacceptableMimeTypeError,
    UnknownApiNameOrVersion,
    UnknownFileType,
from googleapiclient.http import (
    BatchHttpRequest,
    HttpMock,
    HttpMockSequence,
    HttpRequest,
    MediaFileUpload,
    MediaUpload,
    build_http,
from googleapiclient.model import JsonModel, MediaModel, RawModel
from googleapiclient.schema import Schemas
# The client library requires a version of httplib2 that supports RETRIES.
httplib2.RETRIES = 1
URITEMPLATE = re.compile("{[^}]*}")
VARNAME = re.compile("[a-zA-Z0-9_-]+")
DISCOVERY_URI = (
    "https://www.googleapis.com/discovery/v1/apis/" "{api}/{apiVersion}/rest"
V1_DISCOVERY_URI = DISCOVERY_URI
V2_DISCOVERY_URI = (
    "https://{api}.googleapis.com/$discovery/rest?" "version={apiVersion}"
DEFAULT_METHOD_DOC = "A description of how to use this function"
HTTP_PAYLOAD_METHODS = frozenset(["PUT", "POST", "PATCH"])
_MEDIA_SIZE_BIT_SHIFTS = {"KB": 10, "MB": 20, "GB": 30, "TB": 40}
BODY_PARAMETER_DEFAULT_VALUE = {"description": "The request body.", "type": "object"}
MEDIA_BODY_PARAMETER_DEFAULT_VALUE = {
    "description": (
        "The filename of the media request body, or an instance "
        "of a MediaUpload object."
    "required": False,
MEDIA_MIME_TYPE_PARAMETER_DEFAULT_VALUE = {
        "The MIME type of the media request body, or an instance "
_PAGE_TOKEN_NAMES = ("pageToken", "nextPageToken")
# Parameters controlling mTLS behavior. See https://google.aip.dev/auth/4114.
GOOGLE_API_USE_CLIENT_CERTIFICATE = "GOOGLE_API_USE_CLIENT_CERTIFICATE"
GOOGLE_API_USE_MTLS_ENDPOINT = "GOOGLE_API_USE_MTLS_ENDPOINT"
GOOGLE_CLOUD_UNIVERSE_DOMAIN = "GOOGLE_CLOUD_UNIVERSE_DOMAIN"
DEFAULT_UNIVERSE = "googleapis.com"
# Parameters accepted by the stack, but not visible via discovery.
# TODO(dhermes): Remove 'userip' in 'v2'.
STACK_QUERY_PARAMETERS = frozenset(["trace", "pp", "userip", "strict"])
STACK_QUERY_PARAMETER_DEFAULT_VALUE = {"type": "string", "location": "query"}
class APICoreVersionError(ValueError):
            "google-api-core >= 2.18.0 is required to use the universe domain feature."
# Library-specific reserved words beyond Python keywords.
RESERVED_WORDS = frozenset(["body"])
# patch _write_lines to avoid munging '\r' into '\n'
# ( https://bugs.python.org/issue18886 https://bugs.python.org/issue19003 )
class _BytesGenerator(BytesGenerator):
    _write_lines = BytesGenerator.write
def fix_method_name(name):
    """Fix method names to avoid '$' characters and reserved word conflicts.
    name = name.replace("$", "_").replace("-", "_")
    if keyword.iskeyword(name) or name in RESERVED_WORDS:
        return name + "_"
def key2param(key):
    """Converts key names into parameter names.
    For example, converting "max-results" -> "max_results"
    key = list(key)
    if not key[0].isalpha():
        result.append("x")
    for c in key:
        if c.isalnum():
            result.append(c)
            result.append("_")
    return "".join(result)
    http=None,
    discoveryServiceUrl=None,
    developerKey=None,
    model=None,
    requestBuilder=HttpRequest,
    credentials=None,
    cache_discovery=True,
    cache=None,
    client_options=None,
    adc_cert_path=None,
    adc_key_path=None,
    num_retries=1,
    static_discovery=None,
    always_use_jwt_access=False,
    """Construct a Resource for interacting with an API.
      static_discovery: Boolean, whether or not to use the static discovery docs
        included in the library. The default value for `static_discovery` depends
        on the value of `discoveryServiceUrl`. `static_discovery` will default to
        `True` when `discoveryServiceUrl` is also not provided, otherwise it will
        default to `False`.
      always_use_jwt_access: Boolean, whether always use self signed JWT for service
        account credentials. This only applies to
        google.oauth2.service_account.Credentials.
    params = {"api": serviceName, "apiVersion": version}
    # The default value for `static_discovery` depends on the value of
    # `discoveryServiceUrl`. `static_discovery` will default to `True` when
    # `discoveryServiceUrl` is also not provided, otherwise it will default to
    # `False`. This is added for backwards compatability with
    # google-api-python-client 1.x which does not support the `static_discovery`
    if static_discovery is None:
        if discoveryServiceUrl is None:
            static_discovery = True
            static_discovery = False
        discovery_http = build_http()
        discovery_http = http
    service = None
    for discovery_url in _discovery_service_uri_options(discoveryServiceUrl, version):
        requested_url = uritemplate.expand(discovery_url, params)
            content = _retrieve_discovery_doc(
                requested_url,
                discovery_http,
                cache_discovery,
                cache,
                developerKey,
                num_retries=num_retries,
                static_discovery=static_discovery,
            service = build_from_document(
                base=discovery_url,
                http=http,
                developerKey=developerKey,
                requestBuilder=requestBuilder,
                credentials=credentials,
                client_options=client_options,
                adc_cert_path=adc_cert_path,
                adc_key_path=adc_key_path,
                always_use_jwt_access=always_use_jwt_access,
            break  # exit if a service was created
        except HttpError as e:
            if e.resp.status == http_client.NOT_FOUND:
    # If discovery_http was created by this function, we are done with it
    # and can safely close it
        discovery_http.close()
    if service is None:
        raise UnknownApiNameOrVersion("name: %s  version: %s" % (serviceName, version))
        return service
def _discovery_service_uri_options(discoveryServiceUrl, version):
      Returns Discovery URIs to be used for attempting to build the API Resource.
    if discoveryServiceUrl is not None:
        return [discoveryServiceUrl]
    if version is None:
        # V1 Discovery won't work if the requested version is None
        logger.warning(
            "Discovery V1 does not support empty versions. Defaulting to V2..."
        return [V2_DISCOVERY_URI]
        return [DISCOVERY_URI, V2_DISCOVERY_URI]
def _retrieve_discovery_doc(
    http,
    static_discovery=True,
    """Retrieves the discovery_doc from cache or the internet.
        included in the library.
    from . import discovery_cache
    if cache_discovery:
        if cache is None:
            cache = discovery_cache.autodetect()
        if cache:
            content = cache.get(url)
            if content:
    # When `static_discovery=True`, use static discovery artifacts included
    # with the library
    if static_discovery:
        content = discovery_cache.get_static_doc(serviceName, version)
            raise UnknownApiNameOrVersion(
                "name: %s  version: %s" % (serviceName, version)
    actual_url = url
    # REMOTE_ADDR is defined by the CGI spec [RFC3875] as the environment
    # variable that contains the network address of the client sending the
    # request. If it exists then add that to the request for the discovery
    # document to avoid exceeding the quota on discovery requests.
    if "REMOTE_ADDR" in os.environ:
        actual_url = _add_query_parameter(url, "userIp", os.environ["REMOTE_ADDR"])
    if developerKey:
        actual_url = _add_query_parameter(url, "key", developerKey)
    logger.debug("URL being requested: GET %s", actual_url)
    # Execute this request with retries build into HttpRequest
    # Note that it will already raise an error if we don't get a 2xx response
    req = HttpRequest(http, HttpRequest.null_postproc, actual_url)
    resp, content = req.execute(num_retries=num_retries)
        content = content.decode("utf-8")
        service = json.loads(content)
    except ValueError as e:
        logger.error("Failed to parse as JSON: " + content)
        raise InvalidJsonError()
    if cache_discovery and cache:
        cache.set(url, content)
def _check_api_core_compatible_with_credentials_universe(credentials):
    if not HAS_UNIVERSE:
        credentials_universe = getattr(credentials, "universe_domain", None)
        if credentials_universe and credentials_universe != DEFAULT_UNIVERSE:
            raise APICoreVersionError
def build_from_document(
    base=None,
    future=None,
    """Create a Resource for interacting with an API.
    if client_options is None:
        client_options = google.api_core.client_options.ClientOptions()
    if isinstance(client_options, collections.abc.Mapping):
        client_options = google.api_core.client_options.from_dict(client_options)
    if http is not None:
        # if http is passed, the user cannot provide credentials
        banned_options = [
            (credentials, "credentials"),
            (client_options.credentials_file, "client_options.credentials_file"),
        for option, name in banned_options:
            if option is not None:
                    "Arguments http and {} are mutually exclusive".format(name)
    if isinstance(service, str):
        service = json.loads(service)
    elif isinstance(service, bytes):
        service = json.loads(service.decode("utf-8"))
    if "rootUrl" not in service and isinstance(http, (HttpMock, HttpMockSequence)):
        logger.error(
            "You are using HttpMock or HttpMockSequence without"
            + "having the service discovery doc in cache. Try calling "
            + "build() without mocking once first to populate the "
            + "cache."
    # If an API Endpoint is provided on client options, use that as the base URL
    base = urllib.parse.urljoin(service["rootUrl"], service["servicePath"])
    universe_domain = None
    if HAS_UNIVERSE:
        universe_domain_env = os.getenv(GOOGLE_CLOUD_UNIVERSE_DOMAIN, None)
        universe_domain = universe.determine_domain(
            client_options.universe_domain, universe_domain_env
        base = base.replace(universe.DEFAULT_UNIVERSE, universe_domain)
        client_universe = getattr(client_options, "universe_domain", None)
        if client_universe:
    audience_for_self_signed_jwt = base
    if client_options.api_endpoint:
        base = client_options.api_endpoint
    schema = Schemas(service)
    # If the http client is not specified, then we must construct an http client
    # to make requests. If the service has scopes, then we also need to setup
    # authentication.
        # Does the service require scopes?
        scopes = list(
            service.get("auth", {}).get("oauth2", {}).get("scopes", {}).keys()
        # If so, then the we need to setup authentication if no developerKey is
        if scopes and not developerKey:
            # Make sure the user didn't pass multiple credentials
            if client_options.credentials_file and credentials:
                raise google.api_core.exceptions.DuplicateCredentialArgs(
                    "client_options.credentials_file and credentials are mutually exclusive."
            # Check for credentials file via client options
            if client_options.credentials_file:
                credentials = _auth.credentials_from_file(
                    client_options.credentials_file,
                    scopes=client_options.scopes,
                    quota_project_id=client_options.quota_project_id,
            # If the user didn't pass in credentials, attempt to acquire application
            # default credentials.
            if credentials is None:
                credentials = _auth.default_credentials(
            # Check google-api-core >= 2.18.0 if credentials' universe != "googleapis.com".
            _check_api_core_compatible_with_credentials_universe(credentials)
            # The credentials need to be scoped.
            # If the user provided scopes via client_options don't override them
            if not client_options.scopes:
                credentials = _auth.with_scopes(credentials, scopes)
        # For google-auth service account credentials, enable self signed JWT if
        # always_use_jwt_access is true.
            and isinstance(credentials, service_account.Credentials)
            and always_use_jwt_access
            and hasattr(service_account.Credentials, "with_always_use_jwt_access")
            credentials = credentials.with_always_use_jwt_access(always_use_jwt_access)
            credentials._create_self_signed_jwt(audience_for_self_signed_jwt)
        # If credentials are provided, create an authorized http instance;
        # otherwise, skip authentication.
        if credentials:
            http = _auth.authorized_http(credentials)
        # If the service doesn't require scopes then there is no need for
        # Obtain client cert and create mTLS http channel if cert exists.
        client_cert_to_use = None
        if hasattr(mtls, "should_use_client_cert"):
            use_client_cert = mtls.should_use_client_cert()
            # if unsupported, fallback to reading from env var
            use_client_cert_str = os.getenv(
                "GOOGLE_API_USE_CLIENT_CERTIFICATE", "false"
            ).lower()
            use_client_cert = use_client_cert_str == "true"
            if use_client_cert_str not in ("true", "false"):
                raise MutualTLSChannelError(
                    "Unsupported GOOGLE_API_USE_CLIENT_CERTIFICATE value. Accepted values: true, false"
        if client_options and client_options.client_cert_source:
                "ClientOptions.client_cert_source is not supported, please use ClientOptions.client_encrypted_cert_source."
        if use_client_cert:
                client_options
                and hasattr(client_options, "client_encrypted_cert_source")
                and client_options.client_encrypted_cert_source
                client_cert_to_use = client_options.client_encrypted_cert_source
                adc_cert_path and adc_key_path and mtls.has_default_client_cert_source()
                client_cert_to_use = mtls.default_client_encrypted_cert_source(
                    adc_cert_path, adc_key_path
        if client_cert_to_use:
            cert_path, key_path, passphrase = client_cert_to_use()
            # The http object we built could be google_auth_httplib2.AuthorizedHttp
            # or httplib2.Http. In the first case we need to extract the wrapped
            # httplib2.Http object from google_auth_httplib2.AuthorizedHttp.
            http_channel = (
                http.http
                if google_auth_httplib2
                and isinstance(http, google_auth_httplib2.AuthorizedHttp)
                else http
            http_channel.add_certificate(key_path, cert_path, "", passphrase)
        # If user doesn't provide api endpoint via client options, decide which
        # api endpoint to use.
        if "mtlsRootUrl" in service and (
            not client_options or not client_options.api_endpoint
            mtls_endpoint = urllib.parse.urljoin(
                service["mtlsRootUrl"], service["servicePath"]
            use_mtls_endpoint = os.getenv(GOOGLE_API_USE_MTLS_ENDPOINT, "auto")
            if not use_mtls_endpoint in ("never", "auto", "always"):
                    "Unsupported GOOGLE_API_USE_MTLS_ENDPOINT value. Accepted values: never, auto, always"
            # Switch to mTLS endpoint, if environment variable is "always", or
            # environment varibable is "auto" and client cert exists.
            if use_mtls_endpoint == "always" or (
                use_mtls_endpoint == "auto" and client_cert_to_use
                if HAS_UNIVERSE and universe_domain != universe.DEFAULT_UNIVERSE:
                        f"mTLS is not supported in any universe other than {universe.DEFAULT_UNIVERSE}."
                base = mtls_endpoint
        http_credentials = getattr(http, "credentials", None)
        _check_api_core_compatible_with_credentials_universe(http_credentials)
    if model is None:
        features = service.get("features", [])
        model = JsonModel("dataWrapper" in features)
    return Resource(
        baseUrl=base,
        resourceDesc=service,
        rootDesc=service,
        schema=schema,
        universe_domain=universe_domain,
def _cast(value, schema_type):
    """Convert value to a string based on JSON Schema type.
    if schema_type == "string":
        if type(value) == type("") or type(value) == type(""):
    elif schema_type == "integer":
        return str(int(value))
    elif schema_type == "number":
        return str(float(value))
    elif schema_type == "boolean":
        return str(bool(value)).lower()
def _media_size_to_long(maxSize):
    """Convert a string media size, such as 10GB or 3TB into an integer.
    if len(maxSize) < 2:
    units = maxSize[-2:].upper()
    bit_shift = _MEDIA_SIZE_BIT_SHIFTS.get(units)
    if bit_shift is not None:
        return int(maxSize[:-2]) << bit_shift
        return int(maxSize)
def _media_path_url_from_info(root_desc, path_url):
    """Creates an absolute media path URL.
    return "%(root)supload/%(service_path)s%(path)s" % {
        "root": root_desc["rootUrl"],
        "service_path": root_desc["servicePath"],
        "path": path_url,
def _fix_up_parameters(method_desc, root_desc, http_method, schema):
    """Updates parameters of an API method with values specific to this library.
    parameters = method_desc.setdefault("parameters", {})
    # Add in the parameters common to all methods.
    for name, description in root_desc.get("parameters", {}).items():
        parameters[name] = description
    # Add in undocumented query parameters.
    for name in STACK_QUERY_PARAMETERS:
        parameters[name] = STACK_QUERY_PARAMETER_DEFAULT_VALUE.copy()
    # Add 'body' (our own reserved word) to parameters if the method supports
    # a request payload.
    if http_method in HTTP_PAYLOAD_METHODS and "request" in method_desc:
        body = BODY_PARAMETER_DEFAULT_VALUE.copy()
        body.update(method_desc["request"])
        parameters["body"] = body
    return parameters
def _fix_up_media_upload(method_desc, root_desc, path_url, parameters):
    """Adds 'media_body' and 'media_mime_type' parameters if supported by method.
    media_upload = method_desc.get("mediaUpload", {})
    accept = media_upload.get("accept", [])
    max_size = _media_size_to_long(media_upload.get("maxSize", ""))
    media_path_url = None
    if media_upload:
        media_path_url = _media_path_url_from_info(root_desc, path_url)
        parameters["media_body"] = MEDIA_BODY_PARAMETER_DEFAULT_VALUE.copy()
        parameters["media_mime_type"] = MEDIA_MIME_TYPE_PARAMETER_DEFAULT_VALUE.copy()
    return accept, max_size, media_path_url
def _fix_up_method_description(method_desc, root_desc, schema):
    """Updates a method description in a discovery document.
    path_url = method_desc["path"]
    http_method = method_desc["httpMethod"]
    method_id = method_desc["id"]
    parameters = _fix_up_parameters(method_desc, root_desc, http_method, schema)
    # Order is important. `_fix_up_media_upload` needs `method_desc` to have a
    # 'parameters' key and needs to know if there is a 'body' parameter because it
    # also sets a 'media_body' parameter.
    accept, max_size, media_path_url = _fix_up_media_upload(
        method_desc, root_desc, path_url, parameters
    return path_url, http_method, method_id, accept, max_size, media_path_url
def _fix_up_media_path_base_url(media_path_url, base_url):
    Update the media upload base url if its netloc doesn't match base url netloc.
    This can happen in case the base url was overridden by
    client_options.api_endpoint.
      media_path_url: String; the absolute URI for media upload.
      base_url: string, base URL for the API. All requests are relative to this URI.
      String; the absolute URI for media upload.
    parsed_media_url = urllib.parse.urlparse(media_path_url)
    parsed_base_url = urllib.parse.urlparse(base_url)
    if parsed_media_url.netloc == parsed_base_url.netloc:
        return media_path_url
    return urllib.parse.urlunparse(
        parsed_media_url._replace(netloc=parsed_base_url.netloc)
def _urljoin(base, url):
    """Custom urljoin replacement supporting : before / in url."""
    # In general, it's unsafe to simply join base and url. However, for
    # the case of discovery documents, we know:
    #  * base will never contain params, query, or fragment
    #  * url will never contain a scheme or net_loc.
    # In general, this means we can safely join on /; we just need to
    # ensure we end up with precisely one / joining base and url. The
    # exception here is the case of media uploads, where url will be an
    # absolute url.
    if url.startswith("http://") or url.startswith("https://"):
        return urllib.parse.urljoin(base, url)
    new_base = base if base.endswith("/") else base + "/"
    new_url = url[1:] if url.startswith("/") else url
    return new_base + new_url
# TODO(dhermes): Convert this class to ResourceMethod and make it callable
class ResourceMethodParameters(object):
    """Represents the parameters associated with a method.
    def __init__(self, method_desc):
        """Constructor for ResourceMethodParameters.
        self.argmap = {}
        self.required_params = []
        self.repeated_params = []
        self.pattern_params = {}
        self.query_params = []
        # TODO(dhermes): Change path_params to a list if the extra URITEMPLATE
        #                parsing is gotten rid of.
        self.path_params = set()
        self.param_types = {}
        self.enum_params = {}
        self.set_parameters(method_desc)
    def set_parameters(self, method_desc):
        """Populates maps and lists based on method description.
        parameters = method_desc.get("parameters", {})
        sorted_parameters = OrderedDict(sorted(parameters.items()))
        for arg, desc in sorted_parameters.items():
            param = key2param(arg)
            self.argmap[param] = arg
            if desc.get("pattern"):
                self.pattern_params[param] = desc["pattern"]
            if desc.get("enum"):
                self.enum_params[param] = desc["enum"]
            if desc.get("required"):
                self.required_params.append(param)
            if desc.get("repeated"):
                self.repeated_params.append(param)
            if desc.get("location") == "query":
                self.query_params.append(param)
            if desc.get("location") == "path":
                self.path_params.add(param)
            self.param_types[param] = desc.get("type", "string")
        # TODO(dhermes): Determine if this is still necessary. Discovery based APIs
        #                should have all path parameters already marked with
        #                'location: path'.
        for match in URITEMPLATE.finditer(method_desc["path"]):
            for namematch in VARNAME.finditer(match.group(0)):
                name = key2param(namematch.group(0))
                self.path_params.add(name)
                if name in self.query_params:
                    self.query_params.remove(name)
def createMethod(methodName, methodDesc, rootDesc, schema):
    """Creates a method for attaching to a Resource.
    methodName = fix_method_name(methodName)
        pathUrl,
        httpMethod,
        methodId,
        accept,
        mediaPathUrl,
    ) = _fix_up_method_description(methodDesc, rootDesc, schema)
    parameters = ResourceMethodParameters(methodDesc)
    def method(self, **kwargs):
        # Don't bother with doc string, it will be over-written by createMethod.
        # Validate credentials for the configured universe.
        self._validate_credentials()
        for name in kwargs:
            if name not in parameters.argmap:
                raise TypeError("Got an unexpected keyword argument {}".format(name))
        # Remove args that have a value of None.
        keys = list(kwargs.keys())
        for name in keys:
            if kwargs[name] is None:
                del kwargs[name]
        for name in parameters.required_params:
            if name not in kwargs:
                # temporary workaround for non-paging methods incorrectly requiring
                # page token parameter (cf. drive.changes.watch vs. drive.changes.list)
                if name not in _PAGE_TOKEN_NAMES or _findPageTokenName(
                    _methodProperties(methodDesc, schema, "response")
                    raise TypeError('Missing required parameter "%s"' % name)
        for name, regex in parameters.pattern_params.items():
            if name in kwargs:
                if isinstance(kwargs[name], str):
                    pvalues = [kwargs[name]]
                    pvalues = kwargs[name]
                for pvalue in pvalues:
                    if re.match(regex, pvalue) is None:
                            'Parameter "%s" value "%s" does not match the pattern "%s"'
                            % (name, pvalue, regex)
        for name, enums in parameters.enum_params.items():
                # We need to handle the case of a repeated enum
                # name differently, since we want to handle both
                # arg='value' and arg=['value1', 'value2']
                if name in parameters.repeated_params and not isinstance(
                    kwargs[name], str
                    values = kwargs[name]
                    values = [kwargs[name]]
                for value in values:
                    if value not in enums:
                            'Parameter "%s" value "%s" is not an allowed value in "%s"'
                            % (name, value, str(enums))
        actual_query_params = {}
        actual_path_params = {}
        for key, value in kwargs.items():
            to_type = parameters.param_types.get(key, "string")
            # For repeated parameters we cast each member of the list.
            if key in parameters.repeated_params and type(value) == type([]):
                cast_value = [_cast(x, to_type) for x in value]
                cast_value = _cast(value, to_type)
            if key in parameters.query_params:
                actual_query_params[parameters.argmap[key]] = cast_value
            if key in parameters.path_params:
                actual_path_params[parameters.argmap[key]] = cast_value
        body_value = kwargs.get("body", None)
        media_filename = kwargs.get("media_body", None)
        media_mime_type = kwargs.get("media_mime_type", None)
        if self._developerKey:
            actual_query_params["key"] = self._developerKey
        model = self._model
        if methodName.endswith("_media"):
            model = MediaModel()
        elif "response" not in methodDesc:
            model = RawModel()
        api_version = methodDesc.get("apiVersion", None)
        headers, params, query, body = model.request(
            headers, actual_path_params, actual_query_params, body_value, api_version
        expanded_url = uritemplate.expand(pathUrl, params)
        url = _urljoin(self._baseUrl, expanded_url + query)
        resumable = None
        multipart_boundary = ""
        if media_filename:
            # Ensure we end up with a valid MediaUpload object.
            if isinstance(media_filename, str):
                if media_mime_type is None:
                        "media_mime_type argument not specified: trying to auto-detect for %s",
                        media_filename,
                    media_mime_type, _ = mimetypes.guess_type(media_filename)
                    raise UnknownFileType(media_filename)
                if not mimeparse.best_match([media_mime_type], ",".join(accept)):
                    raise UnacceptableMimeTypeError(media_mime_type)
                media_upload = MediaFileUpload(media_filename, mimetype=media_mime_type)
            elif isinstance(media_filename, MediaUpload):
                media_upload = media_filename
                raise TypeError("media_filename must be str or MediaUpload.")
            # Check the maxSize
            if media_upload.size() is not None and media_upload.size() > maxSize > 0:
                raise MediaUploadSizeError("Media larger than: %s" % maxSize)
            # Use the media path uri for media uploads
            expanded_url = uritemplate.expand(mediaPathUrl, params)
            url = _fix_up_media_path_base_url(url, self._baseUrl)
            if media_upload.resumable():
                url = _add_query_parameter(url, "uploadType", "resumable")
                # This is all we need to do for resumable, if the body exists it gets
                # sent in the first request, otherwise an empty body is sent.
                resumable = media_upload
                # A non-resumable upload
                if body is None:
                    # This is a simple media upload
                    headers["content-type"] = media_upload.mimetype()
                    body = media_upload.getbytes(0, media_upload.size())
                    url = _add_query_parameter(url, "uploadType", "media")
                    # This is a multipart/related upload.
                    msgRoot = MIMEMultipart("related")
                    # msgRoot should not write out it's own headers
                    setattr(msgRoot, "_write_headers", lambda self: None)
                    # attach the body as one part
                    msg = MIMENonMultipart(*headers["content-type"].split("/"))
                    msg.set_payload(body)
                    msgRoot.attach(msg)
                    # attach the media as the second part
                    msg = MIMENonMultipart(*media_upload.mimetype().split("/"))
                    msg["Content-Transfer-Encoding"] = "binary"
                    payload = media_upload.getbytes(0, media_upload.size())
                    msg.set_payload(payload)
                    # encode the body: note that we can't use `as_string`, because
                    # it plays games with `From ` lines.
                    fp = io.BytesIO()
                    g = _BytesGenerator(fp, mangle_from_=False)
                    g.flatten(msgRoot, unixfrom=False)
                    body = fp.getvalue()
                    multipart_boundary = msgRoot.get_boundary()
                    headers["content-type"] = (
                        "multipart/related; " 'boundary="%s"'
                    ) % multipart_boundary
                    url = _add_query_parameter(url, "uploadType", "multipart")
        logger.debug("URL being requested: %s %s" % (httpMethod, url))
        return self._requestBuilder(
            self._http,
            model.response,
            method=httpMethod,
            body=body,
            methodId=methodId,
            resumable=resumable,
    docs = [methodDesc.get("description", DEFAULT_METHOD_DOC), "\n\n"]
    if len(parameters.argmap) > 0:
        docs.append("Args:\n")
    # Skip undocumented params and params common to all methods.
    skip_parameters = list(rootDesc.get("parameters", {}).keys())
    skip_parameters.extend(STACK_QUERY_PARAMETERS)
    all_args = list(parameters.argmap.keys())
    args_ordered = [key2param(s) for s in methodDesc.get("parameterOrder", [])]
    # Move body to the front of the line.
    if "body" in all_args:
        args_ordered.append("body")
    for name in sorted(all_args):
        if name not in args_ordered:
            args_ordered.append(name)
    for arg in args_ordered:
        if arg in skip_parameters:
        repeated = ""
        if arg in parameters.repeated_params:
            repeated = " (repeated)"
        required = ""
        if arg in parameters.required_params:
            required = " (required)"
        paramdesc = methodDesc["parameters"][parameters.argmap[arg]]
        paramdoc = paramdesc.get("description", "A parameter")
        if "$ref" in paramdesc:
            docs.append(
                ("  %s: object, %s%s%s\n    The object takes the form of:\n\n%s\n\n")
                % (
                    arg,
                    paramdoc,
                    required,
                    repeated,
                    schema.prettyPrintByName(paramdesc["$ref"]),
            paramtype = paramdesc.get("type", "string")
                "  %s: %s, %s%s%s\n" % (arg, paramtype, paramdoc, required, repeated)
        enum = paramdesc.get("enum", [])
        enumDesc = paramdesc.get("enumDescriptions", [])
        if enum and enumDesc:
            docs.append("    Allowed values\n")
            for (name, desc) in zip(enum, enumDesc):
                docs.append("      %s - %s\n" % (name, desc))
    if "response" in methodDesc:
            docs.append("\nReturns:\n  The media object as a string.\n\n    ")
            docs.append("\nReturns:\n  An object of the form:\n\n    ")
            docs.append(schema.prettyPrintSchema(methodDesc["response"]))
    setattr(method, "__doc__", "".join(docs))
    return (methodName, method)
def createNextMethod(
    pageTokenName="pageToken",
    nextPageTokenName="nextPageToken",
    isPageTokenParameter=True,
    """Creates any _next methods for attaching to a Resource.
    def methodNext(self, previous_request, previous_response):
        """Retrieves the next page of results.
          A request object that you can call 'execute()' on to request the next
        # Retrieve nextPageToken from previous_response
        # Use as pageToken in previous_request to create new request.
        nextPageToken = previous_response.get(nextPageTokenName, None)
        if not nextPageToken:
        request = copy.copy(previous_request)
        if isPageTokenParameter:
            # Replace pageToken value in URI
            request.uri = _add_query_parameter(
                request.uri, pageTokenName, nextPageToken
            logger.debug("Next page request URL: %s %s" % (methodName, request.uri))
            # Replace pageToken value in request body
            body = model.deserialize(request.body)
            body[pageTokenName] = nextPageToken
            request.body = model.serialize(body)
            request.body_size = len(request.body)
            if "content-length" in request.headers:
                del request.headers["content-length"]
            logger.debug("Next page request body: %s %s" % (methodName, body))
    return (methodName, methodNext)
class Resource(object):
    """A class for interacting with a resource."""
        requestBuilder,
        resourceDesc,
        rootDesc,
        universe_domain=universe.DEFAULT_UNIVERSE if HAS_UNIVERSE else "",
        """Build a Resource from the API description.
          universe_domain: string, the universe for the API. The default universe
          is "googleapis.com".
        self._dynamic_attrs = []
        self._http = http
        self._baseUrl = baseUrl
        self._developerKey = developerKey
        self._requestBuilder = requestBuilder
        self._resourceDesc = resourceDesc
        self._rootDesc = rootDesc
        self._schema = schema
        self._universe_domain = universe_domain
        self._credentials_validated = False
        self._set_service_methods()
    def _set_dynamic_attr(self, attr_name, value):
        """Sets an instance attribute and tracks it in a list of dynamic attributes.
        self._dynamic_attrs.append(attr_name)
        self.__dict__[attr_name] = value
        """Trim the state down to something that can be pickled.
        Uses the fact that the instance variable _dynamic_attrs holds attrs that
        will be wiped and restored on pickle serialization.
        state_dict = copy.copy(self.__dict__)
        for dynamic_attr in self._dynamic_attrs:
            del state_dict[dynamic_attr]
        del state_dict["_dynamic_attrs"]
        return state_dict
        """Reconstitute the state of the object from being pickled.
        self.__dict__.update(state)
    def __exit__(self, exc_type, exc, exc_tb):
        """Close httplib2 connections."""
        # httplib2 leaves sockets open by default.
        # Cleanup using the `close` method.
        # https://github.com/httplib2/httplib2/issues/148
        self._http.close()
    def _set_service_methods(self):
        self._add_basic_methods(self._resourceDesc, self._rootDesc, self._schema)
        self._add_nested_resources(self._resourceDesc, self._rootDesc, self._schema)
        self._add_next_methods(self._resourceDesc, self._schema)
    def _add_basic_methods(self, resourceDesc, rootDesc, schema):
        # If this is the root Resource, add a new_batch_http_request() method.
        if resourceDesc == rootDesc:
            batch_uri = "%s%s" % (
                rootDesc["rootUrl"],
                rootDesc.get("batchPath", "batch"),
            def new_batch_http_request(callback=None):
                """Create a BatchHttpRequest object based on the discovery document.
                return BatchHttpRequest(callback=callback, batch_uri=batch_uri)
            self._set_dynamic_attr("new_batch_http_request", new_batch_http_request)
        # Add basic methods to Resource
        if "methods" in resourceDesc:
            for methodName, methodDesc in resourceDesc["methods"].items():
                fixedMethodName, method = createMethod(
                    methodName, methodDesc, rootDesc, schema
                self._set_dynamic_attr(
                    fixedMethodName, method.__get__(self, self.__class__)
                # Add in _media methods. The functionality of the attached method will
                # change when it sees that the method name ends in _media.
                if methodDesc.get("supportsMediaDownload", False):
                        methodName + "_media", methodDesc, rootDesc, schema
    def _add_nested_resources(self, resourceDesc, rootDesc, schema):
        # Add in nested resources
        if "resources" in resourceDesc:
            def createResourceMethod(methodName, methodDesc):
                """Create a method on the Resource to access a nested Resource.
                def methodResource(self):
                        http=self._http,
                        baseUrl=self._baseUrl,
                        developerKey=self._developerKey,
                        requestBuilder=self._requestBuilder,
                        resourceDesc=methodDesc,
                        rootDesc=rootDesc,
                        universe_domain=self._universe_domain,
                setattr(methodResource, "__doc__", "A collection resource.")
                setattr(methodResource, "__is_resource__", True)
                return (methodName, methodResource)
            for methodName, methodDesc in resourceDesc["resources"].items():
                fixedMethodName, method = createResourceMethod(methodName, methodDesc)
    def _add_next_methods(self, resourceDesc, schema):
        # Add _next() methods if and only if one of the names 'pageToken' or
        # 'nextPageToken' occurs among the fields of both the method's response
        # type either the method's request (query parameters) or request body.
        if "methods" not in resourceDesc:
            nextPageTokenName = _findPageTokenName(
            if not nextPageTokenName:
            isPageTokenParameter = True
            pageTokenName = _findPageTokenName(methodDesc.get("parameters", {}))
            if not pageTokenName:
                isPageTokenParameter = False
                pageTokenName = _findPageTokenName(
                    _methodProperties(methodDesc, schema, "request")
            fixedMethodName, method = createNextMethod(
                methodName + "_next",
                pageTokenName,
                nextPageTokenName,
                isPageTokenParameter,
    def _validate_credentials(self):
        """Validates client's and credentials' universe domains are consistent.
            bool: True iff the configured universe domain is valid.
            UniverseMismatchError: If the configured universe domain is not valid.
        credentials = getattr(self._http, "credentials", None)
        self._credentials_validated = (
                self._credentials_validated
                or universe.compare_domains(self._universe_domain, credentials)
            if HAS_UNIVERSE
            else True
        return self._credentials_validated
def _findPageTokenName(fields):
    """Search field names for one like a page token.
    return next(
        (tokenName for tokenName in _PAGE_TOKEN_NAMES if tokenName in fields), None
def _methodProperties(methodDesc, schema, name):
    """Get properties of a field in a method description.
    desc = methodDesc.get(name, {})
    if "$ref" in desc:
        desc = schema.get(desc["$ref"], {})
    return desc.get("properties", {})
"""Automatic discovery of Python modules and packages (for inclusion in the
distribution) and other config values.
For the purposes of this module, the following nomenclature is used:
- "src-layout": a directory representing a Python project that contains a "src"
  folder. Everything under the "src" folder is meant to be included in the
  distribution when packaging the project. Example::
    .
    ├── tox.ini
    ├── pyproject.toml
    └── src/
        └── mypkg/
            ├── __init__.py
            ├── mymodule.py
            └── my_data_file.txt
- "flat-layout": a Python project that does not use "src-layout" but instead
  have a directory under the project root for each package::
- "single-module": a project that contains a single Python script direct under
  the project root (no directory used)::
    └── mymodule.py
from fnmatch import fnmatchcase
from typing import Callable, Dict, Iterator, Iterable, List, Optional, Tuple, Union
from distutils.util import convert_path
_Path = Union[str, os.PathLike]
_Filter = Callable[[str], bool]
StrIter = Iterator[str]
chain_iter = itertools.chain.from_iterable
    from setuptools import Distribution  # noqa
def _valid_name(path: _Path) -> bool:
    # Ignore invalid names that cannot be imported directly
    return os.path.basename(path).isidentifier()
class _Finder:
    """Base class that exposes functionality for module/package finders"""
    ALWAYS_EXCLUDE: Tuple[str, ...] = ()
    DEFAULT_EXCLUDE: Tuple[str, ...] = ()
    def find(
        where: _Path = '.',
        exclude: Iterable[str] = (),
        include: Iterable[str] = ('*',)
    ) -> List[str]:
        """Return a list of all Python items (packages or modules, depending on
        the finder implementation) found within directory 'where'.
        'where' is the root directory which will be searched.
        It should be supplied as a "cross-platform" (i.e. URL-style) path;
        it will be converted to the appropriate local path syntax.
        'exclude' is a sequence of names to exclude; '*' can be used
        as a wildcard in the names.
        When finding packages, 'foo.*' will exclude all subpackages of 'foo'
        (but not 'foo' itself).
        'include' is a sequence of names to include.
        If it's specified, only the named items will be included.
        If it's not specified, all found items will be included.
        'include' can contain shell style wildcard patterns just like
        'exclude'.
        exclude = exclude or cls.DEFAULT_EXCLUDE
            cls._find_iter(
                convert_path(str(where)),
                cls._build_filter(*cls.ALWAYS_EXCLUDE, *exclude),
                cls._build_filter(*include),
    def _find_iter(cls, where: _Path, exclude: _Filter, include: _Filter) -> StrIter:
    def _build_filter(*patterns: str) -> _Filter:
        Given a list of patterns, return a callable that will be true only if
        the input matches at least one of the patterns.
        return lambda name: any(fnmatchcase(name, pat) for pat in patterns)
class PackageFinder(_Finder):
    Generate a list of all Python packages found within a directory
    ALWAYS_EXCLUDE = ("ez_setup", "*__pycache__")
        All the packages found in 'where' that pass the 'include' filter, but
        not the 'exclude' filter.
        for root, dirs, files in os.walk(str(where), followlinks=True):
            # Copy dirs to iterate over it, then empty dirs.
            all_dirs = dirs[:]
            dirs[:] = []
            for dir in all_dirs:
                full_path = os.path.join(root, dir)
                rel_path = os.path.relpath(full_path, where)
                package = rel_path.replace(os.path.sep, '.')
                # Skip directory trees that are not valid packages
                if '.' in dir or not cls._looks_like_package(full_path, package):
                # Should this package be included?
                if include(package) and not exclude(package):
                    yield package
                # Keep searching subdirectories, as there may be more packages
                # down there, even if the parent was excluded.
                dirs.append(dir)
    def _looks_like_package(path: _Path, _package_name: str) -> bool:
        """Does a directory look like a package?"""
        return os.path.isfile(os.path.join(path, '__init__.py'))
class PEP420PackageFinder(PackageFinder):
    def _looks_like_package(_path: _Path, _package_name: str) -> bool:
class ModuleFinder(_Finder):
    """Find isolated Python modules.
    This function will **not** recurse subdirectories.
        for file in glob(os.path.join(where, "*.py")):
            module, _ext = os.path.splitext(os.path.basename(file))
            if not cls._looks_like_module(module):
            if include(module) and not exclude(module):
                yield module
    _looks_like_module = staticmethod(_valid_name)
# We have to be extra careful in the case of flat layout to not include files
# and directories not meant for distribution (e.g. tool-related)
class FlatLayoutPackageFinder(PEP420PackageFinder):
    _EXCLUDE = (
        "ci",
        "bin",
        "doc",
        "docs",
        "manpages",
        "news",
        "changelog",
        "test",
        "unit_test",
        "unit_tests",
        "example",
        "examples",
        "util",
        "utils",
        "python",
        "build",
        "dist",
        "venv",
        "env",
        "requirements",
        # ---- Task runners / Build tools ----
        "tasks",  # invoke
        "fabfile",  # fabric
        "site_scons",  # SCons
        # ---- Other tools ----
        "benchmark",
        "benchmarks",
        "exercise",
        "exercises",
        # ---- Hidden directories/Private packages ----
        "[._]*",
    DEFAULT_EXCLUDE = tuple(chain_iter((p, f"{p}.*") for p in _EXCLUDE))
    """Reserved package names"""
    def _looks_like_package(_path: _Path, package_name: str) -> bool:
        names = package_name.split('.')
        # Consider PEP 561
        root_pkg_is_valid = names[0].isidentifier() or names[0].endswith("-stubs")
        return root_pkg_is_valid and all(name.isidentifier() for name in names[1:])
class FlatLayoutModuleFinder(ModuleFinder):
    DEFAULT_EXCLUDE = (
        "setup",
        "conftest",
        # ---- Task runners ----
        "toxfile",
        "noxfile",
        "pavement",
        "dodo",
        "tasks",
        "fabfile",
        "[Ss][Cc]onstruct",  # SCons
        "conanfile",  # Connan: C/C++ build tool
        "manage",  # Django
        # ---- Hidden files/Private modules ----
    """Reserved top-level module names"""
def _find_packages_within(root_pkg: str, pkg_dir: _Path) -> List[str]:
    nested = PEP420PackageFinder.find(pkg_dir)
    return [root_pkg] + [".".join((root_pkg, n)) for n in nested]
class ConfigDiscovery:
    """Fill-in metadata and options that can be automatically derived
    (from other metadata/options, the file system or conventions)
    def __init__(self, distribution: "Distribution"):
        self.dist = distribution
        self._called = False
        self._disabled = False
        self._skip_ext_modules = False
    def _disable(self):
        """Internal API to disable automatic discovery"""
        self._disabled = True
    def _ignore_ext_modules(self):
        """Internal API to disregard ext_modules.
        Normally auto-discovery would not be triggered if ``ext_modules`` are set
        (this is done for backward compatibility with existing packages relying on
        ``setup.py`` or ``setup.cfg``). However, ``setuptools`` can call this function
        to ignore given ``ext_modules`` and proceed with the auto-discovery if
        ``packages`` and ``py_modules`` are not given (e.g. when using pyproject.toml
        self._skip_ext_modules = True
    def _root_dir(self) -> _Path:
        # The best is to wait until `src_root` is set in dist, before using _root_dir.
        return self.dist.src_root or os.curdir
    def _package_dir(self) -> Dict[str, str]:
        if self.dist.package_dir is None:
        return self.dist.package_dir
    def __call__(self, force=False, name=True, ignore_ext_modules=False):
        """Automatically discover missing configuration fields
        and modifies the given ``distribution`` object in-place.
        Note that by default this will only have an effect the first time the
        ``ConfigDiscovery`` object is called.
        To repeatedly invoke automatic discovery (e.g. when the project
        directory changes), please use ``force=True`` (or create a new
        ``ConfigDiscovery`` instance).
        if force is False and (self._called or self._disabled):
            # Avoid overhead of multiple calls
        self._analyse_package_layout(ignore_ext_modules)
            self.analyse_name()  # depends on ``packages`` and ``py_modules``
        self._called = True
    def _explicitly_specified(self, ignore_ext_modules: bool) -> bool:
        """``True`` if the user has specified some form of package/module listing"""
        ignore_ext_modules = ignore_ext_modules or self._skip_ext_modules
        ext_modules = not (self.dist.ext_modules is None or ignore_ext_modules)
            self.dist.packages is not None
            or self.dist.py_modules is not None
            or ext_modules
            or hasattr(self.dist, "configuration") and self.dist.configuration
            # ^ Some projects use numpy.distutils.misc_util.Configuration
    def _analyse_package_layout(self, ignore_ext_modules: bool) -> bool:
        if self._explicitly_specified(ignore_ext_modules):
            # For backward compatibility, just try to find modules/packages
            # when nothing is given
            "No `packages` or `py_modules` configuration, performing "
            "automatic discovery."
            self._analyse_explicit_layout()
            or self._analyse_src_layout()
            # flat-layout is the trickiest for discovery so it should be last
            or self._analyse_flat_layout()
    def _analyse_explicit_layout(self) -> bool:
        """The user can explicitly give a package layout via ``package_dir``"""
        package_dir = self._package_dir.copy()  # don't modify directly
        package_dir.pop("", None)  # This falls under the "src-layout" umbrella
        root_dir = self._root_dir
        if not package_dir:
        log.debug(f"`explicit-layout` detected -- analysing {package_dir}")
        pkgs = chain_iter(
            _find_packages_within(pkg, os.path.join(root_dir, parent_dir))
            for pkg, parent_dir in package_dir.items()
        self.dist.packages = list(pkgs)
        log.debug(f"discovered packages -- {self.dist.packages}")
    def _analyse_src_layout(self) -> bool:
        """Try to find all packages or modules under the ``src`` directory
        (or anything pointed by ``package_dir[""]``).
        The "src-layout" is relatively safe for automatic discovery.
        We assume that everything within is meant to be included in the
        distribution.
        If ``package_dir[""]`` is not given, but the ``src`` directory exists,
        this function will set ``package_dir[""] = "src"``.
        package_dir = self._package_dir
        src_dir = os.path.join(self._root_dir, package_dir.get("", "src"))
        if not os.path.isdir(src_dir):
        log.debug(f"`src-layout` detected -- analysing {src_dir}")
        package_dir.setdefault("", os.path.basename(src_dir))
        self.dist.package_dir = package_dir  # persist eventual modifications
        self.dist.packages = PEP420PackageFinder.find(src_dir)
        self.dist.py_modules = ModuleFinder.find(src_dir)
        log.debug(f"discovered py_modules -- {self.dist.py_modules}")
    def _analyse_flat_layout(self) -> bool:
        """Try to find all packages and modules under the project root.
        Since the ``flat-layout`` is more dangerous in terms of accidentally including
        extra files/directories, this function is more conservative and will raise an
        error if multiple packages or modules are found.
        This assumes that multi-package dists are uncommon and refuse to support that
        use case in order to be able to prevent unintended errors.
        log.debug(f"`flat-layout` detected -- analysing {self._root_dir}")
        return self._analyse_flat_packages() or self._analyse_flat_modules()
    def _analyse_flat_packages(self) -> bool:
        self.dist.packages = FlatLayoutPackageFinder.find(self._root_dir)
        top_level = remove_nested_packages(remove_stubs(self.dist.packages))
        self._ensure_no_accidental_inclusion(top_level, "packages")
        return bool(top_level)
    def _analyse_flat_modules(self) -> bool:
        self.dist.py_modules = FlatLayoutModuleFinder.find(self._root_dir)
        self._ensure_no_accidental_inclusion(self.dist.py_modules, "modules")
        return bool(self.dist.py_modules)
    def _ensure_no_accidental_inclusion(self, detected: List[str], kind: str):
        if len(detected) > 1:
            from setuptools.errors import PackageDiscoveryError
            msg = f"""Multiple top-level {kind} discovered in a flat-layout: {detected}.
            To avoid accidental inclusion of unwanted files or directories,
            setuptools will not proceed with this build.
            If you are trying to create a single distribution with multiple {kind}
            on purpose, you should not rely on automatic discovery.
            Instead, consider the following options:
            1. set up custom discovery (`find` directive with `include` or `exclude`)
            2. use a `src-layout`
            3. explicitly set `py_modules` or `packages` with a list of names
            To find more information, look for "package discovery" on setuptools docs.
            raise PackageDiscoveryError(cleandoc(msg))
    def analyse_name(self):
        """The packages/modules are the essential contribution of the author.
        Therefore the name of the distribution can be derived from them.
        if self.dist.metadata.name or self.dist.name:
            # get_name() is not reliable (can return "UNKNOWN")
        log.debug("No `name` configuration, performing automatic discovery")
            self._find_name_single_package_or_module()
            or self._find_name_from_packages()
            self.dist.metadata.name = name
            self.dist.name = name
    def _find_name_single_package_or_module(self) -> Optional[str]:
        """Exactly one module or package"""
        for field in ('packages', 'py_modules'):
            items = getattr(self.dist, field, None) or []
            if items and len(items) == 1:
                log.debug(f"Single module/package detected, name: {items[0]}")
                return items[0]
    def _find_name_from_packages(self) -> Optional[str]:
        """Try to find the root package that is not a PEP 420 namespace"""
        if not self.dist.packages:
        packages = remove_stubs(sorted(self.dist.packages, key=len))
        package_dir = self.dist.package_dir or {}
        parent_pkg = find_parent_package(packages, package_dir, self._root_dir)
        if parent_pkg:
            log.debug(f"Common parent package detected, name: {parent_pkg}")
            return parent_pkg
        log.warn("No parent package detected, impossible to derive `name`")
def remove_nested_packages(packages: List[str]) -> List[str]:
    """Remove nested packages from a list of packages.
    >>> remove_nested_packages(["a", "a.b1", "a.b2", "a.b1.c1"])
    ['a']
    >>> remove_nested_packages(["a", "b", "c.d", "c.d.e.f", "g.h", "a.a1"])
    ['a', 'b', 'c.d', 'g.h']
    pkgs = sorted(packages, key=len)
    top_level = pkgs[:]
    size = len(pkgs)
    for i, name in enumerate(reversed(pkgs)):
        if any(name.startswith(f"{other}.") for other in top_level):
            top_level.pop(size - i - 1)
    return top_level
def remove_stubs(packages: List[str]) -> List[str]:
    """Remove type stubs (:pep:`561`) from a list of packages.
    >>> remove_stubs(["a", "a.b", "a-stubs", "a-stubs.b.c", "b", "c-stubs"])
    ['a', 'a.b', 'b']
    return [pkg for pkg in packages if not pkg.split(".")[0].endswith("-stubs")]
def find_parent_package(
    packages: List[str], package_dir: Dict[str, str], root_dir: _Path
) -> Optional[str]:
    """Find the parent package that is not a namespace."""
    packages = sorted(packages, key=len)
    common_ancestors = []
    for i, name in enumerate(packages):
        if not all(n.startswith(f"{name}.") for n in packages[i+1:]):
            # Since packages are sorted by length, this condition is able
            # to find a list of all common ancestors.
            # When there is divergence (e.g. multiple root packages)
            # the list will be empty
        common_ancestors.append(name)
    for name in common_ancestors:
        pkg_path = find_package_path(name, package_dir, root_dir)
        init = os.path.join(pkg_path, "__init__.py")
        if os.path.isfile(init):
def find_package_path(name: str, package_dir: Dict[str, str], root_dir: _Path) -> str:
    """Given a package name, return the path where it should be found on
    disk, considering the ``package_dir`` option.
    >>> path = find_package_path("my.pkg", {"": "root/is/nested"}, ".")
    >>> path.replace(os.sep, "/")
    './root/is/nested/my/pkg'
    >>> path = find_package_path("my.pkg", {"my": "root/is/nested"}, ".")
    './root/is/nested/pkg'
    >>> path = find_package_path("my.pkg", {"my.pkg": "root/is/nested"}, ".")
    './root/is/nested'
    >>> path = find_package_path("other.pkg", {"my.pkg": "root/is/nested"}, ".")
    './other/pkg'
    parts = name.split(".")
    for i in range(len(parts), 0, -1):
        # Look backwards, the most specific package_dir first
        partial_name = ".".join(parts[:i])
        if partial_name in package_dir:
            parent = package_dir[partial_name]
            return os.path.join(root_dir, parent, *parts[i:])
    parent = package_dir.get("") or ""
    return os.path.join(root_dir, *parent.split("/"), *parts)
def construct_package_dir(packages: List[str], package_path: _Path) -> Dict[str, str]:
    parent_pkgs = remove_nested_packages(packages)
    prefix = Path(package_path).parts
    return {pkg: "/".join([*prefix, *pkg.split(".")]) for pkg in parent_pkgs}
