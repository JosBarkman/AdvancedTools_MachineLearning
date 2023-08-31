using UnityEngine;


public class Obstacle : MonoBehaviour {
    private const string OBSTACLE_TAG = "obstacle";



    public void OnEnable() {
        ConfirmTag();
    }


    /// <summary>
    /// Set the obstacle object's tag to the default tag name for obstacles
    /// </summary>
    private void ConfirmTag() {
        tag = OBSTACLE_TAG;
    }
}
