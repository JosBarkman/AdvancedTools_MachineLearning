using System;
using UnityEngine;


public class RotateSelf : MonoBehaviour {
    [Tooltip( "The speed at which the object rotates around its own axis" )]
    [SerializeField, Range( 0, 40f )]
    private float _rotationSpeed = 0.5f;



    private void FixedUpdate() {
        transform.Rotate( new Vector3( 0, _rotationSpeed, 0 ) * Time.fixedDeltaTime );
    }
}
