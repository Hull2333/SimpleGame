
public interface IState
{

    //状态的进入，持续，退出
    void OnEnter();

    void OnUpdate();

    void OnFixUpdate();

    void OnExit();
}
