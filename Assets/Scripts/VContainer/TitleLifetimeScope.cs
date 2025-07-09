using VContainer;
using VContainer.Unity;

public class TitleLifetimeScope : LifetimeScope
{
protected override void Configure(IContainerBuilder builder)
{
// 意図的なフォーマット違反
var test=1;
if(test==1){
var result="test";
}
}
}
