using System;

namespace HairLetLoose
{
    internal class Script : MVRScript
    {
        private string pluginVersion = "0.0.0";

        private HairSimHandler hairSimHandler;

        //registered storables
        private JSONStorableFloat upperAngleLimit;

        private JSONStorableFloat lowerAngleLimit;
        private JSONStorableFloat minMainRigidity;
        private JSONStorableFloat maxMainRigidity;
        private JSONStorableFloat minTipRigidity;
        private JSONStorableFloat maxTipRigidity;
        private JSONStorableFloat minStyleCling;
        private JSONStorableFloat maxStyleCling;

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
                hairSimHandler.Init(
                    containingAtom,
                    minMainRigidity,
                    maxMainRigidity,
                    minTipRigidity,
                    maxTipRigidity,
                    minStyleCling,
                    maxStyleCling
                );
                hairSimHandler.LoadHairSim();
                hairSimHandler.UpdateUpperLimit(upperAngleLimit.val);
                hairSimHandler.UpdateLowerLimit(lowerAngleLimit.val);
            }
            catch(Exception e)
            {
                Log.Error($"Exception caught: {e}");
                hairSimHandler.enabled = false;
            }
        }

        #region User interface

        private void InitPluginUILeft()
        {
            JSONStorableString titleUIText = new JSONStorableString("titleText", "");
            UIDynamicTextField titleUITextField = CreateTextField(titleUIText);
            titleUITextField.UItext.fontSize = 30;
            titleUITextField.height = 100;
            titleUIText.SetVal($"<b>{nameof(HairLetLoose)}</b>\n<size=26>v{pluginVersion}</size>");

            lowerAngleLimit = NewSlider("Lower limit <size=40>°</size>", def: 45f, min: -90f, max: 90f, valueFormat: "F0");
            upperAngleLimit = NewSlider("Upper limit <size=40>°</size>", def: 90f, min: -90f, max: 90f, valueFormat: "F0");
            //NewSpacer(10f);
            minMainRigidity = NewSlider("Main rigidity at lower limit", def: 0.005f, max: 0.100f);
            maxMainRigidity = NewSlider("Main rigidity at upper limit", def: 0.025f, max: 0.100f);
            //NewSpacer(10f);
            minTipRigidity = NewSlider("Tip rigidity at lower limit", def: 0.000f, max: 0.010f, valueFormat: "F4");
            maxTipRigidity = NewSlider("Tip rigidity at upper limit", def: 0.002f, max: 0.010f, valueFormat: "F4");
            //NewSpacer(10f);
            minStyleCling = NewSlider("Style cling at lower limit", valueFormat: "F2");
            maxStyleCling = NewSlider("Style cling at upper limit", valueFormat: "F2");
        }

        private void InitPluginUIRight()
        {
            hairSimHandler.statusUIText = new JSONStorableString("statusText", "");
            UIDynamicTextField statusUITextField = CreateTextField(hairSimHandler.statusUIText, rightSide: true);
            statusUITextField.UItext.fontSize = 30;
            statusUITextField.height = 100;

            JSONStorableString helpUIText = new JSONStorableString("helpText", "");
            UIDynamicTextField helpUITextField = CreateTextField(helpUIText, rightSide: true);
            helpUITextField.UItext.fontSize = 26;
            helpUITextField.height = 255;
            helpUIText.SetVal($"<b><size=30>How it works</size></b>\n\n" +
                $"Hair is the least rigid at the lower limit angle, and the most rigid at the upper limit angle.\n\n" +
                $"90° is upright, 0° is horizontal, -90° is upside down.");

            hairSimHandler.valuesUIText = new JSONStorableString("statusText", "");
            UIDynamicTextField valuesUITextField = CreateTextField(hairSimHandler.valuesUIText, rightSide: true);
            valuesUITextField.UItext.fontSize = 26;
            valuesUITextField.height = 255;
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

        private void InitListeners()
        {
            lowerAngleLimit.slider.onValueChanged.AddListener((float val) =>
            {
                if(val > upperAngleLimit.val)
                {
                    upperAngleLimit.val = val;
                    hairSimHandler.UpdateUpperLimit(val);
                }
                hairSimHandler.UpdateLowerLimit(val);
            });
            upperAngleLimit.slider.onValueChanged.AddListener((float val) =>
            {
                if(val < lowerAngleLimit.val)
                {
                    lowerAngleLimit.val = val;
                    hairSimHandler.UpdateLowerLimit(val);
                }
                hairSimHandler.UpdateUpperLimit(val);
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

        #endregion User interface

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
