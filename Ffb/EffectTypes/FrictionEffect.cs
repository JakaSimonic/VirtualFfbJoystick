using System.Collections.Generic;
using System.Linq;
using System;

namespace Ffb
{
    internal class FrictionEffect : IEffectType
    {
        private readonly ICalculationProvider _calculationProvider;

        private List<double> previousAxesPositions;

        public FrictionEffect(ICalculationProvider calculationProvider)
        {
            _calculationProvider = calculationProvider;
        }

        public List<double> GetForce(JOYSTICK_INPUT joystickInput, Dictionary<Type, object> structDictonary, double elapsedTime)
        {
            SET_EFFECT eff = (SET_EFFECT)structDictonary[typeof(SET_EFFECT)];
            List<CONDITION> cond = (List<CONDITION>)structDictonary[typeof(CONDITION)];

            List<double> forces = joystickInput.axesPositions.Select(x => 0d).ToList();

            if (previousAxesPositions != null)
            {
                var axisSpeed = joystickInput.axesPositions.Zip(previousAxesPositions, (u, v) => u - v).ToList();

                forces = _calculationProvider.GetCondition(cond, axisSpeed);
            }

            previousAxesPositions = joystickInput.axesPositions;

            return forces.Select(x => -_calculationProvider.ApplyGain(x, eff.gain)).ToList();
        }
    }
}