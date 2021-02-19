using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using GeminiLab.Glos;

namespace GeminiLab.Gliep {
    public class GlosBuiltInPureFunctionAttribute : Attribute {
        public GlosBuiltInPureFunctionAttribute(string name) {
            Name = name;
        }

        public string Name { get; }
    }
    
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
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            
            var builtinFns = new List<(MethodInfo methodInfo, bool pure)>();

            foreach (var m in methods) {
                if (m.ReturnType == glosValueArrayType) {
                    if (m.GetCustomAttribute<GlosBuiltInPureFunctionAttribute>() != null) {
                        builtinFns.Add((m, true));
                    } else if (m.GetCustomAttribute<GlosBuiltInFunctionAttribute>() != null && m.GetParameters() is { Length: var len } param && len > 0 && param[0].ParameterType == typeof(GlosCoroutine)) {
                        builtinFns.Add((m, false));
                    }
                }
            }

            foreach (var (m, pure) in builtinFns) {
                string name;

                if (pure) {
                    name = m.GetCustomAttribute<GlosBuiltInPureFunctionAttribute>()!.Name;
                } else {
                    name = m.GetCustomAttribute<GlosBuiltInFunctionAttribute>()!.Name;
                }

                var param = m.GetParameters();
                var paramLen = param.Length;
                var hasParamArray = param[^1].IsDefined(typeof(ParamArrayAttribute), false);

                // todo: maybe build these functions dynamically by reflection to speed it up?
                if (pure) {
                    ctx.CreateVariable(name, (GlosPureEFunction)(p => {
                        var args = new List<object>();
                        parseArgs(true, param, p, args);
                        return (GlosValue[])m.Invoke(instance, args.ToArray())!;
                    }));
                } else {
                    ctx.CreateVariable(name, (GlosEFunction)((cor, p) => {
                        var args = new List<object> { cor };
                        parseArgs(false, param, p, args);
                        return (GlosValue[])m.Invoke(instance, args.ToArray())!;
                    }));
                }
            }

            return ctx;

#if !DEVELOP && CS9
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
            void parseArgs(bool pure, ParameterInfo[] paramList, GlosValue[] args, List<object> dest) {
                if (!pure) paramList = paramList[1..];
                
                var paramLen = paramList.Length;
                var hasParamArray = paramLen > 0 && paramList[^1].IsDefined(typeof(ParamArrayAttribute), false);

                var argLen = args.Length;

                for (int i = 0; i < paramLen; ++i) {
                    if (i == paramLen - 1 && hasParamArray) {
                        // only params GlosValue[] supported now
                        dest.Add(i >= argLen ? Array.Empty<GlosValue>() : args[i..]);
                    } else if (i >= argLen) {
                        // if it's wrong, let it throw
                        dest.Add(null!);
                    } else {
                        ref var pi = ref args[i];
                        var paramType = paramList[i].ParameterType;
                            
                        if (paramType == typeof(GlosValue)) {
                            dest.Add(pi);
                        } else if (paramType == typeof(int)) {
                            dest.Add((int)pi.AssertInteger());
                        } else if (paramType == typeof(long)) {
                            dest.Add(pi.AssertInteger());
                        } else if (paramType == typeof(double)) {
                            dest.Add(pi.ToFloat());
                        } else if (paramType == typeof(string)) {
                            dest.Add(pi.AssertString());
                        } else if (paramType == typeof(GlosVector)) {
                            dest.Add(pi.AssertVector());
                        }
                    }
                }
            }
        }
    }
}
