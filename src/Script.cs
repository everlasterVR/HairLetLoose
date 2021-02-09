using SimpleJSON;
using System;

namespace HairLetLoose
{
    internal class Script : MVRScript
    {
        private string pluginVersion = "0.0.0";

        private HairSimHandler hairSimHandler;

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
                hairSimHandler.Init(containingAtom);
                InitPluginUIRight();
                hairSimHandler.UpdateLimits();
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

            UISliderStore.Init();
            NewSlider(UISliderStore.dummyLowerAngleLimit, valueFormat: "F0");
            NewSlider(UISliderStore.dummyUpperAngleLimit, valueFormat: "F0");
            NewSlider(UISliderStore.dummyMinMainRigidity, valueFormat: "F3");
            NewSlider(UISliderStore.dummyMaxMainRigidity, valueFormat: "F3");
            NewSlider(UISliderStore.dummyMinTipRigidity, valueFormat: "F4");
            NewSlider(UISliderStore.dummyMaxTipRigidity, valueFormat: "F4");
            NewSlider(UISliderStore.dummyMinStyleCling, valueFormat: "F2");
            NewSlider(UISliderStore.dummyMaxStyleCling, valueFormat: "F2");
            UISliderStore.StoreSliders();
        }

        private void InitPluginUIRight()
        {
            hairSimHandler.CreateHairSelect();
            UIDynamicPopup hairUISelectPopup = CreatePopup(hairSimHandler.hairUISelect, rightSide: true);
            hairUISelectPopup.height = 100;

            JSONStorableString helpUIText = new JSONStorableString("helpText", "");
            UIDynamicTextField helpUITextField = CreateTextField(helpUIText, rightSide: true);
            helpUITextField.UItext.fontSize = 26;
            helpUITextField.height = 255;
            helpUIText.SetVal($"<b><size=30>How it works</size></b>\n\n" +
                $"Hair is the least rigid at the lower limit angle, and the most rigid at the upper limit angle.\n\n" +
                $"90° is upright, 0° is horizontal, -90° is upside down.");

            hairSimHandler.valuesUIText = new JSONStorableString("valuesText", "");
            UIDynamicTextField valuesUITextField = CreateTextField(hairSimHandler.valuesUIText, rightSide: true);
            valuesUITextField.UItext.fontSize = 26;
            valuesUITextField.height = 255;

            hairSimHandler.settingsInfoUIText = new JSONStorableString("logText", "");
            UIDynamicTextField logUITextField = CreateTextField(hairSimHandler.settingsInfoUIText, rightSide: true);
            logUITextField.UItext.fontSize = 26;
            logUITextField.height = 525;
        }

        private void NewSlider(
            JSONStorableFloat storable,
            string valueFormat, // 3 decimal places
            bool rightSide = false
        )
        {
            UIDynamicSlider slider = CreateSlider(storable, rightSide);
            slider.valueFormat = valueFormat;
        }

        public override JSONClass GetJSON(bool includePhysical = true, bool includeAppearance = true, bool forceStore = false)
        {
            JSONClass json = base.GetJSON(includePhysical, includeAppearance, forceStore);

            //var animationJSON = new JSONClass
            //{
            //    { "Speed", animation.speed.ToString(CultureInfo.InvariantCulture) },
            //    { "Master", animation.master ? "1" : "0" }
            //};
            //var clipsJSON = new JSONArray();
            //foreach(var clip in animation.clips.Where(c => animationNameFilter == null || c.animationName == animationNameFilter))
            //{
            //    clipsJSON.Add(SerializeClip(clip));
            //}
            //animationJSON.Add("Clips", clipsJSON);
            //return animationJSON;

            //json["HairPhysics"] = serializer.SerializeAnimation(animation);
            //json["Options"] = AtomAnimationSerializer.SerializeEditContext(animationEditContext);

            return json;
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
                hairSimHandler.RestoreAllOriginalPhysics();
                //hairSimHandler.NullifyCurrent();
                hairSimHandler.enabled = false;
            }
        }

        public void OnDestroy()
        {
            hairSimHandler.RestoreAllOriginalPhysics();
            Destroy(gameObject.GetComponent<HairSimHandler>());
        }
    }
}
