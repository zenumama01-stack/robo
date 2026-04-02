class TestAuthWithGoogleAuth(unittest.TestCase):
        _auth.HAS_GOOGLE_AUTH = True
        _auth.HAS_OAUTH2CLIENT = False
        _auth.HAS_OAUTH2CLIENT = True
    def test_default_credentials(self):
        with mock.patch("google.auth.default", autospec=True) as default:
            default.return_value = (mock.sentinel.credentials, mock.sentinel.project)
            credentials = _auth.default_credentials()
            self.assertEqual(credentials, mock.sentinel.credentials)
    def test_credentials_from_file(self):
        with mock.patch(
            "google.auth.load_credentials_from_file", autospec=True
        ) as default:
            credentials = _auth.credentials_from_file("credentials.json")
            default.assert_called_once_with(
                "credentials.json", scopes=None, quota_project_id=None
    def test_default_credentials_with_scopes(self):
            credentials = _auth.default_credentials(scopes=["1", "2"])
            default.assert_called_once_with(scopes=["1", "2"], quota_project_id=None)
    def test_default_credentials_with_quota_project(self):
            credentials = _auth.default_credentials(quota_project_id="my-project")
            default.assert_called_once_with(scopes=None, quota_project_id="my-project")
    def test_with_scopes_non_scoped(self):
        credentials = mock.Mock(spec=google.auth.credentials.Credentials)
        returned = _auth.with_scopes(credentials, mock.sentinel.scopes)
        self.assertEqual(credentials, returned)
    def test_with_scopes_scoped(self):
        class CredentialsWithScopes(
            google.auth.credentials.Credentials, google.auth.credentials.Scoped
        credentials = mock.Mock(spec=CredentialsWithScopes)
        credentials.requires_scopes = True
        self.assertNotEqual(credentials, returned)
        self.assertEqual(returned, credentials.with_scopes.return_value)
        credentials.with_scopes.assert_called_once_with(
            mock.sentinel.scopes, default_scopes=None
    def test_authorized_http(self):
        authorized_http = _auth.authorized_http(credentials)
        self.assertIsInstance(authorized_http, google_auth_httplib2.AuthorizedHttp)
        self.assertEqual(authorized_http.credentials, credentials)
        self.assertIsInstance(authorized_http.http, httplib2.Http)
        self.assertIsInstance(authorized_http.http.timeout, int)
        self.assertGreater(authorized_http.http.timeout, 0)
@unittest.skipIf(not HAS_OAUTH2CLIENT, "oauth2client unavailable.")
class TestAuthWithOAuth2Client(unittest.TestCase):
        _auth.HAS_GOOGLE_AUTH = False
        default_patch = mock.patch(
            "oauth2client.client.GoogleCredentials.get_application_default"
        with default_patch as default:
            default.return_value = mock.sentinel.credentials
        with self.assertRaises(EnvironmentError):
    def test_default_credentials_with_scopes_and_quota_project(self):
                scopes=["1", "2"], quota_project_id="my-project"
        credentials = mock.Mock(spec=oauth2client.client.Credentials)
        credentials = mock.Mock(spec=oauth2client.client.GoogleCredentials)
        credentials.create_scoped_required.return_value = True
        self.assertEqual(returned, credentials.create_scoped.return_value)
        credentials.create_scoped.assert_called_once_with(mock.sentinel.scopes)
        http = credentials.authorize.call_args[0][0]
        self.assertEqual(authorized_http, credentials.authorize.return_value)
        self.assertIsInstance(http, httplib2.Http)
        self.assertIsInstance(http.timeout, int)
        self.assertGreater(http.timeout, 0)
class TestAuthWithoutAuth(unittest.TestCase):
            print(_auth.default_credentials())
class TestGoogleAuthWithoutHttplib2(unittest.TestCase):
        _auth.google_auth_httplib2 = None
        _auth.google_auth_httplib2 = google_auth_httplib2
        with self.assertRaises(ValueError):
            _auth.authorized_http(credentials)
