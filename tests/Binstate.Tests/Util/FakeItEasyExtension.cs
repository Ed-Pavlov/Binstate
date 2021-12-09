using FakeItEasy;
using FakeItEasy.Configuration;

namespace Binstate.Tests.Util;

public static class FakeItEasyExtension
{
  public static UnorderedCallAssertion MustHaveHappenedOnceAndOnly(this IVoidArgumentValidationConfiguration configuration)
  {
    // once exactly call with specified and with any arguments means only call with specified arguments was performed
    configuration.MustHaveHappenedOnceExactly();
    return configuration.WithAnyArguments().MustHaveHappenedOnceExactly();
  }

  public static UnorderedCallAssertion MustHaveHappenedOnceAndOnly<TInterface>(this IReturnValueArgumentValidationConfiguration<TInterface> configuration)
  {
    // once exactly call with specified and with any arguments means only call with specified arguments was performed
    configuration.MustHaveHappenedOnceExactly();
    return configuration.WithAnyArguments().MustHaveHappenedOnceExactly();
  }
}