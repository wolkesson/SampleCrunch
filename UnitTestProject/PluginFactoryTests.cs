using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginFramework;
using System.IO;
using System.Collections.Generic;

namespace UnitTestProject
{
    [TestClass]
    public class PluginFactoryTests
    {
        static string pluginPath;

        [ClassInitialize]
        static void InstallPlugIn()
        {
            pluginPath = AppDomain.CurrentDomain.BaseDirectory;
        }

        //[TestMethod]
        //public void LoadPluginsForDlls()
        //{
        //    if (pluginPath == null) { InstallPlugIn(); }
        //    PluginFactory.LoadPlugins(pluginPath);
        //    Assert.IsTrue( PluginFactory.Parsers.Count >= 1,  "Expected one plugin at path " + pluginPath);
        //    Assert.IsTrue(typeof(ILogFileParser).IsAssignableFrom(PluginFactory.Parsers[0]));
        //}

        //[TestMethod]
        //public void PluginName()
        //{
        //    PluginFactory.LoadPlugins(pluginPath);
        //    ParserPluginAttribute attr = PluginFactory.GetPluginAttribute("SamplePlugins.TestPlugin");
        //    Assert.AreEqual<string>("Test Plugin", attr.Title);
        //}


        //[TestMethod]
        //public void PluginSignals()
        //{
        //    List<string> expectedSignalNames = new List<string>();
        //    expectedSignalNames.Add("Signal 1");
        //    expectedSignalNames.Add("Signal 2");
        //    expectedSignalNames.Add("Strange letters: ~Åäö /");

        //    PluginFactory.LoadPlugins(pluginPath);
        //    //Assert.AreEqual<int>(1, PluginFactory.Plugins.Count,  "Expected on plugin at path " + pluginPath);
        //    Type t = PluginFactory.FindParser("SamplePlugins.TestPlugin");
        //    ILogFileParser parser = PluginFactory.CreateLogFileParser(t);
        //    ILogFile lf = parser.Open("Dummy");
        //    var signals = lf.Signals;
        //    List<string> actualSignalNames = new List<string>();
        //    signals.ForEach((s) => actualSignalNames.Add(s.Name));
        //    CollectionAssert.AreEqual(expectedSignalNames, actualSignalNames, "Signal names are not as expected");
        //}
    }
}
