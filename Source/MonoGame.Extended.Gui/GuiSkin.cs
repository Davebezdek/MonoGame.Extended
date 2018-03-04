using System;
using System.Collections.Generic;
using System.IO;
using MonoGame.Extended.BitmapFonts;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Gui.Controls;
using MonoGame.Extended.Gui.Serialization;
using MonoGame.Extended.TextureAtlases;
using Newtonsoft.Json;

namespace MonoGame.Extended.Gui
{
    public class GuiSkin
    {
        public GuiSkin()
        {
            TextureAtlases = new List<TextureAtlas>();
            Fonts = new List<BitmapFont>();
            NinePatches = new List<NinePatchRegion2D>();
            Styles = new KeyedCollection<string, GuiControlStyle>(s => s.Name ?? s.TargetType.Name);
        }

        [JsonProperty(Order = 0)]
        public string Name { get; set; }

        [JsonProperty(Order = 1)]
        public IList<TextureAtlas> TextureAtlases { get; set; }

        [JsonProperty(Order = 2)]
        public IList<BitmapFont> Fonts { get; set; }

        [JsonProperty(Order = 3)]
        public IList<NinePatchRegion2D> NinePatches { get; set; }

        [JsonProperty(Order = 4)]
        public BitmapFont DefaultFont => Fonts.FirstOrDefault();

        [JsonProperty(Order = 5)]
        public GuiCursor Cursor { get; set; }

        [JsonProperty(Order = 6)]
        public KeyedCollection<string, GuiControlStyle> Styles { get; private set; }

        public GuiControlStyle GetStyle(string name)
        {
            return Styles[name];
        }

        public GuiControlStyle GetStyle(Type controlType)
        {
            GuiControlStyle controlStyle = null;

            while (controlStyle == null && controlType != null)
            {
                controlStyle = Styles.FirstOrDefault(s => s.TargetType == controlType);
                controlType = controlType.GetTypeInfo().BaseType;
            }

            return controlStyle;
        }

        public static GuiSkin FromFile(ContentManager contentManager, string path, params Type[] customControlTypes)
        {
            using (var stream = TitleContainer.OpenStream(path))
            {
                return FromStream(contentManager, stream, customControlTypes);
            }
        }

        public static GuiSkin FromStream(ContentManager contentManager, Stream stream, params Type[] customControlTypes)
        {
            var skinSerializer = new GuiJsonSerializer(contentManager, customControlTypes);

            using (var streamReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                return skinSerializer.Deserialize<GuiSkin>(jsonReader);
            }
        }


        public T Create<T>(string template, string name = null, string text = null)
            where T : GuiControl, new()
        {
            return Create<T>(template, Vector2.Zero, name, text);
        }

        public T Create<T>(string template, Vector2 position, string name = null, string text = null)
            where T : GuiControl, new()
        {
            var control = new T();
            GetStyle(template).Apply(control);
            control.Name = name;
            control.Position = position;
            control.Text = text;
            return control;
        }

        public T Create<T>(string template, Action<T> onCreate)
            where T : GuiControl, new()
        {
            var control = new T();
            GetStyle(template).Apply(control);
            onCreate(control);
            return control;
        }

        public GuiControl Create(Type type, string template)
        {
            var control = (GuiControl)Activator.CreateInstance(type);

            if (template != null)
                GetStyle(template).Apply(control);

            return control;
        }

        public static GuiSkin FromDefault(BitmapFont font)
        {
            var skin = new GuiSkin
            {
                Fonts = { font },
                Styles =
                {
                    new GuiControlStyle(typeof(GuiControl))
                    {
                        {nameof(GuiControl.Color), new Color(51, 51, 55)},
                        {nameof(GuiControl.BorderColor), new Color(67, 67, 70)},
                        {nameof(GuiControl.BorderThickness), 1},
                        {nameof(GuiControl.TextColor), new Color(241, 241, 241)},
                        {nameof(GuiStackPanel.Padding), new Thickness(5)}
                    },
                    new GuiControlStyle(typeof(GuiLabel))
                    {
                        {nameof(GuiControl.Color), Color.Transparent},
                        {nameof(GuiControl.TextColor), Color.White}
                    },
                    new GuiControlStyle(typeof(GuiTextBox))
                    {
                        {nameof(GuiControl.Color), Color.LightGray},
                        {nameof(GuiControl.TextColor), Color.Black},
                        {nameof(GuiControl.BorderColor), new Color(67, 67, 70)},
                    },
                    new GuiControlStyle(typeof(GuiButton))
                    {
                        {nameof(GuiControl.Color), new Color(51, 51, 55)},
                        {nameof(GuiControl.BorderColor), new Color(67, 67, 70)},
                        {nameof(GuiControl.BorderThickness), 1},
                        {nameof(GuiControl.TextColor), new Color(241, 241, 241)},
                        {
                            nameof(GuiControl.HoverStyle),
                            new GuiControlStyle
                            {
                                {nameof(GuiButton.Color), new Color(62, 62, 64)}
                            }
                        },
                        {
                            nameof(GuiButton.PressedStyle),
                            new GuiControlStyle
                            {
                                {nameof(GuiButton.Color), new Color(0, 122, 204)}
                            }
                        }
                    }
                }
            };
            return skin;
        }
    }
}