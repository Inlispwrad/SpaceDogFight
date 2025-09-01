using Godot;
using System;

public partial class PlayerShowcase : Control
{
    [Export] private ColorRect colorRect;
    [Export] private Label playerNameLabel;

    public void SetReady(bool _isReady)
    {
        colorRect.Visible = _isReady;
    }
    public void SetPlayerName(string _playerName)
    {
        playerNameLabel.Text = _playerName;
    }
}
