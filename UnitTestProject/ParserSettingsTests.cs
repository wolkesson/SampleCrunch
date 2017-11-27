using Microsoft.VisualStudio.TestTools.UnitTesting;
using PluginFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PluginFramework.Tests
{
    [TestClass()]
    public class ParserSettingsTests
    {
        [TestMethod()]
        public void TestString()
        {
            ParserSettings settings = new ParserSettings();
            string expected = "value";
            settings.Write("item1", expected);
            string actual = settings.Read("item1").ToString();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void TestInt()
        {
            ParserSettings settings = new ParserSettings();
            int expected = 56;
            settings.Write("item1", expected);
            int actual = (int) settings.Read("item1");
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod()]
        public void TestUpdate()
        {
            ParserSettings settings = new ParserSettings();
            int expected = 56;
            settings.Write("item1", 32);
            settings.Write("item1", expected);
            int actual = (int)settings.Read("item1");
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod()]
        [ExpectedException(typeof(KeyNotFoundException), "Reading an non-existing itemdid not throw an exception")]
        public void TestNoneExisting()
        {
            ParserSettings settings = new ParserSettings();
            int expected = 56;
            int actual = (int)settings.Read("item1");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Test5000()
        {
            ParserSettings settings = new ParserSettings();
            for (int i = 0; i < 5000; i++)
            {
                settings.Write("item" + i, i);
            }

            for (int i = 0; i < 5000; i++)
            {
                int expected = i;
                int actual = (int)settings.Read("item" + i);
                settings.Write("item" + i, i);
                Assert.AreEqual(expected, actual);
            }
        }

        [TestMethod()]
        public void TestSerialize()
        {
            // Create data
            ParserSettings settings = new ParserSettings();
            for (int i = 0; i < 5000; i++)
            {
                settings.Write("item" + i, i);
            }

            // Write data
            MemoryStream stream = new MemoryStream();
            using (XmlWriter xw = XmlWriter.Create(stream))
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(ParserSettings));
                ser.WriteObject(xw, settings);
            }
            
            // Rewind stream
            stream.Position = 0;

            // Read data
            using (XmlReader xr = XmlReader.Create(stream))
            {
                DataContractSerializer ser = new DataContractSerializer(typeof(ParserSettings));
                var settingsOut = (ParserSettings)ser.ReadObject(xr);

                for (int i = 0; i < 5000; i++)
                {
                    Assert.AreEqual(i, (int)settingsOut.Read("item" + i));
                }
            }
        }
    }
}