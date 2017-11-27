using System;
using PluginFramework;

namespace Sample_Crunch.ViewModel
{
    public class MarkerPaneViewModel : PanelViewModel<MarkerPaneModel>
    {
        public MarkerPaneViewModel(MarkerPaneModel model)
            : base(null, model)
        {

        }
    }

    public class ProjectPaneViewModel : PanelViewModel<ProjectPaneModel>
    {
        public ProjectPaneViewModel(ProjectPaneModel model)
            : base(null, model)
        {

        }
    }

}
