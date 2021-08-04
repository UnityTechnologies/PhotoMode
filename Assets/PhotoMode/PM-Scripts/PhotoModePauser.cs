using UnityEngine;
using PhotoMode;

namespace PhotoMode
{
	public class PhotoModePauser : MonoBehaviour
	{
		[SerializeField] private bool pauseActionActivation = false;

		private UnityEngine.InputSystem.PlayerInput playerInput;

		private PhotoModeInputs photoModeInputs;
		private PhotoMode photoModeBehaviors;
		private PhotoModeStickerController stickerController;

		private bool gamePaused;

		private void Awake()
		{
			//Check if there's a PlayerInput component in the scene
			if (FindObjectOfType<UnityEngine.InputSystem.PlayerInput>() != null)
				playerInput = FindObjectOfType<UnityEngine.InputSystem.PlayerInput>();

			photoModeInputs = GetComponent<PhotoModeInputs>();
			photoModeBehaviors = GetComponent<PhotoMode>();
			stickerController = GetComponentInChildren<PhotoModeStickerController>();

			if (pauseActionActivation)
			{
				photoModeInputs.PauseEvent.AddListener(PauseAction);
			}
		}

		public void PauseAction()
		{
			PauseGame(!gamePaused);
		}

		private void PauseGame(bool pause)
		{
			//StickerMode check
			if (stickerController.IsActive())
			{
				stickerController.ToggleStickerMode(false);
				return;
			}

			//Storing original timeScale
			float originalTimeScale = 1;
			if (pause)
				originalTimeScale = Time.deltaTime;

			gamePaused = pause;
			Time.timeScale = gamePaused ? 0 : originalTimeScale;
			photoModeBehaviors.Activate(pause);

			//Disable/enable player's input on pause
			if (playerInput)
				playerInput.enabled = !pause;
		}
	}
}
