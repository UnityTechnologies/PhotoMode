using UnityEngine;
using UnityEngine.UI;
using PhotoMode;

namespace PhotoMode
{

    public class PhotoModeStickerController : MonoBehaviour
    {
        private bool stickerModeOn;

        [Header("Info Bar References")]
        [SerializeField] private InfoBarController infoBarController;

        [Header("Sticker References")]
        [SerializeField] private Transform stickerOverlay;
        [SerializeField] private CanvasGroup stickerCanvas;
        [SerializeField] private CanvasGroup stickerCursor;
        [SerializeField] private Transform stickerPreview;
        [SerializeField] private Image prevSticker, currentSticker, nextSticker;
        [SerializeField] private Button stickerActivateButton;

        [Header("Sticker Settings")]
        [SerializeField] private float stickerCursorSpeed;
        [SerializeField] private float stickerRotateSpeed;
        [SerializeField] private float stickerScaleSpeed;
        [SerializeField] private Sprite[] stickerSprites;

        private RectTransform stickerCursorRect;
        private RectTransform stickerPreviewRect;
        private int stickerAmountCount;
        private int stickerSpriteCount;
        private RectTransform[] stickerPool;
        private Vector3 originalStickerScale;
        private Vector2 originalCursorSize, originalPreviewSize;

        private void Awake()
        {
            //Sticker Mode declarations
            stickerActivateButton.onClick.AddListener(() => ToggleStickerMode(true));
            stickerCursorRect = stickerCursor.GetComponent<RectTransform>();
            stickerPreviewRect = stickerPreview.GetComponent<RectTransform>();
            stickerPool = stickerOverlay.GetComponentsInChildren<RectTransform>();
            originalStickerScale = stickerPreview.localScale;
            originalCursorSize = stickerCursorRect.sizeDelta;
            originalPreviewSize = stickerPreviewRect.sizeDelta;
        }

        public bool IsActive()
        {
            return stickerModeOn;
        }

        public void ToggleStickerMode(bool status)
        {
            stickerModeOn = status;
            infoBarController.StickerModeActivation(status);

            //Update Gallery
            UpdateStickerGallery();

            stickerCanvas.alpha = status ? 1 : 0;
        }

        public void StampSticker()
        {
            if (!stickerModeOn)
                return;

            stickerPool[stickerAmountCount].position = stickerPreviewRect.position;
            stickerPool[stickerAmountCount].rotation = stickerPreviewRect.rotation;
            stickerPool[stickerAmountCount].sizeDelta = stickerPreviewRect.sizeDelta;
            stickerPool[stickerAmountCount].localScale = stickerPreviewRect.localScale;
            stickerPool[stickerAmountCount].GetComponent<Image>().sprite = stickerPreview.GetComponent<Image>().sprite;
            stickerPool[stickerAmountCount].GetComponent<Image>().color = Color.white;

            stickerAmountCount++;
            if (stickerAmountCount > stickerPool.Length - 1) stickerAmountCount = 0;
        }

        public void MoveStickers(Vector2 axis)
        {
            if (!stickerModeOn)
                return;

            stickerCursor.transform.position += (Vector3)axis * Time.unscaledDeltaTime * stickerCursorSpeed;
        }

        public void RotateStickers(float dir)
        {
            if (!stickerModeOn)
                return;

            stickerCursor.transform.eulerAngles += new Vector3(0, 0, -dir) * Time.unscaledDeltaTime * stickerRotateSpeed;
        }

        public void ScaleStickers(float dir)
        {
            if (!stickerModeOn)
                return;

            //Clamp Size
            if (dir == 1 && stickerPreviewRect.sizeDelta.x >= 250 || dir == -1 && stickerPreviewRect.sizeDelta.x <= 50)
                return;

            stickerCursorRect.sizeDelta += new Vector2(dir, dir) * Time.unscaledDeltaTime * stickerScaleSpeed;
            stickerPreviewRect.sizeDelta += new Vector2(dir, dir) * Time.unscaledDeltaTime * stickerScaleSpeed;
        }

        public void ChangeStickerSprite(int input)
        {
            if (!stickerModeOn)
                return;

            if (input == 1)
            {
                stickerSpriteCount++;
                if (stickerSpriteCount > stickerSprites.Length - 1)
                    stickerSpriteCount = 0;
            }

            if (input == -1)
            {
                stickerSpriteCount--;
                if (stickerSpriteCount < 0)
                    stickerSpriteCount = stickerSprites.Length - 1;
            }

            stickerPreview.GetComponent<Image>().sprite = stickerSprites[stickerSpriteCount];
            UpdateStickerGallery();
        }

        public void UpdateStickerGallery()
        {
            stickerPreview.GetComponent<Image>().sprite = stickerSprites[(int)Mathf.Repeat(stickerSpriteCount, stickerSprites.Length)];
            prevSticker.sprite = stickerSprites[(int)Mathf.Repeat(stickerSpriteCount - 1, stickerSprites.Length)];
            currentSticker.sprite = stickerSprites[(int)Mathf.Repeat(stickerSpriteCount, stickerSprites.Length)];
            nextSticker.sprite = stickerSprites[(int)Mathf.Repeat(stickerSpriteCount + 1, stickerSprites.Length)];
        }

        public void DeleteSticker()
        {
            if (!stickerModeOn)
                return;

            int index;
            index = stickerAmountCount - 1;
            if (index < 0)
                index = stickerPool.Length - 1;

            if (stickerPool[index].GetComponent<Image>().color == Color.clear)
                return;

            stickerAmountCount--;
            if (stickerAmountCount < 0) stickerAmountCount = stickerPool.Length - 1;

            stickerPool[stickerAmountCount].GetComponent<Image>().color = Color.clear;
        }

        public void FlipSticker(bool reset)
        {
            if (!stickerModeOn)
                return;

            if (!reset)
                stickerPreviewRect.localScale = new Vector3(stickerPreviewRect.localScale.x * -1, originalStickerScale.y, originalStickerScale.z);
            else
                stickerPreview.localScale = originalStickerScale;
        }

        public void ResetStickers()
        {
            FlipSticker(true);

            foreach (RectTransform rect in stickerPool)
                rect.GetComponent<Image>().color = Color.clear;

            stickerSpriteCount = 0;
            stickerCursorRect.anchoredPosition = Vector3.zero;
            stickerCursor.transform.eulerAngles = Vector3.zero;
            stickerCursorRect.sizeDelta = originalCursorSize;
            stickerPreviewRect.sizeDelta = originalPreviewSize;
        }

    }
}
