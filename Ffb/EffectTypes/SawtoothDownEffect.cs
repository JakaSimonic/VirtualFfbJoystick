using System;
using System.Collections.Generic;

namespace Ffb
{
    internal class SawtoothDownEffect : IEffectType
    {
        private readonly ICalculationProvider _calculationProvider;

        public SawtoothDownEffect(ICalculationProvider calculationProvider)
        {
            _calculationProvider = calculationProvider;
        }

        public List<double> GetForce(JOYSTICK_INPUT joystickInput, Dictionary<Type, object> structDictonary, double elapsedTime)
        {
            SET_EFFECT eff = (SET_EFFECT)structDictonary[typeof(SET_EFFECT)];
            PERIOD periodic = (PERIOD)structDictonary[typeof(PERIOD)];
            ENVELOPE env = (ENVELOPE)structDictonary[typeof(ENVELOPE)];

            List<double> forces = new List<double>();
            double offset = periodic.offset;
            double magnitude = periodic.magnitude;
            double phase = periodic.phase;
            double period = periodic.period;

            magnitude = _calculationProvider.ApplyGain(magnitude, eff.gain);

            double max = offset + magnitude;
            double min = offset - magnitude;
            double phasetime = (phase * period);
            double time = elapsedTime + phasetime;
            double reminder = time % period;
            double slope = (max - min) / period;
            double tempforce = 0;
            tempforce = slope * reminder;
            tempforce += min;

            double envelope = _calculationProvider.ApplyGain(_calculationProvider.GetEnvelope(env, elapsedTime, eff.duration), eff.gain);

            List<double> directions = _calculationProvider.GetDirection(eff);
            foreach (var direction in directions)
            {
                forces.Add(tempforce * envelope * direction);
            }
            return forces;
        }
    }
}