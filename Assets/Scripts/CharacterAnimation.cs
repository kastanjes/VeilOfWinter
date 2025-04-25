using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    Animator animator;
    public float velocity = 0.0f;
    public float acceleration = 0.1f;
    public float deceleration = 0.1f;
    public float idleCycleInterval = 6f;

    private float idleCycleTimer = 0f;
    private float idleSwitch = 0f;

    int VelocityHash;
    int IdleSwitchHash;
    int RunStopTriggerHash;

    float targetIdleSwitch = 0f;

    private bool wasPressingMovementKeysLastFrame = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        VelocityHash = Animator.StringToHash("Velocity");
        IdleSwitchHash = Animator.StringToHash("IdleSwitch");
        RunStopTriggerHash = Animator.StringToHash("RunStopTrigger");
    }

    void Update()
    {
        // Track movement input directly
        bool isPressingMovementKeys = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                                      Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

        // Trigger RunStop if keys were pressed last frame and now none are
        if (wasPressingMovementKeysLastFrame && !isPressingMovementKeys)
        {
            animator.ResetTrigger(RunStopTriggerHash);
            animator.SetTrigger(RunStopTriggerHash);
        }

        wasPressingMovementKeysLastFrame = isPressingMovementKeys;

        // Velocity handling
        if (isPressingMovementKeys && velocity < 1.0f)
            velocity += Time.deltaTime * acceleration;
        else if (!isPressingMovementKeys && velocity > 0.0f)
            velocity -= Time.deltaTime * deceleration;

        velocity = Mathf.Clamp01(velocity);
        animator.SetFloat(VelocityHash, velocity);

        // Idle blending
        if (velocity <= 0.01f)
        {
            idleCycleTimer += Time.deltaTime;
            if (idleCycleTimer >= idleCycleInterval)
            {
                targetIdleSwitch = 1f - targetIdleSwitch;
                idleCycleTimer = 0f;
            }
            idleSwitch = Mathf.MoveTowards(idleSwitch, targetIdleSwitch, Time.deltaTime * 0.5f);
        }
        else
        {
            idleCycleTimer = 0f;
            targetIdleSwitch = 0f;
            idleSwitch = Mathf.MoveTowards(idleSwitch, 0f, Time.deltaTime * 5f);
        }

        animator.SetFloat(IdleSwitchHash, idleSwitch);
    }
}
