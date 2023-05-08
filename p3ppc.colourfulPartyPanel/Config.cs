using p3ppc.colourfulPartyPanel.Template.Configuration;
using System.ComponentModel;
using static p3ppc.colourfulPartyPanel.Colours;

namespace p3ppc.colourfulPartyPanel.Configuration
{
    public class Config : Configurable<Config>
    {

        [DisplayName("Protagonist Colour")]
        [Description("The colour of the protagonist's outlines.")]
        [DefaultValue(false)]
        public Colour ProtagColour { get; set; } = new Colour() { R = 0, G = 0, B = 0, A = 255 };


        [DisplayName("Debug Mode")]
        [Description("Logs additional information to the console that is useful for debugging.")]
        [DefaultValue(false)]
        public bool DebugEnabled { get; set; } = false;

    }

    /// <summary>
    /// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
    /// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
    /// </summary>
    public class ConfiguratorMixin : ConfiguratorMixinBase
    {
        // 
    }
}