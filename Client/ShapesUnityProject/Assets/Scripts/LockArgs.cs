using System;

namespace Assets.Scripts
{
    public class LockArgs : EventArgs
    {
        public bool AcruiredLock { get; set; }
    }
}