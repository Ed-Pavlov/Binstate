namespace Binstate
{
  public interface IStateMachine
  {
    bool InMyState { get; }
    void Fire(object trigger);
    void Fire<T>(object trigger, T parameter);
  }
}