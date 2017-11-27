namespace PluginFramework
{
    using System;

    [AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ParserPluginAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly string title;
        readonly string filetype;

        // This is a positional argument
        public ParserPluginAttribute(string title, string filetype)
        {
            this.title = title;
            this.filetype = filetype;
        }

        /// <summary>
        /// The user friendly name of this plugin
        /// </summary>
        public string Title
        {
            get { return title; }
        }

        /// <summary>
        /// The file type of files that this plugin can open. 
        /// </summary>
        public string FileType
        {
            get { return filetype; }
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class AnalyzerPluginAttribute : Attribute
    {
        readonly Type[] parsers;

        // This is a positional argument
        public AnalyzerPluginAttribute(Type[] parsers)
        {
            this.parsers = parsers;
        }

        // This is a named argument
        public Type[] GetParsers() { return parsers; }
    }

    [AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PanelPluginAttribute : Attribute
    {
        string title;
        bool visible=true;

        // This is a positional argument
        public PanelPluginAttribute()
        {
        }

        /// <summary>
        /// The user friendly name of this plugin
        /// </summary>
        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        /// <summary>
        /// The user friendly name of this plugin
        /// </summary>
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }
    }
}