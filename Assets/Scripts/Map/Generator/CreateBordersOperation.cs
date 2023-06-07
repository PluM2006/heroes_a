using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Loader;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using Random = UnityEngine.Random;
using UnityEngine.Localization;

public class CreateBordersOperation : ILoadingOperation
{
    private Action<string> _onSetNotify;

    private readonly MapManager _root;

    public CreateBordersOperation(MapManager generator)
    {
        _root = generator;
    }

    public async UniTask Load(Action<float> onProgress, Action<string> onSetNotify)
    {
        _onSetNotify = onSetNotify;
        var task1 = CreateEdgeMountain();
        var task2 = CreateBorderMountain();
        var task3 = CreateRandomMountain();

        await UniTask.WhenAll(task1, task2, task3);

        //await UniTask.Delay(1);
    }

    /// <summary>
    /// Create mountain for border areas
    /// </summary>
    /// <returns>UniTask</returns>
    private async UniTask CreateBorderMountain()
    {
        var t = new LocalizedString(Constants.LanguageTable.LANG_TABLE_UILANG, "createdgameobject").GetLocalizedString();
        _onSetNotify(t + " border mountains ...");


        List<GridTileNode> nodes = _root.gridTileHelper.GetAllGridNodes().Where(t =>
            _root.gridTileHelper.CalculateNeighboursByArea(t) < 4
            && !t.StateNode.HasFlag(StateNode.Disable)
            // && t.Empty
            // && t.Enable
            //&& !t.isEdge
            && (
                t.X > 1 &&
                t.X < _root.gameModeData.width - 2 &&
                t.Y > 1 &&
                t.Y < _root.gameModeData.height - 2
            )
        ).ToList();

        foreach (GridTileNode tileNode in nodes)
        {
            //tileNode.Empty && tileNode.Enable
            if (!tileNode.StateNode.HasFlag(StateNode.Disable))
            {

                TileLandscape tileData = _root._dataTypeGround[tileNode.TypeGround];
                List<TileNature> listNature = ResourceSystem.Instance.GetNature().Where(t =>
                            t.typeGround.HasFlag(tileData.typeGround)
                            && t.isCorner
                        ).ToList();
                TileNature cornerTile = listNature[Random.Range(0, listNature.Count)];

                _root._tileMapNature.SetTile(tileNode.position, cornerTile.tile != null ? cornerTile.tile : cornerTile);

                _root._listNatureNode.Add(new GridTileNatureNode(tileNode, cornerTile.idObject, false, cornerTile.name));

                _root.gridTileHelper.SetDisableNode(tileNode, cornerTile.listTypeNoPath, Color.blue);
            }
        }

        await UniTask.Delay(1);
    }

    /// <summary>
    /// Create mountain for edge map
    /// </summary>
    /// <returns>UniTask</returns>
    private async UniTask CreateEdgeMountain()
    {
        var t = new LocalizedString(Constants.LanguageTable.LANG_TABLE_UILANG, "createdgameobject").GetLocalizedString();
        _onSetNotify(t + " edge mountains ...");


        foreach (GridTileNode tileNode in _root.gridTileHelper.GetAllGridNodes().Where(t =>
            t.isEdge
            && _root.gridTileHelper.CalculateNeighbours(t) >= 5
            && !t.StateNode.HasFlag(StateNode.Disable)
        // && t.Empty
        // && t.Enable
        ))
        {
            TileLandscape tileData = _root._dataTypeGround[tileNode.TypeGround];

            List<TileNature> listNature = ResourceSystem.Instance.GetNature().Where(t =>
                        t.typeGround.HasFlag(tileData.typeGround)
                        && t.isCorner
                    ).ToList();

            TileNature cornerTile = listNature[Random.Range(0, listNature.Count)];

            _root._tileMapNature.SetTile(tileNode.position, cornerTile.tile != null ? cornerTile.tile : cornerTile);

            _root._listNatureNode.Add(new GridTileNatureNode(tileNode, cornerTile.idObject, false, cornerTile.name));

            _root.gridTileHelper.SetDisableNode(tileNode, cornerTile.listTypeNoPath, Color.blue);
        }

        await UniTask.Delay(1);
    }

    private async UniTask CreateRandomMountain()
    {
        var t = new LocalizedString(Constants.LanguageTable.LANG_TABLE_UILANG, "createdgameobject").GetLocalizedString();
        _onSetNotify(t + " perlin mountains ...");


        if (
            LevelManager.Instance.Level.GameModeData.noiseScaleMontain == 0
            || LevelManager.Instance.Level.GameModeData.koofMountains == 0)
        {
            return;
        }
        // Random value for noise.
        var xOffSet = Random.Range(-10000f, 10000f);
        var zOffSet = Random.Range(-10000f, 10000f);

        for (int x = 0; x < _root.gameModeData.width; x++)
        {
            for (int y = 0; y < _root.gameModeData.height; y++)
            {
                GridTileNode currentNode = _root.gridTileHelper.GetNode(x, y);

                float noiseValue = Mathf.PerlinNoise(
                    x * LevelManager.Instance.Level.GameModeData.noiseScaleMontain + xOffSet,
                    y * LevelManager.Instance.Level.GameModeData.noiseScaleMontain + zOffSet
                    );

                bool isMountain = noiseValue < LevelManager.Instance.Level.GameModeData.koofMountains;

                //Area area = LevelManager.Instance.GetArea(currentNode.keyArea);

                //float minCountNoMountain = area.countNode * LevelManager.Instance.koofMountains;

                // Create Mountain.
                if (
                    isMountain
                    && !currentNode.StateNode.HasFlag(StateNode.Disable)
                    && _root.gridTileHelper.GetNeighbourListWithTypeGround(currentNode).Count > 3
                    )
                {
                    TileLandscape tileData = _root._dataTypeGround[currentNode.TypeGround];

                    List<TileNature> listTileForDraw = ResourceSystem.Instance.GetNature().Where(t =>
                        t.typeGround.HasFlag(tileData.typeGround)
                        && t.isCorner
                    ).ToList(); //  _tileData.cornerTiles.Concat(_tileData.natureTiles).ToList();

                    TileNature tileForDraw = listTileForDraw[Random.Range(0, listTileForDraw.Count)];
                    // currentNode.Empty && currentNode.Enable
                    if (!currentNode.StateNode.HasFlag(StateNode.Disable))
                    {
                        _root._tileMapNature.SetTile(currentNode.position, tileForDraw.tile != null ? tileForDraw.tile : tileForDraw);

                        _root._listNatureNode.Add(new GridTileNatureNode(currentNode, tileForDraw.idObject, false, tileForDraw.name));

                        _root.gridTileHelper.SetDisableNode(currentNode, tileForDraw.listTypeNoPath, Color.black);
                    }

                    //area.countMountain++;
                }
            }
        }

        await UniTask.Delay(1);
    }

}
