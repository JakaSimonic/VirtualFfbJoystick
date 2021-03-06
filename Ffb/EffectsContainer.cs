using System;
using System.Collections.Generic;
using System.Linq;
using TinyIoC;

namespace Ffb
{
    internal class EffectsContainer
    {
        private List<IEffect> _effects;
        private double _deviceGain;
        private Dictionary<EFFECT_TYPE, Func<IEffectType>> efectsDict;
        private IReportDescriptorProperties _reportDescriptorProperties;

        public EffectsContainer(IReportDescriptorProperties reportDescriptorProperties)
        {
            _effects = new List<IEffect>();

            ICalculationProvider calculationProvider = TinyIoCContainer.Current.Resolve<ICalculationProvider>();
            _reportDescriptorProperties = reportDescriptorProperties;

            efectsDict = new Dictionary<EFFECT_TYPE, Func<IEffectType>>()
              {
                {EFFECT_TYPE.CONSTANT, ()=>new ConstantEffect(calculationProvider) },
                {EFFECT_TYPE.RAMP,()=>new RampEffect(calculationProvider)},
                {EFFECT_TYPE.SQUARE, ()=>new SquareEffect(calculationProvider)},
                {EFFECT_TYPE.SINE, ()=>new SineEffect(calculationProvider)},
                {EFFECT_TYPE.TRIANGLE,()=>new TriangleEffect(calculationProvider) },
                {EFFECT_TYPE.SAWTOOTH_UP, ()=>new SawtoothDownEffect(calculationProvider) },
                {EFFECT_TYPE.SAWTOOTH_DOWN,()=>new SawtoothUpEffect(calculationProvider) },
                {EFFECT_TYPE.SPRING, ()=>new SpringEffect(calculationProvider) },
                {EFFECT_TYPE.DAMPER, ()=>new DamperEffect(calculationProvider)},
                {EFFECT_TYPE.INERTIA, ()=>new InertiaEffect(calculationProvider) },
                {EFFECT_TYPE.FRICTION, ()=>new FrictionEffect(calculationProvider) },
                {EFFECT_TYPE.CUSTOM, ()=>new CustomEffect(calculationProvider)}
              };
        }

        public int GetFirstFree()
        {
            return _effects.GetFirstFreeSlot();
        }

        public void FreeEffect(int effectBlockIndex)
        {
            _effects.InsertEffect(effectBlockIndex, null);
        }

        public void FreeAllEffects()
        {
            _effects = _effects.Select(x => (IEffect)null).ToList();
        }

        public void StopAllEffects()
        {
            foreach (var effect in _effects)
            {
                effect?.Stop();
            }
        }

        public void StartEffect(int effectBlockIndex)
        {
            IEffect effect = _effects.GetEffect(effectBlockIndex);
            effect.Start();
        }

        public void StartAllEffects()
        {
            foreach (var effect in _effects)
            {
                effect?.Start();
            }
        }

        public void StopEffect(int effectBlockIndex)
        {
            IEffect effect = _effects.GetEffect(effectBlockIndex);
            effect.Stop();
        }

        public void ContinueAllEffects()
        {
            foreach (var effect in _effects)
            {
                effect?.Continue();
            }
        }

        public void ContinueEffect(int effectBlockIndex)
        {
            IEffect effect = _effects.GetEffect(effectBlockIndex);
            effect.Continue();
        }

        public List<double> GetForce(JOYSTICK_INPUT joystickInput)
        {
            ManageTriggerEffects(joystickInput.pressedButtonOffsets);

            List<double> forcesSum = joystickInput.axesPositions.Select(x => 0d).ToList();

            foreach (var effect in _effects.ToList())
            {
                if (effect != null && effect.Operational)
                {
                    List<double> forces = effect.GetForce(joystickInput);
                    forcesSum = forcesSum.Zip(forces, (u, v) => u + v).ToList();
                }
            }
            forcesSum = forcesSum.Select(x => x * _deviceGain).ToList();
            return forcesSum;
        }

        private void ManageTriggerEffects(List<int> pressedButtons)
        {
            foreach (var effect in _effects.ToList())
            {
                if (effect != null && effect.Operational)
                {
                    int triggerButton = ((SET_EFFECT)effect.GetParameter(typeof(SET_EFFECT))).trigerButton;
                    if (triggerButton == 0xFF)
                    {
                        continue;
                    }

                    if (pressedButtons.Contains(triggerButton))
                    {
                        effect.TriggerButtonPressed();
                    }
                    else
                    {
                        effect.TriggerButtonReleased();
                    }
                }
            }
        }

        public void CreateNewEffect(int effectBlockIndex, CREATE_NEW_EFFECT parameter)
        {
            EFFECT_TYPE effetType = parameter.effetType;
            IEffectType effectTypeObject = efectsDict[effetType].Invoke();
            IEffect effect = new Effect(effectTypeObject, _reportDescriptorProperties);
            _effects.InsertEffect(effectBlockIndex, effect);
        }

        public void SetParameter(int effectBlockIndex, Type type, object parameter)
        {
            IEffect effect = _effects.GetEffect(effectBlockIndex);
            effect.SetParameter(type, parameter);
        }

        public void SetDuration(int effectBlockIndex, long loopCount)
        {
            IEffect effect = _effects.GetEffect(effectBlockIndex);
            SET_EFFECT setEffect = (SET_EFFECT)effect.GetParameter(typeof(SET_EFFECT));

            if (loopCount == _reportDescriptorProperties.MAX_LOOP)
            {
                ENVELOPE envelope = (ENVELOPE)effect.GetParameter(typeof(ENVELOPE));

                setEffect.duration = _reportDescriptorProperties.DURATION_INFINITE;
                envelope.fadeTime = 0;
            }
            else
            {
                setEffect.duration *= loopCount;
            }
        }

        public void SetDeviceGain(double deviceGain)
        {
            _deviceGain = deviceGain;
        }
    }
}