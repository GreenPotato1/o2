using PFRCenterGlobal.Core.Core.Animations.Base;
using Xamarin.Forms;

namespace PFRCenterGlobal.Core.Core.Triggers
{
    public class BeginAnimation : TriggerAction<VisualElement>
    {
        public AnimationBase Animation { get; set; }

        protected override async void Invoke(VisualElement sender)
        {
            if (Animation != null)
                await Animation.Begin();
        }
    }
}