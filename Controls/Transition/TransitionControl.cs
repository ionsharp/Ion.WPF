using Ion.Collect;
using Ion.Numeral;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Ion.Controls;

#region VisualStates

internal static class VisualStates
{
    public const string GroupCommon = "CommonStates";
    public const string StateNormal = "Normal";
    public const string StateReadOnly = "ReadOnly";
    public const string StateMouseOver = "MouseOver";
    public const string StatePressed = "Pressed";
    public const string StateDisabled = "Disabled";
    public const string GroupFocus = "FocusStates";
    public const string StateUnfocused = "Unfocused";
    public const string StateFocused = "Focused";
    public const string GroupSelection = "SelectionStates";
    public const string StateSelected = "Selected";
    public const string StateUnselected = "Unselected";
    public const string StateSelectedInactive = "SelectedInactive";
    public const string GroupExpansion = "ExpansionStates";
    public const string StateExpanded = "Expanded";
    public const string StateCollapsed = "Collapsed";
    public const string GroupPopup = "PopupStates";
    public const string StatePopupOpened = "PopupOpened";
    public const string StatePopupClosed = "PopupClosed";
    public const string GroupValidation = "ValidationStates";
    public const string StateValid = "Valid";
    public const string StateInvalidFocused = "InvalidFocused";
    public const string StateInvalidUnfocused = "InvalidUnfocused";
    public const string GroupExpandCompassDirection = "ExpandCompassDirectionStates";
    public const string StateExpandDown = "ExpandDown";
    public const string StateExpandUp = "ExpandUp";
    public const string StateExpandLeft = "ExpandLeft";
    public const string StateExpandRight = "ExpandRight";
    public const string GroupHasItems = "HasItemsStates";
    public const string StateHasItems = "HasItems";
    public const string StateNoItems = "NoItems";
    public const string GroupIncrease = "IncreaseStates";
    public const string StateIncreaseEnabled = "IncreaseEnabled";
    public const string StateIncreaseDisabled = "IncreaseDisabled";
    public const string GroupDecrease = "DecreaseStates";
    public const string StateDecreaseEnabled = "DecreaseEnabled";
    public const string StateDecreaseDisabled = "DecreaseDisabled";
    public const string GroupInteractionMode = "InteractionModeStates";
    public const string StateEdit = "Edit";
    public const string StateDisplay = "Display";
    public const string GroupLocked = "LockedStates";
    public const string StateLocked = "Locked";
    public const string StateUnlocked = "Unlocked";
    public const string StateActive = "Active";
    public const string StateInactive = "Inactive";
    public const string GroupActive = "ActiveStates";
    public const string StateUnwatermarked = "Unwatermarked";
    public const string StateWatermarked = "Watermarked";
    public const string GroupWatermark = "WatermarkStates";
    public const string StateCalendarButtonUnfocused = "CalendarButtonUnfocused";
    public const string StateCalendarButtonFocused = "CalendarButtonFocused";
    public const string GroupCalendarButtonFocus = "CalendarButtonFocusStates";
    public const string StateBusy = "Busy";
    public const string StateIdle = "Idle";
    public const string GroupBusyStatus = "BusyStatusStates";
    public const string StateVisible = "Visible";
    public const string StateHidden = "Hidden";
    public const string GroupVisibility = "VisibilityStates";

    public static void GoToState(Control control, bool useTransitions, params string[] stateNames)
    {
        System.Diagnostics.Debug.Assert(control != null, "control should not be null!");
        System.Diagnostics.Debug.Assert(stateNames != null, "stateNames should not be null!");
        System.Diagnostics.Debug.Assert(stateNames.Length > 0, "stateNames should not be empty!");

        foreach (string name in stateNames)
        {
            if (VisualStateManager.GoToState(control, name, useTransitions))
            {
                break;
            }
        }
    }

    public static FrameworkElement GetImplementationRoot(DependencyObject dependencyObject)
    {
        System.Diagnostics.Debug.Assert(dependencyObject != null, "DependencyObject should not be null.");
        return (VisualTreeHelper.GetChildrenCount(dependencyObject) == 1) ?
            VisualTreeHelper.GetChild(dependencyObject, 0) as FrameworkElement :
            null;
    }

    public static VisualStateGroup TryGetVisualStateGroup(DependencyObject dependencyObject, string groupName)
    {
        FrameworkElement root = GetImplementationRoot(dependencyObject);
        if (root is null)
        {
            return null;
        }

        return VisualStateManager.GetVisualStateGroups(root)
            .OfType<VisualStateGroup>().FirstOrDefault(group => string.CompareOrdinal(groupName, @group.Name) == 0);
    }
}

#endregion

#region TransitionControl

/// <summary>
/// A <see cref="ContentControl"/> that animates content as it loads and unloads.
/// </summary>
/// <license>
/// (c) Copyright Microsoft Corporation.
/// This source is subject to the Microsoft Public License (Ms-PL).
/// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
/// All other rights reserved.
/// </license>
public class TransitionControl : ContentControl
{
    #region Constants

    internal const string PresentationGroup = "PresentationStates";

    internal const string NormalState = "Normal";

    internal const string PreviousContentPresentationSitePartName = "PreviousContentPresentationSite";

    internal const string CurrentContentPresentationSitePartName = "CurrentContentPresentationSite";

    public const Transitions DefaultTransitionState = Transitions.Default;

    #endregion

    #region Fields

    private ContentPresenter currentContentPresentationSite;
    private ContentPresenter previousContentPresentationSite;
    private bool allowIsTransitioningPropertyWrite;

    #endregion

    #region Events

    public event RoutedEventHandler TransitionCompleted;

    #endregion

    #region Properties

    private Storyboard currentTransition;

    private Storyboard CurrentTransition
    {
        get
        {
            return currentTransition;
        }
        set
        {
            // decouple event
            if (currentTransition != null)
                currentTransition.Completed -= OnTransitionCompleted;

            currentTransition = value;

            if (currentTransition != null)
                currentTransition.Completed += OnTransitionCompleted;
        }
    }

    public static readonly DependencyProperty IsTransitioningProperty = DependencyProperty.Register("IsTransitioning", typeof(bool), typeof(TransitionControl), new FrameworkPropertyMetadata(OnIsTransitioningPropertyChanged));
    /// <summary>
    /// Gets/sets if the content is transitioning.
    /// </summary>
    public bool IsTransitioning
    {
        get
        {
            return (bool)GetValue(IsTransitioningProperty);
        }
        private set
        {
            allowIsTransitioningPropertyWrite = true;
            SetValue(IsTransitioningProperty, value);
            allowIsTransitioningPropertyWrite = false;
        }
    }

    private static void OnIsTransitioningPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var source = (TransitionControl)d;

        if (!source.allowIsTransitioningPropertyWrite)
        {
            source.IsTransitioning = (bool)e.OldValue;
            throw new InvalidOperationException();
        }
    }

    public static readonly DependencyProperty TransitionProperty = DependencyProperty.Register("Transition", typeof(Transitions), typeof(TransitionControl), new FrameworkPropertyMetadata(Transitions.Default, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.Inherits, OnTransitionPropertyChanged));
    public Transitions Transition
    {
        get
        {
            return (Transitions)GetValue(TransitionProperty);
        }
        set
        {
            SetValue(TransitionProperty, value);
        }
    }

    private static void OnTransitionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var source = (TransitionControl)d;
        var oldTransition = (Transitions)e.OldValue;
        var newTransition = (Transitions)e.NewValue;

        if (source.IsTransitioning)
        {
            source.AbortTransition();
        }

        // find new transition
        Storyboard newStoryboard = source.GetStoryboard(newTransition);

        // unable to find the transition.
        if (newStoryboard is null)
        {
            // could be during initialization of xaml that presentationgroups was not yet defined
            if (VisualStates.TryGetVisualStateGroup(source, PresentationGroup) is null)
            {
                // will delay check
                source.CurrentTransition = null;
            }
            else
            {
                // revert to old value
                source.SetValue(TransitionProperty, oldTransition);

                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Temporary removed exception message", newTransition));
            }
        }
        else
        {
            source.CurrentTransition = newStoryboard;
        }
    }

    public static readonly DependencyProperty RestartTransitionOnContentChangeProperty = DependencyProperty.Register("RestartTransitionOnContentChange", typeof(bool), typeof(TransitionControl), new FrameworkPropertyMetadata(false, OnRestartTransitionOnContentChangePropertyChanged));
    public bool RestartTransitionOnContentChange
    {
        get
        {
            return (bool)GetValue(RestartTransitionOnContentChangeProperty);
        }
        set
        {
            SetValue(RestartTransitionOnContentChangeProperty, value);
        }
    }

    private static void OnRestartTransitionOnContentChangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((TransitionControl)d).OnRestartTransitionOnContentChangeChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    public static readonly DependencyProperty CustomVisualStatesProperty = DependencyProperty.Register("CustomVisualStates", typeof(ListObservable<VisualState>), typeof(TransitionControl), new FrameworkPropertyMetadata(null));
    public ListObservable<VisualState> CustomVisualStates
    {
        get
        {
            return (ListObservable<VisualState>)GetValue(CustomVisualStatesProperty);
        }
        set
        {
            SetValue(CustomVisualStatesProperty, value);
        }
    }

    public static readonly DependencyProperty CustomVisualStatesNameProperty = DependencyProperty.Register("CustomVisualStatesName", typeof(string), typeof(TransitionControl), new FrameworkPropertyMetadata("CustomTransition"));
    /// <summary>
    /// Gets or sets the name of the custom transition visual state.
    /// </summary>
    public string CustomVisualStatesName
    {
        get
        {
            return (string)GetValue(CustomVisualStatesNameProperty);
        }
        set
        {
            SetValue(CustomVisualStatesNameProperty, value);
        }
    }

    #endregion

    #region TransitionControl

    public TransitionControl() : base()
    {
        CustomVisualStates = [];
    }

    #endregion

    #region Methods

    private Storyboard GetStoryboard(Transitions newTransition)
    {
        newTransition = newTransition == Transitions.Random
            ? (Transitions)Number.Random((int)Transitions.Random - 1) //Exclude <Transitions.Custom>
            : newTransition;

        var group = VisualStates.TryGetVisualStateGroup(this, PresentationGroup);

        Storyboard result = null;
        if (group != null)
        {
            var name = GetTransitionName(newTransition);
            result = group.States.OfType<VisualState>()
                .Where(i => i.Name == name)
                .Select(i => i.Storyboard).FirstOrDefault<Storyboard>();
        }
        return result;
    }

    private string GetTransitionName(Transitions transition)
    {
        return transition switch
        {
            Transitions.Normal => "Normal",
            Transitions.Up => "UpTransition",
            Transitions.Down => "DownTransition",
            Transitions.Right => "RightTransition",
            Transitions.RightReplace => "RightReplaceTransition",
            Transitions.Left => "LeftTransition",
            Transitions.LeftReplace => "LeftReplaceTransition",
            Transitions.Custom => CustomVisualStatesName,
            _ => "DefaultTransition",
        };
    }

    private void StartTransition(object oldContent, object newContent)
    {
        // both presenters must be available, otherwise a transition is useless.
        if (currentContentPresentationSite != null && previousContentPresentationSite != null)
        {
            if (RestartTransitionOnContentChange)
            {
                CurrentTransition.Completed -= OnTransitionCompleted;
            }

            if (ContentTemplateSelector != null)
            {
                previousContentPresentationSite.ContentTemplate = ContentTemplateSelector.SelectTemplate(oldContent, this);
                currentContentPresentationSite.ContentTemplate = ContentTemplateSelector.SelectTemplate(newContent, this);
            }
            else
            {
                previousContentPresentationSite.ContentTemplate = ContentTemplate;
                currentContentPresentationSite.ContentTemplate = ContentTemplate;
            }
            currentContentPresentationSite.Content = newContent;
            previousContentPresentationSite.Content = oldContent;

            // and start a new transition
            if (!IsTransitioning || RestartTransitionOnContentChange)
            {
                if (RestartTransitionOnContentChange)
                {
                    CurrentTransition.Completed += OnTransitionCompleted;
                }
                IsTransitioning = true;
                VisualStateManager.GoToState(this, NormalState, false);
                VisualStateManager.GoToState(this, GetTransitionName(Transition), true);
            }
        }
    }

    private void OnTransitionCompleted(object sender, EventArgs e)
    {
        AbortTransition();
        TransitionCompleted?.Invoke(this, new RoutedEventArgs());
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);

        StartTransition(oldContent, newContent);
    }

    public override void OnApplyTemplate()
    {
        if (IsTransitioning)
        {
            AbortTransition();
        }

        if (CustomVisualStates != null && CustomVisualStates.Any())
        {
            var presentationGroup = VisualStates.TryGetVisualStateGroup(this, PresentationGroup);
            if (presentationGroup != null)
            {
                foreach (var state in CustomVisualStates)
                {
                    presentationGroup.States.Add(state);
                }
            }
        }

        base.OnApplyTemplate();

        previousContentPresentationSite = GetTemplateChild(PreviousContentPresentationSitePartName) as ContentPresenter;
        currentContentPresentationSite = GetTemplateChild(CurrentContentPresentationSitePartName) as ContentPresenter;

        if (currentContentPresentationSite != null)
        {
            if (ContentTemplateSelector != null)
            {
                currentContentPresentationSite.ContentTemplate = ContentTemplateSelector.SelectTemplate(Content, this);
            }
            else
            {
                currentContentPresentationSite.ContentTemplate = ContentTemplate;
            }
            currentContentPresentationSite.Content = Content;
        }

        // hookup currenttransition
        Storyboard transition = GetStoryboard(Transition);
        CurrentTransition = transition;
        if (transition is null)
        {
            var invalidTransition = Transition;
            // revert to default
            Transition = DefaultTransitionState;

            throw new ArgumentException(string.Format("'{0}' Transition could not be found!", invalidTransition), "Transition");
        }
        VisualStateManager.GoToState(this, NormalState, false);
    }

    protected virtual void OnRestartTransitionOnContentChangeChanged(bool oldValue, bool newValue)
    {
    }

    public void AbortTransition()
    {
        // go to normal state and release our hold on the old content.
        VisualStateManager.GoToState(this, NormalState, false);
        IsTransitioning = false;
        if (previousContentPresentationSite != null)
        {
            previousContentPresentationSite.ContentTemplate = null;
            previousContentPresentationSite.Content = null;
        }
    }

    /// <summary>
    /// Reload the current transition if the content is the same.
    /// </summary>
    public void ReloadTransition()
    {
        // both presenters must be available, otherwise a transition is useless.
        if (currentContentPresentationSite != null && previousContentPresentationSite != null)
        {
            if (RestartTransitionOnContentChange)
            {
                CurrentTransition.Completed -= OnTransitionCompleted;
            }
            if (!IsTransitioning || RestartTransitionOnContentChange)
            {
                if (RestartTransitionOnContentChange)
                {
                    CurrentTransition.Completed += OnTransitionCompleted;
                }
                IsTransitioning = true;
                VisualStateManager.GoToState(this, NormalState, false);
                VisualStateManager.GoToState(this, GetTransitionName(Transition), true);
            }
        }
    }

    #endregion
}

#endregion
