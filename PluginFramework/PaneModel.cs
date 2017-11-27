using System;
using System.Runtime.Serialization;

namespace PluginFramework
{
    [DataContract(IsReference = true)]
    public class PaneModel : IPanelModel
    {
        public PaneModel()
        {
            ContentID = Guid.NewGuid();
        }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public Guid ContentID { get; set; }

        [DataMember]
        public string FactoryReference
        {
            get;
            set;
        }
    }
}
