using Game.State;
using Godot;
using GodotUtilities;

namespace Game.UI
{
    public class Cursor : CanvasLayer
    {
        private const string ANIM_DEFAULT = "default";
        private const string ANIM_CLICK = "click";
        private const string INPUT_CLICK = "click";
        private const string INPUT_CLICK_ALTERNATE = "click_alternate";

        [Export]
        private Texture destroyTexture;

        [Node("Node2D/Sprite")]
        private Sprite sprite;
        [Node("Node2D/Control")]
        private Control control;
        [Node("Node2D/Control/PanelContainer")]
        private PanelContainer panelContainer;
        [Node("Node2D/Control/PanelContainer/Label")]
        private Label label;
        [Node("Node2D/Control/PanelContainer/AnimationPlayer")]
        private AnimationPlayer panelAnimationPlayer;
        [Node]
        private AnimationPlayer animationPlayer;

        private Node tooltipOwner;
        private Texture originalTexture;

        public override void _EnterTree()
        {
            this.WireNodes();
            GameState.CreateEffect<BoardActions.ShowTooltip>(this, nameof(ShowTooltipEffect));
            GameState.CreateEffect<BoardActions.ClearTooltip>(this, nameof(ClearTooltipEffect));
        }

        public override void _Ready()
        {
            originalTexture = sprite.Texture;
            UpdateTooltip(null, false);
            Input.SetMouseMode(Input.MouseMode.Hidden);
        }

        public override void _Process(float delta)
        {
            sprite.GlobalPosition = sprite.GetGlobalMousePosition();
            panelContainer.RectSize = Vector2.Zero;
            panelContainer.RectPivotOffset = panelContainer.RectSize / 2f;
            panelContainer.RectPosition = new Vector2(-panelContainer.RectPivotOffset.x, panelContainer.RectSize.y);
            control.RectGlobalPosition = sprite.GlobalPosition;

            if (Input.IsActionJustPressed(INPUT_CLICK) || Input.IsActionJustPressed(INPUT_CLICK_ALTERNATE))
            {
                if (animationPlayer.IsPlaying())
                {
                    animationPlayer.Seek(0f, true);
                }
                animationPlayer.Play(ANIM_CLICK);
            }
        }

        private void UpdateTooltip(string text, bool useDestructionCursor)
        {
            if (panelAnimationPlayer.IsPlaying())
            {
                panelAnimationPlayer.Seek(0f, true);
                panelAnimationPlayer.Stop();
            }

            label.Text = text;
            if (!string.IsNullOrEmpty(text))
            {
                sprite.Texture = useDestructionCursor ? destroyTexture : originalTexture;
                panelAnimationPlayer.Play(ANIM_DEFAULT);
            }
            else
            {
                sprite.Texture = originalTexture;
                panelContainer.Visible = false;
            }
        }

        private void ShowTooltipEffect(BoardActions.ShowTooltip showTooltip)
        {
            tooltipOwner = showTooltip.Owner;
            UpdateTooltip(showTooltip.Text, showTooltip.ShowDestroy);
        }

        private void ClearTooltipEffect(BoardActions.ClearTooltip clearTooltip)
        {
            if (tooltipOwner == clearTooltip.Owner || clearTooltip.Force)
            {
                UpdateTooltip(null, false);
            }
        }
    }
}
