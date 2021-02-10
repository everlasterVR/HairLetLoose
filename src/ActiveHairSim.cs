﻿using UnityEngine;

namespace HairLetLoose
{
    public class ActiveHairSim
    {
        private bool usePaintedRigidity;
        private float weight;
        private float drag;
        private float gravityMultiplier;
        private float mainRigidity;
        private float tipRigidity;
        private float cling;

        private HairSimControl hairSim;

        private JSONStorableFloat mainRigidityStorable;
        private JSONStorableFloat tipRigidityStorable;
        private JSONStorableFloat styleClingStorable;

        private JSONStorableFloat lowerAngleLimit;
        private JSONStorableFloat upperAngleLimit;
        private JSONStorableFloat minMainRigidity;
        private JSONStorableFloat maxMainRigidity;
        private JSONStorableFloat minTipRigidity;
        private JSONStorableFloat maxTipRigidity;
        private JSONStorableFloat minStyleCling;
        private JSONStorableFloat maxStyleCling;

        private float upperLimit;
        private float lowerLimit;

        public bool hasSliders = false;
        public bool wasLetLoose = false;
        public string settingInfo;

        public ActiveHairSim(HairSimControl hairSim)
        {
            this.hairSim = hairSim;
            usePaintedRigidity = hairSim.GetBoolParamValue("usePaintedRigidity");
            weight = hairSim.GetFloatParamValue("weight");
            drag = hairSim.GetFloatParamValue("drag");
            gravityMultiplier = hairSim.GetFloatParamValue("gravityMultiplier");
            mainRigidity = hairSim.GetFloatParamValue("mainRigidity");
            tipRigidity = hairSim.GetFloatParamValue("tipRigidity");
            cling = hairSim.GetFloatParamValue("cling");

            mainRigidityStorable = hairSim.GetFloatJSONParam("mainRigidity");
            tipRigidityStorable = hairSim.GetFloatJSONParam("tipRigidity");
            styleClingStorable = hairSim.GetFloatJSONParam("cling");

            InitStorables();
            InitSliders();
        }

        public void InitStorables()
        {
            lowerAngleLimit = UISliderStore.NewLowerAngleLimitStorable();
            upperAngleLimit = UISliderStore.NewUpperAngleLimitStorable();
            minMainRigidity = UISliderStore.NewMinMainRigidityStorable();
            maxMainRigidity = UISliderStore.NewMaxMainRigidityStorable();
            minTipRigidity = UISliderStore.NewMinTipRigidityStorable();
            maxTipRigidity = UISliderStore.NewMaxTipRigidityStorable();
            minStyleCling = UISliderStore.NewMinStyleClingStorable();
            maxStyleCling = UISliderStore.NewMaxStyleClingStorable();
        }

        public void InitSliders()
        {
            hasSliders = true;
            UISliderStore.ApplySliders(
                lowerAngleLimit,
                upperAngleLimit,
                minMainRigidity,
                maxMainRigidity,
                minTipRigidity,
                maxTipRigidity,
                minStyleCling,
                maxStyleCling
            );

            lowerAngleLimit.slider.onValueChanged.AddListener((float val) =>
            {
                if(val > upperAngleLimit.val)
                {
                    upperAngleLimit.val = val;
                    UpdateUpperLimit(val);
                }
                UpdateLowerLimit(val);
            });

            upperAngleLimit.slider.onValueChanged.AddListener((float val) =>
            {
                if(val < lowerAngleLimit.val)
                {
                    lowerAngleLimit.val = val;
                    UpdateLowerLimit(val);
                }
                UpdateUpperLimit(val);
            });

            minMainRigidity.slider.onValueChanged.AddListener((float val) =>
            {
                if(val > maxMainRigidity.val)
                {
                    maxMainRigidity.val = val;
                }
            });

            maxMainRigidity.slider.onValueChanged.AddListener((float val) =>
            {
                if(val < minMainRigidity.val)
                {
                    minMainRigidity.val = val;
                }
            });

            minTipRigidity.slider.onValueChanged.AddListener((float val) =>
            {
                if(val > maxTipRigidity.val)
                {
                    maxTipRigidity.val = val;
                }
            });

            maxTipRigidity.slider.onValueChanged.AddListener((float val) =>
            {
                if(val < minTipRigidity.val)
                {
                    minTipRigidity.val = val;
                }
            });

            minStyleCling.slider.onValueChanged.AddListener((float val) =>
            {
                if(val > maxStyleCling.val)
                {
                    maxStyleCling.val = val;
                }
            });

            maxStyleCling.slider.onValueChanged.AddListener((float val) =>
            {
                if(val < minStyleCling.val)
                {
                    minStyleCling.val = val;
                }
            });
        }

        public void UnsetSliders()
        {
            hasSliders = false;
            lowerAngleLimit.slider.onValueChanged.RemoveAllListeners();
            upperAngleLimit.slider.onValueChanged.RemoveAllListeners();
            minMainRigidity.slider.onValueChanged.RemoveAllListeners();
            maxMainRigidity.slider.onValueChanged.RemoveAllListeners();
            minTipRigidity.slider.onValueChanged.RemoveAllListeners();
            maxTipRigidity.slider.onValueChanged.RemoveAllListeners();
            minStyleCling.slider.onValueChanged.RemoveAllListeners();
            maxStyleCling.slider.onValueChanged.RemoveAllListeners();

            lowerAngleLimit.slider = null;
            upperAngleLimit.slider = null;
            minMainRigidity.slider = null;
            maxMainRigidity.slider = null;
            minTipRigidity.slider = null;
            maxTipRigidity.slider = null;
            minStyleCling.slider = null;
            maxStyleCling.slider = null;
        }

        public void LetLoose()
        {
            wasLetLoose = true;
            settingInfo = "";

            if(usePaintedRigidity)
            {
                settingInfo = $"{settingInfo}\n- disabled painted rigidity";
                hairSim.SetBoolParamValue("usePaintedRigidity", false);
            }

            float adjustedWeight = Mathf.Clamp((float) weight, 1.350f, 1.650f);
            if(weight != adjustedWeight)
            {
                settingInfo = $"{settingInfo}\n- weight set to {adjustedWeight} (was {Calc.RoundToDecimals(weight, 1000f)})";
                hairSim.SetFloatParamValue("weight", adjustedWeight);
            }

            float adjustedDrag = Mathf.Clamp((float) drag, 0.050f, 0.150f);
            if(drag != adjustedDrag)
            {
                settingInfo = $"{settingInfo}\n- drag set to {adjustedDrag} (was {Calc.RoundToDecimals(drag, 1000f)})";
                hairSim.SetFloatParamValue("drag", adjustedDrag);
            }

            float adjustedGravity = Mathf.Clamp((float) gravityMultiplier, 0.900f, 1.100f);
            if(gravityMultiplier != adjustedGravity)
            {
                settingInfo = $"{settingInfo}\n- gravity multiplier set to {adjustedGravity} (was {Calc.RoundToDecimals(gravityMultiplier, 1000f)})";
                hairSim.SetFloatParamValue("gravityMultiplier", adjustedGravity);
            }

            if(mainRigidityStorable.val > maxMainRigidity.max)
            {
                settingInfo = $"{settingInfo}\n- main rigidity set to {maxMainRigidity.max} (was {Calc.RoundToDecimals(mainRigidity, 1000f)})";
                maxMainRigidity.val = maxMainRigidity.max;
            }
            else
            {
                maxMainRigidity.val = mainRigidity;
            }
            maxMainRigidity.defaultVal = maxMainRigidity.val;
            minMainRigidity.val = maxMainRigidity.val / 10;

            if(tipRigidity > maxTipRigidity.max)
            {
                settingInfo = $"{settingInfo}\n- tip rigidity set to {maxTipRigidity.max} (was {Calc.RoundToDecimals((float) tipRigidity, 1000f)})";
                maxTipRigidity.val = maxTipRigidity.max;
            }
            else
            {
                maxTipRigidity.val = tipRigidity;
            }
            maxTipRigidity.defaultVal = maxTipRigidity.val;

            if(cling > maxStyleCling.max)
            {
                settingInfo = $"{settingInfo}\n- style cling set to {maxStyleCling.max} (was {Calc.RoundToDecimals((float) cling, 1000f)})";
                maxStyleCling.val = maxStyleCling.max;
            }
            else
            {
                maxStyleCling.val = cling;
            }
            maxStyleCling.defaultVal = maxStyleCling.val;
            minStyleCling.val = maxStyleCling.val;
            minStyleCling.defaultVal = minStyleCling.val;

            UpdateUpperLimit(upperAngleLimit.val);
            UpdateLowerLimit(lowerAngleLimit.val);
        }

        private void UpdateUpperLimit(float val)
        {
            float amount = Mathf.Clamp(1 - (val + 90)/180, 0, 0.99f); //prevent division by 0
            upperLimit = 1 + amount/(1 - amount);
        }

        private void UpdateLowerLimit(float val)
        {
            float amount = Mathf.Clamp(1 - (90 - val)/180, 0, 0.99f); //prevent division by 0
            lowerLimit = -amount/(1 - amount);
        }

        public void UpdatePhysics(float tiltY)
        {
            float baseVal = Mathf.Clamp(Mathf.Lerp(lowerLimit, upperLimit, tiltY), 0f, 1f); // map tilt to lower-upper range, clamp to 0-1
            mainRigidityStorable.val = Calc.RoundToDecimals(Mathf.Lerp(minMainRigidity.val, maxMainRigidity.val, baseVal), 1000f);
            tipRigidityStorable.val = Calc.RoundToDecimals(Mathf.Lerp(minTipRigidity.val, maxTipRigidity.val, baseVal), 10000f);
            styleClingStorable.val = Calc.RoundToDecimals(Mathf.Lerp(minStyleCling.val, maxStyleCling.val, baseVal), 100f);
        }

        public void RestoreOriginalPhysics()
        {
            if(!wasLetLoose)
            {
                return;
            }

            hairSim.SetBoolParamValue("usePaintedRigidity", usePaintedRigidity);
            hairSim.SetFloatParamValue("weight", weight);
            hairSim.SetFloatParamValue("drag", drag);
            hairSim.SetFloatParamValue("gravityMultiplier", gravityMultiplier);
            hairSim.SetFloatParamValue("mainRigidity", mainRigidity);
            hairSim.SetFloatParamValue("tipRigidity", tipRigidity);
            hairSim.SetFloatParamValue("cling", cling);
        }

        public string GetStatus()
        {
            return $"Main rigidity: {FormatValue(mainRigidityStorable, minMainRigidity, maxMainRigidity)}\n" +
               $"Tip rigidity: {FormatValue(tipRigidityStorable, minTipRigidity, maxTipRigidity)}\n" +
               $"Style cling: {FormatValue(styleClingStorable, minStyleCling, maxStyleCling)}";
        }

        private string FormatValue(JSONStorableFloat storable, JSONStorableFloat min, JSONStorableFloat max)
        {
            string text = $"{storable.val}";
            if(min.val == max.val)
            {
                return text;
            }

            if(storable.val >= max.val)
            {
                text += " (highest)";
            }
            else if(storable.val <= min.val)
            {
                text += " (lowest)";
            }
            return text;
        }
    }
}
