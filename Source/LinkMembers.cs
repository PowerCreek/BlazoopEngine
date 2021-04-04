using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace Blazoop.Source
{
    
    public class LinkMember
    {
        public LinkMember Parent { get; set; }
        public LinkMember Prev { get; set; }
        public LinkMember Next { get; set; }
        public LinkMember END { get; }

        public object Value { get; set; }

        private LinkMember() {}        
        
        public LinkMember(object value)
        {
            END = new LinkMember(){Value = "END"};
            
            Value = value;
            Next = END;
            END.Prev = this;
            END.Parent = this;
        }

        public void Pop()
        {
            Prev?.SetNext(END.Next);
            END.Next?.SetPrev(Prev);
        }

        private void Mend(LinkMember start, LinkMember finish, LinkMember node)
        {
            node.Pop();
            start?.SetNext(node);
            node.SetPrev(start);
            
            finish?.SetPrev(node.END);
            node.END.SetNext(finish);
        }
        
        
        public void InsertBefore(LinkMember node)
        {
            if (this == node)
                return;
            //Can't insert itself before itself.

            var first = PreviousSibling;
            if (first is null)
            {
                Parent.Prepend(node);
                //If previous sibling is null, we have to perform Parent.Prepend(node) 
            }
            else
            {
                if (IsAncestorOf(node)) return;
                //IsAncestorOf is checked in a similar way in Parent.Prepend.
                //The check for PreviousSibling being null is quicker than IsAncestorOf
                //so calling IsAncestorOf here is more appropriate than calling first.

                Mend(first.END, this, node);

                node.Parent = Parent;
            }
        }

        public void InsertAfter(LinkMember node)
        {
            if (this == node) return;
            
            LinkMember after;
            if ((after = NextSibling) is null)
            {
                Parent.Add(node);
            }
            else
            {
                if (IsAncestorOf(node)) return;

                Mend(END, after, node);

                node.Parent = Parent;
            }
        }

        public void Prepend(LinkMember node)
        {
            if (this == node || IsAncestorOf(node)) return;
            
            Mend(this, Next, node);

            node.Parent = this;
        }

        public void Add(LinkMember node)
        {
            if (this == node || IsAncestorOf(node)) return;
            Mend(END.Prev, END, node);
            node.Parent = this;
        }

        public void SetNext(LinkMember node) => Next = node;
        
        public void SetPrev(LinkMember node) => Prev = node;

        public bool IsAncestorOf(LinkMember linkMember) => GetParents().FirstOrDefault(parent => parent == linkMember) is not null;

        public IEnumerable<LinkMember> GetParents()
        {
            var node = Parent;
            do
                yield return node;
            while ((node = node?.Parent) is not null);
        }

        public IEnumerable<LinkMember> GetChildren(bool reverse = false)
        {
            if (reverse)
            {
                var node = Next.END.Prev.GetLastSibling();
                do
                    yield return node;
                while ((node = node?.PreviousSibling) is not null);
            }
            else
            {
                var node = Next;
                if(Next == END) yield break;
                do
                    yield return node;
                while ((node = node?.NextSibling) is not null);
            }
        }
        
        public IEnumerable<LinkMember> GetMembers(bool reverse = false)
        {
            if (reverse)
            {
                var node = END;
                do
                {
                    if (node == this)
                        yield break;

                    if (node.Value.Equals("END"))
                        continue;

                    yield return node;
                } while ((node = node?.Prev) != this);
            }
            else
            {
                var node = Next;
                do
                {
                    if (node.Value.Equals("END") && node.Parent == this)
                        yield break;

                    if (node.Value.Equals("END"))
                        continue;

                    yield return node;
                } while ((node = node?.Next) != END);
            }
        }

        public LinkMember GetLastSibling()
        {
            if (Parent is null) return null;
            LinkMember node;
            return (node = Parent.END.Prev.Parent).IsAncestorOf(Parent) ? node : null;

        }

        public LinkMember PreviousSibling
        {
            get
            {
                LinkMember adjacent = Prev?.Parent;

                if (adjacent is null || adjacent == Parent) return null;
                return adjacent.Parent == Parent ? adjacent : null;
            }
        }
        
        public LinkMember NextSibling
        {
            get
            {
                LinkMember adjacent = END?.Next;

                if (adjacent is null || adjacent == Parent?.END) return null;
                return adjacent.Parent == Parent ? adjacent : null;
            }
        }

        public void Replace(LinkMember remove, LinkMember insert)
        {
            remove.InsertAfter(insert);
            remove.Pop();
        }
        
        public LinkMember this[Index key]
        {
            get => GetChildren(key.IsFromEnd).ElementAt(key.Value);
            set => Replace(GetChildren(key.IsFromEnd).ElementAt(key.Value), value);
        }
        
        public IEnumerable<LinkMember> this[Range key]
        {
            get
            {
                LinkMember stop;
                switch (key.Start.IsFromEnd)
                {
                    case false when !key.End.IsFromEnd:
                        return GetChildren()
                            .SkipWhile((a, b) => b < key.Start.Value)
                            .TakeWhile((a, b) => b < key.End.Value);
                    case true when !key.End.IsFromEnd:
                        return GetChildren(true)
                            .TakeWhile((a, b) => b < key.Start.Value)
                            .Reverse()
                            .TakeWhile((a, b) => b < key.End.Value);
                    case true when key.End.IsFromEnd:
                        stop = GetChildren(true).ElementAt(key.Start.Value);

                        return GetChildren(true)
                            .TakeWhile((a, b) => b < key.Start.Value)
                            .SkipWhile((a, b) => b < key.End.Value).Reverse();
                    case false when key.End.IsFromEnd:
                        stop = GetChildren(true).ElementAt(key.End.Value);

                        return GetChildren(false)
                            .SkipWhile((a, b) => b < key.Start.Value)
                            .TakeWhile((a, b) => a != stop);
                    default:
                        return Enumerable.Empty<LinkMember>();
                }
            }
        }
    }

}