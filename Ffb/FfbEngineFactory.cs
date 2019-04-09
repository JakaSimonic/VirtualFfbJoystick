using System;
using System.Collections.Generic;
using TinyIoC;
namespace Ffb
{
    public class FfbEngineFactory
    {
        private FfbEngineLogic _ffbLogic;
        public FfbEngineFactory(IReportDescriptorProperties reportDescriptorProperties)
        {
            TinyIoCContainer.Current.Register<ICalculationProvider>(new CalculationProvider());
            _ffbLogic = new FfbEngineLogic(reportDescriptorProperties);
        }

        public FfbEngine Create()
        {
            FfbEngine ffb = new FfbEngine(
            new Dictionary<Type, Action<object>>()
              {
                {typeof(SET_EFFECT),  _ffbLogic.SetEffectParameter},
                {typeof(ENVELOPE),  _ffbLogic.SetEnvelopeParameter},
                {typeof(CONDITION),  _ffbLogic.SetConditionParameter},
                {typeof(PERIOD),  _ffbLogic.SetPeriodParameter},
                {typeof(CONSTANT),  _ffbLogic.SetConstantParameter},
                {typeof(RAMP),  _ffbLogic.SetRampParameter},
                {typeof(OPERATION), _ffbLogic.EffectsOperation},
                {typeof(PID_BLOCK_FREE), _ffbLogic.PIDBlockFree },
                {typeof(PID_DEVICE_CONTROL), _ffbLogic.PIDDeviceControl },
                {typeof(DEVICE_GAIN), _ffbLogic.DeviceGain },
                {typeof(CREATE_NEW_EFFECT), _ffbLogic.CreateNewEffect },
                {typeof(CUSTOM_FORCE_DATA_REPORT), _ffbLogic.CustomForceData},
                {typeof(CUSTOM_FORCE_PARAMETER), _ffbLogic.SetCustomForce },
              },

            new Dictionary<Type, Func<object>>()
              {
                {typeof(PID_BLOCK_LOAD), _ffbLogic.GetBlockLoad },
                {typeof(PID_STATE), _ffbLogic.GetPidState},
                {typeof(PID_POOL_REPORT), _ffbLogic.GetPidPoolReport }
              },

            _ffbLogic.GetForces,
            _ffbLogic.EnableActuators,
            _ffbLogic.DisableActuators);

            return ffb;
        }
    }
}