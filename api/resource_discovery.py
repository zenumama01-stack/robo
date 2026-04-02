class ResourceDiscoveryRequest(object):
        self._method = 'GET'
        self._discovery_service_url = 'https://api.office.com/discovery/v2.0/me/services'
    def get_service_info(self, access_token):
        """Send request to discovery service. Return ServiceInfo
        for valid services.
            access_token (str): A valid access token for resource
                'https://api.office.com/discovery/'
            List of :class:`ServiceInfo<onedrivesdk.helpers.resource_discovery.ServiceInfo>`:
                ServiceInfo for each service that the caller can access. NOTE: values that
                do not provide access to OneDrive for Business will be excluded (i.e. must have
                capability = 'MyFiles' and service_api_version = 'v2.0'
        headers = {'Authorization': 'Bearer ' + access_token}
        response = json.loads(requests.get(self._discovery_service_url, headers=headers).text)
        service_info_list = [ServiceInfo(x) for x in response['value']]
        trimmed_service_info_list = [si for si in service_info_list
                                     if si.capability == 'MyFiles' and si.service_api_version == 'v2.0']
        return trimmed_service_info_list
class ServiceInfo(object):
    Objects representing ServiceInfo returned by the Discovery Service.
        More info can be found here:
        https://msdn.microsoft.com/en-us/office/office365/api/discovery-service-rest-operations
        return 'serviceResourceId: {}\nserviceEndpointUri: {}'\
            .format(self.service_resource_id, self.service_endpoint_uri)
    def _prop_dict_get(self, prop):
        if prop in self._prop_dict:
            return self._prop_dict[prop]
    def _prop_dict_set(self, prop, val):
        self._prop_dict[prop] = val
    def capability(self):
            str: The Capability of the service
        return self._prop_dict_get('capability')
    @capability.setter
    def capability(self, value):
        self._prop_dict_set('capability', value)
    def service_id(self):
            str: The ServiceId
        return self._prop_dict_get('serviceId')
    @service_id.setter
    def service_id(self, value):
        self._prop_dict_set('serviceId', value)
    def service_name(self):
            str: The name of the service
        return self._prop_dict_get('serviceName')
    @service_name.setter
    def service_name(self, value):
        self._prop_dict_set('serviceName', value)
    def service_endpoint_uri(self):
            str: The serviceEndpointUri
                Ex: https://contoso-my.sharepoint.com/personal/alexd_contoso_com
        return self._prop_dict_get('serviceEndpointUri')
    @service_endpoint_uri.setter
    def service_endpoint_uri(self, value):
        self._prop_dict_set('serviceEndpointUri', value)
    def service_resource_id(self):
            str: the serviceResourceId
                Ex: https://contoso-my.sharepoint.com/
        return self._prop_dict_get('serviceResourceId')
    @service_resource_id.setter
    def service_resource_id(self, value):
        self._prop_dict_set('serviceResourceId', value)
    def service_api_version(self):
            str: the serviceApiVersion
        return self._prop_dict_get('serviceApiVersion')
    @service_api_version.setter
    def service_api_version(self, value):
        self._prop_dict_set('serviceApiVersion', value)
