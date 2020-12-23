using PiTung.Components;

class CustomLabel : UpdateHandler
{
    protected override void CircuitLogicUpdate()
    {
    }

    private Label DaRealLabel;

    protected override void OnAwake()
    {
        DaRealLabel = GetComponentInChildren<Label>();
    }

    [SaveThis]
    public string LabelText
    {
        get
        {
            return DaRealLabel.text.text;
        }
        set
        {
            GetComponentInChildren<Label>().text.text = value;
        }
    }

    [SaveThis]
    public float LabelTextSize
    {
        get
        {
            return DaRealLabel.text.fontSize;
        }
        set
        {
            GetComponentInChildren<Label>().text.fontSize = value;
        }
    }
}
