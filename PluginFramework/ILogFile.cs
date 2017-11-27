using System;
using System.Collections.Generic;

namespace PluginFramework
{
    public interface ILogFile
    {
        SignalList Signals { get; }
        Sample[] ReadSignal(Signal signal);
        DateTime Origo { get; }
        TimeSpan Length { get; }
        void Close();
    }

    public struct Sample
    {
       public  DateTime Time;
       public  double Value;
    }

    public struct Signal
    {
        public int UID;
        public string Name;
        public string FriendlyName;
        public string Unit;
        public string Description;
        public string Tag;
    }

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
