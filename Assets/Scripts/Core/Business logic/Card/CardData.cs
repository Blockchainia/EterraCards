using static System.Random;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "Card", menuName = "Card/Create new card", order = 0)]
public class CardData : ScriptableObject, ITableElement
{
    [field: SerializeField] public string Id { get; set; }
    [field: SerializeField] public LocalizedString CardNameKey { get; set; }
    [field: SerializeField] public LocalizedString CardDescriptionKey { get; set; }
    [field: Range(1, 10)][field: Min(1)][field: SerializeField] public int[] Power { get; set; } = new int[4];
    [field: SerializeField] public Sprite CardSprite { get; set; }  
    [field: SerializeField] public ElementType ElementType { get; set; }
    [field: SerializeField] public BrawlType BrawlType { get; set; }
    [field: SerializeField] public bool[] BattleArrow { get; set; } = new bool[4];
    [field: Range(1, 16)][field: Min(1)][field: SerializeField] public int[] Genetics { get; set; } = new int[8];
   
    private void Awake()
    {
        //For Testing Purposes
        //Genetic 0 = Physical Strength
        //Genetic 1 = Spell Strength
        //Genetic 2 = Physical Defense 
        //Genetic 3 = Magic Defense 
        //Genetic 4 = Earth
        //Genetic 5 = Water
        //Genetic 6 = Fire
        //Genetic 7 = Wind
        //Genetic 8 = Void
        UpdateGeneticsArray();

    }
    
    public void UpdateGeneticsArray()
    {
        System.Random random = new System.Random();
        
        for (int i = 0; i < Genetics.Length; i++)
        {
            Genetics[i] = random.Next(1, 17); // Generates a random integer between 1 and 16 (inclusive)
        }
    }
}

public enum ElementType
{
    NONE = 0,
    FIRE = 1,
    ICE = 2,
    WIND = 3,
    EARTH = 4,
    WATER = 5,
    POISON = 6,
    HOLY = 7,
    LIGHTNING = 8,
    DARKNESS = 9
}

public enum BrawlType
{
    PHYSICAL = 0,
    SPELL = 1,
    DEFENSE = 2,
    ACE = 3
}