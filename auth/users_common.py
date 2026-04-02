This namespace contains common data types used within the users namespace.
class AccountType(bb.Union):
    What type of account this user has.
    :ivar users_common.AccountType.basic: The basic account type.
    :ivar users_common.AccountType.pro: The Dropbox Pro account type.
    :ivar users_common.AccountType.business: The Dropbox Business account type.
    basic = None
    pro = None
    business = None
    def is_basic(self):
        Check if the union tag is ``basic``.
        return self._tag == 'basic'
    def is_pro(self):
        Check if the union tag is ``pro``.
        return self._tag == 'pro'
    def is_business(self):
        Check if the union tag is ``business``.
        return self._tag == 'business'
        super(AccountType, self)._process_custom_annotations(annotation_type, field_path, processor)
AccountType_validator = bv.Union(AccountType)
AccountId_validator = bv.String(min_length=40, max_length=40)
AccountType._basic_validator = bv.Void()
AccountType._pro_validator = bv.Void()
AccountType._business_validator = bv.Void()
AccountType._tagmap = {
    'basic': AccountType._basic_validator,
    'pro': AccountType._pro_validator,
    'business': AccountType._business_validator,
AccountType.basic = AccountType('basic')
AccountType.pro = AccountType('pro')
AccountType.business = AccountType('business')
