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
    [field: Range(1, 16)][field: Min(1)][field: SerializeField] public int[] Genetics { get; set; } = new int[4];

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