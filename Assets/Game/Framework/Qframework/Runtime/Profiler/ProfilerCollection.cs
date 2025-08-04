using Unity.Profiling;

namespace QFramework.Profiler
{
#if QF_PROFILER
    public class ProfilerCollection
    {
        private static readonly ProfilerCategory MyProfilerCategory = ProfilerCategory.Scripts;

        public static readonly ProfilerCounterValue<int> CommandCount =
 new ProfilerCounterValue<int>(MyProfilerCategory,
            "Command Count",ProfilerMarkerDataUnit.Count,
            ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);

        public static readonly ProfilerCounterValue<int> QueryCount = new ProfilerCounterValue<int>(MyProfilerCategory,
            "Query Count",ProfilerMarkerDataUnit.Count,
            ProfilerCounterOptions.FlushOnEndOfFrame | ProfilerCounterOptions.ResetToZeroOnFlush);
    }
#endif
}