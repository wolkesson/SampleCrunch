using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Sample_Crunch.StandardPanels.Tests
{
    [TestClass()]
    public class CsvFileTests
    {
        [TestMethod()]
        public void SemiColonDot()
        {
            run(';', '.');
        }

        [TestMethod()]
        public void CommaDot()
        {
            run(',', '.');
        }

        [TestMethod()]
        public void TabDot()
        {
            run('\t', '.');
        }

        [TestMethod()]
        public void SemiColonComma()
        {
            run(';', ',');
        }

        [TestMethod()]
        public void TabComma()
        {
            run('\t', ',');
        }

        [TestMethod()]
        public void SemiColonColon()
        {
            run(';', ':');
        }

        [TestMethod()]
        public void CommaColon()
        {
            run(',', ':');
        }

        [TestMethod()]
        public void TabColon()
        {
            run('\t', ':');
        }

        public void run(char delimiter, char decimalSign)
        {
            double[] times = { 1.56, 2.03, 4.05 };
            double[] sig1 = { 1, 2, 3 };
            double[] sig2 = { 1.11, 2.22, 3.33 };


            var nfi = new NumberFormatInfo() { NumberDecimalSeparator = decimalSign.ToString() };

            MemoryStream stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            string delim = delimiter.ToString();
            writer.WriteLine("Time"+ delim+"Signal1"+ delim+"Signal2"+ delim);
            for (int i = 0; i < 3; i++)
            {
                writer.WriteLine(times[i].ToString(nfi) + delim + sig1[i].ToString(nfi) + delim + sig2[i].ToString(nfi) + delim);
            }
            writer.Flush();
            stream.Position = 0;
            var text = new StreamReader(stream).ReadToEnd();

            stream.Position = 0;

            var settings = new PluginFramework.ParserSettings();
            settings.Write("Delimiter", delimiter);
            settings.Write("Decimal", decimalSign);
            settings.Write("TimeVector", 0);

            StreamReader sr = new StreamReader(stream);
            CsvFile csvFile = new CsvFile(sr, settings);

            Assert.AreEqual(3, csvFile.Signals.Count);
            var s0 = csvFile.ReadSignal(csvFile.Signals[0]);

            var s1 = csvFile.ReadSignal(csvFile.Signals[1]);
            var s2 = csvFile.ReadSignal(csvFile.Signals[2]);

            CollectionAssert.AreEqual(sig1, s1.Select((s) => s.Value).ToArray());
            CollectionAssert.AreEqual(sig2, s2.Select((s) => s.Value).ToArray());
        }
    }
}