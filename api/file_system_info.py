class FileSystemInfo(OneDriveObjectBase):
    def created_date_time(self):
        """Gets and sets the createdDateTime
            datetime:
                The createdDateTime
        if "createdDateTime" in self._prop_dict:
            if '.' in self._prop_dict["createdDateTime"]:
                return datetime.strptime(self._prop_dict["createdDateTime"].replace("Z", ""), "%Y-%m-%dT%H:%M:%S.%f")
                return datetime.strptime(self._prop_dict["createdDateTime"].replace("Z", ""), "%Y-%m-%dT%H:%M:%S")
    @created_date_time.setter
    def created_date_time(self, val):
        self._prop_dict["createdDateTime"] = val.isoformat()+((".0" if val.time().microsecond == 0 else "")+"Z")
    def last_modified_date_time(self):
        """Gets and sets the lastModifiedDateTime
                The lastModifiedDateTime
        if "lastModifiedDateTime" in self._prop_dict:
            if '.' in self._prop_dict["lastModifiedDateTime"]:
                return datetime.strptime(self._prop_dict["lastModifiedDateTime"].replace("Z", ""), "%Y-%m-%dT%H:%M:%S.%f")
                return datetime.strptime(self._prop_dict["lastModifiedDateTime"].replace("Z", ""), "%Y-%m-%dT%H:%M:%S")
    @last_modified_date_time.setter
    def last_modified_date_time(self, val):
        self._prop_dict["lastModifiedDateTime"] = val.isoformat()+((".0" if val.time().microsecond == 0 else "")+"Z")
