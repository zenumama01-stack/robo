        #region cmdlets
        /// Gets the value of the specified cmdlet from the cmdlet table.
        /// The name of the cmdlet value to retrieve.
        /// The CmdletInfo representing the cmdlet.
        internal CmdletInfo GetCmdlet(string cmdletName)
            return GetCmdlet(cmdletName, CommandOrigin.Internal);
        /// The origin of the command trying to retrieve this cmdlet.
        internal CmdletInfo GetCmdlet(string cmdletName, CommandOrigin origin)
            CmdletInfo result = null;
            if (string.IsNullOrEmpty(cmdletName))
                result = scope.GetCmdlet(cmdletName);
                    // Now check the visibility of the cmdlet...
                    // Make sure the cmdlet isn't private or if it is that the current
                    // scope is the same scope the cmdlet was retrieved from.
        internal CmdletInfo GetCmdletAtScope(string cmdletName, string scopeID)
        /// Gets an IEnumerable for the cmdlet table.
        internal IDictionary<string, List<CmdletInfo>> GetCmdletTable()
            Dictionary<string, List<CmdletInfo>> result =
                new Dictionary<string, List<CmdletInfo>>(StringComparer.OrdinalIgnoreCase);
                foreach (KeyValuePair<string, List<CmdletInfo>> entry in scope.CmdletTable)
                    if (!result.ContainsKey(entry.Key))
                        List<CmdletInfo> toBeAdded = new List<CmdletInfo>();
                        foreach (CmdletInfo cmdletInfo in entry.Value)
                            if ((cmdletInfo.Options & ScopedItemOptions.Private) == 0 ||
                                toBeAdded.Add(cmdletInfo);
                        result.Add(entry.Key, toBeAdded);
        /// Gets an IEnumerable for the cmdlet table for a given scope.
        internal IDictionary<string, List<CmdletInfo>> GetCmdletTableAtScope(string scopeID)
        internal void RemoveCmdlet(string name, int index, bool force)
            RemoveCmdlet(name, index, force, CommandOrigin.Internal);
        /// Removes a cmdlet from the function table.
        /// The name of the cmdlet to remove.
        /// <param name="index">
        /// If true, the cmdlet is removed even if it is ReadOnly.
        /// If the function is constant.
        internal void RemoveCmdlet(string name, int index, bool force, CommandOrigin origin)
                CmdletInfo cmdletInfo =
                    scope.GetCmdlet(name);
                    if ((cmdletInfo.Options & ScopedItemOptions.Private) != 0 &&
                        cmdletInfo = null;
                        scope.RemoveCmdlet(name, index, force);
        /// Removes a cmdlet entry from the cmdlet table.
        /// The name of the cmdlet entry to remove.
        internal void RemoveCmdletEntry(string name, bool force)
                        scope.RemoveCmdletEntry(name, force);
        #endregion cmdlets
