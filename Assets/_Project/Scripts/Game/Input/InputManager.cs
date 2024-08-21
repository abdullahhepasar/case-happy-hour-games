using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static InputManager Instance;

    // Change this to according the desired precision
    [SerializeField] private float maxDistance = 100f;

    public bool isHolding = false;
    public PointerEventData eventData;
    public Vector2 Input { get; private set; }

    private Vector2 startPos;
    private Vector2 delta;
    
    [HideInInspector] public UnityEvent<PointerEventData> onPointerDown = new UnityEvent<PointerEventData>();
    [HideInInspector] public UnityEvent<PointerEventData> onPointerUp = new UnityEvent<PointerEventData>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Update()
    {
        if (eventData == null) return;

        delta = eventData.position - startPos;
        delta.x = Mathf.Clamp(delta.x, -maxDistance, maxDistance);
        delta.y = Mathf.Clamp(delta.y, -maxDistance, maxDistance);
        Input = delta / maxDistance;
        startPos = eventData.position;
    }

    public void OnPointerDown(PointerEventData _eventData)
    {
        eventData = _eventData;
        startPos = eventData.position;
    
        isHolding = true;

        onPointerDown.Invoke(_eventData);
    }

    public void OnPointerUp(PointerEventData _eventData)
    {
        eventData = null;
        delta = Vector2.zero;
        startPos = Vector2.zero;
        Input = Vector2.zero;

        isHolding = false;

        onPointerUp.Invoke(_eventData);
    }
}
