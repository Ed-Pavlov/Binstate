namespace Binstate;

//TODO: use [NotNullWhen(returnValue: true)]  when upgrading to .netstandard 2.1 and update usages
/// <summary>
///   A delegate to be used with <see cref="Config{TState,TEvent}.Transitions.AddTransition(TEvent,GetState{TState})" />.
/// </summary>
/// <param name="state"> The state to which transition should be performed. </param>
/// <returns> Returns false if no transition should be performed. </returns>
public delegate bool GetState<TState>(out TState? state);