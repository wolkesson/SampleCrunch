using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginFramework;
using System.IO;
using System.Collections.Generic;

namespace PluginFramework.Tests
{
    [TestClass()]
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
            File.Copy(validPlugin, Path.Combine(pluginPath, validPlugin),true);
        }

        [TestMethod()]
        public void LoadPluginsTest()
        {
            PluginFactory.LoadPlugins(pluginPath);

            Assert.IsTrue(PluginFactory.Info.Count >= 1, "Expected one info at path " + pluginPath);
            Assert.IsTrue(PluginFactory.ParserFactories.Count >= 1, "Expected one parser at path " + pluginPath);
            Assert.IsTrue(PluginFactory.PanelFactories.Count >= 1, "Expected one panel at path " + pluginPath);
        }

        [TestMethod()]
        public void ResetTest()
        {
            LoadPluginsTest();

            var expectedInfo = PluginFactory.Info;
            var expectedParsers = PluginFactory.ParserFactories;
            var expectedPanels = PluginFactory.PanelFactories;

            PluginFactory.Reset();

            Assert.IsTrue(PluginFactory.Info.Count == 0, "Expected info to be empty after reset");
            Assert.IsTrue(PluginFactory.ParserFactories.Count == 0, "Expected Parsers to be empty after reset");
            Assert.IsTrue(PluginFactory.PanelFactories.Count == 0, "Expected Panels to be empty after reset");

            LoadPluginsTest();

            CollectionAssert.AreEqual(expectedInfo, PluginFactory.Info);
            CollectionAssert.AreEqual(expectedParsers, PluginFactory.ParserFactories);
            CollectionAssert.AreEqual(expectedPanels, PluginFactory.PanelFactories);
        }

        [TestMethod()]
        public void GetModelTypesTest()
        {
            PluginFactory.LoadPlugins(pluginPath);
            var actual = PluginFactory.GetModelTypes();
            Assert.IsTrue(actual.Length >= 1);
        }

        [TestMethod()]
        public void RegisterModelTypeTest()
        {
            PluginFactory.LoadPlugins(pluginPath);

            var beforeLength = PluginFactory.GetModelTypes().Length;
            PluginFactory.RegisterModelType(typeof(PluginFactoryTests));

            var actual = PluginFactory.GetModelTypes();
            CollectionAssert.Contains(actual, typeof(PluginFactoryTests));
            Assert.IsTrue(actual.Length == beforeLength + 1);
        }

        [TestMethod()]
        public void CreateParserFactoryTest()
        {
            Type expect = typeof(Sample_Crunch.StandardPanels.CsvParserFactory);
            IParserFactory lfp = PluginFactory.CreateParserFactory(expect);
            Assert.IsInstanceOfType(lfp, expect);
        }

        [TestMethod()]
        public void CreateAnalyzerTest()
        {
            //Type expect = typeof(Sample_Crunch.StandardPanels.csvParser);
            //IParserFactory lfp = PluginFactory.CreateParserFactory(expect);
            //Assert.IsInstanceOfType(lfp, expect);
        }

        [TestMethod()]
        public void CreatePanelFactoryTest()
        {
            Type expect = typeof(Sample_Crunch.StandardPanels.TimePlotFactory);
            IPanelFactory lfp = PluginFactory.CreatePanelFactory(expect);
            Assert.IsInstanceOfType(lfp, expect);
        }

        [TestMethod()]
        public void FindParserTest()
        {
            PluginFactory.Reset();
            PluginFactory.LoadPlugins(pluginPath);
            Type factoryType = typeof(Sample_Crunch.StandardPanels.CsvParserFactory);
            IParserFactory lfp = PluginFactory.FindParser(factoryType.FullName);
            IsTypenameSame(lfp.GetType(), factoryType);
        }

        public static void IsTypenameSame(Type actual, Type expected)
        {
            Assert.AreEqual(actual.FullName, expected.FullName);
        }

        [TestMethod()]
        public void FindLogFileParserTest()
        {
            PluginFactory.Reset();
            PluginFactory.LoadPlugins(pluginPath);

            IParserFactory lfp = PluginFactory.FindLogFileParser("dummy.csv");
            IsTypenameSame(lfp.GetType(), typeof(Sample_Crunch.StandardPanels.CsvParserFactory));
        }

        [TestMethod()]
        public void FindAnalyzersTest()
        {

        }

        [TestMethod()]
        public void FindPanelFactoryTest()
        {
            PluginFactory.Reset();
            PluginFactory.LoadPlugins(pluginPath);
            Type factoryType = typeof(Sample_Crunch.StandardPanels.TimePlotFactory);
            var factory = PluginFactory.CreatePanelFactory(factoryType);
            var model = factory.CreateModel();
            IPanelFactory lfp = PluginFactory.FindPanelFactory(model);
            IsTypenameSame(lfp.GetType(), factoryType);
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
            Assert.IsTrue(PluginFactory.ParserFactories.Count >= 1, "Expected one plugin at path " + pluginPath);
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
