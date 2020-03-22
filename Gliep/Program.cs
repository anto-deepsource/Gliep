using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using GeminiLab.Core2;
using GeminiLab.Glos.ViMa;
using GeminiLab.Glug;

namespace GeminiLab.Gliep {
    public static class Program {
        public static void Main(string[] args) {
            var vm = new GlosViMa();
            var unit2Location = new Dictionary<GlosUnit, string>();
            var location2Unit = new Dictionary<string, GlosUnit>();
            var requireCache = new Dictionary<string, GlosValue[]>();

            GlosUnit unit;
            if (args.Length <= 0 || args[0] == "-") {
                unit = TypicalCompiler.Compile(Console.In);
            } else if (args[0] == "-c") {
                unit = TypicalCompiler.Compile(new StringReader(args.Skip(1).JoinBy("\n")));
            } else {
                var entryLoc = new FileInfo(args[0]).FullName;
                using var input = new StreamReader(new FileStream(entryLoc, FileMode.Open, FileAccess.Read));
                unit2Location[unit = location2Unit[entryLoc] = TypicalCompiler.Compile(input)] = entryLoc;
            }
            
            var global = new GlosContext(null!);
            global.CreateVariable("print", GlosValue.NewExternalFunction(param => {
                Console.WriteLine(param.Select(x => vm.Calculator.Stringify(x)).JoinBy(" "));

                return Array.Empty<GlosValue>();
            }));
            global.CreateVariable("debug", GlosValue.NewExternalFunction(param => {
                Console.WriteLine(param.Select(x => GlosValue.Calculator.DebugStringify(x)).JoinBy(" "));

                return Array.Empty<GlosValue>();
            }));
            global.CreateVariable("format", GlosValue.NewExternalFunction(param => {
                if (param.Length <= 0) return new GlosValue[] { "" };

                var format = param[0].AssertString();
                var args = param[1..].Select(x => x.Type switch {
                    GlosValueType.Nil => "nil",
                    GlosValueType.Integer => x.AssumeInteger(),
                    GlosValueType.Float => x.AssumeFloat(),
                    GlosValueType.Boolean => x.AssumeBoolean(),
                    _ => x.ValueObject,
                }).ToArray();

                return new GlosValue[] { string.Format(format, args: args) };
            }));
            global.CreateVariable("require", GlosValue.NewExternalFunction(param => {
                var callerUnit = vm.CallStackFrames[^1].Function.Prototype.Unit;
                var callerLoc = unit2Location.TryGetValue(callerUnit, out var cl) ? cl : ".";

                var required = param[0].AssertString();

                var target = Path.Join(new FileInfo(callerLoc).Directory?.FullName, required);

                if (requireCache.TryGetValue(target, out var cached)) return cached;

                using var targetFS = new StreamReader(new FileStream(target, FileMode.Open, FileAccess.Read));
                var newUnit = TypicalCompiler.Compile(targetFS);

                unit2Location[newUnit] = target;
                location2Unit[target] = newUnit; 

                return requireCache[target] = vm.ExecuteUnit(newUnit, Array.Empty<GlosValue>(), global);
            }));
            global.CreateVariable("__built_in_sqrt", GlosValue.NewExternalFunction(param => {
                return new GlosValue[] { Math.Sqrt(param[0].Type == GlosValueType.Integer ? param[0].AssumeInteger() : param[0].AssumeFloat()) };
            }));

            try {
                vm.ExecuteUnit(unit, Array.Empty<GlosValue>(), global);
            } catch (Exception ex) {
                Console.WriteLine($@"{ex.GetType().Name}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}