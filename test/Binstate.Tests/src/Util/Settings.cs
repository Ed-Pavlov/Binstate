using System;
using BeatyBit.Binstate;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Binstate.Tests.Util;

public class Settings
{
  private static readonly Lazy<ILoggerFactory> LoggerFactory = new Lazy<ILoggerFactory>(CreateLogger);

  public static Builder.Settings WithLogger() => new Builder.Settings { LoggerFactory = LoggerFactory.Value };

  private static ILoggerFactory CreateLogger()
  {
    Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

    return Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddSerilog(dispose: true));
  }
}