﻿using System;
using System.Collections.Generic;

namespace Ffb
{
    internal class CustomEffect : IEffectType
    {
        private ICalculationProvider _calculationProvider;

        public CustomEffect(ICalculationProvider calculationProvider)
        {
            _calculationProvider = calculationProvider;
        }

        public List<double> GetForce(JOYSTICK_INPUT joystickInput, Dictionary<string, object> structDictonary, double elapsedTime)
        {
            SET_EFFECT eff = (SET_EFFECT)structDictonary["SET_EFFECT"];
            CUSTOM_FORCE_PARAMETER customForceParameter = (CUSTOM_FORCE_PARAMETER)structDictonary["CUSTOM_FORCE_PARAMETER"];
            CUSTOM_FORCE_DATA_REPORT customForceDataReport = (CUSTOM_FORCE_DATA_REPORT)structDictonary["CUSTOM_FORCE_DATA_REPORT"];

            List<double> forces = new List<double>();
            List<int> samples = customForceDataReport.samples;
            double sampleCount = customForceParameter.sampleCount;
            double period = customForceParameter.samplePeriod;
            List<double> directions = _calculationProvider.GetDirection(eff);

            int sampleNormalized = (int)Math.Round((elapsedTime / period) * sampleCount);
            int index = _calculationProvider.GetCustomEffectSampleIndex(sampleNormalized, samples.Count);

            foreach (var direction in directions)
            {
                forces.Add(samples[index++] * direction);
            }
            return forces;
        }
    }
}