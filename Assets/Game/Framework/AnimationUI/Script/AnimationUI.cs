using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnimationUI
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class AnimationUI : MonoBehaviour
    {
        enum EPlayState
        {
            None,
            Forward,
            Backwards,
            Loop,
            LoopPause
        }

        [HideInInspector] public float TotalDuration = 0; //Value automatically taken care of by AnimationUIInspector
        public Sequence[] AnimationSequence;
        [HideInInspector] public bool PlayOnStart = false;
        [HideInInspector] public bool Loop = false;
        [HideInInspector] public float CurrentTime = 0; // Don't forget this variable might be in build
        [HideInInspector] public bool IsPlayingInEditMode = false;
        private bool initialized;

        // private bool isPause = true;

        // private bool isForward;
        private EPlayState playState;

        public bool IsPlaying
        {
            get { return playState != EPlayState.None; }
        }

        void Start()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
                foreach (Sequence sequence in AnimationSequence)
                    sequence.Init();

#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
                if (PlayOnStart)
                    Play();
        }

        void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update += EditorUpdate;
#endif
#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
                if (Loop && playState == EPlayState.LoopPause)
                {
                    playState = EPlayState.None;
                    Play();
                }
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= EditorUpdate;
#endif
#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
                if (playState != EPlayState.Loop)
                    playState = EPlayState.None;
                else
                    playState = EPlayState.LoopPause;
        }


        public void Play()
        {
            if (Loop)
                PlayForwardLoop();
            else
                PlayForward();
        }

        public void Pause()
        {
            playState = EPlayState.None;
        }

        public void PlayForwardLoop()
        {
            if (!Loop || playState == EPlayState.Loop || playState == EPlayState.LoopPause || !isActiveAndEnabled)
                return;
            RuntimeInitFunction();
            playState = EPlayState.Loop;
            PlayLoopAnimation();
        }

        public void PlayForward()
        {
            if (playState == EPlayState.Forward || !isActiveAndEnabled)
                return;
            RuntimeInitFunction();
            playState = EPlayState.Forward;
            PlayForwardAnimation();
        }

        public void PlayBackwards()
        {
            if (playState == EPlayState.Backwards || !isActiveAndEnabled)
                return;
            RuntimeInitFunction();
            playState = EPlayState.Backwards;
            PlayBackwardsAnimation();
        }

        public void Restart()
        {
            RuntimeInitFunction();
            if (UpdateSequence == null)
            {
                Debug.Log("No animation exist");
                return;
            }

            CurrentTime = 0;
            UpdateSequence(0);
            Pause();
        }

        public void Complete()
        {
            CurrentTime = TotalDuration;
            RuntimeInitFunction();
            if (UpdateSequence == null)
            {
                Debug.Log("No animation exist");
                return;
            }

            CurrentTime = TotalDuration;
            Delegate[] delegates = UpdateSequence.GetInvocationList();
            // 反序执行事件方法
            for (int i = delegates.Length - 1; i >= 0; i--)
            {
                Animation handler = (Animation)delegates[i];
                handler?.Invoke(CurrentTime);
            }

            // UpdateSequence(CurrentTime);
            Pause();
        }

        async UniTaskVoid PlayLoopAnimation()
        {
            bool IsRuning()
            {
                return Loop && playState == EPlayState.Loop && isActiveAndEnabled;
            }

            while (IsRuning())
            {
                UpdateSequence(CurrentTime);
                if (CurrentTime >= TotalDuration)
                    CurrentTime = 0;
                else
                {
                    CurrentTime += Time.deltaTime;
                    CurrentTime = Mathf.Min(TotalDuration, CurrentTime);
                }

                await UniTask.NextFrame();
            }
        }

        void RuntimeInitFunction()
        {
            if (!initialized)
            {
                InitFunction();
                initialized = true;
            }
        }

        async UniTaskVoid PlayForwardAnimation()
        {
            while (playState == EPlayState.Forward && isActiveAndEnabled)
            {
                UpdateSequence(CurrentTime);
                if (CurrentTime >= TotalDuration)
                {
                    playState = EPlayState.None;
                    break;
                }

                CurrentTime += Time.deltaTime;
                CurrentTime = Mathf.Min(TotalDuration, CurrentTime);
                await UniTask.NextFrame();
            }
        }

        async UniTaskVoid PlayBackwardsAnimation()
        {
            Delegate[] delegates = UpdateSequence.GetInvocationList();
            while (playState == EPlayState.Backwards && isActiveAndEnabled)
            {
                // 反序执行事件方法
                for (int i = delegates.Length - 1; i >= 0; i--)
                {
                    Animation handler = (Animation)delegates[i];
                    handler?.Invoke(CurrentTime);
                }

                // UpdateSequence(CurrentTime);
                if (CurrentTime <= 0)
                {
                    playState = EPlayState.None;
                    break;
                }

                CurrentTime -= Time.deltaTime;
                CurrentTime = Mathf.Max(0, CurrentTime);
                await UniTask.NextFrame();
            }
        }


        #region Tasks

        #region RectTransform

        IEnumerator TaskAnchoredPosition(RectTransform rt, Vector3 start, Vector3 end, float duration, Ease.Function easeFunction)
        {
            float startTime = Time.time;
            float t = (Time.time - startTime) / duration;
            while (t <= 1)
            {
                t = Mathf.Clamp((Time.time - startTime) / duration, 0, 2);
                rt.anchoredPosition = Vector3.LerpUnclamped(start, end, easeFunction(t));
                yield return null;
            }

            rt.anchoredPosition = end;
        }

        IEnumerator TaskLocalScale(RectTransform rt, Vector3 start, Vector3 end, float duration, Ease.Function easeFunction)
        {
            float startTime = Time.time;
            float t = (Time.time - startTime) / duration;
            while (t <= 1)
            {
                t = Mathf.Clamp((Time.time - startTime) / duration, 0, 2);
                rt.localScale = Vector3.LerpUnclamped(start, end, easeFunction(t));
                yield return null;
            }

            rt.localScale = end;
        }

        IEnumerator TaskLocalEulerAngles(RectTransform rt, Vector3 start, Vector3 end, float duration, Ease.Function easeFunction)
        {
            float startTime = Time.time;
            float t = (Time.time - startTime) / duration;
            while (t <= 1)
            {
                t = Mathf.Clamp((Time.time - startTime) / duration, 0, 2);
                rt.localEulerAngles = Vector3.LerpUnclamped(start, end, easeFunction(t));
                yield return null;
            }

            rt.localEulerAngles = end;
        }

        IEnumerator TaskAnchorMax(RectTransform rt, Vector3 start, Vector3 end, float duration, Ease.Function easeFunction)
        {
            float startTime = Time.time;
            float t = (Time.time - startTime) / duration;
            while (t <= 1)
            {
                t = Mathf.Clamp((Time.time - startTime) / duration, 0, 2);
                rt.anchorMax = Vector3.LerpUnclamped(start, end, easeFunction(t));
                yield return null;
            }

            rt.anchorMax = end;
        }

        IEnumerator TaskAnchorMin(RectTransform rt, Vector3 start, Vector3 end, float duration, Ease.Function easeFunction)
        {
            float startTime = Time.time;
            float t = (Time.time - startTime) / duration;
            while (t <= 1)
            {
                t = Mathf.Clamp((Time.time - startTime) / duration, 0, 2);
                rt.anchorMin = Vector3.LerpUnclamped(start, end, easeFunction(t));
                yield return null;
            }

            rt.anchorMin = end;
        }

        IEnumerator TaskSizeDelta(RectTransform rt, Vector3 start, Vector3 end, float duration, Ease.Function easeFunction)
        {
            float startTime = Time.time;
            float t = (Time.time - startTime) / duration;
            while (t <= 1)
            {
                t = Mathf.Clamp((Time.time - startTime) / duration, 0, 2);
                rt.sizeDelta = Vector3.LerpUnclamped(start, end, easeFunction(t));
                yield return null;
            }

            rt.sizeDelta = end;
        }

        IEnumerator TaskPivot(RectTransform rt, Vector3 start, Vector3 end, float duration, Ease.Function easeFunction)
        {
            float startTime = Time.time;
            float t = (Time.time - startTime) / duration;
            while (t <= 1)
            {
                t = Mathf.Clamp((Time.time - startTime) / duration, 0, 2);
                rt.pivot = Vector3.LerpUnclamped(start, end, easeFunction(t));
                yield return null;
            }

            rt.pivot = end;
        }

        #endregion RectTransform

        #region TransformTask

        IEnumerator TaskLocalPosition(Transform trans, Vector3 start, Vector3 end, float duration, Ease.Function easeFunction)
        {
            float startTime = Time.time;
            float t = (Time.time - startTime) / duration;
            while (t <= 1)
            {
                t = Mathf.Clamp((Time.time - startTime) / duration, 0, 2);
                trans.localPosition = Vector3.LerpUnclamped(start, end, easeFunction(t));
                yield return null;
            }

            trans.localPosition = end;
        }

        IEnumerator TaskLocalScale(Transform trans, Vector3 start, Vector3 end, float duration, Ease.Function easeFunction)
        {
            float startTime = Time.time;
            float t = (Time.time - startTime) / duration;
            while (t <= 1)
            {
                t = Mathf.Clamp((Time.time - startTime) / duration, 0, 2);
                trans.localScale = Vector3.LerpUnclamped(start, end, easeFunction(t));
                yield return null;
            }

            trans.localScale = end;
        }

        IEnumerator TaskLocalEulerAngles(Transform trans, Vector3 start, Vector3 end, float duration, Ease.Function easeFunction)
        {
            float startTime = Time.time;
            float t = (Time.time - startTime) / duration;
            while (t <= 1)
            {
                t = Mathf.Clamp((Time.time - startTime) / duration, 0, 2);
                trans.localEulerAngles = Vector3.LerpUnclamped(start, end, easeFunction(t));
                yield return null;
            }

            trans.localEulerAngles = end;
        }

        #endregion TransformTask

        #region ImageTask

        IEnumerator TaskColor(Image img, Color start, Color end, float duration, Ease.Function easeFunction)
        {
            float startTime = Time.time;
            float t = (Time.time - startTime) / duration;
            while (t <= 1)
            {
                t = Mathf.Clamp((Time.time - startTime) / duration, 0, 2);
                img.color = Color.LerpUnclamped(start, end, easeFunction(t));
                yield return null;
            }

            img.color = end;
        }

        IEnumerator TaskFillAmount(Image img, float start, float end, float duration, Ease.Function easeFunction)
        {
            float startTime = Time.time;
            float t = (Time.time - startTime) / duration;
            while (t <= 1)
            {
                t = Mathf.Clamp((Time.time - startTime) / duration, 0, 2);
                img.fillAmount = Mathf.LerpUnclamped(start, end, easeFunction(t));
                yield return null;
            }

            img.fillAmount = end;
        }

        #endregion ImageTask

        #region CanvasGroupTask

        IEnumerator TaskAlpha(CanvasGroup cg, float start, float end, float duration, Ease.Function easeFunction)
        {
            float startTime = Time.time;
            float t = (Time.time - startTime) / duration;
            while (t <= 1)
            {
                t = Mathf.Clamp((Time.time - startTime) / duration, 0, 2);
                cg.alpha = Mathf.LerpUnclamped(start, end, easeFunction(t));
                yield return null;
            }

            cg.alpha = end;
        }

        #endregion CanvasGroupTask

        #region CameraTask

        IEnumerator TaskBackgroundColor(Camera cam, Color start, Color end, float duration, Ease.Function easeFunction)
        {
            float startTime = Time.time;
            float t = (Time.time - startTime) / duration;
            while (t <= 1)
            {
                t = Mathf.Clamp((Time.time - startTime) / duration, 0, 2);
                cam.backgroundColor = Color.LerpUnclamped(start, end, easeFunction(t));
                yield return null;
            }

            cam.backgroundColor = end;
        }

        IEnumerator TaskOrthographicSize(Camera cam, float start, float end, float duration, Ease.Function easeFunction)
        {
            float startTime = Time.time;
            float t = (Time.time - startTime) / duration;
            while (t <= 1)
            {
                t = Mathf.Clamp((Time.time - startTime) / duration, 0, 2);
                cam.orthographicSize = Mathf.LerpUnclamped(start, end, easeFunction(t));
                yield return null;
            }

            cam.orthographicSize = end;
        }

        #endregion ImageTask

        #region TextMeshProTask

        IEnumerator TaskTextMeshProColor(TMP_Text text, Color start, Color end, float duration, Ease.Function easeFunction)
        {
            float startTime = Time.time;
            float t = (Time.time - startTime) / duration;
            while (t <= 1)
            {
                t = Mathf.Clamp((Time.time - startTime) / duration, 0, 2);
                text.color = Color.LerpUnclamped(start, end, easeFunction(t));
                yield return null;
            }

            text.color = end;
        }

        IEnumerator TaskMaxVisibleCharacters(TMP_Text text, float start, float end, float duration, Ease.Function easeFunction)
        {
            float startTime = Time.time;
            float t = (Time.time - startTime) / duration;
            while (t <= 1)
            {
                t = Mathf.Clamp((Time.time - startTime) / duration, 0, 2);
                text.maxVisibleCharacters = (int)Mathf.LerpUnclamped(start, end, easeFunction(t));
                yield return null;
            }

            text.maxVisibleCharacters = (int)end;
        }

        #endregion ImageTask

        #endregion Tasks

        #region Event

        public delegate void AnimationUIEvent();

        AnimationUIEvent atEndEvents;
        List<AnimationUIEvent> atTimeEvents = new List<AnimationUIEvent>();
        List<float> atTimes = new List<float>();

        IEnumerator AtTimeEvent(AnimationUIEvent atTimeEvent, float time)
        {
            yield return new WaitForSecondsRealtime(time);
            atTimeEvent();
        }

        public AnimationUI AddFunctionAt(float time, AnimationUIEvent func)
        {
            atTimes.Add(time);
            atTimeEvents.Add(func);
            return this;
        }

        public AnimationUI AddFunctionAtEnd(AnimationUIEvent func)
        {
            atEndEvents += func;
            return this;
        }

        #endregion Event

        public delegate void Animation(float t);

        public Animation UpdateSequence;

        void InitFunction() //For preview
        {
            UpdateSequence = null;
            for (int i = AnimationSequence.Length - 1; i >= 0; i--)
            {
                var sequence = AnimationSequence[i];
                // sequence.IsDone = false;
                if (sequence.SequenceType == Sequence.Type.Animation)
                {
                    if (sequence.TargetComp == null)
                    {
                        Debug.Log("Please assign Target");
                        return;
                    }

                    sequence.Init();
                    if (sequence.TargetType == Sequence.ObjectType.RectTransform)
                    {
                        RectTransform rt = sequence.TargetComp.GetComponent<RectTransform>();

                        void RtAnchoredPosition(float t)
                        {
                            if ((0 <= t - sequence.StartTime) && (t - sequence.StartTime < sequence.Duration))
                            {
                                sequence.AnchoredPositionState = Sequence.State.During;
                                rt.anchoredPosition
                                    = Vector3.LerpUnclamped(sequence.AnchoredPositionStart, sequence.AnchoredPositionEnd,
                                        sequence.EaseFunction(Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration)));
                            }

                            if ((t - sequence.StartTime >= sequence.Duration) &&
                                (sequence.AnchoredPositionState == Sequence.State.During ||
                                 sequence.AnchoredPositionState == Sequence.State.Before))
                            {
                                rt.anchoredPosition = sequence.AnchoredPositionEnd;
                                sequence.AnchoredPositionState = Sequence.State.After;
                            }
                            else if ((t - sequence.StartTime < 0) &&
                                     (sequence.AnchoredPositionState == Sequence.State.During ||
                                      sequence.AnchoredPositionState == Sequence.State.After))
                            {
                                rt.anchoredPosition = sequence.AnchoredPositionStart;
                                sequence.AnchoredPositionState = Sequence.State.Before;
                            }
                        }

                        void RtLocalEulerAngles(float t)
                        {
                            if ((0 <= t - sequence.StartTime) && (t - sequence.StartTime < sequence.Duration))
                            {
                                sequence.LocalEulerAnglesState = Sequence.State.During;
                                rt.localEulerAngles
                                    = Vector3.LerpUnclamped(sequence.LocalEulerAnglesStart, sequence.LocalEulerAnglesEnd,
                                        sequence.EaseFunction(Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration)));
                            }

                            if ((t - sequence.StartTime >= sequence.Duration) &&
                                (sequence.LocalEulerAnglesState == Sequence.State.During ||
                                 sequence.LocalEulerAnglesState == Sequence.State.Before))
                            {
                                rt.localEulerAngles = sequence.LocalEulerAnglesEnd;
                                sequence.LocalEulerAnglesState = Sequence.State.After;
                            }
                            else if ((t - sequence.StartTime < 0) &&
                                     (sequence.LocalEulerAnglesState == Sequence.State.During ||
                                      sequence.LocalEulerAnglesState == Sequence.State.After))
                            {
                                rt.localEulerAngles = sequence.LocalEulerAnglesStart;
                                sequence.LocalEulerAnglesState = Sequence.State.Before;
                            }
                        }

                        void RtLocalScale(float t)
                        {
                            if ((0 <= t - sequence.StartTime) && (t - sequence.StartTime < sequence.Duration))
                            {
                                sequence.LocalScaleState = Sequence.State.During;
                                rt.localScale
                                    = Vector3.LerpUnclamped(sequence.LocalScaleStart, sequence.LocalScaleEnd,
                                        sequence.EaseFunction(Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration)));
                            }

                            if ((t - sequence.StartTime >= sequence.Duration) &&
                                (sequence.LocalScaleState == Sequence.State.During ||
                                 sequence.LocalScaleState == Sequence.State.Before))
                            {
                                rt.localScale = sequence.LocalScaleEnd;
                                sequence.LocalScaleState = Sequence.State.After;
                            }
                            else if ((t - sequence.StartTime < 0) &&
                                     (sequence.LocalScaleState == Sequence.State.During ||
                                      sequence.LocalScaleState == Sequence.State.After))
                            {
                                rt.localScale = sequence.LocalScaleStart;
                                sequence.LocalScaleState = Sequence.State.Before;
                            }
                        }

                        void RtSizeDelta(float t)
                        {
                            if ((0 <= t - sequence.StartTime) && (t - sequence.StartTime < sequence.Duration))
                            {
                                sequence.SizeDeltaState = Sequence.State.During;
                                rt.sizeDelta
                                    = Vector3.LerpUnclamped(sequence.SizeDeltaStart, sequence.SizeDeltaEnd,
                                        sequence.EaseFunction(Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration)));
                            }

                            if ((t - sequence.StartTime >= sequence.Duration) &&
                                (sequence.SizeDeltaState == Sequence.State.During ||
                                 sequence.SizeDeltaState == Sequence.State.Before))
                            {
                                rt.sizeDelta = sequence.SizeDeltaEnd;
                                sequence.SizeDeltaState = Sequence.State.After;
                            }
                            else if ((t - sequence.StartTime < 0) &&
                                     (sequence.SizeDeltaState == Sequence.State.During ||
                                      sequence.SizeDeltaState == Sequence.State.After))
                            {
                                rt.sizeDelta = sequence.SizeDeltaStart;
                                sequence.SizeDeltaState = Sequence.State.Before;
                            }
                        }

                        void RtAnchorMin(float t)
                        {
                            if ((0 <= t - sequence.StartTime) && (t - sequence.StartTime < sequence.Duration))
                            {
                                sequence.AnchorMinState = Sequence.State.During;
                                rt.anchorMin
                                    = Vector3.LerpUnclamped(sequence.AnchorMinStart, sequence.AnchorMinEnd,
                                        sequence.EaseFunction(Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration)));
                            }

                            if ((t - sequence.StartTime >= sequence.Duration) &&
                                (sequence.AnchorMinState == Sequence.State.During ||
                                 sequence.AnchorMinState == Sequence.State.Before))
                            {
                                rt.anchorMin = sequence.AnchorMinEnd;
                                sequence.AnchorMinState = Sequence.State.After;
                            }
                            else if ((t - sequence.StartTime < 0) &&
                                     (sequence.AnchorMinState == Sequence.State.During ||
                                      sequence.AnchorMinState == Sequence.State.After))
                            {
                                rt.anchorMin = sequence.AnchorMinStart;
                                sequence.AnchorMinState = Sequence.State.Before;
                            }
                        }

                        void RtAnchorMax(float t)
                        {
                            if ((0 <= t - sequence.StartTime) && (t - sequence.StartTime < sequence.Duration))
                            {
                                sequence.AnchorMaxState = Sequence.State.During;
                                rt.anchorMax
                                    = Vector3.LerpUnclamped(sequence.AnchorMaxStart, sequence.AnchorMaxEnd,
                                        sequence.EaseFunction(Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration)));
                            }

                            if ((t - sequence.StartTime >= sequence.Duration) &&
                                (sequence.AnchorMaxState == Sequence.State.During ||
                                 sequence.AnchorMaxState == Sequence.State.Before))
                            {
                                rt.anchorMax = sequence.AnchorMaxEnd;
                                sequence.AnchorMaxState = Sequence.State.After;
                            }
                            else if ((t - sequence.StartTime < 0) &&
                                     (sequence.AnchorMaxState == Sequence.State.During ||
                                      sequence.AnchorMaxState == Sequence.State.After))
                            {
                                rt.anchorMax = sequence.AnchorMaxStart;
                                sequence.AnchorMaxState = Sequence.State.Before;
                            }
                        }

                        void RtPivot(float t)
                        {
                            if ((0 <= t - sequence.StartTime) && (t - sequence.StartTime < sequence.Duration))
                            {
                                sequence.PivotState = Sequence.State.During;
                                rt.pivot
                                    = Vector3.LerpUnclamped(sequence.PivotStart, sequence.PivotEnd,
                                        sequence.EaseFunction(Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration)));
                            }

                            if ((t - sequence.StartTime >= sequence.Duration) &&
                                (sequence.PivotState == Sequence.State.During ||
                                 sequence.PivotState == Sequence.State.Before))
                            {
                                rt.pivot = sequence.PivotEnd;
                                sequence.PivotState = Sequence.State.After;
                            }
                            else if ((t - sequence.StartTime < 0) &&
                                     (sequence.PivotState == Sequence.State.During ||
                                      sequence.PivotState == Sequence.State.After))
                            {
                                rt.pivot = sequence.PivotStart;
                                sequence.PivotState = Sequence.State.Before;
                            }
                        }


                        if (sequence.TargetRtTask.HasFlag(Sequence.RtTask.AnchoredPosition))
                            UpdateSequence += RtAnchoredPosition;
                        if (sequence.TargetRtTask.HasFlag(Sequence.RtTask.LocalEulerAngles))
                            UpdateSequence += RtLocalEulerAngles;
                        if (sequence.TargetRtTask.HasFlag(Sequence.RtTask.LocalScale))
                            UpdateSequence += RtLocalScale;
                        if (sequence.TargetRtTask.HasFlag(Sequence.RtTask.SizeDelta))
                            UpdateSequence += RtSizeDelta;
                        if (sequence.TargetRtTask.HasFlag(Sequence.RtTask.AnchorMax))
                            UpdateSequence += RtAnchorMax;
                        if (sequence.TargetRtTask.HasFlag(Sequence.RtTask.AnchorMin))
                            UpdateSequence += RtAnchorMin;
                        if (sequence.TargetRtTask.HasFlag(Sequence.RtTask.Pivot))
                            UpdateSequence += RtPivot;
                    }
                    else if (sequence.TargetType == Sequence.ObjectType.Transform)
                    {
                        Transform trans = sequence.TargetComp.transform;

                        void TransLocalPosition(float t)
                        {
                            if ((0 <= t - sequence.StartTime) && (t - sequence.StartTime < sequence.Duration))
                            {
                                sequence.LocalPositionState = Sequence.State.During;
                                trans.localPosition
                                    = Vector3.LerpUnclamped(sequence.LocalPositionStart, sequence.LocalPositionEnd,
                                        sequence.EaseFunction(Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration)));
                            }

                            if ((t - sequence.StartTime >= sequence.Duration) &&
                                (sequence.LocalPositionState == Sequence.State.During ||
                                 sequence.LocalPositionState == Sequence.State.Before))
                            {
                                trans.localPosition = sequence.LocalPositionEnd;
                                sequence.LocalPositionState = Sequence.State.After;
                            }
                            else if ((t - sequence.StartTime < 0) &&
                                     (sequence.LocalPositionState == Sequence.State.During ||
                                      sequence.LocalPositionState == Sequence.State.After))
                            {
                                trans.localPosition = sequence.LocalPositionStart;
                                sequence.LocalPositionState = Sequence.State.Before;
                            }
                        }

                        void TransLocalEulerAngles(float t)
                        {
                            if ((0 <= t - sequence.StartTime) && (t - sequence.StartTime < sequence.Duration))
                            {
                                sequence.LocalEulerAnglesState = Sequence.State.During;
                                trans.localEulerAngles
                                    = Vector3.LerpUnclamped(sequence.LocalEulerAnglesStart, sequence.LocalEulerAnglesEnd,
                                        sequence.EaseFunction(Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration)));
                            }

                            if ((t - sequence.StartTime >= sequence.Duration) &&
                                (sequence.LocalEulerAnglesState == Sequence.State.During ||
                                 sequence.LocalEulerAnglesState == Sequence.State.Before))
                            {
                                trans.localEulerAngles = sequence.LocalEulerAnglesEnd;
                                sequence.LocalEulerAnglesState = Sequence.State.After;
                            }
                            else if ((t - sequence.StartTime < 0) &&
                                     (sequence.LocalEulerAnglesState == Sequence.State.During ||
                                      sequence.LocalEulerAnglesState == Sequence.State.After))
                            {
                                trans.localEulerAngles = sequence.LocalEulerAnglesStart;
                                sequence.LocalEulerAnglesState = Sequence.State.Before;
                            }
                        }

                        void TransLocalScale(float t)
                        {
                            if ((0 <= t - sequence.StartTime) && (t - sequence.StartTime < sequence.Duration))
                            {
                                sequence.LocalScaleState = Sequence.State.During;
                                trans.localScale
                                    = Vector3.LerpUnclamped(sequence.LocalScaleStart, sequence.LocalScaleEnd,
                                        sequence.EaseFunction(Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration)));
                            }

                            if ((t - sequence.StartTime >= sequence.Duration) &&
                                (sequence.LocalScaleState == Sequence.State.During ||
                                 sequence.LocalScaleState == Sequence.State.Before))
                            {
                                trans.localScale = sequence.LocalScaleEnd;
                                sequence.LocalScaleState = Sequence.State.After;
                            }
                            else if ((t - sequence.StartTime < 0) &&
                                     (sequence.LocalScaleState == Sequence.State.During ||
                                      sequence.LocalScaleState == Sequence.State.After))
                            {
                                trans.localScale = sequence.LocalScaleStart;
                                sequence.LocalScaleState = Sequence.State.Before;
                            }
                        }

                        if (sequence.TargetTransTask.HasFlag(Sequence.TransTask.LocalPosition))
                            UpdateSequence += TransLocalPosition;
                        if (sequence.TargetTransTask.HasFlag(Sequence.TransTask.LocalEulerAngles))
                            UpdateSequence += TransLocalEulerAngles;
                        if (sequence.TargetTransTask.HasFlag(Sequence.TransTask.LocalScale))
                            UpdateSequence += TransLocalScale;
                    }
                    else if (sequence.TargetType == Sequence.ObjectType.Image)
                    {
                        Image img = sequence.TargetComp.GetComponent<Image>();

                        void ImgColor(float t)
                        {
                            if ((0 <= t - sequence.StartTime) && (t - sequence.StartTime < sequence.Duration))
                            {
                                sequence.ColorState = Sequence.State.During;
                                img.color
                                    = Color.LerpUnclamped(sequence.ColorStart, sequence.ColorEnd,
                                        sequence.EaseFunction(Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration)));
                            }

                            if ((t - sequence.StartTime >= sequence.Duration) &&
                                (sequence.ColorState == Sequence.State.During ||
                                 sequence.ColorState == Sequence.State.Before))
                            {
                                img.color = sequence.ColorEnd;
                                sequence.ColorState = Sequence.State.After;
                            }
                            else if ((t - sequence.StartTime < 0) &&
                                     (sequence.ColorState == Sequence.State.During ||
                                      sequence.ColorState == Sequence.State.After))
                            {
                                img.color = sequence.ColorStart;
                                sequence.ColorState = Sequence.State.Before;
                            }
                        }

                        void ImgFillAmount(float t)
                        {
                            if ((0 <= t - sequence.StartTime) && (t - sequence.StartTime < sequence.Duration))
                            {
                                sequence.ColorState = Sequence.State.During;
                                img.fillAmount
                                    = Mathf.LerpUnclamped(sequence.FillAmountStart, sequence.FillAmountEnd,
                                        sequence.EaseFunction(Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration)));
                            }

                            if ((t - sequence.StartTime >= sequence.Duration) &&
                                (sequence.ColorState == Sequence.State.During ||
                                 sequence.ColorState == Sequence.State.Before))
                            {
                                img.fillAmount = sequence.FillAmountEnd;
                                sequence.ColorState = Sequence.State.After;
                            }
                            else if ((t - sequence.StartTime < 0) &&
                                     (sequence.ColorState == Sequence.State.During ||
                                      sequence.ColorState == Sequence.State.After))
                            {
                                img.fillAmount = sequence.FillAmountStart;
                                sequence.ColorState = Sequence.State.Before;
                            }
                        }

                        if (sequence.TargetImgTask.HasFlag(Sequence.ImgTask.Color))
                            UpdateSequence += ImgColor;
                        if (sequence.TargetImgTask.HasFlag(Sequence.ImgTask.FillAmount))
                            UpdateSequence += ImgFillAmount;
                    }
                    else if (sequence.TargetType == Sequence.ObjectType.CanvasGroup)
                    {
                        CanvasGroup cg = sequence.TargetComp.GetComponent<CanvasGroup>();

                        void CgAlpha(float t)
                        {
                            if ((0 <= t - sequence.StartTime) && (t - sequence.StartTime < sequence.Duration))
                            {
                                sequence.AlphaState = Sequence.State.During;
                                cg.alpha
                                    = Mathf.LerpUnclamped(sequence.AlphaStart, sequence.AlphaEnd,
                                        sequence.EaseFunction(Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration)));
                            }

                            if ((t - sequence.StartTime >= sequence.Duration) &&
                                (sequence.AlphaState == Sequence.State.During ||
                                 sequence.AlphaState == Sequence.State.Before))
                            {
                                cg.alpha = sequence.AlphaEnd;
                                sequence.AlphaState = Sequence.State.After;
                            }
                            else if ((t - sequence.StartTime < 0) &&
                                     (sequence.AlphaState == Sequence.State.During ||
                                      sequence.AlphaState == Sequence.State.After))
                            {
                                cg.alpha = sequence.AlphaStart;
                                sequence.AlphaState = Sequence.State.Before;
                            }
                        }

                        if (sequence.TargetCgTask.HasFlag(Sequence.CgTask.Alpha))
                            UpdateSequence += CgAlpha;
                    }
                    else if (sequence.TargetType == Sequence.ObjectType.Camera)
                    {
                        Camera cam = sequence.TargetComp.GetComponent<Camera>();

                        void CamBackgroundColor(float t)
                        {
                            if ((0 <= t - sequence.StartTime) && (t - sequence.StartTime < sequence.Duration))
                            {
                                sequence.BackgroundColorState = Sequence.State.During;
                                cam.backgroundColor
                                    = Color.LerpUnclamped(sequence.BackgroundColorStart, sequence.BackgroundColorEnd,
                                        sequence.EaseFunction(Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration)));
                            }

                            if ((t - sequence.StartTime >= sequence.Duration) &&
                                (sequence.BackgroundColorState == Sequence.State.During ||
                                 sequence.BackgroundColorState == Sequence.State.Before))
                            {
                                cam.backgroundColor = sequence.BackgroundColorEnd;
                                sequence.BackgroundColorState = Sequence.State.After;
                            }
                            else if ((t - sequence.StartTime < 0) &&
                                     (sequence.BackgroundColorState == Sequence.State.During ||
                                      sequence.BackgroundColorState == Sequence.State.After))
                            {
                                cam.backgroundColor = sequence.BackgroundColorStart;
                                sequence.BackgroundColorState = Sequence.State.Before;
                            }
                        }

                        void CamOrthographicSize(float t)
                        {
                            if ((0 <= t - sequence.StartTime) && (t - sequence.StartTime < sequence.Duration))
                            {
                                sequence.OrthographicSizeState = Sequence.State.During;
                                cam.orthographicSize
                                    = Mathf.LerpUnclamped(sequence.OrthographicSizeStart, sequence.OrthographicSizeEnd,
                                        sequence.EaseFunction(Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration)));
                            }

                            if ((t - sequence.StartTime >= sequence.Duration) &&
                                (sequence.OrthographicSizeState == Sequence.State.During ||
                                 sequence.OrthographicSizeState == Sequence.State.Before))
                            {
                                cam.orthographicSize = sequence.OrthographicSizeEnd;
                                sequence.OrthographicSizeState = Sequence.State.After;
                            }
                            else if ((t - sequence.StartTime < 0) &&
                                     (sequence.OrthographicSizeState == Sequence.State.During ||
                                      sequence.OrthographicSizeState == Sequence.State.After))
                            {
                                cam.orthographicSize = sequence.OrthographicSizeStart;
                                sequence.OrthographicSizeState = Sequence.State.Before;
                            }
                        }

                        if (sequence.TargetCamTask.HasFlag(Sequence.CamTask.BackgroundColor))
                            UpdateSequence += CamBackgroundColor;
                        if (sequence.TargetCamTask.HasFlag(Sequence.CamTask.OrthographicSize))
                            UpdateSequence += CamOrthographicSize;
                    }
                    else if (sequence.TargetType == Sequence.ObjectType.TextMeshPro)
                    {
                        TMP_Text text = sequence.TargetComp.GetComponent<TMP_Text>();

                        void TextMeshProColor(float t)
                        {
                            if ((0 <= t - sequence.StartTime) && (t - sequence.StartTime < sequence.Duration))
                            {
                                sequence.TextMeshProColorState = Sequence.State.During;
                                text.color
                                    = Color.LerpUnclamped(sequence.TextMeshProColorStart, sequence.TextMeshProColorEnd,
                                        sequence.EaseFunction(Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration)));
                            }

                            if ((t - sequence.StartTime >= sequence.Duration) &&
                                (sequence.TextMeshProColorState == Sequence.State.During ||
                                 sequence.TextMeshProColorState == Sequence.State.Before))
                            {
                                text.color = sequence.TextMeshProColorEnd;
                                sequence.TextMeshProColorState = Sequence.State.After;
                            }
                            else if ((t - sequence.StartTime < 0) &&
                                     (sequence.TextMeshProColorState == Sequence.State.During ||
                                      sequence.TextMeshProColorState == Sequence.State.After))
                            {
                                text.color = sequence.TextMeshProColorStart;
                                sequence.TextMeshProColorState = Sequence.State.Before;
                            }
                        }

                        void MaxVisibleCharacters(float t)
                        {
                            if ((0 <= t - sequence.StartTime) && (t - sequence.StartTime < sequence.Duration))
                            {
                                sequence.MaxVisibleCharactersState = Sequence.State.During;
                                text.maxVisibleCharacters
                                    = (int)Mathf.LerpUnclamped(sequence.MaxVisibleCharactersStart, sequence.MaxVisibleCharactersEnd,
                                        sequence.EaseFunction(Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration)));
                            }

                            if ((t - sequence.StartTime >= sequence.Duration) &&
                                (sequence.MaxVisibleCharactersState == Sequence.State.During ||
                                 sequence.MaxVisibleCharactersState == Sequence.State.Before))
                            {
                                text.maxVisibleCharacters = sequence.MaxVisibleCharactersEnd;
                                sequence.MaxVisibleCharactersState = Sequence.State.After;
                            }
                            else if ((t - sequence.StartTime < 0) &&
                                     (sequence.MaxVisibleCharactersState == Sequence.State.During ||
                                      sequence.MaxVisibleCharactersState == Sequence.State.After))
                            {
                                text.maxVisibleCharacters = sequence.MaxVisibleCharactersStart;
                                sequence.MaxVisibleCharactersState = Sequence.State.Before;
                            }
                        }

                        if (sequence.TargetTextMeshProTask.HasFlag(Sequence.TextMeshProTask.Color))
                            UpdateSequence += TextMeshProColor;
                        if (sequence.TargetTextMeshProTask.HasFlag(Sequence.TextMeshProTask.MaxVisibleCharacters))
                            UpdateSequence += MaxVisibleCharacters;
                    }
                    else if (sequence.TargetType == Sequence.ObjectType.UnityEventDynamic)
                    {
                        Image img = sequence.TargetComp.GetComponent<Image>();

                        void EventDynamic(float t)
                        {
                            if (t - sequence.StartTime < 0) return;
                            sequence.EventDynamic?.Invoke(Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration));
                        }

                        UpdateSequence += EventDynamic;
                    }
                }
                else if (sequence.SequenceType == Sequence.Type.Wait)
                {                    
                    void Wait(float t)
                    {
                        float time = Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration);
                        if (!sequence.IsDone)
                        {
                            if (t - sequence.StartTime > -0.01f)
                            {
                                sequence.IsDone = true;
                            }
                        }
                        else if (t - sequence.StartTime < 0)
                        {
                            sequence.IsDone = false;
                        }
                    }

                    UpdateSequence += Wait;
                }
                else if (sequence.SequenceType == Sequence.Type.SetActiveAllInput)
                {
                    void SetActiveALlInput(float t)
                    {
                        float time = Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration);
                        if (!sequence.IsDone) // so that SetActiveAllInput in the first frame can also be called
                        {
                            if (t - sequence.StartTime > -0.01f)
                            {
                                sequence.IsDone = true;
                                Customizable.SetActiveAllInput(sequence.IsActivating);
                            }
                        }
                        else if (t - sequence.StartTime < 0)
                        {
                            sequence.IsDone = false;
                            Customizable.SetActiveAllInput(!sequence.IsActivating);
                        }
                    }

                    // sequence.IsDone = false;
                    UpdateSequence += SetActiveALlInput;
                }
                else if (sequence.SequenceType == Sequence.Type.SetActive)
                {
                    void SetActive(float t)
                    {
                        float time = Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration);
                        if (sequence.Target == null)
                        {
                            Debug.Log("Please assign Target for Sequence at " + sequence.StartTime.ToString() + "s");
                            return;
                        }

                        if (!sequence.IsDone)
                        {
                            if (t - sequence.StartTime >= sequence.Duration)
                            {
                                sequence.IsDone = true;
                                sequence.Target.SetActive(sequence.IsActivating);
                            }
                        }
                        else if (t - sequence.StartTime < 0) // IsDone == true && t-sequence.StartTime < 0 
                        {
                            sequence.IsDone = false;
                            sequence.Target.SetActive(!sequence.IsActivating);
                        }
                    }

                    UpdateSequence += SetActive;
                }
                else if (sequence.SequenceType == Sequence.Type.SFX)
                {
                    void SFX(float t)
                    {
                        float time = Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration);
                        if (!sequence.IsDone) // so that SetActiveAllInput in the first frame can also be called
                        {
                            if (t - sequence.StartTime > -0.01f)
                            {
                                sequence.IsDone = true;
                                if (sequence.PlaySFXBy == Sequence.SFXMethod.File)
                                {
                                    if (sequence.SFXFile == null)
                                    {
                                        // Debug.LogWarning("Please assign SFX for Sequence at "+sequence.StartTime.ToString()+"s");
                                        return;
                                    }

                                    Customizable.PlaySound(sequence.SFXFile);
                                }
                                else
                                    Customizable.PlaySound(sequence.SFXFile);
                            }
                        }
                        else if (t - sequence.StartTime < 0)
                        {
                            if (sequence.PlaySFXBy == Sequence.SFXMethod.File)
                            {
                                if (sequence.SFXFile == null)
                                {
                                    // Debug.LogWarning("Please assign SFX for Sequence at "+sequence.StartTime.ToString()+"s");
                                    return;
                                }

                                Customizable.PlaySound(sequence.SFXFile);
                            }
                            else
                                Customizable.PlaySound(sequence.SFXFile);

                            sequence.IsDone = false;
                        }
                    }

                    // sequence.IsDone = false;
                    UpdateSequence += SFX;
                }
                else if (sequence.SequenceType == Sequence.Type.UnityEvent)
                {
                    void UnityEvent(float t)
                    {
                        // float time = Mathf.Clamp01((t - sequence.StartTime) / sequence.Duration);
                        if (!sequence.IsDone)
                        {
                            // -0.01f so that SetActiveAllInput in the first frame can also be called
                            if (t - sequence.StartTime > -0.01f) //Nested conditional may actually more performant in this case
                            {
                                sequence.IsDone = true;
                                sequence.Event?.Invoke();
                            }
                        }
                        else if (t - sequence.StartTime < 0)
                        {
                            sequence.IsDone = false;
                            sequence.Event?.Invoke();
                        }
                    }

                    sequence.IsDone = false;
                    UpdateSequence += UnityEvent;
                }
            }
        }


#if UNITY_EDITOR
        float _startTime = 0;

        void ForceRepaint()
        {
            if (!Application.isPlaying)
            {
                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
                UnityEditor.SceneView.RepaintAll();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }

        void OnDrawGizmos()
        {
            ForceRepaint();
        }

        void EditorUpdate()
        {
            if (Application.isPlaying) return;
            if (UpdateSequence == null)
                return;
            ForceRepaint();

            if (IsPlayingInEditMode && CurrentTime < TotalDuration)
            {
                CurrentTime = Mathf.Clamp(Time.time - _startTime, 0, TotalDuration);
                UpdateSequence(CurrentTime);
            }
            else
            {
                if (UpdateSequence != null && IsPlayingInEditMode) UpdateSequence(TotalDuration); //Make sure the latest frame is called
                IsPlayingInEditMode = false;
            }
        }

        void UpdateBySlider()
        {
            if (Application.isPlaying) return;
            if (IsPlayingInEditMode) return;
            InitFunction();
            if (UpdateSequence != null) UpdateSequence(CurrentTime);
        }


        void PreviewAnimation()
        {
            InitFunction();
            if (UpdateSequence == null)
            {
                Debug.Log("No animation exist");
                return;
            }

            _startTime = Time.time;
            CurrentTime = 0;
            IsPlayingInEditMode = true;
            UpdateSequence(0); // Make sure the first frame is called
        }

        void PreviewStart()
        {
            InitFunction();
            if (UpdateSequence == null)
            {
                Debug.Log("No animation exist");
                return;
            }

            CurrentTime = 0;
            IsPlayingInEditMode = false;
            UpdateSequence(0);
        }

        void PreviewEnd()
        {
            CurrentTime = TotalDuration;
            InitFunction();
            if (UpdateSequence == null)
            {
                Debug.Log("No animation exist");
                return;
            }

            IsPlayingInEditMode = false;
            CurrentTime = TotalDuration;
            UpdateSequence(CurrentTime);
        }

        void Reset() //For the default value. A hacky way because the inspector reset the value for Serialized class
        {
            AnimationSequence = new Sequence[1]
            {
                new Sequence()
            };
        }


        #region timing

        void InitTime()
        {
            TotalDuration = 0;
            foreach (Sequence sequence in AnimationSequence)
            {
                //TotalDuration += (sequence.SequenceType == Sequence.Type.Wait) ? sequence.Duration : 0;
                TotalDuration += sequence.Duration;
            }

            // for case when the duration of a non wait is bigger
            float currentTimeCheck = 0;
            float tempTotalDuration = TotalDuration;
            foreach (Sequence sequence in AnimationSequence)
            {
                currentTimeCheck += (sequence.SequenceType == Sequence.Type.Wait) ? sequence.Duration : 0;
                if (sequence.SequenceType == Sequence.Type.Animation)
                {
                    if (TotalDuration < currentTimeCheck + sequence.Duration)
                    {
                        TotalDuration = currentTimeCheck + sequence.Duration;
                    }
                }
            }

            CurrentTime = Mathf.Clamp(CurrentTime, 0, TotalDuration);
        }

        #endregion timing

#endif
    }
}