using System;
using System.IO;
using System.Linq;
using GeminiLab.Core2.Markup.Json;

namespace GeminiLab.Glos.Serialization {
    public static class GlosUnitJsonSerializer {
        public static JsonObject ToJson(IGlosUnit unit) => new JsonObject() {
            ["entry"] = unit.Entry,
            ["strings"] = new JsonArray(unit.StringTable.Select(s => new JsonString(s))),
            ["functions"] = new JsonArray(unit.FunctionTable.Select(f => new JsonObject() {
                ["name"] = f.Name,
                ["var_in_ctx"] = new JsonArray(f.VariableInContext.Select(s => new JsonString(s))),
                ["loc_size"] = f.LocalVariableSize,
                ["code"] = Convert.ToBase64String(f.Code),
            }))
        };
        
        /*
        public static JsonObject ToJson2(IGlosUnit unit) {
            var rv = new JsonObject();
            rv["entry"] = unit.Entry;
            
            var strings = new List<JsonValue>();
            foreach (var s in unit.StringTable) {
                strings.Add(s);
            }
            rv["strings"] = new JsonArray(strings);
            
            var functions = new List<JsonValue>();
            foreach (var f in unit.FunctionTable) {
                var fobj = new JsonObject();
                fobj["name"] = f.Name;
                var vic = new List<JsonValue>();
                foreach (var v in f.VariableInContext) vic.Add(v);
                fobj["var_in_ctx"] = new JsonArray(vic);
                fobj["loc_size"] = f.LocalVariableSize;
                fobj["code"] = Convert.ToBase64String(f.Code);
                functions.Add(fobj);
            }
            rv["functions"] = new JsonArray(functions);
            
            return rv;
        }
        */
        
        public static void Serialize(IGlosUnit unit, TextWriter writer) {
            
        }
        
        
    }
}
