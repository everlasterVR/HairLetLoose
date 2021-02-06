//#define SHOW_DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HairLetLoose
{
    internal class Script : MVRScript
    {
        private Transform head;
        private List<HairSimControl> hairSims = new List<HairSimControl>();
        private string pluginVersion = "0.0.0";

        //TODO generalize / select from list
        private HairSimControl test;

        private JSONStorableFloat mainRigidity;
        private JSONStorableFloat tipRigidity;

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
                DAZHairGroup[] activeHairs = geometry.hairItems.Where(it => it.active).ToArray();
                Transform hairContainer = geometry.femaleHairContainer;

                foreach(var hair in activeHairs)
                {
                    if(hair.name != "CustomHairItem")
                    {
                        Log.Error($"Plugin is only compatible with custom hairs, not '{hair.displayName}'");
                        return;
                    }

                    hairSims.Add(hair.GetComponentInChildren<HairSimControl>());
                }

                StartCoroutine(LoadHair());
                InitPluginUILeft();
                InitPluginUIRight();
                InitListeners();
            }
            catch(Exception e)
            {
                Log.Error("Exception caught: " + e);
            }

        }

        private IEnumerator LoadHair()
        {
            yield return new WaitForEndOfFrame();

            test = hairSims.First();
            mainRigidity = test.GetFloatJSONParam("mainRigidity");
            tipRigidity = test.GetFloatJSONParam("tipRigidity");
            maxMainRigidity.defaultVal = mainRigidity.val;
            maxMainRigidity.val = mainRigidity.val;
            maxTipRigidity.defaultVal = tipRigidity.val;
            maxTipRigidity.val = tipRigidity.val;
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
            baseDebugInfoField.UItext.fontSize = 26;
#endif
        }

        private void InitPluginUIRight()
        {
            NewSpacer(100f, true);
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
}
