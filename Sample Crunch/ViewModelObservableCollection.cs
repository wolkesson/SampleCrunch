using System.Collections.ObjectModel;

namespace Sample_Crunch
{
    /// <summary>
    /// An observable collection mapping another observable collection and syncing changes to either collection. Designed to be used to map a viewmodel collection to an underlying model collection
    /// </summary>
    /// <typeparam name="ViewModelType">The ViewModel type</typeparam>
    /// <typeparam name="ModelType">The Model type</typeparam>
    public class ViewModelObservableCollection<ViewModelType, ModelType>:ObservableCollection<ViewModelType> where ViewModelType:IModelType<ModelType>
    {
        private ObservableCollection<ModelType> modelCollection;
        public ViewModelObservableCollection(ObservableCollection<ModelType> modelCollection)
        {
            this.modelCollection = modelCollection;
        }

        protected override void InsertItem(int index, ViewModelType item)
        {
            modelCollection.Add(item.Model);
            base.InsertItem(index, item);
        }
    }

    public interface IModelType<MT>
    {
        MT Model { get; }
    }
}
