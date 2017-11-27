using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sample_Crunch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample_Crunch.Tests
{
    [TestClass()]
    public class HelpersTests
    {
        [TestMethod()]
        public void ClampTest()
        {
            double actual = 30;

            Assert.AreEqual(30, actual.Clamp(0, 30), "Within limit");

            Assert.AreEqual(20, actual.Clamp(0, 20), "Upper limit");

            actual = 10;
            Assert.AreEqual(20, actual.Clamp(20, 30), "Lower limit");

            actual = -10;
            Assert.AreEqual(-5, actual.Clamp(-5, 5), "Lower negative limit");

            actual = 10;
            Assert.AreEqual(-5, actual.Clamp(-10, -5), "Upper negative limit");

        }

        [TestMethod()]
        public void AddUniqueTest()
        {
            Dictionary<string, string> actual = new Dictionary<string, string>();
            actual.AddUnique("key", "value");
            Assert.IsTrue(actual.Count == 1, "First added");

            actual.AddUnique("key", "value");
            Assert.IsTrue(actual.Count == 2, "Second added");
            Assert.IsTrue(actual.Keys.ToArray()[1] == "key_1", "Renaming Second");

            actual.AddUnique("key", "value");
            Assert.IsTrue(actual.Count == 3, "Third added");
            Assert.IsTrue(actual.Keys.ToArray()[2] == "key_2", "Renaming Third");

        }
    }
}