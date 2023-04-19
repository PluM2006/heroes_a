using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.UIElements;


public class UITown : UILocaleBase
{
    [SerializeField] public UnityAction OnHideSetting;
    public static event Action OnExitFromTown;
    [SerializeField] public UnityAction OnSave;
    [SerializeField] private UIDocument _uiDoc;
    [SerializeField] private UITownInfo _uiTownInfo;
    [SerializeField] private UnityEngine.UI.Image _bgImage;
    [SerializeField] private GameObject _townGameObject;

    // private const string _nameBox = "TownPanel";
    private VisualElement _box;
    private const string _nameButtonClose = "ButtonClose";
    private SceneInstance _townScene;

    private EntityTown _activeTown;
    private Player _activePlayer;
    private string NameOverlay = "Overlay";
    // private string _nameBlokHeroVisit = "TownHeroVisitForce";
    // private VisualElement _blokHeroVisit;

    private Camera _cameraMain;
    // private AsyncOperationHandle<GameObject> _townPrefabAsset;
    public ScriptableBuildTown _activeBuildTown;
    private AsyncOperationHandle<ScriptableBuildTown> _asset;

    public void Init(SceneInstance townScene)
    {
        _cameraMain = Camera.main;
        _cameraMain.gameObject.SetActive(false);

        Player activePlayer = LevelManager.Instance.ActivePlayer;
        _activePlayer = activePlayer;
        _activeTown = _activePlayer.ActiveTown;

        // Init town prefab.
        ScriptableEntityTown scriptDataTown = (ScriptableEntityTown)_activeTown.ScriptableData;
        _activeBuildTown = scriptDataTown.BuildTown;
        // var operationHandle = Addressables.LoadAssetAsync<ScriptableBuildTown>(scriptDataTown.BuildTown);


        // await operationHandle.Task;
        // _asset = operationHandle;
        // if (operationHandle.Status == AsyncOperationStatus.Succeeded)
        // {
        //     _activeBuildTown = operationHandle.Result;
        // }
        // // _activeBuildTown = ResourceSystem.Instance.GetBuildTowns().Where(t => t.TypeFaction == scriptDataTown.TypeFaction).First();
        // // _townPrefabAsset = Addressables.InstantiateAsync(
        // //     activeBuildTown.Prefab,
        // //     _townGameObject.gameObject.transform.position,
        // //     Quaternion.identity,
        // //     _townGameObject.transform);

        // // await _townPrefabAsset.Task;

        // // _townGameObject = GameObject.FindGameObjectWithTag("Town");
        // DrawBuilds(new DataResultBuildDialog());

        // // if (_townGameObject != null)
        // // {
        // //     foreach (BuildBase build in _townGameObject.transform.GetComponentsInChildren<BuildBase>())
        // //     {
        // //         build.Init(this);
        // //     }
        // // }

        _bgImage.sprite = _activeBuildTown.Bg;
        _box = _uiDoc.rootVisualElement;

        _uiTownInfo.Init(_box);

        // foreach (var t in activeBuildTown.Builds)
        // {
        //     foreach (var b in t.BuildLevels)
        //     {
        //         Debug.Log($"RequireBuilds= {b.RequiredBuilds}");
        //         Debug.Log($"RequireBuilds= {System.Convert.ToString((byte)b.RequiredBuilds, 2)}");
        //         Debug.Log($"TypeBuild= {b.TypeBuild.ToString()}");
        //         Debug.Log($"TypeBuild= {System.Convert.ToString((byte)b.TypeBuild, 2)}");
        //         Debug.Log($"May be build= {b.RequiredBuilds & _activeTown.Data.ProgressBuilds}");
        //     }
        // }
        _townScene = townScene;

        var btnClose = _box.Q<TemplateContainer>(_nameButtonClose).Q<Button>("Btn");
        btnClose.clickable.clicked += OnClickClose;

        // _bgCanvas.worldCamera = Camera.main;
        // Camera.main.transform.position = new Vector3(-20, 0, -10);
        // Camera.main.orthographicSize = 5.2f;


        // Fill overlay color player.
        Color color = _activePlayer.DataPlayer.color;
        color.a = LevelManager.Instance.ConfigGameSettings.alphaOverlay;
        UQueryBuilder<VisualElement> builder = new UQueryBuilder<VisualElement>(_box);
        List<VisualElement> list = builder.Name(NameOverlay).ToList();
        foreach (var overlay in list)
        {
            overlay.style.backgroundColor = color;
        }

        // _blokHeroVisit = _box.Q<VisualElement>(_nameBlokHeroVisit);
        base.Localize(_box);
    }


    private async void OnClickClose()
    {
        _cameraMain.gameObject.SetActive(true);

        // Release asset prefab town.
        await GameManager.Instance.AssetProvider.UnloadAdditiveScene(_townScene);
        if (_asset.IsValid())
        {
            Addressables.ReleaseInstance(_asset);
            // Addressables.ReleaseInstance(_townPrefabAsset);
        }
        OnExitFromTown?.Invoke();
        // var activeBuildTown = ResourceSystem.Instance.GetBuildTowns().Where(t => t.TypeFaction == TypeFaction.Castle).First();
        // // ResourceSystem.Instance.DestroyAssetsByLabel(Constants.Labels.LABEL_BUILD_TOWN);
        // ResourceSystem.Instance.DestroyAsset(activeBuildTown);

        // Camera.main.transform.position = _activePlayer.ActiveTown.gameObject.transform.position - new Vector3(0, 0, 10);
    }

    // public void DrawBuilds(DataResultBuildDialog result)
    // {
    //     // if (_townGameObject != null)
    //     // {
    //     //     foreach (BaseBuild build in _townGameObject.transform.GetComponentsInChildren<BaseBuild>())
    //     //     {
    //     //         GameObject.Destroy(build.gameObject);
    //     //     }
    //     // }

    //     // for (int i = 0; i < _activeBuildTown.Builds.Count; i++)
    //     // {
    //     //     var bl = _activeBuildTown.Builds[i];
    //     //     int activeLevel = -1;
    //     //     for (int x = 0; x < bl.BuildLevels.Count; x++)
    //     //     {
    //     //         var buildLevel = bl.BuildLevels[x];
    //     //         if (_activePlayer.ActiveTown.Data.ProgressBuilds.Contains(buildLevel.TypeBuild))
    //     //         {
    //     //             activeLevel = x;
    //     //         }
    //     //     }
    //     //     if (activeLevel >= 0)
    //     //     {
    //     //         DrawBuildWithActiveLevel(bl, activeLevel, result.build.TypeBuild);
    //     //     }
    //     // }
    // }

    // private void DrawBuildWithActiveLevel(ScriptableBuilding build, int i, TypeBuild TypeCreateBuild)
    // {
    //     // var boxTownGameObject = GameObject.FindGameObjectWithTag("Town");
    //     // var obj = Instantiate(
    //     //     build.Prefab,
    //     //     boxTownGameObject.gameObject.transform.position,
    //     //     Quaternion.identity,
    //     //     boxTownGameObject.transform
    //     //     );

    //     // obj.Init(this);
    //     // if (TypeCreateBuild == build.BuildLevels[i].TypeBuild)
    //     // {
    //     //     // Debug.Log($"Pulse {build.name}");
    //     //     StartCoroutine(obj.Pulse());
    //     // }

    //     // if (i > 0 && build.BuildLevels[i].UpdatePrefab != null)
    //     // {
    //     //     var objUpdate = Instantiate(
    //     //     build.BuildLevels[i].UpdatePrefab,
    //     //     boxTownGameObject.gameObject.transform.position,
    //     //     Quaternion.identity,
    //     //     boxTownGameObject.transform
    //     //     );

    //     //     objUpdate.Init(this);
    //     //     if (TypeCreateBuild == build.BuildLevels[i].TypeBuild)
    //     //     {
    //     //         // Debug.Log($"Pulse {build.name}");
    //     //         StartCoroutine(objUpdate.Pulse());
    //     //     }
    //     // }
    //     // else
    //     // {
    //     //     for (int y = 0; y < obj.transform.childCount; y++)
    //     //     {
    //     //         // obj.TypeBuild = build.BuildLevels[i].TypeBuild;

    //     //         Transform child = obj.transform.GetChild(y);
    //     //         if (null == child)
    //     //             continue;

    //     //         // obj.LevelBuild = y;
    //     //         if (i == y)
    //     //         {
    //     //             child.gameObject.SetActive(true);
    //     //         }
    //     //         else
    //     //         {
    //     //             child.gameObject.SetActive(false);
    //     //         }
    //     //     }
    //     // }
    // }
}

