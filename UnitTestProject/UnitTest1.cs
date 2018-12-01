using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject
{
    [TestClass]
    public class TelemetryTests
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
        public void SendUsageReport()
        {
            NameValueCollection values = new NameValueCollection();
            values.Add("uid", "test");
            values.Add("lastRunTime", "3");
            values.Add("version", "1.0.45");
            values.Add("nbPlugins", "0");

            WebClient wc = new WebClient();
            byte[] response = wc.UploadValues("https://us-central1-sample-crunch.cloudfunctions.net/reportUsage", values);
            string resp = Encoding.UTF8.GetString(response);
            Console.WriteLine(resp);
            Assert.AreEqual("Reported", resp);
        }

        [TestMethod]
        public void SendFirstUsageReport()
        {
            NameValueCollection values = new NameValueCollection();
            //values.Add("uid", "test");
            //values.Add("runCounter", "2");
            //values.Add("lastRunTime", "3");
            values.Add("version", "1.0.45");
            values.Add("nbPlugins", "0");

            WebClient wc = new WebClient();
            byte[] response = wc.UploadValues("https://us-central1-sample-crunch.cloudfunctions.net/reportUsage", values);
            string resp = Encoding.UTF8.GetString(response);
            Console.WriteLine(resp);
            Assert.AreEqual("I've got uid 0runCounter 2lastRunTime 3version 1.0.45nbPlugins 0", resp);
        }
    }
}
