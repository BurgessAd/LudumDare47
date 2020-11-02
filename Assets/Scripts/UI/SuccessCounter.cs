public class SuccessCounter : CounterBase
{
    protected override void Start()
    {
        base.Start();
        m_GameManager.RegisterSuccessCounter(this);
    }
}
