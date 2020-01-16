using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Message : MonoBehaviour
{
    [SerializeField]
    private Text MessageText;
    [SerializeField]
    private Image Panel;
    public static Message Instance { get; private set; }
    private float _startTime;
    public float delay = 1;
    public float speed = 1;
	void Awake ()
	{
	    Instance = this;
	}

    public void Show(string message)
    {
        _startTime = Time.time;
        MessageText.text = message;
        this.enabled = true;
    }

    void FixedUpdate()
    {
        float ratio = Mathf.Min(delay * Mathf.Sin((Time.time - _startTime) * speed), 1);

        if (ratio >= 0)
        {
            Panel.color = Panel.color.EditColor(a: ratio);
            MessageText.color = MessageText.color.EditColor(a: ratio);
        }
        else
        {
            Panel.color = Panel.color.EditColor(a: 0);
            MessageText.color = MessageText.color.EditColor(a: 0);
            this.enabled = false;
        }
    }
}
