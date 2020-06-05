using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AsyncRoutines.Tests
{
    [TestFixture]
    public class AsyncRoutinesTests
    {
        private bool exceptionCaught = false;
        private RoutineManagerBehavior manager;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            GameObject obj = new GameObject();
            manager = obj.AddComponent<RoutineManagerBehavior>();
            yield break;
        }

        [UnityTest]
        public IEnumerator TestCatchExceptionsFromRoutineAwaiter()
        {
            int result;
            manager.Run(testExceptionCatch());

            yield return null;

            Assert.IsTrue(exceptionCaught);
        }
        private async Routine testExceptionCatch()
        {
            try
            {
                await RoutineBase.WaitForNextFrame();
                await throwEx();
            }
            catch (Exception e)
            {
                exceptionCaught = true;
            }
        }
      
        [UnityTest]
        public IEnumerator TestCatchExceptionsFromRoutineAwaiterTyped()
        {
            int result;
            manager.Run(testExceptionCatchTyped());

            yield return null;

            Assert.IsTrue(exceptionCaught);
        }
        private async Routine testExceptionCatchTyped()
        {
            try
            {
                await RoutineBase.WaitForNextFrame();
                var val = await throwExTyped();
            }
            catch (Exception e)
            {
                exceptionCaught = true;
            }
        }

        private async Routine throwEx()
        {
            throw new Exception();
        }
        private async Routine<int> throwExTyped()
        {
            throw new Exception();
        }

        public async Routine ReturnNextFrame()
        {
            await Routine.WaitForNextFrame();
        }
        public async Routine<int> ReturnNextFrameTyped()
        {
            await Routine.WaitForNextFrame();
            return 5;
        }


        [UnityTest]
        public IEnumerator testWaitForAny()
        {
            Exception e;
            var doneA = false;
            var doneB = false;
            async Routine waitShort()
            {
                await RoutineBase.WaitForNextFrame();
                doneA = true;
            }

            async Routine waitLong()
            {
                await RoutineBase.WaitForNextFrame();
                await RoutineBase.WaitForNextFrame();
                doneB = true;
            }

            async Routine testWaitForAny()
            {
                await RoutineBase.WaitForAny(waitShort(), waitLong());
            }
            
            var handle = manager.Run(testWaitForAny());
            
            Assert.IsFalse(doneA);
            Assert.IsFalse(doneB);
            Assert.IsFalse(handle.IsDead);
            yield return null;
            
            Assert.IsTrue(doneA);
            Assert.IsFalse(doneB);
            Assert.IsTrue(handle.IsDead);
            
            yield return null;
            Assert.IsTrue(doneA);
            Assert.IsFalse(doneB);
        }
        
        [UnityTest]
        public IEnumerator testWaitForAnyException()
        {
            bool hasDetectedException = false;
            Exception e;
            var doneA = false;
            var doneB = false;
            async Routine waitShort()
            {
                await RoutineBase.WaitForNextFrame();
                await RoutineBase.WaitForNextFrame();
                doneA = true;
                throw new Exception();
            }

            async Routine waitLong()
            {
                await RoutineBase.WaitForNextFrame();
                await RoutineBase.WaitForNextFrame();
                await RoutineBase.WaitForNextFrame();
                doneB = true;
            }

            async Routine testWaitForAny()
            {
                try
                {
                    await RoutineBase.WaitForAny(waitShort(), waitLong());

                }
                catch (Exception ex)
                {
                    hasDetectedException = true;
                }
            }
            
            var handle = manager.Run(testWaitForAny());
            
            Assert.IsFalse(doneA);
            Assert.IsFalse(doneB);
            Assert.IsFalse(handle.IsDead);
            yield return null;
            yield return null;

            Assert.IsTrue(doneA);
            Assert.IsFalse(doneB);
            Assert.IsTrue(handle.IsDead);
            Assert.IsTrue(hasDetectedException);

            yield return null;
            Assert.IsTrue(doneA);
            Assert.IsFalse(doneB);

            
        }
        
        [UnityTest]
        public IEnumerator testWaitForAnyExceptionTyped()
        {
            bool hasDetectedException = false;
            Exception e;
            var doneA = false;
            var doneB = false;
            async Routine<int> waitShort()
            {
                await RoutineBase.WaitForNextFrame();
                await RoutineBase.WaitForNextFrame();
                doneA = true;
                throw new Exception();
            }

            async Routine<int> waitLong()
            {
                await RoutineBase.WaitForNextFrame();
                await RoutineBase.WaitForNextFrame();
                await RoutineBase.WaitForNextFrame();
                doneB = true;
                return 5;
            }

            async Routine testWaitForAny()
            {
                try
                {
                    await RoutineBase.WaitForAny(waitShort(), waitLong());

                }
                catch (Exception ex)
                {
                    hasDetectedException = true;
                }
            }
            
            var handle = manager.Run(testWaitForAny());
            
            Assert.IsFalse(doneA);
            Assert.IsFalse(doneB);
            Assert.IsFalse(handle.IsDead);
            yield return null;
            yield return null;

            Assert.IsTrue(doneA);
            Assert.IsFalse(doneB);
            Assert.IsTrue(handle.IsDead);
            Assert.IsTrue(hasDetectedException);

            yield return null;
            Assert.IsTrue(doneA);
            Assert.IsFalse(doneB);

            
        }

        
        [UnityTest]
        public IEnumerator testWaitForAllException()
        {
            async Routine waitForAll()
            {
                await Routine.WaitForAll(ReturnNextFrame(), throwEx());
            }

            
            Exception resultException = null;
            var handle = manager.Run(waitForAll(),ex => resultException = ex);
            Assert.IsFalse(handle.IsDead);
            yield return null;
            Assert.IsTrue(handle.IsDead);
            yield return null;

            yield return null;
            Assert.NotNull(resultException);
            Assert.IsTrue(resultException is AggregateException);
            Assert.AreEqual(1,(resultException as AggregateException)?.InnerExceptions.Count);

        }
        [UnityTest]
        public IEnumerator testWaitForAllExceptionTyped()
        {
            async Routine waitForAll()
            {
                var result= await Routine.WaitForAll(ReturnNextFrameTyped(), throwExTyped());
            }

            
            Exception resultException = null;
            var handle = manager.Run(waitForAll(),ex => resultException = ex);
            Assert.IsFalse(handle.IsDead);
            yield return null;
            Assert.IsTrue(handle.IsDead);
            yield return null;

            yield return null;
            Assert.NotNull(resultException);
            Assert.IsTrue(resultException is AggregateException);
            Assert.AreEqual(1,(resultException as AggregateException)?.InnerExceptions.Count);

        }
      
    }
}