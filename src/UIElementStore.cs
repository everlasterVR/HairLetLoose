using UnityEngine.UI;

namespace HairLetLoose
{
    public static class UIElementStore
    {
        public static JSONStorableFloat dummyLowerAngleLimit;
        public static JSONStorableFloat dummyUpperAngleLimit;
        public static JSONStorableFloat dummyMinMainRigidity;
        public static JSONStorableFloat dummyMaxMainRigidity;
        public static JSONStorableFloat dummyMinTipRigidity;
        public static JSONStorableFloat dummyMaxTipRigidity;
        public static JSONStorableFloat dummyMinStyleCling;
        public static JSONStorableFloat dummyMaxStyleCling;

        public static Slider lowerAngleLimitSlider;
        public static Slider upperAngleLimitSlider;
        public static Slider minMainRigiditySlider;
        public static Slider maxMainRigiditySlider;
        public static Slider minTipRigiditySlider;
        public static Slider maxTipRigiditySlider;
        public static Slider minStyleClingSlider;
        public static Slider maxStyleClingSlider;

        public static UIDynamicButton toggleEnableButton;

        public static void Init()
        {
            dummyLowerAngleLimit = NewLowerAngleLimitStorable(false);
            dummyUpperAngleLimit = NewUpperAngleLimitStorable(false);
            dummyMinMainRigidity = NewMinMainRigidityStorable(false);
            dummyMaxMainRigidity = NewMaxMainRigidityStorable(false);
            dummyMinTipRigidity = NewMinTipRigidityStorable(false);
            dummyMaxTipRigidity = NewMaxTipRigidityStorable(false);
            dummyMinStyleCling = NewMinStyleClingStorable(false);
            dummyMaxStyleCling = NewMaxStyleClingStorable(false);
        }

        public static void StoreSliders()
        {
            lowerAngleLimitSlider = dummyLowerAngleLimit.slider;
            upperAngleLimitSlider = dummyUpperAngleLimit.slider;
            minMainRigiditySlider = dummyMinMainRigidity.slider;
            maxMainRigiditySlider = dummyMaxMainRigidity.slider;
            minTipRigiditySlider = dummyMinTipRigidity.slider;
            maxTipRigiditySlider = dummyMaxTipRigidity.slider;
            minStyleClingSlider = dummyMinStyleCling.slider;
            maxStyleClingSlider = dummyMaxStyleCling.slider;
        }

        public static void ApplySliders(
            JSONStorableFloat lowerAngleLimit,
            JSONStorableFloat upperAngleLimit,
            JSONStorableFloat minMainRigidity,
            JSONStorableFloat maxMainRigidity,
            JSONStorableFloat minTipRigidity,
            JSONStorableFloat maxTipRigidity,
            JSONStorableFloat minStyleCling,
            JSONStorableFloat maxStyleCling
        )
        {
            lowerAngleLimit.slider = lowerAngleLimitSlider;
            upperAngleLimit.slider = upperAngleLimitSlider;
            minMainRigidity.slider = minMainRigiditySlider;
            maxMainRigidity.slider = maxMainRigiditySlider;
            minTipRigidity.slider = minTipRigiditySlider;
            maxTipRigidity.slider = maxTipRigiditySlider;
            minStyleCling.slider = minStyleClingSlider;
            maxStyleCling.slider = maxStyleClingSlider;
        }

        public static void ApplyDummySliders()
        {
            dummyLowerAngleLimit.slider = lowerAngleLimitSlider;
            dummyUpperAngleLimit.slider = upperAngleLimitSlider;
            dummyMinMainRigidity.slider = minMainRigiditySlider;
            dummyMaxMainRigidity.slider = maxMainRigiditySlider;
            dummyMinTipRigidity.slider = minTipRigiditySlider;
            dummyMaxTipRigidity.slider = maxTipRigiditySlider;
            dummyMinStyleCling.slider = minStyleClingSlider;
            dummyMaxStyleCling.slider = maxStyleClingSlider;
            dummyLowerAngleLimit.slider.interactable = false;
            dummyUpperAngleLimit.slider.interactable = false;
            dummyMinMainRigidity.slider.interactable = false;
            dummyMaxMainRigidity.slider.interactable = false;
            dummyMinTipRigidity.slider.interactable = false;
            dummyMaxTipRigidity.slider.interactable = false;
            dummyMinStyleCling.slider.interactable = false;
            dummyMaxStyleCling.slider.interactable = false;
        }

        public static void UpdateToggleButtonText(bool? result)
        {
            string label = "Disable for selected hairstyle";
            if(result.HasValue)
            {
                if(!result.Value)
                {
                    label = "Enable for selected hairstyle";
                }
            }

            toggleEnableButton.label = label;
        }

        public static JSONStorableFloat NewLowerAngleLimitStorable(bool interactable = true)
        {
            return new JSONStorableFloat(
                "Lower limit <size=40>°</size>",
                45f, -90f, 90f,
                interactable: interactable
            );
        }

        public static JSONStorableFloat NewUpperAngleLimitStorable(bool interactable = true)
        {
            return new JSONStorableFloat(
                "Upper limit <size=40>°</size>",
                90f, -90f, 90f,
                interactable: interactable
            );
        }

        public static JSONStorableFloat NewMinMainRigidityStorable(bool interactable = true)
        {
            return new JSONStorableFloat(
                "Main rigidity at lower limit",
                0.005f, 0f, 0.100f,
                interactable: interactable
            );
        }

        public static JSONStorableFloat NewMaxMainRigidityStorable(bool interactable = true)
        {
            return new JSONStorableFloat(
                "Main rigidity at upper limit",
                0.050f, 0f, 0.100f,
                interactable: interactable
            );
        }

        public static JSONStorableFloat NewMinTipRigidityStorable(bool interactable = true)
        {
            return new JSONStorableFloat(
                "Tip rigidity at lower limit",
                0.000f, 0f, 0.010f,
                interactable: interactable
            );
        }

        public static JSONStorableFloat NewMaxTipRigidityStorable(bool interactable = true)
        {
            return new JSONStorableFloat(
                "Tip rigidity at upper limit",
                0.002f, 0f, 0.010f,
                interactable: interactable
            );
        }

        public static JSONStorableFloat NewMinStyleClingStorable(bool interactable = true)
        {
            return new JSONStorableFloat(
                "Style cling at lower limit",
                0f, 0f, 1f,
                interactable: interactable
            );
        }

        public static JSONStorableFloat NewMaxStyleClingStorable(bool interactable = true)
        {
            return new JSONStorableFloat(
                "Style cling at upper limit",
                0f, 0f, 1f,
                interactable: interactable
            );
        }
    }
}
