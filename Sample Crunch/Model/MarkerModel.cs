namespace Sample_Crunch.Model
{
    using PluginFramework;
    using System;
    using System.Runtime.Serialization;

    [DataContract(IsReference = true)]
    public class MarkerModel : IMarkerModel
    {
        [DataMember]
        public string Title { get; set; }
        
        [DataMember]
        public DateTime Time { get; set; }

        [DataMember]
        public bool Global { get; set; }
    }
}
