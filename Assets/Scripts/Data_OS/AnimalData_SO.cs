using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "AnimalDataList_SO", menuName = "Animal/AnimalDataList_SO")]
public class AnimalData_SO : ScriptableObject
{
    public List<AnimalDetails> animDetailsList = new List<AnimalDetails>();
    /// <summary>
    /// 根据ID获取动物信息
    /// </summary>
    /// <param name="animalID"></param>
    /// <returns></returns>
    public AnimalDetails GetAnimalDetails(int animalID)
    {
        return animDetailsList.Find(b => b.animalID == animalID);
    }
    [System.Serializable]
    public class AnimalDetails
    {
        //动物ID、大小、名字、图片、价格、价值、幼崽价值、预制体
        public int animalID;
        public int growthDay;
        public AnimalSizeType animSize;
        public string name;
        public Sprite animalSprite;
        public int animalPrice;
        public int animalValue;
        public int animalBabyValue;
        //住的场景名字
        public string coopName;
        public GameObject animalPrefab;
    }
}
