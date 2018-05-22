using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRHouse.ARTools
{
    /// <summary>
    /// Extension methods for the SessionStatus enumeration.
    /// </summary>
    public static class EazyARSessionStatusExtensions
    {
        /// <summary>
        /// Gets whether a SessionStatus is not yet initialized. If simulated, it always returns false.
        /// </summary>
        /// <param name="status">The SessionStatus to check.</param>
        /// <returns><c>true</c> if the SessionStatus is not initialized, otherwise <c>false</c>.</returns>
        public static bool IsARNotInitialized(this SessionStatus status)
        {
            if (EazyARCoreInterface.isSimulated)
            {
                return false;
            }
            else
            {
                return Session.Status.IsNotInitialized();
            }
        }

        /// <summary>
        /// Gets whether a SessionStatus is initialized and valid. If simulated, it always returns true.
        /// </summary>
        /// <param name="status">The SessionStatus to check.</param>
        /// <returns><c>true</c> if the SessionStatus is initialized and valid,
        /// otherwise <c>false</c>.</returns>
        public static bool IsARValid(this SessionStatus status)
        {
            if (EazyARCoreInterface.isSimulated)
            {
                return true;
            }
            else
            {
                return Session.Status.IsValid();
            }
        }

        /// <summary>
        /// Gets whether a SessionStatus is an error. If simulated, it always returns false.
        /// </summary>
        /// <param name="status">The SessionStatus to check.</param>
        /// <returns><c>true</c> if the SessionStatus is an error,
        /// otherwise <c>false</c>.</returns>
        public static bool IsARError(this SessionStatus status)
        {
            if (EazyARCoreInterface.isSimulated)
            {
                return false;
            }
            else
            {
                return Session.Status.IsError();
            }
        }
    }
}

