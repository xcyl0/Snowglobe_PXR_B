
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WallSocketDetector : UdonSharpBehaviour
{
    [SerializeField] private Animator _animator;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "fork")
        {
            Debug.Log("INSERTING A FORK INTO THIS SOCKET");
            other.gameObject.SetActive(false);
            _animator.Play("InsertFork");
        }
    }
}
