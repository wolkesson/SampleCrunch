using PluginFramework;
using System;

namespace SamplePlugins
{
    [ParserPlugin("Test Plugin", "Log File | *.dat")]
    public class TestPlugin : IParserFactory
    {
        public bool CanOpen(string filename)
        {
            return !string.IsNullOrWhiteSpace(filename);
        }

        public IParser Open(string fileName, ParserSettings settings = null)
        {
            return (IParser)new TestLogFile(fileName);
        }

        public bool ShowSettingsDialog(string fileName, ref ParserSettings settings)
        {
            return true;
        }
    }

    class TestLogFile : IParser
    {
        static private int instance = 0;
        const int sampleTime = 40;

        public TestLogFile(string fileName)
        {
            this.Signals = new SignalList();
            this.Signals.Add(new Signal() { Name = "Signal 1" });
            this.Signals.Add(new Signal() { Name = "Signal 2" });
            this.Signals.Add(new Signal() { Name = "Strange letters: ~Åäö /" });

            this.Signals.Add(new Signal() { Name = "Longitude", Unit = "deg", Tag = "Longitude", UID = 10 });
            this.Signals.Add(new Signal() { Name = "Latitude", Unit = "deg", Tag = "Latitude", UID = 11 });

            this.Origo = new DateTime(2015, 08, 10, 13, 11, 3).AddMinutes(instance);
            this.Length = TimeSpan.FromHours(3);
            instance++;
        }

        public DateTime Origo
        {
            get;
            private set;
        }

        public SignalList Signals
        {
            get;
            private set;
        }

        public Sample[] ReadSignal(Signal signal)
        {
            Sample[] returnSample = new Sample[(int)(this.Length.TotalMilliseconds / sampleTime)];
            DateTime t = Origo;

            switch (signal.UID)
            {
                case 10:
                    for (long i = 0; i < returnSample.LongLength; i++)
                    {
                        returnSample[i] = new Sample() { Time = t, Value = Math.Sin(t.Ticks * 0.00000002) * 0.01 + 58 };
                        t = t.AddMilliseconds(sampleTime);
                    }
                    break;
                case 11:
                    for (long i = 0; i < returnSample.LongLength; i++)
                    {
                        returnSample[i] = new Sample() { Time = t, Value = Math.Cos(t.Ticks * 0.00000002) * 0.01 + 15 };
                        t = t.AddMilliseconds(sampleTime);
                    }
                    break;
                default:
                    Random rnd = new Random();
                    for (long i = 0; i < returnSample.LongLength; i++)
                    {
                        returnSample[i] = new Sample() { Time = t, Value = rnd.NextDouble() };
                        t = t.AddMilliseconds(sampleTime);
                    }
                    break;
            }
            return returnSample;
        }

        public TimeSpan Length
        {
            get;
            private set;
        }
    }
}
