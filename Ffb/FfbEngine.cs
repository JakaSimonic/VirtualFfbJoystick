﻿using System;
using System.Collections.Generic;

namespace Ffb
{
    public class FfbEngine
    {
        private Action _enableActuators;
        private Action _disableActuators;
        public event Action EnableActuators
        {
            add
            {
                _enableActuators += value;
            }
            remove
            {
                _enableActuators -= value;
            }
        }

        public event Action DisableActuators
        {
            add
            {
                _disableActuators += value;
            }
            remove
            {
                _disableActuators -= value;
            }
        }
        internal FfbEngine(Dictionary<Type, Action<object>> dictSet, Dictionary<Type, Func<object>> dictGet, Func<JOYSTICK_INPUT, List<double>> getForces, Action enableActuators, Action disableActuators)
        {
            Set = dictSet;
            Get = dictGet;
            GetForces = getForces;
            _enableActuators = enableActuators;
            _disableActuators = disableActuators;
        }

        public Func<JOYSTICK_INPUT,List<double>> GetForces;

        public Dictionary<Type, Action<object>> Set { get; private set; }

        public Dictionary<Type, Func<object>> Get { get; private set; }
    }
}