using System.IO;
using BepInEx;
using UnityEngine;
using REPOLib.Modules;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace AbioticFactorValuables
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
    public class AbioticFactorValuables : BaseUnityPlugin
    {
        // Config bools
        public static ConfigEntry<bool> enableCrowbar;
        public static ConfigEntry<bool> enableEnergyPistol;
        public static ConfigEntry<bool> enableLodestoneCrossbow;
        public static ConfigEntry<bool> enableSlushieBomb;
        public static ConfigEntry<bool> enableTechScepter;

        public static GameObject freezeExplosionPublic;

        private void Awake()
        {
            string pluginFolderPath = Path.GetDirectoryName(Info.Location);
            string assetBundleFilePath = Path.Combine(pluginFolderPath, "abioticfactorvaluables_assets");
            AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);

            // Config

            enableCrowbar = Config.Bind("Items", "Enable Crowbar", true, "Should the Crowbar be enabled in the shop?");
            enableEnergyPistol = Config.Bind("Items", "Enable Energy Pistol", true, "Should the Energy Pistol be enabled in the shop?");
            enableLodestoneCrossbow = Config.Bind("Items", "Enable Lodestone Crossbow", true, "Should the Lodestone Crossbow be enabled in the shop?");
            enableSlushieBomb = Config.Bind("Items", "Enable Slushie Bomb", true, "Should the Slushie Bomb be enabled in the shop?");
            enableTechScepter = Config.Bind("Items", "Enable Tech Scepter", true, "Should the Tech Scepter be enabled in the shop?");

            // Valuable strings

            List<string> arctic = ["Valuables - Arctic"];
            List<string> manor = ["Valuables - Manor"];
            List<string> wizard = ["Valuables - Wizard"];
            List<string> generic = ["Valuables - Generic"];
            List<string> noarctic = ["Valuables - Manor", "Valuables - Wizard"];
            List<string> nomanor = ["Valuables - Arctic", "Valuables - Wizard"];
            List<string> nowizard = ["Valuables - Arctic", "Valuables - Manor"];

            // Shop items

            if (enableCrowbar.Value)
            {
                Item crowbaritem = assetBundle.LoadAsset<Item>("Item Melee Crowbar");
                Items.RegisterItem(crowbaritem);
            }
            if (enableEnergyPistol.Value)
            {
                Item energypistolitem = assetBundle.LoadAsset<Item>("Item Gun Energy Pistol");
                Items.RegisterItem(energypistolitem);
            }
            if (enableLodestoneCrossbow.Value)
            {
                Item lodestonecrossbowitem = assetBundle.LoadAsset<Item>("Item Crossbow Lodestone");
                Items.RegisterItem(lodestonecrossbowitem);
            }
            if (enableSlushieBomb.Value)
            {
                Item slushiebombitem = assetBundle.LoadAsset<Item>("Item Slushie Bomb");
                Items.RegisterItem(slushiebombitem);
                GameObject freezeexplosion = assetBundle.LoadAsset<GameObject>("Freeze Explosion");
                NetworkPrefabs.RegisterNetworkPrefab(freezeexplosion);
                freezeExplosionPublic = freezeexplosion;
            }
            if (enableTechScepter.Value)
            {
                Item techscepteritem = assetBundle.LoadAsset<Item>("Item Melee Tech Scepter");
                Items.RegisterItem(techscepteritem);
            }

            // McJannek Station valuables
            GameObject anteversegemitem = assetBundle.LoadAsset<GameObject>("Valuable Anteverse Gem");
            GameObject boxofscrewsitem = assetBundle.LoadAsset<GameObject>("Valuable Box of Screws");
            GameObject briefcaseitem = assetBundle.LoadAsset<GameObject>("Valuable Briefcase");
            GameObject deskphoneitem = assetBundle.LoadAsset<GameObject>("Valuable Desk Phone");
            GameObject gatesecuritycrateitem = assetBundle.LoadAsset<GameObject>("Valuable GATE Security Crate");
            GameObject teslacoilitem = assetBundle.LoadAsset<GameObject>("Valuable Tesla Coil");
            GameObject tvforkliftitem = assetBundle.LoadAsset<GameObject>("Valuable TV Forklift Certification");
            GameObject ufsarcademachineitem = assetBundle.LoadAsset<GameObject>("Valuable Unfortunate Spacemen Arcade Machine");
            GameObject vendingmachineitem = assetBundle.LoadAsset<GameObject>("Valuable Snacks Vending Machine");
            GameObject watercooleritem = assetBundle.LoadAsset<GameObject>("Valuable Water Cooler");

            Valuables.RegisterValuable(anteversegemitem, nomanor);
            Valuables.RegisterValuable(boxofscrewsitem, nowizard);
            Valuables.RegisterValuable(briefcaseitem, nowizard);
            Valuables.RegisterValuable(deskphoneitem, arctic);
            Valuables.RegisterValuable(gatesecuritycrateitem, arctic);
            Valuables.RegisterValuable(teslacoilitem, arctic);
            Valuables.RegisterValuable(tvforkliftitem, arctic);
            Valuables.RegisterValuable(ufsarcademachineitem, arctic);
            Valuables.RegisterValuable(vendingmachineitem, arctic);
            Valuables.RegisterValuable(watercooleritem, arctic);

            // Headman Manor valuables
            GameObject cannedpeasitem = assetBundle.LoadAsset<GameObject>("Valuable Can of Peas");
            GameObject creepypaintingitem = assetBundle.LoadAsset<GameObject>("Valuable Creepy Pumpkin Painting");
            GameObject desklegitem = assetBundle.LoadAsset<GameObject>("Valuable Desk Leg");
            GameObject diamondpestitem = assetBundle.LoadAsset<GameObject>("Valuable Diamond Pest Statue");
            GameObject dogphotoitem = assetBundle.LoadAsset<GameObject>("Valuable Dog Photo Frame");
            GameObject brainitem = assetBundle.LoadAsset<GameObject>("Valuable Human Brain");
            GameObject lodestoneitem = assetBundle.LoadAsset<GameObject>("Valuable Lodestone");
            GameObject redchairitem = assetBundle.LoadAsset<GameObject>("Valuable The Red Chair");
            GameObject tvchannel5item = assetBundle.LoadAsset<GameObject>("Valuable TV Channel 5");

            Valuables.RegisterValuable(cannedpeasitem, nowizard);
            Valuables.RegisterValuable(creepypaintingitem, noarctic);
            Valuables.RegisterValuable(desklegitem, nowizard);
            Valuables.RegisterValuable(diamondpestitem, manor);
            Valuables.RegisterValuable(dogphotoitem, nowizard);
            Valuables.RegisterValuable(brainitem, noarctic);
            Valuables.RegisterValuable(lodestoneitem, noarctic);
            Valuables.RegisterValuable(redchairitem, manor);
            Valuables.RegisterValuable(tvchannel5item, manor);

            // Swiftbroom Academy valuables

            GameObject antelightitem = assetBundle.LoadAsset<GameObject>("Valuable Antelight");
            GameObject armorstanditem = assetBundle.LoadAsset<GameObject>("Valuable Armor Stand");
            GameObject cornhuskdollitem = assetBundle.LoadAsset<GameObject>("Valuable Corn Husk Doll");
            GameObject crystallinevialitem = assetBundle.LoadAsset<GameObject>("Valuable Crystalline Vial");
            GameObject foglanternitem = assetBundle.LoadAsset<GameObject>("Valuable Fog Lantern");
            GameObject glowtulipitem = assetBundle.LoadAsset<GameObject>("Valuable Glow Tulip");
            GameObject greyebitem = assetBundle.LoadAsset<GameObject>("Valuable Greyeb");
            GameObject pitchforkitem = assetBundle.LoadAsset<GameObject>("Valuable Pitchfork");
            GameObject reservoirgrowthitem = assetBundle.LoadAsset<GameObject>("Valuable Reservoir Growth");

            Valuables.RegisterValuable(antelightitem, wizard);
            Valuables.RegisterValuable(armorstanditem, wizard);
            Valuables.RegisterValuable(cornhuskdollitem, noarctic);
            Valuables.RegisterValuable(crystallinevialitem, wizard);
            Valuables.RegisterValuable(foglanternitem, wizard);
            Valuables.RegisterValuable(glowtulipitem, wizard);
            Valuables.RegisterValuable(greyebitem, wizard);
            Valuables.RegisterValuable(pitchforkitem, noarctic);
            Valuables.RegisterValuable(reservoirgrowthitem, nomanor);

            // Global valuables
            GameObject anvilitem = assetBundle.LoadAsset<GameObject>("Valuable Anvil");
            GameObject bellitem = assetBundle.LoadAsset<GameObject>("Valuable Unassuming Bell");
            GameObject gravitycubeitem = assetBundle.LoadAsset<GameObject>("Valuable Gravity Cube");
            GameObject nachositem = assetBundle.LoadAsset<GameObject>("Valuable Nachos");
            GameObject rubberbandballitem = assetBundle.LoadAsset<GameObject>("Valuable Rubber Band Ball");
            GameObject saltzitem = assetBundle.LoadAsset<GameObject>("Valuable Saltz");
            GameObject slushieitem = assetBundle.LoadAsset<GameObject>("Valuable Slushie");

            Valuables.RegisterValuable(anvilitem, generic);
            Valuables.RegisterValuable(bellitem, generic);
            Valuables.RegisterValuable(gravitycubeitem, generic);
            Valuables.RegisterValuable(nachositem, generic);
            Valuables.RegisterValuable(rubberbandballitem, generic);
            Valuables.RegisterValuable(saltzitem, generic);
            Valuables.RegisterValuable(slushieitem, generic);
        }
    }
}
