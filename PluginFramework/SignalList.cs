namespace PluginFramework
{
    using System.Collections.Generic;

    public class SignalList:List<Signal>
    {
        public Signal GetSignalByName(string name)
        {
           return this.Find(p => p.Name == name);
        }

        public Signal GetSignalByUID(int uid)
        {
            return this.Find(p => p.UID == uid);
        }
    }
}
