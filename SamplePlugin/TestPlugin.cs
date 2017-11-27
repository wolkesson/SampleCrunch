using PluginFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SamplePlugins
{
    [ParserPlugin("Test Plugin", "Log File | *.dat")]
    public class TestPlugin : ILogFileParser
    {
        public TestPlugin()
        {
        }

        public bool CanOpen(string filename)
        {
            return !string.IsNullOrWhiteSpace(filename);
        }

        public ILogFile Open(string fileName, ParserSettings settings = null)
        {
            return (ILogFile)new TestLogFile(fileName);
        }

        public bool ShowSettingsDialog(string fileName, ref ParserSettings settings)
        {
            return true;
        }
    }

    class TestLogFile:ILogFile
    {
        static private int instance = 0;

        public TestLogFile(string fileName)
        {
            this.Signals = new SignalList();
            this.Signals.Add(new Signal() { Name = "Signal 1",  });
            this.Signals.Add(new Signal() { Name = "Signal 2"});
            this.Signals.Add(new Signal() { Name = "Strange letters: ~Åäö /" });

            this.Signals.Add(new Signal() { Name = "Longitude", Unit="deg", Tag = "Longitude", UID= 10 });
            this.Signals.Add(new Signal() { Name = "Latitude" , Unit="deg", Tag = "Latitude", UID =11 });

            this.Origo = new DateTime(2015, 08, 10,13,11,3).AddMinutes(instance);
            this.Length = TimeSpan.FromHours(3);
            this.SampleTime = 40;
            instance++;
        }

        public Dictionary<string, object> ComputedSignals
        {
            get { throw new NotImplementedException(); }
        }

        public void ComputeData()
        {
            throw new NotImplementedException();
        }

        public DateTime Origo
        {
            get;
            set;
        }

        public SignalList Signals
        {
            get;
            private set;
        }

        public Sample[] ReadSignal(Signal signal)
        {
            return ReadSignal(signal.UID);
        }

        public Sample[] ReadSignal(int uid)
        {
            Sample[] returnSample = new Sample[(int)(this.Length.TotalMilliseconds / SampleTime)];
                DateTime t = Origo;

            switch (uid)
            {
                case 10:
                    for (long i = 0; i < returnSample.LongLength; i++)
                    {
                        returnSample[i] = new Sample() { Time = t, Value = Math.Sin(t.Ticks * 0.00000002) * 0.01 + 58 };
                        t = t.AddMilliseconds(SampleTime);
                    }
                    break;
                case 11:
                    for (long i = 0; i < returnSample.LongLength; i++)
                    {
                        returnSample[i] = new Sample() { Time = t, Value = Math.Cos(t.Ticks * 0.00000002) * 0.01 + 15 };
                        t = t.AddMilliseconds(SampleTime);
                    }
                    break;
                default:
                    Random rnd = new Random();
                    for (long i = 0; i < returnSample.LongLength; i++)
                    {
                        returnSample[i] = new Sample() { Time = t, Value = rnd.NextDouble() };
                        t = t.AddMilliseconds(SampleTime);
                    }
                    break;
            }
            return returnSample;
        }

        public double SampleTime
        {
            get;
            private set;
        }

        public TimeSpan Length
        {
            get;
            private set;
        }

        public void Close()
        {
            throw new NotImplementedException();
        }


        public Sample[] ReadSignal(string signalName)
        {
            return ReadSignal(0);
        }
    }
}
