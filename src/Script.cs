//#define SHOW_DEBUG
using System;
using System.Collections;
using UnityEngine;

namespace HairLetLoose
{
    internal class Script : MVRScript
    {
        private string pluginVersion = "0.0.0";

        private bool enableUpdate = false;
        private bool enableCheck = true;
        private bool loadHairSimInProgress = false;
        private float waitCounter = 0f;
        private float waitSeconds = 2f;
        private float waitLimit = 60f;

        private Transform head;
        private DAZHairGroup[] hairItems;
        private HairSimControl hairSim;

        private JSONStorableFloat mainRigidity;
        private JSONStorableFloat tipRigidity;

        private JSONStorableString statusUIText;

        //registered storables
        private JSONStorableFloat minMainRigidity;
        private JSONStorableFloat maxMainRigidity;
        private JSONStorableFloat minTipRigidity;
        private JSONStorableFloat maxTipRigidity;
        private JSONStorableFloat upperAngleLimit;
        private JSONStorableFloat lowerAngleLimit;

        private float upperLimit;
        private float lowerLimit;

#if SHOW_DEBUG
        protected JSONStorableString baseDebugInfo = new JSONStorableString("Base Debug Info", "");
#endif

        public override void Init()
        {
            try
            {
                if(containingAtom.type != "Person")
                {
                    Log.Error($"Plugin is for use with 'Person' atom, not '{containingAtom.type}'");
                    return;
                }

                head = containingAtom.GetStorableByID("head").transform;
                DAZCharacterSelector geometry = containingAtom.GetComponentInChildren<DAZCharacterSelector>();
                hairItems = geometry.hairItems;

                StartCoroutine(LoadHairSim());
                InitPluginUILeft();
                InitPluginUIRight();
                InitListeners();
            }
            catch(Exception e)
            {
                Log.Error("Exception caught: " + e);
            }

        }

        private IEnumerator LoadHairSim()
        {
            loadHairSimInProgress = true;
            yield return new WaitForEndOfFrame();

            string name = "";
            foreach(DAZHairGroup it in hairItems)
            {
                if(it.active && it.name == "CustomHairItem")
                {
                    name = $"<b><color=#007700>{it.creatorName}</color></b>" +
                        $" | <b><color=#007700>{it.displayName}</color></b>";
                    hairSim = it.GetComponentInChildren<HairSimControl>();
                    break;
                }
            }

            if (hairSim == null || !hairSim.isActiveAndEnabled)
            {
                statusUIText.val = $"<b><color=#AA0000>Select a hairstyle.</color></b>";
                yield return new WaitForSecondsRealtime(waitSeconds);
                waitCounter += waitSeconds;
                loadHairSimInProgress = false;
                yield break;
            }

            mainRigidity = hairSim.GetFloatJSONParam("mainRigidity");
            tipRigidity = hairSim.GetFloatJSONParam("tipRigidity");
            maxMainRigidity.defaultVal = mainRigidity.val;
            maxMainRigidity.val = mainRigidity.val;
            maxTipRigidity.defaultVal = tipRigidity.val;
            maxTipRigidity.val = tipRigidity.val;

            statusUIText.val = $"Active hair:\n{name}";
            enableUpdate = true;
            loadHairSimInProgress = false;
        }

        private void InitPluginUILeft()
        {
            JSONStorableString titleUIText = new JSONStorableString("titleText", "");
            UIDynamicTextField titleUITextField = CreateTextField(titleUIText);
            titleUITextField.UItext.fontSize = 36;
            titleUITextField.height = 100;
            titleUIText.SetVal($"{nameof(HairLetLoose)}\n<size=28>v{pluginVersion}</size>");

            minMainRigidity = NewSlider("Min main rigidity");
            NewSpacer(10f);
            minTipRigidity = NewSlider("Min tip rigidity");
            NewSpacer(10f);
            upperAngleLimit = NewSlider("Upper limit <size=40>°</size>", def: 90f, min: -90f, max: 90f, valueFormat: "F0");
            lowerAngleLimit = NewSlider("Lower limit <size=40>°</size>", def: 45f, min: -90f, max: 90f, valueFormat: "F0");
#if SHOW_DEBUG
            UIDynamicTextField baseDebugInfoField = CreateTextField(baseDebugInfo);
            baseDebugInfoField.height = 300;
            baseDebugInfoField.UItext.fontSize = 24;
#endif
        }

        private void InitPluginUIRight()
        {
            statusUIText = new JSONStorableString("statusText", "");
            UIDynamicTextField statusUITextField = CreateTextField(statusUIText, rightSide: true);
            statusUITextField.UItext.fontSize = 28;
            statusUITextField.height = 100;

            maxMainRigidity = NewSlider("Max main rigidity", def: 0.015f, rightSide: true);
            NewSpacer(10f, true);
            maxTipRigidity = NewSlider("Max tip rigidity", def: 0.002f, rightSide: true);
        }

        private void InitListeners()
        {
            upperAngleLimit.slider.onValueChanged.AddListener((float val) => {
                if (val < lowerAngleLimit.val)
                {
                    lowerAngleLimit.val = val;
                }
                UpdateUpperLimit(val);
            });
            lowerAngleLimit.slider.onValueChanged.AddListener((float val) =>
            {
                if(val > upperAngleLimit.val)
                {
                    upperAngleLimit.val = val;
                }

                UpdateLowerLimit(val);
            });
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

        private JSONStorableFloat NewSlider(
            string name,
            float def = 0f,
            float min = 0f,
            float max = 1f,
            string valueFormat = "F3", // 3 decimal places
            bool rightSide = false
        )
        {
            JSONStorableFloat storable = new JSONStorableFloat(name, def, min, max);
            UIDynamicSlider slider = CreateSlider(storable, rightSide);
            slider.valueFormat = valueFormat;
            RegisterFloat(storable);
            return storable;
        }

        private void NewSpacer(float height, bool rightSide = false)
        {
            UIDynamic spacer = CreateSpacer(rightSide);
            spacer.height = height;
        }

        public void Update()
        {
            try
            {
                if(enableCheck)
                {
                    CheckHairSimStatus();
                }

                if(enableUpdate)
                {
                    float tiltY = (1 + Vector3.Dot(head.up, Vector3.up)) / 2; // 1 = upright, 0 = upside down
                    float baseVal = Mathf.Clamp(Mathf.Lerp(lowerLimit, upperLimit, tiltY), 0f, 1f); // map tilt to lower-upper range, clamp to 0-1
                    mainRigidity.val = Calc.RoundToDecimals(Mathf.Lerp(minMainRigidity.val, maxMainRigidity.val, baseVal), 1000f);
                    tipRigidity.val = Calc.RoundToDecimals(Mathf.Lerp(minTipRigidity.val, maxTipRigidity.val, baseVal), 1000f);
#if SHOW_DEBUG
                    baseDebugInfo.SetVal(
                        $"{Log.NameValueString("tiltY", tiltY, 100f, 10)}\n" +
                        $"{Log.NameValueString("Base val", baseVal, 1000f, 10)}\n" +
                        $"{Log.NameValueString("Tip rigidity", tipRigidity.val, 1000f, 22)}\n" +
                        $"{Log.NameValueString("Main rigidity", mainRigidity.val, 1000f, 20)}"
                    );
#endif
                }
            }
            catch(Exception e)
            {
                enableUpdate = false;
                Log.Error("Exception caught: " + e);
            }
        }

        private void CheckHairSimStatus()
        {
            if(hairSim != null && hairSim.isActiveAndEnabled)
            {
                return;
            }

            enableUpdate = false;
            if(waitCounter >= waitLimit)
            {
                enableCheck = false;
                string msg = "Select a hairstyle and reload plugin.";
                Log.Message($"No hair was selected in {waitLimit} seconds. {msg}");
                statusUIText.val = $"<b><color=#AA0000>{msg}</color></b>";
            }
            else if(!loadHairSimInProgress)
            {
                StartCoroutine(LoadHairSim());
            }
        }
    }
}
