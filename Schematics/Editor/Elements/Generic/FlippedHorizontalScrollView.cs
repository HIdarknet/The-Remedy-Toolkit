using UnityEngine;
using UnityEngine.UIElements;

public class FlippedHorizontalScrollView : VisualElement
{
    private readonly ScrollView _scrollProxy;
    private readonly ScrollView _contentScroll;

    public VisualElement ContentContainer => _contentScroll.contentContainer;

    public FlippedHorizontalScrollView()
    {
        style.flexDirection = FlexDirection.Column;
        style.flexGrow = 1;

        // 1. Top ScrollView: acts as the scrollbar controller
        _scrollProxy = new ScrollView(ScrollViewMode.Horizontal)
        {
            name = "top-scrollbar-proxy"
        };
        _scrollProxy.style.marginTop = 0;
        _scrollProxy.style.paddingBottom = 2;
        _scrollProxy.style.height = 16;
        _scrollProxy.style.flexGrow = 0;
        _scrollProxy.contentContainer.style.minWidth = new StyleLength(Length.Percent(100f));
        _scrollProxy.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        _scrollProxy.horizontalScrollerVisibility = ScrollerVisibility.Auto;
        _scrollProxy.contentContainer.style.minHeight = 0.01f; // So it's not invisible
        _scrollProxy.style.backgroundColor = Color.gray1;
        base.Add(_scrollProxy);

        // 2. Main ScrollView: real content, but no horizontal scrollbar
        _contentScroll = new ScrollView(ScrollViewMode.Horizontal)
        {
            name = "content-scroll",
        };
        _contentScroll.style.flexGrow = 1;
        _contentScroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        _contentScroll.verticalScrollerVisibility = ScrollerVisibility.Auto;
        _contentScroll.style.backgroundColor = Color.gray1;
        base.Add(_contentScroll);

        // 3. Sync proxy scrollbar with content scroll
        _scrollProxy.RegisterCallback<GeometryChangedEvent>(_ => UpdateScrollSync());
        _contentScroll.RegisterCallback<GeometryChangedEvent>(_ => UpdateScrollSync());

        // Keep them in sync
        _scrollProxy.schedule.Execute(() =>
        {
            _contentScroll.scrollOffset = new Vector2(_scrollProxy.scrollOffset.x, _contentScroll.scrollOffset.y);
            if (_scrollProxy.horizontalScroller.style.display != DisplayStyle.None)
                _scrollProxy.style.height = 16;
            else
                _scrollProxy.style.height = 0;
        }).Every(16); // ~60fps
    }

    private void UpdateScrollSync()
    {
        float proxyContentWidth = _scrollProxy.contentContainer.resolvedStyle.width;
        float actualContentWidth = _contentScroll.contentContainer.resolvedStyle.width;

        _scrollProxy.contentContainer.style.width = actualContentWidth;
    }

    public new void Add(VisualElement visualElement)
    {
        _contentScroll.Add(visualElement);
    }
}
