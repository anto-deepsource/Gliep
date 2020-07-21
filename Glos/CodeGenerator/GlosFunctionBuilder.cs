using System;
using System.Collections.Generic;

namespace GeminiLab.Glos.CodeGenerator {
    public class GlosFunctionBuilder {
        internal GlosFunctionBuilder(GlosUnitBuilder parent, int id) {
            Parent = parent;
            Id = id;
            Name = $"function#{id}";
        }

        public GlosUnitBuilder Parent { get; }

        public int Id { get; }

        // TODO: replace temporary workaround
        public IReadOnlyCollection<string> VariableInContext { get; set; } = Array.Empty<string>();

        public string Name { get; set; }

        #region instruction buffer
        private class Instruction {
            public GlosOp OpCode;
            public long Immediate;
            public Label? Target;
            public int Offset;

            public void Deconstruct(out GlosOp opCode, out long immediate, out Label? target, out int offset) {
                opCode = OpCode;
                immediate = Immediate;
                target = Target;
                offset = Offset;
            }
        }
        private readonly List<Instruction> _instructions = new List<Instruction>(); 
        #endregion

        #region label
        private readonly Dictionary<Label, List<int>> _labels = new Dictionary<Label, List<int>>();

        public Label AllocateLabel() {
            var label = new Label(this);
            _labels[label] = new List<int>();
            return label;
        }

        public Label AllocateAndInsertLabel() {
            var label = AllocateLabel();
            InsertLabel(label);
            return label;
        }


        public void InsertLabel(Label label) {
            if (label.Builder != this) throw new InvalidOperationException();
            label.TargetCounter = _instructions.Count;
        }
        #endregion

        #region local variable
        public int LocalVariableCount { get; private set; } = 0;

        public LocalVariable AllocateLocalVariable() => new LocalVariable(this, LocalVariableCount++);
        #endregion

        #region instruction appender
        public void AppendInstruction(GlosOp opCode, long immediate = 0, Label? target = null) =>
            _instructions.Add(new Instruction { OpCode = opCode, Immediate = immediate, Target = target!, Offset = -1 });

        // arithmetic, bitwise, comparison, table and unary operators
        public void AppendAdd() => AppendInstruction(GlosOp.Add);
        public void AppendSub() => AppendInstruction(GlosOp.Sub);
        public void AppendMul() => AppendInstruction(GlosOp.Mul);
        public void AppendDiv() => AppendInstruction(GlosOp.Div);
        public void AppendMod() => AppendInstruction(GlosOp.Mod);
        public void AppendLsh() => AppendInstruction(GlosOp.Lsh);
        public void AppendRsh() => AppendInstruction(GlosOp.Rsh);
        public void AppendAnd() => AppendInstruction(GlosOp.And);
        public void AppendOrr() => AppendInstruction(GlosOp.Orr);
        public void AppendXor() => AppendInstruction(GlosOp.Xor);
        public void AppendGtr() => AppendInstruction(GlosOp.Gtr);
        public void AppendLss() => AppendInstruction(GlosOp.Lss);
        public void AppendGeq() => AppendInstruction(GlosOp.Geq);
        public void AppendLeq() => AppendInstruction(GlosOp.Leq);
        public void AppendEqu() => AppendInstruction(GlosOp.Equ);
        public void AppendNeq() => AppendInstruction(GlosOp.Neq);
        public void AppendSmt() => AppendInstruction(GlosOp.Smt);
        public void AppendGmt() => AppendInstruction(GlosOp.Gmt);
        public void AppendRen() => AppendInstruction(GlosOp.Ren);
        public void AppendUen() => AppendInstruction(GlosOp.Uen);
        public void AppendRenL() => AppendInstruction(GlosOp.RenL);
        public void AppendUenL() => AppendInstruction(GlosOp.UenL);
        public void AppendIen() => AppendInstruction(GlosOp.Ien);
        public void AppendPshv() => AppendInstruction(GlosOp.Pshv);
        public void AppendPopv() => AppendInstruction(GlosOp.Popv);
        public void AppendIniv() => AppendInstruction(GlosOp.Iniv);
        public void AppendNot() => AppendInstruction(GlosOp.Not);
        public void AppendNeg() => AppendInstruction(GlosOp.Neg);
        public void AppendTypeof() => AppendInstruction(GlosOp.Typeof);
        public void AppendIsNil() => AppendInstruction(GlosOp.IsNil);

        // 
        public void AppendRvc() => AppendInstruction(GlosOp.Rvc);
        public void AppendUvc() => AppendInstruction(GlosOp.Uvc);
        public void AppendRvg() => AppendInstruction(GlosOp.Rvg);
        public void AppendUvg() => AppendInstruction(GlosOp.Uvg);

        // load without immediate
        public void AppendLdNeg1() => AppendInstruction(GlosOp.LdNeg1);
        public void AppendLdNTbl() => AppendInstruction(GlosOp.LdNTbl);
        public void AppendLdNil() => AppendInstruction(GlosOp.LdNil);
        public void AppendLdNVec() => AppendInstruction(GlosOp.LdNVec);
        public void AppendLdTrue() => AppendInstruction(GlosOp.LdTrue);
        public void AppendLdFalse() => AppendInstruction(GlosOp.LdFalse);

        public void AppendLdBool(bool value) => AppendInstruction(value ? GlosOp.LdTrue : GlosOp.LdFalse);

        // load (integer)
        public void AppendLd(long imm) => AppendInstruction(GlosOp.Ld, immediate: imm);
        public void AppendLd(ulong imm) => AppendInstruction(GlosOp.Ld, immediate: unchecked((long)imm));

        // load (float)
        public unsafe void AppendLdFlt(double value) => AppendInstruction(GlosOp.LdFlt, immediate: *(long*)&value);
        public void AppendLdFltRaw(ulong value) => AppendInstruction(GlosOp.LdFlt, immediate: unchecked((long)value));

        // load string and function
        public void AppendLdStr(int id) => AppendInstruction(GlosOp.LdStr, immediate: id);
        public void AppendLdStr(string str) => AppendLdStr(Parent.AddOrGetString(str));

        public void AppendLdFun(int id) => AppendInstruction(GlosOp.LdFun, immediate: id);
        public void AppendLdFun(GlosFunctionBuilder fun) => AppendLdFun(fun.Id);

        private long getIdFromLoc(LocalVariable loc) {
            if (loc.Builder != this) throw new ArgumentOutOfRangeException(nameof(loc));
            return loc.LocalVariableId;
        }

        // load/store local variables
        public void AppendLdLoc(LocalVariable loc) => AppendInstruction(GlosOp.LdLoc, immediate: getIdFromLoc(loc));
        public void AppendStLoc(LocalVariable loc) => AppendInstruction(GlosOp.StLoc, immediate: getIdFromLoc(loc));

        // load argument/argc
        public void AppendLdArg(int imm) => AppendInstruction(GlosOp.LdArg, immediate: imm);
        public void AppendLdArgc() => AppendInstruction(GlosOp.LdArgc);

        public void AppendLdArgA() => AppendInstruction(GlosOp.LdArgA);
        public void AppendLdLocA() => AppendInstruction(GlosOp.LdLocA);
        public void AppendStLocA() => AppendInstruction(GlosOp.StLocA);

        // branch
        public void AppendB(Label target) => AppendInstruction(GlosOp.B, target: target);
        public void AppendBf(Label target) => AppendInstruction(GlosOp.Bf, target: target);
        public void AppendBt(Label target) => AppendInstruction(GlosOp.Bt, target: target);
        public void AppendBn(Label target) => AppendInstruction(GlosOp.Bn, target: target);
        public void AppendBnn(Label target) => AppendInstruction(GlosOp.Bnn, target: target);

        // stack auxiliary
        public void AppendDup() => AppendInstruction(GlosOp.Dup);
        public void AppendPop() => AppendInstruction(GlosOp.Pop);
        public void AppendLdDel() => AppendInstruction(GlosOp.LdDel);
        public void AppendCall() => AppendInstruction(GlosOp.Call);
        public void AppendRet() => AppendInstruction(GlosOp.Ret);
        public void AppendBind() => AppendInstruction(GlosOp.Bind);
        public void AppendPopDel() => AppendInstruction(GlosOp.PopDel);
        public void AppendDupList() => AppendInstruction(GlosOp.DupList);
        public void AppendPkv() => AppendInstruction(GlosOp.Pkv);
        public void AppendUpv() => AppendInstruction(GlosOp.Upv);

        // shape return values
        public void AppendShpRv(int imm) => AppendInstruction(GlosOp.ShpRv, immediate: imm);

        // nop
        public void AppendNop() => AppendInstruction(GlosOp.Nop);

        // syscall
        public void AppendSyscall(int imm) => AppendInstruction(GlosOp.SysC0, immediate: imm);

        public void AppendRetIfNone() {
            if (_instructions[^1].OpCode != GlosOp.Ret) AppendRet();
        }
        #endregion

        public void SetEntry() => Parent.Entry = Id;

        // TOD O: revoke template public status 
        internal byte[] GetOpArray() {
            var buff = new List<byte>(_instructions.Count);

            foreach (var instr in _instructions) {
                var (op, imm, target, _) = instr;
                instr.Offset = buff.Count;
                if (GlosOpInfo.Immediates[(byte)op] == GlosOpImmediate.None || GlosOpInfo.Immediates[(byte)op] == GlosOpImmediate.OnStack) {
                    buff.Add((byte)op);
                    continue;
                }

                unchecked {
                    if (op == GlosOp.LdStr || op == GlosOp.LdFun) {
                        if (imm <= sbyte.MaxValue) {
                            buff.AddOp(op + 4);
                            buff.Add((byte)(sbyte)imm);
                        } else {
                            buff.AddOp(op);
                            buff.AddInteger32((int)imm);
                        }
                    } else if (op == GlosOp.Ld) {
                        if (imm == -1) {
                            buff.AddOp(GlosOp.LdNeg1);
                        } else if (imm >= 0 && imm < 4) {
                            buff.AddOp(GlosOp.Ld0 + (byte)imm);
                        } else if (imm >= sbyte.MinValue && imm <= sbyte.MaxValue) {
                            buff.AddOp(GlosOp.LdS);
                            buff.Add((byte)(sbyte)imm);
                        } else if (imm >= int.MinValue && imm <= int.MaxValue) {
                            buff.AddOp(GlosOp.Ld);
                            buff.AddInteger32((int)imm);
                        } else {
                            buff.AddOp(GlosOp.LdQ);
                            buff.AddInteger64(imm);
                        }
                    } else if (op == GlosOp.LdFlt) {
                        buff.AddOp(GlosOp.LdFlt);
                        buff.AddInteger64(imm);
                    } else if (op == GlosOp.LdLoc) {
                        if (imm >= 0 && imm < 8) {
                            buff.AddOp(GlosOp.LdLoc0 + (byte)imm);
                        } else if (imm >= sbyte.MinValue && imm <= sbyte.MaxValue) {
                            buff.AddOp(GlosOp.LdLocS);
                            buff.Add((byte)(sbyte)imm);
                        } else {
                            buff.AddOp(GlosOp.LdLoc);
                            buff.AddInteger32((int)imm);
                        }
                    } else if (op == GlosOp.StLoc) {
                        if (imm >= 0 && imm < 8) {
                            buff.AddOp(GlosOp.StLoc0 + (byte)imm);
                        } else if (imm >= sbyte.MinValue && imm <= sbyte.MaxValue) {
                            buff.AddOp(GlosOp.StLocS);
                            buff.Add((byte)(sbyte)imm);
                        } else {
                            buff.AddOp(GlosOp.StLoc);
                            buff.AddInteger32((int)imm);
                        }
                    } else if (op == GlosOp.LdArg) {
                        if (imm >= 0 && imm < 4) {
                            buff.AddOp(GlosOp.LdArg0 + (byte)imm);
                        } else if (imm >= sbyte.MinValue && imm <= sbyte.MaxValue) {
                            buff.AddOp(GlosOp.LdArgS);
                            buff.Add((byte)(sbyte)imm);
                        } else {
                            buff.AddOp(GlosOp.LdArg);
                            buff.AddInteger32((int)imm);
                        }
                    } else if (op == GlosOp.ShpRv) {
                        if (imm >= 0 && imm < 4) {
                            buff.AddOp(GlosOp.ShpRv0 + (byte)imm);
                        } else if (imm >= sbyte.MinValue && imm <= sbyte.MaxValue) {
                            buff.AddOp(GlosOp.ShpRvS);
                            buff.Add((byte)(sbyte)imm);
                        } else {
                            buff.AddOp(GlosOp.ShpRv);
                            buff.AddInteger32((int)imm);
                        }
                    } else if (op == GlosOp.SysC0) {
                        buff.AddOp(GlosOp.SysC0 + (byte)(0x07 & imm));
                    } else if (GlosOpInfo.Categories[(byte)op] == GlosOpCategory.Branch) {
                        buff.AddOp(op);
                        _labels[target!].Add(buff.Count);
                        buff.AddInteger32(int.MaxValue);
                    } else {
                        // throw new UnknownOpcodeException();
                    }
                }
            }

            foreach (var (label, refs) in _labels) {
                var offset = _instructions[label.TargetCounter].Offset;

                foreach (var r in refs) {
                    buff.SetInteger32(r, offset - r - 4);
                }
            }

            return buff.ToArray();
        }
    }
}
