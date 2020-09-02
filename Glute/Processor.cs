using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using GeminiLab.Core2.IO;
using GeminiLab.Core2.Logger;
using GeminiLab.Gliep;
using GeminiLab.Glos;
using GeminiLab.Glug.AST;
using GeminiLab.Glute.Compile;

namespace GeminiLab.Glute {
    public class Processor {
        public Processor(IFileSystem fs, Logger logger) {
            FileSystem = fs;
            Logger = logger;
        }

        public IFileSystem FileSystem { get; }
        public Logger Logger { get; }

        private GlosViMa _viMa = new GlosViMa();

        protected string TemplateNameToOutputName(string templateName) {
            return FileSystem.Path.Combine(FileSystem.Path.GetDirectoryName(templateName), FileSystem.Path.GetFileNameWithoutExtension(templateName));
        }

        protected virtual void ProcessText(TextReader input, TextWriter output, GlosContext thisContext) {
            _viMa.SetSyscall(0,
                             (stack, callStack, delStack) => {
                                 var v = stack.PopStack();
                                 output.Write(_viMa.Calculator.Stringify(v));
                             });

            var unit = GlutePostProcess.PostProcessAndCodeGen(new GluteParser(new GluteTokenizer(input)).Parse());
            _viMa.ExecuteUnitWithProvidedContextForRootFunction(unit, thisContext);
        }

        protected virtual void ProcessFile(IFileInfo f, GlosContext dirContext, bool isDirFile = false) {
            Logger.Debug($"Processing {f.FullName}.");

            var outputPath = TemplateNameToOutputName(f.FullName);
            var of = FileSystem.FileInfo.FromFileName(outputPath);

            using var input = f.OpenText();
            using var output = isDirFile ? TextWriter.Null : new StreamWriter(FileSystem.File.OpenWrite(outputPath), new UTF8Encoding(false));

            var thisCtx = isDirFile ? dirContext : new GlosContext(dirContext);

            if (!isDirFile) {
                thisCtx.CreateVariable("output_path", of.FullName);
                thisCtx.CreateVariable("output_name", of.Name);
                thisCtx.CreateVariable("input_path", f.FullName);
                thisCtx.CreateVariable("input_name", f.Name);
            }

            ProcessText(input, output, thisCtx);

            Logger.Debug($"Processed {f.FullName}.");
        }

        protected virtual void ProcessDirectory(IDirectoryInfo d, GlosContext parentContext) {
            Logger.Debug($"Enter {d.FullName}.");

            try {
                if (!d.Exists) {
                    Logger.Error($"Directory {d.FullName} does not exist.");
                    return;
                }

                var thisCtx = new GlosContext(parentContext);
                thisCtx.CreateVariable("directory", d.FullName);

                var dirFile = FileSystem.Path.Combine(d.FullName, ".glute");
                if (FileSystem.File.Exists(dirFile)) {
                    ProcessFile(FileSystem.FileInfo.FromFileName(dirFile), thisCtx, true);
                }

                foreach (var file in d.EnumerateFiles("*.glute", SearchOption.TopDirectoryOnly)) {
                    if (file.Name != ".glute") ProcessFile(file, thisCtx);
                }

                foreach (var dir in d.EnumerateDirectories()) {
                    ProcessDirectory(dir, thisCtx);
                }
            } catch (IOException ex) {
                Logger.Error($"{ex.GetType().FullName}: {ex.Message}");
            }

            Logger.Debug($"Exit {d.FullName}.");
        }

        public void ProcessDirectory(string path) => ProcessDirectory(FileSystem.DirectoryInfo.FromDirectoryName(path), GetRootContext());

        protected virtual GlosContext GetRootContext() {
            var rv = new GlosContext(null);

            rv.CreateVariable("create_guid", GlosValue.NewExternalFunction(param => { return new GlosValue[] { Guid.NewGuid().ToString().ToUpper() }; }));
            rv.CreateVariable("format",
                              GlosValue.NewExternalFunction(param => {
                                  if (param.Length <= 0) return new GlosValue[] { "" };

                                  var format = param[0].AssertString();
                                  var args = param[1..]
                                             .Select(x => x.Type switch {
                                                 GlosValueType.Nil     => "nil",
                                                 GlosValueType.Integer => x.AssumeInteger(),
                                                 GlosValueType.Float   => x.AssumeFloat(),
                                                 GlosValueType.Boolean => x.AssumeBoolean(),
                                                 _                     => x.ValueObject,
                                             })
                                             .ToArray();

                                  return new GlosValue[] { string.Format(format, args: args) };
                              }));
            rv.CreateVariable("now",
                              GlosValue.NewExternalFunction(param => {
                                  string format = @"yyyy/MM/dd HH:mm:ss";

                                  if (param.Length > 0 && param[0].Type == GlosValueType.String) {
                                      format = param[0].AssumeString();
                                  }

                                  return new GlosValue[] { DateTime.Now.ToString(format) };
                              }));
            GlosBuiltInFunctionGenerator.AddFromInstanceFunctions(new Functions(_viMa), rv);

            return rv;
        }
    }
}
