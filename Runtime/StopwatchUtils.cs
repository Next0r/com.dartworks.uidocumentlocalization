using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIDocumentLocalization
{
    static class StopwatchUtils
    {
        public static float GetElapsedSeconds(this System.Diagnostics.Stopwatch stopwatch)
        {
            return (float)stopwatch.ElapsedTicks / System.Diagnostics.Stopwatch.Frequency;
        }
    }
}
