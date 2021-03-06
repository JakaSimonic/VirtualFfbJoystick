using System;
using System.Collections.Generic;

namespace Ffb
{
    internal class ConstantEffect : IEffectType
    {
        private ICalculationProvider _calculationProvider;

        public ConstantEffect(ICalculationProvider calculationProvider)
        {
            _calculationProvider = calculationProvider;
        }

        public List<double> GetForce(JOYSTICK_INPUT joystickInput, Dictionary<Type, object> structDictonary, double elapsedTime)
        {
            ENVELOPE env = (ENVELOPE)structDictonary[typeof(ENVELOPE)];
            SET_EFFECT eff = (SET_EFFECT)structDictonary[typeof(SET_EFFECT)];
            CONSTANT constant = (CONSTANT)structDictonary[typeof(CONSTANT)];

            List<double> forces = new List<double>();

            double magnitude = _calculationProvider.ApplyGain(constant.magnitude, eff.gain);

            double envelope = _calculationProvider.ApplyGain(_calculationProvider.GetEnvelope(env, elapsedTime, eff.duration), eff.gain);

            List<double> directions = _calculationProvider.GetDirection(eff);
            foreach (var direction in directions)
            {
               forces.Add(magnitude * envelope * direction);
            }
            return forces;
        }
    }
}