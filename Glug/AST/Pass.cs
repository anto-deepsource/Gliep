using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization.Formatters;

namespace GeminiLab.Glug.AST {
    public class Pass {
#region information

        private static class TypewiseStorage<T> where T : class, new() {
            public static T?                   GlobalInformation;
            public static Dictionary<Node, T>? NodeInformation;
        }

        public T GlobalInformation<T>() where T : class, new() {
            return TypewiseStorage<T>.GlobalInformation ??= new T();
        }

        public bool TryGetGlobalInformation<T>([NotNullWhen(true)] out T? info) where T : class, new() {
            return (info = TypewiseStorage<T>.GlobalInformation) != null;
        }

        public T NodeInformation<T>(Node node) where T : class, new() {
            if (!(TypewiseStorage<T>.NodeInformation is {} dict)) {
                dict = TypewiseStorage<T>.NodeInformation = new Dictionary<Node, T>();
            }

            if (dict.TryGetValue(node, out T v)) return v;

            return dict[node] = new T();
        }

        public bool TryGetNodeInformation<T>(Node node, [NotNullWhen(true)] out T? info) where T : class, new() {
            if (!(TypewiseStorage<T>.NodeInformation is {} dict)) {
                info = null;
                return false;
            }

            return dict.TryGetValue(node, out info);
        }

#endregion

#region visitors

        private List<VisitorBase> _visitors = new List<VisitorBase>();

        public void AppendVisitor(VisitorBase visitor) {
            _visitors.Add(visitor);
        }

        public void Visit(Node root) {
            foreach (var visitor in _visitors) {
                visitor.Visit(root, this);
            }
        }

        public TVisitor GetVisitor<TVisitor>() where TVisitor : VisitorBase {
            return _visitors.OfType<TVisitor>().First();
        }
        
#endregion
    }
}
