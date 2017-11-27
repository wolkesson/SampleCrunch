namespace PluginFramework
{
    public interface IRangeModel
    {
        bool Auto { get; set; }
        double From { get; set; }
        double To { get; set; }
    }
}