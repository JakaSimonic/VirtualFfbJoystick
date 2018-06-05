namespace Ffb
{
    public interface IReportDescriptorProperties
    {
        double MAX_GAIN { get; }
        double ENVELOPE_MAX { get; }
        double DIRECTION_MAX { get; }
        double MAX_PHASE { get; }
        uint FREE_ALL_EFFECTS { get; }
        int MAX_LOOP { get; }
        int DOWNLOAD_FORCE_SAMPLE_AXES { get; }
        int MAX_DEVICE_GAIN { get; }
        long DURATION_INFINITE { get; }
        uint MAX_RAM_POOL { get; }
        double MAX_VALUE_EFFECT { get; }
        double PHYSICAL_MAXIMUM { get; }
        double PHYSICAL_MAXIMUM_ROTATION { get; }
        double MAXIMUM_SIGNED { get; }
    }
}