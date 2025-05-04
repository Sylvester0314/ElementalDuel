using DG.Tweening;
using Shared.Enums;
using UnityEngine;
using UnityEngine.UI;

public class PromptUI : MonoBehaviour
{
    public Global global;
    
    [Header("Sub Components")]
    public Image darkBackground;
    public ElementalTuning tuning;
    public ConfirmButton button;
    public ActionBanner action;
    public Banner banner;
    public Dialog dialog;
    public Header header;
    public Signal signal;
    public Rounds rounds;
    public GameResult result;

    private bool _backgroundShowing;

    public void Start()
    {
        Reset();
    }

    public void CloseAll(bool forced = false)
    {
        if (!forced && global.GetZone<Deck>(Site.Self).IsSwitching)
            return;

        if (forced)
            banner.FixedHide();
        else
            banner.Hide();
        
        if (forced || !global.startingPhase)
            DarkBackgroundHide();
        button.Hide();
        tuning.Hide();
        dialog.Hide();
        header.Hide();
        signal.Hide();
    }
    
    public void Reset()
    {
        DarkBackgroundHide();
        action.Reset();
        banner.Reset();
        button.Reset();
        tuning.Reset();
        dialog.Reset();
        header.Reset();
        signal.Reset();
        rounds.Reset();
    }

    public bool HasComponentShowing()
    {
        return tuning.isShowing || action.isShowing ||
               banner.isShowing || button.isShowing || 
               dialog.isShowing || header.isShowing ||
               signal.isShowing || rounds.isShowing ||
               _backgroundShowing;
    }
    
    public void DarkBackgroundDisplay(float duration = 0)
    {
        darkBackground.DOFade(0.45f, duration);
        darkBackground.enabled = true;
        _backgroundShowing = true;
    }

    public void DarkBackgroundHide(float duration = 0)
    {
        darkBackground.DOFade(0, duration);
        darkBackground.enabled = false;
        _backgroundShowing = false;
    }
}
