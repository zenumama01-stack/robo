class OpenIdError(bb.Union):
    :ivar openid.OpenIdError.incorrect_openid_scopes: Missing openid claims for
        the associated access token.
    incorrect_openid_scopes = None
    def is_incorrect_openid_scopes(self):
        Check if the union tag is ``incorrect_openid_scopes``.
        return self._tag == 'incorrect_openid_scopes'
        super(OpenIdError, self)._process_custom_annotations(annotation_type, field_path, processor)
OpenIdError_validator = bv.Union(OpenIdError)
class UserInfoArgs(bb.Struct):
    No Parameters
        super(UserInfoArgs, self)._process_custom_annotations(annotation_type, field_path, processor)
UserInfoArgs_validator = bv.Struct(UserInfoArgs)
class UserInfoError(bb.Union):
    def openid_error(cls, val):
        Create an instance of this class set to the ``openid_error`` tag with
        :param OpenIdError val:
        :rtype: UserInfoError
        return cls('openid_error', val)
    def is_openid_error(self):
        Check if the union tag is ``openid_error``.
        return self._tag == 'openid_error'
    def get_openid_error(self):
        Only call this if :meth:`is_openid_error` is true.
        :rtype: OpenIdError
        if not self.is_openid_error():
            raise AttributeError("tag 'openid_error' not set")
        super(UserInfoError, self)._process_custom_annotations(annotation_type, field_path, processor)
UserInfoError_validator = bv.Union(UserInfoError)
class UserInfoResult(bb.Struct):
    :ivar openid.UserInfoResult.family_name: Last name of user.
    :ivar openid.UserInfoResult.given_name: First name of user.
    :ivar openid.UserInfoResult.email: Email address of user.
    :ivar openid.UserInfoResult.email_verified: If user is email verified.
    :ivar openid.UserInfoResult.iss: Issuer of token (in this case Dropbox).
    :ivar openid.UserInfoResult.sub: An identifier for the user. This is the
        Dropbox account_id, a string value such as
        dbid:AAH4f99T0taONIb-OurWxbNQ6ywGRopQngc.
        '_family_name_value',
        '_given_name_value',
        '_email_value',
        '_email_verified_value',
        '_iss_value',
        '_sub_value',
                 family_name=None,
                 given_name=None,
                 email=None,
                 email_verified=None,
                 iss=None,
                 sub=None):
        self._family_name_value = bb.NOT_SET
        self._given_name_value = bb.NOT_SET
        self._email_value = bb.NOT_SET
        self._email_verified_value = bb.NOT_SET
        self._iss_value = bb.NOT_SET
        self._sub_value = bb.NOT_SET
        if family_name is not None:
            self.family_name = family_name
        if given_name is not None:
            self.given_name = given_name
        if email is not None:
            self.email = email
        if email_verified is not None:
            self.email_verified = email_verified
        if iss is not None:
            self.iss = iss
        if sub is not None:
            self.sub = sub
    family_name = bb.Attribute("family_name", nullable=True)
    given_name = bb.Attribute("given_name", nullable=True)
    email = bb.Attribute("email", nullable=True)
    email_verified = bb.Attribute("email_verified", nullable=True)
    iss = bb.Attribute("iss")
    sub = bb.Attribute("sub")
        super(UserInfoResult, self)._process_custom_annotations(annotation_type, field_path, processor)
UserInfoResult_validator = bv.Struct(UserInfoResult)
OpenIdError._incorrect_openid_scopes_validator = bv.Void()
OpenIdError._other_validator = bv.Void()
OpenIdError._tagmap = {
    'incorrect_openid_scopes': OpenIdError._incorrect_openid_scopes_validator,
    'other': OpenIdError._other_validator,
OpenIdError.incorrect_openid_scopes = OpenIdError('incorrect_openid_scopes')
OpenIdError.other = OpenIdError('other')
UserInfoArgs._all_field_names_ = set([])
UserInfoArgs._all_fields_ = []
UserInfoError._openid_error_validator = OpenIdError_validator
UserInfoError._other_validator = bv.Void()
UserInfoError._tagmap = {
    'openid_error': UserInfoError._openid_error_validator,
    'other': UserInfoError._other_validator,
UserInfoError.other = UserInfoError('other')
UserInfoResult.family_name.validator = bv.Nullable(bv.String())
UserInfoResult.given_name.validator = bv.Nullable(bv.String())
UserInfoResult.email.validator = bv.Nullable(bv.String())
UserInfoResult.email_verified.validator = bv.Nullable(bv.Boolean())
UserInfoResult.iss.validator = bv.String()
UserInfoResult.sub.validator = bv.String()
UserInfoResult._all_field_names_ = set([
    'family_name',
    'given_name',
    'email_verified',
    'iss',
UserInfoResult._all_fields_ = [
    ('family_name', UserInfoResult.family_name.validator),
    ('given_name', UserInfoResult.given_name.validator),
    ('email', UserInfoResult.email.validator),
    ('email_verified', UserInfoResult.email_verified.validator),
    ('iss', UserInfoResult.iss.validator),
    ('sub', UserInfoResult.sub.validator),
UserInfoResult.iss.default = ''
UserInfoResult.sub.default = ''
userinfo = bb.Route(
    'userinfo',
    UserInfoArgs_validator,
    UserInfoResult_validator,
    UserInfoError_validator,
    'userinfo': userinfo,
