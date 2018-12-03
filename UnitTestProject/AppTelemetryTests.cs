using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sample_Crunch;

namespace UnitTestProject
{
    [TestClass]
    public class AppTelemetryTests
    {

        [TestMethod]
        public void SendEcho()
        {
            NameValueCollection values = new NameValueCollection();
            string expected = "happy lines";
            values.Add("text", expected);
            WebClient wc = new WebClient();
            byte[] response = wc.UploadValues("https://us-central1-sample-crunch.cloudfunctions.net/echo", values);
            string actual = Encoding.UTF8.GetString(response);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public async System.Threading.Tasks.Task RegisterUserAsync()
        {
            AppTelemetry.OverrideUID = "test";
            CultureInfo ci = CultureInfo.InstalledUICulture;
            string resp = await AppTelemetry.RegisterUser(ci.EnglishName, "1.0.45");
            Assert.AreEqual("test", resp);
        }

        [TestMethod]
        public void SendUsageReport()
        {
            AppTelemetry.OverrideUID = "test";
            AppTelemetry.ReportUsage(3, 0, 1, 2);
            // Use REST API to test if write was succesful
        }

        [TestMethod]
        public void SendFirstUsageReport()
        {
            AppTelemetry.ReportUsage(3, 0, 1, 2);
            // Use REST API to test if write was succesful
        }

        [TestMethod]
        public void SendEventReport()
        {
            AppTelemetry.OverrideUID = "test";
            NameValueCollection v = new NameValueCollection();
            v.Add("data", "SomeData");
            v.Add("value", "3");
            AppTelemetry.ReportEvent("test", v);
            // Use REST API to test if write was succesful
        }

        [TestMethod]
        public void SendErrorReport()
        {
            AppTelemetry.OverrideUID = "test";
            AppTelemetry.ReportError("FakeError", new Exception("This is an alternative exeption"));
            // Use REST API to test if write was succesful
        }
    }
}
