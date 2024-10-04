using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public System.Action<Obstacle> OnDestroy;
    public GameObject coin;
    public float coinMinX;
    public float coinMaxX;
    public float coinMinY;
    public float coinMaxY;

    private void Start()
    {
        //Instantiate(coin, new Vector3(Random.Range(coinMinX, coinMaxX), Random.Range(coinMinY, coinMaxY), 0), Quaternion.identity);
    }

    public void CheckToDestroy()
    {
        if (this.transform.position.x - Camera.main.transform.position.x < -7.5f)
        {
            if (OnDestroy != null)
                OnDestroy.Invoke(this);

            //Destroy(coin);
            Destroy(this.gameObject);
        }

    }
}