namespace Sample_Crunch
{
    using PluginFramework;
    using System.Runtime.Serialization;

    [DataContract(IsReference = true)]
    public class ProjectPaneModel : PaneModel
    {
        public ProjectPaneModel()
        {
            this.Title = "Project";
        }
    }

    [DataContract(IsReference = true)]
    public class MarkerPaneModel : PaneModel
    {
        public MarkerPaneModel()
        {
            this.Title = "Markers";
        }
    }
}