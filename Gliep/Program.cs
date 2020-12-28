using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
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

        [GlosBuiltInPureFunction("print")]
        public GlosValue[] Print(params GlosValue[] values) {
            /*
            Console.WriteLine(values.Select(x => _vm.Calculator.Stringify(x)).JoinBy(" "));

            return Array.Empty<GlosValue>();
            */
            return Debug(values);
        }

        [GlosBuiltInPureFunction("debug")]
        public GlosValue[] Debug(params GlosValue[] values) {
            Console.WriteLine(values.Select(x => GlosValue.Calculator.DebugStringify(x)).JoinBy(" "));

            return Array.Empty<GlosValue>();
        }

        [GlosBuiltInPureFunction("format")]
        public GlosValue[] Format(string format, params GlosValue[] values) {
            var args = values.Select(x => x.Type switch {
                GlosValueType.Nil     => "nil",
                GlosValueType.Integer => x.AssumeInteger(),
                GlosValueType.Float   => x.AssumeFloat(),
                GlosValueType.Boolean => x.AssumeBoolean(),
                _                     => x.ValueObject,
            }).ToArray();

            return new GlosValue[] { string.Format(format, args: args) };
        }

        [GlosBuiltInPureFunction("iter")]
        public GlosValue[] Iter(GlosVector vec) {
            var len = vec.Count;
            var idx = -1;

            return new GlosValue[] {
                (GlosExternalPureFunction) (p => new[] { ++idx >= len ? GlosValue.NewNil() : vec[idx] })
            };
        }

        [GlosBuiltInPureFunction("__built_in_sqrt")]
        public GlosValue[] Sqrt(double val) {
            return new GlosValue[] {
                Math.Sqrt(val)
            };
        }
    }

    public class UnitManagerWrapper {
        private readonly UnitManager _um;
        private readonly IFileSystem _fs;

        public UnitManagerWrapper(IFileSystem fs, GlosViMa viMa, GlosContext global) {
            _um = new UnitManager(_fs = fs, viMa, global);
        }

        public void AddEntryManually(string entry, string entryLoc, IGlosUnit unit) {
            _um.AddUnit(new LoadedUnit(entry, _fs.FileInfo.FromFileName(entryLoc), unit, GlosValue.NewNil()));
        }
        
        [GlosBuiltInFunction("require")]
        public GlosValue[] Require(GlosCoroutine cor, string key) {
            return new[] { _um.Load(key, cor).Result };
        }
    }

    public static class Program {
        public static string StringifyGlosCallStackFrame(GlosStackFrame frame) {
            var ip = frame.InstructionPointer;
            var fun = frame.Function.Prototype;
            return $"   at {frame.Function.Prototype.Name}";
        }

        public static void Main(string[] args) {
            var vm = new GlosViMa();
            // vm.WorkingDirectory = Environment.CurrentDirectory;
            var entryLoc = "";

            IGlosUnit unit;
            if (args.Length <= 0 || args[0] == "-") {
                unit = TypicalCompiler.Compile(Console.In, @"<stdin>");
                entryLoc = "./.1";
            } else if (args[0] == "-c") {
                unit = TypicalCompiler.Compile(new StringReader(args.Skip(1).JoinBy("\n")), @"<commandline>");
                entryLoc = "./.1";
            } else {
                entryLoc = new FileInfo(args[0]).FullName;
                using var input = new StreamReader(new FileStream(entryLoc, FileMode.Open, FileAccess.Read));
                unit = TypicalCompiler.Compile(input, entryLoc);
            }
            
            var global = new GlosContext(null!);
            GlosBuiltInFunctionGenerator.AddFromInstanceFunctions(new Functions(vm), global);
            
            var umw = new UnitManagerWrapper(new FileSystem(), vm, global);
            umw.AddEntryManually("entry", entryLoc, unit);
            
            GlosBuiltInFunctionGenerator.AddFromInstanceFunctions(umw, global);

            try {
                vm.ExecuteUnit(unit, Array.Empty<GlosValue>(), global);
            } catch (GlosRuntimeException rex) when (rex.InnerException is GlosException gex) {
                Console.WriteLine($@"RE: {gex.GetType().Name}: {gex.Message}");
                Console.WriteLine(@"Host stacktrace:");
                Console.WriteLine(gex.StackTrace);
                Console.WriteLine(@"Stacktrace:");
                rex.CallStack.Reverse().Select(StringifyGlosCallStackFrame).ForEach(Console.WriteLine);
            } catch (Exception ex) {
                Console.WriteLine($@"{ex.GetType().Name}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
