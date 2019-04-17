using System;
using System.Collections.Generic;
using TinyIoC;
namespace Ffb
{
    public class FfbEngine
    {
        private EffectsContainer _effectsContainer;
        private IReportDescriptorProperties _reportDescriptorProperties;
        private PID_BLOCK_LOAD pidBlockLoad;
        private PID_STATE pidState;

        public Action DisableActuators;
        public Action EnableActuators;

        public FfbEngine(IReportDescriptorProperties reportDescriptorProperties)
        {
            _effectsContainer = new EffectsContainer(reportDescriptorProperties);
            _reportDescriptorProperties = reportDescriptorProperties;

            pidState.status = 30;
            pidState.effectPlayingAndEffectBlockIndex = 0;
            TinyIoCContainer.Current.Register<ICalculationProvider>(new CalculationProvider());
        }

        public void CreateNewEffect(CREATE_NEW_EFFECT parameter)
        {

            int index = _effectsContainer.GetFirstFree();
            pidBlockLoad.effectBlockIndex = index;
            if (pidBlockLoad.effectBlockIndex == -1)
            {                
                pidBlockLoad.loadStatus = LOAD_STATUS.ERROR;
            }
            else
            {
                _effectsContainer.CreateNewEffect(index, parameter);
                pidBlockLoad.loadStatus = LOAD_STATUS.SUCCESS;
            }
            pidBlockLoad.ramPoolAvailable = _reportDescriptorProperties.MAX_RAM_POOL;
        }

        public void SetEnvelopeParameter(ENVELOPE parameter)
        {
            _effectsContainer.SetParameter(parameter.effectBlockIndex, typeof(ENVELOPE), parameter);
        }

        public void SetCustomForce(CUSTOM_FORCE_PARAMETER parameter)
        {
            _effectsContainer.SetParameter(parameter.effectBlockIndex, typeof(CUSTOM_FORCE_PARAMETER), parameter);
        }

        public void CustomForceData(CUSTOM_FORCE_DATA_REPORT parameter)
        {
            _effectsContainer.SetParameter(parameter.effectBlockIndex, typeof(CUSTOM_FORCE_DATA_REPORT), parameter);
        }

        public void SetConditionParameter(CONDITION parameter)
        {
            _effectsContainer.SetParameter(parameter.effectBlockIndex, typeof(CONDITION), parameter);
        }

        public void SetPeriodParameter(PERIOD parameter)
        {
            _effectsContainer.SetParameter(parameter.effectBlockIndex, typeof(PERIOD), parameter);
        }

        public void SetRampParameter(RAMP parameter)
        {
            _effectsContainer.SetParameter(parameter.effectBlockIndex, typeof(RAMP), parameter);
        }

        public void SetEffectParameter(SET_EFFECT parameter)
        {
            _effectsContainer.SetParameter(parameter.effectBlockIndex, typeof(SET_EFFECT), parameter);
        }

        public void SetConstantParameter(CONSTANT parameter)
        {
            _effectsContainer.SetParameter(parameter.effectBlockIndex, typeof(CONSTANT), parameter);
        }

        public void EffectsOperation(OPERATION operation)
        {
            switch (operation.effectOp)
            {
                case EFFECT_OPERATION.START:
                    if (operation.loopCount > 0)
                    {
                        _effectsContainer.SetDuration(operation.effectBlockIndex, operation.loopCount);
                    }
                    _effectsContainer.StartEffect(operation.effectBlockIndex);
                    break;

                case EFFECT_OPERATION.SOLO:
                    _effectsContainer.StopAllEffects();
                    _effectsContainer.StartEffect(operation.effectBlockIndex);

                    break;

                case EFFECT_OPERATION.STOP:
                    _effectsContainer.StopEffect(operation.effectBlockIndex);
                    break;
            }
        }

        public void PIDBlockFree(PID_BLOCK_FREE parameter)
        {
            int effectBlockIndex = parameter.effectBlockIndex;
            if (effectBlockIndex == _reportDescriptorProperties.FREE_ALL_EFFECTS)
            {
                _effectsContainer.FreeAllEffects();
            }
            else
            {
                _effectsContainer.FreeEffect(effectBlockIndex);
            }
        }

        public void PIDDeviceControl(PID_DEVICE_CONTROL parameter)
        {
            PID_CONTROL pidControl = parameter.pidControl;
            switch (pidControl)
            {
                case PID_CONTROL.ENABLE_ACTUATORS:
                    EnableActuators?.Invoke();
                    pidState.status = 30;
                    break;

                case PID_CONTROL.DISABLE_ACTUATORS:
                    DisableActuators?.Invoke();
                    pidState.status = 28;
                    break;

                case PID_CONTROL.STOP_ALL_EFFECTS:
                    _effectsContainer.StopAllEffects();
                    break;

                case PID_CONTROL.RESET:
                    _effectsContainer.StartAllEffects();
                    break;

                case PID_CONTROL.PAUSE:
                    _effectsContainer.StopAllEffects();
                    break;

                case PID_CONTROL.CONTINUE:
                    _effectsContainer.ContinueAllEffects();
                    break;
            }
        }

        public void DeviceGain(DEVICE_GAIN parameter)
        {
            _effectsContainer.SetDeviceGain(parameter.deviceGain);
        }

        public PID_BLOCK_LOAD GetBlockLoad()
        {
            return pidBlockLoad;
        }

        public PID_STATE GetPidState()
        {
            return pidState;
        }
        public PID_POOL_REPORT GetPidPoolReport()
        {
            PID_POOL_REPORT ppr = new PID_POOL_REPORT();
            ppr.report=new byte[] { 0x07, 0xFF, 0xFF, 0xFF, 0x01 };
            return ppr;
        }

        public List<double> GetForces(JOYSTICK_INPUT joystickInput)
        {
            return _effectsContainer.GetForce(joystickInput);
        }
    }
}