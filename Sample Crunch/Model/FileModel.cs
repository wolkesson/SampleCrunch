using PluginFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;

namespace Sample_Crunch.Model
{
    [DataContract(IsReference = true)]
    public class FileModel : IFileModel
    {
        public FileModel()
        {
            SyncOffset = TimeSpan.Zero;
            Markers = new ObservableCollection<IMarkerModel>();
            Settings = new ParserSettings();
        }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string FileName { get; set; }
        
        [DataMember]
        public TimeSpan SyncOffset { get; set; }

        [DataMember]
        public string DataParserType { get; set; }

        [DataMember]
        public IMarkerModel SyncMark { get; set; }
        
        [DataMember]
        public ParserSettings Settings { get; internal set; }

        [DataMember]
        public ObservableCollection<IMarkerModel> Markers { get; private set; }
    }
}
