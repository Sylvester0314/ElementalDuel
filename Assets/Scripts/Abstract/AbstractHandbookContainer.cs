using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public abstract class AbstractHandbookContainer : MonoBehaviour
{
    public RectTransform bookmarks;
    public List<AbstractBookPage> pages;
    
    [Header("In Game Data")] 
    public Bookmark choosingMark;

    public virtual async void Open()
    {
        var prevContainer = Handbook.Instance.choosingContainer;
        if (prevContainer == this)
            return;
        if (prevContainer != null)
            await prevContainer.Close();
        
        gameObject.SetActive(true);
        Handbook.Instance.choosingContainer = this;

        var bookmark = GetBookmark(0);
        bookmark.ChickBookmark(true, -1);
        ShowBookmarks(bookmark.Display);
    }

    private async Task Close()
    {
        var bookmark = choosingMark;
        bookmark.CancelChoosingStatus(false);
        pages[bookmark.index].Close();
        HideBookmarks();

        gameObject.SetActive(false);
        choosingMark = null;
        
        const int delay = (int)(Handbook.Duration * 1000 / 8);
        await Task.Delay(delay);
    }

    private Bookmark GetBookmark(int index)
    {
        return bookmarks.GetChild(index).GetComponent<Bookmark>();
    }
    
    private void HideBookmarks(Action callback = null)
    {
        bookmarks
            .DOAnchorPosX(240, Handbook.Duration)
            .SetEase(Ease.OutExpo)
            .OnComplete(() => callback?.Invoke());
    }

    private void ShowBookmarks(Action callback = null)
    {
        bookmarks
            .DOAnchorPosX(0, Handbook.Duration)
            .SetEase(Handbook.Instance.curve)
            .OnComplete(() => callback?.Invoke());
    }
    
    public void SwitchContentPage(int index)
    {
        if (choosingMark != null)
        {
            choosingMark.CancelChoosingStatus(false);
            pages[choosingMark.index].Close();
        }

        pages[index].Open();
    }
}