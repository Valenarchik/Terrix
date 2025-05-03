using System;
using System.Collections.Generic;
using System.Linq;
using Terrix.Game.GameRules;
using UnityEngine;

namespace Terrix.Networking
{
    public class BGISizeFitterUI : MonoBehaviour
    {
        // [SerializeField] private MainMapEntryPoint mainMapEntryPoint;
        // [SerializeField] private PlayerInfoUI playerInfoUIPrefab;
        // [SerializeField] private PlayerInfoUI titles;
        [SerializeField] private RectTransform listRoot;
        // private Dictionary<int, PlayerInfoUI> playersInfos = new();
        // private PlayerInfoUI[] currentPlayersInfos;
        // private IPlayersProvider playersProvider;
        private float verticalOffset = 30;
        private float horizontalOffset = 80;
        private RectTransform rectTransform;

        // private bool needSetRectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Update()
        {
            // if (needSetRectTransform)
            // {
            //     rectTransform.sizeDelta =
            //         new Vector2(rectTransform.sizeDelta.x, listRoot.sizeDelta.y + verticalOffset);
            //     needSetRectTransform = false;
            // }
            ChangeVerticalSize();
        }

        [ContextMenu("ChangeVerticalSize")]
        public void ChangeVerticalSize()
        {
            rectTransform.sizeDelta =
                new Vector2(listRoot.sizeDelta.x + horizontalOffset, listRoot.sizeDelta.y + verticalOffset);
        }
    }
}