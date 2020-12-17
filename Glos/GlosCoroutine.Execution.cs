using System;

namespace GeminiLab.Glos {
    public enum GlosCoroutineStatus : byte {
        Invalid,
        Initial,
        InStack,
        Running,
        Suspend,
        Stopped,
    }

    public partial class GlosCoroutine {
        public GlosCoroutineStatus Status { get; private set; }

        private ReadOnlySpan<GlosValue> resume(ReadOnlySpan<GlosValue> args) {
            switch (Status) {
            case GlosCoroutineStatus.Initial:
                break;
            case GlosCoroutineStatus.InStack:
                break;
            case GlosCoroutineStatus.Running:
                break;
            case GlosCoroutineStatus.Suspend:
                break;
            case GlosCoroutineStatus.Stopped:
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }

            return new ReadOnlySpan<GlosValue>();
        }

        public static bool ReadInstructionAndImmediate(ReadOnlySpan<byte> code, ref int ip, out GlosOp op, out long imm, out bool immOnStack) {
            var len = code.Length;

            immOnStack = false;
            imm = 0;

            if (ip == len) {
                op = GlosOp.Ret;
                return true;
            }

            var opb = code[ip++];
            op = (GlosOp) opb;
            var immt = GlosOpInfo.Immediates[opb];

            unchecked {
                switch (immt) {
                case GlosOpImmediate.Embedded:
                    imm = opb & 0x07;
                    break;
                case GlosOpImmediate.Byte:
                    if (ip + 1 > len) return false;

                    imm = (sbyte) code[ip++];
                    break;
                case GlosOpImmediate.Dword:
                    if (ip + 4 > len) return false;

                    imm = unchecked((int) (uint) ((ulong) code[ip] | ((ulong) code[ip + 1] << 8) | ((ulong) code[ip + 2] << 16) | ((ulong) code[ip + 3] << 24)));
                    ip += 4;
                    break;
                case GlosOpImmediate.Qword:
                    if (ip + 8 > len) return false;
                    imm = unchecked((long) ((ulong) code[ip] | ((ulong) code[ip + 1] << 8) | ((ulong) code[ip + 2] << 16) | ((ulong) code[ip + 3] << 24) | ((ulong) code[ip + 4] << 32) | ((ulong) code[ip + 5] << 40) |
                                            ((ulong) code[ip + 6] << 48) | ((ulong) code[ip + 7] << 56)));
                    ip += 8;
                    break;
                case GlosOpImmediate.OnStack:
                    immOnStack = true;
                    break;
                }
            }

            return true;
        }

        private void pushNewStackFrame(int bptr, GlosFunction function, int argc, GlosContext? context, int returnSize) {
            ref var frame = ref pushCallStackFrame();

            frame.Function = function;
            frame.Context = context ?? new GlosContext(function.ParentContext);

            frame.StackBase = bptr;
            frame.ArgumentsBase = bptr;
            frame.ArgumentsCount = argc;
            frame.LocalVariablesBase = frame.ArgumentsBase + frame.ArgumentsCount;
            frame.PrivateStackBase = frame.LocalVariablesBase + function.Prototype.LocalVariableSize;
            frame.InstructionPointer = 0;
            frame.DelimiterStackBase = _dptr;

            frame.ReturnSize = returnSize;
            
            foreach (var s in function.Prototype.VariableInContext) frame.Context.CreateVariable(s);
        }

        public enum ExecResultType {
            Return,
            Resume,
            Yield,
        }

        public ref struct ExecResult {
            public ExecResultType Result;
            public GlosValue[]    ReturnValues;
            public int            CoroutineToResume;
        }

        private ExecResult execute(int callStackBase, bool allowCoroutineSchedule) {
            var bptr = _sptr;

            int ip = 0;
            GlosFunctionPrototype proto = null!;
            int len = 0;
            IGlosUnit unit = null!;
            GlosContext ctx = null!, global = null!;

            try {
                restoreStatus();

                while (_cptr > callStackBase) {
                    if (ip < 0 || ip > len) {
                        throw new GlosInvalidInstructionPointerException();
                    }

                    if (!ReadInstructionAndImmediate(proto.Code, ref ip, out var op, out long imms, out bool immOnStack))
                        throw new GlosUnexpectedEndOfCodeException();

                    if (immOnStack) {
                        imms = stackTop().AssertInteger();
                        popStack();
                    }

                    var cat = GlosOpInfo.Categories[(int) op];

                    GlosValue temp = default;
                    // execution
                    switch (cat) {
                    case GlosOpCategory.BinaryOperator:
                        executeBinaryOperation(op);
                        break;
                    case GlosOpCategory.TableVectorOperator when op == GlosOp.Smt:
                        stackTop().AssertTable().Metatable = stackTop(1).AssertTable();
                        popStack(2);
                        break;
                    case GlosOpCategory.TableVectorOperator when op == GlosOp.Gmt:
                        if (stackTop().AssertTable().Metatable is { } mt) {
                            stackTop().SetTable(mt);
                        } else {
                            stackTop().SetNil();
                        }

                        break;
                    case GlosOpCategory.TableVectorOperator when op == GlosOp.Ren: {
                        ref var target = ref stackTop(1);

                        if (target.Type == GlosValueType.Vector) {
                            var vec = target.AssumeVector();
                            var idx = stackTop().ValueNumber.Integer;
                            if (stackTop().Type == GlosValueType.Integer && idx >= 0 && idx < vec.Count) {
                                target = vec[(int) idx];
                            } else {
                                target.SetNil();
                            }
                        } else {
                            if (target.AssertTable().TryReadEntry(stackTop(), out temp)) {
                                target = temp;
                            } else {
                                target.SetNil();
                            }
                        }

                        popStack();
                        break;
                    }
                    case GlosOpCategory.TableVectorOperator when op == GlosOp.Uen: {
                        ref var target = ref stackTop(1);

                        if (target.Type == GlosValueType.Vector) {
                            stackTop(1).AssumeVector()[(int) stackTop().AssertInteger()] = stackTop(2);
                        } else {
                            stackTop(1).AssertTable().UpdateEntry(stackTop(), stackTop(2));
                        }

                        popStack(3);
                        break;
                    }
                    case GlosOpCategory.TableVectorOperator when op == GlosOp.RenL: {
                        ref var target = ref stackTop(1);

                        if (target.Type == GlosValueType.Vector) {
                            var vec = target.AssumeVector();
                            var idx = stackTop().ValueNumber.Integer;
                            if (stackTop().Type == GlosValueType.Integer && idx >= 0 && idx < vec.Count) {
                                target = vec[(int) idx];
                            } else {
                                target.SetNil();
                            }
                        } else {
                            if (target.AssertTable().TryReadEntryLocally(stackTop(), out temp)) {
                                target = temp;
                            } else {
                                target.SetNil();
                            }
                        }

                        popStack();
                        break;
                    }
                    case GlosOpCategory.TableVectorOperator when op == GlosOp.UenL: {
                        ref var target = ref stackTop(1);

                        if (target.Type == GlosValueType.Vector) {
                            stackTop(1).AssumeVector()[(int) stackTop().AssertInteger()] = stackTop(2);
                        } else {
                            stackTop(1).AssertTable().UpdateEntryLocally(stackTop(), stackTop(2));
                        }

                        popStack(3);
                        break;
                    }
                    case GlosOpCategory.TableVectorOperator when op == GlosOp.Ien:
                        stackTop(2).AssertTable().UpdateEntryLocally(stackTop(1), stackTop());
                        popStack(2);
                        break;
                    case GlosOpCategory.TableVectorOperator when op == GlosOp.Pshv:
                        stackTop().AssertVector().Push(in stackTop(1));
                        popStack(2);
                        break;
                    case GlosOpCategory.TableVectorOperator when op == GlosOp.Popv: {
                        ref var tail = ref stackTop().AssertVector().PopRef();
                        stackTop() = tail;
                        tail.SetNil();
                        break;
                    }
                    case GlosOpCategory.UnaryOperator:
                        executeUnaryOperation(op);
                        break;
                    case GlosOpCategory.ContextOperator when op == GlosOp.Rvc:
                        stackTop() = ctx.GetVariableReference(stackTop().AssertString());
                        break;
                    case GlosOpCategory.ContextOperator when op == GlosOp.Uvc:
                        ctx.GetVariableReference(stackTop().AssertString()) = stackTop(1);
                        popStack(2);
                        break;
                    case GlosOpCategory.ContextOperator when op == GlosOp.Rvg:
                        stackTop() = global.GetVariableReference(stackTop().AssertString());
                        break;
                    case GlosOpCategory.ContextOperator when op == GlosOp.Uvg:
                        global.GetVariableReference(stackTop().AssertString()) = stackTop(1);
                        popStack(2);
                        break;
                    case GlosOpCategory.LoadFunction when imms > unit.FunctionTable.Count || imms < 0:
                        throw new GlosFunctionIndexOutOfRangeException((int) imms);
                    case GlosOpCategory.LoadFunction:
                        pushStack().SetFunction(new GlosFunction(unit.FunctionTable[(int) imms]!, null!, unit));
                        break;
                    case GlosOpCategory.LoadString when imms > unit.StringTable.Count || imms < 0:
                        throw new GlosStringIndexOutOfRangeException((int) imms);
                    case GlosOpCategory.LoadString:
                        pushStack().SetString(unit.StringTable[(int) imms]);
                        break;
                    case GlosOpCategory.LoadInteger:
                        pushStack().SetInteger(op == GlosOp.LdNeg1 ? -1 : imms);
                        break;
                    case GlosOpCategory.LoadMisc when op == GlosOp.LdNTbl:
                        pushStack().SetTable(new GlosTable(this));
                        break;
                    case GlosOpCategory.LoadMisc when op == GlosOp.LdNil:
                        pushNil();
                        break;
                    case GlosOpCategory.LoadMisc when op == GlosOp.LdFlt:
                        pushStack().SetFloatByBinaryPresentation(unchecked((ulong) imms));
                        break;
                    case GlosOpCategory.LoadMisc when op == GlosOp.LdNVec:
                        pushStack().SetVector(new GlosVector());
                        break;
                    case GlosOpCategory.LoadMisc when op == GlosOp.LdTrue:
                        pushStack().SetBoolean(true);
                        break;
                    case GlosOpCategory.LoadMisc when op == GlosOp.LdFalse:
                        pushStack().SetBoolean(false);
                        break;
                    case GlosOpCategory.LoadMisc when op == GlosOp.LdArgc:
                        pushStack().SetInteger(callStackTop().ArgumentsCount);
                        break;
                    case GlosOpCategory.LoadLocalVariable when imms >= proto.LocalVariableSize || imms < 0:
                        throw new GlosLocalVariableIndexOutOfRangeException((int) imms);
                    case GlosOpCategory.LoadLocalVariable:
                        pushStack() = _stack[callStackTop().LocalVariablesBase + (int) imms]; // !!!!!
                        break;
                    case GlosOpCategory.StoreLocalVariable when imms >= proto.LocalVariableSize || imms < 0:
                        throw new GlosLocalVariableIndexOutOfRangeException((int) imms);
                    case GlosOpCategory.StoreLocalVariable:
                        _stack[callStackTop().LocalVariablesBase + (int) imms] = stackTop();
                        popStack();
                        break;
                    case GlosOpCategory.LoadArgument:
                        pushNil();
                        if (imms < callStackTop().ArgumentsCount && imms >= 0) stackTop() = _stack[callStackTop().ArgumentsBase + (int) imms];
                        break;
                    case GlosOpCategory.Branch:
                        var dest = ip + (int) imms;
                        var jump = op switch {
                            GlosOp.B    => true,
                            GlosOp.BS   => true,
                            GlosOp.Bf   => stackTop().Falsey(),
                            GlosOp.BfS  => stackTop().Falsey(),
                            GlosOp.Bt   => stackTop().Truthy(),
                            GlosOp.BtS  => stackTop().Truthy(),
                            GlosOp.Bn   => stackTop().IsNil(),
                            GlosOp.BnS  => stackTop().IsNil(),
                            GlosOp.Bnn  => stackTop().IsNonNil(),
                            GlosOp.BnnS => stackTop().IsNonNil(),
                            _           => false,
                        };

                        if (jump) ip = dest;

                        if (op != GlosOp.B && op != GlosOp.BS) popStack();
                        break;
                    case GlosOpCategory.ShpRv: {
                        var count = _sptr - popDelimiter();

                        while (count > imms && count > 0) {
                            popStack();
                            --count;
                        }

                        while (count < imms) {
                            pushNil();
                            ++count;
                        }

                        break;
                    }
                    case GlosOpCategory.Syscall:
                        // _syscalls[(int) imms]?.Invoke(_stack, _callStack, _delStack);
                        break;
                    case GlosOpCategory.Others when op == GlosOp.Dup:
                        pushStack();
                        stackTop() = stackTop(1);
                        break;
                    case GlosOpCategory.Others when op == GlosOp.Pop:
                        popStack();
                        break;
                    case GlosOpCategory.Others when op == GlosOp.LdDel:
                        pushDelimiter();
                        break;
                    case GlosOpCategory.Others when op == GlosOp.Call: {
                        var funv = stackTop();
                        popStack();

                        var ptr = peekDelimiter();
                        var nextArgc = _sptr - ptr;

                        if (funv.Type == GlosValueType.ExternalFunction) {
                            var fun = funv.AssertExternalFunction();

                            var nextArgs = new GlosValue[nextArgc];
                            _stack.AsSpan(ptr, nextArgc).CopyTo(nextArgs.AsSpan());
                            var nextRets = fun(nextArgs);
                            var nextRetc = nextRets.Length;
                            nextRets.AsSpan().CopyTo(_stack.AsSpan(ptr, nextRetc));
                            popUntil(ptr + nextRetc);
                        } else if (funv.Type == GlosValueType.Function) {
                            var fun = funv.AssertFunction();
                            callGlosFunction(fun, nextArgc, -1);
                        } else {
                            throw new GlosValueNotCallableException(funv);
                        }

                        break;
                    }
                    case GlosOpCategory.Others when op == GlosOp.Ret: {
                        // clean up
                        var rtb = popDelimiter();
                        var retc = _sptr - rtb;

                        var returnSize = callStackTop().ReturnSize;
                        if (returnSize >= 0) {
                            while (retc > returnSize) {
                                popStack();
                                --retc;
                            }

                            while (retc < returnSize) {
                                pushNil();
                                ++retc;
                            }
                        }
                        
                        _stack.AsSpan(rtb, retc).CopyTo(_stack.AsSpan(callStackTop().StackBase, retc));
                        popUntil(callStackTop().StackBase + retc);
                        popCurrentFrameDelimiter();
                        popCallStackFrame();
                        restoreStatus();
                        break;
                    }
                    case GlosOpCategory.Others when op == GlosOp.Bind:
                        stackTop().AssertFunction().ParentContext = ctx;
                        break;
                    case GlosOpCategory.Others when op == GlosOp.PopDel:
                        popDelimiter();
                        break;
                    case GlosOpCategory.Others when op == GlosOp.DupList: {
                        var del = peekDelimiter();
                        var count = _sptr - del;

                        pushDelimiter();

                        _stack.PreparePush(count);
                        _stack.AsSpan(del).CopyTo(_stack.AsSpan(_sptr, count));
                        break;
                    }
                    case GlosOpCategory.Others when op == GlosOp.Pkv: {
                        var del = popDelimiter();
                        var count = _sptr - del;

                        var vec = new GlosVector();

                        vec.Container().EnsureCapacity(count);
                        _stack.AsSpan(del, count).CopyTo(vec.Container().AsSpan(0, count));

                        while (_sptr > del) popStack();
                        pushStack(vec);
                        break;
                    }
                    case GlosOpCategory.Others when op == GlosOp.Upv: {
                        var vec = stackTop().AssertVector();
                        popStack();

                        pushDelimiter();
                        vec.Container().AsSpan().CopyTo(_stack.AsSpan(_stack.Count, vec.Container().Count));
                        break;
                    }
                    case GlosOpCategory.Others when op == GlosOp.Nop:
                        // nop;
                        break;
                    default:
                        throw new GlosUnknownOpException((byte) op);
                    }
                }

                var rc = _sptr - bptr;
                var rv = new GlosValue[rc];

                _stack.AsSpan(bptr, rc).CopyTo(rv);
                popUntil(bptr);

                return new ExecResult { Result = ExecResultType.Return, ReturnValues = rv };
            } catch (GlosException ex) when (!(ex is GlosRuntimeException)) {
                throw new Exception();
                // throw new GlosRuntimeException(this, ex);
            } finally {
                storeStatus();
            }

#region Local Functions

            void callGlosFunction(GlosFunction fun, int argc, int returnSize) {
                storeStatus();
                pushNewStackFrame(_sptr - argc, fun, argc, null, returnSize);
                restoreStatus();
                pushUntil(callStackTop().PrivateStackBase);
            }
            
            void restoreStatus() {
                if (_cptr <= callStackBase) return;

                ref var frame = ref callStackTop();

                ip = frame.InstructionPointer;
                proto = frame.Function.Prototype;
                len = proto.Code.Length;
                unit = frame.Function.Unit;
                ctx = frame.Context;
                global = ctx.Global;
            }

            void storeStatus() {
                ref var frame = ref callStackTop();

                frame.InstructionPointer = ip;
            }

            bool tryUseBinaryMetamethod(GlosOp op) {
                
            }

            bool tryUseUnaryMetamethod(GlosOp op) {
                
            }

            void executeBinaryOperation(GlosOp op) {
                if (tryUseBinaryMetamethod(op)) {
                    return;
                }
                
                GlosValueStaticCalculator.ExecuteBinaryOperation(ref stackTop(1), in stackTop(1), in stackTop(), op);
                popStack();
            }

            void executeUnaryOperation(GlosOp op) {
                if (tryUseUnaryMetamethod(op)) {
                    return;
                }
                
                GlosValueStaticCalculator.ExecuteUnaryOperation(ref stackTop(), in stackTop(), op);
            }

#endregion
        }

        public GlosValue[] ExecuteFunctionSync(GlosFunction function, GlosValue[]? args = null, GlosContext? thisContext = null) {
            var bptr = _sptr;
            var callStackBase = _cptr;
            var firstArgc = args?.Length ?? 0;
            args?.AsSpan().CopyTo(_stack.AsSpan(bptr, firstArgc));

            pushNewStackFrame(bptr, function, firstArgc, thisContext, -1);
            pushUntil(callStackTop().PrivateStackBase);

            var result = execute(callStackBase, false);

            if (result.Result != ExecResultType.Return) {
                throw new Exception();
            }

            return result.ReturnValues;
        }

        public GlosCoroutine(GlosViMa parent, GlosFunction entry) {
            Status = GlosCoroutineStatus.Invalid;

            ref var stackframe = ref pushCallStackFrame();
            stackframe.Function = entry;

            Status = GlosCoroutineStatus.Initial;
        }
    }
}
