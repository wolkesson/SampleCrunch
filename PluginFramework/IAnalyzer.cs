﻿using System.Collections.Generic;

namespace PluginFramework
{
    public interface IAnalyzer
    {
        Dictionary<string, object> Analyze(IParser logfile);
    }
}
