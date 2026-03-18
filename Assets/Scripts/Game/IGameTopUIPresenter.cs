using UnityEngine;

public interface IGameTopUIPresenter
{
    void OnAttach(GameTopUI view);
    void OnDetach();
    void Tick(float dt);

    void OnReStage();
}

public abstract class TopUIPresenterBase : IGameTopUIPresenter
{
    protected GameTopUI V;
    public virtual void OnAttach(GameTopUI view) { V = view; }
    public virtual void OnDetach() { }
    public virtual void Tick(float dt) { }

    public virtual void OnReStage() { }

    protected static void SetActive(Component c, bool on) { if (c) c.gameObject.SetActive(on); }
}