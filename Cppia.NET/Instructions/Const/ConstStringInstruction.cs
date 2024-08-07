namespace Cppia.Instructions;

public class ConstStringInstruction : BaseConstInstruction
{
    public ConstStringInstruction(CppiaFile file, CppiaReader reader)
        : base(file, reader) {}

    protected override object? ReadValue(CppiaFile file, CppiaReader reader) 
        => file.Strings[reader.ReadInt()];
}