namespace vHidService
{
    public class Context
    {
        static public Settings Settings { get; set; }
        static public ManualConditionEffects ManualConditionEffects { get; set; }

        static Context()
        {
            Settings = new Settings();
            Settings.plc = new Plc();
            Settings.plc.ipaddress = "::1:AAAA";
            Settings.driver = new Driver();
            ManualConditionEffects = new ManualConditionEffects();
            ManualConditionEffects.damper = new ConditionEffect();
            ManualConditionEffects.spring = new ConditionEffect();
            ManualConditionEffects.inertia = new ConditionEffect();
            ManualConditionEffects.friction = new ConditionEffect();
            ManualConditionEffects.limit = new MotionLimit();
        }
    }

    public class Settings
    {
        public Plc plc;
        public Driver driver;
    }

    public class Plc
    {
        public string ipaddress;
        public int port;
        public long pollPeriod;
    }

    public class Driver
    {
        public int pollPeriod;
    }

    public class ManualConditionEffects
    {
        public ConditionEffect spring;
        public ConditionEffect damper;
        public ConditionEffect inertia;
        public ConditionEffect friction;
        public MotionLimit limit;
        public int gain;
    }

    public class ConditionEffect
    {
        public bool enabled;
        public short coefficient;
        public byte deadBand;
    }

    public class MotionLimit
    {
        public bool enabled;
        public short cpOfffset;
    }
}