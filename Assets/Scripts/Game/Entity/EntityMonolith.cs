using System.Collections.Generic;
using System.Linq;

using UnityEngine;

[System.Serializable]
public class DataMonolith
{
    public int keyArea;
    [SerializeField] public List<Vector3Int> portalPoints;
    public DataMonolith()
    {
        portalPoints = new List<Vector3Int>();
    }
}

[System.Serializable]
public class EntityMonolith : BaseEntity
{
    [SerializeField] public DataMonolith Data = new DataMonolith();
    public ScriptableEntityPortal ConfigData => (ScriptableEntityPortal)ScriptableData;
    public EntityMonolith(ScriptableEntityPortal configData, SaveDataUnit<DataMonolith> saveData = null)
    {
        base.Init();

        if (saveData == null)
        {
            // List<ScriptableEntityPortal> list = ResourceSystem.Instance
            //     .GetEntityByType<ScriptableEntityPortal>(TypeEntity.Portal)
            //     .ToList();
            // ScriptableData = list[UnityEngine.Random.Range(0, list.Count)];
            ScriptableData = configData;
            _idEntity = ScriptableData.idObject;
        }
        else
        {
            ScriptableData = ResourceSystem.Instance
                .GetEntityByType<ScriptableEntityMapObject>(TypeEntity.MapObject)
                .Where(t => t.idObject == saveData.idEntity && t.TypeMapObject == TypeMapObject.Portal)
                .First();

            Data = saveData.data;
            _id = saveData.id;
            _idEntity = saveData.idEntity;
            Effects = saveData.Effects;
        }
    }

    public override async void SetPlayer(Player player)
    {
        ScriptableEntityPortal configData = (ScriptableEntityPortal)ScriptableData;
        await configData.RunHero(player, this);
        // Debug.Log($"Teleport to position::: {Data.portalPoints[0]}");
    }

    #region SaveLoadData
    // public void LoadDataPlay(DataPlay data)
    // {
    //     throw new System.NotImplementedException();
    // }

    public override void SaveEntity(ref DataPlay data)
    {
        var sdata = SaveUnit(Data);
        data.entity.monoliths.Add(sdata);
    }
    #endregion
}
