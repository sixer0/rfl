#nullable enable
namespace RFL.Sim
{
    public enum MatchPhase : byte
    {
        Intro = 0, // "ROUND n" then "FIGHT!"; inputs locked
        Fight = 1,
        RoundOver = 2, // KO or TIME, then the round winner; inputs locked
        CornerBreak = 3, // waits for ApplyCornerBreak
        MatchOver = 4,
    }
}
