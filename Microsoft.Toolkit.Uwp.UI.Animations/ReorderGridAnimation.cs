﻿using System;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace Microsoft.Toolkit.Uwp.UI.Animations
{
    /// <summary>
    /// Provides the ability to assign a reorder animation to a GridView.
    /// </summary>
    public class ReorderGridAnimation
    {
        private static readonly DependencyProperty ReorderAnimationProperty =
            DependencyProperty.RegisterAttached("ReorderAnimation", typeof(bool), typeof(ReorderGridAnimation), new PropertyMetadata(null));

        /// <summary>
        /// Gets a value indicating the duration, in milliseconds, the animation should take.
        /// </summary>
        /// <param name="obj">The object to get the value from.</param>
        /// <returns>A value indicating the duration for the animation.</returns>
        public static double GetDuration(DependencyObject obj)
        {
            return (double)obj.GetValue(DurationProperty);
        }

        /// <summary>
        /// Sets a value for the duration, in milliseconds, the animation should take.
        /// </summary>
        /// <param name="obj">the object to set the value on.</param>
        /// <param name="value">The duration in milliseonds.</param>
        public static void SetDuration(DependencyObject obj, double value)
        {
            obj.SetValue(DurationProperty, value);
        }

        /// <summary>
        /// Identifies the Duration attached dependency property.
        /// </summary>
        /// <returns>The identifier for the Duration attached dependency property.</returns>
        public static readonly DependencyProperty DurationProperty = DependencyProperty.RegisterAttached("Duration", typeof(double), typeof(ReorderGridAnimation), new PropertyMetadata(double.NaN, OnDurationChanged));

        private static void OnDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GridView view = d as GridView;
            if (view != null)
            {
                AssignReorderAnimation(view);

                view.ContainerContentChanging -= OnContainerContentChanging;
                view.ContainerContentChanging += OnContainerContentChanging;
            }
        }

        private static void AssignReorderAnimation(GridView view)
        {
            var compositor = ElementCompositionPreview.GetElementVisual(view).Compositor;
            var elementImplicitAnimation = view.GetValue(ReorderAnimationProperty) as ImplicitAnimationCollection;
            if (elementImplicitAnimation == null)
            {
                elementImplicitAnimation = compositor.CreateImplicitAnimationCollection();
                view.SetValue(ReorderAnimationProperty, elementImplicitAnimation);
            }

            double duration = (double)view.GetValue(DurationProperty);
            elementImplicitAnimation["Offset"] = CreateOffsetAnimation(compositor, duration);
        }

        private static void OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var elementVisual = ElementCompositionPreview.GetElementVisual(args.ItemContainer);
            if (args.InRecycleQueue)
            {
                elementVisual.ImplicitAnimations = null;
            }
            else
            {
                var elementImplicitAnimation = sender.GetValue(ReorderAnimationProperty) as ImplicitAnimationCollection;
                elementVisual.ImplicitAnimations = elementImplicitAnimation;
            }
        }

        private static CompositionAnimationGroup CreateOffsetAnimation(Compositor compositor, double duration)
        {
            Vector3KeyFrameAnimation offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(duration);
            offsetAnimation.Target = "Offset";

            CompositionAnimationGroup animationGroup = compositor.CreateAnimationGroup();
            animationGroup.Add(offsetAnimation);

            return animationGroup;
        }
    }
}