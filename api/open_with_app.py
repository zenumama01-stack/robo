class OpenWithApp(OneDriveObjectBase):
    def app(self):
        Gets and sets the app
                The app
        if "app" in self._prop_dict:
            if isinstance(self._prop_dict["app"], OneDriveObjectBase):
                return self._prop_dict["app"]
                self._prop_dict["app"] = Identity(self._prop_dict["app"])
    @app.setter
    def app(self, val):
        self._prop_dict["app"] = val
    def view_url(self):
        """Gets and sets the viewUrl
                The viewUrl
        if "viewUrl" in self._prop_dict:
            return self._prop_dict["viewUrl"]
    @view_url.setter
    def view_url(self, val):
        self._prop_dict["viewUrl"] = val
    def edit_url(self):
        """Gets and sets the editUrl
                The editUrl
        if "editUrl" in self._prop_dict:
            return self._prop_dict["editUrl"]
    @edit_url.setter
    def edit_url(self, val):
        self._prop_dict["editUrl"] = val
    def view_post_parameters(self):
        """Gets and sets the viewPostParameters
                The viewPostParameters
        if "viewPostParameters" in self._prop_dict:
            return self._prop_dict["viewPostParameters"]
    @view_post_parameters.setter
    def view_post_parameters(self, val):
        self._prop_dict["viewPostParameters"] = val
    def edit_post_parameters(self):
        """Gets and sets the editPostParameters
                The editPostParameters
        if "editPostParameters" in self._prop_dict:
            return self._prop_dict["editPostParameters"]
    @edit_post_parameters.setter
    def edit_post_parameters(self, val):
        self._prop_dict["editPostParameters"] = val
