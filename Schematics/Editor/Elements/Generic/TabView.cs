using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TabView : VisualElement
{
    private readonly VisualElement _tabBar;
    private readonly VisualElement _tabContentContainer;
    private readonly Dictionary<string, VisualElement> _tabContentMap = new();
    private string _selectedTab;

    public VisualElement this[string tabID]
    {
        get
        {
            return _tabContentMap[tabID];
        }
    }

    public new class UxmlFactory : UxmlFactory<TabView, UxmlTraits> { }

    public TabView()
    {
        style.flexDirection = FlexDirection.Column;

        var tabScrollBar = new ScrollView(ScrollViewMode.Horizontal)
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                flexGrow = 0,
                height = 24
            }
        };

        _tabBar = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                flexGrow = 0
            }
        };
        tabScrollBar.Add(_tabBar);

        _tabContentContainer = new VisualElement
        {
            style =
            {
                flexDirection = FlexDirection.Column,
                flexGrow = 1
            }
        };

        Add(tabScrollBar);
        Add(_tabContentContainer);
    }

    public void AddTab(string tabId, Texture2D icon = null)
    {
        var content = new VisualElement();

        if (_tabContentMap.ContainsKey(tabId))
            throw new ArgumentException($"Tab '{tabId}' already exists.");

        // === Tab Content ===
        content.style.display = DisplayStyle.None;
        _tabContentContainer.Add(content);
        _tabContentMap[tabId] = content;

        // === Tab Button ===
        var tabButton = new Button { text = "", tooltip = tabId };
        tabButton.style.flexDirection = FlexDirection.Row;
        tabButton.style.alignItems = Align.Center;

        if (icon != null)
        {
            var img = new Image { image = icon };
            img.style.width = 16;
            img.style.height = 16;
            img.style.marginRight = 4;
            tabButton.Add(img);
        }

        var label = new Label(tabId)
        {
            style = { unityFontStyleAndWeight = FontStyle.Bold }
        };
        tabButton.Add(label);

        tabButton.clicked += () => SelectTab(tabId);
        _tabBar.Add(tabButton);

        // Auto-select first tab
        if (_tabContentMap.Count == 1)
            SelectTab(tabId);
    }

    public void RemoveTab(string tabId)
    {
        if (!_tabContentMap.ContainsKey(tabId)) return;

        var content = _tabContentMap[tabId];
        _tabContentContainer.Remove(content);
        _tabContentMap.Remove(tabId);

        var tabButton = _tabBar.Q<Button>(name: tabId);
        if (tabButton != null) _tabBar.Remove(tabButton);

        if (_selectedTab == tabId)
        {
            _selectedTab = null;
            if (_tabContentMap.Count > 0)
            {
                var nextTab = new List<string>(_tabContentMap.Keys)[0];
                SelectTab(nextTab);
            }
        }
    }

    public void SelectTab(string tabId)
    {
        if (!_tabContentMap.ContainsKey(tabId)) return;

        foreach (var kvp in _tabContentMap)
            kvp.Value.style.display = DisplayStyle.None;

        _tabContentMap[tabId].style.display = DisplayStyle.Flex;
        _selectedTab = tabId;

        // Optional: update tab button styles
        foreach (var child in _tabBar.Children())
        {
            child.style.backgroundColor = Color.gray3;
        }

        var selectedButton = _tabBar.Children().ElementAt(new List<string>(_tabContentMap.Keys).IndexOf(tabId));
        selectedButton.style.backgroundColor = Color.gray1;
    }
}
