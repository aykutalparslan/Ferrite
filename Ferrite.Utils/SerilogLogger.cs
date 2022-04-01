/*
 *   Project Ferrite is an Implementation Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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
        Console.WriteLine(message);
        log.Debug(message);
    }

    public void Debug(Exception exception, string message)
    {
        Console.WriteLine(message);
        log.Debug(exception, message);
    }

    public void Error(string message)
    {
        Console.WriteLine(message);
        log.Error(message);
    }

    public void Error(Exception exception, string message)
    {
        Console.WriteLine(message);
        log.Error(exception, message);
    }

    public void Fatal(string message)
    {
        Console.WriteLine(message);
        log.Fatal(message);
    }

    public void Fatal(Exception exception, string message)
    {
        Console.WriteLine(message);
        log.Fatal(exception, message);
    }

    public void Information(string message)
    {
        Console.WriteLine(message);
        log.Information(message);
    }

    public void Information(Exception exception, string message)
    {
        Console.WriteLine(message);
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
        Console.WriteLine(message);
        log.Warning(message);
    }

    public void Warning(Exception exception, string message)
    {
        Console.WriteLine(message);
        log.Warning(exception, message);
    }
}


