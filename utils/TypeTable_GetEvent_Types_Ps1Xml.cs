        private void Process_GetEvent_Types_Ps1Xml(string filePath, ConcurrentBag<string> errors)
            typesInfo.Add(new SessionStateTypeEntry(filePath));
            PSMemberInfoInternalCollection<PSMemberInfo> memberSetMembers = null;
            HashSet<string> newMembers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            #region System.Diagnostics.Eventing.Reader.EventLogConfiguration
            typeName = @"System.Diagnostics.Eventing.Reader.EventLogConfiguration";
            typeMembers = _extendedMembers.GetOrAdd(typeName, GetValueFactoryBasedOnInitCapacity(capacity: 1));
            // Process standard members.
            memberSetMembers = new PSMemberInfoInternalCollection<PSMemberInfo>(capacity: 1);
            AddMember(
                new PSPropertySet(
                    @"DefaultDisplayPropertySet",
                    new List<string> { "LogName", "MaximumSizeInBytes", "RecordCount", "LogMode" }),
                memberSetMembers,
                isOverride: false);
            ProcessStandardMembers(
                typeMembers,
            #endregion System.Diagnostics.Eventing.Reader.EventLogConfiguration
            #region System.Diagnostics.Eventing.Reader.EventLogRecord
            typeName = @"System.Diagnostics.Eventing.Reader.EventLogRecord";
                    new List<string> { "TimeCreated", "ProviderName", "Id", "Message" }),
            #endregion System.Diagnostics.Eventing.Reader.EventLogRecord
            #region System.Diagnostics.Eventing.Reader.ProviderMetadata
            typeName = @"System.Diagnostics.Eventing.Reader.ProviderMetadata";
            typeMembers = _extendedMembers.GetOrAdd(typeName, GetValueFactoryBasedOnInitCapacity(capacity: 2));
            // Process regular members.
            newMembers.Add(@"ProviderName");
                new PSAliasProperty(@"ProviderName", @"Name", conversionType: null),
                    new List<string> { "Name", "LogLinks" }),
            #endregion System.Diagnostics.Eventing.Reader.ProviderMetadata
            #region Microsoft.PowerShell.Commands.GetCounter.CounterSet
            typeName = @"Microsoft.PowerShell.Commands.GetCounter.CounterSet";
            newMembers.Add(@"Counter");
                new PSAliasProperty(@"Counter", @"Paths", conversionType: null),
            #endregion Microsoft.PowerShell.Commands.GetCounter.CounterSet
            #region Microsoft.PowerShell.Commands.GetCounter.PerformanceCounterSample
            typeName = @"Microsoft.PowerShell.Commands.GetCounter.PerformanceCounterSample";
                    new List<string> { "Path", "InstanceName", "CookedValue" }),
            #endregion Microsoft.PowerShell.Commands.GetCounter.PerformanceCounterSample
            #region Microsoft.PowerShell.Commands.GetCounter.PerformanceCounterSampleSet
            typeName = @"Microsoft.PowerShell.Commands.GetCounter.PerformanceCounterSampleSet";
            newMembers.Add(@"Readings");
                new PSScriptProperty(
                    @"Readings",
                    GetScriptBlock(@"$strPaths = """"
          foreach ($ctr in $this.CounterSamples)
              $strPaths += ($ctr.Path + "" :"" + ""`n"")
              $strPaths += ($ctr.CookedValue.ToString() + ""`n`n"")
          return $strPaths"),
                    setterScript: null,
                    shouldCloneOnAccess: true),
                    new List<string> { "Timestamp", "Readings" }),
            #endregion Microsoft.PowerShell.Commands.GetCounter.PerformanceCounterSampleSet
            // Update binder version for newly added members.
            foreach (string memberName in newMembers)
