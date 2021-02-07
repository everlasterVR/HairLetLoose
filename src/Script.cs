//#define SHOW_DEBUG
using System;

namespace HairLetLoose
{
    internal class Script : MVRScript
    {
        private string pluginVersion = "0.0.0";

        private HairSimHandler hairSimHandler;

        //registered storables
        private JSONStorableFloat minMainRigidity;
        private JSONStorableFloat maxMainRigidity;
        private JSONStorableFloat minTipRigidity;
        private JSONStorableFloat maxTipRigidity;
        private JSONStorableFloat upperAngleLimit;
        private JSONStorableFloat lowerAngleLimit;

        public override void Init()
        {
            try
            {
                if(containingAtom.type != "Person")
                {
                    Log.Error($"Plugin is for use with 'Person' atom, not '{containingAtom.type}'");
                    return;
                }

                if(gameObject.GetComponent<HairSimHandler>() == null)
                {
                    hairSimHandler = gameObject.AddComponent<HairSimHandler>();
                }

                InitPluginUILeft();
                InitPluginUIRight();
                InitListeners();
                hairSimHandler.Init(containingAtom, minMainRigidity, maxMainRigidity, minTipRigidity, maxTipRigidity);
                hairSimHandler.LoadHairSim();
                hairSimHandler.UpdateUpperLimit(upperAngleLimit.val);
                hairSimHandler.UpdateLowerLimit(lowerAngleLimit.val);
            }
            catch(Exception e)
            {
                Log.Error("Exception caught: " + e);
            }
        }

        #region User interface

        private void InitPluginUILeft()
        {
            JSONStorableString titleUIText = new JSONStorableString("titleText", "");
            UIDynamicTextField titleUITextField = CreateTextField(titleUIText);
            titleUITextField.UItext.fontSize = 36;
            titleUITextField.height = 100;
            titleUIText.SetVal($"{nameof(HairLetLoose)}\n<size=28>v{pluginVersion}</size>");

            minMainRigidity = NewSlider("Min main rigidity", def: 0.005f, max: 0.050f);
            NewSpacer(10f);
            minTipRigidity = NewSlider("Min tip rigidity", def: 0.000f, max: 0.005f);
            NewSpacer(10f);
            upperAngleLimit = NewSlider("Upper limit <size=40>°</size>", def: 90f, min: -90f, max: 90f, valueFormat: "F0");
            lowerAngleLimit = NewSlider("Lower limit <size=40>°</size>", def: 45f, min: -90f, max: 90f, valueFormat: "F0");
#if SHOW_DEBUG
            hairSimHandler.baseDebugInfo = new JSONStorableString("Base Debug Info", "");
            UIDynamicTextField baseDebugInfoField = CreateTextField(hairSimHandler.baseDebugInfo);
            baseDebugInfoField.height = 300;
            baseDebugInfoField.UItext.fontSize = 24;
#endif
        }

        private void InitPluginUIRight()
        {
            hairSimHandler.statusUIText = new JSONStorableString("statusText", "");
            UIDynamicTextField statusUITextField = CreateTextField(hairSimHandler.statusUIText, rightSide: true);
            statusUITextField.UItext.fontSize = 28;
            statusUITextField.height = 100;

            maxMainRigidity = NewSlider("Max main rigidity", def: 0.025f, max: 0.100f, rightSide: true);
            NewSpacer(10f, true);
            maxTipRigidity = NewSlider("Max tip rigidity", def: 0.002f, max: 0.010f, rightSide: true);
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

        #endregion User interface

        private void InitListeners()
        {
            upperAngleLimit.slider.onValueChanged.AddListener((float val) =>
            {
                if(val < lowerAngleLimit.val)
                {
                    lowerAngleLimit.val = val;
                    hairSimHandler.UpdateLowerLimit(val);
                }
                hairSimHandler.UpdateUpperLimit(val);
            });
            lowerAngleLimit.slider.onValueChanged.AddListener((float val) =>
            {
                if(val > upperAngleLimit.val)
                {
                    upperAngleLimit.val = val;
                    hairSimHandler.UpdateUpperLimit(val);
                }
                hairSimHandler.UpdateLowerLimit(val);
            });
        }

        public void OnEnable()
        {
            if(hairSimHandler != null)
            {
                hairSimHandler.enabled = true;
            }
        }

        public void OnDisable()
        {
            if(hairSimHandler != null)
            {
                hairSimHandler.RestoreOriginalPhysics();
                hairSimHandler.enabled = false;
            }
        }

        public void OnDestroy()
        {
            hairSimHandler.RestoreOriginalPhysics();
            Destroy(gameObject.GetComponent<HairSimHandler>());
        }
    }
}
