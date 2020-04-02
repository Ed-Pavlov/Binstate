using JetBrains.Annotations;

namespace Binstate
{
  public interface IStateMachine
  {
    bool InMyState { get; }
    void Fire([NotNull] object trigger);
    void Fire<T>([NotNull] object trigger, [NotNull] T parameter);
  }
}