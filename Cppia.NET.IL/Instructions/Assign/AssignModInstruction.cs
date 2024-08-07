

namespace Cppia.NET.Instructions;

public class AssignModInstruction : BaseAssignInstruction
{
    public AssignModInstruction(CppiaFile file, CppiaReader reader) 
        : base(file, reader) {}

    protected override double Assign(double currentValue, double value)
        => currentValue % value;
}