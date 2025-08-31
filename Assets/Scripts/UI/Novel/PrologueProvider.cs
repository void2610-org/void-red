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
            new DialogData("???", "おかえりなさいませ、---様。", "1-1"),
            new DialogData("---", "……？", "1-1"),
            new DialogData("---", "誰……？", "1-1"),
            new DialogData("???", "おや。", "2-3"),
            new DialogData("???", "---様、ずいぶん驚かれているようですね。", "1-1"),
            new DialogData("---", "あなたは……誰？", "1-1"),
            new DialogData("---", "ここは……どこなの？", "1-1"),
            
            // アルヴの自己紹介
            new DialogData("???", "これはこれは。", "2-1"),
            new DialogData("???", "申し遅れました。", "1-1"),
            new DialogData("アルヴ", "この場所の支配人を務めている、アルヴと申します。", "1-2"),
            new DialogData("アルヴ", "以後、お見知り置きを。", "1-4"),
            
            new DialogData("---", "アルヴ……？", "1-4"),
            new DialogData("---", "ここって……何なの？", "1-4"),
            new DialogData("---", "どうして私、ここに……", "1-4"),
            
            // VOID REDの説明
            new DialogData("アルヴ", "本当に、何も覚えていらっしゃらないのですね。", "2-5"),
            new DialogData("アルヴ", "では、ボクがご説明いたしましょう。", "1-1"),
            new DialogData("アルヴ", "ここはVOID RED。", "2-3"),
            new DialogData("アルヴ", "あなたが自ら望んで来られた、オークション会場です。", "2-2"),
            
            new DialogData("---", "……オークション？", "2-2"),
            new DialogData("---", "私が……望んで？", "2-2"),
            
            // 精神オークションの説明
            new DialogData("アルヴ", "ええ。あなたにはとても愉快なオークションにご参加いただきます。", "1-2"),
            new DialogData("アルヴ", "その名も——「精神オークション」。", "1-4"),
            
            new DialogData("---", "精神……オークション？", "1-4"),
            
            // クイズ
            new DialogData("アルヴ", "この会場には、様々な品物が出品されています。", "2-1"),
            new DialogData("アルヴ", "さて、ここでクイズです。", "1-1"),
            new DialogData("アルヴ", "何が並んでいると思いますか？", "1-2"),
            
            new DialogData("---", "……壺？ 絵画？ 高そうなものとか……？", "1-2"),
            
            // 記憶の説明
            new DialogData("アルヴ", "面白いですね。", "2-4"),
            new DialogData("アルヴ", "正解は——「記憶」。", "1-2"),
            
            new DialogData("---", "……記憶！？", "1-2"),
            new DialogData("---", "一体どういうこと…？", "1-2"),
            
            new DialogData("アルヴ", "驚かれるのも当然です。", "2-1"),
            new DialogData("アルヴ", "さらに申し上げますと——", "2-1"),
            new DialogData("アルヴ", "ここには、あなたの記憶も出品されています。", "2-4"),
            
            // 主人公の反発
            new DialogData("---", "……っ！？", "2-4"),
            new DialogData("---", "じゃあ……私が何も思い出せないのって……", "2-4"),
            new DialogData("---", "あなたのせい……？", "2-4"),
            new DialogData("---", "返してよ……私の記憶！", "2-4"),
            
            // ルールの説明
            new DialogData("アルヴ", "それはできません。", "1-5"),
            new DialogData("アルヴ", "ここでは、自分の力で記憶を取り戻していただきます。", "1-3"),
            new DialogData("アルヴ", "それが、この会場のルールです。", "2-1"),
            
            new DialogData("---", "勝手に奪っておいて、ルール？", "2-1"),
            new DialogData("---", "ふざけないで。そんなの、従うわけ——", "2-1"),
            
            new DialogData("アルヴ", "あなたには、ボクの話した通り——", "1-4"),
            new DialogData("アルヴ", "このオークションに参加する以外、記憶を取り戻す術はありません。", "1-4"),
            new DialogData("アルヴ", "……理解が遅いですね。", "2-3"),
            new DialogData("アルヴ", "少々、くどい。", "2-1"),
            
            // 主人公の諦め
            new DialogData("---", "……もう……訳が分からない……。", "2-1"),
            new DialogData("---", "言い返す気力もなくなってきた……", "2-1"),
            
            new DialogData("アルヴ", "では、大人しくボクの案内を聞いていただけると嬉しいです。", "1-4"),
            
            // 精神札の説明
            new DialogData("アルヴ", "さて、あなたには記憶を競り落とすために、他のお客様と戦っていただきます。", "1-1"),
            new DialogData("アルヴ", "その際に使用するのが——精神札。", "1-2"),
            
            new DialogData("アルヴ", "こちらが精神札。", "2-1"),
            new DialogData("アルヴ", "あなたの\"価値\"を数値化したものです。", "2-2"),
            new DialogData("アルヴ", "これがなければ、オークションには参加できません。", "2-5"),
            new DialogData("アルヴ", "……くれぐれも、破損などなさらぬよう。", "2-3"),
            
            new DialogData("---", "破損って……そんなガサツに見えてる？", "2-3"),
            
            // 模擬オークションへ
            new DialogData("アルヴ", "とんでもない。", "1-4"),
            new DialogData("アルヴ", "ただ、無理はなさらぬように。", "1-1"),
            new DialogData("アルヴ", "……まあ、今はまだ意味が分からないでしょうから——", "2-1"),
            new DialogData("アルヴ", "模擬オークションで、慣れていただきましょう。", "2-2"),
            new DialogData("", "（足音が遠ざかる）"),
            
        };
    }
}