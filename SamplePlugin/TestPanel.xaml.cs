using PluginFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SamplePlugins
{
    /// <summary>
    /// Interaction logic for TestPanel.xaml
    /// </summary>
    /// 
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
