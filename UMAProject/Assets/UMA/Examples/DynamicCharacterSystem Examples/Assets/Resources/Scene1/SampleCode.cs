﻿// #define PRELOAD_ALL_RACES

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UMA.Examples;

namespace UMA.CharacterSystem.Examples
{
    public class SampleCode : MonoBehaviour
    {
        public DynamicCharacterAvatar Avatar;
        public GameObject SlotPrefab;
        public GameObject WardrobePrefab;
        public GameObject SlotPanel;
        public GameObject WardrobePanel;
        public GameObject ColorPrefab;
        public GameObject DnaPrefab;
        public GameObject LabelPrefab;
        public GameObject GeneralHelpText;
        public GameObject WardrobeHelpText;
        public GameObject ColorsHelpText;
        public GameObject DnaHelpText;
        public GameObject AvatarPrefab;
		public GameObject NoBuildPrefab;
		public MouseOrbitImproved Orbiter;
        public SharedColorTable HairColor;
        public SharedColorTable SkinColor;
        public SharedColorTable EyesColor;
        public SharedColorTable ClothingColor;
		public Dropdown RaceDropdown;
		public GameObject CharacterUI;
        public bool PreloadAndUnload;

		private List<RaceData> races;

		public void Start()
		{
			UMAAssetIndexer index = UMAAssetIndexer.Instance;
            races = index.GetAllAssets<RaceData>();

            // Preload all the races.
            if (RaceDropdown != null)
            {

                RaceDropdown.options.Clear();
                foreach (RaceData race in races)
                {
                    RaceDropdown.options.Add(new Dropdown.OptionData(race.raceName));
                }
            }
            else
            {
                var asyncop = UMAAssetIndexer.Instance.Preload(races, true); // Base races will always be loaded.
                asyncop.Completed += Asyncop_Completed;
            }
		}

		private void Asyncop_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<IList<Object>> obj)
		{
			//Debug.Log("Race Preload Completed.");
			// Preload any default wardrobe items on our avatar, now that the races are preloaded.
			UMAAssetIndexer.Instance.Preload(Avatar,false).Completed += Avatar_Completed;
		}

		private void Avatar_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<IList<Object>> obj)
		{
            // Ready to go, enable the character and build it.
            //Debug.Log("Avatar preload completed.");
            Avatar.gameObject.SetActive(true);
            // Avatar.BuildCharacter();
            Avatar.BuildCharacterEnabled = true;
            //  Avatar.ChangeRace(Avatar.RacePreset);
        }


        public void SliderChange(float value)
        {
            int index = System.Convert.ToInt32(value);
            List<UMATextRecipe> theRecipes = Avatar.AvailableRecipes["Legs"];
            Avatar.SetSlot(theRecipes[index]);
            Avatar.BuildCharacter();
        }


        /// <summary>
        /// Unloads all loaded items.
        /// </summary>
        /// <param name="force"></param>
        public void UnloadAllItems(bool force)
		{
			UMAAssetIndexer.Instance.UnloadAll(force);
		}

		/// <summary>
		/// Remove any controls from the panels
		/// </summary>
		private void Cleanup()
        {
			if (GeneralHelpText != null) GeneralHelpText.SetActive(false);
			if (DnaHelpText != null) DnaHelpText.SetActive(false);
			if (WardrobeHelpText != null) WardrobeHelpText.SetActive(false);
			if (ColorsHelpText != null) ColorsHelpText.SetActive(false);

            foreach (Transform t in SlotPanel.transform)
            {
                UMAUtils.DestroySceneObject(t.gameObject);
            }
            foreach (Transform t in WardrobePanel.transform)
            {
                UMAUtils.DestroySceneObject(t.gameObject);
            }
        }

        public void HelpClick()
        {
            if (GeneralHelpText.activeSelf)
            {
                GeneralHelpText.SetActive(false);
            }
            else
            {
                Cleanup();
                GeneralHelpText.SetActive(true);
            }
        }

        public void WardrobeHelpClick()
        {
            if (WardrobeHelpText.activeSelf)
            {
                WardrobeHelpText.SetActive(false);
            }
            else
            {
                Cleanup();
                WardrobeHelpText.SetActive(true);
            }
        }

        public void ColorsHelpClick()
        {
            if (ColorsHelpText.activeSelf)
            {
                ColorsHelpText.SetActive(false);
            }
            else
            {
                Cleanup();
                ColorsHelpText.SetActive(true);
            }
        }

        public void DNAHelpClick()
        {
            if (DnaHelpText.activeSelf)
            {
                DnaHelpText.SetActive(false);
            }
            else
            {
                Cleanup();
                DnaHelpText.SetActive(true);
            }
        }
        /// <summary>
        /// DNA Button event Handler
        /// </summary>
        public void DnaClick()
        {
            Cleanup();
            Dictionary<string,DnaSetter> AllDNA = Avatar.GetDNA();
            foreach( KeyValuePair<string, DnaSetter> ds in AllDNA)
            {
                // create a button. 
                // set set the dna setter on it.
                GameObject go = GameObject.Instantiate(DnaPrefab);
                DNAHandler ch = go.GetComponent<DNAHandler>();
                ch.Setup(Avatar, ds.Value, WardrobePanel);

                Text txt = go.GetComponentInChildren<Text>();
                txt.text = ds.Value.Name;
                go.transform.SetParent(SlotPanel.transform);
            }
        }

        /// <summary>
        /// Colors Button event handler
        /// </summary>
        public void ColorsClick()
        {
            Cleanup();

            foreach(UMA.OverlayColorData ocd in Avatar.CurrentSharedColors )
            {
                GameObject go = GameObject.Instantiate(ColorPrefab);
                AvailableColorsHandler ch = go.GetComponent<AvailableColorsHandler>();

                SharedColorTable currColors = ClothingColor;

                if (ocd.name.ToLower() == "skin")
                    currColors = SkinColor;
                else if (ocd.name.ToLower() == "hair")
                    currColors = HairColor;
                else if (ocd.name.ToLower() == "eyes")
                    currColors = EyesColor;

                ch.Setup(Avatar, ocd.name, WardrobePanel,currColors);

                Text txt = go.GetComponentInChildren<Text>();
                txt.text = ocd.name;
                go.transform.SetParent(SlotPanel.transform);
            }
        }

        /// <summary>
        /// Wardrobe Button event handler
        /// </summary>
        public void WardrobeClick()
        {
            Cleanup();

            Dictionary<string, List<UMATextRecipe>> recipes = Avatar.AvailableRecipes;

            foreach (string s in recipes.Keys)
            {
                GameObject go = GameObject.Instantiate(SlotPrefab);
                SlotHandler sh = go.GetComponent<SlotHandler>();
                sh.Setup(Avatar, s,WardrobePanel);
                Text txt = go.GetComponentInChildren<Text>();
                txt.text = s;
                go.transform.SetParent(SlotPanel.transform);
            }
        }

        public SharedColorTable SkinColors;
        public SharedColorTable HairColors;

        public void DynamicCreateClick()
        {
            string[] files = { "Fram", "Bob", "Gobs" };
            float x = Random.Range(-8.0f, 8.0f);
            float z = Random.Range(1.0f, 12.0f);
            GameObject go = GameObject.Instantiate(AvatarPrefab);
            DynamicCharacterAvatar dca = go.GetComponent<DynamicCharacterAvatar>();
    #if false
            // this shows how to load it from a string at initialization
            TextAsset t = Resources.Load<TextAsset>("CharacterRecipes/Bob");
            dca.Preload(t.text);
    #else
            // this shows how to load it from a resource file at initialization
            dca.loadPathType = DynamicCharacterAvatar.loadPathTypes.CharacterSystem;
            dca.loadFilename = files[Random.Range(0, 3)];
    #endif
            go.transform.localPosition = new Vector3(x, 0, z);
            go.SetActive(true);
        }

        public void SetRawColorTest()
        {
            OverlayColorData ColorData = Avatar.GetColor("HairColor");
            ColorData.channelMask[0] = new Color(1f, 1f, 1f, 1f);
            ColorData.channelAdditiveMask[0] = new Color(1f, 0f, 0f, 0f);
            Avatar.SetRawColor("HairColor", ColorData, true);
        }

            public void ChangeRace(int index)
		{
			if (Avatar.gameObject.activeSelf)
			{
                if (PreloadAndUnload)
                {
                    UMAAssetIndexer.Instance.UnloadAll(true);
                }

                string race = RaceDropdown.options[index].text;
                Avatar.ChangeRace(race);
            }
            else
            {
                string race = RaceDropdown.options[index].text;
                Avatar.RacePreset = race;
            }

            if (PreloadAndUnload)
            {
                var asyncop = UMAAssetIndexer.Instance.Preload(Avatar, false);
                asyncop.Completed += Avatar_Completed;
            }
            else
            {
                Avatar.gameObject.SetActive(true);
                Avatar.BuildCharacterEnabled = true;
            }
		}

        public void ChangeRaceOld(int index)
        {
#if !PRELOAD_ALL_RACES


            if (Avatar.gameObject.activeSelf)
            {
                    // Destroy the old one.
                    //Debug.Log("Destroying Old Avatar");
                    Avatar.gameObject.SetActive(false);
                    GameObject.Destroy(Avatar.gameObject);

                    //Debug.Log("Unloading everything (except for 'always loaded' items");
                    // unload everything
                    UMAAssetIndexer.Instance.UnloadAll(true);

                    // Create a new avatar
                    //Debug.Log("Instantiating nobuild prefab");
                    GameObject go = GameObject.Instantiate(NoBuildPrefab);
                    Avatar = go.GetComponentInChildren<DynamicCharacterAvatar>();
                    Orbiter.target = go.transform; 
            }

            string race = RaceDropdown.options[index].text;
            Avatar.RacePreset = race;

            // Load just the current race.
            List<RaceData> preloadRaces = new List<RaceData>();
            preloadRaces.Add(races[index]);

            var asyncop = UMAAssetIndexer.Instance.Preload(preloadRaces, false); // We are loading and unloading races.
            asyncop.Completed += Asyncop_Completed;
#else
			// Races are all preloaded, so we can just change to it.
			string race = RaceDropdown.options[index].text;
			Avatar.ChangeRace(race);
#endif
        }

        public void ChangeSex()
        {
            if (Avatar.activeRace.name == "HumanMale")
            {
                Avatar.ChangeRace("HumanFemale");
            }
            else
            {
                Avatar.ChangeRace("HumanMale");
            }
        }

        public void CenterCam()
        {
            Orbiter.Reset();
        }

        public void ToggleUpdateBounds()
        {
            SkinnedMeshRenderer[] sm = FindObjectsOfType<SkinnedMeshRenderer>();
            foreach(SkinnedMeshRenderer smr in sm)
            {
                smr.updateWhenOffscreen = !smr.updateWhenOffscreen;
            }
        }

        public void RandomClick()
        {
            RandomizeAvatar(Avatar);
        }

        private void RandomizeAvatar(DynamicCharacterAvatar Avatar)
        {
            Dictionary<string, List<UMATextRecipe>> recipes = Avatar.AvailableRecipes;

            // Set random wardrobe slots.
            foreach (string SlotName in recipes.Keys)
            {
                int cnt = recipes[SlotName].Count;
                if (cnt > 0)
                {
                    //Get a random recipe from the slot, and apply it
                    int min = -1;
                    if (SlotName == "Legs") min = 0; // Don't allow pants removal in random test
                    int rnd = Random.Range(min, cnt);
                    if (rnd == -1)
                    {
                        Avatar.ClearSlot(SlotName);
                    }
                    else
                    {
                        Avatar.SetSlot(recipes[SlotName][rnd]);
                    }
                }
            }

            // Set Random DNA 
            Dictionary<string, DnaSetter> setters = Avatar.GetDNA();
            foreach (KeyValuePair<string, DnaSetter> dna in setters)
            {
                dna.Value.Set(0.35f + (Random.value * 0.3f));
            }

            // Set Random Colors for Skin and Hair
            int RandHair = Random.Range(0, HairColors.colors.Length);
            int RandSkin = Random.Range(0, SkinColors.colors.Length);

            Avatar.SetColor("Hair", HairColors.colors[RandHair]);
            Avatar.SetColor("Skin", SkinColors.colors[RandSkin]);
            Avatar.BuildCharacter(true);
            Avatar.ForceUpdate(true, true, true);
        }

        public void LinkToAssets()
        {
            Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/search/page=1/sortby=popularity/query=publisher:5619");
        }

        public void ToggleAnimation()
        {
        // RuntimeAnimatorController rac = Avatar.gameObject.GetComponentInChildren<>
        }
    }
}
