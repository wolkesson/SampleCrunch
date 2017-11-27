

namespace Sample_Crunch
{
    using Xceed.Wpf.AvalonDock.Layout;
    using PluginFramework;
    using Sample_Crunch.ViewModel;
    using System.Windows;
    using System.Windows.Controls;

    class PanesTemplateSelector : DataTemplateSelector
    {
        public PanesTemplateSelector()
        {

        }

        public DataTemplate MarkersPaneTemplate
        {
            get;
            set;
        }

        public DataTemplate ProjectPaneTemplate
        {
            get;
            set;
        }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            var itemAsLayoutContent = item as LayoutContent;
            IPanelViewModel vm = item as IPanelViewModel;
            IPanelFactory factory = vm?.ParentFactory;
            if (factory != null)
            {
                var template = new DataTemplate();
                FrameworkElementFactory spFactory = new FrameworkElementFactory(                factory.ViewType);

                spFactory.SetValue(Control.DataContextProperty, vm);
                template.VisualTree = spFactory;
                return template;
            }

            if (item is MarkerPaneViewModel)
                return MarkersPaneTemplate;

            if (item is ProjectPaneViewModel)
                return ProjectPaneTemplate;

            return base.SelectTemplate(item, container);
        }
    }
}

