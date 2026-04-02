class SecondaryEmail(bb.Struct):
    :ivar secondary_emails.SecondaryEmail.email: Secondary email address.
    :ivar secondary_emails.SecondaryEmail.is_verified: Whether or not the
        secondary email address is verified to be owned by a user.
        '_is_verified_value',
                 is_verified=None):
        self._is_verified_value = bb.NOT_SET
        if is_verified is not None:
            self.is_verified = is_verified
    email = bb.Attribute("email")
    is_verified = bb.Attribute("is_verified")
        super(SecondaryEmail, self)._process_custom_annotations(annotation_type, field_path, processor)
SecondaryEmail_validator = bv.Struct(SecondaryEmail)
SecondaryEmail.email.validator = common.EmailAddress_validator
SecondaryEmail.is_verified.validator = bv.Boolean()
SecondaryEmail._all_field_names_ = set([
    'is_verified',
SecondaryEmail._all_fields_ = [
    ('email', SecondaryEmail.email.validator),
    ('is_verified', SecondaryEmail.is_verified.validator),
