using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sample_Crunch;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample_Crunch.Tests
{
    [TestClass()]
    public class ViewModelObservableCollectionTests
    {
        class ModelClass
        {
            public string Data;
        }

        class ViewModelClass : IModelType<ModelClass>
        {
            public ViewModelClass(ModelClass model)
            {
                this.Model = model;
            }
            public ModelClass Model { get; private set; }
        }

        [TestMethod()]
        public void AddViewModel()
        {
            ObservableCollection<ModelClass> model = new ObservableCollection<ModelClass>();
            ViewModelObservableCollection<ViewModelClass, ModelClass> viewModel = new ViewModelObservableCollection<ViewModelClass, ModelClass>(model);

            Assert.AreEqual(model.Count, 0);
            Assert.AreEqual(viewModel.Count, 0);

            var newViewItem = new ViewModelClass(new ModelClass() { Data = "Set" } );

            viewModel.Add(newViewItem);


            Assert.AreEqual(model.Count, 1);
            Assert.AreEqual(viewModel.Count, 1);
        }
    }
}