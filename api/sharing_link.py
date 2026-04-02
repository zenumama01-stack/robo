class SharingLink(OneDriveObjectBase):
    def type(self):
        """Gets and sets the type
                The type
        if "type" in self._prop_dict:
            return self._prop_dict["type"]
    @type.setter
    def type(self, val):
        self._prop_dict["type"] = val
        """Gets and sets the webUrl
    def web_html(self):
        """Gets and sets the webHtml
                The webHtml
        if "webHtml" in self._prop_dict:
            return self._prop_dict["webHtml"]
    @web_html.setter
    def web_html(self, val):
        self._prop_dict["webHtml"] = val
    def configurator_url(self):
        """Gets and sets the configuratorUrl
                The configuratorUrl
        if "configuratorUrl" in self._prop_dict:
            return self._prop_dict["configuratorUrl"]
    @configurator_url.setter
    def configurator_url(self, val):
        self._prop_dict["configuratorUrl"] = val
