using System;
using System.Collections.Generic;
using CommonServiceLocator;

namespace UnitTestProject.Mock
{
    class MockServiceProvider : IServiceLocator
    {
        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            throw new NotImplementedException();
        }

        public object GetInstance(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public object GetInstance(Type serviceType, string key)
        {
            throw new NotImplementedException();
        }

        public TService GetInstance<TService>()
        {
            string name = typeof(TService).Name;
            // Match on name to not require types to be imported!

            if (name == "MainViewModel")
                {
                //return (TService)new MockMainViewModel();
            }
            else if (name == "TelemetryClient")
                    {
                //return new TelemetryClient();
            }
            return default(TService);
        }

        public TService GetInstance<TService>(string key)
        {
            throw new NotImplementedException();
        }

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
}
