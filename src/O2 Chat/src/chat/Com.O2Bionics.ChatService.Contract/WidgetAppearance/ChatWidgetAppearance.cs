using System.Runtime.Serialization;
using JetBrains.Annotations;
using Jil;

namespace Com.O2Bionics.ChatService.Contract.WidgetAppearance
{
    [DataContract]
    public sealed class ChatWidgetAppearance
    {
        //If you change this class, make changes to the "SpecificClassDiff" class.

        public ChatWidgetAppearance()
        {
            ThemeId = ChatWidgetThemes.Default;
            ThemeMinId = ChatWidgetThemes.DefaultMin;
            Location = ChatWidgetLocation.BottomRight;
            PoweredByVisible = true;
        }

        /// <summary>
        ///     Copy constructor.
        /// </summary>
        /// <param name="chatWidgetAppearance"></param>
        public ChatWidgetAppearance([NotNull] ChatWidgetAppearance chatWidgetAppearance)
        {
            ThemeId = chatWidgetAppearance.ThemeId;
            ThemeMinId = chatWidgetAppearance.ThemeMinId;
            Location = chatWidgetAppearance.Location;
            OffsetX = chatWidgetAppearance.OffsetX;
            OffsetY = chatWidgetAppearance.OffsetY;
            MinimizedStateTitle = chatWidgetAppearance.MinimizedStateTitle;
            CustomCssUrl = chatWidgetAppearance.CustomCssUrl;
            PoweredByVisible = chatWidgetAppearance.PoweredByVisible;
        }

        [JilDirective("themeId")]
        [DataMember(Name = "themeId")]
        public string ThemeId { get; set; }

        [JilDirective("themeMinId")]
        [DataMember(Name = "themeMinId")]
        public string ThemeMinId { get; set; }

        [JilDirective("location")]
        [DataMember(Name = "location")]
        public ChatWidgetLocation Location { get; set; }

        [JilDirective("offsetX")]
        [DataMember(Name = "offsetX")]
        public int OffsetX { get; set; }

        [JilDirective("offsetY")]
        [DataMember(Name = "offsetY")]
        public int OffsetY { get; set; }

        [JilDirective("minStateTitle")]
        [DataMember(Name = "minStateTitle")]
        public string MinimizedStateTitle { get; set; }

        [JilDirective("customCssUrl")]
        [DataMember(Name = "customCssUrl")]
        public string CustomCssUrl { get; set; }

        [JilDirective("poweredByVisible")]
        [DataMember(Name = "poweredByVisible")]
        public bool PoweredByVisible { get; set; }
    }
}