    /// Job wrapping invocation of a CreateInstance or ModifyInstance intrinsic CIM method.
    internal abstract class PropertySettingJob<T> : MethodInvocationJobBase<T>
        internal PropertySettingJob(CimJobContext jobContext, bool passThru, CimInstance objectToModify, MethodInvocationInfo methodInvocationInfo)
                    objectToModify.ToString(),
        internal void ModifyLocalCimInstance(CimInstance cimInstance)
            foreach (MethodParameter methodParameter in this.GetMethodInputParameters())
                CimValueConverter.AssertIntrinsicCimType(methodParameter.ParameterType);
                CimProperty propertyBeingModified = cimInstance.CimInstanceProperties[methodParameter.Name];
                if (propertyBeingModified != null)
                    propertyBeingModified.Value = methodParameter.Value;
                    CimProperty propertyBeingAdded = CimProperty.Create(
                        methodParameter.Name,
                        methodParameter.Value,
                        CimValueConverter.GetCimTypeEnum(methodParameter.ParameterType),
                    cimInstance.CimInstanceProperties.Add(propertyBeingAdded);
