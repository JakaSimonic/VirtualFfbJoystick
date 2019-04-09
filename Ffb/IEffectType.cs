using System.Collections.Generic;
using System;

namespace Ffb
{
    internal interface IEffectType
    {
        List<double> GetForce(JOYSTICK_INPUT joystickInput, Dictionary<Type, object> structDictonary, double elapsedTime);
    }
}