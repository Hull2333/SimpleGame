using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace MFarm.Inventory 
{
    public class SkillBar : MonoBehaviour //调用在PlayerSkillPanel对象上
    {
        [Header("等级经验条")]
        public Image plantingBar;
        public Image cultivetionBar;
        public Image fishingBar;
        public Image fightBar;
        public Image exploreBar;
        [Header("技能等级")]
        public Text plantingLevel;
        public Text cultivetionLevel;
        public Text fishingLevel;
        public Text fightLevel;
        public Text exploreLevel;
        [Header("当前等级经验值")]
        public Text currentPlantingExp;
        public Text currentCultivetionExp;
        public Text currentFishingExp;
        public Text currentFightExp;
        public Text currentExploreExp;
        [Header("当前等级所需经验值")]
        public Text maxPlantingExp;
        public Text maxCultivetionExp;
        public Text maxFishingExp;
        public Text maxFightExp;
        public Text maxExploreExp;
        [Header("满级提示")]
        public Text plantMax;
        public Text cultivetionMax;
        public Text fishingMax;
        public Text fightMax;
        public Text exploreMax;
        private PlayerController playerController => FindObjectOfType<PlayerController>();
        /// <summary>
        /// 更新玩家技能等级和经验
        /// </summary>
        public void UpdateSkillBar()
        {
            plantingBar.fillAmount = (float)playerController.plantingCurrentEXP / (float)playerController.plantingMaxEXP;
            cultivetionBar.fillAmount = (float)playerController.cultivetionCurrentEXP / (float)playerController.cultivetionMaxEXP;
            fishingBar.fillAmount = (float)playerController.fishingCurrentEXP / (float)playerController.fishingMaxEXP;
            fightBar.fillAmount = (float)playerController.fightCurrentEXP / (float)playerController.fightMaxEXP;
            exploreBar.fillAmount = (float)playerController.exploreCurrentEXP / (float)playerController.exploreMaxEXP;
            plantingLevel.text = "Lv" + playerController.plantingSkill;
            cultivetionLevel.text = "Lv" + playerController.cultivetionSkill;
            fishingLevel.text = "Lv" + playerController.fishingSkill;
            fightLevel.text = "Lv" + playerController.fightSkill;
            exploreLevel.text = "Lv" + playerController.exploreSkill;
            currentPlantingExp.text = playerController.plantingCurrentEXP.ToString();
            currentCultivetionExp.text = playerController.cultivetionCurrentEXP.ToString();
            currentFishingExp.text = playerController.fishingCurrentEXP.ToString();
            currentFightExp.text = playerController.fightCurrentEXP.ToString();
            currentExploreExp.text = playerController.exploreCurrentEXP.ToString();
            maxPlantingExp.text = playerController.plantingMaxEXP.ToString();
            maxCultivetionExp.text = playerController.cultivetionMaxEXP.ToString();
            maxFishingExp.text = playerController.fishingMaxEXP.ToString(); 
            maxFightExp.text = playerController.fightMaxEXP.ToString();
            maxExploreExp.text= playerController.exploreMaxEXP.ToString(); 
            if(playerController.plantingSkill == 10)
            {
                plantMax.enabled = true;
            }
            if(playerController.cultivetionSkill == 10)
            {
                cultivetionMax.enabled = true;
            }
            if(playerController.fishingSkill == 10)
            {
                fishingMax.enabled = true;
            }
            if(playerController.fightSkill == 10)
            {
                fightMax.enabled = true;
            }
            if(playerController.exploreSkill == 10)
            {
                exploreMax.enabled = true;
            }
        }
    }
}


