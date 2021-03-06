using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Binstate
{
  public partial class StateMachine<TState, TEvent>
  {
    private class Controller : IStateMachine<TEvent>
    {
      private readonly State<TState, TEvent> _state;
      private readonly StateMachine<TState, TEvent> _owner;

      internal Controller(State<TState, TEvent> state, StateMachine<TState, TEvent> stateMachine)
      {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _owner = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
      }

      public bool InMyState => _state.IsActive;
      
      public bool RaiseAsync([NotNull] TEvent @event) => RaiseAsync<Unit>(@event, default);
      
      public bool RaiseAsync<T>([NotNull] TEvent @event, [CanBeNull] T argument) => RaiseAsyncInternal(@event, argument, Maybe<Unit>.Nothing);

      public IAutoTransition<TEvent> Relaying<TRelay>(bool relayArgumentIsRequired = true) =>
        new ControllerRelayer<TRelay>(this, relayArgumentIsRequired ? Maybe<TRelay>.Nothing : default(TRelay).ToMaybe());
      
      /// <summary>
      /// Implementation shared between <see cref="Controller"/> itself and <see cref="ControllerRelayer{TRelay}"/>
      /// </summary>
      private bool RaiseAsyncInternal<T, TRelay>([NotNull] TEvent @event, [CanBeNull] T argument, Maybe<TRelay> backupValue)
      {
        if (@event == null) throw new ArgumentNullException(nameof(@event));

        var data = _owner.PrepareTransition(@event, argument, backupValue);
        if (data == null) return false;

        Task.Run(() => _owner.PerformTransition(data.Value));
        return true;
      }
      
      private class ControllerRelayer<TRelay> : IAutoTransition<TEvent>
      {
        private readonly Controller _owner;
        private readonly Maybe<TRelay> _backupValue;

        public ControllerRelayer(Controller owner, Maybe<TRelay> backupValue)
        {
          _owner = owner;
          _backupValue = backupValue;
        }

        public bool RaiseAsync(TEvent @event) => RaiseAsync<Unit>(@event, default);

        public bool RaiseAsync<T>(TEvent @event, [CanBeNull] T argument) => _owner.RaiseAsyncInternal(@event, argument, _backupValue);
      }
    }
  }
}