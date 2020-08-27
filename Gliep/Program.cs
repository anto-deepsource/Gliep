using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GeminiLab.Core2;
using GeminiLab.Core2.Collections;
using GeminiLab.Glos;
using GeminiLab.Glug;

namespace GeminiLab.Gliep {
    public class Functions {
        private readonly GlosViMa _vm;
        public Functions(GlosViMa vm) {
            _vm = vm;
        }

        [GlosBuiltInFunction("print")]
        public GlosValue[] Print(params GlosValue[] values) {
            Console.WriteLine(values.Select(x => _vm.Calculator.Stringify(x)).JoinBy(" "));

            return Array.Empty<GlosValue>();
        }
        
        [GlosBuiltInFunction("debug")]
        public GlosValue[] Debug(params GlosValue[] values) {
            Console.WriteLine(values.Select(x => GlosValue.Calculator.DebugStringify(x)).JoinBy(" "));

            return Array.Empty<GlosValue>();
        }

        [GlosBuiltInFunction("format")]
        public GlosValue[] Format(string format, params GlosValue[] values) {
            var args = values.Select(x => x.Type switch {
                GlosValueType.Nil => "nil",
                GlosValueType.Integer => x.AssumeInteger(),
                GlosValueType.Float => x.AssumeFloat(),
                GlosValueType.Boolean => x.AssumeBoolean(),
                _ => x.ValueObject,
            }).ToArray();

            return new GlosValue[] { string.Format(format, args: args) };
        }

        [GlosBuiltInFunction("iter")]
        public GlosValue[] Iter(GlosVector vec) {
            var len = vec.Count;
            var idx = -1;

            return new GlosValue[] {
                (GlosExternalFunction)(p => new[] { ++idx >= len ? GlosValue.NewNil() : vec[idx] })
            };
        }

        [GlosBuiltInFunction("__built_in_sqrt")]
        public GlosValue[] Sqrt(double val) {
            return new GlosValue[] {
                Math.Sqrt(val)
            };
        }
    }

    public static class Program {
        public static void AddBuiltInFunctions(GlosContext ctx) { }

        public static void Main(string[] args) {
            var vm = new GlosViMa();
            var unit2Location = new Dictionary<GlosUnit, string>();
            var location2Unit = new Dictionary<string, GlosUnit>();
            var requireCache = new Dictionary<string, GlosValue[]>();

            GlosUnit unit;
            if (args.Length <= 0 || args[0] == "-") {
                unit = TypicalCompiler.Compile(Console.In, @"<stdin>");
            } else if (args[0] == "-c") {
                unit = TypicalCompiler.Compile(new StringReader(args.Skip(1).JoinBy("\n")), @"<commandline>");
            } else {
                var entryLoc = new FileInfo(args[0]).FullName;
                using var input = new StreamReader(new FileStream(entryLoc, FileMode.Open, FileAccess.Read));
                unit2Location[unit = location2Unit[entryLoc] = TypicalCompiler.Compile(input, entryLoc)] = entryLoc;
            }

            var global = new GlosContext(null!);
            GlosBuiltInFunctionGenerator.AddFromInstanceFunctions(new Functions(vm), global);
            global.CreateVariable("require", GlosValue.NewExternalFunction(param => {
                var callerUnit = vm.CallStackFrames[^1].Function.Prototype.Unit;
                var callerLoc = unit2Location.TryGetValue(callerUnit, out var cl) ? cl : "./.pseudo";

                var required = param[0].AssertString();

                var target = Path.Join(new FileInfo(callerLoc).Directory?.FullName, required);

                if (requireCache.TryGetValue(target, out var cached)) return cached;

                using var targetFS = new StreamReader(new FileStream(target, FileMode.Open, FileAccess.Read));
                var newUnit = TypicalCompiler.Compile(targetFS, target);

                unit2Location[newUnit] = target;
                location2Unit[target] = newUnit;

                return requireCache[target] = vm.ExecuteUnit(newUnit, Array.Empty<GlosValue>(), global);
            }));

            try {
                vm.ExecuteUnit(unit, Array.Empty<GlosValue>(), global);
            } catch (GlosRuntimeException rex) when (rex.InnerException is GlosException gex) {
                Console.WriteLine($@"RE: {gex.GetType().Name}: {gex.Message}");
                Console.WriteLine(@"Host stacktrace:");
                Console.WriteLine(gex.StackTrace);
                Console.WriteLine(@"Stacktrace:");
                rex.CallStack.Reverse().Select(frame => $"   at {frame.Function.Prototype.Name}").ForEach(Console.WriteLine);
            } catch (Exception ex) {
                Console.WriteLine($@"{ex.GetType().Name}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}