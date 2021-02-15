using SimpleJSON;
using UnityEngine;

namespace HairLetLoose
{
    public class ActiveHairSim
    {
        public readonly string optionName;
        private bool usePaintedRigidity;
        private float drag;
        private float gravityMultiplier;
        private float mainRigidity;
        private float tipRigidity;
        private float cling;

        private bool paintedRigidityWasDisabled = false;
        private HairSimControl hairSim;

        private JSONStorableFloat mainRigidityStorable;
        private JSONStorableFloat tipRigidityStorable;
        private JSONStorableFloat clingStorable;

        private JSONStorableFloat lowerAngleLimit;
        private JSONStorableFloat upperAngleLimit;
        private JSONStorableFloat minMainRigidity;
        private JSONStorableFloat maxMainRigidity;
        private JSONStorableFloat minTipRigidity;
        private JSONStorableFloat maxTipRigidity;
        private JSONStorableFloat minStyleCling;
        private JSONStorableFloat maxStyleCling;

        public bool hasSliders = false;
        public bool wasLetLoose = false;
        public bool enabled = true;
        public bool forceDisabled = false;
        public string notifications;

        public ActiveHairSim(string optionName, HairSimControl hairSim)
        {
            this.optionName = optionName;
            this.hairSim = hairSim;

            usePaintedRigidity = hairSim.GetBoolParamValue("usePaintedRigidity");
            drag = hairSim.GetFloatParamValue("drag");
            gravityMultiplier = hairSim.GetFloatParamValue("gravityMultiplier");
            mainRigidity = hairSim.GetFloatParamValue("mainRigidity");
            tipRigidity = hairSim.GetFloatParamValue("tipRigidity");
            cling = hairSim.GetFloatParamValue("cling");

            mainRigidityStorable = hairSim.GetFloatJSONParam("mainRigidity");
            tipRigidityStorable = hairSim.GetFloatJSONParam("tipRigidity");
            clingStorable = hairSim.GetFloatJSONParam("cling");

            InitStorables();
        }

        public void InitStorables()
        {
            lowerAngleLimit = UIElementStore.NewLowerAngleLimitStorable();
            upperAngleLimit = UIElementStore.NewUpperAngleLimitStorable();
            minMainRigidity = UIElementStore.NewMinMainRigidityStorable();
            maxMainRigidity = UIElementStore.NewMaxMainRigidityStorable();
            minTipRigidity = UIElementStore.NewMinTipRigidityStorable();
            maxTipRigidity = UIElementStore.NewMaxTipRigidityStorable();
            minStyleCling = UIElementStore.NewMinStyleClingStorable();
            maxStyleCling = UIElementStore.NewMaxStyleClingStorable();
        }

        public void InitSliders()
        {
            hasSliders = true;
            UIElementStore.ApplySliders(
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
                }
            });

            upperAngleLimit.slider.onValueChanged.AddListener((float val) =>
            {
                if(val < lowerAngleLimit.val)
                {
                    lowerAngleLimit.val = val;
                }
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
            notifications = "";
            wasLetLoose = true;
            DisablePaintedRigidity();
            CheckDrag(drag);
            CheckGravityMultiplier(gravityMultiplier);

            if(mainRigidity > maxMainRigidity.max)
            {
                maxMainRigidity.val = maxMainRigidity.max;
            }
            else
            {
                maxMainRigidity.val = Calc.RoundToDecimals(mainRigidity, 1000f);
            }
            maxMainRigidity.defaultVal = maxMainRigidity.val;
            minMainRigidity.val = Calc.RoundToDecimals(maxMainRigidity.val / 10, 1000f);

            if(tipRigidity > maxTipRigidity.max)
            {
                maxTipRigidity.val = maxTipRigidity.max;
            }
            else
            {
                maxTipRigidity.val = Calc.RoundToDecimals(tipRigidity, 1000f);
            }
            maxTipRigidity.defaultVal = maxTipRigidity.val;

            if(cling > maxStyleCling.max)
            {
                maxStyleCling.val = maxStyleCling.max;
            }
            else
            {
                maxStyleCling.val = Calc.RoundToDecimals(cling, 1000f);
            }
            maxStyleCling.defaultVal = maxStyleCling.val;
            minStyleCling.val = maxStyleCling.val;
            minStyleCling.defaultVal = minStyleCling.val;
        }

        private void DisablePaintedRigidity()
        {
            if(usePaintedRigidity)
            {
                paintedRigidityWasDisabled = true;
                hairSim.SetBoolParamValue("usePaintedRigidity", false);
            }
        }

        private void CheckDrag(float drag)
        {
            float min = 0.050f;
            float max = 0.300f;
            float original = Calc.RoundToDecimals(drag, 1000f);
            float recommended = Mathf.Clamp(original, min, max);
            if(original != recommended)
            {
                notifications = $"{notifications}\n\nDrag {original} seems {(original > recommended ? "high" : "low")}. " +
                    $"Recommended value: between {min} and {max}.";
            }
        }

        private void CheckGravityMultiplier(float gravityMultiplier)
        {
            float min = 0.900f;
            float max = 1.100f;
            float original = Calc.RoundToDecimals(gravityMultiplier, 1000f);
            float recommended = Mathf.Clamp(original, min, max);
            if(original != recommended)
            {
                notifications = $"{notifications}\n\nGravity Multiplier {original} seems {(original > recommended ? "high" : "low")}. " +
                    $"Recommended value: 1.000";
            }
        }

        public void UpdatePhysics(float tiltY)
        {
            if(!enabled || forceDisabled)
            {
                return;
            }

            // map the tilt value from the lower-upper angle range into 0-1
            // https://forum.unity.com/threads/mapping-or-scaling-values-to-a-new-range.180090/
            float baseVal = Mathf.Lerp(0f, 1f, Mathf.InverseLerp(
                lowerAngleLimit.val,
                upperAngleLimit.val,
                tiltY
            ));
            mainRigidityStorable.val = Calc.RoundToDecimals(Mathf.Lerp(minMainRigidity.val, maxMainRigidity.val, baseVal), 1000f);
            tipRigidityStorable.val = Calc.RoundToDecimals(Mathf.Lerp(minTipRigidity.val, maxTipRigidity.val, baseVal), 10000f);
            clingStorable.val = Calc.RoundToDecimals(Mathf.Lerp(minStyleCling.val, maxStyleCling.val, baseVal), 100f);
        }

        public void RestoreOriginalPhysics()
        {
            if(!wasLetLoose)
            {
                return;
            }

            hairSim.SetBoolParamValue("usePaintedRigidity", usePaintedRigidity);
            hairSim.SetFloatParamValue("mainRigidity", mainRigidity);
            hairSim.SetFloatParamValue("tipRigidity", tipRigidity);
            hairSim.SetFloatParamValue("cling", cling);
        }

        public void ReLetLoose()
        {
            notifications = "";
            usePaintedRigidity = hairSim.GetBoolParamValue("usePaintedRigidity");
            drag = hairSim.GetFloatParamValue("drag");
            gravityMultiplier = hairSim.GetFloatParamValue("gravityMultiplier");
            mainRigidity = hairSim.GetFloatParamValue("mainRigidity");
            tipRigidity = hairSim.GetFloatParamValue("tipRigidity");
            cling = hairSim.GetFloatParamValue("cling");

            DisablePaintedRigidity();
            CheckDrag(drag);
            CheckGravityMultiplier(gravityMultiplier);
            hairSim.SetFloatParamValue("mainRigidity", mainRigidityStorable.val);
            hairSim.SetFloatParamValue("tipRigidity", tipRigidityStorable.val);
            hairSim.SetFloatParamValue("cling", clingStorable.val);
        }

        public string GetStatus()
        {
            if(enabled)
            {
                return $"Main rigidity: {MainRigidityStatus()}\n" +
                    $"Tip rigidity: {TipRigidityStatus()}\n" +
                    $"Style cling: {ClingStatus()}";
            }

            return "";
        }

        public string TrackPhysics()
        {
            notifications = "";
            if(hairSim.GetBoolParamValue("usePaintedRigidity"))
            {
                notifications = $"{notifications}\n\n<b><color=#770000>Use Painted Rigidity must be disabled for this plugin to work!</color></b>";
            }
            else if(paintedRigidityWasDisabled)
            {
                notifications = $"{notifications}\n\nPainted rigidity has been disabled for the selected hairstyle.";
            }
            CheckDrag(hairSim.GetFloatParamValue("drag"));
            CheckGravityMultiplier(hairSim.GetFloatParamValue("gravityMultiplier"));
            return notifications;
        }

        private string MainRigidityStatus()
        {
            float rounded = Calc.RoundToDecimals(mainRigidityStorable.val, 1000f);
            return $"{rounded:0.000}" +
                $"{MinOrMax(rounded, minMainRigidity, maxMainRigidity)}";
        }

        private string TipRigidityStatus()
        {
            float rounded = Calc.RoundToDecimals(tipRigidityStorable.val, 10000f);
            return $"{rounded:0.0000}" +
                $"{MinOrMax(rounded, minTipRigidity, maxTipRigidity)}";
        }

        private string ClingStatus()
        {
            float rounded = Calc.RoundToDecimals(clingStorable.val, 100f);
            return $"{rounded:0.00}" +
                $"{MinOrMax(rounded, minStyleCling, maxStyleCling)}";
        }

        private string MinOrMax(float value, JSONStorableFloat min, JSONStorableFloat max)
        {
            if(min.val == max.val)
            {
                return "";
            }

            if(value <= min.val)
            {
                return " (min)";
            }

            if(value >= max.val)
            {
                return " (max)";
            }

            return "";
        }

        public JSONClass Serialize()
        {
            JSONClass originalValues = new JSONClass();
            originalValues["usePaintedRigidity"].AsBool = usePaintedRigidity;
            originalValues["drag"].AsFloat = drag;
            originalValues["gravityMultiplier"].AsFloat = gravityMultiplier;
            originalValues["mainRigidity"].AsFloat = mainRigidity;
            originalValues["tipRigidity"].AsFloat = tipRigidity;
            originalValues["cling"].AsFloat = cling;

            JSONClass jc = new JSONClass
            {
                ["originalValues"] = originalValues,
            };

            jc["enabled"].AsBool = enabled;
            jc["lowerAngleLimit"].AsFloat = lowerAngleLimit.val;
            jc["upperAngleLimit"].AsFloat = upperAngleLimit.val;
            jc["minMainRigidity"].AsFloat = minMainRigidity.val;
            jc["maxMainRigidity"].AsFloat = maxMainRigidity.val;
            jc["minTipRigidity"].AsFloat = minTipRigidity.val;
            jc["maxTipRigidity"].AsFloat = maxTipRigidity.val;
            jc["minStyleCling"].AsFloat = minStyleCling.val;
            jc["maxStyleCling"].AsFloat = maxStyleCling.val;

            return jc;
        }

        public void RestoreFromJSON(JSONClass jc)
        {
            JSONClass originalValues = jc["originalValues"].AsObject;
            usePaintedRigidity = originalValues["usePaintedRigidity"].AsBool;
            drag = originalValues["drag"].AsFloat;
            gravityMultiplier = originalValues["gravityMultiplier"].AsFloat;
            mainRigidity = originalValues["mainRigidity"].AsFloat;
            tipRigidity = originalValues["tipRigidity"].AsFloat;
            cling = originalValues["cling"].AsFloat;

            DisablePaintedRigidity();
            enabled = jc["enabled"].AsBool;
            if(!enabled)
            {
                RestoreOriginalPhysics();
            }
            lowerAngleLimit.val = jc["lowerAngleLimit"].AsFloat;
            upperAngleLimit.val = jc["upperAngleLimit"].AsFloat;
            minMainRigidity.val = jc["minMainRigidity"].AsFloat;
            maxMainRigidity.val = jc["maxMainRigidity"].AsFloat;
            minTipRigidity.val = jc["minTipRigidity"].AsFloat;
            maxTipRigidity.val = jc["maxTipRigidity"].AsFloat;
            minStyleCling.val = jc["minStyleCling"].AsFloat;
            maxStyleCling.val = jc["maxStyleCling"].AsFloat;
        }
    }
}
