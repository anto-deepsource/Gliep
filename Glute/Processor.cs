using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using GeminiLab.Core2.IO;
using GeminiLab.Core2.Logger;
using GeminiLab.Glos;
using GeminiLab.Glug.AST;
using GeminiLab.Glute.Compile;

namespace GeminiLab.Glute {
    public class Processor {
        public Processor(FileSystem fs, Logger logger) {
            FileSystem = fs;
            Logger = logger;
        }

        public FileSystem FileSystem { get; }
        public Logger Logger { get; }

        private GlosViMa _viMa = new GlosViMa();

        protected string TemplateNameToOutputName(string templateName) {
            return FileSystem.Path.Combine(FileSystem.Path.GetDirectoryName(templateName), FileSystem.Path.GetFileNameWithoutExtension(templateName));
        }

        protected virtual void ProcessText(TextReader input, TextWriter output, GlosContext thisContext) {
            _viMa.SetSyscall(0, (stack, callStack, delStack) => {
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

            thisCtx.CreateVariable("directory", f.DirectoryName);

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

                var dirFile = FileSystem.Path.Combine(d.FullName, ".glute");
                if (FileSystem.File.Exists(dirFile)) {
                    ProcessFile(FileSystem.FileInfo.FromFileName(dirFile), thisCtx, true);
                }

                foreach (var file in d.EnumerateFiles("*.glute", SearchOption.TopDirectoryOnly)) {
                    if (file.Name != ".glute") ProcessFile(file, thisCtx);
                }
            } catch (IOException ex) {
                Logger.Error($"{ex.GetType().FullName}: {ex.Message}");
            }

            Logger.Debug($"Exit {d.FullName}.");
        }

        public void ProcessDirectory(string path) => ProcessDirectory(FileSystem.DirectoryInfo.FromDirectoryName(path), new GlosContext(null));
    }
}
