using System;
using QFramework;
using UnityEngine;

namespace MOYV
{
    /// <summary>
    /// 面板数据
    /// </summary>
    public class BasePanelData
    {
        
    }

    /// <summary>
    /// 面板基类
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class AbstractBasePanel : AbstractMonoBehaviourController, ICanSendEvent
    {
        // 面板Canvas
        private Canvas _panelCanvas;

        // 打开时是否使用动画
        [Header("打开时播放动效")]
        [SerializeField] protected bool playVisualEffectOnOpen = true;
        
        // 关闭时是否使用动画
        [Header("关闭时播放动效")]
        [SerializeField] protected bool playVisualEffectOnClose = true;
        
        // 关闭时是否隐藏而非销毁
        [SerializeField] protected bool hideOnClose;

        // 面板数据
        private BasePanelData _panelData;
        
        // 打开后回调
        private Action _onOpenCallBack;
        
        // 关闭后回调
        private Action _onCloseCallBack;

        /// <summary>
        /// 打开面板
        /// </summary>
        public void Open(Action onOpenCallBack = null)
        {
            gameObject.SetActive(true);
            _onOpenCallBack = onOpenCallBack;
            OnResume();

            if (playVisualEffectOnOpen)
            {
                ShowEffectsOnOpen(null);
            }
            OnFinishOpen();
        }
        
        /// <summary>
        /// 打开过程结束
        /// </summary>
        private void OnFinishOpen()
        {
            this.SendEvent(new PanelShowEvent(this));
            _onOpenCallBack?.Invoke();
            _onOpenCallBack = null;
        }


        /// <summary>
        /// 关闭面板
        /// </summary>
        public void Close(Action onCloseCallBack = null)
        {
            _onCloseCallBack = onCloseCallBack;
            OnClose();
            if (playVisualEffectOnClose)
            {
                ShowEffectsOnClose(OnFinishClose);
            }
            else
            {
                OnFinishClose();
            }
        }

        /// <summary>
        /// 关闭过程结束
        /// </summary>
        private void OnFinishClose()
        {
            this.SendEvent(new PanelCloseEvent(this));
            _onCloseCallBack?.Invoke();
            _onCloseCallBack = null;
            if (!hideOnClose)
            {
                Destroy(gameObject);
            }
            else
            {
                Hide();
            }
        }
        
        /// <summary>
        /// 隐藏
        /// </summary>
        private void Hide()
        {
            gameObject.SetActive(false);
            StopAllCoroutines();
            ClearOnHide();
        }

        public override IArchitecture GetArchitecture()
        {
            return Architecture.CurrentArchitecture;
        }

        public void SetSortingOrder(int newSortingOrder)
        {
            if (!_panelCanvas)
            {
                _panelCanvas = GetComponent<Canvas>();
            }

            if (_panelCanvas == null) return;
            _panelCanvas.overrideSorting = true;
            _panelCanvas.sortingOrder = newSortingOrder;
        }
        
        public int GetSortingOrder()
        {
            if (!_panelCanvas)
            {
                _panelCanvas = GetComponent<Canvas>();
            }

            return _panelCanvas == null ? 0 : _panelCanvas.sortingOrder;
        }

        /// <summary>
        /// 是否在关闭面板时隐藏而不是销毁
        /// </summary>
        /// <returns></returns>
        public bool IsHideOnClose()
        {
            return hideOnClose;
        }

        #region 派生类需要关注的接口

        /// <summary>
        /// 使用传入的数据初始化
        /// </summary>
        /// <param name="data"></param>
        public virtual void InitWithPanelData(BasePanelData data)
        {
            
        }
        
        /// <summary>
        /// 打开面板时的效果展示，如缩放、特效、音频等
        /// </summary>
        protected virtual void ShowEffectsOnOpen(Action onFinishOpenProcess)
        {
            onFinishOpenProcess?.Invoke();
        }
        
        /// <summary>
        /// 关闭面板时的效果展示，如缩放、特效、音频等
        /// </summary>
        /// <param name="onFinishCloseProcess"></param>
        protected virtual void ShowEffectsOnClose(Action onFinishCloseProcess)
        {
            onFinishCloseProcess?.Invoke();
        }
        
        /// <summary>
        /// 面板暂停运行时的逻辑
        /// </summary>
        public virtual void OnPause()
        {

        }

        /// <summary>
        /// 面板恢复运行时的逻辑
        /// </summary>
        public virtual void OnResume()
        {

        }

        /// <summary>
        /// 面板关闭时的逻辑
        /// </summary>
        public virtual void OnClose()
        {
            
        }
        
        
        /// <summary>
        /// 面板隐藏的时候需要执行的清理逻辑
        /// </summary>
        protected virtual void ClearOnHide()
        {
        }

        #endregion
    }
}