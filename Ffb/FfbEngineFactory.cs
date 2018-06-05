﻿using System;
using System.Collections.Generic;
using TinyIoC;
namespace Ffb
{
    public  class FfbEngineFactory
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
            new Dictionary<string, Action<object>>()
              {
                {"SetEffect",  _ffbLogic.SetEffectParameter},
                {"SetEnvelope",  _ffbLogic.SetEnvelopeParameter},
                {"SetCondition",  _ffbLogic.SetConditionParameter},
                {"SetPeriodic",  _ffbLogic.SetPeriodParameter},
                {"SetConstantForce",  _ffbLogic.SetConstantParameter},
                {"SetRampForce",  _ffbLogic.SetRampParameter},
                {"EffectOperation", _ffbLogic.EffectsOperation },
                {"PidBlockFree", _ffbLogic.PIDBlockFree },
                {"PidDeviceControl", _ffbLogic.PIDDeviceControl },
                {"DeviceGain", _ffbLogic.DeviceGain },
                {"CreateNewEffect", _ffbLogic.CreateNewEffect },
                {"CustomForceData", _ffbLogic.CustomForceData},
                {"SetCustomForce", _ffbLogic.SetCustomForce },
              },

            new Dictionary<string, Func<object>>()
              {
                {"BlockLoad", _ffbLogic.GetBlockLoad },
                {"PidState", _ffbLogic.GetPidState},
                {"PidPoolReport", _ffbLogic.GetPidPoolReport }
              },

            _ffbLogic.GetForces,
            _ffbLogic.EnableActuators,
            _ffbLogic.DisableActuators);

            return ffb;
        }
    }
}