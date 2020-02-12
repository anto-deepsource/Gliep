using System;

namespace GeminiLab.Glos.ViMa {
    public partial class GlosViMa {
        private void executeFunctionOnStack(long bptr, GlosFunction function, long argc) {
            var proto = function.Prototype;
            var unit = function.Prototype.Unit;
            var env = function.Environment;

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

            _sptr = prib;

            var ops = proto.Op;
            var len = ops.Length;
            ip = 0;
            while (true) {
                if (ip < 0 || ip > len) throw new InvalidProgramCounterException();

                // the implicit return at the end of function
                var op = ip == len ? GlosOp.Ret : (GlosOp)ops[ip++];
                var cat = GlosOpInfo.Categories[(int)op];

                // get imm
                ulong imm;
                long imms;
                switch (GlosOpInfo.Immediates[(int)op]) {
                case GlosOpImmediate.Embedded:
                    imm = ((ulong)op) & 0x07;
                    imms = ((long)op) & 0x07;
                    break;
                case GlosOpImmediate.Byte:
                    if (ip + 1 > len) throw new UnexpectedEndOfOpsException();
                    imm = ops[ip++];
                    imms = unchecked((sbyte)(byte)imm);
                    break;
                case GlosOpImmediate.Dword:
                    if (ip + 4 > len) throw new UnexpectedEndOfOpsException();
                    imm = (ulong)ops[ip]
                          | ((ulong)ops[ip + 1] << 8)
                          | ((ulong)ops[ip + 2] << 16)
                          | ((ulong)ops[ip + 3] << 24);
                    imms = unchecked((int)(uint)imm);
                    ip += 4;
                    break;
                case GlosOpImmediate.Qword:
                    if (ip + 8 > len) throw new UnexpectedEndOfOpsException();
                    imm = (ulong)ops[ip]
                          | ((ulong)ops[ip + 1] << 8)
                          | ((ulong)ops[ip + 2] << 16)
                          | ((ulong)ops[ip + 3] << 24)
                          | ((ulong)ops[ip + 4] << 32)
                          | ((ulong)ops[ip + 5] << 40)
                          | ((ulong)ops[ip + 6] << 48)
                          | ((ulong)ops[ip + 7] << 56);
                    imms = unchecked((long)imm);
                    ip += 8;
                    break;
                case GlosOpImmediate.OnStack:
                    imms = stackTop().AssertInteger();
                    imm = unchecked((ulong)imms);
                    popStack();
                    break;
                default:
                    imm = 0;
                    imms = 0;
                    break;
                }

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
                } else if (op == GlosOp.LdEnv) {
                    pushStack().SetTable(env);
                } else if (op == GlosOp.LdGlb) {
                    pushStack().SetTable(GlobalEnvironment!);
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
                    pushStack().SetFloatByBinaryRepresentation(imm);
                } else if (op == GlosOp.LdTrue) {
                    pushStack().SetBoolean(true);
                } else if (op == GlosOp.LdFalse) {
                    pushStack().SetBoolean(false);
                } else if (cat == GlosOpCategory.LoadLocalVariable) {
                    if (imms >= locc || imms < 0) throw new LocalVariableIndexOutOfRangeException();
                    copy(out pushStack(), in _stack[locb + imms]);
                } else if (cat == GlosOpCategory.StoreLocalVariable) {
                    if (imms >= locc || imms < 0) throw new LocalVariableIndexOutOfRangeException();
                    copy(out _stack[locb + imms], in stackTop());
                    popStack();
                } else if (cat == GlosOpCategory.LoadArgument) {
                    pushNil();
                    if (imms < argc && imms >= 0) copy(out stackTop(), in _stack[argb + imms]);
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
                        Array.Copy(_stack, ptr, nextArgs, 0, nextArgc);
                        var nextRets = fun(nextArgs);
                        var nextRetc = nextRets.Length;
                        Array.Copy(nextRets, 0, _stack, ptr, nextRetc);
                        while (_sptr > ptr + nextRetc) popStack();
                    } else {
                        throw new InvalidOperationException();
                    }

                    pushDelimiter(ptr);
                } else if (op == GlosOp.Ret) {
                    break;
                } else if (op == GlosOp.Bind) {
                    stackTop(1).AssertFunction().Environment = stackTop().AssertTable();
                    popStack();
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
                } else if (op == GlosOp.Nop) {
                    // nop;
                } else if (cat == GlosOpCategory.Syscall) {
                    _syscalls[(int)imms]?.Invoke(_stack, ref _sptr, _callStack, ref _cptr);
                } else {
                    throw new UnknownOpcodeException();
                }
            }

            // clean up
            var rtb = popDelimiter();
            
            var retc = _sptr - rtb;
            for (var i = 0; i < retc; ++i) {
                copy(out _stack[bptr + i], in _stack[rtb + i]);
            }

            while (_sptr > bptr + retc) popStack();
            popCallStackFrame();
        }

        public GlosValue[] ExecuteFunction(GlosFunction function, GlosValue[]? args) {
            args ??= Array.Empty<GlosValue>();
            var argc = args.Length;

            var oldSptr = _sptr;

            Array.Copy(args, 0, _stack, _sptr, argc);
            executeFunctionOnStack(oldSptr, function, argc);

            var retc = _sptr - oldSptr;

            var rv = new GlosValue[retc];
            Array.Copy(_stack, _sptr - retc, rv, 0, retc);

            while (_sptr > oldSptr) popStack();
            return rv;
        }
    }
}
