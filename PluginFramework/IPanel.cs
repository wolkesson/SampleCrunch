namespace PluginFramework
{
    using GalaSoft.MvvmLight;
    using System;

    public interface IPanelFactory
    {
        string Title { get; }
        Type ViewType { get; }
        Type ModelType { get; }

        IPanelModel CreateModel();
        IPanelView CreateView(IPanelViewModel viewModel);
        IPanelViewModel CreateViewModel(IProjectViewModel project, IPanelModel model);
    }

    public interface IPanelView
    {
    }

    public interface IPanelViewModel
    {
        IPanelFactory ParentFactory { get; }
    }

    public class PanelViewModel<T>: ViewModelBase, IPanelViewModel where T:IPanelModel
    {
        protected readonly T model;
        protected readonly IPanelFactory factory;
        public PanelViewModel(IPanelFactory factory, T model)
        {
            this.factory = factory;
            this.model = model;
        }

        public string Title
        {
            get
            {
                return model.Title;
            }
            set
            {
                model.Title = value;
                RaisePropertyChanged<string>(nameof(Title));
            }
        }

        public Guid ContentId
        {
            get
            {
                return model.ContentID;
            }
        }

        public IPanelFactory ParentFactory
        {
            get
            {
                return factory;
            }
        }
    }

    public interface IPanelModel
    {
        string FactoryReference { get; set; }
        Guid ContentID { get; set; }
        string Title { get; set; }
    }
}
