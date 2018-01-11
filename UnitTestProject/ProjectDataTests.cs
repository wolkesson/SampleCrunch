using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Sample_Crunch;
using Sample_Crunch.ViewModel;
using PluginFramework;
using Sample_Crunch.Model;
using Sample_Crunch.StandardPanels;
using GalaSoft.MvvmLight.Ioc;

namespace UnitTestProject
{
    [TestClass]
    public class ProjectTest
    {
        const string filename = "testProject.scp";
        static string pluginPath;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            pluginPath = System.IO.Path.Combine(Environment.CurrentDirectory);
            // Init plugin reader
            PluginFactory.LoadPlugins(pluginPath);
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }

        [TestMethod]
        public void SaveAndOpen()
        {
            ProjectData testProj = SaveProjectFile(filename);
            Assert.IsTrue(File.Exists(filename), "No project file written");

            ProjectData openedProj = ProjectData.Open(filename);
            Assert.AreEqual<int>(testProj.Files.Count, openedProj.Files.Count, "Invalid number of files");
        }

        [TestMethod]
        public void SaveAndOpenAddExtension()
        {
            string filenameNoExt = Path.GetFileNameWithoutExtension(filename);
            ProjectData testProj = SaveProjectFile(filenameNoExt);
            Assert.IsTrue(File.Exists(filename), "No project file written"); // Extension should be added by save

            ProjectData openedProj = ProjectData.Open(filename);
            Assert.AreEqual<int>(testProj.Files.Count, openedProj.Files.Count, "Invalid number of files");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "FileNotFound exception did not throw")]
        public void SaveAndOpenFailures()
        {
            PluginFactory.Reset();
            PluginFactory.LoadPlugins(pluginPath);
            Assert.IsFalse(File.Exists(filename), "Project file should not exist");
            ProjectData testProj = new ProjectData();
            testProj.Files.Add(new FileModel());
            testProj.Save(filename);

            Assert.IsTrue(File.Exists(filename), "No project file written");
            ProjectData openedProj = ProjectData.Open(filename); // Should fail since there is no file in filetopic

            ProjectViewModel projVM = new ProjectViewModel(openedProj);
        }

        private static ProjectData SaveProjectFile(string filename)
        {
            Assert.IsFalse(File.Exists(filename), "Project file should not exist");
            ProjectData testProj = new ProjectData();
            testProj.Files.Add(new FileModel() { FileName = "test.dat" });
            testProj.Save(filename);
            return testProj;
        }

        [TestMethod]
        public void SaveAndOpenCSV()
        {
            SimpleIoc.Default.Register<Mock.MockMainViewModel>();
            //SimpleIoc.Default.Register<TelemetryClient>(()=> new TelemetryClient());
            ProjectData testProj = new ProjectData();
            ProjectViewModel projVM = new ProjectViewModel(testProj);
            IParserFactory lfp = PluginFactory.CreateParserFactory(typeof(CsvParserFactory));
            
            testProj.Files.Add(MockAddFile(@"TestData/data.csv", lfp));
            testProj.Save(filename);
            Assert.IsTrue(File.Exists(filename), "No project file written"); // Extension should be added by save

            ProjectData openedProj = ProjectData.Open(filename);
            Assert.AreEqual<int>(testProj.Files.Count, openedProj.Files.Count, "Invalid number of files");
        }

        private FileModel MockAddFile(string filename, IParserFactory parser)
        {
            FileModel file = new FileModel();
            file.FileName = filename;
            file.Title = System.IO.Path.GetFileNameWithoutExtension(file.FileName);
            file.DataParserType = parser.GetType().FullName;
            
            // Show settings dialog and exit if canceled
            file.Settings.Write("Delimiter", ';');
            file.Settings.Write("TimeVector", 0);
            
            return file;
        }

        [TestCleanup]
        public void TestClenup()
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }
    }
}
