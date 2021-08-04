using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PhotoMode;

namespace PhotoMode
{

    public class InfoBarController : MonoBehaviour
    {
        [SerializeField] private GameObject photoModeMenus;
        [SerializeField] private CanvasGroup photoModeMenusCanvas;
        [SerializeField] private GameObject defaultInfoBarKeyboard;
        [SerializeField] private GameObject stickerInfoBarKeyboard;
        [SerializeField] private GameObject defaultInfoBarGamepad;
        [SerializeField] private GameObject stickerInfoBarGamepad;
        [SerializeField] private CustomInputProvider photoModeCameraInputProvider;

        private bool stickerModeActive = false;
        private bool keyboardModeActive = true;

        private void Awake()
        {
            photoModeMenusCanvas = photoModeMenus.GetComponent<CanvasGroup>();
        }

        public void StickerModeActivation(bool active)
        {
            stickerModeActive = active;

            //Infobar change
            photoModeMenusCanvas.interactable = !active;
            photoModeCameraInputProvider.active = !active;

            if (keyboardModeActive)
            {
                defaultInfoBarKeyboard.SetActive(!active);
                stickerInfoBarKeyboard.SetActive(active);

                //Deactive the unused items
                defaultInfoBarGamepad.SetActive(false);
                stickerInfoBarGamepad.SetActive(false);
            }
            else
            {
                defaultInfoBarGamepad.SetActive(!active);
                stickerInfoBarGamepad.SetActive(active);

                //Deactive the unused items
                defaultInfoBarKeyboard.SetActive(false);
                stickerInfoBarKeyboard.SetActive(false);
            }
        }

        public void SetKeyboardModeActive(bool active)
        {
            keyboardModeActive = active;

            //Update the UI incase anything has changed
            StickerModeActivation(stickerModeActive);
        }
    }
}
