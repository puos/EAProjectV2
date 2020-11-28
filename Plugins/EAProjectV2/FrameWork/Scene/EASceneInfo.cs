using System;


[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class EASceneInfoAttribute : Attribute
{
    public Type _classType { get; private set; }
    public string _className { get; private set; }

    public EASceneInfoAttribute(Type classtype)
    {
        _classType = classtype;
        _className = classtype.Name;
    }
}
