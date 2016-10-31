﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace StevenUniverse.FanGame.StrategyMap.UI
{
    // TODO : Re-use buttons rather than destroying them each time the menu is hidden.
    //        Account for the window appearing offscreen - snap it to always be inside the canvas

    /// <summary>
    /// Popup UI of valid actions for when a character is selected.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class CharacterActionsUI : MonoBehaviour
    {
        public Button pfb_ActionButton_;

        static CanvasGroup canvasGroup_;

        /// <summary>
        /// Buffer used when reading valid actions from a character. Used to avoid unneeded allocations.
        /// </summary>
        static List<CharacterAction> actionsBuffer_ = new List<CharacterAction>();

        //static List<Button> actionButtons_ = new List<Button>();

        public static CharacterActionsUI Instance { get; private set; }

        void Awake()
        {
            canvasGroup_ = GetComponent<CanvasGroup>();
            Instance = this;
        }

        /// <summary>
        /// Show the valid actions for the given character in a popup UI beside the character.
        /// </summary>
        public static void Show( MapCharacter character )
        {
            actionsBuffer_.Clear();
            ClearButtons();
            character.GetActions(actionsBuffer_);

            if( actionsBuffer_.Count == 0 )
            {
                Debug.LogWarningFormat("Attempting to load actions UI for {0}, but they have no actions!", character.name);
                return;
            }
            
            var windowPos = RectTransformUtility.WorldToScreenPoint(Camera.main, character.transform.position + Vector3.right + Vector3.up );

            Instance.transform.position = windowPos;

            // Create buttons mapping to our valid actions.
            for( int i = 0; i < actionsBuffer_.Count; ++i )
            {
                var action = actionsBuffer_[i];
                var newButton = ((Button)Instantiate(Instance.pfb_ActionButton_, Instance.transform, false)).GetComponent<Button>();
                newButton.gameObject.layer = Instance.gameObject.layer;
                var text = newButton.GetComponentInChildren<Text>();
                text.text = action.UIName;
                newButton.onClick.AddListener(action.Execute);
                // Hide our UI when an option is selected.
                newButton.onClick.AddListener(Hide);
            }

            canvasGroup_.alpha = 1f;
            canvasGroup_.blocksRaycasts = true;
 
        }

        /// <summary>
        /// Hide the UI. Currently destroys any previously made buttons. These should be pooled instead.
        /// </summary>
        public static void Hide()
        {
            canvasGroup_.alpha = 0;
            canvasGroup_.blocksRaycasts = false;
        }

        static void ClearButtons()
        {

            var buttons = Instance.GetComponentsInChildren<Button>();

            if (buttons != null)
            {
                foreach (var button in buttons)
                {
                    Destroy(button.gameObject);
                }
            }
        }
    }
}