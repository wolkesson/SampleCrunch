using PluginFramework;
using Sample_Crunch.StandardPanels.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Sample_Crunch.StandardPanels
{
    public class TimePlotFactory : PanelFactory<TimePlotModel, PlotWindow, TimePlotViewModel>
    {
        public TimePlotFactory():base("Time Plot")
        {

        }

        public override IPanelViewModel CreateViewModel(IProjectViewModel project, IPanelModel model)
        {
            return new TimePlotViewModel(project, this, model as TimePlotModel) as IPanelViewModel;
        }
    }
    
    public class XYPlotFactory : PanelFactory<TimePlotModel, PlotWindow, XYPlotPaneViewModel>
    {
        public XYPlotFactory() : base("XY Plot")
        {

        }

        public override IPanelViewModel CreateViewModel(IProjectViewModel project, IPanelModel model)
        {
            return new XYPlotPaneViewModel(project, this, model as TimePlotModel) as IPanelViewModel;
        }
    }

    public class MapPlotFactory : PanelFactory<TimePlotModel, PlotWindow, MapPlotPaneViewModel>
    {
        public MapPlotFactory() : base("Map Plot")
        {

        }

        public override IPanelViewModel CreateViewModel(IProjectViewModel project, IPanelModel model)
        {
            return new MapPlotPaneViewModel(project, this, model as TimePlotModel) as IPanelViewModel;
        }
    }

    [DataContract(IsReference = true)]
    public class PlotSignalModel
    {
        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public string Axis { get; set; }

        public string Resolved { get; set; }
    }

    [DataContract(IsReference = true)]
    public class TimePlotModel : PaneModel
    {
        public TimePlotModel()
        {
            this.Signals = new ObservableCollection<PlotSignalModel>();
        }
        
        [DataMember]
        public ObservableCollection<PlotSignalModel> Signals { get; set; }
    }
}
