namespace EasyStock.Web.Models;

public static class MovementTypeExtensions
{
    public static bool IsIncrease(this MovementType type) =>
        type is MovementType.Entry or MovementType.Return or MovementType.AssignmentReturn;

    public static bool IsManual(this MovementType type) =>
        type is MovementType.Entry or MovementType.Exit or MovementType.Return;

    public static string ToDisplayName(this MovementType type) => type switch
    {
        MovementType.Entry => "Entry",
        MovementType.Exit => "Exit",
        MovementType.Return => "Return",
        MovementType.Assignment => "Assignment",
        MovementType.AssignmentReturn => "Assignment return",
        _ => type.ToString(),
    };
}