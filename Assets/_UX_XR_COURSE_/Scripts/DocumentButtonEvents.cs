using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;

public class DocumentButtonEvents : MonoBehaviour
{
    [SerializeField]
    private UnityEvent<string> _onButtonClicked = new UnityEvent<string>();

    public UnityEvent<string> onButtonClicked { get => _onButtonClicked; set => _onButtonClicked = value; }

    private Button _button;
    private List<Button> _buttons = new List<Button>();

    private UIDocument _doc;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        /*
        var doc = GetComponent<UIDocument>();
        if (doc != null)
        {
            var root = doc.rootVisualElement;

            // Alternativ 1
            // if only one button
            _button = root.Q<Button>() as Button;

            // Or use directly ID
            // if many buttons
            //_button = root.Q<Button>("ButtonCTA") as Button;
            
            if (_button != null)
            {
                Debug.Log("WORK REG");
                // No dont do that!
                _button.clicked += TriggerButtonEventSimple;

                // _button.RegisterCallback<ClickEvent>(TriggerButtonEvent);
            }

            // Alternativ 2
            // if many buttons
            _buttons = root.Query<Button>().ToList();

            foreach (var b in _buttons) {
                b.RegisterCallback<ClickEvent>(TriggerButtonEvent);
                Debug.Log("WORK");

            }

            Debug.Log("WORK");

        }
        */

        _doc = GetComponent<UIDocument>();

        _buttons = _doc.rootVisualElement.Query<Button>().ToList();

        foreach (var b in _buttons)
        {
            b.RegisterCallback<ClickEvent>(TriggerButtonEvent);
            Debug.Log("Adding New Events");
        }
    }

    private void OnEnable()
    {
        if (_doc == null || _doc.rootVisualElement == null) return;

        /*
        // (Re)bind all Buttons under this document
        _doc.rootVisualElement.Query<Button>().ForEach(b =>
        {
            Debug.Log("OnEnable");
            _buttons.Add(b);
            // IMPORTANT: use currentTarget in handler (target can be the internal Label)
            b.RegisterCallback<ClickEvent>(TriggerButtonEvent);
        });
        */


        //if (_button != null) _button.RegisterCallback<ClickEvent>(TriggerButtonEvent);

        /*
        foreach (var b in _buttons)
        {
            b.RegisterCallback<ClickEvent>(TriggerButtonEvent);
        }
        */
     
    }

    private void OnDisable()
    {
        /*
        foreach (var b in _buttons)
        {
            if (b != null)
            {
                b.UnregisterCallback<ClickEvent>(TriggerButtonEvent);
                Debug.Log("OnEnable");
            }
            _buttons.Clear();
        }
        */

        //if (_button != null) _button.UnregisterCallback<ClickEvent>(TriggerButtonEvent);
        /*
        foreach (var b in _buttons)
        {
            b.UnregisterCallback<ClickEvent>(TriggerButtonEvent);
        }
        */
       
    }

    // Update is called once per frame
    void TriggerButtonEventSimple()
    {
        Debug.Log("WORK1");

        if (_onButtonClicked != null)
        {
            Debug.Log("WORK2");

            Debug.Log("WORK3");

            string id = "";
            _onButtonClicked?.Invoke(id);
        }
    }

    // Update is called once per frame
    void TriggerButtonEvent(ClickEvent evt)
    {
        Debug.Log("T1");
        if (_onButtonClicked != null)
        {
            Debug.Log("T2");
            Button b = evt.target as Button;
            if (b != null)
            {
                Debug.Log("T3");
                string id = b.userData as string;
                Debug.Log("Clicked : " + id);
                _onButtonClicked?.Invoke(id);

            }
        }
    }
}
