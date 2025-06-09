using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BeatyBit.Binstate;

internal static partial class Argument
{
  public class Resolver
  {
    private readonly Dictionary<Type, ITuple<IArgumentProvider, IArgumentProvider?>> _argumentSourcesCache  = new();
    private readonly Dictionary<Type, IArgumentProvider>                             _argumentProviderCache = new();

    public readonly Bag ArgumentsBag = new Bag();

    public void PrepareArgumentForState<TArgument>(
      IState    targetState,
      TArgument argument,
      bool      argumentIsFallback,
      IState    sourceState)
    {
      var targetArgumentType = targetState.GetArgumentTypeSafe();
      if(targetArgumentType is not null)
      {
        if(! GetArgumentProviders(targetArgumentType, argument, argumentIsFallback, sourceState, out var argumentProviders))
          throw TransitionException.NoArgumentException(targetState);

        ArgumentsBag.Add(targetState, state => SetArgumentByReflection(state, argumentProviders));
      }
    }

    /// <summary>
    /// Searches for an argument provider in the source state and its parent states that can provide an argument of the specified target type.
    /// </summary>
    /// <param name="sourceState">The source state to start searching from</param>
    /// <param name="targetArgumentType">The type of argument needed by the target state</param>
    /// <returns>
    /// An IArgumentProvider if found that can provide the target type, either directly or from a tuple component.
    /// Returns null if no suitable provider is found.
    /// </returns>
    /// <remarks>
    /// The search is performed in the following order:
    /// 1. Checks if the source state's argument type matches the target type directly
    /// 2. If a source state has a tuple argument, checks if either component matches target type
    /// 3. Recursively checks parent states in the same manner
    /// </remarks>
    public static IArgumentProvider? GetArgumentProvider(IState sourceState, Type targetArgumentType)
    {
      var state = sourceState;
      while(state != null)
      {
        var stateArgumentType = state.GetArgumentTypeSafe();

        if(stateArgumentType is not null)
        {
          if(targetArgumentType.IsAssignableFrom(stateArgumentType))
            return state;

          // if one of the Tuple items of the source state is suitable for the target, use it
          if(stateArgumentType.IsTuple(out var typeX, out var typeY))
          {
            if(targetArgumentType.IsAssignableFrom(typeX))
              return ArgumentProviderFromTupleXFactory.CreateArgumentProvider(targetArgumentType, typeX, typeY, state); // magic is here

            if(targetArgumentType.IsAssignableFrom(typeY))
              return ArgumentProviderFromTupleYFactory.CreateArgumentProvider(targetArgumentType, typeX, typeY, state); // magic is here
          }
        }

        state = state.ParentState;
      }

      return null;
    }

    public bool GetArgumentProviders<TArgument>(
      Type                                                                   targetArgumentType,
      TArgument                                                              argument,
      bool                                                                   argumentIsFallback,
      IState                                                                 sourceState,
      [NotNullWhen(true)] out ITuple<IArgumentProvider, IArgumentProvider?>? providers)
    {
      if(_argumentSourcesCache.TryGetValue(targetArgumentType, out providers))
        return true;

      // if a not fallback argument is passed to Raise method and suitable, set it, don't search active states
      if(! argumentIsFallback)
        if(targetArgumentType.IsAssignableFrom(typeof(TArgument)))
        {
          providers = new Tuple<IArgumentProvider, IArgumentProvider?>(new ArgumentProviderByValue<TArgument>(argument), null);
          _argumentSourcesCache.Add(targetArgumentType, providers);
          return true;
        }

      // search for a state provides an argument of an assignable type if the target requires a Tuple and there is a source
      // with a suitable Tuple, it will be found
      if(GetArgumentProviderForSingleArgument(sourceState, targetArgumentType, out var provider))
      {
        providers = new Tuple<IArgumentProvider, IArgumentProvider?>(provider, null);
        _argumentSourcesCache.Add(targetArgumentType, providers);
        return true;
      }

      var passedArgumentType = typeof(TArgument);

      // if still not found, and the target argument type is a Tuple, try to compose it from different source states
      if(targetArgumentType.IsTuple(out var typeX, out var typeY))
      {
        GetArgumentProviderForSingleArgument(sourceState, typeX, out var providerX);
        GetArgumentProviderForSingleArgument(sourceState, typeY, out var providerY);

        if(providerX is null)
          if(typeX.IsAssignableFrom(passedArgumentType))
            providerX = new ArgumentProviderByValue<TArgument>(argument); // replace it with a fallback value if provided

        if(providerY is null)
          if(typeY.IsAssignableFrom(passedArgumentType))
            providerY = new ArgumentProviderByValue<TArgument>(argument); // replace it with a fallback value if provided

        if(providerX is null || providerY is null)
          return false;

        providers = new Tuple<IArgumentProvider, IArgumentProvider?>(providerX, providerY);
        _argumentSourcesCache.Add(targetArgumentType, providers);
        return true;
      }

      // if still no result and there is a fallback value is provided, use it
      if(targetArgumentType.IsAssignableFrom(passedArgumentType))
      {
        providers = new Tuple<IArgumentProvider, IArgumentProvider?>(new ArgumentProviderByValue<TArgument>(argument), null);
        return true;
      }

      return false;
    }

    /// <summary>
    /// Finds an argument provider for a state with a single argument type from the source state hierarchy.
    /// </summary>
    /// <param name="sourceState">The starting state to search from in the state hierarchy</param>
    /// <param name="targetArgumentType">The required argument type to find a provider for</param>
    /// <param name="provider">The argument provider found, if any</param>
    /// <returns>True if a suitable provider was found, false otherwise</returns>
    /// <remarks>
    /// This method searches in the following order:
    /// 1. Checks if the source state's argument type is assignable to target type
    /// 2. If source state's argument is a Tuple, checks if either component is assignable to target
    /// 3. Recursively searches parent states in hierarchy
    /// Caches found providers for future lookups.
    /// </remarks>
    public bool GetArgumentProviderForSingleArgument(
      IState                                     sourceState,
      Type                                       targetArgumentType,
      [NotNullWhen(true)] out IArgumentProvider? provider)
    {
      // separate cache for single providers, provider still can provide Tuple
      if(_argumentProviderCache.TryGetValue(targetArgumentType, out provider))
        return true;

      provider = null;

      var state = sourceState;
      while(state is not null)
      {
        var stateArgumentType = state.GetArgumentTypeSafe();

        if(stateArgumentType is not null)
        {
          if(targetArgumentType.IsAssignableFrom(stateArgumentType))
          {
            _argumentProviderCache.Add(targetArgumentType, state);
            provider = state;
            return true;
          }

          // if one of the Tuple items of the source state is suitable for the target, use it
          if(stateArgumentType.IsTuple(out var typeX, out var typeY)
          && (
               targetArgumentType.IsAssignableFrom(typeX)
            || targetArgumentType.IsAssignableFrom(typeY)
             ))
          {
            provider = ArgumentProviderFromTupleDynamicFactory.CreateArgumentProvider(targetArgumentType, typeX, typeY, state); // magic is here
            _argumentProviderCache.Add(targetArgumentType, provider);
            return true;
          }
        }

        state = state.ParentState;
      }

      return false;
    }
  }

  public class Bag : Dictionary<IState, Action<IState>>;
}