using System;

namespace RainmeterFreeze.Enumerations {
    /// <summary>
    /// Specifies the algorithm to use when determining when to freeze Rainmeter.
    /// </summary>
    [Serializable]
    public enum FreezeAlgorithm {
        /// <summary>
        /// Rainmeter is only frozen when the foreground window is fully maximized.
        /// </summary>
        Maximized,

        /// <summary>
        /// Rainmeter is frozen when any window that isn't qualified as the
        /// desktop is in focus.
        /// </summary>
        NotOnDesktop,

        /// <summary>
        /// Rainmeter is only frozen when the foreground window is in
        /// full-screen mode (takes up 100% of the total screen area).
        /// </summary>
        FullScreen
    }
}
