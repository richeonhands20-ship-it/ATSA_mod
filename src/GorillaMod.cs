using UnityEngine;
using System;

namespace GorillaTagMod
{
    public class GorillaMod : MonoBehaviour
    {
        private static GorillaMod instance;

        public static GorillaMod Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<GorillaMod>();
                    if (instance == null)
                    {
                        GameObject modObject = new GameObject("GorillaMod");
                        instance = modObject.AddComponent<GorillaMod>();
                        DontDestroyOnLoad(modObject);
                    }
                }
                return instance;
            }
        }

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            Debug.Log("Gorilla Tag Mod initialized!");
        }

        void Update()
        {
            // Add your mod logic here
        }
    }
}
