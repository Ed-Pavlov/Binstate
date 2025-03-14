using System;
using System.Diagnostics.CodeAnalysis;

namespace Binstate;

/// <summary>
/// A delegate to be used with <see cref="Builder{TState,TEvent}.ConfiguratorOf.ITransitions.AddTransition(TEvent,GetState{TState})" />.
/// This delegate allows using the 'default' value of <see cref="ValueType "/> in case it's used as <typeparamref name="TState"/> as a valid state.
/// </summary>
/// <param name="state"> The state to which transition should be performed. </param>
/// <returns> Returns false if no transition should be performed. </returns>
public delegate bool GetState<TState>([NotNullWhen(true)] out TState? state);
