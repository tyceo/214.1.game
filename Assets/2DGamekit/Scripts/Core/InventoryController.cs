using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gamekit2D
{
    public class InventoryController : MonoBehaviour, IDataPersister
    {

        [System.Serializable]
        public class InventoryEvent
        {
            public string key;
            public UnityEvent OnAdd, OnRemove;
        }
        void Start()
        {
            
        }
        [System.Serializable]
        public class InventoryChecker
        {
            public string[] inventoryItems;
            public UnityEvent OnHasItem, OnDoesNotHaveItem;

            public bool CheckInventory(InventoryController inventory)
            {
                if (inventory != null)
                {
                    for (var i = 0; i < inventoryItems.Length; i++)
                    {
                        if (!inventory.HasItem(inventoryItems[i]))
                        {
                            OnDoesNotHaveItem.Invoke();
                            return false;
                        }
                    }
                    OnHasItem.Invoke();
                    return true;
                }
                return false;
            }
        }

        public InventoryEvent[] inventoryEvents;
        public event System.Action OnInventoryLoaded;

        public DataSettings dataSettings;

        HashSet<string> m_InventoryItems = new HashSet<string>();

        // Reference to the SaveLoad script
        private SaveLoad saveLoad;

        //Debug function useful in editor during play mode to print in console all objects in that InventoyController
        [ContextMenu("Dump")]
        void Dump()
        {
            foreach (var item in m_InventoryItems)
            {
                Debug.Log(item);
            }
        }

        void OnEnable()
        {
            PersistentDataManager.RegisterPersister(this);

            // Find the SaveLoad script in the scene
            saveLoad = FindObjectOfType<SaveLoad>();
            if (saveLoad == null)
            {
                Debug.LogError("SaveLoad script not found in the scene.");
            }

            // Load inventory data from the save file
            LoadInventoryFromFile();
        }

        void OnDisable()
        {
            PersistentDataManager.UnregisterPersister(this);
        }

        // Load inventory data from the save file
        private void LoadInventoryFromFile()
        {
            if (saveLoad != null)
            {
                GameData loadedData = saveLoad.LoadGameData();
                if (loadedData != null)
                {
                    // Clear existing inventory
                    m_InventoryItems.Clear();

                    // Add items from the loaded data
                    foreach (var item in loadedData.inventoryItems)
                    {
                        AddItem(item);
                    }

                    Debug.Log("Inventory loaded from file.");
                }
            }
        }

        // Save inventory data to the save file
        private void SaveInventoryToFile()
        {
            if (saveLoad != null)
            {
                // Load existing data from the file
                GameData existingData = saveLoad.LoadGameData();

                // Update the inventory items
                existingData.inventoryItems = new List<string>(m_InventoryItems);

                // Save the updated data back to the file
                saveLoad.SaveGameData(existingData.health, existingData.currentScene, existingData.inventoryItems);
            }
        }

        public void AddItem(string key)
        {
            if (!m_InventoryItems.Contains(key))
            {
                m_InventoryItems.Add(key);
                var ev = GetInventoryEvent(key);
                if (ev != null) ev.OnAdd.Invoke();

                // Save inventory data whenever an item is added
                SaveInventoryToFile();
            }
        }

        public void RemoveItem(string key)
        {
            if (m_InventoryItems.Contains(key))
            {
                var ev = GetInventoryEvent(key);
                if (ev != null) ev.OnRemove.Invoke();
                m_InventoryItems.Remove(key);

                // Save inventory data whenever an item is removed
                SaveInventoryToFile();
            }
        }

        public bool HasItem(string key)
        {
            return m_InventoryItems.Contains(key);
        }

        public void Clear()
        {
            m_InventoryItems.Clear();

            // Save inventory data whenever the inventory is cleared
            SaveInventoryToFile();
        }

        InventoryEvent GetInventoryEvent(string key)
        {
            foreach (var iv in inventoryEvents)
            {
                if (iv.key == key) return iv;
            }
            return null;
        }

        public DataSettings GetDataSettings()
        {
            return dataSettings;
        }

        public void SetDataSettings(string dataTag, DataSettings.PersistenceType persistenceType)
        {
            dataSettings.dataTag = dataTag;
            dataSettings.persistenceType = persistenceType;
        }

        public Data SaveData()
        {
            return new Data<HashSet<string>>(m_InventoryItems);
        }

        public void LoadData(Data data)
        {
            Data<HashSet<string>> inventoryData = (Data<HashSet<string>>)data;
            foreach (var i in inventoryData.value)
                AddItem(i);
            if (OnInventoryLoaded != null) OnInventoryLoaded();
        }
    }
}