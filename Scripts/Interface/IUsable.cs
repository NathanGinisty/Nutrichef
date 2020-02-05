using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUsable
{
    bool CanBeUsed(object _useBy);
    bool Use(object _useBy);
    void StopUse();
}
