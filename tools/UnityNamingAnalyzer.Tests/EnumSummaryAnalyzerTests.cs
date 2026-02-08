using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    UnityNamingAnalyzer.EnumSummaryAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace UnityNamingAnalyzer.Tests
{
    public class EnumSummaryAnalyzerTests
    {
        [Fact]
        public async Task TopLevelEnumMemberWithoutSummary_UNA0006()
        {
            // summaryなし → 検出
            var test = @"
public enum GameState
{
    {|#0:ThemeAnnouncement|},
    {|#1:CardDistribution|}
}";
            var expected = new[]
            {
                Verify.Diagnostic("UNA0006")
                    .WithLocation(0)
                    .WithArguments("ThemeAnnouncement"),
                Verify.Diagnostic("UNA0006")
                    .WithLocation(1)
                    .WithArguments("CardDistribution"),
            };
            await Verify.VerifyAnalyzerAsync(test, expected);
        }

        [Fact]
        public async Task TopLevelEnumMemberWithSummary_NoDiagnostic()
        {
            // summaryあり → 検出なし
            var test = @"
public enum EmotionType
{
    /// <summary> 喜び </summary>
    Joy,
    /// <summary> 信頼 </summary>
    Trust
}";
            await Verify.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task TopLevelEnumMemberWithLineCommentOnly_UNA0006()
        {
            // //コメントのみ → 検出（///じゃないため）
            var test = @"
public enum GameState
{
    // テーマ公開
    {|#0:ThemeAnnouncement|},
    // カード配布
    {|#1:CardDistribution|}
}";
            var expected = new[]
            {
                Verify.Diagnostic("UNA0006")
                    .WithLocation(0)
                    .WithArguments("ThemeAnnouncement"),
                Verify.Diagnostic("UNA0006")
                    .WithLocation(1)
                    .WithArguments("CardDistribution"),
            };
            await Verify.VerifyAnalyzerAsync(test, expected);
        }

        [Fact]
        public async Task NestedEnumMemberWithoutSummary_NoDiagnostic()
        {
            // ネストされたenum → 検出なし（対象外）
            var test = @"
public class OuterClass
{
    public enum NestedEnum
    {
        Value1,
        Value2
    }
}";
            await Verify.VerifyAnalyzerAsync(test);
        }
    }
}
