using PluginFramework;
using System;
using System.Windows.Controls;

namespace SamplePlugins
{
    /// <summary>
    /// Interaction logic for TestPanel.xaml
    /// </summary>
    public partial class TestPanel : UserControl, IPanelView
    {
        public TestPanel()
        {
            InitializeComponent();
        }
    }

    [PanelPlugin(Visible =false)]
    public class TestPanelFactory : PanelFactory<TestPanelModel, TestPanel,TestPanelViewModel>
    {
        public TestPanelFactory():base("Test Plugin")
        {

        }

        public override IPanelViewModel CreateViewModel(IProjectViewModel project, IPanelModel model)
        {
            return new TestPanelViewModel(this, (TestPanelModel)model);
        }
    }

    public class TestPanelViewModel : PanelViewModel<TestPanelModel>, IPanelViewModel
    {
        public TestPanelViewModel(TestPanelFactory factory, TestPanelModel model) : base(factory,model)
        {
        }
    }

    public class TestPanelModel : IPanelModel
    {
        public Guid ContentID
        {
            get;
            set;
        }

        public string FactoryReference
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }
    }

}
