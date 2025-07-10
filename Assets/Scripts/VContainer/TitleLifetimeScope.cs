using UnityEngine;
using VContainer;
using VContainer.Unity;

public class TitleLifetimeScope : LifetimeScope
{
    [SerializeField] private int _test = 0;
    private int test2 = 0;
    public int test3 = 0;
    protected override void Configure(IContainerBuilder builder)
    {
    }
}
