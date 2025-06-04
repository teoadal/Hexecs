namespace Hexecs.Loggers;

public readonly struct ContextLogger(LogService logService, string context)
{
    #region Debug

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Debug(string template)
    {
        logService.Write(LogLevel.Debug, context, template);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Debug<T1>(string template, T1 arg1)
    {
        logService.Write(LogLevel.Debug, context, template, arg1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Debug<T1, T2>(string template, T1 arg1, T2 arg2)
    {
        logService.Write(LogLevel.Debug, context, template, arg1, arg2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Debug<T1, T2, T3>(string template, T1 arg1, T2 arg2, T3 arg3)
    {
        logService.Write(LogLevel.Debug, context, template, arg1, arg2, arg3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Debug<T1, T2, T3, T4>(string template, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        logService.Write(LogLevel.Debug, context, template, arg1, arg2, arg3, arg4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Debug<T1, T2, T3, T4, T5>(string template, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        logService.Write(LogLevel.Debug, context, template, arg1, arg2, arg3, arg4, arg5);
    }

    #endregion

    #region Info

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Info(string template)
    {
        logService.Write(LogLevel.Info, context, template);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Info<T1>(string template, T1 arg1)
    {
        logService.Write(LogLevel.Info, context, template, arg1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Info<T1, T2>(string template, T1 arg1, T2 arg2)
    {
        logService.Write(LogLevel.Info, context, template, arg1, arg2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Info<T1, T2, T3>(string template, T1 arg1, T2 arg2, T3 arg3)
    {
        logService.Write(LogLevel.Info, context, template, arg1, arg2, arg3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Info<T1, T2, T3, T4>(string template, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        logService.Write(LogLevel.Info, context, template, arg1, arg2, arg3, arg4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Info<T1, T2, T3, T4, T5>(string template, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        logService.Write(LogLevel.Info, context, template, arg1, arg2, arg3, arg4, arg5);
    }

    #endregion

    #region Write

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(LogLevel level, string template)
    {
        logService.Write(level, context, template);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write<T1>(LogLevel level, string template, T1 arg1)
    {
        logService.Write(level, context, template, arg1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write<T1, T2>(LogLevel level, string template, T1 arg1, T2 arg2)
    {
        logService.Write(level, context, template, arg1, arg2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write<T1, T2, T3>(LogLevel level, string template, T1 arg1, T2 arg2, T3 arg3)
    {
        logService.Write(level, context, template, arg1, arg2, arg3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write<T1, T2, T3, T4>(
        LogLevel level,
        string template,
        T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        logService.Write(level, context, template, arg1, arg2, arg3, arg4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write<T1, T2, T3, T4, T5>(
        LogLevel level,
        string template,
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
    {
        logService.Write(level, context, template, arg1, arg2, arg3, arg4, arg5);
    }

    #endregion
}