using System.Collections.Generic;

/// <summary>
/// シナリオデータをハードコードで提供するクラス
/// </summary>
public static class PrologueProvider
{
    /// <summary>
    /// プロローグシナリオを取得
    /// </summary>
    public static List<DialogData> GetPrologueScenario()
    {
        return new List<DialogData>
        {
            // 演出：暗転
            new DialogData("", "（エレベーターが停止する音）"),
            new DialogData("", "（扉が開く音）"),
            
            // 主人公の最初の独白
            new DialogData("---", "………。ん……。"),
            new DialogData("---", "あれ……ここは……？"),
            new DialogData("---", "………………。"),
            new DialogData("---", "なにも……思い出せない……。"),
            
            // 足音の後
            new DialogData("", "（足音が近づく）"),
            new DialogData("---", "……何の音……？"),
            
            // アルヴ登場
            new DialogData("???", "おかえりなさいませ、---様。", "Alv/1-1"),
            new DialogData("---", "……？", "Alv/1-1"),
            new DialogData("---", "誰……？", "Alv/1-1"),
            new DialogData("???", "おや。", "Alv/2-3"),
            new DialogData("???", "---様、ずいぶん驚かれているようですね。", "Alv/1-1"),
            new DialogData("---", "あなたは……誰？", "Alv/1-1"),
            new DialogData("---", "ここは……どこなの？", "Alv/1-1"),
            
            // アルヴの自己紹介
            new DialogData("???", "これはこれは。", "Alv/2-1"),
            new DialogData("???", "申し遅れました。", "Alv/1-1"),
            new DialogData("アルヴ", "この場所の支配人を務めている、アルヴと申します。", "Alv/1-2"),
            new DialogData("アルヴ", "以後、お見知り置きを。", "Alv/1-4"),

            new DialogData("---", "アルヴ……？", "Alv/1-4"),
            new DialogData("---", "ここって……何なの？", "Alv/1-4"),
            new DialogData("---", "どうして私、ここに……", "Alv/1-4"),
            
            // VOID REDの説明
            new DialogData("アルヴ", "本当に、何も覚えていらっしゃらないのですね。", "Alv/2-5"),
            new DialogData("アルヴ", "では、ボクがご説明いたしましょう。", "Alv/1-1"),
            new DialogData("アルヴ", "ここはVOID RED。", "Alv/2-3"),
            new DialogData("アルヴ", "あなたが自ら望んで来られた、オークション会場です。", "Alv/2-2"),

            new DialogData("---", "……オークション？", "Alv/2-2"),
            new DialogData("---", "私が……望んで？", "Alv/2-2"),
            
            // 精神オークションの説明
            new DialogData("アルヴ", "ええ。あなたにはとても愉快なオークションにご参加いただきます。", "Alv/1-2"),
            new DialogData("アルヴ", "その名も——「精神オークション」。", "Alv/1-4"),

            new DialogData("---", "精神……オークション？", "Alv/1-4"),
            
            // クイズ
            new DialogData("アルヴ", "この会場には、様々な品物が出品されています。", "Alv/2-1"),
            new DialogData("アルヴ", "さて、ここでクイズです。", "Alv/1-1"),
            new DialogData("アルヴ", "何が並んでいると思いますか？", "Alv/1-2"),

            new DialogData("---", "……壺？ 絵画？ 高そうなものとか……？", "Alv/1-2"),
            
            // 記憶の説明
            new DialogData("アルヴ", "面白いですね。", "Alv/2-4"),
            new DialogData("アルヴ", "正解は——「記憶」。", "Alv/1-2"),

            new DialogData("---", "……記憶！？", "Alv/1-2"),
            new DialogData("---", "一体どういうこと…？", "Alv/1-2"),

            new DialogData("アルヴ", "驚かれるのも当然です。", "Alv/2-1"),
            new DialogData("アルヴ", "さらに申し上げますと——", "Alv/2-1"),
            new DialogData("アルヴ", "ここには、あなたの記憶も出品されています。", "Alv/2-4"),
            
            // 主人公の反発
            new DialogData("---", "……っ！？", "Alv/2-4"),
            new DialogData("---", "じゃあ……私が何も思い出せないのって……", "Alv/2-4"),
            new DialogData("---", "あなたのせい……？", "Alv/2-4"),
            new DialogData("---", "返してよ……私の記憶！", "Alv/2-4"),
            
            // ルールの説明
            new DialogData("アルヴ", "それはできません。", "Alv/1-5"),
            new DialogData("アルヴ", "ここでは、自分の力で記憶を取り戻していただきます。", "Alv/1-3"),
            new DialogData("アルヴ", "それが、この会場のルールです。", "Alv/2-1"),

            new DialogData("---", "勝手に奪っておいて、ルール？", "Alv/2-1"),
            new DialogData("---", "ふざけないで。そんなの、従うわけ——", "Alv/2-1"),

            new DialogData("アルヴ", "あなたには、ボクの話した通り——", "Alv/1-4"),
            new DialogData("アルヴ", "このオークションに参加する以外、記憶を取り戻す術はありません。", "Alv/1-4"),
            new DialogData("アルヴ", "……理解が遅いですね。", "Alv/2-3"),
            new DialogData("アルヴ", "少々、くどい。", "Alv/2-1"),
            
            // 主人公の諦め
            new DialogData("---", "……もう……訳が分からない……。", "Alv/2-1"),
            new DialogData("---", "言い返す気力もなくなってきた……", "Alv/2-1"),

            new DialogData("アルヴ", "では、大人しくボクの案内を聞いていただけると嬉しいです。", "Alv/1-4"),
            
            // 精神札の説明
            new DialogData("アルヴ", "さて、あなたには記憶を競り落とすために、他のお客様と戦っていただきます。", "Alv/1-1"),
            new DialogData("アルヴ", "その際に使用するのが——精神札。", "Alv/1-2"),

            new DialogData("アルヴ", "こちらが精神札。", "Alv/2-1"),
            new DialogData("アルヴ", "あなたの\"価値\"を数値化したものです。", "Alv/2-2"),
            new DialogData("アルヴ", "これがなければ、オークションには参加できません。", "Alv/2-5"),
            new DialogData("アルヴ", "……くれぐれも、破損などなさらぬよう。", "Alv/2-3"),

            new DialogData("---", "破損って……そんなガサツに見えてる？", "Alv/2-3"),
            
            // 模擬オークションへ
            new DialogData("アルヴ", "とんでもない。", "Alv/1-4"),
            new DialogData("アルヴ", "ただ、無理はなさらぬように。", "Alv/1-1"),
            new DialogData("アルヴ", "……まあ、今はまだ意味が分からないでしょうから——", "Alv/2-1"),
            new DialogData("アルヴ", "模擬オークションで、慣れていただきましょう。", "Alv/2-2"),
            new DialogData("", "（足音が遠ざかる）"),

        };
    }

    /// <summary>
    /// プロローグ2シナリオを取得
    /// </summary>
    public static List<DialogData> GetPrologue2Scenario()
    {
        return new List<DialogData>
        {
            new DialogData("システム", "(プロローグ後半)")
        };
    }

    public static List<DialogData> GetEndingScenario()
    {
        return new List<DialogData>
        {
            new DialogData("システム", "アルファ版はここまでです。"),
            new DialogData("システム", "プレイしていただきありがとうございます。"),
            new DialogData("システム", "製品版リリースをお待ちください。")
        };
    }
}
