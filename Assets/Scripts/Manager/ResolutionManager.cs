using System.Collections;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace Common.Manager
{
    public class ResolutionManager : MonoSingleton<ResolutionManager>
    {
        [SerializeField] RectTransform canvasRect = null;

        private Vector2 lastCanvasSize;

        private void Awake()
        {
            lastCanvasSize = canvasRect.rect.size;
        }
        public UnityEvent OnChangeResolution { get; set; } = new();
        
        void Update()
        {
            Vector2 currentSize = canvasRect.rect.size;

            if (currentSize != lastCanvasSize)
            {
                lastCanvasSize = currentSize;
                OnChangeResolution?.Invoke();
            }
        }
    }
}