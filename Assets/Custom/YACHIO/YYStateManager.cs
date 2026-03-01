using UnityEngine;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using TMPro;
[RequireComponent(typeof(SignalManager))]
public class YYStateManager : MonoBehaviour
{
    public TextMeshProUGUI debugger;
    public ProcessingPipeline processingPipeline;
    [HideInInspector]
    public SignalManager signalManager;

    [SerializeField] public InputAction stopButton;

    public IAssistantState CurrentState { get; private set; }
    public YYState initState;

    private bool isStarted = false;

    void Awake()
    {
        isStarted = false;
        signalManager = GetComponent<SignalManager>();
    }
    async void Start()
    {
        signalManager.SendSignal("yya_start", "Connecting to server...");
        while (!processingPipeline.IsStarted() && !processingPipeline.ErrorInStart())
        {
            await Task.Delay(100);
        }
        Init();
    }

    void OnDisable()
    {
        stopButton.Disable();
    }

    void Init()
    {
        if (processingPipeline.IsStarted())
        {
            isStarted = true;
            Debug.Log("Pipeline started");
            signalManager.SendSignal("yya_start", "started");
            CurrentState = initState;
            CurrentState.EnterState();
            stopButton.Enable();
        }
        else
        {
            Debug.LogError("Pipeline not started, please check the configuration");
            signalManager.SendSignal("yya_start", "Error in start");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isStarted)
        {
            CurrentState.UpdateState();
        }
    }

    public void SwitchState(IAssistantState newState)
    {
        CurrentState.ExitState();
        CurrentState = newState;
        CurrentState.EnterState();
    }
}
