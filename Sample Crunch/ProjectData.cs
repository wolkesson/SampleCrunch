using PluginFramework;
using Sample_Crunch.Model;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Sample_Crunch
{
    [DataContract]
    public class ProjectData
    {
        static ProjectData()
        {
            // Temporary add extra type serialization dependencies here
            PluginFactory.RegisterModelType(typeof(ProjectPaneModel));
            PluginFactory.RegisterModelType(typeof(MarkerPaneModel));
            PluginFactory.RegisterModelType(typeof(Model.MarkerModel));
            PluginFactory.RegisterModelType(typeof(Model.FileModel));
        }

        public ProjectData()
        {
            this.Name = "Unnamed";
            this.Version = new Version(0, 9);

            this.Anchorables = new ObservableCollection<IPanelModel>();
            this.Documents = new ObservableCollection<IPanelModel>();
            this.Files = new ObservableCollection<IFileModel>();
            this.Markers = new ObservableCollection<IMarkerModel>(); // TODO: Should be a model not a viewmodel
        }

        [DataMember(IsRequired = true)]
        public string Name { get; set; }

        [DataMember]
        public Version Version { get; set; }

        [DataMember]
        public ObservableCollection<IPanelModel> Documents { get; private set; }

        [DataMember]
        public ObservableCollection<IPanelModel> Anchorables { get; private set; }

        [DataMember]
        public ObservableCollection<IFileModel> Files { get; private set; }

        [DataMember]
        public ObservableCollection<IMarkerModel> Markers { get; private set; }

        [DataMember]
        public String Layout { get; set; }

        internal void Save(string filename)
        {
            const string extension = ".scp";
            if (string.IsNullOrEmpty(filename)) return;
            if (!filename.EndsWith(extension)) filename = filename + extension;

            var settings = new XmlWriterSettings();
            settings.Indent = true;
            using (XmlWriter xw = XmlWriter.Create(filename, settings))
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(ProjectData), PluginFactory.GetModelTypes());
                ser.WriteObject(xw, this);
            }
        }
        
        static internal ProjectData Open(string filename)
        {
            using (FileStream reader = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(ProjectData), PluginFactory.GetModelTypes());
                ProjectData p = (ProjectData)ser.ReadObject(reader);
                return p;
            }
        }
    }
}
