using System;
using System.Collections.Generic;
using System.Text;

namespace RainmeterFreeze.Enumerations {
    /// <summary>
    /// Defines how RainmeterFreeze freezes Rainmeter.
    /// </summary>
    [Serializable]
    public enum FreezeMode {
        /// <summary>
        /// RainmeterFreeze will suspend Rainmeter.
        /// </summary>
        Suspend,

        /// <summary>
        /// RainmeterFreeze will set Rainmeter's prority to Low.
        /// </summary>
        LowPriority
    }
}
