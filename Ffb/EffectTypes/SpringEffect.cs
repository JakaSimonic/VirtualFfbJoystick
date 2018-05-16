using System.Collections.Generic;
using System.Linq;

namespace Ffb
{
    internal class SpringEffect : IEffectType
    {
        private readonly ICalculationProvider _calculationProvider;

        public SpringEffect(ICalculationProvider calculationProvider)
        {
            _calculationProvider = calculationProvider;
        }

        public List<double> GetForce(JOYSTICK_INPUT joystickInput, Dictionary<string, object> structDictonary, double elapsedTime)
        {
            SET_EFFECT eff = (SET_EFFECT)structDictonary["SET_EFFECT"];
            List<CONDITION> cond = (List<CONDITION>)structDictonary["CONDITION"];

            List<double> forces = _calculationProvider.GetCondition(cond, joystickInput.axesPositions);
            return forces.Select(x => _calculationProvider.ApplyGain(x, eff.gain)).ToList();
        }
    }
}