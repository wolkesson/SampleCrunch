using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sample_Crunch
{
    static class AppTelemetry
    {
        static public bool DoNotSend { set; get; }
        static public string OverrideUID { set; private get; }
        const string baseUrl = "https://us-central1-sample-crunch.cloudfunctions.net/";
        
        private static string GetUID()
        {
            if (OverrideUID != null) return OverrideUID;

            // Register user if no UID available
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.AppTelemetryUID)) {
                throw new InvalidOperationException("No user registered");
            }

            return Properties.Settings.Default.AppTelemetryUID;
        }

        internal static async Task<string> RegisterUser(string locale, string version)
        {
            if (DoNotSend) return string.Empty;
            NameValueCollection values = new NameValueCollection();
            if (OverrideUID != null) { values.Add("uid", OverrideUID); }
            values.Add("locale", locale);
            values.Add("version", version);

            WebClient wc = new WebClient();
            byte[] resp = await wc.UploadValuesTaskAsync(baseUrl + "registerUser", values);
            string uid = Encoding.UTF8.GetString(resp);

            // Save UID for future calls
            if (!string.IsNullOrWhiteSpace(uid))
            {
                Properties.Settings.Default.AppTelemetryUID = uid;
                Properties.Settings.Default.Save();
            }
            return uid;
        }

        internal static void ReportUsage(int lastRunTime, int numberOfParsers, int numberOfPanels, int numberOfAnalyzers)
        {
            if (DoNotSend) return;
            string uid = GetUID();

            NameValueCollection values = new NameValueCollection();
            values.Add("uid", uid);
            values.Add("lastRunTime", lastRunTime.ToString());
            values.Add("numberOfParsers", numberOfParsers.ToString());
            values.Add("numberOfPanels", numberOfPanels.ToString());
            values.Add("numberOfAnalyzers", numberOfAnalyzers.ToString());

            WebClient wc = new WebClient();
            wc.UploadValuesAsync(new Uri(baseUrl + "reportUsage"), values);
        }

        internal static void ReportError(string realm, Exception exception)
        {
            if (DoNotSend) return;

            try
            {
                NameValueCollection values = new NameValueCollection();
                values.Add("realm", realm);

                values.Add("message", exception.Message);
                values.Add("trace", exception.StackTrace);

                if (exception.InnerException != null)
                {
                    values.Add("innerMessage", exception.InnerException.Message);
                    values.Add("innerTrace", exception.InnerException.StackTrace);
                }

                WebClient wc = new WebClient();
                wc.UploadValuesAsync(new Uri(baseUrl + "reportError"), values);
            }
            catch (Exception)
            {
                // Don't error handle the error reporting
            }
        }

        internal static void ReportEvent(string type, NameValueCollection data)
        {
            if (DoNotSend) return;

            try
            {
                data.Add("uid", GetUID());
                data.Add("type", type);

                WebClient wc = new WebClient();
                wc.UploadValuesAsync(new Uri(baseUrl + "reportEvent"), data);
            }
            catch (Exception)
            {
                // Don't error handle the event reporting
            }
        }
    }
}
