"""Discovery document tests
Unit tests for objects created from discovery documents.
from collections import defaultdict
import google.api_core.exceptions
from google.auth import __version__ as auth_version
from parameterized import parameterized
    from oauth2client import GOOGLE_TOKEN_URI
    from oauth2client.client import GoogleCredentials, OAuth2Credentials
from googleapiclient.discovery import (
    DISCOVERY_URI,
    MEDIA_BODY_PARAMETER_DEFAULT_VALUE,
    MEDIA_MIME_TYPE_PARAMETER_DEFAULT_VALUE,
    STACK_QUERY_PARAMETER_DEFAULT_VALUE,
    STACK_QUERY_PARAMETERS,
    V1_DISCOVERY_URI,
    V2_DISCOVERY_URI,
    APICoreVersionError,
    ResourceMethodParameters,
    _fix_up_media_path_base_url,
    _fix_up_media_upload,
    _fix_up_method_description,
    _fix_up_parameters,
    _urljoin,
    build_from_document,
    key2param,
from googleapiclient.discovery_cache import DISCOVERY_DOC_MAX_AGE
from googleapiclient.discovery_cache.base import Cache
    MediaIoBaseUpload,
    MediaUploadProgress,
    tunnel_patch,
DATA_DIR = os.path.join(os.path.dirname(__file__), "data")
def _reset_universe_domain(credentials, universe_domain=None):
    if hasattr(credentials, "universe_domain"):
        credentials.universe_domain = universe_domain
    for name in list(expected_query.keys()):
    for name in list(actual_query.keys()):
def assert_discovery_uri(testcase, actual, service_name, version, discovery):
    """Assert that discovery URI used was the one that was expected
    for a given service and version."""
    params = {"api": service_name, "apiVersion": version}
    expanded_requested_uri = uritemplate.expand(discovery, params)
    assertUrisEqual(testcase, expanded_requested_uri, actual)
def validate_discovery_requests(testcase, http_mock, service_name, version, discovery):
    """Validates that there have > 0 calls to Http Discovery
     and that LAST discovery URI used was the one that was expected
    testcase.assertTrue(len(http_mock.request_sequence) > 0)
    if len(http_mock.request_sequence) > 0:
        actual_uri = http_mock.request_sequence[-1][0]
        assert_discovery_uri(testcase, actual_uri, service_name, version, discovery)
def datafile(filename):
    return os.path.join(DATA_DIR, filename)
def read_datafile(filename, mode="r"):
    with open(datafile(filename), mode=mode) as f:
        return f.read()
def parse_version_to_tuple(version_string):
    """Safely converts a semantic version string to a comparable tuple of integers.
    Example: "4.25.8" -> (4, 25, 8)
    Ignores non-numeric parts and handles common version formats.
        version_string: Version string in the format "x.y.z" or "x.y.z<suffix>"
        Tuple of integers for the parsed version string.
    parts = []
    for part in version_string.split("."):
            parts.append(int(part))
            # If it's a non-numeric part (e.g., '1.0.0b1' -> 'b1'), stop here.
            # This is a simplification compared to 'packaging.parse_version', but sufficient
            # for comparing strictly numeric semantic versions.
    return tuple(parts)
class SetupHttplib2(unittest.TestCase):
    def test_retries(self):
        # Merely loading googleapiclient.discovery should set the RETRIES to 1.
        self.assertEqual(1, httplib2.RETRIES)
class Utilities(unittest.TestCase):
        self.zoo_root_desc = json.loads(read_datafile("zoo.json", "r"))
        self.zoo_get_method_desc = self.zoo_root_desc["methods"]["query"]
        self.zoo_animals_resource = self.zoo_root_desc["resources"]["animals"]
        self.zoo_insert_method_desc = self.zoo_animals_resource["methods"]["insert"]
        self.zoo_schema = Schemas(self.zoo_root_desc)
    def test_key2param(self):
        self.assertEqual("max_results", key2param("max-results"))
        self.assertEqual("x007_bond", key2param("007-bond"))
    def _base_fix_up_parameters_test(self, method_desc, http_method, root_desc, schema):
        self.assertEqual(method_desc["httpMethod"], http_method)
        method_desc_copy = copy.deepcopy(method_desc)
        self.assertEqual(method_desc, method_desc_copy)
        parameters = _fix_up_parameters(
            method_desc_copy, root_desc, http_method, schema
        self.assertNotEqual(method_desc, method_desc_copy)
        for param_name in STACK_QUERY_PARAMETERS:
                STACK_QUERY_PARAMETER_DEFAULT_VALUE, parameters[param_name]
        for param_name, value in root_desc.get("parameters", {}).items():
            self.assertEqual(value, parameters[param_name])
    def test_fix_up_parameters_get(self):
        parameters = self._base_fix_up_parameters_test(
            self.zoo_get_method_desc, "GET", self.zoo_root_desc, self.zoo_schema
        # Since http_method is 'GET'
        self.assertFalse("body" in parameters)
    def test_fix_up_parameters_insert(self):
            self.zoo_insert_method_desc, "POST", self.zoo_root_desc, self.zoo_schema
        body = {"description": "The request body.", "type": "object", "$ref": "Animal"}
        self.assertEqual(parameters["body"], body)
    def test_fix_up_parameters_check_body(self):
        dummy_root_desc = {}
        dummy_schema = {
            "Request": {
                    "description": "Required. Dummy parameter.",
        no_payload_http_method = "DELETE"
        with_payload_http_method = "PUT"
        invalid_method_desc = {"response": "Who cares"}
        valid_method_desc = {
            "request": {"key1": "value1", "key2": "value2", "$ref": "Request"}
            invalid_method_desc, dummy_root_desc, no_payload_http_method, dummy_schema
            valid_method_desc, dummy_root_desc, no_payload_http_method, dummy_schema
            invalid_method_desc, dummy_root_desc, with_payload_http_method, dummy_schema
            valid_method_desc, dummy_root_desc, with_payload_http_method, dummy_schema
        body = {
            "description": "The request body.",
            "$ref": "Request",
            "key1": "value1",
            "key2": "value2",
    def test_fix_up_parameters_optional_body(self):
        # Request with no parameters
        dummy_schema = {"Request": {"properties": {}}}
        method_desc = {"request": {"$ref": "Request"}}
        parameters = _fix_up_parameters(method_desc, {}, "POST", dummy_schema)
    def _base_fix_up_method_description_test(
        method_desc,
        initial_parameters,
        final_parameters,
        final_accept,
        final_max_size,
        final_media_path_url,
        fake_root_desc = {
            "rootUrl": "http://root/",
            "servicePath": "fake/",
            "mtlsRootUrl": "http://root/",
        fake_path_url = "fake-path/"
            method_desc, fake_root_desc, fake_path_url, initial_parameters
        self.assertEqual(accept, final_accept)
        self.assertEqual(max_size, final_max_size)
        self.assertEqual(media_path_url, final_media_path_url)
        self.assertEqual(initial_parameters, final_parameters)
    def test_fix_up_media_upload_no_initial_invalid(self):
        self._base_fix_up_method_description_test(
            invalid_method_desc, {}, {}, [], 0, None
    def test_fix_up_media_upload_no_initial_valid_minimal(self):
        valid_method_desc = {"mediaUpload": {"accept": []}}
        final_parameters = {
            "media_body": MEDIA_BODY_PARAMETER_DEFAULT_VALUE,
            "media_mime_type": MEDIA_MIME_TYPE_PARAMETER_DEFAULT_VALUE,
            valid_method_desc,
            "http://root/upload/fake/fake-path/",
    def test_fix_up_media_upload_no_initial_valid_full(self):
        valid_method_desc = {"mediaUpload": {"accept": ["*/*"], "maxSize": "10GB"}}
        ten_gb = 10 * 2**30
            ["*/*"],
            ten_gb,
    def test_fix_up_media_upload_with_initial_invalid(self):
        initial_parameters = {"body": {}}
            invalid_method_desc, initial_parameters, initial_parameters, [], 0, None
    def test_fix_up_media_upload_with_initial_valid_minimal(self):
            "body": {},
    def test_fix_up_media_upload_with_initial_valid_full(self):
    def test_fix_up_method_description_get(self):
        result = _fix_up_method_description(
            self.zoo_get_method_desc, self.zoo_root_desc, self.zoo_schema
        path_url = "query"
        http_method = "GET"
        method_id = "bigquery.query"
        accept = []
        max_size = 0
            result, (path_url, http_method, method_id, accept, max_size, media_path_url)
    def test_fix_up_method_description_insert(self):
            self.zoo_insert_method_desc, self.zoo_root_desc, self.zoo_schema
        path_url = "animals"
        http_method = "POST"
        method_id = "zoo.animals.insert"
        accept = ["image/png"]
        max_size = 1024
        media_path_url = "https://www.googleapis.com/upload/zoo/v1/animals"
    def test_fix_up_media_path_base_url_same_netloc(self):
        result = _fix_up_media_path_base_url(
            "https://www.googleapis.com/upload/foo",
            "https://www.googleapis.com/upload/bar",
        self.assertEqual(result, "https://www.googleapis.com/upload/foo")
    def test_fix_up_media_path_base_url_different_netloc(self):
            "https://www.example.com/upload/bar",
        self.assertEqual(result, "https://www.example.com/upload/foo")
    def test_urljoin(self):
        # We want to exhaustively test various URL combinations.
        simple_bases = ["https://www.googleapis.com", "https://www.googleapis.com/"]
        long_urls = ["foo/v1/bar:custom?alt=json", "/foo/v1/bar:custom?alt=json"]
        long_bases = [
            "https://www.googleapis.com/foo/v1",
            "https://www.googleapis.com/foo/v1/",
        simple_urls = ["bar:custom?alt=json", "/bar:custom?alt=json"]
        final_url = "https://www.googleapis.com/foo/v1/bar:custom?alt=json"
        for base, url in itertools.product(simple_bases, long_urls):
            self.assertEqual(final_url, _urljoin(base, url))
        for base, url in itertools.product(long_bases, simple_urls):
    def test_ResourceMethodParameters_zoo_get(self):
        parameters = ResourceMethodParameters(self.zoo_get_method_desc)
        param_types = {
            "a": "any",
            "b": "boolean",
            "e": "string",
            "er": "string",
            "i": "integer",
            "n": "number",
            "o": "object",
            "q": "string",
            "rr": "string",
        keys = list(param_types.keys())
        self.assertEqual(parameters.argmap, dict((key, key) for key in keys))
        self.assertEqual(parameters.required_params, [])
        self.assertEqual(sorted(parameters.repeated_params), ["er", "rr"])
        self.assertEqual(parameters.pattern_params, {"rr": "[a-z]+"})
            sorted(parameters.query_params),
            ["a", "b", "e", "er", "i", "n", "o", "q", "rr"],
        self.assertEqual(parameters.path_params, set())
        self.assertEqual(parameters.param_types, param_types)
        enum_params = {"e": ["foo", "bar"], "er": ["one", "two", "three"]}
        self.assertEqual(parameters.enum_params, enum_params)
    def test_ResourceMethodParameters_zoo_animals_patch(self):
        method_desc = self.zoo_animals_resource["methods"]["patch"]
        parameters = ResourceMethodParameters(method_desc)
        param_types = {"name": "string"}
        self.assertEqual(parameters.required_params, ["name"])
        self.assertEqual(parameters.repeated_params, [])
        self.assertEqual(parameters.pattern_params, {})
        self.assertEqual(parameters.query_params, [])
        self.assertEqual(parameters.path_params, set(["name"]))
        self.assertEqual(parameters.enum_params, {})
class Discovery(unittest.TestCase):
    def test_discovery_http_is_closed(self):
        http = HttpMock(datafile("malformed.json"), {"status": "200"})
        service = build("plus", "v1", credentials=mock.sentinel.credentials)
        http.close.assert_called_once()
class DiscoveryErrors(unittest.TestCase):
    def test_tests_should_be_run_with_strict_positional_enforcement(self):
            plus = build("plus", "v1", None, static_discovery=False)
            self.fail("should have raised a TypeError exception over missing http=.")
    def test_failed_to_parse_discovery_json(self):
        self.http = HttpMock(datafile("malformed.json"), {"status": "200"})
            plus = build(
                http=self.http,
                cache_discovery=False,
                static_discovery=False,
            self.fail("should have raised an exception over malformed JSON.")
        except InvalidJsonError:
    def test_unknown_api_name_or_version(self):
        http = HttpMockSequence(
                ({"status": "404"}, read_datafile("zoo.json", "rb")),
        with self.assertRaises(UnknownApiNameOrVersion):
            plus = build("plus", "v1", http=http, cache_discovery=False)
    def test_credentials_and_http_mutually_exclusive(self):
        http = HttpMock(datafile("plus.json"), {"status": "200"})
                credentials=mock.sentinel.credentials,
    def test_credentials_file_and_http_mutually_exclusive(self):
                client_options=google.api_core.client_options.ClientOptions(
                    credentials_file="credentials.json"
    def test_credentials_and_credentials_file_mutually_exclusive(self):
        with self.assertRaises(google.api_core.exceptions.DuplicateCredentialArgs):
class DiscoveryFromDocument(unittest.TestCase):
    MOCK_CREDENTIALS = mock.Mock(spec=google.auth.credentials.Credentials)
    _reset_universe_domain(MOCK_CREDENTIALS)
    def test_can_build_from_local_document(self):
        discovery = read_datafile("plus.json")
        plus = build_from_document(
            base="https://www.googleapis.com/",
            credentials=self.MOCK_CREDENTIALS,
        self.assertIsNotNone(plus)
        self.assertTrue(hasattr(plus, "activities"))
    def test_can_build_from_local_deserialized_document(self):
        discovery = json.loads(discovery)
    def test_building_with_base_remembers_base(self):
        base = "https://www.example.com/"
            discovery, base=base, credentials=self.MOCK_CREDENTIALS
        self.assertEqual("https://www.googleapis.com/plus/v1/", plus._baseUrl)
    def test_building_with_optional_http_with_authorization(self):
        # plus service requires Authorization, hence we expect to see AuthorizedHttp object here
        self.assertIsInstance(plus._http, google_auth_httplib2.AuthorizedHttp)
        self.assertIsInstance(plus._http.http, httplib2.Http)
        self.assertIsInstance(plus._http.http.timeout, int)
        self.assertGreater(plus._http.http.timeout, 0)
    def test_building_with_optional_http_with_no_authorization(self):
        # Cleanup auth field, so we would use plain http client
        discovery["auth"] = {}
        discovery = json.dumps(discovery)
            discovery, base="https://www.googleapis.com/", credentials=None
        # plus service requires Authorization
        self.assertIsInstance(plus._http, httplib2.Http)
        self.assertIsInstance(plus._http.timeout, int)
        self.assertGreater(plus._http.timeout, 0)
    def test_building_with_explicit_http(self):
        http = HttpMock()
            discovery, base="https://www.googleapis.com/", http=http
        self.assertEqual(plus._http, http)
    def test_building_with_developer_key_skips_adc(self):
            discovery, base="https://www.googleapis.com/", developerKey="123"
        # It should not be an AuthorizedHttp, because that would indicate that
        # application default credentials were used.
        self.assertNotIsInstance(plus._http, google_auth_httplib2.AuthorizedHttp)
    def test_building_with_context_manager(self):
        with mock.patch("httplib2.Http") as http:
            with build_from_document(
            ) as plus:
            plus._http.http.close.assert_called_once()
    def test_resource_close(self):
        with mock.patch("httplib2.Http", autospec=True) as httplib2_http:
            http = httplib2_http()
            plus.close()
    def test_resource_close_authorized_http(self):
        with mock.patch("google_auth_httplib2.AuthorizedHttp", autospec=True):
            plus._http.close.assert_called_once()
    def test_api_endpoint_override_from_client_options(self):
        api_endpoint = "https://foo.googleapis.com/"
        options = google.api_core.client_options.ClientOptions(
            api_endpoint=api_endpoint
            discovery, client_options=options, credentials=self.MOCK_CREDENTIALS
        self.assertEqual(plus._baseUrl, api_endpoint)
    def test_api_endpoint_override_from_client_options_mapping_object(self):
        mapping_object = defaultdict(str)
        mapping_object["api_endpoint"] = api_endpoint
            discovery, client_options=mapping_object, credentials=self.MOCK_CREDENTIALS
    def test_api_endpoint_override_from_client_options_dict(self):
            client_options={"api_endpoint": api_endpoint},
    def test_scopes_from_client_options(self):
        with mock.patch("googleapiclient._auth.default_credentials") as default:
            _reset_universe_domain(default.return_value)
                client_options={"scopes": ["1", "2"]},
    def test_quota_project_from_client_options(self):
                    quota_project_id="my-project"
    def test_credentials_file_from_client_options(self):
        with mock.patch("googleapiclient._auth.credentials_from_file") as default:
    def test_self_signed_jwt_enabled(self):
        service_account_file_path = os.path.join(DATA_DIR, "service_account.json")
        creds = google.oauth2.service_account.Credentials.from_service_account_file(
            service_account_file_path
        discovery = read_datafile("logging.json")
            "google.oauth2.service_account.Credentials._create_self_signed_jwt"
        ) as _create_self_signed_jwt:
            build_from_document(
                credentials=creds,
                always_use_jwt_access=True,
            _create_self_signed_jwt.assert_called_with(
                "https://logging.googleapis.com/"
    def test_self_signed_jwt_disabled(self):
            _create_self_signed_jwt.assert_not_called()
REGULAR_ENDPOINT = "https://www.googleapis.com/plus/v1/"
MTLS_ENDPOINT = "https://www.mtls.googleapis.com/plus/v1/"
CONFIG_DATA_WITH_WORKLOAD = {
    "version": 1,
    "cert_configs": {
        "workload": {
            "cert_path": "path/to/cert/file",
            "key_path": "path/to/key/file",
CONFIG_DATA_WITHOUT_WORKLOAD = {
    "cert_configs": {},
class DiscoveryFromDocumentMutualTLS(unittest.TestCase):
    ADC_CERT_PATH = "adc_cert_path"
    ADC_KEY_PATH = "adc_key_path"
    ADC_PASSPHRASE = "adc_passphrase"
    def check_http_client_cert(self, resource, has_client_cert="false"):
        if isinstance(resource._http, google_auth_httplib2.AuthorizedHttp):
            certs = list(resource._http.http.certificates.iter(""))
            certs = list(resource._http.certificates.iter(""))
        if has_client_cert == "true":
            self.assertEqual(len(certs), 1)
                certs[0], (self.ADC_KEY_PATH, self.ADC_CERT_PATH, self.ADC_PASSPHRASE)
            self.assertEqual(len(certs), 0)
    def client_encrypted_cert_source(self):
        return self.ADC_CERT_PATH, self.ADC_KEY_PATH, self.ADC_PASSPHRASE
    @parameterized.expand(
            ("never", "true"),
            ("auto", "true"),
            ("always", "true"),
            ("never", "false"),
            ("auto", "false"),
            ("always", "false"),
    def test_mtls_not_trigger_if_http_provided(self, use_mtls_env, use_client_cert):
        with mock.patch.dict(
            "os.environ", {"GOOGLE_API_USE_MTLS_ENDPOINT": use_mtls_env}
                "os.environ", {"GOOGLE_API_USE_CLIENT_CERTIFICATE": use_client_cert}
                plus = build_from_document(discovery, http=httplib2.Http())
                self.assertEqual(plus._baseUrl, REGULAR_ENDPOINT)
                self.check_http_client_cert(plus, has_client_cert="false")
    def test_exception_with_client_cert_source(self, use_mtls_env, use_client_cert):
                with self.assertRaises(MutualTLSChannelError):
                        client_options={"client_cert_source": mock.Mock()},
            ("never", "true", REGULAR_ENDPOINT),
            ("auto", "true", MTLS_ENDPOINT),
            ("always", "true", MTLS_ENDPOINT),
            ("never", "false", REGULAR_ENDPOINT),
            ("auto", "false", REGULAR_ENDPOINT),
            ("always", "false", MTLS_ENDPOINT),
    def test_mtls_with_provided_client_cert(
        self, use_mtls_env, use_client_cert, base_url
                    client_options={
                        "client_encrypted_cert_source": self.client_encrypted_cert_source
                self.check_http_client_cert(plus, has_client_cert=use_client_cert)
                self.assertEqual(plus._baseUrl, base_url)
            ("never", "", CONFIG_DATA_WITH_WORKLOAD, REGULAR_ENDPOINT),
            ("auto", "", CONFIG_DATA_WITH_WORKLOAD, MTLS_ENDPOINT),
            ("always", "", CONFIG_DATA_WITH_WORKLOAD, MTLS_ENDPOINT),
            ("never", "", CONFIG_DATA_WITHOUT_WORKLOAD, REGULAR_ENDPOINT),
            ("auto", "", CONFIG_DATA_WITHOUT_WORKLOAD, REGULAR_ENDPOINT),
            ("always", "", CONFIG_DATA_WITHOUT_WORKLOAD, MTLS_ENDPOINT),
    @pytest.mark.skipif(
        parse_version_to_tuple(auth_version) < (2, 43, 0),
        reason="automatic mtls enablement when supported certs present only"
        "enabled in google-auth<=2.43.0",
    def test_mtls_with_provided_client_cert_unset_environment_variable(
        self, use_mtls_env, use_client_cert, config_data, base_url
        """Tests that mTLS is correctly handled when a client certificate is provided.
        This test case verifies that when a client certificate is explicitly provided
        via `client_options` and GOOGLE_API_USE_CLIENT_CERTIFICATE is unset, the
        discovery document build process correctly configures the base URL for mTLS
        or regular endpoints based on the `GOOGLE_API_USE_MTLS_ENDPOINT` environment variable.
        if hasattr(google.auth.transport.mtls, "should_use_client_cert"):
            config_filename = "mock_certificate_config.json"
            config_file_content = json.dumps(config_data)
            m = mock.mock_open(read_data=config_file_content)
                    with mock.patch("builtins.open", m):
                            "os.environ",
                            {"GOOGLE_API_CERTIFICATE_CONFIG": config_filename},
    def test_endpoint_not_switch(self, use_mtls_env, use_client_cert):
        # Test endpoint is not switched if user provided api endpoint
                        "api_endpoint": "https://foo.googleapis.com",
                        "client_encrypted_cert_source": self.client_encrypted_cert_source,
                self.assertEqual(plus._baseUrl, "https://foo.googleapis.com")
    @mock.patch(
        "google.auth.transport.mtls.has_default_client_cert_source", autospec=True
        "google.auth.transport.mtls.default_client_encrypted_cert_source", autospec=True
    def test_mtls_with_default_client_cert(
        use_mtls_env,
        use_client_cert,
        base_url,
        default_client_encrypted_cert_source,
        has_default_client_cert_source,
        has_default_client_cert_source.return_value = True
        default_client_encrypted_cert_source.return_value = (
            self.client_encrypted_cert_source
                    adc_cert_path=self.ADC_CERT_PATH,
                    adc_key_path=self.ADC_KEY_PATH,
    def test_mtls_with_default_client_cert_with_unset_environment_variable(
        config_data,
        """Tests mTLS handling when falling back to a default client certificate.
        This test simulates the scenario where no client certificate is explicitly
        provided, and the library successfully finds and uses a default client
        certificate when GOOGLE_API_USE_CLIENT_CERTIFICATE is unset. It mocks the
        default certificate discovery process and checks that the base URL is
        correctly set for mTLS or regular endpoints depending on the
        `GOOGLE_API_USE_MTLS_ENDPOINT` environment variable.
            ("auto", "true", REGULAR_ENDPOINT),
    def test_mtls_with_no_client_cert(
        self, use_mtls_env, use_client_cert, base_url, has_default_client_cert_source
        has_default_client_cert_source.return_value = False
class DiscoveryFromHttp(unittest.TestCase):
        self.old_environ = os.environ.copy()
        os.environ = self.old_environ
    def test_userip_is_added_to_discovery_uri(self):
        # build() will raise an HttpError on a 400, use this to pick the request uri
        # out of the raised exception.
        os.environ["REMOTE_ADDR"] = "10.0.0.1"
                [({"status": "400"}, read_datafile("zoo.json", "rb"))]
            zoo = build(
                "zoo",
                discoveryServiceUrl="http://example.com",
            self.fail("Should have raised an exception.")
            self.assertEqual(e.uri, "http://example.com?userIp=10.0.0.1")
    def test_userip_missing_is_not_added_to_discovery_uri(self):
            self.assertEqual(e.uri, "http://example.com")
    def test_key_is_added_to_discovery_uri(self):
                developerKey="foo",
            self.assertEqual(e.uri, "http://example.com?key=foo")
    def test_discovery_loading_from_v2_discovery_uri(self):
                ({"status": "404"}, "Not found"),
                ({"status": "200"}, read_datafile("zoo.json", "rb")),
            "zoo", "v1", http=http, cache_discovery=False, static_discovery=False
        self.assertTrue(hasattr(zoo, "animals"))
            client_options=options,
        self.assertEqual(zoo._baseUrl, api_endpoint)
    def test_discovery_with_empty_version_uses_v2(self):
            version=None,
        validate_discovery_requests(self, http, "zoo", None, V2_DISCOVERY_URI)
    def test_discovery_with_empty_version_preserves_custom_uri(self):
        custom_discovery_uri = "https://foo.bar/$discovery"
            discoveryServiceUrl=custom_discovery_uri,
        validate_discovery_requests(self, http, "zoo", None, custom_discovery_uri)
    def test_discovery_with_valid_version_uses_v1(self):
            version="v123",
        validate_discovery_requests(self, http, "zoo", "v123", V1_DISCOVERY_URI)
class DiscoveryRetryFromHttp(unittest.TestCase):
    def test_repeated_500_retries_and_fails(self):
                ({"status": "500"}, read_datafile("500.json", "rb")),
                ({"status": "503"}, read_datafile("503.json", "rb")),
        with self.assertRaises(HttpError):
            with mock.patch("time.sleep") as mocked_sleep:
        mocked_sleep.assert_called_once()
        # We also want to verify that we stayed with v1 discovery
        validate_discovery_requests(self, http, "zoo", "v1", V1_DISCOVERY_URI)
    def test_v2_repeated_500_retries_and_fails(self):
                ({"status": "404"}, "Not found"),  # last v1 discovery call
        # We also want to verify that we switched to v2 discovery
        validate_discovery_requests(self, http, "zoo", "v1", V2_DISCOVERY_URI)
    def test_single_500_retries_and_succeeds(self):
    def test_single_500_then_404_retries_and_succeeds(self):
class DiscoveryFromAppEngineCache(unittest.TestCase):
        os.environ["GAE_ENV"] = "standard"
    def test_appengine_memcache(self):
        # Hack module import
        self.orig_import = __import__
        self.mocked_api = mock.MagicMock()
        def import_mock(name, *args, **kwargs):
            if name == "google.appengine.api":
                return self.mocked_api
            return self.orig_import(name, *args, **kwargs)
        import_fullname = "__builtin__.__import__"
        if sys.version_info[0] >= 3:
            import_fullname = "builtins.__import__"
        with mock.patch(import_fullname, side_effect=import_mock):
            namespace = "google-api-client"
            self.http = HttpMock(datafile("plus.json"), {"status": "200"})
            self.mocked_api.memcache.get.return_value = None
            plus = build("plus", "v1", http=self.http, static_discovery=False)
            # memcache.get is called once
            url = "https://www.googleapis.com/discovery/v1/apis/plus/v1/rest"
            self.mocked_api.memcache.get.assert_called_once_with(
                url, namespace=namespace
            # memcache.set is called once
            content = read_datafile("plus.json")
            self.mocked_api.memcache.set.assert_called_once_with(
                url, content, time=DISCOVERY_DOC_MAX_AGE, namespace=namespace
            # Returns the cached content this time.
            self.mocked_api.memcache.get.return_value = content
            # Make sure the contents are returned from the cache.
            # (Otherwise it should through an error)
            self.http = HttpMock(None, {"status": "200"})
            # memcache.get is called twice
            self.mocked_api.memcache.get.assert_has_calls(
                    mock.call(url, namespace=namespace),
            # memcahce.set is called just once
class DiscoveryFromStaticDocument(unittest.TestCase):
    def test_retrieve_from_local_when_static_discovery_true(self):
        http = HttpMockSequence([({"status": "400"}, "")])
        drive = build(
            "drive", "v3", http=http, cache_discovery=False, static_discovery=True
        self.assertIsNotNone(drive)
        self.assertTrue(hasattr(drive, "files"))
    def test_retrieve_from_internet_when_static_discovery_false(self):
                "drive", "v3", http=http, cache_discovery=False, static_discovery=False
    def test_unknown_api_when_static_discovery_true(self):
            build("doesnotexist", "v3", cache_discovery=False, static_discovery=True)
class DictCache(Cache):
        self.d = {}
        return self.d.get(url, None)
        self.d[url] = content
    def contains(self, url):
        return url in self.d
class DiscoveryFromFileCache(unittest.TestCase):
    def test_file_based_cache(self):
        cache = mock.Mock(wraps=DictCache())
            "googleapiclient.discovery_cache.autodetect", return_value=cache
            # cache.get is called once
            cache.get.assert_called_once_with(url)
            # cache.set is called once
            cache.set.assert_called_once_with(url, content)
            # Make sure there is a cache entry for the plus v1 discovery doc.
            self.assertTrue(cache.contains(url))
            # cache.get is called twice
            cache.get.assert_has_calls([mock.call(url), mock.call(url)])
            # cahce.set is called just once
    def test_method_error_checking(self):
        # Missing required parameters
            plus.activities().list()
            self.fail()
        except TypeError as e:
            self.assertTrue("Missing" in str(e))
        # Missing required parameters even if supplied as None.
            plus.activities().list(collection=None, userId=None)
        # Parameter doesn't match regex
            plus.activities().list(collection="not_a_collection_name", userId="me")
            self.assertTrue("not an allowed value" in str(e))
        # Unexpected parameter
            plus.activities().list(flubber=12)
            self.assertTrue("unexpected" in str(e))
    def _check_query_types(self, request):
        q = urllib.parse.parse_qs(parsed.query)
        self.assertEqual(q["q"], ["foo"])
        self.assertEqual(q["i"], ["1"])
        self.assertEqual(q["n"], ["1.0"])
        self.assertEqual(q["b"], ["false"])
        self.assertEqual(q["a"], ["[1, 2, 3]"])
        self.assertEqual(q["o"], ["{'a': 1}"])
        self.assertEqual(q["e"], ["bar"])
    def test_type_coercion(self):
        http = HttpMock(datafile("zoo.json"), {"status": "200"})
        zoo = build("zoo", "v1", http=http, static_discovery=False)
        request = zoo.query(
            q="foo", i=1.0, n=1.0, b=0, a=[1, 2, 3], o={"a": 1}, e="bar"
        self._check_query_types(request)
            q="foo", i=1, n=1, b=False, a=[1, 2, 3], o={"a": 1}, e="bar"
            q="foo", i="1", n="1", b="", a=[1, 2, 3], o={"a": 1}, e="bar", er="two"
            q="foo",
            i="1",
            n="1",
            b="",
            a=[1, 2, 3],
            o={"a": 1},
            e="bar",
            er=["one", "three"],
            rr=["foo", "bar"],
        # Five is right out.
        self.assertRaises(TypeError, zoo.query, er=["one", "five"])
    def test_optional_stack_query_parameters(self):
        request = zoo.query(trace="html", fields="description")
        self.assertEqual(q["trace"], ["html"])
        self.assertEqual(q["fields"], ["description"])
    def test_string_params_value_of_none_get_dropped(self):
        request = zoo.query(trace=None, fields="description")
        self.assertFalse("trace" in q)
    def test_model_added_query_parameters(self):
        request = zoo.animals().get(name="Lion")
        self.assertEqual(q["alt"], ["json"])
        self.assertEqual(request.headers["accept"], "application/json")
    def test_fallback_to_raw_model(self):
        request = zoo.animals().getmedia(name="Lion")
        self.assertTrue("alt" not in q)
        self.assertEqual(request.headers["accept"], "*/*")
    def test_patch(self):
        request = zoo.animals().patch(name="lion", body='{"description": "foo"}')
        self.assertEqual(request.method, "PATCH")
    def test_batch_request_from_discovery(self):
        self.http = HttpMock(datafile("zoo.json"), {"status": "200"})
        # zoo defines a batchPath
        zoo = build("zoo", "v1", http=self.http, static_discovery=False)
        batch_request = zoo.new_batch_http_request()
            batch_request._batch_uri, "https://www.googleapis.com/batchZoo"
    def test_batch_request_from_default(self):
        # plus does not define a batchPath
            "plus", "v1", http=self.http, cache_discovery=False, static_discovery=False
        batch_request = plus.new_batch_http_request()
        self.assertEqual(batch_request._batch_uri, "https://www.googleapis.com/batch")
    def test_tunnel_patch(self):
                ({"status": "200"}, "echo_request_headers_as_json"),
        http = tunnel_patch(http)
        resp = zoo.animals().patch(name="lion", body='{"description": "foo"}').execute()
        self.assertTrue("x-http-method-override" in resp)
    def test_plus_resources(self):
        self.assertTrue(getattr(plus, "activities"))
        self.assertTrue(getattr(plus, "people"))
    def test_oauth2client_credentials(self):
        credentials = mock.Mock(spec=GoogleCredentials)
        _reset_universe_domain(credentials)
        credentials.create_scoped_required.return_value = False
        service = build_from_document(discovery, credentials=credentials)
        self.assertEqual(service._http, credentials.authorize.return_value)
    def test_google_auth_credentials(self):
        self.assertIsInstance(service._http, google_auth_httplib2.AuthorizedHttp)
        self.assertEqual(service._http.credentials, credentials)
    def test_no_scopes_no_credentials(self):
        # Zoo doesn't have scopes
        discovery = read_datafile("zoo.json")
        # Should be an ordinary httplib2.Http instance and not AuthorizedHttp.
        self.assertIsInstance(service._http, httplib2.Http)
    def test_full_featured(self):
        # Zoo should exercise all discovery facets
        # and should also have no future.json file.
        self.assertTrue(getattr(zoo, "animals"))
        request = zoo.animals().list(name="bat", projection="full")
        self.assertEqual(q["name"], ["bat"])
        self.assertEqual(q["projection"], ["full"])
    def test_nested_resources(self):
        request = zoo.my().favorites().list(max_results="5")
        self.assertEqual(q["max-results"], ["5"])
    def test_top_level_functions(self):
        self.assertTrue(getattr(zoo, "query"))
        request = zoo.query(q="foo")
    def test_simple_media_uploads(self):
        doc = getattr(zoo.animals().insert, "__doc__")
        self.assertTrue("media_body" in doc)
    def test_simple_media_upload_no_max_size_provided(self):
        request = zoo.animals().crossbreed(media_body=datafile("small.png"))
        self.assertEqual("image/png", request.headers["content-type"])
        self.assertEqual(b"PNG", request.body[1:4])
    def test_simple_media_raise_correct_exceptions(self):
            zoo.animals().insert(media_body=datafile("smiley.png"))
            self.fail("should throw exception if media is too large.")
        except MediaUploadSizeError:
            zoo.animals().insert(media_body=datafile("small.jpg"))
            self.fail("should throw exception if mimetype is unacceptable.")
        except UnacceptableMimeTypeError:
    def test_simple_media_good_upload(self):
        request = zoo.animals().insert(media_body=datafile("small.png"))
        assertUrisEqual(
            "https://www.googleapis.com/upload/zoo/v1/animals?uploadType=media&alt=json",
            request.uri,
    def test_simple_media_unknown_mimetype(self):
            zoo.animals().insert(media_body=datafile("small-png"))
            self.fail("should throw exception if mimetype is unknown.")
        except UnknownFileType:
        request = zoo.animals().insert(
            media_body=datafile("small-png"), media_mime_type="image/png"
    def test_multipart_media_raise_correct_exceptions(self):
            zoo.animals().insert(media_body=datafile("smiley.png"), body={})
            zoo.animals().insert(media_body=datafile("small.jpg"), body={})
    def test_multipart_media_good_upload(self, static_discovery=False):
        request = zoo.animals().insert(media_body=datafile("small.png"), body={})
        self.assertTrue(request.headers["content-type"].startswith("multipart/related"))
        contents = read_datafile("small.png", "rb")
        boundary = re.match(b"--=+([^=]+)", request.body).group(1)
            request.body.rstrip(b"\n"),  # Python 2.6 does not add a trailing \n
            b"--==============="
            + boundary
            + b"==\n"
            + b"Content-Type: application/json\n"
            + b"MIME-Version: 1.0\n\n"
            + b'{"data": {}}\n'
            + b"--==============="
            + b"Content-Type: image/png\n"
            + b"MIME-Version: 1.0\n"
            + b"Content-Transfer-Encoding: binary\n\n"
            + contents
            + b"\n--==============="
            + b"==--",
            "https://www.googleapis.com/upload/zoo/v1/animals?uploadType=multipart&alt=json",
    def test_media_capable_method_without_media(self):
        request = zoo.animals().insert(body={})
        self.assertTrue(request.headers["content-type"], "application/json")
    def test_resumable_multipart_media_good_upload(self):
        media_upload = MediaFileUpload(datafile("small.png"), resumable=True)
        request = zoo.animals().insert(media_body=media_upload, body={})
        self.assertTrue(request.headers["content-type"].startswith("application/json"))
        self.assertEqual('{"data": {}}', request.body)
        self.assertEqual(media_upload, request.resumable)
        self.assertEqual("image/png", request.resumable.mimetype())
        self.assertNotEqual(request.body, None)
        self.assertEqual(request.resumable_uri, None)
                ({"status": "200", "location": "http://upload.example.com"}, ""),
                ({"status": "308", "location": "http://upload.example.com/2"}, ""),
                        "status": "308",
                        "location": "http://upload.example.com/3",
                        "range": "0-12",
                        "location": "http://upload.example.com/4",
                        "range": "0-%d" % (media_upload.size() - 2),
                ({"status": "200"}, '{"foo": "bar"}'),
        status, body = request.next_chunk(http=http)
        self.assertEqual(None, body)
        self.assertTrue(isinstance(status, MediaUploadProgress))
        self.assertEqual(0, status.resumable_progress)
        # Two requests should have been made and the resumable_uri should have been
        # updated for each one.
        self.assertEqual(request.resumable_uri, "http://upload.example.com/2")
        self.assertEqual(0, request.resumable_progress)
        # This next chuck call should upload the first chunk
        self.assertEqual(request.resumable_uri, "http://upload.example.com/3")
        self.assertEqual(13, request.resumable_progress)
        # This call will upload the next chunk
        self.assertEqual(request.resumable_uri, "http://upload.example.com/4")
        self.assertEqual(media_upload.size() - 1, request.resumable_progress)
        # Final call to next_chunk should complete the upload.
        self.assertEqual(body, {"foo": "bar"})
        self.assertEqual(status, None)
    def test_resumable_media_good_upload(self):
        """Not a multipart upload."""
        request = zoo.animals().insert(media_body=media_upload, body=None)
        self.assertEqual(request.body, None)
                        "location": "http://upload.example.com/2",
        self.assertEqual(13, status.resumable_progress)
    def test_resumable_media_good_upload_from_execute(self):
            "https://www.googleapis.com/upload/zoo/v1/animals?uploadType=resumable&alt=json",
                        "range": "0-%d" % media_upload.size(),
        body = request.execute(http=http)
    def test_resumable_media_fail_unknown_response_code_first_request(self):
            [({"status": "400", "location": "http://upload.example.com"}, "")]
            request.execute(http=http)
            self.fail("Should have raised ResumableUploadError.")
        except ResumableUploadError as e:
            self.assertEqual(400, e.resp.status)
    def test_resumable_media_fail_unknown_response_code_subsequent_request(self):
                ({"status": "400"}, ""),
        self.assertRaises(HttpError, request.execute, http=http)
        self.assertTrue(request._in_error_state)
                ({"status": "308", "range": "0-5"}, ""),
                ({"status": "308", "range": "0-6"}, ""),
            status.resumable_progress,
            7,
            "Should have first checked length and then tried to PUT more.",
        self.assertFalse(request._in_error_state)
        # Put it back in an error state.
        # Pretend the last request that 400'd actually succeeded.
        http = HttpMockSequence([({"status": "200"}, '{"foo": "bar"}')])
    def test_media_io_base_stream_unlimited_chunksize_resume(self):
        # Set up a seekable stream and try to upload in single chunk.
        fd = io.BytesIO(b'01234"56789"')
        media_upload = MediaIoBaseUpload(
            fd=fd, mimetype="text/plain", chunksize=-1, resumable=True
        # The single chunk fails, restart at the right point.
                        "range": "0-4",
                ({"status": "200"}, "echo_request_body"),
        self.assertEqual("56789", body)
    def test_media_io_base_stream_chunksize_resume(self):
        # Set up a seekable stream and try to upload in chunks.
        fd = io.BytesIO(b"0123456789")
            fd=fd, mimetype="text/plain", chunksize=5, resumable=True
        # The single chunk fails, pull the content sent out of the exception.
                ({"status": "400"}, "echo_request_body"),
            self.assertEqual(b"01234", e.content)
    def test_resumable_media_handle_uploads_of_unknown_size(self):
        # Create an upload that doesn't know the full size of the media.
        class IoBaseUnknownLength(MediaUpload):
                return 10
                return "image/png"
                return "0123456789"
        upload = IoBaseUnknownLength()
        request = zoo.animals().insert(media_body=upload, body=None)
        self.assertEqual(body, {"Content-Range": "bytes 0-9/*", "Content-Length": "10"})
    def test_resumable_media_no_streaming_on_unsupported_platforms(self):
        class IoBaseHasStream(MediaUpload):
        upload = IoBaseHasStream()
        orig_version = sys.version_info
        sys.version_info = (2, 6, 5, "final", 0)
        # This should raise an exception because stream() will be called.
        self.assertRaises(NotImplementedError, request.next_chunk, http=http)
        sys.version_info = orig_version
    def test_resumable_media_handle_uploads_of_unknown_size_eof(self):
        fd = io.BytesIO(b"data goes here")
        upload = MediaIoBaseUpload(
            fd=fd, mimetype="image/png", chunksize=15, resumable=True
            body, {"Content-Range": "bytes 0-13/14", "Content-Length": "14"}
    def test_resumable_media_handle_resume_of_upload_of_unknown_size(self):
            fd=fd, mimetype="image/png", chunksize=500, resumable=True
        # Put it in an error state.
        self.assertRaises(HttpError, request.next_chunk, http=http)
            [({"status": "400", "range": "0-5"}, "echo_request_headers_as_json")]
            # Should resume the upload by first querying the status of the upload.
            request.next_chunk(http=http)
            expected = {"Content-Range": "bytes */14", "content-length": "0"}
                json.loads(e.content.decode("utf-8")),
                "Should send an empty body when requesting the current upload status.",
    def test_pickle(self):
        sorted_resource_keys = [
            "_baseUrl",
            "_credentials_validated",
            "_developerKey",
            "_dynamic_attrs",
            "_http",
            "_model",
            "_requestBuilder",
            "_resourceDesc",
            "_rootDesc",
            "_schema",
            "_universe_domain",
            "animals",
            "global_",
            "load",
            "loadNoTemplate",
            "my",
            "new_batch_http_request",
            "query",
            "scopedAnimals",
        self.assertEqual(sorted(zoo.__dict__.keys()), sorted_resource_keys)
        pickled_zoo = pickle.dumps(zoo)
        new_zoo = pickle.loads(pickled_zoo)
        self.assertEqual(sorted(new_zoo.__dict__.keys()), sorted_resource_keys)
        self.assertTrue(hasattr(new_zoo, "animals"))
        self.assertTrue(callable(new_zoo.animals))
        self.assertTrue(hasattr(new_zoo, "global_"))
        self.assertTrue(callable(new_zoo.global_))
        self.assertTrue(hasattr(new_zoo, "load"))
        self.assertTrue(callable(new_zoo.load))
        self.assertTrue(hasattr(new_zoo, "loadNoTemplate"))
        self.assertTrue(callable(new_zoo.loadNoTemplate))
        self.assertTrue(hasattr(new_zoo, "my"))
        self.assertTrue(callable(new_zoo.my))
        self.assertTrue(hasattr(new_zoo, "query"))
        self.assertTrue(callable(new_zoo.query))
        self.assertTrue(hasattr(new_zoo, "scopedAnimals"))
        self.assertTrue(callable(new_zoo.scopedAnimals))
        self.assertEqual(sorted(zoo._dynamic_attrs), sorted(new_zoo._dynamic_attrs))
        self.assertEqual(zoo._baseUrl, new_zoo._baseUrl)
        self.assertEqual(zoo._developerKey, new_zoo._developerKey)
        self.assertEqual(zoo._requestBuilder, new_zoo._requestBuilder)
        self.assertEqual(zoo._resourceDesc, new_zoo._resourceDesc)
        self.assertEqual(zoo._rootDesc, new_zoo._rootDesc)
        # _http, _model and _schema won't be equal since we will get new
        # instances upon un-pickling
    def _dummy_zoo_request(self):
        zoo_contents = read_datafile("zoo.json")
        zoo_uri = uritemplate.expand(DISCOVERY_URI, {"api": "zoo", "apiVersion": "v1"})
            zoo_uri = util._add_query_parameter(
                zoo_uri, "userIp", os.environ["REMOTE_ADDR"]
        original_request = http.request
        def wrapped_request(uri, method="GET", *args, **kwargs):
            if uri == zoo_uri:
                return httplib2.Response({"status": "200"}), zoo_contents
            return original_request(uri, method=method, *args, **kwargs)
        http.request = wrapped_request
    def _dummy_token(self):
        access_token = "foo"
        client_id = "some_client_id"
        client_secret = "cOuDdkfjxxnv+"
        refresh_token = "1/0/a.df219fjls0"
        token_expiry = datetime.datetime.now(datetime.timezone.utc)
        user_agent = "refresh_checker/1.0"
        return OAuth2Credentials(
            client_secret,
            refresh_token,
            token_expiry,
            GOOGLE_TOKEN_URI,
            user_agent,
    def test_pickle_with_credentials(self):
        credentials = self._dummy_token()
        http = self._dummy_zoo_request()
        self.assertTrue(hasattr(http.request, "credentials"))
        self.assertEqual(sorted(zoo.__dict__.keys()), sorted(new_zoo.__dict__.keys()))
        new_http = new_zoo._http
        self.assertFalse(hasattr(new_http.request, "credentials"))
    def test_resumable_media_upload_no_content(self):
        media_upload = MediaFileUpload(datafile("empty"), resumable=True)
                        "range": "0-0",
        self.assertEqual(0, status.progress())
class Next(unittest.TestCase):
    def test_next_successful_none_on_no_next_page_token(self):
        self.http = HttpMock(datafile("tasks.json"), {"status": "200"})
        tasks = build("tasks", "v1", http=self.http)
        request = tasks.tasklists().list()
        self.assertEqual(None, tasks.tasklists().list_next(request, {}))
    def test_next_successful_none_on_empty_page_token(self):
        next_request = tasks.tasklists().list_next(request, {"nextPageToken": ""})
        self.assertEqual(None, next_request)
    def test_next_successful_with_next_page_token(self):
        next_request = tasks.tasklists().list_next(request, {"nextPageToken": "123abc"})
        parsed = urllib.parse.urlparse(next_request.uri)
        self.assertEqual(q["pageToken"][0], "123abc")
    def test_next_successful_with_next_page_token_alternate_name(self):
        self.http = HttpMock(datafile("bigquery.json"), {"status": "200"})
        bigquery = build("bigquery", "v2", http=self.http)
        request = bigquery.tabledata().list(
            datasetId="test_dataset", projectId="test_project", tableId="test_table"
        next_request = bigquery.tabledata().list_next(request, {"pageToken": "123abc"})
    def test_next_successful_with_next_page_token_in_body(self):
        self.http = HttpMock(datafile("logging.json"), {"status": "200"})
        logging = build("logging", "v2", http=self.http)
        request = logging.entries().list(body={})
        next_request = logging.entries().list_next(request, {"nextPageToken": "123abc"})
        body = JsonModel().deserialize(next_request.body)
        self.assertEqual(body["pageToken"], "123abc")
        # The body is changed, make sure that body_length is changed too (see
        # github #1403)
        self.assertEqual(next_request.body_size, len(next_request.body))
    def test_next_with_method_with_no_properties(self):
        self.http = HttpMock(datafile("latitude.json"), {"status": "200"})
        service = build("latitude", "v1", http=self.http, static_discovery=False)
        service.currentLocation().get()
    def test_next_nonexistent_with_no_next_page_token(self):
        self.http = HttpMock(datafile("drive.json"), {"status": "200"})
        drive = build("drive", "v3", http=self.http)
        drive.changes().watch(body={})
        self.assertFalse(callable(getattr(drive.changes(), "watch_next", None)))
    def test_next_successful_with_next_page_token_required(self):
        request = drive.changes().list(pageToken="startPageToken")
        next_request = drive.changes().list_next(request, {"nextPageToken": "123abc"})
class MediaGet(unittest.TestCase):
    def test_get_media(self):
        request = zoo.animals().get_media(name="Lion")
        self.assertEqual(q["alt"], ["media"])
        http = HttpMockSequence([({"status": "200"}, "standing in for media")])
        response = request.execute(http=http)
        self.assertEqual(b"standing in for media", response)
class Universe(unittest.TestCase):
        def test_validate_credentials_with_no_client_options(self):
            assert service._validate_credentials()
        def test_validate_credentials_with_client_options_without_universe(self):
                client_options=google.api_core.client_options.ClientOptions(),
        def test_validate_credentials_with_no_universe(self):
            fake_universe = "foo.com"
            http = google_auth_httplib2.AuthorizedHttp(
                credentials=None, http=build_http()
                    universe_domain=universe.DEFAULT_UNIVERSE
                    universe_domain=fake_universe
            with self.assertRaises(universe.UniverseMismatchError):
                service._validate_credentials()
        def test_validate_credentials_with_default_universe(self):
                credentials=mock.Mock(universe_domain=universe.DEFAULT_UNIVERSE),
                http=build_http(),
        def test_validate_credentials_with_a_different_universe(self):
                credentials=mock.Mock(universe_domain=fake_universe),
                credentials=mock.Mock(universe_domain=fake_universe), http=build_http()
        def test_validate_credentials_with_already_validated_credentials(self):
            assert service._credentials_validated
            # Calling service._validate_credentials() again returns service.credentials_validated.
        def test_validate_credentials_before_api_request_success(self):
            credentials.universe_domain = fake_universe
            discovery = read_datafile("tasks.json")
            tasks = build_from_document(
            tasklists = tasks.tasklists()
            request = tasklists.list()
            # Check that credentials are indeed verified before request.
            assert tasklists._validate_credentials()
        def test_validate_credentials_before_api_request_failure(self):
            # Check that credentials are verified before request.
        def test_validate_credentials_before_another_universe_api_request_failure(self):
            another_fake_universe = "bar.com"
                    universe_domain=another_fake_universe
        def test_client_options_with_empty_universe(self):
            with self.assertRaises(universe.EmptyUniverseError):
                        universe_domain=""
        def test_client_options_universe_configured_with_mtls(self):
                    "os.environ", {"GOOGLE_API_USE_MTLS_ENDPOINT": "always"}
        def test_client_options_universe_configured_with_api_override(self):
            fake_api_endpoint = "https://www.bar.com/"
            credentials = mock.Mock(universe_domain=fake_universe)
                    api_endpoint=fake_api_endpoint, universe_domain=fake_universe
            assert tasks._baseUrl == fake_api_endpoint
        def test_universe_env_var_configured_empty(self):
                    "os.environ", {"GOOGLE_CLOUD_UNIVERSE_DOMAIN": ""}
        def test_universe_env_var_configured_with_mtls(self):
                        "GOOGLE_API_USE_MTLS_ENDPOINT": "always",
                        "GOOGLE_CLOUD_UNIVERSE_DOMAIN": fake_universe,
                    tasks = build_from_document(discovery)
        def test_universe_env_var_configured_with_api_override(self):
                "os.environ", {"GOOGLE_CLOUD_UNIVERSE_DOMAIN": fake_universe}
                        api_endpoint=fake_api_endpoint
        def test_universe_env_var_configured_with_client_options_universe(self):
                "os.environ", {"GOOGLE_CLOUD_UNIVERSE_DOMAIN": another_fake_universe}
            assert tasks._universe_domain == fake_universe
        if hasattr(google.api_core.client_options.ClientOptions, "universe_domain"):
            def test_client_options_universe_with_older_version_of_api_core(self):
                with self.assertRaises(APICoreVersionError):
        def test_credentials_universe_with_older_version_of_api_core(self):
        def test_credentials_default_universe_with_older_version_of_api_core(self):
            credentials.universe_domain = "googleapis.com"
        def test_http_credentials_universe_with_older_version_of_api_core(self):
        def test_http_credentials_default_universe_with_older_version_of_api_core(self):
                credentials=mock.Mock(universe_domain="googleapis.com"),
