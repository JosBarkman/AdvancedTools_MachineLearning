using System.Collections.Generic;
using UnityEngine;
using TMPro;



public class AdvancedToolsArea : MonoBehaviour {
    public GameObject TargetPrefab => _targetPrefab;
    public GameObject ObstaclePrefab => _obstaclePrefab;



    [Tooltip( "The agent inside the area" )]
    [SerializeField]
    private AdvancedToolsAgent _advancedToolsAgent = null;

    [Tooltip( "A prefab for the target object to be reached" )]
    [SerializeField]
    private GameObject _targetPrefab = null;

    [Tooltip( "The TextMeshPro text that shows the cumulative reward of the agent" )]
    [SerializeField]
    private TextMeshPro _cumulativeRewardText = null;

    [Tooltip( "Prefab of an obstacle" )]
    [SerializeField]
    private GameObject _obstaclePrefab = null;

    [Tooltip( "The amount of obstacles that are spawned at the start of the simulation" )]
    [SerializeField, Range( 0, 40 )]
    private int _startingObstacles = 0;

    [Tooltip( "The amount of obstacles spawned every second" )]
    [SerializeField, Range( 0, 10 )]
    private float _obstaclesPerSecond = 0f;

    [Tooltip( "" )]
    [SerializeField, Range( 0, 40 )]
    private int _maxObstacles = 0;


    private List<GameObject> _obstacleList = new List<GameObject>();
    private DataGatherer _dataGatherer = null;
    private GameObject _target = null;
    private float _counter = 0;



    /// <summary>
    /// Called when the game starts
    /// </summary>
    private void Start() {
        ResetArea();
        _dataGatherer = GetComponent<DataGatherer>();
    }


    /// <summary>
    /// Called every frame
    /// </summary>
    private void Update() {
        // Update the cumulative reward text
        _cumulativeRewardText.text = _advancedToolsAgent.GetCumulativeReward().ToString( "0.00" );
    }


    /// <summary>
    /// Called periodically in fixed timesteps
    /// </summary>
    private void FixedUpdate() {
        if ( _obstaclesPerSecond > Mathf.Epsilon && ObstaclesInPlay < _maxObstacles ) {
            float maxCount = 1f / _obstaclesPerSecond;

            if ( _counter <= 0f ) {
                // If we fail to spawn an obstacle because of overlap with an agent or target, we don't reset the timer
                // At the next tick, we can try to spawn an obstacle again
                if ( !SpawnObstacles( 1 ) ) {
                    _counter += maxCount;
                }
            }
            _counter -= Time.fixedDeltaTime;
        }
    }


    /// <summary>
    /// Reset the area, including obstacle and agent placement
    /// </summary>
    public void ResetArea() {
        RemoveAllObjects();
        SpawnTarget();
        PlaceAgent();
        SpawnObstacles( _startingObstacles );
    }


    /// <summary>
    /// At the end of the episode, gather data and calculate the statistics
    /// </summary>
    public void NotifyEnd( float reward, int steps, int episodes ) {
        _dataGatherer.Clock( reward, steps, episodes );
    }


    /// <summary>
    /// Choose a random position on the X-Z plane within a partial donut shape
    /// </summary>
    /// <param name="pCenter">The center of the donut</param>
    /// <param name="pMinAngle">Minimum angle of the wedge</param>
    /// <param name="pMaxAngle">Maximum angle of the wedge</param>
    /// <param name="pMinRadius">Minimum distance from the center</param>
    /// <param name="pMaxRadius">Maximum distance from the center</param>
    /// <returns>A position falling within the specified region</returns>
    public static Vector3 ChooseRandomPosition( Vector3 pCenter, float pMinAngle, float pMaxAngle, float pMinRadius, float pMaxRadius ) {
        float radius = pMinRadius;
        float angle = pMinAngle;

        if ( pMaxRadius > pMinRadius ) {
            // Pick a random radius
            radius = Random.Range( pMinRadius, pMaxRadius );
        }

        if ( pMaxAngle > pMinAngle ) {
            // Pick a random angle
            angle = Random.Range( pMinAngle, pMaxAngle );
        }

        // Center position + forward vector rotated around the Y axis by "angle" degrees, multiplies by "radius"
        return pCenter + Quaternion.Euler( 0f, angle, 0f ) * Vector3.forward * radius;
    }


    /// <summary>
    /// The number of obstacles remaining
    /// </summary>
    public int ObstaclesInPlay {
        get { return _obstacleList.Count; }
    }


    /// <summary>
    /// Remove all obstacles from the area
    /// </summary>
    private void RemoveAllObjects() {
        if ( _obstacleList != null ) {
            for ( int i = 0; i < _obstacleList.Count; i++ ) {
                if ( _obstacleList[i] != null ) {
                    Destroy( _obstacleList[i] );
                }
            }
        }
        Destroy( _target );

        _obstacleList = new List<GameObject>();
        _target = null;
    }


    /// <summary>
    /// Place the agent in the area
    /// </summary>
    private void PlaceAgent() {
        Rigidbody rigidbody = _advancedToolsAgent.GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        // Calculate a starting position for the target
        _advancedToolsAgent.transform.position = ChooseRandomPosition( transform.position, 0f, 180f, 4.5f, 10f );
        _advancedToolsAgent.transform.rotation = Quaternion.Euler( 0f, UnityEngine.Random.Range( 0f, 360f ), 0f );
    }


    /// <summary>
    /// Spawn target location for the agent to go to
    /// </summary>
    private void SpawnTarget() {
        // Check if the target is already present
        if ( null == _target ) {
            // Spawn and place the target
            _target = Instantiate( _targetPrefab );
        }

        // Calculate a starting position for the target
        _target.transform.position = ChooseRandomPosition( transform.position, 180f, 360f, 4.5f, 10f );
        _target.transform.rotation = Quaternion.Euler( 0f, UnityEngine.Random.Range( 0f, 360f ), 0f );

        // Set the target's parent to this area's transform
        _target.transform.SetParent( transform );
    }


    /// <summary>
    /// Spawn obstacle between agent and target
    /// </summary>
    private bool SpawnObstacles( int pCount ) {
        bool retry = false;
        for ( int i = 0; i < pCount; ++i ) {
            // We calculate a point between the agent and the target
            Vector2 randomDir = Random.insideUnitCircle;
            Vector3 flatTarget = new Vector3( _target.transform.position.x, 0, _target.transform.position.z );
            Vector3 flatAgent = new Vector3( _advancedToolsAgent.transform.position.x, 0, _advancedToolsAgent.transform.position.z );
            Vector3 centerPos = flatTarget - ( flatTarget - flatAgent ) * 0.5f;
            Vector3 obstaclePos = new Vector3( centerPos.x, _obstaclePrefab.transform.position.y, centerPos.z ) + new Vector3( randomDir.x, 0, randomDir.y ).normalized * Random.Range( 1.25f, 3f );

            // Obstacles are allowed to overlap with each other, but not with the agent or the target
            bool placedSuccessfully = true;
            Ray ray = new Ray( obstaclePos, Vector3.up );
            float obstacleSize = Mathf.Pow( _obstaclePrefab.transform.lossyScale.x * _obstaclePrefab.transform.lossyScale.y * _obstaclePrefab.transform.lossyScale.z, 1f / 3f );
            RaycastHit[] hits = Physics.SphereCastAll( ray, obstacleSize * Mathf.Sqrt( 2f ), 0 );

            for ( int j = 0; j < hits.Length; ++j ) {
                RaycastHit hit = hits[j];
                if ( hit.transform.CompareTag( "agent" ) || hit.transform.CompareTag( "Finish" ) ) {
                    placedSuccessfully = false;
                    break;
                }
            }

            // Spawn an obstacle if the placement is not hindered
            if ( placedSuccessfully ) {
                // Spawn and place the target
                GameObject obstacle = Instantiate( _obstaclePrefab );
                obstacle.transform.position = obstaclePos;
                obstacle.transform.rotation = Quaternion.Euler( 0f, UnityEngine.Random.Range( 0f, 360f ), 0f );

                // Set the target's parent to this area's transform
                obstacle.transform.SetParent( transform );

                // Keep track of the target
                _obstacleList.Add( obstacle );
            }
            else {
                retry = true;
            }
        }
        return retry;
    }
}
