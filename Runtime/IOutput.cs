using System;

public interface IOutput
{
    object Value { get; }
    Type Type { get; }
}