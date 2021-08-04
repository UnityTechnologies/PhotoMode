using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using PhotoMode;

namespace PhotoMode
{

    [RequireComponent(typeof(Slider))]
    public class TextSlider : MonoBehaviour
    {
        private Slider slider;

        [Header("Visual Components")]
        public TextMeshProUGUI sliderText;
        public Image leftArrow;
        public Image rightArrow;

        [Header("Indicator Colors")]
        public Color inactiveColor = new Color(0.427451f, 0.427451f, 0.427451f, 1f);
        public Color activeColor = new Color(1f, 1f, 1f, 1f);

        [Header("Slider Custom Options")]
        public Option[] options;

        [System.Serializable]
        public class Option
        {
            public string optionTitle = "Option Title";
            public UnityEvent optionEvent = new UnityEvent();
        }

        private void Awake()
        {
            slider = GetComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = options.Length - 1;
            UpdateTextSlider(slider.value);
        }

        public void UpdateTextSlider(float index)
        {
            if (options != null)
            {
                sliderText.text = options[(int)index].optionTitle;
                options[(int)index].optionEvent.Invoke();

                if (slider.value == slider.minValue || slider.value == slider.maxValue)
                {
                    if (slider.value == slider.minValue)
                    {
                        leftArrow.color = inactiveColor;
                        rightArrow.color = activeColor;
                    }

                    if (slider.value == slider.maxValue)
                    {
                        leftArrow.color = activeColor;
                        rightArrow.color = inactiveColor;
                    }
                }
                else
                {
                    leftArrow.color = activeColor;
                    rightArrow.color = activeColor;
                }

            }
        }
    }
}
