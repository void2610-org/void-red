/// <summary>
/// プレイヤークラス
/// ユーザーが操作するプレイヤーを表す
/// </summary>
public class Player : PlayerPresenter
{
    public Player(GameProgressService gameProgressService = null) 
        : base(gameProgressService){ }
}