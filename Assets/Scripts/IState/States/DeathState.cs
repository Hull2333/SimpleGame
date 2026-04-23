
public class DeathState : IState
{
    private FSM manager;
    private Parameter parameter;

    public DeathState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }
    public void OnEnter()
    {
        parameter.animator.Play("Death");
        parameter.collider2D.enabled = false;
    }

    public void OnUpdate()
    {

    }
    public void OnExit()
    {

    }

    public void OnFixUpdate()
    {
       
    }
}
