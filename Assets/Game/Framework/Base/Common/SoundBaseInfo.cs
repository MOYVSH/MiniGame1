using System;

namespace MOYV
{
    [Serializable]
    public class SoundBaseInfo
    {
        /// <summary>
        /// Key
        /// </summary>
        public string Key;

        /// <summary>
        /// 音频类型
        /// </summary>
        public string Category;

        /// <summary>
        /// 音频的路径及名称(多个随机用冒号分隔,Unity不带扩展名)
        /// </summary>
        public string Name;

        /// <summary>
        /// 播放模式
        /// </summary>
        public string PlayMode;

        /// <summary>
        /// 音频长度（秒）
        /// </summary>
        public float Duration;

        /// <summary>
        /// 延时播放（秒）
        /// </summary>
        public float Delay;

        /// <summary>
        /// 是否循环播放
        /// </summary>
        public bool IsLoop;

        /// <summary>
        /// 互斥(多个用逗号分隔)
        /// </summary>
        public string MutexSoundID;

        /// <summary>
        /// 打断
        /// </summary>
        public string MutexAndStopSoundID;

        /// <summary>
        /// 音量大小（0 ~ 1）
        /// </summary>
        public float Volume;

        /// <summary>
        /// 渐入曲线
        /// </summary>
        public int FadeInCurveId;

        /// <summary>
        /// 渐出曲线
        /// </summary>
        public int FadeOutCurveId;

        /// <summary>
        /// 渐入时间
        /// </summary>
        public float FadeInTime;

        /// <summary>
        /// 渐出时间
        /// </summary>
        public float FadeOutTime;

        /// <summary>
        /// 音调最小
        /// </summary>
        public float PitchMin;

        /// <summary>
        /// 音调最大
        /// </summary>
        public float PitchMax;
    }
}