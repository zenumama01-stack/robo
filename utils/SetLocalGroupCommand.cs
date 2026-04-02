    /// The Set-LocalGroup cmdlet modifies the properties of a local security group
    [Cmdlet(VerbsCommon.Set, "LocalGroup",
            HelpUri = "https://go.microsoft.com/fwlink/?LinkId=717979")]
    [Alias("slg")]
    public class SetLocalGroupCommand : Cmdlet
        [Parameter(Mandatory = true)]
        /// Specifies the local group account to modify in the local Security
                LocalGroup group = null;
                    if (CheckShouldProcess(InputObject.ToString()))
                        group = InputObject;
                    group = sam.GetLocalGroup(Name);
                    if (!CheckShouldProcess(Name))
                        group = null;
                    group = sam.GetLocalGroup(SID);
                    if (!CheckShouldProcess(SID.ToString()))
                if (group != null)
                    var delta = group.Clone();
                    delta.Description = Description;
                    sam.UpdateLocalGroup(group, delta);
            return ShouldProcess(target, Strings.ActionSetGroup);
