class SearchResult(OneDriveObjectBase):
    def on_click_telemetry_url(self):
        """Gets and sets the onClickTelemetryUrl
                The onClickTelemetryUrl
        if "onClickTelemetryUrl" in self._prop_dict:
            return self._prop_dict["onClickTelemetryUrl"]
    @on_click_telemetry_url.setter
    def on_click_telemetry_url(self, val):
        self._prop_dict["onClickTelemetryUrl"] = val
