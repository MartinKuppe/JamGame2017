using UnityEngine;
using System.Collections;

public class CharacterControl : MonoBehaviour
{
    public float speed;
    private Animator _animator;

    private Transform _camTransform;
    private Vector3 _camLocalPos;

    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

        _camTransform = Camera.main.transform;
        _camTransform.SetParent(transform.parent, false);
    }

    // Update is called once per frame
    void Update()
    {
        _camTransform.position = new Vector3(transform.position.x, transform.position.y, _camTransform.position.z);

        float inputHorizontal = Input.GetAxis("Horizontal");
        float inputVertical = Input.GetAxis("Vertical");

        Vector3 newVelocity = new Vector3(inputHorizontal * speed, inputVertical * speed, 0.0f);
        _rigidbody.velocity = newVelocity;

        float animVal = Mathf.Max(Mathf.Abs(inputHorizontal), Mathf.Abs(inputVertical));
        _animator.SetFloat("Velocity", animVal);

        if(inputHorizontal < 0)
        {
            transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        else if (inputHorizontal > 0)
        {
            transform.localEulerAngles = new Vector3(0, 180, 0);
        }
    }
}