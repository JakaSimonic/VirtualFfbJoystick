using System;
using System.Collections.Generic;

namespace Ffb
{
    internal class SineEffect : IEffectType
    {
        private readonly ICalculationProvider _calculationProvider;

        public SineEffect(ICalculationProvider calculationProvider)
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

            double angle = ((elapsedTime / period) + (phase) * period) * 2 * Math.PI;
            double sine = Math.Sin(angle);
            double tempforce = sine * magnitude;
            tempforce += offset;

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