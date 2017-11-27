
namespace PluginFramework
{
    using System;

    public abstract class PanelFactory<Model, View, ViewModel> : IPanelFactory where Model : IPanelModel, new() where View : IPanelView, new() where ViewModel : IPanelViewModel
    {
        public PanelFactory(string title)
        {
            this.Title = title;
        }

        public Type ModelType
        {
            get
            {
                return typeof(Model);
            }
        }

        public Type ViewType
        {
            get
            {
                return typeof(View);
            }
        }

        public string Title
        {
            get;
            protected set;
        }

        public virtual IPanelModel CreateModel()
        {
            IPanelModel model = new Model();
            model.Title = "New " + this.Title;
            model.FactoryReference = this.GetType().FullName;

            return model;
        }

        public virtual IPanelView CreateView(IPanelViewModel viewModel)
        {
            return new View();
        }

        public abstract IPanelViewModel CreateViewModel(IProjectViewModel project, IPanelModel model);
    }
}
