using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Terrix.Networking
{
    public class EnterCustomLobbyUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;

        public int GetInputID() => Convert.ToInt32(inputField.text);
    }
}