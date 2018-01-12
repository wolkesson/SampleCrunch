using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace PluginFramework
{
    /// <summary>
    /// 
    /// </summary>
    public static class PluginFactory
    {
        static PluginFactory()
        {
            Info = new List<PluginInfo>();
            ParserFactories = new List<IParserFactory>();
            Analyzers = new List<Type>();
            PanelFactories = new List<IPanelFactory>();
        }

        /// <summary>
        /// Add plugins from path. Can be called multiple times to include several paths.
        /// </summary>
        /// <param name="pluginPath">The path to include plugins from.</param>
        public static List<Exception> LoadPlugins(string pluginPath)
        {
            if (!Directory.Exists(pluginPath))
            {
                throw new DirectoryNotFoundException("The plugin directory (" + Path.GetFullPath(pluginPath) + ") could not be found!");
            }

            // Save any exception and continue loading next dll
            List<Exception> exList = new List<Exception>();
            foreach (var dll in Directory.EnumerateFiles(pluginPath, "*.dll"))
            {
                var domain = AppDomain.CurrentDomain;
                domain.ReflectionOnlyAssemblyResolve += MyInterceptMethod;
                string name = Path.GetFileName(dll);
                try
                {
                    Assembly nextAssembly = Assembly.Load(File.ReadAllBytes(dll));
                    var info = new PluginInfo(string.Format("{0} v{1}", nextAssembly.GetName().Name, nextAssembly.GetName().Version), dll);

                    foreach (var type in nextAssembly.GetTypes())
                    {
                        foreach (var item in type.GetInterfaces())
                        {
                            if (item.Name == nameof(IParserFactory) && !type.IsGenericType && !type.IsInterface)
                            {
                                IParserFactory factory = PluginFactory.CreateParserFactory(type);
                                ParserFactories.Add(factory);

                                ParserPluginAttribute attr = type.GetCustomAttribute<ParserPluginAttribute>(false);
                                if (attr != null)
                                {
                                    info.AddItem(new PluginInfo.ItemInfo() { Name = string.Format("{0} ({1})", attr.Title, attr.FileType), Type = PluginInfo.ItemType.Parser });
                                }
                            }
                            else if (item.Name == nameof(IAnalyzer) && !type.IsGenericType && !type.IsInterface)
                            {
                                AnalyzerPluginAttribute attr = type.GetCustomAttribute<AnalyzerPluginAttribute>(false);
                                Analyzers.Add(type);
                                info.AddItem(new PluginInfo.ItemInfo() { Name = type.Name, Type = PluginInfo.ItemType.Analyzer });
                            }
                            else if (item.Name == nameof(IPanelFactory) && !type.IsGenericType && !type.IsInterface)
                            {
                                IPanelFactory factory = PluginFactory.CreatePanelFactory(type);
                                PanelFactories.Add(factory);
                                info.AddItem(new PluginInfo.ItemInfo() { Name = factory.Title, Type = PluginInfo.ItemType.Display });
                            }
                        }
                    }

                    // Add assembly item is containing any plugin
                    if (info.Items.Count > 0) Info.Add(info);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Exception exL = ex.LoaderExceptions[0];
                    if (exL is TypeLoadException)
                    {
                        exList.Add(new PluginLoadException("Plugin " + name + " could not load from " + pluginPath + "! Please contact plugin author.", exL));
                    }
                    else
                    {
                        exList.Add(new PluginLoadException("Plugin " + name + " could not load! Please contact plugin author.", ex.LoaderExceptions[0]));
                    }
                }
                catch (Exception ex)
                {
                    exList.Add(new PluginLoadException("Plugin " + name + " could not load! Please contact plugin author.", ex));
                }
                finally
                {
                    domain.ReflectionOnlyAssemblyResolve -= MyInterceptMethod;
                }
            } // End of file loop

            return exList;
        }

        /// <summary>
        /// Resets the PluginFactory. Should olny be used in test classes.
        /// </summary>
        public static void Reset()
        {
            Info.Clear();
            ParserFactories.Clear();
            Analyzers.Clear();
            PanelFactories.Clear();
        }

        private static List<Type> modelTypes = new List<Type>();
        public static Type[] GetModelTypes()
        {
            var types = from item in PluginFactory.PanelFactories select item.ModelType;
            var list = types.ToList<Type>();
            list.AddRange(modelTypes);
            return list.ToArray<Type>();
        }

        public static void RegisterModelType(Type modelType)
        {
            if (!modelTypes.Contains(modelType)) modelTypes.Add(modelType);
        }

        public static List<PluginInfo> Info {get; private set;}
        public static List<IParserFactory> ParserFactories { get; private set; }
        public static List<Type> Analyzers { get; private set; }
        public static List<IPanelFactory> PanelFactories { get; private set; }

        public static string FileFilterString
        {
            get
            {
                List<string> fileFilter = new List<string>();
                foreach (IParserFactory type in ParserFactories)
                {
                    ParserPluginAttribute attr = type.GetType().GetCustomAttribute<ParserPluginAttribute>(false);
                    if (attr != null)
                    {
                        fileFilter.Add(attr.FileType);
                    }
                }

                fileFilter.Add("All files |*.*");

                return String.Join("|", fileFilter.ToArray());
            }
        }

        public static IParserFactory CreateParserFactory(Type type)
        {
            IParserFactory plugin = (IParserFactory)Activator.CreateInstance(type);
            return plugin;
        }

        public static IAnalyzer CreateAnalyzer(Type type)
        {
            IAnalyzer plugin = (IAnalyzer)Activator.CreateInstance(type);
            return plugin;
        }

        public static IPanelFactory CreatePanelFactory(Type type)
        {
            IPanelFactory factory = (IPanelFactory)Activator.CreateInstance(type);
            return factory;
        }

        public static IParserFactory FindParser(string typename)
        {
            var ret = from item in ParserFactories where item.GetType().FullName == typename select item;

            return ret.FirstOrDefault();
        }

        public static IParserFactory FindLogFileParser(string filename)
        {
            IParserFactory suggestedLogFileParser = null;
            foreach (IParserFactory tmpParser in PluginFactory.ParserFactories)
            {
                if (tmpParser.CanOpen(filename))
                {
                    suggestedLogFileParser = tmpParser;
                    break;
                }
            }

            return suggestedLogFileParser;
        }

        /// <summary>
        /// Finds all analyzers that can work upon the log file provided.
        /// </summary>
        /// <param name="ILogFile">The i log file.</param>
        /// <returns></returns>
        public static List<Type> FindAnalyzers(Type logfileParser)
        {
            List<Type> returnAnalyzers = new List<Type>();
            foreach (Type analyzerType in Analyzers)
            {
                AnalyzerPluginAttribute attr = analyzerType.GetCustomAttribute<AnalyzerPluginAttribute>(false);
                if (attr != null)
                {
                    if (attr.GetParsers().Contains(logfileParser))
                    {
                        returnAnalyzers.Add(analyzerType);
                    }
                }
            }

            return returnAnalyzers;
        }

        public static IPanelFactory FindPanelFactory(IPanelModel model)
        {
            var ret = from item in PanelFactories where item.GetType().FullName == model.FactoryReference select item;

            return ret.FirstOrDefault();
        }

        private static Assembly MyInterceptMethod(object sender, ResolveEventArgs e)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            Assembly a = Assembly.ReflectionOnlyLoadFrom(Path.Combine(path, "PluginFramework.dll"));
            return a;
        }

        public class PluginInfo
        {
            List<ItemInfo> items = new List<ItemInfo>();

            public PluginInfo(string assemblyName, string assemblyPath)
            {
                this.AssemblyName = assemblyName;
                this.AssemblyPath = assemblyPath;
            }

            public string AssemblyName { get; private set; }
            public string AssemblyPath { get; private set; }
            public ReadOnlyCollection<ItemInfo> Items { get { return items.AsReadOnly(); } }

            public void AddItem(ItemInfo info)
            {
                this.items.Add(info);
            }

            public struct ItemInfo
            {
                public string Name { get; set; }
                public ItemType Type { get; set; }
            }

            public enum ItemType
            {
                Parser,
                Display,
                Analyzer
            }
        }


        [Serializable]
        public class PluginLoadException : Exception
        {
            public PluginLoadException() { }
            public PluginLoadException(string message) : base(message) { }
            public PluginLoadException(string message, Exception inner) : base(message, inner) { }
            protected PluginLoadException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
    }
}
