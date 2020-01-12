using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public float MoveSpeed = 10f;
    public float SprintSpeed = 20f;
    public float RotateSpeed = 250f;
    public Transform ArtTransform;

    public Slider StaminaBar;
    public Image StaminaFill;
    public Color ReadyColor;
    public Color CooldownColor;

    private float _currentStamina = 100f;
    private float _maxStamina = 100f;
    private float _thresholdStamina = 20f;

    public TrailRenderer MovementTrail;
    private float _moveTime = 0.2f;
    private float _moveWidth = 0.04f;
    private float _sprintTime = 0.3f;
    private float _sprintWidth = 0.1f;

    private float _currentSpeed = 10f;
    private Animator _animator;
    private Vector3 _moveVector = Vector3.zero;
    private Quaternion _rotateQuat = Quaternion.identity;
    private bool _moving = false;
    private bool _sprinting = false;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _moveVector = Vector3.zero;
        _rotateQuat = Quaternion.identity;
        _moving = false;
        _sprinting = false;
        _currentStamina = 100f;
        _maxStamina = 100f;
        _thresholdStamina = 20f;

        StartCoroutine(RegenRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        _moveVector = new Vector3(
            Input.GetAxis("Horizontal"),
            0,
            Input.GetAxis("Vertical"));

        if (Input.GetKeyDown(KeyCode.LeftShift) && _currentStamina > _thresholdStamina)
        {
            _sprinting = true;
            _currentSpeed = SprintSpeed;
            MovementTrail.startWidth = Mathf.Max(_currentStamina / _maxStamina * _sprintWidth, _moveWidth);
            MovementTrail.time = Mathf.Max(_currentStamina / _maxStamina * _sprintTime, _moveTime);
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) || _currentStamina == 0f)
        {
            _sprinting = false;
            MovementTrail.startWidth = _moveWidth;
            MovementTrail.time = _moveTime;
        }

        StaminaBar.value = _currentStamina / _maxStamina;
        StaminaFill.color = _currentStamina > _thresholdStamina ? ReadyColor : CooldownColor;

        if (!_sprinting)
        {
            _currentSpeed = MoveSpeed;
        }

        _moveVector = _moveVector.normalized * _currentSpeed;

        if (_moveVector != Vector3.zero)
        {
            if (!_moving)
            {
                _moving = true;
                _animator.SetBool("Moving", true);
            }
            _rotateQuat = Quaternion.LookRotation(_moveVector);
        }
        else if (_moveVector == Vector3.zero && _moving)
        {
            _moving = false;
            _animator.SetBool("Moving", false);
        }

        transform.Translate(_moveVector * Time.deltaTime);
        ArtTransform.rotation = Quaternion.Slerp(ArtTransform.rotation, _rotateQuat, RotateSpeed * Time.deltaTime);
    }

    private IEnumerator RegenRoutine()
    {
        while (true)
        {
            if (_sprinting)
            {
                _currentStamina = Mathf.Max(0, _currentStamina - 2);
                MovementTrail.startWidth = Mathf.Max(_currentStamina / _maxStamina * _sprintWidth, _moveWidth);
                MovementTrail.time = Mathf.Max(_currentStamina / _maxStamina * _sprintTime, _moveTime);
            }
            else if (_currentStamina < _maxStamina)
                _currentStamina = Mathf.Min(100f, _currentStamina + 1);

            yield return new WaitForSeconds(0.05f);
        }
    }
}
