﻿namespace d4160.GameFramework
{
    using d4160.Core;
    using d4160.Systems.DataPersistence;
    using Malee;
#if UNITY_EDITOR
    using NaughtyAttributes;
    using System;
    using System.Reflection;
#endif
    using UnityEngine;
    using UnityEngine.GameFoundation.DataPersistence;

    [CreateAssetMenu(fileName = "GameFrameworkDatabase.asset", menuName = "Game Framework/Database")]
    public class GameFrameworkDatabase : ScriptableObject, IDataSerializationAdapter
    {
        protected IDataSerializationAdapter m_dataAdapter;
        
        [Reorderable(paginate = true, pageSize = 10)]
        [SerializeField] protected GameDataReorderableArray m_gameData;

        public GameDataReorderableArray GameData => m_gameData;

        public IDataSerializationAdapter DataAdapter
        {
            get {
                if (m_dataAdapter == null)
                    m_dataAdapter = new DefaultGameDataSerializationAdapter();

                return m_dataAdapter;
            }
            set => m_dataAdapter = value;
        }

        public ScriptableObjectBase this[int index]
        {
            get
            {
                if (m_gameData.IsValidIndex(index))
                    return m_gameData[index];

                return null;
            }
        }

        public T GetGameData<T>(int index) where T : ScriptableObjectBase
        {
            var gameData = this[index];
            if (gameData)
                return gameData as T;

            return null;
        }

        public ISerializableData GetSerializableData()
        {
            var data = new DefaultAppSettingsSerializableData();

            var gameData = new BaseSettingsSerializableData[m_gameData.Length];
            for (int i = 0; i < m_gameData.Length; i++)
            {
                gameData[i] = m_gameData[i].GetSerializableData() as BaseSettingsSerializableData;
            }

            data.SettingsData = gameData;

            return data;
        }

        public void FillFromSerializableData(ISerializableData data)
        {
            var appSettingsData = data as DefaultAppSettingsSerializableData;

            if (appSettingsData == null || appSettingsData.SettingsData == null)
            {
                Debug.LogWarning($"AppSettingsSerializableData is null. Object {appSettingsData.SettingsData == null} ");
                return;
            }

            for (int i = 0; i < appSettingsData.SettingsData.Length; i++)
            {
                if (!m_gameData.IsValidIndex(i)) break;

                m_gameData[i].FillFromSerializableData(appSettingsData.SettingsData[i]);
            }
        }

        public void InitializeData(ISerializableData data)
        {
            if (data != null)
            {
                FillFromSerializableData(data);
            }
        }

        public void Unintialize()
        {
        }

        public bool IsInitialized => m_gameData != null && m_gameData.Length > 0;

        #region Editor Members
#if UNITY_EDITOR
        [Button]
        protected virtual void CreateDefaultArchetypes()
        {
            var bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var fields = GetType().GetFields(bindings);

            for (int i = 0; i < fields.Length; i++)
            {
                var value = fields[i].GetValue(this);
                if (value is IArchetypeOperations)
                {
                    var archetypeInterface = value as IArchetypeOperations;
                    var attrib = Attribute.GetCustomAttribute(
                        fields[i], typeof(DefaultArchetypesAttribute));

                    if (attrib != null)
                    {
                        var defArchetypesAttrib = attrib as DefaultArchetypesAttribute;

                        if (defArchetypesAttrib.OnlyOnEmptyLists && archetypeInterface.Count != 0)
                        {
                            continue;
                        }
                        else
                        {
                            archetypeInterface.AddRange(defArchetypesAttrib.kValues);
                        }
                    }
                }
            }
        }
#endif
        #endregion
    }

    [System.Serializable]
    public class GameDataReorderableArray : ReorderableArrayForUnityObject<ScriptableObjectBase>
    {
    }
}