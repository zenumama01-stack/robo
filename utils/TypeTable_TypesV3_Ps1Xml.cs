        private void Process_TypesV3_Ps1Xml(string filePath, ConcurrentBag<string> errors)
            #region System.Security.Cryptography.X509Certificates.X509Certificate2
            typeName = @"System.Security.Cryptography.X509Certificates.X509Certificate2";
            typeMembers = _extendedMembers.GetOrAdd(typeName, GetValueFactoryBasedOnInitCapacity(capacity: 3));
            newMembers.Add(@"EnhancedKeyUsageList");
                    @"EnhancedKeyUsageList",
                    GetScriptBlock(@",(new-object Microsoft.Powershell.Commands.EnhancedKeyUsageProperty -argumentlist $this).EnhancedKeyUsageList;"),
            newMembers.Add(@"DnsNameList");
                    @"DnsNameList",
                    GetScriptBlock(@",(new-object Microsoft.Powershell.Commands.DnsNameProperty -argumentlist $this).DnsNameList;"),
            newMembers.Add(@"SendAsTrustedIssuer");
                    @"SendAsTrustedIssuer",
                    GetScriptBlock(@"[Microsoft.Powershell.Commands.SendAsTrustedIssuerProperty]::ReadSendAsTrustedIssuerProperty($this)"),
                    GetScriptBlock(@"$sendAsTrustedIssuer = $args[0]
                    [Microsoft.Powershell.Commands.SendAsTrustedIssuerProperty]::WriteSendAsTrustedIssuerProperty($this,$this.PsPath,$sendAsTrustedIssuer)"),
            #endregion System.Security.Cryptography.X509Certificates.X509Certificate2
            #region System.Management.Automation.Remoting.PSSenderInfo
            typeName = @"System.Management.Automation.Remoting.PSSenderInfo";
            newMembers.Add(@"ConnectedUser");
                    @"ConnectedUser",
                    GetScriptBlock(@"$this.UserInfo.Identity.Name"),
            newMembers.Add(@"RunAsUser");
                    @"RunAsUser",
                    GetScriptBlock(@"if($null -ne $this.UserInfo.WindowsIdentity)
                $this.UserInfo.WindowsIdentity.Name
            }"),
            #endregion System.Management.Automation.Remoting.PSSenderInfo
            #region System.Management.Automation.CompletionResult
            typeName = @"System.Management.Automation.CompletionResult";
                new PSNoteProperty(@"SerializationDepth", 1),
            #endregion System.Management.Automation.CompletionResult
            #region Deserialized.System.Management.Automation.CompletionResult
            typeName = @"Deserialized.System.Management.Automation.CompletionResult";
                new PSNoteProperty(@"TargetTypeForDeserialization", typeof(Microsoft.PowerShell.DeserializingTypeConverter)),
            #endregion Deserialized.System.Management.Automation.CompletionResult
            #region System.Management.Automation.CommandCompletion
            typeName = @"System.Management.Automation.CommandCompletion";
            #endregion System.Management.Automation.CommandCompletion
            #region Deserialized.System.Management.Automation.CommandCompletion
            typeName = @"Deserialized.System.Management.Automation.CommandCompletion";
            #endregion Deserialized.System.Management.Automation.CommandCompletion
            #region Microsoft.PowerShell.Commands.ModuleSpecification
            typeName = @"Microsoft.PowerShell.Commands.ModuleSpecification";
            #endregion Microsoft.PowerShell.Commands.ModuleSpecification
            #region Deserialized.Microsoft.PowerShell.Commands.ModuleSpecification
            typeName = @"Deserialized.Microsoft.PowerShell.Commands.ModuleSpecification";
            #endregion Deserialized.Microsoft.PowerShell.Commands.ModuleSpecification
            #region System.Management.Automation.JobStateEventArgs
            typeName = @"System.Management.Automation.JobStateEventArgs";
                new PSNoteProperty(@"SerializationDepth", 2),
            #endregion System.Management.Automation.JobStateEventArgs
            #region Deserialized.System.Management.Automation.JobStateEventArgs
            typeName = @"Deserialized.System.Management.Automation.JobStateEventArgs";
            #endregion Deserialized.System.Management.Automation.JobStateEventArgs
            #region System.Exception
            typeName = @"System.Exception";
            #endregion System.Exception
            #region System.Management.Automation.Remoting.PSSessionOption
            typeName = @"System.Management.Automation.Remoting.PSSessionOption";
            #endregion System.Management.Automation.Remoting.PSSessionOption
            #region Deserialized.System.Management.Automation.Remoting.PSSessionOption
            typeName = @"Deserialized.System.Management.Automation.Remoting.PSSessionOption";
            #endregion Deserialized.System.Management.Automation.Remoting.PSSessionOption
            #region System.Management.Automation.DebuggerStopEventArgs
            typeName = @"System.Management.Automation.DebuggerStopEventArgs";
            newMembers.Add(@"SerializedInvocationInfo");
                new PSCodeProperty(
                    @"SerializedInvocationInfo",
                    GetMethodInfo(typeof(Microsoft.PowerShell.DeserializingTypeConverter), @"GetInvocationInfo"),
                    setterCodeReference: null)
                { IsHidden = true },
            memberSetMembers = new PSMemberInfoInternalCollection<PSMemberInfo>(capacity: 3);
                new PSNoteProperty(@"SerializationMethod", @"SpecificProperties"),
                    @"PropertySerializationSet",
                    new List<string> { "Breakpoints", "ResumeAction", "SerializedInvocationInfo" }),
            #endregion System.Management.Automation.DebuggerStopEventArgs
            #region Deserialized.System.Management.Automation.DebuggerStopEventArgs
            typeName = @"Deserialized.System.Management.Automation.DebuggerStopEventArgs";
            #endregion Deserialized.System.Management.Automation.DebuggerStopEventArgs
