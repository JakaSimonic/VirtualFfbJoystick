using System;

namespace VHClibWrapper
{
    public class HidEventArg : EventArgs
    {
        public HidEventArg(byte[] _buffer, HidIoctlEnum _HidIoctlEnum)
        {
            buffer = _buffer;
            HidIoctlEnum = _HidIoctlEnum;
        }
        public byte[] buffer;
        public HidIoctlEnum HidIoctlEnum;
    }
}