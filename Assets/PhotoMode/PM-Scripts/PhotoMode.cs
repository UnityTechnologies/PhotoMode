using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cinemachine;
using PhotoMode;

namespace PhotoMode
{

    [System.Serializable] public class PhotoModeEvent : UnityEvent<bool> { }
    public class PhotoMode : MonoBehaviour
    {
        private static PhotoMode instance;

        private EventSystem projectEventSystem;
        private CinemachineBrain projectCinemachineBrain;
        CinemachineBrain.UpdateMethod project_cm_update;
        CinemachineBlendDefinition.Style project_cm_blend;

        [Header("Character Reference")]
        [SerializeField] private GameObject playerObject;
        [SerializeField] private GameObject photoModeCameraOrbit;

        private EventSystem photoModeEventSystem;
        private PhotoModeInputs photoModeInputs;
        private CinemachineFreeLook photoModeCamera;
        private CinemachineCameraOffset photoModeCameraOffset;
        private Transform photoModeUI;
        private GameObject photoModeFrame;
        private Volume photoModeVolume;
        private VolumeProfile photoModeVolumeProfile;
        private CanvasGroup photoModeMenusCanvas;
        private float photoModeCameraXAxis;
        private float photoModeCameraYAxis;

        [Header("UI References")]
        [SerializeField] private GameObject photoModeMenus;
        [SerializeField] private GameObject photoModeGrid;
        [SerializeField] private PhotoModeStickerController stickerController;
        [SerializeField] private Image photoModeVignette;

        [Header("Photo Mode Settings")]
        [SerializeField] private MinMax viewRoll = new MinMax(-90f, 90f);
        [SerializeField] private MinMax camDist = new MinMax(3f, -3f);
        [SerializeField] private MinMax focusDistance = new MinMax(0.1f, 20f);
        [SerializeField] private MinMax aperture = new MinMax(1f, 32f);
        [SerializeField] private MinMax exposure = new MinMax(-3f, 3f);
        [SerializeField] private MinMax contrast = new MinMax(-50f, 50f);
        [SerializeField] private MinMax saturation = new MinMax(-100f, 100f);
        [SerializeField] private MinMax vignette = new MinMax(-1f, 1f);
        [SerializeField] private MinMax verticalArm = new MinMax(-1f, 1f);
        [Range(.5f, 2)] [SerializeField] private float verticalArmSpeed = 0.01f;

        [Header("Filter Material")]
        [SerializeField] private Material postProcessingMaterial;
        [SerializeField] private Blit blit;

        [Header("Debug Behavior")]
        [SerializeField] private PhotoModeDebugger photoModeDebugger;

        private Shader unlitShader;
        private bool photoModeOn;
        private Color vignetteColor;
        private ColorAdjustments colorAdj;
        private DepthOfField dof;

        [Space]
        public PhotoModeEvent OnPhotoModeActivation;

        private void Awake()
        {
            instance = this;

            //Check if projectCinemachineBrain is set
            if (FindObjectOfType<CinemachineBrain>() != null)
            {
                projectCinemachineBrain = FindObjectOfType<CinemachineBrain>();
                project_cm_blend = projectCinemachineBrain.m_DefaultBlend.m_Style;
                project_cm_update = projectCinemachineBrain.m_UpdateMethod;
            }

            //Store the project's Event System
            projectEventSystem = EventSystem.current;

            // Photo Mode declarations
            photoModeEventSystem = GetComponentInChildren<EventSystem>();
            photoModeInputs = GetComponent<PhotoModeInputs>();
            photoModeCamera = GetComponentInChildren<CinemachineFreeLook>();
            photoModeCameraOffset = photoModeCamera.GetComponent<CinemachineCameraOffset>();
            photoModeUI = transform.Find("PhotoMode_UI");
            photoModeVolume = GetComponentInChildren<Volume>();
            photoModeVolumeProfile = photoModeVolume.profile;
            photoModeVolumeProfile.TryGet<DepthOfField>(out dof);
            photoModeVolumeProfile.TryGet<ColorAdjustments>(out colorAdj);
            photoModeMenusCanvas = photoModeMenus.GetComponent<CanvasGroup>();

            //Disable Photo Mode Event System if project has an exisiting Event System
            if (projectEventSystem != photoModeEventSystem)
                photoModeEventSystem.enabled = false;

            //Add Input Listeners
            photoModeInputs.ResetEvent.AddListener(ResetValues);
            photoModeInputs.ToggleGridEvent.AddListener(ToggleGrid);
            photoModeInputs.ToggleInterfaceEvent.AddListener(ToggleUI);
            photoModeInputs.SubmitEvent.AddListener(stickerController.StampSticker);
            photoModeInputs.SwapStickerEvent.AddListener(() => stickerController.ChangeStickerSprite(1));
            photoModeInputs.DeleteStickerEvent.AddListener(stickerController.DeleteSticker);
            photoModeInputs.FlipStickerEvent.AddListener(() => stickerController.FlipSticker(false));

            //Get the unlit texture shader for use later
            unlitShader = Shader.Find("Unlit/Texture");

            //Try to force finding a player
            if (playerObject == null && FindObjectOfType<UnityEngine.InputSystem.PlayerInput>() != null)
                playerObject = FindObjectOfType<UnityEngine.InputSystem.PlayerInput>().gameObject;

            if (playerObject != null)
                photoModeCameraOrbit.transform.position = playerObject.transform.position;

            //Debug messages
            DebugBehavior();

            //Deactivate the UI if it's enabled
            if (photoModeUI.gameObject.activeSelf)
                photoModeUI.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (stickerController.IsActive())
            {
                stickerController.MoveStickers(photoModeInputs.moveAxis);
                stickerController.RotateStickers(photoModeInputs.modifier1_value);
                stickerController.ScaleStickers(photoModeInputs.modifier2_value);
            }
            else if (photoModeOn)
            {
                CraneCamera(photoModeInputs.modifier2_value);
            }
        }

        // Toggles applicable Photo Mode values based on game's pause status
        public void Activate(bool active)
        {
            OnPhotoModeActivation.Invoke(active);

            if (photoModeCameraOrbit != null && playerObject != null)
                photoModeCameraOrbit.transform.position = playerObject.transform.position;

            //General Canvas Group
            photoModeUI.gameObject.SetActive(active);

            //Project EventSystem configuration
            if (active)
                projectEventSystem = EventSystem.current;

            if (projectEventSystem != null)
            {
                if (projectEventSystem != photoModeEventSystem)
                    projectEventSystem.enabled = !active;
            }

            photoModeEventSystem.enabled = active;

            //Project CinemachineBrain configuration
            ProjectCinemachineConfig(active);

            //Remember the starting position of the photo mode camera for use in the reset function
            photoModeCameraXAxis = photoModeCamera.m_XAxis.Value;
            photoModeCameraYAxis = photoModeCamera.m_YAxis.Value;

            //Reset default Photo Mode Values
            ResetValues();

            //Transition Cameras
            photoModeOn = active;
            photoModeCamera.Priority = active ? 99 : 1;

            if (blit != null)
                blit.SetActive(active);

            if (active == true)
                SetEventSystemSelectedObj(GetComponentInChildren<Slider>().gameObject);
        }

        // Camera Offset functionality
        public void CraneCamera(float value)
        {
            if (value < 0 && photoModeCameraOffset.m_Offset.y > verticalArm.min)
                photoModeCameraOffset.m_Offset.y -= (verticalArmSpeed * Mathf.Abs(value)) * Time.unscaledDeltaTime;

            if (value > 0 && photoModeCameraOffset.m_Offset.y < verticalArm.max)
                photoModeCameraOffset.m_Offset.y += (verticalArmSpeed * Mathf.Abs(value)) * Time.unscaledDeltaTime;
        }

        // Invoked by applicable UI sliders when their value changes
        public void ViewRoll(Slider slider)
        {
            photoModeCamera.m_Lens.Dutch = (slider.value - slider.minValue) / (slider.maxValue - slider.minValue) * (viewRoll.max - viewRoll.min) + viewRoll.min;
        }

        public void CameraDistance(Slider slider)
        {
            photoModeCameraOffset.m_Offset.z = (slider.value - slider.minValue) / (slider.maxValue - slider.minValue) * (camDist.max - camDist.min) + camDist.min;
        }

        public void FocusDistance(Slider slider)
        {
            dof.focusDistance.Override((slider.value - slider.minValue) / (slider.maxValue - slider.minValue) * (focusDistance.max - focusDistance.min) + focusDistance.min);
        }

        public void Aperture(Slider slider)
        {
            dof.aperture.Override((slider.value - slider.minValue) / (slider.maxValue - slider.minValue) * (aperture.max - aperture.min) + aperture.min);
        }

        public void Exposure(Slider slider)
        {
            colorAdj.postExposure.Override((slider.value - slider.minValue) / (slider.maxValue - slider.minValue) * (exposure.max - exposure.min) + exposure.min);
        }

        public void Contrast(Slider slider)
        {
            colorAdj.contrast.Override((slider.value - slider.minValue) / (slider.maxValue - slider.minValue) * (contrast.max - contrast.min) + contrast.min);
        }

        public void Saturation(Slider slider)
        {
            colorAdj.saturation.Override((slider.value - slider.minValue) / (slider.maxValue - slider.minValue) * (saturation.max - saturation.min) + saturation.min);
        }

        public void Vignette(Slider slider)
        {
            vignetteColor = slider.value > 0 ? Color.white : Color.black;
            vignetteColor.a = Mathf.Abs((slider.value - slider.minValue) / (slider.maxValue - slider.minValue) * (vignette.max - vignette.min) + vignette.min);
            photoModeVignette.color = vignetteColor;
        }

        public void ChangeFilter(Shader shader)
        {
            postProcessingMaterial.shader = shader;
        }

        public void ChangeFrame(GameObject frame)
        {
            ResetFrame();
            photoModeFrame = frame;
            frame.gameObject.SetActive(true);
        }

        public void ResetFrame()
        {
            if (photoModeFrame != null)
                photoModeFrame.SetActive(false);
        }

        // Toggles photo mode UI visiblity so user can take an unobstructed screenshot
        public void ToggleUI()
        {
            if (!photoModeOn)
                return;

            if (!stickerController.IsActive())
                photoModeMenusCanvas.alpha = photoModeMenusCanvas.alpha == 0 ? 1 : 0;
        }

        // Toggles rule-of-thirds photoModeGrid visiblity within the photo mode UI
        public void ToggleGrid()
        {
            if (!photoModeOn)
                return;

            if (!stickerController.IsActive())
                photoModeGrid.SetActive(!photoModeGrid.activeSelf);
        }

        // Resets all photo mode values to their defaults
        public void ResetValues()
        {
            if (stickerController.IsActive())
                return;

            //Reset camera Crane & Zoom
            photoModeCameraOffset.m_Offset = Vector3.zero;

            //Reset the photo mode camera position
            photoModeCamera.m_XAxis.Value = photoModeCameraXAxis;
            photoModeCamera.m_YAxis.Value = photoModeCameraYAxis;

            //Reset every slider value
            foreach (Slider slider in photoModeMenus.GetComponentsInChildren<Slider>())
                slider.value = 0;

            //Toggle UI if it was disabled
            if (photoModeMenusCanvas.alpha == 0)
                ToggleUI();

            //Reset the filter shader to the built in unlit texture shader
            ChangeFilter(unlitShader);

            stickerController.ResetStickers();
            stickerController.ToggleStickerMode(false);

            if (EventSystem.current != null)
                SetEventSystemSelectedObj(EventSystem.current.currentSelectedGameObject);
        }


        private void SetEventSystemSelectedObj(GameObject obj)
        {
            GameObject currentSelected = obj;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(currentSelected);
        }

        private void OnApplicationQuit()
        {
            ResetValues();
        }

        private void DebugBehavior()
        {
            if (playerObject == null)
                photoModeDebugger.PlayerAvailability(false);

            if (blit == null)
                photoModeDebugger.BlitAvailability(false);

            if (photoModeVolume == null)
                photoModeDebugger.VolumeAvailability(false);

            if (photoModeCameraOffset == null)
                photoModeDebugger.CameraOffsetAvailability(false);
        }

        private void ProjectCinemachineConfig(bool active)
        {
            if (projectCinemachineBrain != null)
            {
                projectCinemachineBrain.m_IgnoreTimeScale = active;
                projectCinemachineBrain.m_UpdateMethod = active ? CinemachineBrain.UpdateMethod.SmartUpdate : project_cm_update;
                StartCoroutine(BlendTimeReset());

                IEnumerator BlendTimeReset()
                {
                    float blendTime = projectCinemachineBrain.m_DefaultBlend.m_Time;
                    projectCinemachineBrain.m_DefaultBlend.m_Time = 0;
                    projectCinemachineBrain.m_DefaultBlend.m_Style = active ? CinemachineBlendDefinition.Style.Cut : project_cm_blend;
                    yield return new WaitForEndOfFrame();
                    projectCinemachineBrain.m_DefaultBlend.m_Time = blendTime;
                }
            }
        }

        /// <summary>
        /// Utilize this function to determine the object that the Photo Mode Camera will orbit around.
        /// </summary>
        /// <param name="player">GameObject that Photo Mode will orbit around</param>
        public static void SetPlayerObject(GameObject player)
        {
            instance.playerObject = player;
            instance.photoModeCameraOrbit.transform.position = player.transform.position;
        }
    }
}
