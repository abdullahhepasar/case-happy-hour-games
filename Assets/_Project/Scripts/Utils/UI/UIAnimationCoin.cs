using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(ObjectPool))]
public class UIAnimationCoin : MonoBehaviour
{
    [Header("Coin Prefab")]
    public GameObject CoinPrefab;
    [Header("Parent")]
    public Transform CoinsParent;
    [Header("Coin Count")]
    public int SpawnCoinCount = 30;
    [Header("Start Transform")]
    public RectTransform StartTransform;
    [Header("Final Transform")]
    public RectTransform FinalTransform;
    [Header("Random Circle Boyutu")]
    public float CircleOffset = 300;
    [Header("First Point Speed")]
    public float FirstLerpDuration = 0.3f;
    public Ease FirstEaseType = Ease.InSine;

    [Header("Final Point Speed")]
    public float FinalLerpDuration = 0.5f;
    public Ease FinalEaseType = Ease.InSine;
    [Header("First Point Wait Timer")]
    public float TimerCount = 1f;
    [Header("Final Point Wait Timer")]
    public float FinalTimerCount = 0.3f;

    private bool AnimationActive = false;

    private List<Vector2> RandomPositions;

    private ObjectPool objectPool;
    private List<GameObject> UICoins = new List<GameObject>();

    private void Start()
    {
        SetPool();
    }

    private void SetPool()
    {
        UICoins = new List<GameObject>();
        objectPool = GetComponent<ObjectPool>();
        objectPool.SetStart(CoinPrefab, SpawnCoinCount);
    }

    /// <summary>
    /// Crowd Coins Animation
    /// </summary>
    public void SetCrowdCoinsAnimation()
    {
        StartCoroutine(SpawnCoins(SpawnCoinCount, false));
    }

    public void SetSingleCoinAnimation()
    {
        StartCoroutine(SpawnCoins(1, true));
    }

    #region Coin Animation

    private IEnumerator SpawnCoins(int count, bool isSingle)
    {
        AnimationActive = true;

        RandomPositions = new List<Vector2>();
        for (int i = 0; i < count; i++)
        {
            RandomPositions.Add(UnityEngine.Random.insideUnitCircle * CircleOffset);
        }

        for (int i = 0; i < count; i++)
        {
            int index = i;

            GameObject tempCoinPrefab = objectPool.GetObject();
            tempCoinPrefab.transform.SetParent(CoinsParent);
            UICoins.Add(tempCoinPrefab);

            tempCoinPrefab.transform.position = StartTransform.position;
            if (SpawnCoinCount - 1 == index)
            {
                //Last
                yield return StartCoroutine(CoinAnimation(tempCoinPrefab, index, isSingle));

                ResetPoolObjects();

                OnCoinAnimationComplete();
            }
            else
            {
                yield return new WaitForSeconds(0.03f);
                StartCoroutine(CoinAnimation(tempCoinPrefab, index, isSingle));
            }
        }
    }

    private IEnumerator CoinAnimation(GameObject coinPrefab, int index, bool isSingle)
    {
        RectTransform coinRectTransform = coinPrefab.transform.GetComponent<RectTransform>();

        if (isSingle)
        {
            coinRectTransform.position = StartTransform.position;
        }
        else
        {
            Vector2 target = RandomPositions[index] + new Vector2(StartTransform.position.x, StartTransform.position.y);
            coinRectTransform.DOMove(target, FirstLerpDuration).SetEase(FirstEaseType);

            yield return new WaitForSeconds(TimerCount);

            coinRectTransform.DOMove(FinalTransform.position, FinalLerpDuration).SetEase(FinalEaseType);

        }

        yield return new WaitForSeconds(FinalTimerCount);

        yield return null;
    }

    private void OnCoinAnimationComplete()
    {
        AnimationActive = false;
    }

    private void ResetPoolObjects()
    {
        for (int i = 0; i < UICoins.Count; i++)
        {
            objectPool.ReturnObject(UICoins[i]);
        }

        UICoins = new List<GameObject>();
    }

    #endregion

    /// <summary>
    /// Get Animation Active
    /// </summary>
    /// <returns></returns>
    public bool GetAnimationActive() => AnimationActive;

}
