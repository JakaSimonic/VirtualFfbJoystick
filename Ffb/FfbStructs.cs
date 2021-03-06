using System.Collections.Generic;

namespace Ffb
{
    public struct CONDITION
    {
        public int effectBlockIndex;
        public int blockOffset;
        public double deadBand;
        public int cpOffset;
        public double negativeSaturation;
        public double positiveSaturation;
        public int negativeCoefficient;
        public int positiveCoefficient;
    }

    public struct CONSTANT
    {
        public int effectBlockIndex;
        public double magnitude;
    }

    public struct ENVELOPE
    {
        public int effectBlockIndex;
        public double attackLevel;
        public int attackTime;
        public double fadeLevel;
        public int fadeTime;
    }

    public struct OPERATION
    {
        public int effectBlockIndex;
        public EFFECT_OPERATION effectOp;
        public long loopCount;
    }

    public struct PERIOD
    {
        public int effectBlockIndex;
        public double magnitude;
        public double offset;
        public int period;
        public int phase;
    }

    public struct RAMP
    {
        public int effectBlockIndex;
        public double end;
        public double start;
    }

    public struct SET_EFFECT
    {
        public int effectBlockIndex;
        public long duration;
        public double gain;
        public long samplePeriod;
        public int trigerButton;
        public long triggerRepeatInterval;
        public double directionX;
        public double directionY;
        public bool polar;
        public List<bool> axisEnabled;
        public EFFECT_TYPE effetType;
        public long startDelay;
    }

    public struct PID_BLOCK_FREE
    {
        public int effectBlockIndex;
    }

    public struct PID_DEVICE_CONTROL
    {
        public PID_CONTROL pidControl;
    }

    public struct DEVICE_GAIN
    {
        public double deviceGain;
    }
    public struct CREATE_NEW_EFFECT
    {
        public EFFECT_TYPE effetType;
    }

    public struct PID_POOL_REPORT
    {
        public byte[] report; 
    }

    public struct PID_BLOCK_LOAD
    {
        public int effectBlockIndex;   // 1..40
        public LOAD_STATUS loadStatus; // 1=Success,2=Full,3=Error
        public uint ramPoolAvailable;	// =0 or 0xFFFF?
    }

    public struct PID_STATE
    {
        public int status;
        public int effectPlayingAndEffectBlockIndex;
    }

    public struct CUSTOM_FORCE_DATA_REPORT
    {
        public int effectBlockIndex;
        public List<int> samples;
    }

    public struct CUSTOM_FORCE_PARAMETER
    {
        public int effectBlockIndex;
        public int sampleCount;
        public int samplePeriod;
    }
    public enum LOAD_STATUS
    {
        SUCCESS = 1,
        FULL = 2,
        ERROR = 3
    }

    public enum EFFECT_OPERATION
    {
        START = 1,
        SOLO = 2,
        STOP = 3
    }

    public enum PID_CONTROL
    {
        ENABLE_ACTUATORS = 1,
        DISABLE_ACTUATORS = 2,
        STOP_ALL_EFFECTS = 4,
        RESET = 8,
        PAUSE = 16,
        CONTINUE = 32
    }

    public enum EFFECT_TYPE
    {
        CONSTANT = 1,
        RAMP = 2,
        SQUARE = 3,
        SINE = 4,
        TRIANGLE = 5,
        SAWTOOTH_UP = 6,
        SAWTOOTH_DOWN = 7,
        SPRING = 8,
        DAMPER = 9,
        INERTIA = 10,
        FRICTION = 11,
        CUSTOM = 12
    }

    public struct JOYSTICK_INPUT
    {
        public List<double> axesPositions;
        public List<int> pressedButtonOffsets;
    }
}