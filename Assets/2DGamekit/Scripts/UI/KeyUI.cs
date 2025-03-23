using UnityEngine;
using System.Collections.Generic;

namespace Gamekit2D
{
    public class KeyUI : MonoBehaviour
    {
        public static KeyUI Instance { get; protected set; }

        public GameObject keyIconPrefab;
        public string[] keyNames;

        protected Animator[] m_KeyIconAnimators;

        protected readonly int m_HashActivePara = Animator.StringToHash("Active");
        protected const float k_KeyIconAnchorWidth = 0.041f;

        // Reference to the SaveLoad script
        private SaveLoad saveLoad;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Find the SaveLoad script in the scene
            saveLoad = FindObjectOfType<SaveLoad>();
            if (saveLoad == null)
            {
                Debug.LogError("SaveLoad script not found in the scene.");
            }

            // Set up the key UI
            SetInitialKeyCount();

            // Update the key UI based on the JSON file contents
            UpdateKeyUIFromJSON();
        }

        public void SetInitialKeyCount()
        {
            if (m_KeyIconAnimators != null && m_KeyIconAnimators.Length == keyNames.Length)
                return;

            m_KeyIconAnimators = new Animator[keyNames.Length];

            for (int i = 0; i < m_KeyIconAnimators.Length; i++)
            {
                GameObject healthIcon = Instantiate(keyIconPrefab);
                healthIcon.transform.SetParent(transform);
                RectTransform healthIconRect = healthIcon.transform as RectTransform;
                healthIconRect.anchoredPosition = Vector2.zero;
                healthIconRect.sizeDelta = Vector2.zero;
                healthIconRect.anchorMin -= new Vector2(k_KeyIconAnchorWidth, 0f) * i;
                healthIconRect.anchorMax -= new Vector2(k_KeyIconAnchorWidth, 0f) * i;
                m_KeyIconAnimators[i] = healthIcon.GetComponent<Animator>();
            }
        }

        // Update the key UI based on the JSON file contents
        private void UpdateKeyUIFromJSON()
        {
            if (saveLoad != null)
            {
                // Load the game data from the JSON file
                GameData gameData = saveLoad.LoadGameData();

                if (gameData != null)
                {
                    // Check each key name and update the UI
                    for (int i = 0; i < keyNames.Length; i++)
                    {
                        bool hasKey = gameData.inventoryItems.Contains(keyNames[i]);
                        m_KeyIconAnimators[i].SetBool(m_HashActivePara, hasKey);
                    }
                }
            }
        }

        public void ChangeKeyUI(InventoryController controller)
        {
            for (int i = 0; i < keyNames.Length; i++)
            {
                m_KeyIconAnimators[i].SetBool(m_HashActivePara, controller.HasItem(keyNames[i]));
            }
        }
    }
}