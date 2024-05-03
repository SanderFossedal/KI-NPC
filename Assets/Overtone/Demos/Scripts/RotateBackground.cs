using UnityEngine;

namespace LeastSquares.Overtone
{
    public class RotateBackground : MonoBehaviour
    {
        [SerializeField] public float speed = 10f;

        void Update()
        {
            transform.Rotate(Vector3.forward * speed * Time.deltaTime);
        }
    }

}