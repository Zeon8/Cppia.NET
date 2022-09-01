using Cppia.Instructions;
using Cppia.Runtime;

namespace Cppia;

public class CppiaRuntime
{
    public List<CppiaInstruction> MainMethods { get; } = new();
    
    private readonly Dictionary<string, IClass> _classses = new();
    private readonly Dictionary<string, CppiaEnum> _enums = new();
    private readonly Dictionary<string, Delegate> _globals = new();

    public CppiaRuntime()
    {
        RegisterType(typeof(EnumValue), "::hx::EnumBase");
        RegisterType(typeof(CppiaException), "haxe.Exception");
        RegisterType(typeof(ValueException), "haxe.ValueException");
        RegisterType(typeof(Std.CppiaStd), "Std");
        RegisterType(typeof(Std.Date), "Date");
    }

    public IClass? GetClass(string fullName)
    {
        if(_classses.ContainsKey(fullName))
            return _classses[fullName];
        return null;
    }

    public CppiaEnum? GetEnum(string name)
    {
        if(_enums.ContainsKey(name))
            return _enums[name];
        return null;
    }

    /// <returns>Main method instruction</returns>
    public CppiaInstruction? Load(string fileName) => Load(File.OpenRead(fileName));
    
    /// <returns>Main method instruction</returns>
    public CppiaInstruction? Load(byte[] bytes) => Load(new MemoryStream(bytes));
    
    /// <returns>Main method call instruction</returns>
    public CppiaInstruction? Load(Stream stream)
    {
        var file = new CppiaFile(stream);
        var classes = file.CppiaTypes.OfType<CppiaClass>()
            .Where(c => !_classses.ContainsKey(c.Name))
            .Select(c => new InterpretedClass(c, this))
            .ToArray();

        // Add types
        foreach (var @class in classes) _classses.Add(@class.Name, @class);
        foreach (var @enum in file.CppiaTypes.OfType<CppiaEnum>())
            _enums.Add(@enum.Name, @enum);
        
        // Initialize class varibles
        foreach (var @class in classes)
            @class.Initialize();
        return file.Main;
    }

    public void RegisterType(Type type, string? customName = null)
    {
        if(customName is null)
            customName = type.Name;

        _classses.Add(customName, new NativeClass(type, customName));
    }

    public void RegisterGlobal(string name, Delegate @delegate)
    {
        _globals.Add(name, @delegate);
    }

    internal object? Execute(FunctionInstruction function, object? thisObj, object?[] args)
    {
        var context = new Context(this, thisObj as CppiaInstance);
        for (int i = 0; i < function.Arguments.Count; i++)
        {
            var argument = function.Arguments[i];
            context.Varibles.Add(argument.Id, args[i]);
        }
        return function.Invoke(context);
    }

    internal object? InvokeGlobal(string name, params object?[] args)
    {
        if(!_globals.ContainsKey(name))
            throw new Exception("Global not found: " + name);
        return _globals[name].DynamicInvoke(args);
    }
}