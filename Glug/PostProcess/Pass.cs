using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization.Formatters;

namespace GeminiLab.Glug.AST {
    public class Pass {
#region information

        private Dictionary<Type, object> _globalInformation = new Dictionary<Type, object>();
        private Dictionary<Type, Dictionary<Node, object>> _nodeInformation = new Dictionary<Type, Dictionary<Node, object>>();

        public T GlobalInformation<T>() where T : class, new() {
            if (TryGetGlobalInformation<T>(out var result)) return result;

            return (T) (_globalInformation[typeof(T)] = new T());
        }

        public bool TryGetGlobalInformation<T>([NotNullWhen(true)] out T? info) where T : class, new() {
            if (_globalInformation.TryGetValue(typeof(T), out var result)) {
                info = (T) result;
                return true;
            }

            info = null;
            return false;
        }

        public T NodeInformation<T>(Node node) where T : class, new() {
            if (TryGetNodeInformation<T>(node, out var result)) return result;

            return (T) (_nodeInformation[typeof(T)][node] = new T());
        }

        public bool TryGetNodeInformation<T>(Node node, [NotNullWhen(true)] out T? info) where T : class, new() {
            if (!_nodeInformation.TryGetValue(typeof(T), out var dict)) {
                dict = _nodeInformation[typeof(T)] = new Dictionary<Node, object>();
            }

            if (dict.TryGetValue(node, out var result)) {
                info = (T) result;
                return true;
            }

            info = null;
            return false;
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
