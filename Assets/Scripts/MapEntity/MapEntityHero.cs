using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using Cysharp.Threading.Tasks;

using UnityEngine;

//[System.Serializable]
public class MapEntityHero : BaseMapEntity
{
    private bool _canMove = false;
    // public static event Action<EntityHero> onChangeParamsActiveHero;

    [NonSerialized] private Animator _animator;
    [NonSerialized] private Transform _model;

    private CancellationTokenSource cancelTokenSource;


    #region Unity methods
    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponentInChildren<Animator>();
        _model = transform.Find("Model");
    }
    //private void OnDestroy() => GameManager.OnBeforeStateChanged -= OnChangeGameState;

    // public override void OnBeforeStateChanged(GameState newState)
    // {
    //     base.OnBeforeStateChanged(newState);
    //     OnChangeGameState(newState);
    // }
    // public override void OnAfterStateChanged(GameState newState)
    // {
    //     base.OnAfterStateChanged(newState);
    // }

    //protected override void Start()
    //{
    //    base.Start();
    //    // Example usage of a static system
    //    // AudioSystem.Instance.PlaySound(_someSound);
    //}

    #endregion

    public override void InitUnit(MapObject mapObject)
    {

        base.InitUnit(mapObject);
        // MapObjectClass = (EntityHero)mapObject;
        // Data.hit = 100f;
        // Data.speed = 100;
        // Data.name = MapObjectClass.ScriptableData.name;

        if (mapObject.Entity.Player != null)
        {
            SetPlayer(mapObject.Entity.Player);
        }
    }


    public void SetPlayer(Player player)
    {
        Transform flag = transform.Find("Flag");
        flag.GetComponent<SpriteRenderer>().color = player.DataPlayer.color;
    }



    //private Action<GameState> OnChangeGameState()
    //{
    //    throw new NotImplementedException();
    //}


    // private void OnChangeGameState(GameState newState)
    // {
    //     var entityHero = (EntityHero)_mapObject.Entity;
    //     // _canMove = newState == GameState.StartMoveHero && this.MapObjectClass ==
    //     //     LevelManager.Instance.ActivePlayer.DataPlayer.PlayerDataReferences.ActiveHero;
    //     // if (_canMove && entityHero.IsExistPath)
    //     // {
    //     //     entityHero.Data.path.RemoveAt(0);
    //     //     StartCoroutine(MoveHero());

    //     // }

    //     switch (newState)
    //     {
    //         case GameState.NextPlayer:
    //             entityHero.Data.hit = 100f;
    //             var data = (EntityHero)_mapObject.Entity;
    //             data.Data.hit = 100f;
    //             data.Data.mana = 100f;
    //             break;
    //     }
    // }

    public async UniTask StartMove()
    {
        var entityHero = (EntityHero)_mapObject.Entity;
        // _canMove = this.MapObjectClass ==
        //     LevelManager.Instance.ActivePlayer.DataPlayer.PlayerDataReferences.ActiveHero;
        //     _canMove &&
        _canMove = true;
        if (entityHero.IsExistPath)
        {
            if (entityHero.Data.path[0] == _mapObject.OccupiedNode) entityHero.Data.path.RemoveAt(0);
            cancelTokenSource = new CancellationTokenSource();
            await MoveHero(cancelTokenSource.Token);
        }
    }

    private async UniTask MoveHero(CancellationToken cancellationToken)
    {
        var entityHero = (EntityHero)_mapObject.Entity;
        GridTileNode prevNode = MapObject.OccupiedNode;
        while (
            entityHero.Data.path.Count > 0
            && _canMove
            && entityHero.Data.mp >= 1
            && !cancellationToken.IsCancellationRequested
            )
        {
            var nodeTo = entityHero.Data.path[0];
            ScriptableEntityMapObject configNodeData
                = (ScriptableEntityMapObject)nodeTo.OccupiedUnit?.ConfigData;
            Vector3 moveKoof
                = configNodeData?.RulesInput.Count > 0 || nodeTo.StateNode.HasFlag(StateNode.Input)
                    ? new Vector3(.5f, .0f)
                    : new Vector3(.5f, .0f);

            if (nodeTo.StateNode.HasFlag(StateNode.Protected))
            {
                // // GameManager.Instance.ChangeState(GameState.StopMoveHero);
                // // _canMove = false;
                // // _animator.SetBool("isWalking", false);
                // entityHero.ChangeHitHero(nodeTo);
                // await nodeTo.ProtectedUnit.MapObjectGameObject.OnGoHero(MapObjectClass.Player); // LevelManager.Instance.GetPlayer(heroEntity.Data.idPlayer)
                // entityHero.SetPathHero(null);
                // cancelTokenSource.Cancel();
                // cancelTokenSource.Dispose();
                // // break;
                await DoObject(nodeTo, nodeTo.ProtectedUnit.MapObjectGameObject, prevNode);
                cancelTokenSource.Cancel();
                cancelTokenSource.Dispose();
            }

            if (!cancellationToken.IsCancellationRequested && nodeTo.OccupiedUnit != null)
            {
                var maoObj = (ScriptableEntityMapObject)nodeTo.OccupiedUnit.ConfigData;
                if (maoObj.TypeWorkObject == TypeWorkObject.One)
                {
                    // // _canMove = false;
                    // // _animator.SetBool("isWalking", false);
                    // entityHero.ChangeHitHero(nodeTo);
                    // await nodeTo.OccupiedUnit.MapObjectGameObject.OnGoHero(MapObjectClass.Player);
                    // entityHero.SetPathHero(null);
                    // cancelTokenSource.Cancel();
                    // cancelTokenSource.Dispose();
                    // // break;
                    await DoObject(nodeTo, nodeTo.OccupiedUnit.MapObjectGameObject, prevNode);
                    cancelTokenSource.Cancel();
                    cancelTokenSource.Dispose();
                }
            }
            if (!cancellationToken.IsCancellationRequested)
            {
                UpdateAnimate(_mapObject.Position, nodeTo.position);
                _animator.SetBool("isWalking", true);

                // yield return StartCoroutine(
                //     SmoothLerp((Vector3)MapObjectClass.Position + moveKoof, (Vector3)entityHero.Data.path[0].position + moveKoof));
                await SmoothLerp(
                    transform.position,// (Vector3)MapObjectClass.Position + moveKoof,
                    (Vector3)nodeTo.position + moveKoof
                    );

                // await UniTask.Yield();
                // entityHero.SetPositionHero(nodeTo.position);

                entityHero.ChangeHitHero(nodeTo, prevNode);
                entityHero.SetGuestForNode(nodeTo);

                // if (MapObjectClass.Player.DataPlayer.playerType != PlayerType.Bot)
                // {
                GameManager.Instance.MapManager.DrawCursor(entityHero.Data.path, entityHero);
                // }

                entityHero.Data.path.RemoveAt(0);

                if (nodeTo.OccupiedUnit != null)
                {
                    // await nodeTo.OccupiedUnit.MapObjectGameObject.OnGoHero(MapObjectClass.Player);
                    await DoObject(nodeTo, nodeTo.OccupiedUnit.MapObjectGameObject, prevNode);
                    cancelTokenSource.Cancel();
                    cancelTokenSource.Dispose();
                    // break;
                }

            }
            prevNode = nodeTo;
        }
        GameManager.Instance.ChangeState(GameState.StopMoveHero);
        _canMove = false;
        _animator.SetBool("isWalking", false);
    }

    public async UniTask DoObject(GridTileNode nodeTo, BaseMapEntity mapEntity, GridTileNode nodePrev)
    {
        var entityHero = (EntityHero)_mapObject.Entity;

        // _canMove = false;
        _animator.SetBool("isWalking", false);

        await mapEntity.OnGoHero(_mapObject.Entity.Player);
        entityHero.ChangeHitHero(nodeTo, nodePrev);
        entityHero.SetPathHero(null);
    }

    private async UniTask SmoothLerp(Vector3 startPosition, Vector3 endPosition)
    {
        float time = LevelManager.Instance.ConfigGameSettings.speedHero;
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, (elapsedTime / time));
            if (Camera.main != null)
            {
                Camera.main.transform.position = Vector3.Lerp(
                    startPosition + new Vector3(0, 0, -10),
                    endPosition + new Vector3(0, 0, -10),
                    (elapsedTime / time));
            }
            //new Vector3((float)startPosition.x, (float)endPosition.y, -10f);
            elapsedTime += Time.deltaTime;
            // yield return null;
            await UniTask.Yield();
        }
    }

    public void UpdateAnimate(Vector3Int startPosition, Vector3Int endPosition)
    {
        if (_animator != null && startPosition != Vector3Int.zero && endPosition != Vector3Int.zero)
        {
            if (startPosition.x > endPosition.x)
            {
                _model.transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                _model.transform.localScale = new Vector3(1, 1, 1);
            }

            Vector3Int direction = endPosition - startPosition;
            //Debug.Log($"Animator change::: {direction}");
            _animator.SetFloat("X", (float)direction.x);
            _animator.SetFloat("Y", (float)direction.y);

            _animator.SetBool("isWalking", true);
        }
    }

    // public void ChangeHit(GridTileNode node, float _hit)
    // {
    //     var data = (EntityHero)_mapObject.Entity;
    //     data.Data.hit += _hit * data.Data.speed;
    // }

    private void OnMouseDown()
    {
        // Only allow interaction when it's the hero turn
        //if (GameManager.Instance.State != GameState.HeroTurn) return;

        //// Don't move if we've already moved
        //if (!_canMove) return;

        // Show movement/attack options

        // Eventually either deselect or ExecuteMove(). You could split ExecuteMove into multiple functions
        // like Move() / Attack() / Dance()

        //Debug.Log("Unit clicked");

        if (LevelManager.Instance.ActivePlayer.DataPlayer.PlayerDataReferences.ListHero.Contains((EntityHero)this.MapObject.Entity))
        {
            //LevelManager.Instance.GetActivePlayer().SetActiveHero(this);
            EntityHero entityHero = (EntityHero)_mapObject.Entity;
            entityHero.SetHeroAsActive();
            Debug.Log($"Check My hero {name}");
        }
        else
        {
            Debug.Log($"Check Enemy hero {name}");
        }
    }

    //public virtual void ExecuteMove() {
    //    // Override this to do some hero-specific logic, then call this base method to clean up the turn

    //    //_canMove = false;
    //}

    //public override void OnSaveUnit()
    //{
    //    SaveUnit(Data);
    //}
    //public override void OnLoadUnit(SaveDataUnit<DataHero> saveData)
    //{
    //    Data = saveData.data;
    //    LoadUnit(saveData);
    //}

    // public void LoadDataPlay(DataPlay data)
    // {
    //     //throw new System.NotImplementedException();
    // }

    // public void SaveDataPlay(ref DataPlay data)
    // {
    //     var sdata = SaveUnit(Data);
    //     data.Units.heroes.Add(sdata);
    // }

    // public override void OnLoadUnit<T>(SaveDataUnit<T> Data)
    // {
    //     base.OnLoadUnit(Data);
    //     DataHero dh = Data.data as DataHero;
    //     this.Data = dh;
    // }

}