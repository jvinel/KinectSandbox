﻿/*
 * This file is part of KinectSandbox application. https://github.com/jvinel/KinectSandbox
 * Copyright (C) 2015 Julien Vinel jvinel@gmail.com
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace kinectSandboxUI.UiElement
{
    public class SizeAdorner : Adorner
    {
        private SizeChrome chrome;
        private VisualCollection visuals;
        private ContentControl designerItem;

        protected override int VisualChildrenCount
        {
            get
            {
                return this.visuals.Count;
            }
        }

        public SizeAdorner(ContentControl designerItem)
            : base(designerItem)
        {
            this.SnapsToDevicePixels = true;
            this.designerItem = designerItem;
            this.chrome = new SizeChrome();
            this.chrome.DataContext = designerItem;
            this.visuals = new VisualCollection(this);
            this.visuals.Add(this.chrome);
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.visuals[index];
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            this.chrome.Arrange(new Rect(new Point(0.0, 0.0), arrangeBounds));
            return arrangeBounds;
        }
    }
}
