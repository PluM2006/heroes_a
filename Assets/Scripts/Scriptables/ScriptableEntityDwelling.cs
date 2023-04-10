using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEntityDwelling", menuName = "Game/Entity/MapObject/Dwelling")]
public class ScriptableEntityDwelling : ScriptableEntityMapObject, IEffected
{
    [Header("Options Dwelling")]
    public TypeFaction TypeFaction;
    [SerializeField] public List<BuildCostResource> CostResource;
    [SerializeField] public ScriptableEntityCreature Creature;
    public override void OnDoHero(ref Player player, BaseEntity entity)
    {
        base.OnDoHero(ref player, entity);
        // foreach (var perk in Perks)
        // {
        //     perk.OnDoHero(ref player, entity);
        // }
    }
}
