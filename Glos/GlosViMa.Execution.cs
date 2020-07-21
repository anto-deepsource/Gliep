using System;

namespace GeminiLab.Glos {
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

        private void pushNewCallFrame(int bptr, GlosFunction function, int argc, GlosContext? context) {
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

            foreach (var s in function.Prototype.VariableInContext) frame.Context.CreateVariable(s);
        }

        // TODO: assess whether this struct is necessary
        // is it really slower fetching following values from call stack than caching them here?
        private ref struct ExecutorContext {
            public int CallStackBase;
            public int CurrentCallStack;
            public int InstructionPointer;
            public GlosFunctionPrototype Prototype;
            public ReadOnlySpan<byte> Code;
            public int Length;
            public GlosUnit Unit;
            public GlosContext Context;
            public GlosContext Global;
        }

        void restoreStatus(ref ExecutorContext ctx) {
            if (_cptr <= ctx.CallStackBase) return;
            ref var frame = ref callStackTop();

            ctx.CurrentCallStack = _cptr - 1;
            ctx.InstructionPointer = frame.InstructionPointer;
            ctx.Prototype = frame.Function.Prototype;
            ctx.Code = ctx.Prototype.Code;
            ctx.Length = ctx.Code.Length;
            ctx.Unit = ctx.Prototype.Unit;
            ctx.Context = frame.Context;
            ctx.Global = ctx.Context.Global;
        }

        void storeStatus(ref ExecutorContext ctx) {
            ref var frame = ref _callStack[ctx.CurrentCallStack];

            frame.InstructionPointer = ctx.InstructionPointer;
        }

        private GlosValue[] execute(GlosFunction function, GlosValue[]? args = null, GlosContext? thisContext = null) {
            var callStackBase = _cptr;
            var bptr = _sptr;

            ExecutorContext execCtx = default;
            execCtx.CallStackBase = callStackBase;

            ref var ip = ref execCtx.InstructionPointer;
            ref GlosFunctionPrototype proto = ref execCtx.Prototype!;
            ref var code = ref execCtx.Code;
            ref var len = ref execCtx.Length;
            ref GlosUnit unit = ref execCtx.Unit!;
            ref GlosContext ctx = ref execCtx.Context!;
            ref GlosContext global = ref execCtx.Global!;

            try {
                var firstArgc = args?.Length ?? 0;
                args?.AsSpan().CopyTo(_stack.AsSpan(bptr, firstArgc));

                pushNewCallFrame(bptr, function, firstArgc, thisContext);
                restoreStatus(ref execCtx);
                pushUntil(callStackTop().PrivateStackBase);

                while (_cptr > callStackBase) {
                    if (ip < 0 || ip > len) throw new GlosInvalidInstructionPointerException();

                    if (!ReadInstructionAndImmediate(code, ref ip, out var op, out long imms, out bool immOnStack))
                        throw new GlosUnexpectedEndOfCodeException();

                    if (immOnStack) {
                        imms = stackTop().AssertInteger();
                        popStack();
                    }

                    // the implicit return at the end of function
                    var cat = GlosOpInfo.Categories[(int)op];

                    GlosValue temp = default;
                    // execution
                    switch (cat) {
                    case GlosOpCategory.BinaryOperator:
                        Calculator.ExecuteBinaryOperation(ref stackTop(1), in stackTop(1), in stackTop(), op);
                        popStack();
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
                    case GlosOpCategory.TableVectorOperator when op == GlosOp.Ren:
                        if (stackTop(1).AssertTable().TryReadEntry(stackTop(), out temp)) {
                            stackTop(1) = temp;
                        } else {
                            stackTop(1).SetNil();
                        }
                        popStack();
                        break;
                    case GlosOpCategory.TableVectorOperator when op == GlosOp.Uen:
                        stackTop(1).AssertTable().UpdateEntry(stackTop(), stackTop(2));
                        popStack(3);
                        break;
                    case GlosOpCategory.TableVectorOperator when op == GlosOp.RenL:
                        if (stackTop(1).AssertTable().TryReadEntryLocally(stackTop(), out temp)) {
                            stackTop(1) = temp;
                        } else {
                            stackTop(1).SetNil();
                        }
                        popStack();
                        break;
                    case GlosOpCategory.TableVectorOperator when op == GlosOp.UenL:
                        stackTop(1).AssertTable().UpdateEntryLocally(stackTop(), stackTop(2));
                        popStack(3);
                        break;
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
                    case GlosOpCategory.TableVectorOperator when op == GlosOp.Iniv:
                        stackTop(1).AssertVector().Push(in stackTop());
                        popStack();
                        break;
                    case GlosOpCategory.UnaryOperator:
                        Calculator.ExecuteUnaryOperation(ref stackTop(), in stackTop(), op);
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
                        throw new ArgumentOutOfRangeException();
                    case GlosOpCategory.LoadFunction:
                        pushStack().SetFunction(new GlosFunction(unit.FunctionTable[(int)imms]!, null!));
                        break;
                    case GlosOpCategory.LoadString when imms > unit.StringTable.Count || imms < 0:
                        throw new ArgumentOutOfRangeException();
                    case GlosOpCategory.LoadString:
                        pushStack().SetString(unit.StringTable[(int)imms]);
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
                        pushStack().SetFloatByBinaryPresentation(unchecked((ulong)imms));
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
                        throw new GlosLocalVariableIndexOutOfRangeException((int)imms);
                    case GlosOpCategory.LoadLocalVariable:
                        pushStack() = _stack[callStackTop().LocalVariablesBase + (int)imms]; // !!!!!
                        break;
                    case GlosOpCategory.StoreLocalVariable when imms >= proto.LocalVariableSize || imms < 0:
                        throw new GlosLocalVariableIndexOutOfRangeException((int)imms);
                    case GlosOpCategory.StoreLocalVariable:
                        _stack[callStackTop().LocalVariablesBase + (int)imms] = stackTop();
                        popStack();
                        break;
                    case GlosOpCategory.LoadArgument:
                        pushNil();
                        if (imms < callStackTop().ArgumentsCount && imms >= 0) stackTop() = _stack[callStackTop().ArgumentsBase + (int)imms];
                        break;
                    case GlosOpCategory.Branch:
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
                        _syscalls[(int)imms]?.Invoke(_stack, _callStack, _delStack);
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
                            storeStatus(ref execCtx);
                            pushNewCallFrame(ptr, fun, nextArgc, null);
                            restoreStatus(ref execCtx);
                            pushUntil(callStackTop().PrivateStackBase);
                        } else {
                            throw new GlosValueNotCallableException(funv);
                        }

                        break;
                    }
                    case GlosOpCategory.Others when op == GlosOp.Ret: {
                        // clean up
                        var rtb = popDelimiter();
                        var retc = _sptr - rtb;

                        _stack.AsSpan(rtb, retc).CopyTo(_stack.AsSpan(callStackTop().StackBase, retc));
                        popUntil(callStackTop().StackBase + retc);
                        popCurrentFrameDelimiter();
                        popCallStackFrame();
                        restoreStatus(ref execCtx);
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
                        var del = peekDelimiter();
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
                        throw new GlosUnknownOpException((byte)op);
                    }
                }

                var rc = _sptr - bptr;
                var rv = new GlosValue[rc];

                _stack.AsSpan(bptr, rc).CopyTo(rv);
                popUntil(bptr);
                return rv;
            } catch (GlosException ex) when (!(ex is GlosRuntimeException)) {
                throw new GlosRuntimeException(this, ex);
            } finally {
                storeStatus(ref execCtx);
            }
        }

        public GlosValue[] ExecuteFunctionWithProvidedContext(GlosFunction function, GlosContext context, GlosValue[]? args = null) {
            return execute(function, args, context);
        }

        public GlosValue[] ExecuteFunction(GlosFunction function, GlosValue[]? args = null) {
            return execute(function, args);
        }

        // though ViMa shouldn't manage units, this function is necessary
        public GlosValue[] ExecuteUnit(GlosUnit unit, GlosValue[]? args = null, GlosContext? parentContext = null) {
            return ExecuteFunction(new GlosFunction(unit.FunctionTable[unit.Entry], parentContext ?? new GlosContext(null)), args);
        }

        public GlosValue[] ExecuteUnitWithProvidedContextForRootFunction(GlosUnit unit, GlosContext context, GlosValue[]? args = null) {
            return ExecuteFunctionWithProvidedContext(new GlosFunction(unit.FunctionTable[unit.Entry], context.Parent!), context, args);
        }
    }
}
