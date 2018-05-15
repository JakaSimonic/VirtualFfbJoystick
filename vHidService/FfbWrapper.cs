using Ffb;
using System;
using System.Collections.Generic;
using System.Linq;

namespace vHidService
{
    public class FfbWrapper
    {
        private FfbEngine _ffbEngine;
        private Dictionary<int, EFFECT_OPERATION> EffectOperationMapper;
        private Dictionary<int, EFFECT_TYPE> EffectTypeMapper;
        private Dictionary<int, SetInvoke> FfbWriteReportTypeMapper;
        private Dictionary<int, SetInvoke> FfbSetFeatureTypeMapper;
        private Dictionary<int, GetInvoke> FfbGetFeatureTypeMapper;
        private Dictionary<int, GetInvoke> FfbReadReportTypeMapper;

        public FfbWrapper()
        {
            _ffbEngine = (new FfbEngineFactory(new ReportDescriptorProoperties())).Create();

            EffectOperationMapper = new Dictionary<int, EFFECT_OPERATION>()
            {
              {1, EFFECT_OPERATION.START},
              {2 , EFFECT_OPERATION.SOLO},
              {3 , EFFECT_OPERATION.STOP}
            };

            EffectTypeMapper = new Dictionary<int, EFFECT_TYPE>()
            {
                {1,EFFECT_TYPE.CONSTANT},
                {2,EFFECT_TYPE.RAMP},
                {3,EFFECT_TYPE.SQUARE},
                {4,EFFECT_TYPE.SINE},
                {5,EFFECT_TYPE.TRIANGLE},
                {6,EFFECT_TYPE.SAWTOOTH_UP},
                {7,EFFECT_TYPE.SAWTOOTH_DOWN},
                {8,EFFECT_TYPE.SPRING},
                {9,EFFECT_TYPE.DAMPER},
                {10,EFFECT_TYPE.INERTIA},
                {11,EFFECT_TYPE.FRICTION},
                {12,EFFECT_TYPE.CUSTOM}
                };

            FfbWriteReportTypeMapper = new Dictionary<int, SetInvoke>()
            {
                { 1, new SetInvoke(_ffbEngine.Set["SetEffect"], SetEffectMapper ) },
                { 2, new SetInvoke(_ffbEngine.Set["SetEnvelope"], EnvelopeMapper ) },
                { 3, new SetInvoke(_ffbEngine.Set["SetCondition"], ConditionMapper) },
                { 4,  new SetInvoke( _ffbEngine.Set["SetPeriodic"], PeriodMapper) },
                { 5,  new SetInvoke( _ffbEngine.Set["SetConstantForce"], ConstantForceMapper ) },
                { 6,  new SetInvoke( _ffbEngine.Set["SetRampForce"], RampMapper ) },
                { 10, new SetInvoke( _ffbEngine.Set["EffectOperation"], OperationMapper ) },
                { 13, new SetInvoke( _ffbEngine.Set["DeviceGain"], DeviceGainMapper ) },
                { 12, new SetInvoke( _ffbEngine.Set["PidDeviceControl"], PidDeviceControlMapper) },
                { 11, new SetInvoke(_ffbEngine.Set["PidBlockFree"], BlockFreeMapper ) },
                { 7, new SetInvoke( _ffbEngine.Set["CustomForceData"], CustomForceDataMapper) },
                { 14,new SetInvoke(_ffbEngine.Set["SetCustomForce"], CustomForceMapper) }
            };

            FfbSetFeatureTypeMapper = new Dictionary<int, SetInvoke>()
            {
                { 5, new SetInvoke(_ffbEngine.Set["CreateNewEffect"], CreateNewEffectMapper)}
            };

            FfbGetFeatureTypeMapper = new Dictionary<int, GetInvoke>()
            {
                {6, new GetInvoke(_ffbEngine.Get["BlockLoad"], PidBlockLoadMapper) },
                {7, new GetInvoke(_ffbEngine.Get["PidPoolReport"], PidPoolReportMapper) }
            };

            FfbReadReportTypeMapper = new Dictionary<int, GetInvoke>()
            {
                {2, new GetInvoke(_ffbEngine.Get["PidState"], PidStateMapper) },
            };
        }

        private class GetInvoke
        {
            private Func<object, byte[]> _mapper;
            private Func<object> _ffbFunc;

            internal GetInvoke(Func<object> ffbFunc, Func<object, byte[]> mapper)
            {
                _mapper = mapper;
                _ffbFunc = ffbFunc;
            }

            internal byte[] Invoke(byte[] buffer)
            {
                if (_ffbFunc != null)
                {
                    return _mapper(_ffbFunc());
                }
                else
                {
                    return _mapper(null);
                }
            }
        }

        public void WriteReport(byte[] buffer)
        {
            SetInvoke invoke = FfbWriteReportTypeMapper[buffer[0]];
            invoke.Invoke(buffer);
        }

        public void SetFeature(byte[] buffer)
        {
            SetInvoke invoke = FfbSetFeatureTypeMapper[buffer[0]];
            invoke.Invoke(buffer);
        }

        public byte[] GetFeature(byte[] buffer)
        {
            GetInvoke invoke = FfbGetFeatureTypeMapper[buffer[0]];
            return invoke.Invoke(buffer);
        }

        private class SetInvoke
        {
            private Func<byte[], object> _mapper;
            private Action<object> _ffbFunc;

            internal SetInvoke(Action<object> ffbFunc, Func<byte[], object> mapper)
            {
                _mapper = mapper;
                _ffbFunc = ffbFunc;
            }

            internal void Invoke(byte[] buffer)
            {
                _ffbFunc(_mapper(buffer));
            }
        }

        private object ConditionMapper(byte[] buffer)
        {
            return new CONDITION
            {
                effectBlockIndex = buffer[1],
                blockOffset = buffer[2],
                cpOffset = BitConverter.ToInt16(buffer, 3),
                positiveCoefficient = BitConverter.ToInt16(buffer, 5),
                negativeCoefficient = BitConverter.ToInt16(buffer, 7),
                positiveSaturation = buffer[9],
                negativeSaturation = buffer[10],
                deadBand = buffer[11],
            };
        }

        private object ConstantForceMapper(byte[] buffer)
        {
            return new CONSTANT
            {
                effectBlockIndex = buffer[1],
                magnitude = BitConverter.ToUInt16(buffer, 2)
            };
        }

        private object EnvelopeMapper(byte[] buffer)
        {
            return new ENVELOPE
            {
                effectBlockIndex = buffer[1],
                attackLevel = buffer[2],
                attackTime = BitConverter.ToUInt16(buffer, 4),
                fadeLevel = buffer[3],
                fadeTime = BitConverter.ToUInt16(buffer, 6)
            };
        }

        private object OperationMapper(byte[] buffer)
        {
            return new OPERATION
            {
                effectBlockIndex = buffer[1],
                effectOp = (EFFECT_OPERATION)buffer[2],
                loopCount = buffer[3]
            };
        }

        private object PeriodMapper(byte[] buffer)
        {
            return new PERIOD
            {
                effectBlockIndex = buffer[1],
                magnitude = buffer[2],
                offset = (sbyte)buffer[3] * 2,
                phase = buffer[4],
                period = BitConverter.ToUInt16(buffer, 5),
            };
        }

        private object RampMapper(byte[] buffer)
        {
            return new RAMP
            {
                effectBlockIndex = buffer[1],
                end = (sbyte)buffer[3] * 2,
                start = (sbyte)buffer[2] * 2
            };
        }

        private object SetEffectMapper(byte[] buffer)
        {
            return new SET_EFFECT
            {
                effectBlockIndex = buffer[1],
                effetType = (EFFECT_TYPE)buffer[2],
                duration = BitConverter.ToUInt16(buffer, 3),
                triggerRepeatInterval = BitConverter.ToUInt16(buffer, 5),
                samplePeriod = BitConverter.ToUInt16(buffer, 7),
                gain = buffer[9],
                trigerButton = buffer[10],
                axisEnabled = new List<bool>() { ((buffer[11] & 1) != 0), (((buffer[11] >> 1) & 1) != 0) },
                polar = (((buffer[11] >> 2) & 1) != 0),
                directionX = (sbyte)buffer[12],
                directionY = (sbyte)buffer[13],
                startDelay = BitConverter.ToUInt16(buffer, 14)
            };
        }

        private object BlockFreeMapper(byte[] buffer)
        {
            return new PID_BLOCK_FREE
            {
                effectBlockIndex = buffer[1]
            };
        }

        private object PidDeviceControlMapper(byte[] buffer)
        {
            return new PID_DEVICE_CONTROL
            {
                pidControl = (PID_CONTROL)buffer[1]
            };
        }

        private object DeviceGainMapper(byte[] buffer)
        {
            return new DEVICE_GAIN
            {
                deviceGain = buffer[1]
            };
        }

        private object CreateNewEffectMapper(byte[] buffer)
        {
            return new CREATE_NEW_EFFECT
            {
                effetType = (EFFECT_TYPE)buffer[1]
            };
        }

        private byte[] PidBlockLoadMapper(object ffbStruct)
        {
            PID_BLOCK_LOAD pidBlockLoad = (PID_BLOCK_LOAD)ffbStruct;
            byte[] buffer = new byte[5];

            buffer[0] = 6;
            buffer[1] = (byte)pidBlockLoad.effectBlockIndex;
            buffer[2] = (byte)pidBlockLoad.loadStatus;
            buffer[3] = (byte)(pidBlockLoad.ramPoolAvailable >> 8);
            buffer[4] = (byte)pidBlockLoad.ramPoolAvailable;

            return buffer;
        }

        private byte[] PidPoolReportMapper(object ffbStruct)
        {
            return (byte[])ffbStruct;
        }

        private byte[] PidStateMapper(object ffbStruct)
        {
            byte[] buffer = new byte[3];
            buffer[0] = 2;
            buffer[1] = (byte)((PID_STATE)ffbStruct).status;
            buffer[2] = (byte)((PID_STATE)ffbStruct).effectPlayingAndEffectBlockIndex;
            return buffer;
        }

        private object CustomForceMapper(byte[] buffer)
        {
            return new CUSTOM_FORCE_DATA_REPORT
            {
                effectBlockIndex = buffer[1],
                samples = buffer.Skip(4).Select(x => (int)(sbyte)x).ToList()
            };
        }

        private object CustomForceDataMapper(byte[] buffer)
        {
            return new CUSTOM_FORCE_PARAMETER
            {
                effectBlockIndex = buffer[1],
                sampleCount = buffer[2],
                samplePeriod = BitConverter.ToUInt16(buffer, 3)
            };
        }

        internal class ReportDescriptorProoperties : IReportDescriptorProperties
        {
            public double MAX_GAIN { get; private set; }
            public double ENVELOPE_MAX { get; private set; }
            public double DIRECTION_MAX { get; private set; }
            public double MAX_PHASE { get; private set; }
            public uint FREE_ALL_EFFECTS { get; }
            public int MAX_LOOP { get; }
            public int DOWNLOAD_FORCE_SAMPLE_AXES { get; }
            public int MAX_DEVICE_GAIN { get; }
            public uint MAX_RAM_POOL { get; }
            public long DURATION_INFINITE { get; }

            public double MAX_VALUE_EFFECT { get; }

            public ReportDescriptorProoperties()
            {
                DIRECTION_MAX = 255d;

                DOWNLOAD_FORCE_SAMPLE_AXES = 2;

                DURATION_INFINITE = -1;

                ENVELOPE_MAX = 255d;

                FREE_ALL_EFFECTS = 0xFF;

                MAX_DEVICE_GAIN = 255;

                MAX_GAIN = 255d;

                MAX_LOOP = 255;

                MAX_PHASE = 255d;

                MAX_RAM_POOL = 0xFFFF;

                MAX_VALUE_EFFECT = 255;
            }
        }

        public List<double> GetForces(JOYSTICK_INPUT input)
        {
            return _ffbEngine.GetForces(input);
        }
    }
}