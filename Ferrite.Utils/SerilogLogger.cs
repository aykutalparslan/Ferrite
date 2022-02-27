using System;
using Serilog;
using Serilog.Core;

namespace Ferrite.Utils;

public class SerilogLogger : ILogger
{
    private Logger log;
    public SerilogLogger()
    {
        log = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("ferrite.log")
            .CreateLogger();
    }

    public void Debug(string message)
    {
        log.Debug(message);
    }

    public void Debug(Exception exception, string message)
    {
        log.Debug(exception, message);
    }

    public void Error(string message)
    {
        log.Error(message);
    }

    public void Error(Exception exception, string message)
    {
        log.Error(exception, message);
    }

    public void Fatal(string message)
    {
        log.Fatal(message);
    }

    public void Fatal(Exception exception, string message)
    {
        log.Fatal(exception, message);
    }

    public void Information(string message)
    {
        log.Information(message);
    }

    public void Information(Exception exception, string message)
    {
        log.Information(exception, message);
    }

    public void Verbose(string message)
    {
        log.Verbose(message);
    }

    public void Verbose(Exception exception, string message)
    {
        log.Verbose(exception, message);
    }

    public void Warning(string message)
    {
        log.Warning(message);
    }

    public void Warning(Exception exception, string message)
    {
        log.Warning(exception, message);
    }
}


