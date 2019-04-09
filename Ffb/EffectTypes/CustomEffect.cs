using System;
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

        public List<double> GetForce(JOYSTICK_INPUT joystickInput, Dictionary<Type, object> structDictonary, double elapsedTime)
        {
            SET_EFFECT eff = (SET_EFFECT)structDictonary[typeof(SET_EFFECT)];
            CUSTOM_FORCE_PARAMETER customForceParameter = (CUSTOM_FORCE_PARAMETER)structDictonary[typeof(CUSTOM_FORCE_PARAMETER)];
            CUSTOM_FORCE_DATA_REPORT customForceDataReport = (CUSTOM_FORCE_DATA_REPORT)structDictonary[typeof(CUSTOM_FORCE_DATA_REPORT)];

            List<double> forces = new List<double>();
            List<int> samples = customForceDataReport.samples;
            int sampleCount = customForceParameter.sampleCount;
            double period = customForceParameter.samplePeriod;
            List<double> directions = _calculationProvider.GetDirection(eff);

            int sampleNormalized = (int)Math.Round((elapsedTime / period) * sampleCount);

            if (sampleNormalized > sampleCount)
            {
                sampleNormalized %= sampleCount;
            }
            int index = sampleNormalized * directions.Count;

            foreach (var direction in directions)
            {
                forces.Add(samples[index++] * direction);
            }
            return forces;
        }
    }
}