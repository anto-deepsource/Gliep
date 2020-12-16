using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GeminiLab.Glos;

namespace GeminiLab.Gliep {
    public class GlosBuiltInFunctionAttribute : Attribute {
        public GlosBuiltInFunctionAttribute(string name) {
            Name = name;
        }

        public string Name { get; }
    }

    public static class GlosBuiltInFunctionGenerator {
        public static GlosContext AddFromInstanceFunctions<T>(T instance, GlosContext ctx) {
            var type = typeof(T);
            var glosValueArrayType = typeof(GlosValue).MakeArrayType();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(m => m.ReturnType == glosValueArrayType && m.GetCustomAttribute<GlosBuiltInFunctionAttribute>() != null);

            foreach (var m in methods) {
                var attr = m.GetCustomAttribute<GlosBuiltInFunctionAttribute>()!;
                var name = attr.Name;

                var param = m.GetParameters();
                var paramLen = param.Length;
                var hasParamArray = param[^1].IsDefined(typeof(ParamArrayAttribute), false);

                // todo: maybe build this function dynamically by reflection to speed it up?
                ctx.CreateVariable(name, (GlosExternalPureFunction)(p => {
                    var len = p.Length;
                    var args = new List<object>();

                    for (int i = 0; i < paramLen; ++i) {
                        if (i == paramLen - 1 && hasParamArray) {
                            // only params GlosValue[] supported now
                            args.Add(i >= len ? Array.Empty<GlosValue>() : p[i..]);
                        } else if (i >= len) {
                            args.Add(null!);
                        } else {
                            ref var pi = ref p[i];
                            var paramType = param[i].ParameterType;
                            
                            if (paramType == typeof(GlosValue)) {
                                args.Add(pi);
                            } else if (paramType == typeof(int)) {
                                args.Add(unchecked((int)pi.AssertInteger()));
                            } else if (paramType == typeof(long)) {
                                args.Add(pi.AssertInteger());
                            } else if (paramType == typeof(double)) {
                                args.Add(pi.ToFloat());
                            } else if (paramType == typeof(string)) {
                                args.Add(pi.AssertString());
                            } else if (paramType == typeof(GlosVector)) {
                                args.Add(pi.AssertVector());
                            }
                        }
                    }

                    return (GlosValue[])m.Invoke(instance, args.ToArray())!;
                }));
            }

            return ctx;
        }
    }
}
