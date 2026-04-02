    #region VERBS
    /// Verbs that are commonly used in cmdlet names.
    /// These verbs are recommended over their synonyms when used as the verb name
    /// for cmdlets.
    public static class VerbsCommon
        /// Synonyms: Add to, append or attach.
        public const string Add = "Add";
        /// Remove all the elements or content of a container.
        public const string Clear = "Clear";
        /// Change the state of a resource to make it inaccessible, unavailable, or unusable.
        public const string Close = "Close";
        /// Copy a resource to another name or another container.
        public const string Copy = "Copy";
        /// Enters a context.
        public const string Enter = "Enter";
        /// Exits a context.
        public const string Exit = "Exit";
        /// Search for an object.
        public const string Find = "Find";
        /// Formats an object for output.
        public const string Format = "Format";
        /// Get the contents/object/children/properties/relations/... of a resource.
        public const string Get = "Get";
        /// Remove from visibility.
        public const string Hide = "Hide";
        /// Combines resources into one resource.
        public const string Join = "Join";
        /// Lock a resource.
        public const string Lock = "Lock";
        /// Move a resource.
        public const string Move = "Move";
        /// Create a new resource.
        public const string New = "New";
        /// Change the state of a resource to make it accessible, available, or usable.
        public const string Open = "Open";
        /// Increases the effectiveness of a resource.
        public const string Optimize = "Optimize";
        /// To set as the current context, including the ability
        /// to reverse this action.
        public const string Push = "Push";
        /// To restore a context saved by a Push operation.
        public const string Pop = "Pop";
        /// Act on a resource again.
        public const string Redo = "Redo";
        /// Remove a resource from a container.
        public const string Remove = "Remove";
        /// Give a resource a new name.
        public const string Rename = "Rename";
        /// Set/reset the contents/object/properties/relations... of a resource.
        public const string Reset = "Reset";
        /// Changes the size of a resource.
        public const string Resize = "Resize";
        /// Get a reference to a resource or summary information about a resource by looking in a specified collection.
        /// Does not actually retrieve that resource.
        public const string Search = "Search";
        /// To take as a choice from among several; pick out.
        public const string Select = "Select";
        /// Set the contents/object/properties/relations... of a resource.
        public const string Set = "Set";
        /// Makes visible, or displays information. Combines get, format, and out verbs.
        public const string Show = "Show";
        /// Pass from one resource or point to another while disregarding or omitting intervening resources or points.
        public const string Skip = "Skip";
        /// Split an object into portions. parts or fragments.
        public const string Split = "Split";
        /// Move to the next point or resource.
        public const string Step = "Step";
        public const string Switch = "Switch";
        /// Reverse an action or process.
        public const string Undo = "Undo";
        /// Unlock a resource.
        public const string Unlock = "Unlock";
        /// Continually inspect a resource for changes.
        public const string Watch = "Watch";
    /// Verbs that are commonly used in cmdlet names when the cmdlet manipulates data.
    public static class VerbsData
        /// Backup.
        public const string Backup = "Backup";
        /// Establish a well defined state to be able to roll back to.
        public const string Checkpoint = "Checkpoint";
        /// Compare this resource with another one and produce a set of differences.
        public const string Compare = "Compare";
        /// Reduce in size.
        public const string Compress = "Compress";
        /// Change from one encoding to another or from one unit base to another (e.g. feet to meters)
        public const string Convert = "Convert";
        /// Convert from the format named in the noun to a general-purpose format (e.g. string or int).
        public const string ConvertFrom = "ConvertFrom";
        /// Convert from a general-purpose format (e.g. string or int) to the format named in the noun.
        public const string ConvertTo = "ConvertTo";
        /// To dismount - to get off. To detach.
        public const string Dismount = "Dismount";
        /// Performs an in-place modification of a resource.
        public const string Edit = "Edit";
        /// Uncompress or increase in size.
        public const string Expand = "Expand";
        /// Make a copy of a set of resources using an interchange format.
        public const string Export = "Export";
        /// Arrange or associate one or more resources.
        public const string Group = "Group";
        /// Create a set of resources using an interchange format.
        public const string Import = "Import";
        /// Prepare a resource for use. Assign a beginning value to something.
        public const string Initialize = "Initialize";
        /// Limit the consumption of a resource or apply a constraint on a resource.
        public const string Limit = "Limit";
        /// Take multiple instances and create a single instance.
        public const string Merge = "Merge";
        /// To mount - to attache a named entity to a hierarchy at the pathname location. To set in position.
        public const string Mount = "Mount";
        /// Out - direct to a port. Output something to a port.
        public const string Out = "Out";
        /// Make known and accessible to another.
        public const string Publish = "Publish";
        /// Rollback state to a predefined snapshot/checkpoint.
        public const string Restore = "Restore";
        /// Store state in a permanent location.
        public const string Save = "Save";
        /// Coerce one or more resources to the same state.
        public const string Sync = "Sync";
        /// Remove from public access and visibility.
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unpublish")]
        public const string Unpublish = "Unpublish";
        /// Update a resource with new elements or refresh from a source of truth.
        public const string Update = "Update";
    /// Verbs that are commonly used in cmdlet names when the cmdlet manipulates the lifecycle of something.
    public static class VerbsLifecycle
        /// Agree to the status of a resource or process.
        public const string Approve = "Approve";
        /// State or affirm the state of an object.
        public const string Assert = "Assert";
        /// Creates an artifact (usually a binary or document) out of some set of input files (usually source code or declarative documents)
        public const string Build = "Build";
        /// Finalize an interruptible activity. Makes pending changes permanent.
        public const string Complete = "Complete";
        /// Acknowledge, verify, or validate the state of a resource.
        public const string Confirm = "Confirm";
        /// Refuse, object, block, or oppose the state of a resource or process.
        public const string Deny = "Deny";
        /// Sends an application, website, or solution to a remote target[s] in such a way that a consumer of that solution can access it after deployment is complete.
        public const string Deploy = "Deploy";
        /// Stop and/or configure something to be unavailable (e.g unable to not start again)
        public const string Disable = "Disable";
        /// Configure to be available (e.g. able to start)
        public const string Enable = "Enable";
        /// Settle in an indicated place or condition (optionally initializing for use)
        public const string Install = "Install";
        /// Calls or launches an activity that cannot be stopped.
        public const string Invoke = "Invoke";
        /// Record details about an item in a public store or publishing location
        public const string Register = "Register";
        /// Ask for a resource or permissions.
        public const string Request = "Request";
        /// Terminate existing activity and begin it again (with the same configuration)
        public const string Restart = "Restart";
        /// Begin an activity again after it was suspended.
        public const string Resume = "Resume";
        /// Begin an activity.
        public const string Start = "Start";
        ///Discontinue or cease an activity
        public const string Stop = "Stop";
        /// Present a resource for approval.
        public const string Submit = "Submit";
        /// Suspend an activity temporarily.
        public const string Suspend = "Suspend";
        /// Remove or disassociate.
        public const string Uninstall = "Uninstall";
        /// Remove details of an item from a public store or publishing location
        public const string Unregister = "Unregister";
        /// Suspend execution until an expected event
        public const string Wait = "Wait";
    /// Verbs that are commonly used in cmdlet names when the cmdlet is used to diagnose the health of something.
    public static class VerbsDiagnostic
        /// Iteratively interact with a resource or activity for the purpose finding a flaw or better understanding of what is occurring.
        public const string Debug = "Debug";
        /// Calculate/identify resources consumed by a specified operation or retrieve statistics about a resource.
        public const string Measure = "Measure";
        /// Determine whether a resource is alive and responding to requests.
        public const string Ping = "Ping";
        /// Detect and correct problems.
        public const string Repair = "Repair";
        /// Map a shorthand name will be bound to a longname.
        public const string Resolve = "Resolve";
        /// Verify the operational validity or consistency of a resource.
        public const string Test = "Test";
        /// Trace activities performed by a specified operation.
        public const string Trace = "Trace";
    /// Verbs that are commonly used in cmdlet names when the cmdlet is used to communicate with something.
    public static class VerbsCommunications
        /// Associate subsequent activities with a resource.
        public const string Connect = "Connect";
        /// Disassociate from a resource.
        public const string Disconnect = "Disconnect";
        /// To read - to obtain (data) from a storage medium or port.
        public const string Read = "Read";
        /// Take or acquire from a source.
        public const string Receive = "Receive";
        /// Convey by an intermediary to a destination.
        public const string Send = "Send";
        /// TO write - communicate or express. Display data.
        public const string Write = "Write";
    /// Verbs that are commonly used in cmdlet names when the cmdlet is used to secure a resource.
    public static class VerbsSecurity
        /// Prevent access to or usage of a resource.
        public const string Block = "Block";
        /// Gives access to a resource.
        public const string Grant = "Grant";
        /// Guard a resource from attack or loss.
        public const string Protect = "Protect";
        /// Removes access to a resource.
        public const string Revoke = "Revoke";
        /// Allow access to or usage of a resource.
        public const string Unblock = "Unblock";
        /// Remove guards from a resource that prevent it from attack or loss.
        public const string Unprotect = "Unprotect";
    /// Canonical verbs that don't fit into any of the other categories.
    public static class VerbsOther
        /// To use or include a resource. To set as the context of an action.
        public const string Use = "Use";
    /// Verb descriptions.
    internal static class VerbDescriptions
        /// Gets verb description from the resource file.
        public static string GetVerbDescription(string verb)
            return VerbDescriptionStrings.ResourceManager.GetString(verb);
    /// Verb Alias prefixes.
    internal static class VerbAliasPrefixes
        /// "Add" verb alias prefix.
        public const string Add = "a";
        /// "Approve" verb alias prefix.
        public const string Approve = "ap";
        /// "Assert" verb alias prefix.
        public const string Assert = "as";
        /// "Backup" verb alias prefix.
        public const string Backup = "ba";
        /// "Block" verb alias prefix.
        public const string Block = "bl";
        /// "Build" verb alias prefix.
        public const string Build = "bd";
        /// "Checkpoint" verb alias prefix.
        public const string Checkpoint = "ch";
        /// "Clear" verb alias prefix.
        public const string Clear = "cl";
        /// "Close" verb alias prefix.
        public const string Close = "cs";
        /// "Compare" verb alias prefix.
        public const string Compare = "cr";
        /// "Complete" verb alias prefix.
        public const string Complete = "cmp";
        /// "Compress" verb alias prefix.
        public const string Compress = "cm";
        /// "Confirm" verb alias prefix.
        public const string Confirm = "cn";
        /// "Connect" verb alias prefix.
        public const string Connect = "cc";
        /// "Convert" verb alias prefix.
        public const string Convert = "cv";
        /// "ConvertFrom" verb alias prefix.
        public const string ConvertFrom = "cf";
        /// "ConvertTo" verb alias prefix.
        public const string ConvertTo = "ct";
        /// "Copy" verb alias prefix.
        public const string Copy = "cp";
        /// "Debug" verb alias prefix.
        public const string Debug = "db";
        /// "Deny" verb alias prefix.
        public const string Deny = "dn";
        /// "Deploy" verb alias prefix.
        public const string Deploy = "dp";
        /// "Disable" verb alias prefix.
        public const string Disable = "d";
        /// "Disconnect" verb alias prefix.
        public const string Disconnect = "dc";
        /// "Dismount" verb alias prefix.
        public const string Dismount = "dm";
        /// "Edit" verb alias prefix.
        public const string Edit = "ed";
        /// "Enable" verb alias prefix.
        public const string Enable = "e";
        /// "Enter" verb alias prefix.
        public const string Enter = "et";
        /// "Exit" verb alias prefix.
        public const string Exit = "ex";
        /// "Expand" verb alias prefix.
        public const string Expand = "en";
        /// "Export" verb alias prefix.
        public const string Export = "ep";
        /// "Find" verb alias prefix.
        public const string Find = "fd";
        /// "Format" verb alias prefix.
        public const string Format = "f";
        /// "Get" verb alias prefix.
        public const string Get = "g";
        /// "Grant" verb alias prefix.
        public const string Grant = "gr";
        /// "Group" verb alias prefix.
        public const string Group = "gp";
        /// "Hide" verb alias prefix.
        public const string Hide = "h";
        /// "Import" verb alias prefix.
        public const string Import = "ip";
        /// "Initialize" verb alias prefix.
        public const string Initialize = "in";
        /// "Install" verb alias prefix.
        public const string Install = "is";
        /// "Invoke" verb alias prefix.
        public const string Invoke = "i";
        /// "Join" verb alias prefix.
        public const string Join = "j";
        /// "Limit" verb alias prefix.
        public const string Limit = "l";
        /// "Lock" verb alias prefix.
        public const string Lock = "lk";
        /// "Measure" verb alias prefix.
        public const string Measure = "ms";
        /// "Merge" verb alias prefix.
        public const string Merge = "mg";
        /// "Mount" verb alias prefix.
        public const string Mount = "mt";
        /// "Move" verb alias prefix.
        public const string Move = "m";
        /// "New" verb alias prefix.
        public const string New = "n";
        /// "Open" verb alias prefix.
        public const string Open = "op";
        /// "Optimize" verb alias prefix.
        public const string Optimize = "om";
        /// "Out" verb alias prefix.
        public const string Out = "o";
        /// "Ping" verb alias prefix.
        public const string Ping = "pi";
        /// "Pop" verb alias prefix.
        public const string Pop = "pop";
        /// "Protect" verb alias prefix.
        public const string Protect = "pt";
        /// "Publish" verb alias prefix.
        public const string Publish = "pb";
        /// "Push" verb alias prefix.
        public const string Push = "pu";
        /// "Read" verb alias prefix.
        public const string Read = "rd";
        /// "Receive" verb alias prefix.
        public const string Receive = "rc";
        /// "Redo" verb alias prefix.
        public const string Redo = "re";
        /// "Register" verb alias prefix.
        public const string Register = "rg";
        /// "Remove" verb alias prefix.
        public const string Remove = "r";
        /// "Rename" verb alias prefix.
        public const string Rename = "rn";
        /// "Repair" verb alias prefix.
        public const string Repair = "rp";
        /// "Request" verb alias prefix.
        public const string Request = "rq";
        /// "Reset" verb alias prefix.
        public const string Reset = "rs";
        /// "Resize" verb alias prefix.
        public const string Resize = "rz";
        /// "Resolve" verb alias prefix.
        public const string Resolve = "rv";
        /// "Restart" verb alias prefix.
        public const string Restart = "rt";
        /// "Restore" verb alias prefix.
        public const string Restore = "rr";
        /// "Resume" verb alias prefix.
        public const string Resume = "ru";
        /// "Revoke" verb alias prefix.
        public const string Revoke = "rk";
        /// "Save" verb alias prefix.
        public const string Save = "sv";
        /// "Search" verb alias prefix.
        public const string Search = "sr";
        /// "Select" verb alias prefix.
        public const string Select = "sc";
        /// "Send" verb alias prefix.
        public const string Send = "sd";
        /// "Set" verb alias prefix.
        public const string Set = "s";
        /// "Show" verb alias prefix.
        public const string Show = "sh";
        /// "Sync" verb alias prefix.
        public const string Sync = "sy";
        /// "Skip" verb alias prefix.
        public const string Skip = "sk";
        /// "Split" verb alias prefix.
        public const string Split = "sl";
        /// "Start" verb alias prefix.
        public const string Start = "sa";
        /// "Step" verb alias prefix.
        public const string Step = "st";
        /// "Stop" verb alias prefix.
        public const string Stop = "sp";
        /// "Submit" verb alias prefix.
        public const string Submit = "sb";
        /// "Suspend" verb alias prefix.
        public const string Suspend = "ss";
        /// "Switch" verb alias prefix.
        public const string Switch = "sw";
        /// "Test" verb alias prefix.
        public const string Test = "t";
        /// "Trace" verb alias prefix.
        public const string Trace = "tr";
        /// "Unblock" verb alias prefix.
        public const string Unblock = "ul";
        /// "Undo" verb alias prefix.
        public const string Undo = "un";
        /// "Uninstall" verb alias prefix.
        public const string Uninstall = "us";
        /// "Unlock" verb alias prefix.
        public const string Unlock = "uk";
        /// "Unprotect" verb alias prefix.
        public const string Unprotect = "up";
        /// "Unpublish" verb alias prefix.
        public const string Unpublish = "ub";
        /// "Unregister" verb alias prefix.
        public const string Unregister = "ur";
        /// "Update" verb alias prefix.
        public const string Update = "ud";
        /// "Use" verb alias prefix.
        public const string Use = "u";
        /// "Wait" verb alias prefix.
        public const string Wait = "w";
        /// "Watch" verb alias prefix.
        public const string Watch = "wc";
        /// "Write" verb alias prefix.
        public const string Write = "wr";
        /// Gets verb prefix.
        public static string GetVerbAliasPrefix(string verb)
            FieldInfo aliasField = typeof(VerbAliasPrefixes).GetField(verb);
            if (aliasField != null)
                return (string)aliasField.GetValue(null);
    /// Provides information about a verb used to name commands defined in PowerShell.
    public class VerbInfo
        /// The verb name, used to begin command names.
        /// The alias prefix, recommended for aliases to commands that begin with this verb.
        public string AliasPrefix
        /// The name of the functional category of commands that begin with this verb.
        public string Group
        /// Explains what the verb is meant to do with its object.
    internal static class Verbs
        static Verbs()
            foreach (Type type in VerbTypes)
                foreach (FieldInfo field in type.GetFields())
                    if (field.IsLiteral)
                        s_validVerbs.Add((string)field.GetValue(null), true);
            s_recommendedAlternateVerbs.Add("accept", new string[] { "Receive" });
            s_recommendedAlternateVerbs.Add("acquire", new string[] { "Get", "Read" });
            s_recommendedAlternateVerbs.Add("allocate", new string[] { "New" });
            s_recommendedAlternateVerbs.Add("allow", new string[] { "Enable", "Grant", "Unblock" });
            s_recommendedAlternateVerbs.Add("amend", new string[] { "Edit" });
            s_recommendedAlternateVerbs.Add("analyze", new string[] { "Measure", "Test" });
            s_recommendedAlternateVerbs.Add("append", new string[] { "Add" });
            s_recommendedAlternateVerbs.Add("assign", new string[] { "Set" });
            s_recommendedAlternateVerbs.Add("associate", new string[] { "Join", "Merge" });
            s_recommendedAlternateVerbs.Add("attach", new string[] { "Add", "Debug" });
            s_recommendedAlternateVerbs.Add("bc", new string[] { "Compare" });
            s_recommendedAlternateVerbs.Add("boot", new string[] { "Start" });
            s_recommendedAlternateVerbs.Add("break", new string[] { "Disconnect" });
            s_recommendedAlternateVerbs.Add("broadcast", new string[] { "Send" });
            s_recommendedAlternateVerbs.Add("burn", new string[] { "Backup" });
            s_recommendedAlternateVerbs.Add("calculate", new string[] { "Measure" });
            s_recommendedAlternateVerbs.Add("cancel", new string[] { "Stop" });
            s_recommendedAlternateVerbs.Add("cat", new string[] { "Get" });
            s_recommendedAlternateVerbs.Add("change", new string[] { "Convert", "Edit", "Rename" });
            s_recommendedAlternateVerbs.Add("clean", new string[] { "Uninstall" });
            s_recommendedAlternateVerbs.Add("clone", new string[] { "Copy" });
            s_recommendedAlternateVerbs.Add("combine", new string[] { "Join", "Merge" });
            s_recommendedAlternateVerbs.Add("compact", new string[] { "Compress" });
            s_recommendedAlternateVerbs.Add("compile", new string[] { "Build" });
            s_recommendedAlternateVerbs.Add("concatenate", new string[] { "Add" });
            s_recommendedAlternateVerbs.Add("configure", new string[] { "Set" });
            s_recommendedAlternateVerbs.Add("create", new string[] { "New" });
            s_recommendedAlternateVerbs.Add("cut", new string[] { "Remove" });
            // recommendedAlternateVerbs.Add("debug",      new string[] {"Ping"});
            s_recommendedAlternateVerbs.Add("delete", new string[] { "Remove" });
            s_recommendedAlternateVerbs.Add("detach", new string[] { "Dismount", "Remove" });
            s_recommendedAlternateVerbs.Add("determine", new string[] { "Measure", "Resolve" });
            s_recommendedAlternateVerbs.Add("diagnose", new string[] { "Debug", "Test" });
            s_recommendedAlternateVerbs.Add("diff", new string[] { "Checkpoint", "Compare" });
            s_recommendedAlternateVerbs.Add("difference", new string[] { "Checkpoint", "Compare" });
            s_recommendedAlternateVerbs.Add("dig", new string[] { "Trace" });
            s_recommendedAlternateVerbs.Add("dir", new string[] { "Get" });
            s_recommendedAlternateVerbs.Add("discard", new string[] { "Remove" });
            s_recommendedAlternateVerbs.Add("display", new string[] { "Show", "Write" });
            s_recommendedAlternateVerbs.Add("dispose", new string[] { "Remove" });
            s_recommendedAlternateVerbs.Add("divide", new string[] { "Split" });
            s_recommendedAlternateVerbs.Add("dump", new string[] { "Get" });
            s_recommendedAlternateVerbs.Add("duplicate", new string[] { "Copy" });
            s_recommendedAlternateVerbs.Add("empty", new string[] { "Clear" });
            s_recommendedAlternateVerbs.Add("end", new string[] { "Stop" });
            s_recommendedAlternateVerbs.Add("erase", new string[] { "Clear", "Remove" });
            s_recommendedAlternateVerbs.Add("examine", new string[] { "Get" });
            s_recommendedAlternateVerbs.Add("execute", new string[] { "Invoke" });
            s_recommendedAlternateVerbs.Add("explode", new string[] { "Expand" });
            s_recommendedAlternateVerbs.Add("extract", new string[] { "Export" });
            s_recommendedAlternateVerbs.Add("fix", new string[] { "Repair", "Restore" });
            s_recommendedAlternateVerbs.Add("flush", new string[] { "Clear" });
            s_recommendedAlternateVerbs.Add("follow", new string[] { "Trace" });
            s_recommendedAlternateVerbs.Add("generate", new string[] { "New" });
            // recommendedAlternateVerbs.Add("get",        new string[] {"Read"});
            s_recommendedAlternateVerbs.Add("halt", new string[] { "Disable" });
            s_recommendedAlternateVerbs.Add("in", new string[] { "ConvertTo" });
            s_recommendedAlternateVerbs.Add("index", new string[] { "Update" });
            s_recommendedAlternateVerbs.Add("initiate", new string[] { "Start" });
            s_recommendedAlternateVerbs.Add("input", new string[] { "ConvertTo", "Unregister" });
            s_recommendedAlternateVerbs.Add("insert", new string[] { "Add", "Unregister" });
            s_recommendedAlternateVerbs.Add("inspect", new string[] { "Trace" });
            s_recommendedAlternateVerbs.Add("kill", new string[] { "Stop" });
            s_recommendedAlternateVerbs.Add("launch", new string[] { "Start" });
            s_recommendedAlternateVerbs.Add("load", new string[] { "Import" });
            s_recommendedAlternateVerbs.Add("locate", new string[] { "Search", "Select" });
            s_recommendedAlternateVerbs.Add("logoff", new string[] { "Disconnect" });
            s_recommendedAlternateVerbs.Add("mail", new string[] { "Send" });
            s_recommendedAlternateVerbs.Add("make", new string[] { "New" });
            s_recommendedAlternateVerbs.Add("match", new string[] { "Select" });
            s_recommendedAlternateVerbs.Add("migrate", new string[] { "Move" });
            s_recommendedAlternateVerbs.Add("modify", new string[] { "Edit" });
            s_recommendedAlternateVerbs.Add("name", new string[] { "Move" });
            s_recommendedAlternateVerbs.Add("nullify", new string[] { "Clear" });
            s_recommendedAlternateVerbs.Add("obtain", new string[] { "Get" });
            // recommendedAlternateVerbs.Add("out",        new string[] {"ConvertFrom"});
            s_recommendedAlternateVerbs.Add("output", new string[] { "ConvertFrom" });
            s_recommendedAlternateVerbs.Add("pause", new string[] { "Suspend", "Wait" });
            s_recommendedAlternateVerbs.Add("peek", new string[] { "Receive" });
            s_recommendedAlternateVerbs.Add("permit", new string[] { "Enable" });
            s_recommendedAlternateVerbs.Add("purge", new string[] { "Clear", "Remove" });
            s_recommendedAlternateVerbs.Add("pick", new string[] { "Select" });
            // recommendedAlternateVerbs.Add("pop",        new string[] {"Enter", "Exit"});
            s_recommendedAlternateVerbs.Add("prevent", new string[] { "Block" });
            s_recommendedAlternateVerbs.Add("print", new string[] { "Write" });
            s_recommendedAlternateVerbs.Add("prompt", new string[] { "Read" });
            // recommendedAlternateVerbs.Add("push",       new string[] {"Enter", "Exit"});
            s_recommendedAlternateVerbs.Add("put", new string[] { "Send", "Write" });
            s_recommendedAlternateVerbs.Add("puts", new string[] { "Write" });
            s_recommendedAlternateVerbs.Add("quota", new string[] { "Limit" });
            s_recommendedAlternateVerbs.Add("quote", new string[] { "Limit" });
            s_recommendedAlternateVerbs.Add("rebuild", new string[] { "Initialize" });
            s_recommendedAlternateVerbs.Add("recycle", new string[] { "Restart" });
            s_recommendedAlternateVerbs.Add("refresh", new string[] { "Update" });
            s_recommendedAlternateVerbs.Add("reinitialize", new string[] { "Initialize" });
            s_recommendedAlternateVerbs.Add("release", new string[] { "Clear", "Install", "Publish", "Unlock" });
            s_recommendedAlternateVerbs.Add("reload", new string[] { "Update" });
            s_recommendedAlternateVerbs.Add("renew", new string[] { "Initialize", "Update" });
            s_recommendedAlternateVerbs.Add("replicate", new string[] { "Copy" });
            s_recommendedAlternateVerbs.Add("resample", new string[] { "Convert" });
            // recommendedAlternateVerbs.Add("reset",      new string[] {"Set"});
            // recommendedAlternateVerbs.Add("resize",     new string[] {"Convert"});
            s_recommendedAlternateVerbs.Add("restrict", new string[] { "Lock" });
            s_recommendedAlternateVerbs.Add("return", new string[] { "Repair", "Restore" });
            s_recommendedAlternateVerbs.Add("revert", new string[] { "Unpublish" });
            s_recommendedAlternateVerbs.Add("revise", new string[] { "Edit" });
            s_recommendedAlternateVerbs.Add("run", new string[] { "Invoke", "Start" });
            s_recommendedAlternateVerbs.Add("salvage", new string[] { "Test" });
            // recommendedAlternateVerbs.Add("save",       new string[] {"Backup"});
            s_recommendedAlternateVerbs.Add("secure", new string[] { "Lock" });
            s_recommendedAlternateVerbs.Add("separate", new string[] { "Split" });
            s_recommendedAlternateVerbs.Add("setup", new string[] { "Initialize", "Install" });
            s_recommendedAlternateVerbs.Add("sleep", new string[] { "Suspend", "Wait" });
            s_recommendedAlternateVerbs.Add("starttransaction", new string[] { "Checkpoint" });
            s_recommendedAlternateVerbs.Add("telnet", new string[] { "Connect" });
            s_recommendedAlternateVerbs.Add("terminate", new string[] { "Stop" });
            s_recommendedAlternateVerbs.Add("track", new string[] { "Trace" });
            s_recommendedAlternateVerbs.Add("transfer", new string[] { "Move" });
            s_recommendedAlternateVerbs.Add("type", new string[] { "Get" });
            // recommendedAlternateVerbs.Add("undo",       new string[] {"Repair", "Restore"});
            s_recommendedAlternateVerbs.Add("unite", new string[] { "Join", "Merge" });
            s_recommendedAlternateVerbs.Add("unlink", new string[] { "Dismount" });
            s_recommendedAlternateVerbs.Add("unmark", new string[] { "Clear" });
            s_recommendedAlternateVerbs.Add("unrestrict", new string[] { "Unlock" });
            s_recommendedAlternateVerbs.Add("unsecure", new string[] { "Unlock" });
            s_recommendedAlternateVerbs.Add("unset", new string[] { "Clear" });
            s_recommendedAlternateVerbs.Add("verify", new string[] { "Test" });
            foreach (KeyValuePair<string, string[]> entry in s_recommendedAlternateVerbs)
                Dbg.Assert(!IsStandard(entry.Key), "prohibited verb is standard");
                foreach (string suggested in entry.Value)
                    Dbg.Assert(IsStandard(suggested), "suggested verb is not standard");
        /// Gets all verb types.
        /// <value>List of all verb types.</value>
        private static Type[] VerbTypes => new Type[] {
            typeof(VerbsCommon),
            typeof(VerbsCommunications),
            typeof(VerbsData),
            typeof(VerbsDiagnostic),
            typeof(VerbsLifecycle),
            typeof(VerbsOther),
            typeof(VerbsSecurity)
        /// Gets verb group display name from type.
        /// <param name="verbType">The verb type.</param>
        /// <returns>Verb group display name.</returns>
        private static string GetVerbGroupDisplayName(Type verbType) => verbType.Name.Substring(5);
        /// Filters by verbs and groups.
        /// <param name="verbs">The array of verbs.</param>
        /// <param name="groups">The array of groups.</param>
        /// <returns>List of Verbs.</returns>
        internal static IEnumerable<VerbInfo> FilterByVerbsAndGroups(string[] verbs, string[] groups)
            if (groups is null || groups.Length == 0)
                foreach (Type verbType in VerbTypes)
                    foreach (VerbInfo verb in FilterVerbsByType(verbs, verbType))
                        yield return verb;
                if (GroupsContainVerbType(groups, verbType))
        /// Checks if verb type exists in list of groups.
        /// <param name="groups">The list of groups</param>
        /// <param name="verbType">The verb type to check.</param>
        /// <returns>True if verb type was found, False if not found.</returns>
        private static bool GroupsContainVerbType(string[] groups, Type verbType)
            => SessionStateUtilities.CollectionContainsValue(
                groups,
                GetVerbGroupDisplayName(verbType),
        /// Enumerates field names from a Verb Type.
        /// <returns>List of field names.</returns>
        private static IEnumerable<string> EnumerateFieldNamesFromVerbType(Type verbType)
            foreach (FieldInfo field in verbType.GetFields())
                    yield return field.Name;
        /// Enumerates field names from all Verb Types.
        private static IEnumerable<string> EnumerateFieldNamesFromAllVerbTypes()
                foreach (string fieldName in EnumerateFieldNamesFromVerbType(verbType))
                    yield return fieldName;
        /// Enumerates command verb names.
        /// <param name="commands">The collection of commands.</param>
        /// <returns>List of command verb names.</returns>
        private static IEnumerable<string> EnumerateCommandVerbNames(Collection<CmdletInfo> commands)
            foreach (CmdletInfo command in commands)
                yield return command.Verb;
        /// Filters verbs by type.
        private static IEnumerable<VerbInfo> FilterVerbsByType(string[] verbs, Type verbType)
            if (verbs is null || verbs.Length == 0)
                    yield return CreateVerbFromField(fieldName, verbType);
            Collection<WildcardPattern> verbPatterns = SessionStateUtilities.CreateWildcardsFromStrings(
                verbs,
                        verbPatterns,
        /// Creates Verb info object from field info.
        /// <param name="fieldName">The field name.</param>
        /// <returns>VerbInfo object.</returns>
        private static VerbInfo CreateVerbFromField(string fieldName, Type verbType) => new()
            Verb = fieldName,
            AliasPrefix = VerbAliasPrefixes.GetVerbAliasPrefix(fieldName),
            Group = GetVerbGroupDisplayName(verbType),
            Description = VerbDescriptions.GetVerbDescription(fieldName)
                // Completion: Get-Verb -Group <group> -Verb <wordToComplete>
                if (commandName.Equals("Get-Verb", StringComparison.OrdinalIgnoreCase)
                    && fakeBoundParameters.Contains("Group"))
                    string[] groups = null;
                    object groupParameterValue = fakeBoundParameters["Group"];
                    Type groupParameterValueType = groupParameterValue.GetType();
                    if (groupParameterValueType == typeof(string))
                        groups = new string[] { groupParameterValue.ToString() };
                    else if (groupParameterValueType.IsArray
                             && groupParameterValueType.GetElementType() == typeof(object))
                        groups = Array.ConvertAll((object[])groupParameterValue, group => group.ToString());
                    return CompleteVerbWithGroups(wordToComplete, groups);
                // Completion: Get-Command -Noun <noun> -Verb <wordToComplete>
                else if (commandName.Equals("Get-Command", StringComparison.OrdinalIgnoreCase)
                         && fakeBoundParameters.Contains("Noun"))
                    ps.AddParameter("Noun", fakeBoundParameters["Noun"]);
                    if (fakeBoundParameters.Contains("Module"))
                        ps.AddParameter("Module", fakeBoundParameters["Module"]);
                    Collection<CmdletInfo> commands = ps.Invoke<CmdletInfo>();
                    return CompleteVerbWithCommands(wordToComplete, commands);
                // Complete all verbs by default if above cases not completed
                return CompleteVerbForAllTypes(wordToComplete);
            /// Completes verb with list of groups.
            /// <param name="groups">The list of groups.</param>
            /// <returns>List of completions for verb.</returns>
            private static IEnumerable<CompletionResult> CompleteVerbWithGroups(string wordToComplete, string[] groups)
                        foreach (CompletionResult result in CompletionHelpers.GetMatchingResults(
                            possibleCompletionValues: EnumerateFieldNamesFromVerbType(verbType)))
            /// Completes verb with list of commands.
            private static IEnumerable<CompletionResult> CompleteVerbWithCommands(string wordToComplete, Collection<CmdletInfo> commands)
                    possibleCompletionValues: EnumerateCommandVerbNames(commands));
            /// Completes verb for all types.
            private static IEnumerable<CompletionResult> CompleteVerbForAllTypes(string wordToComplete)
                    possibleCompletionValues: EnumerateFieldNamesFromAllVerbTypes());
        private static readonly Dictionary<string, bool> s_validVerbs = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string[]> s_recommendedAlternateVerbs = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        internal static bool IsStandard(string verb)
            return s_validVerbs.ContainsKey(verb);
        internal static string[] SuggestedAlternates(string verb)
            s_recommendedAlternateVerbs.TryGetValue(verb, out result);
    #endregion VERBS
