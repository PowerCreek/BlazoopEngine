using System;
using System.Collections.Generic;
using System.Linq;
using Blazoop.ExternalDeps.Classes.Management;
using Blazoop.ExternalDeps.Classes.Management.Operations;
using Blazoop.Source.ElementContexts;
using Blazoop.Source.Properties.Vector;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Blazoop.Source.Operations
{
    public class WindowingService : OperationBase
    {
        public ContainerContext ContainerContext { get; set; }
        
        public WindowContext UnjoinedWindow { get; set; } 

        public Dictionary<object, WindowContext> WindowContextMap = new();
        
        //public Dictionary<TabContext, TabContext> TabContextMap = new();
        public List<WindowContext> WindowRenderOrder = new();
        
        public TabService TabService { get; }
        public DockService DockService { get; }
        
        public WindowingService(IJSRuntime jsRuntime) : base(jsRuntime)
        {
            TabService = new TabService();
            DockService = new DockService();
        }

        public WindowContext CreateWindow
        {
            get
            {
                WindowContext windowContext = new WindowContext(ContainerContext.NodeBase);
                ContainerContext.SlideMember.Add(RegisterWindow(windowContext).ElementNode);
                ContainerContext.Slider.SurrogateReference?.ChangeState();
                TabService.CreateGroup(windowContext);
                return windowContext;
            }
        }

        public void RemoveWindow(WindowContext context)
        {
            if (context == UnjoinedWindow) return;
            
            //TabService.UnjoinTabGroup(context);
            TabService.TabGroupMap[TabService.ObjectTabMap[context]].Group.ToList()
                .ForEach(tab => AddTabToWindow(UnjoinedWindow, tab));
            
            TabService.TabGroupMap.Remove(TabService.ObjectTabMap[context]);
            TabService.ObjectTabMap.Remove(context);
            
            WindowContextMap.Remove(context);
            WindowRenderOrder.Remove(context);
            context.ElementNode.Pop();
            context.Key += "_";
            context.SurrogateReference?.ChangeState();
            ContainerContext.Slider.SurrogateReference?.ChangeState();
            TabService.OnTabMove?.Invoke();
        }

        public WindowContext RegisterWindow(WindowContext context)
        {
            WindowToFront(context);
            WindowContextMap.Add(context, context);
            return context;
        }
        
        public void WindowToFront(WindowContext context)
        {
            WindowRenderOrder.Remove(context);
            int index = 1;
            foreach (var item in WindowRenderOrder.Where(item => item != context))
            {
                item.WithAttribute("style", out StyleContext styleContext);
                styleContext.WithStyle(item.StyleOperator, item, 
                    ("z-index",$"{index++}"));
            }

            WindowRenderOrder.Add(context);
            context.WithAttribute("style", out StyleContext styleContext2);
            styleContext2.WithStyle(context.StyleOperator, context, 
                ("z-index",$"{index}"));
        }

        public TabData CreateTab<T>() where T: TabData, new() => TabService.CreateTab();

        public void AddTabToWindow(WindowContext context, TabData tab)
        {
            if (context is null)
            {
                return;
            }
            
            if (TabService.ObjectTabMap[context] == tab.TabGroup) return;
            TabService.AddTabToGroup(TabService.ObjectTabMap[context], tab);
            tab.ConnectToWindow(context);
            UpdateTabs(TabService.ObjectTabMap[context]);
        }

        public void UpdateTabs(string group)
        {
            var tabs = TabService.TabGroupMap[group];
            var lastTab = tabs.Order.LastOrDefault();
            SelectTab(lastTab);
        }

        public void RemoveTabFromWindow(TabData tab)
        {
            string previousGroup = tab.TabGroup;
            //tab.DisconnectTab();
            //TabService.AddTabToGroup(TabService.UNJOINED, tab);
            AddTabToWindow(UnjoinedWindow, tab);
            ((ElementContext)tab.TabContext.ElementNode.Parent.Value).SurrogateReference.ChangeState();
            
            UpdateTabs(previousGroup);
            //((ElementContext)tab.Content.ElementNode.Parent.Value).SurrogateReference.ChangeState();
        }

        public void CreateUnjoinedWindow()
        {
            WindowContext windowContext = new WindowContext(ContainerContext.NodeBase)
            {
                cssClass = "none"
            };
            
            ContainerContext.SlideMember.Add(RegisterWindow(windowContext).ElementNode);
            ContainerContext.Slider.SurrogateReference?.ChangeState();
            TabService.ObjectTabMap.Add(windowContext, TabService.UNJOINED);
            
            UnjoinedWindow = windowContext;
        }

        public void SelectTab(TabData tab)
        {
            if(tab is not null) TabService.SelectTab(tab);
        }
        
        public void OnContainerWheel(dynamic args)
        {
            
            if (LastCursor is not "") return;

            int amount = -Math.Sign(args.DeltaY) * 25;

            if (args.ShiftKey)
            {
                StartWheelPos.X += amount;
                ContainerContext.SliderTransform.Position.X += amount;
            }
            else
            {
                StartWheelPos.Y += amount;
                ContainerContext.SliderTransform.Position.Y += amount;
            }

            if (WindowDraggingWithTitlebar == null) return;

            if (WindowDraggingWithTitlebar != null)
            {
                var position = WindowDraggingWithTitlebar.Transform.Position;
                if (args.ShiftKey)
                {
                    position.X -= amount;
                }else
                {
                    position.Y -= amount;
                }
            }
        }

        public Action<int> SlideContainer;
        
        ////////////////////////////////////////////////////////////////////////////////////////////////
        public WindowContext WindowDraggingWithTitlebar { get; set; }
        public WindowContext WindowDraggingWithResize { get; set; }
        
        private TabData TabDragData = null;
        public Position ScreenStartPos { get; set; }
        public Position CurrentScreenPos { get; set; }
        public Position BeforeScreenPos { get; set; }
        public Position DeltaPos { get; set; }

        public const int MIN_WIDTH = 80;
        public const int MIN_HEIGHT = 60;
        
        public Position ConvertMousePosition(dynamic args)
        {
            Position hold = new Position((int) args.ClientX, (int) args.ClientY);

            hold.X -= ContainerContext.SliderTransform.Position.X;
            hold.Y -= ContainerContext.SliderTransform.Position.Y+ContainerContext.ToolbarTransform.Size.Height;
            
            return hold;
        }

        public void ContainerMouseLeave(dynamic args) => EndTitleDrag();
        
        public void ContainerMouseUp(dynamic args)
        {
            LeftMouseDown = false;
            
            ContainerContext.SetCursor("default");
            EndTitleDrag();
            EndResizeDrag();
        }
        
        public void ContainerMouseDown(dynamic args)
        {
            LeftMouseDown = true;
        }
        
        public void OnContainerTabDropped(dynamic args)
        {
            if (TabDragData is null) return;
            
            WindowContext currentWindow = TabService.ObjectTabMap.FirstOrDefault(e=>e.Value==TabDragData.TabGroup).Key as WindowContext;
            
            WindowContext createdWindow = null;
            Position cPos = currentWindow.Transform.Position;
            
            if (TabService.TabGroupMap[TabDragData.TabGroup].Group.Count > 1)
            {
                string oldGroup = TabDragData.TabGroup;
                createdWindow = CreateWindow;
                createdWindow.Transform.Size = currentWindow.Transform.Size;
                //Console.WriteLine(currentWindow.Transform.Size.Width+":"+currentWindow.Transform.Size.Height);
                currentWindow = createdWindow;
                AddTabToWindow(createdWindow, TabDragData);
                UpdateTabs(oldGroup);
            }
            
            Position changePos = ConvertMousePosition(args);
            
            changePos.X -= Math.Abs(cPos.X - WindowStartPos.X);
            changePos.Y -= Math.Abs(cPos.Y - WindowStartPos.Y);
            
            currentWindow.Transform.Position = changePos;
            createdWindow?.SurrogateReference?.ChangeState();

            WindowToFront(currentWindow);
            EndTabDrop();
        }

        public void OnTabWindowDrop(dynamic args, WindowContext windowContext)
        {
            if (TabDragData is null || TabDragData.TabGroup == TabService.ObjectTabMap[windowContext]) return;

            string oldGroup = TabDragData.TabGroup;
            
            WindowContext currentWindow = TabService.ObjectTabMap.FirstOrDefault(e=>e.Value==TabDragData.TabGroup).Key as WindowContext;


            AddTabToWindow(windowContext, TabDragData);
            UpdateTabs(oldGroup);
            UpdateTabs(TabDragData.TabGroup);

            windowContext.TabSection.SurrogateReference?.ChangeState();
            currentWindow.TabSection.SurrogateReference?.ChangeState();
            
            WindowToFront(windowContext);
            
            if (TabService.TabGroupMap[oldGroup].Group.Count is 0)
                RemoveWindow(currentWindow);
            
            EndTabDrop();
        }

        public void OnTabTabDrop(DragEventArgs args, TabData tabTarget)
        {
            if (tabTarget == TabDragData) return;
            WindowContext targetWindow =
                TabService.ObjectTabMap.FirstOrDefault(e => e.Value == tabTarget.TabGroup).Key as WindowContext;

            WindowContext currentWindow =
                TabService.ObjectTabMap.FirstOrDefault(e => e.Value == TabDragData.TabGroup).Key as WindowContext;
            string oldGroup = TabDragData.TabGroup;
            AddTabToWindow(targetWindow, TabDragData);

            var hold = TabService.TabGroupMap[tabTarget.TabGroup].Group;

            bool minRange = args.OffsetX <= 12;

            if (hold[1] == TabDragData && hold[0] == tabTarget)
            {
                minRange = true;
            }else if (hold[1] == tabTarget && hold[0] == TabDragData)
            {
                minRange = false;
            }
            else if(hold[^1] == tabTarget && hold[^2]==TabDragData)
            {
                minRange = false;
            }
            else if(hold[^1] == TabDragData && hold[^2]==tabTarget)
            {
                minRange = true;
            }

            TabService.InsertTab(TabDragData, tabTarget, minRange);
            
            UpdateTabs(oldGroup);
            UpdateTabs(tabTarget.TabGroup);
            
            targetWindow.TabSection.SurrogateReference?.ChangeState();
            currentWindow.TabSection.SurrogateReference?.ChangeState();

            if (TabService.TabGroupMap[oldGroup].Group.Count is 0) RemoveWindow(currentWindow);
        }
        
        public void ContainerMouseMove(dynamic args)
        {
            
            if (CurrentScreenPos != null)
            {
                //BeforeScreenPos = new((int) args.ScreenX, (int) args.ScreenY);
                BeforeScreenPos = ConvertMousePosition(args);
                BeforeScreenPos.Y += StartWheelPos.Y;
                BeforeScreenPos.X += StartWheelPos.X;
                DeltaPos = new(CurrentScreenPos.X - BeforeScreenPos.X, CurrentScreenPos.Y - BeforeScreenPos.Y);
            }

            CurrentScreenPos = BeforeScreenPos;
            
            if (WindowDraggingWithTitlebar != null && LastCursor == "")
            {
                WindowDraggingWithTitlebar.Transform.Position.X -= DeltaPos.X;
                WindowDraggingWithTitlebar.Transform.Position.Y -= DeltaPos.Y;
            }

            if (WindowDraggingWithResize != null && (DirX != 0 || DirY != 0) )
            {
                
                var changeX = WindowDraggingWithResize.Transform.Position.X;
                var changeY = WindowDraggingWithResize.Transform.Position.Y;
                var changeW = WindowDraggingWithResize.Transform.Size.Width;
                var changeH = WindowDraggingWithResize.Transform.Size.Height;
                
                if (DirX == -1)
                {
                    int nX = CurrentScreenPos.X;
                    int nW = WindowDraggingWithResize.Transform.Size.Width-(nX-WindowDraggingWithResize.Transform.Position.X);
                    if (nW <= MIN_WIDTH)
                    {
                        changeX += changeW-MIN_WIDTH;
                        changeW = MIN_WIDTH;
                    }
                    else
                    {
                        changeX = CurrentScreenPos.X;
                        changeW = nW;
                    }
                }
                if (DirX == 1)
                {
                    int nX = CurrentScreenPos.X;
                    int nW = CurrentScreenPos.X-changeX;
                    changeW = nW <= MIN_WIDTH ? MIN_WIDTH : nW;
                }

                if (DirY == -1)
                {
                    int nY = CurrentScreenPos.Y;
                    int nH = WindowDraggingWithResize.Transform.Size.Height-(nY-WindowDraggingWithResize.Transform.Position.Y);
                    if (nH <= MIN_HEIGHT)
                    {
                        changeY += changeH-MIN_HEIGHT;
                        changeH = MIN_HEIGHT;
                    }
                    else
                    {
                        changeY = CurrentScreenPos.Y;
                        changeH = nH;
                    }
                    //changeY -= StartWheelPos;
                    //changeH += StartWheelPos;
                }

                if (DirY == 1)
                {
                    int nY = CurrentScreenPos.Y;
                    int nH = CurrentScreenPos.Y-changeY;
                    changeH = nH <= MIN_HEIGHT ? MIN_HEIGHT : nH;
                    changeH -= StartWheelPos.Y;

                    //changeH;
                }

                if (DirX != 0)
                {
                    WindowDraggingWithResize.Transform.Position.X = changeX;
                    WindowDraggingWithResize.Transform.Size.Width = changeW;
                }

                if (DirY != 0)
                {
                    WindowDraggingWithResize.Transform.Position.Y = changeY;
                    WindowDraggingWithResize.Transform.Size.Height = changeH;
                }
            }
        }

        public void EndTitleDrag()
        {
            WindowDraggingWithTitlebar = null;
        }
        public void EndResizeDrag()
        {
            if (WindowDraggingWithResize == null) return;
                WindowDraggingWithResize = null;
                
            ContainerContext.SetCursor("default");
            LastCursor = "";
        }

        public Position StartWheelPos = new Position();
        
        public void StartDragAction(dynamic args)
        {
            StartWheelPos.X = 0;
            StartWheelPos.Y = 0;
            CurrentScreenPos = ScreenStartPos = ConvertMousePosition(args);
        }

        public void WindowTitleBarDown(dynamic args, WindowContext windowContext)
        {            
            StartDragAction(args);
            WindowDraggingWithTitlebar = windowContext;
        }

        public Position WindowStartPos;
        
        public void WindowMouseDown(dynamic args, WindowContext windowContext)
        {
            WindowStartPos = new Position(
                ConvertMousePosition(args).X,
                ConvertMousePosition(args).Y);
            
            StartDragAction(args);
            WindowDraggingWithResize = windowContext;
        }
        
        public void WindowTitleBarUp(dynamic args, WindowContext windowContext)
        {
            EndTitleDrag();
            
        }
        
        public void WindowResizeUp(dynamic args)
        {
            EndResizeDrag();
        }

        private string LastCursor { get; set; } = "";
        
        private int DirX = 0;
        private int DirY = 0;

        public bool LeftMouseDown = false;
        
        public void WindowMouseMove(dynamic args, WindowContext windowContext)
        {
            if (LeftMouseDown) return;
            if (args.Buttons != 0) return;
            DirX = 0;
            DirY = 0;
         
            //Position Offset = new((int)args.ScreenX - windowContext.Transform.Position.X, 
            //    (int) args.ScreenY - windowContext.Transform.Position.Y);

            Position Offset = ConvertMousePosition(args);
            Offset.X -= windowContext.Transform.Position.X;
            Offset.Y -= windowContext.Transform.Position.Y;

            Size Bounds = windowContext.Transform.Size;
            
            string cursor = "";

            const int space = 4;

            if (Offset.X <= space || Offset.X >= Bounds.Width - space)
            {
                (cursor, DirX) = Offset.X <= space ? ("w", -1) : ("e", 1);
            }

            if (Offset.Y <= space || Offset.Y >= Bounds.Height - space)
            {
                if (DirX != 0 && Offset.Y <= space)
                {
                    (cursor, DirY) = ("n" + cursor, -1);
                }

                if (Offset.Y >= Bounds.Height - space)
                {
                    (cursor, DirY) = ("s" + cursor, 1);
                }
            }

            if (cursor == "n")
            {
                //cursor = $"{cursor}-resize";
            }
            
            if (cursor != "")
            {
                cursor = $"{cursor}-resize";
            }
            
            if (cursor == "" && LastCursor != "")
            {
                ContainerContext.SetCursor(cursor);
            }

            if (cursor != "" && cursor != LastCursor)
            {
                ContainerContext.SetCursor(cursor);
            }
            LastCursor = cursor;

            if (cursor == "")
            {
                WindowDraggingWithResize = null;
            }
        }

        public void WindowMouseLeave(dynamic args)
        {
            
            if (LastCursor != "" && !LeftMouseDown)
            {
                LastCursor = "";
                ContainerContext.SetCursor("default");
            }
        }

        
        public void TabDragStart(dynamic args, TabData tabData)
        {
            TabDragData = tabData;
        }

        public void EndTabDrop()
        {
            TabDragData = null;
            LeftMouseDown = false;
            WindowDraggingWithTitlebar = null;
            WindowDraggingWithResize = null;
        }
        
        public void TabDragEnd(dynamic args, TabData tabData)
        {
            WindowDraggingWithTitlebar = null;
            WindowDraggingWithResize = null;
            TabDragData = null;
            LeftMouseDown = false;
        }


        public void TabMouseDown(dynamic args, TabData tabdata)
        {
            int xTrim = 24 - (int) args.OffsetX;
        }
    }
    
}