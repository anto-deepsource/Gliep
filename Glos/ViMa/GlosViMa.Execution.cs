using System;

namespace GeminiLab.Glos.ViMa {
    public partial class GlosViMa {
        public static bool ReadInstructionAndImmediate(ReadOnlySpan<byte> code, ref int ip, out GlosOp op, out long imm, out bool immOnStack) {
            var len = code.Length;

            immOnStack = false;
            imm = 0;

            if (ip == len) {
                op = GlosOp.Ret;
                return true;
            }

            var opb = code[ip++];
            op = (GlosOp)opb;
            var immt = GlosOpInfo.Immediates[opb];

            unchecked {
                switch (immt) {
                    case GlosOpImmediate.Embedded:
                        imm = opb & 0x07;
                        break;
                    case GlosOpImmediate.Byte:
                        if (ip + 1 > len) return false;
                        imm = (sbyte)code[ip++];
                        break;
                    case GlosOpImmediate.Dword:
                        if (ip + 4 > len) return false;
                        imm = unchecked((int)(uint)((ulong)code[ip] | ((ulong)code[ip + 1] << 8) | ((ulong)code[ip + 2] << 16) | ((ulong)code[ip + 3] << 24)));
                        ip += 4;
                        break;
                    case GlosOpImmediate.Qword:
                        if (ip + 8 > len) return false;
                        imm = unchecked((long)((ulong)code[ip] | ((ulong)code[ip + 1] << 8) | ((ulong)code[ip + 2] << 16) | ((ulong)code[ip + 3] << 24) | ((ulong)code[ip + 4] << 32) | ((ulong)code[ip + 5] << 40) | ((ulong)code[ip + 6] << 48) | ((ulong)code[ip + 7] << 56)));
                        ip += 8;
                        break;
                    case GlosOpImmediate.OnStack:
                        immOnStack = true;
                        break;
                }
            }

            return true;
        }

        private void executeFunctionOnStack(int bptr, GlosFunction function, int argc) {
            var proto = function.Prototype;
            var unit = function.Prototype.Unit;
            var parent = function.ParentContext;
            var ctx = new GlosContext(parent);
            var global = ctx.Root;

            foreach (var s in proto.VariableInContext) ctx.CreateVariable(s);

            var locc = proto.LocalVariableSize;

            ref var frame = ref pushCallStackFrame();

            ref var argb = ref frame.ArgumentsBase;
            ref var locb = ref frame.LocalVariablesBase;
            ref var prib = ref frame.PrivateStackBase;
            ref var ip = ref frame.InstructionPointer;

            argb = bptr;
            locb = argb + argc;
            prib = locb + locc;

            frame.StackBase = bptr;
            frame.Function = function;
            frame.ArgumentsCount = argc;
            frame.DelimiterStackBase = _dptr;

            pushUntil(prib);

            var code = proto.Code;
            var len = code.Length;
            ip = 0;
            while (true) {
                if (ip < 0 || ip > len) throw new GlosInvalidProgramCounterException(this, ip);

                if (!ReadInstructionAndImmediate(code, ref ip, out var op, out long imms, out bool immOnStack)) throw new GlosUnexpectedEndOfCodeException(this);

                if (immOnStack) {
                    imms = stackTop().AssertInteger();
                    popStack();
                }

                // the implicit return at the end of function
                var cat = GlosOpInfo.Categories[(int)op];
                
                // execution
                if (cat == GlosOpCategory.BinaryArithmeticOperator) {
                    binaryArithmeticOperator(op, ref stackTop(1), in stackTop(1), in stackTop());
                    popStack();
                } else if (cat == GlosOpCategory.BinaryBitwiseOperator) {
                    binaryBitwiseOperator(op, ref stackTop(1), in stackTop(1), in stackTop());
                    popStack();
                } else if (cat == GlosOpCategory.BinaryComparisonOperator) {
                    binaryComparisonOperator(op, ref stackTop(1), in stackTop(1), in stackTop());
                    popStack();
                } else if (op == GlosOp.Smt) {
                    stackTop(1).AssertTable().Metatable = stackTop().AssertTable();
                    popStack();
                } else if (op == GlosOp.Gmt) {
                    if (stackTop().AssertTable().Metatable is { } mt) {
                        stackTop().SetTable(mt);
                    } else {
                        stackTop().SetNil();
                    }
                } else if (op == GlosOp.Ren) {
                    if (stackTop(1).AssertTable().TryReadEntry(stackTop(), out var result)) {
                        stackTop(1) = result;
                    } else {
                        stackTop(1).SetNil();
                    }

                    popStack();
                } else if (op == GlosOp.Uen) {
                    stackTop(2).AssertTable().UpdateEntry(stackTop(1), stackTop());

                    popStack(3);
                } else if (op == GlosOp.RenL) {
                    if (stackTop(1).AssertTable().TryReadEntryLocally(stackTop(), out var result)) {
                        stackTop(1) = result;
                    } else {
                        stackTop(1).SetNil();
                    }

                    popStack();
                } else if (op == GlosOp.UenL) {
                    stackTop(2).AssertTable().UpdateEntryLocally(stackTop(1), stackTop());

                    popStack(3);
                } else if (cat == GlosOpCategory.UnaryOperator) {
                    unaryOperator(op, ref stackTop());
                } else if (op == GlosOp.Rvc) {
                    stackTop() = ctx.GetVariableReference(stackTop().AssertString());
                } else if (op == GlosOp.Uvc) {
                    ctx.GetVariableReference(stackTop(1).AssertString()) = stackTop();
                    popStack(2);
                } else if (op == GlosOp.Rvg) {
                    stackTop() = global.GetVariableReference(stackTop().AssertString());
                } else if (op == GlosOp.Uvg) {
                    global.GetVariableReference(stackTop(1).AssertString()) = stackTop();
                    popStack(2);
                } else if (op == GlosOp.UvcR) {
                    ctx.GetVariableReference(stackTop().AssertString()) = stackTop(1);
                    popStack(2);
                } else if (op == GlosOp.UvgR) {
                    global.GetVariableReference(stackTop().AssertString()) = stackTop(1);
                    popStack(2);
                } else if (op == GlosOp.LdFun || op == GlosOp.LdFunS) {
                    if (imms > unit.FunctionTable.Count || imms < 0) throw new ArgumentOutOfRangeException();
                    int index = (int)imms;
                    pushStack().SetFunction(new GlosFunction(unit.FunctionTable[index]!, null!));
                } else if (op == GlosOp.LdStr || op == GlosOp.LdStrS) {
                    if (imms > unit.StringTable.Count || imms < 0) throw new ArgumentOutOfRangeException();
                    int index = (int)imms;
                    pushStack().SetString(unit.StringTable[index]);
                } else if (cat == GlosOpCategory.LoadInteger) {
                    pushStack().SetInteger(op == GlosOp.LdNeg1 ? -1 : imms);
                } else if (op == GlosOp.LdNTbl) {
                    pushStack().SetTable(new GlosTable(this));
                } else if (op == GlosOp.LdNil) {
                    pushNil();
                } else if (op == GlosOp.LdFlt) {
                    pushStack().SetFloatByBinaryPresentation(unchecked((ulong)imms));
                } else if (op == GlosOp.LdTrue) {
                    pushStack().SetBoolean(true);
                } else if (op == GlosOp.LdFalse) {
                    pushStack().SetBoolean(false);
                } else if (cat == GlosOpCategory.LoadLocalVariable) {
                    if (imms >= locc || imms < 0) throw new GlosLocalVariableIndexOutOfRangeException(this, (int)imms);
                    pushStack() = _stack[locb + (int)imms];
                } else if (cat == GlosOpCategory.StoreLocalVariable) {
                    if (imms >= locc || imms < 0) throw new GlosLocalVariableIndexOutOfRangeException(this, (int)imms);
                    _stack[locb + (int)imms] = stackTop();
                    popStack();
                } else if (cat == GlosOpCategory.LoadArgument) {
                    pushNil();
                    if (imms < argc && imms >= 0) stackTop() = _stack[argb + (int)imms];
                } else if (op == GlosOp.LdArgc) {
                    pushStack().SetInteger(argc);
                } else if (cat == GlosOpCategory.Branch) {
                    var dest = ip + (int)imms;
                    var jump = op switch {
                        GlosOp.B => true,
                        GlosOp.BS => true,
                        GlosOp.Bf => stackTop().Falsey(),
                        GlosOp.BfS => stackTop().Falsey(),
                        GlosOp.Bt => stackTop().Truthy(),
                        GlosOp.BtS => stackTop().Truthy(),
                        GlosOp.Bn => stackTop().IsNil(),
                        GlosOp.BnS => stackTop().IsNil(),
                        GlosOp.Bnn => stackTop().IsNonNil(),
                        GlosOp.BnnS => stackTop().IsNonNil(),
                        _ => false,
                    };

                    if (jump) ip = dest;

                    if (op != GlosOp.B && op != GlosOp.BS) popStack();
                } else if (op == GlosOp.Dup) {
                    pushStack(in stackTop());
                } else if (op == GlosOp.Pop) {
                    popStack();
                } else if (op == GlosOp.LdDel) {
                    pushDelimiter();
                } else if (op == GlosOp.Call) {
                    var funv = stackTop();
                    popStack();

                    var ptr = popDelimiter();
                    var nextArgc = _sptr - ptr;

                    if (funv.Type == GlosValueType.Function) {
                        var fun = funv.AssertFunction();
                        executeFunctionOnStack(ptr, fun, nextArgc);
                    } else if (funv.Type == GlosValueType.ExternalFunction) {
                        var fun = funv.AssertExternalFunction();

                        var nextArgs = new GlosValue[nextArgc];
                        _stack.AsSpan(ptr, nextArgc).CopyTo(nextArgs.AsSpan());
                        var nextRets = fun(nextArgs);
                        var nextRetc = nextRets.Length;
                        nextRets.AsSpan().CopyTo(_stack.AsSpan(ptr, nextRetc));
                        popUntil(ptr + nextRetc);
                    } else {
                        throw new InvalidOperationException();
                    }

                    pushDelimiter(ptr);
                } else if (op == GlosOp.Ret) {
                    break;
                } else if (op == GlosOp.Bind) {
                    stackTop().AssertFunction().ParentContext = ctx;
                } else if (cat == GlosOpCategory.ShpRv) {
                    var count = _sptr - popDelimiter();

                    while (count > imms && count > 0) {
                        popStack();
                        --count;
                    }

                    while (count < imms) {
                        pushNil();
                        ++count;
                    }
                } else if (op == GlosOp.PopDel) {
                    popDelimiter();
                } else if (op == GlosOp.Nop) {
                    // nop;
                } else if (cat == GlosOpCategory.Syscall) {
                    // _syscalls[(int)imms]?.Invoke(_stack, ref _sptr, _callStack, ref _cptr);
                } else {
                    throw new GlosUnknownOpException(this, (byte)op);
                }
            }

            // clean up
            var rtb = popDelimiter();
            var retc = _sptr - rtb;

            _stack.AsSpan(rtb, retc).CopyTo(_stack.AsSpan(bptr, retc));
            popUntil(bptr + retc);
            popCurrentFrameDelimiter();
            popCallStackFrame();
        }

        public GlosValue[] ExecuteFunction(GlosFunction function, GlosValue[]? args) {
            var oldSptr = _sptr;

            args ??= Array.Empty<GlosValue>();
            var argc = args.Length;
            args.AsSpan().CopyTo(_stack.AsSpan(_sptr, argc));

            function.ParentContext ??= new GlosContext(null);

            executeFunctionOnStack(oldSptr, function, argc);

            var retc = _sptr - oldSptr;
            var rv = new GlosValue[retc];
            _stack.AsSpan(oldSptr, retc).CopyTo(rv.AsSpan());

            popUntil(oldSptr);
            return rv;
        }

        // though ViMa shouldn't manage units, this function is necessary
        public GlosValue[] ExecuteUnit(GlosUnit unit, GlosValue[]? args) {
            return ExecuteFunction(new GlosFunction(unit.FunctionTable[unit.Entry], new GlosContext(null)), args);
        }

        public GlosValue[] ExecuteUnit(GlosUnit unit, GlosValue[]? args, GlosContext parentContext) {
            return ExecuteFunction(new GlosFunction(unit.FunctionTable[unit.Entry], parentContext), args);
        }
    }
}
