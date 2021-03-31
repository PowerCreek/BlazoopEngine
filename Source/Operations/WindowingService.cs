using System;
using System.Collections.Generic;
using System.Linq;
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

        public Dictionary<object, WindowContext> WindowContextMap = new();
        
        //public Dictionary<TabContext, TabContext> TabContextMap = new();
        public List<WindowContext> WindowRenderOrder = new();
        
        public TabService TabService { get; }
        
        public WindowingService(IJSRuntime jsRuntime) : base(jsRuntime)
        {
            TabService = new TabService();
        }

        public WindowContext CreateWindow
        {
            get
            {
                WindowContext windowContext = new WindowContext(ContainerContext.NodeBase);
                ContainerContext.SlideMember.Add(RegisterWindow(windowContext).ElementNode);
                ContainerContext.SurrogateReference?.ChangeState();
                TabService.CreateGroup(windowContext);
                return windowContext;
            }
        }

        public void RemoveWindow(WindowContext context)
        {
            WindowContextMap.Add(context, context);
            WindowRenderOrder.Remove(context);
            context.ElementNode.Pop();
            ContainerContext.SurrogateReference?.ChangeState();
            TabService.UnjoinTabGroup(context);
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
                item.WithAttribute("Style", out StyleContext styleContext);
                styleContext.WithStyle(context.StyleOperator, item, ("z-index",$"{index++}"));
            }

            WindowRenderOrder.Add(context);
            context.WithAttribute("Style", out StyleContext styleContext2);
            styleContext2.WithStyle(context.StyleOperator, context, ("z-index",$"{index}"));
        }
        
        //REGION//REGION//REGION//REGION
        
        
        
        public void ContainerWheel(dynamic args)
        {
            int amount = - Math.Sign(args.DeltaY) * 25;
            
            ContainerContext.SliderTransform.Position.Y += amount;
            StartWheelPos += amount;
            
            if (WindowDraggingWithTitlebar == null && WindowDraggingWithResize == null) return;

            if (WindowDraggingWithTitlebar != null)
            {
                WindowDraggingWithTitlebar.Transform.Position.Y = 
                    WindowDraggingWithTitlebar.Transform.Position.Y + Math.Sign(args.DeltaY)*25;
            }
        }

        public Action<int> SlideContainer;
        
        ////////////////////////////////////////////////////////////////////////////////////////////////
        public WindowContext WindowDraggingWithTitlebar { get; set; }
        public WindowContext WindowDraggingWithResize { get; set; }
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
            hold.Y -= ContainerContext.SliderTransform.Position.Y;
            
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
        
        public void ContainerMouseMove(dynamic args)
        {
            if (CurrentScreenPos != null)
            {
                //BeforeScreenPos = new((int) args.ScreenX, (int) args.ScreenY);
                BeforeScreenPos = ConvertMousePosition(args);
                BeforeScreenPos.Y += StartWheelPos;
                DeltaPos = new(CurrentScreenPos.X - BeforeScreenPos.X, CurrentScreenPos.Y - BeforeScreenPos.Y);
            }

            CurrentScreenPos = BeforeScreenPos;
            
            if (WindowDraggingWithTitlebar != null)
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
                    changeH -= StartWheelPos;

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
        }

        public int StartWheelPos = 0;
        
        public void StartDragAction(dynamic args)
        {
            StartWheelPos = 0;
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

        private TabData TabDragData = null;
        
        public void TabDragStart(dynamic args, TabData tabData)
        {
            TabDragData = tabData;
        }

        public void EndTabDrop()
        {
            TabDragData = null;
        }

        
        public void TabDragEnd(dynamic args, TabData tabData) => TabDragData = null;

        //REGION//REGION//REGION//REGION

        public bool IsMouseOverTabGroup = false; 
        public void TabGroupMouseMove(object args)
        {
            IsMouseOverTabGroup = true;
        }
    }
    
}