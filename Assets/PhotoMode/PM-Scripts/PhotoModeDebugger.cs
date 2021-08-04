using UnityEngine;
using PhotoMode;

namespace PhotoMode
{
    public class PhotoModeDebugger : MonoBehaviour
    {

        [SerializeField] private CanvasGroup[] cameraOffsetCanvases;
        [SerializeField] private CanvasGroup[] postProcessingCanvases;
        [SerializeField] private CanvasGroup[] filterCanvases;

        internal void PlayerAvailability(bool available)
        {
            if (!available)
                DebugMessage("Player Object", null);
        }

        internal void VolumeAvailability(bool available)
        {
            SetBlock(postProcessingCanvases, available);
            if (!available)
                DebugMessage("Volume Component", postProcessingCanvases);
        }

        internal void BlitAvailability(bool available)
        {
            SetBlock(filterCanvases, available);
            if (!available)
                DebugMessage("Blit Asset", filterCanvases);
        }

        internal void CameraOffsetAvailability(bool available)
        {
            SetBlock(cameraOffsetCanvases, available);
            if (!available)
                DebugMessage("Camera Offset", cameraOffsetCanvases);
        }

        private void SetBlock(CanvasGroup[] allCanvas, bool state)
        {
            foreach (CanvasGroup canvas in allCanvas)
            {
                canvas.interactable = state;
                canvas.alpha = state ? 1f : .2f;
            }
        }

        private void DebugMessage(string identifier, CanvasGroup[] allCanvas)
        {
            string baseMessage = "<b>Photo Mode Debug:</b> \n <b>" + identifier + "</b> is not set.";
            string additionalMessage = string.Empty;

            if (allCanvas != null)
            {
                additionalMessage = " UI block" + ((allCanvas.Length > 1) ? "s" : string.Empty) + " " + "disabled: ";

                for (int i = 0; i < allCanvas.Length; i++)
                    additionalMessage += (i > 0 ? ", " : string.Empty) + "<b><i>" + allCanvas[i].name + "</i></b>";
            }

            Debug.LogWarning(baseMessage + additionalMessage);
        }

    }
}
