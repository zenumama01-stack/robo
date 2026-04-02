class OneDriveObjectBase(object):
    DATETIME_FORMAT = "%Y-%m-%dT%H:%M:%S.%fZ"
    DATETIME_FORMAT_NO_MILLISECONDS = "%Y-%m-%dT%H:%M:%SZ"
    def to_dict(self):
        """Returns the serialized form of the :class:`OneDriveObjectBase`
        as a dict. All sub-objects that are based off of :class:`OneDriveObjectBase`
        are also serialized and inserted into the dict
            dict: The serialized form of the :class:`OneDriveObjectBase`
        serialized = {}
        for prop in self._prop_dict:
            if isinstance(self._prop_dict[prop], OneDriveObjectBase):
                serialized[prop] = self._prop_dict[prop].to_dict()
                serialized[prop] = self._prop_dict[prop]
    def get_datetime_from_string(s):
            dt = datetime.strptime(
                OneDriveObjectBase.DATETIME_FORMAT)
        except ValueError as ve:
            # Try again with other format
                OneDriveObjectBase.DATETIME_FORMAT_NO_MILLISECONDS)
        return dt
    def get_string_from_datetime(dt):
        return dt.strftime(OneDriveObjectBase.DATETIME_FORMAT)
