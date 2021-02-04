//#define SHOW_DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HairLetLoose
{
    internal class Main : MVRScript
    {
        private Transform head;
        private List<HairSimControl> hairSims = new List<HairSimControl>();
        private string pluginVersion = "0.0.0";

        //TODO generalize / select from list
        private HairSimControl test;

        private JSONStorableFloat rootRigidity;
        private JSONStorableFloat mainRigidity;
        private JSONStorableFloat tipRigidity;

        //registered storables
        private JSONStorableFloat minRootRigidity;

        private JSONStorableFloat maxRootRigidity;
        private JSONStorableFloat minMainRigidity;
        private JSONStorableFloat maxMainRigidity;
        private JSONStorableFloat minTipRigidity;
        private JSONStorableFloat maxTipRigidity;
        private float baseVal = 0f;

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

                Log.Message($"{head.eulerAngles}");

                foreach(var hair in activeHairs)
                {
                    if(hair.name != "CustomHairItem")
                    {
                        Log.Error($"Plugin is only compatible with custom hairs, not '{hair.displayName}'");
                        return;
                    }

                    hairSims.Add(hair.GetComponentInChildren<HairSimControl>());
                }

                //TODO attempt reload if fails on scene load
                test = hairSims.First();
                rootRigidity = test.GetFloatJSONParam("rootRigidity");
                mainRigidity = test.GetFloatJSONParam("mainRigidity");
                tipRigidity = test.GetFloatJSONParam("tipRigidity");
                InitPluginUILeft();
                InitPluginUIRight();
            }
            catch(Exception e)
            {
                Log.Error("Exception caught: " + e);
            }
        }

        //  slider for how close to upside down until fully at minimum
        //  slider for how close to upright until fully at maximum
        private void InitPluginUILeft()
        {
            JSONStorableString titleUIText = new JSONStorableString("titleText", "");
            UIDynamicTextField titleUITextField = CreateTextField(titleUIText);
            titleUITextField.UItext.fontSize = 36;
            titleUITextField.height = 100;
            titleUIText.SetVal($"{nameof(Main)}\n<size=28>v{pluginVersion}</size>");

            minRootRigidity = NewRigiditySlider("Min root rigidity");
            NewSpacer(10f);
            minMainRigidity = NewRigiditySlider("Min main rigidity");
            NewSpacer(10f);
            minTipRigidity = NewRigiditySlider("Min tip rigidity");

#if SHOW_DEBUG
            UIDynamicTextField baseDebugInfoField = CreateTextField(baseDebugInfo);
            baseDebugInfoField.height = 150;
            baseDebugInfoField.UItext.fontSize = 26;
#endif
        }

        private void InitPluginUIRight()
        {
            NewSpacer(100f, true);
            maxRootRigidity = NewRigiditySlider("Max root rigidity", rootRigidity.val, rightSide: true);
            NewSpacer(10f, true);
            maxMainRigidity = NewRigiditySlider("Max main rigidity", mainRigidity.val, rightSide: true);
            NewSpacer(10f, true);
            maxTipRigidity = NewRigiditySlider("Max tip rigidity", tipRigidity.val, rightSide: true);
        }

        private JSONStorableFloat NewRigiditySlider(string name, float def = 0f, float min = 0f, float max = 1f, bool rightSide = false)
        {
            JSONStorableFloat storable = new JSONStorableFloat(name, def, min, max);
            UIDynamicSlider slider = CreateSlider(storable, rightSide);
            slider.valueFormat = "F3"; // 3 decimal places
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
            float roll = Calc.Roll(head.rotation);
            float pitch = Calc.Pitch(head.rotation);
            float rollFactor = Calc.RollFactor(roll);

            //forward
            if(pitch > 0)
            {
                if(pitch < 90)
                {
                    baseVal = Calc.UprightFactor(rollFactor, pitch, 45);
                }
                else
                {
                    baseVal = Calc.UpsideDownFactor(rollFactor, pitch, 45);
                }
            }
            //back
            else
            {
                if(pitch > -90)
                {
                    baseVal = Calc.UprightFactor(rollFactor, Mathf.Abs(pitch), 45);
                }
                else
                {
                    baseVal = Calc.UpsideDownFactor(rollFactor, Mathf.Abs(pitch), 45);
                }
            }

            //probably a bad idea performance wise
            //rootRigidity.val = Calc.RoundToDecimals(Mathf.SmoothStep(minRootRigidity.val, maxRootRigidity.val, baseVal), 1000f);

            //mainRigidity.val = Calc.RoundToDecimals(Mathf.SmoothStep(minMainRigidity.val, maxMainRigidity.val, baseVal), 1000f);
            mainRigidity.val = Calc.RoundToDecimals(Mathf.Lerp(minMainRigidity.val, 0.010f, baseVal), 1000f);

            //check if any quality difference from 10000f - costs almost 10% fps
            //tipRigidity.val = Calc.RoundToDecimals(Mathf.SmoothStep(minTipRigidity.val, maxTipRigidity.val, baseVal), 1000f);
            tipRigidity.val = Calc.RoundToDecimals(Mathf.Lerp(minTipRigidity.val, 0.016f, baseVal), 1000f);

#if SHOW_DEBUG
            baseDebugInfo.val = Log.BaseDebugInfo(roll, pitch, tipRigidity.val, mainRigidity.val, baseVal);
#endif
        }
    }
}
