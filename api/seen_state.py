class PlatformType(bb.Union):
    Possible platforms on which a user may view content.
    :ivar seen_state.PlatformType.web: The content was viewed on the web.
    :ivar seen_state.PlatformType.desktop: The content was viewed on a desktop
        client.
    :ivar seen_state.PlatformType.mobile_ios: The content was viewed on a mobile
        iOS client.
    :ivar seen_state.PlatformType.mobile_android: The content was viewed on a
        mobile android client.
    :ivar seen_state.PlatformType.api: The content was viewed from an API
    :ivar seen_state.PlatformType.unknown: The content was viewed on an unknown
        platform.
    :ivar seen_state.PlatformType.mobile: The content was viewed on a mobile
        client. DEPRECATED: Use mobile_ios or mobile_android instead.
    web = None
    desktop = None
    mobile_ios = None
    mobile_android = None
    api = None
    unknown = None
    mobile = None
    def is_web(self):
        Check if the union tag is ``web``.
        return self._tag == 'web'
    def is_desktop(self):
        Check if the union tag is ``desktop``.
        return self._tag == 'desktop'
    def is_mobile_ios(self):
        Check if the union tag is ``mobile_ios``.
        return self._tag == 'mobile_ios'
    def is_mobile_android(self):
        Check if the union tag is ``mobile_android``.
        return self._tag == 'mobile_android'
    def is_api(self):
        Check if the union tag is ``api``.
        return self._tag == 'api'
    def is_unknown(self):
        Check if the union tag is ``unknown``.
        return self._tag == 'unknown'
    def is_mobile(self):
        Check if the union tag is ``mobile``.
        return self._tag == 'mobile'
        super(PlatformType, self)._process_custom_annotations(annotation_type, field_path, processor)
PlatformType_validator = bv.Union(PlatformType)
PlatformType._web_validator = bv.Void()
PlatformType._desktop_validator = bv.Void()
PlatformType._mobile_ios_validator = bv.Void()
PlatformType._mobile_android_validator = bv.Void()
PlatformType._api_validator = bv.Void()
PlatformType._unknown_validator = bv.Void()
PlatformType._mobile_validator = bv.Void()
PlatformType._other_validator = bv.Void()
PlatformType._tagmap = {
    'web': PlatformType._web_validator,
    'desktop': PlatformType._desktop_validator,
    'mobile_ios': PlatformType._mobile_ios_validator,
    'mobile_android': PlatformType._mobile_android_validator,
    'api': PlatformType._api_validator,
    'unknown': PlatformType._unknown_validator,
    'mobile': PlatformType._mobile_validator,
    'other': PlatformType._other_validator,
PlatformType.web = PlatformType('web')
PlatformType.desktop = PlatformType('desktop')
PlatformType.mobile_ios = PlatformType('mobile_ios')
PlatformType.mobile_android = PlatformType('mobile_android')
PlatformType.api = PlatformType('api')
PlatformType.unknown = PlatformType('unknown')
PlatformType.mobile = PlatformType('mobile')
PlatformType.other = PlatformType('other')
