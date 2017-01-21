using UnityEngine;
using System.Collections;

public class CharacterControl : MonoBehaviour
{
    public float speed;
    private Animator animator;

    private Rigidbody2D _rigidbody;

    private SpriteRenderer[] _allRenderers;

    private void Awake()
    {
        _allRenderers = GetComponentsInChildren<SpriteRenderer>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    // Use this for initialization
    void Start()
    {
        animator = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float inputHorizontal = Input.GetAxis("Horizontal");
        float inputVertical = Input.GetAxis("Vertical");

        Vector3 newVelocity = new Vector3(inputHorizontal * speed, inputVertical * speed, 0.0f);
        _rigidbody.velocity = newVelocity;

        if(inputHorizontal > 0)
        {
            foreach(SpriteRenderer r in _allRenderers)
            {
                r.flipX = true;
            }
        }
        else if (inputHorizontal < 0)
        {
            foreach (SpriteRenderer r in _allRenderers)
            {
                r.flipX = false;
            }
        }
    }
}