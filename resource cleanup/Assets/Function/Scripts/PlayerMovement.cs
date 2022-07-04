using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("움직임")]
    [SerializeField]private float moveSpeed;
    public float walkSpeed;
    public float runSpeed;
    public float slowlyWalkSpeed;

    public float groundDrag;

    [Header("앉기")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("경사각")]
    public float maxSlopeAngle; // 최대 경사각
    private RaycastHit slopeHit;
    

    [Header("키코드")]
    public KeyCode runKey = KeyCode.Space;
    public KeyCode slowWalkKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("바닥 체크")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("기타등등")]
    public Transform orientation;
    
    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    // 스태미너
    private StatusController statusController;

    [Header("움직임 상태")]
    public MovementState state;
    public enum MovementState
    {
        walking,
        running,
        slowlyWalking,
        crouching
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startYScale = transform.localScale.y;
        statusController = FindObjectOfType<StatusController>();
    }

    void Update()
    {
        // 바닥 체크
        // 플레이어 키의 절반에서 약간 더 긴 길이로 레이를 쏨.
        grounded = Physics.Raycast(transform.position,Vector3.down, playerHeight * 0.5f + 0.5f, whatIsGround);
        Debug.DrawRay(transform.position,Vector3.down,Color.red);
        // 플레이어 입력 메서드 호출
        PlayerInput();
        // 속도 제어 메서드 호출
        SpeedControl();
        // 상태 적용 메서드 호출
        StateHandler();

        // drag(오브젝트가 힘으로 움직일 때 공기 저항력의 값) 적용
        if (grounded) // 바닥인 경우 
        {
            // 공기 저항력을 변수 값으로 설정
            rb.drag = groundDrag;
        }
        else // 바닥이 아닌 경우
        {
            // 공기 저항력 = 0
            rb.drag = 0;
        }
    }

    private void FixedUpdate() 
    {
        // 플레이어 이동 호출
        MovePlayer();
    }   

    // 플레이어 입력 받기
    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // 앉기 시작
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // 앉기 멈춤
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    // 어떤 키의 입력에 따라 상태를 변화시키기 위한 상태 적용 메서드
    private void StateHandler()
    {
        // Crouching
        if (grounded && Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }
        // Running
        if (grounded && Input.GetKey(runKey))
        {
            if(statusController.GetCurrentSP()>0f)
            {
                state = MovementState.running;
                moveSpeed = runSpeed;
                statusController.DecreaseStamina(1f);
            }
            else if (statusController.GetCurrentSP()<=0f)
            {
                state = MovementState.walking;
                moveSpeed = walkSpeed;
            }
        }
        // SlowlyWalking
        else if (grounded && Input.GetKey(slowWalkKey))
        {
            state = MovementState.slowlyWalking;
            moveSpeed = slowlyWalkSpeed;
        }
        // Walking
        else if(grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
    }    

    // 플레이어 이동 기능
    private void MovePlayer()
    {
        // 이동 방향을 계산
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // 경사각 위일 때
        if (OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
        }
        // 바닥에 닿았을 때
        if (grounded)
        {
            // 플레이어에 힘 추가 - 이동속도만큼 곱하기
            rb.AddForce(moveDirection.normalized * moveSpeed * 5f, ForceMode.Force);
        }

        // 경사면에 있을 시 중력을 없앰
        rb.useGravity = !OnSlope();
    }

    // 플레이어 속도 제어
    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // 필요한 경우 속도 제한
        if ( flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    // 경사각을 위한 bool값
    private bool OnSlope()
    {   // 레이캐스트 실행(경사면에 부딫힌 물체정보 저장)
        if ( Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            // 기울기가 얼만큼 가파른지 계산하기 위한 각도 벡터
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            // 각도가 0이 아닌 maxSlopeAngle보다 작으면 true반환
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    // 기울기를 기준으로 한 방향을 위한 벡터
    private Vector3 GetSlopeMoveDirection()
    {   
        // 평면(이동방향으로 통과, 레이가 닿는 표면의 법선)
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
