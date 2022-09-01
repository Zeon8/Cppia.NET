

namespace Cppia.Instructions;

public class AssignSubInstruction : BaseAssignInstruction
{
    public AssignSubInstruction(CppiaFile file, CppiaReader reader) 
        : base(file, reader) {}

    protected override double Assign(double currentValue, double value)
    {
        return currentValue - value;
    }
}