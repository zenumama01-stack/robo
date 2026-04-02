from ..model.open_with_app import OpenWithApp
class OpenWithSet(OneDriveObjectBase):
    def web(self):
        Gets and sets the web
            :class:`OpenWithApp<onedrivesdk.model.open_with_app.OpenWithApp>`:
                The web
        if "web" in self._prop_dict:
            if isinstance(self._prop_dict["web"], OneDriveObjectBase):
                return self._prop_dict["web"]
                self._prop_dict["web"] = OpenWithApp(self._prop_dict["web"])
    @web.setter
    def web(self, val):
        self._prop_dict["web"] = val
    def web_embed(self):
        Gets and sets the webEmbed
                The webEmbed
        if "webEmbed" in self._prop_dict:
            if isinstance(self._prop_dict["webEmbed"], OneDriveObjectBase):
                return self._prop_dict["webEmbed"]
                self._prop_dict["webEmbed"] = OpenWithApp(self._prop_dict["webEmbed"])
    @web_embed.setter
    def web_embed(self, val):
        self._prop_dict["webEmbed"] = val
