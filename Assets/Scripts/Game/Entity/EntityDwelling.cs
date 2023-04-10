using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[System.Serializable]
public struct DataEntityDwelling
{

}

[System.Serializable]
public class EntityDwelling : BaseEntity, ISaveDataPlay
{
    [SerializeField] public DataEntityDwelling Data = new DataEntityDwelling();
    public ScriptableEntityDwelling ConfigData => (ScriptableEntityDwelling)ScriptableData;
    public EntityDwelling(
        GridTileNode node,
        ScriptableEntityDwelling configData,
        SaveDataUnit<DataEntityDwelling> saveData = null)
    {
        if (saveData == null)
        {
            ScriptableData = configData;
        }
        else
        {
            ScriptableData = ResourceSystem.Instance
                .GetEntityByType<ScriptableEntityMapObject>(TypeEntity.MapObject)
                .Where(t => t.idObject == saveData.idObject && t.TypeMapObject == TypeMapObject.Dwelling)
                .First();
            Data = saveData.data;
            idUnit = saveData.idUnit;
        }

        base.Init(ScriptableData, node);
    }

    public override void SetPlayer(Player player)
    {
        ScriptableEntityDwelling configData = (ScriptableEntityDwelling)ScriptableData;
        configData.OnDoHero(ref player, this);
    }

    #region SaveData
    public void SaveDataPlay(ref DataPlay data)
    {
        var sdata = SaveUnit(Data);
        data.entity.dwellings.Add(sdata);
    }
    #endregion
}