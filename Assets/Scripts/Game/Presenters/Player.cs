/// <summary>
/// プレイヤークラス
/// ユーザーが操作するプレイヤーを表す
/// </summary>
public class Player : PlayerPresenter
{
    public Player(HandView handView, GameProgressService gameProgressService = null, int maxHandSize = 3) 
        : base(handView, gameProgressService, maxHandSize) { }
}