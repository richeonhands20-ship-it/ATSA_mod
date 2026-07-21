using UnityEngine;
using System.Collections.Generic;

namespace GorillaTagMod
{
    /// <summary>
    /// Displays player IDs and usernames above each player's head
    /// Press 'P' to toggle player ID display on/off
    /// </summary>
    public class PlayerIDDisplay : MonoBehaviour
    {
        private static PlayerIDDisplay instance;

        private const KeyCode TOGGLE_DISPLAY_KEY = KeyCode.P;

        [SerializeField] private bool isDisplayEnabled = false;
        [SerializeField] private float updateInterval = 0.5f;
        [SerializeField] private float displayDistance = 50f; // Only show IDs within 50m
        [SerializeField] private int maxCharacters = 20;

        private Dictionary<GameObject, GameObject> playerLabels = new Dictionary<GameObject, GameObject>();
        private Dictionary<GameObject, string> playerIDs = new Dictionary<GameObject, string>();
        private float lastUpdateTime = -1f;

        private Transform playerTransform;
        private List<GameObject> allPlayers = new List<GameObject>();

        public static PlayerIDDisplay Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<PlayerIDDisplay>();
                    if (instance == null)
                    {
                        GameObject displayObject = new GameObject("PlayerIDDisplay");
                        instance = displayObject.AddComponent<PlayerIDDisplay>();
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
            InitializePlayer();
            Debug.Log("PlayerIDDisplay initialized!");
            Debug.Log($"Press '{TOGGLE_DISPLAY_KEY}' to toggle player ID display");
            Debug.Log("Status: DISABLED (Press P to enable)");
        }

        void Update()
        {
            HandleDisplayInput();

            if (isDisplayEnabled && Time.time - lastUpdateTime >= updateInterval)
            {
                UpdatePlayerDisplay();
                lastUpdateTime = Time.time;
            }
        }

        /// <summary>
        /// Initialize player reference
        /// </summary>
        private void InitializePlayer()
        {
            playerTransform = GameObject.Find("Player")?.transform;

            if (playerTransform == null)
            {
                playerTransform = GameObject.Find("MainPlayer")?.transform;
            }

            if (playerTransform != null)
            {
                Debug.Log("Player initialized for ID display!");
            }
            else
            {
                Debug.LogWarning("Could not find player in scene!");
            }
        }

        /// <summary>
        /// Handle display toggle input
        /// </summary>
        private void HandleDisplayInput()
        {
            if (Input.GetKeyDown(TOGGLE_DISPLAY_KEY))
            {
                ToggleDisplay();
            }
        }

        /// <summary>
        /// Toggle player ID display on/off
        /// </summary>
        public void ToggleDisplay()
        {
            isDisplayEnabled = !isDisplayEnabled;

            if (isDisplayEnabled)
            {
                Debug.Log("👁️ PLAYER ID DISPLAY ENABLED!");
                Debug.Log("Player IDs and usernames now visible above heads");
            }
            else
            {
                Debug.Log("Player ID display DISABLED");
                ClearAllLabels();
            }
        }

        /// <summary>
        /// Update player display - find new players and update labels
        /// </summary>
        private void UpdatePlayerDisplay()
        {
            if (playerTransform == null)
            {
                return;
            }

            // Find all potential player objects
            FindAllPlayers();

            // Update or create labels for each player
            foreach (GameObject player in allPlayers)
            {
                if (player == null || player == playerTransform.gameObject)
                {
                    continue; // Skip self
                }

                float distance = Vector3.Distance(playerTransform.position, player.transform.position);

                if (distance > displayDistance)
                {
                    // Too far, remove label
                    if (playerLabels.ContainsKey(player))
                    {
                        Destroy(playerLabels[player]);
                        playerLabels.Remove(player);
                        playerIDs.Remove(player);
                    }
                    continue;
                }

                // Update or create label
                if (!playerLabels.ContainsKey(player))
                {
                    CreatePlayerLabel(player);
                }
                else
                {
                    UpdatePlayerLabel(player);
                }
            }

            // Remove labels for players that no longer exist
            List<GameObject> deadPlayers = new List<GameObject>();
            foreach (var kvp in playerLabels)
            {
                if (kvp.Key == null)
                {
                    if (kvp.Value != null)
                    {
                        Destroy(kvp.Value);
                    }
                    deadPlayers.Add(kvp.Key);
                }
            }

            foreach (var deadPlayer in deadPlayers)
            {
                playerLabels.Remove(deadPlayer);
                playerIDs.Remove(deadPlayer);
            }
        }

        /// <summary>
        /// Find all players in the scene
        /// </summary>
        private void FindAllPlayers()
        {
            allPlayers.Clear();

            // Search for player objects by tag
            GameObject[] playersByTag = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in playersByTag)
            {
                if (!allPlayers.Contains(player))
                {
                    allPlayers.Add(player);
                }
            }

            // Search for player objects by name patterns
            foreach (GameObject obj in FindObjectsOfType<GameObject>())
            {
                if (obj.name.Contains("Player") || 
                    obj.name.Contains("Gorilla") || 
                    obj.name.Contains("Avatar"))
                {
                    if (obj.GetComponent<CharacterController>() != null || 
                        obj.GetComponent<Rigidbody>() != null)
                    {
                        if (!allPlayers.Contains(obj))
                        {
                            allPlayers.Add(obj);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create a label for a player
        /// </summary>
        private void CreatePlayerLabel(GameObject player)
        {
            // Get or generate player ID
            string playerID = GetPlayerID(player);
            string playerName = GetPlayerName(player);

            // Create label object
            GameObject labelObject = new GameObject("IDLabel");
            labelObject.transform.SetParent(player.transform);
            labelObject.transform.localPosition = Vector3.up * 2f; // Above the head

            // Add text renderer (using 3D text)
            TextMesh textMesh = labelObject.AddComponent<TextMesh>();
            textMesh.text = $"{playerName}\n[{playerID}]";
            textMesh.fontSize = 40;
            textMesh.characterSize = 0.1f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.cyan;

            // Add mesh renderer
            MeshRenderer meshRenderer = labelObject.GetComponent<MeshRenderer>();
            Material labelMat = new Material(Shader.Find("GUI/Text Shader"));
            labelMat.color = Color.cyan;
            meshRenderer.material = labelMat;

            // Make it always face the camera
            labelObject.AddComponent<BillboardBehavior>();

            playerLabels[player] = labelObject;
            playerIDs[player] = playerID;

            Debug.Log($"Created label for player: {playerName} ({playerID})");
        }

        /// <summary>
        /// Update player label position and content
        /// </summary>
        private void UpdatePlayerLabel(GameObject player)
        {
            if (!playerLabels.ContainsKey(player) || playerLabels[player] == null)
            {
                return;
            }

            GameObject labelObject = playerLabels[player];

            // Update position (above head)
            if (player.transform != null)
            {
                labelObject.transform.position = player.transform.position + Vector3.up * 2.5f;
            }

            // Keep it facing camera
            if (playerTransform != null && playerTransform.GetComponentInChildren<Camera>() != null)
            {
                Camera mainCamera = playerTransform.GetComponentInChildren<Camera>();
                labelObject.transform.LookAt(labelObject.transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
            }
        }

        /// <summary>
        /// Get or generate a player ID
        /// </summary>
        private string GetPlayerID(GameObject player)
        {
            if (player == null)
            {
                return "UNKNOWN";
            }

            // Try to get from Photon or networking component
            Component[] components = player.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp.GetType().Name.Contains("PhotonView"))
                {
                    return GetComponentProperty(comp, "ViewID").ToString();
                }
                if (comp.GetType().Name.Contains("NetworkIdentity"))
                {
                    return GetComponentProperty(comp, "netId").ToString();
                }
            }

            // Fallback: use instance ID
            return player.GetInstanceID().ToString().Substring(0, Mathf.Min(8, player.GetInstanceID().ToString().Length));
        }

        /// <summary>
        /// Get player name/username
        /// </summary>
        private string GetPlayerName(GameObject player)
        {
            if (player == null)
            {
                return "Unknown";
            }

            // Try to get name from various sources
            if (!string.IsNullOrEmpty(player.name) && player.name != "Player")
            {
                return player.name.Length > maxCharacters 
                    ? player.name.Substring(0, maxCharacters) + "..." 
                    : player.name;
            }

            // Check for player script with username
            Component[] components = player.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp.GetType().Name.Contains("Player"))
                {
                    object username = GetComponentProperty(comp, "username");
                    if (username != null)
                    {
                        return username.ToString();
                    }

                    object playerName = GetComponentProperty(comp, "playerName");
                    if (playerName != null)
                    {
                        return playerName.ToString();
                    }

                    object name = GetComponentProperty(comp, "name");
                    if (name != null)
                    {
                        return name.ToString();
                    }
                }
            }

            return "Player";
        }

        /// <summary>
        /// Helper to get component property safely
        /// </summary>
        private object GetComponentProperty(Component comp, string propertyName)
        {
            try
            {
                System.Reflection.PropertyInfo prop = comp.GetType().GetProperty(propertyName);
                if (prop != null)
                {
                    return prop.GetValue(comp, null);
                }

                System.Reflection.FieldInfo field = comp.GetType().GetField(propertyName);
                if (field != null)
                {
                    return field.GetValue(comp);
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Clear all labels
        /// </summary>
        private void ClearAllLabels()
        {
            foreach (var label in playerLabels.Values)
            {
                if (label != null)
                {
                    Destroy(label);
                }
            }

            playerLabels.Clear();
            playerIDs.Clear();
        }

        // Getters
        public bool IsDisplayEnabled() => isDisplayEnabled;
        public int GetVisiblePlayerCount() => playerLabels.Count;
    }

    /// <summary>
    /// Billboard behavior - makes text face the camera
    /// </summary>
    public class BillboardBehavior : MonoBehaviour
    {
        private Camera mainCamera;

        void Start()
        {
            mainCamera = Camera.main;
        }

        void LateUpdate()
        {
            if (mainCamera != null)
            {
                transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                                mainCamera.transform.rotation * Vector3.up);
            }
        }
    }
}
