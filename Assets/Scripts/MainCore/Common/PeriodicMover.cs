using System.Diagnostics;
using UnityEngine;

public class PeriodicMover : MonoBehaviour
{
    private Vector3 originPosition; 
    [SerializeField] private float amplitude = 1f, period = 1f;
    
    private Stopwatch stopwatch = new Stopwatch();

    // Start is called before the first frame update
    private void Awake()
    {
        originPosition = transform.position;
    }

    private void Start()
    {
        stopwatch.Start();
        Update();
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position = originPosition + new Vector3(0f,
            amplitude * Mathf.Sin(2 * Mathf.PI * stopwatch.ElapsedMilliseconds / 1000f / period), 0f);
    }

    private void OnDisable()
    {
        transform.position = originPosition;
    }
}