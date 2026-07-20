using UnityEngine;
using System.Collections.Generic;

namespace GorillaTagMod
{
    /// <summary>
    /// Auto-leave lobby if player is reported or if someone tries to play/tag them
    /// Press 'L' to toggle the auto-leave feature on/off
    /// </summary>
    public class AutoLobbyLeave : MonoBehaviour
    {
        private static AutoLobbyLeave instance;

        private const KeyCode TOGGLE_AUTO_LEAVE_KEY = KeyCode.L;

        [SerializeField] private bool isAutoLeaveEnabled = false;
        [SerializeField] private float reportDetectionDelay = 0.5f;
        [SerializeField] private float contactDetectionDelay = 0.2f;

        private bool hasBeenReported = false;
        private bool hasBeenTagged = false;
        private float lastContactTime = -1f;
        private float lastReportCheckTime = -1f;

        private List<string> playersInLobby = new List<string>();
        private Transform playerTransform;
        private Collider playerCollider;

        public static AutoLobbyLeave Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AutoLobbyLeave>();
                    if (instance == null)
                    {
                        GameObject autoLeaveObject = new GameObject("AutoLobbyLeave");
                        instance = autoLeaveObject.AddComponent<AutoLobbyLeave>();
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
            Debug.Log("AutoLobbyLeave initialized!");
            Debug.Log($"Press '{TOGGLE_AUTO_LEAVE_KEY}' to toggle auto-leave feature");
            Debug.Log("Status: DISABLED (Press L to enable)");
        }

        void Update()
        {
            HandleAutoLeaveInput();
            
            if (isAutoLeaveEnabled)
            {
                CheckForReport();
                CheckForContact();
            }
        }

        /// <summary>
        /// Initialize player references
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
                playerCollider = playerTransform.GetComponent<Collider>();
                if (playerCollider == null)
                {
                    // Try to find a collider in children
                    playerCollider = playerTransform.GetComponentInChildren<Collider>();
                }
                Debug.Log("Player initialized for auto-leave detection!");
            }
            else
            {
                Debug.LogWarning("Could not find player in scene!");
            }
        }

        /// <summary>
        /// Handle auto-leave toggle input
        /// </summary>
        private void HandleAutoLeaveInput()
        {
            if (Input.GetKeyDown(TOGGLE_AUTO_LEAVE_KEY))
            {
                ToggleAutoLeave();
            }
        }

        /// <summary>
        /// Toggle auto-leave on/off
        /// </summary>
        public void ToggleAutoLeave()
        {
            isAutoLeaveEnabled = !isAutoLeaveEnabled;

            if (isAutoLeaveEnabled)
            {
                Debug.Log("⚠️ AUTO-LEAVE ENABLED!");
                Debug.Log("You will automatically leave if:");
                Debug.Log("  • You get reported by another player");
                Debug.Log("  • Someone tries to tag/touch you");
                hasBeenReported = false;
                hasBeenTagged = false;
            }
            else
            {
                Debug.Log("Auto-leave DISABLED");
            }
        }

        /// <summary>
        /// Check if the player has been reported
        /// </summary>
        private void CheckForReport()
        {
            if (Time.time - lastReportCheckTime < reportDetectionDelay)
            {
                return;
            }

            lastReportCheckTime = Time.time;

            // Check for report UI or notifications
            // Look for report dialog or notification
            GameObject reportDialog = GameObject.Find("ReportDialog");
            if (reportDialog != null && reportDialog.activeSelf)
            {
                hasBeenReported = true;
                Debug.LogError("🚨 REPORT DETECTED! Leaving lobby...");
                LeaveLobbySafely();
                return;
            }

            // Alternative detection: check for report UI elements
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                if (canvas.name.Contains("Report") || canvas.name.Contains("Notification"))
                {
                    if (canvas.enabled)
                    {
                        hasBeenReported = true;
                        Debug.LogError("🚨 REPORT DIALOG DETECTED! Leaving lobby...");
                        LeaveLobbySafely();
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Check for player contact/collision (tagging attempt)
        /// </summary>
        private void CheckForContact()
        {
            if (Time.time - lastContactTime < contactDetectionDelay)
            {
                return;
            }

            if (playerCollider == null || playerTransform == null)
            {
                return;
            }

            lastContactTime = Time.time;

            // Check for collisions with other players
            Collider[] collidersNearby = Physics.OverlapSphere(playerTransform.position, 2f);

            foreach (Collider collider in collidersNearby)
            {
                // Skip self
                if (collider.gameObject == playerTransform.gameObject)
                {
                    continue;
                }

                // Check if it's another player
                if (IsPlayerObject(collider.gameObject))
                {
                    hasBeenTagged = true;
                    Debug.LogError($"🚨 CONTACT DETECTED with {collider.gameObject.name}! Leaving lobby...");
                    LeaveLobbySafely();
                    return;
                }
            }
        }

        /// <summary>
        /// Detect if a game object is another player
        /// </summary>
        private bool IsPlayerObject(GameObject obj)
        {
            // Check common player object names/tags
            string objName = obj.name.ToLower();
            if (objName.Contains("player") || 
                objName.Contains("gorilla") || 
                objName.Contains("hand") ||
                objName.Contains("body"))
            {
                return true;
            }

            // Check if it has a specific player script/component
            if (obj.GetComponent<CharacterController>() != null)
            {
                return true;
            }

            if (obj.GetComponent<Rigidbody>() != null && 
                obj.tag == "Player")
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Leave the lobby safely
        /// </summary>
        private void LeaveLobbySafely()
        {
            try
            {
                // Method 1: Try to find and call lobby leave function
                GameObject lobbyManager = GameObject.Find("LobbyManager");
                if (lobbyManager != null)
                {
                    Debug.Log("Found LobbyManager, attempting to leave...");
                    // Send a message to leave
                    lobbyManager.SendMessage("LeaveLobby", SendMessageOptions.DontRequireReceiver);
                }

                // Method 2: Try to load menu scene
                Debug.Log("Loading lobby/menu scene...");
                LoadScene("Menu");
                LoadScene("Lobby");
                LoadScene("MainMenu");

                // Method 3: Direct application quit if needed
                Debug.LogWarning("AUTO-LEAVE EXECUTED - Returning to main menu");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error leaving lobby: {ex.Message}");
            }
        }

        /// <summary>
        /// Attempt to load a scene by name
        /// </summary>
        private void LoadScene(string sceneName)
        {
            try
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
                Debug.Log($"Loaded scene: {sceneName}");
            }
            catch
            {
                // Scene doesn't exist, continue trying others
            }
        }

        /// <summary>
        /// Manual trigger to leave lobby
        /// </summary>
        public void ManuallyLeaveLobby()
        {
            Debug.Log("Manually triggering lobby leave...");
            LeaveLobbySafely();
        }

        // Getters
        public bool IsAutoLeaveEnabled() => isAutoLeaveEnabled;
        public bool HasBeenReported() => hasBeenReported;
        public bool HasBeenTagged() => hasBeenTagged;

        /// <summary>
        /// Reset detection states
        /// </summary>
        public void ResetDetectionStates()
        {
            hasBeenReported = false;
            hasBeenTagged = false;
            Debug.Log("Detection states reset");
        }
    }
}
