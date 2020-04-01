using System.Threading.Tasks;

namespace Binstate
{
  public interface IStateMachine
  {
    bool InMyState { get; }
    void Fire(object trigger);
    Task FireAsync(object trigger);
    void Fire<T>(object trigger, T parameter);
    Task FireAsync<T>(object trigger, T parameter);
  }
}