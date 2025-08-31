namespace Shared.Core.Protocols;
public static class FighterOperators
{
    // Non energy consume action
    public const string ForwardOn = "forward_on";
    public const string ForwardOff = "forward_off";
    public const string TurnLeftOn = "turn_left_on";
    public const string TurnLeftOff = "turn_left_off";
    public const string TurnRightOn = "turn_right_on";
    public const string TurnRightOff = "turn_right_off";

    // Thrust
    public const string ThrustOn = "thrust_on";
    public const string ThrustOff = "thrust_off";
    public const string ThrustLeftDash = "thrust_left_dash";
    public const string RightThrustDash = "thrust_right_dash";
    public const string ThrustBackwardDash = "thrust_backward_dash";

    // Gear (Weapon)
    public const string MainGearOn = "main_gear_on";
    public const string MainGearOff = "main_gear_off";
    public const string SubGearOn = "sub_gear_on";
    public const string SubGearOff = "sub_gear_off";
}