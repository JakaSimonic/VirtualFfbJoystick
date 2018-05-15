using System;
using System.Collections.Generic;
using System.Linq;

namespace Ffb
{
    internal class Effect : IEffect
    {
        private long startTime;
        private long pauseTime = 0;
        private long lastUpdate;
        private bool paused = true;
        private bool buttonReleased = false;
        private IEffectType _effect;
        private Dictionary<string, object> _structDictonary;
        private IReportDescriptorProperties _reportDescriptorProperties;

        public bool Operational
        {
            get;
            private set;
        } = false;

        public Effect(IEffectType effect, IReportDescriptorProperties reportDescriptorProperties)
        {
            _effect = effect;
            _structDictonary = new Dictionary<string, object>();
            _reportDescriptorProperties = reportDescriptorProperties;
        }

        public void Start()
        {
            paused = false;
            long startDelay = ((SET_EFFECT)_structDictonary["SET_EFFECT"]).startDelay;
            startTime = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) + startDelay;
            lastUpdate = startTime;
            pauseTime = 0;
        }

        public void Stop()
        {
            paused = true;
            pauseTime = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
        }

        public void Continue()
        {
            paused = false;
            startTime += (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - pauseTime;
            pauseTime = 0;
        }

        public List<double> GetForce(JOYSTICK_INPUT joystickInput)
        {
            List<double> forces = joystickInput.axesPositions.Select(x => 0d).ToList();
            if (!paused)
            {
                long elapsedTime = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTime;
                long lastUpdateDeltaTime = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - lastUpdate;
                lastUpdate = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
                SET_EFFECT setEffect = (SET_EFFECT)_structDictonary["SET_EFFECT"];

                long duration = setEffect.duration;
                long samplePeriod = setEffect.samplePeriod;
                if (((elapsedTime > 0) && 
                    ((elapsedTime < duration) || (duration == _reportDescriptorProperties.DURATION_INFINITE))) && 
                    (lastUpdateDeltaTime > samplePeriod))
                {
                    forces = _effect.GetForce(joystickInput, _structDictonary, elapsedTime);
                }
            }
            return forces;
        }

        public void SetParameter(string parmName, object parameter)
        {
            if (!parmName.Equals("CONDITION") && !parmName.Equals("CUSTOM_FORCE_DATA_REPORT"))
            {
                if (parmName.Equals("SET_EFFECT"))
                {
                    Operational = true;
                }

                if (_structDictonary.ContainsKey(parmName))
                {
                    _structDictonary[parmName] = parameter;
                }
                else
                {
                    _structDictonary.Add(parmName, parameter);
                }
            }
            else if (parmName == "CONDITION")
            {
                if (_structDictonary.ContainsKey(parmName))
                {
                    List<CONDITION> conditionList = (List<CONDITION>)_structDictonary[parmName];
                    int index = ((CONDITION)parameter).blockOffset;
                    if (conditionList.Count > index)
                    {
                        conditionList[index] = (CONDITION)parameter;
                    }
                    else
                    {
                        conditionList.Add((CONDITION)parameter);
                        //_structDictonary[parmName] = conditionList;
                    }
                }
                else
                {
                    List<CONDITION> conditionList = new List<CONDITION>();
                    conditionList.Add((CONDITION)parameter);
                    _structDictonary.Add(parmName, conditionList);
                }
            }
            else
            {
                if (_structDictonary.ContainsKey(parmName))
                {
                    List<int> samples = ((CUSTOM_FORCE_DATA_REPORT)parameter).samples;
                    ((CUSTOM_FORCE_DATA_REPORT)_structDictonary[parmName]).samples.AddRange(samples);
                }
                else
                {
                    _structDictonary.Add(parmName, parameter);
                }
            }
        }

        public object GetParameter(string parmName)
        {
            return _structDictonary[parmName];
        }

        public void TriggerButtonPressed()
        {
            if (paused)
            {
                Start();
                buttonReleased = false;
                return;
            }

            if (!buttonReleased)
            {
                long elapsedTime = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - startTime;
                SET_EFFECT setEffect = (SET_EFFECT)_structDictonary["SET_EFFECT"];
                long duration = setEffect.duration;
                long triggerRepeatInterval = setEffect.triggerRepeatInterval;
                if ((duration + triggerRepeatInterval) > elapsedTime)
                {
                    Start();
                }
            }
        }

        public void TriggerButtonReleased()
        {
            buttonReleased = true;
            paused = true;
        }
    }
}