class Location(OneDriveObjectBase):
    def altitude(self):
        """Gets and sets the altitude
                The altitude
        if "altitude" in self._prop_dict:
            return self._prop_dict["altitude"]
    @altitude.setter
    def altitude(self, val):
        self._prop_dict["altitude"] = val
    def latitude(self):
        """Gets and sets the latitude
                The latitude
        if "latitude" in self._prop_dict:
            return self._prop_dict["latitude"]
    @latitude.setter
    def latitude(self, val):
        self._prop_dict["latitude"] = val
    def longitude(self):
        """Gets and sets the longitude
                The longitude
        if "longitude" in self._prop_dict:
            return self._prop_dict["longitude"]
    @longitude.setter
    def longitude(self, val):
        self._prop_dict["longitude"] = val
