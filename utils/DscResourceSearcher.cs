    /// Searcher class for finding DscResources on the system.
    internal class DscResourceSearcher : IEnumerable<DscResourceInfo>, IEnumerator<DscResourceInfo>
        internal DscResourceSearcher(
            Diagnostics.Assert(!string.IsNullOrEmpty(resourceName), "caller to verify commandName is valid");
            _resourceName = resourceName;
        #region private properties
        private readonly string _resourceName = null;
        private readonly ExecutionContext _context = null;
        private DscResourceInfo _currentMatch = null;
        private IEnumerator<DscResourceInfo> _matchingResource = null;
        private Collection<DscResourceInfo> _matchingResourceList = null;
        /// Reset the Iterator.
            _matchingResource = null;
        /// Reset and dispose the Iterator.
        /// Get the Enumerator.
        IEnumerator<DscResourceInfo> IEnumerable<DscResourceInfo>.GetEnumerator()
        /// Move to the Next value in the enumerator.
            _currentMatch = GetNextDscResource();
        /// Return the current DscResource.
        DscResourceInfo IEnumerator<DscResourceInfo>.Current
        /// Return the current DscResource as object.
                return ((IEnumerator<DscResourceInfo>)this).Current;
        /// Invoke command Get-DscResource with resource name to find the resource.
        /// When found add them to the enumerator. If we have already got it, return the next resource.
        /// <returns>Next DscResource Info object or null if none are found.</returns>
        private DscResourceInfo GetNextDscResource()
            var ps = PowerShell.Create(RunspaceMode.CurrentRunspace).AddCommand("Get-DscResource");
            WildcardPattern resourceMatcher = WildcardPattern.Get(_resourceName, WildcardOptions.IgnoreCase);
            if (_matchingResourceList == null)
                Collection<PSObject> psObjs = ps.Invoke();
                _matchingResourceList = new Collection<DscResourceInfo>();
                foreach (dynamic resource in psObjs)
                    if (resource.Name != null)
                        string resourceName = resource.Name;
                        if (resourceMatcher.IsMatch(resourceName))
                            DscResourceInfo resourceInfo = new DscResourceInfo(resourceName,
                                                                               resource.ResourceType,
                                                                               resource.Path,
                                                                               resource.ParentPath,
                                                                               _context
                            resourceInfo.FriendlyName = resource.FriendlyName;
                            resourceInfo.CompanyName = resource.CompanyName;
                            PSModuleInfo psMod = resource.Module as PSModuleInfo;
                            if (psMod != null)
                                resourceInfo.Module = psMod;
                            if (resource.ImplementedAs != null)
                                ImplementedAsType impType;
                                if (Enum.TryParse<ImplementedAsType>(resource.ImplementedAs.ToString(), out impType))
                                    resourceInfo.ImplementedAs = impType;
                            var properties = resource.Properties as IList;
                                List<DscResourcePropertyInfo> propertyList = new List<DscResourcePropertyInfo>();
                                foreach (dynamic prop in properties)
                                    DscResourcePropertyInfo propInfo = new DscResourcePropertyInfo();
                                    propInfo.Name = prop.Name;
                                    propInfo.PropertyType = prop.PropertyType;
                                    propInfo.UpdateValues(prop.Values);
                                    propertyList.Add(propInfo);
                                resourceInfo.UpdateProperties(propertyList);
                            _matchingResourceList.Add(resourceInfo);
                    _matchingResource = _matchingResourceList.GetEnumerator();
            if (!_matchingResource.MoveNext())
                return _matchingResource.Current;
