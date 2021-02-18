using System;
using System.Collections.Generic;
using GeminiLab.Glos;
using GeminiLab.XUnitTester.Gliep.Misc;
using Xunit;

namespace GeminiLab.XUnitTester.Gliep.Glos.Async {
    public class AsyncEFunctions : GlosTestBase {
        class AsyncSumFunction : IGlosAsyncEFunction {
            class C : IGlosAsyncEFunctionCall {
                private long _sum = 0;

                public AsyncEFunctionResumeResult Resume(ReadOnlySpan<GlosValue> arguments) {
                    long num = 0;
                    if (arguments.Length >= 1 && arguments[0].Type == GlosValueType.Integer) {
                        num = arguments[0].AssumeInteger();
                    }

                    if (num < 0) {
                        return AsyncEFunctionResumeResult.Return(_sum);
                    } else {
                        _sum += num;
                        return AsyncEFunctionResumeResult.Yield(_sum);
                    }
                }
            }

            public IGlosAsyncEFunctionCall Call(GlosCoroutine coroutine) {
                return new C();
            }
        }

        [Fact]
        public void AsyncExternalFunction() {
            var f = Builder.AddFunction();
            var c = f.AllocateLocalVariable();

            f.AppendLdArg(0);
            f.AppendMkc();
            f.AppendStLoc(c);

            f.AppendLdDel();

            f.AppendLdDel();
            f.AppendLd(1);
            f.AppendLdLoc(c);
            f.AppendResume();
            f.AppendPopDel();

            f.AppendLdDel();
            f.AppendLd(2);
            f.AppendLdLoc(c);
            f.AppendResume();
            f.AppendPopDel();

            f.AppendLdDel();
            f.AppendLd(3);
            f.AppendLdLoc(c);
            f.AppendResume();
            f.AppendPopDel();

            f.AppendLdDel();
            f.AppendLd(-1);
            f.AppendLdLoc(c);
            f.AppendResume();
            f.AppendPopDel();

            f.AppendRet();

            f.SetEntry();

            var res = Execute(new[] { GlosValue.NewAsyncEFunction(new AsyncSumFunction()) });

            GlosValueArrayChecker.Create(res)
                .FirstOne().AssertInteger(1)
                .MoveNext().AssertInteger(3)
                .MoveNext().AssertInteger(6)
                .MoveNext().AssertInteger(6)
                .MoveNext().AssertEnd();
        }

        class SchedulableMapFunction : IGlosAsyncEFunction {
            class C : IGlosAsyncEFunctionCall {
                private int        _processed = -1;
                private int        _length    = 0;
                private GlosVector _mapped    = null!;
                private GlosVector _vec       = null!;
                private GlosValue  _fun       = default;

                public AsyncEFunctionResumeResult Resume(ReadOnlySpan<GlosValue> arguments) {
                    if (_processed < 0) {
                        _vec = arguments[0].AssertVector();
                        _fun = arguments[1];
                        _fun.AssertInvokable();

                        _length = _vec.Count;

                        _mapped = new GlosVector(_length);
                        _processed = 0;
                    } else {
                        if (arguments.Length >= 1) {
                            _mapped.Push(in arguments[0]);
                        } else {
                            _mapped.PushNil();
                        }
                    }

                    if (_processed >= _length) {
                        return AsyncEFunctionResumeResult.Return(_mapped);
                    }

                    return AsyncEFunctionResumeResult.Call(_fun, _vec[_processed++]);
                }
            }

            public IGlosAsyncEFunctionCall Call(GlosCoroutine coroutine) {
                return new C();
            }
        }

        [Fact]
        public void SchedulableMap() {
            // infOne is a coroutine that yields 1 endlessly
            var infOne = Builder.AddFunction();
            var begin = infOne.AllocateAndInsertLabel();
            infOne.AppendLdDel();
            infOne.AppendLd(1);
            infOne.AppendYield();
            infOne.AppendShpRv(0);
            infOne.AppendB(begin);

            // sqrAddOne is a function add 1 (get from infOne) to its first argument squared and returns it
            var sqrAddOne = Builder.AddFunction();
            var infOneLv = sqrAddOne.AllocateLocalVariable();
            sqrAddOne.AppendLdStr("inf_one");
            sqrAddOne.AppendRvg();
            sqrAddOne.AppendStLoc(infOneLv);
            sqrAddOne.AppendLdDel();
            sqrAddOne.AppendLdArg(0);
            sqrAddOne.AppendDup();
            sqrAddOne.AppendMul();
            sqrAddOne.AppendLdDel();
            sqrAddOne.AppendLdLoc(infOneLv);
            sqrAddOne.AppendResume();
            sqrAddOne.AppendShpRv(1);
            sqrAddOne.AppendAdd();
            sqrAddOne.AppendRet();

            // main test function
            // args: map
            var f = Builder.AddFunction();
            var map = f.AllocateLocalVariable();
            var mapper = f.AllocateLocalVariable();

            f.AppendLdArg(0);
            f.AppendStLoc(map);

            f.AppendLdFun(sqrAddOne);
            f.AppendBind();
            f.AppendStLoc(mapper);

            f.AppendLdFun(infOne);
            f.AppendBind();
            f.AppendMkc();
            f.AppendLdStr("inf_one");
            f.AppendUvg();

            f.AppendLdDel();
            f.AppendLd(0);
            f.AppendLd(1);
            f.AppendLd(2);
            f.AppendLd(3);
            f.AppendPkv();
            f.AppendLdLoc(mapper);
            f.AppendLdLoc(map);
            f.AppendCall();
            f.AppendUpv();
            f.AppendRet();

            f.SetEntry();

            var res = Execute(new[] { GlosValue.NewAsyncEFunction(new SchedulableMapFunction()) });

            GlosValueArrayChecker.Create(res)
                .FirstOne().AssertInteger(1)
                .MoveNext().AssertInteger(2)
                .MoveNext().AssertInteger(5)
                .MoveNext().AssertInteger(10)
                .MoveNext().AssertEnd();
        }

        class SimpleCoroutineScheduler : IGlosAsyncEFunction {
            class C : IGlosAsyncEFunctionCall {
                private int              _count      = 0;
                private GlosCoroutine[]? _coroutines = null;
                private bool[]           _finished   = null!;
                private int              _last       = 0;

                private List<GlosValue> _values = new List<GlosValue>();

                private int findNext(int first) {
                    for (int it = first; it < first + _count; ++it) {
                        if (!_finished[it % _count]) {
                            return it % _count;
                        }
                    }

                    return -1;
                }

                public AsyncEFunctionResumeResult Resume(ReadOnlySpan<GlosValue> arguments) {
                    if (_coroutines == null) {
                        var cors = new List<GlosCoroutine>();
                        foreach (var v in arguments) {
                            if (v.Type == GlosValueType.Coroutine) {
                                cors.Add(v.AssertCoroutine());
                            }
                        }

                        _coroutines = cors.ToArray();
                        _finished = new bool[_count = cors.Count];

                        _last = -1;
                    } else {
                        foreach (var v in arguments) {
                            _values.Add(v);
                        }

                        if (_coroutines[_last].Status != GlosCoroutineStatus.Suspend) {
                            _finished[_last] = true;
                        }
                    }

                    _last = findNext(_last + 1);
                    if (_last < 0) {
                        return AsyncEFunctionResumeResult.Return(_values.ToArray());
                    }

                    return AsyncEFunctionResumeResult.Resume(_coroutines[_last]);
                }
            }

            public IGlosAsyncEFunctionCall Call(GlosCoroutine coroutine) {
                return new C();
            }
        }

        [Fact]
        public void SimpleScheduler() {
            // cor0: 0 * 2
            var cor0 = Builder.AddFunction();
            cor0.AppendLdDel();
            cor0.AppendLd(0);
            cor0.AppendYield();
            cor0.AppendShpRv(0);
            cor0.AppendLdDel();
            cor0.AppendLd(0);
            cor0.AppendYield();
            cor0.AppendShpRv(0);
            
            // cor1: 1 * 1
            var cor1 = Builder.AddFunction();
            cor1.AppendLdDel();
            cor1.AppendLd(1);
            cor1.AppendYield();
            cor1.AppendShpRv(0);
            
            // cor2: 2 * 3
            var cor2 = Builder.AddFunction();
            cor2.AppendLdDel();
            cor2.AppendLd(2);
            cor2.AppendYield();
            cor2.AppendShpRv(0);
            cor2.AppendLdDel();
            cor2.AppendLd(2);
            cor2.AppendYield();
            cor2.AppendShpRv(0);
            cor2.AppendLdDel();
            cor2.AppendLd(2);
            cor2.AppendYield();
            cor2.AppendShpRv(0);
            
            // main test function
            // args: scheduler
            var f = Builder.AddFunction();
            var s = f.AllocateLocalVariable();

            f.AppendLdArg(0);
            f.AppendStLoc(s);

            f.AppendLdDel();
            f.AppendLdFun(cor0);
            f.AppendBind();
            f.AppendMkc();
            f.AppendLdFun(cor1);
            f.AppendBind();
            f.AppendMkc();
            f.AppendLdFun(cor2);
            f.AppendBind();
            f.AppendMkc();
            f.AppendLdLoc(s);
            f.AppendCall();
            f.AppendRet();

            f.SetEntry();

            var res = Execute(new[] { GlosValue.NewAsyncEFunction(new SimpleCoroutineScheduler()) });

            GlosValueArrayChecker.Create(res)
                .FirstOne().AssertInteger(0)
                .MoveNext().AssertInteger(1)
                .MoveNext().AssertInteger(2)
                .MoveNext().AssertInteger(0)
                .MoveNext().AssertInteger(2)
                .MoveNext().AssertInteger(2)
                .MoveNext().AssertEnd();
        }
    }
}
