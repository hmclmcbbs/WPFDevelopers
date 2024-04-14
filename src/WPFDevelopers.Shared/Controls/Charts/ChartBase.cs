﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using WPFDevelopers.Helpers;

namespace WPFDevelopers.Controls
{
    public class ChartBase : Control
    {

        public static readonly DependencyProperty DatasProperty =
            DependencyProperty.Register("Datas", typeof(IEnumerable<KeyValuePair<string, double>>),
                typeof(ChartBase), new UIPropertyMetadata(DatasChanged));

        static ChartBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChartBase),
                new FrameworkPropertyMetadata(typeof(ChartBase)));
        }

        protected double Rows { get; } = 5;

        protected double Interval { get; } = 120;

        protected short ScaleFactor { get; private set; } = 80;

        protected Brush ChartFill { get; private set; }

        protected double StartX { get; private set; }

        protected double StartY { get; private set; }

        protected double MaxY { get; }

        protected double IntervalY { get; private set; }

        protected Brush NormalBrush => ControlsHelper.PrimaryNormalBrush;


        public IEnumerable<KeyValuePair<string, double>> Datas
        {
            get => (IEnumerable<KeyValuePair<string, double>>) GetValue(DatasProperty);
            set => SetValue(DatasProperty, value);
        }

        private static void DatasChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ChartBase;
            if (e.NewValue != null)
                ctrl.InvalidateVisual();
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (Datas == null || Datas.Count() == 0)
                return;
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;
            ChartFill = Application.Current.TryFindResource("WD.ChartFillSolidColorBrush") as Brush;
            var myPen = new Pen
            {
                Thickness = 1,
                Brush = ChartFill
            };
            myPen.Freeze();

            var xAxiHeight = 4;
            StartY = ActualHeight - (xAxiHeight + myPen.Thickness) - 20;
            var w = ActualWidth;
            StartX = 40;
            var width = Datas.Count() * Interval + StartX;
            IntervalY = 0;
            var x = StartX;
            var y = StartY + myPen.Thickness;

            drawingContext.DrawSnappedLinesBetweenPoints(myPen, myPen.Thickness, new Point(StartX, StartY),
                new Point(width, StartY));

            var points = new List<Point>();
            for (var i = 0; i < Datas.Count() + 1; i++)
            {
                points.Add(new Point(x, y));
                points.Add(new Point(x, y + xAxiHeight));
                x += Interval;
            }

            drawingContext.DrawSnappedLinesBetweenPoints(myPen, myPen.Thickness, points.ToArray());

            var formattedText = DrawingContextHelper.GetFormattedText(IntervalY.ToString(),
                ChartFill, FlowDirection.LeftToRight);
            drawingContext.DrawText(formattedText,
                new Point(StartX - formattedText.Width * 2, StartY - formattedText.Height / 2));

            var xAxisPen = new Pen
            {
                Thickness = 1,
                Brush = Application.Current.TryFindResource("WD.ChartXAxisSolidColorBrush") as Brush
            };
            xAxisPen.Freeze();
            var max = Convert.ToInt32(Datas.Max(kvp => kvp.Value));
            var min = Convert.ToInt32(Datas.Min(kvp => kvp.Value));
            ScaleFactor = Convert.ToInt16(StartY / Rows);
            var yAxis = StartY - ScaleFactor;
            points.Clear();
            var average = Convert.ToInt32(max / Rows);
            var result = Enumerable.Range(0, (Convert.ToInt32(max) - average) / average + 1)
                .Select(i => average + i * average);
            foreach (var item in result)
            {
                points.Add(new Point(StartX, yAxis));
                points.Add(new Point(width, yAxis));
                IntervalY = item;
                formattedText = DrawingContextHelper.GetFormattedText(IntervalY.ToString(),
                    ChartFill, FlowDirection.LeftToRight);
                drawingContext.DrawText(formattedText,
                    new Point(StartX - formattedText.Width - 10, yAxis - formattedText.Height / 2));
                yAxis -= ScaleFactor;
            }
            drawingContext.DrawSnappedLinesBetweenPoints(xAxisPen, xAxisPen.Thickness, points.ToArray());
        }
    }
}