using System;
using System.Collections.Generic;

public class ControllerFactory
{
    private static ControllerFactory _instance;
    private Dictionary<string, Func<object>> factoryDictionary;

    public static ControllerFactory Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ControllerFactory();
            }
            return _instance;
        }
    }

    private ControllerFactory()
    {
        factoryDictionary = new Dictionary<string, Func<object>>
        {
            { "login", () => new LoginController() },
            { "person", () => new PersonController() },
            { "createTask", () => new CreateTaskController() } // Aggiunta di CreateTaskController
            // Altri controller possono essere aggiunti qui
        };
    }

    public T CreateController<T>(string type) where T : class
    {
        if (factoryDictionary.TryGetValue(type, out Func<object> factory))
        {
            return factory() as T;
        }
        else
        {
            throw new ArgumentException("Controller type not recognized");
        }
    }
}
