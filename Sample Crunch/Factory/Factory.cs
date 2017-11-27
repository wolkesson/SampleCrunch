using PluginFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample_Crunch.Factory
{
    [PanelPlugin(Visible = false)]
    public class MarkerPanelFactory : PanelFactory<MarkerPaneModel, MarkersPane, MarkersPanelViewModel>
    {
        public MarkerPanelFactory() : base("Marker Plugin")
        {

        }

        public override IPanelModel CreateModel()
        {
            // Override base class to not add New to the name
            var m = base.CreateModel();
            m.Title = "Markers";
            return m;
        }

        public override IPanelViewModel CreateViewModel(IProjectViewModel project, IPanelModel model)
        {
            return new MarkersPanelViewModel(this, (MarkerPaneModel)model);
        }
    }

    public class MarkersPanelViewModel: PanelViewModel<MarkerPaneModel>, IPanelViewModel
    {
        public MarkersPanelViewModel(MarkerPanelFactory factory, MarkerPaneModel model) : base(factory,model)
        {
        }
    }

    [PanelPlugin(Visible = false)]
    public class ProjectPanelFactory : PanelFactory<ProjectPaneModel, ProjectPane, ProjectPanelViewModel>
    {
        public ProjectPanelFactory() : base("Project")
        {

        }

        public override IPanelModel CreateModel()
        {
            // Override base class to not add New to the name
            var m = base.CreateModel();
            m.Title = "Project";
            return m;
        }

        public override IPanelViewModel CreateViewModel(IProjectViewModel project, IPanelModel model)
        {
            return new ProjectPanelViewModel(this, (ProjectPaneModel)model);
        }
    }

    public class ProjectPanelViewModel : PanelViewModel<ProjectPaneModel>, IPanelViewModel
    {
        public ProjectPanelViewModel(ProjectPanelFactory factory, ProjectPaneModel model) : base(factory, model)
        {
        }
    }
}
