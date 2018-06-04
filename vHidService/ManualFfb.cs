using System.IO;
using System.Linq;

namespace vHidService
{
    internal class ManualFfb
    {
        private FfbWrapper _ffbWrapper;

        public ManualFfb(FfbWrapper ffbWrapper)
        {
            _ffbWrapper = ffbWrapper;
            using (StreamReader reader = new StreamReader("InitFfbManuly.txt"))
            {
                string line;
                int counter = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    byte[] stream = line.Split(new char[] { '-' }).Select(x => byte.Parse(x, System.Globalization.NumberStyles.HexNumber)).ToArray();
                    if (counter++ % 4 == 0)
                        ffbWrapper.SetFeature(stream);
                    else
                        ffbWrapper.WriteReport(stream);
                }
            }
        }

        public void UpdateEffects()
        {
            UpdateEffect(1, Context.ManualConditionEffects.spring);
            UpdateEffect(2, Context.ManualConditionEffects.damper);
            UpdateEffect(3, Context.ManualConditionEffects.inertia);
            UpdateEffect(4, Context.ManualConditionEffects.friction);
            UpdateEffect(5, Context.ManualConditionEffects.limit);
            UpdateEffect(7, Context.ManualConditionEffects.gain);
        }

        private void UpdateEffect(int effectOffset, object parameter)
        {
            switch (effectOffset)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    ConditionEffect conditionEffect = (ConditionEffect)parameter;
                    byte[] stream = "03-03-00-00-00-10-27-10-27-00-FF-FF".Split(new char[] { '-' }).Select(x => byte.Parse(x, System.Globalization.NumberStyles.HexNumber)).ToArray();
                    stream[1] = (byte)effectOffset;
                    stream[5] = (byte)(conditionEffect.coefficient % 256);
                    stream[6] = (byte)(conditionEffect.coefficient / 256);
                    stream[7] = (byte)(conditionEffect.coefficient % 256);
                    stream[8] = (byte)(conditionEffect.coefficient / 256);
                    stream[9] = conditionEffect.deadBand;
                    _ffbWrapper.WriteReport(stream);

                    stream = "0A-01-01-01".Split(new char[] { '-' }).Select(x => byte.Parse(x, System.Globalization.NumberStyles.HexNumber)).ToArray();
                    stream[1] = (byte)effectOffset;
                    stream[2] = (byte)(conditionEffect.enabled ? 1 : 3);
                    _ffbWrapper.WriteReport(stream);
                    break;

                case 5:
                    MotionLimit motinLimit = (MotionLimit)parameter;
                    byte[] stream2 = "03-03-00-00-00-10-27-10-27-00-FF-FF".Split(new char[] { '-' }).Select(x => byte.Parse(x)).ToArray();
                    stream2[1] = (byte)effectOffset;
                    stream2[3] = (byte)(motinLimit.cpOfffset % 256);
                    stream2[4] = (byte)(motinLimit.cpOfffset / 256);
                    stream2[5] = 0xFF;
                    stream2[6] = 0x7F;
                    stream2[7] = 0;
                    stream2[8] = 0;

                    _ffbWrapper.WriteReport(stream2);

                    stream = "0A-01-01-01".Split(new char[] { '-' }).Select(x => byte.Parse(x, System.Globalization.NumberStyles.HexNumber)).ToArray();
                    stream[1] = (byte)effectOffset;
                    stream[2] = (byte)(motinLimit.enabled ? 1 : 3);
                    _ffbWrapper.WriteReport(stream);

                    stream2[1]++;
                    stream2[5] = 0;
                    stream2[6] = 0;
                    stream2[7] = 0xFF;
                    stream2[8] = 0x7F;

                    _ffbWrapper.WriteReport(stream2);

                    stream = "0A-01-01-01".Split(new char[] { '-' }).Select(x => byte.Parse(x, System.Globalization.NumberStyles.HexNumber)).ToArray();
                    stream[1] = (byte)effectOffset;
                    stream[2] = (byte)(motinLimit.enabled ? 1 : 3);
                    _ffbWrapper.WriteReport(stream);

                    break;

                case 7:
                    _ffbWrapper.WriteReport(new byte[2] { 0x0D, (byte)(int)parameter });
                    break;

                default:
                    break;
            }
        }
    }
}