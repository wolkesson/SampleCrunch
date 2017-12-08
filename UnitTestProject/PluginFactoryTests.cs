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
        private const string validPlugin = "StandardPanels.dll";
        private const string invalidPlugin = "InvalidPlugin.dll";
        static string pluginPath;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PluginTest");
            Directory.CreateDirectory(pluginPath);
            // Add valid plugin
            File.Copy(validPlugin, Path.Combine(pluginPath, validPlugin));
        }

        [TestMethod]
        public void LoadPluginsFromDlls()
        {
            PluginFactory.LoadPlugins(pluginPath);
            Assert.IsTrue(PluginFactory.Parsers.Count >= 1, "Expected one plugin at path " + pluginPath);
            Assert.IsTrue(typeof(ILogFileParser).IsAssignableFrom(PluginFactory.Parsers[0]));
        }

        [TestMethod]
        public void InvalidPlugin()
        {
            StreamWriter sw = File.CreateText(Path.Combine(pluginPath, invalidPlugin));
            sw.WriteLine("Not a valid dll!");
            sw.Close();

            try
            {
                List<Exception> execptions = PluginFactory.LoadPlugins(pluginPath);
                Assert.IsTrue(execptions.Count > 0, "Expect at least one error");
                Assert.IsTrue(execptions[0].ToString().Contains(invalidPlugin));
            }
            catch (Exception ex)
            {
                Assert.Fail("Unexpected exception" + ex.ToString());
            }

            // Verify that it continued
            Assert.IsTrue(PluginFactory.Parsers.Count >= 1, "Expected one plugin at path " + pluginPath);
        }

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

        [TestCleanup]
        public void TestCleanup()
        {
            PluginFactory.Reset();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Directory.Delete(pluginPath, true);
        }
    }
}
