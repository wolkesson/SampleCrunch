using PluginFramework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnitTestProject")]

namespace Sample_Crunch.StandardPanels
{
    public class CsvFile : IParser, IDisposable
    {
        private readonly string fileName;
        private readonly MemoryMappedFile file;
        private MemoryMappedViewStream fs;
        private StreamReader sr;
        private SignalList signals;
        private int timeColumn = 0;
        private char splitChar = ';';
        private NumberFormatInfo nfi;
        private DateTime[] timeVector;

        public CsvFile(string filename, ParserSettings settings)
        {
            readSettings(settings);
            this.file = MemoryMappedFile.CreateFromFile(
               //include a readonly shared stream
               File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read),
               //not mapping to a name
               null,
               //use the file's actual size
               0L,
               //read only access
               MemoryMappedFileAccess.Read,
               //not configuring security
               null,
               //adjust as needed
               HandleInheritability.None,
               //close the previously passed in stream when done
               false);

            this.fs = file.CreateViewStream(0L, 0L, MemoryMappedFileAccess.Read);
            this.sr = new StreamReader(fs);
            readSignalsAndTime();
        }

        /// <summary>
        /// Only used for testing
        /// </summary>
        /// <param name="settings"></param>
        internal CsvFile(StreamReader sr, ParserSettings settings)
        {
            this.sr = sr;
            readSettings(settings);
            readSignalsAndTime();
        }

        private void readSettings(ParserSettings settings)
        {
            splitChar = (char)settings.Read("Delimiter", ';');
            nfi = new NumberFormatInfo() { NumberDecimalSeparator = settings.Read("Decimal", ".").ToString() };
            timeColumn = (int)settings.Read("TimeVector", 0);
        }

        internal void readSignalsAndTime() {

            // Assume first line to be titles
            string titleLine = sr.ReadLine();
            var titles = titleLine.Split(new char[] { splitChar }, StringSplitOptions.RemoveEmptyEntries);

            this.signals = new SignalList();
            int col = 0;
            foreach (string title in titles)
            {
                signals.Add(new Signal() { Name = title, UID = col });
                col++;
            }

            // Assume first column to be time
            List<DateTime> samples = new List<DateTime>();
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();

                var values = line.Split(new char[] { splitChar }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length > timeColumn)
                {
                    var timeStr = values[timeColumn];
                    // Try to parse as double
                    double doubleValue;
                    DateTime dt;
                    if (double.TryParse(timeStr, NumberStyles.Any, nfi, out doubleValue))
                    {
                        dt = new DateTime((long)(doubleValue * TimeSpan.TicksPerSecond));
                        samples.Add(dt);
                    }
                    else if (DateTime.TryParse(timeStr, out dt))
                    {
                        samples.Add(dt);
                    }
                }
            }

            timeVector = samples.ToArray();
        }

        public SignalList Signals
        {
            get
            {
                return signals;
            }
        }

        public DateTime Origo
        {
            get
            {
               return timeVector[0];
            }
        }

        public TimeSpan Length
        {
            get
            {
                return timeVector[timeVector.Length-1]- Origo;
            }
        }

        public Sample[] ReadSignal(Signal signal)
        {
            sr.BaseStream.Seek(0, SeekOrigin.Begin);

            // Assume first line to be titles
            string titleLine = sr.ReadLine();
            var titles = titleLine.Split(splitChar);

            List<Sample> samples = new List<Sample>();
            int row = 0;
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var values = line.Split(splitChar);
                double value = double.NaN;
                if (values.Length > signal.UID && double.TryParse(values[signal.UID], NumberStyles.Any, nfi, out value))
                {
                    var sample = new Sample() { Time = timeVector[row], Value = value };
                    samples.Add(sample);
                }
                row++;
            }

            return samples.ToArray();
        }

        public void Dispose()
        {
            fs.Close();
            file.Dispose();
        }
    }

    [ParserPlugin("CSV Plugin", "CSV File | *.csv")]
    public class CsvParserFactory : IParserFactory
    {        
        public bool CanOpen(string filename)
        {
            return (Path.GetExtension(filename) == ".csv");
        }

        public IParser Open(string filename, ParserSettings settings = null)
        {
            return new CsvFile(filename, settings);
        }

        public bool ShowSettingsDialog(string filename, ref ParserSettings settings)
        {
            StreamReader sr = new StreamReader(filename);

            // Assume first line to be titles
            string titleLine = sr.ReadLine();

            CsvViewModel settingsModel = new CsvViewModel(titleLine);
            CsvOpenForm f = new CsvOpenForm(settingsModel);

            if (f.ShowDialog().Value)
            {
                settings.Write("Delimiter", CsvViewModel.GetDelimiter(settingsModel.SelectedDelimiter));
                settings.Write("Decimal", CsvViewModel.GetDecimalSeparator(settingsModel.SelectedDecimalSeparator));
                settings.Write("TimeVector", settingsModel.SelectedTimeVector);
                return true;
            }

            return false;
        }
    }
}
