
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TelescopeController : UdonSharpBehaviour
{

    [SerializeField] private Animator animator;
    [SerializeField] private string _fadeInStateName;
    override public void Interact()
    {
        animator.Play(_fadeInStateName);
    }

}
