public class DefeatCounter : CounterBase
{
    protected override void Start()
    {
        base.Start();
        m_GameManager.RegisterDefeatCounter(this);
    }
}
