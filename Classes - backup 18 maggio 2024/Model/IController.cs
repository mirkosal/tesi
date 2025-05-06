using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IController<T>
{
    // Metodo ExecuteTask con parametri generici
    T ExecuteTask(params object[] parameters);
}
