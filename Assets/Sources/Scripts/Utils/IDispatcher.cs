using System;
using System.Collections.Generic;

public interface IDispatcher
{
    void Invoke(Action fn);
}

