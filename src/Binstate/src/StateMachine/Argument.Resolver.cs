using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BeatyBit.Bits;

namespace BeatyBit.Binstate;

internal static partial class Argument
{
  public static IArgumentBag CreateBag() => new Bag();

  public class Resolver
  {
    private readonly Dictionary<Type, ITuple<IArgumentProvider, IArgumentProvider?>> _argumentSourcesCache  = new();
    private readonly Dictionary<Type, IArgumentProvider>                             _argumentProviderCache = new();

    public readonly IArgumentBag ArgumentsBag = new Bag();

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
          Throw.NoArgument(targetState);

        ArgumentsBag.Add(targetState, state => SetArgumentByReflection(state, argumentProviders));
      }
    }

    private bool GetArgumentProviders<TArgument>(
      Type                                                                   targetArgumentType,
      TArgument                                                              argument,
      bool                                                                   argumentIsFallback,
      IState                                                                 rootState,
      [NotNullWhen(true)] out ITuple<IArgumentProvider, IArgumentProvider?>? providers)
    {
      if(_argumentSourcesCache.TryGetValue(targetArgumentType, out providers))
        return true;

      // if a not fallback argument is passed to Raise method and suitable, set it, don't search active states
      if(! argumentIsFallback)
        if(targetArgumentType.IsAssignableFrom(typeof(TArgument)))
        {
          providers = new Tuple<IArgumentProvider, IArgumentProvider?>(new ArgumentProvider<TArgument>(argument), null);
          _argumentSourcesCache.Add(targetArgumentType, providers);
          return true;
        }

      // search for a state provides argument of an assignable type if target requires a Tuple and there is a source with suitable Tuple, it will be found
      if(GetArgumentProviderForSingleArgument(rootState, targetArgumentType, out var provider))
      {
        providers = new Tuple<IArgumentProvider, IArgumentProvider?>(provider, null);
        _argumentSourcesCache.Add(targetArgumentType, providers);
        return true;
      }

      var passedArgumentType = typeof(TArgument);

      // if still not found, and the target argument type is a Tuple, try to compose it from different source states
      if(targetArgumentType.IsTuple(out var typeX, out var typeY))
      {
        GetArgumentProviderForSingleArgument(rootState, typeX, out var providerX);
        GetArgumentProviderForSingleArgument(rootState, typeY, out var providerY);

        if(providerX is null)
          if(typeX.IsAssignableFrom(passedArgumentType))
            providerX = new ArgumentProvider<TArgument>(argument); // replace it with fallback value if provided

        if(providerY is null)
          if(typeY.IsAssignableFrom(passedArgumentType))
            providerY = new ArgumentProvider<TArgument>(argument); // replace it with fallback value if provided

        if(providerX is null || providerY is null)
          return false;

        providers = new Tuple<IArgumentProvider, IArgumentProvider?>(providerX, providerY);
        _argumentSourcesCache.Add(targetArgumentType, providers);
        return true;
      }

      // if still no result and there is a fallback value is provided, use it
      if(targetArgumentType.IsAssignableFrom(passedArgumentType))
      {
        providers = new Tuple<IArgumentProvider, IArgumentProvider?>(new ArgumentProvider<TArgument>(argument), null);
        return true;
      }

      return false;
    }

//    public static void SetArgumentByReflection(IState target, ITuple<IArgumentProvider, IArgumentProvider?> tuple)

    private bool GetArgumentProviderForSingleArgument(
      IState                                     sourceRoot,
      Type                                       targetArgumentType,
      [NotNullWhen(true)] out IArgumentProvider? provider)
    {
      // separate cache for single providers, provider still can provide Tuple
      if(_argumentProviderCache.TryGetValue(targetArgumentType, out provider))
        return true;

      provider = null;

      var state = sourceRoot;
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

          // if one of the Tuple items of the source state is suitable for target, use it
          if(stateArgumentType.IsTuple(out var typeX, out var typeY)
          && (
               targetArgumentType.IsAssignableFrom(typeX)
            || targetArgumentType.IsAssignableFrom(typeY)
             ))
          {
            provider = StateTupleArgumentProvider.Create(targetArgumentType, typeX, typeY, state); // magic is here
            _argumentProviderCache.Add(targetArgumentType, provider);
            return true;
          }
        }

        state = state.ParentState;
      }

      return false;
    }
  }

  private class Bag : IArgumentBag
  {
    private readonly Dictionary<IState, Action<IState>> _dictionary = new Dictionary<IState, Action<IState>>();

    public void Add(IState state, Action<IState> setArgument) => _dictionary.Add(state, setArgument);

    public Action<IState>? GetValueSafe(IState state) => _dictionary.GetValueSafe(state);
  }
}