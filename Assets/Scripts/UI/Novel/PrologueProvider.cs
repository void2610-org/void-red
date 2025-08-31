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
            new DialogData("???", "おかえりなさいませ、---様。"),
            new DialogData("---", "……？"),
            new DialogData("---", "誰……？"),
            new DialogData("???", "おや。"),
            new DialogData("???", "---様、ずいぶん驚かれているようですね。"),
            new DialogData("---", "あなたは……誰？"),
            new DialogData("---", "ここは……どこなの？"),
            
            // アルヴの自己紹介
            new DialogData("???", "これはこれは。"),
            new DialogData("???", "申し遅れました。"),
            new DialogData("アルヴ", "この場所の支配人を務めている、アルヴと申します。"),
            new DialogData("アルヴ", "以後、お見知り置きを。"),
            
            new DialogData("---", "アルヴ……？"),
            new DialogData("---", "ここって……何なの？"),
            new DialogData("---", "どうして私、ここに……"),
            
            // VOID REDの説明
            new DialogData("アルヴ", "本当に、何も覚えていらっしゃらないのですね。"),
            new DialogData("アルヴ", "では、ボクがご説明いたしましょう。"),
            new DialogData("アルヴ", "ここはVOID RED。"),
            new DialogData("アルヴ", "あなたが自ら望んで来られた、オークション会場です。"),
            
            new DialogData("---", "……オークション？"),
            new DialogData("---", "私が……望んで？"),
            
            // 精神オークションの説明
            new DialogData("アルヴ", "ええ。あなたにはとても愉快なオークションにご参加いただきます。"),
            new DialogData("アルヴ", "その名も——「精神オークション」。"),
            
            new DialogData("---", "精神……オークション？"),
            
            // クイズ
            new DialogData("アルヴ", "この会場には、様々な品物が出品されています。"),
            new DialogData("アルヴ", "さて、ここでクイズです。"),
            new DialogData("アルヴ", "何が並んでいると思いますか？"),
            
            new DialogData("---", "……壺？ 絵画？ 高そうなものとか……？"),
            
            // 記憶の説明
            new DialogData("アルヴ", "面白いですね。"),
            new DialogData("アルヴ", "正解は——「記憶」。"),
            
            new DialogData("---", "……記憶！？"),
            new DialogData("---", "一体どういうこと…？"),
            
            new DialogData("アルヴ", "驚かれるのも当然です。"),
            new DialogData("アルヴ", "さらに申し上げますと——"),
            new DialogData("アルヴ", "ここには、あなたの記憶も出品されています。"),
            
            // 主人公の反発
            new DialogData("---", "……っ！？"),
            new DialogData("---", "じゃあ……私が何も思い出せないのって……"),
            new DialogData("---", "あなたのせい……？"),
            new DialogData("---", "返してよ……私の記憶！"),
            
            // ルールの説明
            new DialogData("アルヴ", "それはできません。"),
            new DialogData("アルヴ", "ここでは、自分の力で記憶を取り戻していただきます。"),
            new DialogData("アルヴ", "それが、この会場のルールです。"),
            
            new DialogData("---", "勝手に奪っておいて、ルール？"),
            new DialogData("---", "ふざけないで。そんなの、従うわけ——"),
            
            new DialogData("アルヴ", "あなたには、ボクの話した通り——"),
            new DialogData("アルヴ", "このオークションに参加する以外、記憶を取り戻す術はありません。"),
            new DialogData("アルヴ", "……理解が遅いですね。"),
            new DialogData("アルヴ", "少々、くどい。"),
            
            // 主人公の諦め
            new DialogData("---", "……もう……訳が分からない……。"),
            new DialogData("---", "言い返す気力もなくなってきた……"),
            
            new DialogData("アルヴ", "では、大人しくボクの案内を聞いていただけると嬉しいです。"),
            
            // 精神札の説明
            new DialogData("アルヴ", "さて、あなたには記憶を競り落とすために、他のお客様と戦っていただきます。"),
            new DialogData("アルヴ", "その際に使用するのが——精神札。"),
            
            new DialogData("アルヴ", "こちらが精神札。"),
            new DialogData("アルヴ", "あなたの\"価値\"を数値化したものです。"),
            new DialogData("アルヴ", "これがなければ、オークションには参加できません。"),
            new DialogData("アルヴ", "……くれぐれも、破損などなさらぬよう。"),
            
            new DialogData("---", "破損って……そんなガサツに見えてる？"),
            
            // 模擬オークションへ
            new DialogData("アルヴ", "とんでもない。"),
            new DialogData("アルヴ", "ただ、無理はなさらぬように。"),
            new DialogData("アルヴ", "……まあ、今はまだ意味が分からないでしょうから——"),
            new DialogData("アルヴ", "模擬オークションで、慣れていただきましょう。"),
            new DialogData("", "（足音が遠ざかる）"),
            
        };
    }
}