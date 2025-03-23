using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Gamekit2D
{
    public class Damageable : MonoBehaviour, IDataPersister
    {
        [Serializable]
        public class HealthEvent : UnityEvent<Damageable>
        { }

        [Serializable]
        public class DamageEvent : UnityEvent<Damager, Damageable>
        { }

        [Serializable]
        public class HealEvent : UnityEvent<int, Damageable>
        { }

        public int startingHealth = 5;
        public bool invulnerableAfterDamage = true;
        public float invulnerabilityDuration = 3f;
        public bool disableOnDeath = false;
        [Tooltip("An offset from the obejct position used to set from where the distance to the damager is computed")]
        public Vector2 centreOffset = new Vector2(0f, 1f);
        public HealthEvent OnHealthSet;
        public DamageEvent OnTakeDamage;
        public DamageEvent OnDie;
        public HealEvent OnGainHealth;
        [HideInInspector]
        public DataSettings dataSettings;

        protected bool m_Invulnerable;
        protected float m_InulnerabilityTimer;
        public int m_CurrentHealth;
        protected Vector2 m_DamageDirection;
        protected bool m_ResetHealthOnSceneReload;

        // Reference to the SaveLoad script
        private SaveLoad saveLoad;

        public int CurrentHealth
        {
            get { return m_CurrentHealth; }
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

            // Load health from the save file
            LoadHealthFromFile();

            OnHealthSet.Invoke(this);
            DisableInvulnerability();

            
        }

        void OnDisable()
        {
            PersistentDataManager.UnregisterPersister(this);
        }

        void Update()
        {
            if (m_Invulnerable)
            {
                m_InulnerabilityTimer -= Time.deltaTime;

                if (m_InulnerabilityTimer <= 0f)
                {
                    m_Invulnerable = false;
                }
            }
        }

        // Load health from the save file
        private void LoadHealthFromFile()
        {
            if (saveLoad != null)
            {
                GameData loadedData = saveLoad.LoadGameData();
                m_CurrentHealth = loadedData.health;
                Debug.Log("Health loaded from file: " + m_CurrentHealth);
            }
        }

        public void EnableInvulnerability(bool ignoreTimer = false)
        {
            m_Invulnerable = true;
            m_InulnerabilityTimer = ignoreTimer ? float.MaxValue : invulnerabilityDuration;
        }

        public void DisableInvulnerability()
        {
            m_Invulnerable = false;
        }

        public Vector2 GetDamageDirection()
        {
            return m_DamageDirection;
        }

        public void TakeDamage(Damager damager, bool ignoreInvincible = false)
        {
            if ((m_Invulnerable && !ignoreInvincible) || m_CurrentHealth <= 0)
                return;

            if (!m_Invulnerable)
            {
                m_CurrentHealth -= damager.damage;
                OnHealthSet.Invoke(this);
                SaveHealthToFile(); // Save health whenever it changes
            }

            m_DamageDirection = transform.position + (Vector3)centreOffset - damager.transform.position;

            OnTakeDamage.Invoke(damager, this);

            if (m_CurrentHealth <= 0)
            {
                OnDie.Invoke(damager, this);
                m_ResetHealthOnSceneReload = true;
                EnableInvulnerability();
                if (disableOnDeath) gameObject.SetActive(false);
            }
        }

        public void GainHealth(int amount)
        {
            m_CurrentHealth += amount;

            if (m_CurrentHealth > startingHealth)
                m_CurrentHealth = startingHealth;

            OnHealthSet.Invoke(this);
            SaveHealthToFile(); // Save health whenever it changes

            OnGainHealth.Invoke(amount, this);
        }

        public void SetHealth(int amount)
        {
            m_CurrentHealth = amount;

            if (m_CurrentHealth <= 0)
            {
                OnDie.Invoke(null, this);
                m_ResetHealthOnSceneReload = true;
                EnableInvulnerability();
                if (disableOnDeath) gameObject.SetActive(false);
            }

            OnHealthSet.Invoke(this);
            SaveHealthToFile(); // Save health whenever it changes
        }

        // Method to save health to file
        private void SaveHealthToFile()
        {
            if (saveLoad != null && m_CurrentHealth !=5)
            {
                // Load existing data from the save file
                GameData existingData = saveLoad.LoadGameData();

                // Save the data with the existing inventory items
                saveLoad.SaveGameData(m_CurrentHealth, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, existingData.inventoryItems);
            }
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
            return new Data<int, bool>(CurrentHealth, m_ResetHealthOnSceneReload);
        }

        public void LoadData(Data data)
        {
            Data<int, bool> healthData = (Data<int, bool>)data;

            // If value1 is true, reset health to startingHealth
            if (healthData.value1)
            {
                m_CurrentHealth = startingHealth;
            }
            else
            {
                // Otherwise, set health to the saved value (value0)
                m_CurrentHealth = healthData.value0;
            }

            // Ensure health is within valid bounds
            m_CurrentHealth = Mathf.Clamp(m_CurrentHealth, 0, startingHealth);

            OnHealthSet.Invoke(this);
        }
    }
}
