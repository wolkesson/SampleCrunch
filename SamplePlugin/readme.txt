
= Creating plugins =
The softwar is centred around a plugin framework to allow support for new and custom file formats. It also allow new panels (displays) to be created to visualize the data. Most panels and parsers in the software are build using the framework so there are many examples in the source code. There's also an exmple plugin to show the bare minium. 

== Parser Plugin ==
Parsers is used to provide data from a logfile of a format. It consists of two parts; a factory class to provide settings and opening the file and a file parser / reader that reads the actual data from file. 
[ParserPlugin("Test Plugin", "Log File | *.dat")]
    public class TestPlugin : ILogFileParser