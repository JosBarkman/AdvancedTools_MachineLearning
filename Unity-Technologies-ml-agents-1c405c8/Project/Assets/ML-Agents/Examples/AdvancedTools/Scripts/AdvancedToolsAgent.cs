using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class AdvancedToolsAgent : Agent {
    private const string AGENT_TAG = "agent";


    public float Radius => _thiccness;


    [Tooltip( "How fast the agent moves forward" )]
    [SerializeField]
    private readonly float _moveSpeed = 5f;

    [Tooltip( "How fast the agent turns" )]
    [SerializeField]
    private readonly float _turnSpeed = 180f;

    [Tooltip( "The obstacle detection radius of the agent (top-down 2D)." )]
    [SerializeField, Range( 0f, 1f )]
    private readonly float _thiccness = .5f;
    /// NOTE: this detection radius should be scaled along with the agent's model, to roughly cover its size


    new private Rigidbody rigidbody;
    private AdvancedToolsArea _advancedToolsArea;
    private GameObject _target;



    /// <summary>
    /// Initial setup, called when the agent is enabled
    /// </summary>
    public override void Initialize() {
        base.Initialize();
        _advancedToolsArea = GetComponentInParent<AdvancedToolsArea>();
        _target = _advancedToolsArea.TargetPrefab;
        rigidbody = GetComponent<Rigidbody>();
        ConfirmTag();
    }


    /// <summary>
    /// Set the agent object's tag to the default tag name for agents
    /// </summary>
    private void ConfirmTag() {
        tag = AGENT_TAG;
    }


    /// <summary>
    /// Perform actions based on a vector of numbers
    /// </summary>
    /// <param name="actionBuffers">The struct of actions to take</param>
    public override void OnActionReceived( ActionBuffers actionBuffers ) {
        // Convert the first action to forward movement
        // DiscreteActions[0] can either be 0 or 1, indicating whether to remain in place (0) or move forward at full speed (1)
        float forwardAmount = actionBuffers.DiscreteActions[0];

        // Convert the second action to turning left or right
        // DiscreteActions[1] can either be 0, 1, or 2, indicating whether to not turn( 0 ), turn in the negative direction( 1 ), or turn in the positive direction( 2 )
        float turnAmount = 0f;
        if ( actionBuffers.DiscreteActions[1] == 1f ) {
            turnAmount = -1f;
        }
        else if ( actionBuffers.DiscreteActions[1] == 2f ) {
            turnAmount = 1f;
        }

        // Check obstacle collision
        Vector3 newPos = transform.position + transform.forward * forwardAmount * _moveSpeed * Time.fixedDeltaTime;
        Ray ray = new Ray( newPos, Vector3.up );
        RaycastHit[] hits = Physics.SphereCastAll( ray, _thiccness, 0 );
        for ( int i = 0; i < hits.Length; ++i ) {
            RaycastHit hit = hits[i];
            if ( hit.transform.CompareTag( "obstacle" ) ) {
                // We hit an obstacle if we go here, so we have to cancel our movement
                // and apply a negative reward for 'bumping' into it to discourage the agent from
                // trying to move into obstacles
                newPos = transform.position;
                AddReward( -10f / MaxStep );
                break;
            }
        }


        // Apply movement
        rigidbody.MovePosition( newPos );
        transform.Rotate( transform.up * turnAmount * _turnSpeed * Time.fixedDeltaTime );

        // Apply a tiny negative reward every step to encourage action
        if ( MaxStep > 0 ) AddReward( -1f / MaxStep );
    }


    /// <summary>
    /// Read inputs from the keyboard and convert them to a list of actions.
    /// This is called only when the player wants to control the agent and has set
    /// Behavior Type to "Heuristic Only" in the Behavior Parameters inspector.
    /// The agent can be controlled using the W, A, and D keys, where W means forward
    /// and A and D rotate the agent.
    /// </summary>
    /// <returns>A vectorAction array of floats that will be passed into <see cref="AgentAction(float[])"/></returns>
    public override void Heuristic( in ActionBuffers actionsOut ) {
        int forwardAction = 0;
        int turnAction = 0;

        /// NOTE: uses the old input system because at the time this was made, Unity's new input system didn't exist yet
        /// I don't plan on updating it to the new system as I consider that to be outside the scope of this assignment
        if ( Input.GetKey( KeyCode.W ) ) {
            // move forward
            forwardAction = 1;
        }
        if ( Input.GetKey( KeyCode.A ) ) {
            // turn left
            turnAction = 1;
        }
        else if ( Input.GetKey( KeyCode.D ) ) {
            // turn right
            turnAction = 2;
        }

        // Put the actions into the array
        actionsOut.DiscreteActions.Array[0] = forwardAction;
        actionsOut.DiscreteActions.Array[1] = turnAction;
    }


    /// <summary>
    /// When a new episode begins, reset the area
    /// </summary>
    public override void OnEpisodeBegin() {
        _advancedToolsArea.ResetArea();
    }


    /// <summary>
    /// When an episode ends, we notify the area
    /// </summary>
    public override void EndEpisode() {
        SendData();
        base.EndEpisode();
    }


    /// <summary>
    /// When an episode is interrupted, we notify the area
    /// </summary>
    public override void EpisodeInterrupted() {
        SendData();
        base.EpisodeInterrupted();
    }


    /// <summary>
    /// Gather data from the agent's current episode and send it to the the area
    /// </summary>
    private void SendData() {
        float rewardAmount = GetCumulativeReward();
        int stepAmount = m_StepCount;
        int episodeAmount = m_CompletedEpisodes;
        _advancedToolsArea.NotifyEnd( rewardAmount, stepAmount, episodeAmount );
    }


    /// <summary>
    /// Collect all non-Raycast observations
    /// </summary>
    /// <param name="sensor">The vector sensor to add observations to</param>
    public override void CollectObservations( VectorSensor sensor ) {
        /// NOTE: memory-enhanced agents exist, but that is outside of the scope of this assignment
        /// This agent will be guided by raycasts instead to observe the periphery

        // Distance to the target (1 float = 1 value)
        sensor.AddObservation( Vector3.Distance( _target.transform.position, transform.position ) );

        // Direction to target (1 Vector3 = 3 values)
        sensor.AddObservation( ( _target.transform.position - transform.position ).normalized );

        // Direction agent is facing (1 Vector3 = 3 values)
        sensor.AddObservation( transform.forward );

        // 1 + 3 + 3 = 7 total values
    }


    /// <summary>
    /// When the agent collides with something, take action
    /// </summary>
    /// <param name="collision">The collision info</param>
    private void OnCollisionEnter( Collision collision ) {
        if ( collision.transform.CompareTag( "Finish" ) ) {
            // We reached the target
            Win();
        }
    }


    /// <summary>
    /// Get a reward
    /// </summary>
    private void Win() {
        AddReward( 1f );
        EndEpisode();
    }
}
