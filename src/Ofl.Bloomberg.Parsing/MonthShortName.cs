using System.Runtime.CompilerServices;

namespace Ofl.Bloomberg.Parsing;

public static class MonthShortName
{
    #region Helpers

    /*
     * We know that the according to
     * 
     * 
     * 
     * The values of the months when a short name is upper case.
     * 
     * We can use that to take a branching strategy approach, looking at the first letter, second, then third
     * (since there are only three letters) to see which month is being parsed.
     * 
     * That method is implemented 👇 as well as here:
     * 
     * https://stash.elliottmgmt.com/projects/MAR/repos/bloomberg-bulk-processor/browse/Ofl.Bloomberg.BackOffice/src/Ofl.Bloomberg.Parsing/MonthShortName.cs?at=647d8bbba2f20c41efee6f7550dcb4bd1449b3de#59
     * 
     * That was compared to comparing the sequences of all the months:
     * 
     * https://stash.elliottmgmt.com/projects/MAR/repos/bloomberg-bulk-processor/browse/Ofl.Bloomberg.BackOffice/src/Ofl.Bloomberg.Parsing/ByteParser.cs?at=647d8bbba2f20c41efee6f7550dcb4bd1449b3de#439
     * 
     * The results were benchmarked with:
     * 
     * https://stash.elliottmgmt.com/projects/MAR/repos/bloomberg-bulk-processor/browse/Ofl.Bloomberg.BackOffice/benchmarks/Ofl.Bloomberg.Parsing.Benchmarks/ByteParserBenchmarks.cs?at=4d8ae49332b75dd823da8df75676b4282b5301f2#34,37
     * 
     * The results of which are 👇
     * 
|                                          Method | MonthShortName |       Mean |     Error |    StdDev | Allocated |
|------------------------------------------------ |--------------- |-----------:|----------:|----------:|----------:|
|                MonthShortNameParseWithBranching |            JAN |  1.5907 ns | 0.0808 ns | 0.1051 ns |         - |
| MonthShortNameParseWithConsecutiveSequenecEqual |            JAN |  3.7639 ns | 0.0913 ns | 0.0854 ns |         - |
|                MonthShortNameParseWithBranching |            FEB |  2.5782 ns | 0.0991 ns | 0.1217 ns |         - |
| MonthShortNameParseWithConsecutiveSequenecEqual |            FEB | 49.6166 ns | 0.8647 ns | 0.8088 ns |         - |
|                MonthShortNameParseWithBranching |            MAR |  0.8383 ns | 0.0656 ns | 0.0674 ns |         - |
| MonthShortNameParseWithConsecutiveSequenecEqual |            MAR | 16.9292 ns | 0.3535 ns | 0.4956 ns |         - |
|                MonthShortNameParseWithBranching |            APR |  1.9826 ns | 0.0891 ns | 0.1126 ns |         - |
| MonthShortNameParseWithConsecutiveSequenecEqual |            APR | 44.6865 ns | 0.9205 ns | 0.8160 ns |         - |
|                MonthShortNameParseWithBranching |            MAY |  2.2245 ns | 0.0910 ns | 0.1470 ns |         - |
| MonthShortNameParseWithConsecutiveSequenecEqual |            MAY | 53.7836 ns | 0.7667 ns | 0.7172 ns |         - |
|                MonthShortNameParseWithBranching |            JUN |  1.1330 ns | 0.0604 ns | 0.0958 ns |         - |
| MonthShortNameParseWithConsecutiveSequenecEqual |            JUN |  8.0504 ns | 0.2092 ns | 0.2409 ns |         - |
|                MonthShortNameParseWithBranching |            JUL |  0.6814 ns | 0.0623 ns | 0.0552 ns |         - |
| MonthShortNameParseWithConsecutiveSequenecEqual |            JUL | 40.7624 ns | 0.8380 ns | 0.9314 ns |         - |
|                MonthShortNameParseWithBranching |            AUG |  1.7983 ns | 0.0846 ns | 0.0750 ns |         - |
| MonthShortNameParseWithConsecutiveSequenecEqual |            AUG | 21.5050 ns | 0.2975 ns | 0.2782 ns |         - |
|                MonthShortNameParseWithBranching |            SEP |  1.9435 ns | 0.0581 ns | 0.0515 ns |         - |
| MonthShortNameParseWithConsecutiveSequenecEqual |            SEP | 25.7368 ns | 0.3685 ns | 0.3077 ns |         - |
|                MonthShortNameParseWithBranching |            OCT |  1.4037 ns | 0.0746 ns | 0.0662 ns |         - |
| MonthShortNameParseWithConsecutiveSequenecEqual |            OCT | 31.8584 ns | 0.6762 ns | 0.8793 ns |         - |
|                MonthShortNameParseWithBranching |            NOV |  1.6180 ns | 0.0826 ns | 0.1286 ns |         - |
| MonthShortNameParseWithConsecutiveSequenecEqual |            NOV | 36.0698 ns | 0.6251 ns | 0.5847 ns |         - |
|                MonthShortNameParseWithBranching |            DEC |  3.5375 ns | 0.0993 ns | 0.1104 ns |         - |
| MonthShortNameParseWithConsecutiveSequenecEqual |            DEC | 12.5700 ns | 0.2914 ns | 0.3989 ns |         - |
|                MonthShortNameParseWithBranching |        GARBAGE |  0.0000 ns | 0.0000 ns | 0.0000 ns |         - |
| MonthShortNameParseWithConsecutiveSequenecEqual |        GARBAGE |  6.4956 ns | 0.1749 ns | 0.1366 ns |         - |     

     * We can see that the branched version is faster, from 2.5 up to 44 times faster with zero memory allocation.
     * 
     * So we kept the branch code.
     */

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Parse(this in ReadOnlySpan<byte> source)
    {
        // If not three, bail.
        if (source.Length != 3) return 0;

        // Get the first, second, and third character.
        var first = source[0];
        var second = source[1];
        var third = source[2];

        // Switch based on the first character.
        return first switch { 
            UpperCaseAsciiCharacterByte.F => 
                second == UpperCaseAsciiCharacterByte.E && third == UpperCaseAsciiCharacterByte.B ? 2 : 0
            , UpperCaseAsciiCharacterByte.S =>
                second == UpperCaseAsciiCharacterByte.E && third == UpperCaseAsciiCharacterByte.P ? 9 : 0
            , UpperCaseAsciiCharacterByte.O =>
                second == UpperCaseAsciiCharacterByte.C && third == UpperCaseAsciiCharacterByte.T ? 10 : 0
            , UpperCaseAsciiCharacterByte.N =>
                second == UpperCaseAsciiCharacterByte.O && third == UpperCaseAsciiCharacterByte.V ? 11 : 0
            , UpperCaseAsciiCharacterByte.D =>
                second == UpperCaseAsciiCharacterByte.E && third == UpperCaseAsciiCharacterByte.C ? 12 : 0
            , UpperCaseAsciiCharacterByte.A =>
                second == UpperCaseAsciiCharacterByte.P && third == UpperCaseAsciiCharacterByte.R
                    ? 4
                    : (
                        second == UpperCaseAsciiCharacterByte.U && third == UpperCaseAsciiCharacterByte.G 
                        ? 8 
                        : 0
                    )
            , UpperCaseAsciiCharacterByte.M =>
                second == UpperCaseAsciiCharacterByte.A 
                    ? third switch {
                        UpperCaseAsciiCharacterByte.R => 3
                        , UpperCaseAsciiCharacterByte.Y => 5
                        , _ => 0
                    }
                    : 0
            , UpperCaseAsciiCharacterByte.J =>
                second == UpperCaseAsciiCharacterByte.A && third == UpperCaseAsciiCharacterByte.N
                    ? 1 
                    : (
                        second == UpperCaseAsciiCharacterByte.U
                        ? third switch {
                            UpperCaseAsciiCharacterByte.N => 6
                            , UpperCaseAsciiCharacterByte.L => 7
                            , _ => 0
                        }
                        : 0
                    )
            , _ => 0
        };
    }

    #endregion
}
