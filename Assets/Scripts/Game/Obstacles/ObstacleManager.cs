using UnityEngine;
using System.Collections.Generic;

public class ObstacleManager : MonoBehaviour
{
    const float DISTANCE_BETWEEN_OBSTACLES = 6f;
    const float COIN_DISTANCE_BETWEEN_OBSTACLES = 10f;
    const float HEIGHT_RANDOM = 3f;
    const float COIN_HEIGHT_RANDOM = 2f;
    const int MIN_COUNT = 3;
    const int COIN_MIN_COUNT = 10;
    public GameObject prefab;
    public GameObject prefabCoin;
    Vector3 pos = new Vector3(DISTANCE_BETWEEN_OBSTACLES, 0, 0);
    Vector3 coinPos = new Vector3(DISTANCE_BETWEEN_OBSTACLES, 0, 0);

    List<Obstacle> obstacles = new List<Obstacle>();
    List<Coin> coins = new List<Coin>();

    private static ObstacleManager instance = null;

    public static ObstacleManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<ObstacleManager>();

            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    public void Reset()
    {
        for (int i = 0; i < obstacles.Count; i++)
            Destroy(obstacles[i].gameObject);
        for (int i = 0; i < coins.Count; i++)
            Destroy(coins[i].gameObject);
        obstacles.Clear();
        coins.Clear();
        pos.x = 0;
        coinPos.x = 0;
        InstantiateObstacle();
        InstantiateObstacle();
        InstantiateCoin();
    }

    public Obstacle GetNextObstacle(Vector3 pos)
    {
        for (int i = 0; i < obstacles.Count; i++)
        {
            if (pos.x < obstacles[i].transform.position.x + 2f)
                return obstacles[i];
        }

        return null;
    }

    public Coin GetNextCoin(Vector3 pos)
    {
        for (int i = 0; i < coins.Count; i++)
        {
            if (pos.x < coins[i].transform.position.x + 2f)
                return coins[i];
        }

        return null;
    }

    public bool IsColliding(Vector3 pos)
    {
        Collider2D collider = Physics2D.OverlapBox(pos, new Vector2(0.3f, 0.3f), 0);

        if (collider != null)
            return true;

        return false;
    }

    public void CheckAndInstatiate()
    {
        for (int i = 0; i < obstacles.Count; i++)
        {
            obstacles[i].CheckToDestroy();
        }
        
        for (int i = 0; i < coins.Count; i++)
        {
            coins[i].CheckToDestroy();
        }

        while (obstacles.Count < MIN_COUNT)
            InstantiateObstacle();

        while (coins.Count < COIN_MIN_COUNT)
            InstantiateCoin();
    }

    void InstantiateObstacle()
    {
        pos.x += DISTANCE_BETWEEN_OBSTACLES;
        pos.y = Random.Range(-HEIGHT_RANDOM, HEIGHT_RANDOM);
        GameObject go = GameObject.Instantiate(prefab, pos, Quaternion.identity);
        go.transform.SetParent(this.transform, false);
        Obstacle obstacle = go.GetComponent<Obstacle>();
        obstacle.OnDestroy += OnObstacleDestroy;
        obstacles.Add(obstacle);
    }

    void InstantiateCoin()
    {
        coinPos.x += COIN_DISTANCE_BETWEEN_OBSTACLES;
        coinPos.y = Random.Range(-COIN_HEIGHT_RANDOM, COIN_HEIGHT_RANDOM);
        GameObject go = GameObject.Instantiate(prefabCoin, coinPos, Quaternion.identity);
        go.transform.SetParent(this.transform, false);
        Coin coin = go.GetComponent<Coin>();
        coin.OnDestroy += OnCoinDestroy;
        coin.id = coins.Count;
        coins.Add(coin);
    }

    void OnObstacleDestroy(Obstacle obstacle)
    {
        obstacle.OnDestroy -= OnObstacleDestroy;
        obstacles.Remove(obstacle);
    }

    void OnCoinDestroy(Coin coin)
    {
        coin.OnDestroy -= OnCoinDestroy;
        coins.Remove(coin);
    }
}
