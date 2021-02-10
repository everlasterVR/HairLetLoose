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
            dummyLowerAngleLimit = NewLowerAngleLimitStorable();
            dummyUpperAngleLimit = NewUpperAngleLimitStorable();
            dummyMinMainRigidity = NewMinMainRigidityStorable();
            dummyMaxMainRigidity = NewMaxMainRigidityStorable();
            dummyMinTipRigidity = NewMinTipRigidityStorable();
            dummyMaxTipRigidity = NewMaxTipRigidityStorable();
            dummyMinStyleCling = NewMinStyleClingStorable();
            dummyMaxStyleCling = NewMaxStyleClingStorable();
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

        public static void UpdateToggleButtonText(bool? result)
        {
            if(result.HasValue)
            {
                if(result.Value)
                {
                    toggleEnableButton.label = "Deactivate current hairstyle";
                }
                else
                {
                    toggleEnableButton.label = "Activate current hairstyle";
                }
            }
        }

        public static JSONStorableFloat NewLowerAngleLimitStorable()
        {
            return new JSONStorableFloat("Lower limit <size=40>°</size>", 45f, -90f, 90f);
        }

        public static JSONStorableFloat NewUpperAngleLimitStorable()
        {
            return new JSONStorableFloat("Upper limit <size=40>°</size>", 90f, -90f, 90f);
        }

        public static JSONStorableFloat NewMinMainRigidityStorable()
        {
            return new JSONStorableFloat("Main rigidity at lower limit", 0.005f, 0f, 0.100f);
        }

        public static JSONStorableFloat NewMaxMainRigidityStorable()
        {
            return new JSONStorableFloat("Main rigidity at upper limit", 0.025f, 0f, 0.100f);
        }

        public static JSONStorableFloat NewMinTipRigidityStorable()
        {
            return new JSONStorableFloat("Tip rigidity at lower limit", 0.000f, 0f, 0.010f);
        }

        public static JSONStorableFloat NewMaxTipRigidityStorable()
        {
            return new JSONStorableFloat("Tip rigidity at upper limit", 0.002f, 0f, 0.010f);
        }

        public static JSONStorableFloat NewMinStyleClingStorable()
        {
            return new JSONStorableFloat("Style cling at lower limit", 0f, 0f, 1f);
        }

        public static JSONStorableFloat NewMaxStyleClingStorable()
        {
            return new JSONStorableFloat("Style cling at upper limit", 0f, 0f, 1f);
        }
    }
}
