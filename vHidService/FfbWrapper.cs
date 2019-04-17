using Ffb;
using System;
using System.Collections.Generic;
using System.Linq;

namespace vHidService
{
    public class FfbWrapper
    {
        private FfbEngine _ffbEngine;
        private IReportDescriptorProperties reportDescriptorProperties = new ReportDescriptorProperties();
        private Dictionary<int, object> FfbWriteReportPipe;
        private Dictionary<int, object> FfbSetFeaturePipe;
        private Dictionary<int, object> FfbGetFeaturePipe;
        private Dictionary<int, object> FfbReadReportPipe;
        private Dictionary<Type, Delegate> FFbSetMethods;
        private Dictionary<Type, Delegate> FfbGetMethods;

        public FfbWrapper()
        {
            _ffbEngine = new FfbEngine(reportDescriptorProperties);

            FFbSetMethods = new Dictionary<Type, Delegate>()
              {
                {typeof(SET_EFFECT), new Action<SET_EFFECT>(_ffbEngine.SetEffectParameter)},
                {typeof(ENVELOPE), new Action<ENVELOPE>(_ffbEngine.SetEnvelopeParameter)},
                {typeof(CONDITION), new Action<CONDITION>(_ffbEngine.SetConditionParameter)},
                {typeof(PERIOD), new Action<PERIOD>(_ffbEngine.SetPeriodParameter)},
                {typeof(CONSTANT), new Action<CONSTANT>(_ffbEngine.SetConstantParameter)},
                {typeof(RAMP), new Action<RAMP>(_ffbEngine.SetRampParameter)},
                {typeof(OPERATION), new Action<OPERATION>(_ffbEngine.EffectsOperation)},
                {typeof(PID_BLOCK_FREE), new Action<PID_BLOCK_FREE>(_ffbEngine.PIDBlockFree) },
                {typeof(PID_DEVICE_CONTROL), new Action<PID_DEVICE_CONTROL>(_ffbEngine.PIDDeviceControl) },
                {typeof(DEVICE_GAIN), new Action<DEVICE_GAIN>(_ffbEngine.DeviceGain) },
                {typeof(CREATE_NEW_EFFECT), new Action<CREATE_NEW_EFFECT>(_ffbEngine.CreateNewEffect) },
                {typeof(CUSTOM_FORCE_DATA_REPORT), new Action<CUSTOM_FORCE_DATA_REPORT>(_ffbEngine.CustomForceData)},
                {typeof(CUSTOM_FORCE_PARAMETER), new Action<CUSTOM_FORCE_PARAMETER>(_ffbEngine.SetCustomForce) },
              };

            FfbGetMethods = new Dictionary<Type, Delegate>()
              {
                {typeof(PID_BLOCK_LOAD), new Func<PID_BLOCK_LOAD>(_ffbEngine.GetBlockLoad) },
                {typeof(PID_STATE), new Func<PID_STATE>(_ffbEngine.GetPidState)},
                {typeof(PID_POOL_REPORT), new Func<PID_POOL_REPORT>(_ffbEngine.GetPidPoolReport) }
              };

            FfbWriteReportPipe = new Dictionary<int, object>()
            {
                { 1, new SetInvoke<SET_EFFECT>(FFbSetMethods[typeof(SET_EFFECT)],  new Func<byte[], SET_EFFECT>(SetEffectMapper) ) },
                { 2, new SetInvoke<ENVELOPE>(FFbSetMethods[typeof(ENVELOPE)],  new Func<byte[], ENVELOPE>(EnvelopeMapper) ) },
                { 3, new SetInvoke<CONDITION>(FFbSetMethods[typeof(CONDITION)],  new Func<byte[], CONDITION>(ConditionMapper)) },
                { 4,  new SetInvoke<PERIOD>( FFbSetMethods[typeof(PERIOD)],  new Func<byte[], PERIOD>(PeriodMapper)) },
                { 5,  new SetInvoke<CONSTANT>( FFbSetMethods[typeof(CONSTANT)],  new Func<byte[], CONSTANT>(ConstantForceMapper) ) },
                { 6,  new SetInvoke<RAMP>( FFbSetMethods[typeof(RAMP)],  new Func<byte[], RAMP>(RampMapper) ) },
                { 10, new SetInvoke<OPERATION>( FFbSetMethods[typeof(OPERATION)],  new Func<byte[], OPERATION>(OperationMapper) ) },
                { 13, new SetInvoke<DEVICE_GAIN>( FFbSetMethods[typeof(DEVICE_GAIN)],  new Func<byte[], DEVICE_GAIN>(DeviceGainMapper) ) },
                { 12, new SetInvoke<PID_DEVICE_CONTROL>( FFbSetMethods[typeof(PID_DEVICE_CONTROL)],  new Func<byte[], PID_DEVICE_CONTROL>(PidDeviceControlMapper)) },
                { 11, new SetInvoke<PID_BLOCK_FREE>(FFbSetMethods[typeof(PID_BLOCK_FREE)],  new Func<byte[], PID_BLOCK_FREE>(BlockFreeMapper) ) },
                { 7, new SetInvoke<CUSTOM_FORCE_PARAMETER>( FFbSetMethods[typeof(CUSTOM_FORCE_PARAMETER)],  new Func<byte[], CUSTOM_FORCE_PARAMETER>(CustomForceDataMapper)) },
                { 14,new SetInvoke<CUSTOM_FORCE_DATA_REPORT>(FFbSetMethods[typeof(CUSTOM_FORCE_DATA_REPORT)],  new Func<byte[], CUSTOM_FORCE_DATA_REPORT>(CustomForceMapper)) }
            };

            FfbSetFeaturePipe = new Dictionary<int, object>()
            {
                { 5, new SetInvoke<CREATE_NEW_EFFECT>(FFbSetMethods[typeof(CREATE_NEW_EFFECT)],  new Func<byte[], CREATE_NEW_EFFECT>(CreateNewEffectMapper)) }
            };

            FfbGetFeaturePipe = new Dictionary<int, object>()
            {
                {6, new GetInvoke<PID_BLOCK_LOAD>(FfbGetMethods[typeof(PID_BLOCK_LOAD)],  new Func<PID_BLOCK_LOAD, byte[]>(PidBlockLoadMapper)) },
                {7, new GetInvoke<PID_POOL_REPORT>(FfbGetMethods[typeof(PID_POOL_REPORT)],  new Func<PID_POOL_REPORT, byte[]>(PidPoolReportMapper)) }
            };

            FfbReadReportPipe = new Dictionary<int, object>()
            {
                {2, new GetInvoke<PID_STATE>(FfbGetMethods[typeof(PID_STATE)],  new Func<PID_STATE, byte[]>(PidStateMapper)) },
            };
        }

        private class GetInvoke<T>
        {
            private Func<T, byte[]> _mapper;
            private Delegate _ffbFunc;

            internal GetInvoke(Delegate ffbFunc, Func<T, byte[]> mapper)
            {
                _mapper = mapper;
                _ffbFunc = ffbFunc;
            }

            internal byte[] Invoke(byte[] buffer)
            {
                return _mapper((T)_ffbFunc.DynamicInvoke());
            }
        }

        private class SetInvoke<T>
        {
            private Func<byte[], T> _mapper;
            private Delegate _ffbFunc;

            internal SetInvoke(Delegate ffbFunc, Func<byte[], T> mapper)
            {
                _mapper = mapper;
                _ffbFunc = ffbFunc;
            }

            internal void Invoke(byte[] buffer)
            {
                _ffbFunc.DynamicInvoke(_mapper(buffer));
            }
        }

        public void WriteReport(byte[] buffer)
        {
            dynamic invoke = FfbWriteReportPipe[buffer[0]];
            invoke.Invoke(buffer);
        }

        public void SetFeature(byte[] buffer)
        {
            dynamic invoke = FfbSetFeaturePipe[buffer[0]];
            invoke.Invoke(buffer);
        }

        public byte[] GetFeature(byte[] buffer)
        {
            dynamic invoke = FfbGetFeaturePipe[buffer[0]];
            return invoke.Invoke(buffer);
        }

        public List<double> GetForces(JOYSTICK_INPUT input)
        {
            return _ffbEngine.GetForces(input);
        }

        #region Mappers
        private CONDITION ConditionMapper(byte[] buffer)
        {
            return new CONDITION
            {
                effectBlockIndex = buffer[1],
                blockOffset = buffer[2],
                cpOffset = BitConverter.ToInt16(buffer, 3),
                positiveCoefficient = BitConverter.ToInt16(buffer, 5),
                negativeCoefficient = BitConverter.ToInt16(buffer, 7),
                positiveSaturation = buffer[10] / reportDescriptorProperties.MAX_GAIN * reportDescriptorProperties.PHYSICAL_MAXIMUM,
                negativeSaturation = buffer[11] / reportDescriptorProperties.MAX_GAIN * reportDescriptorProperties.PHYSICAL_MAXIMUM,
                deadBand = buffer[9] / reportDescriptorProperties.MAX_GAIN * reportDescriptorProperties.PHYSICAL_MAXIMUM
            };
        }

        private CONSTANT ConstantForceMapper(byte[] buffer)
        {
            return new CONSTANT
            {
                effectBlockIndex = buffer[1],
                magnitude = BitConverter.ToUInt16(buffer, 2) / reportDescriptorProperties.MAX_GAIN * reportDescriptorProperties.PHYSICAL_MAXIMUM
            };
        }

        private ENVELOPE EnvelopeMapper(byte[] buffer)
        {
            return new ENVELOPE
            {
                effectBlockIndex = buffer[1],
                attackLevel = buffer[2] / reportDescriptorProperties.MAX_GAIN,
                attackTime = BitConverter.ToUInt16(buffer, 4),
                fadeLevel = buffer[3] / reportDescriptorProperties.MAX_GAIN,
                fadeTime = BitConverter.ToUInt16(buffer, 6)
            };
        }

        private OPERATION OperationMapper(byte[] buffer)
        {
            return new OPERATION
            {
                effectBlockIndex = buffer[1],
                effectOp = (EFFECT_OPERATION)buffer[2],
                loopCount = buffer[3]
            };
        }

        private PERIOD PeriodMapper(byte[] buffer)
        {
            return new PERIOD
            {
                effectBlockIndex = buffer[1],
                magnitude = buffer[2] / reportDescriptorProperties.MAX_GAIN * reportDescriptorProperties.PHYSICAL_MAXIMUM,
                offset = (sbyte)buffer[3] / reportDescriptorProperties.MAXIMUM_SIGNED * reportDescriptorProperties.PHYSICAL_MAXIMUM,
                phase = buffer[4],
                period = BitConverter.ToUInt16(buffer, 5),
            };
        }

        private RAMP RampMapper(byte[] buffer)
        {
            return new RAMP
            {
                effectBlockIndex = buffer[1],
                end = (sbyte)buffer[3] / reportDescriptorProperties.MAXIMUM_SIGNED * reportDescriptorProperties.PHYSICAL_MAXIMUM,
                start = (sbyte)buffer[2] / reportDescriptorProperties.MAXIMUM_SIGNED * reportDescriptorProperties.PHYSICAL_MAXIMUM
            };
        }

        private SET_EFFECT SetEffectMapper(byte[] buffer)
        {
            return new SET_EFFECT
            {
                effectBlockIndex = buffer[1],
                effetType = (EFFECT_TYPE)buffer[2],
                duration = BitConverter.ToInt16(buffer, 3),
                triggerRepeatInterval = BitConverter.ToInt16(buffer, 5),
                samplePeriod = BitConverter.ToUInt16(buffer, 7),
                gain = buffer[9] / reportDescriptorProperties.MAX_GAIN,
                trigerButton = buffer[10],
                axisEnabled = new List<bool>() { ((buffer[11] & 1) != 0), (((buffer[11] >> 1) & 1) != 0) },
                polar = (((buffer[11] >> 2) & 1) != 0),
                directionX = (sbyte)buffer[12] / reportDescriptorProperties.MAX_GAIN * reportDescriptorProperties.PHYSICAL_MAXIMUM_ROTATION,
                directionY = (sbyte)buffer[13] / reportDescriptorProperties.MAX_GAIN * reportDescriptorProperties.PHYSICAL_MAXIMUM_ROTATION,
                startDelay = BitConverter.ToUInt16(buffer, 14)
            };
        }

        private PID_BLOCK_FREE BlockFreeMapper(byte[] buffer)
        {
            return new PID_BLOCK_FREE
            {
                effectBlockIndex = buffer[1]
            };
        }

        private PID_DEVICE_CONTROL PidDeviceControlMapper(byte[] buffer)
        {
            return new PID_DEVICE_CONTROL
            {
                pidControl = (PID_CONTROL)buffer[1]
            };
        }

        private DEVICE_GAIN DeviceGainMapper(byte[] buffer)
        {
            return new DEVICE_GAIN
            {
                deviceGain = buffer[1] / reportDescriptorProperties.MAX_GAIN
            };
        }

        private CREATE_NEW_EFFECT CreateNewEffectMapper(byte[] buffer)
        {
            return new CREATE_NEW_EFFECT
            {
                effetType = (EFFECT_TYPE)buffer[1]
            };
        }

        private byte[] PidBlockLoadMapper(PID_BLOCK_LOAD pidBlockLoad)
        {
            byte[] buffer = new byte[5];

            buffer[0] = 6;
            buffer[1] = (byte)pidBlockLoad.effectBlockIndex;
            buffer[2] = (byte)pidBlockLoad.loadStatus;
            buffer[3] = (byte)(pidBlockLoad.ramPoolAvailable >> 8);
            buffer[4] = (byte)pidBlockLoad.ramPoolAvailable;

            return buffer;
        }

        private byte[] PidPoolReportMapper(PID_POOL_REPORT ffbStruct)
        {
            return ffbStruct.report;
        }

        private byte[] PidStateMapper(PID_STATE ffbStruct)
        {
            byte[] buffer = new byte[3];
            buffer[0] = 2;
            buffer[1] = (byte)ffbStruct.status;
            buffer[2] = (byte)ffbStruct.effectPlayingAndEffectBlockIndex;
            return buffer;
        }

        private CUSTOM_FORCE_DATA_REPORT CustomForceMapper(byte[] buffer)
        {
            return new CUSTOM_FORCE_DATA_REPORT
            {
                effectBlockIndex = buffer[1],
                samples = buffer.Skip(4).Select(x => (sbyte)x + 127).ToList()
            };
        }

        private CUSTOM_FORCE_PARAMETER CustomForceDataMapper(byte[] buffer)
        {
            return new CUSTOM_FORCE_PARAMETER
            {
                effectBlockIndex = buffer[1],
                sampleCount = buffer[2],
                samplePeriod = BitConverter.ToUInt16(buffer, 3)
            };
        }

        private class ReportDescriptorProperties : IReportDescriptorProperties
        {
            public double MAX_GAIN { get; }
            public double ENVELOPE_MAX { get; }
            public double DIRECTION_MAX { get; }
            public double MAX_PHASE { get; }
            public uint FREE_ALL_EFFECTS { get; }
            public int MAX_LOOP { get; }
            public int DOWNLOAD_FORCE_SAMPLE_AXES { get; }
            public int MAX_DEVICE_GAIN { get; }
            public uint MAX_RAM_POOL { get; }
            public long DURATION_INFINITE { get; }

            public double MAX_VALUE_EFFECT { get; }
            public double PHYSICAL_MAXIMUM { get; }
            public double PHYSICAL_MAXIMUM_ROTATION { get; }
            public double MAXIMUM_SIGNED { get; }

            public ReportDescriptorProperties()
            {
                DIRECTION_MAX = 255d;

                DOWNLOAD_FORCE_SAMPLE_AXES = 2;

                DURATION_INFINITE = -1;

                ENVELOPE_MAX = 10000;

                FREE_ALL_EFFECTS = 0xFF;

                MAX_DEVICE_GAIN = 255;

                MAX_GAIN = 255d;

                MAX_LOOP = 255;

                MAX_PHASE = 255d;

                MAX_RAM_POOL = 0xFFFF;

                MAX_VALUE_EFFECT = 255;

                PHYSICAL_MAXIMUM = 10000;

                PHYSICAL_MAXIMUM_ROTATION = 360;
                MAXIMUM_SIGNED = 127;
            }
        }
    }
    #endregion
}