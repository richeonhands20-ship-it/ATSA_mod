using UnityEngine;
using System.Collections.Generic;

namespace GorillaTagMod
{
    /// <summary>
    /// VR Controller Menu System for Meta Quest 2
    /// Left Controller Menu Button opens mod menu on left side
    /// </summary>
    public class VRControllerMenu : MonoBehaviour
    {
        private static VRControllerMenu instance;

        private OVRInput.Controller leftController = OVRInput.Controller.LTouch;
        private OVRInput.Controller rightController = OVRInput.Controller.RTouch;

        [SerializeField] private bool isMenuOpen = false;
        [SerializeField] private Canvas menuCanvas;
        [SerializeField] private float menuDistance = 1.5f;
        [SerializeField] private float menuHeight = 1.5f;

        private Transform playerHead;
        private Transform leftControllerTransform;
        private Transform rightControllerTransform;
        private GameObject menuPanel;

        public static VRControllerMenu Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<VRControllerMenu>();
                    if (instance == null)
                    {
                        GameObject vrMenuObject = new GameObject("VRControllerMenu");
                        instance = vrMenuObject.AddComponent<VRControllerMenu>();
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
            InitializeVRComponents();
            CreateMenuUI();
            Debug.Log("VRControllerMenu initialized!");
            Debug.Log("Press LEFT CONTROLLER MENU BUTTON to open mod menu");
        }

        void Update()
        {
            HandleMenuInput();
            UpdateMenuPosition();
        }

        /// <summary>
        /// Initialize VR hand tracking and head position
        /// </summary>
        private void InitializeVRComponents()
        {
            // Get player head (camera)
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                playerHead = mainCamera.transform;
                Debug.Log("Player head/camera found!");
            }

            // Get controller transforms
            OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
            if (cameraRig != null)
            {
                leftControllerTransform = cameraRig.leftHandAnchor;
                rightControllerTransform = cameraRig.rightHandAnchor;
                Debug.Log("VR controllers initialized!");
            }
            else
            {
                Debug.LogWarning("OVRCameraRig not found - VR controllers may not work properly");
            }
        }

        /// <summary>
        /// Create the menu UI panel
        /// </summary>
        private void CreateMenuUI()
        {
            // Create canvas if it doesn't exist
            if (menuCanvas == null)
            {
                GameObject canvasObject = new GameObject("ModMenuCanvas");
                canvasObject.transform.SetParent(playerHead);
                canvasObject.transform.localPosition = new Vector3(-0.5f, 0, 1.5f);
                canvasObject.transform.localRotation = Quaternion.identity;

                Canvas canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;

                RectTransform rectTransform = canvasObject.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(400, 600);

                menuCanvas = canvas;
            }

            // Create menu panel
            menuPanel = new GameObject("MenuPanel");
            menuPanel.transform.SetParent(menuCanvas.transform);
            menuPanel.transform.localPosition = Vector3.zero;

            RectTransform panelRect = menuPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = menuPanel.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            // Create menu title
            CreateMenuTitle();

            // Create menu buttons
            CreateMenuButtons();

            // Initially hide menu
            menuPanel.SetActive(false);

            Debug.Log("Menu UI created!");
        }

        /// <summary>
        /// Create menu title
        /// </summary>
        private void CreateMenuTitle()
        {
            GameObject titleObject = new GameObject("Title");
            titleObject.transform.SetParent(menuPanel.transform);

            RectTransform titleRect = titleObject.AddComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 250);
            titleRect.sizeDelta = new Vector2(400, 60);

            Text titleText = titleObject.AddComponent<Text>();
            titleText.text = "⚙️ MOD MENU";
            titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            titleText.fontSize = 40;
            titleText.fontStyle = FontStyle.Bold;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.cyan;
        }

        /// <summary>
        /// Create menu buttons for mod features
        /// </summary>
        private void CreateMenuButtons()
        {
            string[] buttonLabels = new string[]
            {
                "👁️ Player IDs (P)",
                "🛟 Platforms (M)",
                "⚡ Speed Boost (S)",
                "✨ Tag Aura (T)",
                "⚠️ Auto-Leave (L)",
                "❌ CLOSE MENU"
            };

            for (int i = 0; i < buttonLabels.Length; i++)
            {
                CreateMenuButton(buttonLabels[i], i);
            }
        }

        /// <summary>
        /// Create individual menu button
        /// </summary>
        private void CreateMenuButton(string label, int index)
        {
            GameObject buttonObject = new GameObject($"Button_{index}");
            buttonObject.transform.SetParent(menuPanel.transform);

            RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
            buttonRect.anchoredPosition = new Vector2(0, 200 - (index * 70));
            buttonRect.sizeDelta = new Vector2(350, 60);

            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.7f, 0.7f, 0.8f);

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;

            // Button colors
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.7f, 0.7f, 0.8f);
            colors.highlightedColor = new Color(0.3f, 0.8f, 0.8f, 1f);
            colors.pressedColor = new Color(0.1f, 0.6f, 0.6f, 0.9f);
            button.colors = colors;

            // Add text
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(buttonObject.transform);

            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text buttonText = textObject.AddComponent<Text>();
            buttonText.text = label;
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 20;
            buttonText.fontStyle = FontStyle.Bold;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;

            // Add button click listener
            int buttonIndex = index;
            button.onClick.AddListener(() => HandleMenuButtonClick(buttonIndex));
        }

        /// <summary>
        /// Handle menu button clicks
        /// </summary>
        private void HandleMenuButtonClick(int buttonIndex)
        {
            switch (buttonIndex)
            {
                case 0: // Player IDs
                    if (PlayerIDDisplay.Instance != null)
                    {
                        PlayerIDDisplay.Instance.ToggleDisplay();
                        Debug.Log("Player ID Display toggled!");
                    }
                    break;

                case 1: // Platforms
                    if (DisappearingPlatforms.Instance != null)
                    {
                        DisappearingPlatforms.Instance.TogglePlatformMode();
                        Debug.Log("Platform mode toggled!");
                    }
                    break;

                case 2: // Speed Boost
                    if (SpeedBoots.Instance != null)
                    {
                        SpeedBoots.Instance.ToggleSpeedBoost();
                        Debug.Log("Speed boost toggled!");
                    }
                    break;

                case 3: // Tag Aura
                    if (TagAura.Instance != null)
                    {
                        TagAura.Instance.ToggleAura();
                        Debug.Log("Tag aura toggled!");
                    }
                    break;

                case 4: // Auto-Leave
                    if (AutoLobbyLeave.Instance != null)
                    {
                        AutoLobbyLeave.Instance.ToggleAutoLeave();
                        Debug.Log("Auto-leave toggled!");
                    }
                    break;

                case 5: // Close Menu
                    CloseMenu();
                    break;
            }
        }

        /// <summary>
        /// Handle left controller menu button input
        /// </summary>
        private void HandleMenuInput()
        {
            // Check if left controller menu button is pressed
            if (OVRInput.GetDown(OVRInput.Button.Start, leftController))
            {
                if (isMenuOpen)
                {
                    CloseMenu();
                }
                else
                {
                    OpenMenu();
                }
            }
        }

        /// <summary>
        /// Open the mod menu
        /// </summary>
        public void OpenMenu()
        {
            if (menuPanel != null)
            {
                menuPanel.SetActive(true);
                isMenuOpen = true;
                Debug.Log("📋 MOD MENU OPENED");
            }
        }

        /// <summary>
        /// Close the mod menu
        /// </summary>
        public void CloseMenu()
        {
            if (menuPanel != null)
            {
                menuPanel.SetActive(false);
                isMenuOpen = false;
                Debug.Log("Menu closed");
            }
        }

        /// <summary>
        /// Update menu position to follow left side of player
        /// </summary>
        private void UpdateMenuPosition()
        {
            if (menuCanvas != null && playerHead != null && isMenuOpen)
            {
                // Position menu on left side of player's view
                menuCanvas.transform.position = playerHead.position + 
                                               playerHead.right * -0.5f + 
                                               playerHead.up * -0.2f + 
                                               playerHead.forward * menuDistance;

                // Face towards player
                menuCanvas.transform.LookAt(playerHead.position);
                menuCanvas.transform.Rotate(0, 180, 0);
            }
        }

        // Getters
        public bool IsMenuOpen() => isMenuOpen;
    }
}
