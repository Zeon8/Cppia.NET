namespace Cppia.Runtime;

public class InterpretedClass : IClass
{
    public string Name => _class.Name;

    public IClass? BaseClass { get; private set; }
    public InterpretedMethod? Constructor { get; }

    private readonly Dictionary<string, InterpretedVarible> _varibles;
    private readonly Dictionary<string, InterpretedMethod> _methods;
    private readonly CppiaClass _class;
    private readonly CppiaRuntime _runtime;

    public InterpretedClass(CppiaClass type, CppiaRuntime runtime)
    {
        _class = type;
        _runtime = runtime;

        _varibles = _class.GetVaribles()
            .Select(v => new InterpretedVarible(v, runtime, this))
            .ToDictionary(v => v.Name);
        _methods = _class.GetMethods()
            .Select(m => new InterpretedMethod(m, runtime))
            .ToDictionary(m => m.Name);
        
        if(_methods.ContainsKey("new"))
        {
            Constructor = _methods["new"];
            _methods.Remove("new");
        }
    }

    internal void Initialize()
    {
        foreach (var item in _varibles)
            if(item.Value.IsStatic)
                item.Value.Initialize();
        
        if(_class.Super is not null)
            BaseClass = _runtime.GetClass(_class.Super);
    }

    public bool IsOfType(IClass @class)
    {
        if(BaseClass is null)
            return false;
        if(BaseClass == @class)
            return true;
        return BaseClass.IsOfType(@class);
    }

    public object Construct(params object?[] parameters)
    {
        if(Constructor is null)
            throw new NullReferenceException("Cannot construct class that doesn't have constructor");
        var obj = new CppiaInstance(this);
        Constructor.Invoke(obj, parameters);
        return obj;
    }

    public IVarible? GetVarible(string name)
    {
        if(_varibles.ContainsKey(name))
            return _varibles[name];
        if(BaseClass is not null)
            return BaseClass.GetVarible(name);
        return null;
    }

    public IMethod? GetMethod(string name)
    {
        if(_methods.ContainsKey(name))
            return _methods[name];
        if(BaseClass is not null)
            return BaseClass.GetMethod(name);
        return null;
    }
}