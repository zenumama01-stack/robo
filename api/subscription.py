class Subscription(OneDriveObjectBase):
    def app_type(self):
        Gets and sets the appType
                The appType
        if "appType" in self._prop_dict:
            return self._prop_dict["appType"]
    @app_type.setter
    def app_type(self, val):
        self._prop_dict["appType"] = val
    def client_state(self):
        Gets and sets the clientState
                The clientState
        if "clientState" in self._prop_dict:
            return self._prop_dict["clientState"]
    @client_state.setter
    def client_state(self, val):
        self._prop_dict["clientState"] = val
    def expiration_date_time(self):
        Gets and sets the expirationDateTime
                The expirationDateTime
        if "expirationDateTime" in self._prop_dict:
            if '.' in self._prop_dict["expirationDateTime"]:
                return datetime.strptime(self._prop_dict["expirationDateTime"].replace("Z", ""), "%Y-%m-%dT%H:%M:%S.%f")
                return datetime.strptime(self._prop_dict["expirationDateTime"].replace("Z", ""), "%Y-%m-%dT%H:%M:%S")
    @expiration_date_time.setter
    def expiration_date_time(self, val):
        self._prop_dict["expirationDateTime"] = val.isoformat()+((".0" if val.time().microsecond == 0 else "")+"Z")
    def muted(self):
        Gets and sets the muted
                The muted
        if "muted" in self._prop_dict:
            return self._prop_dict["muted"]
    @muted.setter
    def muted(self, val):
        self._prop_dict["muted"] = val
    def notification_url(self):
        Gets and sets the notificationUrl
                The notificationUrl
        if "notificationUrl" in self._prop_dict:
            return self._prop_dict["notificationUrl"]
    @notification_url.setter
    def notification_url(self, val):
        self._prop_dict["notificationUrl"] = val
    def resource(self):
        Gets and sets the resource
                The resource
        if "resource" in self._prop_dict:
            return self._prop_dict["resource"]
    @resource.setter
    def resource(self, val):
        self._prop_dict["resource"] = val
    def scenarios(self):
        Gets and sets the scenarios
                The scenarios
        if "scenarios" in self._prop_dict:
            return self._prop_dict["scenarios"]
    @scenarios.setter
    def scenarios(self, val):
        self._prop_dict["scenarios"] = val
