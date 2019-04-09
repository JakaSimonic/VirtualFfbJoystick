using System;
using System.Collections.Generic;

namespace Ffb
{
    internal interface IEffect
    {
        void Start();
        void Continue();
        void Stop();
        object GetParameter(Type parmType);
        void SetParameter(Type type, object parameter);
        List<double> GetForce(JOYSTICK_INPUT jostickInput);
        void TriggerButtonPressed();
        void TriggerButtonReleased();
        bool Operational { get; }
    }
}