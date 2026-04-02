class Photo(OneDriveObjectBase):
    def camera_make(self):
        """Gets and sets the cameraMake
                The cameraMake
        if "cameraMake" in self._prop_dict:
            return self._prop_dict["cameraMake"]
    @camera_make.setter
    def camera_make(self, val):
        self._prop_dict["cameraMake"] = val
    def camera_model(self):
        """Gets and sets the cameraModel
                The cameraModel
        if "cameraModel" in self._prop_dict:
            return self._prop_dict["cameraModel"]
    @camera_model.setter
    def camera_model(self, val):
        self._prop_dict["cameraModel"] = val
    def exposure_denominator(self):
        """Gets and sets the exposureDenominator
                The exposureDenominator
        if "exposureDenominator" in self._prop_dict:
            return self._prop_dict["exposureDenominator"]
    @exposure_denominator.setter
    def exposure_denominator(self, val):
        self._prop_dict["exposureDenominator"] = val
    def exposure_numerator(self):
        """Gets and sets the exposureNumerator
                The exposureNumerator
        if "exposureNumerator" in self._prop_dict:
            return self._prop_dict["exposureNumerator"]
    @exposure_numerator.setter
    def exposure_numerator(self, val):
        self._prop_dict["exposureNumerator"] = val
    def focal_length(self):
        """Gets and sets the focalLength
                The focalLength
        if "focalLength" in self._prop_dict:
            return self._prop_dict["focalLength"]
    @focal_length.setter
    def focal_length(self, val):
        self._prop_dict["focalLength"] = val
    def f_number(self):
        """Gets and sets the fNumber
                The fNumber
        if "fNumber" in self._prop_dict:
            return self._prop_dict["fNumber"]
    @f_number.setter
    def f_number(self, val):
        self._prop_dict["fNumber"] = val
    def taken_date_time(self):
        """Gets and sets the takenDateTime
                The takenDateTime
        if "takenDateTime" in self._prop_dict:
            if '.' in self._prop_dict["takenDateTime"]:
                return datetime.strptime(self._prop_dict["takenDateTime"].replace("Z", ""), "%Y-%m-%dT%H:%M:%S.%f")
                return datetime.strptime(self._prop_dict["takenDateTime"].replace("Z", ""), "%Y-%m-%dT%H:%M:%S")
    @taken_date_time.setter
    def taken_date_time(self, val):
        self._prop_dict["takenDateTime"] = val.isoformat()+((".0" if val.time().microsecond == 0 else "")+"Z")
    def iso(self):
        """Gets and sets the iso
                The iso
        if "iso" in self._prop_dict:
            return self._prop_dict["iso"]
    @iso.setter
    def iso(self, val):
        self._prop_dict["iso"] = val
