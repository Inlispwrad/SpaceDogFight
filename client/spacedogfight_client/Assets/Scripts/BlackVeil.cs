using Godot;
using System;

public partial class BlackVeil : Panel
{
    [Export] private RichTextLabel blackScreenNoticer;

    private Timer fadeOutTimer = null;
    private Timer fadeInTimer = null;
    
    public override void _Process(double delta)
    {
        if (fadeOutTimer != null)
        {
            float alpha = (float)(fadeOutTimer.TimeLeft / fadeOutTimer.WaitTime);
            Modulate = new Color(Modulate, alpha);
        }

        if (fadeInTimer != null)
        {
            float alpha = (float)((fadeInTimer.WaitTime - fadeInTimer.TimeLeft) / fadeInTimer.WaitTime);
            Modulate = new Color(Modulate, alpha);
        }
    }

    public void Close(float _duration = 0f)
    {
        if (_duration > 0)
        {
            fadeOutTimer = new Timer();
            fadeOutTimer.WaitTime = _duration;
            fadeOutTimer.Timeout += () =>
            {
                this.RemoveChild(fadeOutTimer);
                this.Visible = false;
                fadeOutTimer = null;
            };
            AddChild(fadeOutTimer);
            fadeOutTimer.Start();
        }
        else
        {
            this.Visible = false;
        }
    }

    public void Show(float _duration = 0)
    {
        this.Visible = true;
        if (_duration > 0)
        {
            fadeInTimer = new Timer();
            fadeInTimer.WaitTime = _duration;
            fadeInTimer.Timeout += () =>
            {
                this.RemoveChild(fadeInTimer);
                fadeInTimer = null;
            };
            AddChild(fadeInTimer);
            fadeInTimer.Start();
        }
    }

    public void SetNotice(string _noticeText)
    {
        blackScreenNoticer.Text = $"[font_size=48]{_noticeText}[/font_size]";
    }

}
