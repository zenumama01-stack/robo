class UploadSession(OneDriveObjectBase):
    def upload_url(self):
        """Gets and sets the uploadUrl
                The uploadUrl
        if "uploadUrl" in self._prop_dict:
            return self._prop_dict["uploadUrl"]
    @upload_url.setter
    def upload_url(self, val):
        self._prop_dict["uploadUrl"] = val
        """Gets and sets the expirationDateTime
    def next_expected_ranges(self):
        """Gets and sets the nextExpectedRanges
                The nextExpectedRanges
        if "nextExpectedRanges" in self._prop_dict:
            return self._prop_dict["nextExpectedRanges"]
    @next_expected_ranges.setter
    def next_expected_ranges(self, val):
        self._prop_dict["nextExpectedRanges"] = val
