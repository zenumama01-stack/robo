        internal void AddAppliesToTypeGroup (string typeGroupName)
            TypeGroupReference tgr = new TypeGroupReference ();
            tgr.name = typeGroupName;
            this.referenceList.Add (tgr);
        internal void AddAppliesToType(string typeName)
            TypeReference tr = new TypeReference();
            tr.name = typeName;
            this.referenceList.Add(tr);
