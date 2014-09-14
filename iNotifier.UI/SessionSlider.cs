using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SessionSpotter
{
    public class SessionSlider : Slider
    {
        public static readonly DependencyProperty PrefixProperty =
           DependencyProperty.Register("Prefix", typeof(string), typeof(SessionSlider));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SessionSlider));
        
        public static readonly DependencyProperty PostfixProperty =
            DependencyProperty.Register("Postfix", typeof(string), typeof(SessionSlider));
        
        public static readonly DependencyProperty ThumbTemplateProperty =
            DependencyProperty.Register("ThumbTemplate", typeof(ControlTemplate), typeof(SessionSlider));

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(SessionSlider));

        public static readonly DependencyProperty PrefixForegroundProperty =
            DependencyProperty.Register("PrefixForeground", typeof(Brush), typeof(SessionSlider));
        
        public static readonly DependencyProperty PostfixForegroundProperty =
            DependencyProperty.Register("PostfixForeground", typeof(Brush), typeof(SessionSlider));

        public ControlTemplate ThumbTemplate
        {
            get { return (ControlTemplate)GetValue(ThumbTemplateProperty); }
            set { SetValue(ThumbTemplateProperty, value); }
        }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public string Prefix
        {
            get { return (string)GetValue(PrefixProperty); }
            set { SetValue(PrefixProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string Postfix
        {
            get { return (string)GetValue(PostfixProperty); }
            set { SetValue(PostfixProperty, value); }
        }

        public Brush PrefixForeground
        {
            get { return (Brush)GetValue(PrefixForegroundProperty); }
            set { SetValue(PrefixForegroundProperty, value); }
        }

        public Brush PostfixForeground
        {
            get { return (Brush)GetValue(PostfixForegroundProperty); }
            set { SetValue(PostfixForegroundProperty, value); }
        }
    }
}
